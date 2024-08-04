using System.ComponentModel.DataAnnotations;
namespace Grass.Logic.Models;

/// <summary>Provides information for playing a card.</summary>
public class PlayOptions
{
	/// <summary>Current player.</summary>
	public Player? Player { get; set; }

	/// <summary>Selected card to play.</summary>
	public Card? ChosenCard { get; set; }

	/// <summary>Other player involved.</summary>
	[Range( 1, 6, ErrorMessage = "Need another player to play card." )]
	public int OtherId { get; set; } = 0;

	/// <summary>Other cards involved.</summary>
	public List<Card> OtherCards { get; set; } = [];

	internal void Reset()
	{
		Player = null;
		ChosenCard = null;
		OtherId = 0;
		OtherCards.Clear();
	}
}