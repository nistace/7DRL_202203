using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _7DRL.Data;
using _7DRL.MiscConstants;
using _7DRL.TextInput;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace _7DRL.Games {
	public static class GameFactory {
		public static IEnumerator CreateGame(UnityAction<Game> callback) {
			var letterPowers = GenerateDefaultLetterPowers();
			yield return null;
			var playerInitialCommands = Memory.commandTypes.Select(type => Memory.commands.Where(command => command.type == type).Random()).ToArray();
			yield return null;
			var playerInitialLetters = GeneratePlayerInitialLetters(letterPowers, playerInitialCommands);
			yield return null;
			var map = GenerateMap(RlConstants.Dungeon.width, RlConstants.Dungeon.height, letterPowers);
			yield return null;
			callback?.Invoke(new Game(letterPowers, playerInitialCommands, playerInitialLetters, map));
		}

		private static Dictionary<char, int> GeneratePlayerInitialLetters(Dictionary<char, int> letterPowers, IEnumerable<Command> playerInitialCommands) {
			var playerInitialLetters = (1 + 'Z' - 'A').CreateArray(t => (char)('A' + t)).ToDictionary(t => t, t => 0);
			foreach (var letterPower in letterPowers) {
				playerInitialLetters[letterPower.Key] += Mathf.CeilToInt((float)RlConstants.Player.initialLettersPerPower / letterPower.Value);
			}
			foreach (var knownCommand in playerInitialCommands) {
				foreach (var letter in knownCommand.inputName) {
					playerInitialLetters[letter] += RlConstants.Player.initialLettersPerKnownCommands;
				}
			}
			return playerInitialLetters;
		}

		private static Dictionary<char, int> GenerateDefaultLetterPowers() {
			var allInputNames = Memory.commands.Select(t => t.inputName).Union(Memory.foeTypes.Select(t => t.inputName)).ToArray();
			var totalLetters = allInputNames.Sum(t => t.Length);
			var letterPowers = new Dictionary<char, int>();
			for (var letter = 'A'; letter <= 'Z'; letter++) {
				letterPowers.Add(letter, totalLetters / allInputNames.Sum(t => t.Count(u => u == letter)));
			}
			return letterPowers;
		}

		private static Encounter GenerateEncounter(Vector2Int position, float distanceRatio, IReadOnlyDictionary<char, int> letterPowers) {
			var level = (Encounter.Level)(Mathf.Clamp01(Mathf.Clamp01(distanceRatio) - .01f) * EnumUtils.SizeOf<Encounter.Level>() - 1);

			var countFoes = Mathf.Clamp(Random.Range((int)level, (int)level + 3), 1, 3);

			var minSeed = Mathf.Max(0, (int)level * 3 - countFoes) * 100;
			var maxSeed = minSeed + 150;

			return GenerateEncounter(position, level, countFoes.CreateArray(t => Random.Range(minSeed, maxSeed)), letterPowers);
		}

		public static Encounter GenerateEncounter(Vector2Int position, Encounter.Level level, IEnumerable<int> foeSeeds, IReadOnlyDictionary<char, int> letterPowers) =>
			new Encounter(level, position, foeSeeds.Select(t => GenerateFoe(t, letterPowers)).ToArray());

		private static Foe GenerateFoe(int seed, IReadOnlyDictionary<char, int> letterPowers) {
			seed %= 1000;
			var type = Memory.foeTypes[seed % Memory.foeTypes.Count];
			var level = 1 + seed / 100;
			var commands = new HashSet<Command>();
			if (TryGenerateFoeCommands(seed + RlConstants.magicNumber, 1 + Mathf.FloorToInt(level / 2f), Memory.commandsPerType[Memory.CommandTypes.attack], out var attackCommands))
				commands.AddAll(attackCommands);
			if (TryGenerateFoeCommands(seed + RlConstants.magicNumber, Mathf.FloorToInt(level / 2f), Memory.commandsPerType[Memory.CommandTypes.defense], out var defenseCommands))
				commands.AddAll(defenseCommands);
			if (TryGenerateFoeCommands(seed + RlConstants.magicNumber, Mathf.FloorToInt(level / 3f), Memory.commandsPerType[Memory.CommandTypes.dodge], out var dodgeCommands))
				commands.AddAll(dodgeCommands);
			if (TryGenerateFoeCommands(seed + RlConstants.magicNumber, Mathf.FloorToInt(level / 4f), Memory.commandsPerType[Memory.CommandTypes.heal], out var healCommands))
				commands.AddAll(healCommands);
			return new Foe(type, level, RlConstants.Foes.initialMaxHealthCoefficient * TextUtils.GetInputValue(type.inputName, letterPowers) * level, 1, commands);
		}

		private static bool TryGenerateFoeCommands(int seed, int count, IReadOnlyList<Command> fromList, out ISet<Command> commands) {
			commands = null;
			if (count == 0) return false;
			if (fromList == null) return false;
			if (fromList.Count == 0) return false;
			commands = new HashSet<Command>();
			var nextAttackSeed = seed;
			for (var i = 0; i < count; ++i) {
				commands.Add(fromList[nextAttackSeed % fromList.Count]);
				nextAttackSeed += RlConstants.magicNumber;
			}
			return true;
		}

		private static DungeonMap GenerateMap(int width, int height, Dictionary<char, int> letterPowers) {
			var gridDirections = new DungeonMap.Direction[width, height].FilledWith(DungeonMap.noDirection);

			GenerateDungeonPaths(gridDirections, 0, 0);
			GenerateMoreRandomPaths(gridDirections);
			var rooms = GenerateDungeonRooms(gridDirections);
			var bossCell = rooms.FirstWhereMaxOrDefault(t => Vector2.SqrMagnitude(RlConstants.Dungeon.playerStartPosition - t));
			var miscRoomContents = GenerateMiscRoomContents(rooms.Except(new[] { bossCell, new Vector2Int(0, 0) }), letterPowers);
			var encounters = GenerateEncounters(gridDirections, rooms, bossCell, letterPowers);
			return new DungeonMap(gridDirections, rooms, miscRoomContents, encounters);
		}

		private static IReadOnlyDictionary<Vector2Int, Encounter> GenerateEncounters(DungeonMap.Direction[,] gridDirections, IReadOnlyCollection<Vector2Int> rooms, Vector2Int bossCell,
			IReadOnlyDictionary<char, int> letterPowers) {
			var result = new Dictionary<Vector2Int, Encounter> {
				{ bossCell, GenerateEncounter(bossCell, Encounter.Level.Boss, new[] { Random.Range(700, 800), Random.Range(800, 900), Random.Range(900, 1000) }, letterPowers) }
			};

			var maxDistance = Vector2.Distance(bossCell, RlConstants.Dungeon.playerStartPosition);
			while (result.Count < RlConstants.Dungeon.encounters) {
				var position = new Vector2Int(Random.Range(0, gridDirections.GetLength(0)), Random.Range(0, gridDirections.GetLength(1)));
				if (result.ContainsKey(position)) continue;
				if (rooms.Contains(position)) continue;

				result.Add(position, GenerateEncounter(position, Vector2.Distance(RlConstants.Dungeon.playerStartPosition, position) / maxDistance, letterPowers));
			}

			return result;
		}

		private static Dictionary<Vector2Int, IDungeonMisc> GenerateMiscRoomContents(IEnumerable<Vector2Int> rooms, Dictionary<char, int> letterPowers) {
			var result = new Dictionary<Vector2Int, IDungeonMisc>();

			var remainingRooms = new Stack<Vector2Int>(rooms.ToArray().Shuffled().ToArray());

			if (remainingRooms.Count > 2) {
				var portal0 = remainingRooms.Pop();
				var portal1 = remainingRooms.Pop();
				result.Add(portal0, new DungeonPortal(portal0, portal1));
				result.Add(portal1, new DungeonPortal(portal1, portal0));
			}

			while (remainingRooms.Count > 0) {
				var position = remainingRooms.Pop();
				result.Add(position, GenerateChest(letterPowers, position));
			}
			return result;
		}

		private static DungeonChest GenerateChest(Dictionary<char, int> letterPowers, Vector2Int position) {
			var result = new LetterReserve();
			var score = RlConstants.Dungeon.chestBountyScore * Vector2.Distance(position, RlConstants.Dungeon.playerStartPosition);
			while (score > 0) {
				var bountyItem = (char)Random.Range('A', 'Z' + 1);
				result.Add(bountyItem);
				score -= letterPowers[bountyItem];
			}
			return new DungeonChest(position, result);
		}

		private static void GenerateMoreRandomPaths(DungeonMap.Direction[,] gridDirections) {
			for (var count = 0; count < RlConstants.Dungeon.additionalRandomPaths; ++count) {
				var x = Random.Range(0, gridDirections.GetLength(0));
				var y = Random.Range(0, gridDirections.GetLength(1));
				var direction = (DungeonMap.Direction)(1 << Random.Range(0, EnumUtils.SizeOf<DungeonMap.Direction>()));

				var otherX = x + DungeonMap.directionToV2[direction].x;
				var otherY = y + DungeonMap.directionToV2[direction].y;
				if (otherX >= 0 && otherX < gridDirections.GetLength(0) && otherY >= 0 && otherY < gridDirections.GetLength(1)) {
					if (gridDirections[x, y].HasFlag(direction)) count--;
					else {
						gridDirections[x, y] |= direction;
						gridDirections[otherX, otherY] |= DungeonMap.oppositeDirections[direction];
					}
				}
			}
		}

		private static IReadOnlyCollection<Vector2Int> GenerateDungeonRooms(DungeonMap.Direction[,] gridDirections) {
			var rooms = new HashSet<Vector2Int> { RlConstants.Dungeon.playerStartPosition };

			for (var x = 0; x < gridDirections.GetLength(0); x++)
			for (var y = 0; y < gridDirections.GetLength(1); y++) {
				var direction = gridDirections[x, y];
				if (direction != 0 && ((direction - 1) & direction) == 0) rooms.Add(new Vector2Int(x, y));
			}

			while (rooms.Count < RlConstants.Dungeon.minRoomCount) {
				rooms.Add(new Vector2Int(Random.Range(0, gridDirections.GetLength(0)), Random.Range(0, gridDirections.GetLength(1))));
			}

			return rooms;
		}

		private static void GenerateDungeonPaths(DungeonMap.Direction[,] griDirections, int x, int y) {
			var possibilities = new HashSet<DungeonMap.Direction>();

			foreach (var direction in EnumUtils.Values<DungeonMap.Direction>()) {
				var otherX = x + DungeonMap.directionToV2[direction].x;
				var otherY = y + DungeonMap.directionToV2[direction].y;
				if (otherX >= 0 && otherX < griDirections.GetLength(0) && otherY >= 0 && otherY < griDirections.GetLength(1)) {
					if (griDirections[otherX, otherY].HasFlag(DungeonMap.oppositeDirections[direction])) griDirections[x, y] |= direction;
					else if (griDirections[otherX, otherY] == DungeonMap.noDirection) possibilities.Add(direction);
				}
			}

			if (possibilities.Count == 0) return;
			var newRandomDirection = possibilities.Random();
			griDirections[x, y] |= newRandomDirection;
			GenerateDungeonPaths(griDirections, x + DungeonMap.directionToV2[newRandomDirection].x, y + DungeonMap.directionToV2[newRandomDirection].y);
			GenerateDungeonPaths(griDirections, x, y);
		}
	}
}