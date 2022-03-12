using System;
using System.Collections;
using System.Linq;
using _7DRL.GameComponents.Interactions;
using _7DRL.GameComponents.TextAndLetters;
using _7DRL.Games;
using _7DRL.MiscConstants;
using _7DRL.Ui;
using UnityEngine;
using Utils.Extensions;

namespace _7DRL.Scenes.GameOver {
	public class GameOverSceneManager : SceneManager {
		[SerializeField] protected GameOverCharacter _gameOverCharacter;
		[SerializeField] protected Vector3           _cameraPosition = Vector3.back;

		public void Show(GameOverType type) => StartCoroutine(PlayGameOverScreen(type));

		private IEnumerator PlayGameOverScreen(GameOverType type) {
			CommonGameUi.SetToggleKnownCommandsEnabled(false);
			CameraUtils.main.transform.position = _cameraPosition;
			yield return StartCoroutine(GetResolutionFunc(type)());
		}

		private Func<IEnumerator> GetResolutionFunc(GameOverType type) {
			switch (type) {
				case GameOverType.Victory: return ResolveVictory;
				case GameOverType.Dead: return ResolveDead;
				case GameOverType.Lost: return ResolveLost;
				default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		private IEnumerator ResolveLost() {
			_gameOverCharacter.PlayLost();
			return ShowFinalDialogBox(InteractionType.GameOverLost, "Game Over", "You can't play any more command and will stay in this dungeon forever, unable to achieve your objective...");
		}

		private IEnumerator ResolveDead() {
			_gameOverCharacter.PlayDead();
			return ShowFinalDialogBox(InteractionType.GameOverDead, "Game Over", "What a sad ending, bleeding to death...");
		}

		private IEnumerator ResolveVictory() {
			_gameOverCharacter.PlayVictory();
			return ShowFinalDialogBox(InteractionType.GameOverVictory, "Victory", "This a triumph! Huge Success!");
		}

		private IEnumerator ShowFinalDialogBox(InteractionType type, string title, string text) {
			var interaction = Memory.interactionOptions[type].Where(t => t.charged && Game.instance.playerCharacter.letterReserve.CanPay(t.textInput)).RandomOrDefault()
									?? Memory.interactionOptions[type].Where(t => !t.charged).Random();

			CommonGameUi.dialogPanel.Clean();
			CommonGameUi.dialogPanel.AddText(text);
			CommonGameUi.dialogPanel.AddOption(interaction.inputValue, interaction.endOfSentence, interaction.charged);
			yield return null;
			CommonGameUi.dialogPanel.Show(title);
			var listenInputCallbacks = new TextInputManager.ListenUntilResultCallbacks<InteractionOption> {
				inputChanged = HandleDialogInputChanged, letterPaid = HandleLetterPaid, letterReimbursed = HandleLetterReimbursed, missingLetter = HandleLetterMissing
			};
			yield return StartCoroutine(TextInputManager.ListenUntilResult(new[] { interaction }, Game.instance.playerCharacter.letterReserve, listenInputCallbacks));

			yield return new WaitForSeconds(.5f);
			GameEvents.onGameOverEnded.Invoke();
			CommonGameUi.dialogPanel.Hide();
		}
	}
}