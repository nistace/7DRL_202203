using System.Collections.Generic;
using _7DRL.Data;
using UnityEngine;
using Utils.Extensions;
using Utils.Libraries;

namespace _7DRL.Ui {
	public class LetterReserveUi : MonoBehaviour {
		[SerializeField] protected LetterReserveItemUi _itemPrefab;
		[SerializeField] protected Transform           _container;

		private LetterReserve                         reserve { get; set; }
		private Dictionary<char, LetterReserveItemUi> items   { get; } = new Dictionary<char, LetterReserveItemUi>();

		public void Set(LetterReserve reserve) {
			this.reserve?.onReserveChanged.RemoveListener(Refresh);
			this.reserve = reserve;
			_container.ClearChildren();
			for (var c = 'A'; c <= 'Z'; c++) {
				items.Add(c, Instantiate(_itemPrefab, _container));
				items[c].letter = c;
			}
			this.reserve?.onReserveChanged.AddListenerOnce(Refresh);
			Refresh();
		}

		private void Refresh() {
			for (var c = 'A'; c <= 'Z'; c++) {
				items[c].amount = reserve[c];
				items[c].color = Colors.Of($"ui.text.player.{(reserve[c] > 0 ? "active" : "inactive")}");
			}
		}
	}
}