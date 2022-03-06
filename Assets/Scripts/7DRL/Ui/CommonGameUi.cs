using UnityEngine;

namespace _7DRL.Ui {
	public class CommonGameUi : MonoBehaviour {
		private static CommonGameUi instance { get; set; }

		[SerializeField] protected LetterReserveUi _playerLetterReserve;

		public static LetterReserveUi playerLetterReserve => instance?._playerLetterReserve;

		private void Awake() {
			instance = this;
		}
	}
}