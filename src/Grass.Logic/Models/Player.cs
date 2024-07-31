namespace Grass.Logic.Models;

/// <summary>Initializes a new instance of the Player class.</summary>
/// <param name="name">Player name.</param>
/// <param name="id">Player Id.</param>
[System.Diagnostics.DebuggerDisplay( "{Name}" )]
public class Player( string name, int id = 0 )
{
	internal enum Action
	{
		Nothing = 0,
		Play = 1,
		Pass = 2,
		Trade = 3
	}

	internal Action ToDo { get; set; }

	/// <summary>Indicates whether the player needs to play a card in hand.</summary>
	public bool Play => ToDo == Action.Play;

	/// <summary>Indicates whether the player needs to pass a card in hand.</summary>
	public bool Pass => ToDo == Action.Pass;

	#region Properties

	/// <summary>Identifier of the player.</summary>
	public int Id { get; set; } = id;

	/// <summary>Name of the player.</summary>
	public string Name { get; set; } = name;

	/// <summary>List of hands completed for the player.</summary>
	public List<Hand> Completed { get; private set; } = [];

	/// <summary>Current hand for the player.</summary>
	public Hand Current
	{
		get
		{
			_currentHand ??= new Hand { Player = Name, Count = Completed.Count + 1 };
			return _currentHand;
		}
	}
	private Hand? _currentHand;
	internal void ResetCurrent() => _currentHand = null;

	/// <summary>Current score for the game.</summary>
	public int Total { get; internal set; }

	#endregion

	#region Methods

	/// <inheritdoc/>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public override string ToString() => $"{Name} game total {Total:$###,##0}";

	internal void Reset()
	{
		Total = 0;
		ResetCurrent();
		Completed.Clear();
		ToDo = Action.Nothing;
	}

	#endregion
}