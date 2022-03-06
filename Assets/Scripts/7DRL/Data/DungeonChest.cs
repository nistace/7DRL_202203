using UnityEngine;

namespace _7DRL.Data {
	public class DungeonChest : IDungeonMisc {
		public LetterReserve     bounty          { get; }
		public Vector2Int        dungeonPosition { get; set; }
		public IDungeonMisc.Type type            => IDungeonMisc.Type.Chest;

		public DungeonChest(Vector2Int position, LetterReserve bounty) {
			dungeonPosition = position;
			this.bounty = bounty;
		}
	}
}