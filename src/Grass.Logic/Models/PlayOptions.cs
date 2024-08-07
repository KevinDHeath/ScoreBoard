using System.ComponentModel.DataAnnotations;
namespace Grass.Logic.Models;

/// <summary>Provides information for playing a card.</summary>
public class PlayOptions
{
	/// <summary>Current player.</summary>
	public Player? Player { get; set; }

	/// <summary>Selected card to play.</summary>
	public Card? Card { get; set; }

	/// <summary>Result of whether the card can be played.</summary>
	public PlayResult CanPlay { get; internal set; } = PlayResult.Success!;

	/// <summary>Indicates whether the card can be discarded.</summary>
	public bool CanDiscard { get; internal set; }

	/// <summary>Id of other player involved.</summary>
	[Range( 1, 6, ErrorMessage = "Select an option for this card." )]
	public int OtherId { get; set; } = 0;

	/// <summary>Collection of other cards involved.</summary>
	internal List<Card> OtherCards { get; set; } = [];

	internal void Reset()
	{
		Player = null;
		Card = null;
		OtherId = 0;
		OtherCards.Clear();
		CanDiscard = false;
		CanPlay = PlayResult.Success!;
	}
}