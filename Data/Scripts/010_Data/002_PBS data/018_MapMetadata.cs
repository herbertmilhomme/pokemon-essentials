//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class MapMetadata {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int outdoor_map		{ get { return _outdoor_map; } }			protected int _outdoor_map;
		public int announce_location		{ get { return _announce_location; } }			protected int _announce_location;
		public int can_bicycle		{ get { return _can_bicycle; } }			protected int _can_bicycle;
		public int always_bicycle		{ get { return _always_bicycle; } }			protected int _always_bicycle;
		public int teleport_destination		{ get { return _teleport_destination; } }			protected int _teleport_destination;
		public int weather		{ get { return _weather; } }			protected int _weather;
		public int town_map_position		{ get { return _town_map_position; } }			protected int _town_map_position;
		public int dive_map_id		{ get { return _dive_map_id; } }			protected int _dive_map_id;
		public int dark_map		{ get { return _dark_map; } }			protected int _dark_map;
		public int safari_map		{ get { return _safari_map; } }			protected int _safari_map;
		public int snap_edges		{ get { return _snap_edges; } }			protected int _snap_edges;
		public int still_reflections		{ get { return _still_reflections; } }			protected int _still_reflections;
		public int random_dungeon		{ get { return _random_dungeon; } }			protected int _random_dungeon;
		public int battle_background		{ get { return _battle_background; } }			protected int _battle_background;
		public int wild_battle_BGM		{ get { return _wild_battle_BGM; } }			protected int _wild_battle_BGM;
		public int trainer_battle_BGM		{ get { return _trainer_battle_BGM; } }			protected int _trainer_battle_BGM;
		public int wild_victory_BGM		{ get { return _wild_victory_BGM; } }			protected int _wild_victory_BGM;
		public int trainer_victory_BGM		{ get { return _trainer_victory_BGM; } }			protected int _trainer_victory_BGM;
		public int wild_capture_ME		{ get { return _wild_capture_ME; } }			protected int _wild_capture_ME;
		public int town_map_size		{ get { return _town_map_size; } }			protected int _town_map_size;
		public int battle_environment		{ get { return _battle_environment; } }			protected int _battle_environment;
		public int flags		{ get { return _flags; } }			protected int _flags;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "map_metadata.dat";
		public const string PBS_BASE_FILENAME = "map_metadata";
		SCHEMA = {
			"SectionName"       => new {:id,                   "u"},
			"Name"              => new {:real_name,            "s"},
			"Outdoor"           => new {:outdoor_map,          "b"},
			"ShowArea"          => new {:announce_location,    "b"},
			"Bicycle"           => new {:can_bicycle,          "b"},
			"BicycleAlways"     => new {:always_bicycle,       "b"},
			"HealingSpot"       => new {:teleport_destination, "vuu"},
			"Weather"           => new {:weather,              "eu", :Weather},
			"MapPosition"       => new {:town_map_position,    "uuu"},
			"DiveMap"           => new {:dive_map_id,          "v"},
			"DarkMap"           => new {:dark_map,             "b"},
			"SafariMap"         => new {:safari_map,           "b"},
			"SnapEdges"         => new {:snap_edges,           "b"},
			"StillReflections"  => new {:still_reflections,    "b"},
			"Dungeon"           => new {:random_dungeon,       "b"},
			"BattleBack"        => new {:battle_background,    "s"},
			"WildBattleBGM"     => new {:wild_battle_BGM,      "s"},
			"TrainerBattleBGM"  => new {:trainer_battle_BGM,   "s"},
			"WildVictoryBGM"    => new {:wild_victory_BGM,     "s"},
			"TrainerVictoryBGM" => new {:trainer_victory_BGM,  "s"},
			"WildCaptureME"     => new {:wild_capture_ME,      "s"},
			"MapSize"           => new {:town_map_size,        "us"},
			"Environment"       => new {:battle_environment,   "e", :Environment},
			"Flags"             => new {:flags,                "*s"}
		}

		extend ClassMethodsIDNumbers;
		include InstanceMethods;

		public static void editor_properties() {
			return new {
				new {"ID",                ReadOnlyProperty,        _INTL("ID number of this map.")},
				new {"Name",              StringProperty,          _INTL("The name of the map, as seen by the player. Can be different to the map's name as seen in RMXP.")},
				new {"Outdoor",           BooleanProperty,         _INTL("If true, this map is an outdoor map and will be tinted according to time of day.")},
				new {"ShowArea",          BooleanProperty,         _INTL("If true, the game will display the map's name upon entry.")},
				new {"Bicycle",           BooleanProperty,         _INTL("If true, the bicycle can be used on this map.")},
				new {"BicycleAlways",     BooleanProperty,         _INTL("If true, the bicycle will be mounted automatically on this map and cannot be dismounted.")},
				new {"HealingSpot",       MapCoordsProperty,       _INTL("Map ID of this Pokémon Center's town, and X and Y coordinates of its entrance within that town.")},
				new {"Weather",           WeatherEffectProperty,   _INTL("Weather conditions in effect for this map.")},
				new {"MapPosition",       RegionMapCoordsProperty, _INTL("Identifies the point on the regional map for this map.")},
				new {"DiveMap",           MapProperty,             _INTL("Specifies the underwater layer of this map. Use only if this map has deep water.")},
				new {"DarkMap",           BooleanProperty,         _INTL("If true, this map is dark and a circle of light appears around the player. Flash can be used to expand the circle.")},
				new {"SafariMap",         BooleanProperty,         _INTL("If true, this map is part of the Safari Zone (both indoor and outdoor). Not to be used in the reception desk.")},
				new {"SnapEdges",         BooleanProperty,         _INTL("If true, when the player goes near this map's edge, the game doesn't center the player as usual.")},
				new {"StillReflections",  BooleanProperty,         _INTL("If true, reflections of events and the player will not ripple horizontally.")},
				new {"Dungeon",           BooleanProperty,         _INTL("If true, this map has a randomly generated layout. See the wiki for more information.")},
				new {"BattleBack",        StringProperty,          _INTL("PNG files named 'XXX_bg', 'XXX_base0', 'XXX_base1', 'XXX_message' in Battlebacks folder, where XXX is this property's value.")},
				new {"WildBattleBGM",     BGMProperty,             _INTL("Default BGM for wild Pokémon battles on this map.")},
				new {"TrainerBattleBGM",  BGMProperty,             _INTL("Default BGM for trainer battles on this map.")},
				new {"WildVictoryBGM",    BGMProperty,             _INTL("Default BGM played after winning a wild Pokémon battle on this map.")},
				new {"TrainerVictoryBGM", BGMProperty,             _INTL("Default BGM played after winning a Trainer battle on this map.")},
				new {"WildCaptureME",     MEProperty,              _INTL("Default ME played after catching a wild Pokémon on this map.")},
				new {"MapSize",           MapSizeProperty,         _INTL("The width of the map in Town Map squares, and a string indicating which squares are part of this map.")},
				new {"Environment",       new GameDataProperty(:Environment), _INTL("The default battle environment for battles on this map.")},
				new {"Flags",             StringListProperty,      _INTL("Words/phrases that distinguish this map from others.")}
			}
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id                   = hash.id;
			@real_name            = hash.real_name;
			@outdoor_map          = hash.outdoor_map;
			@announce_location    = hash.announce_location;
			@can_bicycle          = hash.can_bicycle;
			@always_bicycle       = hash.always_bicycle;
			@teleport_destination = hash.teleport_destination;
			@weather              = hash.weather;
			@town_map_position    = hash.town_map_position;
			@dive_map_id          = hash.dive_map_id;
			@dark_map             = hash.dark_map;
			@safari_map           = hash.safari_map;
			@snap_edges           = hash.snap_edges;
			@still_reflections    = hash.still_reflections;
			@random_dungeon       = hash.random_dungeon;
			@battle_background    = hash.battle_background;
			@wild_battle_BGM      = hash.wild_battle_BGM;
			@trainer_battle_BGM   = hash.trainer_battle_BGM;
			@wild_victory_BGM     = hash.wild_victory_BGM;
			@trainer_victory_BGM  = hash.trainer_victory_BGM;
			@wild_capture_ME      = hash.wild_capture_ME;
			@town_map_size        = hash.town_map_size;
			@battle_environment   = hash.battle_environment;
			@flags                = hash.flags           || [];
			@s_file_suffix      = hash.s_file_suffix || "";
		}

		// @return [String] the translated name of this map
		public void name() {
			ret = GetMessageFromHash(MessageTypes.MAP_NAMES, @real_name);
			if (nil_or_empty(ret)) ret = GetBasicMapNameFromId(@id);
			if (Game.GameData.player) ret = System.Text.RegularExpressions.Regex.Replace(ret, "\\PN", Game.GameData.player.name);
			return ret;
		}

		public bool has_flag(flag) {
			return @flags.any(f => f.downcase == flag.downcase);
		}

		unless (method_defined(:__orig__get_property_for_PBS)) alias __orig__get_property_for_PBS get_property_for_PBS;
		public void get_property_for_PBS(key) {
			if (key == "ID") key = "SectionName";
			return __orig__get_property_for_PBS(key);
		}
	}
}
