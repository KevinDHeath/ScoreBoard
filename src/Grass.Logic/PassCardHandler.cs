using System.ComponentModel;
using Grass.Logic.Models;
namespace Grass.Logic;

/// <summary>Class providing the ability to pass cards from one player to the next.</summary>
public abstract class PassCardHandler : IDisposable
{
	/// <summary>Default event handler for paranoia played.</summary>
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
					// Assume the paranoia card is already played
					Card worst = Decision.GetWorstCard( player.Current.Cards );
					game.AddCardToPass( player, worst );
				}
			}
		}
	}

	/// <inheritdoc/>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public virtual void Dispose() => GC.SuppressFinalize( this );
}