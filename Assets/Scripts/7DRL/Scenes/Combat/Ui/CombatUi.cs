using _7DRL.GameComponents.Characters;
using UnityEngine;

namespace _7DRL.Scenes.Combat.Ui {
	[RequireComponent(typeof(Canvas))]
	public class CombatUi : MonoBehaviour {
		[SerializeField] protected Canvas                 _canvas;
		[SerializeField] protected CombatCharacterBarUi   _playerBar;
		[SerializeField] protected CombatCharacterBarUi[] _foeBars;

		public Canvas                 canvas    => _canvas ? _canvas : _canvas = GetComponent<Canvas>();
		public CombatCharacterBarUi   playerBar => _playerBar;
		public CombatCharacterBarUi[] foeBars   => _foeBars;

		public void InitBars(PlayerCharacter player, Foe[] foes) {
			_playerBar.Set(player);
			for (var i = 0; i < _foeBars.Length; ++i) {
				_foeBars[i].gameObject.SetActive(foes.Length > i);
				if (foes.Length > i) _foeBars[i].Set(foes[i]);
			}
		}

		public void SetVisible(bool visible) => canvas.enabled = visible;
	}
}