namespace Grass.Logic;

/// <summary>Represents a container for the results of a play request.</summary>
public class PlayResult
{
	#region Member Fields

	/// <summary>Represents the success of the play request.</summary>
	/// <remarks>The <c>null</c> value is used to indicate success. Consumers of
	/// <see cref="PlayResult"/>s should compare the values to
	/// <see cref="Success" /> rather than checking for <c>null</c>.</remarks>
	public static readonly PlayResult? Success;

	#endregion

	#region Constructors

	/// <summary>Initializes a new instance of the <see cref="PlayResult"/> class by using an
	/// error message.</summary>
	/// <param name="errorMessage">The user-visible error message.</param>
	public PlayResult( string? errorMessage ) : this( errorMessage, null )
	{ }

	/// <summary>Initializes a new instance of the <see cref="PlayResult"/> class by using an
	/// error message and a list of members that have validation errors.</summary>
	/// <param name="errorMessage">The user-visible error message.</param>
	/// <param name="memberNames">The list of member names that have play errors.</param>
	public PlayResult( string? errorMessage, IEnumerable<string>? memberNames )
	{
		ErrorMessage = errorMessage;
		MemberNames = memberNames ?? Array.Empty<string>();
	}

	/// <summary>Initializes a new instance of the <see cref="PlayResult"/> class by using a
	/// PlayResult object.</summary>
	/// <param name="playResult">The play result.</param>
	/// <exception cref="ArgumentNullException">The <paramref name="playResult" /> is <c>null</c>.</exception>
	protected PlayResult( PlayResult playResult )
	{
		ArgumentNullException.ThrowIfNull( playResult );

		ErrorMessage = playResult.ErrorMessage;
		MemberNames = playResult.MemberNames;
	}

	#endregion

	#region Properties

	/// <summary>Gets the error message for this result.</summary>
	public string? ErrorMessage { get; set; }

	/// <summary>Gets the collection of member names affected by this result.</summary>
	/// <remarks>The collection may be empty but will never be <c>null</c>.</remarks>
	public IEnumerable<string> MemberNames { get; }

	#endregion

	#region Methods

	/// <summary>Returns a string representation of the current play result.</summary>
	/// <returns>The current play result.</returns>
	/// <remarks>If <see cref="ErrorMessage"/> is empty, it still qualifies as specified
	/// and is therefore returned by <see cref="ToString()"/>.</remarks>
	public override string ToString() => ErrorMessage ?? base.ToString()!;

	#endregion Methods
}