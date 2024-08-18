// Ignore Spelling: Feelgood, rtn
using Grass.Logic.Models;
namespace Grass.Logic;

[System.Diagnostics.DebuggerDisplay( "{Player.Name}" )]
internal class Decision
{
	#region Auto-Play

	private static int TotalHighUnprotected = 0;
	private static int TotalLowUnprotected = 0;
	internal static Player? FeelgoodInPlay = null;

	#region Properties and Constructor

	internal Player Player { get; private set; }
	internal Hand Hand { get; private set; }
	internal bool NotOpen => Hand.HasslePile.Count == 0;

	internal int TotalTabled => Hand.Protected + Hand.UnProtected;
	private Card? LowUnprotected => Hand.LowestUnProtected;
	private int LowValue => LowUnprotected is not null ? LowUnprotected.Info.Value : 0;
	private Card? HighUnprotected => Hand.HighestUnProtected;
	private int HighValue => HighUnprotected is not null ? HighUnprotected.Info.Value : 0;
	internal bool DrFeelgood => HighUnprotected is not null && HighUnprotected.Info.Id == CardInfo.cDrFeelgood;

	internal Decision( Player player )
	{
		Player = player;
		Hand = player.Current;
	}

	#endregion

	internal static void SetTotals( List<Decision> others )
	{
		TotalHighUnprotected = 0;
		TotalLowUnprotected = 0;
		FeelgoodInPlay = null;
		foreach( Decision d in others )
		{
			TotalLowUnprotected += d.LowValue;
			TotalHighUnprotected += d.HighValue;
			if( d.DrFeelgood && FeelgoodInPlay == null ) { FeelgoodInPlay = d.Player; }
		}
	}

	internal static Decision? Trade( Decision data, List<Decision> others, string required )
	{
		// Must have money in hand
		Card? low = Card.GetLowPeddle( data.Hand.Cards );
		if( low is null ) { return null; } // No money to trade
		Decision? other = null;

		// Another player must have the card to trade
		foreach( Decision trade in others )
		{
			List<Card> res = Card.GetCards( trade.Hand.Cards, required ).ToList();
			if( res.Count == 0 ) { continue; }
			switch( required )
			{
				case CardInfo.cOpen:
					// Must be open and have 1 in hand or not be open and have more than 1 in hand
					int num = trade.NotOpen ? 1 : 0;
					if( res.Count > num ) { other = trade; }
					break;
				default:
					if( res.Count > 0 ) { other = trade; }
					break;
			}
			if( other is not null ) { break; } // Found other player
		}
		return other;
	}

	internal static bool Close( Decision data, Game game )
	{
		// Must have score greater than target
		int score = data.TotalTabled + data.Player.Total + data.Hand.CurrentNet();
		int target = game.Target; // 200000; game.Target

		if( score > target ) { return true; }
		return false;
	}

	internal static Card? HeatOff( Decision data, Card? heatOn )
	{
		if( heatOn is null ) { return null; }
		List<Card> list = Card.GetCards( data.Hand.Cards, CardInfo.cHeatOff ).ToList();
		if( list.Count == 0 ) { return null; } // No cards in hand

		// Match heat-off card with heat-on
		Card? rtn = heatOn.Id switch
		{
			CardInfo.cOnBust => Card.GetFirst( list, CardInfo.cOffBust ),
			CardInfo.cOnDetained => Card.GetFirst( list, CardInfo.cOffDetained ),
			CardInfo.cOnFelony => Card.GetFirst( list, CardInfo.cOffFelony ),
			CardInfo.cOnSearch => Card.GetFirst( list, CardInfo.cOffSearch ),
			_ => null
		} ?? Card.GetFirst( list, CardInfo.cPayFine ); // Check for pay fine
		if( rtn is null ) { return null; }

		// cPayFine - Must have small unprotected card in stash
		//            worth using if large unprotected and high value peddle in hand
		if( rtn.Id == CardInfo.cPayFine )
		{
			Card? fine = data.LowUnprotected;
			if( fine is null || fine.Info.Value > 25000 ) { return null; }
		}

		return rtn;
	}

	internal static Card? Protect( Decision data, List<Card> peddles )
	{
		List<Card> cards = Card.GetCards( data.Hand.Cards, CardInfo.cProtection ).ToList();
		if( cards.Count == 0 ) { return null; }

		// Must have unprotected card in stash with same value
		// cCatchabuzz      single $25,000 or multiple lower values
		// cGrabasnack      single $25,000 or multiple lower values
		// cLustconquersall single $50,000 or multiple lower values

		Card? rtn = null;
		List<Card> stash = Card.GetUnprotected( data.Hand.StashPile );
		if( stash.Count == 0 ) { return null; }

		// Match unprotected stash card using highest protection value first
		foreach( Card protect in cards.OrderByDescending( x => x.Info.Value ).ToList() )
		{
			foreach( Card peddle in stash )
			{
				if( peddle.Info.Value != protect.Info.Value ) { continue; }
				peddles.Add( peddle );
				rtn = protect;
				break;
			}
			if( rtn is not null ) { break; }
		}
		return rtn;
	}

	internal static Card? Peddle( Decision data )
	{
		Card? rtn = data.LowValue == 0
			? Card.GetLowPeddle( data.Hand.Cards )
			: Card.GetHighPeddle( data.Hand.Cards );

		// Don't play Dr. Feelgood if no other unprotected cards in stack
		//return rtn is not null && rtn.Info.Id == CardInfo.cDrFeelgood ? null : rtn;

		return rtn;
	}

	internal static Dictionary<Player, Card?> Steal( Decision data )
	{
		Dictionary<Player, Card?> rtn = [];

		// Don't steal if no other unprotected cards in stack
		if( data.LowValue == 0 ) { return rtn; }

		// Currently only works for Dr. Feelgood 
		if( FeelgoodInPlay is null ) { return rtn; }
		Player other = FeelgoodInPlay;
		Card? steal = Card.GetFirst( other.Current.StashPile, CardInfo.cDrFeelgood );

		// My official rules state other CAN have heat on
		if( other.Current.MarketIsOpen ) { return rtn; }
		rtn.Add( other, steal );

		return rtn;
	}

	internal static Card? Nirvana( List<Card> list )
	{
		// cStonehigh should play if any player just has cDrFeelgood as only unprotected stash?
		//            should play if others have large amount of unprotected
		// cEuphoria  shouldn't play if very little other's unprotected
		//            should play if market not open and very little unprotected?
		//            shouldn't play if Dr. Feelgood not on table?

		Card? rtn = null;
		foreach( Card pick in list ) // Pick a card from the list
		{
			if( pick.Id == CardInfo.cEuphoria && TotalHighUnprotected > 105000 ) { rtn = pick; }
			else if( TotalLowUnprotected > 50000 ) { rtn = pick; }
			if( rtn is not null ) { break; }
		}
		return rtn;
	}

	internal static Decision? HeatOn( List<Decision> others )
	{
		// Find a player without heat on and a score greater than 55,000 
		int val = 55000;
		Decision? rtn = null;
		foreach( Decision other in others )
		{
			if( !other.Hand.MarketIsOpen ) { continue; }
			if( other.TotalTabled > val )
			{
				val = other.TotalTabled;
				rtn = other;
				continue;
			}
		}

		return rtn;
	}

	internal static Card? Paranoia( Decision data, List<Card> list )
	{
		// cSoldout     lowest tabled unprotected peddle should be 25,000 or less
		//              Utterly wiped out is worst card
		// cDoublecross Highest tabled unprotected peddle should be 50,000 or less
		//              Utterly wiped out is worst card
		// cWipedOut    Shouldn't play unless market open is available or Dr Feelgood is in stack
		//              Shouldn't play if large amount of unprotected

		Card? rtn = null;
		Card? pass = null;
		// Process in order of severity, cSoldout, cDoublecross, cWipedOut
		foreach( Card card in list.OrderByDescending( x => x.Info.Value ).ToList() )
		{
			pass = Actor.GetWorstCard( data.Hand.Cards, card );
			bool worse = ( pass is not null ) && ( pass.Id.StartsWith( CardInfo.cParanoia ) ) &&
				( pass.Info.Value < card.Info.Value );

			switch( card.Id ) // What if pass value is worse
			{
				case CardInfo.cSoldout:
					if( data.LowValue <= 25000 || worse )
					{
						// verified with lowest 5,000 and with 2 protected and no other
						if( data.LowUnprotected is not null ) { rtn = card; }
					}
					break;
				case CardInfo.cDoublecross:
					if( data.HighValue <= 50000 || worse ) 
					{
						if( data.HighUnprotected is not null ) { rtn = card; }
					}
					break;
				case CardInfo.cWipedOut:
					if( data.NotOpen || data.TotalTabled == 0 )
					{
						rtn = card; // Manually set to debug removal of hassle pile
					}
					break;
			}
			if( rtn is not null ) { break; }
		}

		return rtn;
	}

	#endregion

	#region Interactive

	private static Dictionary<Player, int> GetHasslePlayers( List<Player> others, Card card )
	{
		Dictionary<Player, int> rtn = [];
		foreach( Player other in others )
		{
			if( !other.Current.MarketIsOpen ) { continue; }
			int val = 0;
			if( card.Type == CardInfo.cHeatOn )
			{
				val = other.Total + other.Current.Protected + other.Current.UnProtected;
			}
			else
			{
				if( other.Current.HighestUnProtected is not null )
				{
					val = other.Current.HighestUnProtected.Info.Value;
				}
			}
			if( val >= Rules.cBonusAmount ) { rtn.Add( other, val ); }
		}
		if( rtn.Count > 0 ) { rtn = rtn.OrderByDescending( p => p.Value ).ToDictionary( p => p.Key, p => p.Value ); }
		return rtn;
	}

	private static List<Player> GetTradePlayers( List<Player> others, CardInfo trade, Card card )
	{
		List<Player> rtn = [];
		foreach( Player player in others )
		{
			Hand hand = player.Current;
			List<Card> cards = Card.GetCards( hand.Cards, trade.Id ).ToList();
			int count = trade.Id == CardInfo.cOpen && hand.HasslePile.Count == 0 ? 1 : 0;
			if( cards.Count > count )
			{
				PlayResult canPlay = PlayResult.Success!;
				if( card.Type == CardInfo.cHeatOff ) { canPlay = Rules.CanPlay( hand, card ); }
				if( canPlay == PlayResult.Success )
				{
					rtn.Add( player );
					player.ToDo = Player.Action.Trade;
				}
			}
		}
		return rtn;
	}

	internal static Dictionary<Player, int> CheckPlay( Game game, PlayOptions options )
	{
		Dictionary<Player, int> rtn = [];
		if( options.Player is null || options.Card is null ) { return rtn; }
		options.OtherCards.Clear();
		Player player = options.Player;
		Card card = options.Card;

		options.CanPlay = Rules.CanPlay( player.Current, card );
		List<Player> others = game.Players.Where( p => p != player ).ToList();

		CardInfo? trade = null;
		if( options.CanPlay != PlayResult.Success )
		{
			if( options.CanPlay.ErrorMessage == Rules.cNotActive ) // Try trade for Market Open
			{
				trade = CardInfo.GetCardInfo( CardInfo.cOpen );
				options.TradeFor = GetTradePlayers( others, trade, card ).Count > 0 ? trade : null;
			}
			else if( options.CanPlay.ErrorMessage == Rules.cNotOpen ) // Try trade for Heat Off
			{
				CardInfo? heatOff = CardInfo.GetHeatOffInfo( player.Current.HasslePile );
				if( heatOff is not null )
				{
					trade = heatOff;
					options.TradeFor = GetTradePlayers( others, trade, card ).Count > 0 ? trade : null;
				}
			}
			if( options.TradeFor is not null && trade is not null )
			{
				options.CanPlay = new( $"Possible trade for {trade.Caption}" );
			}
		}
		else
		{
			if( card.Type == CardInfo.cHeatOn || card.Id == CardInfo.cSteal )
			{
				rtn = GetHasslePlayers( others, card );
				if( rtn.Count == 0 ) { options.CanPlay = new( "No players to hassle." ); }
			}
			else if( card.Type == CardInfo.cProtection )
			{
				options.OtherCards = Card.GetPeddlesToProtect( player.Current.StashPile, card.Info.Value );
			}
		}
		return rtn;
	}

	internal static string? GetInfoMessage( Game game, PlayOptions options )
	{
		if( options.Card is null || options.Player is null ) { return null; }
		if( options.Player.ToDo == Player.Action.Trade )
		{
			options.TradeFor = null;
			if( game.TradeRq is not null && game.TradeRq.TradeFor is not null )
			{
				if( options.Card.Id != game.TradeRq.TradeFor.Id )
				{ return "This is not the correct card to trade."; }
			}
		}
		if( options.Card.Type == CardInfo.cProtection )
		{
			int total = 0;
			foreach( Card p in options.OtherCards ) { total += p.Info.Value; }
			if( total < options.Card.Info.Value )
			{ return $"Warning: Total protected value only {total:$###,##0}"; }
		}
		return null;
	}

	#endregion
}