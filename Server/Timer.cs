/***************************************************************************
 *                                 Timer.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id$
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Diagnostics;

namespace Server
{
	/* This no longer does anything. It remains here to maintain API
	 * compatibility */
	public enum TimerPriority
	{
		EveryTick,
		TenMS,
		TwentyFiveMS,
		FiftyMS,
		TwoFiftyMS,
		OneSecond,
		FiveSeconds,
		OneMinute
	}

	public delegate void TimerCallback();
	public delegate void TimerStateCallback( object state );
	public delegate void TimerStateCallback<T>( T state );

	public class Timer
	{
		private long m_Expiration; /* The time at which this timer will next expire, in ticks */
		private long m_Delay; /* Relative delay until first expiry, in ticks */
		private long m_Interval; /* Relative delay between subsequent expiries, in ticks */
		private bool m_Running; /* Whether the timer is currently scheduled */
		private bool m_Reschedule; /* Whether to reschedule the timer when it expires */
		private int m_Count; /* The number of times to reschedule the timer automatically */
		private TimerPriority m_Priority; /* Priority is no longer used and only remains for API compatibility */

		/* A pointer to the next timer in the current time wheel bin. Only
		 * intended to be used by the HierarchicalTimeWheel */
		private Timer m_Next;
		/* The previous timer in the current bin */
		private Timer m_Prev;
		/* The current level in the time wheel */
		private uint m_Level;
		/* The current bin in the time wheel */
		private uint m_Bin;

		private static string FormatDelegate( Delegate callback )
		{
			if ( callback == null )
				return "null";

			return String.Format( "{0}.{1}", callback.Method.DeclaringType.FullName, callback.Method.Name );
		}


		#region static data

		public static void DumpInfo( TextWriter tw )
		{
			// TODO
		}

		private static HierarchicalTimeWheel m_TimeWheel = new HierarchicalTimeWheel();

		public static void Slice()
		{
			m_TimeWheel.Expire();
		}

		#endregion

		public TimerPriority Priority
		{
			/* Timers no longer have priority. To avoid removing the priority system,
			 * just fake it.
			 */
			get
			{
				return m_Priority;
			}
			set
			{
				m_Priority = value;
			}
		}

		public TimeSpan Remaining
		{
			get
			{
				/* Convert timer ticks to milliseconds */
				return TimeSpan.FromMilliseconds((m_Expiration - Core.Now) * Core.MILLISECONDS_PER_ENGINE_TICK);
			}
		}

		public DateTime Next
		{
			get { return DateTime.UtcNow + Remaining; }
		}

		public TimeSpan Delay
		{
			set
			{
				long ms = (long)value.TotalMilliseconds;

				if (ms > 0 && ms < Core.MILLISECONDS_PER_ENGINE_TICK)
				{
					Console.WriteLine("Attempted to set a timer for less than a single engine tick.");
					ms = (long)Core.MILLISECONDS_PER_ENGINE_TICK;
				}

				m_Delay = (ms * Core.HW_TICKS_PER_MILLISECOND) >> Core.HW_TICKS_PER_ENGINE_TICK_POW_2;
			}
		}

		public TimeSpan Interval
		{
			set
			{
				long ms = (long)value.TotalMilliseconds;

				if (ms > 0 && ms < Core.MILLISECONDS_PER_ENGINE_TICK)
				{
					Console.WriteLine("Attempted to set a timer for less than a single engine tick.");
					ms = (long)Core.MILLISECONDS_PER_ENGINE_TICK;
				}

				m_Interval = (ms * Core.HW_TICKS_PER_MILLISECOND) >> Core.HW_TICKS_PER_ENGINE_TICK_POW_2;
			}
		}

		public bool Running
		{
			get { return m_Running; }
		}

		public class HierarchicalTimeWheel
		{
			/* The size of the wheel, as a power of two.
			 * 7 is chosen because it results in 8 levels of 128,
			 * which is exactly enough to handle any 32 bit
			 * integer. */
			private const int WHEEL_SIZE_POW_TWO = 7;
			private const uint NUM_LEVELS = 8;

			/* The size of the wheel as an integer. Don't modify
			 * this value. Modify the power of two value instead.
			 */
			private const uint WHEEL_SIZE = 1 << WHEEL_SIZE_POW_TWO;

			private Timer[,] m_Wheels;
			private uint[] m_WheelPosition;

			public HierarchicalTimeWheel()
			{
				m_Wheels = new Timer[NUM_LEVELS, WHEEL_SIZE];
				m_WheelPosition = new uint[NUM_LEVELS];

				for (int i = 0; i < NUM_LEVELS; i++)
				{
					m_WheelPosition[i] = WHEEL_SIZE - 1;
				}
			}

			public void Insert(Timer t)
			{
				/* Determine the time left until expiry */
				if (Core.Now > t.m_Expiration)
				{
					/* The timer is already expired */
					t.Expire();
					return;
				}

				var delta = t.m_Expiration - Core.Now;

				/* Determine which level and bin to place the timer in */
				for (uint level = 0; level < NUM_LEVELS; level++)
				{
					if ((delta & ~(WHEEL_SIZE - 1)) == 0)
					{
						/* This is the appropriate level for the wheel */
						uint bin = (uint)delta;

						/* Account for the wheel being circular */
						bin = (m_WheelPosition[level] + bin) % WHEEL_SIZE;

						/* Insert the timer */
						t.m_Prev = null;
						t.m_Next = m_Wheels[level, bin];
						t.m_Level = level;
						t.m_Bin = bin;
						if (m_Wheels[level, bin] != null)
						{
							m_Wheels[level, bin].m_Prev = t;
						}
						m_Wheels[level, bin] = t;
						break;

					}

					delta >>= WHEEL_SIZE_POW_TWO;
				}
			}

			public void Remove(Timer t)
			{
				if (m_Wheels[t.m_Level, t.m_Bin] == t)
				{
					m_Wheels[t.m_Level, t.m_Bin] = t.m_Next;
					if (m_Wheels[t.m_Level, t.m_Bin] != null)
					{
						m_Wheels[t.m_Level, t.m_Bin].m_Prev = null;
					}
				}

				if (t.m_Next != null)
				{
					t.m_Next.m_Prev = t.m_Prev;
				}

				if (t.m_Prev != null)
				{
					t.m_Prev.m_Next = t.m_Next;
				}
			}

			public void AdvanceLevel(int level)
			{
				if (level == NUM_LEVELS)
				{
					return;
				}

				m_WheelPosition[level] = (m_WheelPosition[level] + 1) % WHEEL_SIZE;

				/* Pull all timers from the current bin and re-insert them
				 * into the level below */
				var t = m_Wheels[level, m_WheelPosition[level]];
				m_Wheels[level, m_WheelPosition[level]] = null;
				while (t != null)
				{
					var next = t.m_Next;
					Insert(t);
					t = next;
				}

				if (m_WheelPosition[level] == (WHEEL_SIZE - 1))
				{
					/* Recursively transfer timers down a level */
					AdvanceLevel(level + 1);
				}
			}

			public void Expire()
			{
				/* Advance the current position by one */
				m_WheelPosition[0] = (m_WheelPosition[0] + 1) % WHEEL_SIZE;

				/* Expire all timers in this bin */
				var t = m_Wheels[0, m_WheelPosition[0]];
				m_Wheels[0, m_WheelPosition[0]] = null;
				while (t != null)
				{
					var next = t.m_Next;
					t.Expire();
					t = next;
				}

				if (m_WheelPosition[0] == (WHEEL_SIZE - 1))
				{
					/* Recursively transfer timers from the given level to this one */
					AdvanceLevel(1);
				}

			}
		}

		public Timer( TimeSpan delay ) : this( delay, TimeSpan.Zero, 1 )
		{
		}

		public Timer( TimeSpan delay, TimeSpan interval ) : this( delay, interval, 0 )
		{
		}

		public Timer( TimeSpan delay, TimeSpan interval, int count ) : this((uint)delay.TotalMilliseconds, (uint)interval.TotalMilliseconds, count)
		{
		}

		/* Note that this is specifically a 32 bit unsigned integer and the
		 * units are in milliseconds. This allows for delays up to
		 * 49.7 days. */
		public Timer(uint DelayInMs, uint IntervalInMs, int count)
		{
			m_Delay = (DelayInMs * Core.HW_TICKS_PER_MILLISECOND) >> Core.HW_TICKS_PER_ENGINE_TICK_POW_2;
			m_Interval = (IntervalInMs * Core.HW_TICKS_PER_MILLISECOND) >> Core.HW_TICKS_PER_ENGINE_TICK_POW_2;
			m_Count = count;
			m_Running = false;

			if (IntervalInMs > 0)
			{
				m_Reschedule = true;
			} else
			{
				m_Reschedule = false;
			}
		}

		public override string ToString()
		{
			return GetType().FullName;
		}

		private void Expire()
		{
			m_Running = false;
			OnTick();

			if (m_Reschedule)
			{
				/* If count is 0, reschedule indefinitely */
				if (m_Count == 0)
				{
					m_Delay = m_Interval;
					Start();
					return;
				}

				/* Otherwise, decrement count and reschedule */
				m_Count--;
				m_Delay = m_Interval;

				/* If this is the last reschedule, mark to not reschedule again*/
				if (m_Count == 1)
				{
					m_Reschedule = false;
				}

				Start();
			}
		}

		#region DelayCall(..)

		public static Timer DelayCall( TimerCallback callback )
		{
			return DelayCall( TimeSpan.Zero, TimeSpan.Zero, 1, callback );
		}

		public static Timer DelayCall( TimeSpan delay, TimerCallback callback )
		{
			return DelayCall( delay, TimeSpan.Zero, 1, callback );
		}

		public static Timer DelayCall( TimeSpan delay, TimeSpan interval, TimerCallback callback )
		{
			return DelayCall( delay, interval, 0, callback );
		}

		public static Timer DelayCall( TimeSpan delay, TimeSpan interval, int count, TimerCallback callback )
		{
			Timer t = new DelayCallTimer( delay, interval, count, callback );

			t.Start();

			return t;
		}

		public static Timer DelayCall( TimerStateCallback callback, object state )
		{
			return DelayCall( TimeSpan.Zero, TimeSpan.Zero, 1, callback, state );
		}

		public static Timer DelayCall( TimeSpan delay, TimerStateCallback callback, object state )
		{
			return DelayCall( delay, TimeSpan.Zero, 1, callback, state );
		}

		public static Timer DelayCall( TimeSpan delay, TimeSpan interval, TimerStateCallback callback, object state )
		{
			return DelayCall( delay, interval, 0, callback, state );
		}

		public static Timer DelayCall( TimeSpan delay, TimeSpan interval, int count, TimerStateCallback callback, object state )
		{
			Timer t = new DelayStateCallTimer( delay, interval, count, callback, state );

			t.Start();

			return t;
		}
		#endregion

		#region DelayCall<T>(..)
		public static Timer DelayCall<T>( TimerStateCallback<T> callback, T state )
		{
			return DelayCall( TimeSpan.Zero, TimeSpan.Zero, 1, callback, state );
		}

		public static Timer DelayCall<T>( TimeSpan delay, TimerStateCallback<T> callback, T state )
		{
			return DelayCall( delay, TimeSpan.Zero, 1, callback, state );
		}

		public static Timer DelayCall<T>( TimeSpan delay, TimeSpan interval, TimerStateCallback<T> callback, T state )
		{
			return DelayCall( delay, interval, 0, callback, state );
		}

		public static Timer DelayCall<T>( TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T> callback, T state )
		{
			Timer t = new DelayStateCallTimer<T>( delay, interval, count, callback, state );

			t.Start();

			return t;
		}
		#endregion

		#region DelayCall Timers
		private class DelayCallTimer : Timer
		{
			private TimerCallback m_Callback;

			public TimerCallback Callback{ get{ return m_Callback; } }

			public DelayCallTimer( TimeSpan delay, TimeSpan interval, int count, TimerCallback callback ) : base( delay, interval, count )
			{
				m_Callback = callback;
			}

			protected override void OnTick()
			{
				if ( m_Callback != null )
					m_Callback();
			}

			public override string ToString()
			{
				return String.Format( "DelayCallTimer[{0}]", FormatDelegate( m_Callback ) );
			}
		}

		private class DelayStateCallTimer : Timer
		{
			private TimerStateCallback m_Callback;
			private object m_State;

			public TimerStateCallback Callback{ get{ return m_Callback; } }

			public DelayStateCallTimer( TimeSpan delay, TimeSpan interval, int count, TimerStateCallback callback, object state ) : base( delay, interval, count )
			{
				m_Callback = callback;
				m_State = state;
			}

			protected override void OnTick()
			{
				if ( m_Callback != null )
					m_Callback( m_State );
			}

			public override string ToString()
			{
				return String.Format( "DelayStateCall[{0}]", FormatDelegate( m_Callback ) );
			}
		}

		private class DelayStateCallTimer<T> : Timer
		{
			private TimerStateCallback<T> m_Callback;
			private T m_State;

			public TimerStateCallback<T> Callback { get { return m_Callback; } }

			public DelayStateCallTimer( TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T> callback, T state )
				: base( delay, interval, count )
			{
				m_Callback = callback;
				m_State = state;
			}

			protected override void OnTick()
			{
				if ( m_Callback != null )
					m_Callback( m_State );
			}

			public override string ToString()
			{
				return String.Format( "DelayStateCall[{0}]", FormatDelegate( m_Callback ) );
			}
		}
		#endregion

		public void Start()
		{
			if (m_Running == true)
			{
				return;
			}
			m_Running = true;
			m_Expiration = Core.Now + m_Delay;
			if (m_Delay > 0)
			{
				m_TimeWheel.Insert(this);
			}
			else
			{
				Expire();
			}
		}

		public void Stop()
		{
			if (m_Running)
			{
				m_TimeWheel.Remove(this);
				m_Expiration = 0;
				m_Running = false;
			}
			m_Reschedule = false;
		}

		/* Override this method in a subclass */
		protected virtual void OnTick()
		{
		}

	}
}
