using System.Collections.Generic;
using System.Linq;
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
			return result;
		}

		public static IEnumerable<FoeType> LoadFoeTypes() {
			var commandsCsv = Resources.Load<TextAsset>("Sheets/foes");
			var columns = commandsCsv.CsvHeaderAsDictionary();
			return commandsCsv.CsvLines().Select(csvLine => new FoeType(csvLine[columns["Name"]]));
		}
	}
}