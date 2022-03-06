using UnityEngine;

namespace _7DRL.Scenes.Map {
	public class MapToken : MonoBehaviour {
		[SerializeField] protected SpriteRenderer _renderer;

		public bool visible {
			get => _renderer.enabled;
			set => _renderer.enabled = value;
		}

		public Sprite sprite {
			get => _renderer.sprite;
			set => _renderer.sprite = value;
		}

		public Color color {
			get => _renderer.color;
			set => _renderer.color = value;
		}

		public Vector2 position {
			get => transform.position;
			set => transform.position = value;
		}

		public int priority {
			get => _renderer.sortingOrder - 5;
			set => _renderer.sortingOrder = value + 5;
		}
	}
}