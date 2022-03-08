using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _7DRL.Data;
using _7DRL.Games;
using _7DRL.MiscConstants;
using _7DRL.Scenes.Combat.Ui;
using _7DRL.TextInput;
using _7DRL.Ui;
using UnityEngine;
using Utils.Extensions;
using Random = UnityEngine.Random;

namespace _7DRL.Scenes.Combat {
	public class CombatSceneManager : SceneManager {
		[SerializeField] protected CombatUi        _ui;
		[SerializeField] protected CombatScene     _combatScene;
		[SerializeField] protected CombatCharacter _combatCharacterPrefab;
		[SerializeField] protected Vector3         _cameraPosition = Vector3.zero;

		private enum BattleStep {
			Player = 0,
			Foes   = 1
		}

		private Encounter                                  encounter        { get; set; }
		private Dictionary<CharacterBase, CombatCharacter> combatCharacters { get; } = new Dictionary<CharacterBase, CombatCharacter>();
		private bool                                       playerEscaped    { get; set; }
		private BattleStep                                 battleStep       { get; set; }

		[ContextMenu("Init Random")]
		public void InitRandom() => Init(GameFactory.GenerateEncounter(Vector2Int.zero, Encounter.Level.Weak, new[] { Random.Range(1, 199) },
			Game.instance.defaultLetterPowers.Select((t, i) => (t, i)).ToDictionary(t => (char)('A' + t.i), t => t.t)));

		public void Init(Encounter encounter) {
			this.encounter = encounter;
			InitBattle();
		}

		[ContextMenu("StartBattle")] public void StartBattle() => StartCoroutine(DoBattle());

		private IEnumerator DoBattle() {
			while (!TryInterruptBattle()) {
				yield return StartCoroutine(DoCurrentTurn());
				battleStep = (BattleStep)((int)(battleStep + 1) % EnumUtils.SizeOf<BattleStep>());
			}
		}

		private IEnumerator DoCurrentTurn() {
			CommonGameUi.SetToggleKnownCommandsEnabled(true);
			CommonGameUi.knownCommands.SetValidCommands(CommandType.Location.Combat);
			switch (battleStep) {
				case BattleStep.Player: return DoPlayerTurn();
				case BattleStep.Foes: return DoFoesTurn();
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private bool TryInterruptBattle() {
			if (playerEscaped) {
				GameEvents.onPlayerFledBattle.Invoke();
				return true;
			}
			if (Game.instance.playerCharacter.dead) {
				GameEvents.onPlayerDead.Invoke();
				return true;
			}
			if (encounter.foes.All(t => t.dead)) {
				GameEvents.onEncounterDefeated.Invoke(encounter);
				return true;
			}
			return false;
		}

		private void InitBattle() {
			CameraUtils.main.transform.position = _cameraPosition;
			Game.instance.playerCharacter.ResetForBattle();
			playerEscaped = false;

			if (combatCharacters.ContainsKey(Game.instance.playerCharacter)) combatCharacters.Remove(Game.instance.playerCharacter);
			combatCharacters.Values.ForEach(t => Destroy(t.gameObject));
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
			battleStep = BattleStep.Player;
		}

		private IEnumerator DoFoesTurn() {
			foreach (var foe in encounter.foes.Where(t => !t.dead)) {
				yield return new WaitForSeconds(1);
				foe.ProgressCurrentCommand();
				if (foe.IsCurrentCommandReady()) {
					yield return new WaitForSeconds(1);

					yield return StartCoroutine(ResolveCommand(foe, Game.instance.playerCharacter, foe.currentCommand));

					foe.PrepareNextCommand();
				}
				yield return new WaitForSeconds(1);
			}
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
			yield return StartCoroutine(ResolveCommand(Game.instance.playerCharacter, target, command));

			Game.instance.playerCharacter.SetCurrentCommand(string.Empty, CommandType.Location.Combat);
		}

		private IEnumerator ResolveCommand(CharacterBase source, CharacterBase target, Command command) {
			yield return StartCoroutine(GetCommandAction(command)(source, target, source.GetCommandPower(command)));
		}

		private Func<CharacterBase, CharacterBase, int, IEnumerator> GetCommandAction(Command command) {
			if (command.type == Memory.CommandTypes.attack) return ResolveAttackCommand;
			if (command.type == Memory.CommandTypes.defense) return ResolveDefenseCommand;
			if (command.type == Memory.CommandTypes.heal) return ResolveHealCommand;
			if (command.type == Memory.CommandTypes.dodge) return ResolveDodgeCommand;
			if (command.type == Memory.CommandTypes.escape) return ResolveEscapeCommand;
			Debug.LogError($"Command type {command.type.name} is not handled");
			return ResolveDefaultCommand;
		}

		private static IEnumerator ResolveDefaultCommand(CharacterBase source, CharacterBase target, int power) {
			yield return null;
		}

		private IEnumerator ResolveAttackCommand(CharacterBase source, CharacterBase target, int power) {
			combatCharacters[source].PlayAttack();
			var dodged = target.RollDodge();
			if (dodged) combatCharacters[target].PlayDodge();
			else combatCharacters[target].PlayDamaged();
			yield return new WaitForSeconds(.5f);
			if (!dodged) {
				target.Damage(power);
				if (target.dead) {
					combatCharacters[target].SetDead(true);
					yield return new WaitForSeconds(1);
				}
			}
			yield return new WaitForSeconds(.5f);
			if (dodged) target.ResetChanceToDodge();
		}

		private IEnumerator ResolveEscapeCommand(CharacterBase source, CharacterBase target, int power) {
			yield return new WaitForSeconds(.5f);
			source.AddChanceToEscape(power);
			var escaped = source.RollEscape();
			combatCharacters[source].PlayEscape(escaped);
			yield return new WaitForSeconds(.5f);
			playerEscaped = escaped;
			yield return new WaitForSeconds(.5f);
		}

		private IEnumerator ResolveDodgeCommand(CharacterBase source, CharacterBase target, int power) {
			combatCharacters[source].PlayDodge();
			yield return new WaitForSeconds(.5f);
			source.AddChanceToDodge(power);
			yield return new WaitForSeconds(.5f);
		}

		private IEnumerator ResolveHealCommand(CharacterBase source, CharacterBase target, int power) {
			combatCharacters[source].PlayHeal();
			yield return new WaitForSeconds(.5f);
			source.Heal(power);
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