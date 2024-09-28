using System.Text.Json.Serialization;
namespace Grass.Logic.Models;

/// <summary>Score card for each player per hand.</summary>
public class Score
{
	/// <summary>Hand number.</summary>
	[JsonInclude]
	public int Number { get; internal set; }

	/// <summary>Protected profit.</summary>
	[JsonInclude]
	public int Protected { get; internal set; }

	/// <summary>At risk profit.</summary>
	[JsonInclude]
	public int UnProtected { get; internal set; }

	/// <summary>Banker's skim.</summary>
	/// <remarks>A negative value for what was skimmed from the player, a positive value
	/// for what was added as a bonus to the player holding the Banker card.</remarks>
	[JsonInclude]
	public int Skimmed { get; internal set; }

	/// <summary>Highest Peddle in the player's hand.</summary>
	[JsonInclude]
	public int HighestPeddle { get; internal set; }

	/// <summary>Paranoia fines.</summary>
	[JsonInclude]
	public int ParanoiaFines { get; internal set; }

	/// <summary>Net profit.</summary>
	/// <remarks>This is the total peddle money in the stash minus the
	/// highest peddle value held and all paranoia fines held.</remarks>
	[JsonInclude]
	public int NetProfit { get; internal set; }

	/// <summary>Bonus for winner of hand.</summary>
	[JsonInclude]
	public int Bonus { get; internal set; }

	/// <summary>Total for the player's hand.</summary>
	public int Total => NetProfit + Bonus;

	/// <summary>End of hand reason.</summary>
	public string Reason { get; internal set; } = string.Empty;
}