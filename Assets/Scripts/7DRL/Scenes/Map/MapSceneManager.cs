using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _7DRL.GameComponents;
using _7DRL.GameComponents.Characters;
using _7DRL.GameComponents.Dungeons;
using _7DRL.GameComponents.Dungeons.Misc;
using _7DRL.GameComponents.Interactions;
using _7DRL.GameComponents.TextAndLetters;
using _7DRL.Games;
using _7DRL.MiscConstants;
using _7DRL.Ui;
using UnityEngine;
using Utils.Audio;
using Utils.Extensions;
using Utils.Libraries;

namespace _7DRL.Scenes.Map {
	public class MapSceneManager : SceneManager {
		[SerializeField] protected Follow         _cameraFollow;
		[SerializeField] protected SpriteRenderer _pathPrefab;
		[SerializeField] protected Transform      _pathContainer;
		[SerializeField] protected Vector2        _pathChunkSize;
		[SerializeField] protected MapToken       _tokenPrefab;
		[SerializeField] protected Transform      _tokensContainer;
		[SerializeField] protected float          _tokensSpeed = 1;
		[SerializeField] protected MapUi          _ui;

		private SpriteRenderer[,]                     pathChunks             { get; set; }
		private Dictionary<IDungeonCrawler, MapToken> tokens                 { get; } = new Dictionary<IDungeonCrawler, MapToken>();
		private Vector2Int                            playerPreviousPosition { get; set; }

		private void OnEnable() {
			_cameraFollow.enabled = true;
			_cameraFollow.Jump();
		}

		private void OnDisable() {
			if (_cameraFollow) _cameraFollow.enabled = false;
		}

		public void Init(Game game) {
			InitMap(game.dungeonMap);
			InitTokens(game);
			DiscoverAround(game.playerCharacter.dungeonPosition);

			_cameraFollow.target = tokens[game.playerCharacter].transform;
			RevealPosition(game.playerCharacter.dungeonPosition);
			_cameraFollow.Jump();

			_ui.Init(game.playerCharacter);
			RefreshWindRose();

			GameEvents.onEncounterDefeated.AddListenerOnce(HandleEncounterDefeated);
			GameEvents.onPlayerFledBattle.AddListenerOnce(HandlePlayerFled);
		}

		private void HandlePlayerFled() {
			(playerPreviousPosition, Game.instance.playerCharacter.dungeonPosition) = (Game.instance.playerCharacter.dungeonPosition, playerPreviousPosition);
			Game.instance.ChangeTurnStep(Game.TurnStep.Player);
			TeleportToken(Game.instance.playerCharacter);
			RefreshWindRose();
		}

		private void HandleEncounterDefeated(Encounter defeatedEncounter) {
			if (!tokens.ContainsKey(defeatedEncounter)) return;
			Game.instance.dungeonMap.RemoveEncounter(defeatedEncounter);
			Destroy(tokens[defeatedEncounter].gameObject);
			tokens.Remove(defeatedEncounter);
		}

		private void RefreshWindRose() {
			_ui.windRose.SetDirectionEnabled(DungeonMap.Direction.North, CanExecute(Memory.CommandTypes.moveNorth));
			_ui.windRose.SetDirectionEnabled(DungeonMap.Direction.West, CanExecute(Memory.CommandTypes.moveWest));
			_ui.windRose.SetDirectionEnabled(DungeonMap.Direction.East, CanExecute(Memory.CommandTypes.moveEast));
			_ui.windRose.SetDirectionEnabled(DungeonMap.Direction.South, CanExecute(Memory.CommandTypes.moveSouth));
		}

		private void InitTokens(Game game) {
			_tokensContainer.ClearChildren();
			tokens.Clear();
			CreateToken(game.playerCharacter, true, Sprites.Of("token.player"), Colors.Of("token.player"), 3);

			foreach (var dungeonMisc in game.dungeonMap.miscRoomContents) {
				CreateToken(dungeonMisc, false, dungeonMisc.tokenSprite, Colors.Of("token.misc"), 1);
			}

			foreach (var encounter in game.dungeonMap.encounters) {
				CreateToken(encounter, false, encounter.tokenSprite, Colors.Of("token.foe"), 2);
			}
		}

		private void CreateToken(IDungeonCrawler dungeonCrawler, bool visible, Sprite sprite, Color color, int priority) {
			tokens.Add(dungeonCrawler, Instantiate(_tokenPrefab, _tokensContainer));
			tokens[dungeonCrawler].priority = priority;
			tokens[dungeonCrawler].sprite = sprite;
			tokens[dungeonCrawler].color = color;
			tokens[dungeonCrawler].visible = visible;
			tokens[dungeonCrawler].transform.position = GridToWorldPosition(dungeonCrawler.dungeonPosition);
		}

		private void InitMap(DungeonMap map) {
			_pathContainer.ClearChildren();
			pathChunks = new SpriteRenderer[map.width, map.height];
			for (var x = 0; x < map.width; ++x)
			for (var y = 0; y < map.height; ++y) {
				if (map[x, y] == DungeonMap.noDirection) continue;
				pathChunks[x, y] = Instantiate(_pathPrefab, GridToWorldPosition(x, y), Quaternion.identity, _pathContainer);
				pathChunks[x, y].sprite = Sprites.Of($"dungeon.{(map.IsRoom(x, y) ? "room" : "path")}.{(int)map[x, y]}");
				pathChunks[x, y].enabled = false;
			}
		}

		private Vector2 GridToWorldPosition(Vector2Int position) => GridToWorldPosition(position.x, position.y);
		private Vector2 GridToWorldPosition(int x, int y) => new Vector2(x * _pathChunkSize.x, y * _pathChunkSize.y);

		public void Continue() => StartCoroutine(DoContinue());

		private IEnumerator DoContinue() {
			CommonGameUi.SetToggleKnownCommandsEnabled(true);
			CommonGameUi.knownCommands.SetValidCommands(CommandType.Location.Map);
			while (!TryInterruptScene()) {
				yield return StartCoroutine(DoCurrentTurnStep());
				Game.instance.NextTurnStep();
			}

			//TODO manage no valid command
		}

		private IEnumerator DoCurrentTurnStep() {
			switch (Game.instance.turnStep) {
				case Game.TurnStep.Player: return DoPlayerTurnStep();
				case Game.TurnStep.SolveMisc: return DoSolveMiscTurnStep();
				case Game.TurnStep.Foe: return DoFoeTurnStep();
				default: throw new ArgumentOutOfRangeException();
			}
		}

		private static bool TryInterruptScene() {
			if (Game.instance.dungeonMap.TryGetEncounter(Game.instance.playerCharacter.dungeonPosition, out var encounter)) {
				GameEvents.onEncounterAtPlayerPosition.Invoke(encounter);
				return true;
			}
			return false;
		}

		private IEnumerator DoPlayerTurnStep() {
			var commands = Game.instance.playerCharacter.knownCommands.Where(t => t.type.IsUsable(CommandType.Location.Map) && CanExecute(t)).ToArray();
			Game.instance.playerCharacter.SetCurrentCommand(string.Empty, CommandType.Location.Map);

			Command command = null;
			var listenInputCallbacks = new TextInputManager.ListenUntilResultCallbacks<Command> {
				completed = t => command = t,
				inputChanged = (input, recognizedCommand) => Game.instance.playerCharacter.SetCurrentCommand(input, CommandType.Location.Map),
				letterPaid = HandleLetterPaid,
				letterReimbursed = HandleLetterReimbursed,
				missingLetter = HandleLetterMissing
			};

			yield return StartCoroutine(TextInputManager.ListenUntilResult(commands, Game.instance.playerCharacter.letterReserve, listenInputCallbacks));
			yield return new WaitForSeconds(1);
			yield return StartCoroutine(ResolvePlayerCommand(command));

			Game.instance.playerCharacter.SetCurrentCommand(string.Empty, CommandType.Location.Map);
		}

		private static bool CanExecute(Command command) => CanExecute(command.type);

		private static bool CanExecute(CommandType type) {
			if (type == Memory.CommandTypes.moveNorth) return Game.instance.dungeonMap.IsMovementAllowed(Game.instance.playerCharacter.dungeonPosition, DungeonMap.Direction.North, true);
			if (type == Memory.CommandTypes.moveSouth) return Game.instance.dungeonMap.IsMovementAllowed(Game.instance.playerCharacter.dungeonPosition, DungeonMap.Direction.South, true);
			if (type == Memory.CommandTypes.moveWest) return Game.instance.dungeonMap.IsMovementAllowed(Game.instance.playerCharacter.dungeonPosition, DungeonMap.Direction.West, true);
			if (type == Memory.CommandTypes.moveEast) return Game.instance.dungeonMap.IsMovementAllowed(Game.instance.playerCharacter.dungeonPosition, DungeonMap.Direction.East, true);
			return true;
		}

		private IEnumerator DoFoeTurnStep() {
			for (var wait = 0f; wait < .5f; wait += Time.deltaTime) {
				_ui.gameTurn.progress = wait.Remap(0, .5f, 0, .25f);
				yield return null;
			}

			foreach (var encounter in Game.instance.dungeonMap.encounters.ToArray()) {
				if (encounter.level != Encounter.Level.Boss) {
					var possibleMovements = Game.instance.dungeonMap.GetPossibleMovements(encounter.dungeonPosition, false).ToArray();
					if (possibleMovements.Length > 0) {
						Game.instance.dungeonMap.MoveEncounter(encounter, possibleMovements.Random());
						StartCoroutine(MoveToken(encounter));
					}
				}
			}

			for (var wait = 0f; wait < .5f; wait += Time.deltaTime) {
				_ui.gameTurn.progress = wait.Remap(0, .5f, .25f, .5f);
				yield return null;
			}

			foreach (var encounter in Game.instance.dungeonMap.encounters) {
				tokens[encounter].visible = pathChunks[encounter.dungeonPosition.x, encounter.dungeonPosition.y].enabled;
			}

			for (var wait = 0f; wait < 1; wait += Time.deltaTime) {
				_ui.gameTurn.progress = wait.Remap(0, 1, .5f, 1);
				yield return null;
			}
		}

		private void TeleportToken(IDungeonCrawler source) => tokens[source].position = GridToWorldPosition(source.dungeonPosition);

		private IEnumerator MoveToken(IDungeonCrawler source) {
			var worldPosition = GridToWorldPosition(source.dungeonPosition);
			var token = tokens[source];

			while (token.position != worldPosition) {
				token.position = Vector3.MoveTowards(token.position, worldPosition, _tokensSpeed * Time.deltaTime);
				yield return null;
			}
		}

		private void DiscoverAround(Vector2Int gridPosition) {
			pathChunks[gridPosition.x, gridPosition.y].enabled = true;
			foreach (var direction in EnumUtils.Values<DungeonMap.Direction>()) {
				if (Game.instance.dungeonMap.IsMovementAllowed(gridPosition, direction, true)) {
					RevealPosition(gridPosition + DungeonMap.directionToV2[direction]);
				}
			}
		}

		private void RevealPosition(Vector2Int gridPosition) {
			var position = new Vector2Int(gridPosition.x, gridPosition.y);
			pathChunks[position.x, position.y].enabled = true;
			if (Game.instance.dungeonMap.TryGetMisc(position, out var misc)) tokens[misc].visible = true;
			if (Game.instance.dungeonMap.TryGetEncounter(position, out var encounter)) tokens[encounter].visible = true;
		}

		private IEnumerator ResolvePlayerCommand(Command command) {
			yield return StartCoroutine(GetPlayerCommandAction(command.type)(command));
		}

		private Func<Command, IEnumerator> GetPlayerCommandAction(CommandType type) {
			if (type == Memory.CommandTypes.moveNorth) return ResolveMoveNorthCommand;
			if (type == Memory.CommandTypes.moveSouth) return ResolveMoveSouthCommand;
			if (type == Memory.CommandTypes.moveWest) return ResolveMoveWestCommand;
			if (type == Memory.CommandTypes.moveEast) return ResolveMoveEastCommand;
			if (type == Memory.CommandTypes.heal) return ResolveHealCommand;
			if (type == Memory.CommandTypes.rest) return ResolveRestCommand;
			throw new ArgumentException($"Command type {type.name} is not handled");
		}

		private static IEnumerator ResolveHealCommand(Command command) {
			AudioManager.Sfx.PlayRandom("combat.heal");
			yield return new WaitForSeconds(.5f);
			Game.instance.playerCharacter.Heal(Game.instance.playerCharacter.GetCommandPower(command));
			yield return new WaitForSeconds(.5f);
		}

		private IEnumerator ResolveRestCommand(Command command) => ResolveRestCommand(command, CameraUtils.main.WorldToScreenPoint(tokens[Game.instance.playerCharacter].position));
		private IEnumerator ResolveMoveNorthCommand(Command command) => ResolveMoveCommand(DungeonMap.Direction.North);
		private IEnumerator ResolveMoveSouthCommand(Command command) => ResolveMoveCommand(DungeonMap.Direction.South);
		private IEnumerator ResolveMoveWestCommand(Command command) => ResolveMoveCommand(DungeonMap.Direction.West);
		private IEnumerator ResolveMoveEastCommand(Command command) => ResolveMoveCommand(DungeonMap.Direction.East);

		private IEnumerator ResolveMoveCommand(DungeonMap.Direction direction) {
			playerPreviousPosition = Game.instance.playerCharacter.dungeonPosition;
			Game.instance.playerCharacter.dungeonPosition += DungeonMap.directionToV2[direction];
			RevealPosition(Game.instance.playerCharacter.dungeonPosition);
			yield return StartCoroutine(MoveToken(Game.instance.playerCharacter));
			DiscoverAround(Game.instance.playerCharacter.dungeonPosition);
			RefreshWindRose();
		}

		[ContextMenu("Reveal All")]
		private void RevealAll() {
			for (var x = 0; x < pathChunks.GetLength(0); ++x)
			for (var y = 0; y < pathChunks.GetLength(1); ++y) {
				pathChunks[x, y].enabled = true;
			}
			tokens.Values.ForEach(t => t.visible = true);
		}

		#region SolveMisc

		private IEnumerator DoSolveMiscTurnStep() {
			Game.instance.playerCharacter.SetCurrentCommand(string.Empty, CommandType.Location.Map);
			if (Game.instance.dungeonMap.TryGetMisc(Game.instance.playerCharacter.dungeonPosition, out var misc)) {
				CommonGameUi.dialogPanel.Clean();
				CommonGameUi.dialogPanel.AddText(misc.interactionText);
				foreach (var option in misc.interactionOptions) {
					CommonGameUi.dialogPanel.AddOption(option.inputValue, option.endOfSentence, option.charged);
				}
				yield return null;
				CommonGameUi.dialogPanel.Show(misc.interactionName);

				InteractionOption inputInteraction = null;
				var listenInputCallbacks = new TextInputManager.ListenUntilResultCallbacks<InteractionOption> {
					completed = t => inputInteraction = t,
					inputChanged = HandleDialogInputChanged,
					letterPaid = HandleLetterPaid,
					letterReimbursed = HandleLetterReimbursed,
					missingLetter = HandleLetterMissing
				};
				yield return StartCoroutine(TextInputManager.ListenUntilResult(misc.interactionOptions, Game.instance.playerCharacter.letterReserve, listenInputCallbacks));
				yield return StartCoroutine(ResolveMisc(misc, inputInteraction));

				CommonGameUi.dialogPanel.Hide();
			}
		}

		private IEnumerator ResolveMisc(IDungeonMisc source, InteractionOption option) => GetResolveFunc(option.type)(source);

		private Func<IDungeonMisc, IEnumerator> GetResolveFunc(InteractionType type) {
			switch (type) {
				case InteractionType.Chest: return ResolveMiscChest;
				case InteractionType.Skip: return t => ResolveSkipInteraction();
				case InteractionType.Book: return ResolveMiscBook;
				case InteractionType.Skill: return ResolveMiscSkill;
				case InteractionType.Power: return ResolveMiscPower;
				case InteractionType.MaxHealth: return ResolveMiscMaxHealth;
				case InteractionType.Portal: return ResolveMiscPortal;
				case InteractionType.Fountain: return ResolveMiscFountain;
				default: throw new ArgumentOutOfRangeException();
			}
		}

		private static IEnumerator ResolveMiscFountain(IDungeonMisc misc) {
			AudioManager.Sfx.PlayRandom("combat.heal");
			yield return new WaitForSeconds(.5f);
			Game.instance.playerCharacter.HealToMaxHealth();
			yield return new WaitForSeconds(.5f);
		}

		private IEnumerator ResolveMiscChest(IDungeonMisc misc) {
			var chest = misc as DungeonChest ?? throw new InvalidCastException();
			foreach (var objectInChest in chest.chestBounty) {
				yield return StartCoroutine(EarnLettersFromDialog(objectInChest));
			}
			yield return new WaitForSeconds(.5f);
			Destroy(tokens[misc].gameObject);
			Game.instance.dungeonMap.RemoveMisc(misc);
		}

		private IEnumerator ResolveMiscBook(IDungeonMisc misc) {
			var bookMisc = misc as IBookDungeonMisc ?? throw new InvalidCastException();
			yield return StartCoroutine(EarnLettersFromDialog(bookMisc.bookName));
			yield return new WaitForSeconds(.5f);
			Destroy(tokens[misc].gameObject);
			Game.instance.dungeonMap.RemoveMisc(misc);
		}

		private IEnumerator ResolveMiscSkill(IDungeonMisc misc) {
			var skillMisc = misc as ISkillDungeonMisc ?? throw new InvalidCastException();
			yield return StartCoroutine(ResolveSkillInteraction(skillMisc.skillCommand));
			Destroy(tokens[misc].gameObject);
			Game.instance.dungeonMap.RemoveMisc(misc);
		}

		private IEnumerator ResolveMiscPower(IDungeonMisc misc) {
			var powerDungeonMisc = misc as IPowerDungeonMisc ?? throw new InvalidCastException();
			yield return StartCoroutine(ResolvePowerInteraction(powerDungeonMisc.powerLetter));
			Destroy(tokens[misc].gameObject);
			Game.instance.dungeonMap.RemoveMisc(misc);
		}

		private IEnumerator ResolveMiscMaxHealth(IDungeonMisc misc) {
			yield return StartCoroutine(ResolveMaxHealthInteraction());
			Destroy(tokens[misc].gameObject);
			Game.instance.dungeonMap.RemoveMisc(misc);
		}

		private IEnumerator ResolveMiscPortal(IDungeonMisc misc) {
			var portal = misc as DungeonPortal ?? throw new InvalidCastException();
			yield return new WaitForSeconds(.5f);
			playerPreviousPosition = Game.instance.playerCharacter.dungeonPosition;
			Game.instance.playerCharacter.dungeonPosition = portal.portalDestination;
			RevealPosition(Game.instance.playerCharacter.dungeonPosition);
			TeleportToken(Game.instance.playerCharacter);
			DiscoverAround(Game.instance.playerCharacter.dungeonPosition);
			RefreshWindRose();
			yield return new WaitForSeconds(.5f);
		}

		#endregion
	}
}