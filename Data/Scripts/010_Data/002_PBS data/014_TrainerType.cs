//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class TrainerType {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int gender		{ get { return _gender; } }			protected int _gender;
		public int base_money		{ get { return _base_money; } }			protected int _base_money;
		public int skill_level		{ get { return _skill_level; } }			protected int _skill_level;
		public int poke_ball		{ get { return _poke_ball; } }			protected int _poke_ball;
		public int flags		{ get { return _flags; } }			protected int _flags;
		public int intro_BGM		{ get { return _intro_BGM; } }			protected int _intro_BGM;
		public int battle_BGM		{ get { return _battle_BGM; } }			protected int _battle_BGM;
		public int victory_BGM		{ get { return _victory_BGM; } }			protected int _victory_BGM;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "trainer_types.dat";
		public const string PBS_BASE_FILENAME = "trainer_types";
		SCHEMA = {
			"SectionName" => new {:id,          "m"},
			"Name"        => new {:real_name,   "s"},
			"Gender"      => new {:gender,      "e", {"Male" => 0, "male" => 0, "M" => 0, "m" => 0, "0" => 0,
																						"Female" => 1, "female" => 1, "F" => 1, "f" => 1, "1" => 1,
																						"Unknown" => 2, "unknown" => 2, "Other" => 2, "other" => 2,
																						"Mixed" => 2, "mixed" => 2, "X" => 2, "x" => 2, "2" => 2}},
			"BaseMoney"   => new {:base_money,  "u"},
			"SkillLevel"  => new {:skill_level, "u"},
			"PokeBall"    => new {:poke_ball,   "e", :Item},
			"Flags"       => new {:flags,       "*s"},
			"IntroBGM"    => new {:intro_BGM,   "s"},
			"BattleBGM"   => new {:battle_BGM,  "s"},
			"VictoryBGM"  => new {:victory_BGM, "s"}
		}

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void editor_properties() {
			gender_array = new List<string>();
			self.schema["Gender"][2].each((key, value) => { if (!gender_array[value]) gender_array[value] = key; });
			return new {
				new {"ID",         ReadOnlyProperty,               _INTL("ID of this Trainer Type (used as a symbol like :XXX).")},
				new {"Name",       StringProperty,                 _INTL("Name of this Trainer Type as displayed by the game.")},
				new {"Gender",     new EnumProperty(gender_array), _INTL("Gender of this Trainer Type.")},
				new {"BaseMoney",  new LimitProperty(9999),        _INTL("Player earns this much money times the highest level among the trainer's Pokémon.")},
				new {"SkillLevel", new LimitProperty2(9999),       _INTL("Skill level of this Trainer Type.")},
				new {"PokeBall",   ItemProperty,                   _INTL("Default Poké Ball that all Pokémon of trainers of this Trainer Type are in.")},
				new {"Flags",      StringListProperty,             _INTL("Words/phrases that can be used to make trainers of this type behave differently to others.")},
				new {"IntroBGM",   BGMProperty,                    _INTL("BGM played before battles against trainers of this type.")},
				new {"BattleBGM",  BGMProperty,                    _INTL("BGM played in battles against trainers of this type.")},
				new {"VictoryBGM", BGMProperty,                    _INTL("BGM played when player wins battles against trainers of this type.")}
			}
		}

		public static void check_file(tr_type, path, optional_suffix = "", suffix = "") {
			tr_type_data = self.try_get(tr_type);
			if (tr_type_data.null()) return null;
			// Check for files
			if (optional_suffix && !optional_suffix.empty()) {
				ret = path + tr_type_data.id.ToString() + optional_suffix + suffix;
				if (ResolveBitmap(ret)) return ret;
			}
			ret = path + tr_type_data.id.ToString() + suffix;
			return (ResolveBitmap(ret)) ? ret : null;
		}

		public static void charset_filename(tr_type) {
			return self.check_file(tr_type, "Graphics/Characters/trainer_");
		}

		public static void charset_filename_brief(tr_type) {
			ret = self.charset_filename(tr_type);
			ret&.slice!("Graphics/Characters/");
			return ret;
		}

		public static void front_sprite_filename(tr_type) {
			return self.check_file(tr_type, "Graphics/Trainers/");
		}

		public static void player_front_sprite_filename(tr_type) {
			outfit = (Game.GameData.player) ? Game.GameData.player.outfit : 0;
			return self.check_file(tr_type, "Graphics/Trainers/", string.Format("_{0}", outfit));
		}

		public static void back_sprite_filename(tr_type) {
			return self.check_file(tr_type, "Graphics/Trainers/", "", "_back");
		}

		public static void player_back_sprite_filename(tr_type) {
			outfit = (Game.GameData.player) ? Game.GameData.player.outfit : 0;
			return self.check_file(tr_type, "Graphics/Trainers/", string.Format("_{0}", outfit), "_back");
		}

		public static void map_icon_filename(tr_type) {
			return self.check_file(tr_type, "Graphics/UI/Town Map/player_");
		}

		public static void player_map_icon_filename(tr_type) {
			outfit = (Game.GameData.player) ? Game.GameData.player.outfit : 0;
			return self.check_file(tr_type, "Graphics/UI/Town Map/player_", string.Format("_{0}", outfit));
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id              = hash.id;
			@real_name       = hash.real_name       || "Unnamed";
			@gender          = hash.gender          || 2;
			@base_money      = hash.base_money      || 30;
			@skill_level     = hash.skill_level     || @base_money;
			@poke_ball       = hash.poke_ball;
			@flags           = hash.flags           || [];
			@intro_BGM       = hash.intro_BGM;
			@battle_BGM      = hash.battle_BGM;
			@victory_BGM     = hash.victory_BGM;
			@s_file_suffix = hash.s_file_suffix || "";
		}

		// @return [String] the translated name of this trainer type
		public void name() {
			return GetMessageFromHash(MessageTypes.TRAINER_TYPE_NAMES, @real_name);
		}

		public bool male() {   return @gender == 0; }
		public bool female() { return @gender == 1; }

		public bool has_flag(flag) {
			return @flags.any(f => f.downcase == flag.downcase);
		}

		unless (method_defined(:__orig__get_property_for_PBS)) alias __orig__get_property_for_PBS get_property_for_PBS;
		public void get_property_for_PBS(key) {
			if (key == "ID") key = "SectionName";
			ret = __orig__get_property_for_PBS(key);
			if (key == "SkillLevel" && ret == @base_money) ret = null;
			return ret;
		}
	}
}
