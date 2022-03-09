using System;
using UnityEngine;

namespace _7DRL.GameComponents.TextAndLetters {
	[Serializable]
	public class Command {
		[SerializeField] protected string      _name;
		[SerializeField] protected string      _inputName;
		[SerializeField] protected CommandType _type;
		[SerializeField] protected int         _order;

		public string      name      => _name;
		public string      inputName => _inputName;
		public CommandType type      => _type;
		public int         order     => _order;

		public Command(string name, CommandType type, int order) {
			_name = name;
			_inputName = TextUtils.ToInputName(name);
			_type = type;
			_order = order;
		}
	}
}