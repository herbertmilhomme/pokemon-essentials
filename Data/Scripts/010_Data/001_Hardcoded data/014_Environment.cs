//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Environment {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int battle_base		{ get { return _battle_base; } }			protected int _battle_base;

		DATA = new List<string>();

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void load() { }
		public static void save() { }

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id          = hash.id;
			@real_name   = hash.name || "Unnamed";
			@battle_base = hash.battle_base;
		}

		// @return [String] the translated name of this environment
		public void name() {
			return _INTL(@real_name);
		}
	}
}

//===============================================================================
//
//===============================================================================

GameData.Environment.register({
	id   = :None,
	name = _INTL("None");
});

GameData.Environment.register({
	id          = :Grass,
	name        = _INTL("Grass"),
	battle_base = "grass";
});

GameData.Environment.register({
	id          = :TallGrass,
	name        = _INTL("Tall grass"),
	battle_base = "grass";
});

GameData.Environment.register({
	id          = :MovingWater,
	name        = _INTL("Moving water"),
	battle_base = "water";
});

GameData.Environment.register({
	id          = :StillWater,
	name        = _INTL("Still water"),
	battle_base = "water";
});

GameData.Environment.register({
	id          = :Puddle,
	name        = _INTL("Puddle"),
	battle_base = "puddle";
});

GameData.Environment.register({
	id   = :Underwater,
	name = _INTL("Underwater");
});

GameData.Environment.register({
	id   = :Cave,
	name = _INTL("Cave");
});

GameData.Environment.register({
	id   = :Rock,
	name = _INTL("Rock");
});

GameData.Environment.register({
	id          = :Sand,
	name        = _INTL("Sand"),
	battle_base = "sand";
});

GameData.Environment.register({
	id   = :Forest,
	name = _INTL("Forest");
});

GameData.Environment.register({
	id          = :ForestGrass,
	name        = _INTL("Forest grass"),
	battle_base = "grass";
});

GameData.Environment.register({
	id   = :Snow,
	name = _INTL("Snow");
});

GameData.Environment.register({
	id          = :Ice,
	name        = _INTL("Ice"),
	battle_base = "ice";
});

GameData.Environment.register({
	id   = :Volcano,
	name = _INTL("Volcano");
});

GameData.Environment.register({
	id   = :Graveyard,
	name = _INTL("Graveyard");
});

GameData.Environment.register({
	id   = :Sky,
	name = _INTL("Sky");
});

GameData.Environment.register({
	id   = :Space,
	name = _INTL("Space");
});

GameData.Environment.register({
	id   = :UltraSpace,
	name = _INTL("Ultra Space");
});
