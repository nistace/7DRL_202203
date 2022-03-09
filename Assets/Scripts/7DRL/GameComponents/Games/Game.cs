using System;
using System.Collections.Generic;
using _7DRL.GameComponents.Characters;
using _7DRL.GameComponents.Dungeons;
using _7DRL.GameComponents.TextAndLetters;
using UnityEngine;
using Utils.Extensions;

namespace _7DRL.Games {
	[Serializable]
	public class Game {
		public static Game instance { get; set; }

		[SerializeField] protected PlayerCharacter _playerCharacter;
		[SerializeField] protected List<int>       _defaultLetterPowers;
		[SerializeField] protected DungeonMap      _dungeonMap;
		[SerializeField] protected int             _turn;
		[SerializeField] protected TurnStep        _turnStep;

		public PlayerCharacter    playerCharacter     => _playerCharacter;
		public DungeonMap         dungeonMap          => _dungeonMap;
		public IReadOnlyList<int> defaultLetterPowers => _defaultLetterPowers;
		public int                turn                => _turn;
		public TurnStep           turnStep            => _turnStep;

		public enum TurnStep {
			Player    = 0,
			SolveMisc = 1,
			Foe       = 2
		}

		public Game(Dictionary<char, int> letterPowers, IEnumerable<Command> playerInitialCommands, IReadOnlyDictionary<char, int> playerInitialLetters, DungeonMap map) {
			_defaultLetterPowers = new List<int>();
			letterPowers.ForEach(t => SetDefaultLetterPower(t.Key, t.Value));
			Foe.letterPowers = _defaultLetterPowers;
			_playerCharacter = new PlayerCharacter(_defaultLetterPowers, playerInitialLetters, playerInitialCommands);
			_dungeonMap = map;
			_turn = 1;
			_turnStep = TurnStep.Player;
		}

		private void SetDefaultLetterPower(char letter, int power) {
			while (_defaultLetterPowers.Count <= letter - 'A') _defaultLetterPowers.Add(0);
			_defaultLetterPowers[letter - 'A'] = power;
		}

		public void NextTurnStep() => ChangeTurnStep((TurnStep)((int)(_turnStep + 1) % EnumUtils.SizeOf<TurnStep>()));

		public void ChangeTurnStep(TurnStep turnStep) {
			if (_turnStep == turnStep) return;
			if (_turnStep > turnStep) _turn++;
			_turnStep = turnStep;
			GameEvents.onTurnChanged.Invoke();
		}
	}
}