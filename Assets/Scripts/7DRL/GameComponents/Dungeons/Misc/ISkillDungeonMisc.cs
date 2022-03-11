using _7DRL.GameComponents.TextAndLetters;

namespace _7DRL.GameComponents.Dungeons.Misc {
	public interface ISkillDungeonMisc : IDungeonMisc {
		Command skillCommand { get; }
	}
}