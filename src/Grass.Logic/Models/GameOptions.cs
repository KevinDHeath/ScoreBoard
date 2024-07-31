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
}