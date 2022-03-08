using System;
using _7DRL.Games;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace _7DRL.Scenes.Map {
	public class GameTurnUi : MonoBehaviour {
		[SerializeField] protected TMP_Text _turnCounter;
		[SerializeField] protected TMP_Text _turnStep;
		[SerializeField] protected Image    _progress;

		public float progress {
			set => _progress.fillAmount = value;
		}

		public void Init() {
			Refresh();
			GameEvents.onTurnChanged.AddListenerOnce(Refresh);
		}

		private void Refresh() {
			_turnCounter.text = $"Turn {Game.instance.turn}";
			_turnStep.text = GetTurnStepText();
			progress = 0;
		}

		private static string GetTurnStepText() {
			switch (Game.instance.turnStep) {
				case Game.TurnStep.Player: return "Your turn.";
				case Game.TurnStep.SolveMisc: return "Your turn.";
				case Game.TurnStep.Foe: return "You hear footsteps...";
				default: throw new ArgumentOutOfRangeException();
			}
		}
	}
}