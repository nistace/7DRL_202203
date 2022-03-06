using System;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace _7DRL.Data {
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
	}
}