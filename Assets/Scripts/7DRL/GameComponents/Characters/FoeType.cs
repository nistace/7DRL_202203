using System;
using _7DRL.GameComponents.TextAndLetters;
using UnityEngine;

namespace _7DRL.GameComponents.Characters {
	[Serializable]
	public class FoeType {
		[SerializeField] protected string _name;
		[SerializeField] protected string _inputName;

		public string name      => _name;
		public string inputName => _inputName;

		public FoeType(string name) {
			_name = name;
			_inputName = TextUtils.ToInputName(name);
		}
	}
}