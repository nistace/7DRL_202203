using System;
using System.Collections.Generic;
using System.Linq;
using _7DRL.GameComponents.Characters;
using _7DRL.GameComponents.Dungeons.Misc;
using UnityEngine;
using Utils.Extensions;

namespace _7DRL.GameComponents.Dungeons {
	[Serializable]
	public class DungeonMap {
		protected Dictionary<Vector2Int, IDungeonMisc> _miscRoomContents;
		protected Dictionary<Vector2Int, Encounter>    _encounters;

		[Flags]
		public enum Direction {
			North = 1 << 0,
			East  = 1 << 1,
			South = 1 << 2,
			West  = 1 << 3
		}

		public const Direction noDirection   = 0;
		public const Direction allDirections = Direction.North | Direction.West | Direction.South | Direction.East;

		public static IReadOnlyDictionary<Direction, Vector2Int> directionToV2 { get; } = new Dictionary<Direction, Vector2Int> {
			{ Direction.North, Vector2Int.up }, { Direction.West, Vector2Int.left }, { Direction.South, Vector2Int.down }, { Direction.East, Vector2Int.right }
		};

		public static IReadOnlyDictionary<Direction, Direction> oppositeDirections { get; } = new Dictionary<Direction, Direction> {
			{ Direction.North, Direction.South }, { Direction.West, Direction.East }, { Direction.South, Direction.North }, { Direction.East, Direction.West }
		};

		private Direction[,] grid   { get; set; }
		public  int          width  => grid.GetLength(0);
		public  int          height => grid.GetLength(1);
		public Direction this[int x, int y] => grid[x, y];

		public IReadOnlyList<Vector2Int>         rooms            { get; }
		public IReadOnlyCollection<IDungeonMisc> miscRoomContents => _miscRoomContents.Values;
		public IReadOnlyCollection<Encounter>    encounters       => _encounters.Values;

		public DungeonMap(Direction[,] grid, IEnumerable<Vector2Int> rooms, Dictionary<Vector2Int, IDungeonMisc> miscRoomContents, IReadOnlyDictionary<Vector2Int, Encounter> encounters) {
			this.grid = grid;
			this.rooms = new List<Vector2Int>(rooms);
			_miscRoomContents = miscRoomContents.ToDictionary(t => t.Key, t => t.Value);
			_encounters = encounters.ToDictionary(t => t.Key, t => t.Value);
		}

		public bool IsMovementAllowed(Vector2Int from, Direction direction, bool allowCollisionWithEncounter) {
			if (!this[from.x, from.y].HasFlag(direction)) return false;
			if (!allowCollisionWithEncounter && _encounters.ContainsKey(from + directionToV2[direction])) return false;
			return true;
		}

		public bool IsRoom(Vector2Int coordinates) => rooms.Contains(coordinates);
		public bool IsRoom(int x, int y) => rooms.Contains(new Vector2Int(x, y));
		public bool TryGetMisc(Vector2Int position, out IDungeonMisc misc) => (misc = _miscRoomContents.Of(position)) != null;
		public bool TryGetEncounter(Vector2Int position, out Encounter encounter) => (encounter = _encounters.Of(position)) != null;

		public IEnumerable<Direction> GetPossibleMovements(Vector2Int from, bool allowCollisionWithEncounter) =>
			EnumUtils.Values<Direction>().Where(t => IsMovementAllowed(from, t, allowCollisionWithEncounter));

		public void MoveEncounter(Encounter encounter, Direction direction) {
			_encounters.Remove(encounter.dungeonPosition);
			encounter.dungeonPosition += directionToV2[direction];
			_encounters.Add(encounter.dungeonPosition, encounter);
		}

		public void RemoveEncounter(Encounter encounter) => _encounters.Remove(encounter.dungeonPosition);
		public void RemoveMisc(IDungeonMisc misc) => _miscRoomContents.Remove(misc.dungeonPosition);
	}
}