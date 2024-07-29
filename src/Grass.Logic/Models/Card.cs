namespace Grass.Logic.Models;

/// <summary>Provides the details of a playing card.</summary>
[System.Diagnostics.DebuggerDisplay( "{Id}" )]
public class Card
{
	#region Properties

	/// <summary>Card type information.</summary>
	public CardInfo Info { get; private set; }

	/// <summary>Card type.</summary>
	/// <remarks>Used to group playing cards of the same type no matter what the
	/// card identifier is.<br/>
	/// For example, all peddle cards have a type of <c>Peddle</c> that includes
	/// <c>Mexico</c>, <c>Jamaica</c> and <c>Dr.Feelgood</c>, etc.</remarks>
	public string Type => Info.Id.Split( '-' )[0];

	/// <summary>Card type identification.</summary>
	/// <remarks>Used internally to determine how each card is processed. It is unique to
	/// each card type but there can be multiple playing cards with the same Id.</remarks>
	public string Id => Info.Id;

	/// <summary>Card type image filename.</summary>
	public string Image => Id + ".png";

	/// <summary>Indicates whether a peddle card is protected in the players stash.</summary>
	public bool Protected { get; set; }

	/// <summary>Card activity comment.</summary>
	/// <remarks>The latest recorded comment is returned.</remarks>
	public string Comment => History.Count == 0 ? string.Empty : History.Last();

	internal Card( CardInfo info ) => Info = info;

	internal List<string> History { get; private set; } = [];

	#endregion

	#region Methods

	/// <inheritdoc/>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public override string ToString()
	{
		return Info.Value == 0 ? Id : $"{Id} {Info.Value:###,##0}".Trim();
	}

	internal void AddComment( string text )
	{
		if( text.Length > 0 ) { History.Add( text ); }
	}

	internal static bool TransferCard( List<Card> from, List<Card> to, Card card )
	{
		if( !from.Contains( card ) ) {  return false; }
		if( to.Contains( card ) ) { return false; }
		from.Remove( card );
		to.Add( card );
		return true;
	}

	internal static List<Card> GetProtected( List<Card> list )
	{
		return list.Where( c => c.Id.StartsWith( CardInfo.cPeddle ) && c.Protected ).ToList();
	}

	internal static List<Card> GetUnprotected( List<Card> list )
	{
		return list.Where( c => c.Id.StartsWith( CardInfo.cPeddle ) && !c.Protected ).ToList();
	}

	internal static Card? GetLowPeddle( List<Card> list, bool protect = false )
	{
		List<Card> cards = protect ? GetProtected( list ) : GetUnprotected( list );
		return cards.OrderBy( c => c.Info.Value ).FirstOrDefault();
	}

	internal static Card? GetHighPeddle( List<Card> list, bool protect = false )
	{
		List<Card> cards = protect ? GetProtected( list ) : GetUnprotected( list );
		return cards.OrderByDescending( c => c.Info.Value ).FirstOrDefault();
	}

	#endregion
}