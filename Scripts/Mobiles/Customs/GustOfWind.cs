using System;
using Server;
using Server.Items;
using System.Collections;

namespace Server.Mobiles
{
	[CorpseName( "a wind gust" )]
	public class GustOfWind : BaseCreature
	{
		public override bool DeleteCorpseOnDeath { get { return Summoned; } }
		public override bool AlwaysMurderer{ get{ return true; } } // Or Llama vortices will appear gray.

		public override double DispelDifficulty { get { return 80.0; } }
		public override double DispelFocus { get { return 20.0; } }

		public override double GetFightModeRanking( Mobile m, FightMode acqType, bool bPlayerOnly )
		{
			return ( m.Int + m.Skills[SkillName.Magery].Value ) / Math.Max( GetDistanceToSqrt( m ), 1.0 );
		}

		[Constructable]
		public GustOfWind()
			: base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a wind gust";

			Body = 164;
			Hue = 2581;

			SetStr( 1 );
			SetDex( 1000 );
			SetInt( 100 );

			SetHits( ( Core.SE ) ? 5 : 5 );
			SetStam( 250 );
			SetMana( 0 );

			SetDamage( 1, 1 );

			SetDamageType( ResistanceType.Physical, 0 );
			SetDamageType( ResistanceType.Energy, 100 );

			SetResistance( ResistanceType.Physical, 60, 70 );
			SetResistance( ResistanceType.Fire, 40, 50 );
			SetResistance( ResistanceType.Cold, 40, 50 );
			SetResistance( ResistanceType.Poison, 40, 50 );
			SetResistance( ResistanceType.Energy, 90, 100 );

			SetSkill( SkillName.MagicResist, 99.9 );
			SetSkill( SkillName.Tactics, 10.0 );
			SetSkill( SkillName.Wrestling, 10.0 );

			Fame = 0;
			Karma = 0;

			VirtualArmor = 40;
			ControlSlots = ( Core.SE ) ? 2 : 1;
		}

		public override bool BleedImmune{ get{ return true; } }
		public override Poison PoisonImmune { get { return Poison.Lethal; } }

		public override int GetAngerSound()
		{
			return 0x15;
		}

		public override int GetAttackSound()
		{
			return 0x28;
		}

		public override void OnThink()
		{
			if ( Core.SE && Summoned )
			{
				ArrayList spirtsOrVortexes = new ArrayList();

				foreach ( Mobile m in GetMobilesInRange( 5 ) )
				{
					if ( m is GustOfWind || m is BladeSpirits )
					{
						if ( ( (BaseCreature) m ).Summoned )
							spirtsOrVortexes.Add( m );
					}
				}

				while ( spirtsOrVortexes.Count > 6 )
				{
					int index = Utility.Random( spirtsOrVortexes.Count );
					//TODO: Confirm if it's the dispel with all the pretty effects or just a deletion of it.
					Dispel( ( (Mobile) spirtsOrVortexes[index] ) );
					spirtsOrVortexes.RemoveAt( index );
				}
			}

			base.OnThink();
		}

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{

			if ( 0.3 >= Utility.RandomDouble() )
				{
					switch ( Utility.Random( 2 ) )
					{
					case 0: this.BoltEffect( 0 ); break;
					case 1: this.Animate( BreathAngerAnimation, 5, 1, true, false, 0 ); break;
					}
				}

		}

		public GustOfWind( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if ( BaseSoundID == 263 )
				BaseSoundID = 0;
		}
	}
}