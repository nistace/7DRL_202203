using System;
using System.Collections.Generic;
using System.Linq;
using _7DRL.GameComponents.Characters;
using _7DRL.GameComponents.Dungeons;
using _7DRL.GameComponents.TextAndLetters;
using UnityEngine;
using Utils.Extensions;

namespace _7DRL.Data {
	public static class DataFactory {
		public static IEnumerable<Command> LoadCommands() {
			var result = new List<Command>();

			var types = Resources.LoadAll<CommandType>("Data/CommandTypes").ToDictionary(t => t.name.ToUpper(), t => t);
			var commandsCsv = Resources.Load<TextAsset>("Sheets/commands");

			var columns = commandsCsv.CsvHeaderAsDictionary();
			var csvLines = commandsCsv.CsvLines();
			foreach (var csvLine in csvLines) {
				var commandName = csvLine[columns["Name"]];
				if (!types.TryGetValue(csvLine[columns["Type"]].ToUpper(), out var commandType)) {
					Debug.LogError($"Type {csvLine[columns["Type"]]} not found for command {commandName}. Command skipped");
					continue;
				}
				var commandOrder = int.TryParse(csvLine[columns["Order"]], out var parsedOrder) ? parsedOrder : 0;

				result.Add(new Command(commandName, commandType, commandOrder));
			}
			CheckConflicts(result);
			return result;
		}

		private static void CheckConflicts(List<Command> allCommands) {
			foreach (var first in allCommands)
			foreach (var second in allCommands.Where(second => first != second && (first.type.usedInLocations & second.type.usedInLocations) > 0 && first.inputName.StartsWith(second.inputName))) {
				Debug.LogError($"CONFLICT: between {first.inputName} and {second.inputName}");
			}
		}

		public static IEnumerable<FoeType> LoadFoeTypes() {
			var commandsCsv = Resources.Load<TextAsset>("Sheets/foes");
			var columns = commandsCsv.CsvHeaderAsDictionary();
			return commandsCsv.CsvLines().Select(csvLine => new FoeType(csvLine[columns["Name"]]));
		}

		public static ChestContentGenerator LoadChestContentGenerator() {
			var commandsCsv = Resources.Load<TextAsset>("Sheets/chestObjects");
			var columns = commandsCsv.CsvHeaderAsDictionary();
			return new ChestContentGenerator(commandsCsv.CsvLines().Select(csvLine => csvLine[columns["Name"]]));
		}

		public static BookNameGenerator LoadBookNameGenerator() {
			var words = new HashSet<string>[EnumUtils.SizeOf<BookNameGenerator.Column>()];
			for (var i = 0; i < words.Length; ++i) words[i] = new HashSet<string>();
			var csv = Resources.Load<TextAsset>("Sheets/bookNames");
			var columns = csv.CsvHeaderAsDictionary();
			foreach (var line in csv.CsvLines()) {
				foreach (var column in EnumUtils.Values<BookNameGenerator.Column>())
					words[(int)column].Add(line[columns[$"{column}"]]);
			}
			return new BookNameGenerator(words);
		}

		public static Dictionary<InteractionType, IReadOnlyCollection<InteractionOption>> LoadInteractionOptions() {
			var result = new HashSet<InteractionOption>[EnumUtils.SizeOf<InteractionType>()];
			for (var i = 0; i < result.Length; ++i) result[i] = new HashSet<InteractionOption>();
			var csv = Resources.Load<TextAsset>("Sheets/interactionOptions");
			var columns = csv.CsvHeaderAsDictionary();
			foreach (var line in csv.CsvLines()) {
				if (!Enum.TryParse<InteractionType>(line[columns["Type"]], true, out var type)) {
					Debug.LogError("Interaction Option type not found: " + line[columns["Type"]]);
					continue;
				}
				var charged = string.Equals(line[columns["Charged"]], "1");
				var command = line[columns["Command"]];
				var endOfSentence = line.GetSafe(columns["EndOfSentence"]);
				result[(int)type].Add(new InteractionOption(command, type, endOfSentence, charged));
			}
			return result.Select((t, i) => (i, t)).ToDictionary(t => (InteractionType)t.i, t => (IReadOnlyCollection<InteractionOption>)t.t);
		}
	}
}