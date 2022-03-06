using UnityEngine;

namespace _7DRL.MiscConstants {
	public static class RlConstants {
		public const int magicNumber = 7919;

		public static class Dungeon {
			public const  int        width  = 10;
			public const  int        height = 10;
			public static int        additionalRandomPaths { get; } = Mathf.FloorToInt(width * height * .1f);
			public static Vector2Int playerStartPosition   { get; } = Vector2Int.zero;
			public static float      chestBountyScore      => 6;
			public static int        minRoomCount          { get; } = Mathf.CeilToInt(width * height * .2f);
			public static int        encounters            { get; } = Mathf.CeilToInt(width * height * .2f);
		}

		public static class Player {
			public const int initialMaxHealth               = 1000;
			public const int initialLettersPerPower         = 100;
			public const int initialLettersPerKnownCommands = 5;
		}

		public static class Foes {
			public const int initialMaxHealthCoefficient = 3;
		}
	}
}