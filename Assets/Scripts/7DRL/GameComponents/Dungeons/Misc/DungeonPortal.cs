using System.Collections.Generic;
using _7DRL.GameComponents.Interactions;
using UnityEngine;
using Utils.Libraries;

namespace _7DRL.GameComponents.Dungeons.Misc {
	public class DungeonPortal : IDungeonMisc {
		public Vector2Int                             dungeonPosition    { get; set; }
		public IDungeonMisc.Type                      type               => IDungeonMisc.Type.Portal;
		public InteractionOption                      portalInteraction  { get; }
		public Vector2Int                             portalDestination  { get; }
		public InteractionOption                      skipInteraction    { get; }
		public string                                 interactionName    => "Portal";
		public Sprite                                 tokenSprite        => Sprites.Of("token.portal");
		public string                                 interactionText    => "You are standing in front of a portal. You assume it will lead you somewhere else in the dungeon. What do you want to do?";
		public IReadOnlyCollection<InteractionOption> interactionOptions => new[] { portalInteraction, skipInteraction };

		public DungeonPortal(Vector2Int position, (InteractionOption interaction, Vector2Int destination) portalInteraction, InteractionOption skipInteraction) {
			dungeonPosition = position;
			this.portalInteraction = portalInteraction.interaction;
			portalDestination = portalInteraction.destination;
			this.skipInteraction = skipInteraction;
		}
	}
}