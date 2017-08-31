/***************************************************************************
 *                          SecureTradeContainer.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id$
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

using System;

using Server.Accounting;
using Server.Network;

namespace Server.Items
{
	public class SecureTradeContainer : Container
	{
		private SecureTrade m_Trade;

		public SecureTrade Trade
		{
			get
			{
				return m_Trade;
			}
		}

		public SecureTradeContainer( SecureTrade trade ) : base( 0x1E5E )
		{
			m_Trade = trade;

			Movable = false;
		}

		public SecureTradeContainer( Serial serial ) : base( serial )
		{
		}

		public override bool CheckHold( Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight )
		{
			if (item == Trade.From.VirtualCheck || item == Trade.To.VirtualCheck)
			{
				return true;
			}

			Mobile to;

			if ( this.Trade.From.Container != this )
				to = this.Trade.From.Mobile;
			else
				to = this.Trade.To.Mobile;

			return m.CheckTrade( to, item, this, message, checkItems, plusItems, plusWeight );
		}

		public override bool CheckLift( Mobile from, Item item, ref LRReason reject )
		{
			reject = LRReason.CannotLift;
			return false;
		}

		public override bool IsAccessibleTo( Mobile check )
		{
			if ( !IsChildOf( check ) || m_Trade == null || !m_Trade.Valid )
				return false;

			return base.IsAccessibleTo( check );
		}

		public override void OnItemAdded( Item item )
		{
			if ( !(item is VirtualCheck) )
			{
				ClearChecks();
			}
		}

		public override void OnItemRemoved( Item item )
		{
			if ( !(item is VirtualCheck) )
			{
				ClearChecks();
			}
		}

		public override void OnSubItemAdded( Item item )
		{
			if ( !(item is VirtualCheck) )
			{
				ClearChecks();
			}
		}

		public override void OnSubItemRemoved( Item item )
		{
			if ( !(item is VirtualCheck) )
			{
				ClearChecks();
			}
		}

		public void ClearChecks( )
		{
			if ( m_Trade != null )
			{
				if ( m_Trade.From != null && !m_Trade.From.IsDisposed )
				{
					m_Trade.From.Accepted = false;
				}

				if ( m_Trade.To != null && !m_Trade.To.IsDisposed )
				{
					m_Trade.To.Accepted = false;
				}

				m_Trade.Update();
			}
		}

		public override bool IsChildVisibleTo( Mobile m, Item child )
		{
			if (child is VirtualCheck)
			{
				return AccountGold.Enabled && (m.NetState == null || !m.NetState.NewSecureTrading);
			}

			return base.IsChildVisibleTo(m, child);
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