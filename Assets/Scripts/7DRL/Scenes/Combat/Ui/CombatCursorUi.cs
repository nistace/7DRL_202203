using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _7DRL.Scenes.Combat.Ui {
	public class CombatCursorUi : MonoBehaviour {
		[SerializeField] protected Image _image;
		[SerializeField] protected float _delay = .5f;

		private Image image => _image ? _image : _image = GetComponent<Image>();

		public bool visible {
			get => _image.enabled;
			set => _image.enabled = value;
		}

		private Color color {
			get => image.color;
			set => image.color = value;
		}

		public Vector2 position {
			get => transform.position;
			private set => transform.position = value;
		}

		public void Jump(Color color, Vector2 targetPosition) {
			this.color = color;
			position = targetPosition;
		}

		public IEnumerator Change(Color newColor, Vector2 targetPosition) {
			for (var lerp = 0f; lerp < 1; lerp += Time.deltaTime / _delay) {
				Jump(Color.Lerp(color, newColor, lerp), Vector2.Lerp(position, targetPosition, lerp));
				yield return null;
			}
			Jump(newColor, targetPosition);
		}
	}
}