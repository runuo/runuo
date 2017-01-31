using System;
using System.Collections;
using Server.Items;
using Server.Targeting;
using Server.Misc;

namespace Server.Mobiles
{
	[CorpseName( "an orc taskmaster corpse" )]
	public class GokturTaskmaster : BaseCreature
	{
		public override InhumanSpeech SpeechType{ get{ return InhumanSpeech.Orc; } }

		[Constructable]
		public GokturTaskmaster() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = NameList.RandomName( "orc" );
			Body = 0x190;
			BaseSoundID = 0x45A;
			Title = "the Goktur Tribe Taskmaster";
			Hue = 2211;

			SetStr( 96, 120 );
			SetDex( 81, 105 );
			SetInt( 36, 60 );

			SetHits( 58, 72 );

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

			OrcishKinMask helm = new OrcishKinMask();
			helm.Hue = 2211;
			AddItem( helm );

			Lantern lantern = new Lantern();
			lantern.Hue = 0;
			lantern.ItemID = 0xA25;
			AddItem ( lantern );
			
			FullApron fullapron = new FullApron();
			fullapron.Hue = 0;
			AddItem ( fullapron );

			Sandals sandals = new Sandals();
			sandals.Hue = 0;
			AddItem ( sandals );
			
			TaskmastersMotivator taskmastersmotivator = new TaskmastersMotivator();
			taskmastersmotivator.Hue = 0;
			AddItem ( taskmastersmotivator );

	
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Meager );
		}

		public override bool CanRummageCorpses{ get{ return true; } }
		public override int TreasureMapLevel{ get{ return 1; } }
		public override int Meat{ get{ return 1; } }
		public override bool AlwaysMurderer{ get{ return true; } }		

		public override OppositionGroup OppositionGroup
		{
			get{ return OppositionGroup.SavagesAndOrcs; }
		}

		public GokturTaskmaster( Serial serial ) : base( serial )
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
