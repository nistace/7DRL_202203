using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _7DRL.GameComponents.Characters;
using _7DRL.GameComponents.TextAndLetters;
using _7DRL.Games;
using _7DRL.MiscConstants;
using _7DRL.Scenes.Combat.Ui;
using _7DRL.Ui;
using UnityEngine;
using Utils.Audio;
using Utils.Extensions;
using Random = UnityEngine.Random;

namespace _7DRL.Scenes.Combat {
	public class CombatSceneManager : SceneManager {
		[SerializeField] protected CombatUi        _ui;
		[SerializeField] protected CombatScene     _combatScene;
		[SerializeField] protected CombatCharacter _combatCharacterPrefab;
		[SerializeField] protected Vector3         _cameraOffset = Vector3.back;

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

			//TODO show cursor over currently playing character
			//TODO manage player dead
			//TODO level up player
			//TODO create different visuals for the characters
		}

		private IEnumerator DoCurrentTurn() {
			CommonGameUi.SetToggleKnownCommandsEnabled(true);
			CommonGameUi.knownCommands.SetValidCommands(CommandType.Location.Combat);
			switch (battleStep) {
				case BattleStep.Player: return DoPlayerTurn();
				case BattleStep.Foes: return DoFoesTurn();
				default: throw new ArgumentOutOfRangeException();
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
			Game.instance.playerCharacter.ResetForBattle();
			playerEscaped = false;

			if (combatCharacters.ContainsKey(Game.instance.playerCharacter)) combatCharacters.Remove(Game.instance.playerCharacter);
			combatCharacters.Values.ForEach(t => Destroy(t.gameObject));
			combatCharacters.Clear();
			combatCharacters.Add(Game.instance.playerCharacter, _combatScene.player);
			_combatScene.player.Init(0, false);
			var farthestFoePosition = Vector3.zero;
			for (var foeIndex = 0; foeIndex < encounter.foes.Length; foeIndex++) {
				encounter.foes[foeIndex].ResetForBattle();
				var foePosition = _combatScene.GetFoePosition(foeIndex);
				var foeCharacter = Instantiate(_combatCharacterPrefab, Vector3.zero, Quaternion.identity, foePosition);
				foeCharacter.transform.localPosition = Vector3.zero;
				foeCharacter.Init(encounter.foes[foeIndex].spriteSeed, true);
				combatCharacters.Add(encounter.foes[foeIndex], foeCharacter);
				farthestFoePosition = foePosition.position;
			}

			CameraUtils.main.transform.position = (farthestFoePosition + _combatScene.playerPosition.position) / 2 + _cameraOffset;

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
			var commands = Game.instance.playerCharacter.knownCommands.Where(t => t.type.IsUsable(CommandType.Location.Combat)).ToDictionary(t => t.inputName, t => t);
			Command command = null;
			Game.instance.playerCharacter.SetCurrentCommand(string.Empty, CommandType.Location.Combat);
			yield return StartCoroutine(TextInputManager.ListenUntilResult(commands, OnPlayerInputChanged, t => command = t));
			yield return new WaitForSeconds(1);
			var target = encounter.foes.First(t => !t.dead);
			yield return StartCoroutine(ResolveCommand(Game.instance.playerCharacter, target, command));
			Game.instance.playerCharacter.SetCurrentCommand(string.Empty, CommandType.Location.Combat);
		}

		private static void OnPlayerInputChanged(string input, Command preferredCommand) => Game.instance.playerCharacter.SetCurrentCommand(input, CommandType.Location.Combat);

		private IEnumerator ResolveCommand(CharacterBase source, CharacterBase target, Command command) {
			yield return StartCoroutine(GetCommandAction(command)(source, target, command));
		}

		private Func<CharacterBase, CharacterBase, Command, IEnumerator> GetCommandAction(Command command) {
			if (command.type == Memory.CommandTypes.attack) return ResolveAttackCommand;
			if (command.type == Memory.CommandTypes.defense) return ResolveDefenseCommand;
			if (command.type == Memory.CommandTypes.heal) return ResolveHealCommand;
			if (command.type == Memory.CommandTypes.dodge) return ResolveDodgeCommand;
			if (command.type == Memory.CommandTypes.escape) return ResolveEscapeCommand;
			if (command.type == Memory.CommandTypes.rest) return ResolveRestCommand;
			throw new ArgumentException($"Command type {command.type.name} is not handled");
		}

		private IEnumerator ResolveAttackCommand(CharacterBase source, CharacterBase target, Command command) {
			combatCharacters[source].PlayAttack();
			var dodged = target.RollDodge();
			if (dodged) {
				combatCharacters[target].PlayDodge();
				AudioManager.Sfx.PlayRandom("combat.attack.dodge");
			}
			else {
				combatCharacters[target].PlayDamaged();
				AudioManager.Sfx.PlayRandom("combat.attack.hit");
			}
			yield return new WaitForSeconds(.5f);
			if (!dodged) {
				target.Damage(source.GetCommandPower(command));
				if (target.dead) {
					AudioManager.Sfx.PlayRandom("combat.attack.dead");
					combatCharacters[target].SetDead(true);

					yield return new WaitForSeconds(1);
					if (source == Game.instance.playerCharacter) {
						var targetPosition = CameraUtils.main.WorldToScreenPoint(combatCharacters[target].transform.position);
						yield return StartCoroutine(EarnLetters(target.name, targetPosition));
						foreach (var targetKnownCommand in target.knownCommands) {
							yield return StartCoroutine(EarnLetters(targetKnownCommand.inputName, targetPosition));
						}
					}
				}
			}
			yield return new WaitForSeconds(.5f);
			if (dodged) target.ResetChanceToDodge();
		}

		private IEnumerator ResolveRestCommand(CharacterBase source, CharacterBase target, Command command) =>
			ResolveRestCommand(command, CameraUtils.main.WorldToScreenPoint(combatCharacters[source].headTransform.position));

		private IEnumerator ResolveEscapeCommand(CharacterBase source, CharacterBase target, Command command) {
			yield return new WaitForSeconds(.5f);
			source.AddChanceToEscape(source.GetCommandPower(command));
			var escaped = source.RollEscape();
			combatCharacters[source].PlayEscape(escaped);
			AudioManager.Sfx.PlayRandom($"combat.escape.{(escaped ? "success" : "fail")}");
			yield return new WaitForSeconds(.5f);
			playerEscaped = escaped;
			yield return new WaitForSeconds(.5f);
		}

		private IEnumerator ResolveDodgeCommand(CharacterBase source, CharacterBase target, Command command) {
			combatCharacters[source].PlayDodge();
			AudioManager.Sfx.PlayRandom("combat.attack.dodge");
			yield return new WaitForSeconds(.5f);
			source.AddChanceToDodge(source.GetCommandPower(command));
			yield return new WaitForSeconds(.5f);
		}

		private IEnumerator ResolveHealCommand(CharacterBase source, CharacterBase target, Command command) {
			combatCharacters[source].PlayHeal();
			AudioManager.Sfx.PlayRandom("combat.heal");
			yield return new WaitForSeconds(.5f);
			source.Heal(source.GetCommandPower(command));
			yield return new WaitForSeconds(.5f);
		}

		private IEnumerator ResolveDefenseCommand(CharacterBase source, CharacterBase target, Command command) {
			combatCharacters[source].PlayDefense();
			AudioManager.Sfx.PlayRandom("combat.armor");
			yield return new WaitForSeconds(.5f);
			source.AddArmor(source.GetCommandPower(command));
			yield return new WaitForSeconds(.5f);
		}
	}
}