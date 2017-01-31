using System;
using System.Collections;
using Server.Items;
using Server.Targeting;
using Server.Misc;

namespace Server.Mobiles
{
	[CorpseName( "an orc warlord corpse" )]
	public class GokturWarlord : BaseCreature
	{
	

		[Constructable]
		public GokturWarlord() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.01, 0.005 )
		{
			Name = NameList.RandomName( "orc" );
			Body = 0x190;
			BaseSoundID = 0x45A;
			Title = "the Goktur Clan War Chieftain";
			Hue = Utility.RandomMinMax( 2207,2212 );

			SetStr( 96, 120 );
			SetDex( 450, 500 );
			SetInt( 36, 60 );

			SetHits( 200, 300 );

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
			SetSkill( SkillName.Macing, 50.1, 70.0 );
			SetSkill( SkillName.Fencing, 50.1, 70.0 );

			Fame = 0;
			Karma = 0;

			VirtualArmor = 28;

			OrcishKinMask helm = new OrcishKinMask();
			helm.Hue = this.Hue;
			AddItem( helm );

			WarHammer warhammer = new WarHammer();
			warhammer.Hue = 0;
			AddItem ( warhammer );
			
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


		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Meager );
		}

		public override bool CanRummageCorpses{ get{ return true; } }
		public override int TreasureMapLevel{ get{ return 1; } }
		public override int Meat{ get{ return 1; } }
		public override bool AlwaysMurderer{ get{ return true; } }		

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			base.OnGaveMeleeAttack( defender );
			this.PlaySound( 0x233 );
			if ( 0.1 >= Utility.RandomDouble() )
			this.Say( true, String.Format( "For Hukor!" ) );

		}
		public override OppositionGroup OppositionGroup
		{
			get{ return OppositionGroup.SavagesAndOrcs; }
		}

		public GokturWarlord( Serial serial ) : base( serial )
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
