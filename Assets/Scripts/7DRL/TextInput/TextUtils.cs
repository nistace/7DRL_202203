using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utils.Extensions;

namespace _7DRL.TextInput {
	public static class TextUtils {
		public static string ToInputName(string name) => Regex.Replace(name, "[^a-zA-Z]", "").ToUpper();

		public static int GetInputValue(string input, IReadOnlyList<int> letterPowers) => letterPowers == null ? 0 : input.Sum(t => letterPowers.Count <= t - 'A' ? 0 : letterPowers[t - 'A']);
		public static int GetInputValue(string input, IReadOnlyDictionary<char, int> letterPowers) => letterPowers == null ? 0 : input.Sum(t => letterPowers.Of(t));
	}
}