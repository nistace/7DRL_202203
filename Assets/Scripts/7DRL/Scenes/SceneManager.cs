using System.Collections;
using System.Linq;
using _7DRL.GameComponents.TextAndLetters;
using _7DRL.GameComponents.TextAndLetters.Ui;
using _7DRL.Games;
using _7DRL.Ui;
using UnityEngine;
using Utils.Extensions;

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

		protected IEnumerator ResolveRestCommand(Command command, Vector2 lettersOrigin) {
			yield return new WaitForSeconds(.5f);
			Coroutine lastCoroutine = null;
			var possibleLetters = TextUtils.allLetters.Except(command.inputName).ToArray();
			foreach (var letter in Game.instance.playerCharacter.GetCommandPower(command).CreateArray(t => possibleLetters.Random())) {
				lastCoroutine = StartCoroutine(EarnLetter(letter, lettersOrigin));
				yield return new WaitForSeconds(.1f);
			}
			if (lastCoroutine != null) yield return lastCoroutine;
		}
	}
}