using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _7DRL.Data;
using _7DRL.Data.IntroScript;
using _7DRL.GameComponents.Characters;
using _7DRL.GameComponents.Dungeons;
using _7DRL.GameComponents.Interactions;
using _7DRL.GameComponents.TextAndLetters;
using UnityEngine;

namespace _7DRL.MiscConstants {
	public class Memory : MonoBehaviour {
		private static Memory instance { get; set; }

		[SerializeField] protected CommandType _attackCommandType;
		[SerializeField] protected CommandType _defenseCommandType;
		[SerializeField] protected CommandType _healCommandType;
		[SerializeField] protected CommandType _dodgeCommandType;
		[SerializeField] protected CommandType _escapeCommandType;
		[SerializeField] protected CommandType _moveNorthCommandType;
		[SerializeField] protected CommandType _moveSouthCommandType;
		[SerializeField] protected CommandType _moveEastCommandType;
		[SerializeField] protected CommandType _moveWestCommandType;
		[SerializeField] protected CommandType _restCommandType;

		public static class CommandTypes {
			public static CommandType attack    => instance._attackCommandType;
			public static CommandType defense   => instance._defenseCommandType;
			public static CommandType heal      => instance._healCommandType;
			public static CommandType dodge     => instance._dodgeCommandType;
			public static CommandType escape    => instance._escapeCommandType;
			public static CommandType moveNorth => instance._moveNorthCommandType;
			public static CommandType moveSouth => instance._moveSouthCommandType;
			public static CommandType moveWest  => instance._moveWestCommandType;
			public static CommandType moveEast  => instance._moveEastCommandType;
			public static CommandType rest      => instance._restCommandType;
		}

		private void Awake() => instance = this;

		public static IReadOnlyList<CommandType>                                                   commandTypes          { get; private set; }
		public static IReadOnlyList<Command>                                                       commands              { get; private set; }
		public static IReadOnlyDictionary<CommandType, IReadOnlyList<Command>>                     commandsPerType       { get; private set; }
		public static IReadOnlyList<FoeType>                                                       foeTypes              { get; private set; }
		public static IReadOnlyDictionary<InteractionType, IReadOnlyCollection<InteractionOption>> interactionOptions    { get; private set; }
		public static BookNameGenerator                                                            bookNameGenerator     { get; private set; }
		public static ChestContentGenerator                                                        chestContentGenerator { get; private set; }
		public static IReadOnlyList<IntroScriptLine>                                               introScriptLines      { get; private set; }

		public static IEnumerator Load() {
			commands = DataFactory.LoadCommands().ToArray();
			yield return null;
			commandTypes = commands.Select(t => t.type).Distinct().ToList();
			yield return null;
			commandsPerType = commandTypes.ToDictionary(t => t, t => (IReadOnlyList<Command>)commands.Where(command => command.type == t).ToList());
			yield return null;
			foeTypes = DataFactory.LoadFoeTypes().ToList();
			yield return null;
			bookNameGenerator = DataFactory.LoadBookNameGenerator();
			yield return null;
			chestContentGenerator = DataFactory.LoadChestContentGenerator();
			yield return null;
			interactionOptions = DataFactory.LoadInteractionOptions();
			yield return null;
			introScriptLines = DataFactory.LoadIntroScriptLines();
			yield return null;
		}
	}
}