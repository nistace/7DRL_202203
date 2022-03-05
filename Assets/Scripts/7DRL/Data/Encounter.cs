using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _7DRL.Data {
	[Serializable]
	public class Encounter {
		[SerializeField] protected Foe[] _foes;

		public Foe[] foes => _foes;

		public Encounter(IEnumerable<Foe> foes) {
			_foes = foes.ToArray();
		}
	}
}