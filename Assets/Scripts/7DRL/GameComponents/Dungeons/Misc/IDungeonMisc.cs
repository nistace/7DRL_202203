using System.Collections.Generic;
using _7DRL.GameComponents.Interactions;
using UnityEngine;

namespace _7DRL.GameComponents.Dungeons.Misc {
	public interface IDungeonMisc : IDungeonCrawler {
		enum Type {
			Chest,
			Portal,
			StoneTabletOfKnowledge,
			FountainOfYouth,
			TrainingDummy
		}

		Type type { get; }

		IReadOnlyCollection<InteractionOption> interactionOptions { get; }
		string                                 interactionText    { get; }
		string                                 interactionName    { get; }
		Sprite                                 tokenSprite        { get; }
	}
}