using System;
using _7DRL.TextInput;
using UnityEngine;

namespace _7DRL.Data {
	[Serializable]
	public class Command {
		[SerializeField] protected string      _name;
		[SerializeField] protected string      _inputName;
		[SerializeField] protected CommandType _type;

		public string      name      => _name;
		public string      inputName => _inputName;
		public CommandType type      => _type;

		public Command(string name, CommandType type) {
			_name = name;
			_inputName = TextUtils.ToInputName(name);
			_type = type;
		}
	}
}