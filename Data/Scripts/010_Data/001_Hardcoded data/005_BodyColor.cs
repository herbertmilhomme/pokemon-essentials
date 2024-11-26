//===============================================================================
// NOTE: The order these colors are registered are the order they are listed in
//       the Pokédex search screen.
//===============================================================================
public static partial class GameData {
	public partial class BodyColor {
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

		// @return [String] the translated name of this body color
		public void name() {
			return _INTL(@real_name);
		}
	}
}

//===============================================================================
//
//===============================================================================

GameData.BodyColor.register({
	id   = :Red,
	name = _INTL("Red");
});

GameData.BodyColor.register({
	id   = :Blue,
	name = _INTL("Blue");
});

GameData.BodyColor.register({
	id   = :Yellow,
	name = _INTL("Yellow");
});

GameData.BodyColor.register({
	id   = :Green,
	name = _INTL("Green");
});

GameData.BodyColor.register({
	id   = :Black,
	name = _INTL("Black");
});

GameData.BodyColor.register({
	id   = :Brown,
	name = _INTL("Brown");
});

GameData.BodyColor.register({
	id   = :Purple,
	name = _INTL("Purple");
});

GameData.BodyColor.register({
	id   = :Gray,
	name = _INTL("Gray");
});

GameData.BodyColor.register({
	id   = :White,
	name = _INTL("White");
});

GameData.BodyColor.register({
	id   = :Pink,
	name = _INTL("Pink");
});
