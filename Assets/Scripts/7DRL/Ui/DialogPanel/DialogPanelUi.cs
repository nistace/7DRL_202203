using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils.Extensions;
using Utils.Ui;

public class DialogPanelUi : MonoBehaviour {
	[SerializeField] protected GameObject               _panel;
	[SerializeField] protected TMP_Text                 _titleText;
	[SerializeField] protected ForceRebuildLayoutGroups _linesContainer;
	[SerializeField] private   DialogOptionLineUi       _optionLine;
	[SerializeField] private   DialogTextLineUi         _textLine;
	[SerializeField] protected Transform                _topOfTheBox;

	public Transform topOfTheBox => _topOfTheBox;

	private Dictionary<string, DialogOptionLineUi> optionsPerCommand { get; } = new Dictionary<string, DialogOptionLineUi>();

	public void Clean() {
		optionsPerCommand.Clear();
		_linesContainer.transform.ClearChildren();
	}

	public void AddText(string text) => Instantiate(_textLine, _linesContainer.transform).Set(text);

	public void AddOption(string command, string text, bool charged) {
		var line = Instantiate(_optionLine, _linesContainer.transform);
		line.Set(command, text, charged);
		optionsPerCommand.Add(command, line);
	}

	public void SetCommandProgress(string command, int progress) {
		optionsPerCommand.ForEach(t => t.Value.activeLetters = t.Key == command ? progress : 0);
	}

	public void Show(string title) {
		_panel.SetActive(true);
		_titleText.text = title;
		_linesContainer.Play(5);
	}

	public void Hide() {
		_panel.SetActive(false);
	}

	public bool TryGetCommandTransform(string command, out Transform commandTransform) {
		commandTransform = default;
		if (!optionsPerCommand.ContainsKey(command)) return false;
		commandTransform = optionsPerCommand[command].transform;
		return true;
	}
}