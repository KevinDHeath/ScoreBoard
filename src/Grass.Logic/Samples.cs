using Grass.Logic.Models;
namespace Grass.Logic;

/// <summary>Static data</summary>
[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
public class Samples
{
	/// <summary>Populate data</summary>
	public static void Populate( Game game, bool endgame = false )
	{
		game.Date = "July 9, 2024 10:31 PM";
		Player? dealer = game.Players.FirstOrDefault( p => p.Name == "Bob" );
		if( dealer is not null ) { game.Dealer = dealer; }
		game.StartHand();

		// Put dealt cards back in the stack
		foreach( Player p in game.PlayOrder ) { BackToStack( game, p.Current.Cards ); }
		Player? janis = null, john = null, amy = null, bob = null;

		#region To end of round 13
		foreach( Player player in game.Players )
		{
			Hand hand = player.Current;
			hand.Round = 13;
			Card? card = null;
			List <Card> to = [];
			switch( player.Name )
			{
				case "Janis":
					janis = player;
					game.Take( hand, CardInfo.cClose );
					game.Take( hand, CardInfo.cClose );
					game.Take( hand, CardInfo.cBanker );
					game.Take( hand, CardInfo.cOffFelony );
					game.Take( hand, CardInfo.cSoldout );
					game.Take( hand, CardInfo.cOffFelony );
					card = hand.Cards.LastOrDefault( c => c.Id == CardInfo.cOffFelony );
					card?.AddComment( "passed by Bob to Janis (round 9)" );

					to = hand.HasslePile;
					Add( game, hand, CardInfo.cOpen, to, "(round 1)" );

					to = hand.StashPile;
					Add( game, hand, CardInfo.cMexico, to );
					Add( game, hand, CardInfo.cColumbia, to );
					break;

				case "John":
					john = player;
					game.Take( hand, CardInfo.cSteal );
					game.Take( hand, CardInfo.cPayFine );
					game.Take( hand, CardInfo.cStonehigh );
					game.Take( hand, CardInfo.cPayFine );
					game.Take( hand, CardInfo.cOpen );
					game.Take( hand, CardInfo.cOffDetained );

					to = hand.HasslePile;
					Add( game, hand, CardInfo.cOnBust, to, "by Bob stash was 60,000 (round 9)" );
					Add( game, hand, CardInfo.cOffBust, to, "played (round 10)" );
					Add( game, hand, CardInfo.cOnSearch, to, "by Janis stash was 60,000 (round 11)" );
					Add( game, hand, CardInfo.cOffSearch, to, "played (round 11)" );
					Add( game, hand, CardInfo.cOpen, to, "(round 6)" );

					to = hand.StashPile;
					Add( game, hand, CardInfo.cMexico, to );
					Add( game, hand, CardInfo.cHomegrown, to );
					Add( game, hand, CardInfo.cPanama, to );
					Add( game, hand, CardInfo.cPanama, to );
					break;

				case "Amy":
					amy = player;
					game.Take( hand, CardInfo.cOffFelony );
					card = hand.Cards.FirstOrDefault( c => c.Id == CardInfo.cOffFelony );
					card?.AddComment( "passed by John to Amy (round 5)" );
					game.Take( hand, CardInfo.cOpen );
					game.Take( hand, CardInfo.cOnDetained );
					game.Take( hand, CardInfo.cEuphoria );
					game.Take( hand, CardInfo.cDoublecross );
					game.Take( hand, CardInfo.cClose );

					to = hand.HasslePile;
					Add( game, hand, CardInfo.cOnSearch, to, "by John stash was 100,000 (round 12)" );
					Add( game, hand, CardInfo.cOffSearch, to, "played (round 12)" );
					Add( game, hand, CardInfo.cOpen, to, "trade with John (round 7)" );

					to = hand.StashPile;
					Add( game, hand, CardInfo.cColumbia, to, "protected (round 9)", protect: true );
					Add( game, hand, CardInfo.cGrabaSnack, to, "played (round 9)" );
					Add( game, hand, CardInfo.cColumbia, to, "protected (round 13)", protect: true );
					Add( game, hand, CardInfo.cPanama, to );
					Add( game, hand, CardInfo.cGrabaSnack, to, "played (round 13)" );
					break;

				case "Bob":
					bob = player;
					game.Take( hand, CardInfo.cOffSearch );
					game.Take( hand, CardInfo.cStonehigh );
					game.Take( hand, CardInfo.cOnBust );
					game.Take( hand, CardInfo.cSteal );
					game.Take( hand, CardInfo.cJamaica );
					game.Take( hand, CardInfo.cOffDetained );

					to = hand.HasslePile;
					Add( game, hand, CardInfo.cOpen, to, "(round 1)" );

					to = hand.StashPile;
					Add( game, hand, CardInfo.cHomegrown, to );
					Add( game, hand, CardInfo.cPanama, to, "protected (round 12)", protect: true );
					Add( game, hand, CardInfo.cLustConquers, to, "played (round 12)" );
					Add( game, hand, CardInfo.cJamaica, to );
					Add( game, hand, CardInfo.cDrFeelgood, to );
					break;
			}
		}
		#endregion
		if( endgame ) { PlayRound14( game, amy, bob, janis, john ); }
	}

	private static void PlayRound14( Game game, Player? amy, Player? bob, Player? janis, Player? john )
	{
		if( amy is null || bob is null || janis is null || john is null ) return;
		int round = 14;

		Hand hand = janis.Current;
		hand.Round = round;
		game.Take( hand, CardInfo.cLustConquers ); // Pick-up
		Card? card = hand.Cards.FirstOrDefault( c => c.Id == CardInfo.cSoldout );
		if( card is not null ) { _ = game.Play( janis, card ); }

		hand = john.Current;
		hand.Round = round;
		game.Take( hand, CardInfo.cMexico ); // Pick-up
		Transfer( hand.Cards, hand.StashPile, CardInfo.cMexico ); // Play

		hand = amy.Current;
		hand.Round = round;
		game.Take( hand, CardInfo.cSteal ); // Pick-up (turn 1)
		card = hand.Cards.FirstOrDefault( c => c.Id == CardInfo.cEuphoria );
		if( card is not null ) { _ = game.Play( amy, card ); } // Play (turn 1)
		game.Take( hand, CardInfo.cOnFelony ); // Pick-up (turn 2)
		Transfer( hand.Cards, hand.HasslePile, CardInfo.cClose, "(round 14)" ); // Play (turn 2)

		game.EndHand( amy );
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

	private static void BackToStack( Game game, List<Card> cards )
	{
		List<Card> temp = new( cards );
		foreach( Card card in temp ) { Card.TransferCard( cards, game.GrassStack, card ); }
	}

	private static void Add( Game game, Hand hand, string cardName, List<Card> to,
		string? msg = null, bool protect = false )
	{
		game.Take( hand, cardName );
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
			new( "Amy", 1 ),
			new( "Bob", 2 ),
			new( "Janis", 3 ),
			new( "John", 4 ),
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
			Console.WriteLine( $"{winner.Current.Round} rounds played in final hand - {game.EndReason}" );
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