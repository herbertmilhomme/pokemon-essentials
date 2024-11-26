//===============================================================================
//
//===============================================================================
public static partial class Compiler {
	#region Class Functions
	#endregion

	public void write_all_pbs_files() {
		Console.echo_h1(_INTL("Writing all PBS files"));
		write_town_map;
		write_connections;
		write_types;
		write_abilities;
		write_moves;
		write_items;
		write_berry_plants;
		write_pokemon;
		write_pokemon_forms;
		write_pokemon_metrics;
		write_shadow_pokemon;
		write_regional_dexes;
		write_ribbons;
		write_encounters;
		write_trainer_types;
		write_trainers;
		write_trainer_lists;
		write_metadata;
		write_map_metadata;
		write_dungeon_tilesets;
		write_dungeon_parameters;
		write_phone;
		echoln "";
		Console.echo_h2(_INTL("Successfully rewrote all PBS files"), text: :green);
	}

	//-----------------------------------------------------------------------------
	// Generic methods used when writing PBS files.
	//-----------------------------------------------------------------------------
	public void write_pbs_file_message_start(filename) {
		// The `` around the file's name turns it cyan
		Console.echo_li(_INTL("Writing PBS file `{1}`...", filename.split("/").last));
	}

	public void get_all_PBS_file_paths(game_data) {
		ret = new List<string>();
		game_data.each(element => { if (!ret.Contains(element.s_file_suffix)) ret.Add(element.s_file_suffix); });
		ret.each_with_index do |element, i|
			ret[i] = new {string.Format("PBS/{0}.txt", game_data.PBS_BASE_FILENAME), element};
			if (!nil_or_empty(element)) {
				ret[i][0] = string.Format("PBS/{0}_{0}.txt", game_data.PBS_BASE_FILENAME, element);
			}
		}
		return ret;
	}

	public void add_PBS_header_to_file(file) {
		file.write(0xEF.chr);
		file.write(0xBB.chr);
		file.write(0xBF.chr);
		file.write("\# " + _INTL("See the documentation on the wiki to learn how to edit this file.") + "\r\n");
	}

	public void write_PBS_file_generic(game_data) {
		paths = get_all_PBS_file_paths(game_data);
		schema = game_data.schema;
		idx = 0;
		foreach (var path in paths) { //'paths.each' do => |path|
			write_pbs_file_message_start(path[0]);
			File.open(path[0], "wb") do |f|
				add_PBS_header_to_file(f);
				// Write each element in turn
				foreach (var element in game_data) { //'game_data.each' do => |element|
					if (element.s_file_suffix != path[1]) continue;
					if (idx % 100 == 0) echo ".";
					if (idx % 500 == 0) Graphics.update;
					idx += 1;
					f.write("\#-------------------------------\r\n");
					if (schema["SectionName"]) {
						f.write("[");
						WriteCsvRecord(element.get_property_for_PBS("SectionName"), f, schema["SectionName"]);
						f.write("]\r\n");
					} else {
						f.write($"[{element.id}]\r\n");
					}
					foreach (var key in schema) { //schema.each_key do => |key|
						if (key == "SectionName") continue;
						val = element.get_property_for_PBS(key);
						if (val.null()) continue;
						if (schema[key][1][0] == "^" && val.Length > 0) {
							foreach (var sub_val in val) { //'val.each' do => |sub_val|
								f.write(string.Format("{0} = ", key));
								WriteCsvRecord(sub_val, f, schema[key]);
								f.write("\r\n");
							}
						} else {
							f.write(string.Format("{0} = ", key));
							WriteCsvRecord(val, f, schema[key]);
							f.write("\r\n");
						}
					}
				}
			}
			process_pbs_file_message_end;
		}
	}

	//-----------------------------------------------------------------------------
	// Save Town Map data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_town_map() {
		write_PBS_file_generic(GameData.TownMap);
	}

	//-----------------------------------------------------------------------------
	// Save map connections to PBS file.
	//-----------------------------------------------------------------------------
	public void normalize_connection(conn) {
		ret = conn.clone;
		if (conn[1].negative() != conn[4].negative()) {   // Exactly one is negative
			ret[4] = -conn[1];
			ret[1] = -conn[4];
		}
		if (conn[2].negative() != conn[5].negative()) {   // Exactly one is negative
			ret[5] = -conn[2];
			ret[2] = -conn[5];
		}
		return ret;
	}

	public void get_connection_text(map1, x1, y1, map2, x2, y2) {
		dims1 = MapFactoryHelper.getMapDims(map1);
		dims2 = MapFactoryHelper.getMapDims(map2);
		if (x1 == 0 && x2 == dims2[0]) {
			return string.Format("{0},West,{0},{0},East,{0}", map1, y1, map2, y2);
		} else if (y1 == 0 && y2 == dims2[1]) {
			return string.Format("{0},North,{0},{0},South,{0}", map1, x1, map2, x2);
		} else if (x1 == dims1[0] && x2 == 0) {
			return string.Format("{0},East,{0},{0},West,{0}", map1, y1, map2, y2);
		} else if (y1 == dims1[1] && y2 == 0) {
			return string.Format("{0},South,{0},{0},North,{0}", map1, x1, map2, x2);
		}
		return string.Format("{0},{0},{0},{0},{0},{0}", map1, x1, y1, map2, x2, y2);
	}

	public void write_connections(path = "PBS/map_connections.txt") {
		conndata = load_data("Data/map_connections.dat");
		if (!conndata) return;
		write_pbs_file_message_start(path);
		mapinfos = LoadMapInfos;
		File.open(path, "wb") do |f|
			add_PBS_header_to_file(f);
			f.write("\#-------------------------------\r\n");
			foreach (var conn in conndata) { //'conndata.each' do => |conn|
				if (mapinfos) {
					// Skip if map no longer exists
					if (!mapinfos[conn[0]] || !mapinfos[conn[3]]) continue;
					f.write(string.Format("# {0} ({0}) - %s ({0})\r\n",
													(mapinfos[conn[0]]) ? mapinfos[conn[0]].name : "???", conn[0],
													(mapinfos[conn[3]]) ? mapinfos[conn[3]].name : "???", conn[3]))
				}
				if (conn[1].is_a(String) || conn[4].is_a(String)) {
					f.write(string.Format("{0},{0},{0},{0},{0},{0}", conn[0], conn[1], conn[2],
													conn[3], conn[4], conn[5]));
				} else {
					ret = normalize_connection(conn);
					f.write(get_connection_text(ret[0], ret[1], ret[2], ret[3], ret[4], ret[5]));
				}
				f.write("\r\n");
			}
		}
		process_pbs_file_message_end;
	}

	//-----------------------------------------------------------------------------
	// Save type data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_types() {
		write_PBS_file_generic(GameData.Type);
	}

	//-----------------------------------------------------------------------------
	// Save ability data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_abilities() {
		write_PBS_file_generic(GameData.Ability);
	}

	//-----------------------------------------------------------------------------
	// Save move data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_moves() {
		write_PBS_file_generic(GameData.Move);
	}

	//-----------------------------------------------------------------------------
	// Save item data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_items() {
		write_PBS_file_generic(GameData.Item);
	}

	//-----------------------------------------------------------------------------
	// Save berry plant data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_berry_plants() {
		write_PBS_file_generic(GameData.BerryPlant);
	}

	//-----------------------------------------------------------------------------
	// Save Pokémon data to PBS file.
	// NOTE: Doesn't use write_PBS_file_generic because it needs to ignore defined
	//       species with a form that isn't 0.
	//-----------------------------------------------------------------------------
	public void write_pokemon() {
		paths = new List<string>();
		GameData.Species.each_species(element => { if (!paths.Contains(element.s_file_suffix)) paths.Add(element.s_file_suffix); });
		paths.each_with_index do |element, i|
			paths[i] = new {string.Format("PBS/{0}.txt", GameData.Species.PBS_BASE_FILENAME[0]), element};
			if (!nil_or_empty(element)) {
				paths[i][0] = string.Format("PBS/{0}_{0}.txt", GameData.Species.PBS_BASE_FILENAME[0], element);
			}
		}
		schema = GameData.Species.schema;
		idx = 0;
		foreach (var path in paths) { //'paths.each' do => |path|
			write_pbs_file_message_start(path[0]);
			File.open(path[0], "wb") do |f|
				add_PBS_header_to_file(f);
				// Write each element in turn
				foreach (var element in GameData.Species) { //GameData.Species.each_species do => |element|
					if (element.s_file_suffix != path[1]) continue;
					if (idx % 100 == 0) echo ".";
					if (idx % 500 == 0) Graphics.update;
					idx += 1;
					f.write("\#-------------------------------\r\n");
					if (schema["SectionName"]) {
						f.write("[");
						WriteCsvRecord(element.get_property_for_PBS("SectionName"), f, schema["SectionName"]);
						f.write("]\r\n");
					} else {
						f.write($"[{element.id}]\r\n");
					}
					foreach (var key in schema) { //schema.each_key do => |key|
						if (key == "SectionName") continue;
						val = element.get_property_for_PBS(key);
						if (val.null()) continue;
						if (schema[key][1][0] == "^" && val.Length > 0) {
							foreach (var sub_val in val) { //'val.each' do => |sub_val|
								f.write(string.Format("{0} = ", key));
								WriteCsvRecord(sub_val, f, schema[key]);
								f.write("\r\n");
							}
						} else {
							f.write(string.Format("{0} = ", key));
							WriteCsvRecord(val, f, schema[key]);
							f.write("\r\n");
						}
					}
				}
			}
			process_pbs_file_message_end;
		}
	}

	//-----------------------------------------------------------------------------
	// Save Pokémon forms data to PBS file.
	// NOTE: Doesn't use write_PBS_file_generic because it needs to ignore defined
	//       species with a form of 0, and needs its own schema.
	//-----------------------------------------------------------------------------
	public void write_pokemon_forms() {
		paths = new List<string>();
		foreach (var element in GameData.Species) { //'GameData.Species.each' do => |element|
			if (element.form == 0) continue;
			if (!paths.Contains(element.s_file_suffix)) paths.Add(element.s_file_suffix);
		}
		paths.each_with_index do |element, i|
			paths[i] = new {string.Format("PBS/{0}.txt", GameData.Species.PBS_BASE_FILENAME[1]), element};
			if (!nil_or_empty(element)) {
				paths[i][0] = string.Format("PBS/{0}_{0}.txt", GameData.Species.PBS_BASE_FILENAME[1], element);
			}
		}
		schema = GameData.Species.schema(true);
		idx = 0;
		foreach (var path in paths) { //'paths.each' do => |path|
			write_pbs_file_message_start(path[0]);
			File.open(path[0], "wb") do |f|
				add_PBS_header_to_file(f);
				// Write each element in turn
				foreach (var element in GameData.Species) { //'GameData.Species.each' do => |element|
					if (element.form == 0) continue;
					if (element.s_file_suffix != path[1]) continue;
					if (idx % 100 == 0) echo ".";
					if (idx % 500 == 0) Graphics.update;
					idx += 1;
					f.write("\#-------------------------------\r\n");
					if (schema["SectionName"]) {
						f.write("[");
						WriteCsvRecord(element.get_property_for_PBS("SectionName", true), f, schema["SectionName"]);
						f.write("]\r\n");
					} else {
						f.write($"[{element.id}]\r\n");
					}
					foreach (var key in schema) { //schema.each_key do => |key|
						if (key == "SectionName") continue;
						val = element.get_property_for_PBS(key, true);
						if (val.null()) continue;
						if (schema[key][1][0] == "^" && val.Length > 0) {
							foreach (var sub_val in val) { //'val.each' do => |sub_val|
								f.write(string.Format("{0} = ", key));
								WriteCsvRecord(sub_val, f, schema[key]);
								f.write("\r\n");
							}
						} else {
							f.write(string.Format("{0} = ", key));
							WriteCsvRecord(val, f, schema[key]);
							f.write("\r\n");
						}
					}
				}
			}
			process_pbs_file_message_end;
		}
	}

	//-----------------------------------------------------------------------------
	// Write species metrics.
	// NOTE: Doesn't use write_PBS_file_generic because it needs to ignore defined
	//       metrics for forms of species where the metrics are the same as for the
	//       base species.
	//-----------------------------------------------------------------------------
	public void write_pokemon_metrics() {
		paths = new List<string>();
		foreach (var element in GameData.SpeciesMetrics) { //'GameData.SpeciesMetrics.each' do => |element|
			if (element.form == 0) continue;
			if (!paths.Contains(element.s_file_suffix)) paths.Add(element.s_file_suffix);
		}
		paths.each_with_index do |element, i|
			paths[i] = new {string.Format("PBS/{0}.txt", GameData.SpeciesMetrics.PBS_BASE_FILENAME), element};
			if (!nil_or_empty(element)) {
				paths[i][0] = string.Format("PBS/{0}_{0}.txt", GameData.SpeciesMetrics.PBS_BASE_FILENAME, element);
			}
		}
		schema = GameData.SpeciesMetrics.schema;
		idx = 0;
		foreach (var path in paths) { //'paths.each' do => |path|
			write_pbs_file_message_start(path[0]);
			File.open(path[0], "wb") do |f|
				add_PBS_header_to_file(f);
				// Write each element in turn
				foreach (var element in GameData.SpeciesMetrics) { //'GameData.SpeciesMetrics.each' do => |element|
					if (element.s_file_suffix != path[1]) continue;
					if (element.form > 0) {
						base_element = GameData.SpeciesMetrics.get(element.species);
						if (element.back_sprite == base_element.back_sprite &&
										element.front_sprite == base_element.front_sprite &&
										element.front_sprite_altitude == base_element.front_sprite_altitude &&
										element.shadow_x == base_element.shadow_x &&
										element.shadow_size == base_element.shadow_size) continue;
					}
					if (idx % 100 == 0) echo ".";
					if (idx % 500 == 0) Graphics.update;
					idx += 1;
					f.write("\#-------------------------------\r\n");
					if (schema["SectionName"]) {
						f.write("[");
						WriteCsvRecord(element.get_property_for_PBS("SectionName"), f, schema["SectionName"]);
						f.write("]\r\n");
					} else {
						f.write($"[{element.id}]\r\n");
					}
					foreach (var key in schema) { //schema.each_key do => |key|
						if (key == "SectionName") continue;
						val = element.get_property_for_PBS(key);
						if (val.null()) continue;
						if (schema[key][1][0] == "^" && val.Length > 0) {
							foreach (var sub_val in val) { //'val.each' do => |sub_val|
								f.write(string.Format("{0} = ", key));
								WriteCsvRecord(sub_val, f, schema[key]);
								f.write("\r\n");
							}
						} else {
							f.write(string.Format("{0} = ", key));
							WriteCsvRecord(val, f, schema[key]);
							f.write("\r\n");
						}
					}
				}
			}
			process_pbs_file_message_end;
		}
	}

	//-----------------------------------------------------------------------------
	// Save Shadow Pokémon data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_shadow_pokemon() {
		if (GameData.ShadowPokemon.DATA.empty()) return;
		write_PBS_file_generic(GameData.ShadowPokemon);
	}

	//-----------------------------------------------------------------------------
	// Save Regional Dexes to PBS file.
	//-----------------------------------------------------------------------------
	public void write_regional_dexes(path = "PBS/regional_dexes.txt") {
		write_pbs_file_message_start(path);
		dex_lists = LoadRegionalDexes;
		File.open(path, "wb") do |f|
			add_PBS_header_to_file(f);
			// Write each Dex list in turn
			dex_lists.each_with_index do |list, index|
				f.write("\#-------------------------------\r\n");
				f.write($"[{index}]");
				comma = false;
				current_family = null;
				foreach (var species in list) { //'list.each' do => |species|
					if (!species) continue;
					if (current_family&.Contains(species)) {
						if (comma) f.write(",");
					} else {
						current_family = GameData.Species.get(species).get_family_species;
						comma = false;
						f.write("\r\n");
					}
					f.write(species);
					comma = true;
				}
				f.write("\r\n");
			}
		}
		process_pbs_file_message_end;
	}

	//-----------------------------------------------------------------------------
	// Save ability data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_ribbons() {
		write_PBS_file_generic(GameData.Ribbon);
	}

	//-----------------------------------------------------------------------------
	// Save wild encounter data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_encounters() {
		paths = get_all_PBS_file_paths(GameData.Encounter);
		map_infos = LoadMapInfos;
		idx = 0;
		foreach (var path in paths) { //'paths.each' do => |path|
			write_pbs_file_message_start(path[0]);
			File.open(path[0], "wb") do |f|
				add_PBS_header_to_file(f);
				foreach (var element in GameData.Encounter) { //'GameData.Encounter.each' do => |element|
					if (element.s_file_suffix != path[1]) continue;
					if (idx % 100 == 0) echo ".";
					if (idx % 500 == 0) Graphics.update;
					idx += 1;
					f.write("\#-------------------------------\r\n");
					map_name = (map_infos[element.map]) ? $" # {map_infos[element.map].name}" : "";
					if (element.version > 0) {
						f.write(string.Format("[{0:3},{0}]{0}\r\n", element.map, element.version, map_name));
					} else {
						f.write(string.Format("[{0:3}]{0}\r\n", element.map, map_name));
					}
					element.types.each do |type, slots|
						if (!slots || slots.length == 0) continue;
						if (element.step_chances[type] && element.step_chances[type] > 0) {
							f.write(string.Format("{0},{0}\r\n", type.ToString(), element.step_chances[type]));
						} else {
							f.write(string.Format("{0}\r\n", type.ToString()));
						}
						foreach (var slot in slots) { //'slots.each' do => |slot|
							if (slot[2] == slot[3]) {
								f.write(string.Format("    {0},{0},{0}\r\n", slot[0], slot[1], slot[2]));
							} else {
								f.write(string.Format("    {0},{0},{0},{0}\r\n", slot[0], slot[1], slot[2], slot[3]));
							}
						}
					}
				}
			}
			process_pbs_file_message_end;
		}
	}

	//-----------------------------------------------------------------------------
	// Save trainer type data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_trainer_types() {
		write_PBS_file_generic(GameData.TrainerType);
	}

	//-----------------------------------------------------------------------------
	// Save individual trainer data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_trainers() {
		paths = get_all_PBS_file_paths(GameData.Trainer);
		schema = GameData.Trainer.schema;
		sub_schema = GameData.Trainer.sub_schema;
		idx = 0;
		foreach (var path in paths) { //'paths.each' do => |path|
			write_pbs_file_message_start(path[0]);
			File.open(path[0], "wb") do |f|
				add_PBS_header_to_file(f);
				// Write each element in turn
				foreach (var element in GameData.Trainer) { //'GameData.Trainer.each' do => |element|
					if (element.s_file_suffix != path[1]) continue;
					if (idx % 100 == 0) echo ".";
					if (idx % 500 == 0) Graphics.update;
					idx += 1;
					f.write("\#-------------------------------\r\n");
					if (schema["SectionName"]) {
						f.write("[");
						WriteCsvRecord(element.get_property_for_PBS("SectionName"), f, schema["SectionName"]);
						f.write("]\r\n");
					} else {
						f.write($"[{element.id}]\r\n");
					}
					// Write each trainer property
					foreach (var key in schema) { //schema.each_key do => |key|
						if (new []{"SectionName", "Pokemon"}.Contains(key)) continue;
						val = element.get_property_for_PBS(key);
						if (val.null()) continue;
						f.write(string.Format("{0} = ", key));
						WriteCsvRecord(val, f, schema[key]);
						f.write("\r\n");
					}
					// Write each Pokémon in turn
					element.pokemon.each_with_index do |pkmn, i|
						// Write species/level
						val = element.get_pokemon_property_for_PBS("Pokemon", i);
						f.write("Pokemon = ");
						WriteCsvRecord(val, f, schema["Pokemon"]);
						f.write("\r\n");
						// Write other Pokémon properties
						foreach (var key in sub_schema) { //sub_schema.each_key do => |key|
							val = element.get_pokemon_property_for_PBS(key, i);
							if (val.null()) continue;
							f.write(string.Format("    {0} = ", key));
							WriteCsvRecord(val, f, sub_schema[key]);
							f.write("\r\n");
						}
					}
				}
			}
			process_pbs_file_message_end;
		}
	}

	//-----------------------------------------------------------------------------
	// Save trainer list data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_trainer_lists(path = "PBS/battle_facility_lists.txt") {
		trainerlists = load_data("Data/trainer_lists.dat") rescue null;
		if (!trainerlists) return;
		write_pbs_file_message_start(path);
		File.open(path, "wb") do |f|
			add_PBS_header_to_file(f);
			foreach (var tr in trainerlists) { //'trainerlists.each' do => |tr|
				echo ".";
				f.write("\#-------------------------------\r\n");
				f.write(((tr[5]) ? "[DefaultTrainerList]" : "[TrainerList]") + "\r\n");
				f.write("Trainers = " + tr[3] + "\r\n");
				f.write("Pokemon = " + tr[4] + "\r\n");
				if (!tr[5]) f.write("Challenges = " + tr[2].join(",") + "\r\n");
				write_battle_tower_trainers(tr[0], "PBS/" + tr[3]);
				write_battle_tower_pokemon(tr[1], "PBS/" + tr[4]);
			}
		}
		process_pbs_file_message_end;
	}

	//-----------------------------------------------------------------------------
	// Save Battle Tower trainer data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_battle_tower_trainers(bttrainers, filename) {
		if (!bttrainers || !filename) return;
		btTrainersRequiredTypes = {
			"Type"          => new {0, "e", null},   // Specifies a trainer
			"Name"          => new {1, "s"},
			"BeginSpeech"   => new {2, "s"},
			"EndSpeechWin"  => new {3, "s"},
			"EndSpeechLose" => new {4, "s"},
			"PokemonNos"    => new {5, "*u"}
		}
		File.open(filename, "wb") do |f|
			add_PBS_header_to_file(f);
			for (int i = bttrainers.length; i < bttrainers.length; i++) { //for 'bttrainers.length' times do => |i|
				if (!bttrainers[i]) continue;
				f.write("\#-------------------------------\r\n");
				f.write(string.Format("[{0:3}]\r\n", i));
				foreach (var key in btTrainersRequiredTypes) { //btTrainersRequiredTypes.each_key do => |key|
					schema = btTrainersRequiredTypes[key];
					record = bttrainers[i][schema[0]];
					if (record.null()) continue;
					f.write(string.Format("{0} = ", key));
					switch (key) {
						case "Type":
							f.write(record.ToString());
							break;
						case "PokemonNos":
							f.write(record.join(","));   // WriteCsvRecord somehow won't work here
							break;
						default:
							WriteCsvRecord(record, f, schema);
							break;
					}
					f.write("\r\n");
				}
			}
		}
		Graphics.update;
	}

	//-----------------------------------------------------------------------------
	// Save Battle Tower Pokémon data to PBS file.
	//-----------------------------------------------------------------------------
	public void write_battle_tower_pokemon(btpokemon, filename) {
		if (!btpokemon || !filename) return;
		species = new List<string>();
		moves   = new List<string>();
		items   = new List<string>();
		natures = new List<string>();
		evs = {
			HP              = "HP",
			ATTACK          = "ATK",
			DEFENSE         = "DEF",
			SPECIAL_ATTACK  = "SA",
			SPECIAL_DEFENSE = "SD",
			SPEED           = "SPD";
		}
		File.open(filename, "wb") do |f|
			add_PBS_header_to_file(f);
			f.write("\#-------------------------------\r\n");
			for (int i = btpokemon.length; i < btpokemon.length; i++) { //for 'btpokemon.length' times do => |i|
				if (i % 500 == 0) Graphics.update;
				pkmn = btpokemon[i];
				c1 = (species[pkmn.species]) ? species[pkmn.species] : (species[pkmn.species] = GameData.Species.get(pkmn.species).species.ToString());
				c2 = null;
				if (pkmn.item && GameData.Item.exists(pkmn.item)) {
					c2 = (items[pkmn.item]) ? items[pkmn.item] : (items[pkmn.item] = GameData.Item.get(pkmn.item).id.ToString());
				}
				c3 = (natures[pkmn.nature]) ? natures[pkmn.nature] : (natures[pkmn.nature] = GameData.Nature.get(pkmn.nature).id.ToString());
				evlist = "";
				pkmn.ev.each_with_index do |stat, j|
					if (j > 0) evlist += ",";
					evlist += evs[stat];
				}
				c4 = c5 = c6 = c7 = "";
				new {pkmn.move1, pkmn.move2, pkmn.move3, pkmn.move4}.each_with_index do |move, j|
					if (!move) continue;
					text = (moves[move]) ? moves[move] : (moves[move] = GameData.Move.get(move).id.ToString());
					switch (j) {
						case 0:  c4 = text; break;
						case 1:  c5 = text; break;
						case 2:  c6 = text; break;
						case 3:  c7 = text; break;
					}
				}
				f.write($"{c1};{c2};{c3};{evlist};{c4},{c5},{c6},{c7}\r\n");
			}
		}
		Graphics.update;
	}

	//-----------------------------------------------------------------------------
	// Save metadata data to PBS file.
	// NOTE: Doesn't use write_PBS_file_generic because it contains data for two
	//       different GameData classes.
	//-----------------------------------------------------------------------------
	public void write_metadata() {
		paths = new List<string>();
		foreach (var element in GameData.Metadata) { //'GameData.Metadata.each' do => |element|
			if (!paths.Contains(element.s_file_suffix)) paths.Add(element.s_file_suffix);
		}
		foreach (var element in GameData.PlayerMetadata) { //'GameData.PlayerMetadata.each' do => |element|
			if (!paths.Contains(element.s_file_suffix)) paths.Add(element.s_file_suffix);
		}
		paths.each_with_index do |element, i|
			paths[i] = new {string.Format("PBS/{0}.txt", GameData.Metadata.PBS_BASE_FILENAME), element};
			if (!nil_or_empty(element)) {
				paths[i][0] = string.Format("PBS/{0}_{0}.txt", GameData.Metadata.PBS_BASE_FILENAME, element);
			}
		}
		global_schema = GameData.Metadata.schema;
		player_schema = GameData.PlayerMetadata.schema;
		foreach (var path in paths) { //'paths.each' do => |path|
			write_pbs_file_message_start(path[0]);
			File.open(path[0], "wb") do |f|
				add_PBS_header_to_file(f);
				// Write each element in turn
				new {GameData.Metadata, GameData.PlayerMetadata}.each do |game_data|
					if (game_data == GameData.Metadata) schema = global_schema;
					if (game_data == GameData.PlayerMetadata) schema = player_schema;
					foreach (var element in game_data) { //'game_data.each' do => |element|
						if (element.s_file_suffix != path[1]) continue;
						f.write("\#-------------------------------\r\n");
						if (schema["SectionName"]) {
							f.write("[");
							WriteCsvRecord(element.get_property_for_PBS("SectionName"), f, schema["SectionName"]);
							f.write("]\r\n");
						} else {
							f.write($"[{element.id}]\r\n");
						}
						foreach (var key in schema) { //schema.each_key do => |key|
							if (key == "SectionName") continue;
							val = element.get_property_for_PBS(key);
							if (val.null()) continue;
							if (schema[key][1][0] == "^" && val.Length > 0) {
								foreach (var sub_val in val) { //'val.each' do => |sub_val|
									f.write(string.Format("{0} = ", key));
									WriteCsvRecord(sub_val, f, schema[key]);
									f.write("\r\n");
								}
							} else {
								f.write(string.Format("{0} = ", key));
								WriteCsvRecord(val, f, schema[key]);
								f.write("\r\n");
							}
						}
					}
				}
			}
			process_pbs_file_message_end;
		}
	}

	//-----------------------------------------------------------------------------
	// Save map metadata data to PBS file.
	// NOTE: Doesn't use write_PBS_file_generic because it writes the RMXP map name
	//       next to the section header for each map.
	//-----------------------------------------------------------------------------
	public void write_map_metadata() {
		paths = get_all_PBS_file_paths(GameData.MapMetadata);
		map_infos = LoadMapInfos;
		schema = GameData.MapMetadata.schema;
		idx = 0;
		foreach (var path in paths) { //'paths.each' do => |path|
			write_pbs_file_message_start(path[0]);
			File.open(path[0], "wb") do |f|
				add_PBS_header_to_file(f);
				foreach (var element in GameData.MapMetadata) { //'GameData.MapMetadata.each' do => |element|
					if (element.s_file_suffix != path[1]) continue;
					if (idx % 100 == 0) echo ".";
					if (idx % 500 == 0) Graphics.update;
					idx += 1;
					f.write("\#-------------------------------\r\n");
					map_name = (map_infos && map_infos[element.id]) ? map_infos[element.id].name : null;
					f.write(string.Format("[{0:3}]", element.id));
					if ((map_name) f.write(string.Format(") {   // {0}", map_name));
					f.write("\r\n");
					foreach (var key in schema) { //schema.each_key do => |key|
						if (key == "SectionName") continue;
						val = element.get_property_for_PBS(key);
						if (val.null()) continue;
						if (schema[key][1][0] == "^" && val.Length > 0) {
							foreach (var sub_val in val) { //'val.each' do => |sub_val|
								f.write(string.Format("{0} = ", key));
								WriteCsvRecord(sub_val, f, schema[key]);
								f.write("\r\n");
							}
						} else {
							f.write(string.Format("{0} = ", key));
							WriteCsvRecord(val, f, schema[key]);
							f.write("\r\n");
						}
					}
				}
			}
			process_pbs_file_message_end;
		}
	}

	//-----------------------------------------------------------------------------
	// Save dungeon tileset contents data to PBS file.
	// NOTE: Doesn't use write_PBS_file_generic because it writes the tileset name
	//       next to the section header for each tileset.
	//-----------------------------------------------------------------------------
	public void write_dungeon_tilesets() {
		paths = get_all_PBS_file_paths(GameData.DungeonTileset);
		schema = GameData.DungeonTileset.schema;
		tilesets = load_data("Data/Tilesets.rxdata");
		foreach (var path in paths) { //'paths.each' do => |path|
			write_pbs_file_message_start(path[0]);
			File.open(path[0], "wb") do |f|
				add_PBS_header_to_file(f);
				// Write each element in turn
				foreach (var element in GameData.DungeonTileset) { //'GameData.DungeonTileset.each' do => |element|
					if (element.s_file_suffix != path[1]) continue;
					f.write("\#-------------------------------\r\n");
					if (schema["SectionName"]) {
						f.write("[");
						WriteCsvRecord(element.get_property_for_PBS("SectionName"), f, schema["SectionName"]);
						f.write("]");
						if ((tilesets && tilesets[element.id]) f.write($") {   // {tilesets[element.id].name}");
						f.write("\r\n");
					} else {
						f.write($"[{element.id}]\r\n");
					}
					foreach (var key in schema) { //schema.each_key do => |key|
						if (key == "SectionName") continue;
						val = element.get_property_for_PBS(key);
						if (val.null()) continue;
						if (schema[key][1][0] == "^" && val.Length > 0) {
							foreach (var sub_val in val) { //'val.each' do => |sub_val|
								f.write(string.Format("{0} = ", key));
								WriteCsvRecord(sub_val, f, schema[key]);
								f.write("\r\n");
							}
						} else {
							f.write(string.Format("{0} = ", key));
							WriteCsvRecord(val, f, schema[key]);
							f.write("\r\n");
						}
					}
				}
			}
			process_pbs_file_message_end;
		}
	}

	//-----------------------------------------------------------------------------
	// Save dungeon parameters to PBS file.
	//-----------------------------------------------------------------------------
	public void write_dungeon_parameters() {
		write_PBS_file_generic(GameData.DungeonParameters);
	}

	//-----------------------------------------------------------------------------
	// Save phone messages to PBS file.
	//-----------------------------------------------------------------------------
	public void write_phone() {
		write_PBS_file_generic(GameData.PhoneMessage);
	}
}
