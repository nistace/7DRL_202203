using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace _7DRL.Data {
	[Serializable]
	public abstract class CharacterBase {
		[SerializeField] protected int           _maxHealth;
		[SerializeField] protected int           _health;
		[SerializeField] protected int           _armor;
		[SerializeField] protected int           _dodge;
		[SerializeField] protected int           _escape;
		[SerializeField] protected int           _level;
		[SerializeField] protected List<Command> _knownCommands;

		public          bool                   dead                         => _health <= 0;
		public          int                    level                        => _level;
		public abstract string                 name                         { get; }
		public          string                 completeName                 => $"{name} lvl {_level}";
		public abstract string                 currentCommandLetters        { get; }
		public abstract string                 currentCommandMissingLetters { get; }
		public          int                    health                       => _health;
		public          int                    maxHealth                    => _maxHealth;
		public          int                    armor                        => _armor;
		public          IReadOnlyList<Command> knownCommands                => _knownCommands;
		public          int                    dodge                        => _dodge;
		public          int                    escape                       => _escape;

		public UnityEvent onHealthChanged         { get; } = new UnityEvent();
		public UnityEvent onArmorChanged          { get; } = new UnityEvent();
		public UnityEvent onDodgeChanceChanged    { get; } = new UnityEvent();
		public UnityEvent onEscapeChanceChanged   { get; } = new UnityEvent();
		public UnityEvent onCurrentCommandChanged { get; } = new UnityEvent();

		protected CharacterBase(int level, int maxHealth, IEnumerable<Command> knownCommands) {
			_level = level;
			_maxHealth = maxHealth;
			_health = maxHealth;
			_knownCommands = knownCommands.ToList();
		}

		public abstract int GetCommandPower(Command command);
		public abstract bool TryGetCurrentCommand(out Command command);
		public string GetCommandDescription(Command buildingCommand) => buildingCommand.type.GetDescription(GetCommandPower(buildingCommand));

		public void Damage(int amount) {
			if (_health == 0) return;
			var lostArmor = Mathf.Min(amount, armor);
			var lostHealth = Mathf.Min(amount - lostArmor, health);
			SetHealth(health - lostHealth);
			SetArmor(armor - lostArmor);
		}

		public void Heal(int amount) => SetHealth(Mathf.Min(_maxHealth, _health + amount));
		public void AddArmor(int amount) => SetArmor(armor + amount);

		private void SetHealth(int health) {
			if (_health == health) return;
			_health = health;
			onHealthChanged.Invoke();
		}

		private void SetArmor(int armor) {
			if (_armor == armor) return;
			_armor = armor;
			onArmorChanged.Invoke();
		}

		private void SetDodge(int amount) {
			if (_dodge == amount) return;
			_dodge = Mathf.Clamp(amount, 0, 100);
			onDodgeChanceChanged.Invoke();
		}

		private void SetEscape(int amount) {
			if (_escape == amount) return;
			_escape = Mathf.Clamp(amount, 0, 100);
			onEscapeChanceChanged.Invoke();
		}

		public void AddChanceToDodge(int amount) => SetDodge(_dodge + amount);
		public void AddChanceToEscape(int amount) => SetEscape(_escape + amount);
		public bool RollDodge() => Random.Range(0, 100) < _dodge;
		public bool RollEscape() => Random.Range(0, 100) < _escape;
		public void ResetChanceToDodge() => SetDodge(0);
		public void ResetChanceToEscape() => SetEscape(0);
		public void ResetArmor() => SetArmor(0);

		public void ResetForBattle() {
			ResetChanceToDodge();
			ResetChanceToEscape();
			ResetArmor();
		}
	}
}