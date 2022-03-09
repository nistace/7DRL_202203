using UnityEngine;

namespace _7DRL.MiscConstants {
	public static class RlConstants {

		public static class Dungeon {
			public const  int        width  = 10;
			public const  int        height = 10;
			public static int        additionalRandomPaths { get; } = Mathf.FloorToInt(width * height * .1f);
			public static Vector2Int playerStartPosition   { get; } = Vector2Int.zero;
			public static float      chestBountyScore      => 50;
			public static float      readBookNameScore     => 50;
			public static float      skillCommandScore     => 10;
			public static float      powerLetterScore      => 5;
			public static int        minRoomCount          { get; } = Mathf.CeilToInt(width * height * .2f);
			public static int        encounters            { get; } = Mathf.CeilToInt(width * height * .2f);
		}

		public static class Player {
			public const int initialMaxHealth               = 500;
			public const int initialLettersPerPower         = 100;
			public const int initialLettersPerKnownCommands = 5;
		}

		public static class Foes {
			public const float maxHealthCoefficient = .2f;
			public const float maxHealthConstant    = 150;
		}
	}
}