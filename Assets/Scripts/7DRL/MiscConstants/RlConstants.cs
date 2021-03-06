using UnityEngine;

namespace _7DRL.MiscConstants {
	public static class RlConstants {
		public const int letterMaxPower = 100;

		public static class Dungeon {
			public const  int        width  =8;
			public const  int        height = 8;
			public static int        additionalRandomPaths { get; } = Mathf.FloorToInt(width * height * .25f);
			public static Vector2Int playerStartPosition   { get; } = Vector2Int.zero;
			public static float      chestBountyScore      => 50;
			public static float      bookNameScore         => 100;
			public static float      skillCommandScore     => 10;
			public static int        minRoomCount          { get; } = Mathf.CeilToInt(width * height * .3f);
			public static int        encounters            { get; } = Mathf.CeilToInt(width * height * .2f);
		}

		public static class Player {
			public const string name                           = "Typist";
			public const float  maxHealthCoefficient           = 1.2f;
			public const int    baseLetterAmount               = 12;
			public const int    initialLettersPerKnownCommands = 3;
		}

		public static class Foes {
			public const float maxHealthCoefficient = .04f;
			public const float maxHealthConstant    = 60;
		}
	}
}