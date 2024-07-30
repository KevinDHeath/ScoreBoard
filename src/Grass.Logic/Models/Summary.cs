using System.Text.Json.Serialization;
namespace Grass.Logic.Models;

/// <summary>Game summary.</summary>
public class Summary
{
	/// <summary>Game date.</summary>
	[JsonInclude]
	public string Date { get; private set; } = string.Empty;

	/// <summary>Game target value.</summary>
	[JsonInclude]
	public int Target { get; private set; }

	/// <summary>Winning total value.</summary>
	[JsonInclude]
	public int Total { get; private set; }

	/// <summary>Number of hands.</summary>
	[JsonInclude]
	public int Hands { get; private set; }

	/// <summary>Winner.</summary>
	[JsonInclude]
	public string Winner { get; private set; } = string.Empty;

	/// <summary>Reason why the game finished.</summary>
	[JsonInclude]
	public string EndReason { get; private set; } = string.Empty;

	/// <summary>Players and score cards.</summary>
	[JsonInclude]
	public Dictionary<string, List<Score>> Players { get; internal set; } = [];

	/// <summary>Build a summary of a game.</summary>
	/// <param name="game">Game to be summarized.</param>
	/// <returns>An object that can be serialized as JSON.</returns>
	/// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to">
	/// How to write .NET objects as JSON (serialize)</seealso>
	/// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/immutability">
	/// Use immutable types and properties</seealso>

	internal static Summary BuildSummary( Game game )
	{
		Summary rtn = new()
		{
			Date = game.Date,
			Target = game.Target,
			Hands = game.Hand
		};

		if( game.Winner is not null )
		{
			rtn.Winner = game.Winner.Name;
			rtn.Total = game.Winner.Total;
			rtn.EndReason = game.EndReason;
		}

		foreach( Player player in game.PlayOrder )
		{
			List<Score> scoreList = new();
			List<Hand> reverse = player.Completed.OrderByDescending( h => h.Count ).ToList();
			foreach( Hand hand in reverse )
			{
				Score score = new()
				{
					Number = hand.Count,
					Protected = hand.Protected,
					UnProtected = hand.UnProtected,
					Skimmed = hand.Skimmed,
					HighestPeddle = -hand.HighestPeddle,
					ParanoiaFines = hand.ParanoiaFines,
					NetProfit = hand.NetScore,
					Bonus = hand.Bonus
				};
				scoreList.Add( score );
			}
			rtn.Players.Add( player.Name, scoreList );
		}

		return rtn;
	}
}