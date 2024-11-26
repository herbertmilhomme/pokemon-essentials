//===============================================================================
//
//===============================================================================
public partial class Player : Trainer {
	//===============================================================================
	// Represents the player's Pokédex.
	//===============================================================================
	public partial class Pokedex {
		// @return [Array<Integer>] an array of accessible Dexes
		/// <see cref="refresh_accessible_dexes"/>
		public int accessible_dexes		{ get { return _accessible_dexes; } }			protected int _accessible_dexes;

		public override void inspect() {
			str = base.inspect().chop;
			str << string.Format(" seen: {0}, owned: {0}>", self.seen_count, self.owned_count);
			return str;
		}

		// Creates an empty Pokédex.
		public void initialize() {
			@unlocked_dexes = new List<string>();
			foreach (var i in 0.upto(LoadRegionalDexes.length)) { //0.upto(LoadRegionalDexes.length) do => |i|
				@unlocked_dexes[i] = (i == 0);
			}
			self.clear;
		}

		// Clears the Pokédex.
		public void clear() {
			@seen            = new List<string>();
			@owned           = new List<string>();
			@seen_forms      = new List<string>();   // Gender (0 or 1), shiny (0 or 1), form number
			@seen_eggs       = new List<string>();
			@last_seen_forms = new List<string>();
			@owned_shadow    = new List<string>();
			@caught_counts   = new List<string>();
			@defeated_counts = new List<string>();
			self.refresh_accessible_dexes;
		}

		//---------------------------------------------------------------------------

		// Sets the given species as seen in the Pokédex.
		/// <param name="species">species to set as seen | Symbol, GameData.Species</param>
		/// <param name="should_refresh_dexes">whether Dex accessibility should be recalculated</param>
		public void set_seen(Symbol species, Boolean should_refresh_dexes = true) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return;
			@seen[species_id] = true;
			if (should_refresh_dexes) self.refresh_accessible_dexes;
		}

		/// <param name="species">species to check | Symbol, GameData.Species</param>
		// @return [Boolean] whether the species is seen
		public bool seen(Symbol species) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return false;
			return @seen[species_id] == true;
		}

		/// <param name="species">species to check | Symbol, GameData.Species</param>
		/// <param name="gender">gender to check</param>
		/// <param name="form">form to check</param>
		/// <param name="shiny">shininess to check (checks both if null) | Boolean, null</param>
		// @return [Boolean] whether the species of the given gender/form/shininess is seen
		public bool seen_form(Symbol species, Integer gender, Integer form, Boolean shiny = null) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return false;
			@seen_forms[species_id] ||= new {new {[], new List<string>()}, new {[], new List<string>()}}
			if (shiny.null()) {
				return @seen_forms[species_id][gender][0][form] || @seen_forms[species_id][gender][1][form];
			}
			shin = (shiny) ? 1 : 0;
			return @seen_forms[species_id][gender][shin][form] == true;
		}

		// Sets the egg for the given species as seen.
		/// <param name="species">species to set as seen | Symbol, GameData.Species</param>
		public void set_seen_egg(Symbol species) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return;
			@seen_eggs[species_id] = true;
		}

		/// <param name="species">species to check | Symbol, GameData.Species</param>
		// @return [Boolean] whether the egg for the given species is seen
		public bool seen_egg(Symbol species) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return false;
			return @seen_eggs[species_id] == true;
		}

		// Returns the amount of seen Pokémon.
		// If a region ID is given, returns the amount of seen Pokémon
		// in that region.
		/// <param name="dex">region ID</param>
		public void seen_count(Integer dex = -1) {
			validate dex => Integer;
			return self.count_species(@seen, dex);
		}

		// Returns whether there are any seen Pokémon.
		// If a region is given, returns whether there are seen Pokémon
		// in that region.
		/// <param name="dex">region ID</param>
		// @return [Boolean] whether there are any seen Pokémon
		public bool seen_any(Integer dex = -1) {
			validate dex => Integer;
			if (dex == -1) {
				GameData.Species.each_species(s => { if (@seen[s.species]) return true; });
			} else {
				AllRegionalSpecies(dex).each(s => { if (s && @seen[s]) return true; });
			}
			return false;
		}

		// Returns the amount of seen forms for the given species.
		/// <param name="species">Pokémon species | Symbol, GameData.Species</param>
		// @return [Integer] amount of seen forms
		public void seen_forms_count(Symbol species) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return 0;
			ret = 0;
			@seen_forms[species_id] ||= new {new {[], new List<string>()}, new {[], new List<string>()}}
			array = @seen_forms[species_id];
			for (int i = (int)Math.Max(array[0].length, array[1].length); i < (int)Math.Max(array[0].length, array[1].length); i++) { //for '(int)Math.Max(array[0].length, array[1].length)' times do => |i|
				if (array[0][0][i] || array[0][1][i] || // male or genderless shiny/non-shiny
										array[1][0][i] || array[1][1][i]) ret += 1;      // female shiny/non-shiny
			}
			return ret;
		}

		/// <param name="species">Pokémon species | Symbol, GameData.Species</param>
		public void last_form_seen(Symbol species) {
			@last_seen_forms[species] ||= new List<string>();
			return @last_seen_forms[species][0] || 0, @last_seen_forms[species][1] || 0, @last_seen_forms[species][2] || false;
		}

		/// <param name="species">Pokémon species | Symbol, GameData.Species</param>
		/// <param name="gender">gender (0=male, 1=female, 2=genderless)</param>
		/// <param name="form">form number</param>
		/// <param name="shiny">shininess</param>
		public void set_last_form_seen(Symbol species, Integer gender = 0, Integer form = 0, Boolean shiny = false) {
			@last_seen_forms[species] = new {gender, form, shiny};
		}

		//---------------------------------------------------------------------------

		// Sets the given species as owned in the Pokédex.
		/// <param name="species">species to set as owned | Symbol, GameData.Species</param>
		/// <param name="should_refresh_dexes">whether Dex accessibility should be recalculated</param>
		public void set_owned(Symbol species, Boolean should_refresh_dexes = true) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return;
			@owned[species_id] = true;
			if (should_refresh_dexes) self.refresh_accessible_dexes;
		}

		// Sets the given species as owned in the Pokédex.
		/// <param name="species">species to set as owned | Symbol, GameData.Species</param>
		public void set_shadow_pokemon_owned(Symbol species) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return;
			@owned_shadow[species_id] = true;
			self.refresh_accessible_dexes;
		}

		/// <param name="species">species to check | Symbol, GameData.Species</param>
		// @return [Boolean] whether the species is owned
		public bool owned(Symbol species) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return false;
			return @owned[species_id] == true;
		}

		/// <param name="species">species to check | Symbol, GameData.Species</param>
		// @return [Boolean] whether a Shadow Pokémon of the species is owned
		public bool owned_shadow_pokemon(Symbol species) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return false;
			return @owned_shadow[species_id] == true;
		}

		// Returns the amount of owned Pokémon.
		// If a region ID is given, returns the amount of owned Pokémon
		// in that region.
		/// <param name="dex">region ID</param>
		public void owned_count(Integer dex = -1) {
			validate dex => Integer;
			return self.count_species(@owned, dex);
		}

		//---------------------------------------------------------------------------

		/// <param name="species">Pokemon to register as seen | Pokemon, Symbol, GameData.Species</param>
		/// <param name="gender">gender to register (0=male, 1=female, 2=genderless)</param>
		/// <param name="form">form to register</param>
		/// <param name="shiny">shininess to register</param>
		/// <param name="should_refresh_dexes">whether to recalculate accessible Dex lists</param>
		public void register(Pokemon species, Integer gender = 0, Integer form = 0, Boolean shiny = false, Boolean should_refresh_dexes = true) {
			if (species.is_a(Pokemon)) {
				species_data = species.species_data;
				gender = species.gender;
				shiny = species.shiny();
			} else {
				species_data = GameData.Species.get_species_form(species, form);
			}
			species = species_data.species;
			if (gender >= 2) gender = 0;
			form = species_data.form
			shin = (shiny) ? 1 : 0;
			if (form != species_data.pokedex_form) {
				species_data = GameData.Species.get_species_form(species, species_data.pokedex_form);
				form = species_data.form
			}
			if (species_data.form_name.null() || species_data.form_name.empty()) form = 0;
			// Register as seen
			@seen[species] = true;
			@seen_forms[species] ||= new {new {[], new List<string>()}, new {[], new List<string>()}}
			@seen_forms[species][gender][shin][form] = true;
			@last_seen_forms[species] ||= new List<string>();
			if (@last_seen_forms[species] == new List<string>()) @last_seen_forms[species] = new {gender, form, shiny};
			if (should_refresh_dexes) self.refresh_accessible_dexes;
		}

		/// <param name="pkmn">Pokemon to register as most recently seen</param>
		public void register_last_seen(Pokemon pkmn) {
			validate pkmn => Pokemon;
			species_data = pkmn.species_data;
			form = species_data.pokedex_form
			if (species_data.form_name.null() || species_data.form_name.empty()) form = 0;
			@last_seen_forms[pkmn.species] = new {pkmn.gender, form, pkmn.shiny()};
		}

		//---------------------------------------------------------------------------

		/// <param name="species">species to check | Symbol, GameData.Species</param>
		// @return [Integer] the number of Pokémon of the given species that have
		//   been caught by the player
		public void caught_count(Symbol species) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return 0;
			return @caught_counts[species_id] || 0;
		}

		/// <param name="species">species to check | Symbol, GameData.Species</param>
		// @return [Integer] the number of Pokémon of the given species that have
		//   been defeated by the player
		public void defeated_count(Symbol species) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return 0;
			return @defeated_counts[species_id] || 0;
		}

		/// <param name="species">species to check | Symbol, GameData.Species</param>
		// @return [Integer] the number of Pokémon of the given species that have
		//   been defeated or caught by the player
		public void battled_count(Symbol species) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return 0;
			return (@defeated_counts[species_id] || 0) + (@caught_counts[species_id] || 0);
		}

		/// <param name="species">species to count as caught | Symbol, GameData.Species</param>
		public void register_caught(Symbol species) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return;
			if (@caught_counts[species_id].null()) @caught_counts[species_id] = 0;
			@caught_counts[species_id] += 1;
		}

		/// <param name="species">species to count as defeated | Symbol, GameData.Species</param>
		public void register_defeated(Symbol species) {
			species_id = GameData.Species.try_get(species)&.species;
			if (species_id.null()) return;
			if (@defeated_counts[species_id].null()) @defeated_counts[species_id] = 0;
			@defeated_counts[species_id] += 1;
		}

		//---------------------------------------------------------------------------

		// Unlocks the given Dex, -1 being the National Dex.
		/// <param name="dex">Dex ID (-1 is the National Dex)</param>
		public void unlock(Integer dex) {
			validate dex => Integer;
			if (dex < 0 || dex > @unlocked_dexes.length - 1) dex = @unlocked_dexes.length - 1;
			@unlocked_dexes[dex] = true;
			self.refresh_accessible_dexes;
		}

		// Locks the given Dex, -1 being the National Dex.
		/// <param name="dex">Dex ID (-1 is the National Dex)</param>
		public void lock(Integer dex) {
			validate dex => Integer;
			if (dex < 0 || dex > @unlocked_dexes.length - 1) dex = @unlocked_dexes.length - 1;
			@unlocked_dexes[dex] = false;
			self.refresh_accessible_dexes;
		}

		/// <param name="dex">Dex ID (-1 is the National Dex)</param>
		// @return [Boolean] whether the given Dex is unlocked
		public bool unlocked(Integer dex) {
			validate dex => Integer;
			if (dex == -1) dex = @unlocked_dexes.length - 1;
			return @unlocked_dexes[dex] == true;
		}

		// @return [Integer] the number of defined Dexes (including the National Dex)
		public void dexes_count() {
			return @unlocked_dexes.length;
		}

		// Decides which Dex lists are able to be viewed (i.e. they are unlocked and
		// have at least 1 seen species in them), and saves all accessible Dex region
		// numbers into {#accessible_dexes}. National Dex comes after all regional
		// Dexes.
		// If the Dex list shown depends on the player's location, this just decides
		// if (a species in the current region has been seen - doesn't look at other) {
		// regions.
		public void refresh_accessible_dexes() {
			@accessible_dexes = new List<string>();
			if (Settings.USE_CURRENT_REGION_DEX) {
				region = GetCurrentRegion;
				if (region >= dexes_count - 1) region = -1;
				if (self.seen_any(region)) @accessible_dexes[0] = region;
				return;
			}
			if (dexes_count == 1) {   // Only National Dex is defined
				if (self.unlocked(0) && self.seen_any()) @accessible_dexes.Add(-1);
			} else {   // Regional Dexes + National Dex
				for (int i = dexes_count; i < dexes_count; i++) { //for 'dexes_count' times do => |i|
					dex_list_to_check = (i == dexes_count - 1) ? -1 : i;
					if (self.unlocked(i) && self.seen_any(dex_list_to_check)) {
						@accessible_dexes.Add(dex_list_to_check);
					}
				}
			}
		}

		public bool species_in_unlocked_dex(species) {
			if (@unlocked_dexes.last) return true;
			for (int i = (@unlocked_dexes.length - 1); i < (@unlocked_dexes.length - 1); i++) { //for '(@unlocked_dexes.length - 1)' times do => |i|
				if (!self.unlocked(i)) continue;
				if (GetRegionalNumber(i, species) > 0) return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------

		private;

		/// <param name="hash"></param>
		/// <param name="region"></param>
		// @return [Integer]
		public void count_species(Hash hash, Integer region = -1) {
			ret = 0;
			if (region == -1) {
				GameData.Species.each_species(s => { if (hash[s.species]) ret += 1; });
			} else {
				AllRegionalSpecies(region).each(s => { if (s && hash[s]) ret += 1; });
			}
			return ret;
		}
	}
}
