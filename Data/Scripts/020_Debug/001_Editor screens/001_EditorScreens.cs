//===============================================================================
// Wild encounters editor.
//===============================================================================
// Main editor method for editing wild encounters. Lists all defined encounter
// sets, and edits them.
public void EncountersEditor() {
	map_infos = LoadMapInfos;
	commands = new List<string>();
	maps = new List<string>();
	list = ListWindow(new List<string>());
	help_window = Window_UnformattedTextPokemon.newWithSize(
		_INTL("Edit wild encounters"), Graphics.width / 2, 0, Graphics.width / 2, 96
	);
	help_window.z = 99999;
	ret = 0;
	need_refresh = true;
	do { //loop; while (true);
		if (need_refresh) {
			commands.clear;
			maps.clear;
			commands.Add(_INTL("[Add new encounter set]"));
			foreach (var enc_data in GameData.Encounter) { //'GameData.Encounter.each' do => |enc_data|
				name = (map_infos[enc_data.map]) ? map_infos[enc_data.map].name : null;
				if (enc_data.version > 0 && name) {
					commands.Add(string.Format("{0:3} (v.{0}): {0}", enc_data.map, enc_data.version, name));
				} else if (enc_data.version > 0) {
					commands.Add(string.Format("{0:3} (v.{0})", enc_data.map, enc_data.version));
				} else if (name) {
					commands.Add(string.Format("{0:3}: {0}", enc_data.map, name));
				} else {
					commands.Add(string.Format("{0:3}", enc_data.map));
				}
				maps.Add(new {enc_data.map, enc_data.version});
			}
			need_refresh = false;
		}
		ret = Commands2(list, commands, -1, ret);
		if (ret == 0) {   // Add new encounter set
			new_map_ID = ListScreen(_INTL("Choose a map"), new MapLister(DefaultMap));
			if (new_map_ID > 0) {
				new_version = new LimitProperty2(999).set(_INTL("version number"), 0);
				if (new_version && new_version >= 0) {
					if (GameData.Encounter.exists(new_map_ID, new_version)) {
						Message(_INTL("A set of encounters for map {1} version {2} already exists.", new_map_ID, new_version));
					} else {
						// Construct encounter hash
						key = string.Format("{0}_{0}", new_map_ID, new_version).to_sym;
						encounter_hash = {
							id           = key,
							map          = new_map_ID,
							version      = new_version,
							step_chances = {},
							types        = {}
						}
						GameData.Encounter.register(encounter_hash);
						maps.Add(new {new_map_ID, new_version});
						maps.sort! { |a, b| (a[0] == b[0]) ? a[1] <=> b[1] : a[0] <=> b[0] };
						ret = maps.index(new {new_map_ID, new_version}) + 1;
						need_refresh = true;
					}
				}
			}
		} else if (ret > 0) {   // Edit an encounter set
			this_set = maps[ret - 1];
			switch (ShowCommands(null, new {_INTL("Edit"), _INTL("Copy"), _INTL("Delete"), _INTL("Cancel")}, 4)) {
			break;
			case 0:   // Edit
				EncounterMapVersionEditor(GameData.Encounter.get(this_set[0], this_set[1]));
				need_refresh = true;
			break;
			case 1:   // Copy
				new_map_ID = ListScreen(_INTL("Copy to which map?"), new MapLister(this_set[0]));
				if (new_map_ID > 0) {
					new_version = new LimitProperty2(999).set(_INTL("version number"), 0);
					if (new_version && new_version >= 0) {
						if (GameData.Encounter.exists(new_map_ID, new_version)) {
							Message(_INTL("A set of encounters for map {1} version {2} already exists.", new_map_ID, new_version));
						} else {
							// Construct encounter hash
							key = string.Format("{0}_{0}", new_map_ID, new_version).to_sym;
							encounter_hash = {
								id              = key,
								map             = new_map_ID,
								version         = new_version,
								step_chances    = {},
								types           = {},
								s_file_suffix = GameData.Encounter.get(this_set[0], this_set[1]).s_file_suffix;
							}
							GameData.Encounter.get(this_set[0], this_set[1]).step_chances.each do |type, value|
								encounter_hash.step_chances[type] = value;
							}
							GameData.Encounter.get(this_set[0], this_set[1]).types.each do |type, slots|
								if (!type || !slots || slots.length == 0) continue;
								encounter_hash.types[type] = new List<string>();
								slots.each(slot => encounter_hash.types[type].Add(slot.clone));
							}
							GameData.Encounter.register(encounter_hash);
							maps.Add(new {new_map_ID, new_version});
							maps.sort! { |a, b| (a[0] == b[0]) ? a[1] <=> b[1] : a[0] <=> b[0] };
							ret = maps.index(new {new_map_ID, new_version}) + 1;
							need_refresh = true;
						}
					}
				}
			break;
			case 2:   // Delete
				if (ConfirmMessage(_INTL("Delete the encounter set for map {1} version {2}?", this_set[0], this_set[1]))) {
					key = string.Format("{0}_{0}", this_set[0], this_set[1]).to_sym;
					GameData.Encounter.DATA.delete(key);
					ret -= 1;
					need_refresh = true;
				}
			}
		} else {
			break;
		}
	}
	if (ConfirmMessage(_INTL("Save changes?"))) {
		GameData.Encounter.save;
		Compiler.write_encounters;   // Rewrite PBS file encounters.txt
	} else {
		GameData.Encounter.load;
	}
	list.dispose;
	help_window.dispose;
	Input.update;
}

// Lists the map ID, version number and defined encounter types for the given
// encounter data (a GameData.Encounter instance), and edits them.
public void EncounterMapVersionEditor(enc_data) {
	map_infos = LoadMapInfos;
	commands = new List<string>();
	enc_types = new List<string>();
	list = ListWindow(new List<string>());
	help_window = Window_UnformattedTextPokemon.newWithSize(
		_INTL("Edit map's encounters"), Graphics.width / 2, 0, Graphics.width / 2, 96
	);
	help_window.z = 99999;
	ret = 0;
	need_refresh = true;
	do { //loop; while (true);
		if (need_refresh) {
			commands.clear;
			enc_types.clear;
			map_name = (map_infos[enc_data.map]) ? map_infos[enc_data.map].name : null;
			if (map_name) {
				commands.Add(_INTL("Map ID={1} ({2})", enc_data.map, map_name));
			} else {
				commands.Add(_INTL("Map ID={1}", enc_data.map));
			}
			commands.Add(_INTL("Version={1}", enc_data.version));
			enc_data.types.each do |enc_type, slots|
				if (!enc_type) continue;
				commands.Add(_INTL("{1} (x{2})", enc_type.ToString(), slots.length));
				enc_types.Add(enc_type);
			}
			commands.Add(_INTL("[Add new encounter type]"));
			need_refresh = false;
		}
		ret = Commands2(list, commands, -1, ret);
		if (ret == 0) {   // Edit map ID
			old_map_ID = enc_data.map;
			new_map_ID = ListScreen(_INTL("Choose a new map"), new MapLister(old_map_ID));
			if (new_map_ID > 0 && new_map_ID != old_map_ID) {
				if (GameData.Encounter.exists(new_map_ID, enc_data.version)) {
					Message(_INTL("A set of encounters for map {1} version {2} already exists.", new_map_ID, enc_data.version));
				} else {
					GameData.Encounter.DATA.delete(enc_data.id);
					enc_data.map = new_map_ID;
					enc_data.id = string.Format("{0}_{0}", enc_data.map, enc_data.version).to_sym;
					GameData.Encounter.DATA[enc_data.id] = enc_data;
					need_refresh = true;
				}
			}
		} else if (ret == 1) {   // Edit version number
			old_version = enc_data.version;
			new_version = new LimitProperty2(999).set(_INTL("version number"), old_version);
			if (new_version && new_version != old_version) {
				if (GameData.Encounter.exists(enc_data.map, new_version)) {
					Message(_INTL("A set of encounters for map {1} version {2} already exists.", enc_data.map, new_version));
				} else {
					GameData.Encounter.DATA.delete(enc_data.id);
					enc_data.version = new_version;
					enc_data.id = string.Format("{0}_{0}", enc_data.map, enc_data.version).to_sym;
					GameData.Encounter.DATA[enc_data.id] = enc_data;
					need_refresh = true;
				}
			}
		} else if (ret == commands.length - 1) {   // Add new encounter type
			new_type_commands = new List<string>();
			new_types = new List<string>();
			foreach (var enc in GameData.EncounterType) { //GameData.EncounterType.each_alphabetically do => |enc|
				if (enc_data.types[enc.id]) continue;
				new_type_commands.Add(enc.real_name);
				new_types.Add(enc.id);
			}
			if (new_type_commands.length > 0) {
				chosen_type_cmd = ShowCommands(null, new_type_commands, -1);
				if (chosen_type_cmd >= 0) {
					new_type = new_types[chosen_type_cmd];
					enc_data.step_chances[new_type] = GameData.EncounterType.get(new_type).trigger_chance;
					enc_data.types[new_type] = new List<string>();
					EncounterTypeEditor(enc_data, new_type);
					enc_types.Add(new_type);
					ret = enc_types.sort.index(new_type) + 2;
					need_refresh = true;
				}
			} else {
				Message(_INTL("There are no unused encounter types to add."));
			}
		} else if (ret > 0) {   // Edit an encounter type (its step chance and slots)
			this_type = enc_types[ret - 2];
			switch (ShowCommands(null, new {_INTL("Edit"), _INTL("Copy"), _INTL("Delete"), _INTL("Cancel")}, 4)) {
			break;
			case 0:   // Edit
				EncounterTypeEditor(enc_data, this_type);
				need_refresh = true;
			break;
			case 1:   // Copy
				new_type_commands = new List<string>();
				new_types = new List<string>();
				foreach (var enc in GameData.EncounterType) { //GameData.EncounterType.each_alphabetically do => |enc|
					if (enc_data.types[enc.id]) continue;
					new_type_commands.Add(enc.real_name);
					new_types.Add(enc.id);
				}
				if (new_type_commands.length > 0) {
					chosen_type_cmd = Message(_INTL("Choose an encounter type to copy to."),
																			new_type_commands, -1);
					if (chosen_type_cmd >= 0) {
						new_type = new_types[chosen_type_cmd];
						enc_data.step_chances[new_type] = enc_data.step_chances[this_type];
						enc_data.types[new_type] = new List<string>();
						enc_data.types[this_type].each(slot => enc_data.types[new_type].Add(slot.clone));
						enc_types.Add(new_type);
						ret = enc_types.sort.index(new_type) + 2;
						need_refresh = true;
					}
				} else {
					Message(_INTL("There are no unused encounter types to copy to."));
				}
			break;
			case 2:   // Delete
				if (ConfirmMessage(_INTL("Delete the encounter type {1}?", GameData.EncounterType.get(this_type).real_name))) {
					enc_data.step_chances.delete(this_type);
					enc_data.types.delete(this_type);
					need_refresh = true;
				}
			}
		} else {
			break;
		}
	}
	list.dispose;
	help_window.dispose;
	Input.update;
}

// Lists the step chance and encounter slots for the given encounter type in the
// given encounter data (a GameData.Encounter instance), and edits them.
public void EncounterTypeEditor(enc_data, enc_type) {
	commands = new List<string>();
	list = ListWindow(new List<string>());
	help_window = Window_UnformattedTextPokemon.newWithSize(
		_INTL("Edit encounter slots"), Graphics.width / 2, 0, Graphics.width / 2, 96
	);
	help_window.z = 99999;
	enc_type_name = "";
	ret = 0;
	need_refresh = true;
	do { //loop; while (true);
		if (need_refresh) {
			enc_type_name = GameData.EncounterType.get(enc_type).real_name;
			commands.clear;
			commands.Add(_INTL("Step chance={1}%", enc_data.step_chances[enc_type] || 0));
			commands.Add(_INTL("Encounter type={1}", enc_type_name));
			if (enc_data.types[enc_type] && enc_data.types[enc_type].length > 0) {
				foreach (var slot in enc_data.types[enc_type]) { //'enc_data.types[enc_type].each' do => |slot|
					commands.Add(EncounterSlotProperty.format(slot));
				}
			}
			commands.Add(_INTL("[Add new slot]"));
			need_refresh = false;
		}
		ret = Commands2(list, commands, -1, ret);
		if (ret == 0) {   // Edit step chance
			old_step_chance = enc_data.step_chances[enc_type] || 0;
			new_step_chance = new LimitProperty(255).set(_INTL("Step chance"), old_step_chance);
			if (new_step_chance != old_step_chance) {
				enc_data.step_chances[enc_type] = new_step_chance;
				need_refresh = true;
			}
		} else if (ret == 1) {   // Edit encounter type
			new_type_commands = new List<string>();
			new_types = new List<string>();
			chosen_type_cmd = 0;
			foreach (var enc in GameData.EncounterType) { //GameData.EncounterType.each_alphabetically do => |enc|
				if (enc_data.types[enc.id] && enc.id != enc_type) continue;
				new_type_commands.Add(enc.real_name);
				new_types.Add(enc.id);
				if (enc.id == enc_type) chosen_type_cmd = new_type_commands.length - 1;
			}
			chosen_type_cmd = ShowCommands(null, new_type_commands, -1, chosen_type_cmd);
			if (chosen_type_cmd >= 0 && new_types[chosen_type_cmd] != enc_type) {
				new_type = new_types[chosen_type_cmd];
				enc_data.step_chances[new_type] = enc_data.step_chances[enc_type];
				enc_data.step_chances.delete(enc_type);
				enc_data.types[new_type] = enc_data.types[enc_type];
				enc_data.types.delete(enc_type);
				enc_type = new_type;
				need_refresh = true;
			}
		} else if (ret == commands.length - 1) {   // Add new encounter slot
			new_slot_data = EncounterSlotProperty.set(enc_type_name, null);
			if (new_slot_data) {
				enc_data.types[enc_type].Add(new_slot_data);
				need_refresh = true;
			}
		} else if (ret > 0) {   // Edit a slot
			switch (ShowCommands(null, new {_INTL("Edit"), _INTL("Copy"), _INTL("Delete"), _INTL("Cancel")}, 4)) {
			break;
			case 0:   // Edit
				old_slot_data = enc_data.types[enc_type][ret - 2];
				new_slot_data = EncounterSlotProperty.set(enc_type_name, old_slot_data.clone);
				if (new_slot_data && new_slot_data != old_slot_data) {
					enc_data.types[enc_type][ret - 2] = new_slot_data;
					need_refresh = true;
				}
			break;
			case 1:   // Copy
				enc_data.types[enc_type].insert(ret - 1, enc_data.types[enc_type][ret - 2].clone);
				ret += 1;
				need_refresh = true;
			break;
			case 2:   // Delete
				if (ConfirmMessage(_INTL("Delete this encounter slot?"))) {
					enc_data.types[enc_type].delete_at(ret - 2);
					need_refresh = true;
				}
			}
		} else {
			break;
		}
	}
	list.dispose;
	help_window.dispose;
	Input.update;
}

//===============================================================================
// Trainer type editor.
//===============================================================================
public void TrainerTypeEditor() {
	properties = GameData.TrainerType.editor_properties;
	ListScreenBlock(_INTL("Trainer Types"), new TrainerTypeLister(0, true)) do |button, tr_type|
		if (tr_type) {
			switch (button) {
				case Input.ACTION:
					if (tr_type.is_a(Symbol) && ConfirmMessageSerious(_INTL("Delete this trainer type?"))) {
						GameData.TrainerType.DATA.delete(tr_type);
						GameData.TrainerType.save;
						ConvertTrainerData;
						Message(_INTL("The Trainer type was deleted."));
					}
					break;
				case Input.USE:
					if (tr_type.is_a(Symbol)) {
						t_data = GameData.TrainerType.get(tr_type);
						data = new List<string>();
						foreach (var prop in properties) { //'properties.each' do => |prop|
							val = t_data.get_property_for_PBS(prop[0]);
							if (val.null() && prop[1].respond_to(:defaultValue)) val = prop[1].defaultValue;
							data.Add(val);
						}
						if (PropertyList(t_data.id.ToString(), data, properties, true)) {
							// Construct trainer type hash
							schema = GameData.TrainerType.schema;
							type_hash = new List<string>();
							properties.each_with_index do |prop, i|
								switch (prop[0]) {
									case "ID":
										type_hash[schema["SectionName"][0]] = data[i];
										break;
									default:
										type_hash[schema[prop[0]][0]] = data[i];
										break;
								}
							}
							type_hash.s_file_suffix = t_data.s_file_suffix;
							// Add trainer type's data to records
							GameData.TrainerType.register(type_hash);
							GameData.TrainerType.save;
							ConvertTrainerData;
						}
					} else {   // Add a new trainer type
						TrainerTypeEditorNew(null);
					}
					break;
			}
		}
	}
}

public void TrainerTypeEditorNew(default_name) {
	// Choose a name
	name = MessageFreeText(_INTL("Please enter the trainer type's name."),
													(default_name) ? System.Text.RegularExpressions.Regex.Replace(default_name, "_+", " ") : "", false, 30);
	if (nil_or_empty(name)) {
		if (!default_name) return null;
		name = default_name;
	}
	// Generate an ID based on the item name
	id = System.Text.RegularExpressions.Regex.Replace(name, "é", "e");
	id = System.Text.RegularExpressions.Regex.Replace(id, "[^A-Za-z0-9_]", "");
	id = id.upcase;
	if (id.length == 0) {
		id = string.Format("T_{0:3}", GameData.TrainerType.count);
	} else if (!System.Text.RegularExpressions.Regex.IsMatch(idnew[0],@"[A-Z]")) {
		id = "T_" + id;
	}
	if (GameData.TrainerType.exists(id)) {
		(1..100).each do |i|
			trial_id = string.Format("{0}_{0}", id, i);
			if (GameData.TrainerType.exists(trial_id)) continue;
			id = trial_id;
			break;
		}
	}
	if (GameData.TrainerType.exists(id)) {
		Message(_INTL("Failed to create the trainer type. Choose a different name."));
		return null;
	}
	// Choose a gender
	gender = Message(_INTL("Is the Trainer male, female or unknown?"),
										new {_INTL("Male"), _INTL("Female"), _INTL("Unknown")}, 0);
	// Choose a base money value
	params = new ChooseNumberParams();
	params.setRange(0, 255);
	params.setDefaultValue(30);
	base_money = MessageChooseNumber(_INTL("Set the money per level won for defeating the Trainer."), params);
	// Construct trainer type hash
	tr_type_hash = {
		id         = id.to_sym,
		name       = name,
		gender     = gender,
		base_money = base_money;
	}
	// Add trainer type's data to records
	GameData.TrainerType.register(tr_type_hash);
	GameData.TrainerType.save;
	ConvertTrainerData;
	Message(_INTL("The trainer type {1} was created (ID: {2}).", name, id.ToString()));
	Message(_INTL("Put the Trainer's graphic ({1}.png) in Graphics/Trainers, or it will be blank.", id.ToString()));
	return id.to_sym;
}

//===============================================================================
// Individual trainer editor.
//===============================================================================
public static partial class TrainerBattleProperty {
	public const int NUM_ITEMS = 8;

	public static void set(settingname, oldsetting) {
		if (!oldsetting) return null;
		properties = new {
			new {_INTL("Trainer Type"), TrainerTypeProperty,     _INTL("Name of the trainer type for this Trainer.")},
			new {_INTL("Trainer Name"), StringProperty,          _INTL("Name of the Trainer.")},
			new {_INTL("Version"),      new LimitProperty(9999), _INTL("Number used to distinguish Trainers with the same name and trainer type.")},
			new {_INTL("Lose Text"),    StringProperty,          _INTL("Message shown in battle when the Trainer is defeated.")}
		}
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			properties.Add(new {_INTL("Pokémon {1}", i + 1), TrainerPokemonProperty, _INTL("A Pokémon owned by the Trainer.")});
		}
		for (int i = NUM_ITEMS; i < NUM_ITEMS; i++) { //for 'NUM_ITEMS' times do => |i|
			properties.Add(new {_INTL("Item {1}", i + 1), ItemProperty, _INTL("An item used by the Trainer during battle.")});
		}
		if (!PropertyList(settingname, oldsetting, properties, true)) return null;
		if (!oldsetting[0]) oldsetting = null;
		return oldsetting;
	}

	public static void format(value) {
		return value.inspect;
	}
}

//===============================================================================
//
//===============================================================================
public void TrainerBattleEditor() {
	modified = false;
	ListScreenBlock(_INTL("Trainer Battles"), new TrainerBattleLister(0, true)) do |button, trainer_id|
		if (trainer_id) {
			switch (button) {
				case Input.ACTION:
					if (trainer_id.Length > 0 && ConfirmMessageSerious(_INTL("Delete this trainer battle?"))) {
						tr_data = GameData.Trainer.DATA[trainer_id];
						GameData.Trainer.DATA.delete(trainer_id);
						modified = true;
						Message(_INTL("The Trainer battle was deleted."));
					}
					break;
				case Input.USE:
					if (trainer_id.Length > 0) {   // Edit existing trainer
						tr_data = GameData.Trainer.DATA[trainer_id];
						old_type = tr_data.trainer_type;
						old_name = tr_data.real_name;
						old_version = tr_data.version;
						data = new {
							tr_data.trainer_type,
							tr_data.real_name,
							tr_data.version,
							tr_data.real_lose_text;
						}
						for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
							data.Add(tr_data.pokemon[i]);
						}
						for (int i = TrainerBattleProperty.NUM_ITEMS; i < TrainerBattleProperty.NUM_ITEMS; i++) { //for 'TrainerBattleProperty.NUM_ITEMS' times do => |i|
							data.Add(tr_data.items[i]);
						}
						do { //loop; while (true);
							data = TrainerBattleProperty.set(tr_data.real_name, data);
							if (!data) break;
							party = new List<string>();
							items = new List<string>();
							for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
								if (data[4 + i] && data[4 + i][:species]) party.Add(data[4 + i]);
							}
							for (int i = TrainerBattleProperty.NUM_ITEMS; i < TrainerBattleProperty.NUM_ITEMS; i++) { //for 'TrainerBattleProperty.NUM_ITEMS' times do => |i|
								if (data[4 + Settings.MAX_PARTY_SIZE + i]) items.Add(data[4 + Settings.MAX_PARTY_SIZE + i]);
							}
							if (!data[0]) {
								Message(_INTL("Can't save. No trainer type was chosen."));
							} else if (!data[1] || data[1].empty()) {
								Message(_INTL("Can't save. No name was entered."));
							} else if (party.length == 0) {
								Message(_INTL("Can't save. The Pokémon list is empty."));
							} else {
								trainer_hash = {
									trainer_type    = data[0],
									real_name       = data[1],
									version         = data[2],
									lose_text       = data[3],
									pokemon         = party,
									items           = items,
									s_file_suffix = tr_data.s_file_suffix;
								}
								// Add trainer type's data to records
								trainer_hash.id = new {trainer_hash.trainer_type, trainer_hash.real_name, trainer_hash.version};
								GameData.Trainer.register(trainer_hash);
								if (data[0] != old_type || data[1] != old_name || data[2] != old_version) {
									GameData.Trainer.DATA.delete(new {old_type, old_name, old_version});
								}
								modified = true;
								break;
							}
						}
					} else {   // New trainer
						tr_type = null;
						ret = Message(_INTL("First, define the new trainer's type."),
														new {_INTL("Use existing type"),
														_INTL("Create new type"),
														_INTL("Cancel")}, 3);
						switch (ret) {
							case 0:
								tr_type = ListScreen(_INTL("TRAINER TYPE"), new TrainerTypeLister(0, false));
								break;
							case 1:
								tr_type = TrainerTypeEditorNew(null);
								break;
							default:
								continue;
								break;
						}
						if (!tr_type) continue;
						tr_name = MessageFreeText(_INTL("Now enter the trainer's name."), "", false, 30);
						if (nil_or_empty(tr_name)) continue;
						tr_version = GetFreeTrainerParty(tr_type, tr_name);
						if (tr_version < 0) {
							Message(_INTL("There is no room to create a trainer of that type and name."));
							continue;
						}
						t = NewTrainer(tr_type, tr_name, tr_version, false);
						if t
							trainer_hash = {
								trainer_type = tr_type,
								real_name    = tr_name,
								version      = tr_version,
								pokemon      = [];
							}
							foreach (var pkmn in t[3]) { //'t[3].each' do => |pkmn|
								trainer_hash.pokemon.Add(
									{
										species = pkmn[0],
										level   = pkmn[1];
									}
								);
							}
							// Add trainer's data to records
							trainer_hash.id = new {trainer_hash.trainer_type, trainer_hash.real_name, trainer_hash.version};
							GameData.Trainer.register(trainer_hash);
							Message(_INTL("The Trainer battle was added."));
							modified = true;
						}
					}
					break;
			}
		}
	}
	if (modified && ConfirmMessage(_INTL("Save changes?"))) {
		GameData.Trainer.save;
		ConvertTrainerData;
	} else {
		GameData.Trainer.load;
	}
}

//===============================================================================
// Trainer Pokémon editor.
//===============================================================================
public static partial class TrainerPokemonProperty {
	public static void set(settingname, initsetting) {
		if (!initsetting) initsetting = {species = null, level = 10};
		oldsetting = new {
			initsetting.species,
			initsetting.level,
			initsetting.real_name,
			initsetting.form,
			initsetting.gender,
			initsetting.shininess,
			initsetting.super_shininess,
			initsetting.shadowness;
		}
		for (int i = Pokemon.MAX_MOVES; i < Pokemon.MAX_MOVES; i++) { //for 'Pokemon.MAX_MOVES' times do => |i|
			oldsetting.Add((initsetting.moves) ? initsetting.moves[i] : null);
		}
		oldsetting.concat(new {initsetting.ability,
											initsetting.ability_index,
											initsetting.item,
											initsetting.nature,
											initsetting.iv,
											initsetting.ev,
											initsetting.happiness,
											initsetting.poke_ball});
		max_level = GameData.GrowthRate.max_level;
		pkmn_properties = new {
			new {_INTL("Species"),       SpeciesProperty,                         _INTL("Species of the Pokémon.")},
			new {_INTL("Level"),         new NonzeroLimitProperty(max_level),     _INTL("Level of the Pokémon (1-{1}).", max_level)},
			new {_INTL("Name"),          StringProperty,                          _INTL("Nickname of the Pokémon.")},
			new {_INTL("Form"),          new LimitProperty2(999),                 _INTL("Form of the Pokémon.")},
			new {_INTL("Gender"),        GenderProperty,                          _INTL("Gender of the Pokémon.")},
			new {_INTL("Shiny"),         BooleanProperty2,                        _INTL("If set to true, the Pokémon is a different-colored Pokémon.")},
			new {_INTL("SuperShiny"),    BooleanProperty2,                        _INTL("Whether the Pokémon is super shiny (shiny with a special shininess animation).")},
			new {_INTL("Shadow"),        BooleanProperty2,                        _INTL("If set to true, the Pokémon is a Shadow Pokémon.")}
		}
		for (int i = Pokemon.MAX_MOVES; i < Pokemon.MAX_MOVES; i++) { //for 'Pokemon.MAX_MOVES' times do => |i|
			pkmn_properties.Add(new {_INTL("Move {1}", i + 1),
														new MovePropertyForSpecies(oldsetting), _INTL("A move known by the Pokémon. Leave all moves blank (use Z key to delete) for a wild moveset.")});
		}
		pkmn_properties.concat(new {
			new {_INTL("Ability"),       AbilityProperty,                         _INTL("Ability of the Pokémon. Overrides the ability index.")},
			new {_INTL("Ability index"), new LimitProperty2(99),                  _INTL("Ability index. 0=first ability, 1=second ability, 2+=hidden ability.")},
			new {_INTL("Held item"),     ItemProperty,                            _INTL("Item held by the Pokémon.")},
			new {_INTL("Nature"),        new GameDataProperty(:Nature),           _INTL("Nature of the Pokémon.")},
			new {_INTL("IVs"),           new IVsProperty(Pokemon.IV_STAT_LIMIT),  _INTL("Individual values for each of the Pokémon's stats.")},
			new {_INTL("EVs"),           new EVsProperty(Pokemon.EV_STAT_LIMIT),  _INTL("Effort values for each of the Pokémon's stats.")},
			new {_INTL("Happiness"),     new LimitProperty2(255),                 _INTL("Happiness of the Pokémon (0-255).")},
			new {_INTL("Poké Ball"),     new BallProperty(oldsetting),            _INTL("The kind of Poké Ball the Pokémon is kept in.")}}
		);
		PropertyList(settingname, oldsetting, pkmn_properties, false);
		if (!oldsetting[0]) return null;   // Species is null
		ret = {
			species         = oldsetting[0],
			level           = oldsetting[1],
			real_name       = oldsetting[2],
			form            = oldsetting[3],
			gender          = oldsetting[4],
			shininess       = oldsetting[5],
			super_shininess = oldsetting[6],
			shadowness      = oldsetting[7],
			ability         = oldsetting[8 + Pokemon.MAX_MOVES],
			ability_index   = oldsetting[9 + Pokemon.MAX_MOVES],
			item            = oldsetting[10 + Pokemon.MAX_MOVES],
			nature          = oldsetting[11 + Pokemon.MAX_MOVES],
			iv              = oldsetting[12 + Pokemon.MAX_MOVES],
			ev              = oldsetting[13 + Pokemon.MAX_MOVES],
			happiness       = oldsetting[14 + Pokemon.MAX_MOVES],
			poke_ball       = oldsetting[15 + Pokemon.MAX_MOVES]
		}
		moves = new List<string>();
		for (int i = Pokemon.MAX_MOVES; i < Pokemon.MAX_MOVES; i++) { //for 'Pokemon.MAX_MOVES' times do => |i|
			moves.Add(oldsetting[8 + i]);
		}
		moves.uniq!;
		moves.compact!;
		ret.moves = moves;
		return ret;
	}

	public static void format(value) {
		if (!value || !value.species) return "-";
		return string.Format("{0},{0}", GameData.Species.get(value.species).name, value.level);
	}
}

//===============================================================================
// Metadata editor.
//===============================================================================
public void MetadataScreen() {
	sel_player = -1;
	do { //loop; while (true);
		sel_player = ListScreen(_INTL("SET METADATA"), new MetadataLister(sel_player, true));
		if (sel_player == -1) break;
		switch (sel_player) {
			case -2:   // Add new player
				EditPlayerMetadata(-1);
				break;
			case 0:   // Edit global metadata
				EditMetadata;
				break;
			default:   // Edit player character
				if (sel_player >= 1) EditPlayerMetadata(sel_player);
				break;
		}
	}
}

public void EditMetadata() {
	data = new List<string>();
	metadata = GameData.Metadata.get;
	properties = GameData.Metadata.editor_properties;
	foreach (var property in properties) { //'properties.each' do => |property|
		val = metadata.get_property_for_PBS(property[0]);
		if (val.null() && property[1].respond_to(:defaultValue)) val = property[1].defaultValue;
		data.Add(val);
	}
	if (PropertyList(_INTL("Global Metadata"), data, properties, true)) {
		// Construct metadata hash
		schema = GameData.Metadata.schema;
		metadata_hash = new List<string>();
		properties.each_with_index do |prop, i|
			metadata_hash[schema[prop[0]][0]] = data[i];
		}
		metadata_hash.id              = 0;
		metadata_hash.s_file_suffix = metadata.s_file_suffix;
		// Add metadata's data to records
		GameData.Metadata.register(metadata_hash);
		GameData.Metadata.save;
		Compiler.write_metadata;
	}
}

public void EditPlayerMetadata(player_id = 1) {
	metadata = null;
	if (player_id < 1) {
		// Adding new player character; get lowest unused player character ID
		ids = GameData.PlayerMetadata.keys;
		1.upto(ids.max + 1) do |i|
			if (ids.Contains(i)) continue;
			player_id = i;
			break;
		}
		metadata = new GameData.PlayerMetadata({id = player_id});
	} else if (!GameData.PlayerMetadata.exists(player_id)) {
		Message(_INTL("Metadata for player character {1} was not found.", player_id));
		return;
	}
	data = new List<string>();
	if (metadata.null()) metadata = GameData.PlayerMetadata.try_get(player_id);
	properties = GameData.PlayerMetadata.editor_properties;
	foreach (var property in properties) { //'properties.each' do => |property|
		val = metadata.get_property_for_PBS(property[0]);
		if (val.null() && property[1].respond_to(:defaultValue)) val = property[1].defaultValue;
		data.Add(val);
	}
	if (PropertyList(_INTL("Player {1}", metadata.id), data, properties, true)) {
		// Construct player metadata hash
		schema = GameData.PlayerMetadata.schema;
		metadata_hash = new List<string>();
		properties.each_with_index do |prop, i|
			switch (prop[0]) {
				case "ID":
					metadata_hash[schema["SectionName"][0]] = data[i];
					break;
				default:
					metadata_hash[schema[prop[0]][0]] = data[i];
					break;
			}
		}
		metadata_hash.s_file_suffix = metadata.s_file_suffix;
		// Add player metadata's data to records
		GameData.PlayerMetadata.register(metadata_hash);
		GameData.PlayerMetadata.save;
		Compiler.write_metadata;
	}
}

//===============================================================================
// Map metadata editor.
//===============================================================================
public void MapMetadataScreen(map_id = 0) {
	do { //loop; while (true);
		map_id = ListScreen(_INTL("SET METADATA"), new MapLister(map_id));
		if (map_id < 0) break;
		(map_id == 0) ? EditMetadata : EditMapMetadata(map_id)
	}
}

public void EditMapMetadata(map_id) {
	mapinfos = LoadMapInfos;
	data = new List<string>();
	map_name = mapinfos[map_id].name;
	metadata = GameData.MapMetadata.try_get(map_id);
	if (!metadata) metadata = new GameData.MapMetadata({id = map_id});
	properties = GameData.MapMetadata.editor_properties;
	foreach (var property in properties) { //'properties.each' do => |property|
		val = metadata.get_property_for_PBS(property[0]);
		if (val.null() && property[1].respond_to(:defaultValue)) val = property[1].defaultValue;
		data.Add(val);
	}
	if (PropertyList(map_name, data, properties, true)) {
		// Construct map metadata hash
		schema = GameData.MapMetadata.schema;
		metadata_hash = new List<string>();
		properties.each_with_index do |prop, i|
			switch (prop[0]) {
				case "ID":
					metadata_hash[schema["SectionName"][0]] = data[i];
					break;
				default:
					metadata_hash[schema[prop[0]][0]] = data[i];
					break;
			}
		}
		metadata_hash.s_file_suffix = metadata.s_file_suffix;
		// Add map metadata's data to records
		GameData.MapMetadata.register(metadata_hash);
		GameData.MapMetadata.save;
		Compiler.write_map_metadata;
	}
}

//===============================================================================
// Item editor.
//===============================================================================
public void ItemEditor() {
	properties = GameData.Item.editor_properties;
	ListScreenBlock(_INTL("Items"), new ItemLister(0, true)) do |button, item|
		if (item) {
			switch (button) {
				case Input.ACTION:
					if (item.is_a(Symbol) && ConfirmMessageSerious(_INTL("Delete this item?"))) {
						GameData.Item.DATA.delete(item);
						GameData.Item.save;
						Compiler.write_items;
						Message(_INTL("The item was deleted."));
					}
					break;
				case Input.USE:
					if (item.is_a(Symbol)) {
						itm = GameData.Item.get(item);
						data = new List<string>();
						foreach (var prop in properties) { //'properties.each' do => |prop|
							val = itm.get_property_for_PBS(prop[0]);
							if (val.null() && prop[1].respond_to(:defaultValue)) val = prop[1].defaultValue;
							data.Add(val);
						}
						if (PropertyList(itm.id.ToString(), data, properties, true)) {
							// Construct item hash
							schema = GameData.Item.schema;
							item_hash = new List<string>();
							properties.each_with_index do |prop, i|
								switch (prop[0]) {
									case "ID":
										item_hash[schema["SectionName"][0]] = data[i];
										break;
									default:
										item_hash[schema[prop[0]][0]] = data[i];
										break;
								}
							}
							item_hash.s_file_suffix = itm.s_file_suffix;
							// Add item's data to records
							GameData.Item.register(item_hash);
							GameData.Item.save;
							Compiler.write_items;
						}
					} else {   // Add a new item
						ItemEditorNew(null);
					}
					break;
			}
		}
	}
}

public void ItemEditorNew(default_name) {
	// Choose a name
	name = MessageFreeText(_INTL("Please enter the item's name."),
													(default_name) ? System.Text.RegularExpressions.Regex.Replace(default_name, "_+", " ") : "", false, 30)
	if (nil_or_empty(name)) {
		if (!default_name) return;
		name = default_name;
	}
	// Generate an ID based on the item name
	id = System.Text.RegularExpressions.Regex.Replace(name, "é", "e");
	id = System.Text.RegularExpressions.Regex.Replace(id, "[^A-Za-z0-9_]", "");
	id = id.upcase;
	if (id.length == 0) {
		id = string.Format("ITEM_{0:3}", GameData.Item.count);
	} else if (!System.Text.RegularExpressions.Regex.IsMatch(idnew[0],@"[A-Z]")) {
		id = "ITEM_" + id;
	}
	if (GameData.Item.exists(id)) {
		(1..100).each do |i|
			trial_id = string.Format("{0}_{0}", id, i);
			if (GameData.Item.exists(trial_id)) continue;
			id = trial_id;
			break;
		}
	}
	if (GameData.Item.exists(id)) {
		Message(_INTL("Failed to create the item. Choose a different name."));
		return;
	}
	// Choose a pocket
	pocket = PocketProperty.set("", 0);
	if (pocket == 0) return;
	// Choose a price
	price = new LimitProperty(999_999).set(_INTL("Purchase price"), -1);
	if (price == -1) return;
	// Choose a description
	description = StringProperty.set(_INTL("Description"), "");
	// Construct item hash
	item_hash = {
		id          = id.to_sym,
		name        = name,
		name_plural = name + "s",
		pocket      = pocket,
		price       = price,
		description = description;
	}
	// Add item's data to records
	GameData.Item.register(item_hash);
	GameData.Item.save;
	Compiler.write_items;
	Message(_INTL("The item {1} was created (ID: {2}).", name, id.ToString()));
	Message(_INTL("Put the item's graphic ({1}.png) in Graphics/Items, or it will be blank.", id.ToString()));
}

//===============================================================================
// Pokémon species editor.
//===============================================================================
public void PokemonEditor() {
	properties = GameData.Species.editor_properties;
	ListScreenBlock(_INTL("Pokémon species"), new SpeciesLister(0, false)) do |button, species|
		if (species) {
			switch (button) {
				case Input.ACTION:
					if (species.is_a(Symbol) && ConfirmMessageSerious(_INTL("Delete this species?"))) {
						GameData.Species.DATA.delete(species);
						GameData.Species.save;
						Compiler.write_pokemon;
						Message(_INTL("The species was deleted."));
					}
					break;
				case Input.USE:
					if (species.is_a(Symbol)) {
						spec = GameData.Species.get(species);
						data = new List<string>();
						foreach (var prop in properties) { //'properties.each' do => |prop|
							val = spec.get_property_for_PBS(prop[0]);
							if (val.null() && prop[1].respond_to(:defaultValue)) val = prop[1].defaultValue;
							if (new []{"Height", "Weight"}.Contains(prop[0])) val = (int)Math.Round(val * 10);
							data.Add(val);
						}
						// Edit the properties
						if (PropertyList(spec.id.ToString(), data, properties, true)) {
							// Construct species hash
							schema = GameData.Species.schema;
							species_hash = new List<string>();
							properties.each_with_index do |prop, i|
								if (new []{"Height", "Weight"}.Contains(prop[0])) data[i] = data[i].to_f / 10;
								switch (prop[0]) {
									case "ID":
										species_hash[schema["SectionName"][0]] = data[i];
										break;
									default:
										species_hash[schema[prop[0]][0]] = data[i];
										break;
								}
							}
							species_hash.s_file_suffix = spec.s_file_suffix;
							// Sanitise data
							Compiler.validate_compiled_pokemon(species_hash);
							foreach (var evo in species_hash.evolutions) { //'species_hash.evolutions.each' do => |evo|
								param_type = GameData.Evolution.get(evo[1]).parameter;
								if (param_type.null()) {
									evo[2] = null;
								} else if (param_type == Integer) {
									evo[2] = Compiler.cast_csv_value(evo[2], "u");
								} else if (param_type != String) {
									evo[2] = Compiler.cast_csv_value(evo[2], "e", param_type);
								}
							}
							// Add species' data to records
							GameData.Species.register(species_hash);
							GameData.Species.save;
							Compiler.write_pokemon;
							Message(_INTL("Data saved."));
						}
					} else {
						Message(_INTL("Can't add a new species."));
					}
					break;
			}
		}
	}
}

//===============================================================================
// Regional Dexes editor.
//===============================================================================
public void RegionalDexEditor(dex) {
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = 99999;
	cmd_window = ListWindow(new List<string>());
	info = Window_AdvancedTextPokemon.newWithSize(
		_INTL("Z+Up/Down: Rearrange entries\nZ+Right: Insert new entry\nZ+Left: Delete entry\nD: Clear entry"),
		Graphics.width / 2, 64, Graphics.width / 2, Graphics.height - 64, viewport
	);
	info.z = 2;
	dex.compact!;
	ret = dex.clone;
	commands = new List<string>();
	refresh_list = true;
	cmd = new {0, 0};   // new {action, index in list}
	do { //loop; while (true);
		// Populate commands
		if (refresh_list) {
			do { //loop; while (true);
				if (dex.length == 0 || dex[-1]) break;
				dex.slice!(-1);
			}
			commands = new List<string>();
			dex.each_with_index do |species, i|
				text = (species) ? GameData.Species.get(species).real_name : "----------";
				commands.Add(string.Format("{0:3}: {0}", i + 1, text));
			}
			commands.Add(string.Format("{0:3}: ----------", commands.length + 1));
			cmd[1] = (int)Math.Min(cmd[1], commands.length - 1);
			refresh_list = false;
		}
		// Choose to do something
		cmd = Commands3(cmd_window, commands, -1, cmd[1], true);
		switch (cmd[0]) {
			case 1:   // Swap entry up
				if (cmd[1] < dex.length - 1) {
					dex[cmd[1] + 1], dex[cmd[1]] = dex[cmd[1]], dex[cmd[1] + 1];
					refresh_list = true;
				}
				break;
			case 2:   // Swap entry down
				if (cmd[1] > 0) {
					dex[cmd[1] - 1], dex[cmd[1]] = dex[cmd[1]], dex[cmd[1] - 1];
					refresh_list = true;
				}
				break;
			case 3:   // Delete spot
				if (cmd[1] < dex.length) {
					dex.delete_at(cmd[1]);
					refresh_list = true;
				}
				break;
			case 4:   // Insert spot
				if (cmd[1] < dex.length) {
					dex.insert(cmd[1], null);
					refresh_list = true;
				}
				break;
			case 5:   // Clear spot
				if (dex[cmd[1]]) {
					dex[cmd[1]] = null;
					refresh_list = true;
				}
				break;
			case 0:
				if (cmd[1] >= 0) {   // Edit entry
					switch (Message("\\ts[]" + _INTL("Do what with this entry?"),
												new {_INTL("Change species"), _INTL("Clear"),
													_INTL("Insert entry"), _INTL("Delete entry"),
													_INTL("Cancel")}, 5)) {
						case 0:   // Change species
							species = ChooseSpeciesList(dex[cmd[1]]);
							if (species) {
								dex[cmd[1]] = species;
								dex.each_with_index((s, i) => { if (i != cmd[1] && s == species) dex[i] = null; });
								refresh_list = true;
							}
							break;
						case 1:   // Clear spot
							if (dex[cmd[1]]) {
								dex[cmd[1]] = null;
								refresh_list = true;
							}
							break;
						case 2:   // Insert spot
							if (cmd[1] < dex.length) {
								dex.insert(cmd[1], null);
								refresh_list = true;
							}
							break;
						case 3:   // Delete spot
							if (cmd[1] < dex.length) {
								dex.delete_at(cmd[1]);
								refresh_list = true;
							}
							break;
					}
				} else {   // Cancel
					switch (Message(_INTL("Save changes?"),
												new {_INTL("Yes"), _INTL("No"), _INTL("Cancel")}, 3)) {
						case 0:   // Save all changes to Dex
							dex.slice!(-1) until dex[-1];
							ret = dex;
							break;
							break;
						case 1:   // Just quit
							break;
							break;
					}
				}
				break;
		}
	}
	info.dispose;
	cmd_window.dispose;
	viewport.dispose;
	ret.compact!;
	return ret;
}

public void RegionalDexEditorMain() {
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = 99999;
	cmd_window = ListWindow(new List<string>());
	cmd_window.viewport = viewport;
	cmd_window.z        = 2;
	title = Window_UnformattedTextPokemon.newWithSize(
		_INTL("Regional Dexes Editor"), Graphics.width / 2, 0, Graphics.width / 2, 64, viewport
	);
	title.z = 2;
	info = Window_AdvancedTextPokemon.newWithSize(
		_INTL("Z+Up/Down: Rearrange Dexes"), Graphics.width / 2, 64,
		Graphics.width / 2, Graphics.height - 64, viewport
	);
	info.z = 2;
	dex_lists = new List<string>();
	LoadRegionalDexes.each_with_index((d, index) => dex_lists[index] = d.clone);
	commands = new List<string>();
	refresh_list = true;
	oldsel = -1;
	cmd = new {0, 0};   // new {action, index in list}
	do { //loop; while (true);
		// Populate commands
		if (refresh_list) {
			commands = [_INTL("[ADD DEX]")];
			dex_lists.each_with_index do |list, i|
				commands.Add(_INTL("Dex {1} (size {2})", i + 1, list.length));
			}
			refresh_list = false;
		}
		// Choose to do something
		oldsel = -1;
		cmd = Commands3(cmd_window, commands, -1, cmd[1], true);
		switch (cmd[0]) {
			case 1:   // Swap Dex up
				if (cmd[1] > 0 && cmd[1] < commands.length - 1) {
					dex_lists[cmd[1] - 1], dex_lists[cmd[1]] = dex_lists[cmd[1]], dex_lists[cmd[1] - 1];
					refresh_list = true;
				}
				break;
			case 2:   // Swap Dex down
				if (cmd[1] > 1) {
					dex_lists[cmd[1] - 2], dex_lists[cmd[1] - 1] = dex_lists[cmd[1] - 1], dex_lists[cmd[1] - 2];
					refresh_list = true;
				}
				break;
			case 0:   // Clicked on a command/Dex
				if (cmd[1] == 0) {   // Add new Dex
					switch (Message(_INTL("Fill in this new Dex?"),
												new {_INTL("Leave blank"), _INTL("National Dex"),
													_INTL("Nat. Dex grouped families"), _INTL("Cancel")}, 4)) {
						case 0:   // Leave blank
							dex_lists.Add(new List<string>());
							refresh_list = true;
							break;
						case 1:   // Fill with National Dex
							new_dex = new List<string>();
							GameData.Species.each_species(s => new_dex.Add(s.species));
							dex_lists.Add(new_dex);
							refresh_list = true;
							break;
						case 2:   // Fill with National Dex (grouped families)
							new_dex = new List<string>();
							seen = new List<string>();
							foreach (var s in GameData.Species) { //GameData.Species.each_species do => |s|
								if (seen.Contains(s.species)) continue;
								family = s.get_family_species;
								new_dex.concat(family);
								seen.concat(family);
							}
							dex_lists.Add(new_dex);
							refresh_list = true;
							break;
					}
				} else if (cmd[1] > 0) {   // Edit a Dex
					switch (Message("\\ts[]" + _INTL("Do what with this Dex?"),
												new {_INTL("Edit"), _INTL("Copy"), _INTL("Delete"), _INTL("Cancel")}, 4)) {
						case 0:   // Edit
							dex_lists[cmd[1] - 1] = RegionalDexEditor(dex_lists[cmd[1] - 1]);
							refresh_list = true;
							break;
						case 1:   // Copy
							dex_lists[dex_lists.length] = dex_lists[cmd[1] - 1].clone;
							cmd[1] = dex_lists.length;
							refresh_list = true;
							break;
						case 2:   // Delete
							dex_lists.delete_at(cmd[1] - 1);
							cmd[1] = (int)Math.Min(cmd[1], dex_lists.length);
							refresh_list = true;
							break;
					}
				} else {   // Cancel
					switch (Message(_INTL("Save changes?"),
												new {_INTL("Yes"), _INTL("No"), _INTL("Cancel")}, 3)) {
						case 0:   // Save all changes to Dexes
							save_data(dex_lists, "Data/regional_dexes.dat");
							Game.GameData.game_temp.regional_dexes_data = null;
							Compiler.write_regional_dexes;
							Message(_INTL("Data saved."));
							break;
							break;
						case 1:   // Just quit
							break;
							break;
					}
				}
				break;
		}
	}
	title.dispose;
	info.dispose;
	cmd_window.dispose;
	viewport.dispose;
}

public void AppendEvoToFamilyArray(species, array, seenarray) {
	if (seenarray[species]) return;
	array.Add(species);
	seenarray[species] = true;
	evos = GameData.Species.get(species).get_evolutions;
	if (evos.length > 0) {
		subarray = new List<string>();
		foreach (var i in evos) { //'evos.each' do => |i|
			AppendEvoToFamilyArray(i[0], subarray, seenarray);
		}
		if (subarray.length > 0) array.Add(subarray);
	}
}

public void GetEvoFamilies() {
	seen = new List<string>();
	ret = new List<string>();
	foreach (var sp in GameData.Species) { //GameData.Species.each_species do => |sp|
		species = sp.get_baby_species;
		if (seen[species]) continue;
		subret = new List<string>();
		AppendEvoToFamilyArray(species, subret, seen);
		if (subret.length > 0) ret.Add(subret.flatten);
	}
	return ret;
}

public void EvoFamiliesToStrings() {
	ret = new List<string>();
	families = GetEvoFamilies;
	for (int fam = families.length; fam < families.length; fam++) { //for 'families.length' times do => |fam|
		string = "";
		for (int p = families[fam].length; p < families[fam].length; p++) { //for 'families[fam].length' times do => |p|
			if (p >= 3) {
				string += $" + {families[fam].length - 3} more";
				break;
			}
			if (p > 0) string += "/";
			string += GameData.Species.get(families[fam][p]).name;
		}
		ret[fam] = string;
	}
	return ret;
}

//===============================================================================
// Battle animations rearranger.
//===============================================================================
public void AnimationsOrganiser() {
	list = LoadBattleAnimations;
	if (!list || !list[0]) {
		Message(_INTL("No animations exist."));
		return;
	}
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = 99999;
	cmdwin = ListWindow(new List<string>());
	cmdwin.viewport = viewport;
	cmdwin.z        = 2;
	title = Window_UnformattedTextPokemon.newWithSize(
		_INTL("Animations Organiser"), Graphics.width / 2, 0, Graphics.width / 2, 64, viewport
	);
	title.z = 2;
	info = Window_AdvancedTextPokemon.newWithSize(
		_INTL("Z+Up/Down: Swap\nZ+Left: Delete\nZ+Right: Insert"),
		Graphics.width / 2, 64, Graphics.width / 2, Graphics.height - 64, viewport
	);
	info.z = 2;
	commands = new List<string>();
	refreshlist = true;
	oldsel = -1;
	cmd = new {0, 0};
	do { //loop; while (true);
		if (refreshlist) {
			commands = new List<string>();
			for (int i = list.length; i < list.length; i++) { //for 'list.length' times do => |i|
				commands.Add(string.Format("{0}: {0}", i, (list[i]) ? list[i].name : "???"));
			}
		}
		refreshlist = false;
		oldsel = -1;
		cmd = Commands3(cmdwin, commands, -1, cmd[1], true);
		switch (cmd[0]) {
			case 1:   // Swap animation up
				if (cmd[1] >= 0 && cmd[1] < commands.length - 1) {
					list[cmd[1] + 1], list[cmd[1]] = list[cmd[1]], list[cmd[1] + 1];
					refreshlist = true;
				}
				break;
			case 2:   // Swap animation down
				if (cmd[1] > 0) {
					list[cmd[1] - 1], list[cmd[1]] = list[cmd[1]], list[cmd[1] - 1];
					refreshlist = true;
				}
				break;
			case 3:   // Delete spot
				list.delete_at(cmd[1]);
				cmd[1] = (int)Math.Min(cmd[1], list.length - 1);
				refreshlist = true;
				Wait(0.2);
				break;
			case 4:   // Insert spot
				list.insert(cmd[1], new Animation());
				refreshlist = true;
				Wait(0.2);
				break;
			case 0:
				cmd2 = Message(_INTL("Save changes?"),
												new {_INTL("Yes"), _INTL("No"), _INTL("Cancel")}, 3);
				if (new []{0, 1}.Contains(cmd2)) {
					if (cmd2 == 0) {
						// Save animations here
						save_data(list, "Data/PkmnAnimations.rxdata");
						Game.GameData.game_temp.battle_animations_data = null;
						Message(_INTL("Data saved."));
					}
					break;
				}
				break;
		}
	}
	title.dispose;
	info.dispose;
	cmdwin.dispose;
	viewport.dispose;
}
