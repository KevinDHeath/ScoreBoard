using System.ComponentModel.DataAnnotations;
namespace Grass.Logic.Models;

/// <summary>Provides information for playing a card.</summary>
public class PlayOptions
{
	/// <summary>Current player.</summary>
	public Player? Player { get; set; }

	/// <summary>Selected card to play.</summary>
	public Card? Card { get; set; }

	/// <summary>Result of whether the chosen card can be played.</summary>
	public PlayResult CanPlay { get; internal set; } = PlayResult.Success!;

	/// <summary>Indicates whether the chosen card can be discarded.</summary>
	public bool CanDiscard { get; internal set; }

	/// <summary>Other player involved.</summary>
	[Range( 1, 6, ErrorMessage = "Need another player to play this card." )]
	public int OtherId { get; set; } = 0;

	/// <summary>Other cards involved.</summary>
	public List<Card> OtherCards { get; internal set; } = [];

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