using System.Collections.Generic;
using System.Linq;
using _7DRL.GameComponents.Characters;
using _7DRL.GameComponents.TextAndLetters;
using _7DRL.Games;
using UnityEngine;
using Utils.Extensions;

namespace _7DRL.Ui {
	public class KnownCommandsUi : MonoBehaviour {
		[SerializeField] protected Transform           _container;
		[SerializeField] protected KnownCommandsItemUi _itemPrefab;

		private List<KnownCommandsItemUi> items            { get; } = new List<KnownCommandsItemUi>();
		private CommandType.Location      commandsValidity { get; set; }

		private void OnEnable() => RefreshItems();

		public void Set(PlayerCharacter player) {
			RefreshList();
			player.letterReserve.onReserveChanged.AddListenerOnce(RefreshItems);
			player.onKnownCommandsChanged.AddListenerOnce(RefreshList);
			player.onLetterPowersChanged.AddListenerOnce(RefreshItems);
		}

		private void RefreshItems() => items.ForEach(t => t.Refresh());

		private void RefreshList() {
			items.Clear();
			_container.ClearChildren();
			if (Game.instance == null) return;
			foreach (var command in Game.instance.playerCharacter.knownCommands.OrderBy(t => t.order).ThenBy(t => t.textInput)) {
				var newInstance = Instantiate(_itemPrefab, _container);
				newInstance.Set(command);
				items.Add(newInstance);
			}
			RefreshValidCommands();
		}

		public void SetValidCommands(CommandType.Location location) {
			commandsValidity = location;
			RefreshValidCommands();
		}

		private void RefreshValidCommands() => items.ForEach(t => t.valid = t.command.type.IsUsable(commandsValidity));
	}
}