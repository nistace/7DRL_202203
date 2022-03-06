using UnityEngine;

namespace _7DRL.Data {
	public class DungeonPortal : IDungeonMisc {
		public Vector2Int        destination     { get; }
		public Vector2Int        dungeonPosition { get; set; }
		public IDungeonMisc.Type type            => IDungeonMisc.Type.Portal;

		public DungeonPortal(Vector2Int position, Vector2Int destination) {
			dungeonPosition = position;
			this.destination = destination;
		}
	}
}