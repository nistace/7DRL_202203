using System.Collections.Generic;
using Utils.Extensions;

namespace _7DRL.GameComponents.TextAndLetters {
	public class BookNameGenerator {
		public enum Column {
			ActionAdjective    = 0,
			Action             = 1,
			CharacterAdjective = 2,
			Character          = 3,
			LocationAdjective  = 4,
			Location           = 5
		}

		private IReadOnlyList<IReadOnlyCollection<string>> bookNameParts { get; }

		private string this[Column column] => bookNameParts[(int)column].Random();

		public BookNameGenerator(IReadOnlyList<IReadOnlyCollection<string>> bookNameParts) {
			this.bookNameParts = bookNameParts;
		}

		public string Generate(int minPower, IReadOnlyDictionary<char, int> letterPowers) {
			var action = this[Column.Action];
			var character = this[Column.Character];
			var name = $"The {action} of the {character}";
			if (TextUtils.GetValueOfRaw(name, letterPowers) > minPower) return name;
			var characterAdjective = this[Column.CharacterAdjective];
			name = $"The {action} of the {characterAdjective} {character}";
			if (TextUtils.GetValueOfRaw(name, letterPowers) > minPower) return name;
			var actionAdjective = this[Column.ActionAdjective];
			name = $"The {actionAdjective} {action} of the {characterAdjective} {character}";
			if (TextUtils.GetValueOfRaw(name, letterPowers) > minPower) return name;
			var location = this[Column.Location];
			name = $"The {actionAdjective} {action} of the {characterAdjective} {character} of the {location}";
			if (TextUtils.GetValueOfRaw(name, letterPowers) > minPower) return name;
			var locationAdjective = this[Column.LocationAdjective];
			name = $"The {actionAdjective} {action} of the {characterAdjective} {character} of the {locationAdjective} {location}";
			return name;
		}
	}
}