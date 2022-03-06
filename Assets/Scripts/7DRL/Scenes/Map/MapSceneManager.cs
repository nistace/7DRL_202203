using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _7DRL.Data;
using _7DRL.Games;
using _7DRL.MiscConstants;
using _7DRL.TextInput;
using UnityEngine;
using Utils.Extensions;
using Utils.Libraries;

namespace _7DRL.Scenes.Map {
	public class MapSceneManager : SceneManager {
		[SerializeField] protected Follow         _cameraFollow;
		[SerializeField] protected DungeonSprites _sprites;
		[SerializeField] protected SpriteRenderer _pathPrefab;
		[SerializeField] protected Transform      _pathContainer;
		[SerializeField] protected Vector2        _pathChunkSize;
		[SerializeField] protected MapToken       _tokenPrefab;
		[SerializeField] protected Transform      _tokensContainer;
		[SerializeField] protected float          _tokensSpeed = 1;
		[SerializeField] protected MapUi          _ui;

		private SpriteRenderer[,]                     pathChunks { get; set; }
		private Dictionary<IDungeonCrawler, MapToken> tokens     { get; } = new Dictionary<IDungeonCrawler, MapToken>();

		private enum Turn {
			Player    = 0,
			SolveMisc = 1,
			Foe       = 2
		}

		private Turn turn { get; set; }

		private void OnEnable() {
			_cameraFollow.enabled = true;
			_cameraFollow.Jump();
		}

		private void OnDisable() {
			_cameraFollow.enabled = false;
		}

		public void Init(Game game) {
			InitMap(game.dungeonMap);
			InitTokens(game);
			DiscoverAround(game.playerCharacter.dungeonPosition);

			_cameraFollow.target = tokens[game.playerCharacter].transform;
			RevelPosition(game.playerCharacter.dungeonPosition);
			_cameraFollow.Jump();

			_ui.Init(game.playerCharacter);
			RefreshWindRose();

			GameEvents.onEncounterDefeated.AddListenerOnce(HandleEncounterDefeated);

			turn = Turn.Player;
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
			CreateToken(game.playerCharacter, true, _sprites.player, Colors.Of("token.player"), 3);

			foreach (var dungeonMisc in game.dungeonMap.miscRoomContents) {
				CreateToken(dungeonMisc, false, _sprites.GetMisc(dungeonMisc.type), Colors.Of("token.misc"), 1);
			}

			foreach (var encounter in game.dungeonMap.encounters) {
				CreateToken(encounter, false, _sprites.GetFoe(encounter.level), Colors.Of("token.foe"), 2);
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
				pathChunks[x, y].sprite = map.IsRoom(x, y) ? _sprites.GetRoom(map[x, y]) : _sprites.GetLane(map[x, y]);
				pathChunks[x, y].enabled = false;
			}
		}

		private Vector2 GridToWorldPosition(Vector2Int position) => GridToWorldPosition(position.x, position.y);
		private Vector2 GridToWorldPosition(int x, int y) => new Vector2(x * _pathChunkSize.x, y * _pathChunkSize.y);

		public void Continue() => StartCoroutine(DoContinue());

		private IEnumerator DoContinue() {
			Encounter encounterAtPlayerPosition;
			while (!CheckBattle(out encounterAtPlayerPosition)) {
				if (turn == Turn.Player) yield return StartCoroutine(DoPlayerTurn());
				else if (turn == Turn.SolveMisc) yield return StartCoroutine(DoSolveMiscTurn());
				else if (turn == Turn.Foe) yield return StartCoroutine(DoFoeTurn());
				turn = (Turn)((int)(turn + 1) % EnumUtils.SizeOf<Turn>());
			}
			GameEvents.onEncounterAtPlayerPosition.Invoke(encounterAtPlayerPosition);
		}

		private static bool CheckBattle(out Encounter encounter) => Game.instance.dungeonMap.TryGetEncounter(Game.instance.playerCharacter.dungeonPosition, out encounter);

		private static IEnumerator DoSolveMiscTurn() {
			if (Game.instance.dungeonMap.TryGetMisc(Game.instance.playerCharacter.dungeonPosition, out var misc)) {
				Debug.Log("Misc: " + misc);
				yield return null;
			}
		}

		private IEnumerator DoPlayerTurn() {
			TextInputManager.ClearInput();
			TextInputManager.StartListening();
			var lastInput = string.Empty;
			Game.instance.playerCharacter.SetCurrentCommand(string.Empty, CommandType.Location.Map);
			Command command = null;
			var hasCommand = false;
			var canExecuteCommand = false;
			while (!hasCommand || !canExecuteCommand) {
				if (TextInputManager.currentInput != lastInput) {
					lastInput = TextInputManager.currentInput;
					Game.instance.playerCharacter.SetCurrentCommand(lastInput, CommandType.Location.Map);
					hasCommand = Game.instance.playerCharacter.TryGetCurrentCommandIfComplete(out command);
					canExecuteCommand = hasCommand && CanExecute(command) || Game.instance.playerCharacter.TryGetCurrentCommand(out var advisedCommand) && CanExecute(advisedCommand);
					_ui.commandTracker.valid = canExecuteCommand;
				}
				yield return null;
			}
			TextInputManager.StopListening();
			yield return new WaitForSeconds(1);

			var power = Game.instance.playerCharacter.GetCommandPower(command);
			yield return StartCoroutine(ResolvePlayerCommand(command, power));
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

		private IEnumerator DoFoeTurn() {
			yield return new WaitForSeconds(1);
			foreach (var encounter in Game.instance.dungeonMap.encounters.ToArray()) {
				var possibleMovements = Game.instance.dungeonMap.GetPossibleMovements(encounter.dungeonPosition, false).ToArray();
				if (possibleMovements.Length > 0) {
					Game.instance.dungeonMap.MoveEncounter(encounter, possibleMovements.Random());
					StartCoroutine(MoveToken(encounter));
				}
			}
			yield return new WaitForSeconds(2);
		}

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
					RevelPosition(gridPosition + DungeonMap.directionToV2[direction]);
				}
			}
		}

		private void RevelPosition(Vector2Int gridPosition) {
			var position = new Vector2Int(gridPosition.x, gridPosition.y);
			pathChunks[position.x, position.y].enabled = true;
			if (Game.instance.dungeonMap.TryGetMisc(position, out var misc)) tokens[misc].visible = true;
			if (Game.instance.dungeonMap.TryGetEncounter(position, out var encounter)) tokens[encounter].visible = true;
		}

		private IEnumerator ResolvePlayerCommand(Command command, int power) {
			yield return StartCoroutine(GetPlayerCommandAction(command)(power));
		}

		private Func<int, IEnumerator> GetPlayerCommandAction(Command command) {
			if (command.type == Memory.CommandTypes.moveNorth) return ResolveMoveNorthCommand;
			if (command.type == Memory.CommandTypes.moveSouth) return ResolveMoveSouthCommand;
			if (command.type == Memory.CommandTypes.moveWest) return ResolveMoveWestCommand;
			if (command.type == Memory.CommandTypes.moveEast) return ResolveMoveEastCommand;
			Debug.LogError($"Command type {command.type.name} is not handled");
			return ResolveDefaultCommand;
		}

		private static IEnumerator ResolveDefaultCommand(int power) {
			yield return null;
		}

		private IEnumerator ResolveMoveNorthCommand(int power) => ResolveMoveCommand(DungeonMap.Direction.North);
		private IEnumerator ResolveMoveSouthCommand(int power) => ResolveMoveCommand(DungeonMap.Direction.South);
		private IEnumerator ResolveMoveWestCommand(int power) => ResolveMoveCommand(DungeonMap.Direction.West);
		private IEnumerator ResolveMoveEastCommand(int power) => ResolveMoveCommand(DungeonMap.Direction.East);

		private IEnumerator ResolveMoveCommand(DungeonMap.Direction direction) {
			Game.instance.playerCharacter.dungeonPosition += DungeonMap.directionToV2[direction];
			RevelPosition(Game.instance.playerCharacter.dungeonPosition);
			pathChunks[Game.instance.playerCharacter.dungeonPosition.x, Game.instance.playerCharacter.dungeonPosition.y].enabled = true;
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
	}
}