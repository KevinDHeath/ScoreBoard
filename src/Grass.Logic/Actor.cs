// Ignore Spelling: Feelgood
using Grass.Logic.Models;
namespace Grass.Logic;

/// <summary>Auto-play Actor.</summary>
internal class Actor : PassCardHandler
{
	#region Constructor and Methods

	private readonly Game _game;

	internal Actor( Game game ) : base()
	{
		_game = game;
		_game.GameChanged += OnParanoiaPlayed;
	}

	/// <inheritdoc/>
	public override void Dispose()
	{
		_game.GameChanged -= OnParanoiaPlayed;
		base.Dispose();
	}

	#endregion

	internal bool Play( Hand current )
	{
		return PlayRound( _game, current );
	}

	private static bool PlayRound( Game game, Hand current )
	{
		#region Populate decision data

		Decision? data = null;
		List<Decision> others = [];
		foreach( Player p in game.Players )
		{
			Decision temp = new( p );
			if( p.Current == current ) { data = temp; }
			else { others.Add( temp ); }
		}
		if( data is null ) { return false; }

		Decision.SetTotals( others );

		#endregion

		// If picked up Utterly wiped out try and use paranoia to get rid of it

		if( !game.Take( data.Hand ) ) { return false; } // Game over (no cards left in stack)

		Card? card = Open( game, data, others );  // Market needs to be open
		card ??= HeatOff( game, data, others );   // Heat needs to be removed
		card ??= Close( game, data );             // Game over - close market
		card ??= Protect( game, data );           // Protection
		card ??= Peddle( game, data );            // Peddle
		card ??= Steal( game, data, others );     // Steal
		card ??= Nirvana( game, data );           // Nirvana
		card ??= HeatOn( game, data, others );    // Heat on
		card ??= Paranoia( game, data );          // Paranoia
		if( card is not null && card.Id == CardInfo.cClose ) { return false; } // Game over (closed)
		bool ok = card != null;

		if( !ok ) { ok = Decision.Discard( data.Player, game ); }

		if( data.Hand.Cards.Count != Rules.cMaxNumber ) { ok = false; }
		foreach( Decision d in others ) { if( d.Hand.Cards.Count != Rules.cMaxNumber ) { ok = false; } }
		return ok;
	}

	#region Single Player Methods

	private static Card? Open( Game game, Decision data, List<Decision> others )
	{
		if( !data.NotOpen ) return null; // Already opened
		Card? card = CardInfo.GetFirst( data.Hand.Cards, CardInfo.cOpen );
		card ??= Trade( game, data, others, CardInfo.cOpen );
		if( card is null ) { return null; }

		PlayResult res = game.Play( data.Player, card );
		return res == PlayResult.Success ? card : null;
	}

	private static Card? HeatOff( Game game, Decision data, List<Decision> others )
	{
		if( data.Hand.MarketIsOpen ) { return null; } // Market must have no heat

		Card? heat = data.Hand.HasslePile.LastOrDefault();
		if( heat is null ) { return null; }
		List<Card> list = CardInfo.GetCards( data.Hand.Cards, CardInfo.cHeatOff ).ToList();
		if( list.Count == 0 ) // No cards in hand - try and make trade
		{
			string name = heat.Id.Replace( CardInfo.cHeatOn, CardInfo.cHeatOff );
			Card? trade = Trade( game, data, others, name );
			if( trade is null ) { return null; }
		}

		Card? card = Decision.HeatOff( data, heat );
		if( card is null ) { return null; }

		PlayResult res = game.Play( data.Player, card );
		return res == PlayResult.Success ? card : null;
	}

	private static Card? Close( Game game, Decision data )
	{
		Card? card = CardInfo.GetFirst( data.Hand.Cards, CardInfo.cClose );
		if( card is null ) { return null; } // No card in hand
		if( !Decision.Close( data, game ) ) { return null; }

		PlayResult res = game.Play( data.Player, card );
		return res == PlayResult.Success ? card : null;
	}

	private static Card? Peddle( Game game, Decision data )
	{
		if( data.NotOpen ) return null; // Never opened
		Card? card = Decision.Peddle( data );
		if( card is null ) { return null; } // No card in hand

		PlayResult res = game.Play( data.Player, card );
		return res == PlayResult.Success ? card : null;
	}

	private static Card? Protect( Game game, Decision data )
	{
		if( !data.Hand.MarketIsOpen ) { return null; }

		List<Card> peddles = [];
		Card? card = Decision.Protect( data, peddles );
		if( card is null ) { return null; } // No cards to play

		PlayResult res = Rules.CanPlay( data.Player.Current, card );
		if( res != PlayResult.Success ) { return null; } // Cannot play
		res = Rules.Protect( game, data.Player, card, peddles );
		//res = game.Protect( data.Player, card, peddles );
		return res == PlayResult.Success ? card : null;
	}

	private static Card? Nirvana( Game game, Decision data )
	{
		if( data.NotOpen ) return null; // Never opened
		List<Card> list = CardInfo.GetCards( data.Hand.Cards, CardInfo.cNirvana ).ToList();
		if( list.Count == 0 ) { return null; } // No cards in hand

		Card? card = Decision.Nirvana( list );
		if( card is null ) { return null; } // No card to play

		PlayResult res = game.Play( data.Player, card );
		if( data.DrFeelgood ) { Decision.FeelgoodInPlay = data.Player; }
		return res == PlayResult.Success ? card : null;
	}

	private static Card? Paranoia( Game game, Decision data )
	{
		//if( !Rules.CanPlayCard( data.Hand, CardInfo.cParanoia ) ) { return null; }

		List<Card> list = CardInfo.Paranoia( data.Hand.Cards ).ToList();
		if( list.Count == 0 ) { return null; } // No card in hand

		Card? card = Decision.Paranoia( data, list );
		if( card is null ) { return null; }

		PlayResult res = game.Play( data.Player, card );
		return res == PlayResult.Success ? card : null;
	}

	#endregion

	#region Multi Player Methods

	private static Card? HeatOn( Game game, Decision data, List<Decision> others )
	{
		List<Card> list = CardInfo.GetCards( data.Hand.Cards, CardInfo.cHeatOn ).ToList();
		if( list.Count == 0 ) { return null; } // No card in hand

		Decision? other = Decision.HeatOn( others );
		if( other is null || !other.Hand.MarketIsOpen ) { return null; } // No card to play

		Card card = list[0];
		PlayResult res = game.Play( data.Player, card, other.Player, card );
		return res == PlayResult.Success ? card : null;
	}

	private static Card? Steal( Game game, Decision data, List<Decision> others )
	{
		Card? rtn = CardInfo.GetFirst( data.Hand.Cards, CardInfo.cSteal );
		if( rtn is null ) { return null; } // No card in hand
		PlayResult res = Rules.CanPlay( data.Player.Current, rtn );
		if( res != PlayResult.Success ) { return null; } // Cannot play

		Dictionary<Player, Card?> steal = Decision.Steal( data, others );
		if( steal.Count == 0 ) { return null; }

		KeyValuePair<Player, Card?> test = steal.First();
		Player hand = test.Key;
		Card? card = test.Value;
		if( card is null ) { return null; }

		res = game.Play( data.Player, rtn, hand, card );
		bool ok = res == PlayResult.Success;
		if( ok && card.Id is CardInfo.cDrFeelgood ) { Decision.FeelgoodInPlay = data.Player; }
		return ok ? rtn : null;
	}

	private static Card? Trade( Game game, Decision data, List<Decision> others, string cardName )
	{
		if( string.IsNullOrWhiteSpace( cardName ) ) { return null; }
		Decision? other = Decision.Trade( data, others, cardName );
		if( other is null ) { return null; } // No other player

		Card? low = Card.GetLowPeddle( data.Hand.Cards );
		if( low is null ) { return null; } // No low unprotected
		Card? card = CardInfo.GetFirst( other.Hand.Cards, cardName );
		if( card is null ) { return null; }

		PlayResult res = game.Play( data.Player, low, other.Player, card );
		return res == PlayResult.Success ? card : null;
	}

	#endregion
}