using UnityEngine;

namespace _7DRL.Scenes {
	public abstract class SceneManager : MonoBehaviour {
		public void Enable() {
			gameObject.SetActive(true);
		}

		public void Disable() {
			StopAllCoroutines();
			gameObject.SetActive(false);
		}
	}
}