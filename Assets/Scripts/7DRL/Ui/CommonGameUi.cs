using _7DRL.GameComponents.Characters;
using _7DRL.GameComponents.TextAndLetters.Ui;
using _7DRL.Input.Controls;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Extensions;

namespace _7DRL.Ui {
	public class CommonGameUi : MonoBehaviour {
		private static CommonGameUi instance { get; set; }

		[SerializeField] protected LetterReserveUi _playerLetterReserve;
		[SerializeField] protected KnownCommandsUi _knownCommands;
		[SerializeField] protected DialogPanelUi   _dialogPanel;

		public static LetterReserveUi playerLetterReserve => instance?._playerLetterReserve;
		public static KnownCommandsUi knownCommands       => instance?._knownCommands;
		public static DialogPanelUi   dialogPanel         => instance?._dialogPanel;

		private void Awake() {
			instance = this;
		}

		private void Start() {
			knownCommands.gameObject.SetActive(false);
			dialogPanel.Hide();
		}

		public static void SetToggleKnownCommandsEnabled(bool enabled) {
			if (!enabled) knownCommands.gameObject.SetActive(false);
			Inputs.controls.Main.KnownCommands.SetAnyListenerOnce(SetKnownCommandsVisible, enabled);
		}

		private static void SetKnownCommandsVisible(InputAction.CallbackContext obj) => knownCommands.gameObject.SetActive(obj.performed);

		public static void Init(PlayerCharacter player) {
			playerLetterReserve.Set(player.letterReserve);
			knownCommands.Set(player);
		}

		public static void SetPlayerLetterReserveVisible(bool visible) => playerLetterReserve.gameObject.SetActive(visible);
	}
}