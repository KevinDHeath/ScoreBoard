namespace Grass.Play.Components;

internal class HideShow
{
	private bool Hidden => Class == string.Empty;
	private const string cClass = "is-hidden";
	private readonly string _text;

	internal HideShow( string text = "", bool hide = true )
	{
		_text = text;
		if( hide ) { Class = cClass; }
	}

	internal string Class { get; set; } = string.Empty;

	internal string Text => Hidden ? $"Hide {_text}".Trim() : $"Show {_text}".Trim();

	internal void Toggle() => Class = Hidden ? cClass : string.Empty;

	internal void Hide() => Class = cClass;

	internal void Show() => Class = string.Empty;
}
