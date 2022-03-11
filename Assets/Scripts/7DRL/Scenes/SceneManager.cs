using System.Collections;
using System.Linq;
using _7DRL.GameComponents.Interactions;
using _7DRL.GameComponents.TextAndLetters;
using _7DRL.GameComponents.TextAndLetters.Ui;
using _7DRL.Games;
using _7DRL.Ui;
using UnityEngine;
using Utils.Audio;
using Utils.Extensions;

namespace _7DRL.Scenes {
	public abstract class SceneManager : MonoBehaviour {
		private static TextEffectUi.ShowLineAndMoveLettersCallbacks earnWordLettersCallbacks { get; } = new TextEffectUi.ShowLineAndMoveLettersCallbacks {
			letterEffect = { onLetterArrived = HandleEarnedLetterArrived }, onLetterDetached = HandleWordLetterDetached
		};

		private static TextEffectUi.CreateLetterEffectCallbacks earnLetterCallbacks { get; } = new TextEffectUi.CreateLetterEffectCallbacks { onLetterArrived = HandleEarnedLetterArrived };

		public void Enable() {
			gameObject.SetActive(true);
		}

		public void Disable() {
			StopAllCoroutines();
			gameObject.SetActive(false);
		}

		protected static IEnumerator EarnLettersFromDialog(string line) => EarnLetters(line, CommonGameUi.dialogPanel.topOfTheBox.position);

		protected static IEnumerator EarnLetters(string line, Vector2 originScreenPosition) => TextEffectUi.ShowLineAndMoveLettersToReserve(TextUtils.ToInputNameWithOtherCharacters(line),
			originScreenPosition, CommonGameUi.playerLetterReserve, earnWordLettersCallbacks);

		protected static IEnumerator EarnLetter(char letter, Vector2 origin) => TextEffectUi.CreateLetterEffect(letter, origin, CommonGameUi.playerLetterReserve, earnLetterCallbacks);

		private static void HandleEarnedLetterArrived(char letter) {
			AudioManager.Sfx.Play("bonus.letter");
			CommonGameUi.playerLetterReserve.FlashLine(letter, LetterReserveUi.FlashType.Add);
			Game.instance.playerCharacter.letterReserve.Add(letter);
		}

		private static void HandleWordLetterDetached(char letter) => AudioManager.Sfx.Play("bonus.letter");

		protected IEnumerator ResolveRestCommand(Command command, Vector2 lettersOrigin) {
			yield return new WaitForSeconds(.5f);
			Coroutine lastCoroutine = null;
			var possibleLetters = TextUtils.allLetters.Except(command.textInput).ToArray();
			foreach (var letter in Game.instance.playerCharacter.GetCommandPower(command).CreateArray(t => possibleLetters.Random())) {
				AudioManager.Sfx.Play("bonus.letter");
				lastCoroutine = StartCoroutine(EarnLetter(letter, lettersOrigin));
				yield return new WaitForSeconds(.1f);
			}
			if (lastCoroutine != null) yield return lastCoroutine;
		}

		protected static void HandleDialogInputChanged(string input, InteractionOption preferred) => CommonGameUi.dialogPanel.SetCommandProgress(preferred.inputValue, input.Length);
		protected static void HandleLetterPaid(char letter) => CommonGameUi.playerLetterReserve.FlashLine(letter, LetterReserveUi.FlashType.Remove);
		protected static void HandleLetterReimbursed(char letter) => CommonGameUi.playerLetterReserve.FlashLine(letter, LetterReserveUi.FlashType.Add);
		protected static void HandleLetterMissing(char letter) => CommonGameUi.playerLetterReserve.FlashLine(letter, LetterReserveUi.FlashType.Missing);

		protected static IEnumerator ResolveSkillInteraction(Command command) {
			yield return new WaitForSeconds(.5f);
			Game.instance.playerCharacter.LearnCommand(command);
			AudioManager.Sfx.Play("bonus.misc");
			yield return new WaitForSeconds(.5f);
		}

		protected static IEnumerator ResolvePowerInteraction(char letter) {
			yield return new WaitForSeconds(.5f);
			Game.instance.playerCharacter.EnhanceLetterPower(letter, 2);
			AudioManager.Sfx.Play("bonus.misc");
			yield return new WaitForSeconds(.5f);
		}

		protected static IEnumerator ResolveMaxHealthInteraction() {
			yield return new WaitForSeconds(.5f);
			Game.instance.playerCharacter.IncreaseMaxHealth();
			Game.instance.playerCharacter.HealToMaxHealth();
			AudioManager.Sfx.Play("bonus.misc");
			yield return new WaitForSeconds(.5f);
		}

		protected static IEnumerator ResolveSkipInteraction() {
			yield return new WaitForSeconds(.5f);
		}
	}
}