//===============================================================================
//
//===============================================================================
public partial class Battle.ActiveField {
	public int effects		{ get { return _effects; } set { _effects = value; } }			protected int _effects;
	public int defaultWeather		{ get { return _defaultWeather; } set { _defaultWeather = value; } }			protected int _defaultWeather;
	public int weather		{ get { return _weather; } set { _weather = value; } }			protected int _weather;
	public int weatherDuration		{ get { return _weatherDuration; } set { _weatherDuration = value; } }			protected int _weatherDuration;
	public int defaultTerrain		{ get { return _defaultTerrain; } set { _defaultTerrain = value; } }			protected int _defaultTerrain;
	public int terrain		{ get { return _terrain; } set { _terrain = value; } }			protected int _terrain;
	public int terrainDuration		{ get { return _terrainDuration; } set { _terrainDuration = value; } }			protected int _terrainDuration;

	public void initialize() {
		@effects = new List<string>();
		@effects.AmuletCoin      = false;
		@effects.FairyLock       = 0;
		@effects.FusionBolt      = false;
		@effects.FusionFlare     = false;
		@effects.Gravity         = 0;
		@effects.HappyHour       = false;
		@effects.IonDeluge       = false;
		@effects.MagicRoom       = 0;
		@effects.MudSportField   = 0;
		@effects.PayDay          = 0;
		@effects.TrickRoom       = 0;
		@effects.WaterSportField = 0;
		@effects.WonderRoom      = 0;
		@defaultWeather  = :None;
		@weather         = :None;
		@weatherDuration = 0;
		@defaultTerrain  = :None;
		@terrain         = :None;
		@terrainDuration = 0;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Battle.ActiveSide {
	public int effects		{ get { return _effects; } set { _effects = value; } }			protected int _effects;

	public void initialize() {
		@effects = new List<string>();
		@effects.AuroraVeil         = 0;
		@effects.CraftyShield       = false;
		@effects.EchoedVoiceCounter = 0;
		@effects.EchoedVoiceUsed    = false;
		@effects.LastRoundFainted   = -1;
		@effects.LightScreen        = 0;
		@effects.LuckyChant         = 0;
		@effects.MatBlock           = false;
		@effects.Mist               = 0;
		@effects.QuickGuard         = false;
		@effects.Rainbow            = 0;
		@effects.Reflect            = 0;
		@effects.Round              = false;
		@effects.Safeguard          = 0;
		@effects.SeaOfFire          = 0;
		@effects.Spikes             = 0;
		@effects.StealthRock        = false;
		@effects.StickyWeb          = false;
		@effects.Swamp              = 0;
		@effects.Tailwind           = 0;
		@effects.ToxicSpikes        = 0;
		@effects.WideGuard          = false;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Battle.ActivePosition {
	public int effects		{ get { return _effects; } set { _effects = value; } }			protected int _effects;

	public void initialize() {
		@effects = new List<string>();
		@effects.FutureSightCounter        = 0;
		@effects.FutureSightMove           = null;
		@effects.FutureSightUserIndex      = -1;
		@effects.FutureSightUserPartyIndex = -1;
		@effects.HealingWish               = false;
		@effects.LunarDance                = false;
		@effects.Wish                      = 0;
		@effects.WishAmount                = 0;
		@effects.WishMaker                 = -1;
	}
}
