using _7DRL.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace _7DRL.Scenes.Map {
	public class MapCharacterUi : MonoBehaviour {
		[SerializeField] protected TMP_Text         _characterText;
		[SerializeField] protected Image            _healthBar;
		[SerializeField] protected TMP_Text         _healthText;
		[SerializeField] protected CommandTrackerUi _commandTracker;

		private PlayerCharacter  playerCharacter { get; set; }
		public  CommandTrackerUi commandTracker  => _commandTracker;

		public void Set(PlayerCharacter playerCharacter) {
			this.playerCharacter = playerCharacter;
			_commandTracker.Set(playerCharacter);
			playerCharacter.onHealthChanged.AddListenerOnce(Refresh);
			Refresh();
		}

		private void Refresh() {
			_characterText.text = playerCharacter.completeName;
			_healthBar.fillAmount = Mathf.Clamp01((float)playerCharacter.health / playerCharacter.maxHealth);
			_healthText.text = $"{playerCharacter.health}/{playerCharacter.maxHealth}";
		}
	}
}