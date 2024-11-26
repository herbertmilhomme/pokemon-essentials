//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Metadata {
		public int id		{ get { return _id; } }			protected int _id;
		public int start_money		{ get { return _start_money; } }			protected int _start_money;
		public int start_item_storage		{ get { return _start_item_storage; } }			protected int _start_item_storage;
		public int home		{ get { return _home; } }			protected int _home;
		public int real_storage_creator		{ get { return _real_storage_creator; } }			protected int _real_storage_creator;
		public int wild_battle_BGM		{ get { return _wild_battle_BGM; } }			protected int _wild_battle_BGM;
		public int trainer_battle_BGM		{ get { return _trainer_battle_BGM; } }			protected int _trainer_battle_BGM;
		public int wild_victory_BGM		{ get { return _wild_victory_BGM; } }			protected int _wild_victory_BGM;
		public int trainer_victory_BGM		{ get { return _trainer_victory_BGM; } }			protected int _trainer_victory_BGM;
		public int wild_capture_ME		{ get { return _wild_capture_ME; } }			protected int _wild_capture_ME;
		public int surf_BGM		{ get { return _surf_BGM; } }			protected int _surf_BGM;
		public int bicycle_BGM		{ get { return _bicycle_BGM; } }			protected int _bicycle_BGM;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "metadata.dat";
		public const string PBS_BASE_FILENAME = "metadata";
		SCHEMA = {
			"SectionName"       => new {:id,                   "u"},
			"StartMoney"        => new {:start_money,          "u"},
			"StartItemStorage"  => new {:start_item_storage,   "*e", :Item},
			"Home"              => new {:home,                 "vuuu"},
			"StorageCreator"    => new {:real_storage_creator, "s"},
			"WildBattleBGM"     => new {:wild_battle_BGM,      "s"},
			"TrainerBattleBGM"  => new {:trainer_battle_BGM,   "s"},
			"WildVictoryBGM"    => new {:wild_victory_BGM,     "s"},
			"TrainerVictoryBGM" => new {:trainer_victory_BGM,  "s"},
			"WildCaptureME"     => new {:wild_capture_ME,      "s"},
			"SurfBGM"           => new {:surf_BGM,             "s"},
			"BicycleBGM"        => new {:bicycle_BGM,          "s"}
		}

		extend ClassMethodsIDNumbers;
		include InstanceMethods;

		public static void editor_properties() {
			return new {
				new {"StartMoney",        new LimitProperty(Settings.MAX_MONEY), _INTL("The amount of money that the player starts the game with.")},
				new {"StartItemStorage",  new GameDataPoolProperty(:Item),        _INTL("Items that are already in the player's PC at the start of the game.")},
				new {"Home",              MapCoordsFacingProperty, _INTL("Map ID and X/Y coordinates of where the player goes after a loss if no Pokémon Center was visited.")},
				new {"StorageCreator",    StringProperty,          _INTL("Name of the Pokémon Storage creator (the storage option is named \"XXX's PC\").")},
				new {"WildBattleBGM",     BGMProperty,             _INTL("Default BGM for wild Pokémon battles.")},
				new {"TrainerBattleBGM",  BGMProperty,             _INTL("Default BGM for Trainer battles.")},
				new {"WildVictoryBGM",    BGMProperty,             _INTL("Default BGM played after winning a wild Pokémon battle.")},
				new {"TrainerVictoryBGM", BGMProperty,             _INTL("Default BGM played after winning a Trainer battle.")},
				new {"WildCaptureME",     MEProperty,              _INTL("Default ME played after catching a Pokémon.")},
				new {"SurfBGM",           BGMProperty,             _INTL("BGM played while surfing.")},
				new {"BicycleBGM",        BGMProperty,             _INTL("BGM played while on a bicycle.")}
			}
		}

		public static void get() {
			return DATA[0];
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id                   = hash.id                 || 0;
			@start_money          = hash.start_money        || 3000;
			@start_item_storage   = hash.start_item_storage || [];
			@home                 = hash.home;
			@real_storage_creator = hash.real_storage_creator;
			@wild_battle_BGM      = hash.wild_battle_BGM;
			@trainer_battle_BGM   = hash.trainer_battle_BGM;
			@wild_victory_BGM     = hash.wild_victory_BGM;
			@trainer_victory_BGM  = hash.trainer_victory_BGM;
			@wild_capture_ME      = hash.wild_capture_ME;
			@surf_BGM             = hash.surf_BGM;
			@bicycle_BGM          = hash.bicycle_BGM;
			@s_file_suffix      = hash.s_file_suffix    || "";
		}

		// @return [String] the translated name of the Pokémon Storage creator
		public void storage_creator() {
			ret = GetMessageFromHash(MessageTypes.STORAGE_CREATOR_NAME, @real_storage_creator);
			return nil_or_empty(ret) ? _INTL("Bill") : ret;
		}
	}
}
