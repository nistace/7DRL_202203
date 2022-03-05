using System;
using System.Collections.Generic;
using System.Linq;
using _7DRL.MiscConstants;
using _7DRL.TextInput;
using UnityEngine;
using Utils.Extensions;

namespace _7DRL.Data {
	[Serializable]
	public class PlayerCharacter : CharacterBase {
		[SerializeField] protected List<int> _letterPowers = new List<int>();
		[SerializeField] protected string    _currentCommandLetters;
		[SerializeField] protected string    _currentCommandMissingLetters;
		[SerializeField] protected Command   _advisedCurrentCommand;

		public override string name                         => "Player";
		public override string currentCommandLetters        => _currentCommandLetters;
		public override string currentCommandMissingLetters => _currentCommandMissingLetters;

		public IEnumerable<Command> learnedCombatCommands => _knownCommands.Where(t => t.type.usedInCombat);
		public IReadOnlyList<int>   letterPowers          => _letterPowers;
		public Command              advisedCurrentCommand => _advisedCurrentCommand;

		public PlayerCharacter() : this(1, Constants.Player.initialMaxHealth, Array.Empty<Command>()) { }

		public void LearnCommand(Command command) => _knownCommands.Add(command);
		public void SetLetterPowers(IEnumerable<int> letterPowers) => _letterPowers = new List<int>(letterPowers);
		public PlayerCharacter(int level, int maxHealth, IEnumerable<Command> knownCommands) : base(level, maxHealth, knownCommands) { }

		public void SetCurrentCommand(string currentCommand) {
			_currentCommandLetters = currentCommand;
			_advisedCurrentCommand = string.IsNullOrEmpty(currentCommand) ? null : _knownCommands.FirstOrDefault(t => t.inputName.StartsWith(currentCommand));
			_currentCommandMissingLetters = _advisedCurrentCommand?.inputName.Substring(currentCommand.Length) ?? string.Empty;
			onCurrentCommandChanged.Invoke();
		}

		private string GetCurrentCommandMissingLetters(string currentCommand) {
			if (string.IsNullOrEmpty(currentCommand)) return string.Empty;
			if (_advisedCurrentCommand != null) return _advisedCurrentCommand.inputName.Substring(currentCommand.Length);
			return string.Empty;
		}

		public bool TryRecognizeCurrentCommand(out Command recognizedCommand) => _knownCommands.TryFirst(t => t.inputName == _currentCommandLetters, out recognizedCommand);
		public override int GetCommandPower(Command command) => command.type.FixPower(TextUtils.GetInputValue(command.inputName, _letterPowers));
		public override bool TryGetCurrentCommand(out Command command) => (command = advisedCurrentCommand) != null;
	}
}