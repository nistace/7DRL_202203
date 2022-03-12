using _7DRL.GameComponents.TextAndLetters;

namespace _7DRL.Data.IntroScript {
	public class IntroCommand : ITextInputResult {
		public string command       { get; }
		public string endOfSentence { get; }
		public string textInput     { get; }
		public bool   isFreeInput   => true;

		public IntroCommand(string command, string endOfSentence) {
			this.command = command;
			this.endOfSentence = endOfSentence;
			textInput = TextUtils.ToInputName(command);
		}
	}
}