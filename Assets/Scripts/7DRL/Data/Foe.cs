using System;
using System.Collections.Generic;
using System.Linq;
using _7DRL.TextInput;
using UnityEngine;
using Utils.Extensions;

namespace _7DRL.Data {
	[Serializable]
	public class Foe : CharacterBase {
		public static IReadOnlyList<int> letterPowers { get; set; }

		[SerializeField] protected FoeType _type;
		[SerializeField] protected int     _currentCommandIndex;
		[SerializeField] protected int     _currentCommandProgress;
		[SerializeField] protected float   _powerCoefficient;

		public override string name                         => _type.name;
		public override string currentCommandLetters        => currentCommand.inputName.Substring(0, _currentCommandProgress);
		public override string currentCommandMissingLetters => currentCommand.inputName.Substring(_currentCommandProgress);

		public Command currentCommand => _knownCommands[_currentCommandIndex];

		public Foe(FoeType type, int level, int maxHealth, float powerCoefficient, IEnumerable<Command> knownCommands) : base(level, maxHealth, knownCommands.ToList().Shuffled()) {
			_type = type;
			_currentCommandIndex = 0;
			_powerCoefficient = powerCoefficient;
		}

		public void ProgressCurrentCommand() {
			_currentCommandProgress = Mathf.Min(_currentCommandProgress + _level, currentCommand.inputName.Length);
			onCurrentCommandChanged.Invoke();
		}

		public bool IsCurrentCommandReady() => _currentCommandProgress == currentCommand.inputName.Length;

		public void PrepareNextCommand() {
			_currentCommandIndex = (_currentCommandIndex + 1) % _knownCommands.Count;
			_currentCommandProgress = 0;
			onCurrentCommandChanged.Invoke();
		}

		public override int GetCommandPower(Command command) => command.type.FixPower(Mathf.RoundToInt(_powerCoefficient * TextUtils.GetInputValue(command.inputName, letterPowers)));

		public override bool TryGetCurrentCommand(out Command command) {
			command = currentCommand;
			return command != null && !dead;
		}
	}
}