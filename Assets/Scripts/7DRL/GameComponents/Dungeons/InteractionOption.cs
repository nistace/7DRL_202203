using _7DRL.GameComponents.TextAndLetters;

namespace _7DRL.GameComponents.Dungeons {
	public class InteractionOption {
		public string          inputValue    { get; }
		public string          endOfSentence { get; }
		public InteractionType type          { get; }
		public bool            charged       { get; }

		public InteractionOption(InteractionOption origin, string endOfSentence) : this(origin.inputValue, origin.type, endOfSentence, origin.charged) { }

		public InteractionOption(string command, InteractionType type, string endOfSentence, bool charged) {
			inputValue = TextUtils.ToInputName(command);
			this.endOfSentence = endOfSentence;
			this.type = type;
			this.charged = charged;
		}
	}
}