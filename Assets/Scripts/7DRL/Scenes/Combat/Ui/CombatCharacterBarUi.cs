using _7DRL.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;
using Utils.Ui;

public class CombatCharacterBarUi : MonoBehaviourUi {
	[SerializeField] protected Transform _followWorldTransform;
	[SerializeField] protected Vector3   _offsetWithWorldTransform;
	[SerializeField] protected TMP_Text  _characterNameText;
	[SerializeField] protected Image     _healthFillBar;
	[SerializeField] protected Image     _armorFillBar;
	[SerializeField] protected TMP_Text  _healthText;
	[SerializeField] protected TMP_Text  _commandText;
	[SerializeField] protected TMP_Text  _commandDescriptionText;
	[SerializeField] protected Color     _activeLetterColor   = Color.white;
	[SerializeField] protected Color     _inactiveLetterColor = Color.black;

	private CharacterBase character { get; set; }

	public void Set(CharacterBase character) {
		this.character = character;
		_characterNameText.text = character.completeName;
		character.onHealthOrArmorChanged.AddListenerOnce(RefreshHealthBar);
		character.onCurrentCommandChanged.AddListenerOnce(RefreshCurrentCommand);
		RefreshCurrentCommand();
		RefreshHealthBar();
	}

	private void RefreshCurrentCommand() {
		if (character.dead) {
			_commandText.text = string.Empty;
			_commandDescriptionText.text = string.Empty;
			return;
		}
		_commandText.text = $"<{_activeLetterColor.ToHexaString(true)}>{character.currentCommandLetters}<{_inactiveLetterColor.ToHexaString(true)}>{character.currentCommandMissingLetters}";
		_commandDescriptionText.text = character.TryGetCurrentCommand(out var buildingCommand) ? buildingCommand.type.GetDescription(character.GetCommandPower(buildingCommand)) : string.Empty;
	}

	private void RefreshHealthBar() {
		var total = (float)Mathf.Max(character.health + character.armor, character.maxHealth);
		_armorFillBar.fillAmount = (character.health + character.armor) / total;
		_healthFillBar.fillAmount = character.health / total;
		_healthText.text = $"{character.health}/{character.maxHealth}";
		if (character.armor > 0) _healthText.text += $" <{_armorFillBar.color.ToHexaString(true)}>+{character.armor}";
		if (character.dead) RefreshCurrentCommand();
	}

	private void Update() => transform.MoveOverWorldTransform(_followWorldTransform, _offsetWithWorldTransform);
}