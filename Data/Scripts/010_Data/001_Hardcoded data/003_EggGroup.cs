//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class EggGroup {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;

		DATA = new List<string>();

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void load() { }
		public static void save() { }

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id        = hash.id;
			@real_name = hash.name || "Unnamed";
		}

		// @return [String] the translated name of this egg group
		public void name() {
			return _INTL(@real_name);
		}
	}
}

//===============================================================================
//
//===============================================================================

GameData.EggGroup.register({
	id   = :Undiscovered,
	name = _INTL("Undiscovered");
});

GameData.EggGroup.register({
	id   = :Monster,
	name = _INTL("Monster");
});

GameData.EggGroup.register({
	id   = :Water1,
	name = _INTL("Water 1");
});

GameData.EggGroup.register({
	id   = :Bug,
	name = _INTL("Bug");
});

GameData.EggGroup.register({
	id   = :Flying,
	name = _INTL("Flying");
});

GameData.EggGroup.register({
	id   = :Field,
	name = _INTL("Field");
});

GameData.EggGroup.register({
	id   = :Fairy,
	name = _INTL("Fairy");
});

GameData.EggGroup.register({
	id   = :Grass,
	name = _INTL("Grass");
});

GameData.EggGroup.register({
	id   = :Humanlike,
	name = _INTL("Humanlike");
});

GameData.EggGroup.register({
	id   = :Water3,
	name = _INTL("Water 3");
});

GameData.EggGroup.register({
	id   = :Mineral,
	name = _INTL("Mineral");
});

GameData.EggGroup.register({
	id   = :Amorphous,
	name = _INTL("Amorphous");
});

GameData.EggGroup.register({
	id   = :Water2,
	name = _INTL("Water 2");
});

GameData.EggGroup.register({
	id   = :Ditto,
	name = _INTL("Ditto");
});

GameData.EggGroup.register({
	id   = :Dragon,
	name = _INTL("Dragon");
});
