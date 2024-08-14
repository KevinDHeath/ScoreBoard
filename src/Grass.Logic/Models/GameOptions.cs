namespace Grass.Logic.Models;

/// <summary>Provides information for a Game.</summary>
public class GameOptions
{
	/// <summary>List of players in the game.</summary>
	public List<Player> Players { get; set; } = [];

	/// <summary>Game target value. The default is $250,000</summary>
	public int Target { get; set; } = 250000;

	/// <summary>Indicates whether to add card comments. The default is <c>true</c>.</summary>
	/// <remarks></remarks>
	public bool CardComments { get; set; } = true;

	/// <summary>Indicates whether to reverse the order of play ever alternate deal.
	/// The default is <c>false</c>.</summary>
	public bool ReversePlay { get; set; } = false;

	#region Properties for testing

	/// <summary>Indicates whether to use auto-play. The default is <c>false</c>.</summary>
	/// <remarks>
	/// <b>Important:</b><i> This should only be set as <c>true</c> for testing purposes
	/// as it automates the decision process of which card each player will play.</i>
	/// </remarks>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public bool AutoPlay { get; set; } = false;

	/// <summary>Indicates whether to populate a game for in-progress testing. The default is <c>false</c>.</summary>
	/// <remarks>
	/// <b>Important:</b><i> This depends on the <c>AutoPlay</c> property being set as <c>true</c>.</i>
	/// </remarks>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public bool InProgress { get; set; } = false;

	#endregion

	/// <summary>Indicates whether the maximum number of players has been reached.</summary>
	/// <returns><c>true</c> if the number of players is equal to the maximum.</returns>
	public bool IsMaxPlayers => Players.Count == Rules.cMaxNumber;

	private readonly StringComparison strCompare = StringComparison.OrdinalIgnoreCase;

	/// <summary>Check if a player can be added to the list.</summary>
	/// <param name="name">Name of the player.</param>
	/// <returns><c>false</c> is returned if the maximum number of players has been reached
	/// or a player with the same name is already present.</returns>
	public bool CanAddPlayer( string? name )
	{
		if( IsMaxPlayers || string.IsNullOrEmpty( name ) ) { return false; }
		if( Players.Any( p => string.Equals( p.Name, name, strCompare ) ) ) { return false; }
		return true;
	}

	/// <summary>Add a player to the list.</summary>
	/// <param name="name">Name of the player.</param>
	/// <returns><c>true</c> if the player was added.</returns>
	public bool AddPlayer( string name )
	{
		if( !CanAddPlayer( name ) ) { return false; }
		var player = new Player( name, Players.Count + 1 );
		Players.Add( player );
		return true;
	}
}