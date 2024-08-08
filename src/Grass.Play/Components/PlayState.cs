#region Modification History
// Date         Developer    Description
// -----------  ------------ -----------
// 08 Aug 2024  kdheath      Clear other player Id on dialog cancel.
// 29 Jul 2024  kdheath      Class created.
#endregion

using Grass.Logic.Models;
namespace Grass.Play.Components;

/// <summary>Controls the state of the Card Action modal dialog.</summary>
internal class PlayState
{
	internal PlayOptions Options { get; set; } = new();

	internal bool IsOpen => Options.Card is not null;

	internal void Show( Player? player, Card card )
	{
		Options.Card = card;
		Options.Player = player;
	}

	internal void Cancel()
	{
		Options.Card = null;
		Options.OtherId = 0;
	}
}