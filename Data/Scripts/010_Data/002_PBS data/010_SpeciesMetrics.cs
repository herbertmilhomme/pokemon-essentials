//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class SpeciesMetrics {
		public int id		{ get { return _id; } }			protected int _id;
		public int species		{ get { return _species; } }			protected int _species;
		public int form		{ get { return _form; } }			protected int _form;
		public int back_sprite		{ get { return _back_sprite; } set { _back_sprite = value; } }			protected int _back_sprite;
		public int front_sprite		{ get { return _front_sprite; } set { _front_sprite = value; } }			protected int _front_sprite;
		public int front_sprite_altitude		{ get { return _front_sprite_altitude; } set { _front_sprite_altitude = value; } }			protected int _front_sprite_altitude;
		public int shadow_x		{ get { return _shadow_x; } set { _shadow_x = value; } }			protected int _shadow_x;
		public int shadow_size		{ get { return _shadow_size; } set { _shadow_size = value; } }			protected int _shadow_size;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "species_metrics.dat";
		public const string PBS_BASE_FILENAME = "pokemon_metrics";
		SCHEMA = {
			"SectionName"         => new {:id,                    "eV", :Species},
			"BackSprite"          => new {:back_sprite,           "ii"},
			"FrontSprite"         => new {:front_sprite,          "ii"},
			"FrontSpriteAltitude" => new {:front_sprite_altitude, "i"},
			"ShadowX"             => new {:shadow_x,              "i"},
			"ShadowSize"          => new {:shadow_size,           "u"}
		}

		extend ClassMethodsSymbols;
		include InstanceMethods;

		/// <param name="species"> | Symbol, String</param>
		/// <param name="form"></param>
		// @return [self, null]
		public static void get_species_form(Symbol species, Integer form) {
			if (!species || !form) return null;
			validate species => [Symbol, String];
			validate form => Integer;
			if (!GameData.Species.exists(species)) raise _INTL("Undefined species {1}.", species);
			if (species.is_a(String)) species = species.to_sym;
			if (form > 0) {
				trial = string.Format("{0}_{0}", species, form).to_sym;
				if (!DATA.has_key(trial)) {
					if (!DATA[species]) self.register({id = species});
					self.register({
						id                    = trial,
						species               = species,
						form                  = form,
						back_sprite           = DATA[species].back_sprite.clone,
						front_sprite          = DATA[species].front_sprite.clone,
						front_sprite_altitude = DATA[species].front_sprite_altitude,
						shadow_x              = DATA[species].shadow_x,
						shadow_size           = DATA[species].shadow_size;
					});
				}
				return DATA[trial];
			}
			if (!DATA[species]) self.register({id = species});
			return DATA[species];
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id                    = hash.id;
			@species               = hash.species               || @id;
			@form                  = hash.form                  || 0;
			@back_sprite           = hash.back_sprite           || new {0, 0};
			@front_sprite          = hash.front_sprite          || new {0, 0};
			@front_sprite_altitude = hash.front_sprite_altitude || 0;
			@shadow_x              = hash.shadow_x              || 0;
			@shadow_size           = hash.shadow_size           || 2;
			@s_file_suffix       = hash.s_file_suffix       || "";
		}

		public void apply_metrics_to_sprite(sprite, index, shadow = false) {
			if (shadow) {
				if ((index & 1) == 1) sprite.x += @shadow_x * 2;   // Foe Pokémon
			} else if ((index & 1) == 0) {   // Player's Pokémon
				sprite.x += @back_sprite[0] * 2;
				sprite.y += @back_sprite[1] * 2;
			} else {                     // Foe Pokémon
				sprite.x += @front_sprite[0] * 2;
				sprite.y += @front_sprite[1] * 2;
				sprite.y -= @front_sprite_altitude * 2;
			}
		}

		public bool shows_shadow() {
			return true;
//      return @front_sprite_altitude > 0
		}

		unless (method_defined(:__orig__get_property_for_PBS)) alias __orig__get_property_for_PBS get_property_for_PBS;
		public void get_property_for_PBS(key) {
			ret = __orig__get_property_for_PBS(key);
			switch (key) {
				case "SectionName":
					ret = new {@species, (@form > 0) ? @form : null};
					break;
				case "FrontSpriteAltitude":
					if (ret == 0) ret = null;
					break;
			}
			return ret;
		}
	}
}
