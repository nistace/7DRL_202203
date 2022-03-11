using _7DRL.GameComponents.Characters;
using _7DRL.Ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;
using Utils.Ui;

namespace _7DRL.Scenes.Combat.Ui {
	public class CombatCharacterBarUi : MonoBehaviourUi {
		[SerializeField] protected Transform        _followWorldTransform;
		[SerializeField] protected Vector3          _offsetWithWorldTransform;
		[SerializeField] protected TMP_Text         _characterNameText;
		[SerializeField] protected Image            _healthFillBar;
		[SerializeField] protected Image            _armorFillBar;
		[SerializeField] protected TMP_Text         _healthText;
		[SerializeField] protected TMP_Text         _armorText;
		[SerializeField] protected TMP_Text         _dodgeText;
		[SerializeField] protected TMP_Text         _escapeText;
		[SerializeField] protected CommandTrackerUi _commandTracker;

		private CharacterBase character { get; set; }

		public void Set(CharacterBase character) {
			this.character = character;
			_characterNameText.text = character.completeName;
			character.onHealthChanged.AddListenerOnce(RefreshHealthAndArmor);
			character.onHealthChanged.AddListenerOnce(RefreshDodgeChance);
			character.onHealthChanged.AddListenerOnce(RefreshEscapeChance);
			character.onArmorChanged.AddListenerOnce(RefreshHealthAndArmor);
			character.onDodgeChanceChanged.AddListenerOnce(RefreshDodgeChance);
			character.onEscapeChanceChanged.AddListenerOnce(RefreshEscapeChance);
			_commandTracker.Set(character);
			RefreshHealthAndArmor();
			RefreshDodgeChance();
			RefreshEscapeChance();
		}

		private void RefreshHealthAndArmor() {
			var total = (float)Mathf.Max(character.health + character.armor, character.maxHealth);
			_armorFillBar.fillAmount = (character.health + character.armor) / total;
			_healthFillBar.fillAmount = character.health / total;
			_healthText.text = $"{character.health}/{character.maxHealth}";
			_armorText.text = character.armor == 0 ? string.Empty : $"+{character.armor}";
		}

		private void RefreshDodgeChance() => _dodgeText.text = character.dead || character.dodge == 0 ? string.Empty : $"Dodge: {character.dodge}";
		private void RefreshEscapeChance() => _escapeText.text = character.dead || character.escape == 0 ? string.Empty : $"Escape: {character.escape}";

		private void Update() => transform.MoveOverWorldTransform(_followWorldTransform, _offsetWithWorldTransform);
	}
}