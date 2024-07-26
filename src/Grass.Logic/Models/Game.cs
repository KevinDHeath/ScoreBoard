using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
namespace Grass.Logic.Models;

/// <summary>Provides the details of a Game.</summary>
public class Game
{
	#region Constructor

	internal Game( List<Player> players, int target, bool reverse, bool comment, bool auto )
	{
		Players = players;
		Target = target;
		Comment = comment;
		Auto = auto;
		Date = DateTime.Now.ToString( @"MMMM d, yyyy hh:mm tt" );
		Dealer = SetDealer( null, Hand );
		PlayOrder = [];
		ReversePlay = reverse;

		cardsToPass.CollectionChanged += CardsToPassChanged;
	}

	#endregion

	#region Events

	private EventHandler<PropertyChangedEventArgs>? _gameChanged;

	internal event EventHandler<PropertyChangedEventArgs>? GameChanged
	{
		add
		{
			int? count = _gameChanged?.GetInvocationList().Length;
			if( count is null ) { _gameChanged += value; } // Can only have 1 subscriber
		}
		remove { _gameChanged -= value; }
	}

	internal void OnGameChanged( [System.Runtime.CompilerServices.CallerMemberName] string prop = "" )
	{
		_gameChanged?.Invoke( this, new PropertyChangedEventArgs( prop ) );
	}

	private Player? _paranoiaPlayer;
	internal Player? ParanoiaPlayer
	{
		get { return _paranoiaPlayer; }
		set
		{
			if( _paranoiaPlayer != value )
			{
				_paranoiaPlayer = value;
				if( _paranoiaPlayer is not null ) { OnGameChanged(); }
			}
		}
	}

	private readonly ObservableCollection<KeyValuePair<Player, Card>> cardsToPass = [];

	private void CardsToPassChanged( object? sender, NotifyCollectionChangedEventArgs e )
	{
		if( e.Action == NotifyCollectionChangedAction.Add )
		{
			if( cardsToPass.Count == Players.Count )
			{
				Rules.PassCards( this, new( cardsToPass ) );
				cardsToPass.Clear();
				ParanoiaPlayer = null;
			}
		}
	}

	#endregion

	#region Properties

	/// <summary>Number of cards left in the stack.</summary>
	public int StackCount => GrassStack.Count;

	/// <summary>List of players in the game.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public List<Player> Players { get; private set; }

	/// <summary>Game target value.</summary>
	public int Target { get; private set; }

	/// <summary>Game date.</summary>
	public string Date { get; internal set; }

	/// <summary>Number of hands.</summary>
	public int Hand { get; internal set; }

	/// <summary>Current dealer.</summary>
	public Player Dealer { get; internal set; }

	/// <summary>List of players in order of play.</summary>
	public List<Player> PlayOrder { get; private set; }

	/// <summary>Winner of the game.</summary>
	/// <returns>If the game is not completed the value is <c>null</c>.</returns>
	public Player? Winner { get; internal set; }

	/// <summary>Reason why the game ended.</summary>
	public string EndReason
	{
		get
		{
			if( Winner is null ) { return string.Empty; }
			string by = LastPlayer is not null ? "by " + LastPlayer.Name : string.Empty;
			return StackCount == 0 ? "Stack ran out" : $"Market closed {by}".Trim();
		}
	}

	/// <summary>List of cards in the wasted pile.</summary>
	public List<Card> WastedPile { get; private set; } = [];

	internal bool Comment { get; set; }

	internal bool ReversePlay { get; set; }

	internal List<Card> GrassStack { get; set; } = [];

	private Player? LastPlayer { get; set; }

	#endregion

	#region Public Methods

	/// <summary>Gets the banker for the current round.</summary>
	/// <returns>The player holding the banker card, or <see langword="null"/>
	/// if nobody has the card.</returns>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public Player? GetBanker() => GetBanker( Players );

	#endregion

	#region Internal Methods

	/// <summary>Start a hand of the game.</summary>
	/// <returns><see langword="true"/> if the hand was successfully started.</returns>
	/// <remarks>There must be 104 cards and 2-6 players to play a game.</remarks>
	internal bool StartHand()
	{
		GrassStack = CardInfo.BuildStack();
		if( GrassStack.Count != Rules.cCardTotal ) { return false; } // Check # of cards
		if( Players.Count < 2 || Players.Count > Rules.cMaxNumber ) { return false; } // Check # of players

		if( WastedPile.Count > 0 ) { WastedPile.Clear(); } // Reset wasted pile
		Hand++;
		return Deal();
	}

	/// <summary>End a hand of the game.</summary>
	/// <param name="last">Last player in case of market close.</param>
	/// <remarks>Calculates the net scores for each player, assigns the bonus to the player
	/// with the highest net score, and checks if there is a winner of the game.</remarks>
	internal void EndHand( Player? last )
	{
		// Calculate net scores
		Player? banker = GetBanker();
		foreach( Player player in Players )
		{
			player.Current.EndHand( banker );
			player.Completed.Add( player.Current );
		}
		banker?.Current.EndHand( null ); // Recalculate the banker net score after all skims

		// Assign the bonus to the player with the highest net score
		int high = 0;
		Player? bonus = null;
		foreach( Player player in Players )
		{
			player.Total += player.Current.NetScore;
			if( player.Current.NetScore > high )
			{
				high = player.Current.NetScore;
				bonus = player;
			}
		}
		if( bonus is not null )
		{
			bonus.Current.Bonus = Rules.cBonusAmount;
			bonus.Total += bonus.Current.Bonus;
		}

		// Check for game winner with the highest total
		high = Target;
		foreach( Player player in Players ) 
		{
			if( player.Total > high ) { high = player.Total; Winner = player; }
		}
		if( Winner is not null )
		{
			_gameChanged = null; // Clear paranoia play listener
			LastPlayer = last;
		}
		else
		{
			// If no winner reset for the next hand
			foreach( Player player in Players ) { player.ResetCurrent(); }
			if( bonus is not null ) { Dealer = bonus; }
		}
		return;
	}

	internal bool AddCardToPass( Player player, Card card )
	{
		if( ParanoiaPlayer is null || cardsToPass.Count == Players.Count ) { return false; }
		Dictionary<Player, Card> dict = new( cardsToPass );
		if( dict.ContainsKey( player ) ) { return false; }
		if( !player.Current.Cards.Contains( card ) ) { return false; }
		cardsToPass.Add( new KeyValuePair<Player, Card>( player, card ) );
		return true;
	}

	internal bool Discard( Player player, Card card )
	{
		Hand hand = player.Current;
		bool ok = hand.Cards.Remove( card );
		if( ok )
		{
			// TODO: Need to figure out how to trigger the pass cards function
			// This should raise an event and the Actor class needs to listen for it
			//if( card.Type == CardInfo.cParanoia )
			//{
			//	Play( player, card );
			//}
			WastedPile.Add( card );
			if( Comment ) { card.AddComment( $"{player.Name} discard (round {hand.Round})" ); }
		}
		return ok;
	}

	internal PlayResult Play( Player player, Card card ) => Rules.Play( this, player, card );

	internal PlayResult Play( Player player, Card card, Player with, Card other ) =>
		Rules.Play( this, player, card, with, other );

	internal PlayResult Protect( Player player, Card card, List<Card> peddles ) =>
		Rules.Protect( this, player, card, peddles );

	internal bool Take( Hand hand )
	{
		if( GrassStack.Count == 0 ) { return false; }
		Card card = GrassStack[^1];
		bool rtn = GrassStack.Remove( card );
		if( rtn ) { hand.Cards.Add( card ); }
		return rtn;
	}

	internal bool Take( Hand hand, string cardName )
	{
		if( GrassStack.Count == 0 ) { return false; }
		Card? card = GrassStack.Where( c => c.Id == cardName ).FirstOrDefault();
		if( card == null ) { return false; }
		bool rtn = GrassStack.Remove( card );
		if( rtn ) { hand.Cards.Add( card ); }
		return rtn;
	}

	internal PlayResult CheckState( Card card )
	{
		if( Winner is not null ) // Game over
		{
			if( _gameChanged is not null ) { _gameChanged = null; } // Clear all subscribers
			return new( "The game is over!" );
		}
		else if( card.Type is CardInfo.cParanoia )
		{
			int? count = _gameChanged?.GetInvocationList().Length; // Must be 1 listener
			if( count is null ) { return new( "Nothing is listening for card passing!" ); }
		}

		return PlayResult.Success!;
	}

	#endregion

	#region Private Methods

	private Player SetDealer( Player? dealer, int handNumber )
	{
		if( dealer is null )
		{
			// Pick a random player if players populated
			Random random = new();
			int idx = random.Next( 0, Players.Count );
			dealer = Players[idx];
		}
		if( handNumber == 0 ) { return dealer; }

		// Order players for hand
		if( PlayOrder.Count > 0 ) { PlayOrder.Clear(); }
		List<Player> order = new( Players );
		bool reverse = handNumber % 2 == 0;
		if( ReversePlay & reverse ) { order.Reverse(); } // every alternate hand

		int start = order.FindIndex( x => x == dealer ) + 1;
		for( int i = start; PlayOrder.Count < Players.Count; i++ )
		{
			if( i == Players.Count ) { i = 0; }
			PlayOrder.Add( order[i] );
		}
		return dealer;
	}

	internal bool Deal()
	{
		Dealer = SetDealer( Dealer, Hand );
		for( int i = 0; i < Rules.cMaxNumber; i++ ) // Cards in hand
		{
			foreach( Player player in PlayOrder )
			{
				if( !Take( player.Current ) ) { return false; };
			}
		}
		return true;
	}

	private static Player? GetBanker( List<Player> players )
	{
		foreach( Player player in players )
		{
			Card? card = CardInfo.GetFirst( player.Current.Cards, CardInfo.cBanker );
			if( card is not null ) { return player; }
		}
		return null;
	}

	#endregion

	#region Auto-Play

	internal bool Auto { get; set; }

	internal bool Play()
	{
		if( Hand > 0 ) { return true; } // Game already populated
		Actor? actor = Auto ? new( this ) : null;

		while( Winner is null )
		{
			if( !StartHand() ) { return false; }
			Player? last = null;
			if( actor is not null ) { last = PlayHand( actor ); }
			EndHand( last );
		}
		return true;
	}

	private Player? PlayHand( Actor? actor )
	{
		int round = 0;
		while( GrassStack.Count > 0 )
		{
			round++;
			foreach( Player player in PlayOrder )
			{
				Hand hand = player.Current;
				hand.Round = round;

				// Miss turns due to playing Paranoia
				if( hand.Turns < 0 )
				{
					player.Current.Turns++;
					continue;
				}

				while( hand.Turns >= 0 )
				{
					bool played = false;  // TODO: card must be played real-time
					if( actor is not null ) { played = actor.Play( hand ); }
					if( !played )
					{
						if( GrassStack.Count == 0 ) { return null; } // End of grass stack
						else { return player; } // Market close played
					}

					// Extra turns due to playing Nirvana
					if( player.Current.Turns > 0 )
					{
						player.Current.Turns--;
						continue;
					}
					break;
				}
			}
		}
		return null;
	}

	#endregion
}