using System;
using System.Collections;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "a dead root" )]
	public class HungryRoot : BaseCreature
	{
		[Constructable]
		public HungryRoot() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a hungry root";
			Body = 8;
			BaseSoundID = 684;
			Hue = 1814;

			SetStr( 400, 450 );
			SetDex( 26, 45 );
			SetInt( 26, 40 );

			SetHits( 100, 125 );
			SetMana( 0 );

			SetDamage( 10, 23 );

			SetDamageType( ResistanceType.Physical, 60 );
			SetDamageType( ResistanceType.Poison, 40 );

			SetResistance( ResistanceType.Physical, 15, 20 );
			SetResistance( ResistanceType.Fire, 15, 25 );
			SetResistance( ResistanceType.Cold, 10, 20 );
			SetResistance( ResistanceType.Poison, 20, 30 );

			SetSkill( SkillName.MagicResist, 15.1, 20.0 );
			SetSkill( SkillName.Tactics, 45.1, 60.0 );
			SetSkill( SkillName.Wrestling, 45.1, 60.0 );

			Fame = 1000;
			Karma = -1000;

			VirtualArmor = 18;

			if ( 0.25 > Utility.RandomDouble() )
				PackItem( new Board( 10 ) );
			else
				PackItem( new Log( 10 ) );

			PackItem( new MandrakeRoot( 3 ) );
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Meager );
		}


		public HungryRoot( Serial serial ) : base( serial )
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

			if ( BaseSoundID == 352 )
				BaseSoundID = 684;
		}
	}
}
