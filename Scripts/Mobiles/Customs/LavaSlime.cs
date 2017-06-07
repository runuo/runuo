using System;
using System.Collections;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "cooled lava" )]
	public class LavaSlime : BaseCreature
	{
		[Constructable]
		public LavaSlime() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "molten lava";
			Body = 51;
			BaseSoundID = 838;

			Hue = 1161;

			SetStr( 22, 34 );
			SetDex( 16, 21 );
			SetInt( 16, 20 );

			SetHits( 15, 19 );

			SetDamage( 1, 5 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 5, 10 );
			SetResistance( ResistanceType.Poison, 10, 20 );

			SetSkill( SkillName.Poisoning, 30.1, 50.0 );
			SetSkill( SkillName.MagicResist, 15.1, 20.0 );
			SetSkill( SkillName.Tactics, 19.3, 34.0 );
			SetSkill( SkillName.Wrestling, 19.3, 34.0 );

			Fame = 300;
			Karma = -300;

			VirtualArmor = 8;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 23.1;
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Poor );
			AddLoot( LootPack.Gems );
		}

		public override Poison PoisonImmune{ get{ return Poison.Lethal; } }
		public override FoodType FavoriteFood{ get{ return FoodType.Meat | FoodType.Fish | FoodType.FruitsAndVegies | FoodType.GrainsAndHay | FoodType.Eggs; } }

		public LavaSlime( Serial serial ) : base( serial )
		{
		}

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			base.OnGaveMeleeAttack( defender );
			defender.Damage( Utility.Random( 10, 10 ), this );
			Animate( BreathAngerAnimation, 5, 1, true, false, 0 );			
			defender.FixedParticles( 0x3709, 10, 30, 5052, EffectLayer.LeftFoot );
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
