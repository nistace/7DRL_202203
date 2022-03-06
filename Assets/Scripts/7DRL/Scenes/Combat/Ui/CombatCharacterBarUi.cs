using _7DRL.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;
using Utils.Ui;

public class CombatCharacterBarUi : MonoBehaviourUi {
	[SerializeField] protected Transform        _followWorldTransform;
	[SerializeField] protected Vector3          _offsetWithWorldTransform;
	[SerializeField] protected TMP_Text         _characterNameText;
	[SerializeField] protected Image            _healthFillBar;
	[SerializeField] protected Image            _armorFillBar;
	[SerializeField] protected TMP_Text         _healthText;
	[SerializeField] protected CommandTrackerUi _commandTracker;

	private CharacterBase character { get; set; }

	public void Set(CharacterBase character) {
		this.character = character;
		_characterNameText.text = character.completeName;
		character.onHealthOrArmorChanged.AddListenerOnce(RefreshHealthBar);
		_commandTracker.Set(character);
		RefreshHealthBar();
	}

	private void RefreshHealthBar() {
		var total = (float)Mathf.Max(character.health + character.armor, character.maxHealth);
		_armorFillBar.fillAmount = (character.health + character.armor) / total;
		_healthFillBar.fillAmount = character.health / total;
		_healthText.text = $"{character.health}/{character.maxHealth}";
		if (character.armor > 0) _healthText.text += $" <{_armorFillBar.color.ToHexaString(true)}>+{character.armor}";
	}

	private void Update() => transform.MoveOverWorldTransform(_followWorldTransform, _offsetWithWorldTransform);
}