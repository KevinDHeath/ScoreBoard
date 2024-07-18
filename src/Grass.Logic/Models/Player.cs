namespace Grass.Logic.Models;

/// <summary>Initializes a new instance of the Player class.</summary>
/// <param name="name">Player name.</param>
[System.Diagnostics.DebuggerDisplay( "{Name}" )]
public class Player( string name )
{
	#region Properties

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

	#endregion
}