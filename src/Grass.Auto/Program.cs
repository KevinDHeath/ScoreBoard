using System.Diagnostics;
using Grass.Logic;
using Grass.Logic.Models;
namespace Grass.Auto;

internal class Program
{
	private static readonly Stopwatch sStopWatch = new();

	static void Main()
	{
		GameOptions options = new()
		{
			Players = Samples.GetPlayers(),
			CardComments = true,
			ReversePlay = true,
			AutoPlay = true,
			InProgress = false
		};

		GameService svc = new();
		Game game;
		try
		{
			game = svc.Setup( options );
			sStopWatch.Start();
			if( !AutoPlayAsync( svc ).Result ) { Console.WriteLine( "Failed to successfully auto-play game." ); }
			else
			{
				sStopWatch.Stop();
				Samples.ShowResults( game );

				//HtmlBuilder.CreateHtml( game );

				if( svc.Summaries.Count > 0 )
				{
					Console.WriteLine();
					Console.WriteLine( "Exported Game Summary:" );
					string json = svc.ExportSummary( svc.Summaries[0], true );
					Console.WriteLine( json );
				}
			}
		}
		catch( Exception ex ) { Console.WriteLine( ex.ToString() ); }
		finally
		{
			Console.WriteLine();
			Console.WriteLine( $"Runtime: {sStopWatch.Elapsed} milliseconds" );
			if( Environment.UserInteractive )
			{
				Console.Write( @"Press any key to continue . . . " );
				_ = Console.Read();
			}
		}
	}

	private static async Task<bool> AutoPlayAsync( GameService svc )
	{
		Task<bool> task = svc.GameAsync();
		// Do some other work if required
		return await task;
	}
}