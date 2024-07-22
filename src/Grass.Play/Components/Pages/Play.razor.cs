using Grass.Logic.Models;
using Microsoft.AspNetCore.Components;

namespace Grass.Play.Components.Pages;

public partial class Play
{
	[Parameter]
	public int Id { get; set; }

	private readonly CollapseExpand inHand = new();
	private readonly CollapseExpand stash = new();
	private readonly CollapseExpand score = new();

	private readonly HideShow active = new( "in Hand" );
	private readonly HideShow hands = new( hide: false );

	protected override void OnInitialized()
	{
		if( Id > 0 )
		{
			Player player = Service.Current.PlayOrder[Id - 1];
			var hand = player.Current;
			// player needs to be able to provide the Cards in hand
		}
	}
}