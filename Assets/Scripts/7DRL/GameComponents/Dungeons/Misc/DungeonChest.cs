using System.Collections.Generic;
using UnityEngine;
using Utils.Libraries;

namespace _7DRL.GameComponents.Dungeons.Misc {
	public class DungeonChest : IDungeonMisc {
		public Vector2Int                     dungeonPosition    { get; set; }
		public IDungeonMisc.Type              type               => IDungeonMisc.Type.Chest;
		public InteractionOption              chestInteraction   { get; }
		public IEnumerable<string>            chestBounty        { get; }
		public InteractionOption              skipInteraction    { get; }
		public Sprite                         tokenSprite        => Sprites.Of("token.chest");
		public string                         interactionName    => "Chest";
		public string                         interactionText    => "You found a chest that could contain great treasures. What do you want to do with it?";
		public IEnumerable<InteractionOption> interactionOptions => new[] { chestInteraction, skipInteraction };

		public DungeonChest(Vector2Int position, (InteractionOption interaction, IEnumerable<string> bounty) chestInteraction, InteractionOption skipInteraction) {
			dungeonPosition = position;
			(this.chestInteraction, chestBounty) = chestInteraction;
			this.skipInteraction = skipInteraction;
		}
	}
}