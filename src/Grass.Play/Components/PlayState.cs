using Grass.Logic.Models;
namespace Grass.Play.Components;

internal class PlayState
{
	internal PlayOptions Options { get; set; } = new();

	internal bool IsOpen => Options.ChosenCard is not null;

	internal void Show( Player? player, Card card )
	{
		Options.ChosenCard = card;
		Options.Player = player;
	}

	internal void Cancel()
	{
		Options.ChosenCard = null;
	}
}