//===============================================================================
//
//===============================================================================
public partial class PokemonEncounters {
	public int step_count		{ get { return _step_count; } }			protected int _step_count;

	public void initialize() {
		@step_chances       = new List<string>();
		@encounter_tables   = new List<string>();
		@chance_accumulator = 0;
	}

	public void setup(map_ID) {
		@step_count       = 0;
		@step_chances     = new List<string>();
		@encounter_tables = new List<string>();
		encounter_data = GameData.Encounter.get(map_ID, Game.GameData.PokemonGlobal.encounter_version);
		if (encounter_data) {
			encounter_data.step_chances.each((type, value) => @step_chances[type] = value);
			@encounter_tables = Marshal.load(Marshal.dump(encounter_data.types));
		}
	}

	public void reset_step_count() {
		@step_count = 0;
		@chance_accumulator = 0;
	}

	//-----------------------------------------------------------------------------

	// Returns whether encounters for the given encounter type have been defined
	// for the current map.
	public bool has_encounter_type(enc_type) {
		if (!enc_type) return false;
		return @encounter_tables[enc_type] && @encounter_tables[enc_type].length > 0;
	}

	// Returns whether encounters for the given encounter type have been defined
	// for the given map. Only called by Bug-Catching Contest to see if it can use
	// the map's BugContest encounter type to generate caught Pokémon for the other
	// contestants.
	public bool map_has_encounter_type(map_ID, enc_type) {
		if (!enc_type) return false;
		encounter_data = GameData.Encounter.get(map_ID, Game.GameData.PokemonGlobal.encounter_version);
		if (!encounter_data) return false;
		return encounter_data.types[enc_type] && encounter_data.types[enc_type].length > 0;
	}

	// Returns whether land-like encounters have been defined for the current map.
	// Applies only to encounters triggered by moving around.
	public bool has_land_encounters() {
		foreach (var enc_type in GameData.EncounterType) { //'GameData.EncounterType.each' do => |enc_type|
			if (!new []{:land, :contest}.Contains(enc_type.type)) continue;
			if (has_encounter_type(enc_type.id)) return true;
		}
		return false;
	}

	// Returns whether land-like encounters have been defined for the current map
	// (ignoring the Bug-Catching Contest one).
	// Applies only to encounters triggered by moving around.
	public bool has_normal_land_encounters() {
		foreach (var enc_type in GameData.EncounterType) { //'GameData.EncounterType.each' do => |enc_type|
			if (enc_type.type == types.land && has_encounter_type(enc_type.id)) return true;
		}
		return false;
	}

	// Returns whether cave-like encounters have been defined for the current map.
	// Applies only to encounters triggered by moving around.
	public bool has_cave_encounters() {
		foreach (var enc_type in GameData.EncounterType) { //'GameData.EncounterType.each' do => |enc_type|
			if (enc_type.type == types.cave && has_encounter_type(enc_type.id)) return true;
		}
		return false;
	}

	// Returns whether water-like encounters have been defined for the current map.
	// Applies only to encounters triggered by moving around (i.e. not fishing).
	public bool has_water_encounters() {
		foreach (var enc_type in GameData.EncounterType) { //'GameData.EncounterType.each' do => |enc_type|
			if (enc_type.type == types.water && has_encounter_type(enc_type.id)) return true;
		}
		return false;
	}

	//-----------------------------------------------------------------------------

	// Returns whether the player's current location allows wild encounters to
	// trigger upon taking a step.
	public bool encounter_possible_here() {
		if (Game.GameData.PokemonGlobal.surfing) return true;
		terrain_tag = Game.GameData.game_map.terrain_tag(Game.GameData.game_player.x, Game.GameData.game_player.y);
		if (terrain_tag.ice) return false;
		if (has_cave_encounters()) return true;   // i.e. this map is a cave
		if (has_land_encounters() && terrain_tag.land_wild_encounters) return true;
		return false;
	}

	// Returns whether a wild encounter should happen, based on its encounter
	// chance. Called when taking a step and by Rock Smash.
	public bool encounter_triggered(enc_type, repel_active = false, triggered_by_step = true) {
		if (!enc_type || !GameData.EncounterType.exists(enc_type)) {
			Debug.LogError(new ArgumentError(_INTL("Encounter type {1} does not exist", enc_type)));
			//throw new ArgumentException(new ArgumentError(_INTL("Encounter type {1} does not exist", enc_type)));
		}
		if (Game.GameData.game_system.encounter_disabled) return false;
		if (!Game.GameData.player) return false;
		if (Core.DEBUG && Input.press(Input.CTRL)) return false;
		// Check if enc_type has a defined step chance/encounter table
		if (!@step_chances[enc_type] || @step_chances[enc_type] == 0) return false;
		if (!has_encounter_type(enc_type)) return false;
		// Poké Radar encounters always happen, ignoring the minimum step period and
		// trigger probabilities
		if (PokeRadarOnShakingGrass) return true;
		// Get base encounter chance and minimum steps grace period
		encounter_chance = @step_chances[enc_type].to_f;
		min_steps_needed = (8 - (encounter_chance / 10)).clamp(0, 8).to_f;
		// Apply modifiers to the encounter chance and the minimum steps amount
		if (triggered_by_step) {
			encounter_chance += @chance_accumulator / 200;
			if (Game.GameData.PokemonGlobal.bicycle) encounter_chance *= 0.8;
		}
		if (Game.GameData.PokemonMap.lower_encounter_rate) {
			encounter_chance /= 2;
			min_steps_needed *= 2;
		} else if (Game.GameData.PokemonMap.higher_encounter_rate) {
			encounter_chance *= 1.5;
			min_steps_needed /= 2;
		}
		first_pkmn = Game.GameData.player.first_pokemon;
		if (first_pkmn) {
			switch (first_pkmn.item_id) {
				case :CLEANSETAG:
					encounter_chance *= 2.0 / 3;
					min_steps_needed *= 4 / 3.0;
					break;
				case :PUREINCENSE:
					encounter_chance *= 2.0 / 3;
					min_steps_needed *= 4 / 3.0;
					break;
				default:   // Ignore ability effects if an item effect applies
					switch (first_pkmn.ability_id) {
						case :STENCH: case :WHITESMOKE: case :QUICKFEET:
							encounter_chance /= 2;
							min_steps_needed *= 2;
							break;
						case :INFILTRATOR:
							if (Settings.MORE_ABILITIES_AFFECT_WILD_ENCOUNTERS) {
								encounter_chance /= 2;
								min_steps_needed *= 2;
							}
							break;
						case :SNOWCLOAK:
							if (GameData.Weather.get(Game.GameData.game_screen.weather_type).category == :Hail) {
								encounter_chance /= 2;
								min_steps_needed *= 2;
							}
							break;
						case :SANDVEIL:
							if (GameData.Weather.get(Game.GameData.game_screen.weather_type).category == :Sandstorm) {
								encounter_chance /= 2;
								min_steps_needed *= 2;
							}
							break;
						case :SWARM:
							encounter_chance *= 1.5;
							min_steps_needed /= 2;
							break;
						case :ILLUMINATE: case :ARENATRAP: case :NOGUARD:
							encounter_chance *= 2;
							min_steps_needed /= 2;
							break;
					}
					break;
			}
		}
		// Wild encounters are much less likely to happen for the first few steps
		// after a previous wild encounter
		if (triggered_by_step && @step_count < min_steps_needed) {
			@step_count += 1;
			if (rand(100) >= encounter_chance * 5 / (@step_chances[enc_type] + (@chance_accumulator / 200))) return false;
		}
		// Decide whether the wild encounter should actually happen
		if (rand(100) < encounter_chance) return true;
		// If encounter didn't happen, make the next step more likely to produce one
		if (triggered_by_step) {
			@chance_accumulator += @step_chances[enc_type];
			if (repel_active) @chance_accumulator = 0;
		}
		return false;
	}

	// Returns whether an encounter with the given Pokémon should be allowed after
	// taking into account Repels and ability effects.
	public bool allow_encounter(enc_data, repel_active = false) {
		if (!enc_data) return false;
		if (PokeRadarOnShakingGrass) return true;
		// Repel
		if (repel_active) {
			first_pkmn = (Settings.REPEL_COUNTS_FAINTED_POKEMON) ? Game.GameData.player.first_pokemon : Game.GameData.player.first_able_pokemon;
			if (first_pkmn && enc_data[1] < first_pkmn.level) {
				@chance_accumulator = 0;
				return false;
			}
		}
		// Some abilities make wild encounters less likely if the wild Pokémon is
		// sufficiently weaker than the Pokémon with the ability
		first_pkmn = Game.GameData.player.first_pokemon;
		if (first_pkmn) {
			switch (first_pkmn.ability_id) {
				case :INTIMIDATE: case :KEENEYE:
					if (enc_data[1] <= first_pkmn.level - 5 && rand(100) < 50) return false;
					break;
			}
		}
		return true;
	}

	// Returns whether a wild encounter should be turned into a double wild
	// encounter.
	public bool have_double_wild_battle() {
		if (Game.GameData.game_temp.force_single_battle) return false;
		if (InSafari()) return false;
		if (Game.GameData.PokemonGlobal.partner) return true;
		if (Game.GameData.player.able_pokemon_count <= 1) return false;
		if (Game.GameData.game_player.TerrainTag.double_wild_encounters && rand(100) < 30) return true;
		return false;
	}

	// Checks the defined encounters for the current map and returns the encounter
	// type that the given time should produce. Only returns an encounter type if
	// it has been defined for the current map.
	public void find_valid_encounter_type_for_time(base_type, time) {
		ret = null;
		if (DayNight.isDay(time)) {
			try_type = null;
			if (DayNight.isMorning(time)) {
				try_type = (base_type.ToString() + "Morning").to_sym;
			} else if (DayNight.isAfternoon(time)) {
				try_type = (base_type.ToString() + "Afternoon").to_sym;
			} else if (DayNight.isEvening(time)) {
				try_type = (base_type.ToString() + "Evening").to_sym;
			}
			if (try_type && has_encounter_type(try_type)) ret = try_type;
			if (!ret) {
				try_type = (base_type.ToString() + "Day").to_sym;
				if (has_encounter_type(try_type)) ret = try_type;
			}
		} else {
			try_type = (base_type.ToString() + "Night").to_sym;
			if (has_encounter_type(try_type)) ret = try_type;
		}
		if (ret) return ret;
		return (has_encounter_type(base_type)) ? base_type : null;
	}

	// Returns the encounter method that the current encounter should be generated
	// from, depending on the player's current location.
	public void encounter_type() {
		time = GetTimeNow;
		ret = null;
		if (Game.GameData.PokemonGlobal.surfing) {
			ret = find_valid_encounter_type_for_time(:Water, time);
		} else {   // Land/Cave (can have both in the same map)
			if (has_land_encounters() && Game.GameData.game_map.terrain_tag(Game.GameData.game_player.x, Game.GameData.game_player.y).land_wild_encounters) {
				if (InBugContest() && has_encounter_type(types.BugContest)) ret = :BugContest;
				if (!ret) ret = find_valid_encounter_type_for_time(:Land, time);
			}
			if (!ret && has_cave_encounters()) {
				ret = find_valid_encounter_type_for_time(:Cave, time);
			}
		}
		return ret;
	}

	//-----------------------------------------------------------------------------

	// For the current map, randomly chooses a species and level from the encounter
	// list for the given encounter type. Returns null if there are none defined.
	// A higher chance_rolls makes this method prefer rarer encounter slots.
	public void choose_wild_pokemon(enc_type, chance_rolls = 1) {
		if (!enc_type || !GameData.EncounterType.exists(enc_type)) {
			Debug.LogError(new ArgumentError(_INTL("Encounter type {1} does not exist", enc_type)));
			//throw new ArgumentException(new ArgumentError(_INTL("Encounter type {1} does not exist", enc_type)));
		}
		enc_list = @encounter_tables[enc_type];
		if (!enc_list || enc_list.length == 0) return null;
		// Static/Magnet Pull prefer wild encounters of certain types, if possible.
		// If they activate, they remove all Pokémon from the encounter table that do
		// not have the type they favor. If none have that type, nothing is changed.
		first_pkmn = Game.GameData.player.first_pokemon;
		if (first_pkmn) {
			favored_type = null;
			switch (first_pkmn.ability_id) {
				case :FLASHFIRE:
					if (Settings.MORE_ABILITIES_AFFECT_WILD_ENCOUNTERS &&
																	GameData.Types.exists(Types.FIRE) && rand(100) < 50) favored_type = types.FIRE;
					break;
				case :HARVEST:
					if (Settings.MORE_ABILITIES_AFFECT_WILD_ENCOUNTERS &&
																	GameData.Types.exists(Types.GRASS) && rand(100) < 50) favored_type = types.GRASS;
					break;
				case :LIGHTNINGROD:
					if (Settings.MORE_ABILITIES_AFFECT_WILD_ENCOUNTERS &&
																			GameData.Types.exists(Types.ELECTRIC) && rand(100) < 50) favored_type = types.ELECTRIC;
					break;
				case :MAGNETPULL:
					if (GameData.Types.exists(Types.STEEL) && rand(100) < 50) favored_type = types.STEEL;
					break;
				case :STATIC:
					if (GameData.Types.exists(Types.ELECTRIC) && rand(100) < 50) favored_type = types.ELECTRIC;
					break;
				case :STORMDRAIN:
					if (Settings.MORE_ABILITIES_AFFECT_WILD_ENCOUNTERS &&
																	GameData.Types.exists(Types.WATER) && rand(100) < 50) favored_type = types.WATER;
					break;
			}
			if (favored_type) {
				new_enc_list = new List<string>();
				foreach (var enc in enc_list) { //'enc_list.each' do => |enc|
					species_data = GameData.Species.get(enc[1]);
					if (species_data.types.Contains(favored_type)) new_enc_list.Add(enc);
				}
				if (new_enc_list.length > 0) enc_list = new_enc_list;
			}
		}
		enc_list.sort! { |a, b| b[0] <=> a[0] };   // Highest probability first
		// Calculate the total probability value
		chance_total = 0;
		enc_list.each(a => chance_total += a[0]);
		// Choose a random entry in the encounter table based on entry probabilities
		rnd = 0;
		chance_rolls.times do;
			r = rand(chance_total);
			if (r > rnd) rnd = r;   // Prefer rarer entries if rolling repeatedly
		}
		encounter = null;
		foreach (var enc in enc_list) { //'enc_list.each' do => |enc|
			rnd -= enc[0];
			if (rnd >= 0) continue;
			encounter = enc;
			break;
		}
		// Get the chosen species and level
		level = rand(encounter[2]..encounter[3]);
		// Some abilities alter the level of the wild Pokémon
		if (first_pkmn) {
			switch (first_pkmn.ability_id) {
				case :HUSTLE: case :PRESSURE: case :VITALSPIRIT:
					if (rand(100) < 50) level = encounter[3];   // Highest possible level
					break;
			}
		}
		// Black Flute and White Flute alter the level of the wild Pokémon
		if (Game.GameData.PokemonMap.lower_level_wild_pokemon) {
			level = (int)Math.Max(level - rand(1..4), 1);
		} else if (Game.GameData.PokemonMap.higher_level_wild_pokemon) {
			level = (int)Math.Min(level + rand(1..4), GameData.GrowthRate.max_level);
		}
		// Return new {species, level}
		return new {encounter[1], level};
	}

	// For the given map, randomly chooses a species and level from the encounter
	// list for the given encounter type. Returns null if there are none defined.
	// Used by the Bug-Catching Contest to choose what the other participants
	// caught.
	public void choose_wild_pokemon_for_map(map_ID, enc_type) {
		if (!enc_type || !GameData.EncounterType.exists(enc_type)) {
			Debug.LogError(new ArgumentError(_INTL("Encounter type {1} does not exist", enc_type)));
			//throw new ArgumentException(new ArgumentError(_INTL("Encounter type {1} does not exist", enc_type)));
		}
		// Get the encounter table
		encounter_data = GameData.Encounter.get(map_ID, Game.GameData.PokemonGlobal.encounter_version);
		if (!encounter_data) return null;
		enc_list = encounter_data.types[enc_type];
		if (!enc_list || enc_list.length == 0) return null;
		// Calculate the total probability value
		chance_total = 0;
		enc_list.each(a => chance_total += a[0]);
		// Choose a random entry in the encounter table based on entry probabilities
		rnd = rand(chance_total);
		encounter = null;
		foreach (var enc in enc_list) { //'enc_list.each' do => |enc|
			rnd -= enc[0];
			if (rnd >= 0) continue;
			encounter = enc;
			break;
		}
		// Return new {species, level}
		level = rand(encounter[2]..encounter[3]);
		return new {encounter[1], level};
	}
}

//===============================================================================
//
//===============================================================================
// Creates and returns a Pokémon based on the given species and level.
// Applies wild Pokémon modifiers (wild held item, shiny chance modifiers,
// Pokérus, gender/nature forcing because of player's lead Pokémon).
public void GenerateWildPokemon(species, level, isRoamer = false) {
	genwildpoke = new Pokemon(species, level);
	// Give the wild Pokémon a held item
	items = genwildpoke.wildHoldItems;
	first_pkmn = Game.GameData.player.first_pokemon;
	chances = new {50, 5, 1};
	if (Settings.MECHANICS_GENERATION >= 9) {
		chances[0] = 30;
	} else if (first_pkmn) {
		switch (first_pkmn.ability_id) {
			case :COMPOUNDEYES:
				chances = new {60, 20, 5};
				break;
			case :SUPERLUCK:
				if (Settings.MORE_ABILITIES_AFFECT_WILD_ENCOUNTERS) chances = new {60, 20, 5};
				break;
		}
	}
	itemrnd = rand(100);
	if ((items[0] == items[1] && items[1] == items[2]) || itemrnd < chances[0]) {
		genwildpoke.item = items[0].sample;
	} else if (itemrnd < (chances[0] + chances[1])) {
		genwildpoke.item = items[1].sample;
	} else if (itemrnd < (chances[0] + chances[1] + chances[2])) {
		genwildpoke.item = items[2].sample;
	}
	// Improve chances of shiny Pokémon with Shiny Charm and battling more of the
	// same species
	shiny_retries = 0;
	if (Game.GameData.bag.has(:SHINYCHARM)) shiny_retries += 2;
	if (Settings.HIGHER_SHINY_CHANCES_WITH_NUMBER_BATTLED) {
		values = new {0, 0};
		switch (Game.GameData.player.pokedex.battled_count(species)) {
			case 0...50:     values = new {0, 0}; break;
			case 50...100:   values = new {1, 15}; break;
			case 100...200:  values = new {2, 20}; break;
			case 200...300:  values = new {3, 25}; break;
			case 300...500:  values = new {4, 30}; break;
			default:                values = new {5, 30};
		}
		if (values[1] > 0 && rand(1000) < values[1]) shiny_retries += values[0];
	}
	if (shiny_retries > 0) {
		shiny_retries.times do;
			if (genwildpoke.shiny()) break;
			genwildpoke.shiny = null;   // Make it recalculate shininess
			genwildpoke.personalID = rand(Math.Pow(2, 16)) | (rand(Math.Pow(2, 16)) << 16);
		}
	}
	// Give Pokérus
	if (rand(65_536) < Settings.POKERUS_CHANCE) genwildpoke.givePokerus;
	// Change wild Pokémon's gender/nature depending on the lead party Pokémon's
	// ability
	if (first_pkmn) {
		if (first_pkmn.hasAbility(Abilitys.CUTECHARM) && !genwildpoke.singleGendered()) {
			if (first_pkmn.male()) {
				(rand(3) < 2) ? genwildpoke.makeFemale : genwildpoke.makeMale
			} else if (first_pkmn.female()) {
				(rand(3) < 2) ? genwildpoke.makeMale : genwildpoke.makeFemale
			}
		} else if (first_pkmn.hasAbility(Abilitys.SYNCHRONIZE)) {
			if (!isRoamer && (Settings.MORE_ABILITIES_AFFECT_WILD_ENCOUNTERS || (rand(100) < 50))) {
				genwildpoke.nature = first_pkmn.nature;
			}
		}
	}
	// Trigger events that may alter the generated Pokémon further
	if (MultipleForms.hasFunction(genwildpoke.species, "getForm")) genwildpoke.form_simple = genwildpoke.form;
	EventHandlers.trigger(:on_wild_pokemon_created, genwildpoke);
	return genwildpoke;
}

// Used by fishing rods and Headbutt/Rock Smash/Sweet Scent to generate a wild
// Pokémon (or two if it's Sweet Scent) for a triggered wild encounter.
public void Encounter(enc_type, only_single = true) {
	Game.GameData.game_temp.encounter_type = enc_type;
	encounter1 = Game.GameData.PokemonEncounters.choose_wild_pokemon(enc_type);
	EventHandlers.trigger(:on_wild_species_chosen, encounter1);
	if (!encounter1) return false;
	if (!only_single && Game.GameData.PokemonEncounters.have_double_wild_battle()) {
		encounter2 = Game.GameData.PokemonEncounters.choose_wild_pokemon(enc_type);
		EventHandlers.trigger(:on_wild_species_chosen, encounter2);
		if (!encounter2) return false;
		WildBattle.start(encounter1, encounter2, can_override: true);
	} else {
		WildBattle.start(encounter1, can_override: true);
	}
	Game.GameData.game_temp.encounter_type = null;
	Game.GameData.game_temp.force_single_battle = false;
	return true;
}
