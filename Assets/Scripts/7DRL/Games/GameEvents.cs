using _7DRL.Data;
using UnityEngine.Events;

namespace _7DRL.Games {
	public static class GameEvents {
		public static UnityEvent      onPlayerFledBattle          { get; } = new UnityEvent();
		public static UnityEvent      onPlayerDead                { get; } = new UnityEvent();
		public static UnityEvent      onPlayerStuck               { get; } = new UnityEvent();
		public static Encounter.Event onEncounterDefeated         { get; } = new Encounter.Event();
		public static Encounter.Event onEncounterAtPlayerPosition { get; } = new Encounter.Event();
		public static UnityEvent      onTurnChanged               { get; } = new UnityEvent();
	}
}