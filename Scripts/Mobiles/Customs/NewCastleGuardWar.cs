using System; 
using System.Collections; 
using Server.Items; 
using Server.ContextMenus; 
using Server.Misc; 
using Server.Network; 

namespace Server.Mobiles 
{ 
	public class CastleGuardWarrior : BaseCreature 
	{ 

			private static bool m_Talked;
			string[] kfcsay = new string[]
			{
			"Stand thee down, citizen!",
			"Thou best approach no further!",
			};

		[Constructable] 
		public CastleGuardWarrior() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 ) 
		{ 
			SpeechHue = Utility.RandomDyedHue(); 
			
			Hue = Utility.RandomSkinHue(); 

			Title = "the Knight, Lord British Imperial Guard"; 

			SetStr( 1500, 1500 );
			SetDex( 150, 150 );
			SetInt( 61, 75 );

			SetSkill( SkillName.MagicResist, 120.0, 120.0 );
			SetSkill( SkillName.Swords, 120.0, 120.0 );
			SetSkill( SkillName.Tactics,120.0, 120.0 );
			SetSkill( SkillName.Anatomy, 120.0,120.0 );

			VikingSword weapon = new VikingSword();
			weapon.Hue = 2413;
			weapon.Movable = false;
			AddItem( weapon );

			MetalKiteShield shield = new MetalKiteShield();
			shield.Hue = 2413;
			shield.Movable = false;
			AddItem( shield );

			PlateHelm helm = new PlateHelm();
			helm.Hue = 2413;
			AddItem( helm );

			PlateArms arms = new PlateArms();
			arms.Hue = 2413;
			AddItem( arms );

			PlateGloves gloves = new PlateGloves();
			gloves.Hue = 2413;
			AddItem( gloves );

			PlateChest tunic = new PlateChest();
			tunic.Hue = 2413;
			AddItem( tunic );

			PlateLegs legs = new PlateLegs();
			legs.Hue = 2413;
			AddItem( legs );

			Body = 400; 			
			Name = NameList.RandomName( "male" ); 
				
			SetDamage( 45, 75 );

			VirtualArmor = 100;

			Utility.AssignRandomHair( this );

		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.FilthyRich );
			AddLoot( LootPack.Meager );
		}

		public override bool AlwaysMurderer{ get{ return true; } }

		public CastleGuardWarrior( Serial serial ) : base( serial ) 
		{ 
		}

		public override void OnMovement( Mobile m, Point3D oldLocation ) 
                {                                                    
         	if( m_Talked == false ) 
        	 { 
          	 	 if ( m.InRange( this, 1 ) ) 
          	 {                
          				m_Talked = true; 
              				SayRandom( kfcsay, this ); 
				this.Move( GetDirectionTo( m.Location ) ); 
				SpamTimer t = new SpamTimer(); 
				t.Start(); 
            			} 
		} 
		} 

		private class SpamTimer : Timer 
		{ 
		public SpamTimer() : base( TimeSpan.FromSeconds( 10 ) ) 
		{ 
			Priority = TimerPriority.OneSecond; 
		} 

		protected override void OnTick() 
		{ 
		m_Talked = false; 
		} 
		} 

		private static void SayRandom( string[] say, Mobile m ) 
		{ 
		m.Say( say[Utility.Random( say.Length )] ); 
		}

	        private static int GetRandomHue()
        	{
            	switch ( Utility.Random( 6 ) )
            	{
                default:
                case 0: return 0;
                case 1: return Utility.RandomBlueHue();
                case 2: return Utility.RandomGreenHue();
                case 3: return Utility.RandomRedHue();
                case 4: return Utility.RandomYellowHue();
                case 5: return Utility.RandomNeutralHue();
	            }
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
		} 
	} 
}