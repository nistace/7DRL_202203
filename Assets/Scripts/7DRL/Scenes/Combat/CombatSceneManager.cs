using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _7DRL.GameComponents.Characters;
using _7DRL.GameComponents.Interactions;
using _7DRL.GameComponents.TextAndLetters;
using _7DRL.Games;
using _7DRL.MiscConstants;
using _7DRL.Scenes.Combat.Ui;
using _7DRL.Ui;
using UnityEngine;
using Utils.Audio;
using Utils.Extensions;
using Utils.Libraries;

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

		public void Init(Encounter encounter) {
			this.encounter = encounter;
			InitBattle();
		}

		public void StartBattle() => StartCoroutine(DoBattle());

		private IEnumerator DoBattle() {
			_ui.SetVisible(true);

			while (!IsBattleComplete()) {
				yield return StartCoroutine(DoCurrentTurn());
				battleStep = (BattleStep)((int)(battleStep + 1) % EnumUtils.SizeOf<BattleStep>());
			}

			yield return StartCoroutine(_ui.cursor.Change(Color.white.With(a: 0), _ui.cursor.position));
			_ui.SetVisible(false);

			yield return StartCoroutine(ResolveBattle());

			//TODO manage player dead
			//TODO manage no valid command
		}

		private IEnumerator ResolveBattle() {
			if (playerEscaped) {
				GameEvents.onPlayerFledBattle.Invoke();
				yield break;
			}
			if (Game.instance.playerCharacter.dead) {
				GameEvents.onPlayerDead.Invoke();
				yield break;
			}
			if (encounter.foes.All(t => t.dead)) {
				yield return StartCoroutine(ResolveLevelUpDialog());
				GameEvents.onEncounterDefeated.Invoke(encounter);
				yield break;
			}
			throw new ArgumentException("Cannot resolve the battle when no possible outcome has been reached");
		}

		private bool IsBattleComplete() {
			if (playerEscaped) return true;
			if (Game.instance.playerCharacter.dead) return true;
			if (encounter.foes.All(t => t.dead)) return true;
			return false;
		}

		private IEnumerator ResolveLevelUpDialog() {
			Game.instance.playerCharacter.SetCurrentCommand(string.Empty, CommandType.Location.Combat);
			CommonGameUi.dialogPanel.Clean();
			CommonGameUi.dialogPanel.AddText("You successfully defeated this encounter, and leveled up. How would you like to use what you learned?");

			foreach (var option in encounter.interactionOptions) {
				CommonGameUi.dialogPanel.AddOption(option.inputValue, option.endOfSentence, option.charged);
			}
			yield return null;
			CommonGameUi.dialogPanel.Show("Level up");

			InteractionOption inputInteraction = null;
			var listenInputCallbacks = new TextInputManager.ListenUntilResultCallbacks<InteractionOption> {
				completed = t => inputInteraction = t,
				inputChanged = HandleDialogInputChanged,
				letterPaid = HandleLetterPaid,
				letterReimbursed = HandleLetterReimbursed,
				missingLetter = HandleLetterMissing
			};
			yield return StartCoroutine(TextInputManager.ListenUntilResult(encounter.interactionOptions, Game.instance.playerCharacter.letterReserve, listenInputCallbacks));
			yield return StartCoroutine(ResolveLevelUp(inputInteraction));
			Game.instance.playerCharacter.LevelUp();

			CommonGameUi.dialogPanel.Hide();
		}

		private IEnumerator ResolveLevelUp(InteractionOption option) => GetResolveLevelUpFunc(option.type)();

		private Func<IEnumerator> GetResolveLevelUpFunc(InteractionType type) {
			switch (type) {
				case InteractionType.Skip: return ResolveSkipInteraction;
				case InteractionType.Skill: return () => ResolveSkillInteraction(encounter.skillInteractionCommand);
				case InteractionType.Power: return () => ResolvePowerInteraction(encounter.powerInteractionLetter);
				case InteractionType.MaxHealth: return ResolveMaxHealthInteraction;
				case InteractionType.Chest:
				case InteractionType.Book:
				case InteractionType.Portal:
				case InteractionType.Fountain:
				default: throw new ArgumentException();
			}
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

		private void InitBattle() {
			Game.instance.playerCharacter.ResetForBattle();
			playerEscaped = false;

			if (combatCharacters.ContainsKey(Game.instance.playerCharacter)) combatCharacters.Remove(Game.instance.playerCharacter);
			combatCharacters.Values.ForEach(t => Destroy(t.gameObject));
			combatCharacters.Clear();
			combatCharacters.Add(Game.instance.playerCharacter, _combatScene.player);
			_combatScene.player.Init(0, false, false);
			_combatScene.RelocatePlayerAndFoes();
			var farthestFoePosition = Vector3.zero;
			for (var foeIndex = 0; foeIndex < encounter.foes.Length; foeIndex++) {
				encounter.foes[foeIndex].ResetForBattle();
				var foePosition = _combatScene.GetFoePosition(foeIndex);
				var foeCharacter = Instantiate(_combatCharacterPrefab, Vector3.zero, Quaternion.identity, foePosition);
				foeCharacter.transform.localPosition = Vector3.zero;
				foeCharacter.Init(encounter.foes[foeIndex].spriteSeed, true, encounter.foes[foeIndex].dead);
				combatCharacters.Add(encounter.foes[foeIndex], foeCharacter);
				farthestFoePosition = foePosition.position;
			}

			var middlePoint = (farthestFoePosition + _combatScene.playerPosition.position) / 2;
			CameraUtils.main.transform.position = middlePoint + _cameraOffset;
			_ui.cursor.Jump(Color.white.With(a: 0), CameraUtils.main.WorldToScreenPoint(middlePoint));

			_ui.InitBars(Game.instance.playerCharacter, encounter.foes);
			battleStep = BattleStep.Player;
		}

		private IEnumerator DoFoesTurn() {
			foreach (var foe in encounter.foes) {
				if (foe.dead) {
					combatCharacters[foe].SetDead(true);
					continue;
				}
				yield return StartCoroutine(_ui.cursor.Change(Colors.Of("combat.cursor.foe"), CameraUtils.main.WorldToScreenPoint(combatCharacters[foe].transform.position)));
				yield return new WaitForSeconds(.5f);
				foe.ProgressCurrentCommand();
				AudioManager.Sfx.PlayRandom("input.build");
				if (foe.IsCurrentCommandReady()) {
					yield return new WaitForSeconds(.5f);
					yield return StartCoroutine(ResolveCommand(foe, Game.instance.playerCharacter, foe.currentCommand));
					foe.PrepareNextCommand();
				}
				yield return new WaitForSeconds(1f);
			}
		}

		private IEnumerator DoPlayerTurn() {
			var commands = Game.instance.playerCharacter.knownCommands.Where(t => t.type.IsUsable(CommandType.Location.Combat)).ToArray();
			Command command = null;
			Game.instance.playerCharacter.SetCurrentCommand(string.Empty, CommandType.Location.Combat);
			StartCoroutine(_ui.cursor.Change(Colors.Of("combat.cursor.player"), CameraUtils.main.WorldToScreenPoint(_combatScene.playerPosition.position)));
			var listenInputCallbacks = new TextInputManager.ListenUntilResultCallbacks<Command>() {
				completed = t => command = t,
				inputChanged = (input, recognizedCommand) => Game.instance.playerCharacter.SetCurrentCommand(input, CommandType.Location.Combat),
				letterPaid = HandleLetterPaid,
				letterReimbursed = HandleLetterReimbursed,
				missingLetter = HandleLetterMissing
			};
			yield return StartCoroutine(TextInputManager.ListenUntilResult(commands, Game.instance.playerCharacter.letterReserve, listenInputCallbacks));
			yield return new WaitForSeconds(.5f);
			var target = encounter.foes.First(t => !t.dead);
			yield return StartCoroutine(ResolveCommand(Game.instance.playerCharacter, target, command));
			Game.instance.playerCharacter.SetCurrentCommand(string.Empty, CommandType.Location.Combat);
		}

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
							yield return StartCoroutine(EarnLetters(targetKnownCommand.textInput, targetPosition));
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