//===============================================================================
// These are in-battle terrain effects caused by moves like Electric Terrain.
//===============================================================================
public static partial class GameData {
	public partial class BattleTerrain {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int animation		{ get { return _animation; } }			protected int _animation;

		DATA = new List<string>();

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void load() { }
		public static void save() { }

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id        = hash.id;
			@real_name = hash.name || "Unnamed";
			@animation = hash.animation;
		}

		// @return [String] the translated name of this battle terrain
		public void name() {
			return _INTL(@real_name);
		}
	}
}

//===============================================================================
//
//===============================================================================

GameData.BattleTerrain.register({
	id   = :None,
	name = _INTL("None");
});

GameData.BattleTerrain.register({
	id        = :Electric,
	name      = _INTL("Electric"),
	animation = "ElectricTerrain";
});

GameData.BattleTerrain.register({
	id        = :Grassy,
	name      = _INTL("Grassy"),
	animation = "GrassyTerrain";
});

GameData.BattleTerrain.register({
	id        = :Misty,
	name      = _INTL("Misty"),
	animation = "MistyTerrain";
});

GameData.BattleTerrain.register({
	id        = :Psychic,
	name      = _INTL("Psychic"),
	animation = "PsychicTerrain";
});
