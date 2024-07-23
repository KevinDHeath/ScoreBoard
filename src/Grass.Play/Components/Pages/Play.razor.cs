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

	// Collapsible sections
	private readonly CollapseExpand inHand = new();
	private readonly CollapseExpand stash = new();
	private readonly CollapseExpand score = new();

	// In-hand button
	private readonly HideShow button = new( hide: false );
	private readonly HideShow active = new( "In hand" );
	private readonly HideShow scores = new( hide: false );

	protected override void OnInitialized()
	{
		if( Id > 0 )
		{
			Player = Service.Current.Players[Id - 1];
			Hand = Player.Current;
			SetPage( Player );
		}
	}

	private void SetPage( Player player )
	{
		// Show the in-hand section when game-over so hide toggle button
		if( Service.Current.Winner is not null ) { button.Hide(); active.Show(); }
		// Hide the score cards when in-play and on 1st hand
		else if( player.Current.Count == 1 ) { scores.Hide(); }
	}

	private string HasslePileHtml()
	{
		if( Hand is null ) { return "<tr></tr>"; }

		StringBuilder rtn = new( "<tr>" );
		Card? card = Hand.HasslePile.LastOrDefault();
		string wrk = card is not null
			? GameCard( card, showTitle: false )
			: "<td class='game-card' style='vertical-align: middle; border: 1px solid;'><i>Market not Open</i></td>";
		_ = rtn.AppendLine( wrk ) 

		.Append( $"<td><p style=\"margin-bottom: 0px; font-size: larger;\">Game total: {Home.FormatAmt( Player?.Total, true )}<br>" )
		.Append( $"Stash total: {( Hand.Protected + Hand.UnProtected ):$###,##0}</p>" );

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

	private string InHandHtml()
	{
		if( Hand is null ) { return string.Empty; }

		StringBuilder rtn = new( "<tr>" );
		foreach( Card card in Hand.InHand )
		{
			_ = rtn.AppendLine( GameCard( card ) );
		}
		rtn.Append( "</tr>" );
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
			_ = rtn.AppendLine( "<div class=\"column\">" )
			.AppendLine( "<table class=\"score-card\">" )
			.AppendLine( $"<tr><th colspan=\"2\">{hand.Count.DisplayWithSuffix()} Hand</th></tr>" )
			.AppendLine( $"<tr><td>Protected profit{sep}{Home.FormatAmt( hand.Protected )}</td></tr>" )
			.AppendLine( $"<tr><td>+ At risk profit{sep}{Home.FormatAmt( hand.UnProtected )}</td></tr>" )
			.AppendLine( $"<tr><td>- Banker's skim /+ bonus{sep}{Home.FormatAmt( hand.Skimmed )}</td></tr>" )
			.AppendLine( $"<tr><td>- Highest Peddle in hand{sep}{Home.FormatAmt( hand.HighestPeddle )}</td></tr>" )
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
		return $"<td class=\"game-card\"><img class=\"image\" {title}src=\"./img/cards/{card.Id}.png\"></td>";
	}
}