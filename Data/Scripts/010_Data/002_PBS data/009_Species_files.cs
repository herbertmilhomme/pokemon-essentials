//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Species {
		public static void check_graphic_file(path, species, form = 0, gender = 0, shiny = false, shadow = false, subfolder = "") {
			try_subfolder = string.Format("{0}/", subfolder);
			try_species = species;
			try_form    = (form > 0) ? string.Format("_{0}", form) : "";
			try_gender  = (gender == 1) ? "_female" : "";
			try_shadow  = (shadow) ? "_shadow" : "";
			factors = new List<string>();
			if (shiny) factors.Add(new {4, string.Format("{0} shiny/", subfolder), try_subfolder});
			if (shadow) factors.Add(new {3, try_shadow, ""});
			if (gender == 1) factors.Add(new {2, try_gender, ""});
			if (form > 0) factors.Add(new {1, try_form, ""});
			factors.Add(new {0, try_species, "000"});
			// Go through each combination of parameters in turn to find an existing sprite
			for (int i = (2**factors.length); i < (2**factors.length); i++) { //for '(2**factors.length)' times do => |i|
				// Set try_ parameters for this combination
				factors.each_with_index do |factor, index|
					value = ((i / (2**index)).even()) ? factor[1] : factor[2];
					switch (factor[0]) {
						case 0:  try_species   = value; break;
						case 1:  try_form      = value; break;
						case 2:  try_gender    = value; break;
						case 3:  try_shadow    = value; break;
						case 4:  try_subfolder = value; break;   // Shininess
					}
				}
				// Look for a graphic matching this combination's parameters
				try_species_text = try_species;
				ret = ResolveBitmap(string.Format("{0}{0}{0}{0}{0}{0}", path, try_subfolder,
																			try_species_text, try_form, try_gender, try_shadow));
				if (ret) return ret;
			}
			return null;
		}

		public static void check_egg_graphic_file(path, species, form, suffix = "") {
			species_data = self.get_species_form(species, form);
			if (species_data.null()) return null;
			if (form > 0) {
				ret = ResolveBitmap(string.Format("{0}{0}_{0}{0}", path, species_data.species, form, suffix));
				if (ret) return ret;
			}
			return ResolveBitmap(string.Format("{0}{0}{0}", path, species_data.species, suffix));
		}

		public static void front_sprite_filename(species, form = 0, gender = 0, shiny = false, shadow = false) {
			return self.check_graphic_file("Graphics/Pokemon/", species, form, gender, shiny, shadow, "Front");
		}

		public static void back_sprite_filename(species, form = 0, gender = 0, shiny = false, shadow = false) {
			return self.check_graphic_file("Graphics/Pokemon/", species, form, gender, shiny, shadow, "Back");
		}

		public static void egg_sprite_filename(species, form) {
			ret = self.check_egg_graphic_file("Graphics/Pokemon/Eggs/", species, form);
			return (ret) ? ret : ResolveBitmap("Graphics/Pokemon/Eggs/000");
		}

		public static void egg_cracks_sprite_filename(species, form) {
			ret = self.check_egg_graphic_file("Graphics/Pokemon/Eggs/", species, form, "_cracks");
			return (ret) ? ret : ResolveBitmap("Graphics/Pokemon/Eggs/000_cracks");
		}

		public static void sprite_filename(species, form = 0, gender = 0, shiny = false, shadow = false, back = false, egg = false) {
			if (egg) return self.egg_sprite_filename(species, form);
			if (back) return self.back_sprite_filename(species, form, gender, shiny, shadow);
			return self.front_sprite_filename(species, form, gender, shiny, shadow);
		}

		public static void front_sprite_bitmap(species, form = 0, gender = 0, shiny = false, shadow = false) {
			filename = self.front_sprite_filename(species, form, gender, shiny, shadow);
			return (filename) ? new AnimatedBitmap(filename) : null;
		}

		public static void back_sprite_bitmap(species, form = 0, gender = 0, shiny = false, shadow = false) {
			filename = self.back_sprite_filename(species, form, gender, shiny, shadow);
			return (filename) ? new AnimatedBitmap(filename) : null;
		}

		public static void egg_sprite_bitmap(species, form = 0) {
			filename = self.egg_sprite_filename(species, form);
			return (filename) ? new AnimatedBitmap(filename) : null;
		}

		public static void sprite_bitmap(species, form = 0, gender = 0, shiny = false, shadow = false, back = false, egg = false) {
			if (egg) return self.egg_sprite_bitmap(species, form);
			if (back) return self.back_sprite_bitmap(species, form, gender, shiny, shadow);
			return self.front_sprite_bitmap(species, form, gender, shiny, shadow);
		}

		public static void sprite_bitmap_from_pokemon(pkmn, back = false, species = null) {
			if (!species) species = pkmn.species;
			species = GameData.Species.get(species).species;   // Just to be sure it's a symbol
			if (pkmn.egg()) return self.egg_sprite_bitmap(species, pkmn.form);
			if (back) {
				ret = self.back_sprite_bitmap(species, pkmn.form, pkmn.gender, pkmn.shiny(), pkmn.shadowPokemon());
			} else {
				ret = self.front_sprite_bitmap(species, pkmn.form, pkmn.gender, pkmn.shiny(), pkmn.shadowPokemon());
			}
			alter_bitmap_function = MultipleForms.getFunction(species, "alterBitmap");
			if (ret && alter_bitmap_function) {
				new_ret = ret.copy;
				ret.dispose;
				new_ret.each(bitmap => alter_bitmap_function.call(pkmn, bitmap));
				ret = new_ret;
			}
			return ret;
		}

		//---------------------------------------------------------------------------

		public static void egg_icon_filename(species, form) {
			ret = self.check_egg_graphic_file("Graphics/Pokemon/Eggs/", species, form, "_icon");
			return (ret) ? ret : ResolveBitmap("Graphics/Pokemon/Eggs/000_icon");
		}

		public static void icon_filename(species, form = 0, gender = 0, shiny = false, shadow = false, egg = false) {
			if (egg) return self.egg_icon_filename(species, form);
			return self.check_graphic_file("Graphics/Pokemon/", species, form, gender, shiny, shadow, "Icons");
		}

		public static void icon_filename_from_pokemon(pkmn) {
			return self.icon_filename(pkmn.species, pkmn.form, pkmn.gender, pkmn.shiny(), pkmn.shadowPokemon(), pkmn.egg());
		}

		public static void egg_icon_bitmap(species, form) {
			filename = self.egg_icon_filename(species, form);
			return (filename) ? new AnimatedBitmap(filename).deanimate : null;
		}

		public static void icon_bitmap(species, form = 0, gender = 0, shiny = false, shadow = false, egg = false) {
			if (egg) return self.egg_icon_bitmap(species, form);
			filename = self.icon_filename(species, form, gender, shiny, shadow);
			return (filename) ? new AnimatedBitmap(filename).deanimate : null;
		}

		public static void icon_bitmap_from_pokemon(pkmn) {
			return self.icon_bitmap(pkmn.species, pkmn.form, pkmn.gender, pkmn.shiny(), pkmn.shadowPokemon(), pkmn.egg());
		}

		//---------------------------------------------------------------------------

		public static void footprint_filename(species, form = 0) {
			species_data = self.get_species_form(species, form);
			if (species_data.null()) return null;
			if (form > 0) {
				ret = ResolveBitmap(string.Format("Graphics/Pokemon/Footprints/{0}_{0}", species_data.species, form));
				if (ret) return ret;
			}
			return ResolveBitmap(string.Format("Graphics/Pokemon/Footprints/{0}", species_data.species));
		}

		//---------------------------------------------------------------------------

		public static void shadow_filename(species, form = 0) {
			species_data = self.get_species_form(species, form);
			if (species_data.null()) return null;
			// Look for species-specific shadow graphic
			if (form > 0) {
				ret = ResolveBitmap(string.Format("Graphics/Pokemon/Shadow/{0}_{0}", species_data.species, form));
				if (ret) return ret;
			}
			ret = ResolveBitmap(string.Format("Graphics/Pokemon/Shadow/{0}", species_data.species));
			if (ret) return ret;
			// Use general shadow graphic
			metrics_data = GameData.SpeciesMetrics.get_species_form(species_data.species, form);
			return ResolveBitmap(string.Format("Graphics/Pokemon/Shadow/{0}", metrics_data.shadow_size));
		}

		public static void shadow_bitmap(species, form = 0) {
			filename = self.shadow_filename(species, form);
			return (filename) ? new AnimatedBitmap(filename) : null;
		}

		public static void shadow_bitmap_from_pokemon(pkmn) {
			filename = self.shadow_filename(pkmn.species, pkmn.form);
			return (filename) ? new AnimatedBitmap(filename) : null;
		}

		//---------------------------------------------------------------------------

		public static void check_cry_file(species, form, suffix = "") {
			species_data = self.get_species_form(species, form);
			if (species_data.null()) return null;
			if (form > 0) {
				ret = string.Format("Cries/{0}_{0}{0}", species_data.species, form, suffix);
				if (ResolveAudioSE(ret)) return ret;
			}
			ret = string.Format("Cries/{0}{0}", species_data.species, suffix);
			return (ResolveAudioSE(ret)) ? ret : null;
		}

		public static void cry_filename(species, form = 0, suffix = "") {
			return self.check_cry_file(species, form || 0, suffix);
		}

		public static void cry_filename_from_pokemon(pkmn, suffix = "") {
			return self.check_cry_file(pkmn.species, pkmn.form, suffix);
		}

		public static void play_cry_from_species(species, form = 0, volume = 90, pitch = 100) {
			filename = self.cry_filename(species, form);
			if (!filename) return;
			SEPlay(new RPG.AudioFile(filename, volume, pitch)) rescue null;
		}

		public static void play_cry_from_pokemon(pkmn, volume = 90, pitch = 100) {
			if (!pkmn || pkmn.egg()) return;
			filename = self.cry_filename_from_pokemon(pkmn);
			if (!filename) return;
			pitch ||= 100;
			SEPlay(new RPG.AudioFile(filename, volume, pitch)) rescue null;
		}

		public static void play_cry(pkmn, volume = 90, pitch = 100) {
			if (pkmn.is_a(Pokemon)) {
				self.play_cry_from_pokemon(pkmn, volume, pitch);
			} else {
				self.play_cry_from_species(pkmn, 0, volume, pitch);
			}
		}

		public static void cry_length(species, form = 0, pitch = 100, suffix = "") {
			pitch ||= 100;
			if (!species || pitch <= 0) return 0;
			pitch = pitch.to_f / 100;
			ret = 0.0;
			if (species.is_a(Pokemon)) {
				if (!species.egg()) {
					filename = self.cry_filename_from_pokemon(species, suffix);
					if (!filename && !nil_or_empty(suffix)) filename = self.cry_filename_from_pokemon(species);
					filename = ResolveAudioSE(filename);
					if (filename) ret = getPlayTime(filename);
				}
			} else {
				filename = self.cry_filename(species, form, suffix);
				if (!filename && !nil_or_empty(suffix)) filename = self.cry_filename(species, form);
				filename = ResolveAudioSE(filename);
				if (filename) ret = getPlayTime(filename);
			}
			ret /= pitch;   // Sound played at a lower pitch lasts longer
			return ret;
		}
	}
}
