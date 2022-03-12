using System.Collections;
using UnityEngine;

namespace _7DRL.Scenes.Intro {
	public class IntroScene : MonoBehaviour {
		[SerializeField] protected SpriteRenderer _renderer;
		[SerializeField] protected float          _changeSpriteSpeed = 2f;

		public Color color {
			get => _renderer.color;
			set => _renderer.color = value;
		}

		public Sprite sprite {
			get => _renderer.sprite;
			set => _renderer.sprite = value;
		}

		public IEnumerator ChangeSprite(Sprite newSprite) {
			if (_renderer.sprite == newSprite) yield break;
			var lerp = 0f;
			while (lerp < 1) {
				_renderer.color = Color.Lerp(Color.white, Color.black, lerp);
				lerp += 2 * Time.deltaTime * _changeSpriteSpeed;
				yield return null;
			}
			_renderer.color = Color.black;
			yield return null;
			_renderer.sprite = newSprite;
			yield return null;
			while (lerp > 0) {
				_renderer.color = Color.Lerp(Color.white, Color.black, lerp);
				lerp -= 2 * Time.deltaTime * _changeSpriteSpeed;
				yield return null;
			}
			_renderer.color = Color.white;
		}

		public void SetSprite(Sprite sprite) => _renderer.sprite = sprite;
	}
}