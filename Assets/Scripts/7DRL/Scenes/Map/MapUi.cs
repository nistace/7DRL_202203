using _7DRL.Data;
using UnityEngine;

namespace _7DRL.Scenes.Map {
	public class MapUi : MonoBehaviour {
		[SerializeField] protected WindRoseUi     _windRose;
		[SerializeField] protected MapCharacterUi _mapCharacter;
		[SerializeField] protected GameTurnUi     _gameTurn;

		public MapCharacterUi mapCharacter => _mapCharacter;
		public WindRoseUi     windRose     => _windRose;
		public GameTurnUi     gameTurn     => _gameTurn;

		public void Init(PlayerCharacter character) {
			_gameTurn.Init();
			_windRose.Set(character);
			_mapCharacter.Set(character);
		}
	}
}