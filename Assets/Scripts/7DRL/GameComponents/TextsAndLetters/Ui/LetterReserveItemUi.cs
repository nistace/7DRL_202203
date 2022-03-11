using System.Collections;
using TMPro;
using UnityEngine;

namespace _7DRL.GameComponents.TextAndLetters.Ui {
	public class LetterReserveItemUi : MonoBehaviour {
		[SerializeField] protected Color    _defaultColor;
		[SerializeField] protected TMP_Text _letter;
		[SerializeField] protected TMP_Text _amount;

		public char letter {
			set => _letter.text = $"{value}";
		}

		public int amount {
			set => _amount.text = $"{Mathf.Clamp(value, 0, 99)}";
		}

		public Color defaultColor {
			get => _defaultColor;
			set {
				_defaultColor = value;
				color = value;
			}
		}

		private Color color {
			set {
				_letter.color = value;
				_amount.color = value;
			}
		}

		public IEnumerator Flash(Color c, float time) {
			var lerp = 1f;
			while (lerp > 0) {
				lerp -= Time.deltaTime * time;
				color = Color.Lerp(defaultColor, c, lerp);
				yield return null;
			}
			color = defaultColor;
		}
	}
}