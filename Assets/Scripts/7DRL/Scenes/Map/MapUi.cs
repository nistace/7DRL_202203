using _7DRL.Data;
using UnityEngine;

namespace _7DRL.Scenes.Map {
	public class MapUi : MonoBehaviour {
		[SerializeField] protected WindRoseUi       _windRose;
		[SerializeField] protected CommandTrackerUi _commandTracker;

		public CommandTrackerUi commandTracker => _commandTracker;
		public WindRoseUi       windRose       => _windRose;

		public void Init(PlayerCharacter character) {
			_windRose.Set(character);
			_commandTracker.Set(character);
		}
	}
}