using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _7DRL.Data;
using _7DRL.Games;
using _7DRL.MiscConstants;
using _7DRL.Scenes.Combat.Ui;
using _7DRL.TextInput;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _7DRL.Scenes.Combat {
	public class CombatSceneManager : SceneManager {
		[SerializeField] protected CombatUi        _ui;
		[SerializeField] protected CombatScene     _combatScene;
		[SerializeField] protected CombatCharacter _combatCharacterPrefab;

		private Encounter                                  encounter        { get; set; }
		private Dictionary<CharacterBase, CombatCharacter> combatCharacters { get; } = new Dictionary<CharacterBase, CombatCharacter>();

		[ContextMenu("Init Random")]
		public void InitRandom() => Init(GameFactory.GenerateEncounter(Vector2Int.zero, Encounter.Level.Weak, new[] { Random.Range(1, 199) },
			Game.instance.defaultLetterPowers.Select((t, i) => (t, i)).ToDictionary(t => (char)('A' + t.i), t => t.t)));

		public void Init(Encounter encounter) {
			this.encounter = encounter;
			InitBattle();
		}

		[ContextMenu("StartBattle")] public void StartBattle() => StartCoroutine(DoBattle());

		private IEnumerator DoBattle() {
			while (!IsBattleOver()) {
				yield return StartCoroutine(DoPlayerTurn());
				foreach (var foe in encounter.foes) {
					if (!foe.dead) {
						yield return StartCoroutine(DoFoeTurn(foe));
					}
				}
			}

			if (Game.instance.playerCharacter.dead) GameEvents.onPlayerDead.Invoke();
			else {
				GameEvents.onEncounterDefeated.Invoke(encounter);
				GameEvents.onBattleEnded.Invoke();
			}
		}

		private bool IsBattleOver() {
			if (Game.instance.playerCharacter.dead) return true;
			if (encounter.foes.All(t => t.dead)) return true;
			return false;
		}

		private void InitBattle() {
			combatCharacters.Clear();
			combatCharacters.Add(Game.instance.playerCharacter, _combatScene.player);
			_combatScene.player.Init(true);
			for (var foeIndex = 0; foeIndex < encounter.foes.Length; foeIndex++) {
				var foeCharacter = Instantiate(_combatCharacterPrefab, Vector3.zero, Quaternion.identity, _combatScene.GetFoePosition(foeIndex));
				foeCharacter.transform.localPosition = Vector3.zero;
				foeCharacter.Init(false);
				combatCharacters.Add(encounter.foes[foeIndex], foeCharacter);
			}

			_ui.InitBars(Game.instance.playerCharacter, encounter.foes);
		}

		private IEnumerator DoFoeTurn(Foe foe) {
			yield return new WaitForSeconds(1);
			foe.ProgressCurrentCommand();
			if (foe.IsCurrentCommandReady()) {
				yield return new WaitForSeconds(1);

				var power = Game.instance.playerCharacter.GetCommandPower(foe.currentCommand);
				yield return StartCoroutine(ResolveCommand(foe, Game.instance.playerCharacter, foe.currentCommand, power));

				foe.PrepareNextCommand();
			}
			yield return new WaitForSeconds(1);
		}

		private IEnumerator DoPlayerTurn() {
			TextInputManager.ClearInput();
			TextInputManager.StartListening();
			var lastInput = string.Empty;
			Game.instance.playerCharacter.SetCurrentCommand(string.Empty, CommandType.Location.Combat);
			Command command = null;
			var hasCommand = false;
			while (!hasCommand) {
				if (TextInputManager.currentInput != lastInput) {
					lastInput = TextInputManager.currentInput;
					Game.instance.playerCharacter.SetCurrentCommand(lastInput, CommandType.Location.Combat);
					hasCommand = Game.instance.playerCharacter.TryGetCurrentCommandIfComplete(out command);
				}
				yield return null;
			}
			TextInputManager.StopListening();
			yield return new WaitForSeconds(1);

			var target = encounter.foes.First(t => !t.dead);
			var power = Game.instance.playerCharacter.GetCommandPower(command);
			yield return StartCoroutine(ResolveCommand(Game.instance.playerCharacter, target, command, power));

			Game.instance.playerCharacter.SetCurrentCommand(string.Empty, CommandType.Location.Combat);
		}

		private IEnumerator ResolveCommand(CharacterBase source, CharacterBase target, Command command, int power) {
			yield return StartCoroutine(GetCommandAction(command)(source, target, power));
		}

		private Func<CharacterBase, CharacterBase, int, IEnumerator> GetCommandAction(Command command) {
			if (command.type == Memory.CommandTypes.attack) return ResolveAttackCommand;
			if (command.type == Memory.CommandTypes.defense) return ResolveDefenseCommand;
			Debug.LogError($"Command type {command.type.name} is not handled");
			return ResolveDefaultCommand;
		}

		private static IEnumerator ResolveDefaultCommand(CharacterBase source, CharacterBase target, int power) {
			yield return null;
		}

		private IEnumerator ResolveAttackCommand(CharacterBase source, CharacterBase target, int power) {
			combatCharacters[source].PlayAttack();
			combatCharacters[target].PlayDamaged();
			yield return new WaitForSeconds(.5f);
			target.Damage(power);
			if (target.dead) {
				combatCharacters[target].SetDead(true);
				yield return new WaitForSeconds(1);
			}
			yield return new WaitForSeconds(.5f);
		}

		private IEnumerator ResolveDefenseCommand(CharacterBase source, CharacterBase target, int power) {
			combatCharacters[source].PlayDefense();
			yield return new WaitForSeconds(.5f);
			source.AddArmor(power);
			yield return new WaitForSeconds(.5f);
		}
	}
}