using System;
using System.Collections.Generic;
using System.Linq;
using _7DRL.GameComponents.Interactions;
using _7DRL.GameComponents.TextAndLetters;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;
using Utils.Libraries;

namespace _7DRL.GameComponents.Characters {
	[Serializable]
	public class Encounter : IDungeonCrawler {
		public class Event : UnityEvent<Encounter> { }

		public enum Level {
			Weak   = 0,
			Normal = 1,
			Strong = 2,
			Boss   = 3
		}

		[SerializeField] protected Foe[]      _foes;
		[SerializeField] protected Vector2Int _dungeonPosition;
		[SerializeField] protected Level      _level;

		public Foe[] foes  => _foes;
		public Level level => _level;

		public Vector2Int dungeonPosition {
			get => _dungeonPosition;
			set => _dungeonPosition = value;
		}

		public Sprite tokenSprite => Sprites.Of($"token.foe.{level}");

		public InteractionOption              maxHealthInteraction    { get; }
		public InteractionOption              skillInteraction        { get; }
		public Command                        skillInteractionCommand { get; }
		public InteractionOption              powerInteraction        { get; }
		public char                           powerInteractionLetter  { get; }
		public InteractionOption              skipInteraction         { get; }
		public IEnumerable<InteractionOption> interactionOptions      { get; }

		public Encounter(Level level, Vector2Int dungeonPosition, IEnumerable<Foe> foes, InteractionOption maxHealthInteraction, (InteractionOption, Command ) skillInteraction,
			(InteractionOption, char) powerInteraction, InteractionOption skipInteraction) {
			_foes = foes.ToArray();
			_level = level;
			_dungeonPosition = dungeonPosition;
			this.maxHealthInteraction = maxHealthInteraction;
			(this.skillInteraction, skillInteractionCommand) = skillInteraction;
			(this.powerInteraction, powerInteractionLetter) = powerInteraction;
			this.skipInteraction = skipInteraction;
			interactionOptions = new[] { this.maxHealthInteraction, this.skillInteraction, this.powerInteraction, this.skipInteraction }.NotNull().ToArray();
		}
	}
}