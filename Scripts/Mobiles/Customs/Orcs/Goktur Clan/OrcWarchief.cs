using System;
using System.Collections;
using Server.Items;
using Server.Targeting;
using Server.Misc;

namespace Server.Mobiles
{
	[CorpseName( "an orc war chieftain corpse" )]
	public class GokturWarchief : BaseCreature
	{
	

		[Constructable]
		public GokturWarchief() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = NameList.RandomName( "orc" );
			Body = 0x190;
			BaseSoundID = 0x45A;
			Title = "the Goktur Clan War Chieftain";
			Hue = Utility.RandomMinMax( 2207,2212 );

			SetStr( 96, 120 );
			SetDex( 450, 500 );
			SetInt( 36, 60 );

			SetHits( 1000, 1200 );

			SetDamage( 5, 7 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 25, 30 );
			SetResistance( ResistanceType.Fire, 20, 30 );
			SetResistance( ResistanceType.Cold, 10, 20 );
			SetResistance( ResistanceType.Poison, 10, 20 );
			SetResistance( ResistanceType.Energy, 20, 30 );

			SetSkill( SkillName.MagicResist, 50.1, 75.0 );
			SetSkill( SkillName.Tactics, 55.1, 80.0 );
			SetSkill( SkillName.Wrestling, 50.1, 70.0 );

			Fame = 0;
			Karma = 0;

			VirtualArmor = 28;

			BearMask helm = new BearMask();
			helm.Hue = 0;
			AddItem( helm );

			Club club = new Club();
			club.Hue = 0;
			AddItem ( club );
			
			BoneLegs bonelegs = new BoneLegs();
			bonelegs.Hue = 0;
			AddItem ( bonelegs );

			BoneChest bonechest = new BoneChest();
			bonechest.Hue = 0;
			AddItem ( bonechest );

			BoneArms bonearms = new BoneArms();
			bonearms.Hue = 0;
			AddItem ( bonearms );

			BoneGloves bonegloves = new BoneGloves();
			bonegloves.Hue = 0;
			AddItem ( bonegloves );

			Sandals sandals = new Sandals();
			sandals.Hue = 0;
			AddItem ( sandals );

			WoodenShield woodenshield = new WoodenShield();
			woodenshield.Hue = 0;
			AddItem ( woodenshield );
	
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Meager );
		}

		public override bool CanRummageCorpses{ get{ return true; } }
		public override int TreasureMapLevel{ get{ return 1; } }
		public override int Meat{ get{ return 1; } }
		public override bool AlwaysMurderer{ get{ return true; } }

		public void SpawnGokturWarlord( Mobile target )
		{
			Map map = this.Map;

			if ( map == null )
				return;

			this.Say( true, String.Format( "*The Chieftain beats his drum to summon the Goktur Warlords!*" ) );
			this.PlaySound( 0x38 );
			int newGokturWarlords = Utility.RandomMinMax( 2, 3 );
			for ( int i = 0; i < newGokturWarlords; ++i )
			{
				GokturWarlord gokturwarlord = new GokturWarlord();

				gokturwarlord.Team = this.Team;
				gokturwarlord.FightMode = FightMode.Closest;

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
			
				gokturwarlord.MoveToWorld( loc, map );
				gokturwarlord.Combatant = target;
			}
		}
			
		
		public override void OnGotMeleeAttack( Mobile attacker )
		{
			base.OnGotMeleeAttack( attacker );
			if ( 0.05 >= Utility.RandomDouble() )
			SpawnGokturWarlord( attacker );

		}

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			base.OnGaveMeleeAttack( defender );
			this.PlaySound( 0x233 );
		}



		public override OppositionGroup OppositionGroup
		{
			get{ return OppositionGroup.SavagesAndOrcs; }
		}

		public GokturWarchief( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
