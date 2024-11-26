//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class PlayerMetadata {
		public int id		{ get { return _id; } }			protected int _id;
		public int trainer_type		{ get { return _trainer_type; } }			protected int _trainer_type;
		public int walk_charset		{ get { return _walk_charset; } }			protected int _walk_charset;
		public int home		{ get { return _home; } }			protected int _home;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "player_metadata.dat";
		SCHEMA = {
			"SectionName"     => new {:id,                "u"},
			"TrainerType"     => new {:trainer_type,      "e", :TrainerType},
			"WalkCharset"     => new {:walk_charset,      "s"},
			"RunCharset"      => new {:run_charset,       "s"},
			"CycleCharset"    => new {:cycle_charset,     "s"},
			"SurfCharset"     => new {:surf_charset,      "s"},
			"DiveCharset"     => new {:dive_charset,      "s"},
			"FishCharset"     => new {:fish_charset,      "s"},
			"SurfFishCharset" => new {:surf_fish_charset, "s"},
			"Home"            => new {:home,              "vuuu"}
		}

		extend ClassMethodsIDNumbers;
		include InstanceMethods;

		public static void editor_properties() {
			return new {
				new {"ID",              ReadOnlyProperty,        _INTL("ID number of this player.")},
				new {"TrainerType",     TrainerTypeProperty,     _INTL("Trainer type of this player.")},
				new {"WalkCharset",     CharacterProperty,       _INTL("Charset used while the player is still or walking.")},
				new {"RunCharset",      CharacterProperty,       _INTL("Charset used while the player is running. Uses WalkCharset if undefined.")},
				new {"CycleCharset",    CharacterProperty,       _INTL("Charset used while the player is cycling. Uses RunCharset if undefined.")},
				new {"SurfCharset",     CharacterProperty,       _INTL("Charset used while the player is surfing. Uses CycleCharset if undefined.")},
				new {"DiveCharset",     CharacterProperty,       _INTL("Charset used while the player is diving. Uses SurfCharset if undefined.")},
				new {"FishCharset",     CharacterProperty,       _INTL("Charset used while the player is fishing. Uses WalkCharset if undefined.")},
				new {"SurfFishCharset", CharacterProperty,       _INTL("Charset used while the player is fishing while surfing. Uses FishCharset if undefined.")},
				new {"Home",            MapCoordsFacingProperty, _INTL("Map ID and X/Y coordinates of where the player goes after a loss if no Pokémon Center was visited.")}
			}
		}

		/// <param name="player_id"></param>
		// @return [self, null]
		public static void get(Integer player_id = 1) {
			validate player_id => Integer;
			if (self.DATA.has_key(player_id)) return self.DATA[player_id];
			return self.DATA[1];
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id                = hash.id;
			@trainer_type      = hash.trainer_type;
			@walk_charset      = hash.walk_charset;
			@run_charset       = hash.run_charset;
			@cycle_charset     = hash.cycle_charset;
			@surf_charset      = hash.surf_charset;
			@dive_charset      = hash.dive_charset;
			@fish_charset      = hash.fish_charset;
			@surf_fish_charset = hash.surf_fish_charset;
			@home              = hash.home;
			@s_file_suffix   = hash.s_file_suffix || "";
		}

		public void run_charset() {
			return @run_charset || @walk_charset;
		}

		public void cycle_charset() {
			return @cycle_charset || run_charset;
		}

		public void surf_charset() {
			return @surf_charset || cycle_charset;
		}

		public void dive_charset() {
			return @dive_charset || surf_charset;
		}

		public void fish_charset() {
			return @fish_charset || @walk_charset;
		}

		public void surf_fish_charset() {
			return @surf_fish_charset || fish_charset;
		}

		unless (method_defined(:__orig__get_property_for_PBS)) alias __orig__get_property_for_PBS get_property_for_PBS;
		public void get_property_for_PBS(key) {
			if (key == "ID") key = "SectionName";
			return __orig__get_property_for_PBS(key);
		}
	}
}
