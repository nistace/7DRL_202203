using System;
using UnityEngine;

namespace _7DRL.Data {
	[Serializable]
	public class DungeonSprites {
		[SerializeField] protected Sprite[] _lanes;
		[SerializeField] protected Sprite[] _rooms;
		[SerializeField] protected Sprite[] _foes;
		[SerializeField] protected Sprite   _player;
		[SerializeField] protected Sprite   _chest;
		[SerializeField] protected Sprite   _portal;

		public Sprite player => _player;
		public Sprite chest  => _chest;
		public Sprite portal => _portal;

		public Sprite GetLane(DungeonMap.Direction direction) => _lanes[(int)direction];
		public Sprite GetRoom(DungeonMap.Direction direction) => _rooms[(int)direction];
		public Sprite GetFoe(Encounter.Level level) => _foes[(int)level];

		public Sprite GetMisc(IDungeonMisc.Type type) {
			switch (type) {
				case IDungeonMisc.Type.Chest: return chest;
				case IDungeonMisc.Type.Portal: return portal;
				default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
	}
}