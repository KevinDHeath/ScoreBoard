using Microsoft.AspNetCore.Components;
using Grass.Logic.Models;
namespace Grass.Play.Components.Pages;

public partial class Home
{
	[Inject]
	private IConfiguration? Configuration { get; set; }

	private bool AllowTests { get; set; } = false;

	private bool AllowRegister => Options.Players.Count < 6;

	private bool AllowStart => HasWinner || (!AllowTests && Options.Players.Count > 1) || AllowTests;

	private bool HasWinner => Current is not null && Current.Winner is not null;

	private GameOptions Options { get { return Service.Options; } set { Service.Options = value; } }

	private Game? Current { get; set; }

	private string Title { get; set; } = "Not started";

	private string playerName = string.Empty;
	private string? userTitle = null;

	private void GameChanged( object? sender, EventArgs e ) => InvokeAsync( Refresh );

	protected override void OnInitialized()
	{
		if( Configuration is not null )
		{
			string? val = Configuration["AllowTests"];
			AllowTests = val != null && bool.Parse( val ); // comment out to switch off tests
			Configuration = null;
		}
		Options = Service.Options;
		if( Options is null )
		{
			Options = new();
			if( AllowTests )
			{
				Options.AllowTests = true;
				Options.AutoPlay = true;
				Options.InProgress = false;
			}
		}
	}

	protected override async Task OnAfterRenderAsync( bool firstRender )
	{
		if( firstRender )
		{
			var res = await ProtectedSessionStore.GetAsync<string?>( "user" );
			if( userTitle is null && res.Value is not null ) { userTitle = "- " + res.Value; }
			Refresh();
			Service.GameChanged += GameChanged;
		}
	}

	private void Refresh()
	{
		Current = Service.Current;
		if( Current is null ) { return; }
		Title = HasWinner ? "Game over" : "In progress...";
		StateHasChanged();
	}

	public void Dispose()
	{
		Service.GameChanged -= GameChanged;
		GC.SuppressFinalize( this );
	}

	private async Task AddPlayer()
	{
		if( Options is not null && Options.CanAddPlayer( playerName ) )
		{
			var res = await ProtectedSessionStore.GetAsync<string?>( "user" );
			if( res.Value is null )
			{
				Options.AddPlayer( playerName );
				await ProtectedSessionStore.SetAsync( "user", playerName );
				userTitle = "- " + playerName;
			}
		}
	}

	private void Submit()
	{
		if( Options is not null )
		{
			Current = Service.Setup( Options );
			Refresh();
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