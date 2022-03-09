using System.Collections;
using System.Linq;
using _7DRL.GameComponents.Characters;
using _7DRL.Games;
using _7DRL.Input.Controls;
using _7DRL.MiscConstants;
using _7DRL.Scenes;
using _7DRL.Scenes.Combat;
using _7DRL.Scenes.Map;
using _7DRL.Ui;
using UnityEngine;
using Utils.Extensions;
using Utils.Libraries;
using Utils.Loading;

namespace _7DRL {
	public class App : MonoBehaviour {
		[SerializeField] protected Game               _currentGame;
		[SerializeField] protected MapSceneManager    _mapManager;
		[SerializeField] protected CombatSceneManager _combatManager;
		[SerializeField] protected LoadingCanvas      _loadingScreen;

		private SceneManager currentScene { get; set; }

		public void Start() {
			_loadingScreen.SetVisible();
			_combatManager.Disable();
			_mapManager.Disable();
			GameEvents.onEncounterAtPlayerPosition.AddListenerOnce(HandleEncounterAtPlayerPosition);
			GameEvents.onEncounterDefeated.AddListenerOnce(HandleEncounterDefeated);
			GameEvents.onPlayerFledBattle.AddListenerOnce(HandlePlayerFled);
			StartCoroutine(Init());
		}

		private IEnumerator Init() {
			_loadingScreen.SetProgress(0);
			Colors.LoadLibrary(Resources.LoadAll<ColorLibrary>("Libraries").FirstOrDefault());
			AudioClips.LoadLibrary(Resources.LoadAll<AudioClipLibrary>("Libraries").FirstOrDefault());
			Sprites.LoadLibrary(Resources.LoadAll<SpriteLibrary>("Libraries").FirstOrDefault());
			_loadingScreen.SetProgress(.2f);
			yield return null;
			yield return StartCoroutine(Memory.Load());
			_loadingScreen.SetProgress(.5f);
			yield return StartCoroutine(GameFactory.CreateGame(t => Game.instance = t));
			CommonGameUi.Init(Game.instance.playerCharacter);
			_loadingScreen.SetProgress(.8f);
			yield return null;
			_currentGame = Game.instance;
			_mapManager.Init(Game.instance);
			_loadingScreen.SetProgress(1);
			yield return null;
			yield return StartCoroutine(MoveToMap());
			Inputs.controls.Enable();
			_loadingScreen.Hide();
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

		private void HandleEncounterDefeated(Encounter defeatedEncounter) {
			if (defeatedEncounter.level == Encounter.Level.Boss) {
				Debug.Log("You win !");
			}
			else {
				StartCoroutine(MoveToMap());
			}
		}

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
	}
}