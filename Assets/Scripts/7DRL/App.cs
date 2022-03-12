using System.Collections;
using System.Linq;
using _7DRL.GameComponents.Characters;
using _7DRL.Games;
using _7DRL.Input.Controls;
using _7DRL.MiscConstants;
using _7DRL.Scenes;
using _7DRL.Scenes.Combat;
using _7DRL.Scenes.GameOver;
using _7DRL.Scenes.Intro;
using _7DRL.Scenes.Map;
using _7DRL.Ui;
using UnityEngine;
using Utils.Audio;
using Utils.Extensions;
using Utils.Libraries;
using Utils.Loading;

namespace _7DRL {
	public class App : MonoBehaviour {
		[SerializeField] protected IntroSceneManager    _introManager;
		[SerializeField] protected MapSceneManager      _mapManager;
		[SerializeField] protected CombatSceneManager   _combatManager;
		[SerializeField] protected GameOverSceneManager _gameOverManager;
		[SerializeField] protected LoadingCanvas        _loadingScreen;

		private SceneManager currentScene { get; set; }

		public void Start() {
			_loadingScreen.SetVisible();
			_combatManager.Disable();
			_mapManager.Disable();
			_gameOverManager.Disable();
			GameEvents.onEncounterAtPlayerPosition.AddListenerOnce(HandleEncounterAtPlayerPosition);
			GameEvents.onEncounterDefeated.AddListenerOnce(HandleEncounterDefeated);
			GameEvents.onPlayerFledBattle.AddListenerOnce(HandlePlayerFled);
			GameEvents.onPlayerLost.AddListenerOnce(HandlePlayerLost);
			GameEvents.onPlayerDead.AddListenerOnce(HandlePlayerDead);
			GameEvents.onGameOverEnded.AddListenerOnce(HandleGameOverEnded);
			GameEvents.onNewGameIntroEnded.AddListenerOnce(HandleNewGameIntroEnded);
			GameEvents.onQuitGame.AddListenerOnce(HandleQuitGame);
			Inputs.controls.Main.VolumeUp.AddPerformListenerOnce(t => AudioManager.masterVolume += .1f);
			Inputs.controls.Main.VolumeDown.AddPerformListenerOnce(t => AudioManager.masterVolume -= .1f);
			Inputs.controls.Enable();

			StartCoroutine(Init());
		}

		private void HandleQuitGame() => _loadingScreen.Show(() => {
			if (Application.isEditor && Application.isPlaying) UnityEditor.EditorApplication.isPlaying = false;
			else Application.Quit();
		});

		private void HandleNewGameIntroEnded() => StartCoroutine(StartNewGame());

		private IEnumerator Init() {
			_loadingScreen.SetProgress(0);
			Colors.LoadLibrary(Resources.LoadAll<ColorLibrary>("Libraries").FirstOrDefault());
			AudioClips.LoadLibrary(Resources.LoadAll<AudioClipLibrary>("Libraries").FirstOrDefault());
			Sprites.LoadLibrary(Resources.LoadAll<SpriteLibrary>("Libraries").FirstOrDefault());
			_loadingScreen.SetProgress(.5f);
			yield return null;
			yield return StartCoroutine(Memory.Load());
			_loadingScreen.SetProgress(1);
			StartCoroutine(ShowIntro());
		}

		private IEnumerator MoveToMap() {
			_loadingScreen.SetProgress(0);
			yield return StartCoroutine(_loadingScreen.DoFadeIn());
			if (currentScene) currentScene.Disable();
			currentScene = _mapManager;
			_mapManager.Enable();
			yield return StartCoroutine(_loadingScreen.DoFadeOut());
			_mapManager.Continue();
		}

		private void HandleEncounterDefeated(Encounter defeatedEncounter) => StartCoroutine(defeatedEncounter.level == Encounter.Level.Boss ? ShowGameOver(GameOverType.Victory) : MoveToMap());
		private void HandleEncounterAtPlayerPosition(Encounter encounter) => StartCoroutine(MoveToCombat(encounter));
		private void HandlePlayerFled() => StartCoroutine(MoveToMap());

		private IEnumerator MoveToCombat(Encounter encounter) {
			_loadingScreen.SetProgress(0);
			yield return StartCoroutine(_loadingScreen.DoFadeIn());
			if (currentScene) currentScene.Disable();
			currentScene = _combatManager;
			_combatManager.Init(encounter);
			if (currentScene) currentScene.Enable();
			yield return new WaitForSeconds(.3f);
			yield return StartCoroutine(_loadingScreen.DoFadeOut());
			_combatManager.StartBattle();
		}

		private void HandlePlayerLost() => StartCoroutine(ShowGameOver(GameOverType.Lost));
		private void HandlePlayerDead() => StartCoroutine(ShowGameOver(GameOverType.Dead));

		private IEnumerator ShowGameOver(GameOverType type) {
			_loadingScreen.SetProgress(0);
			yield return StartCoroutine(_loadingScreen.DoFadeIn());
			if (currentScene) currentScene.Disable();
			currentScene = _gameOverManager;
			if (currentScene) currentScene.Enable();
			yield return null;
			_gameOverManager.Show(type);
			yield return new WaitForSeconds(.3f);
			yield return StartCoroutine(_loadingScreen.DoFadeOut());
		}

		private void HandleGameOverEnded() => StartCoroutine(ShowIntro());

		private IEnumerator ShowIntro() {
			_loadingScreen.SetProgress(0);
			yield return StartCoroutine(_loadingScreen.DoFadeIn());
			if (currentScene) currentScene.Disable();
			currentScene = _introManager;
			if (currentScene) currentScene.Enable();
			_introManager.Prepare();
			yield return null;
			yield return new WaitForSeconds(.3f);
			yield return StartCoroutine(_loadingScreen.DoFadeOut());
			yield return new WaitForSeconds(1f);
			_introManager.StartMainMenu();
		}

		private IEnumerator StartNewGame() {
			yield return StartCoroutine(_loadingScreen.DoFadeIn());
			yield return StartCoroutine(GameFactory.CreateGame(t => Game.instance = t));
			CommonGameUi.Init(Game.instance.playerCharacter);
			yield return null;
			_mapManager.Init(Game.instance);
			_loadingScreen.SetProgress(1);
			yield return null;
			yield return StartCoroutine(MoveToMap());
		}
	}
}