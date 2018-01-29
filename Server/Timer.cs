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
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Server.Diagnostics;

namespace Server
{
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
		private long m_Next;
		private long m_Delay;
		private long m_Interval;
		private bool m_Running;
		private int m_Index, m_Count;
		private TimerPriority m_Priority;
		private List<Timer> m_List;
		private bool m_PrioritySet;

		private static string FormatDelegate( Delegate callback )
		{
			if ( callback == null )
				return "null";

			return String.Format( "{0}.{1}", callback.Method.DeclaringType.FullName, callback.Method.Name );
		}

		public static void DumpInfo( TextWriter tw )
		{
			TimerThread.DumpInfo( tw );
		}

		public TimerPriority Priority
		{
			get
			{
				return m_Priority;
			}
			set
			{
				if ( !m_PrioritySet )
					m_PrioritySet = true;

				if ( m_Priority != value )
				{
					m_Priority = value;

					if ( m_Running )
						TimerThread.PriorityChange( this, (int)m_Priority );
				}
			}
		}

		public DateTime Next
		{
			// Obnoxious
			get { return DateTime.UtcNow + TimeSpan.FromMilliseconds(m_Next-Core.TickCount); }
		}

		public TimeSpan Delay
		{
			get { return TimeSpan.FromMilliseconds(m_Delay); }
			set { m_Delay = (long)value.TotalMilliseconds; }
		}

		public TimeSpan Interval
		{
			get { return TimeSpan.FromMilliseconds(m_Interval); }
			set { m_Interval = (long)value.TotalMilliseconds; }
		}

		public bool Running
		{
			get { return m_Running; }
			set {
				if ( value ) {
					Start();
				} else {
					Stop();
				}
			}
		}

		public TimerProfile GetProfile()
		{
			if ( !Core.Profiling ) {
				return null;
			}

			string name = ToString();

			if ( name == null ) {
				name = "null";
			}

			return TimerProfile.Acquire( name );
		}

		public class TimerThread
		{
			private static Dictionary<Timer,TimerChangeEntry> m_Changed = new Dictionary<Timer,TimerChangeEntry>();

			private static long[] m_NextPriorities = new long[8];
			private static long[] m_PriorityDelays = new long[8]
			{
				0,
				10,
				25,
				50,
				250,
				1000,
				5000,
				60000
			};

			private static List<Timer>[] m_Timers = new List<Timer>[8]
			{
				new List<Timer>(),
				new List<Timer>(),
				new List<Timer>(),
				new List<Timer>(),
				new List<Timer>(),
				new List<Timer>(),
				new List<Timer>(),
				new List<Timer>(),
			};

			public static void DumpInfo( TextWriter tw )
			{
				for ( int i = 0; i < 8; ++i )
				{
					tw.WriteLine( "Priority: {0}", (TimerPriority)i );
					tw.WriteLine();

					Dictionary<string, List<Timer>> hash = new Dictionary<string, List<Timer>>();

					for ( int j = 0; j < m_Timers[i].Count; ++j )
					{
						Timer t = m_Timers[i][j];

						string key = t.ToString();

						List<Timer> list;
						hash.TryGetValue( key, out list );

						if ( list == null )
							hash[key] = list = new List<Timer>();

						list.Add( t );
					}

					foreach ( KeyValuePair<string, List<Timer>> kv in hash )
					{
						string key = kv.Key;
						List<Timer> list = kv.Value;

						tw.WriteLine( "Type: {0}; Count: {1}; Percent: {2}%", key, list.Count, (int)(100 * (list.Count / (double)m_Timers[i].Count)) );
					}

					tw.WriteLine();
					tw.WriteLine();
				}
			}

			private class TimerChangeEntry
			{
				public Timer m_Timer;
				public int m_NewIndex;
				public bool m_IsAdd;

				private TimerChangeEntry( Timer t, int newIndex, bool isAdd )
				{
					m_Timer = t;
					m_NewIndex = newIndex;
					m_IsAdd = isAdd;
				}

				public void Free()
				{
					lock (m_InstancePool) {
						if (m_InstancePool.Count < 200) // Arbitrary
							m_InstancePool.Enqueue( this );
					}
				}

				private static Queue<TimerChangeEntry> m_InstancePool = new Queue<TimerChangeEntry>();

				public static TimerChangeEntry GetInstance( Timer t, int newIndex, bool isAdd )
				{
					TimerChangeEntry e = null;

					lock (m_InstancePool) {
						if ( m_InstancePool.Count > 0 ) {
							e = m_InstancePool.Dequeue();
						}
					}

					if (e != null) {
						e.m_Timer = t;
						e.m_NewIndex = newIndex;
						e.m_IsAdd = isAdd;
					} else {
						e = new TimerChangeEntry( t, newIndex, isAdd );
					}

					return e;
				}
			}

			public TimerThread()
			{
			}

			public static void Change( Timer t, int newIndex, bool isAdd )
			{
				lock (m_Changed)
					m_Changed[t] = TimerChangeEntry.GetInstance(t, newIndex, isAdd);
				m_Signal.Set();
			}

			public static void AddTimer( Timer t )
			{
				Change( t, (int)t.Priority, true );
			}

			public static void PriorityChange( Timer t, int newPrio )
			{
				Change( t, newPrio, false );
			}

			public static void RemoveTimer( Timer t )
			{
				Change( t, -1, false );
			}

			private static void ProcessChanged()
			{
				lock (m_Changed) {
					long curTicks = Core.TickCount;

					foreach (TimerChangeEntry tce in m_Changed.Values) {
						Timer timer = tce.m_Timer;
						int newIndex = tce.m_NewIndex;

						if (timer.m_List != null)
							timer.m_List.Remove(timer);

						if (tce.m_IsAdd) {
							timer.m_Next = curTicks + timer.m_Delay;
							timer.m_Index = 0;
						}

						if (newIndex >= 0) {
							timer.m_List = m_Timers[newIndex];
							timer.m_List.Add(timer);
						} else {
							timer.m_List = null;
						}

						tce.Free();
					}

					m_Changed.Clear();
				}
			}

			private static AutoResetEvent m_Signal = new AutoResetEvent( false );
			public static void Set() { m_Signal.Set(); }

			public void TimerMain()
			{
				long now;
				int i, j;
				bool loaded;

				while ( !Core.Closing )
				{
					if (World.Loading || World.Saving)
					{
						m_Signal.WaitOne(1, false);
						continue;
					}

					ProcessChanged();

					loaded = false;

					for ( i = 0; i < m_Timers.Length; i++)
					{
						now = Core.TickCount;
						if ( now < m_NextPriorities[i] )
							break;

						m_NextPriorities[i] = now + m_PriorityDelays[i];

						for ( j = 0; j < m_Timers[i].Count; j++)
						{
							Timer t = m_Timers[i][j];

							if ( !t.m_Queued && now > t.m_Next )
							{
								t.m_Queued = true;

								m_Queue.Enqueue( t );

								loaded = true;
									
								if ( t.m_Count != 0 && (++t.m_Index >= t.m_Count) )
								{
									t.Stop();
								}
								else
								{
									t.m_Next = now + t.m_Interval;
								}
							}
						}
					}

					if ( loaded )
						Core.Set();

					m_Signal.WaitOne(1, false);
				}
			}
		}

		private static ConcurrentQueue<Timer> m_Queue = new ConcurrentQueue<Timer>();
		private static int m_BreakCount = 20000;

		public static int BreakCount{ get{ return m_BreakCount; } set{ m_BreakCount = value; } }

		private bool m_Queued;

		public static void Slice()
		{
			int count = 0;

			while (count++ < m_BreakCount) {

				if (!m_Queue.TryDequeue(out Timer t)) {
					break;
				}

				TimerProfile prof = t.GetProfile();

				if ( prof != null ) {
					prof.Start();
				}

				t.OnTick();
				t.m_Queued = false;

				if ( prof != null ) {
					prof.Finish();
				}
			}
		}

		public Timer( TimeSpan delay ) : this( delay, TimeSpan.Zero, 1 )
		{
		}

		public Timer( TimeSpan delay, TimeSpan interval ) : this( delay, interval, 0 )
		{
		}

		public virtual bool DefRegCreation
		{
			get{ return true; }
		}

		public void RegCreation()
		{
			TimerProfile prof = GetProfile();

			if ( prof != null ) {
				prof.Created++;
			}
		}

		public Timer( TimeSpan delay, TimeSpan interval, int count )
		{
			m_Delay = (long)delay.TotalMilliseconds;
			m_Interval = (long)interval.TotalMilliseconds;
			m_Count = count;

			if ( !m_PrioritySet ) {
				if ( count == 1 ) {
					m_Priority = ComputePriority( delay );
				} else {
					m_Priority = ComputePriority( interval );
				}
				m_PrioritySet = true;
			}

			if ( DefRegCreation )
				RegCreation();
		}

		public override string ToString()
		{
			return GetType().FullName;
		}

		public static TimerPriority ComputePriority( TimeSpan ts )
		{
			if ( ts >= TimeSpan.FromMinutes( 1.0 ) )
				return TimerPriority.FiveSeconds;

			if ( ts >= TimeSpan.FromSeconds( 10.0 ) )
				return TimerPriority.OneSecond;

			if ( ts >= TimeSpan.FromSeconds( 5.0 ) )
				return TimerPriority.TwoFiftyMS;

			if ( ts >= TimeSpan.FromSeconds( 2.5 ) )
				return TimerPriority.FiftyMS;

			if ( ts >= TimeSpan.FromSeconds( 1.0 ) )
				return TimerPriority.TwentyFiveMS;

			if ( ts >= TimeSpan.FromSeconds( 0.5 ) )
				return TimerPriority.TenMS;

			return TimerPriority.EveryTick;
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

			if ( count == 1 )
				t.Priority = ComputePriority( delay );
			else
				t.Priority = ComputePriority( interval );

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

			if ( count == 1 )
				t.Priority = ComputePriority( delay );
			else
				t.Priority = ComputePriority( interval );

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

			if( count == 1 )
				t.Priority = ComputePriority( delay );
			else
				t.Priority = ComputePriority( interval );

			t.Start();

			return t;
		}
		#endregion

		#region DelayCall Timers
		private class DelayCallTimer : Timer
		{
			private TimerCallback m_Callback;

			public TimerCallback Callback{ get{ return m_Callback; } }

			public override bool DefRegCreation{ get{ return false; } }

			public DelayCallTimer( TimeSpan delay, TimeSpan interval, int count, TimerCallback callback ) : base( delay, interval, count )
			{
				m_Callback = callback;
				RegCreation();
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

			public override bool DefRegCreation{ get{ return false; } }

			public DelayStateCallTimer( TimeSpan delay, TimeSpan interval, int count, TimerStateCallback callback, object state ) : base( delay, interval, count )
			{
				m_Callback = callback;
				m_State = state;

				RegCreation();
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

			public override bool DefRegCreation { get { return false; } }

			public DelayStateCallTimer( TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T> callback, T state )
				: base( delay, interval, count )
			{
				m_Callback = callback;
				m_State = state;

				RegCreation();
			}

			protected override void OnTick()
			{
				if( m_Callback != null )
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
			if ( !m_Running )
			{
				m_Running = true;
				TimerThread.AddTimer( this );

				TimerProfile prof = GetProfile();

				if ( prof != null ) {
					prof.Started++;
				}
			}
		}

		public void Stop()
		{
			if ( m_Running )
			{
				m_Running = false;
				TimerThread.RemoveTimer( this );

				TimerProfile prof = GetProfile();

				if ( prof != null ) {
					prof.Stopped++;
				}
			}
		}

		protected virtual void OnTick()
		{
		}
	}
}
