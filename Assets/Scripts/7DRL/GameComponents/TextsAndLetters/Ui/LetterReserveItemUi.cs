using TMPro;
using UnityEngine;

namespace _7DRL.GameComponents.TextAndLetters.Ui {
	public class LetterReserveItemUi : MonoBehaviour {
		[SerializeField] protected TMP_Text _letter;
		[SerializeField] protected TMP_Text _amount;

		public char letter {
			set => _letter.text = $"{value}";
		}

		public int amount {
			set => _amount.text = $"{value}";
		}

		public Color color {
			set {
				_letter.color = value;
				_amount.color = value;
			}
		}
	}
}