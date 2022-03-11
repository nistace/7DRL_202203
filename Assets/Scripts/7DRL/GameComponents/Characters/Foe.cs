using System;
using System.Collections.Generic;
using System.Linq;
using _7DRL.GameComponents.TextAndLetters;
using UnityEngine;
using Utils.Extensions;
using Random = UnityEngine.Random;

namespace _7DRL.GameComponents.Characters {
	[Serializable]
	public class Foe : CharacterBase {
		public static IReadOnlyList<int> letterPowers { get; set; }

		[SerializeField] protected FoeType _type;
		[SerializeField] protected int     _currentCommandIndex;
		[SerializeField] protected int     _currentCommandProgress;
		[SerializeField] protected float   _powerCoefficient;

		public override string  name                         => _type.name;
		public          byte    spriteSeed                   { get; }
		public override string  currentCommandLetters        => currentCommand.textInput.Substring(0, _currentCommandProgress);
		public override string  currentCommandMissingLetters => currentCommand.textInput.Substring(_currentCommandProgress);
		public          Command currentCommand               => _knownCommands[_currentCommandIndex];

		public Foe(FoeType type, int level, int maxHealth, float powerCoefficient, IEnumerable<Command> knownCommands) : base(level, maxHealth, knownCommands.ToList().Shuffled()) {
			_type = type;
			spriteSeed = (byte)Random.Range(0, byte.MaxValue);
			_currentCommandIndex = 0;
			_powerCoefficient = powerCoefficient;
		}

		public void ProgressCurrentCommand() {
			_currentCommandProgress = Mathf.Min(_currentCommandProgress + _level, currentCommand.textInput.Length);
			onCurrentCommandChanged.Invoke();
		}

		public bool IsCurrentCommandReady() => _currentCommandProgress == currentCommand.textInput.Length;

		public void PrepareNextCommand() {
			_currentCommandIndex = (_currentCommandIndex + 1) % _knownCommands.Count;
			_currentCommandProgress = 0;
			onCurrentCommandChanged.Invoke();
		}

		public override int GetCommandPower(Command command) => command.type.FixPower(Mathf.RoundToInt(_powerCoefficient * TextUtils.GetInputValue(command.textInput, letterPowers)));

		public override bool TryGetCurrentCommand(out Command command) {
			command = currentCommand;
			return command != null && !dead;
		}

		public override void ResetForBattle() {
			base.ResetForBattle();
			_currentCommandIndex = 0;
			_currentCommandProgress = 0;
		}
	}
}