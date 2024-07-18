using Grass.Logic.Models;
using System.ComponentModel;
namespace Grass.Logic;

/// <summary>Service providing actions to be taken with cards during a Grass game.</summary>
public class GameService : PassCardHandler
{
	internal readonly Game _game;
	private bool _disposed;

	/// <summary>Initializes a new instance of the <see cref="GameService"/> class.</summary>
	/// <param name="game">Game to be played.</param>
	public GameService( Game game )
	{
		_game = game;
		if( _game.Auto ) { _game.GameChanged += OnParanoiaPlayed; }
	}

	/// <summary>Initializes a new Grass game.</summary>
	/// <param name="players">List of players in the game.</param>
	/// <param name="target">Target for the game. The default is $250,000</param>
	/// <param name="reverse">Indicates whether to reverse the play order ever alternate deal.
	/// The default is <c>false</c>.</param>
	/// <param name="comment">Indicates whether to add card comments. The default is <c>true</c>.</param>
	/// <param name="auto">Indicates whether to use auto-play. The default is <c>false</c>.
	/// <br/><b>Note:</b><i> This should only be set as <c>true</c> for testing purposes
	/// as it automates the decision process of which card each player will play.</i></param>
	/// <returns>An initialized game of Grass.</returns>
	public static Game Setup( List<Player> players, int target = 250000,
		bool reverse = false, bool comment = true, bool auto = false )
	{
		Game game = new( players, target, reverse, comment, auto );
		return game;
	}

	/// <summary>Play a game asynchronously.</summary>
	/// <returns><c>true</c> is returned if the game was completed successfully.</returns>
	/// <remarks><c>await Task.Run()</c> scenarios can be avoided if the created task is returned directly.</remarks>
	/// <seealso href="https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios">
	/// Asynchronous programming scenarios</seealso>
	public Task<bool> GameAsync() => Task.Run( () => { return _game.Play(); } );

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

	/// <summary>Add card to pass due to paranoia being played.</summary>
	/// <param name="player">Player object.</param>
	/// <param name="card">Card object.</param>
	/// <returns><see langword="false"/> if the player has already added a card or
	/// the card is not in the players hand.</returns>
	public bool AddCardToPass( Player player, Card card ) => _game.AddCardToPass( player, card );

	/// <summary>Discard a card.</summary>
	/// <param name="player">Current player.</param>
	/// <param name="card">Card to discard.</param>
	/// <returns><see langword="true"/> if the card is successfully discarded.</returns>
	public bool Discard( Player player, Card card ) => _game.Discard( player, card );

	/// <summary>Play a card in the players current hand.</summary>
	/// <param name="player">Current player.</param>
	/// <param name="card">Card to play.</param>
	/// <returns>A <see cref="PlayResult" /> object representing the results
	/// of the play.</returns>
	/// <remarks>The <c>null</c> return value is used to indicate success. The result should be compared
	/// to <see cref="PlayResult.Success"/> rather than checking for <c>null</c>.</remarks>
	public PlayResult Play( Player player, Card card ) => _game.Play( player, card );

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

	/// <summary>Take the next card from the stack.</summary>
	/// <param name="hand">Current player hand to add the card to.</param>
	/// <returns><see langword="true"/> if the card is successfully taken
	/// from the stack and added to the players hand.</returns>
	public bool Take( Hand hand ) => _game.Take( hand );

	#endregion
}