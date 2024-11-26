//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Habitat {
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

		// @return [String] the translated name of this habitat
		public void name() {
			return _INTL(@real_name);
		}
	}
}

//===============================================================================
//
//===============================================================================

GameData.Habitat.register({
	id   = :None,
	name = _INTL("None");
});

GameData.Habitat.register({
	id   = :Grassland,
	name = _INTL("Grassland");
});

GameData.Habitat.register({
	id   = :Forest,
	name = _INTL("Forest");
});

GameData.Habitat.register({
	id   = :WatersEdge,
	name = _INTL("Water's Edge");
});

GameData.Habitat.register({
	id   = :Sea,
	name = _INTL("Sea");
});

GameData.Habitat.register({
	id   = :Cave,
	name = _INTL("Cave");
});

GameData.Habitat.register({
	id   = :Mountain,
	name = _INTL("Mountain");
});

GameData.Habitat.register({
	id   = :RoughTerrain,
	name = _INTL("Rough Terrain");
});

GameData.Habitat.register({
	id   = :Urban,
	name = _INTL("Urban");
});

GameData.Habitat.register({
	id   = :Rare,
	name = _INTL("Rare");
});
