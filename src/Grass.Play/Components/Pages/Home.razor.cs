using Microsoft.AspNetCore.Components;
using Grass.Logic.Models;
using Microsoft.JSInterop;
namespace Grass.Play.Components.Pages;

public partial class Home
{
	private bool AllowTests { get; set; } = false;

	private bool AllowRegister => !Options.IsMaxPlayers && userTitle is null;

	private bool AllowStart => (CanStart && !AllowTests && Options.Players.Count > 1) || AllowTests;

	private bool CanStart => Current == null || HasWinner;

	private bool HasWinner => Current is not null && Current.Winner is not null;

	private GameOptions Options { get { return Service.Options; } set { Service.Options = value; } }

	private Game? Current { get; set; }

	private string Title { get; set; } = "Not started";

	private string playerName = string.Empty;
	private string? userTitle = null;
	private const string cTest = "allow-tests";

	private void GameChanged( object? sender, EventArgs e ) => InvokeAsync( Refresh );

	protected override void OnInitialized()
	{
		Options = Service.Options;
		Options ??= new();
	}

	protected override async Task OnAfterRenderAsync( bool firstRender )
	{
		if( firstRender )
		{
			string? name = await GetName( JS );
			if( userTitle is null && name is not null && name != cTest ) { userTitle = "- " + name; }
			Refresh();
			Service.GameChanged += GameChanged;
		}
	}

	private void Refresh()
	{
		Current = Service.Current;
		if( Current is not null ) { Title = HasWinner ? "Game over" : "In progress..."; }
		StateHasChanged();
	}

	public void Dispose()
	{
		Service.GameChanged -= GameChanged;
		GC.SuppressFinalize( this );
	}

	internal static async Task<string?> GetName( IJSRuntime JS ) =>
		JS != null ? await JS.InvokeAsync<string?>( "PlayApp.getName" ) : null;

	private async Task<bool> SetName( string name ) =>
		JS != null && await JS.InvokeAsync<bool>( "PlayApp.setName", name );

	private async Task AddPlayer()
	{
		bool ok = false;
		if( string.Equals( playerName, cTest, StringComparison.OrdinalIgnoreCase ) )
		{
			ok = true;
			AllowTests = ok;
			Options.AllowTests = AllowTests;
		}
		else if( Options is not null && Options.CanAddPlayer( playerName ) )
		{
			ok = await SetName( playerName );
			if( ok )
			{
				Service.RegisterPlayer( playerName );
				userTitle = "- " + playerName;
			}
		}
		if( ok )
		{
			playerName = string.Empty;
			StateHasChanged();
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