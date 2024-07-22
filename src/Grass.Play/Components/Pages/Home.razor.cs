using Grass.Logic.Models;
using Grass.Logic;

namespace Grass.Play.Components.Pages;

public partial class Home
{
	private GameOptions? Model { get; set; }

	private Game? Current { get; set; }

	private bool HasWinner => Current is not null && Current.Winner is not null;

	private bool AllowTests { get; set; } = true;

	private string Title { get; set; } = "Not started";

	private int Count {  get; set; }

	protected override void OnInitialized()
	{
		// Why are Blazor life-cycle methods getting executed twice?
		// https://stackoverflow.com/questions/58075628/why-are-blazor-lifecycle-methods-getting-executed-twice
		Model ??= new();
		Model.Players = Samples.GetPlayers();
		if( AllowTests )
		{
			Model.AutoPlay = true;
			Model.Sample = true;
			Model.EndGame = true;
		}
		if( Current is null && Service.Current is not null )
		{
			Current = Service.Current;
			Title = HasWinner ? "Game over" : "In progress...";
		}
	}

	private void Submit()
	{
		if( Model is not null )
		{
			Current = Service.Setup( Model );
			Title = HasWinner ? "Game over" : "In progress...";
		}
	}
}