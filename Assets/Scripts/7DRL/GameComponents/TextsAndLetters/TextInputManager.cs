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

		private static string currentInput { get; set; }
		private static bool   listening    { get; set; }

		private static void StartListening() => instance.StartCoroutine(ListenInput());
		private static void StopListening() => listening = false;
		private static void ClearInput() => currentInput = string.Empty;

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

		public static IEnumerator ListenUntilResult<E>(IReadOnlyCollection<E> options, LetterReserve payer, ListenUntilResultCallbacks<E> callbacks = null) where E : ITextInputResult {
			ClearInput();
			StartListening();
			var lastValidInput = string.Empty;
			E lastValidOption = default;
			while (string.IsNullOrEmpty(lastValidInput) || lastValidInput != lastValidOption?.textInput) {
				if (currentInput != lastValidInput) {
					var newInputIsValid = false;
					E newRecognizedOption;

					if (lastValidInput.Length > currentInput.Length) {
						if (!lastValidOption.isFreeInput) {
							payer.Add(lastValidInput.Substring(currentInput.Length));
							lastValidInput.Substring(currentInput.Length).ForEach(c => callbacks?.letterReimbursed?.Invoke(c));
						}
						newInputIsValid = true;
						newRecognizedOption = lastValidOption;
					}
					else if (options.TryFirst(t => t.textInput.StartsWith(currentInput), out newRecognizedOption)) {
						if (newRecognizedOption.isFreeInput) {
							newInputIsValid = true;
						}
						else if (payer.TryRemove(currentInput.Substring(lastValidInput.Length))) {
							newInputIsValid = true;
							currentInput.Substring(lastValidInput.Length).ForEach(c => callbacks?.letterPaid?.Invoke(c));
						}
						else {
							currentInput.Substring(lastValidInput.Length).ForEach(c => callbacks?.missingLetter?.Invoke(c));
						}
					}
					if (newInputIsValid) {
						lastValidInput = currentInput;
						lastValidOption = newRecognizedOption;
						AudioManager.Sfx.PlayRandom("input.build");
						callbacks?.inputChanged?.Invoke(lastValidInput, lastValidOption);
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
			callbacks?.completed?.Invoke(lastValidOption);
		}

		public class ListenUntilResultCallbacks<E> {
			public Action<string, E> inputChanged     { get; set; }
			public Action<E>         completed        { get; set; }
			public Action<char>      letterPaid       { get; set; }
			public Action<char>      letterReimbursed { get; set; }
			public Action<char>      missingLetter    { get; set; }
		}
	}
}