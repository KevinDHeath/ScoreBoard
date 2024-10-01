using System.ComponentModel;
namespace Grass.Logic.Models;

/// <summary>Initializes a new instance of the Player class.</summary>
/// <param name="name">Player name.</param>
/// <param name="id">Player Id.</param>
[System.Diagnostics.DebuggerDisplay( "{Name}" )]
public class Player( string name, int id = 0 )
{
	private const string cPlay = "Play a card";
	private const string cPass = "Pass a card";
	private const string cMiss = "Miss a turn";

	#region Actions

	internal enum Action
	{
		Nothing = 0,
		Play = 1,
		Pass = 2,
		Trade = 3,
		MissTurn = 4
	}

	internal Action ToDo { get; set; }

	/// <summary>Indicates whether the player needs to play a card in hand.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public bool Play => ToDo == Action.Play;

	/// <summary>Indicates whether the player needs to pass a card in hand.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public bool Pass => ToDo == Action.Pass;

	/// <summary>Indicates whether the player can trade a card in hand.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public bool Trade => ToDo == Action.Trade;

	/// <summary>Notification message for the player.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public string? Notify { get; set; }

	#endregion

	#region Properties

	/// <summary>Identifier of the player.</summary>
	public int Id { get; set; } = id;

	/// <summary>Name of the player.</summary>
	public string Name { get; set; } = name;

	/// <summary>Score of hands completed for the player.</summary>
	public List<Score> Scores { get; private set; } = [];

	/// <summary>Current hand for the player.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public Hand Current
	{
		get
		{
			_currentHand ??= new Hand { Player = Name, Count = Scores.Count + 1 };
			return _currentHand;
		}
	}
	private Hand? _currentHand;

	internal void ResetCurrent() => _currentHand = null;

	/// <summary>Current score for the game.</summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public int Total { get; internal set; }

	#endregion

	#region Methods

	/// <inheritdoc/>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public override string ToString() => $"{Name} game total {Total:$###,##0}";

	internal void AddScore( Game game, Player? last, Hand hand, int bonus = 0 )
	{
		if( bonus == 0 )
		{
			string by = last is not null ? "by " + last.Name : string.Empty;
			string reason = game.StackCount == 0 ? "Stack ran out" : $"Market closed {by}".Trim();

			Score score = new()
			{
				Number = hand.Count,
				Protected = hand.Protected,
				UnProtected = hand.UnProtected,
				Skimmed = hand.Skimmed,
				HighestPeddle = -hand.HighestPeddle,
				ParanoiaFines = hand.ParanoiaFines,
				NetProfit = hand.NetScore,
				Bonus = bonus,
				Reason = reason
			};
			Scores.Add( score );
		}
		if( bonus > 0 )
		{
			Score? score = Scores.Find( s => s.Number == hand.Count );
			if( score is not null ) { score.Bonus = bonus; }
		}
	}

	/// <summary>Set the notification message for the player.</summary>
	/// <returns><c>null</c> if there is no notification message.</returns>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public string? SetNotification()
	{
		if( Notify is not null ) { return Notify; }
		return ToDo switch
		{
			Action.Play => cPlay + $" for round {Current.Round}",
			Action.Pass => cPass,
			Action.MissTurn => cMiss,
			_ => Notify,
		};
	}

	internal void Reset()
	{
		Total = 0;
		ResetCurrent();
		Scores.Clear();
		ToDo = Action.Nothing;
	}

	#endregion
}