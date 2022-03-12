using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _7DRL.GameComponents.Characters;
using _7DRL.GameComponents.Dungeons;
using _7DRL.GameComponents.Dungeons.Misc;
using _7DRL.GameComponents.Interactions;
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
			var playerInitialLetters = GeneratePlayerInitialLetters(playerInitialCommands);
			yield return null;
			var map = GenerateMap(RlConstants.Dungeon.width, RlConstants.Dungeon.height, letterPowers, ref playerUnknownCommands);
			yield return null;
			callback?.Invoke(new Game(letterPowers, playerInitialCommands, playerInitialLetters, map));
		}

		private static IReadOnlyCollection<Command> GeneratePlayerInitialCommands() => Memory.commandTypes.Where(t => t.initialPlayerAmount > 0)
			.SelectMany(t => Memory.commandsPerType[t].ToList().Shuffled().Take(t.initialPlayerAmount)).ToList();

		private static Dictionary<char, int> GeneratePlayerInitialLetters(IEnumerable<Command> playerInitialCommands) {
			var playerInitialLetters = TextUtils.allLetters.ToDictionary(t => t, t => RlConstants.Player.baseLetterAmount);
			foreach (var knownCommand in playerInitialCommands) {
				foreach (var letter in knownCommand.textInput) {
					playerInitialLetters[letter] += Mathf.RoundToInt(RlConstants.Player.initialLettersPerKnownCommands * knownCommand.type.initialLetterAmountCoefficient);
				}
			}
			return playerInitialLetters;
		}

		private static Dictionary<char, int> GenerateDefaultLetterPowers() {
			var allInputNames = Memory.commands.Select(t => t.textInput).ToArray();
			float totalLetters = allInputNames.Sum(t => t.Length);
			var lettersPresence = TextUtils.allLetters.ToDictionary(c => c, c => allInputNames.Sum(t => t.Count(u => u == c)) / totalLetters);
			var letterRawPower = lettersPresence.ToDictionary(t => t.Key, t => 1f / t.Value);
			var minRawPower = letterRawPower.Min(t => t.Value);
			var maxRawPower = letterRawPower.Max(t => t.Value);
			return letterRawPower.ToDictionary(t => t.Key, t => Mathf.RoundToInt(t.Value.Remap(minRawPower, maxRawPower, 1, RlConstants.letterMaxPower)));
		}

		private static Encounter GenerateEncounter(Vector2Int position, float distanceRatio, ref HashSet<Command> playerUnknownCommands, IReadOnlyDictionary<char, int> letterPowers) {
			var level = (Encounter.Level)Mathf.FloorToInt(Mathf.Clamp01(Mathf.Clamp01(distanceRatio) - .01f) * (EnumUtils.SizeOf<Encounter.Level>() - 1));

			var countFoes = Mathf.Clamp((int)(level + 1), 1, 3);
			var minSeed = 1 + (float)level;
			var maxSeed = minSeed * 1.4f + ((int)level + 1) * (distanceRatio * (EnumUtils.SizeOf<Encounter.Level>() - 1) % 1);

			return GenerateEncounter(position, level, countFoes.CreateArray(t => (int)Random.Range(minSeed, maxSeed)), ref playerUnknownCommands, letterPowers);
		}

		private static Encounter GenerateEncounter(Vector2Int position, Encounter.Level level, IEnumerable<int> foesLevel, ref HashSet<Command> playerUnknownCommands,
			IReadOnlyDictionary<char, int> letterPowers) {
			var foes = foesLevel.Select(t => GenerateFoe(t, letterPowers)).ToArray();

			InteractionOption maxHealthInteraction = default; // 0 | 1
			(InteractionOption interaction, Command skill) skillInteraction = (default, default); // 0 | 2
			(InteractionOption interaction, char letter) powerInteraction = (default, default); // 1 | 2
			var foesCommandsUnknownByPlayer = foes.SelectMany(t => t.knownCommands).Intersect(playerUnknownCommands).ToArray();
			var options = foesCommandsUnknownByPlayer.Length == 0 ? 1 : Random.Range(0, 3);
			var uniqueFirstLetters = new HashSet<char>();

			if (options == 0 || options == 1) { // max health
				maxHealthInteraction = GetRandomInteraction(InteractionType.MaxHealth, uniqueFirstLetters);
			}
			if (options == 0 || options == 2) { // skill
				skillInteraction.skill = foesCommandsUnknownByPlayer.Random();
				skillInteraction.interaction = new InteractionOption(GetRandomInteraction(InteractionType.Skill, uniqueFirstLetters), $"the {skillInteraction.skill.textInput} command.");
				playerUnknownCommands.Remove(skillInteraction.skill);
			}
			if (options == 1 || options == 2) { // power
				powerInteraction.letter = TextUtils.allLetters.Random();
				powerInteraction.interaction = new InteractionOption(GetRandomInteraction(InteractionType.Power, uniqueFirstLetters), $"the letter \"{powerInteraction.letter}\" to double its power.");
			}

			var skipInteraction = GetRandomInteraction(InteractionType.Skip, uniqueFirstLetters);

			return new Encounter(level, position, foes, maxHealthInteraction, skillInteraction, powerInteraction, skipInteraction);
		}

		private static InteractionOption GetRandomInteraction(InteractionType type, HashSet<char> forbiddenLetters) {
			var option = Memory.interactionOptions[type].Where(t => !forbiddenLetters.Contains(t.inputValue[0])).Random();
			forbiddenLetters.Add(option.inputValue[0]);
			return option;
		}

		private static Foe GenerateFoe(int level, IReadOnlyDictionary<char, int> letterPowers) {
			var type = Memory.foeTypes.Random();
			const int powerCoefficient = 1;
			var commands = new HashSet<Command>();
			if (level >= 1) commands.Add(Memory.commandsPerType[Memory.CommandTypes.attack].Except(commands).Random());
			if (level >= 2) commands.Add(Memory.commandsPerType[Memory.CommandTypes.defense].Except(commands).Random());
			if (level >= 3) commands.Add(Memory.commandsPerType[Memory.CommandTypes.attack].Except(commands).Random());
			if (level >= 4) commands.Add(Memory.commandsPerType[Memory.CommandTypes.heal].Except(commands).Random());
			if (level >= 5) commands.Add(Memory.commandsPerType[Memory.CommandTypes.dodge].Except(commands).Random());
			if (level >= 6) commands.Add(Memory.commandsPerType[Memory.CommandTypes.attack].Except(commands).Random());
			if (level >= 7) commands.Add(Memory.commandsPerType[Memory.CommandTypes.defense].Except(commands).Random());
			if (level >= 8) commands.Add(Memory.commandsPerType[Memory.CommandTypes.attack].Except(commands).Random());
			if (level >= 9) commands.Add(Memory.commandsPerType[Memory.CommandTypes.defense].Except(commands).Random());
			var score = TextUtils.GetInputValue(type.inputName, letterPowers) + commands.Sum(t => TextUtils.GetInputValue(t.textInput, letterPowers));
			var health = Mathf.RoundToInt(RlConstants.Foes.maxHealthConstant + RlConstants.Foes.maxHealthCoefficient * score) * level;
			return new Foe(type, level, health, powerCoefficient, commands);
		}

		private static DungeonMap GenerateMap(int width, int height, Dictionary<char, int> letterPowers, ref HashSet<Command> playerUnknownCommands) {
			var gridDirections = new DungeonMap.Direction[width, height].FilledWith(DungeonMap.noDirection);

			GenerateDungeonPaths(gridDirections, 0, 0);
			GenerateMoreRandomPaths(gridDirections);
			var rooms = GenerateDungeonRooms(gridDirections);
			var bossCell = rooms.FirstWhereMaxOrDefault(t => Vector2.SqrMagnitude(RlConstants.Dungeon.playerStartPosition - t));
			var miscRoomContents = GenerateMiscRoomContents(rooms.Except(new[] { bossCell, new Vector2Int(0, 0) }), letterPowers, ref playerUnknownCommands);
			var encounters = GenerateEncounters(gridDirections, rooms, bossCell, ref playerUnknownCommands, letterPowers);
			return new DungeonMap(gridDirections, rooms, miscRoomContents, encounters);
		}

		private static IReadOnlyDictionary<Vector2Int, Encounter> GenerateEncounters(DungeonMap.Direction[,] gridDirections, IReadOnlyCollection<Vector2Int> rooms, Vector2Int bossCell,
			ref HashSet<Command> playerUnknownCommands, IReadOnlyDictionary<char, int> letterPowers) {
			var result = new Dictionary<Vector2Int, Encounter> { { bossCell, GenerateEncounter(bossCell, Encounter.Level.Boss, new[] { 4, 5, 6 }, ref playerUnknownCommands, letterPowers) } };

			var maxDistance = Vector2.Distance(bossCell, RlConstants.Dungeon.playerStartPosition);
			while (result.Count < RlConstants.Dungeon.encounters) {
				var position = new Vector2Int(Random.Range(0, gridDirections.GetLength(0)), Random.Range(0, gridDirections.GetLength(1)));
				if (result.ContainsKey(position)) continue;
				if (rooms.Contains(position)) continue;
				var distance = Vector2.Distance(RlConstants.Dungeon.playerStartPosition, position) / maxDistance - .1f;

				result.Add(position, GenerateEncounter(position, distance, ref playerUnknownCommands, letterPowers));
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

			if (remainingRooms.Count >= 1) {
				var fountainPosition = remainingRooms.Pop();
				result.Add(fountainPosition, GenerateFountainOfYouth(fountainPosition));
			}

			while (remainingRooms.Count > 0) {
				var position = remainingRooms.Pop();
				if (remainingRooms.Count % 2 == 0) result.Add(position, GenerateChest(letterPowers, position));
				else result.Add(position, GenerateStoneTableOfKnowledge(letterPowers, ref playerUnknownCommands, position));
				//TODO Generate Training Dummy
			}
			return result;
		}

		private static DungeonPortal GeneratePortal(Vector2Int origin, Vector2Int destination) {
			var uniqueFirstLetters = new HashSet<char>();
			var portalInteraction = GetRandomInteraction(InteractionType.Portal, uniqueFirstLetters);
			var skipInteraction = GetRandomInteraction(InteractionType.Skip, uniqueFirstLetters);
			return new DungeonPortal(origin, (portalInteraction, destination), skipInteraction);
		}

		private static DungeonFountainOfYouth GenerateFountainOfYouth(Vector2Int position) {
			var uniqueFirstLetters = new HashSet<char>();
			var fountainInteraction = GetRandomInteraction(InteractionType.Fountain, uniqueFirstLetters);
			var skipInteraction = GetRandomInteraction(InteractionType.Skip, uniqueFirstLetters);
			return new DungeonFountainOfYouth(position, fountainInteraction, skipInteraction);
		}

		private static DungeonChest GenerateChest(IReadOnlyDictionary<char, int> letterPowers, Vector2Int position) {
			var uniqueFirstLetters = new HashSet<char>();
			var chestInteraction = GetRandomInteraction(InteractionType.Chest, uniqueFirstLetters);
			var score = RlConstants.Dungeon.chestBountyScore * Vector2.Distance(position, RlConstants.Dungeon.playerStartPosition) + TextUtils.GetInputValue(chestInteraction.inputValue, letterPowers);
			var bounty = Memory.chestContentGenerator.Generate(Mathf.RoundToInt(score), letterPowers);
			var skipInteraction = GetRandomInteraction(InteractionType.Skip, uniqueFirstLetters);
			return new DungeonChest(position, (chestInteraction, bounty), skipInteraction);
		}

		private static DungeonStoneTableOfKnowledge GenerateStoneTableOfKnowledge(IReadOnlyDictionary<char, int> letterPowers, ref HashSet<Command> playerUnknownCommands, Vector2Int position) {
			(InteractionOption interaction, string bookName) readInteraction = (default, default); // 0 | 1
			(InteractionOption interaction, Command skill) skillInteraction = (default, default); // 0 | 2
			(InteractionOption interaction, char letter) powerInteraction = (default, default); // 1 | 2
			var options = playerUnknownCommands.Count == 0 ? 1 : Random.Range(0, 3);
			var uniqueFirstLetters = new HashSet<char>();

			if (options == 0 || options == 1) { // book
				var action = GetRandomInteraction(InteractionType.Book, uniqueFirstLetters);
				var score = Mathf.RoundToInt(RlConstants.Dungeon.bookNameScore * Vector2.Distance(position, RlConstants.Dungeon.playerStartPosition))
								+ TextUtils.GetInputValue(action.inputValue, letterPowers);
				readInteraction.bookName = Memory.bookNameGenerator.Generate(score, letterPowers);
				readInteraction.interaction = new InteractionOption(action, $"\"{readInteraction.bookName}\"");
			}
			if (options == 0 || options == 2) { // skill
				var score = Mathf.RoundToInt(RlConstants.Dungeon.skillCommandScore * Vector2.Distance(position, RlConstants.Dungeon.playerStartPosition));
				skillInteraction.skill = playerUnknownCommands.GetWithClosestScore(t => TextUtils.GetInputValue(t.textInput, letterPowers), score);
				skillInteraction.interaction = new InteractionOption(GetRandomInteraction(InteractionType.Skill, uniqueFirstLetters), $"the {skillInteraction.skill.textInput} command.");
				playerUnknownCommands.Remove(skillInteraction.skill);
			}
			if (options == 1 || options == 2) { // power
				powerInteraction.letter = TextUtils.allLetters.Random();
				powerInteraction.interaction = new InteractionOption(GetRandomInteraction(InteractionType.Power, uniqueFirstLetters), $"the letter \"{powerInteraction.letter}\" to double its power.");
			}

			var skipInteraction = GetRandomInteraction(InteractionType.Skip, uniqueFirstLetters);
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