using System.Collections;
using System.Linq;
using _7DRL.Data;
using _7DRL.Games;
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
			GameEvents.onEncounterAtPlayerPosition.AddListenerOnce(StartBattle);
			GameEvents.onEncounterDefeated.AddListenerOnce(HandleBattleEnded);

			StartCoroutine(Init());
		}

		private IEnumerator Init() {
			_loadingScreen.SetProgress(0);
			Colors.LoadLibrary(Resources.LoadAll<ColorLibrary>("Libraries").FirstOrDefault());
			_loadingScreen.SetProgress(.2f);
			yield return null;
			yield return StartCoroutine(Memory.Load());
			_loadingScreen.SetProgress(.5f);
			yield return StartCoroutine(GameFactory.CreateGame(t => Game.instance = t));
			CommonGameUi.playerLetterReserve.Set(Game.instance.playerCharacter.letterReserve);
			_loadingScreen.SetProgress(.8f);
			yield return null;
			_currentGame = Game.instance;
			_mapManager.Init(Game.instance);
			_loadingScreen.SetProgress(1);
			yield return null;
			yield return StartCoroutine(MoveToMap());
			_loadingScreen.Hide();
		}

		private IEnumerator MoveToMap() {
			yield return StartCoroutine(ChangeScene(_mapManager));
			_mapManager.Continue();
		}

		private void HandleBattleEnded(Encounter defeatedEncounter) {
			if (defeatedEncounter.level == Encounter.Level.Boss) {
				Debug.Log("You win !");
			}
			else {
				StartCoroutine(MoveToMap());
			}
		}

		private void StartBattle(Encounter encounter) => StartCoroutine(MoveToCombat(encounter));

		private IEnumerator MoveToCombat(Encounter encounter) {
			yield return StartCoroutine(ChangeScene(_combatManager));
			_combatManager.Init(encounter);
			yield return new WaitForSeconds(1);
			_combatManager.StartBattle();
		}

		private IEnumerator ChangeScene(SceneManager newScene) {
			_loadingScreen.SetProgress(0);
			yield return StartCoroutine(_loadingScreen.DoFadeIn());
			if (currentScene) currentScene.Disable();
			currentScene = newScene;
			if (currentScene) currentScene.Enable();
			yield return StartCoroutine(_loadingScreen.DoFadeOut());
		}
	}
}