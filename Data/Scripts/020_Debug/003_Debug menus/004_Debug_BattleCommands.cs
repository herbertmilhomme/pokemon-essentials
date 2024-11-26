//===============================================================================
// Battler options.
//===============================================================================

MenuHandlers.add(:battle_debug_menu, :battlers, {
	"name"        => _INTL("Battlers..."),
	"parent"      => :main,
	"description" => _INTL("Look at Pokémon in battle and change their properties.");
});

MenuHandlers.add(:battle_debug_menu, :list_player_battlers, {
	"name"        => _INTL("Player-side battlers"),
	"parent"      => :battlers,
	"description" => _INTL("Edit Pokémon on the player's side of battle."),
	"effect"      => battle => {
		battlers = new List<string>();
		cmds = new List<string>();
		foreach (var b in battle.allSameSideBattlers) { //'battle.allSameSideBattlers.each' do => |b|
			battlers.Add(b);
			text = $"[{b.index}] {b.name}";
			if (b.OwnedByPlayer()) {
				text += " (yours)";
			} else {
				text += " (ally's)";
			}
			cmds.Add(text);
		}
		cmd = 0;
		do { //loop; while (true);
			cmd = Message("\\ts[]" + _INTL("Choose a Pokémon."), cmds, -1, null, cmd);
			if (cmd < 0) break;
			battle.BattlePokemonDebug(battlers[cmd].pokemon, battlers[cmd]);
		}
	}
});

MenuHandlers.add(:battle_debug_menu, :list_foe_battlers, {
	"name"        => _INTL("Foe-side battlers"),
	"parent"      => :battlers,
	"description" => _INTL("Edit Pokémon on the opposing side of battle."),
	"effect"      => battle => {
		battlers = new List<string>();
		cmds = new List<string>();
		foreach (var b in battle.allOtherSideBattlers) { //'battle.allOtherSideBattlers.each' do => |b|
			battlers.Add(b);
			cmds.Add($"[{b.index}] {b.name}");
		}
		cmd = 0;
		do { //loop; while (true);
			cmd = Message("\\ts[]" + _INTL("Choose a Pokémon."), cmds, -1, null, cmd);
			if (cmd < 0) break;
			battle.BattlePokemonDebug(battlers[cmd].pokemon, battlers[cmd]);
		}
	}
});

MenuHandlers.add(:battle_debug_menu, :speed_order, {
	"name"        => _INTL("View battler speed order"),
	"parent"      => :battlers,
	"description" => _INTL("Show all battlers in order from fastest to slowest."),
	"effect"      => battle => {
		battlers = battle.allBattlers.map { |b| new {b, b.Speed} };
		battlers.sort! { |a, b| b[1] <=> a[1] };
		commands = new List<string>();
		foreach (var value in battlers) { //'battlers.each' do => |value|
			b = value[0];
			commands.Add(string.Format("[{0}] {0} (speed: {0})", b.index, b.ToString(), value[1]));
		}
		Message("\\ts[]" + _INTL("Battlers are listed from fastest to slowest. Speeds include modifiers."),
							commands, -1);
	}
});

//===============================================================================
// Pokémon.
//===============================================================================

MenuHandlers.add(:battle_debug_menu, :pokemon_teams, {
	"name"        => _INTL("Pokémon teams"),
	"parent"      => :main,
	"description" => _INTL("Look at and edit all Pokémon in each team."),
	"effect"      => battle => {
		player_party_starts = battle.PartyStarts(0);
		foe_party_starts = battle.PartyStarts(1);
		cmd = 0;
		do { //loop; while (true);
			// Find all teams and how many Pokémon they have
			commands = new List<string>();
			team_indices = new List<string>();
			if (battle.opponent) {
				battle.opponent.each_with_index do |trainer, i|
					first_index = foe_party_starts[i];
					last_index = (i < foe_party_starts.length - 1) ? foe_party_starts[i + 1] : battle.Party(1).length;
					num_pkmn = last_index - first_index;
					commands.Add(_INTL("Opponent {1}: {2} ({3} Pokémon)", i + 1, trainer.full_name, num_pkmn));
					team_indices.Add(new {1, i, first_index});
				}
			} else {
				commands.Add(_INTL("Opponent: {1} wild Pokémon", battle.Party(1).length));
				team_indices.Add(new {1, 0, 0});
			}
			battle.player.each_with_index do |trainer, i|
				first_index = player_party_starts[i];
				last_index = (i < player_party_starts.length - 1) ? player_party_starts[i + 1] : battle.Party(0).length;
				num_pkmn = last_index - first_index;
				if (i == 0) {   // Player
					commands.Add(_INTL("You: {1} ({2} Pokémon)", trainer.full_name, num_pkmn));
				} else {
					commands.Add(_INTL("Ally {1}: {2} ({3} Pokémon)", i, trainer.full_name, num_pkmn));
				}
				team_indices.Add(new {0, i, first_index});
			}
			// Choose a team
			cmd = Message("\\ts[]" + _INTL("Choose a team."), commands, -1, null, cmd);
			if (cmd < 0) break;
			// Pick a Pokémon to look at
			pkmn_cmd = 0;
			do { //loop; while (true);
				pkmn = new List<string>();
				pkmn_cmds = new List<string>();
				battle.eachInTeam(team_indices[cmd][0], team_indices[cmd][1]) do |p|
					pkmn.Add(p);
					pkmn_cmds.Add($"[{pkmn_cmds.length + 1}] {p.name} Lv.{p.level} (HP: {p.hp}/{p.totalhp})");
				}
				pkmn_cmd = Message("\\ts[]" + _INTL("Choose a Pokémon."), pkmn_cmds, -1, null, pkmn_cmd);
				if (pkmn_cmd < 0) break;
				battle.BattlePokemonDebug(pkmn[pkmn_cmd],
																		battle.FindBattler(team_indices[cmd][2] + pkmn_cmd, team_indices[cmd][0]));
			}
		}
	}
});

//===============================================================================
// Trainer options.
//===============================================================================

MenuHandlers.add(:battle_debug_menu, :trainers, {
	"name"        => _INTL("Trainer options..."),
	"parent"      => :main,
	"description" => _INTL("Variables that apply to trainers.");
});

MenuHandlers.add(:battle_debug_menu, :trainer_items, {
	"name"        => _INTL("NPC trainer items"),
	"parent"      => :trainers,
	"description" => _INTL("View and change the items each NPC trainer has access to."),
	"effect"      => battle => {
		cmd = 0;
		do { //loop; while (true);
			// Find all NPC trainers and their items
			commands = new List<string>();
			item_arrays = new List<string>();
			trainer_indices = new List<string>();
			if (battle.opponent) {
				battle.opponent.each_with_index do |trainer, i|
					items = battle.items ? battle.items[i].clone : [];
					commands.Add(_INTL("Opponent {1}: {2} ({3} items)", i + 1, trainer.full_name, items.length));
					item_arrays.Add(items);
					trainer_indices.Add(new {1, i});
				}
			}
			if (battle.player.length > 1) {
				battle.player.each_with_index do |trainer, i|
					if (i == 0) continue;   // Player
					items = battle.ally_items ? battle.ally_items[i].clone : [];
					commands.Add(_INTL("Ally {1}: {2} ({3} items)", i, trainer.full_name, items.length));
					item_arrays.Add(items);
					trainer_indices.Add(new {0, i});
				}
			}
			if (commands.length == 0) {
				Message("\\ts[]" + _INTL("There are no NPC trainers in this battle."));
				break;
			}
			// Choose a trainer
			cmd = Message("\\ts[]" + _INTL("Choose a trainer."), commands, -1, null, cmd);
			if (cmd < 0) break;
			// Get trainer's items
			items = item_arrays[cmd];
			indices = trainer_indices[cmd];
			// Edit trainer's items
			item_list_property = new GameDataPoolProperty(:Item);
			new_items = item_list_property.set(null, items);
			if (indices[0] == 0) {   // Ally
				if (!battle.ally_items) battle.ally_items = new List<string>();
				battle.ally_items[indices[1]] = new_items;
			} else {   // Opponent
				if (!battle.items) battle.items = new List<string>();
				battle.items[indices[1]] = new_items;
			}
		}
	}
});

MenuHandlers.add(:battle_debug_menu, :mega_evolution, {
	"name"        => _INTL("Mega Evolution"),
	"parent"      => :trainers,
	"description" => _INTL("Whether each trainer is allowed to Mega Evolve."),
	"effect"      => battle => {
		cmd = 0;
		do { //loop; while (true);
			commands = new List<string>();
			cmds = new List<string>();
			battle.megaEvolution.each_with_index do |side_values, side|
				trainers = (side == 0) ? battle.player : battle.opponent;
				if (!trainers) continue;
				side_values.each_with_index do |value, i|
					if (!trainers[i]) continue;
					text = (side == 0) ? "Your side:" : "Foe side:";
					text += string.Format(" {0}: {0}", i, trainers[i].name);
					if (value == -1) text += " [ABLE]";
					if (value == -2) text += " [UNABLE]";
					commands.Add(text);
					cmds.Add(new {side, i});
				}
			}
			cmd = Message("\\ts[]" + _INTL("Choose trainer to toggle whether they can Mega Evolve."),
											commands, -1, null, cmd);
			if (cmd < 0) break;
			real_cmd = cmds[cmd];
			if (battle.megaEvolution[real_cmd[0]][real_cmd[1]] == -1) {
				battle.megaEvolution[real_cmd[0]][real_cmd[1]] = -2;   // Make unable
			} else {
				battle.megaEvolution[real_cmd[0]][real_cmd[1]] = -1;   // Make able
			}
		}
	}
});

//===============================================================================
// Field options.
//===============================================================================

MenuHandlers.add(:battle_debug_menu, :field, {
	"name"        => _INTL("Field effects..."),
	"parent"      => :main,
	"description" => _INTL("Effects that apply to the whole battlefield.");
});

MenuHandlers.add(:battle_debug_menu, :weather, {
	"name"        => _INTL("Weather"),
	"parent"      => :field,
	"description" => _INTL("Set weather and duration."),
	"effect"      => battle => {
		weather_types = new List<string>();
		weather_cmds = new List<string>();
		foreach (var weather in GameData.BattleWeather) { //'GameData.BattleWeather.each' do => |weather|
			if (weather.id == weathers.None) continue;
			weather_types.Add(weather.id);
			weather_cmds.Add(weather.name);
		}
		cmd = 0;
		do { //loop; while (true);
			weather_data = GameData.BattleWeather.try_get(battle.field.weather);
			msg = _INTL("Current weather: {1}", weather_data.name || _INTL("Unknown"));
			if (weather_data.id != :None) {
				if (battle.field.weatherDuration > 0) {
					msg += "\n";
					msg += _INTL("Duration : {1} more round(s)", battle.field.weatherDuration);
				} else if (battle.field.weatherDuration < 0) {
					msg += "\n";
					msg += _INTL("Duration : Infinite");
				}
			}
			cmd = Message("\\ts[]" + msg, new {_INTL("Change type"),
																			_INTL("Change duration"),
																			_INTL("Clear weather")}, -1, null, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Change type
					weather_cmd = weather_types.index(battle.field.weather) || 0;
					new_weather = Message(
						"\\ts[]" + _INTL("Choose the new weather type."), weather_cmds, -1, null, weather_cmd
					);
					if (new_weather >= 0) {
						battle.field.weather = weather_types[new_weather];
						if (battle.field.weatherDuration == 0) battle.field.weatherDuration = 5;
					}
					break;
				case 1:   // Change duration
					if (battle.field.weather == weathers.None) {
						Message("\\ts[]" + _INTL("There is no weather."));
						continue;
					}
					params = new ChooseNumberParams();
					params.setRange(0, 99);
					params.setInitialValue((int)Math.Max(battle.field.weatherDuration, 0));
					params.setCancelValue((int)Math.Max(battle.field.weatherDuration, 0));
					new_duration = MessageChooseNumber(
						"\\ts[]" + _INTL("Choose the new weather duration (0=infinite)."), params
					);
					if (new_duration != (int)Math.Max(battle.field.weatherDuration, 0)) {
						battle.field.weatherDuration = (new_duration == 0) ? -1 : new_duration;
					}
					break;
				case 2:   // Clear weather
					battle.field.weather = weathers.None;
					battle.field.weatherDuration = 0;
					break;
			}
		}
	}
});

MenuHandlers.add(:battle_debug_menu, :terrain, {
	"name"        => _INTL("Terrain"),
	"parent"      => :field,
	"description" => _INTL("Set terrain and duration."),
	"effect"      => battle => {
		terrain_types = new List<string>();
		terrain_cmds = new List<string>();
		foreach (var terrain in GameData.BattleTerrain) { //'GameData.BattleTerrain.each' do => |terrain|
			if (terrain.id == :None) continue;
			terrain_types.Add(terrain.id);
			terrain_cmds.Add(terrain.name);
		}
		cmd = 0;
		do { //loop; while (true);
			terrain_data = GameData.BattleTerrain.try_get(battle.field.terrain);
			msg = _INTL("Current terrain: {1}", terrain_data.name || _INTL("Unknown"));
			if (terrain_data.id != :None) {
				if (battle.field.terrainDuration > 0) {
					msg += "\n";
					msg += _INTL("Duration : {1} more round(s)", battle.field.terrainDuration);
				} else if (battle.field.terrainDuration < 0) {
					msg += "\n";
					msg += _INTL("Duration : Infinite");
				}
			}
			cmd = Message("\\ts[]" + msg, new {_INTL("Change type"),
																			_INTL("Change duration"),
																			_INTL("Clear terrain")}, -1, null, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Change type
					terrain_cmd = terrain_types.index(battle.field.terrain) || 0;
					new_terrain = Message(
						"\\ts[]" + _INTL("Choose the new terrain type."), terrain_cmds, -1, null, terrain_cmd
					);
					if (new_terrain >= 0) {
						battle.field.terrain = terrain_types[new_terrain];
						if (battle.field.terrainDuration == 0) battle.field.terrainDuration = 5;
					}
					break;
				case 1:   // Change duration
					if (battle.field.terrain == :None) {
						Message("\\ts[]" + _INTL("There is no terrain."));
						continue;
					}
					params = new ChooseNumberParams();
					params.setRange(0, 99);
					params.setInitialValue((int)Math.Max(battle.field.terrainDuration, 0));
					params.setCancelValue((int)Math.Max(battle.field.terrainDuration, 0));
					new_duration = MessageChooseNumber(
						"\\ts[]" + _INTL("Choose the new terrain duration (0=infinite)."), params
					);
					if (new_duration != (int)Math.Max(battle.field.terrainDuration, 0)) {
						battle.field.terrainDuration = (new_duration == 0) ? -1 : new_duration;
					}
					break;
				case 2:   // Clear terrain
					battle.field.terrain = :None;
					battle.field.terrainDuration = 0;
					break;
			}
		}
	}
});

MenuHandlers.add(:battle_debug_menu, :environment_time, {
	"name"        => _INTL("Environment/time"),
	"parent"      => :field,
	"description" => _INTL("Set the battle's environment and time of day."),
	"effect"      => battle => {
		environment_types = new List<string>();
		environment_cmds = new List<string>();
		foreach (var environment in GameData.Environment) { //'GameData.Environment.each' do => |environment|
			environment_types.Add(environment.id);
			environment_cmds.Add(environment.name);
		}
		cmd = 0;
		do { //loop; while (true);
			environment_data = GameData.Environment.try_get(battle.environment);
			msg = _INTL("Environment: {1}", environment_data.name || _INTL("Unknown"));
			msg += "\n";
			msg += _INTL("Time of day: {1}", new {_INTL("Day"), _INTL("Evening"), _INTL("Night")}[battle.time]);
			cmd = Message("\\ts[]" + msg, new {_INTL("Change environment"),
																			_INTL("Change time of day")}, -1, null, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Change environment
					environment_cmd = environment_types.index(battle.environment) || 0;
					new_environment = Message(
						"\\ts[]" + _INTL("Choose the new environment."), environment_cmds, -1, null, environment_cmd
					);
					if (new_environment >= 0) {
						battle.environment = environment_types[new_environment];
					}
					break;
				case 1:   // Change time of day
					new_time = Message("\\ts[]" + _INTL("Choose the new time."),
															new {_INTL("Day"), _INTL("Evening"), _INTL("Night")}, -1, null, battle.time);
					if (new_time >= 0 && new_time != battle.time) battle.time = new_time;
					break;
			}
		}
	}
});

MenuHandlers.add(:battle_debug_menu, :backdrop, {
	"name"        => _INTL("Backdrop names"),
	"parent"      => :field,
	"description" => _INTL("Set the names of the backdrop and base graphics."),
	"effect"      => battle => {
		do { //loop; while (true);
			cmd = Message("\\ts[]" + _INTL("Set which backdrop name?"),
											new {_INTL("Backdrop"),
											_INTL("Base modifier")}, -1);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Backdrop
					text = MessageFreeText("\\ts[]" + _INTL("Set the backdrop's name."),
																	battle.backdrop, false, 100, Graphics.width);
					battle.backdrop = (nil_or_empty(text)) ? "Indoor1" : text;
					break;
				case 1:   // Base modifier
					text = MessageFreeText("\\ts[]" + _INTL("Set the base modifier text."),
																	battle.backdropBase, false, 100, Graphics.width);
					battle.backdropBase = (nil_or_empty(text)) ? null : text;
					break;
			}
		}
	}
});

MenuHandlers.add(:battle_debug_menu, :set_field_effects, {
	"name"        => _INTL("Other field effects..."),
	"parent"      => :field,
	"description" => _INTL("View/set other effects that apply to the whole battlefield."),
	"effect"      => battle => {
		editor = new Battle.DebugSetEffects(battle, :field);
		editor.update;
		editor.dispose;
	}
});

MenuHandlers.add(:battle_debug_menu, :player_side, {
	"name"        => _INTL("Player's side effects..."),
	"parent"      => :field,
	"description" => _INTL("Effects that apply to the side the player is on."),
	"effect"      => battle => {
		editor = new Battle.DebugSetEffects(battle, :side, 0);
		editor.update;
		editor.dispose;
	}
});

MenuHandlers.add(:battle_debug_menu, :opposing_side, {
	"name"        => _INTL("Foe's side effects..."),
	"parent"      => :field,
	"description" => _INTL("Effects that apply to the opposing side."),
	"effect"      => battle => {
		editor = new Battle.DebugSetEffects(battle, :side, 1);
		editor.update;
		editor.dispose;
	}
});

MenuHandlers.add(:battle_debug_menu, :position_effects, {
	"name"        => _INTL("Battler position effects..."),
	"parent"      => :field,
	"description" => _INTL("Effects that apply to individual battler positions."),
	"effect"      => battle => {
		positions = new List<string>();
		cmds = new List<string>();
		battle.positions.each_with_index do |position, i|
			if (!position) continue;
			positions.Add(i);
			battler = battle.battlers[i];
			if (battler && !battler.fainted()) {
				text = $"[{i}] {battler.name}";
			} else {
				text = $"[{i}] " + _INTL("(empty)");
			}
			if (battler.OwnedByPlayer()) {
				text += " " + _INTL("(yours)");
			} else if (battle.opposes(i)) {
				text += " " + _INTL("(opposing)");
			} else {
				text += " " + _INTL("(ally's)");
			}
			cmds.Add(text);
		}
		cmd = 0;
		do { //loop; while (true);
			cmd = Message("\\ts[]" + _INTL("Choose a battler position."), cmds, -1, null, cmd);
			if (cmd < 0) break;
			editor = new Battle.DebugSetEffects(battle, :position, positions[cmd]);
			editor.update;
			editor.dispose;
		}
	}
});
