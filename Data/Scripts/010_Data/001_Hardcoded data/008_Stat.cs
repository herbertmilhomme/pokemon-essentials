//===============================================================================
// The s_order value determines the order in which the stats are written in
// several PBS files, where base stats/IVs/EVs/EV yields are defined. Only stats
// which are yielded by the "each_main" method can have stat numbers defined in
// those places. The values of s_order defined below should start with 0 and
// increase without skipping any numbers.
//===============================================================================
public static partial class GameData {
	public partial class Stat {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int real_name_brief		{ get { return _real_name_brief; } }			protected int _real_name_brief;
		public int type		{ get { return _type; } }			protected int _type;
		public int s_order		{ get { return _pbs_order; } }			protected int _pbs_order;

		DATA = new List<string>();

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void load() { }
		public static void save() { }

		// These stats are defined in PBS files, and should have the :s_order
		// property.
		public static void each_main() {
			if (new []{:main, :main_battle}.Contains(s.type) }) self.each { |s| yield s;
		}

		public static void each_main_battle() {
			self.each(s => { if ([:main_battle].Contains(s.type)) yield s; });
		}

		// These stats have associated stat stages in battle.
		public static void each_battle() {
			if (new []{:main_battle, :battle}.Contains(s.type) }) self.each { |s| yield s;
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id              = hash.id;
			@real_name       = hash.name       || "Unnamed";
			@real_name_brief = hash.name_brief || "None";
			@type            = hash.type       || :none;
			@s_order       = hash.s_order  || -1;
		}

		// @return [String] the translated name of this stat
		public void name() {
			return _INTL(@real_name);
		}

		// @return [String] the translated brief name of this stat
		public void name_brief() {
			return _INTL(@real_name_brief);
		}
	}
}

//===============================================================================
//
//===============================================================================

GameData.Stat.register({
	id         = :HP,
	name       = _INTL("HP"),
	name_brief = _INTL("HP"),
	type       = :main,
	s_order  = 0;
});

GameData.Stat.register({
	id         = :ATTACK,
	name       = _INTL("Attack"),
	name_brief = _INTL("Atk"),
	type       = :main_battle,
	s_order  = 1;
});

GameData.Stat.register({
	id         = :DEFENSE,
	name       = _INTL("Defense"),
	name_brief = _INTL("Def"),
	type       = :main_battle,
	s_order  = 2;
});

GameData.Stat.register({
	id         = :SPECIAL_ATTACK,
	name       = _INTL("Special Attack"),
	name_brief = _INTL("SpAtk"),
	type       = :main_battle,
	s_order  = 4;
});

GameData.Stat.register({
	id         = :SPECIAL_DEFENSE,
	name       = _INTL("Special Defense"),
	name_brief = _INTL("SpDef"),
	type       = :main_battle,
	s_order  = 5;
});

GameData.Stat.register({
	id         = :SPEED,
	name       = _INTL("Speed"),
	name_brief = _INTL("Spd"),
	type       = :main_battle,
	s_order  = 3;
});

GameData.Stat.register({
	id         = :ACCURACY,
	name       = _INTL("accuracy"),
	name_brief = _INTL("Acc"),
	type       = :battle;
});

GameData.Stat.register({
	id         = :EVASION,
	name       = _INTL("evasiveness"),
	name_brief = _INTL("Eva"),
	type       = :battle;
});
