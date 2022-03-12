using System.Collections.Generic;
using UnityEngine;
using Utils.Extensions;
using Utils.Libraries;

namespace _7DRL.GameComponents.TextAndLetters.Ui {
	public class LetterReserveUi : MonoBehaviour {
		[SerializeField] protected LetterReserveItemUi _itemPrefab;
		[SerializeField] protected Transform           _container;
		[SerializeField] protected float               _letterFlashTime = .5f;

		public enum FlashType {
			Add,
			Remove,
			Missing
		}

		private LetterReserve                         reserve { get; set; }
		private Dictionary<char, LetterReserveItemUi> items   { get; } = new Dictionary<char, LetterReserveItemUi>();

		public void Set(LetterReserve reserve) {
			this.reserve?.onReserveChanged.RemoveListener(Refresh);
			this.reserve = reserve;
			_container.ClearChildren();
			items.Clear();
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
				items[c].defaultColor = Colors.Of($"ui.text.player.{(reserve[c] > 0 ? "active" : "inactive")}");
			}
		}

		public bool TryGetPosition(char letter, out Transform position) => position = items.ContainsKey(letter) ? items[letter].transform : null;
		public void FlashLine(char c, FlashType type) => StartCoroutine(items[c].Flash(Colors.Of($"letterReserve.flash.{type}"), _letterFlashTime));
	}
}