using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a molten guardian corpse" )]
	public class NewMoltenGuardian : BaseCreature
	{
		public override double DispelDifficulty{ get{ return 117.5; } }
		public override double DispelFocus{ get{ return 45.0; } }

		[Constructable]
		public NewMoltenGuardian () : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a molten guardian";
			Body = 15;
			BaseSoundID = 838;

			SetStr( 25, 35 );
			SetDex( 500, 600 );
			SetInt( 86, 125 );

			SetHits( 3000, 3500 );

			SetDamage( 1, 2 );


			SetDamageType( ResistanceType.Physical, 25 );
			SetDamageType( ResistanceType.Fire, 75 );

			SetResistance( ResistanceType.Physical, 35, 45 );
			SetResistance( ResistanceType.Fire, 60, 80 );
			SetResistance( ResistanceType.Cold, 5, 10 );
			SetResistance( ResistanceType.Poison, 30, 40 );
			SetResistance( ResistanceType.Energy, 30, 40 );

			SetSkill( SkillName.EvalInt, 60.1, 75.0 );
			SetSkill( SkillName.Magery, 60.1, 75.0 );
			SetSkill( SkillName.MagicResist, 75.2, 105.0 );
			SetSkill( SkillName.Tactics, 80.1, 100.0 );
			SetSkill( SkillName.Wrestling, 10.1, 20.1 );

			Fame = 4500;
			Karma = -4500;

			VirtualArmor = 40;
			ControlSlots = 4;

			PackItem( new SulfurousAsh( 3 ) );

			AddItem( new LightSource() );
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Average );
			AddLoot( LootPack.Meager );
			AddLoot( LootPack.Gems );
		}

		public override bool BleedImmune{ get{ return true; } }
		public override int TreasureMapLevel{ get{ return 2; } }

		public override void OnGotMeleeAttack( Mobile attacker )
		{
			base.OnGotMeleeAttack( attacker );
			attacker.Damage( Utility.Random( 10, 10 ), this );
			attacker.FixedParticles( 0x3709, 20, 10, 5044, EffectLayer.RightFoot );
                        attacker.PlaySound( 0x21F );
                        attacker.FixedParticles( 0x36BD, 10, 30, 5052, EffectLayer.Head );
                        attacker.PlaySound( 0x208 );
		}
		public override void OnGaveMeleeAttack( Mobile defender )
		{
			base.OnGaveMeleeAttack( defender );
			defender.Damage( Utility.Random( 10, 10 ), this );
			Animate( BreathAngerAnimation, 5, 1, true, false, 0 );			
			defender.FixedParticles( 0x3709, 10, 30, 5052, EffectLayer.LeftFoot );
		}
		public NewMoltenGuardian( Serial serial ) : base( serial )
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

			if ( BaseSoundID == 274 )
				BaseSoundID = 838;
		}
	}
}
