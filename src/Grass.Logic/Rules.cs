using Grass.Logic.Models;
namespace Grass.Logic;

internal class Rules
{
	internal const int cCardTotal = 104;
	internal const int cMaxNumber = 6; // Maximum players and cards in hand
	internal const int cBonusAmount = 25000;

	private const string cDiscard = "Can only discard.";
	private const string cIsActive = "Market is already active.";
	internal const string cPlayOption = "Missing player or card option.";
	private const string cNoHeatOn = "No heat on.";
	private const string cNoHeatOnMatch = "No matching heat on.";
	private const string cNoMoney = "No money to pay fine.";
	private const string cNoPeddle = "No peddle to protect.";
	internal const string cNotActive = "Market must be active.";
	internal const string cNotOpen = "Market is not open.";
	private const string cNotProtection = "Card is not of type protection.";
	private const string cStealNotFound = "Card to steal not found.";
	private const string cTradeNotFound = "Card to trade not found.";

	#region Internal Methods

	internal static bool IsMarketOpen( List<Card> list )
	{
		Card? card = list.LastOrDefault(); // Null if not active
		if( card is null || card.Id is not CardInfo.cOpen ) { return false; }
		return true;
	}

	internal static bool RemoveHeat( Hand hand )
	{
		Card? card = Card.GetFirst( hand.HasslePile, CardInfo.cOpen );
		if( card is null || hand.MarketIsOpen ) { return false; }

		bool rtn = hand.HasslePile.Remove( card );
		if( rtn ) { hand.HasslePile.Add( card ); }
		return rtn;
	}

	internal static int SkimAmt( Hand hand, Player banker )
	{
		if( hand == banker.Current ) { return 0; }
		int rtn = hand.UnProtected / 100 * 20; // Banker skims 20% of unprotected in stash pile
		return rtn;
	}

	internal static PlayResult CanPlay( Hand hand, Card card )
	{
		switch( card.Type )
		{
			case CardInfo.cPeddle:
				if( hand.HasslePile.Count == 0 ) { return new( cNotActive ); }
				if( !hand.MarketIsOpen ) { return new( cNotOpen ); }
				break;
			case CardInfo.cProtection:
				if( hand.HasslePile.Count == 0 ) { return new( cNotActive ); }
				if( !hand.MarketIsOpen ) { return new( cNotOpen ); }
				List<Card> peddles = Card.GetPeddlesToProtect( hand.StashPile, card.Info.Value );
				if( peddles.Count == 0 ) { return new( cNoPeddle ); }
				break;
			case CardInfo.cHeatOff:
				if( hand.HasslePile.Count == 0 ) { return new( cNotActive ); }
				if( hand.MarketIsOpen ) { return new( cNoHeatOn ); }
				if( card.Id != CardInfo.cPayFine )
				{
					CardInfo? heatoff = CardInfo.GetHeatOffInfo( hand.HasslePile );
					if( heatoff is null || heatoff.Id != card.Id ) { return new( cNoHeatOnMatch ); }
				}
				else { if( hand.LowestUnProtected is null ) { return new( cNoMoney ); } }
				break;
			case CardInfo.cNirvana:
				if( hand.HasslePile.Count == 0 ) { return new( cNotActive ); }
				break;
			case CardInfo.cHeatOn:
				if( !hand.MarketIsOpen ) { return new( cNotOpen ); }
				break;
			case CardInfo.cParanoia:
				break;
			default: // Individual cards
				if( card.Id == CardInfo.cOpen && hand.HasslePile.Count > 0 )
				{ return new( cIsActive ); }
				if( card.Id is CardInfo.cClose or CardInfo.cSteal )
				{ if( !hand.MarketIsOpen ) { return new( cNotOpen ); } }
				if( card.Id == CardInfo.cBanker ) { return new( cDiscard ); }
				break;
		}
		return PlayResult.Success!;
	}

	// Single player
	internal static PlayResult Play( Game game, Player player, Card card )
	{
		PlayResult res = game.CheckState( card );
		if( res != PlayResult.Success ) { return res; }
		Hand hand = player.Current;
		res = CanPlay( hand, card );
		if( res != PlayResult.Success ) { return res; }

		bool comment = game.Comment;
		bool ok;
		switch( card.Type )
		{
			case CardInfo.cPeddle:
				//if( comment ) { card.AddComment( $"played (round {hand.Round})" ); }
				ok = Card.TransferCard( hand.Cards, hand.StashPile, card );
				if( ok ) { return res; }
				break;
			case CardInfo.cHeatOff:
				ok = Card.TransferCard( hand.Cards, hand.HasslePile, card );
				if( ok )
				{
					if( comment ) { card.AddComment( $"played (round {hand.Round})" ); }
					ok = RemoveHeat( hand );
					if( ok && card.Id is CardInfo.cPayFine && hand.LowestUnProtected is not null )
					{
						Card fine = hand.LowestUnProtected;
						Card.TransferCard( hand.StashPile, game.WastedPile, fine );
						if( ok & comment ) { fine.AddComment( $"{player.Name} paid fine (round {hand.Round})" ); }
					}
					if( ok ) { return res; }
				}
				break;
			case CardInfo.cNirvana:
				ok = Nirvana( game, player, card, comment );
				if( ok ) { return res; }
				break;
			case CardInfo.cParanoia:
				ok = Paranoia( game, player, card, comment );
				if( ok ) { return res; }
				break;
			default: // Individual cards
				if( card.Id is CardInfo.cOpen or CardInfo.cClose )
				{
					ok = Card.TransferCard( hand.Cards, hand.HasslePile, card );
					if( ok && comment && card.Comment.Length == 0 ) { card.AddComment( $"(round {hand.Round})" ); }
					if( ok ) { return res; }
				}
				break;
		}
		return new( $"Unable to process {card.Info.Caption}" );
	}

	// Multi player
	internal static PlayResult Play( Game game, Player player, Card card, Player with, Card other )
	{
		PlayResult res = game.CheckState( card );
		if( res != PlayResult.Success ) { return res; }

		bool comment = game.Comment;
		Hand hand = player.Current;
		Hand wHand = with.Current;
		switch( card.Id )
		{
			case CardInfo.cSteal:
				// Both hands must be hassle free
				//if( !hand.MarketIsOpen || !otherHand.MarketIsOpen ) { return false; }
				if( CanPlay( hand, card ) != PlayResult.Success ) { return new( cNotOpen ); }
				if( !wHand.StashPile.Remove( other ) ) { return new( cStealNotFound ); }
				Card.TransferCard( hand.Cards, wHand.HasslePile, card );
				if( comment ) { card.AddComment( player.Name + $" stole {other.Info.Caption} (round {hand.Round})" ); }
				RemoveHeat( wHand ); // Must reset Market Open as Steal card added to Hassle pile
				hand.StashPile.Add( other );
				//if( comment ) { other.AddComment( $"stole from {with.Name} (round {hand.Round})" ); }
				break;
			default: // Heat On
				if( card.Type is CardInfo.cHeatOn )
				{
					if( CanPlay( wHand, card ) != PlayResult.Success ) { return new( cNotOpen ); }
					Card.TransferCard( hand.Cards, wHand.HasslePile, card );
					if( comment )
					{
						int total = wHand.Protected + wHand.UnProtected;
						card.AddComment( $"by {player.Name}, stash was {total:###,##0} (round {hand.Round})" );
					}
					break;
				}
				break;
		}
		return PlayResult.Success!;
	}

	internal static PlayResult Trade( Game game, Player player, Card card, Player with, Card other )
	{
		bool comment = game.Comment;
		Hand hand = player.Current;
		Hand wHand = with.Current;

		if( !wHand.Cards.Contains( other ) ) { return new( cTradeNotFound ); }
		if( !hand.Cards.Contains( card ) ) { return new( cTradeNotFound ); }
		Card.TransferCard( wHand.Cards, hand.Cards, other );
		Card.TransferCard( hand.Cards, wHand.Cards, card );
		if( comment ) { other.AddComment( $"trade with {with.Name} (round {hand.Round})" ); }

		return PlayResult.Success!;
	}

	internal static PlayResult Protect( Game game, Player player, Card card, List<Card> peddles )
	{
		if( card.Type != CardInfo.cProtection ) { return new( cNotProtection ); }
		PlayResult res = game.CheckState( card );
		if( res != PlayResult.Success ) { return res; }
		Hand hand = player.Current;
		foreach( Card c in peddles )
		{
			c.Protected = true;
			//if( game.Comment ) { c.AddComment( $"protected (round {hand.Round})" ); }
		}
		Card.TransferCard( hand.Cards, hand.StashPile, card );
		if( game.Comment ) { card.AddComment( $"played (round {hand.Round})" ); }

		return res;
	}

	internal static void PassCards( Game game, Dictionary<Player, Card> cardsToPass )
	{
		if( game.ParanoiaPlayer is null ) { return; }
		Player player = game.ParanoiaPlayer;

		// Pass in order of play starting with the paranoia player
		int start = game.PlayOrder.FindIndex( x => x == player );
		int count = 0;
		for( int i = start; count < game.PlayOrder.Count; i++ )
		{
			count++;
			if( i == game.PlayOrder.Count ) { i = 0; }
			Player from = game.PlayOrder[i];
			Card pass = cardsToPass[from];

			int next = i + 1;
			if( next == game.PlayOrder.Count ) { next = 0; }
			Player to = game.PlayOrder[next];

			Card.TransferCard( from.Current.Cards, to.Current.Cards, pass );
			if( game.Comment ) { pass.AddComment( $"passed by {from.Name} to {to.Name} (round {player.Current.Round})" ); }
		}
	}

	#endregion

	private static bool Nirvana( Game game, Player player, Card card, bool comment )
	{
		Hand hand = player.Current;
		bool low = true;
		switch( card.Id )
		{
			case CardInfo.cStonehigh:
				hand.Turns += 1; // 1 extra turn
				low = true; // Lowest tabled unprotected peddle for all others
				break;
			case CardInfo.cEuphoria:
				hand.Turns += 2; // 2 extra turns
				low = false; // Highest tabled unprotected peddle for all others
				break;
		}

		List<Card> transfer = hand.MarketIsOpen ? hand.StashPile : hand.HasslePile;
		Card.TransferCard( hand.Cards, transfer, card );

		_ = RemoveHeat( hand ); // Cancel any heat on
		List<string> names = [];
		foreach( Player other in game.Players ) // Get cards from other players
		{
			if( other.Equals( player ) ) { continue; }
			Card? steal = low ? other.Current.LowestUnProtected : other.Current.HighestUnProtected;
			if( steal is not null )
			{
				names.Add( $"{other.Name}: {steal.Info.Value:#,###,##0}" );
				Card.TransferCard( other.Current.StashPile, hand.StashPile, steal );
				//if( comment ) { steal.AddComment( $"from {other.Name} (round {hand.Round})" ); }
			}
			else { names.Add( $"{other.Name}: nothing" ); }
		}
		if( comment ) { card.AddComment( $"{string.Join( ", ", names )} (round {hand.Round})" ); }
		return true;
	}

	private static bool Paranoia( Game game, Player player, Card card, bool comment )
	{
		Hand hand = player.Current;
		List<Card> lose = [];
		switch( card.Id )
		{
			case CardInfo.cSoldout:
				hand.Turns -= 1; // Miss 1 turn and lose lowest unprotected peddle
				if( hand.LowestUnProtected is not null ) { lose.Add( hand.LowestUnProtected ); }
				break;
			case CardInfo.cDoublecross:
				hand.Turns -= 2; // Miss 2 turns and lose highest unprotected peddle
				if( hand.HighestUnProtected is not null ) { lose.Add( hand.HighestUnProtected ); }
				break;
			case CardInfo.cWipedOut:
				hand.Turns -= 2; // Miss 2 turns and all unprotected in stash pile
				lose.AddRange( hand.StashPile.Where( c => c.Id.StartsWith( CardInfo.cPeddle ) && !c.Protected ).ToList() );
				break;
		}

		string text = $"{player.Name} played (round {hand.Round})";
		Card.TransferCard( hand.Cards, game.WastedPile, card );
		if( comment ) { card.AddComment( text ); }

		text = $"{card.Info.Caption} played (round {hand.Round})";
		foreach( Card waste in lose ) // Remove card(s) from stash pile
		{
			Card.TransferCard( hand.StashPile, game.WastedPile, waste );
			if( comment ) { waste.AddComment( text ); }
		}
		if( card.Id == CardInfo.cWipedOut ) 
		{
			// Remove cards from hassle pile (including market open)
			List<Card> hassle = new( hand.HasslePile );
			foreach( Card waste in hassle )
			{
				Card.TransferCard( hand.HasslePile, game.WastedPile, waste );
				if( comment ) { waste.AddComment( text ); }
			}
		}

		game.ParanoiaPlayer = player; // Do this last as it triggers the game change handler
		return true;
	}
}