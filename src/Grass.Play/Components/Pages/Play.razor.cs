using System.Text;
using Grass.Logic;
using Grass.Logic.Models;
namespace Grass.Play.Components.Pages;

public partial class Play
{
	[Microsoft.AspNetCore.Components.Parameter]
	public int Id { get; set; }

	private Player? Player { get; set; }

	private Hand? Hand { get; set; }

	private string? Message { get; set; }

	private string? Notify { get; set; } = null;

	// Collapsible sections
	private readonly CollapseExpand inHand = new();
	private readonly CollapseExpand stash = new();
	private readonly CollapseExpand score = new();

	// In-hand button
	private readonly HideShow button = new( hide: false );
	private readonly HideShow active = new( "In Hand" );
	private readonly HideShow scores = new( hide: false );

	private string? user = null;
	private string? userTitle = null;

	private void GameChanged( object? sender, EventArgs e ) => InvokeAsync( () => { Refresh( Player! ); } );

	protected override void OnParametersSet()
	{
		if( Id != Player?.Id )
		{
			if( Id > Service.Current.Players.Count ) { Id = 1; }
			Player = Service.Current.Players[Id - 1];
			Refresh( Player );
		}
	}

	protected override async Task OnAfterRenderAsync( bool firstRender )
	{
		if( firstRender )
		{
			string? name = await Home.GetName( JS );
			if( user is null && name is not null )
			{
				user = name;
				userTitle = "- " + user;
			}
			if( Player is not null ) { Refresh( Player ); }
			Service.GameChanged += GameChanged;
		}
	}

	private void Refresh( Player player )
	{
		if( Service.Current is null || player is null ) { return; }
		if( user is not null ) { active.Reset(); }
		Hand = player.Current;
		if( Service.Current.Winner is not null )
		{
			button.Hide(); active.Show(); scores.Show();
			Message = Service.Current.Winner != Player ? Service.Current.Winner.Name + " won" : "Winner!";
		}
		else
		{
			Message = string.Empty;
			if( player.Current.Count == 1 ) { scores.Hide(); } else { scores.Show(); }
			Notify = player.SetNotification();
			if( user is not null && player.Name == user ) { button.Hide(); active.Show(); }
		}
		StateHasChanged();
	}

	public void Dispose()
	{
		Service.GameChanged -= GameChanged;
		GC.SuppressFinalize( this );
	}

	private void PassCard()
	{
		if( Player is not null && Player.Pass )
		{
			_ = Service.PassCard( PlayState.Options );
		}
	}

	private void PlayCard()
	{
		Notify = null;
		if( Player is not null && Player.Play )
		{
			_ = Service.PlayCard( PlayState.Options );
		}
	}

	private void TradeCard()
	{
		if( Player is not null )
		{
			if( Player.Trade )
			{
				_ = Service.TradeCard( PlayState.Options );
			}
			else
			{
				Service.TradeRequest( PlayState.Options );
			}
		}
	}

	private void WasteCard()
	{
		Notify = null;
		if( Player is not null && Player.Play )
		{
			_ = Service.WasteCard( PlayState.Options );
		}
	}

	private string HasslePileHtml()
	{
		if( Hand is null ) { return "<tr></tr>"; }

		StringBuilder rtn = new( "<tr>" );
		Card? card = Hand.HasslePile.LastOrDefault();
		string wrk = card is not null
			? GameCard( card, showTitle: false )
			: "<td class='game-card' style='vertical-align: middle; border: 1px solid;'><i>Market not Active</i></td>";
		_ = rtn.AppendLine( wrk )

		.Append( $"<td><p style=\"margin-bottom: 0px; font-size: larger;\">Game total: {Home.FormatAmt( Player?.Total, true )}" );
		if( Service.Current.Winner is null )
		{
			rtn.Append( "<br>" )
			.Append( $"Stash total: {( Hand.Protected + Hand.UnProtected ):$###,##0}</p>" );
		}
		if( Hand.HasslePile.Count < 1 ) { _ = rtn.Append( "</td>" ); }
		else
		{
			_ = rtn.Append( Environment.NewLine )
			.AppendLine( "<div style=\"height: 110px; overflow: auto;\">" )
			.AppendLine( "<table style=\"height: 110px;\" class=\"table\">" )
			.AppendLine( "<tr><td><ul style=\"margin-left: 9px; margin-top: 0px;\">" );
			List<Card> reverse = new( Hand.HasslePile );
			reverse.Reverse();
			string li = "<li class=\"hassle-history\">";

			foreach( Card hassle in reverse )
			{
				//if( hassle.Id.StartsWith( "Market" ) ) { continue; }
				_ = rtn.AppendLine( $"{li}{hassle.Info.Caption} {hassle.Comment}</li>" );
			}
			_ = rtn.AppendLine( "</ul></td></tr>" )
			.AppendLine( "</table></div>" )
			.Append( "</td>" );
		}
		_ = rtn.AppendLine( "</tr>" );

		return rtn.ToString();
	}

	private string StashPileHtml()
	{
		if( Hand is null ) { return string.Empty; }
		if( Hand.StashView.Count == 0 ) { return "<tr><td><i>Nothing</i></td></tr>"; }

		StringBuilder rtn = new( "<tr>" );
		int idx = 0;
		foreach( Card card in Hand.StashView )
		{
			idx++;
			if( idx > 6 ) { _ = rtn.AppendLine( "</tr><tr>" ); idx = 1; }
			_ = rtn.AppendLine( GameCard( card ) );
		}
		rtn.Append( "</tr>" );
		return rtn.ToString();
	}

	private string ScoreCardsHtml()
	{
		if( Hand is null ) { return string.Empty; }
		List<Hand> scores = [];
		if( Player is not null && Player.Completed.Count > 0 )
		{
			scores = Player.Completed.OrderByDescending( h => h.Count ).ToList();
		}

		string sep = "</td><td class=\"align-right\">";
		StringBuilder rtn = new();
		foreach( Hand hand in scores )
		{
			int total = hand.NetScore + hand.Bonus;
			int peddle = -hand.HighestPeddle;
			_ = rtn.AppendLine( "<div class=\"column\">" )
			.AppendLine( "<table class=\"score-card\">" )
			.AppendLine( $"<tr><th colspan=\"2\">{hand.Count.DisplayWithSuffix()} Hand</th></tr>" )
			.AppendLine( $"<tr><td>Protected profit{sep}{Home.FormatAmt( hand.Protected )}</td></tr>" )
			.AppendLine( $"<tr><td>+ At risk profit{sep}{Home.FormatAmt( hand.UnProtected )}</td></tr>" )
			.AppendLine( $"<tr><td>- Banker's skim /+ bonus{sep}{Home.FormatAmt( hand.Skimmed )}</td></tr>" )
			.AppendLine( $"<tr><td>- Highest Peddle in hand{sep}{Home.FormatAmt( peddle )}</td></tr>" )
			.AppendLine( $"<tr><td>- Paranoia{sep}{Home.FormatAmt( hand.ParanoiaFines )}</td></tr>" )
			.AppendLine( $"<tr><td>= Net profit{sep}{Home.FormatAmt( hand.NetScore )}</td></tr>" )
			.AppendLine( $"<tr><td nowrap>+ Bonus for winner of hand{sep}{Home.FormatAmt( hand.Bonus )}</td></tr>" )
			.AppendLine( $"<tr><td>Total{sep}{Home.FormatAmt( total )}</td></tr>" )
			.AppendLine( $"</table>" )
			.AppendLine( $"</div>" );
		}
		return rtn.ToString();
	}

	private static string GameCard( Card card, bool showTitle = true )
	{
		string caption = card.Comment;
		string title = showTitle && caption.Length > 0 ? $"title=\"{card.Comment}\" " : string.Empty;
		return $"<td class=\"game-card\"><img class=\"image\" {title}src=\"./img/cards/{card.Image}\"></td>";
	}
}