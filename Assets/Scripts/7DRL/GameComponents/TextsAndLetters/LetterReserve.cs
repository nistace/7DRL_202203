using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace _7DRL.GameComponents.TextAndLetters {
	[Serializable]
	public class LetterReserve {
		[SerializeField] protected int[] _reserve;

		public int this[char c] {
			get => _reserve[c - 'A'];
			set => _reserve[c - 'A'] = value;
		}

		public UnityEvent onReserveChanged { get; } = new UnityEvent();

		public LetterReserve() {
			_reserve = new int[1 + 'Z' - 'A'].FilledWith(0);
		}

		public void Remove(char c, int amount = 1) => Change(c, -amount);
		public void Add(char c, int amount = 1) => Change(c, amount);

		public void Change(char c, int change) {
			if (change == 0) return;
			this[c] += change;
			onReserveChanged.Invoke();
		}

		public void Add(LetterReserve other) {
			for (var i = 0; i < _reserve.Length; ++i) {
				_reserve[i] += other._reserve[i];
			}
			onReserveChanged.Invoke();
		}

		public void Add(string word) => word.ForEach(t => Add(t));

		public bool TryRemove(string word) {
			if (!TextUtils.allLetters.All(c => this[c] >= word.Count(t => t == c))) return false;
			word.ForEach(t => Remove(t));
			return true;
		}
	}
}