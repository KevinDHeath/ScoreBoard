using Microsoft.AspNetCore.Components;
using Grass.Logic.Models;
using Grass.Logic;

namespace Grass.Play.Components.Pages;

public partial class Home
{
	[Inject]
	private IConfiguration? Configuration { get; set; }

	private bool AllowTests { get; set; } = false;

	private GameOptions? Options { get; set; }

	private Game? Current { get; set; }

	private bool HasWinner => Current is not null && Current.Winner is not null;

	private string Title { get; set; } = "Not started";

	protected override void OnInitialized()
	{
		// Why are Blazor life-cycle methods getting executed twice?
		// https://stackoverflow.com/questions/58075628/why-are-blazor-lifecycle-methods-getting-executed-twice
		// https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle?view=aspnetcore-8.0
		// https://learn.microsoft.com/en-us/training/modules/blazor-build-rich-interactive-components/4-improve-app-interactivity-lifecycle-events

		if( Configuration is not null )
		{
			string? val = Configuration["AllowTests"];
			AllowTests = val != null && bool.Parse( val );
			Configuration = null;
		}

		Options = Service.Options;
		if( Options is null )
		{
			Options = new() { Players = Samples.GetPlayers() };
			if( AllowTests )
			{
				Options.AutoPlay = true;
				Options.Sample = false;
				Options.EndGame = false;
			}
		}

		Current = Service.Current;
		if( Current is not null )
		{
			Title = HasWinner ? "Game over" : "In progress...";
		}
	}

	private void Submit()
	{
		if( Options is not null )
		{
			Current = Service.Setup( Options );
			Title = HasWinner ? "Game over" : "In progress...";
		}
	}

	internal static MarkupString FormatAmt( int? amt, bool dollar = true )
	{
		if( amt == null ) { return (MarkupString)string.Empty; }
		bool neg = amt < 0;
		if( neg ) { amt = Math.Abs( amt.Value ); }
		string rtn = neg ? "<span class=\"negative-value\">" : string.Empty;
		rtn += dollar ? amt.Value.ToString( "$###,##0" ) : amt.Value.ToString( "###,##0" );
		rtn += neg ? "</span>" : string.Empty;
		return (MarkupString)rtn;
	}
}