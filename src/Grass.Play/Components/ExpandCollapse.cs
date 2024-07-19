namespace Grass.Play.Components;

internal class ExpandCollapse
{
	bool? IsOpen = null;

	internal string IconClass
	{
		get
		{
			if( IsOpen is null ) { IsOpen = true; return string.Empty; }
			return ( IsOpen.Value == false ) ? "toggleCollapsed" : string.Empty;
		}
	}

	internal string TableStyle => ( IsOpen is not null && IsOpen.Value == false )
		? "display: none;" : string.Empty;

	internal void Toggle()
	{
		if( IsOpen is not null ) { IsOpen = !IsOpen.Value; }
	}
}