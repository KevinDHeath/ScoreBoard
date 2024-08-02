namespace Grass.Logic.Models;

/// <summary>Provides information for a Card play.</summary>
[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
public class PlayOptions
{
	/// <summary>Current player.</summary>
	public Player? Player { get; set; }

	/// <summary>Selected card to play.</summary>
	public Card? ChosenCard { get; set; }

	/// <summary>Other player involved.</summary>
	public int OtherId { get; set; } = 0;

	/// <summary>Other cards involved.</summary>
	public List<Card> OtherCards { get; set; } = [];
}