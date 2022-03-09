using System;
using System.Linq;
using _7DRL.GameComponents.Characters;
using _7DRL.GameComponents.Dungeons;
using _7DRL.GameComponents.TextAndLetters;
using _7DRL.MiscConstants;
using TMPro;
using UnityEngine;
using Utils.Extensions;
using Utils.Libraries;

namespace _7DRL.Scenes.Map {
	public class WindRoseUi : MonoBehaviour {
		[SerializeField] protected TMP_Text _northCommandsText;
		[SerializeField] protected TMP_Text _eastCommandsText;
		[SerializeField] protected TMP_Text _westCommandsText;
		[SerializeField] protected TMP_Text _southCommandsText;

		private PlayerCharacter character { get; set; }

		public void Set(PlayerCharacter character) {
			this.character = character;
			this.character.onKnownCommandsChanged.AddListenerOnce(Refresh);
			Refresh();
		}

		private void Refresh() {
			Refresh(_northCommandsText, Memory.CommandTypes.moveNorth);
			Refresh(_eastCommandsText, Memory.CommandTypes.moveEast);
			Refresh(_westCommandsText, Memory.CommandTypes.moveWest);
			Refresh(_southCommandsText, Memory.CommandTypes.moveSouth);
		}

		private void Refresh(TMP_Text text, CommandType type) {
			text.text = character.knownCommands.Where(t => t.type == type).OrderBy(t => t.order).ThenBy(t => t.name).Select(t => t.inputName).Join("<br>");
		}

		public void SetDirectionEnabled(DungeonMap.Direction direction, bool enabled) {
			GetText(direction).color = Colors.Of("ui.text.player." + (enabled ? "active" : "disabled"));
		}

		private TMP_Text GetText(DungeonMap.Direction direction) {
			switch (direction) {
				case DungeonMap.Direction.North: return _northCommandsText;
				case DungeonMap.Direction.East: return _eastCommandsText;
				case DungeonMap.Direction.South: return _southCommandsText;
				case DungeonMap.Direction.West: return _westCommandsText;
				default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}
		}
	}
}