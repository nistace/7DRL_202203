using _7DRL.GameComponents.TextAndLetters;

namespace _7DRL.GameComponents.Interactions {
	public class InteractionOption : ITextInputResult {
		public string          inputValue    { get; }
		public string          endOfSentence { get; }
		public InteractionType type          { get; }
		public bool            charged       { get; }
		public string          textInput     => inputValue;
		public bool            isFreeInput   => !charged;

		public InteractionOption(InteractionOption origin, string endOfSentence) : this(origin.inputValue, origin.type, endOfSentence, origin.charged) { }

		public InteractionOption(string command, InteractionType type, string endOfSentence, bool charged) {
			inputValue = TextUtils.ToInputName(command);
			this.endOfSentence = endOfSentence;
			this.type = type;
			this.charged = charged;
		}
	}
}