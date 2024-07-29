using Grass.Logic.Models;
namespace Grass.Play.Components;

internal class PlayState
{
	internal bool ShowingDialog => ChosenCard is not null;

	internal Card? ChosenCard { get; set; }

	internal Hand? Hand { get; private set; }

	internal void ShowChooseActionDialog( Hand? hand, Card card )
	{
		Hand = hand;
		ChosenCard = card;
	}

	internal void CancelChooseActionDialog()
	{
		ChosenCard = null;
	}
}