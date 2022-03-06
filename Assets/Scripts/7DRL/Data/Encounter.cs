using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace _7DRL.Data {
	[Serializable]
	public class Encounter : IDungeonCrawler {
		public class Event : UnityEvent<Encounter> { }

		public enum Level {
			Weak   = 0,
			Normal = 1,
			Strong = 2,
			Boss   = 3
		}

		[SerializeField] protected Foe[]      _foes;
		[SerializeField] protected Vector2Int _dungeonPosition;
		[SerializeField] protected Level      _level;

		public Foe[] foes  => _foes;
		public Level level => _level;

		public Vector2Int dungeonPosition {
			get => _dungeonPosition;
			set => _dungeonPosition = value;
		}

		public Encounter(Level level, Vector2Int dungeonPosition, IEnumerable<Foe> foes) {
			_foes = foes.ToArray();
			_level = level;
			_dungeonPosition = dungeonPosition;
		}
	}
}