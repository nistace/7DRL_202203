using _7DRL.GameComponents.TextAndLetters;
using _7DRL.Games;
using TMPro;
using UnityEngine;
using Utils.Libraries;

namespace _7DRL.Ui {
	public class KnownCommandsItemUi : MonoBehaviour {
		[SerializeField] protected TMP_Text _commandText;
		[SerializeField] protected TMP_Text _amountText;
		[SerializeField] protected TMP_Text _commandDescriptionText;
		[SerializeField] protected bool     _valid;
		[SerializeField] protected int      _amount;

		public Command command { get; private set; }

		public bool valid {
			get => _valid;
			set {
				_valid = value;
				Refresh();
			}
		}

		private Color amountColor {
			get {
				if (_valid && _amount == 0) return Colors.Of("ui.text.invalid.active");
				if (_valid) return Colors.Of("ui.text.player.active");
				if (_amount == 0) return Colors.Of("ui.text.invalid.inactive");
				return Colors.Of("ui.text.player.disabled");
			}
		}

		private Color textColor {
			get {
				if (_valid) return Colors.Of("ui.text.player.active");
				return Colors.Of("ui.text.player.disabled");
			}
		}

		public void Set(Command command) {
			this.command = command;
			Refresh();
		}

		public void Refresh() {
			_commandText.text = command.inputName;
			_commandText.color = textColor;
			if (Game.instance == null) return;
			_amount = Game.instance.playerCharacter.CountOpportunitiesToPlay(command);
			_amountText.text = $"{Game.instance.playerCharacter.CountOpportunitiesToPlay(command)}";
			_amountText.color = amountColor;
			_commandDescriptionText.text = Game.instance.playerCharacter.GetCommandDescription(command);
			_commandDescriptionText.color = textColor;
		}
	}
}