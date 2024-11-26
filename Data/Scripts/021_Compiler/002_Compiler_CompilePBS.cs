//===============================================================================
//
//===============================================================================
public static partial class Compiler {
	@@categories.s_files = {
		should_compile = compiling => { next should_compile_pbs_files() },
		header_text    = () => { next _INTL("Compiling PBS files") },
		skipped_text   = () => { next _INTL("Not compiled") },
		compile        = () => {
			// Delete old data files in preparation for recompiling
			foreach (var filename in get_all_pbs_data_filenames_to_compile) { //'get_all_pbs_data_filenames_to_compile.each' do => |filename|
				begin;
					File.delete($"Data/{filename[0]}") if FileTest.exist($"Data/{filename[0]}");
				rescue SystemCallError;
				}
			}
			compile_pbs_files;
		}
	}

	#region Class Functions
	#endregion

	public void get_all_pbs_data_filenames_to_compile() {
		ret = GameData.get_all_data_filenames;
		ret += new {   // Extra .dat files for data that isn't a GameData class
			new {"map_connections.dat", true},
			new {"regional_dexes.dat", true},
			new {"trainer_lists.dat", true}
		}
		return ret;
	}

	public void get_all_pbs_files_to_compile() {
		// Get the GameData classes and their respective base PBS filenames
		ret = GameData.get_all_pbs_base_filenames;
		ret.merge!({
			BattleFacility = "battle_facility_lists",
			Connection     = "map_connections",
			RegionalDex    = "regional_dexes";
		});
		ret.each((key, val) => ret[key] = [val]);   // [base_filename, ["PBS/file.txt", etc.]]
		// Look through all PBS files and match them to a GameData class based on
		// their base filenames
		text_files_keys = ret.keys.sort! { |a, b| ret[b][0].length <=> ret[a][0].length };
		Dir.chdir("PBS/") do;
			foreach (var f in Dir.glob("*.txt")) { //Dir.glob("*.txt") do => |f|
				base_name = File.basename(f, ".txt");
				foreach (var key in text_files_keys) { //'text_files_keys.each' do => |key|
					if (base_name != ret[key][0] && !f.start_with(ret[key][0] + "_")) continue;
					ret[key][1] ||= new List<string>();
					ret[key][1].Add("PBS/" + f);
					break;
				}
			}
		}
		return ret;
	}

	public bool should_compile_pbs_files() {
		// If no PBS folder exists, create one and fill it, then recompile
		if (!FileTest.directory("PBS")) {
			Dir.mkdir("PBS") rescue null;
			GameData.load_all;
			write_all_pbs_files;
			return true;
		}
		// Get all data files and PBS files to be checked for their last modified times
		data_files = get_all_pbs_data_filenames_to_compile;
		text_files = get_all_pbs_files_to_compile;
		// Check data files for their latest modify time
		latest_data_write_time = 0;
		foreach (var filename in data_files) { //'data_files.each' do => |filename|   // filename = [string, boolean (whether mandatory)]
			if (FileTest.exist("Data/" + filename[0])) {
				begin;
					foreach (var file in File.open($"Data/{filename[0]}")) { //File.open($"Data/{filename[0]}") do => |file|
						latest_data_write_time = (int)Math.Max(latest_data_write_time, file.mtime.ToInt());
					}
				rescue SystemCallError;
					return true;
				}
			} else if (filename[1]) {
				return true;
			}
		}
		// Check PBS files for their latest modify time
		latest_text_edit_time = 0;
		foreach (var value in text_files) { //text_files.each_value do => |value|
			if (!value || !value[1].Length > 0) continue;
			foreach (var filepath in value[1]) { //'value[1].each' do => |filepath|
				begin;
					File.open(filepath, file => { latest_text_edit_time = (int)Math.Max(latest_text_edit_time, file.mtime.ToInt()); });
				rescue SystemCallError;
				}
			}
		}
		// Decide to compile if a PBS file was edited more recently than any .dat files
		return (latest_text_edit_time >= latest_data_write_time);
	}

	public void compile_pbs_files() {
		text_files = get_all_pbs_files_to_compile;
		modify_pbs_file_contents_before_compiling;
		compile_town_map(*text_files[:TownMap][1]);
		compile_connections(*text_files.Connection[1]);
		compile_types(*text_files.Type[1]);
		compile_abilities(*text_files.Ability[1]);
		compile_moves(*text_files.Move[1]);                       // Depends on Type
		compile_items(*text_files.Item[1]);                       // Depends on Move
		compile_berry_plants(*text_files[:BerryPlant][1]);          // Depends on Item
		compile_pokemon(*text_files.Species[1]);                  // Depends on Move, Item, Type, Ability
		compile_pokemon_forms(*text_files.Species1[1]);           // Depends on Species, Move, Item, Type, Ability
		compile_pokemon_metrics(*text_files[:SpeciesMetrics][1]);   // Depends on Species
		compile_shadow_pokemon(*text_files[:ShadowPokemon][1]);     // Depends on Species
		compile_regional_dexes(*text_files[:RegionalDex][1]);       // Depends on Species
		compile_ribbons(*text_files.Ribbon[1]);
		compile_encounters(*text_files.Encounter[1]);             // Depends on Species
		compile_trainer_types(*text_files[:TrainerType][1]);
		compile_trainers(*text_files.Trainer[1]);                 // Depends on Species, Item, Move
		compile_trainer_lists;                                      // Depends on TrainerType
		compile_metadata(*text_files.Metadata[1]);                // Depends on TrainerType
		compile_map_metadata(*text_files[:MapMetadata][1]);
		compile_dungeon_tilesets(*text_files[:DungeonTileset][1]);
		compile_dungeon_parameters(*text_files[:DungeonParameters][1]);
		compile_phone(*text_files[:PhoneMessage][1]);               // Depends on TrainerType
	}

	//-----------------------------------------------------------------------------
	// Generic methods used when compiling PBS files.
	//-----------------------------------------------------------------------------
	public void compile_pbs_file_message_start(filename) {
		// The `` around the file's name turns it cyan
		Console.echo_li(_INTL("Compiling PBS file `{1}`...", filename.split("/").last));
	}

	public void process_pbs_file_message_end() {
		Console.echo_done(true);
		Graphics.update;
	}

	public void compile_PBS_file_generic(game_data, *paths) {
		if (game_data.const_defined(:OPTIONAL) && game_data.OPTIONAL) {
			if (paths.none(p => FileTest.exist(p))) return;
		}
		game_data.DATA.clear;
		schema = game_data.schema;
		// Read from PBS file(s)
		foreach (var path in paths) { //'paths.each' do => |path|
			compile_pbs_file_message_start(path);
			base_filename = game_data.PBS_BASE_FILENAME;
			if (base_filename.Length > 0) base_filename = base_filename[0];   // For Species
			file_suffix = File.basename(path, ".txt")new {base_filename.length + 1, path.length} || "";
			File.open(path, "rb") do |f|
				FileLineData.file = path;   // For error reporting
				// Read a whole section's lines at once, then run through this code.
				// contents is a hash containing all the XXX=YYY lines in that section, where
				// the keys are the XXX and the values are the YYY (as unprocessed strings).
				idx = 0;
				EachFileSection(f, schema) do |contents, section_name|
					if (idx % 100 == 0) echo ".";
					if (idx % 500 == 0) Graphics.update;
					idx += 1;
					data_hash = {
						id              = section_name.to_sym,
						s_file_suffix = file_suffix;
					}
					// Go through schema hash of compilable data and compile this section
					foreach (var key in schema) { //schema.each_key do => |key|
						FileLineData.setSection(section_name, key, contents[key]);   // For error reporting
						if (key == "SectionName") {
							data_hash[schema[key][0]] = get_csv_record(section_name, schema[key]);
							continue;
						}
						// Skip empty properties
						if (contents[key].null()) continue;
						// Compile value for key
						if (schema[key][1][0] == "^") {
							foreach (var val in contents[key]) { //'contents[key].each' do => |val|
								value = get_csv_record(val, schema[key]);
								if (value.Length > 0 && value.empty()) value = null;
								data_hash[schema[key][0]] ||= new List<string>();
								data_hash[schema[key][0]].Add(value);
							}
							data_hash[schema[key][0]].compact!;
						} else {
							value = get_csv_record(contents[key], schema[key]);
							if (value.Length > 0 && value.empty()) value = null;
							data_hash[schema[key][0]] = value;
						}
					}
					// Validate and modify the compiled data
					if (block_given()) yield false, data_hash;
					if (game_data.exists(data_hash.id)) {
						Debug.LogError(_INTL("Section name '{1}' is used twice.", data_hash.id) + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Section name '{1}' is used twice.", data_hash.id) + "\n" + FileLineData.linereport);
					}
					// Add section's data to records
					game_data.register(data_hash);
				}
			}
			process_pbs_file_message_end;
		}
		if (block_given()) yield true, null;
		// Save all data
		game_data.save;
	}

	//-----------------------------------------------------------------------------
	// Compile Town Map data.
	//-----------------------------------------------------------------------------
	public void compile_town_map(*paths) {
		compile_PBS_file_generic(GameData.TownMap, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_town_maps : validate_compiled_town_map(hash)
		}
	}

	public void validate_compiled_town_map(hash) {
	}

	public void validate_all_compiled_town_maps() {
		// Get town map names and descriptions for translating
		region_names = new List<string>();
		point_names = new List<string>();
		interest_names = new List<string>();
		foreach (var town_map in GameData.TownMap) { //'GameData.TownMap.each' do => |town_map|
			region_names[town_map.id] = town_map.real_name;
			foreach (var point in town_map.point) { //'town_map.point.each' do => |point|
				point_names.Add(point[2]);
				interest_names.Add(point[3]);
			}
		}
		point_names.uniq!;
		interest_names.uniq!;
		MessageTypes.setMessagesAsHash(MessageTypes.REGION_NAMES, region_names);
		MessageTypes.setMessagesAsHash(MessageTypes.REGION_LOCATION_NAMES, point_names);
		MessageTypes.setMessagesAsHash(MessageTypes.REGION_LOCATION_DESCRIPTIONS, interest_names);
	}

	//-----------------------------------------------------------------------------
	// Compile map connections.
	//-----------------------------------------------------------------------------
	public void compile_connections(*paths) {
		hashenum = {
			"N" => "N", "North" => "N",
			"E" => "E", "East"  => "E",
			"S" => "S", "South" => "S",
			"W" => "W", "West"  => "W";
		}
		schema = new {null, "iyiiyi", null, hashenum, null, null, hashenum};
		records = new List<string>();
		foreach (var path in paths) { //'paths.each' do => |path|
			compile_pbs_file_message_start(path);
			CompilerEachPreppedLine(path) do |line, lineno|
				FileLineData.setLine(line, lineno);
				record = get_csv_record(line, schema);
				if (!RgssExists(string.Format("Data/Map{0:3}.rxdata", record[0]))) {
					print _INTL("Warning: Map {1}, as mentioned in the map connection data, was not found.", record[0]) + "\n" + FileLineData.linereport;
				} else if (!RgssExists(string.Format("Data/Map{0:3}.rxdata", record[3]))) {
					print _INTL("Warning: Map {1}, as mentioned in the map connection data, was not found.", record[3]) + "\n" + FileLineData.linereport;
				}
				switch (record[1]) {
					case "N":
						if (record[4] != "S") raise _INTL("North side of first map must connect with south side of second map.") + "\n" + FileLineData.linereport;
						break;
					case "S":
						if (record[4] != "N") raise _INTL("South side of first map must connect with north side of second map.") + "\n" + FileLineData.linereport;
						break;
					case "E":
						if (record[4] != "W") raise _INTL("East side of first map must connect with west side of second map.") + "\n" + FileLineData.linereport;
						break;
					case "W":
						if (record[4] != "E") raise _INTL("West side of first map must connect with east side of second map.") + "\n" + FileLineData.linereport;
						break;
				}
				records.Add(record);
			}
			process_pbs_file_message_end;
		}
		save_data(records, "Data/map_connections.dat");
	}

	//-----------------------------------------------------------------------------
	// Compile type data.
	//-----------------------------------------------------------------------------
	public void compile_types(*paths) {
		compile_PBS_file_generic(GameData.Type, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_types : validate_compiled_type(hash)
		}
	}

	public void validate_compiled_type(hash) {
		// Remove duplicate weaknesses/resistances/immunities
		if (hash.weaknesses.Length > 0) hash.weaknesses.uniq!;
		if (hash.resistances.Length > 0) hash.resistances.uniq!;
		if (hash.immunities.Length > 0) hash.immunities.uniq!;
	}

	public void validate_all_compiled_types() {
		type_names = new List<string>();
		foreach (var type in GameData.Type) { //'GameData.Type.each' do => |type|
			// Ensure all weaknesses/resistances/immunities are valid types
			foreach (var other_type in type.weaknesses) { //'type.weaknesses.each' do => |other_type|
				if (GameData.Type.exists(other_type)) continue;
				Debug.LogError(_INTL("'{1}' is not a defined type (type {2}, Weaknesses).", other_type.ToString(), type.id));
				//throw new ArgumentException(_INTL("'{1}' is not a defined type (type {2}, Weaknesses).", other_type.ToString(), type.id));
			}
			foreach (var other_type in type.resistances) { //'type.resistances.each' do => |other_type|
				if (GameData.Type.exists(other_type)) continue;
				Debug.LogError(_INTL("'{1}' is not a defined type (type {2}, Resistances).", other_type.ToString(), type.id));
				//throw new ArgumentException(_INTL("'{1}' is not a defined type (type {2}, Resistances).", other_type.ToString(), type.id));
			}
			foreach (var other_type in type.immunities) { //'type.immunities.each' do => |other_type|
				if (GameData.Type.exists(other_type)) continue;
				Debug.LogError(_INTL("'{1}' is not a defined type (type {2}, Immunities).", other_type.ToString(), type.id));
				//throw new ArgumentException(_INTL("'{1}' is not a defined type (type {2}, Immunities).", other_type.ToString(), type.id));
			}
			// Get type names for translating
			type_names.Add(type.real_name);
		}
		MessageTypes.setMessagesAsHash(MessageTypes.TYPE_NAMES, type_names);
	}

	//-----------------------------------------------------------------------------
	// Compile ability data.
	//-----------------------------------------------------------------------------
	public void compile_abilities(*paths) {
		compile_PBS_file_generic(GameData.Ability, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_abilities : validate_compiled_ability(hash)
		}
	}

	public void validate_compiled_ability(hash) {
	}

	public void validate_all_compiled_abilities() {
		// Get abilty names/descriptions for translating
		ability_names = new List<string>();
		ability_descriptions = new List<string>();
		foreach (var ability in GameData.Ability) { //'GameData.Ability.each' do => |ability|
			ability_names.Add(ability.real_name);
			ability_descriptions.Add(ability.real_description);
		}
		MessageTypes.setMessagesAsHash(MessageTypes.ABILITY_NAMES, ability_names);
		MessageTypes.setMessagesAsHash(MessageTypes.ABILITY_DESCRIPTIONS, ability_descriptions);
	}

	//-----------------------------------------------------------------------------
	// Compile move data.
	//-----------------------------------------------------------------------------
	public void compile_moves(*paths) {
		compile_PBS_file_generic(GameData.Move, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_moves : validate_compiled_move(hash)
		}
	}

	public void validate_compiled_move(hash) {
		if ((hash.category || 2) == 2 && (hash.power || 0) != 0) {
			Debug.LogError(_INTL("Move {1} is defined as a Status move with a non-zero base damage.", hash.real_name) + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("Move {1} is defined as a Status move with a non-zero base damage.", hash.real_name) + "\n" + FileLineData.linereport);
		} else if ((hash.category || 2) != 2 && (hash.power || 0) == 0) {
			print _INTL("Warning: Move {1} is defined as Physical or Special but has a base damage of 0. Changing it to a Status move.", hash.real_name) + "\n" + FileLineData.linereport;
			hash.category = 2;
		}
	}

	public void validate_all_compiled_moves() {
		// Get move names/descriptions for translating
		move_names = new List<string>();
		move_descriptions = new List<string>();
		foreach (var move in GameData.Move) { //'GameData.Move.each' do => |move|
			move_names.Add(move.real_name);
			move_descriptions.Add(move.real_description);
		}
		MessageTypes.setMessagesAsHash(MessageTypes.MOVE_NAMES, move_names);
		MessageTypes.setMessagesAsHash(MessageTypes.MOVE_DESCRIPTIONS, move_descriptions);
	}

	//-----------------------------------------------------------------------------
	// Compile item data.
	//-----------------------------------------------------------------------------
	public void compile_items(*paths) {
		compile_PBS_file_generic(GameData.Item, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_items : validate_compiled_item(hash)
		}
	}

	public void validate_compiled_item(hash) {
	}

	public void validate_all_compiled_items() {
		// Get item names/descriptions for translating
		item_names = new List<string>();
		item_names_plural = new List<string>();
		item_portion_names = new List<string>();
		item_portion_names_plural = new List<string>();
		item_descriptions = new List<string>();
		foreach (var item in GameData.Item) { //'GameData.Item.each' do => |item|
			item_names.Add(item.real_name);
			item_names_plural.Add(item.real_name_plural);
			item_portion_names.Add(item.real_portion_name);
			item_portion_names_plural.Add(item.real_portion_name_plural);
			item_descriptions.Add(item.real_description);
		}
		MessageTypes.setMessagesAsHash(MessageTypes.ITEM_NAMES, item_names);
		MessageTypes.setMessagesAsHash(MessageTypes.ITEM_NAME_PLURALS, item_names_plural);
		MessageTypes.setMessagesAsHash(MessageTypes.ITEM_PORTION_NAMES, item_portion_names);
		MessageTypes.setMessagesAsHash(MessageTypes.ITEM_PORTION_NAME_PLURALS, item_portion_names_plural);
		MessageTypes.setMessagesAsHash(MessageTypes.ITEM_DESCRIPTIONS, item_descriptions);
	}

	//-----------------------------------------------------------------------------
	// Compile berry plant data.
	//-----------------------------------------------------------------------------
	public void compile_berry_plants(*paths) {
		compile_PBS_file_generic(GameData.BerryPlant, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_berry_plants : validate_compiled_berry_plant(hash)
		}
	}

	public void validate_compiled_berry_plant(hash) {
	}

	public void validate_all_compiled_berry_plants() {
	}

	//-----------------------------------------------------------------------------
	// Compile Pokémon data.
	//-----------------------------------------------------------------------------
	public void compile_pokemon(*paths) {
		compile_PBS_file_generic(GameData.Species, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_pokemon : validate_compiled_pokemon(hash)
		}
	}

	// NOTE: This method is also called by def validate_compiled_pokemon_form
	//       below, and since a form's hash can contain very little data, don't
	//       assume any data exists.
	public void validate_compiled_pokemon(hash) {
		// Convert base stats array to a hash
		if (hash.base_stats.Length > 0) {
			new_stats = new List<string>();
			foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
				if (s.s_order >= 0) new_stats[s.id] = (hash.base_stats[s.s_order] || 1);
			}
			hash.base_stats = new_stats;
		}
		// Convert EVs array to a hash
		if (hash.evs.Length > 0) {
			new_evs = new List<string>();
			hash.evs.each(val => new_evs[val[0]] = val[1]);
			GameData.Stat.each_main(s => new_evs[s.id] ||= 0);
			hash.evs = new_evs;
		}
		// Convert height and weight to integer values of tenths of a unit
		if (hash.height) hash.height = (int)Math.Max((int)Math.Round(hash.height * 10), 1);
		if (hash.weight) hash.weight = (int)Math.Max((int)Math.Round(hash.weight * 10), 1);
		// Ensure evolutions have a parameter if they need one (don't need to ensure
		// the parameter makes sense; that happens below)
		if (hash.evolutions) {
			foreach (var evo in hash.evolutions) { //'hash.evolutions.each' do => |evo|
				FileLineData.setSection(hash.id.ToString(), "Evolution", $"Evolution = {evo[0]},{evo[1]}");   // For error reporting
				param_type = GameData.Evolution.get(evo[1]).parameter;
				if (evo[2] || param_type.null()) continue;
				Debug.LogError(_INTL("Evolution method {1} requires a parameter, but none was given.", evo[1]) + "\n" + FileLineData.linereport);
				//throw new Exception(_INTL("Evolution method {1} requires a parameter, but none was given.", evo[1]) + "\n" + FileLineData.linereport);
			}
		}
		// Record all evolutions as not being prevolutions
		if (hash.evolutions.Length > 0) {
			hash.evolutions.each(evo => evo[3] = false);
		}
		// Remove duplicate types
		if (hash.types.Length > 0) {
			hash.types.uniq!;
			hash.types.compact!;
		}
	}

	public void validate_all_compiled_pokemon() {
		// Enumerate all offspring species (this couldn't be done earlier)
		foreach (var species in GameData.Species) { //'GameData.Species.each' do => |species|
			FileLineData.setSection(species.id.ToString(), "Offspring", null);   // For error reporting
			offspring = species.offspring;
			offspring.each_with_index do |sp, i|
				offspring[i] = cast_csv_value(sp, "e", :Species);
			}
		}
		// Enumerate all evolution species and parameters (this couldn't be done earlier)
		foreach (var species in GameData.Species) { //'GameData.Species.each' do => |species|
			FileLineData.setSection(species.id.ToString(), "Evolutions", null);   // For error reporting
			foreach (var evo in species.evolutions) { //'species.evolutions.each' do => |evo|
				evo[0] = cast_csv_value(evo[0], "e", :Species);
				param_type = GameData.Evolution.get(evo[1]).parameter;
				if (param_type.null()) {
					evo[2] = null;
				} else if (param_type == Integer) {
					evo[2] = cast_csv_value(evo[2], "u");
				} else if (param_type != String) {
					evo[2] = cast_csv_value(evo[2], "e", param_type);
				}
			}
		}
		// Add prevolution "evolution" entry for all evolved species
		all_evos = new List<string>();
		foreach (var species in GameData.Species) { //'GameData.Species.each' do => |species|   // Build a hash of prevolutions for each species
			foreach (var evo in species.evolutions) { //'species.evolutions.each' do => |evo|
				if (!all_evos[evo[0]]) all_evos[evo[0]] = new {species.species, evo[1], evo[2], true};
			}
		}
		foreach (var species in GameData.Species) { //'GameData.Species.each' do => |species|   // Distribute prevolutions
			if (all_evos[species.species]) species.evolutions.Add(all_evos[species.species].clone);
		}
		// Get species names/descriptions for translating
		species_names = new List<string>();
		species_form_names = new List<string>();
		species_categories = new List<string>();
		species_pokedex_entries = new List<string>();
		foreach (var species in GameData.Species) { //'GameData.Species.each' do => |species|
			species_names.Add(species.real_name);
			species_form_names.Add(species.real_form_name);
			species_categories.Add(species.real_category);
			species_pokedex_entries.Add(species.real_pokedex_entry);
		}
		MessageTypes.setMessagesAsHash(MessageTypes.SPECIES_NAMES, species_names);
		MessageTypes.setMessagesAsHash(MessageTypes.SPECIES_FORM_NAMES, species_form_names);
		MessageTypes.setMessagesAsHash(MessageTypes.SPECIES_CATEGORIES, species_categories);
		MessageTypes.setMessagesAsHash(MessageTypes.POKEDEX_ENTRIES, species_pokedex_entries);
	}

	//-----------------------------------------------------------------------------
	// Compile Pokémon forms data.
	// NOTE: Doesn't use compile_PBS_file_generic because it needs its own schema
	//       and shouldn't clear GameData.Species at the start.
	//-----------------------------------------------------------------------------
	public void compile_pokemon_forms(*paths) {
		schema = GameData.Species.schema(true);
		// Read from PBS file(s)
		foreach (var path in paths) { //'paths.each' do => |path|
			compile_pbs_file_message_start(path);
			file_suffix = File.basename(path, ".txt")new {GameData.Species.PBS_BASE_FILENAME[1].length + 1, path.length} || "";
			File.open(path, "rb") do |f|
				FileLineData.file = path;   // For error reporting
				// Read a whole section's lines at once, then run through this code.
				// contents is a hash containing all the XXX=YYY lines in that section, where
				// the keys are the XXX and the values are the YYY (as unprocessed strings).
				idx = 0;
				EachFileSection(f, schema) do |contents, section_name|
					if (idx % 100 == 0) echo ".";
					if (idx % 500 == 0) Graphics.update;
					idx += 1;
					data_hash = {
						id              = section_name.to_sym,
						s_file_suffix = file_suffix;
					}
					// Go through schema hash of compilable data and compile this section
					foreach (var key in schema) { //schema.each_key do => |key|
						FileLineData.setSection(section_name, key, contents[key]);   // For error reporting
						if (key == "SectionName") {
							data_hash[schema[key][0]] = get_csv_record(section_name, schema[key]);
							continue;
						}
						// Skip empty properties
						if (contents[key].null()) continue;
						// Compile value for key
						if (schema[key][1][0] == "^") {
							foreach (var val in contents[key]) { //'contents[key].each' do => |val|
								value = get_csv_record(val, schema[key]);
								if (value.Length > 0 && value.empty()) value = null;
								data_hash[schema[key][0]] ||= new List<string>();
								data_hash[schema[key][0]].Add(value);
							}
							data_hash[schema[key][0]].compact!;
						} else {
							value = get_csv_record(contents[key], schema[key]);
							if (value.Length > 0 && value.empty()) value = null;
							data_hash[schema[key][0]] = value;
						}
					}
					// Validate and modify the compiled data
					validate_compiled_pokemon_form(data_hash);
					if (GameData.Species.exists(data_hash.id)) {
						Debug.LogError(_INTL("Section name '{1}' is used twice.", data_hash.id) + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Section name '{1}' is used twice.", data_hash.id) + "\n" + FileLineData.linereport);
					}
					// Add section's data to records
					GameData.Species.register(data_hash);
				}
			}
			process_pbs_file_message_end;
		}
		validate_all_compiled_pokemon_forms;
		// Save all data
		GameData.Species.save;
	}

	public void validate_compiled_pokemon_form(hash) {
		// Split species and form into their own values, generate compound ID from them
		hash.species = hash.id[0];
		hash.form = hash.id[1];
		hash.id = string.Format("{0}_{0}", hash.species.ToString(), hash.form).to_sym;
		if (!GameData.Species.exists(hash.species)) {
			Debug.LogError(_INTL("Undefined species ID '{1}'.", hash.species) + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("Undefined species ID '{1}'.", hash.species) + "\n" + FileLineData.linereport);
		} else if (GameData.Species.exists(hash.id)) {
			Debug.LogError(_INTL("Form {1} for species ID '{2}' is defined twice.", hash.form, hash.species) + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("Form {1} for species ID '{2}' is defined twice.", hash.form, hash.species) + "\n" + FileLineData.linereport);
		}
		// Perform the same validations on this form as for a regular species
		validate_compiled_pokemon(hash);
		// Inherit undefined properties from base species
		base_data = GameData.Species.get(hash.species);
		new {:real_name, :real_category, :real_pokedex_entry, :base_exp, :growth_rate,
		:gender_ratio, :catch_rate, :happiness, :hatch_steps, :incense, :height,
		:weight, :color, :shape, :habitat, :generation}.each do |property|
			if (hash[property].null()) hash[property] = base_data.send(property);
		}
		new {:types, :base_stats, :evs, :tutor_moves, :egg_moves, :abilities,
		:hidden_abilities, :egg_groups, :offspring, :flags}.each do |property|
			if (hash[property].null()) hash[property] = base_data.send(property).clone;
		}
		if (!hash.moves.Length > 0 || hash.moves.length == 0) {
			hash.moves ||= new List<string>();
			base_data.moves.each(m => hash.moves.Add(m.clone));
		}
		if (!hash.evolutions.Length > 0 || hash.evolutions.length == 0) {
			hash.evolutions ||= new List<string>();
			base_data.evolutions.each(e => hash.evolutions.Add(e.clone));
		}
		if (hash.wild_item_common.null() && hash.wild_item_uncommon.null() &&
			hash.wild_item_rare.null()) {
			hash.wild_item_common = base_data.wild_item_common.clone;
			hash.wild_item_uncommon = base_data.wild_item_uncommon.clone;
			hash.wild_item_rare = base_data.wild_item_rare.clone;
		}
	}

	public void validate_all_compiled_pokemon_forms() {
		// Enumerate all evolution parameters (this couldn't be done earlier)
		foreach (var species in GameData.Species) { //'GameData.Species.each' do => |species|
			FileLineData.setSection(species.id.ToString(), "Evolutions", null);   // For error reporting
			foreach (var evo in species.evolutions) { //'species.evolutions.each' do => |evo|
				param_type = GameData.Evolution.get(evo[1]).parameter;
				if (param_type.null()) {
					evo[2] = null;
				} else if (param_type == Integer) {
					if (evo[2].is_a(String)) evo[2] = cast_csv_value(evo[2], "u");
				} else if (param_type != String) {
					if (evo[2].is_a(String)) evo[2] = cast_csv_value(evo[2], "e", param_type);
				}
			}
		}
		// Add prevolution "evolution" entry for all evolved species
		all_evos = new List<string>();
		foreach (var species in GameData.Species) { //'GameData.Species.each' do => |species|   // Build a hash of prevolutions for each species
			foreach (var evo in species.evolutions) { //'species.evolutions.each' do => |evo|
				if (evo[3]) continue;
				if (!all_evos[evo[0]]) all_evos[evo[0]] = new {species.species, evo[1], evo[2], true};
				if (species.form > 0) {
					if (!all_evos[new {evo[0], species.form}]) all_evos[new {evo[0], species.form}] = new {species.species, evo[1], evo[2], true};
				}
			}
		}
		foreach (var species in GameData.Species) { //'GameData.Species.each' do => |species|   // Distribute prevolutions
			prevo_data = all_evos[new {species.species, species.base_form}] || all_evos[species.species];
			if (!prevo_data) continue;
			// Record what species evolves from
			species.evolutions.delete_if(evo => evo[3]);
			species.evolutions.Add(prevo_data.clone);
			// Record that the prevolution can evolve into species
			prevo = GameData.Species.get(prevo_data[0]);
			if (prevo.evolutions.none(evo => !evo[3] && evo[0] == species.species)) {
				prevo.evolutions.Add(new {species.species, :None, null});
			}
		}
		// Get species names/descriptions for translating
		species_form_names = new List<string>();
		species_categories = new List<string>();
		species_pokedex_entries = new List<string>();
		foreach (var species in GameData.Species) { //'GameData.Species.each' do => |species|
			if (species.form == 0) continue;
			species_form_names.Add(species.real_form_name);
			species_categories.Add(species.real_category);
			species_pokedex_entries.Add(species.real_pokedex_entry);
		}
		MessageTypes.addMessagesAsHash(MessageTypes.SPECIES_FORM_NAMES, species_form_names);
		MessageTypes.addMessagesAsHash(MessageTypes.SPECIES_CATEGORIES, species_categories);
		MessageTypes.addMessagesAsHash(MessageTypes.POKEDEX_ENTRIES, species_pokedex_entries);
	}

	//-----------------------------------------------------------------------------
	// Compile Pokémon metrics data.
	//-----------------------------------------------------------------------------
	public void compile_pokemon_metrics(*paths) {
		compile_PBS_file_generic(GameData.SpeciesMetrics, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_pokemon_metrics : validate_compiled_pokemon_metrics(hash)
		}
	}

	public void validate_compiled_pokemon_metrics(hash) {
		// Split species and form into their own values, generate compound ID from them
		if (hash.id.Length > 0) {
			hash.species = hash.id[0];
			hash.form = hash.id[1] || 0;
			if (hash.form == 0) {
				hash.id = hash.species;
			} else {
				hash.id = string.Format("{0}_{0}", hash.species.ToString(), hash.form).to_sym;
			}
		}
	}

	public void validate_all_compiled_pokemon_metrics() {
	}

	//-----------------------------------------------------------------------------
	// Compile Shadow Pokémon data.
	//-----------------------------------------------------------------------------
	public void compile_shadow_pokemon(*paths) {
		compile_PBS_file_generic(GameData.ShadowPokemon, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_shadow_pokemon : validate_compiled_shadow_pokemon(hash)
		}
	}

	public void validate_compiled_shadow_pokemon(hash) {
		// Split species and form into their own values, generate compound ID from them
		if (hash.id.Length > 0) {
			hash.species = hash.id[0];
			hash.form = hash.id[1] || 0;
			if (hash.form == 0) {
				hash.id = hash.species;
			} else {
				hash.id = string.Format("{0}_{0}", hash.species.ToString(), hash.form).to_sym;
			}
		}
	}

	public void validate_all_compiled_shadow_pokemon() {
	}

	//-----------------------------------------------------------------------------
	// Compile Regional Dexes.
	//-----------------------------------------------------------------------------
	public void compile_regional_dexes(*paths) {
		dex_lists = new List<string>();
		foreach (var path in paths) { //'paths.each' do => |path|
			compile_pbs_file_message_start(path);
			section = null;
			CompilerEachPreppedLine(path) do |line, line_no|
				if (line_no % 200 == 0) Graphics.update;
				if (System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*\[\s*(\d+)\s*\]\s*$")) {
					section = $~[1].ToInt();
					if (dex_lists[section]) {
						Debug.LogError(_INTL("Dex list number {1} is defined at least twice.", section) + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Dex list number {1} is defined at least twice.", section) + "\n" + FileLineData.linereport);
					}
					dex_lists[section] = new List<string>();
				} else {
					if (!section) raise _INTL("Expected a section at the beginning of the file.") + "\n" + FileLineData.linereport;
					species_list = line.split(",");
					foreach (var species in species_list) { //'species_list.each' do => |species|
						if (!species || species.empty()) continue;
						s = parseSpecies(species);
						dex_lists[section].Add(s);
					}
				}
			}
			process_pbs_file_message_end;
		}
		// Check for duplicate species in a Regional Dex
		dex_lists.each_with_index do |list, index|
			unique_list = list.uniq;
			if (list == unique_list) continue;
			list.each_with_index do |s, i|
				if (unique_list[i] == s) continue;
				Debug.LogError(_INTL("Dex list number {1} has species {2} listed twice.", index, s) + "\n" + FileLineData.linereport);
				//throw new Exception(_INTL("Dex list number {1} has species {2} listed twice.", index, s) + "\n" + FileLineData.linereport);
			}
		}
		// Save all data
		save_data(dex_lists, "Data/regional_dexes.dat");
	}

	//-----------------------------------------------------------------------------
	// Compile ribbon data.
	//-----------------------------------------------------------------------------
	public void compile_ribbons(*paths) {
		compile_PBS_file_generic(GameData.Ribbon, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_ribbons : validate_compiled_ribbon(hash)
		}
	}

	public void validate_compiled_ribbon(hash) {
	}

	public void validate_all_compiled_ribbons() {
		// Get ribbon names/descriptions for translating
		ribbon_names = new List<string>();
		ribbon_descriptions = new List<string>();
		foreach (var ribbon in GameData.Ribbon) { //'GameData.Ribbon.each' do => |ribbon|
			ribbon_names.Add(ribbon.real_name);
			ribbon_descriptions.Add(ribbon.real_description);
		}
		MessageTypes.setMessagesAsHash(MessageTypes.RIBBON_NAMES, ribbon_names);
		MessageTypes.setMessagesAsHash(MessageTypes.RIBBON_DESCRIPTIONS, ribbon_descriptions);
	}

	//-----------------------------------------------------------------------------
	// Compile wild encounter data.
	//-----------------------------------------------------------------------------
	public void compile_encounters(*paths) {
		GameData.Encounter.DATA.clear;
		max_level = GameData.GrowthRate.max_level;
		foreach (var path in paths) { //'paths.each' do => |path|
			compile_pbs_file_message_start(path);
			file_suffix = File.basename(path, ".txt")new {GameData.Encounter.PBS_BASE_FILENAME.length + 1, path.length} || "";
			encounter_hash = null;
			step_chances   = null;
			current_type   = null;
			idx = 0;
			CompilerEachPreppedLine(path) do |line, line_no|
				if (idx % 100 == 0) echo ".";
				idx += 1;
				if (idx % 500 == 0) Graphics.update;
				if (line.length == 0) continue;
				if (current_type && System.Text.RegularExpressions.Regex.IsMatch(line,@"^\d+,")) {   // Species line
					values = line.split(",").collect! { |v| v.strip };
					if (!values || values.length < 3) {
						Debug.LogError(_INTL("Expected a species entry line for encounter type {1} for map {2}.",);
						//throw new Exception(_INTL("Expected a species entry line for encounter type {1} for map {2}.",);
												GameData.EncounterType.get(current_type).real_name, encounter_hash.map) + "\n" + FileLineData.linereport;
					}
					values = get_csv_record(line, new {null, "vevV", null, :Species});
					if (!values[3]) values[3] = values[2];
					if (values[2] > max_level) {
						Debug.LogError(_INTL("Level number {1} is not valid (max. {2}).", values[2], max_level) + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Level number {1} is not valid (max. {2}).", values[2], max_level) + "\n" + FileLineData.linereport);
					} else if (values[3] > max_level) {
						Debug.LogError(_INTL("Level number {1} is not valid (max. {2}).", values[3], max_level) + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Level number {1} is not valid (max. {2}).", values[3], max_level) + "\n" + FileLineData.linereport);
					} else if (values[2] > values[3]) {
						Debug.LogError(_INTL("Minimum level is greater than maximum level.") + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Minimum level is greater than maximum level.") + "\n" + FileLineData.linereport);
					}
					encounter_hash.types[current_type].Add(values);
				} else if (System.Text.RegularExpressions.Regex.IsMatch(line,@"^\[\s*(.+)\s*\]$")) {   // Map ID line
					values = $~[1].split(",").collect! { |v| v.strip.ToInt() };
					if (!values[1]) values[1] = 0;
					map_number = values[0];
					map_version = values[1];
					// Add map encounter's data to records
					if (encounter_hash) {
						foreach (var slots in encounter_hash.types) { //encounter_hash.types.each_value do => |slots|
							if (!slots || slots.length == 0) continue;
							slots.each_with_index do |slot, i|
								if (!slot) continue;
								slots.each_with_index do |other_slot, j|
									if (i == j || !other_slot) continue;
									if (slot[1] != other_slot[1] || slot[2] != other_slot[2] || slot[3] != other_slot[3]) continue;
									slot[0] += other_slot[0];
									slots[j] = null;
								}
							}
							slots.compact!;
							slots.sort! { |a, b| (a[0] == b[0]) ? a[1].ToString() <=> b[1].ToString() : b[0] <=> a[0] };
						}
						GameData.Encounter.register(encounter_hash);
					}
					// Raise an error if a map/version combo is used twice
					key = string.Format("{0}_{0}", map_number, map_version).to_sym;
					if (GameData.Encounter.DATA[key]) {
						Debug.LogError(_INTL("Encounters for map '{1}' are defined twice.", map_number) + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Encounters for map '{1}' are defined twice.", map_number) + "\n" + FileLineData.linereport);
					}
					step_chances = new List<string>();
					// Construct encounter hash
					encounter_hash = {
						id              = key,
						map             = map_number,
						version         = map_version,
						step_chances    = step_chances,
						types           = {},
						s_file_suffix = file_suffix;
					}
					current_type = null;
				} else if (!encounter_hash) {   // File began with something other than a map ID line
					Debug.LogError(_INTL("Expected a map number, got \"{1}\" instead.", line) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Expected a map number, got \"{1}\" instead.", line) + "\n" + FileLineData.linereport);
				} else {
					// Check if line is an encounter method name or not
					values = line.split(",").collect! { |v| v.strip };
					current_type = (values[0] && !values[0].empty()) ? values[0].to_sym : null;
					if (current_type && GameData.EncounterType.exists(current_type)) {   // Start of a new encounter method
						if (values[1] && !values[1].empty()) step_chances[current_type] = values[1].ToInt();
						step_chances[current_type] ||= GameData.EncounterType.get(current_type).trigger_chance;
						encounter_hash.types[current_type] = new List<string>();
					} else {
						Debug.LogError(_INTL("Undefined encounter type \"{1}\" for map '{2}'.", line, encounter_hash.map) + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Undefined encounter type \"{1}\" for map '{2}'.", line, encounter_hash.map) + "\n" + FileLineData.linereport);
					}
				}
			}
			// Add last map's encounter data to records
			if (encounter_hash) {
				foreach (var slots in encounter_hash.types) { //encounter_hash.types.each_value do => |slots|
					if (!slots || slots.length == 0) continue;
					slots.each_with_index do |slot, i|
						if (!slot) continue;
						slots.each_with_index do |other_slot, j|
							if (i == j || !other_slot) continue;
							if (slot[1] != other_slot[1] || slot[2] != other_slot[2] || slot[3] != other_slot[3]) continue;
							slot[0] += other_slot[0];
							slots[j] = null;
						}
					}
					slots.compact!;
					slots.sort! { |a, b| (a[0] == b[0]) ? a[1].ToString() <=> b[1].ToString() : b[0] <=> a[0] };
				}
				GameData.Encounter.register(encounter_hash);
			}
			process_pbs_file_message_end;
		}
		// Save all data
		GameData.Encounter.save;
	}

	//-----------------------------------------------------------------------------
	// Compile trainer type data.
	//-----------------------------------------------------------------------------
	public void compile_trainer_types(*paths) {
		compile_PBS_file_generic(GameData.TrainerType, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_trainer_types : validate_compiled_trainer_type(hash)
		}
	}

	public void validate_compiled_trainer_type(hash) {
		// Ensure valid Poké Ball
		if (hash.poke_ball) {
			if (!GameData.Item.get(hash.poke_ball).is_poke_ball()) {
				Debug.LogError(_INTL("Value '{1}' isn't a defined Poké Ball.", hash.poke_ball) + "\n" + FileLineData.linereport);
				//throw new Exception(_INTL("Value '{1}' isn't a defined Poké Ball.", hash.poke_ball) + "\n" + FileLineData.linereport);
			}
		}
	}

	public void validate_all_compiled_trainer_types() {
		// Get trainer type names for translating
		trainer_type_names = new List<string>();
		foreach (var tr_type in GameData.TrainerType) { //'GameData.TrainerType.each' do => |tr_type|
			trainer_type_names.Add(tr_type.real_name);
		}
		MessageTypes.setMessagesAsHash(MessageTypes.TRAINER_TYPE_NAMES, trainer_type_names);
	}

	//-----------------------------------------------------------------------------
	// Compile individual trainer data.
	//-----------------------------------------------------------------------------
	public void compile_trainers(*paths) {
		GameData.Trainer.DATA.clear;
		schema = GameData.Trainer.schema;
		sub_schema = GameData.Trainer.sub_schema;
		idx = 0;
		// Read from PBS file(s)
		foreach (var path in paths) { //'paths.each' do => |path|
			compile_pbs_file_message_start(path);
			file_suffix = File.basename(path, ".txt")new {GameData.Trainer.PBS_BASE_FILENAME.length + 1, path.length} || "";
			data_hash = null;
			current_pkmn = null;
			section_name = null;
			section_line = null;
			// Read each line of trainers.txt at a time and compile it as a trainer property
			CompilerEachPreppedLine(path) do |line, line_no|
				if (idx % 100 == 0) echo ".";
				idx += 1;
				if (idx % 500 == 0) Graphics.update;
				FileLineData.setSection(section_name, null, section_line);
				if (System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*\[\s*(.+)\s*\]\s*$")) {
					// New section new {trainer_type, name} or new {trainer_type, name, version}
					section_name = $~[1];
					section_line = line;
					if (data_hash) {
						validate_compiled_trainer(data_hash);
						GameData.Trainer.register(data_hash);
					}
					FileLineData.setSection(section_name, null, section_line);
					// Construct data hash
					data_hash = {
						s_file_suffix = file_suffix;
					}
					data_hash[schema["SectionName"][0]] = get_csv_record(section_name.clone, schema["SectionName"]);
					data_hash[schema["Pokemon"][0]] = new List<string>();
					current_pkmn = null;
				} else if (System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*(\w+)\s*=\s*(.*)$")) {
					// XXX=YYY lines
					if (!data_hash) {
						Debug.LogError(_INTL("Expected a section at the beginning of the file.") + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Expected a section at the beginning of the file.") + "\n" + FileLineData.linereport);
					}
					key = $~[1];
					if (schema[key]) {   // Property of the trainer
						property_value = get_csv_record($~[2], schema[key]);
						if (key == "Pokemon") {
							current_pkmn = {
								species = property_value[0],
								level   = property_value[1];
							}
							data_hash[schema[key][0]].Add(current_pkmn);
						} else {
							data_hash[schema[key][0]] = property_value;
						}
					} else if (sub_schema[key]) {   // Property of a Pokémon
						if (!current_pkmn) {
							Debug.LogError(_INTL("Property \"{1}\" is Pokémon-specific, but a Pokémon hasn't been defined yet.", key) + "\n" + FileLineData.linereport);
							//throw new Exception(_INTL("Property \"{1}\" is Pokémon-specific, but a Pokémon hasn't been defined yet.", key) + "\n" + FileLineData.linereport);
						}
						current_pkmn[sub_schema[key][0]] = get_csv_record($~[2], sub_schema[key]);
					}
				}
			}
			// Add last trainer's data to records
			if (data_hash) {
				FileLineData.setSection(section_name, null, section_line);
				validate_compiled_trainer(data_hash);
				GameData.Trainer.register(data_hash);
			}
			process_pbs_file_message_end;
		}
		validate_all_compiled_trainers;
		// Save all data
		GameData.Trainer.save;
	}

	public void validate_compiled_trainer(hash) {
		// Split trainer type, name and version into their own values, generate compound ID from them
		hash.id[2] ||= 0;
		hash.trainer_type = hash.id[0];
		hash.real_name = hash.id[1];
		hash.version = hash.id[2];
		// Ensure the trainer has at least one Pokémon
		if (hash.pokemon.empty()) {
			Debug.LogError(_INTL("Trainer with ID '{1}' has no Pokémon.", hash.id) + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("Trainer with ID '{1}' has no Pokémon.", hash.id) + "\n" + FileLineData.linereport);
		}
		max_level = GameData.GrowthRate.max_level;
		foreach (var pkmn in hash.pokemon) { //'hash.pokemon.each' do => |pkmn|
			// Ensure valid level
			if (pkmn.level > max_level) {
				Debug.LogError(_INTL("Invalid Pokémon level {1} (must be 1-{2}).", pkmn.level, max_level) + "\n" + FileLineData.linereport);
				//throw new Exception(_INTL("Invalid Pokémon level {1} (must be 1-{2}).", pkmn.level, max_level) + "\n" + FileLineData.linereport);
			}
			// Ensure valid name length
			if (pkmn.real_name && pkmn.real_name.length > Pokemon.MAX_NAME_SIZE) {
				Debug.LogError(_INTL("Invalid Pokémon nickname: {1} (must be 1-{2} characters).",);
				//throw new Exception(_INTL("Invalid Pokémon nickname: {1} (must be 1-{2} characters).",);
										pkmn.real_name, Pokemon.MAX_NAME_SIZE) + "\n" + FileLineData.linereport;
			}
			// Ensure no duplicate moves
			if (pkmn.moves) pkmn.moves.uniq!;
			// Ensure valid IVs, convert IVs to hash format
			if (pkmn.iv) {
				iv_hash = new List<string>();
				foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
					if (s.s_order < 0) continue;
					iv_hash[s.id] = pkmn.iv[s.s_order] || pkmn.iv[0];
					if (iv_hash[s.id] > Pokemon.IV_STAT_LIMIT) {
						Debug.LogError(_INTL("Invalid IV: {1} (must be 0-{2}).", iv_hash[s.id], Pokemon.IV_STAT_LIMIT) + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Invalid IV: {1} (must be 0-{2}).", iv_hash[s.id], Pokemon.IV_STAT_LIMIT) + "\n" + FileLineData.linereport);
					}
				}
				pkmn.iv = iv_hash;
			}
			// Ensure valid EVs, convert EVs to hash format
			if (pkmn.ev) {
				ev_hash = new List<string>();
				ev_total = 0;
				foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
					if (s.s_order < 0) continue;
					ev_hash[s.id] = pkmn.ev[s.s_order] || pkmn.ev[0];
					ev_total += ev_hash[s.id];
					if (ev_hash[s.id] > Pokemon.EV_STAT_LIMIT) {
						Debug.LogError(_INTL("Invalid EV: {1} (must be 0-{2}).", ev_hash[s.id], Pokemon.EV_STAT_LIMIT) + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Invalid EV: {1} (must be 0-{2}).", ev_hash[s.id], Pokemon.EV_STAT_LIMIT) + "\n" + FileLineData.linereport);
					}
				}
				pkmn.ev = ev_hash;
				if (ev_total > Pokemon.EV_LIMIT) {
					Debug.LogError(_INTL("Invalid EV set (must sum to {1} or less).", Pokemon.EV_LIMIT) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Invalid EV set (must sum to {1} or less).", Pokemon.EV_LIMIT) + "\n" + FileLineData.linereport);
				}
			}
			// Ensure valid happiness
			if (pkmn.happiness) {
				if (pkmn.happiness > 255) {
					Debug.LogError(_INTL("Bad happiness: {1} (must be 0-255).", pkmn.happiness) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Bad happiness: {1} (must be 0-255).", pkmn.happiness) + "\n" + FileLineData.linereport);
				}
			}
			// Ensure valid Poké Ball
			if (pkmn.poke_ball) {
				if (!GameData.Item.get(pkmn.poke_ball).is_poke_ball()) {
					Debug.LogError(_INTL("Value '{1}' isn't a defined Poké Ball.", pkmn.poke_ball) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Value '{1}' isn't a defined Poké Ball.", pkmn.poke_ball) + "\n" + FileLineData.linereport);
				}
			}
		}
	}

	public void validate_all_compiled_trainers() {
		// Get trainer names and lose texts for translating
		trainer_names = new List<string>();
		lose_texts = new List<string>();
		pokemon_nicknames = new List<string>();
		foreach (var trainer in GameData.Trainer) { //'GameData.Trainer.each' do => |trainer|
			trainer_names.Add(trainer.real_name);
			lose_texts.Add(trainer.real_lose_text);
			foreach (var pkmn in trainer.pokemon) { //'trainer.pokemon.each' do => |pkmn|
				if (!nil_or_empty(pkmn.real_name)) pokemon_nicknames.Add(pkmn.real_name);
			}
		}
		MessageTypes.setMessagesAsHash(MessageTypes.TRAINER_NAMES, trainer_names);
		MessageTypes.setMessagesAsHash(MessageTypes.TRAINER_SPEECHES_LOSE, lose_texts);
		MessageTypes.setMessagesAsHash(MessageTypes.POKEMON_NICKNAMES, pokemon_nicknames);
	}

	//-----------------------------------------------------------------------------
	// Compile Battle Tower and other Cups trainers/Pokémon.
	//-----------------------------------------------------------------------------
	public void compile_trainer_lists(path = "PBS/battle_facility_lists.txt") {
		compile_pbs_file_message_start(path);
		btTrainersRequiredTypes = {
			"Trainers"   => new {0, "s"},
			"Pokemon"    => new {1, "s"},
			"Challenges" => new {2, "*s"}
		}
		if (!FileTest.exist(path)) {
			File.open(path, "wb") do |f|
				f.write(0xEF.chr);
				f.write(0xBB.chr);
				f.write(0xBF.chr);
				f.write("[DefaultTrainerList]\r\n");
				f.write("Trainers = battle_tower_trainers.txt\r\n");
				f.write("Pokemon = battle_tower_pokemon.txt\r\n");
			}
		}
		sections = new List<string>();
		MessageTypes.setMessagesAsHash(MessageTypes.FRONTIER_INTRO_SPEECHES, new List<string>());
		MessageTypes.setMessagesAsHash(MessageTypes.FRONTIER_END_SPEECHES_WIN, new List<string>());
		MessageTypes.setMessagesAsHash(MessageTypes.FRONTIER_END_SPEECHES_LOSE, new List<string>());
		File.open(path, "rb") do |f|
			FileLineData.file = path;
			idx = 0;
			EachFileSection(f) do |section, name|
				echo ".";
				idx += 1;
				Graphics.update;
				if (name != "DefaultTrainerList" && name != "TrainerList") continue;
				rsection = new List<string>();
				foreach (var key in section) { //section.each_key do => |key|
					FileLineData.setSection(name, key, section[key]);
					schema = btTrainersRequiredTypes[key];
					if (key == "Challenges" && name == "DefaultTrainerList") continue;
					if (!schema) continue;
					record = get_csv_record(section[key], schema);
					rsection[schema[0]] = record;
				}
				if (!rsection[0]) {
					Debug.LogError(_INTL("No trainer data file given in section {1}.", name) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("No trainer data file given in section {1}.", name) + "\n" + FileLineData.linereport);
				}
				if (!rsection[1]) {
					Debug.LogError(_INTL("No trainer data file given in section {1}.", name) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("No trainer data file given in section {1}.", name) + "\n" + FileLineData.linereport);
				}
				rsection[3] = rsection[0];
				rsection[4] = rsection[1];
				rsection[5] = (name == "DefaultTrainerList");
				if (FileTest.exist("PBS/" + rsection[0])) {
					rsection[0] = compile_battle_tower_trainers("PBS/" + rsection[0]);
				} else {
					rsection[0] = new List<string>();
				}
				if (FileTest.exist("PBS/" + rsection[1])) {
					filename = "PBS/" + rsection[1];
					rsection[1] = new List<string>();
					CompilerEachCommentedLine(filename) do |line, _lineno|
						rsection[1].Add(Pokemon.fromInspected(line));
					}
				} else {
					rsection[1] = new List<string>();
				}
				if (!rsection[2]) rsection[2] = new List<string>();
				while (rsection[2].Contains("")) {
					rsection[2].delete("");
				}
				rsection[2].compact!;
				sections.Add(rsection);
			}
		}
		save_data(sections, "Data/trainer_lists.dat");
		process_pbs_file_message_end;
	}

	public void compile_battle_tower_trainers(filename) {
		sections = new List<string>();
		requiredtypes = {
			"Type"          => new {0, "e", :TrainerType},
			"Name"          => new {1, "s"},
			"BeginSpeech"   => new {2, "s"},
			"EndSpeechWin"  => new {3, "s"},
			"EndSpeechLose" => new {4, "s"},
			"PokemonNos"    => new {5, "*u"}
		}
		trainernames  = new List<string>();
		beginspeech   = new List<string>();
		endspeechwin  = new List<string>();
		endspeechlose = new List<string>();
		if (FileTest.exist(filename)) {
			File.open(filename, "rb") do |f|
				FileLineData.file = filename;
				EachFileSection(f) do |section, name|
					rsection = new List<string>();
					foreach (var key in section) { //section.each_key do => |key|
						FileLineData.setSection(name, key, section[key]);
						schema = requiredtypes[key];
						if (!schema) continue;
						record = get_csv_record(section[key], schema);
						rsection[schema[0]] = record;
					}
					trainernames.Add(rsection[1]);
					beginspeech.Add(rsection[2]);
					endspeechwin.Add(rsection[3]);
					endspeechlose.Add(rsection[4]);
					sections.Add(rsection);
				}
			}
		}
		MessageTypes.addMessagesAsHash(MessageTypes.TRAINER_NAMES, trainernames);
		MessageTypes.addMessagesAsHash(MessageTypes.FRONTIER_INTRO_SPEECHES, beginspeech);
		MessageTypes.addMessagesAsHash(MessageTypes.FRONTIER_END_SPEECHES_WIN, endspeechwin);
		MessageTypes.addMessagesAsHash(MessageTypes.FRONTIER_END_SPEECHES_LOSE, endspeechlose);
		return sections;
	}

	//-----------------------------------------------------------------------------
	// Compile metadata.
	// NOTE: Doesn't use compile_PBS_file_generic because it contains data for two
	//       different GameData classes.
	//-----------------------------------------------------------------------------
	public void compile_metadata(*paths) {
		GameData.Metadata.DATA.clear;
		GameData.PlayerMetadata.DATA.clear;
		global_schema = GameData.Metadata.schema;
		player_schema = GameData.PlayerMetadata.schema;
		foreach (var path in paths) { //'paths.each' do => |path|
			compile_pbs_file_message_start(path);
			file_suffix = File.basename(path, ".txt")[GameData.Metadata.PBS_BASE_FILENAME.length + 1, path.length] || "";
			// Read from PBS file
			File.open(path, "rb") do |f|
				FileLineData.file = path;   // For error reporting
				// Read a whole section's lines at once, then run through this code.
				// contents is a hash containing all the XXX=YYY lines in that section, where
				// the keys are the XXX and the values are the YYY (as unprocessed strings).
				idx = 0;
				EachFileSection(f) do |contents, section_name|
					if (idx % 100 == 0) echo ".";
					if (idx % 500 == 0) Graphics.update;
					idx += 1;
					schema = (section_name.ToInt() == 0) ? global_schema : player_schema;
					data_hash = {
						id              = section_name.to_sym,
						s_file_suffix = file_suffix;
					}
					// Go through schema hash of compilable data and compile this section
					foreach (var key in schema) { //schema.each_key do => |key|
						FileLineData.setSection(section_name, key, contents[key]);   // For error reporting
						if (key == "SectionName") {
							data_hash[schema[key][0]] = get_csv_record(section_name, schema[key]);
							continue;
						}
						// Skip empty properties
						if (contents[key].null()) continue;
						// Compile value for key
						if (schema[key][1][0] == "^") {
							foreach (var val in contents[key]) { //'contents[key].each' do => |val|
								value = get_csv_record(val, schema[key]);
								if (value.Length > 0 && value.empty()) value = null;
								data_hash[schema[key][0]] ||= new List<string>();
								data_hash[schema[key][0]].Add(value);
							}
							data_hash[schema[key][0]].compact!;
						} else {
							value = get_csv_record(contents[key], schema[key]);
							if (value.Length > 0 && value.empty()) value = null;
							data_hash[schema[key][0]] = value;
						}
					}
					// Validate and modify the compiled data
					if (data_hash.id == 0) {
						validate_compiled_global_metadata(data_hash);
						if (GameData.Metadata.exists(data_hash.id)) {
							Debug.LogError(_INTL("Global metadata ID '{1}' is used twice.", data_hash.id) + "\n" + FileLineData.linereport);
							//throw new Exception(_INTL("Global metadata ID '{1}' is used twice.", data_hash.id) + "\n" + FileLineData.linereport);
						}
					} else {
						validate_compiled_player_metadata(data_hash);
						if (GameData.PlayerMetadata.exists(data_hash.id)) {
							Debug.LogError(_INTL("Player metadata ID '{1}' is used twice.", data_hash.id) + "\n" + FileLineData.linereport);
							//throw new Exception(_INTL("Player metadata ID '{1}' is used twice.", data_hash.id) + "\n" + FileLineData.linereport);
						}
					}
					// Add section's data to records
					if (data_hash.id == 0) {
						GameData.Metadata.register(data_hash);
					} else {
						GameData.PlayerMetadata.register(data_hash);
					}
				}
			}
			process_pbs_file_message_end;
		}
		validate_all_compiled_metadata;
		// Save all data
		GameData.Metadata.save;
		GameData.PlayerMetadata.save;
	}

	public void validate_compiled_global_metadata(hash) {
		if (hash.home.null()) {
			Debug.LogError(_INTL("The entry 'Home' is required in metadata.txt section 0.") + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("The entry 'Home' is required in metadata.txt section 0.") + "\n" + FileLineData.linereport);
		}
	}

	public void validate_compiled_player_metadata(hash) {
	}

	// Should be used to check both global metadata and player character metadata.
	public void validate_all_compiled_metadata() {
		// Ensure global metadata is defined
		if (!GameData.Metadata.exists(0)) {
			Debug.LogError(_INTL("Global metadata is not defined in metadata.txt but should be.") + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("Global metadata is not defined in metadata.txt but should be.") + "\n" + FileLineData.linereport);
		}
		// Ensure player character 1's metadata is defined
		if (!GameData.PlayerMetadata.exists(1)) {
			Debug.LogError(_INTL("Metadata for player character 1 is not defined in metadata.txt but should be.") + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("Metadata for player character 1 is not defined in metadata.txt but should be.") + "\n" + FileLineData.linereport);
		}
		// Get storage creator's name for translating
		storage_creator = [GameData.Metadata.get.real_storage_creator];
		MessageTypes.setMessagesAsHash(MessageTypes.STORAGE_CREATOR_NAME, storage_creator);
	}

	//-----------------------------------------------------------------------------
	// Compile map metadata.
	//-----------------------------------------------------------------------------
	public void compile_map_metadata(*paths) {
		compile_PBS_file_generic(GameData.MapMetadata, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_map_metadata : validate_compiled_map_metadata(hash)
		}
	}

	public void validate_compiled_map_metadata(hash) {
		// Give the map its RMXP map name if it doesn't define its own
		if (nil_or_empty(hash.real_name)) {
			hash.real_name = LoadMapInfos[hash.id].name;
		}
	}

	public void validate_all_compiled_map_metadata() {
		// Get map names for translating
		map_names = new List<string>();
		GameData.MapMetadata.each(map => map_names[map.id] = map.real_name);
		MessageTypes.setMessagesAsHash(MessageTypes.MAP_NAMES, map_names);
	}

	//-----------------------------------------------------------------------------
	// Compile dungeon tileset data.
	//-----------------------------------------------------------------------------
	public void compile_dungeon_tilesets(*paths) {
		compile_PBS_file_generic(GameData.DungeonTileset, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_dungeon_tilesets : validate_compiled_dungeon_tileset(hash)
		}
	}

	public void validate_compiled_dungeon_tileset(hash) {
	}

	public void validate_all_compiled_dungeon_tilesets() {
	}

	//-----------------------------------------------------------------------------
	// Compile dungeon parameters data.
	//-----------------------------------------------------------------------------
	public void compile_dungeon_parameters(*paths) {
		compile_PBS_file_generic(GameData.DungeonParameters, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_dungeon_parameters : validate_compiled_dungeon_parameters(hash)
		}
	}

	public void validate_compiled_dungeon_parameters(hash) {
		// Split area and version into their own values, generate compound ID from them
		hash.area = hash.id[0];
		hash.version = hash.id[1] || 0;
		if (hash.version == 0) {
			hash.id = hash.area;
		} else {
			hash.id = string.Format("{0}_{0}", hash.area.ToString(), hash.version).to_sym;
		}
		if (GameData.DungeonParameters.exists(hash.id)) {
			Debug.LogError(_INTL("Version {1} of dungeon area {2} is defined twice.", hash.version, hash.area) + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("Version {1} of dungeon area {2} is defined twice.", hash.version, hash.area) + "\n" + FileLineData.linereport);
		}
	}

	public void validate_all_compiled_dungeon_parameters() {
	}

	//-----------------------------------------------------------------------------
	// Compile phone messages.
	//-----------------------------------------------------------------------------
	public void compile_phone(*paths) {
		compile_PBS_file_generic(GameData.PhoneMessage, *paths) do |final_validate, hash|
			(final_validate) ? validate_all_compiled_phone_contacts : validate_compiled_phone_contact(hash)
		}
	}

	public void validate_compiled_phone_contact(hash) {
		// Split trainer type/name/version into their own values, generate compound ID from them
		if (hash.id.strip.downcase == "default") {
			hash.id = "default";
			hash.trainer_type = hash.id;
		} else {
			line_data = get_csv_record(hash.id, new {null, "esU", :TrainerType});
			hash.trainer_type = line_data[0];
			hash.real_name = line_data[1];
			hash.version = line_data[2] || 0;
			hash.id = new {hash.trainer_type, hash.real_name, hash.version};
		}
	}

	public void validate_all_compiled_phone_contacts() {
		// Get all phone messages for translating
		messages = new List<string>();
		foreach (var contact in GameData.PhoneMessage) { //'GameData.PhoneMessage.each' do => |contact|
			new {:intro, :intro_morning, :intro_afternoon, :intro_evening, :body, :body1,
			:body2, :battle_request, :battle_remind, :end}.each do |msg_type|
				msgs = contact.send(msg_type);
				if (!msgs || msgs.length == 0) continue;
				msgs.each(msg => messages.Add(msg));
			}
		}
		MessageTypes.setMessagesAsHash(MessageTypes.PHONE_MESSAGES, messages);
	}
}
