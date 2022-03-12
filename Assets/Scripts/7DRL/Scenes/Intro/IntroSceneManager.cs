using System;
using System.Collections;
using System.Collections.Generic;
using _7DRL.Data.IntroScript;
using _7DRL.GameComponents.TextAndLetters;
using _7DRL.Games;
using _7DRL.MiscConstants;
using _7DRL.Ui;
using UnityEngine;
using Utils.Libraries;

namespace _7DRL.Scenes.Intro {
	public class IntroSceneManager : SceneManager {
		[SerializeField] protected IntroScene _scene;

		private bool firstTime { get; set; } = true;

		public void Show() { }

		public void Prepare() {
			CommonGameUi.SetToggleKnownCommandsEnabled(false);
			CommonGameUi.SetPlayerLetterReserveVisible(false);
			_scene.sprite = Sprites.Of("intro.title");
			_scene.color = Color.white;
		}

		public void StartMainMenu() => StartCoroutine(ShowMainMenu());

		private IEnumerator ShowMainMenu() {
			yield return null;

			var commands = new List<IntroCommand> { new IntroCommand("start", "a new game.") };
#if !UNITY_WEBPLAYER
			commands.Add(new IntroCommand("quit", "to desktop."));
#endif
			IntroCommand command = null;
			yield return StartCoroutine(ShowDialogAndWaitForCommand("Main Menu", firstTime ? "First time? Use your keyboard to type \"Start\", as displayed below in orange, to start a new game." : null,
				commands, t => command = t));
			firstTime = false;
			if (command.command == "start") {
				CommonGameUi.dialogPanel.Hide();
				yield return StartCoroutine(StartIntro());
			}
			else if (command.command == "quit") {
				GameEvents.onQuitGame.Invoke();
			}
		}

		private IEnumerator StartIntro() {
			CommonGameUi.dialogPanel.Clean();
			CommonGameUi.dialogPanel.Show("The Gate");
			var skipped = false;
			IntroCommand selectedLineCommand = null;
			for (var scriptLineIndex = 0; !skipped && scriptLineIndex < Memory.introScriptLines.Count; ++scriptLineIndex) {
				var scriptLine = Memory.introScriptLines[scriptLineIndex];
				CommonGameUi.dialogPanel.Clean();
				yield return StartCoroutine(_scene.ChangeSprite(Sprites.Of(scriptLine.spriteKey)));
				yield return StartCoroutine(ShowDialogAndWaitForCommand("The Gate", scriptLine.text, scriptLine.commands, t => selectedLineCommand = t));

				yield return new WaitForSeconds(.5f);
				skipped = selectedLineCommand == scriptLine.skipCommand;
			}
			CommonGameUi.dialogPanel.Hide();
			GameEvents.onNewGameIntroEnded.Invoke();
		}

		private IEnumerator ShowDialogAndWaitForCommand(string title, string infoText, IReadOnlyCollection<IntroCommand> commands, Action<IntroCommand> callback) {
			CommonGameUi.dialogPanel.Clean();
			if (!string.IsNullOrEmpty(infoText)) CommonGameUi.dialogPanel.AddText(infoText);
			foreach (var command in commands) {
				CommonGameUi.dialogPanel.AddOption(command.textInput, command.endOfSentence, false);
			}
			yield return null;
			CommonGameUi.dialogPanel.Show(title);
			var listenInputCallbacks = new TextInputManager.ListenUntilResultCallbacks<IntroCommand> { inputChanged = HandleDialogInputChanged, completed = callback };
			yield return StartCoroutine(TextInputManager.ListenUntilResult(commands, null, listenInputCallbacks));
		}
	}
}