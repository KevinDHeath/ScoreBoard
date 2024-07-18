using System.Diagnostics;
using Grass.Logic;
using Grass.Logic.Models;
namespace Grass.Auto;

internal class Program
{
	private static readonly Stopwatch sStopWatch = new();

	static void Main()
	{
		Game game = GameService.Setup( Samples.GetPlayers(), auto: true, comment: true,
			reverse: true );
		try
		{
			GameService svc = new( game );
			//Game sample = Samples.Populate( svc, endgame: true ); // endgame:false will cause issue

			sStopWatch.Start();
			if( !AutoPlayAsync( svc ).Result ) { Console.WriteLine( "Failed to successfully auto-play game." ); }
			else
			{
				sStopWatch.Stop();
				Samples.ShowResults( game );
				//HtmlBuilder.CreateHtml( game );

				//Summary summary = Summary.BuildSummary( game );
				//string json = System.Text.Json.JsonSerializer.Serialize( summary );
				//Summary? obj = System.Text.Json.JsonSerializer.Deserialize<Summary>( json );
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