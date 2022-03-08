using System;
using _7DRL.Data;
using _7DRL.Input.Controls;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Extensions;

namespace _7DRL.Ui {
	public class CommonGameUi : MonoBehaviour {
		private static CommonGameUi instance { get; set; }

		[SerializeField] protected LetterReserveUi _playerLetterReserve;
		[SerializeField] protected KnownCommandsUi _knownCommands;

		public static LetterReserveUi playerLetterReserve => instance?._playerLetterReserve;
		public static KnownCommandsUi knownCommands       => instance?._knownCommands;

		private void Awake() {
			instance = this;
		}

		private void Start() {
			knownCommands.gameObject.SetActive(false);
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
	}
}