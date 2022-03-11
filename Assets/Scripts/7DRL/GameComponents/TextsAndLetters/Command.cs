using System;
using UnityEngine;

namespace _7DRL.GameComponents.TextAndLetters {
	[Serializable]
	public class Command : ITextInputResult {
		[SerializeField] protected string      _name;
		[SerializeField] protected string      _textInput;
		[SerializeField] protected CommandType _type;
		[SerializeField] protected int         _order;

		public string      name        => _name;
		public string      textInput   => _textInput;
		public bool        isFreeInput => false;
		public CommandType type        => _type;
		public int         order       => _order;

		public Command(string name, CommandType type, int order) {
			_name = name;
			_textInput = TextUtils.ToInputName(name);
			_type = type;
			_order = order;
		}
	}
}