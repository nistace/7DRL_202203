using _7DRL.Data;
using TMPro;
using UnityEngine;
using Utils.Extensions;
using Utils.Libraries;

public class CommandTrackerUi : MonoBehaviour {
	[SerializeField] protected TMP_Text _commandText;
	[SerializeField] protected TMP_Text _commandDescriptionText;
	[SerializeField] protected bool     _player;
	[SerializeField] protected bool     _valid;

	private CharacterBase character { get; set; }

	public bool valid {
		get => _valid;
		set {
			_valid = value;
			Refresh();
		}
	}

	private Color activeLettersColor {
		get {
			if (!_valid) return Colors.Of($"ui.text.invalid.active");
			if (!_player) return Colors.Of($"ui.text.foe.active");
			return Colors.Of($"ui.text.player.active");
		}
	}

	private Color inactiveLettersColor {
		get {
			if (!_valid) return Colors.Of("ui.text.invalid.inactive");
			if (!_player) return Colors.Of("ui.text.foe.inactive");
			return Colors.Of("ui.text.player.inactive");
		}
	}

	public void Set(CharacterBase character) {
		this.character = character;
		character.onCurrentCommandChanged.AddListenerOnce(Refresh);
		character.onHealthOrArmorChanged.AddListenerOnce(HandleCharacterHealthChanged);
		Refresh();
	}

	private void HandleCharacterHealthChanged() {
		if (character.dead) Refresh();
	}

	private void Refresh() {
		if (character.dead) {
			_commandText.text = string.Empty;
			_commandDescriptionText.text = string.Empty;
			return;
		}

		_commandText.text = $"<{activeLettersColor.ToHexaString(true)}>{character.currentCommandLetters}<{inactiveLettersColor.ToHexaString(true)}>{character.currentCommandMissingLetters}";
		_commandDescriptionText.text = character.TryGetCurrentCommand(out var buildingCommand) ? buildingCommand.type.GetDescription(character.GetCommandPower(buildingCommand)) : string.Empty;
	}
}