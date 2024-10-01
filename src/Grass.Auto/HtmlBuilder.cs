using Grass.Logic;
using Grass.Logic.Models;
using System.Text;

namespace Grass.Auto;

internal class HtmlBuilder()
{
	private const string cSub = @"samples\AutoPlay";
	private const string cUrl = "https://KevinDHeath.github.io/score/media/";

	internal static void CreateHtml( Game game )
	{
		FileInfo fi = new( @"..\..\..\html-template.txt" ); // Load the template
		if( !fi.Exists ) { return; }

		string? output = fi.DirectoryName?.Replace( "Grass.Auto", cSub );
		if( output is null ) { return; }
		DirectoryInfo target = new( output );
		if( !target.Exists ) { target.Create(); } // Set the target folder

		foreach( Player p in game.Players )
		{
			StringBuilder html = new( File.ReadAllText( fi.FullName ) );
			_ = html.Replace( "{title}", "Auto play" );
			_ = html.Replace( "{hand-count}", $"{p.Scores.Count}" );
			_ = html.Replace( "{page-header}", PageHeader( game, p ) );
			_ = html.Replace( "{menu-label}",  MenuLabel( p ) );
			_ = html.Replace( "{hassle-pile}", HasslePile( p ) );
			_ = html.Replace( "{in-hand}",     InHand( p ) );
			_ = html.Replace( "{stash-pile}",  StashPile( p.Current ) );
			_ = html.Replace( "{score-cards}", ScoreCards( p.Scores ) );
			_ = html.Replace( "{player-list}", PlayerList( game.Players, p.Name ) );

			string outFile = p.Name + ".html";
			File.WriteAllText( Path.Combine( target.FullName, outFile ), html.ToString() );
		}
		Console.WriteLine( @"Html output in " + target.FullName );
	}

	#region Section Builders

	private static string PageHeader( Game game, Player p )
	{
		StringBuilder rtn = new();
		string indent = new( ' ', 4 );
		string msg;
		if( game.Winner is not null ) { msg = p == game.Winner ? "Winner!" : $"{game.Winner.Name} won"; }
		else { msg = @"Hand #{p.Hands.Count}"; }

		_ = rtn.AppendLine( indent + $"<div class=\"level-left\">Game target: {game.Target:$#,###,##0}</div>" );
		_ = rtn.AppendLine( indent + $"<div class=\"level-center\">{msg}</div>" );
		_ = rtn.Append( indent + $"<div class=\"level-right game-datetime\">{game.Date}</div>" );
		return rtn.ToString();
	}

	private static string MenuLabel( Player player )
	{
		return $"<p class=\"menu-label\"><strong>{player.Name}</strong></p>";
	}

	private static string HasslePile( Player player )
	{
		string indent = new( ' ', 10 );
		Hand hand = player.Current;
		Card? status = hand.HasslePile.LastOrDefault();
		StringBuilder rtn = new();

		if( status is null ) { _ = rtn.AppendLine( indent +
			"<td class=\"game-card\" style=\"vertical-align: middle; border: 1px solid;\"><i>Market not Open</i></td>" ); }
		else { _ = rtn.AppendLine( indent + GameCard( status ) ); }

		_ = rtn.Append( indent + $"<td><p style=\"margin-bottom: 0px;\">Game total: {Format( player.Total, true )}<br/>" );
		_ = rtn.Append( $"Stash total: {( hand.Protected + hand.UnProtected ):$###,##0}</p>" );
		if( hand.HasslePile.Count < 2 ) { _ = rtn.Append( "</td>" ); }
		else // include hassle history
		{
			_ = rtn.Append( Environment.NewLine );
			_ = rtn.AppendLine( indent + "  <div style=\"height: 110px; overflow: auto;\"><table style=\"height: 110px;\" class=\"table\">" );
			_ = rtn.AppendLine( indent + "    <tr><td><ul style=\"margin-left: 9px; margin-top: 0px;\">" );
			List<Card> reverse = new( hand.HasslePile );
			reverse.Reverse();
			string li = indent + "      <li class=\"hassle-history\">";
			foreach( Card card in reverse )
			{
				if( card.Id.StartsWith( "Market" ) ) { continue; }
				_ = rtn.AppendLine( li + $"{card.Info.Caption} {card.Comment}</li>" );
			}
			_ = rtn.AppendLine( indent + "    </ul></td></tr>" );
			_ = rtn.Append( indent + "  </table></div>" );
			_ = rtn.Append( Environment.NewLine + indent + "</td>" );
		}
		return rtn.ToString();
	}

	// Maximum of 7 columns when taking a turn
	private static string InHand( Player player )
	{
		StringBuilder rtn = new();
		string indent = new( ' ', 12 );
		int idx = 0;
		IReadOnlyCollection<Card> list = player.Current.InHand;
		foreach( Card card in list )
		{
			idx++;
			_ = rtn.Append( indent + GameCard( card ) );
			if( idx < list.Count ) { _ = rtn.Append( Environment.NewLine ); }
		}
		return rtn.ToString();
	}

	// Can be multiple rows, 6 per row
	private static string StashPile( Hand hand )
	{
		StringBuilder rtn = new();
		string indent = new( ' ', 8 );
		int idx = 0;
		List<Card> list = hand.StashView;

		if( list.Count == 0 ) { _ = rtn.Append( indent + "<tr><td><i>Nothing</i></td></tr>" ); }
		else { _ = rtn.AppendLine( indent + "<tr>" ); }
		foreach( Card card in list )
		{
			idx++;
			if( idx > 6 ) { _ = rtn.AppendLine( indent + "</tr><tr>" ); idx = 1; }
			_ = rtn.AppendLine( indent + "  " + GameCard( card ) );
		}
		if( list.Count > 0 ) { _ = rtn.Append( indent + "</tr>" ); }
		return rtn.ToString();
	}

	private static string ScoreCards( List<Score> scores )
	{
		if( scores.Count == 0 ) { return string.Empty; }
		StringBuilder rtn = new();
		string indent = new( ' ', 8 );
		string row = indent + @"      <tr><td";

		_ = rtn.AppendLine( indent + "<div class=\"columns is-multiline\">" );
		foreach( Score score in scores )
		{
			int total = score.NetProfit + score.Bonus;
			_ = rtn.AppendLine( indent + "  <div class=\"column\">" );
			_ = rtn.AppendLine( indent + "    " + "<table class=\"score-card\">" );
			_ = rtn.AppendLine( indent + @$"      <tr><th colspan='2'>{score.Number.DisplayWithSuffix()} Hand - {score.Reason}</th></tr>" );
			_ = rtn.AppendLine( row + $">Protected profit</td><td class=\"align-right\">{Format( score.Protected )}</td></tr>" );
			_ = rtn.AppendLine( row + $">+ At risk profit</td><td class=\"align-right\">{Format( score.UnProtected )}</td></tr>" );
			_ = rtn.AppendLine( row + $">- Banker's skim /+ bonus</td><td class=\"align-right\">{Format( score.Skimmed )}</td></tr>" );
			_ = rtn.AppendLine( row + $">- Highest Peddle in hand</td><td class=\"align-right\">{Format( score.HighestPeddle )}</td></tr>" );
			_ = rtn.AppendLine( row + $">- Paranoia</td><td class=\"align-right\">{Format( score.ParanoiaFines )}</td></tr>" );
			_ = rtn.AppendLine( row + $">= Net profit</td><td class=\"align-right\">{Format( score.NetProfit )}</td></tr>" );
			_ = rtn.AppendLine( row + @$" nowrap>+ Bonus for winner of hand</td><td class='align-right'>{Format( score.Bonus )}</td></tr>" );
			_ = rtn.AppendLine( row + $">Total</td><td class=\"align-right\">{Format( total )}</td></tr>" );
			_ = rtn.AppendLine( indent + "    " + "</table>" );
			_ = rtn.AppendLine( indent + "  </div>" );
		}
		_ = rtn.Append( indent + "</div>" );
		return rtn.ToString();
	}

	private static string PlayerList( List<Player> players, string current )
	{
		StringBuilder rtn = new();
		string indent = new( ' ', 10 );
		int idx = 0;
		foreach( Player p in players )
		{
			idx++;
			string link = p.Name == current ? "class=\"player-current" : $"href=\"{p.Name}.html";
			_ = rtn.Append( indent + $"<li><a {link}\">{p.Name}</a></li>" );
			if( idx < players.Count ) { _ = rtn.Append( Environment.NewLine ); }
		}
		return rtn.ToString();
	}

	#endregion

	#region Helper Methods

	private static string GameCard( Card card )
	{
		string caption = card.Comment;
		string title = caption.Length > 0 ? $"title=\"{card.Comment}\" " : string.Empty;
		return $"<td class=\"game-card\"><img class=\"image\" {title}src=\"{cUrl}{card.Id}.png\"></td>";
	}

	private static string Format( int amt, bool dollar = false )
	{
		bool neg = amt < 0;
		if( neg ) { amt = Math.Abs( amt ); }
		string rtn = neg ? "<span class=\"negative-value\">" : string.Empty;
		rtn += dollar ? amt.ToString( "$###,##0" ) : amt.ToString( "###,##0" );
		rtn += neg ? "</span>" : string.Empty;
		return rtn;
	}

	internal static bool CopyForTesting( DirectoryInfo source )
	{
		string user = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile );
		DirectoryInfo target = new( user + @"\source\Working\WIP\github\PagesTest\score\html\" + cSub );
		if( target.Exists )
		{
			foreach( FileInfo file in source.GetFiles() )
			{ file.CopyTo( Path.Combine( target.FullName, file.Name ), true ); }
			Console.WriteLine( "Copied to test " + target.FullName );

		}
		return target.Exists;
	}

	#endregion
}