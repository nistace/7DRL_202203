using System.Collections;
using System.Linq;
using _7DRL.Games;
using UnityEngine;
using Utils.Extensions;

namespace _7DRL {
	public class App : MonoBehaviour {
		[SerializeField] protected Game _currentGame;

		public void Start() {
			StartCoroutine(Init());
		}

		private IEnumerator Init() {
			yield return StartCoroutine(Memory.Load());
			Game.instance = GameFactory.CreateGame();
			_currentGame = Game.instance;
		}
	}
}