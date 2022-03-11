using System.Collections.Generic;
using _7DRL.GameComponents.Interactions;
using _7DRL.GameComponents.TextAndLetters;
using UnityEngine;
using Utils.Extensions;
using Utils.Libraries;

namespace _7DRL.GameComponents.Dungeons.Misc {
	public class DungeonStoneTableOfKnowledge : IDungeonMisc {
		private InteractionOption              readInteraction    { get; }
		public  string                         readBookName       { get; }
		private InteractionOption              skillInteraction   { get; }
		public  Command                        skillCommand       { get; }
		private InteractionOption              powerInteraction   { get; }
		public  char                           powerLetter        { get; }
		private InteractionOption              skipInteraction    { get; }
		public  Sprite                         tokenSprite        => Sprites.Of("token.stoneTabletOfKnowledge");
		public  string                         interactionName    => "Stone Tablet of Knowledge";
		public  string                         interactionText    => "You found a Stone Tablet of Knowledge. Will you spend some time to unravel its secrets?";
		public  IEnumerable<InteractionOption> interactionOptions { get; }
		public  Vector2Int                     dungeonPosition    { get; set; }
		public  IDungeonMisc.Type              type               => IDungeonMisc.Type.StoneTabletOfKnowledge;

		public DungeonStoneTableOfKnowledge(Vector2Int position, (InteractionOption interaction, string bookName) readInteraction, (InteractionOption interaction, Command command) skillInteraction,
			(InteractionOption interaction, char letter) powerInteraction, InteractionOption skipInteraction) {
			dungeonPosition = position;
			(this.readInteraction, readBookName) = readInteraction;
			(this.skillInteraction, skillCommand) = skillInteraction;
			(this.powerInteraction, powerLetter) = powerInteraction;
			this.skipInteraction = skipInteraction;
			interactionOptions = new List<InteractionOption> { this.readInteraction, this.skillInteraction, this.powerInteraction, this.skipInteraction }.NotNull();
		}
	}
}