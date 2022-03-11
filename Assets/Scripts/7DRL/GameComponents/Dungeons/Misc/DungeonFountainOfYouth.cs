using System.Collections.Generic;
using _7DRL.GameComponents.Interactions;
using UnityEngine;
using Utils.Libraries;

namespace _7DRL.GameComponents.Dungeons.Misc {
	public class DungeonFountainOfYouth : IDungeonMisc {
		public Vector2Int                     dungeonPosition     { get; set; }
		public IDungeonMisc.Type              type                => IDungeonMisc.Type.FountainOfYouth;
		public InteractionOption              fountainInteraction { get; }
		public InteractionOption              skipInteraction     { get; }
		public string                         interactionName     => "Portal";
		public Sprite                         tokenSprite         => Sprites.Of("token.fountainOfYouth");
		public string                         interactionText     => "You found a Fountain of Youth. Drink from it to get all your health points. What do you want to do?";
		public IEnumerable<InteractionOption> interactionOptions  => new[] { fountainInteraction, skipInteraction };

		public DungeonFountainOfYouth(Vector2Int position, InteractionOption fountainInteraction, InteractionOption skipInteraction) {
			dungeonPosition = position;
			this.fountainInteraction = fountainInteraction;
			this.skipInteraction = skipInteraction;
		}
	}
}