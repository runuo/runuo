using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Mobiles
{
	[CorpseName( "a molten guardian's corpse" )]
	public class MoltenGuardian: BaseMount
	{
		[Constructable]
		public MoltenGuardian() : this( "a molten guardian" )
		{
		}

		[Constructable]
		public MoltenGuardian( string name ) : base( name, 0x74, 0x3EA7, AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			BaseSoundID = 838;

			SetStr( 25, 35 );
			SetDex( 500, 600 );
			SetInt( 86, 125 );

			SetHits( 3000, 3500 );

			SetDamage( 1, 2 );

			SetDamageType( ResistanceType.Physical, 40 );
			SetDamageType( ResistanceType.Fire, 40 );
			SetDamageType( ResistanceType.Energy, 20 );

			SetResistance( ResistanceType.Physical, 55, 65 );
			SetResistance( ResistanceType.Fire, 30, 40 );
			SetResistance( ResistanceType.Cold, 30, 40 );
			SetResistance( ResistanceType.Poison, 30, 40 );
			SetResistance( ResistanceType.Energy, 20, 30 );

			SetSkill( SkillName.EvalInt, 10.4, 50.0 );
			SetSkill( SkillName.Magery, 10.4, 50.0 );
			SetSkill( SkillName.MagicResist, 85.3, 100.0 );
			SetSkill( SkillName.Tactics, 97.6, 100.0 );
			SetSkill( SkillName.Wrestling, 10, 20 );

			Fame = 14000;
			Karma = -14000;

			VirtualArmor = 60;

			Tamable = true;
			ControlSlots = 2;
			MinTameSkill = 95.1;

			switch ( Utility.Random( 3 ) )
			{
				case 0:
				{
					BodyValue = 15;
					ItemID = 16039;
					break;
				}
				case 1:
				{
					BodyValue = 15;
					ItemID = 16041;
					break;
				}
				case 2:
				{
					BodyValue = 15;
					ItemID = 16055;
					break;
				}
			}

			PackItem( new SulfurousAsh( Utility.RandomMinMax( 3, 5 ) ) );
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Rich );
			AddLoot( LootPack.Average );
			AddLoot( LootPack.LowScrolls );
			AddLoot( LootPack.Potions );
		}

		

		public override bool HasBreath{ get{ return true; } } // fire breath enabled
		public override int Meat{ get{ return 5; } }
		public override int Hides{ get{ return 10; } }
		public override HideType HideType{ get{ return HideType.Barbed; } }
		public override FoodType FavoriteFood{ get{ return FoodType.Meat; } }
		public override bool CanAngerOnTame { get { return true; } }

		public void SpawnLavaSlimes( Mobile target )
		{
			Map map = this.Map;

			if ( map == null )
				return;

			this.Say( true, String.Format( "*The ground erupts with lava!*" ) );

			int newLavaSlimes = Utility.RandomMinMax( 3, 6 );

			for ( int i = 0; i < newLavaSlimes; ++i )
			{
				LavaSlime lavaslime = new LavaSlime();

				lavaslime.Team = this.Team;
				lavaslime.FightMode = FightMode.Closest;

				bool validLocation = false;
				Point3D loc = this.Location;

				for ( int j = 0; !validLocation && j < 10; ++j )
				{
					int x = X + Utility.Random( 3 ) - 1;
					int y = Y + Utility.Random( 3 ) - 1;
					int z = map.GetAverageZ( x, y );

					if ( validLocation = map.CanFit( x, y, this.Z, 16, false, false ) )
						loc = new Point3D( x, y, Z );
					else if ( validLocation = map.CanFit( x, y, z, 16, false, false ) )
						loc = new Point3D( x, y, z );
				}

				lavaslime.MoveToWorld( loc, map );
				lavaslime.Combatant = target;
			}
		}

		
		public override void OnGotMeleeAttack( Mobile attacker )
		{
			base.OnGotMeleeAttack( attacker );
			attacker.Damage( Utility.Random( 10, 10 ), this );
/*			switch ( Utility.Random( 2 ) )
				{
					case 0:
					{
						attacker.FixedParticles( 0x36BD, 20, 10, 5044, EffectLayer.Head );
						break;
					}
					case 1:
					{	
						attacker.FixedParticles( 0x3709, 10, 30, 5052, EffectLayer.LeftFoot );
						break;
					}
					case 2:
					{
						attacker.FixedParticles( 0x3709, 10, 30, 5052, EffectLayer.Head );
						break;
					}					
				} */
			 attacker.FixedParticles( 0x3709, 20, 10, 5044, EffectLayer.RightFoot );
                         attacker.PlaySound( 0x21F );
                         attacker.FixedParticles( 0x36BD, 10, 30, 5052, EffectLayer.Head );
                         attacker.PlaySound( 0x208 );

			if ( 0.1 >= Utility.RandomDouble() )
			SpawnLavaSlimes( attacker );

		}

		

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			base.OnGaveMeleeAttack( defender );
			defender.Damage( Utility.Random( 10, 10 ), this );
			Animate( BreathAngerAnimation, 5, 1, true, false, 0 );			
			defender.FixedParticles( 0x3709, 10, 30, 5052, EffectLayer.LeftFoot );
			defender.PlaySound( 0x21F );
		}

		public MoltenGuardian( Serial serial ) : base( serial )
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

			if ( Core.AOS && BaseSoundID == 838 )
				BaseSoundID = 838;
			else if ( !Core.AOS && BaseSoundID == 838 )
				BaseSoundID = 838;
		}
	}
}