namespace Grass.Logic;

/// <summary>Grass logic extensions.</summary>
[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
public static class Extensions
{
	/// <summary>Get the ordinal value of positive integers.</summary>
	/// <remarks>Only works for English-based cultures.
	/// Code from: http://stackoverflow.com/questions/20156/is-there-a-quick-way-to-create-ordinals-in-c/31066#31066
	/// With help: http://www.wisegeek.com/what-is-an-ordinal-number.htm</remarks>
	/// <param name="number">The number.</param>
	/// <returns>Ordinal value of positive integers, or <see cref="int.ToString()"/> if less than 1.</returns>
	public static string DisplayWithSuffix( this int number )
	{
		string rtn = number.ToString();
		if( number < 1 ) { return rtn; }
		const string cTH = "th";

		if( rtn.EndsWith( "11" ) || rtn.EndsWith( "12" ) || rtn.EndsWith( "13" ) ) { return rtn + cTH; }
		if( rtn.EndsWith( '1' ) ) { return rtn + "st"; }
		if( rtn.EndsWith( '2' ) ) { return rtn + @"nd"; }
		if( rtn.EndsWith( '3' ) ) { return rtn + "rd"; }
		return rtn + cTH;
	}
}