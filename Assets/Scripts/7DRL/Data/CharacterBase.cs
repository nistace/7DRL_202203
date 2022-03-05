using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace _7DRL.Data {
	[Serializable]
	public abstract class CharacterBase {
		[SerializeField] protected int           _maxHealth;
		[SerializeField] protected int           _health;
		[SerializeField] protected int           _armor;
		[SerializeField] protected int           _level;
		[SerializeField] protected List<Command> _knownCommands;

		public bool dead  => _health <= 0;
		public int  level => _level;

		public abstract string name                         { get; }
		public          string completeName                 => $"{name} lvl {_level}";
		public abstract string currentCommandLetters        { get; }
		public abstract string currentCommandMissingLetters { get; }
		public          int    health                       => _health;
		public          int    maxHealth                    => _maxHealth;
		public          int    armor                        => _armor;

		public UnityEvent onHealthOrArmorChanged  { get; } = new UnityEvent();
		public UnityEvent onCurrentCommandChanged { get; } = new UnityEvent();

		protected CharacterBase(int level, int maxHealth, IEnumerable<Command> knownCommands) {
			_level = level;
			_maxHealth = maxHealth;
			_health = maxHealth;
			_knownCommands = knownCommands.ToList();
		}

		public void Damage(int amount) {
			if (_health == 0) return;
			var lostArmor = Mathf.Min(amount, armor);
			var lostHealth = Mathf.Min(amount - lostArmor, health);
			SetHealthAndArmor(health - lostHealth, armor - lostArmor);
		}

		public void Heal(int amount) {
			if (_health == _maxHealth) return;
			SetHealthAndArmor(Mathf.Min(_maxHealth, _health + amount), armor);
		}

		public void AddArmor(int amount) => SetHealthAndArmor(health, armor + amount);

		private void SetHealthAndArmor(int health, int armor) {
			_health = health;
			_armor = armor;
			onHealthOrArmorChanged.Invoke();
		}

		public abstract int GetCommandPower(Command command);
		public abstract bool TryGetCurrentCommand(out Command command);
	}
}