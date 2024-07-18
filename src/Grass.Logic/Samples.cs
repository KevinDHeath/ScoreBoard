using Grass.Logic.Models;
namespace Grass.Logic;

/// <summary>Static Game</summary>
[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
public class Samples
{
	private static Game sGame = default!;

	/// <summary>Populate data</summary>
	public static Game Populate( GameService service, bool endgame = false )
	{
		sGame = service._game;
		sGame.Date = "July 9, 2024 10:31 PM";
		Player? dealer = service._game.Players.FirstOrDefault( p => p.Name == "Bob" );
		if( dealer is not null ) { service._game.Dealer = dealer; }
		sGame.StartHand();

		// Put dealt cards back in the stack
		foreach( Player p in sGame.PlayOrder ) { BackToStack( p.Current.Cards ); }
		Player? janis = null, john = null, amy = null, bob = null;

		#region To end of round 13
		foreach( Player player in sGame.Players )
		{
			Hand hand = player.Current;
			hand.Round = 13;
			Card? card = null;
			List <Card> to = [];
			switch( player.Name )
			{
				case "Janis":
					janis = player;
					sGame.Take( hand, CardInfo.cClose );
					sGame.Take( hand, CardInfo.cClose );
					sGame.Take( hand, CardInfo.cBanker );
					sGame.Take( hand, CardInfo.cOffFelony );
					sGame.Take( hand, CardInfo.cSoldout );
					sGame.Take( hand, CardInfo.cOffFelony );
					card = hand.Cards.LastOrDefault( c => c.Id == CardInfo.cOffFelony );
					card?.AddComment( "passed by Bob to Janis (round 9)" );

					to = hand.HasslePile;
					Add( hand, CardInfo.cOpen, to, "(round 1)" );

					to = hand.StashPile;
					Add( hand, CardInfo.cMexico, to );
					Add( hand, CardInfo.cColumbia, to );
					break;

				case "John":
					john = player;
					sGame.Take( hand, CardInfo.cSteal );
					sGame.Take( hand, CardInfo.cPayFine );
					sGame.Take( hand, CardInfo.cStonehigh );
					sGame.Take( hand, CardInfo.cPayFine );
					sGame.Take( hand, CardInfo.cOpen );
					sGame.Take( hand, CardInfo.cOffDetained );

					to = hand.HasslePile;
					Add( hand, CardInfo.cOnBust, to, "by Bob stash was 60,000 (round 9)" );
					Add( hand, CardInfo.cOffBust, to, "played (round 10)" );
					Add( hand, CardInfo.cOnSearch, to, "by Janis stash was 60,000 (round 11)" );
					Add( hand, CardInfo.cOffSearch, to, "played (round 11)" );
					Add( hand, CardInfo.cOpen, to, "(round 6)" );

					to = hand.StashPile;
					Add( hand, CardInfo.cMexico, to );
					Add( hand, CardInfo.cHomegrown, to );
					Add( hand, CardInfo.cPanama, to );
					Add( hand, CardInfo.cPanama, to );
					break;

				case "Amy":
					amy = player;
					sGame.Take( hand, CardInfo.cOffFelony );
					card = hand.Cards.FirstOrDefault( c => c.Id == CardInfo.cOffFelony );
					card?.AddComment( "passed by John to Amy (round 5)" );
					sGame.Take( hand, CardInfo.cOpen );
					sGame.Take( hand, CardInfo.cOnDetained );
					sGame.Take( hand, CardInfo.cEuphoria );
					sGame.Take( hand, CardInfo.cDoublecross );
					sGame.Take( hand, CardInfo.cClose );

					to = hand.HasslePile;
					Add( hand, CardInfo.cOnSearch, to, "by John stash was 100,000 (round 12)" );
					Add( hand, CardInfo.cOffSearch, to, "played (round 12)" );
					Add( hand, CardInfo.cOpen, to, "trade with John (round 7)" );

					to = hand.StashPile;
					Add( hand, CardInfo.cColumbia, to, "protected (round 9)", protect: true );
					Add( hand, CardInfo.cGrabaSnack, to, "played (round 9)" );
					Add( hand, CardInfo.cColumbia, to, "protected (round 13)", protect: true );
					Add( hand, CardInfo.cPanama, to );
					Add( hand, CardInfo.cGrabaSnack, to, "played (round 13)" );
					break;

				case "Bob":
					bob = player;
					sGame.Take( hand, CardInfo.cOffSearch );
					sGame.Take( hand, CardInfo.cStonehigh );
					sGame.Take( hand, CardInfo.cOnBust );
					sGame.Take( hand, CardInfo.cSteal );
					sGame.Take( hand, CardInfo.cJamaica );
					sGame.Take( hand, CardInfo.cOffDetained );

					to = hand.HasslePile;
					Add( hand, CardInfo.cOpen, to, "(round 1)" );

					to = hand.StashPile;
					Add( hand, CardInfo.cHomegrown, to );
					Add( hand, CardInfo.cPanama, to, "protected (round 12)", protect: true );
					Add( hand, CardInfo.cLustConquers, to, "played (round 12)" );
					Add( hand, CardInfo.cJamaica, to );
					Add( hand, CardInfo.cDrFeelgood, to );
					break;
			}
		}
		#endregion
		if( endgame ) { PlayRound14( amy, bob, janis, john ); }

		return sGame;
	}

	private static void PlayRound14( Player? amy, Player? bob, Player? janis, Player? john )
	{
		if( amy is null || bob is null || janis is null || john is null ) return;
		int round = 14;

		Hand hand = janis.Current;
		hand.Round = round;
		sGame.Take( hand, CardInfo.cLustConquers ); // Pick-up
		Card? card = hand.Cards.FirstOrDefault( c => c.Id == CardInfo.cSoldout );
		if( card is not null ) { _ = sGame.Play( janis, card ); }

		hand = john.Current;
		hand.Round = round;
		sGame.Take( hand, CardInfo.cMexico ); // Pick-up
		Transfer( hand.Cards, hand.StashPile, CardInfo.cMexico ); // Play

		hand = amy.Current;
		hand.Round = round;
		sGame.Take( hand, CardInfo.cSteal ); // Pick-up (turn 1)
		card = hand.Cards.FirstOrDefault( c => c.Id == CardInfo.cEuphoria );
		if( card is not null ) { _ = sGame.Play( amy, card ); } // Play (turn 1)
		sGame.Take( hand, CardInfo.cOnFelony ); // Pick-up (turn 2)
		Transfer( hand.Cards, hand.HasslePile, CardInfo.cClose, "(round 14)" ); // Play (turn 2)

		sGame.EndHand();
		ShowResults( sGame );
	}

	#region Helper Functions

	private static void Transfer( List<Card> from, List<Card> to, string cardId, string? msg = null )
	{
		Card? card = from.FirstOrDefault( c => c.Id == cardId && !c.Protected );
		if( card is not null )
		{
			Card.TransferCard( from, to, card );
			if( msg is not null ) { card.AddComment( msg ); }
		}
	}

	private static void BackToStack( List<Card> cards )
	{
		List<Card> temp = new( cards );
		foreach( Card card in temp ) { Card.TransferCard( cards, sGame.GrassStack, card ); }
	}

	private static void Add( Hand hand, string cardName, List<Card> to,
		string? msg = null, bool protect = false )
	{
		sGame.Take( hand, cardName );
		if( hand.Cards.Count > 6 )
		{
			Card? card = hand.Cards.LastOrDefault();
			if( card is not null )
			{
				Card.TransferCard( hand.Cards, to, card );
				if( protect ) { card.Protected = true; }
				if( msg is not null ) { card.AddComment( msg ); }
			}
		}
	}

	#endregion

	/// <summary>Get players</summary>
	public static List<Player> GetPlayers() => [
			new( "Amy" ),
			new( "Bob" ),
			new( "Janis" ),
			new( "John" ),
		];

	#region Results to Console

	/// <summary>Outputs the results to the console.</summary>
	/// <param name="game">Game that was played.</param>
	public static void ShowResults( Game game )
	{
		Console.WriteLine( $"Game target: {game.Target:$#,###,##0} on {game.Date}" );
		Player? winner = game.Winner;
		if( winner is not null )
		{
			Console.WriteLine( $"{winner.Name} won with {winner.Total:$#,###,##0}" );
			Console.WriteLine( $"{winner.Completed.Count} hand(s) played" );
			string reason = game.StackCount == 0 ? "Stack ran out" : "Market closed";
			Console.WriteLine( $"{winner.Current.Round} rounds played in final hand - {reason}" );
		}
		Console.WriteLine( $"{game.Dealer.Name} was the Dealer" );
		Player? banker = game.GetBanker();
		if( banker is not null ) { Console.WriteLine( $"{banker.Name} was the Banker" ); }
		else { Console.WriteLine( "No Banker" ); }
		Console.WriteLine();

		Console.WriteLine( $"Wasted pile count: {game.WastedPile.Count}" );
		foreach( Card card in game.WastedPile ) { Console.WriteLine( $"{card} {card.Comment}" ); }

		ConsoleColor dftColor = Console.ForegroundColor;
		foreach( Player player in game.PlayOrder )
		{
			Hand? hand = player.Completed.LastOrDefault();
			if( hand is null ) { continue; }
			Console.WriteLine();
			Console.WriteLine( "------------------------------" );
			Console.WriteLine( $"{player}" );
			Console.WriteLine( "------------------------------" );

			Console.WriteLine( "-- hassle:" );
			Console.ForegroundColor = ConsoleColor.Red;
			foreach( Card card in hand.HasslePile ) { Console.WriteLine( $"{card} {card.Comment}" ); }
			Console.ForegroundColor = dftColor;
			Console.WriteLine();

			Console.WriteLine( "-- in hand:" );
			Console.ForegroundColor = ConsoleColor.Yellow;
			foreach( Card card in hand.Cards ) { Console.WriteLine( $"{card} {card.Comment}" ); }
			Console.ForegroundColor = dftColor;
			Console.WriteLine();

			Console.WriteLine( "-- stash:" );
			Console.ForegroundColor = ConsoleColor.Green;
			foreach( Card card in hand.StashPile ) { Console.WriteLine( $"{card} {card.Comment}" ); }
			Console.ForegroundColor = dftColor;
			Console.WriteLine();

			Console.WriteLine( "-- score:" );
			Console.WriteLine( $"Protected......: {hand.Protected:###,##0}" );
			Console.WriteLine( $"UnProtected....: {hand.UnProtected:###,##0}" );
			Console.WriteLine( $"Skimmed........: {hand.Skimmed:###,##0}" );
			if( hand.HighestPeddle > 0 ) Console.WriteLine( $"Highest Peddle.: {hand.HighestPeddle:-###,##0}" );
			else Console.WriteLine( $"Highest Peddle.: {hand.HighestPeddle:###,##0}" );
			Console.WriteLine( $"Paranoia Fines.: {hand.ParanoiaFines:###,##0}" );
			Console.WriteLine( $"Net Score......: {hand.NetScore:###,##0}" );
			if( hand.Bonus > 0 ) { Console.WriteLine( $"Win hand Bonus.: {hand.Bonus:###,##0}" ); }
		}
	}

	#endregion
}