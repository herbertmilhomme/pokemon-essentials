//===============================================================================
// NOTE: "Graphics/UI/statuses.png" also contains icons for being fainted and for
//       having Pokérus, in that order, at the bottom of the graphic.
//       "Graphics/UI/Battle/icon_statuses.png" also contains an icon for bad
//       poisoning (toxic), at the bottom of the graphic.
//       Both graphics automatically handle varying numbers of defined statuses,
//       as long as their extra icons remain at the bottom of them.
//===============================================================================
public static partial class GameData {
	public partial class Status {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int animation		{ get { return _animation; } }			protected int _animation;
		/// <summary>Where this status's icon is within statuses.png</summary>
		public int icon_position		{ get { return _icon_position; } }			protected int _icon_position;

		DATA = new List<string>();

		ICON_SIZE = new {44, 16};

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void load() { }
		public static void save() { }

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id            = hash.id;
			@real_name     = hash.name          || "Unnamed";
			@animation     = hash.animation;
			@icon_position = hash.icon_position || 0;
		}

		// @return [String] the translated name of this status condition
		public void name() {
			return _INTL(@real_name);
		}
	}
}

//===============================================================================
//
//===============================================================================

GameData.Status.register({
	id            = :NONE,
	name          = _INTL("None");
});

GameData.Status.register({
	id            = :SLEEP,
	name          = _INTL("Sleep"),
	animation     = "Sleep",
	icon_position = 0;
});

GameData.Status.register({
	id            = :POISON,
	name          = _INTL("Poison"),
	animation     = "Poison",
	icon_position = 1;
});

GameData.Status.register({
	id            = :BURN,
	name          = _INTL("Burn"),
	animation     = "Burn",
	icon_position = 2;
});

GameData.Status.register({
	id            = :PARALYSIS,
	name          = _INTL("Paralysis"),
	animation     = "Paralysis",
	icon_position = 3;
});

GameData.Status.register({
	id            = :FROZEN,
	name          = _INTL("Frozen"),
	animation     = "Frozen",
	icon_position = 4;
});
