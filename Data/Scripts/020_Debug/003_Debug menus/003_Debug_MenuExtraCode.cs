//===============================================================================
//
//===============================================================================
public void DefaultMap() {
	if (Game.GameData.game_map) return Game.GameData.game_map.map_id;
	if (Game.GameData.data_system) return Game.GameData.data_system.edit_map_id;
	return 0;
}

public void WarpToMap() {
	mapid = ListScreen(_INTL("WARP TO MAP"), new MapLister(DefaultMap));
	if (mapid > 0) {
		map = new Game_Map();
		map.setup(mapid);
		success = false;
		x = 0;
		y = 0;
		100.times do;
			x = rand(map.width);
			y = rand(map.height);
			if (!map.passableStrict(x, y, 0, Game.GameData.game_player)) continue;
			blocked = false;
			foreach (var event in map.events) { //map.events.each_value do => |event|
				if (event.at_coordinate(x, y) && !event.through && event.character_name != "") {
					blocked = true;
				}
			}
			if (blocked) continue;
			success = true;
			break;
		}
		if (!success) {
			x = rand(map.width);
			y = rand(map.height);
		}
		return new {mapid, x, y};
	}
	return null;
}

//===============================================================================
// Debug Variables screen.
//===============================================================================
public partial class SpriteWindow_DebugVariables : Window_DrawableCommand {
	public int mode		{ get { return _mode; } }			protected int _mode;

	public override void initialize(viewport) {
		base.initialize(0, 0, Graphics.width, Graphics.height, viewport);
	}

	public void itemCount() {
		return (@mode == 0) ? Game.GameData.data_system.switches.size - 1 : Game.GameData.data_system.variables.size - 1;
	}

	public int mode { set {
		@mode = mode;
		refresh;
		}
	}

	public void shadowtext(x, y, w, h, t, align = 0, colors = 0) {
		width = self.contents.text_size(t).width;
		switch (align) {
			case 1:   // Right aligned
				x += (w - width);
				break;
			case 2:   // Centre aligned
				x += (w / 2) - (width / 2);
				break;
		}
		y += 8;   // TEXT OFFSET
		base = new Color(96, 96, 96);
		switch (colors) {
			case 1:   // Red
				base = new Color(168, 48, 56);
				break;
			case 2:   // Green
				base = new Color(0, 144, 0);
				break;
		}
		DrawShadowText(self.contents, x, y, (int)Math.Max(width, w), h, t, base, new Color(208, 208, 200));
	}

	public void drawItem(index, _count, rect) {
		SetNarrowFont(self.contents);
		colors = 0;
		codeswitch = false;
		if (@mode == 0) {
			name = Game.GameData.data_system.switches[index + 1];
			codeswitch = (System.Text.RegularExpressions.Regex.IsMatch(name,@"^s\:"));
			if (codeswitch) {
				code = $~.post_match;
				code_parts = code.split(/[(\[=<>. ]/);
				code_parts[0].strip!;
				code_parts[0] = System.Text.RegularExpressions.Regex.Replace(code_parts[0], "^\s*!", "");
				val = null;
				if (System.Text.RegularExpressions.Regex.IsMatch(code_parts[0][0],@"[a-z]",RegexOptions.IgnoreCase)) {
					if (code_parts[0][0].upcase == code_parts[0][0] &&
						(Kernel.const_defined(code_parts[0]) rescue false)) {
						val = (eval(code) rescue null);   // Code starts with a class/method name
					} else if (code_parts[0][0].downcase == code_parts[0][0] &&
								!(Interpreter.method_defined(code_parts[0].to_sym) rescue false) &&
								!(Game_Event.method_defined(code_parts[0].to_sym) rescue false)) {
						val = (eval(code) rescue null);   // Code starts with a method name (that isn't in Interpreter/Game_Event)
					}
				} else {
					// Code doesn't start with a letter, probably $, just evaluate it
					val = (eval(code) rescue null);
				}
			} else {
				val = Game.GameData.game_switches[index + 1];
			}
			if (val.null()) {
				status = "[-]";
				colors = 0;
				codeswitch = true;
			} else if (val) {   // true
				status = "[ON]";
				colors = 2;
			} else {   // false
				status = "[OFF]";
				colors = 1;
			}
		} else {
			name = Game.GameData.data_system.variables[index + 1];
			status = Game.GameData.game_variables[index + 1].ToString();
			if (nil_or_empty(status)) status = "\"__\"";
		}
		name ||= "";
		id_text = string.Format("{0:4}:", index + 1);
		rect = drawCursor(index, rect);
		totalWidth = rect.width;
		idWidth     = totalWidth * 15 / 100;
		nameWidth   = totalWidth * 65 / 100;
		statusWidth = totalWidth * 20 / 100;
		self.shadowtext(rect.x, rect.y, idWidth, rect.height, id_text);
		self.shadowtext(rect.x + idWidth, rect.y, nameWidth, rect.height, name, 0, (codeswitch) ? 1 : 0);
		self.shadowtext(rect.x + idWidth + nameWidth, rect.y, statusWidth, rect.height, status, 1, colors);
	}
}

//===============================================================================
//
//===============================================================================
public void DebugSetVariable(id, diff) {
	if (Game.GameData.game_variables[id].null()) Game.GameData.game_variables[id] = 0;
	if (Game.GameData.game_variables[id].is_a(Numeric)) {
		PlayCursorSE;
		Game.GameData.game_variables[id] = (int)Math.Min(Game.GameData.game_variables[id] + diff, 99_999_999);
		Game.GameData.game_variables[id] = (int)Math.Max(Game.GameData.game_variables[id], -99_999_999);
		Game.GameData.game_map.need_refresh = true;
	}
}

public void DebugVariableScreen(id) {
	switch (Game.GameData.game_variables[id]) {
		case Numeric:
			value = Game.GameData.game_variables[id];
			params = new ChooseNumberParams();
			params.setDefaultValue(value);
			params.setMaxDigits(8);
			params.setNegativesAllowed(true);
			value = MessageChooseNumber(_INTL("Set variable {1}.", id), params);
			Game.GameData.game_variables[id] = (int)Math.Min(value, 99_999_999);
			Game.GameData.game_variables[id] = (int)Math.Max(Game.GameData.game_variables[id], -99_999_999);
			Game.GameData.game_map.need_refresh = true;
			break;
		case String:
			value = MessageFreeText(_INTL("Set variable {1}.", id),
																Game.GameData.game_variables[id], false, 250, Graphics.width);
			Game.GameData.game_variables[id] = value;
			Game.GameData.game_map.need_refresh = true;
			break;
	}
}

public void DebugVariables(mode) {
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = 99999;
	sprites = new List<string>();
	sprites["right_window"] = new SpriteWindow_DebugVariables(viewport);
	right_window = sprites["right_window"];
	right_window.mode     = mode;
	right_window.active   = true;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		UpdateSpriteHash(sprites);
		if (Input.trigger(Input.BACK)) {
			PlayCancelSE
			break;
		}
		current_id = right_window.index + 1;
		switch (mode) {
			case 0:   // Switches
				if (Input.trigger(Input.USE)) {
					PlayDecisionSE;
					Game.GameData.game_switches[current_id] = !Game.GameData.game_switches[current_id];
					right_window.refresh;
					Game.GameData.game_map.need_refresh = true;
				}
				break;
			case 1:   // Variables
				if (Input.repeat(Input.LEFT)) {
					DebugSetVariable(current_id, -1);
					right_window.refresh;
				} else if (Input.repeat(Input.RIGHT)) {
					DebugSetVariable(current_id, 1);
					right_window.refresh;
				} else if (Input.trigger(Input.ACTION)) {
					switch (Game.GameData.game_variables[current_id]) {
						case 0:
							Game.GameData.game_variables[current_id] = "";
							break;
						case "":
							Game.GameData.game_variables[current_id] = 0;
							break;
						default:
							switch (Game.GameData.game_variables[current_id]) {
								case Numeric:
									Game.GameData.game_variables[current_id] = 0;
									break;
								case String:
									Game.GameData.game_variables[current_id] = "";
									break;
							}
							break;
					}
					right_window.refresh;
					Game.GameData.game_map.need_refresh = true;
				} else if (Input.trigger(Input.USE)) {
					PlayDecisionSE;
					DebugVariableScreen(current_id);
					right_window.refresh;
				}
				break;
		}
	}
	DisposeSpriteHash(sprites);
	viewport.dispose;
}

//===============================================================================
// Debug Day Care screen.
//===============================================================================
public void DebugDayCare() {
	day_care = Game.GameData.PokemonGlobal.day_care;
	cmd_window = Window_CommandPokemonEx.newEmpty(0, 0, Graphics.width, Graphics.height);
	commands = new List<string>();
	cmd = 0;
	compat = 0;
	need_refresh = true;
	do { //loop; while (true);
		if (need_refresh) {
			commands.clear;
			day_care.slots.each_with_index do |slot, i|
				if (slot.filled()) {
					pkmn = slot.pokemon;
					msg = _INTL("{1} ({2})", pkmn.name, pkmn.speciesName);
					if (pkmn.male()) {
						msg += ", ♂";
					} else if (pkmn.female()) {
						msg += ", ♀";
					}
					if (slot.level_gain > 0) {
						msg += ", " + _INTL("Lv.{1} (+{2})", pkmn.level, slot.level_gain);
					} else {
						msg += ", " + _INTL("Lv.{1}", pkmn.level);
					}
					commands.Add(_INTL("[Slot {1}] {2}", i, msg));
				} else {
					commands.Add(_INTL("[Slot {1}] Empty", i));
				}
			}
			compat = Game.GameData.PokemonGlobal.day_care.get_compatibility;
			if (day_care.egg_generated) {
				commands.Add(_INTL("[Egg available]"));
			} else if (compat > 0) {
				commands.Add(_INTL("[Can produce egg]"));
			} else {
				commands.Add(_INTL("[Cannot breed]"));
			}
			commands.Add(_INTL("[Steps to next cycle: {1}]", 256 - day_care.step_counter));
			cmd_window.commands = commands;
			need_refresh = false;
		}
		cmd = Commands2(cmd_window, commands, -1, cmd, true);
		if (cmd < 0) break;
		if (cmd == commands.length - 2) {   // Egg
			compat = Game.GameData.PokemonGlobal.day_care.get_compatibility;
			if (compat == 0) {
				Message(_INTL("Pokémon cannot breed."));
			} else {
				msg = _INTL("Pokémon can breed (compatibility = {1}).", compat);
				// Show compatibility
				if (day_care.egg_generated) {
					switch (Message("\\ts[]" + msg,) {
												new {_INTL("Collect egg"), _INTL("Clear egg"), _INTL("Cancel")}, 3);
					break;
					case 0:   // Collect egg
						if (Game.GameData.player.party_full()) {
							Message(_INTL("Party is full, can't collect the egg."));
						} else {
							DayCare.collect_egg;
							Message(_INTL("Collected the {1} egg.", Game.GameData.player.last_party.speciesName));
							need_refresh = true;
						}
					break;
					case 1:   // Clear egg
						day_care.egg_generated = false;
						need_refresh = true;
					}
				} else {
					switch (Message("\\ts[]" + msg, new {_INTL("Make egg available"), _INTL("Cancel")}, 2)) {
					break;
					case 0:   // Make egg available
						day_care.egg_generated = true;
						need_refresh = true;
					}
				}
			}
		} else if (cmd == commands.length - 1) {   // Steps to next cycle
			switch (Message("\\ts[]" + _INTL("Change number of steps to next cycle?"),) {
										new {_INTL("Set to 1"), _INTL("Set to 256"), _INTL("Set to other value"), _INTL("Cancel")}, 4);
			break;
			case 0:   // Set to 1
				day_care.step_counter = 255;
				need_refresh = true;
			break;
			case 1:   // Set to 256
				day_care.step_counter = 0;
				need_refresh = true;
			break;
			case 2:   // Set to other value
				params = new ChooseNumberParams();
				params.setDefaultValue(day_care.step_counter);
				params.setRange(1, 256);
				new_counter = MessageChooseNumber(_INTL("Set steps until next cycle (1-256)."), params);
				if (new_counter != 256 - day_care.step_counter) {
					day_care.step_counter = 256 - new_counter;
					need_refresh = true;
				}
			}
		} else {   // Slot
			slot = day_care[cmd];
			if (slot.filled()) {
				pkmn = slot.pokemon;
				msg = _INTL("Cost: ${1}", slot.cost);
				if (pkmn.level < GameData.GrowthRate.max_level) {
					end_exp = pkmn.growth_rate.minimum_exp_for_level(pkmn.level + 1);
					msg += "\n" + _INTL("Steps to next level: {1}", end_exp - pkmn.exp);
				}
				// Show level change and cost
				switch (Message("\\ts[]" + msg,) {
											new {_INTL("Summary"), _INTL("Withdraw"), _INTL("Cancel")}, 3);
				break;
				case 0:   // Summary
					FadeOutIn do;
						scene = new PokemonSummary_Scene();
						screen = new PokemonSummaryScreen(scene, false);
						screen.StartScreen([pkmn], 0);
						need_refresh = true;
					}
				break;
				case 1:   // Withdraw
					if (Game.GameData.player.party_full()) {
						Message(_INTL("Party is full, can't withdraw Pokémon."));
					} else {
						Game.GameData.player.party.Add(pkmn);
						slot.reset;
						day_care.reset_egg_counters;
						need_refresh = true;
					}
				}
			} else {
				switch (Message("\\ts[]" + _INTL("This slot is empty."),) {
											new {_INTL("Deposit"), _INTL("Cancel")}, 2);
				break;
				case 0:   // Deposit
					if (Game.GameData.player.party.empty()) {
						Message(_INTL("Party is empty, can't deposit Pokémon."));
					} else {
						ChooseNonEggPokemon(1, 3);
						party_index = Get(1);
						if (party_index >= 0) {
							pkmn = Game.GameData.player.party[party_index];
							slot.deposit(pkmn);
							Game.GameData.player.party.delete_at(party_index);
							day_care.reset_egg_counters;
							need_refresh = true;
						}
					}
				}
			}

		}
	}
	cmd_window.dispose;
}

//===============================================================================
// Debug roaming Pokémon screen.
//===============================================================================
public partial class SpriteWindow_DebugRoamers : Window_DrawableCommand {
	public override void initialize(viewport) {
		base.initialize(0, 0, Graphics.width, Graphics.height, viewport);
	}

	public void roamerCount() {
		return Settings.ROAMING_SPECIES.length;
	}

	public void itemCount() {
		return self.roamerCount + 2;
	}

	public void shadowtext(t, x, y, w, h, align = 0, colors = 0) {
		y += 8;   // TEXT OFFSET
		width = self.contents.text_size(t).width;
		switch (align) {
			case 1:
				x += (w - width);             // Right aligned
				break;
			case 2:
				x += (w / 2) - (width / 2);   // Centre aligned
				break;
		}
		base = new Color(96, 96, 96);
		switch (colors) {
			case 1:
				base = new Color(168, 48, 56);   // Red
				break;
			case 2:
				base = new Color(0, 144, 0);     // Green
				break;
		}
		DrawShadowText(self.contents, x, y, (int)Math.Max(width, w), h, t, base, new Color(208, 208, 200));
	}

	public void drawItem(index, _count, rect) {
		SetNarrowFont(self.contents);
		rect = drawCursor(index, rect);
		nameWidth   = rect.width * 50 / 100;
		statusWidth = rect.width * 50 / 100;
		if (index == self.itemCount - 2) {
			// Advance roaming
			self.shadowtext(_INTL("[All roam to new locations]"), rect.x, rect.y, nameWidth, rect.height);
		} else if (index == self.itemCount - 1) {
			// Advance roaming
			self.shadowtext(_INTL("[Clear all current roamer locations]"), rect.x, rect.y, nameWidth, rect.height);
		} else {
			pkmn = Settings.ROAMING_SPECIES[index];
			name = GameData.Species.get(pkmn[0]).name + $" (Lv. {pkmn[1]})";
			status = "";
			statuscolor = 0;
			if (pkmn[2] <= 0 || Game.GameData.game_switches[pkmn[2]]) {
				status = Game.GameData.PokemonGlobal.roamPokemon[index];
				if (status == true) {
					if (Game.GameData.PokemonGlobal.roamPokemonCaught[index]) {
						status = "[CAUGHT]";
					} else {
						status = "[DEFEATED]";
					}
					statuscolor = 1;
				} else {
					// roaming
					curmap = Game.GameData.PokemonGlobal.roamPosition[index];
					if (curmap) {
						mapinfos = LoadMapInfos;
						status = $"[ROAMING][{curmap}: {mapinfos[curmap].name}]";
					} else {
						status = "[ROAMING][map not set]";
					}
					statuscolor = 2;
				}
			} else {
				status = $"[NOT ROAMING][Switch {pkmn[2]} is off]";
			}
			self.shadowtext(name, rect.x, rect.y, nameWidth, rect.height);
			self.shadowtext(status, rect.x + nameWidth, rect.y, statusWidth, rect.height, 1, statuscolor);
		}
	}
}

//===============================================================================
//
//===============================================================================
public void DebugRoamers() {
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = 99999;
	sprites = new List<string>();
	sprites["cmdwindow"] = new SpriteWindow_DebugRoamers(viewport);
	cmdwindow = sprites["cmdwindow"];
	cmdwindow.active = true;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		UpdateSpriteHash(sprites);
		if (cmdwindow.index < cmdwindow.roamerCount) {
			pkmn = Settings.ROAMING_SPECIES[cmdwindow.index];
		} else {
			pkmn = null;
		}
		if (Input.trigger(Input.ACTION) && cmdwindow.index < cmdwindow.roamerCount &&
			(pkmn[2] <= 0 || Game.GameData.game_switches[pkmn[2]]) &&
			Game.GameData.PokemonGlobal.roamPokemon[cmdwindow.index] != true) {
			// Roam selected Pokémon
			PlayDecisionSE;
			if (Input.press(Input.CTRL)) {   // Roam to current map
				if (Game.GameData.PokemonGlobal.roamPosition[cmdwindow.index] == DefaultMap) {
					Game.GameData.PokemonGlobal.roamPosition[cmdwindow.index] = null;
				} else {
					Game.GameData.PokemonGlobal.roamPosition[cmdwindow.index] = DefaultMap;
				}
			} else {   // Roam to a random other map
				oldmap = Game.GameData.PokemonGlobal.roamPosition[cmdwindow.index];
				RoamPokemonOne(cmdwindow.index);
				if (Game.GameData.PokemonGlobal.roamPosition[cmdwindow.index] == oldmap) {
					Game.GameData.PokemonGlobal.roamPosition[cmdwindow.index] = null;
					RoamPokemonOne(cmdwindow.index);
				}
				Game.GameData.PokemonGlobal.roamedAlready = false;
			}
			cmdwindow.refresh;
		} else if (Input.trigger(Input.BACK)) {
			PlayCancelSE
			break;
		} else if (Input.trigger(Input.USE)) {
			if (cmdwindow.index < cmdwindow.roamerCount) {
				PlayDecisionSE;
				// Toggle through roaming, not roaming, defeated
				if (pkmn[2] > 0 && !Game.GameData.game_switches[pkmn[2]]) {
					// not roaming -> roaming
					Game.GameData.game_switches[pkmn[2]] = true;
				} else if (Game.GameData.PokemonGlobal.roamPokemon[cmdwindow.index] != true) {
					// roaming -> defeated
					Game.GameData.PokemonGlobal.roamPokemon[cmdwindow.index] = true;
					Game.GameData.PokemonGlobal.roamPokemonCaught[cmdwindow.index] = false;
				} else if (Game.GameData.PokemonGlobal.roamPokemon[cmdwindow.index] == true &&
							!Game.GameData.PokemonGlobal.roamPokemonCaught[cmdwindow.index]) {
					// defeated -> caught
					Game.GameData.PokemonGlobal.roamPokemonCaught[cmdwindow.index] = true;
				} else if (pkmn[2] > 0) {
					// caught -> not roaming (or roaming if Switch ID is 0)
					if (pkmn[2] > 0) Game.GameData.game_switches[pkmn[2]] = false;
					Game.GameData.PokemonGlobal.roamPokemon[cmdwindow.index] = null;
					Game.GameData.PokemonGlobal.roamPokemonCaught[cmdwindow.index] = false;
				}
				cmdwindow.refresh;
			} else if (cmdwindow.index == cmdwindow.itemCount - 2) {   // All roam
				if (Settings.ROAMING_SPECIES.length == 0) {
					PlayBuzzerSE;
				} else {
					PlayDecisionSE;
					RoamPokemon;
					Game.GameData.PokemonGlobal.roamedAlready = false;
					cmdwindow.refresh;
				}
			} else {   // Clear all roaming locations
				if (Settings.ROAMING_SPECIES.length == 0) {
					PlayBuzzerSE;
				} else {
					PlayDecisionSE;
					for (int i = Settings.ROAMING_SPECIES.length; i < Settings.ROAMING_SPECIES.length; i++) { //for 'Settings.ROAMING_SPECIES.length' times do => |i|
						Game.GameData.PokemonGlobal.roamPosition[i] = null;
					}
					Game.GameData.PokemonGlobal.roamedAlready = false;
					cmdwindow.refresh;
				}
			}
		}
	}
	DisposeSpriteHash(sprites);
	viewport.dispose;
}

//===============================================================================
// Battle animations import/export.
//===============================================================================
public void ExportAllAnimations() {
	begin;
		Dir.mkdir("Animations") rescue null;
		animations = LoadBattleAnimations;
		if (animations) {
			msgwindow = CreateMessageWindow;
			foreach (var anim in animations) { //'animations.each' do => |anim|
				if (!anim || anim.length == 0 || anim.name == "") continue;
				MessageDisplay(msgwindow, anim.name, false);
				Graphics.update;
				safename = System.Text.RegularExpressions.Regex.Replace(anim.name, "\W", "_");
				Dir.mkdir($"Animations/{safename}") rescue null;
				File.open($"Animations/{safename}/{safename}.anm", "wb") do |f|
					f.write(BattleAnimationEditor.dumpBase64Anim(anim));
				}
				if (anim.graphic && anim.graphic != "") {
					graphicname = RTP.getImagePath("Graphics/Animations/" + anim.graphic);
					SafeCopyFile(graphicname, $"Animations/{safename}/" + File.basename(graphicname));
				}
				foreach (var timing in anim.timing) { //'anim.timing.each' do => |timing|
					if (!timing.timingType || timing.timingType == 0) {
						if (timing.name && timing.name != "") {
							audioName = RTP.getAudioPath("Audio/SE/Anim/" + timing.name);
							SafeCopyFile(audioName, $"Animations/{safename}/" + File.basename(audioName));
						}
					} else if (timing.timingType == 1 || timing.timingType == 3) {
						if (timing.name && timing.name != "") {
							graphicname = RTP.getImagePath("Graphics/Animations/" + timing.name);
							SafeCopyFile(graphicname, $"Animations/{safename}/" + File.basename(graphicname));
						}
					}
				}
			}
			DisposeMessageWindow(msgwindow);
			Message(_INTL("All animations were extracted and saved to the Animations folder."));
		} else {
			Message(_INTL("There are no animations to export."));
		}
	rescue;
		p $!.message, $!.backtrace;
		Message(_INTL("The export failed."));
	}
}

public void ImportAllAnimations() {
	animationFolders = new List<string>();
	if (FileTest.directory("Animations")) {
		foreach (var fb in Dir.foreach("Animations")) { //Dir.foreach("Animations") do => |fb|
			f = "Animations/" + fb;
			if (FileTest.directory(f) && fb != "." && fb != "..") animationFolders.Add(f);
		}
	}
	if (animationFolders.length == 0) {
		Message(_INTL("There are no animations to import. Put each animation in a folder within the Animations folder."));
	} else {
		msgwindow = CreateMessageWindow;
		animations = LoadBattleAnimations;
		if (!animations) animations = new Animations();
		foreach (var folder in animationFolders) { //'animationFolders.each' do => |folder|
			MessageDisplay(msgwindow, folder, false);
			Graphics.update;
			audios = new List<string>();
			files = Dir.glob(folder + "/*.*");
			new {"wav", "ogg", "mp3", "midi", "mid", "wma"}.each do |ext|
				upext = ext.upcase;
				audios.concat(files.find_all(f => f[f.length - 3, 3] == ext));
				audios.concat(files.find_all(f => f[f.length - 3, 3] == upext));
			}
			foreach (var audio in audios) { //'audios.each' do => |audio|
				SafeCopyFile(audio, RTP.getAudioPath("Audio/SE/Anim/" + File.basename(audio)), "Audio/SE/Anim/" + File.basename(audio));
			}
			images = new List<string>();
			new {"png", "gif"}.each do |ext|   // jpg jpeg bmp
				upext = ext.upcase;
				images.concat(files.find_all(f => f[f.length - 3, 3] == ext));
				images.concat(files.find_all(f => f[f.length - 3, 3] == upext));
			}
			foreach (var image in images) { //'images.each' do => |image|
				SafeCopyFile(image, RTP.getImagePath("Graphics/Animations/" + File.basename(image)), "Graphics/Animations/" + File.basename(image));
			}
			Dir.glob(folder + "/*.anm") do |f|
				textdata = BattleAnimationEditor.loadBase64Anim(IO.read(f)) rescue null;
				if (textdata.is_a(Animation)) {
					index = AllocateAnimation(animations, textdata.name);
					missingFiles = new List<string>();
					if (textdata.name == "") textdata.name = File.basename(folder);
					textdata.id = -1;   // This is not an RPG Maker XP animation
					BattleAnimationEditor.ConvertAnimToNewFormat(textdata);
					if (textdata.graphic && textdata.graphic != "" &&
						!FileTest.exist(folder + "/" + textdata.graphic) &&
						!FileTest.image_exist("Graphics/Animations/" + textdata.graphic)) {
						textdata.graphic = "";
						missingFiles.Add(textdata.graphic);
					}
					foreach (var timing in textdata.timing) { //'textdata.timing.each' do => |timing|
						if (!timing.name || timing.name == "" ||
										FileTest.exist(folder + "/" + timing.name) ||
										FileTest.audio_exist("Audio/SE/Anim/" + timing.name)) continue;
						timing.name = "";
						missingFiles.Add(timing.name);
					}
					animations[index] = textdata;
				}
			}
		}
		save_data(animations, "Data/PkmnAnimations.rxdata");
		Game.GameData.game_temp.battle_animations_data = null;
		DisposeMessageWindow(msgwindow);
		Message(_INTL("All animations were imported."));
	}
}

//===============================================================================
// Properly erases all non-existent tiles in maps (including event graphics).
//===============================================================================
public void DebugFixInvalidTiles() {
	total_errors = 0;
	num_error_maps = 0;
	tilesets = Game.GameData.data_tilesets;
	mapData = new Compiler.MapData();
	t = System.uptime;
	Graphics.update;
	total_maps = mapData.mapinfos.keys.length;
	Console.echo_h1(_INTL("Checking {1} maps for invalid tiles", total_maps));
	foreach (var id in mapData.mapinfos.keys.sort) { //'mapData.mapinfos.keys.sort.each' do => |id|
		if (System.uptime - t >= 5) {
			t += 5;
			Graphics.update;
		}
		map_errors = 0;
		map = mapData.getMap(id);
		if (!map || !mapData.mapinfos[id]) continue;
		passages = mapData.getTilesetPassages(map, id);
		// Check all tiles in map for non-existent tiles
		for (int x = map.data.xsize; x < map.data.xsize; x++) { //for 'map.data.xsize' times do => |x|
			for (int y = map.data.ysize; y < map.data.ysize; y++) { //for 'map.data.ysize' times do => |y|
				for (int i = map.data.zsize; i < map.data.zsize; i++) { //for 'map.data.zsize' times do => |i|
					tile_id = map.data[x, y, i];
					if (CheckTileValidity(tile_id, map, tilesets, passages)) continue;
					map.data[x, y, i] = 0;
					map_errors += 1;
				}
			}
		}
		// Check all events in map for page graphics using a non-existent tile
		foreach (var key in map.events) { //map.events.each_key do => |key|
			event = map.events[key];
			foreach (var page in event.pages) { //'event.pages.each' do => |page|
				if (page.graphic.tile_id <= 0) continue;
				if (CheckTileValidity(page.graphic.tile_id, map, tilesets, passages)) continue;
				page.graphic.tile_id = 0;
				map_errors += 1;
			}
		}
		if (map_errors == 0) continue;
		// Map was changed; save it
		Console.echoln_li(_INTL("{1} error tile(s) found on map {2}: {3}.", map_errors, id, mapData.mapinfos[id].name));
		total_errors += map_errors;
		num_error_maps += 1;
		mapData.saveMap(id);
	}
	if (num_error_maps == 0) {
		Console.echo_h2(_INTL("Done. No errors found."), text: :green);
		Message(_INTL("No invalid tiles were found."));
	} else {
		echoln "";
		Console.echo_h2(_INTL("Done. {1} errors found and fixed.", total_errors), text: :green);
		Console.echo_warn(_INTL("RMXP data was altered. Close RMXP now to ensure changes are applied."));
		echoln "";
		Message(_INTL("{1} error(s) were found across {2} map(s) and fixed.", total_errors, num_error_maps));
		Message(_INTL("Close RPG Maker XP to ensure the changes are applied properly."));
	}
}

public void CheckTileValidity(tile_id, map, tilesets, passages) {
	if (!tile_id) return false;
	if (tile_id > 0 && tile_id < 384) {
		// Check for defined autotile
		autotile_id = (tile_id / 48) - 1;
		autotile_name = tilesets[map.tileset_id].autotile_names[autotile_id];
		if (autotile_name && autotile_name != "") return true;
	} else {
		// Check for tileset data
		if (passages[tile_id]) return true;
	}
	return false;
}

//===============================================================================
// Pseudo-party screen for editing Pokémon being set up for a wild battle.
//===============================================================================
public partial class PokemonDebugPartyScreen {
	public void initialize() {
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@messageBox = new Window_AdvancedTextPokemon("");
		@messageBox.viewport       = @viewport;
		@messageBox.visible        = false;
		@messageBox.letterbyletter = true;
		BottomLeftLines(@messageBox, 2);
		@helpWindow = new Window_UnformattedTextPokemon("");
		@helpWindow.viewport = @viewport;
		@helpWindow.visible  = true;
		BottomLeftLines(@helpWindow, 1);
	}

	public void EndScreen() {
		@messageBox.dispose;
		@helpWindow.dispose;
		@viewport.dispose;
	}

	public void Display(text) {
		@messageBox.text    = text;
		@messageBox.visible = true;
		@helpWindow.visible = false;
		PlayDecisionSE;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (@messageBox.busy()) {
				if (Input.trigger(Input.USE)) {
					if (@messageBox.pausing()) PlayDecisionSE;
					@messageBox.resume;
				}
			} else {
				if (Input.trigger(Input.BACK) || Input.trigger(Input.USE)) {
					break;
				}
			}
		}
		@messageBox.visible = false;
		@helpWindow.visible = true;
	}

	public void Confirm(text) {
		ret = -1;
		@messageBox.text    = text;
		@messageBox.visible = true;
		@helpWindow.visible = false;
		using(cmdwindow = new Window_CommandPokemon(new {_INTL("Yes"), _INTL("No")})) do;
			cmdwindow.visible = false;
			BottomRight(cmdwindow);
			cmdwindow.y -= @messageBox.height;
			cmdwindow.z = @viewport.z + 1;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				if (!@messageBox.busy()) cmdwindow.visible = true;
				cmdwindow.update;
				Update;
				if (!@messageBox.busy()) {
					if (Input.trigger(Input.BACK)) {
						ret = false;
						break;
					} else if (Input.trigger(Input.USE) && @messageBox.resume) {
						ret = (cmdwindow.index == 0);
						break;
					}
				}
			}
		}
		@messageBox.visible = false;
		@helpWindow.visible = true;
		return ret;
	}

	public void ShowCommands(text, commands, index = 0) {
		ret = -1;
		@helpWindow.visible = true;
		using(cmdwindow = new Window_CommandPokemonColor(commands)) do;
			cmdwindow.z     = @viewport.z + 1;
			cmdwindow.index = index;
			BottomRight(cmdwindow);
			@helpWindow.resizeHeightToFit(text, Graphics.width - cmdwindow.width);
			@helpWindow.text = text;
			BottomLeft(@helpWindow);
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				cmdwindow.update;
				Update;
				if (Input.trigger(Input.BACK)) {
					PlayCancelSE
					ret = -1;
					break;
				} else if (Input.trigger(Input.USE)) {
					PlayDecisionSE;
					ret = cmdwindow.index;
					break;
				}
			}
		}
		return ret;
	}

	public void ChooseMove(pkmn, text, index = 0) {
		moveNames = new List<string>();
		foreach (var i in pkmn.moves) { //'pkmn.moves.each' do => |i|
			if (i.total_pp <= 0) {
				moveNames.Add(_INTL("{1} (PP: ---)", i.name));
			} else {
				moveNames.Add(_INTL("{1} (PP: {2}/{3})", i.name, i.pp, i.total_pp));
			}
		}
		return ShowCommands(text, moveNames, index);
	}

	public void RefreshSingle(index) {}

	public void update() {
		@messageBox.update;
		@helpWindow.update;
	}
	alias Update update;
}
