#region Modification History
// Date         Developer    Description
// -----------  ------------ -----------
// 02 Aug 2024  kdheath      Class created.
#endregion

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

	/// <summary>Indicates whether the card can be traded.</summary>
	public CardInfo? TradeFor { get; internal set; }

	/// <summary>Id of other player involved.</summary>
	[Range( 1, 6, ErrorMessage = "Please select an option for this card." )]
	public int OtherId { get; set; } = 0;

	/// <summary>Collection of other cards involved.</summary>
	internal List<Card> OtherCards { get; set; } = [];

	internal PlayOptions SetTradeRq()
	{
		return new PlayOptions
		{
			Player = Player,
			Card = Card,
			TradeFor = TradeFor
		};
	}

	internal void Reset()
	{
		if( Player is not null ) Player.Notify = null;
		Player = null;
		Card = null;
		OtherId = 0;
		OtherCards.Clear();
		CanDiscard = false;
		CanPlay = PlayResult.Success!;
		TradeFor = null;
	}
}