using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Utils.Audio;
using Utils.Extensions;

namespace _7DRL.GameComponents.TextAndLetters {
	public class TextInputManager : MonoBehaviour {
		private static TextInputManager instance { get; set; }

		private void Awake() {
			instance = this;
			ClearInput();
		}

		public static  string currentInput { get; private set; }
		private static bool   listening    { get; set; }

		public static void StartListening() => instance.StartCoroutine(ListenInput());

		public static void StopListening() => listening = false;
		public static void ClearInput() => currentInput = string.Empty;

		private static IEnumerator ListenInput() {
			listening = true;
			while (listening) {
				if (currentInput.Length > 0 && UnityEngine.Input.GetKeyDown(KeyCode.Backspace)) {
					currentInput = currentInput.Substring(0, currentInput.Length - 1);
				}
				else if (!string.IsNullOrEmpty(UnityEngine.Input.inputString) && Regex.IsMatch(UnityEngine.Input.inputString, "^[a-zA-Z]$")) {
					currentInput += UnityEngine.Input.inputString.ToUpper();
				}
				yield return null;
			}
		}

		public static IEnumerator ListenUntilResult<E>(IReadOnlyDictionary<string, E> options, Action<string, E> inputChangedCallback, Action<E> completedCallback) {
			ClearInput();
			StartListening();
			var lastValidInput = string.Empty;
			var preferredOption = string.Empty;
			while (string.IsNullOrEmpty(lastValidInput) || lastValidInput != preferredOption) {
				if (currentInput != lastValidInput) {
					if (options.Keys.TryFirst(t => t.StartsWith(currentInput), out preferredOption)) {
						lastValidInput = currentInput;
						inputChangedCallback?.Invoke(lastValidInput, options[preferredOption]);
						AudioManager.Sfx.PlayRandom("input.build");
					}
					else {
						currentInput = lastValidInput;
						AudioManager.Sfx.PlayRandom("input.invalid");
					}
				}
				yield return null;
			}
			StopListening();
			AudioManager.Sfx.PlayRandom("input.complete");
			completedCallback?.Invoke(options[preferredOption]);
		}
	}
}