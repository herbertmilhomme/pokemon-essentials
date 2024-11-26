//===============================================================================
// HP/Status options.
//===============================================================================

MenuHandlers.add(:battle_pokemon_debug_menu, :hp_status_menu, {
	"name"   => _INTL("HP/status..."),
	"parent" => :main,
	"usage"  => :both;
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_hp, {
	"name"   => _INTL("Set HP"),
	"parent" => :hp_status_menu,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		if (pkmn.egg()) {
			Message("\\ts[]" + _INTL("{1} is an egg.", pkmn.name));
			continue;
		} else if (battler && pkmn.totalhp == 1) {
			Message("\\ts[]" + _INTL("Can't change HP, {1}'s maximum HP is 1 and it's in battle.", pkmn.name));
			continue;
		}
		min_hp = (battler) ? 1 : 0;
		params = new ChooseNumberParams();
		params.setRange(min_hp, pkmn.totalhp);
		params.setDefaultValue(pkmn.hp);
		new_hp = MessageChooseNumber(
			"\\ts[]" + _INTL("Set {1}'s HP ({2}-{3}).", (battler) ? battler.ToString(true) : pkmn.name, min_hp, pkmn.totalhp),
			params
		);
		if (new_hp == pkmn.hp) continue;
		(battler || pkmn).hp = new_hp
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_status, {
	"name"   => _INTL("Set status"),
	"parent" => :hp_status_menu,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		if (pkmn.egg()) {
			Message("\\ts[]" + _INTL("{1} is an egg.", pkmn.name));
			continue;
		} else if (pkmn.hp <= 0) {
			Message("\\ts[]" + _INTL("{1} is fainted, can't change status.", pkmn.name));
			continue;
		}
		cmd = 0;
		commands = [_INTL("[Cure]")];
		ids = [:NONE];
		foreach (var s in GameData.Status) { //'GameData.Status.each' do => |s|
			if (s.id == :NONE) continue;
			commands.Add(_INTL("Set {1}", s.name));
			ids.Add(s.id);
		}
		do { //loop; while (true);
			msg = _INTL("Current status: {1}", GameData.Status.get(pkmn.status).name);
			if (pkmn.status == statuses.SLEEP) {
				msg += " " + _INTL("(turns: {1})", pkmn.statusCount);
			} else if (pkmn.status == statuses.POISON && pkmn.statusCount > 0) {
				if (battler) {
					msg += " " + _INTL("(toxic, count: {1})", battler.effects.Toxic);
				} else {
					msg += " " + _INTL("(toxic)");
				}
			}
			cmd = Message("\\ts[]" + msg, commands, -1, null, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Cure
					if (battler) {
						battler.status = :NONE;
					} else {
						pkmn.heal_status;
					}
				default:   // Give status problem
					pkmn_name = (battler) ? battler.ToString(true) : pkmn.name;
					switch (ids[cmd]) {
						case :SLEEP:
							params = new ChooseNumberParams();
							params.setRange(0, 99);
							params.setDefaultValue((pkmn.status == statuses.SLEEP) ? pkmn.statusCount : 3);
							params.setCancelValue(-1);
							count = MessageChooseNumber("\\ts[]" + _INTL("Set {1}'s sleep count (0-99).", pkmn_name), params);
							if (count < 0) continue;
							(battler || pkmn).statusCount = count
							break;
						case :POISON:
							if (ConfirmMessage("\\ts[]" + _INTL("Make {1} badly poisoned (toxic)?", pkmn_name))) {
								if (battler) {
									params = new ChooseNumberParams();
									params.setRange(0, 16);
									params.setDefaultValue(battler.effects.Toxic);
									params.setCancelValue(-1);
									count = MessageChooseNumber(
										"\\ts[]" + _INTL("Set {1}'s toxic count (0-16).", pkmn_name), params
									);
									if (count < 0) continue;
									battler.statusCount = 1;
									battler.effects.Toxic = count;
								} else {
									pkmn.statusCount = 1;
								}
							} else {
								(battler || pkmn).statusCount = 0
							}
							break;
					}
					(battler || pkmn).status = ids[cmd]
					break;
			}
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :full_heal, {
	"name"   => _INTL("Heal HP and status"),
	"parent" => :hp_status_menu,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		if (pkmn.egg()) {
			Message("\\ts[]" + _INTL("{1} is an egg.", pkmn.name));
			continue;
		}
		if (battler) {
			battler.hp = battler.totalhp;
			battler.status = :NONE;
		} else {
			pkmn.heal_HP;
			pkmn.heal_status;
		}
	}
});

//===============================================================================
// Level/stats options.
//===============================================================================

MenuHandlers.add(:battle_pokemon_debug_menu, :level_stats, {
	"name"   => _INTL("Stats/level..."),
	"parent" => :main,
	"usage"  => :both;
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_stat_stages, {
	"name"   => _INTL("Set stat stages"),
	"parent" => :level_stats,
	"usage"  => :battler,
	"effect" => (pkmn, battler, battle) => {
		if (pkmn.egg()) {
			Message("\\ts[]" + _INTL("{1} is an egg.", pkmn.name));
			continue;
		}
		cmd = 0;
		do { //loop; while (true);
			commands = new List<string>();
			stat_ids = new List<string>();
			foreach (var stat in GameData.Stat) { //GameData.Stat.each_battle do => |stat|
				command_name = stat.name + ": ";
				if (battler.stages[stat.id] > 0) command_name += "+";
				command_name += battler.stages[stat.id].ToString();
				commands.Add(command_name);
				stat_ids.Add(stat.id);
			}
			commands.Add(_INTL("[Reset all]"));
			cmd = Message("\\ts[]" + _INTL("Choose a stat stage to change."), commands, -1, null, cmd);
			if (cmd < 0) break;
			if (cmd < stat_ids.length) {   // Set a stat
				params = new ChooseNumberParams();
				params.setRange(-Battle.Battler.STAT_STAGE_MAXIMUM, Battle.Battler.STAT_STAGE_MAXIMUM);
				params.setNegativesAllowed(true);
				params.setDefaultValue(battler.stages[stat_ids[cmd]]);
				value = MessageChooseNumber(
					"\\ts[]" + _INTL("Set the stage for {1}.", GameData.Stat.get(stat_ids[cmd]).name), params
				);
				battler.stages[stat_ids[cmd]] = value;
			} else {   // Reset all stats
				GameData.Stat.each_battle(stat => battler.stages[stat.id] = 0);
			}
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_stat_values, {
	"name"   => _INTL("Set stat values"),
	"parent" => :level_stats,
	"usage"  => :battler,
	"effect" => (pkmn, battler, battle) => {
		if (pkmn.egg()) {
			Message("\\ts[]" + _INTL("{1} is an egg.", pkmn.name));
			continue;
		}
		stat_ids = new List<string>();
		stat_vals = new List<string>();
		foreach (var stat in GameData.Stat) { //GameData.Stat.each_main_battle do => |stat|
			stat_ids.Add(stat.id);
			switch (stat.id) {
				case :ATTACK:           stat_vals.Add(battler.attack); break;
				case :DEFENSE:          stat_vals.Add(battler.defense); break;
				case :SPECIAL_ATTACK:   stat_vals.Add(battler.spatk); break;
				case :SPECIAL_DEFENSE:  stat_vals.Add(battler.spdef); break;
				case :SPEED:            stat_vals.Add(battler.speed); break;
				default:                       stat_vals.Add(1); break;
			}
		}
		cmd = 0;
		do { //loop; while (true);
			commands = new List<string>();
			foreach (var stat in GameData.Stat) { //GameData.Stat.each_main_battle do => |stat|
				command_name = stat.name + ": " + stat_vals[stat_ids.index(stat.id)].ToString();
				commands.Add(command_name);
			}
			commands.Add(_INTL("[Reset all]"));
			cmd = Message("\\ts[]" + _INTL("Choose a stat value to change."), commands, -1, null, cmd);
			if (cmd < 0) break;
			if (cmd < stat_ids.length) {   // Set a stat
				params = new ChooseNumberParams();
				params.setRange(1, 9999);
				params.setDefaultValue(stat_vals[cmd]);
				value = MessageChooseNumber(
					"\\ts[]" + _INTL("Set the value for {1}.", GameData.Stat.get(stat_ids[cmd]).name), params
				);
				switch (stat_ids[cmd]) {
					case :ATTACK:           battler.attack  = value; break;
					case :DEFENSE:          battler.defense = value; break;
					case :SPECIAL_ATTACK:   battler.spatk   = value; break;
					case :SPECIAL_DEFENSE:  battler.spdef   = value; break;
					case :SPEED:            battler.speed   = value; break;
				}
				stat_vals[cmd] = value;
			} else {   // Reset all stat values
				battler.Update;
				foreach (var stat in GameData.Stat) { //GameData.Stat.each_main_battle do => |stat|
					switch (stat.id) {
						case :ATTACK:           stat_vals[stat_ids.index(stat.id)] = battler.attack; break;
						case :DEFENSE:          stat_vals[stat_ids.index(stat.id)] = battler.defense; break;
						case :SPECIAL_ATTACK:   stat_vals[stat_ids.index(stat.id)] = battler.spatk; break;
						case :SPECIAL_DEFENSE:  stat_vals[stat_ids.index(stat.id)] = battler.spdef; break;
						case :SPEED:            stat_vals[stat_ids.index(stat.id)] = battler.speed; break;
						default:                       stat_vals[stat_ids.index(stat.id)] = 1; break;
					}
				}
			}
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_level, {
	"name"   => _INTL("Set level"),
	"parent" => :level_stats,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		if (pkmn.egg()) {
			Message("\\ts[]" + _INTL("{1} is an egg.", pkmn.name));
			continue;
		}
		params = new ChooseNumberParams();
		params.setRange(1, GameData.GrowthRate.max_level);
		params.setDefaultValue(pkmn.level);
		level = MessageChooseNumber(
			"\\ts[]" + _INTL("Set the Pokémon's level (max. {1}).", params.maxNumber), params
		);
		if (level != pkmn.level) {
			pkmn.level = level;
			pkmn.calc_stats;
			battler&.Update;
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_exp, {
	"name"   => _INTL("Set Exp"),
	"parent" => :level_stats,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		if (pkmn.egg()) {
			Message("\\ts[]" + _INTL("{1} is an egg.", pkmn.name));
			continue;
		}
		min_exp = pkmn.growth_rate.minimum_exp_for_level(pkmn.level);
		max_exp = pkmn.growth_rate.minimum_exp_for_level(pkmn.level + 1);
		if (min_exp == max_exp) {
			Message("\\ts[]" + _INTL("{1} is at the maximum level.", pkmn.name));
			continue;
		}
		params = new ChooseNumberParams();
		params.setRange(min_exp, max_exp - 1);
		params.setDefaultValue(pkmn.exp);
		new_exp = MessageChooseNumber(
			"\\ts[]" + _INTL("Set the Pokémon's Exp (range {1}-{2}).", min_exp, max_exp - 1), params
		);
		if (new_exp != pkmn.exp) pkmn.exp = new_exp;
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :hidden_values, {
	"name"   => _INTL("EV/IV..."),
	"parent" => :level_stats,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		cmd = 0;
		do { //loop; while (true);
			cmd = Message("\\ts[]" + _INTL("Choose hidden values to edit."),
											new {_INTL("Set EVs"), _INTL("Set IVs")}, -1, null, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Set EVs
					cmd2 = 0;
					do { //loop; while (true);
						total_evs = 0;
						ev_commands = new List<string>();
						ev_id = new List<string>();
						foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
							ev_commands.Add(s.name + $" ({pkmn.ev[s.id]})");
							ev_id.Add(s.id);
							total_evs += pkmn.ev[s.id];
						}
						ev_commands.Add(_INTL("Randomise all"));
						ev_commands.Add(_INTL("Max randomise all"));
						cmd2 = Message("\\ts[]" + _INTL("Change which EV?\nTotal: {1}/{2} ({3}%)",
																							total_evs, Pokemon.EV_LIMIT, 100 * total_evs / Pokemon.EV_LIMIT),
														ev_commands, -1, null, cmd2);
						if (cmd2 < 0) break;
						if (cmd2 < ev_id.length) {
							params = new ChooseNumberParams();
							upperLimit = 0;
							GameData.Stat.each_main(s => { if (s.id != ev_id[cmd2]) upperLimit += pkmn.ev[s.id]; });
							upperLimit = Pokemon.EV_LIMIT - upperLimit;
							upperLimit = (int)Math.Min(upperLimit, Pokemon.EV_STAT_LIMIT);
							thisValue = (int)Math.Min(pkmn.ev[ev_id[cmd2]], upperLimit);
							params.setRange(0, upperLimit);
							params.setDefaultValue(thisValue);
							params.setCancelValue(thisValue);
							f = MessageChooseNumber("\\ts[]" + _INTL("Set the EV for {1} (max. {2}).",
																												GameData.Stat.get(ev_id[cmd2]).name, upperLimit), params);
							if (f != pkmn.ev[ev_id[cmd2]]) {
								pkmn.ev[ev_id[cmd2]] = f;
								pkmn.calc_stats;
								battler&.Update;
							}
						} else {   // (Max) Randomise all
							evTotalTarget = Pokemon.EV_LIMIT;
							if (cmd2 == ev_commands.length - 2) {   // Randomize all (not max)
								evTotalTarget = rand(Pokemon.EV_LIMIT);
							}
							GameData.Stat.each_main(s => pkmn.ev[s.id] = 0);
							while (evTotalTarget > 0) {
								r = rand(ev_id.length);
								if (pkmn.ev[ev_id[r]] >= Pokemon.EV_STAT_LIMIT) continue;
								addVal = 1 + rand(Pokemon.EV_STAT_LIMIT / 4);
								addVal = addVal.clamp(0, evTotalTarget);
								addVal = addVal.clamp(0, Pokemon.EV_STAT_LIMIT - pkmn.ev[ev_id[r]]);
								if (addVal == 0) continue;
								pkmn.ev[ev_id[r]] += addVal;
								evTotalTarget -= addVal;
							}
							pkmn.calc_stats;
							battler&.Update;
						}
					}
					break;
				case 1:   // Set IVs
					cmd2 = 0;
					do { //loop; while (true);
						hiddenpower = HiddenPower(pkmn);
						totaliv = 0;
						ivcommands = new List<string>();
						iv_id = new List<string>();
						foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
							ivcommands.Add(s.name + $" ({pkmn.iv[s.id]})");
							iv_id.Add(s.id);
							totaliv += pkmn.iv[s.id];
						}
						msg = _INTL("Change which IV?\nHidden Power: {1}, power {2}\nTotal: {3}/{4} ({5}%)",
												GameData.Type.get(hiddenpower[0]).name, hiddenpower[1], totaliv,
												iv_id.length * Pokemon.IV_STAT_LIMIT, 100 * totaliv / (iv_id.length * Pokemon.IV_STAT_LIMIT));
						ivcommands.Add(_INTL("Randomise all"));
						cmd2 = Message("\\ts[]\\l[3]" + msg, ivcommands, -1, null, cmd2);
						if (cmd2 < 0) break;
						if (cmd2 < iv_id.length) {
							params = new ChooseNumberParams();
							params.setRange(0, Pokemon.IV_STAT_LIMIT);
							params.setDefaultValue(pkmn.iv[iv_id[cmd2]]);
							params.setCancelValue(pkmn.iv[iv_id[cmd2]]);
							f = MessageChooseNumber("\\ts[]" + _INTL("Set the IV for {1} (max. 31).",
																												GameData.Stat.get(iv_id[cmd2]).name), params);
							if (f != pkmn.iv[iv_id[cmd2]]) {
								pkmn.iv[iv_id[cmd2]] = f;
								pkmn.calc_stats;
								battler&.Update;
							}
						} else {   // Randomise all
							GameData.Stat.each_main(s => pkmn.iv[s.id] = rand(Pokemon.IV_STAT_LIMIT + 1));
							pkmn.calc_stats;
							battler&.Update;
						}
					}
					break;
			}
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_happiness, {
	"name"   => _INTL("Set happiness"),
	"parent" => :level_stats,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		params = new ChooseNumberParams();
		params.setRange(0, 255);
		params.setDefaultValue(pkmn.happiness);
		h = MessageChooseNumber("\\ts[]" + _INTL("Set the Pokémon's happiness (max. 255)."), params);
		if (h != pkmn.happiness) pkmn.happiness = h;
	}
});

//===============================================================================
// Types.
//===============================================================================

MenuHandlers.add(:battle_pokemon_debug_menu, :set_types, {
	"name"   => _INTL("Set types"),
	"parent" => :main,
	"usage"  => :battler,
	"effect" => (pkmn, battler, battle) => {
		max_main_types = 5;   // Arbitrary value, could be any number
		cmd = 0;
		do { //loop; while (true);
			commands = new List<string>();
			types = new List<string>();
			for (int i = max_main_types; i < max_main_types; i++) { //for 'max_main_types' times do => |i|
				type = battler.types[i];
				type_name = (type) ? GameData.Type.get(type).name : "-";
				commands.Add(_INTL("Type {1}: {2}", i + 1, type_name));
				types.Add(type);
			}
			extra_type = battler.effects.ExtraType;
			extra_type_name = (extra_type) ? GameData.Type.get(extra_type).name : "-";
			commands.Add(_INTL("Extra type: {1}", extra_type_name));
			types.Add(extra_type);
			msg = _INTL("Effective types: {1}", battler.Types(true).map(t => GameData.Type.get(t).name).join("/"));
			msg += "\n" + _INTL("(Change a type to itself to remove it.)");
			cmd = Message("\\ts[]" + msg, commands, -1, null, cmd);
			if (cmd < 0) break;
			old_type = types[cmd];
			new_type = ChooseTypeList(old_type);
			if (new_type) {
				if (new_type == old_type) {
					if (ConfirmMessage(_INTL("Remove this type?"))) {
						if (cmd < max_main_types) {
							battler.types[cmd] = null;
						} else {
							battler.effects.ExtraType = null;
						}
						battler.types.compact!;
					}
				} else if (cmd < max_main_types) {
					battler.types[cmd] = new_type;
				} else {
					battler.effects.ExtraType = new_type;
				}
			}
		}
	}
});

//===============================================================================
// Moves options.
//===============================================================================

MenuHandlers.add(:battle_pokemon_debug_menu, :moves, {
	"name"   => _INTL("Moves..."),
	"parent" => :main,
	"usage"  => :both;
});

MenuHandlers.add(:battle_pokemon_debug_menu, :teach_move, {
	"name"   => _INTL("Teach move"),
	"parent" => :moves,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		if (pkmn.numMoves >= Pokemon.MAX_MOVES) {
			Message("\\ts[]" + _INTL("{1} already knows {2} moves. It needs to forget one first.",
																pkmn.name, pkmn.numMoves));
			continue;
		}
		new_move = ChooseMoveList;
		if (!new_move) continue;
		move_name = GameData.Move.get(new_move).name;
		if (pkmn.hasMove(new_move)) {
			Message("\\ts[]" + _INTL("{1} already knows {2}.", pkmn.name, move_name));
			continue;
		}
		pkmn.learn_move(new_move);
		battler&.moves&.Add(Battle.Move.from_pokemon_move(battle, pkmn.moves.last));
		Message("\\ts[]" + _INTL("{1} learned {2}!", pkmn.name, move_name));
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :forget_move, {
	"name"   => _INTL("Forget move"),
	"parent" => :moves,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		move_names = new List<string>();
		move_indices = new List<string>();
		pkmn.moves.each_with_index do |move, index|
			if (!move || !move.id) continue;
			if (move.total_pp <= 0) {
				move_names.Add(_INTL("{1} (PP: ---)", move.name));
			} else {
				move_names.Add(_INTL("{1} (PP: {2}/{3})", move.name, move.pp, move.total_pp));
			}
			move_indices.Add(index);
		}
		cmd = Message("\\ts[]" + _INTL("Forget which move?"), move_names, -1);
		if (cmd < 0) continue;
		old_move_name = pkmn.moves[move_indices[cmd]].name;
		pkmn.forget_move_at_index(move_indices[cmd]);
		battler&.moves&.delete_at(move_indices[cmd]);
		Message("\\ts[]" + _INTL("{1} forgot {2}.", pkmn.name, old_move_name));
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_move_pp, {
	"name"   => _INTL("Set move PP"),
	"parent" => :moves,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		cmd = 0;
		do { //loop; while (true);
			move_names = new List<string>();
			move_indices = new List<string>();
			pkmn.moves.each_with_index do |move, index|
				if (!move || !move.id) continue;
				if (move.total_pp <= 0) {
					move_names.Add(_INTL("{1} (PP: ---)", move.name));
				} else {
					move_names.Add(_INTL("{1} (PP: {2}/{3})", move.name, move.pp, move.total_pp));
				}
				move_indices.Add(index);
			}
			commands = move_names + [_INTL("Restore all PP")];
			cmd = Message("\\ts[]" + _INTL("Alter PP of which move?"), commands, -1, null, cmd);
			if (cmd < 0) break;
			if (cmd >= 0 && cmd < move_names.length) {   // Move
				move = pkmn.moves[move_indices[cmd]];
				move_name = move.name;
				if (move.total_pp <= 0) {
					Message("\\ts[]" + _INTL("{1} has infinite PP.", move_name));
				} else {
					cmd2 = 0;
					do { //loop; while (true);
						msg = _INTL("{1}: PP {2}/{3} (PP Up {4}/3)", move_name, move.pp, move.total_pp, move.ppup);
						cmd2 = Message("\\ts[]" + msg,
														new {_INTL("Set PP"), _INTL("Full PP"), _INTL("Set PP Up")}, -1, null, cmd2);
						if (cmd2 < 0) break;
						switch (cmd2) {
							case 0:   // Change PP
								params = new ChooseNumberParams();
								params.setRange(0, move.total_pp);
								params.setDefaultValue(move.pp);
								h = MessageChooseNumber(
									"\\ts[]" + _INTL("Set PP of {1} (max. {2}).", move_name, move.total_pp), params
								);
								move.pp = h;
								if (battler && battler.moves[move_indices[cmd]].id == move.id) {
									battler.moves[move_indices[cmd]].pp = move.pp;
								}
								break;
							case 1:   // Full PP
								move.pp = move.total_pp;
								if (battler && battler.moves[move_indices[cmd]].id == move.id) {
									battler.moves[move_indices[cmd]].pp = move.pp;
								}
								break;
							case 2:   // Change PP Up
								params = new ChooseNumberParams();
								params.setRange(0, 3);
								params.setDefaultValue(move.ppup);
								h = MessageChooseNumber(
									"\\ts[]" + _INTL("Set PP Up of {1} (max. 3).", move_name), params
								);
								move.ppup = h;
								if (move.pp > move.total_pp) move.pp = move.total_pp;
								if (battler && battler.moves[move_indices[cmd]].id == move.id) {
									battler.moves[move_indices[cmd]].pp = move.pp;
								}
								break;
						}
					}
				}
			} else if (cmd == commands.length - 1) {   // Restore all PP
				pkmn.heal_PP;
				if (battler) {
					battler.moves.each(m => m.pp = m.total_pp);
				}
			}
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :reset_moves, {
	"name"   => _INTL("Reset moves"),
	"parent" => :moves,
	"usage"  => :pokemon,
	"effect" => (pkmn, battler, battle) => {
		if (!ConfirmMessage(_INTL("Replace Pokémon's moves with ones it would know;) if (it was wild?"))) continue;
		pkmn.reset_moves;
		Message("\\ts[]" + _INTL("{1}'s moves were reset.", pkmn.name));
	}
});

//===============================================================================
// Other options.
//===============================================================================

MenuHandlers.add(:battle_pokemon_debug_menu, :set_item, {
	"name"   => _INTL("Set item"),
	"parent" => :main,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		cmd = 0;
		commands = new {
			_INTL("Change item"),
			_INTL("Remove item");
		}
		do { //loop; while (true);
			msg = (pkmn.hasItem()) ? _INTL("Item is {1}.", pkmn.item.name) : _INTL("No item.");
			cmd = Message("\\ts[]" + msg, commands, -1, null, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Change item
					item = ChooseItemList(pkmn.item_id);
					if (item && item != pkmn.item_id) {
						(battler || pkmn).item = item
						if (GameData.Item.get(item).is_mail()) {
							pkmn.mail = new Mail(item, _INTL("Text"), Game.GameData.player.name);
						}
					}
					break;
				case 1:   // Remove item
					if (pkmn.hasItem()) {
						(battler || pkmn).item = null
						pkmn.mail = null;
					}
					break;
				default:
					break;
					break;
			}
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_ability, {
	"name"   => _INTL("Set ability"),
	"parent" => :main,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		cmd = 0;
		commands = new List<string>();
		commands.Add(_INTL("Set ability for Pokémon"));
		if (battler) commands.Add(_INTL("Set ability for battler"));
		commands.Add(_INTL("Reset"));
		do { //loop; while (true);
			if (battler) {
				msg = _INTL("Battler's ability is {1}. Pokémon's ability is {2}.",
										battler.abilityName, pkmn.ability.name);
			} else {
				msg = _INTL("Pokémon's ability is {1}.", pkmn.ability.name);
			}
			cmd = Message("\\ts[]" + msg, commands, -1, null, cmd);
			if (cmd < 0) break;
			if (cmd >= 1 && !battler) cmd = 2;   // Correct command for Pokémon (no battler)
			switch (cmd) {
				case 0:   // Set ability for Pokémon
					new_ability = ChooseAbilityList(pkmn.ability_id);
					if (new_ability && new_ability != pkmn.ability_id) {
						pkmn.ability = new_ability;
						if (battler) battler.ability = pkmn.ability;
					}
					break;
				case 1:   // Set ability for battler
					if (battler) {
						new_ability = ChooseAbilityList(battler.ability_id);
						if (new_ability && new_ability != battler.ability_id) {
							battler.ability = new_ability;
						}
					} else {
						Message(_INTL("This Pokémon isn't in battle."));
					}
					break;
				case 2:   // Reset
					pkmn.ability_index = null;
					pkmn.ability = null;
					if (battler) battler.ability = pkmn.ability;
					break;
			}
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_nature, {
	"name"   => _INTL("Set nature"),
	"parent" => :main,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		commands = new List<string>();
		ids = new List<string>();
		foreach (var nature in GameData.Nature) { //'GameData.Nature.each' do => |nature|
			if (nature.stat_changes.length == 0) {
				commands.Add(_INTL("{1} (---)", nature.real_name));
			} else {
				plus_text = "";
				minus_text = "";
				foreach (var change in nature.stat_changes) { //'nature.stat_changes.each' do => |change|
					if (change[1] > 0) {
						if (!plus_text.empty()) plus_text += "/";
						plus_text += GameData.Stat.get(change[0]).name_brief;
					} else if (change[1] < 0) {
						if (!minus_text.empty()) minus_text += "/";
						minus_text += GameData.Stat.get(change[0]).name_brief;
					}
				}
				commands.Add(_INTL("{1} (+{2}, -{3})", nature.real_name, plus_text, minus_text));
			}
			ids.Add(nature.id);
		}
		commands.Add(_INTL("[Reset]"));
		cmd = ids.index(pkmn.nature_id || ids[0]);
		do { //loop; while (true);
			msg = _INTL("Nature is {1}.", pkmn.nature.name);
			cmd = Message("\\ts[]" + msg, commands, -1, null, cmd);
			if (cmd < 0) break;
			if (cmd >= 0 && cmd < commands.length - 1) {   // Set nature
				pkmn.nature = ids[cmd];
			} else if (cmd == commands.length - 1) {   // Reset
				pkmn.nature = null;
			}
			battler&.Update;
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_gender, {
	"name"   => _INTL("Set gender"),
	"parent" => :main,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		if (pkmn.singleGendered()) {
			Message("\\ts[]" + _INTL("{1} is single-gendered or genderless.", pkmn.speciesName));
			continue;
		}
		cmd = 0;
		do { //loop; while (true);
			msg = new {_INTL("Gender is male."), _INTL("Gender is female.")}[pkmn.male() ? 0 : 1];
			cmd = Message("\\ts[]" + msg,
											new {_INTL("Make male"), _INTL("Make female"), _INTL("Reset")}, -1, null, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Make male
					pkmn.makeMale;
					if (!pkmn.male()) Message("\\ts[]" + _INTL("{1}'s gender couldn't be changed.", pkmn.name));
					break;
				case 1:   // Make female
					pkmn.makeFemale;
					if (!pkmn.female()) Message("\\ts[]" + _INTL("{1}'s gender couldn't be changed.", pkmn.name));
					break;
				case 2:   // Reset
					pkmn.gender = null;
					break;
			}
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_form, {
	"name"   => _INTL("Set form"),
	"parent" => :main,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		cmd = 0;
		formcmds = new {[], new List<string>()}
		foreach (var sp in GameData.Species) { //'GameData.Species.each' do => |sp|
			if (sp.species != pkmn.species) continue;
			form_name = sp.form_name
			if (!form_name || form_name.empty()) form_name = _INTL("Unnamed form");
			form_name = string.Format("{0}: {0}", sp.form, form_name)
			formcmds[0].Add(sp.form)
			formcmds[1].Add(form_name)
			if (pkmn.form == sp.form) cmd = formcmds[0].length - 1;
		}
		if (formcmds[0].length <= 1) {
			Message("\\ts[]" + _INTL("Species {1} only has one form.", pkmn.speciesName));
			continue;
		}
		do { //loop; while (true);
			cmd = Message("\\ts[]" + _INTL("Form is {1}.", pkmn.form), formcmds[1], -1, null, cmd);
			if (cmd < 0) break;
			f = formcmds[0][cmd];
			if (f == pkmn.form) continue;
			pkmn.forced_form = null;
			if (MultipleForms.hasFunction(pkmn, "getForm")) {
				if (!ConfirmMessage(_INTL("This species decides its own form. Override?"))) continue;
				pkmn.forced_form = f;
			}
			pkmn.form_simple = f;
			if (battler) battler.form = pkmn.form;
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_species, {
	"name"   => _INTL("Set species"),
	"parent" => :main,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		species = ChooseSpeciesList(pkmn.species);
		if (species && species != pkmn.species) {
			pkmn.species = species;
			if (battler) battler.species = species;
			battler&.Update(true);
			if (battler) battler.name = pkmn.name;
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_shininess, {
	"name"   => _INTL("Set shininess"),
	"parent" => :main,
	"usage"  => :both,
	"effect" => (pkmn, battler, battle) => {
		cmd = 0;
		do { //loop; while (true);
			msg_idx = pkmn.shiny() ? (pkmn.super_shiny() ? 1 : 0) : 2;
			msg = new {_INTL("Is shiny."), _INTL("Is super shiny."), _INTL("Is normal (not shiny).")}[msg_idx];
			cmd = Message("\\ts[]" + msg,
											new {_INTL("Make shiny"),
											_INTL("Make super shiny"),
											_INTL("Make normal"),
											_INTL("Reset")}, -1, null, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Make shiny
					pkmn.shiny = true;
					pkmn.super_shiny = false;
					break;
				case 1:   // Make super shiny
					pkmn.super_shiny = true;
					break;
				case 2:   // Make normal
					pkmn.shiny = false;
					pkmn.super_shiny = false;
					break;
				case 3:   // Reset
					pkmn.shiny = null;
					pkmn.super_shiny = null;
					break;
			}
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :shadow_pokemon, {
	"name"   => _INTL("Shadow Pokémon"),
	"parent" => :main,
	"usage"  => :battler,
	"effect" => (pkmn, battler, battle) => {
		if (battler.shadowPokemon()) {
			do { //loop; while (true);
				if (battler.inHyperMode()) {
					msg = _INTL("Shadow Pokémon (in Hyper Mode)");
				} else {
					msg = _INTL("Shadow Pokémon (not in Hyper Mode)");
				}
				cmd = Message("\\ts[]" + msg, new {_INTL("Toggle Hyper Mode"), _INTL("Cancel")}, -1, null, 0);
				if (cmd != 0) break;
				if (battler.inHyperMode()) {
					pkmn.hyper_mode = false;
				} else if (battler.fainted() || !battler.OwnedByPlayer()) {
					Message("\\ts[]" + _INTL("Pokémon is fainted or not the player's. Can't put it in Hyper Mode."));
				} else {
					pkmn.hyper_mode = true;
				}
			}
		} else {
			Message("\\ts[]" + _INTL("Pokémon is not a Shadow Pokémon."));
		}
	}
});

MenuHandlers.add(:battle_pokemon_debug_menu, :set_effects, {
	"name"   => _INTL("Set effects"),
	"parent" => :main,
	"usage"  => :battler,
	"effect" => (pkmn, battler, battle) => {
		editor = new Battle.DebugSetEffects(battle, :battler, battler.index);
		editor.update;
		editor.dispose;
	}
});
