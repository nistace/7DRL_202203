using System.Collections.Generic;

namespace _7DRL.Data.IntroScript {
	public class IntroScriptLine {
		public string                            text            { get; }
		public IntroCommand                      continueCommand { get; }
		public IntroCommand                      skipCommand     { get; }
		public string                            spriteKey       { get; }
		public IReadOnlyCollection<IntroCommand> commands        => new[] { continueCommand, skipCommand };

		public IntroScriptLine(string text, (string command, string endOfSentence) continueCommand, (string command, string endOfSentence) skipCommand, string spriteKey) {
			this.text = text;
			this.continueCommand = new IntroCommand(continueCommand.command, continueCommand.endOfSentence);
			this.skipCommand = new IntroCommand(skipCommand.command, skipCommand.endOfSentence);
			this.spriteKey = spriteKey;
		}
	}
}