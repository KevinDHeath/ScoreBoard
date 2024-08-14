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
	public GameOptions Options { get { return _options; } set { _options = value; } }

	/// <summary>Current game.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public Game Current => _game;

	/// <summary>Setup and start a new Grass game.</summary>
	/// <param name="options">Options for the Game.</param>
	/// <returns>An initialized game object with the provided options.</returns>
	public Game Setup( GameOptions options )
	{
		if( options.AutoPlay && options.Players.Count == 0 )
		{
			options.Players = Samples.GetPlayers();
		}

		foreach( var player in options.Players ) { player.Reset(); }
		_options = options;
		_game = new( options.Players, options.Target, options.ReversePlay, options.CardComments, options.AutoPlay );

		// Play the game
		if( _game.Auto && options.InProgress )
		{
			_game.Auto = false; // switch off auto-play when populating in-progress sample
			Samples.InProgress( _game );
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

		return _game;
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

	/// <summary>Play a game asynchronously.</summary>
	/// <returns><c>true</c> is returned if the game was completed successfully.</returns>
	/// <remarks><c>await Task.Run()</c> scenarios can be avoided if the created task is returned directly.</remarks>
	/// <seealso href="https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios">
	/// Asynchronous programming scenarios</seealso>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public Task<bool> GameAsync() => Task.Run( () => { return _game.Play(); } );

	internal static List<CardInfo> sCards = CardInfo.Info();

	internal static string GetCardCaption( string cardId )
	{
		var info = sCards.FirstOrDefault( c => c.Id == cardId );
		return info is not null ? info.Caption : string.Empty;
	}

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

	/// <summary>Checks whether a card can be played.</summary>
	/// <param name="options">Card play options.</param>
	/// <returns>Collection of amounts and players that can be used to hassle.</returns>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public Dictionary<Player, int> CheckPlay( PlayOptions options )
	{
		Dictionary<Player, int> rtn = [];
		if( options.Player is null || options.Card is null ) { return rtn; }
		Player player = options.Player;
		Card card = options.Card;

		options.OtherCards.Clear();
		options.CanPlay = Rules.CanPlay( player.Current, card );
		options.CanDiscard = AllowDiscard( card );
		if( options.CanPlay != PlayResult.Success ) { return rtn; }

		if( card.Type == CardInfo.cHeatOn || card.Id == CardInfo.cSteal )
		{
			foreach( Player other in _game.Players )
			{
				if( player == other || !other.Current.MarketIsOpen ) { continue; }
				if( card.Type == CardInfo.cHeatOn )
				{
					int total = other.Total + other.Current.Protected + other.Current.UnProtected;
					if( total > 0 ) { rtn.Add( other, total ); }
				}
				else
				{
					if( other.Current.HighestUnProtected is not null )
					{
						int total = other.Current.HighestUnProtected.Info.Value;
						if( total >= 25000 )
						{
							rtn.Add( other, total );
						}
					}
				}
			}
			if( rtn.Count == 0 ) { options.CanPlay = new( "No players to hassle." ); }
			else
			{
				rtn = rtn.OrderByDescending( p => p.Value ).ToDictionary( p => p.Key, p => p.Value );
			}
		}
		else if( card.Type == CardInfo.cProtection )
		{
			options.OtherCards = Card.GetPeddlesToProtect( player.Current.StashPile, card.Info.Value );
		}
		return rtn;
	}

	#region Action Methods

	/// <summary>Add a card to pass due to paranoia being played.</summary>
	/// <param name="options">Play card options.</param>
	/// <returns><c>false</c> if the player has already added a card or
	/// the card is not in the players hand.</returns>
	public bool CardToPass( PlayOptions options )
	{
		if( options.Player is null || options.Card is null ) { return false; }
		bool rtn = _game.AddCardToPass( options.Player, options.Card );
		if( rtn ) { options.Reset(); }
		return rtn;
	}

	/// <summary>Discard a card in a players current hand.</summary>
	/// <param name="options">Play card options.</param>
	/// <returns><c>true</c> if the card is successfully discarded.</returns>
	public bool Discard( PlayOptions options )
	{
		if( options.Player is null || options.Card is null ) { return false; }
		bool rtn = _game.Discard( options.Player, options.Card );
		if( rtn )
		{
			bool over = _game.SetNextPlayer( options.Player );
			if( over ) { StoreSummary( _game ); }
			options.Reset();
		}
		return rtn;
	}

	/// <summary>Play a card in a players current hand.</summary>
	/// <param name="options">Play card options.</param>
	/// <returns>An object representing the play result.</returns>
	/// <remarks>The <c>null</c> return value is used to indicate success. The result should be compared
	/// to <see cref="PlayResult.Success"/> rather than checking for <c>null</c>.</remarks>
	public PlayResult Play( PlayOptions options )
	{
		PlayResult rtn = new( $"Missing player or card." );
		if( options.Player is null || options.Card is null ) { return rtn; }
		Card card = options.Card;

		if( options.OtherCards.Count > 0 )
		{
			rtn = _game.Protect( options.Player, card, options.OtherCards );
		}
		else if( options.OtherId > 0 )
		{
			Player? other = Current.Players.FirstOrDefault( p => p.Id == options.OtherId );
			if( other is not null )
			{
				Card? oCard = card.Type == CardInfo.cHeatOn ? card : other.Current.HighestUnProtected;
				if( oCard is not null ) { rtn = _game.Play( options.Player, card, other, oCard ); }
			}
		}
		else { rtn = _game.Play( options.Player, card ); }

		if( rtn == PlayResult.Success )
		{
			bool over = false;
			if( card.Id == CardInfo.cClose ) { over = _game.CheckWinner( options.Player ); }
			else if( card.Type != CardInfo.cParanoia ) { over = _game.SetNextPlayer( options.Player ); }
			if( over ) { StoreSummary( _game ); }
			options.Reset();
		}
		return rtn;
	}

	#endregion

	#region Game Summaries

	private readonly JsonSerializerOptions options = new() { WriteIndented = false };

	/// <summary>Collection of Game summaries.</summary>
	public List<Summary> Summaries { get; set; } = [];

	internal void StoreSummary( Game game )
	{
		Summaries.Add( Summary.BuildSummary( game ) );
	}

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