using System;
using System.Collections.Generic;
using _7DRL.Data;
using _7DRL.TextInput;
using UnityEngine;

namespace _7DRL.Games {
	[Serializable]
	public class Game {
		public static Game instance { get; set; }

		[SerializeField] protected PlayerCharacter _playerCharacter;
		[SerializeField] protected List<int>       _defaultLetterPowers = new List<int>();

		public PlayerCharacter playerCharacter => _playerCharacter;

		public Game() {
			_playerCharacter = new PlayerCharacter();
		}

		public void SetDefaultLetterPower(char letter, int power) {
			while (_defaultLetterPowers.Count <= letter - 'A') _defaultLetterPowers.Add(0);
			_defaultLetterPowers[letter - 'A'] = power;
		}

		public int GetDefaultPower(string inputName) => TextUtils.GetInputValue(inputName, _defaultLetterPowers);

		public void ApplyDefaultLetterPowers() {
			_playerCharacter.SetLetterPowers(_defaultLetterPowers);
			Foe.letterPowers = _defaultLetterPowers;
		}
	}
}