using System.Collections.Generic;
using System.Linq;
using _7DRL.Data;
using _7DRL.MiscConstants;
using UnityEngine;
using Utils.Extensions;

namespace _7DRL.Games {
	public static class GameFactory {
		public static Game CreateGame() {
			var newGame = new Game();
			Memory.commandTypes.ForEach(type => newGame.playerCharacter.LearnCommand(Memory.commands.Where(command => command.type == type).Random()));
			var allInputNames = Memory.commands.Select(t => t.inputName).Union(Memory.foeTypes.Select(t => t.inputName)).ToArray();
			var totalLetters = allInputNames.Sum(t => t.Length);
			for (var letter = 'A'; letter <= 'Z'; letter++) {
				newGame.SetDefaultLetterPower(letter, totalLetters / allInputNames.Sum(t => t.Count(u => u == letter)));
			}
			newGame.ApplyDefaultLetterPowers();
			return newGame;
		}

		public static Encounter GenerateEncounter(int seed) {
			var foeSeeds = 3.CreateArray(t => (int)(seed / Mathf.Pow(10, t + 1)) % 1000);
			var foes = foeSeeds.Where(t => t != 0).Select(GenerateFoe).ToArray();
			return new Encounter(foes);
		}

		private static Foe GenerateFoe(int seed) {
			seed %= 1000;
			var type = Memory.foeTypes[seed % Memory.foeTypes.Count];
			var level = 1 + seed / 100;
			var commands = new HashSet<Command>();
			if (TryGenerateFoeCommands(seed + Constants.magicNumber, 1 + Mathf.FloorToInt(level / 2f), Memory.attackCommands, out var attackCommands)) commands.AddAll(attackCommands);
			if (TryGenerateFoeCommands(seed + Constants.magicNumber, Mathf.FloorToInt(level / 2f), Memory.defenseCommands, out var defenseCommands)) commands.AddAll(defenseCommands);
			if (TryGenerateFoeCommands(seed + Constants.magicNumber, Mathf.FloorToInt(level / 3f), Memory.dodgeCommands, out var dodgeCommands)) commands.AddAll(dodgeCommands);
			if (TryGenerateFoeCommands(seed + Constants.magicNumber, Mathf.FloorToInt(level / 4f), Memory.healCommands, out var healCommands)) commands.AddAll(healCommands);
			return new Foe(type, level, Constants.Foes.initialMaxHealthCoefficient * Game.instance.GetDefaultPower(type.inputName) * level, 1, commands);
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
				nextAttackSeed += Constants.magicNumber;
			}
			return true;
		}
	}
}