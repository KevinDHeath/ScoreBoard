namespace Grass.Play.Components;

internal class CollapseExpand
{
	private bool? isOpen = null;

	internal string Class
	{
		get
		{
			if( isOpen is null ) { isOpen = true; return string.Empty; }
			return ( isOpen.Value == false ) ? "toggleCollapsed" : string.Empty;
		}
	}

	internal string Style => ( isOpen is not null && isOpen.Value == false )
		? "display: none;" : string.Empty;

	internal void Toggle()
	{
		if( isOpen is not null ) { isOpen = !isOpen.Value; }
	}
}