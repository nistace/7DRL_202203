using TMPro;
using UnityEngine;
using Utils.Extensions;

namespace _7DRL.GameComponents.TextAndLetters.Ui {
	public class TextEffectWordUi : MonoBehaviour {
		[SerializeField] protected Transform _letterAnchor;
		[SerializeField] protected TMP_Text  _text;

		public Vector2 letterAnchor => _letterAnchor.position;

		public int textLength => _text.text.Length;

		public string text {
			get => _text.text;
			set => _text.text = value;
		}

		public float alpha {
			get => _text.color.a;
			set => _text.color = _text.color.With(a: value);
		}

		public char PopFirstLetter() {
			var firstLetter = _text.text[0];
			_text.text = _text.text.Substring(1);
			return firstLetter;
		}
	}
}