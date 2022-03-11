using System.Collections.Generic;
using _7DRL.GameComponents.Interactions;
using _7DRL.GameComponents.TextAndLetters;
using UnityEngine;
using Utils.Extensions;
using Utils.Libraries;

namespace _7DRL.GameComponents.Dungeons.Misc {
	public class DungeonStoneTableOfKnowledge : ISkillDungeonMisc, IPowerDungeonMisc, IBookDungeonMisc {
		private InteractionOption              bookInteraction    { get; }
		public  string                         bookName           { get; }
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

		public DungeonStoneTableOfKnowledge(Vector2Int position, (InteractionOption interaction, string bookName) bookInteraction, (InteractionOption interaction, Command command) skillInteraction,
			(InteractionOption interaction, char letter) powerInteraction, InteractionOption skipInteraction) {
			dungeonPosition = position;
			(this.bookInteraction, bookName) = bookInteraction;
			(this.skillInteraction, skillCommand) = skillInteraction;
			(this.powerInteraction, powerLetter) = powerInteraction;
			this.skipInteraction = skipInteraction;
			interactionOptions = new List<InteractionOption> { this.bookInteraction, this.skillInteraction, this.powerInteraction, this.skipInteraction }.NotNull();
		}
	}
}