//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class BattleWeather {
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

		// @return [String] the translated name of this battle weather
		public void name() {
			return _INTL(@real_name);
		}
	}
}

//===============================================================================
//
//===============================================================================

GameData.BattleWeather.register({
	id   = :None,
	name = _INTL("None");
});

GameData.BattleWeather.register({
	id        = :Sun,
	name      = _INTL("Sun"),
	animation = "Sun";
});

GameData.BattleWeather.register({
	id        = :Rain,
	name      = _INTL("Rain"),
	animation = "Rain";
});

GameData.BattleWeather.register({
	id        = :Sandstorm,
	name      = _INTL("Sandstorm"),
	animation = "Sandstorm";
});

GameData.BattleWeather.register({
	id        = :Hail,
	name      = _INTL("Hail"),
	animation = "Hail";
});

GameData.BattleWeather.register({
	id        = :Snowstorm,
	name      = _INTL("Snowstorm"),
	animation = "Snowstorm";
});

GameData.BattleWeather.register({
	id        = :HarshSun,
	name      = _INTL("Harsh Sun"),
	animation = "HarshSun";
});

GameData.BattleWeather.register({
	id        = :HeavyRain,
	name      = _INTL("Heavy Rain"),
	animation = "HeavyRain";
});

GameData.BattleWeather.register({
	id        = :StrongWinds,
	name      = _INTL("Strong Winds"),
	animation = "StrongWinds";
});

GameData.BattleWeather.register({
	id        = :ShadowSky,
	name      = _INTL("Shadow Sky"),
	animation = "ShadowSky";
});
