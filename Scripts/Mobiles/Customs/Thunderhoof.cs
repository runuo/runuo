using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Mobiles
{
	[CorpseName( "Thunderhoof's corpse" )]
	public class Thunderhoof: BaseMount
	{
		[Constructable]
		public Thunderhoof() : this( "Thunderhoof" )
		{
		}

		[Constructable]
		public Thunderhoof( string name ) : base( name, 0x74, 0x3EA7, AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			BaseSoundID = Core.AOS ? 0xA8 : 0x16A;

			SetStr( 25, 35 );
			SetDex( 500, 600 );
			SetInt( 86, 125 );

			SetHits( 298, 315 );

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
					BodyValue = 116;
					ItemID = 16039;
					break;
				}
				case 1:
				{
					BodyValue = 178;
					ItemID = 16041;
					break;
				}
				case 2:
				{
					BodyValue = 179;
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

		public override int GetAngerSound()
		{
			if ( !Controlled )
				return 0x16A;

			return base.GetAngerSound();
		}

		public override bool HasBreath{ get{ return true; } } // fire breath enabled
		public override int Meat{ get{ return 5; } }
		public override int Hides{ get{ return 10; } }
		public override HideType HideType{ get{ return HideType.Barbed; } }
		public override FoodType FavoriteFood{ get{ return FoodType.Meat; } }
		public override bool CanAngerOnTame { get { return true; } }

		public override void OnGotMeleeAttack( Mobile attacker )
		{
			base.OnGotMeleeAttack( attacker );
			attacker.Damage( Utility.Random( 30, 30 ), this );
			attacker.BoltEffect( 0 );
			attacker.FixedParticles( 0x36BD, 10, 30, 5052, EffectLayer.RightFoot );
                        attacker.PlaySound( 0x208 );
		}

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			base.OnGaveMeleeAttack( defender );
			defender.Damage( Utility.Random( 30, 30 ), this );			
			defender.BoltEffect( 0 );
			PlaySound( 0x307 );
			defender.FixedParticles( 0x36BD, 10, 30, 5052, EffectLayer.RightFoot );
                        defender.PlaySound( 0x208 );
		}

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{

		if ( 0.1 >= Utility.RandomDouble() )
		{
		this.BoltEffect( 0 ); 	
		}
		}
		

		public Thunderhoof( Serial serial ) : base( serial )
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

			if ( Core.AOS && BaseSoundID == 0x16A )
				BaseSoundID = 0xA8;
			else if ( !Core.AOS && BaseSoundID == 0xA8 )
				BaseSoundID = 0x16A;
		}
	}
}