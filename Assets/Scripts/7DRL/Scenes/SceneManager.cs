using System.Collections;
using _7DRL.GameComponents.TextAndLetters;
using _7DRL.GameComponents.TextAndLetters.Ui;
using _7DRL.Games;
using _7DRL.Ui;
using UnityEngine;

namespace _7DRL.Scenes {
	public abstract class SceneManager : MonoBehaviour {
		public void Enable() {
			gameObject.SetActive(true);
		}

		public void Disable() {
			StopAllCoroutines();
			gameObject.SetActive(false);
		}

		protected static IEnumerator EarnLettersFromDialog(string line) => EarnLetters(line, CommonGameUi.dialogPanel.topOfTheBox.position);

		protected static IEnumerator EarnLetters(string line, Vector2 originScreenPosition) => TextEffectUi.ShowLineAndMoveLettersToReserve(TextUtils.ToInputNameWithOtherCharacters(line),
			originScreenPosition, CommonGameUi.playerLetterReserve, HandleEarnedLetterArrived);

		protected static IEnumerator EarnLetter(char letter, Vector2 origin) => TextEffectUi.CreateLetterEffect(letter, origin, CommonGameUi.playerLetterReserve, HandleEarnedLetterArrived);

		private static void HandleEarnedLetterArrived(char letter) => Game.instance.playerCharacter.letterReserve.Add(letter);
	}
}