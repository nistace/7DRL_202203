using System;
using System.Collections.Generic;
using System.Linq;
using _7DRL.MiscConstants;
using _7DRL.TextInput;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace _7DRL.Data {
	[Serializable]
	public class PlayerCharacter : CharacterBase, IDungeonCrawler {
		[SerializeField] protected List<int>     _letterPowers;
		[SerializeField] protected string        _currentCommandLetters;
		[SerializeField] protected string        _currentCommandMissingLetters;
		[SerializeField] protected Command       _advisedCurrentCommand;
		[SerializeField] protected LetterReserve _letterReserve;
		[SerializeField] protected Vector2Int    _dungeonPosition;

		public override string             name                         => "Player";
		public override string             currentCommandLetters        => _currentCommandLetters;
		public override string             currentCommandMissingLetters => _currentCommandMissingLetters;
		public          IReadOnlyList<int> letterPowers                 => _letterPowers;
		public          Command            advisedCurrentCommand        => _advisedCurrentCommand;
		public          LetterReserve      letterReserve                => _letterReserve;

		public UnityEvent onKnownCommandsChanged { get; } = new UnityEvent();
		public UnityEvent onLetterPowersChanged  { get; } = new UnityEvent();

		public Vector2Int dungeonPosition {
			get => _dungeonPosition;
			set => _dungeonPosition = value;
		}

		public PlayerCharacter(IEnumerable<int> defaultLetterPowers, IReadOnlyDictionary<char, int> playerInitialLetters, IEnumerable<Command> playerInitialCommands) : base(1,
			RlConstants.Player.initialMaxHealth, playerInitialCommands) {
			_letterPowers = new List<int>(defaultLetterPowers);
			_letterReserve = new LetterReserve();
			playerInitialLetters.ForEach(t => _letterReserve.Add(t.Key, t.Value));
			_dungeonPosition = Vector2Int.zero;
		}

		public void LearnCommand(Command command) {
			_knownCommands.Add(command);
			onKnownCommandsChanged.Invoke();
		}

		public void EnhanceLetterPower(char letter, float coefficient) {
			_letterPowers[letter - 'A'] = Mathf.RoundToInt(coefficient * _letterPowers[letter - 'A']);
			onLetterPowersChanged.Invoke();
		}

		public void SetLetterPowers(IEnumerable<int> letterPowers) => _letterPowers = new List<int>(letterPowers);

		public void SetCurrentCommand(string command, CommandType.Location location) {
			_currentCommandLetters = command;
			_advisedCurrentCommand = !string.IsNullOrEmpty(command) && _knownCommands.TryFirst(t => t.inputName.StartsWith(command) && t.type.IsUsable(location), out var advised) ? advised : null;
			_currentCommandMissingLetters = _advisedCurrentCommand?.inputName.Substring(command.Length) ?? string.Empty;
			onCurrentCommandChanged.Invoke();
		}

		private string GetCurrentCommandMissingLetters(string currentCommand) {
			if (string.IsNullOrEmpty(currentCommand)) return string.Empty;
			if (_advisedCurrentCommand != null) return _advisedCurrentCommand.inputName.Substring(currentCommand.Length);
			return string.Empty;
		}

		public override int GetCommandPower(Command command) => command.type.FixPower(TextUtils.GetInputValue(command.inputName, _letterPowers));
		public override bool TryGetCurrentCommand(out Command command) => (command = advisedCurrentCommand) != null;

		public bool TryGetCurrentCommandIfComplete(out Command command) => TryGetCurrentCommand(out command) && command.inputName == _currentCommandLetters;

		public int CountOpportunitiesToPlay(Command command) {
			var count = int.MaxValue;
			for (var c = 'A'; c <= 'Z'; ++c) {
				var charCount = command.inputName.Count(t => t == c);
				if (charCount > 0) {
					count = Mathf.Min(count, letterReserve[c] / charCount);
				}
			}
			return count;
		}
	}
}