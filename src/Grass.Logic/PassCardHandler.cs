using System.ComponentModel;
using Grass.Logic.Models;
namespace Grass.Logic;

/// <summary>Class providing the ability to pass cards from one player to the next.</summary>
public abstract class PassCardHandler : IDisposable
{
	/// <summary>Auto-Play event handler for paranoia played.</summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="e">Property changed event arguments.</param>
	protected virtual void OnParanoiaPlayed( object? sender, PropertyChangedEventArgs e )
	{
		if( sender is Game game && e.PropertyName is not null )
		{
			if( e.PropertyName == nameof( Game.ParanoiaPlayer ) && game.ParanoiaPlayer is not null )
			{
				foreach( Player player in game.Players )
				{
					player.ToDo = Player.Action.Pass;
					// Assume the paranoia card is already played
					Card worst = Actor.GetWorstCard( player.Current.Cards );
					game.AddCardToPass( player, worst );
				}
			}
		}
	}

	/// <summary>Interactive event handler for paranoia played.</summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="e">Property changed event arguments.</param>
	protected virtual void OnParanoiaInteractive( object? sender, PropertyChangedEventArgs e )
	{
		if( sender is Game game && e.PropertyName is not null )
		{
			if( e.PropertyName == nameof( Game.ParanoiaPlayer ) && game.ParanoiaPlayer is not null )
			{
				foreach( Player player in game.Players )
				{
					player.ToDo = Player.Action.Pass;
				}
			}
		}
	}

	/// <inheritdoc/>
	public virtual void Dispose() => GC.SuppressFinalize( this );
}