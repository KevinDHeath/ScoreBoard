// Ignore Spelling: json
using System.Text.Json;
using Grass.Logic.Models;
using System.ComponentModel;
namespace Grass.Logic;

/// <summary>Service providing actions to be taken with cards during a Grass game.</summary>
public class GameService : PassCardHandler
{
	internal Game _game;
	private GameOptions _options = default!;
	private bool _disposed;

	/// <summary>Initializes a new instance of the <see cref="GameService"/> class.</summary>
	public GameService() => _game = default!;

	/// <summary>Current game options.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public GameOptions Options => _options;

	/// <summary>Current game.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public Game Current => _game;

	/// <summary>Setup a new Grass game.</summary>
	/// <param name="options">Options for the Game.</param>
	/// <returns>An initialized game.</returns>
	public Game Setup( GameOptions options )
	{
		foreach( var player in options.Players ) { player.Reset(); }
		_options = options;
		_game = new( options.Players, options.Target, options.ReversePlay, options.CardComments, options.AutoPlay );

		// Play the game
		if( _game.Auto ) { _game.GameChanged += OnParanoiaPlayed; }

		if( _game.Auto && options.InProgress )
		{
			_game.Auto = false; // switch off auto-play when populating in-progress sample
			Samples.InProgress( _game );
			SetNextPlayer();
		}
		else if( _game.Auto ) { _game.Play(); StoreSummary( _game ); }
		else
		{
			_game.GameChanged += OnParanoiaPlayed; 
			_game.StartHand();
			SetNextPlayer();
		}

		return _game;
	}

	private void SetNextPlayer( Player? current = null )
	{
		Player? next = null;
		if( current is null ) { next = _game.PlayOrder.First(); }
		else
		{
			// Check for end of stack
			if( _game.StackCount == 0 )
			{
				HandFinished( current );
				return;
			}

			// Extra turns due to playing Nirvana
			if( current.Current.Turns > 0 )
			{
				current.Current.Turns--;
				_game.Take( current.Current );
				return;
			}

			current.ToDo = Player.Action.Nothing;
			int idx = _game.PlayOrder.FindIndex( x => x == current ) + 1;
			while( next is null )
			{
				if( idx == _game.Players.Count ) { idx = 0; }
				next = _game.PlayOrder[idx];

				// Miss turns due to previously playing Paranoia
				if( next.Current.Turns < 0 )
				{
					next.Current.Turns++;
					next.Current.Round++;
					next = null;
					idx++;
				}
			}
		}

		next.Current.Round++;
		_game.Take( next.Current );
		next.ToDo = Player.Action.Play;
	}

	private void HandFinished( Player player )
	{
		_game.EndHand( player );
		player.ToDo = Player.Action.Nothing;
		if( _game.Winner is null )
		{
			_game.StartHand();
			SetNextPlayer();
		}
		else { StoreSummary( _game ); }
	}

	/// <summary>Play a game asynchronously.</summary>
	/// <returns><c>true</c> is returned if the game was completed successfully.</returns>
	/// <remarks><c>await Task.Run()</c> scenarios can be avoided if the created task is returned directly.</remarks>
	/// <seealso href="https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios">
	/// Asynchronous programming scenarios</seealso>
	public Task<bool> GameAsync() => Task.Run( () => { return _game.Play(); } );

	/// <summary>Checks whether a card can be played.</summary>
	/// <param name="hand">Hand containing the card.</param>
	/// <param name="card">The card to play.</param>
	/// <returns>The <c>null</c> value is used to indicate success. Consumers of
	/// <see cref="PlayResult"/>s should compare the values to
	/// <see cref="PlayResult.Success" /> rather than checking for <c>null</c>.</returns>
	public static PlayResult CanPlay( Hand hand, Card card ) => Rules.CanPlay( hand, card );

	/// <summary>Indicates whether a card should be allowed to be discarded.</summary>
	/// <param name="hand">Hand containing the card.</param>
	/// <param name="card">The card to play.</param>
	/// <returns>False if the card should not be discarded.</returns>
	/// <remarks>It is assumed that the card has been checked that it can be played.</remarks>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public static bool AllowDiscard( Hand hand, Card card )
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

	/// <inheritdoc/>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public override void Dispose()
	{
		if( _disposed ) { return; }
		if( _game.Auto ) { _game.GameChanged -= OnParanoiaPlayed; }
		GC.SuppressFinalize( this );
		_disposed = true;
	}

	#region Action Methods

	/// <summary>Add a card to pass due to paranoia being played.</summary>
	/// <param name="player">Player object.</param>
	/// <param name="card">Card object.</param>
	/// <returns><see langword="false"/> if the player has already added a card or
	/// the card is not in the players hand.</returns>
	public bool CardToPass( Player player, Card card ) => _game.AddCardToPass( player, card );

	/// <summary>Discard a card in the players current hand.</summary>
	/// <param name="player">Current player.</param>
	/// <param name="card">Card to discard.</param>
	/// <returns><see langword="true"/> if the card is successfully discarded.</returns>
	public bool Discard( Player player, Card card )
	{
		bool rtn = _game.Discard( player, card );
		if( rtn ) { SetNextPlayer( player ); }
		return rtn;
	}

	/// <summary>Play a card in the players current hand.</summary>
	/// <param name="player">Current player.</param>
	/// <param name="card">Card to play.</param>
	/// <returns>A <see cref="PlayResult" /> object representing the results
	/// of the play.</returns>
	/// <remarks>The <c>null</c> return value is used to indicate success. The result should be compared
	/// to <see cref="PlayResult.Success"/> rather than checking for <c>null</c>.</remarks>
	public PlayResult Play( Player player, Card card )
	{
		PlayResult rtn = _game.Play( player, card );
		if( rtn == PlayResult.Success )
		{
			if( card.Id == CardInfo.cClose ) { HandFinished( player ); }
			else { SetNextPlayer( player ); }
		}
		return rtn;
	}

	/// <summary>Play a card in the players current hand with another player.</summary>
	/// <param name="player">Current player.</param>
	/// <param name="card">Card to play.</param>
	/// <param name="with">Other player.</param>
	/// <param name="other">Other players card.</param>
	/// <returns>A <see cref="PlayResult" /> object representing the results
	/// of the play.</returns>
	/// <remarks>The <c>null</c> return value is used to indicate success. The result should be compared
	/// to <see cref="PlayResult.Success"/> rather than checking for <c>null</c>.</remarks>
	public PlayResult Play( Player player, Card card, Player with, Card other ) =>
		_game.Play( player, card, with, other );

	/// <summary>Protect the players peddle cards.</summary>
	/// <param name="player">Current player.</param>
	/// <param name="card">Card to play.</param>
	/// <param name="peddles">List of peddle cards to protect</param>
	/// <returns>A <see cref="PlayResult" /> object representing the results
	/// of the play.</returns>
	/// <remarks>The <c>null</c> return value is used to indicate success. The result should be compared
	/// to <see cref="PlayResult.Success"/> rather than checking for <c>null</c>.</remarks>
	public PlayResult Protect( Player player, Card card, List<Card> peddles ) =>
		_game.Protect( player, card, peddles );

	#endregion

	#region Game Summaries

	private readonly JsonSerializerOptions options = new() { WriteIndented = false };

	/// <summary>Collection of Game summaries.</summary>
	public List<Summary> Summaries { get; set; } = [];

	private void StoreSummary( Game game )
	{
		Summaries.Add( Summary.BuildSummary( game ) );
	}

	/// <summary>Export a game summary.</summary>
	/// <param name="summary">Summary to export.</param>
	/// <param name="indent">Indicates whether to use JSON indentation.</param>
	/// <returns>A string representing the game summary as JSON.</returns>
	public string ExportSummary( Summary summary, bool indent = false )
	{
		options.WriteIndented = indent;
		return JsonSerializer.Serialize( summary, options );
	}

	/// <summary>Import a game summary.</summary>
	/// <param name="json">The JSON string.</param>
	/// <returns>A <c>null</c> is returned if invalid JSON was provided.</returns>
	internal static Summary? ImportSummary( ref string json )
	{
		try { return JsonSerializer.Deserialize<Summary>( json ); }
		catch { return null; }
	}

	#endregion
}