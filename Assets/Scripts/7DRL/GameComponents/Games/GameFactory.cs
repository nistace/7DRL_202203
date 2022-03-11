using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _7DRL.GameComponents.Characters;
using _7DRL.GameComponents.Dungeons;
using _7DRL.GameComponents.Dungeons.Misc;
using _7DRL.GameComponents.TextAndLetters;
using _7DRL.MiscConstants;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace _7DRL.Games {
	public static class GameFactory {
		public static IEnumerator CreateGame(UnityAction<Game> callback) {
			var letterPowers = GenerateDefaultLetterPowers();
			yield return null;
			var playerInitialCommands = GeneratePlayerInitialCommands();
			var playerUnknownCommands = new HashSet<Command>(Memory.commands);
			playerUnknownCommands.RemoveAll(playerInitialCommands);
			yield return null;
			var playerInitialLetters = GeneratePlayerInitialLetters(letterPowers, playerInitialCommands);
			yield return null;
			var map = GenerateMap(RlConstants.Dungeon.width, RlConstants.Dungeon.height, letterPowers, ref playerUnknownCommands);
			yield return null;
			callback?.Invoke(new Game(letterPowers, playerInitialCommands, playerInitialLetters, map));
		}

		private static IReadOnlyCollection<Command> GeneratePlayerInitialCommands() => Memory.commandTypes.Where(t => t.initialPlayerAmount > 0)
			.SelectMany(t => Memory.commandsPerType[t].ToList().Shuffled().Take(t.initialPlayerAmount)).ToList();

		private static Dictionary<char, int> GeneratePlayerInitialLetters(Dictionary<char, int> letterPowers, IEnumerable<Command> playerInitialCommands) {
			var playerInitialLetters = TextUtils.allLetters.ToDictionary(t => t, t => 0);
			foreach (var letterPower in letterPowers) {
				playerInitialLetters[letterPower.Key] += Mathf.CeilToInt((float)RlConstants.Player.initialLettersPerPower / letterPower.Value);
			}
			foreach (var knownCommand in playerInitialCommands) {
				foreach (var letter in knownCommand.inputName) {
					playerInitialLetters[letter] += Mathf.RoundToInt(RlConstants.Player.initialLettersPerKnownCommands * knownCommand.type.initialLetterAmountCoefficient);
				}
			}
			return playerInitialLetters;
		}

		private static Dictionary<char, int> GenerateDefaultLetterPowers() {
			var allInputNames = Memory.commands.Select(t => t.inputName)
				//	.Union(Memory.foeTypes.Select(t => t.inputName))
				//	.Union(Memory.interactionOptions.Values.SelectMany(t => t).Select(t => t.inputValue))
				.ToArray();
			var totalLetters = allInputNames.Sum(t => t.Length);
			return TextUtils.allLetters.ToDictionary(c => c, c => totalLetters / allInputNames.Sum(t => t.Count(u => u == c)));
		}

		private static Encounter GenerateEncounter(Vector2Int position, float distanceRatio, IReadOnlyDictionary<char, int> letterPowers) {
			var level = (Encounter.Level)Mathf.FloorToInt(Mathf.Clamp01(Mathf.Clamp01(distanceRatio) - .01f) * (EnumUtils.SizeOf<Encounter.Level>() - 1));

			var countFoes = Mathf.Clamp((int)(level + 1), 1, 3);
			var minSeed = 1 + (float)level * 1.4f;
			var maxSeed = minSeed * 1.4f + ((int)level + 1) * (distanceRatio * (EnumUtils.SizeOf<Encounter.Level>() - 1) % 1);

			return GenerateEncounter(position, level, countFoes.CreateArray(t => (int)Random.Range(minSeed, maxSeed)), letterPowers);
		}

		public static Encounter GenerateEncounter(Vector2Int position, Encounter.Level level, IEnumerable<int> foesLevel, IReadOnlyDictionary<char, int> letterPowers) =>
			new Encounter(level, position, foesLevel.Select(t => GenerateFoe(t, letterPowers)).ToArray());

		private static Foe GenerateFoe(int level, IReadOnlyDictionary<char, int> letterPowers) {
			var type = Memory.foeTypes.Random();
			var powerCoefficient = 1 + (level - 2f) / 10;
			var commands = new HashSet<Command>();
			if (TryGenerateFoeCommands((level + 3) / 4, Memory.commandsPerType[Memory.CommandTypes.attack], out var attackCommands))
				commands.AddAll(attackCommands);
			if (TryGenerateFoeCommands((level + 2) / 4, Memory.commandsPerType[Memory.CommandTypes.defense], out var defenseCommands))
				commands.AddAll(defenseCommands);
			if (TryGenerateFoeCommands((level + 1) / 4, Memory.commandsPerType[Memory.CommandTypes.heal], out var healCommands))
				commands.AddAll(healCommands);
			if (TryGenerateFoeCommands((level + 0) / 4, Memory.commandsPerType[Memory.CommandTypes.dodge], out var dodgeCommands))
				commands.AddAll(dodgeCommands);
			var health = Mathf.RoundToInt(RlConstants.Foes.maxHealthConstant + RlConstants.Foes.maxHealthCoefficient * TextUtils.GetInputValue(type.inputName, letterPowers)) * level;
			return new Foe(type, level, health, powerCoefficient, commands);
		}

		private static bool TryGenerateFoeCommands(int count, IReadOnlyList<Command> fromList, out ISet<Command> commands) {
			commands = null;
			if (count == 0) return false;
			if (fromList == null) return false;
			if (fromList.Count == 0) return false;
			if (fromList.Count <= count) {
				commands = new HashSet<Command>(fromList);
				return true;
			}
			commands = new HashSet<Command>();
			for (var i = 0; i < count; ++i) {
				commands.Add(fromList.Random());
			}
			return true;
		}

		private static DungeonMap GenerateMap(int width, int height, Dictionary<char, int> letterPowers, ref HashSet<Command> playerUnknownCommands) {
			var gridDirections = new DungeonMap.Direction[width, height].FilledWith(DungeonMap.noDirection);

			GenerateDungeonPaths(gridDirections, 0, 0);
			GenerateMoreRandomPaths(gridDirections);
			var rooms = GenerateDungeonRooms(gridDirections);
			var bossCell = rooms.FirstWhereMaxOrDefault(t => Vector2.SqrMagnitude(RlConstants.Dungeon.playerStartPosition - t));
			var miscRoomContents = GenerateMiscRoomContents(rooms.Except(new[] { bossCell, new Vector2Int(0, 0) }), letterPowers, ref playerUnknownCommands);
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

		private static Dictionary<Vector2Int, IDungeonMisc> GenerateMiscRoomContents(IEnumerable<Vector2Int> rooms, IReadOnlyDictionary<char, int> letterPowers,
			ref HashSet<Command> playerUnknownCommands) {
			var result = new Dictionary<Vector2Int, IDungeonMisc>();

			var remainingRooms = new Stack<Vector2Int>(rooms.ToArray().Shuffled().ToArray());

			if (remainingRooms.Count > 2) {
				var portal0 = remainingRooms.Pop();
				var portal1 = remainingRooms.Pop();
				result.Add(portal0, GeneratePortal(portal0, portal1));
				result.Add(portal1, GeneratePortal(portal1, portal0));
			}

			// TODO generate fountain

			while (remainingRooms.Count > 0) {
				var position = remainingRooms.Pop();
				if (remainingRooms.Count % 2 == 0) result.Add(position, GenerateChest(letterPowers, position));
				else result.Add(position, GenerateStoneTableOfKnowledge(letterPowers, ref playerUnknownCommands, position));
				//TODO Generate Training Dummy
			}
			return result;
		}

		private static DungeonPortal GeneratePortal(Vector2Int origin, Vector2Int destination) {
			var portalInteraction = Memory.interactionOptions[InteractionType.Portal].Random();
			var skipInteraction = Memory.interactionOptions[InteractionType.Skip].Random();
			return new DungeonPortal(origin, (portalInteraction, destination), skipInteraction);
		}

		private static DungeonChest GenerateChest(IReadOnlyDictionary<char, int> letterPowers, Vector2Int position) {
			var chestInteraction = Memory.interactionOptions[InteractionType.Chest].Random();
			var score = RlConstants.Dungeon.chestBountyScore * Vector2.Distance(position, RlConstants.Dungeon.playerStartPosition) + TextUtils.GetInputValue(chestInteraction.inputValue, letterPowers);
			var bounty = Memory.chestContentGenerator.Generate(Mathf.RoundToInt(score), letterPowers);
			var skipInteraction = Memory.interactionOptions[InteractionType.Skip].Random();
			return new DungeonChest(position, (chestInteraction, bounty), skipInteraction);
		}

		private static DungeonStoneTableOfKnowledge GenerateStoneTableOfKnowledge(IReadOnlyDictionary<char, int> letterPowers, ref HashSet<Command> playerUnknownCommands, Vector2Int position) {
			(InteractionOption interaction, string bookName) readInteraction = (default, default); // 0 | 1
			(InteractionOption interaction, Command skill) skillInteraction = (default, default); // 0 | 2
			(InteractionOption interaction, char letter) powerInteraction = (default, default); // 1 | 2
			var options = playerUnknownCommands.Count == 0 ? 1 : Random.Range(0, 3);

			if (options == 0 || options == 1) { // book
				var action = Memory.interactionOptions[InteractionType.Read].Random();
				var score = Mathf.RoundToInt(RlConstants.Dungeon.readBookNameScore * Vector2.Distance(position, RlConstants.Dungeon.playerStartPosition))
								+ TextUtils.GetInputValue(action.inputValue, letterPowers);
				readInteraction.bookName = Memory.bookNameGenerator.Generate(score, letterPowers);
				readInteraction.interaction = new InteractionOption(action, $"\"{readInteraction.bookName}\"");
			}
			if (options == 0 || options == 2) { // skill
				var score = Mathf.RoundToInt(RlConstants.Dungeon.skillCommandScore * Vector2.Distance(position, RlConstants.Dungeon.playerStartPosition));
				skillInteraction.skill = playerUnknownCommands.GetWithClosestScore(t => TextUtils.GetInputValue(t.inputName, letterPowers), score);
				skillInteraction.interaction = new InteractionOption(Memory.interactionOptions[InteractionType.Skill].Random(), $"the {skillInteraction.skill.inputName} command");
				playerUnknownCommands.Remove(skillInteraction.skill);
			}
			if (options == 1 || options == 2) { // power
				var score = Mathf.RoundToInt(RlConstants.Dungeon.powerLetterScore * Vector2.Distance(position, RlConstants.Dungeon.playerStartPosition));
				powerInteraction.letter = letterPowers.Keys.GetWithClosestScore(t => letterPowers[t], score);
				powerInteraction.interaction = new InteractionOption(Memory.interactionOptions[InteractionType.Power].Random(), $"the letter \"{powerInteraction.letter}\" to double its power.");
			}

			var skipInteraction = Memory.interactionOptions[InteractionType.Skip].Random();
			return new DungeonStoneTableOfKnowledge(position, readInteraction, skillInteraction, powerInteraction, skipInteraction);
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