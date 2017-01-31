using System;
using System.Collections;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "a bonded earth spirit corpse" )]
	public class BondedEarth : BaseCreature
	{

		[Constructable]
		public BondedEarth() : base( AIType.AI_Healer, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a bonded earth spirit";
			Body = 14;
			BaseSoundID = 268;
			Hue = 2412;
		
			SetStr( 126, 155 );
			SetDex( 66, 85 );
			SetInt( 71, 92 );

			SetHits( 250, 300 );

			SetDamage( 9, 16 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 30, 35 );
			SetResistance( ResistanceType.Fire, 10, 20 );
			SetResistance( ResistanceType.Cold, 10, 20 );
			SetResistance( ResistanceType.Poison, 15, 25 );
			SetResistance( ResistanceType.Energy, 15, 25 );

			SetSkill( SkillName.MagicResist, 50.1, 95.0 );
			SetSkill( SkillName.Tactics, 60.1, 100.0 );
			SetSkill( SkillName.Wrestling, 60.1, 100.0 );
			SetSkill( SkillName.Magery, 90.1, 100.0 );

			Fame = 0;
			Karma = 0;

			VirtualArmor = 34;
			ControlSlots = 2;

			PackItem( new FertileDirt( Utility.RandomMinMax( 1, 4 ) ) );
			PackItem( new MandrakeRoot() );
			
			Item ore = new IronOre( 5 );
			ore.ItemID = 0x19B7;
			PackItem( ore );
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Average );
			AddLoot( LootPack.Meager );
			AddLoot( LootPack.Gems );
		}

		public override bool BleedImmune{ get{ return true; } }

		public BondedEarth( Serial serial ) : base( serial )
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