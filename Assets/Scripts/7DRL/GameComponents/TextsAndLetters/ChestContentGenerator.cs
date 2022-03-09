using System.Collections.Generic;
using System.Linq;
using Utils.Extensions;

namespace _7DRL.GameComponents.TextAndLetters {
	public class ChestContentGenerator {
		private IReadOnlyList<string> objects { get; }

		public ChestContentGenerator(IEnumerable<string> objects) {
			this.objects = objects.ToArray();
		}

		public IReadOnlyList<string> Generate(int minScore, IReadOnlyDictionary<char, int> letterPowers) {
			var score = 0;
			var result = new List<string>();
			while (score < minScore) {
				var newObject = objects.Random();
				if (!result.Contains(newObject)) {
					result.Add(newObject);
					score += TextUtils.GetValueOfRaw(newObject, letterPowers);
				}
			}
			return result;
		}
	}
}