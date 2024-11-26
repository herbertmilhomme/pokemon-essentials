//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Nature {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int stat_changes		{ get { return _stat_changes; } }			protected int _stat_changes;

		DATA = new List<string>();

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void load() { }
		public static void save() { }

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id           = hash.id;
			@real_name    = hash.name         || "Unnamed";
			@stat_changes = hash.stat_changes || [];
		}

		// @return [String] the translated name of this nature
		public void name() {
			return _INTL(@real_name);
		}
	}
}

//===============================================================================
//
//===============================================================================

GameData.Nature.register({
	id           = :HARDY,
	name         = _INTL("Hardy");
});

GameData.Nature.register({
	id           = :LONELY,
	name         = _INTL("Lonely"),
	stat_changes = new {new {:ATTACK, 10}, new {:DEFENSE, -10}}
});

GameData.Nature.register({
	id           = :BRAVE,
	name         = _INTL("Brave"),
	stat_changes = new {new {:ATTACK, 10}, new {:SPEED, -10}}
});

GameData.Nature.register({
	id           = :ADAMANT,
	name         = _INTL("Adamant"),
	stat_changes = new {new {:ATTACK, 10}, new {:SPECIAL_ATTACK, -10}}
});

GameData.Nature.register({
	id           = :NAUGHTY,
	name         = _INTL("Naughty"),
	stat_changes = new {new {:ATTACK, 10}, new {:SPECIAL_DEFENSE, -10}}
});

GameData.Nature.register({
	id           = :BOLD,
	name         = _INTL("Bold"),
	stat_changes = new {new {:DEFENSE, 10}, new {:ATTACK, -10}}
});

GameData.Nature.register({
	id           = :DOCILE,
	name         = _INTL("Docile");
});

GameData.Nature.register({
	id           = :RELAXED,
	name         = _INTL("Relaxed"),
	stat_changes = new {new {:DEFENSE, 10}, new {:SPEED, -10}}
});

GameData.Nature.register({
	id           = :IMPISH,
	name         = _INTL("Impish"),
	stat_changes = new {new {:DEFENSE, 10}, new {:SPECIAL_ATTACK, -10}}
});

GameData.Nature.register({
	id           = :LAX,
	name         = _INTL("Lax"),
	stat_changes = new {new {:DEFENSE, 10}, new {:SPECIAL_DEFENSE, -10}}
});

GameData.Nature.register({
	id           = :TIMID,
	name         = _INTL("Timid"),
	stat_changes = new {new {:SPEED, 10}, new {:ATTACK, -10}}
});

GameData.Nature.register({
	id           = :HASTY,
	name         = _INTL("Hasty"),
	stat_changes = new {new {:SPEED, 10}, new {:DEFENSE, -10}}
});

GameData.Nature.register({
	id           = :SERIOUS,
	name         = _INTL("Serious");
});

GameData.Nature.register({
	id           = :JOLLY,
	name         = _INTL("Jolly"),
	stat_changes = new {new {:SPEED, 10}, new {:SPECIAL_ATTACK, -10}}
});

GameData.Nature.register({
	id           = :NAIVE,
	name         = _INTL("Naive"),
	stat_changes = new {new {:SPEED, 10}, new {:SPECIAL_DEFENSE, -10}}
});

GameData.Nature.register({
	id           = :MODEST,
	name         = _INTL("Modest"),
	stat_changes = new {new {:SPECIAL_ATTACK, 10}, new {:ATTACK, -10}}
});

GameData.Nature.register({
	id           = :MILD,
	name         = _INTL("Mild"),
	stat_changes = new {new {:SPECIAL_ATTACK, 10}, new {:DEFENSE, -10}}
});

GameData.Nature.register({
	id           = :QUIET,
	name         = _INTL("Quiet"),
	stat_changes = new {new {:SPECIAL_ATTACK, 10}, new {:SPEED, -10}}
});

GameData.Nature.register({
	id           = :BASHFUL,
	name         = _INTL("Bashful");
});

GameData.Nature.register({
	id           = :RASH,
	name         = _INTL("Rash"),
	stat_changes = new {new {:SPECIAL_ATTACK, 10}, new {:SPECIAL_DEFENSE, -10}}
});

GameData.Nature.register({
	id           = :CALM,
	name         = _INTL("Calm"),
	stat_changes = new {new {:SPECIAL_DEFENSE, 10}, new {:ATTACK, -10}}
});

GameData.Nature.register({
	id           = :GENTLE,
	name         = _INTL("Gentle"),
	stat_changes = new {new {:SPECIAL_DEFENSE, 10}, new {:DEFENSE, -10}}
});

GameData.Nature.register({
	id           = :SASSY,
	name         = _INTL("Sassy"),
	stat_changes = new {new {:SPECIAL_DEFENSE, 10}, new {:SPEED, -10}}
});

GameData.Nature.register({
	id           = :CAREFUL,
	name         = _INTL("Careful"),
	stat_changes = new {new {:SPECIAL_DEFENSE, 10}, new {:SPECIAL_ATTACK, -10}}
});

GameData.Nature.register({
	id           = :QUIRKY,
	name         = _INTL("Quirky");
});
