using System;
using System.Collections;
using Server.Misc;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "an orc outrider corpse" )]
	public class GokturMageOutrider : BaseCreature
	{
		public override InhumanSpeech SpeechType{ get{ return InhumanSpeech.Orc; } }

		[Constructable]
		public GokturMageOutrider() : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.01, 0.005 )
		{
			Name = NameList.RandomName( "orc" );
			Body = 0x190;
			BaseSoundID = 0x45A;
			Title = "the Goktur Clan Outrider";
			Hue = Utility.RandomMinMax( 2207,2212 );

			SetStr( 146, 180 );
			SetDex( 101, 130 );
			SetInt( 116, 140 );

			SetHits( 88, 108 );

			SetDamage( 4, 10 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 40, 55 );
			SetResistance( ResistanceType.Fire, 10, 20 );
			SetResistance( ResistanceType.Cold, 10, 20 );
			SetResistance( ResistanceType.Poison, 10, 20 );
			SetResistance( ResistanceType.Energy, 10, 20 );

			SetSkill( SkillName.Anatomy, 60.2, 100.0 );
			SetSkill( SkillName.Archery, 80.1, 90.0 );
			SetSkill( SkillName.MagicResist, 65.1, 90.0 );
			SetSkill( SkillName.Tactics, 50.1, 75.0 );
			SetSkill( SkillName.Wrestling, 50.1, 75.0 );
			SetSkill( SkillName.Wrestling, 80.1, 90.0 );

			Fame = 0;
			Karma = 0;

			VirtualArmor = 56;

			AddItem( new Spellbook() );
		


			OrcishKinMask helm = new OrcishKinMask();
			helm.Hue = this.Hue;
			AddItem( helm );

			Boots boots = new Boots();
			boots.Hue = 0;
			AddItem ( boots );

			Cloak cloak = new Cloak();
			cloak.Hue = 637;
			AddItem ( cloak );			
			new CuSidhe().Rider = this;

		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Rich );
		}

		public override bool CanRummageCorpses{ get{ return true; } }
		public override bool AlwaysMurderer{ get{ return true; } }
		public GokturMageOutrider( Serial serial ) : base( serial )
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
