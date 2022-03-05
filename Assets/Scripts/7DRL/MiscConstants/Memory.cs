using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _7DRL.Data;
using _7DRL.MiscConstants;

namespace _7DRL {
	public static class Memory {
		public static IReadOnlyList<CommandType> commandTypes    { get; private set; }
		public static IReadOnlyList<Command>     commands        { get; private set; }
		public static IReadOnlyList<Command>     attackCommands  { get; private set; }
		public static IReadOnlyList<Command>     defenseCommands { get; private set; }
		public static IReadOnlyList<Command>     dodgeCommands   { get; private set; }
		public static IReadOnlyList<Command>     healCommands    { get; private set; }
		public static IReadOnlyList<FoeType>     foeTypes        { get; private set; }

		public static IEnumerator Load() {
			commands = DataFactory.LoadCommands().ToArray();
			attackCommands = commands.Where(t => t.type.name == Constants.commandTypeAttack).ToArray();
			defenseCommands = commands.Where(t => t.type.name == Constants.commandTypeDefense).ToArray();
			dodgeCommands = commands.Where(t => t.type.name == Constants.commandTypeDodge).ToArray();
			healCommands = commands.Where(t => t.type.name == Constants.commandTypeHeal).ToArray();
			yield return null;
			commandTypes = commands.Select(t => t.type).Distinct().ToList();
			yield return null;
			foeTypes = DataFactory.LoadFoeTypes().ToList();
			yield return null;
		}
	}
}