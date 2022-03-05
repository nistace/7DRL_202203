﻿using _7DRL.Data;
using UnityEngine;

namespace _7DRL.Scenes.Combat.Ui {
	public class CombatUi : MonoBehaviour {
		[SerializeField] protected CombatCharacterBarUi   _playerBar;
		[SerializeField] protected CombatCharacterBarUi[] _foeBars;

		public CombatCharacterBarUi   playerBar => _playerBar;
		public CombatCharacterBarUi[] foeBars   => _foeBars;

		public void InitBars(PlayerCharacter player, Foe[] foes) {
			_playerBar.Set(player);
			for (var i = 0; i < _foeBars.Length; ++i) {
				_foeBars[i].gameObject.SetActive(foes.Length > i);
				if (foes.Length > i) _foeBars[i].Set(foes[i]);
			}
		}
	}
}