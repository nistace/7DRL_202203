using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utils.Extensions;

namespace _7DRL.GameComponents.TextAndLetters {
	public static class TextUtils {
		public static IReadOnlyList<char> allLetters { get; } = ('Z' - 'A' + 1).CreateArray(t => (char)('A' + t));

		public static string ToInputName(string name) => Regex.Replace(name, "[^a-zA-Z]", "").ToUpper();
		public static string ToInputNameWithOtherCharacters(string name) => name.ToUpper();

		public static int GetValueOfRaw(string input, IReadOnlyList<int> letterPowers) => GetInputValue(ToInputName(input), letterPowers);
		public static int GetValueOfRaw(string input, IReadOnlyDictionary<char, int> letterPowers) => GetInputValue(ToInputName(input), letterPowers);
		public static int GetInputValue(string input, IReadOnlyList<int> letterPowers) => letterPowers == null ? 0 : input.Sum(t => letterPowers.Count <= t - 'A' ? 0 : letterPowers[t - 'A']);
		public static int GetInputValue(string input, IReadOnlyDictionary<char, int> letterPowers) => letterPowers == null ? 0 : input.Sum(t => letterPowers.Of(t));
	}
}