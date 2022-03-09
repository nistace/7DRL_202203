using TMPro;
using UnityEngine;
using Utils.Extensions;
using Utils.Libraries;

public class DialogOptionLineUi : MonoBehaviour {
	[SerializeField] protected TMP_Text _command;
	[SerializeField] protected TMP_Text _text;
	[SerializeField] protected bool     _charged = true;
	[SerializeField] protected int      _activeLetters;

	private string commandInputValue { get; set; }

	public int activeLetters {
		get => _activeLetters;
		set {
			if (_activeLetters == value) return;
			_activeLetters = value;
			Refresh();
		}
	}

	private Color activeLettersColor {
		get {
			if (_charged) return Colors.Of("ui.text.player.active");
			return Colors.Of("ui.text.costless.active");
		}
	}

	private Color inactiveLettersColor {
		get {
			if (_charged) return Colors.Of("ui.text.player.inactive");
			return Colors.Of("ui.text.costless.inactive");
		}
	}

	public void Set(string commandInputValue, string text, bool charged) {
		_charged = charged;
		this.commandInputValue = commandInputValue;
		_text.text = text;
		Refresh();
	}

	private void Refresh() {
		_command.text = string.Empty;
		if (_activeLetters > 0) _command.text += $"<{activeLettersColor.ToHexaString(true)}>{commandInputValue.Substring(0, _activeLetters)}";
		if (activeLetters < commandInputValue.Length) _command.text += $"<{inactiveLettersColor.ToHexaString(true)}>{commandInputValue.Substring(_activeLetters)}";
	}
}