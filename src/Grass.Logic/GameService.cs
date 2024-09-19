// Ignore Spelling: json
using System.Text.Json;
using Grass.Logic.Models;
using System.ComponentModel;
namespace Grass.Logic;

/// <summary>Service providing actions taken by players during a Grass game.</summary>
public class GameService : PassCardHandler
{
	private Game _game;
	private GameOptions _options = default!;
	private bool _disposed;
	private static readonly object _lock = new();

	/// <summary>Initializes a new instance of the <see cref="GameService"/> class.</summary>
	public GameService() => _game = default!;

	/// <summary>Game changed event used to trigger page state changed.</summary>
	public event EventHandler? GameChanged;
	private void OnGameChanged( EventArgs e ) => GameChanged?.Invoke( this, e );

	/// <summary>Current game.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public Game Current => _game;

	/// <summary>Current game options.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public GameOptions Options { get { return _options; } set { _options = value; } }

	/// <summary>Register player.</summary>
	/// <param name="playerName">Player name.</param>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public void RegisterPlayer( string playerName )
	{
		if( _options is not null )
		{
			Options.AddPlayer( playerName );
			OnGameChanged( new() );
		}
	}

	/// <summary>Setup and start a new Grass game.</summary>
	/// <param name="options">Options for the Game.</param>
	/// <returns>An initialized game object with the provided options.</returns>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public Game Setup( GameOptions options )
	{
		if( options.Players.Count == 0 && options.AllowTests ) { options.Players = Samples.GetPlayers(); }
		//if( options.Players.Count == 0 && options.AllowTests ) { options.Players = Samples.GetMinPlayers(); }
		foreach( var player in options.Players ) { player.Reset(); }
		if( options.Players.Count < 2 ) { return Current; }
		_game = new( options.Players, options.Target, options.ReversePlay, options.CardComments, options.AutoPlay );

		// Play the game
		if( _game.Auto && options.InProgress )
		{
			_game.Auto = false; // switch off auto-play when populating in-progress sample
			Samples.InProgress( _game );
			//Samples.TestParanoia( _game );
			_game.GameChanged += OnParanoiaInteractive;
			_game.SetNextPlayer();
		}
		else if( _game.Auto )
		{
			_game.GameChanged += OnParanoiaPlayed;
			_game.Play(); StoreSummary( _game );
		}
		else
		{
			_game.GameChanged += OnParanoiaInteractive;
			_game.StartHand();
			_game.SetNextPlayer();
		}
		OnGameChanged( new() );
		return _game;
	}

	/// <inheritdoc/>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public override void Dispose()
	{
		if( _disposed ) { return; }
		RemoveListeners();
		GC.SuppressFinalize( this );
		_disposed = true;
	}

	/// <summary>Reset the game.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public void ResetGame()
	{
		RemoveListeners();
		_game = default!;
		if( Options.AllowTests ) { Options.Players.Clear(); }
		OnGameChanged( new() );
	}

	/// <summary>Play a game asynchronously.</summary>
	/// <returns><c>true</c> is returned if the game was completed successfully.</returns>
	/// <remarks><c>await Task.Run()</c> scenarios can be avoided if the created task is returned directly.</remarks>
	/// <seealso href="https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios">
	/// Asynchronous programming scenarios</seealso>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public Task<bool> GameAsync() => Task.Run( () => { return _game.Play(); } );

	/// <summary>Checks whether a card can be played.</summary>
	/// <param name="options">Card play options.</param>
	/// <returns>Collection of amounts and players that can be used to hassle.</returns>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public Dictionary<Player, int> CheckPlay( PlayOptions options )
	{
		if( options.Player is null || options.Card is null ) { return []; }
		Dictionary<Player, int> rtn = Decision.CheckPlay( _game, options );
		options.CanDiscard = AllowDiscard( options.Card );
		return rtn;
	}

	/// <summary>Returns a message for a specific card plays.</summary>
	/// <param name="options">Card play options.</param>
	/// <returns><c>null</c> if there is not message to display.</returns>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public string? GetInfoMessage( PlayOptions options ) => Decision.GetInfoMessage( _game, options );

	private static bool AllowDiscard( Card card )
	{
		bool rtn = true;
		if( card.Id.StartsWith( CardInfo.cParanoia ) ||
			card.Id.StartsWith( CardInfo.cNirvana ) ||
			card.Id.Equals( CardInfo.cOpen ) ||
			card.Id.StartsWith( CardInfo.cPeddle ) ||
			card.Id.StartsWith( CardInfo.cHeatOff ) )
		{ rtn = false; }
		return rtn;
	}

	private void RemoveListeners()
	{
		if( _game is null ) { return; }
		if( _game.Auto ) { _game.GameChanged -= OnParanoiaPlayed; }
		else { _game.GameChanged -= OnParanoiaInteractive; }
	}

	private void Reset( PlayOptions options, Player? ignore = null )
	{
		if( _game.TradeRq is not null ) { _game.TradeRq = null; }
		foreach( Player player in _game.Players )
		{
			if( ignore is not null && player == ignore ) { continue; }
			if( player.Notify is not null ) { player.Notify = null; }
			if( player.Trade ) { player.ToDo = Player.Action.Nothing; }
		}
		options.Reset();
		OnGameChanged( new() );
	}

	#region Action Methods

	/// <summary>Pass a card due to paranoia being played.</summary>
	/// <param name="options">Play card options.</param>
	/// <returns><c>false</c> if the player has already passed a card or the card is
	/// not in the players hand.</returns>
	public bool PassCard( PlayOptions options )
	{
		if( options.Player is null || options.Card is null ) { return false; }
		bool rtn = _game.AddCardToPass( options.Player, options.Card );
		if( rtn ) { Reset( options ); }
		return rtn;
	}

	/// <summary>Play a card on a players turn.</summary>
	/// <param name="options">Play card options.</param>
	/// <returns>An object representing the play result.</returns>
	/// <remarks>The <c>null</c> return value is used to indicate success. The result should be
	/// compared to <see cref="PlayResult.Success"/> rather than checking for <c>null</c>.</remarks>
	public PlayResult PlayCard( PlayOptions options )
	{
		PlayResult rtn = new( Rules.cPlayOption );
		if( options.Player is null || options.Card is null ) { return rtn; }
		Card card = options.Card;
		if( options.OtherCards.Count > 0 )
		{
			rtn = Rules.Protect( _game, options.Player, card, options.OtherCards );
			if( rtn == PlayResult.Success )
			{
				string? comment = Decision.GetInfoMessage( _game, options );
				if( comment is not null ) { card.AddComment( comment ); }
			}
		}
		else if( options.OtherId > 0 )
		{
			Player? other = _game.Players.FirstOrDefault( p => p.Id == options.OtherId );
			if( other is not null )
			{
				Card? oCard = card.Type == CardInfo.cHeatOn ? card : other.Current.HighestUnProtected;
				if( oCard is not null ) { rtn = Rules.Play( _game, options.Player, card, other, oCard ); }
			}
		}
		else { rtn = Rules.Play( _game, options.Player, card ); }

		if( rtn == PlayResult.Success )
		{
			bool over = false;
			if( card.Id == CardInfo.cClose ) { over = _game.CheckWinner( options.Player ); }
			else if( card.Type != CardInfo.cParanoia ) { over = _game.SetNextPlayer( options.Player ); }
			if( over ) { StoreSummary( _game ); }
			Reset( options );
		}
		return rtn;
	}

	/// <summary>Trade a card with another player.</summary>
	/// <param name="options">Play card options.</param>
	/// <returns>An object representing the trade result.</returns>
	/// <remarks>The <c>null</c> return value is used to indicate success. The result should be
	/// compared to <see cref="PlayResult.Success"/> rather than checking for <c>null</c>.</remarks>
	public PlayResult TradeCard( PlayOptions options )
	{
		PlayResult rtn = new( Rules.cPlayOption );
		if( options.Player is null || options.Card is null ) { return rtn; }
		if( _game.TradeRq is null ) { return rtn; }
		PlayOptions? rq;
		lock( _lock ) { rq = _game.TradeRq; _game.TradeRq = null; }
		if( rq is null || rq.Player is null || rq.Card is null ) { return rtn; }

		rtn = Rules.Trade( _game, rq.Player, rq.Card, options.Player, options.Card );
		if( rtn == PlayResult.Success )
		{
			rq.Player.Notify = $"Trade accepted by {options.Player.Name}";
			Reset( options, rq.Player );
		}
		return rtn;
	}

	/// <summary>Request a card trade on a players turn.</summary>
	/// <param name="options">Play card options.</param>
	public void TradeRequest( PlayOptions options )
	{
		if( options.Player is null || options.Card is null ) { return; }
		if( options.TradeFor is not null )
		{
			Player player = options.Player;
			Card card = options.Card;
			_game.TradeRq = options.SetTradeRq();
			string notify = $"{player.Name} would like to trade {card.Info} for {options.TradeFor}";
			foreach( Player other in _game.Players )
			{
				if( other == player ) { continue; }
				other.Notify = notify;
			}
		}
		options.Reset(); // Don't do full reset
		OnGameChanged( new() );
	}

	/// <summary>Waste a card (discard) on a players turn.</summary>
	/// <param name="options">Play card options.</param>
	/// <returns><c>true</c> if the card is successfully discarded.</returns>
	public bool WasteCard( PlayOptions options )
	{
		if( options.Player is null || options.Card is null ) { return false; }
		bool rtn = _game.Discard( options.Player, options.Card );
		if( rtn )
		{
			bool over = _game.SetNextPlayer( options.Player );
			if( over ) { StoreSummary( _game ); }
			Reset( options );
		}
		return rtn;
	}

	#endregion

	#region Game Summaries

	private readonly JsonSerializerOptions options = new() { WriteIndented = false };

	/// <summary>Collection of completed Game summaries.</summary>
	public List<Summary> Summaries { get; set; } = [];

	internal void StoreSummary( Game game ) => Summaries.Add( Summary.BuildSummary( game ) );

	/// <summary>Export a game summary.</summary>
	/// <param name="summary">Summary to export.</param>
	/// <param name="indent">Indicates whether to use JSON indentation.</param>
	/// <returns>A string representing the game summary as JSON.</returns>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public string ExportSummary( Summary summary, bool indent = false )
	{
		options.WriteIndented = indent;
		return JsonSerializer.Serialize( summary, options );
	}

	#endregion
}