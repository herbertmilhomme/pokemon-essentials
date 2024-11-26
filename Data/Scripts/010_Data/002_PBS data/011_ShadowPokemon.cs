//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class ShadowPokemon {
		public int id		{ get { return _id; } }			protected int _id;
		public int species		{ get { return _species; } }			protected int _species;
		public int form		{ get { return _form; } }			protected int _form;
		public int moves		{ get { return _moves; } }			protected int _moves;
		public int gauge_size		{ get { return _gauge_size; } }			protected int _gauge_size;
		public int flags		{ get { return _flags; } }			protected int _flags;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "shadow_pokemon.dat";
		public const string PBS_BASE_FILENAME = "shadow_pokemon";
		public const bool OPTIONAL = true;
		SCHEMA = {
			"SectionName" => new {:id,         "eV", :Species},
			"GaugeSize"   => new {:gauge_size, "v"},
			"Moves"       => new {:moves,      "*e", :Move},
			"Flags"       => new {:flags,      "*s"}
		}
		public const int HEART_GAUGE_SIZE = 4000;   // Default gauge size

		extend ClassMethodsSymbols;
		include InstanceMethods;

		unless (singleton_class.method_defined(:__orig__load)) singleton_class.alias_method(:__orig__load, :load);
		public static void load() {
			if (FileTest.exist($"Data/{self.DATA_FILENAME}")) __orig__load;
		}

		/// <param name="species"> | Symbol, self, String</param>
		/// <param name="form"></param>
		// @return [self, null]
		public static void get_species_form(Symbol species, Integer form) {
			if (!species || !form) return null;
			validate species => [Symbol, self, String];
			validate form => Integer;
			if (species.is_a(self)) species = species.species;
			if (species.is_a(String)) species = species.to_sym;
			trial = string.Format("{0}_{0}", species, form).to_sym;
			species_form = (DATA[trial].null()) ? species : trial;
			return (DATA.has_key(species_form)) ? DATA[species_form] : null;
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id              = hash.id;
			@species         = hash.species         || @id;
			@form            = hash.form            || 0;
			@gauge_size      = hash.gauge_size      || HEART_GAUGE_SIZE;
			@moves           = hash.moves           || [];
			@flags           = hash.flags           || [];
			@s_file_suffix = hash.s_file_suffix || "";
		}

		public bool has_flag(flag) {
			return @flags.any(f => f.downcase == flag.downcase);
		}

		unless (method_defined(:__orig__get_property_for_PBS)) alias __orig__get_property_for_PBS get_property_for_PBS;
		public void get_property_for_PBS(key) {
			ret = __orig__get_property_for_PBS(key);
			switch (key) {
				case "SectionName":
					ret = new {@species, (@form > 0) ? @form : null};
					break;
			}
			return ret;
		}
	}
}
