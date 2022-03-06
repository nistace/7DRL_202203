using System;
using System.Collections.Generic;
using _7DRL.Data;
using _7DRL.TextInput;
using UnityEngine;
using Utils.Extensions;

namespace _7DRL.Games {
	[Serializable]
	public class Game {
		public static Game instance { get; set; }

		[SerializeField] protected PlayerCharacter _playerCharacter;
		[SerializeField] protected List<int>       _defaultLetterPowers;
		[SerializeField] protected DungeonMap      _dungeonMap;

		public PlayerCharacter    playerCharacter     => _playerCharacter;
		public DungeonMap         dungeonMap          => _dungeonMap;
		public IReadOnlyList<int> defaultLetterPowers => _defaultLetterPowers;

		public Game(Dictionary<char, int> letterPowers, IEnumerable<Command> playerInitialCommands, IReadOnlyDictionary<char, int> playerInitialLetters, DungeonMap map) {
			_defaultLetterPowers = new List<int>();
			letterPowers.ForEach(t => SetDefaultLetterPower(t.Key, t.Value));
			Foe.letterPowers = _defaultLetterPowers;

			_playerCharacter = new PlayerCharacter(_defaultLetterPowers, playerInitialLetters, playerInitialCommands);

			_dungeonMap = map;
		}

		private void SetDefaultLetterPower(char letter, int power) {
			while (_defaultLetterPowers.Count <= letter - 'A') _defaultLetterPowers.Add(0);
			_defaultLetterPowers[letter - 'A'] = power;
		}
	}
}