using System;

namespace _7DRL.Data {
	public interface IDungeonMisc : IDungeonCrawler {
		enum Type {
			Chest,
			Portal
		}

		Type type { get; }
	}
}