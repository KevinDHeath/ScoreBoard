namespace Grass.Logic.Models;

/// <summary>Playing card type information.</summary>
[System.Diagnostics.DebuggerDisplay( "{Id}" )]
public class CardInfo
{
	[System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
	private static readonly List<CardInfo> sCards = Info();

	[System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
	private static readonly Random random = new();

	#region Properties

	/// <summary>User friendly card name.</summary>
	public string Caption { get; private set; } = string.Empty;

	/// <summary>The number of playing cards of this type.</summary>
	public int Count { get; private set; }

	/// <summary>Card type value in the game.</summary>
	/// <remarks>The value depends on the type of card. Peddle cards have positive values ranging
	/// from $5,000 to $100,000. Paranoia cards have a negative value that give penalties to players.
	/// Both of these types are used to determine the scores.<br/>
	/// Protection cards allow a player to protect their peddle cards against skim cards and
	/// range from $5,000 to $50,000.</remarks>
	public int Value { get; private set; }

	internal string Id { get; private set; }

	#endregion

	private CardInfo( string id ) { Id = id; }

	/// <inheritdoc/>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public override string ToString()
	{
		return Value == 0 ? Caption : $"{Value:$###,##0} {Id.Split( '-' )[0]}".Trim();
	}

	#region Internal Constants

	internal const string cPeddle = "Peddle";
	internal const string cProtection = "Protection";
	internal const string cHeatOn = "HeatOn";
	internal const string cHeatOff = "HeatOff";
	internal const string cNirvana = "Nirvana";
	internal const string cParanoia = "Paranoia";
	internal const string cSkim = "Skim";

	internal const string cOpen = "Market-Open";
	internal const string cClose = "Market-Close";
	internal const string cMexico = cPeddle + "-Mexico";
	internal const string cHomegrown = cPeddle + "-Homegrown";
	internal const string cColumbia = cPeddle + "-Columbia";
	internal const string cJamaica = cPeddle + "-Jamaica";
	internal const string cPanama = cPeddle + "-Panama";
	internal const string cDrFeelgood = cPeddle + "-Dr.Feelgood";
	internal const string cCatchaBuzz = cProtection + @"-CatchaBuzz";
	internal const string cGrabaSnack = cProtection + @"-GrabaSnack";
	internal const string cLustConquers = cProtection + @"-LustConquersAll";
	internal const string cOnBust = cHeatOn + "-Bust";
	internal const string cOnDetained = cHeatOn + "-Detained";
	internal const string cOnFelony = cHeatOn + "-Felony";
	internal const string cOnSearch = cHeatOn + @"-SearchAndSeizure";
	internal const string cOffBust = cHeatOff + "-Bust";
	internal const string cOffDetained = cHeatOff + "-Detained";
	internal const string cOffFelony = cHeatOff + "-Felony";
	internal const string cOffSearch = cHeatOff + @"-SearchAndSeizure";
	internal const string cPayFine = cHeatOff + @"-Payfine";
	internal const string cStonehigh = cNirvana + @"-Stonehigh";
	internal const string cEuphoria = cNirvana + "-Euphoria";
	internal const string cSoldout = cParanoia + @"-Soldout";
	internal const string cDoublecross = cParanoia + @"-Doublecrossed";
	internal const string cWipedOut = cParanoia + @"-UtterlyWipedOut";
	internal const string cSteal = cSkim + @"-StealYourNeighborsPot";
	internal const string cBanker = cSkim + "-TheBanker";

	#endregion

	#region Internal Methods

	internal static CardInfo GetCardInfo( string cardId )
		=> sCards.FirstOrDefault( c => c.Id == cardId ) ?? sCards[0];

	internal static List<Card> BuildStack()
	{
		List<Card> rtn = new();
		foreach( CardInfo info in sCards )
		{
			for( int count = 0; count < info.Count; count++ ) { rtn.Add( new Card( info ) ); }
		}

		Shuffle( rtn, 600 );
		return rtn;
	}

	internal static CardInfo? GetHeatOffInfo( List<Card> list )
	{
		Card? heat = list.LastOrDefault();
		if( heat is not null && heat.Type == cHeatOn )
		{
			return GetCardInfo( heat.Id.Replace( cHeatOn, cHeatOff ) );
		}
		return null;
	}

	internal static List<CardInfo> Info()
	{
		return new List<CardInfo>()
		{
			{ new CardInfo( cOpen ) { Count = 10, Caption = "Market Open" } },
			{ new CardInfo( cClose) { Count = 5, Caption = "Market Close" } },
			{ new CardInfo( cMexico ) { Count = 6, Value = 5000, Caption = "Mexico" } },
			{ new CardInfo( cHomegrown ) { Count = 6, Value = 5000, Caption = "Home grown" } },
			{ new CardInfo( cColumbia ) { Count = 5, Value = 25000, Caption = "Columbia" } },
			{ new CardInfo( cJamaica ) { Count = 5, Value = 25000, Caption = "Jamaica" } },
			{ new CardInfo( cPanama ) { Count = 5, Value = 50000, Caption = "Panama" } },
			{ new CardInfo( cDrFeelgood ) { Count = 1, Value = 100000, Caption = @"Dr. Feelgood" } },
			{ new CardInfo( cCatchaBuzz ) { Count = 2, Value = 25000, Caption = "Catch a buzz" } },
			{ new CardInfo( cGrabaSnack ) { Count = 2, Value = 25000, Caption = "Grab a snack" } },
			{ new CardInfo( cLustConquers ) { Count = 2, Value = 50000, Caption = "Lust conquers all" } },
			{ new CardInfo( cOnBust ) { Count = 3, Caption = "Heat on Bust" } },
			{ new CardInfo( cOnDetained ) { Count = 3, Caption = "Heat on Detained" } },
			{ new CardInfo( cOnFelony ) { Count = 3, Caption = "Heat on Felony" } },
			{ new CardInfo( cOnSearch ) { Count = 3, Caption = "Heat on Search and seizure" } },
			{ new CardInfo( cOffBust ) { Count = 5, Caption = "Heat off Bust" } },
			{ new CardInfo( cOffDetained ) { Count = 5, Caption = "Heat off Detained" } },
			{ new CardInfo( cOffFelony ) { Count = 5, Caption = "Heat off Felony" } },
			{ new CardInfo( cOffSearch ) { Count = 5, Caption = "Heat off Search and seizure" } },
			{ new CardInfo( cPayFine ) { Count = 4, Caption = "Heat off Pay fine" } },
			{ new CardInfo( cStonehigh ) { Count = 5, Caption = @"Stonehigh" } },
			{ new CardInfo( cEuphoria ) { Count = 1, Caption = "Euphoria" } },
			{ new CardInfo( cSoldout ) { Count = 4, Value = -25000, Caption = "Sold out" } },
			{ new CardInfo( cDoublecross ) { Count = 3, Value = -50000, Caption = @"Doublecrossed" } },
			{ new CardInfo( cWipedOut ) { Count = 1, Value = -100000, Caption = "Utterly wiped out" } },
			{ new CardInfo( cSteal ) { Count = 4, Caption = "Steal your neighbor's pot" } },
			{ new CardInfo( cBanker ) { Count = 1, Caption = "The Banker" } }
		};
	}

	#region Shuffles
	// https://bost.ocks.org/mike/shuffle/
	internal static void Shuffle( List<Card> stack )
	{
		for( int n = stack.Count - 1; n > 0; --n )
		{
			int k = random.Next( n + 1 );
			(stack[k], stack[n]) = (stack[n], stack[k]);
		}
	}

	// https://stackoverflow.com/questions/1150646/card-shuffling-in-c-sharp
	internal static void Shuffle( List<Card> stack, int timesToShuffle = 600 )
	{
		// Based on Java code from Wikipedia
		Card temp;
		int cardToShuffle1, cardToShuffle2;

		for( int x = 0; x < timesToShuffle; x++ )
		{
			cardToShuffle1 = random.Next( stack.Count );
			cardToShuffle2 = random.Next( stack.Count );
			temp = stack[cardToShuffle1];

			stack[cardToShuffle1] = stack[cardToShuffle2];
			stack[cardToShuffle2] = temp;
		}
	}

	internal static void Shuffle1( List<Card> stack, int timesToShuffle = 600 )
	{
		// Takes a portion of the stack, then put them in random places
		List<Card> temp;
		int picked, backPortion; // the last used random numbers

		for( int x = 0; x < timesToShuffle; x++ )
		{
			picked = random.Next( 10, 30 ); // number of cards we pick out
			temp = stack.GetRange( 0, picked );
			stack.RemoveRange( 0, picked );

			while( temp.Count != 0 )
			{
				picked = random.Next( 10, stack.Count - 1 ); // where we place a range of cards
				backPortion = random.Next( 1, ( temp.Count / 3 ) + 1 ); // the number of cards we but back in one step
				stack.InsertRange( picked, temp.GetRange( 0, backPortion ) ); // inserting a range of cards
				temp.RemoveRange( 0, backPortion ); // we remove what we just placed back
			}
		}
	}
	#endregion

	#endregion
}