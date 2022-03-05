using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using Utils.Events;

namespace _7DRL.TextInput {
	public class TextInputManager : MonoBehaviour {
		private static TextInputManager instance { get; set; }

		private void Awake() {
			instance = this;
			ClearInput();
		}

		public static  string currentInput { get; private set; }
		private static bool   listening    { get; set; }

		public static StringEvent onCurrentInputChanged { get; } = new StringEvent();

		public static void StartListening() => instance.StartCoroutine(ListenInput());

		public static void StopListening() => listening = false;
		public static void ClearInput() => currentInput = string.Empty;

		private static IEnumerator ListenInput() {
			listening = true;
			while (listening) {
				if (currentInput.Length > 0 && Input.GetKeyDown(KeyCode.Backspace)) {
					currentInput = currentInput.Substring(0, currentInput.Length - 1);
					onCurrentInputChanged.Invoke(currentInput);
				}
				else if (!string.IsNullOrEmpty(Input.inputString) && Regex.IsMatch(Input.inputString, "^[a-zA-Z]$")) {
					currentInput += Input.inputString.ToUpper();
					onCurrentInputChanged.Invoke(currentInput);
				}
				yield return null;
			}
		}
	}
}