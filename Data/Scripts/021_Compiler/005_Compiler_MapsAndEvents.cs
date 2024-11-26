//===============================================================================
//
//===============================================================================
public static partial class Compiler {
	SCRIPT_REPLACEMENTS = new {
		new {"Kernel.",                      ""},
		new {"Game.GameData.PokemonBag.Quantity",       "Game.GameData.bag.quantity"},
		new {"Game.GameData.PokemonBag.HasItem()",       "Game.GameData.bag.has()"},
		new {"Game.GameData.PokemonBag.CanStore()",      "Game.GameData.bag.can_add()"},
		new {"Game.GameData.PokemonBag.StoreItem",      "Game.GameData.bag.add"},
		new {"Game.GameData.PokemonBag.StoreAllOrNone", "Game.GameData.bag.add_all"},
		new {"Game.GameData.PokemonBag.ChangeItem",     "Game.GameData.bag.replace_item"},
		new {"Game.GameData.PokemonBag.DeleteItem",     "Game.GameData.bag.remove"},
		new {"Game.GameData.PokemonBag.IsRegistered()",  "Game.GameData.bag.registered()"},
		new {"Game.GameData.PokemonBag.RegisterItem",   "Game.GameData.bag.register"},
		new {"Game.GameData.PokemonBag.UnregisterItem", "Game.GameData.bag.unregister"},
		new {"Game.GameData.PokemonBag",                  "Game.GameData.bag"},
		new {"Quantity",                   "Game.GameData.bag.quantity"},
		new {"HasItem()",                   "Game.GameData.bag.has()"},
		new {"CanStore()",                  "Game.GameData.bag.can_add()"},
		new {"StoreItem",                  "Game.GameData.bag.add"},
		new {"StoreAllOrNone",             "Game.GameData.bag.add_all"},
		new {"Game.GameData.Trainer",                     "Game.GameData.player"},
		new {"Game.GameData.SaveVersion",                 "Game.GameData.save_engine_version"},
		new {"Game.GameData.game_version",                "Game.GameData.save_game_version"},
		new {"Game.GameData.MapFactory",                  "Game.GameData.map_factory"},
		new {"DayCareDeposited",           "DayCare.count"},
		new {"DayCareGetDeposited",        "DayCare.get_details"},
		new {"DayCareGetLevelGain",        "DayCare.get_level_gain"},
		new {"DayCareDeposit",             "DayCare.deposit"},
		new {"DayCareWithdraw",            "DayCare.withdraw"},
		new {"DayCareChoose",              "DayCare.choose"},
		new {"DayCareGetCompatibility",    "DayCare.get_compatibility"},
		new {"EggGenerated()",              "DayCare.egg_generated()"},
		new {"DayCareGenerateEgg",         "DayCare.collect_egg"},
		new {"get_character(0)",             "get_self"},
		new {"get_character(-1)",            "get_player"},
		new {"CheckAble",                  "Game.GameData.player.has_other_able_pokemon()"},
		new {"Game.GameData.PokemonTemp.lastbattle",      "Game.GameData.game_temp.last_battle_record"},
		new {"calcStats",                    "calc_stats"}
	}

	@@categories.map_data = {
		should_compile = compiling => { next import_new_maps },
		header_text    = () => { next _INTL("Modifying map data") },
		skipped_text   = () => { next _INTL("Not modified") },
		compile        = () => compile_trainer_events;
	}

	@@categories.messages = {
		should_compile = compiling => { next compiling.Contains(:s_files) || compiling.Contains(:map_data) },
		header_text    = () => { next _INTL("Gathering messages for translations") },
		skipped_text   = () => { next _INTL("Not gathered") },
		compile        = () => {
			Console.echo_li(_INTL("Finding messages..."));
			Translator.gather_script_and_event_texts;
			Console.echo_done(true);
			Console.echo_li(_INTL("Saving messages..."));
			MessageTypes.save_default_messages;
			if (FileTest.exist("Data/messages_core.dat")) MessageTypes.load_default_messages;
			Console.echo_done(true);
		}
	}

	#region Class Functions
	#endregion

	//-----------------------------------------------------------------------------
	// Add new map files to the map tree.
	//-----------------------------------------------------------------------------
	public void import_new_maps() {
		if (!Core.DEBUG) return false;
		mapfiles = new List<string>();
		// Get IDs of all maps in the Data folder
		Dir.chdir("Data") do;
			mapData = string.Format("Map*.rxdata");
			foreach (var map in Dir.glob(mapData)) { //'Dir.glob(mapData).each' do => |map|
				if (System.Text.RegularExpressions.Regex.IsMatch(map,@"map(\d+)\.rxdata",RegexOptions.IgnoreCase)) mapfiles[Game.GameData.1.ToInt(10)] = true;
			}
		}
		mapinfos = LoadMapInfos;
		maxOrder = 0;
		// Exclude maps found in mapinfos
		foreach (var id in mapinfos) { //mapinfos.each_key do => |id|
			if (!mapinfos[id]) continue;
			if (mapfiles[id]) mapfiles.delete(id);
			maxOrder = (int)Math.Max(maxOrder, mapinfos[id].order);
		}
		// Import maps not found in mapinfos
		maxOrder += 1;
		imported = false;
		count = 0;
		foreach (var id in mapfiles) { //mapfiles.each_key do => |id|
			if (id == 999) continue;   // Ignore 999 (random dungeon map)
			mapinfo = new RPG.MapInfo();
			mapinfo.order = maxOrder;
			mapinfo.name  = string.Format("MAP{0:3}", id);
			maxOrder += 1;
			mapinfos[id] = mapinfo;
			imported = true;
			count += 1;
		}
		if (imported) {
			save_data(mapinfos, "Data/MapInfos.rxdata");
			Game.GameData.game_temp.map_infos = null;
			Message(_INTL("{1} new map(s) copied to the Data folder were successfully imported.", count));
		}
		return imported;
	}

	//-----------------------------------------------------------------------------
	// Generate and modify event commands.
	//-----------------------------------------------------------------------------
	public void generate_move_route(commands) {
		route           = new RPG.MoveRoute();
		route.repeat    = false;
		route.skippable = true;
		route.list.clear;
		i = 0;
		while (i < commands.length) {
			switch (commands[i]) {
				case MoveRoute.WAIT: case MoveRoute.SWITCH_ON, MoveRoute.SWITCH_OFF,:
						MoveRoute.CHANGE_SPEED, MoveRoute.CHANGE_FREQUENCY, MoveRoute.OPACITY,
						MoveRoute.BLENDING, MoveRoute.PLAY_SE, MoveRoute.SCRIPT;
					route.list.Add(new RPG.MoveCommand(commands[i], [commands[i + 1]]));
					i += 1;
					break;
				case MoveRoute.SCRIPT_ASYNC:
					route.list.Add(new RPG.MoveCommand(MoveRoute.SCRIPT, [commands[i + 1]]));
					route.list.Add(new RPG.MoveCommand(MoveRoute.WAIT, [0]));
					i += 1;
					break;
				case MoveRoute.JUMP:
					route.list.Add(new RPG.MoveCommand(commands[i], new {commands[i + 1], commands[i + 2]}));
					i += 2;
					break;
				case MoveRoute.GRAPHIC:
					route.list.Add(new RPG.MoveCommand(commands[i], new {commands[i + 1], commands[i + 2], commands[i + 3], commands[i + 4]}));
					i += 4;
					break;
				default:
					route.list.Add(new RPG.MoveCommand(commands[i]));
					break;
			}
			i += 1;
		}
		route.list.Add(new RPG.MoveCommand(0));
		return route;
	}

	public void push_move_route(list, character, route, indent = 0) {
		if (route.Length > 0) route = generate_move_route(route);
		for (int i = route.list.length; i < route.list.length; i++) { //for 'route.list.length' times do => |i|
			list.Add(
				new RPG.EventCommand((i == 0) ? 209 : 509, indent,
															(i == 0) ? new {character, route} : [route.list[i - 1]])
			);
		}
	}

	public void push_move_route_and_wait(list, character, route, indent = 0) {
		push_move_route(list, character, route, indent);
		push_event(list, 210, new List<string>(), indent);
	}

	public void push_wait(list, frames, indent = 0) {
		push_event(list, 106, [frames], indent);
	}

	public void push_event(list, cmd, params = null, indent = 0) {
		list.Add(new RPG.EventCommand(cmd, indent, params || []));
	}

	public void push_end(list) {
		list.Add(new RPG.EventCommand(0, 0, new List<string>()));
	}

	public void push_comment(list, cmt, indent = 0) {
		textsplit2 = cmt.split(/\n/);
		for (int i = textsplit2.length; i < textsplit2.length; i++) { //for 'textsplit2.length' times do => |i|
			list.Add(new RPG.EventCommand((i == 0) ? 108 : 408, indent, new {System.Text.RegularExpressions.Regex.Replace(textsplit2[i], "\s+$", "")}));
		}
	}

	public void push_text(list, text, indent = 0) {
		if (!text) return;
		textsplit = text.split(/\\m/);
		foreach (var t in textsplit) { //'textsplit.each' do => |t|
			first = true;
			textsplit2 = t.split(/\n/);
			for (int i = textsplit2.length; i < textsplit2.length; i++) { //for 'textsplit2.length' times do => |i|
				textchunk = System.Text.RegularExpressions.Regex.Replace(textsplit2[i], "\s+$", "");
				if (textchunk && textchunk != "") {
					list.Add(new RPG.EventCommand((first) ? 101 : 401, indent, [textchunk]));
					first = false;
				}
			}
		}
	}

	public void push_script(list, script, indent = 0) {
		if (!script) return;
		first = true;
		textsplit2 = script.split(/\n/);
		for (int i = textsplit2.length; i < textsplit2.length; i++) { //for 'textsplit2.length' times do => |i|
			textchunk = System.Text.RegularExpressions.Regex.Replace(textsplit2[i], "\s+$", "");
			if (textchunk && textchunk != "") {
				list.Add(new RPG.EventCommand((first) ? 355 : 655, indent, [textchunk]));
				first = false;
			}
		}
	}

	public void push_exit(list, indent = 0) {
		list.Add(new RPG.EventCommand(115, indent, new List<string>()));
	}

	public void push_else(list, indent = 0) {
		list.Add(new RPG.EventCommand(0, indent, new List<string>()));
		list.Add(new RPG.EventCommand(411, indent - 1, new List<string>()));
	}

	public void push_branch(list, script, indent = 0) {
		list.Add(new RPG.EventCommand(111, indent, new {12, script}));
	}

	public void push_branch_end(list, indent = 0) {
		list.Add(new RPG.EventCommand(0, indent, new List<string>()));
		list.Add(new RPG.EventCommand(412, indent - 1, new List<string>()));
	}

	// cancel is 1/2/3/4 for the options, 0 for disallow, 5 for branch
	public void push_choices(list, choices, cancel = 0, indent = 0) {
		list.Add(new RPG.EventCommand(102, indent, new {choices, cancel, new {0, 1, 2, 3}}));
	}

	public void push_choice(list, index, text, indent = 0) {
		if (index > 0) list.Add(new RPG.EventCommand(0, indent, new List<string>()));
		list.Add(new RPG.EventCommand(402, indent - 1, new {index, text}));
	}

	public void push_choices_end(list, indent = 0) {
		list.Add(new RPG.EventCommand(0, indent, new List<string>()));
		list.Add(new RPG.EventCommand(404, indent - 1, new List<string>()));
	}

	public void push_self_switch(list, swtch, switchOn, indent = 0) {
		list.Add(new RPG.EventCommand(123, indent, new {swtch, switchOn ? 0 : 1}));
	}

	public void apply_pages(page, pages) {
		foreach (var p in pages) { //'pages.each' do => |p|
			p.graphic       = page.graphic;
			p.walk_anime    = page.walk_anime;
			p.step_anime    = page.step_anime;
			p.direction_fix = page.direction_fix;
			p.through       = page.through;
			p.always_on_top = page.always_on_top;
		}
	}

	public void add_passage_list(event, mapData) {
		if (!event || event.pages.length == 0) return;
		page                         = new RPG.Event.Page();
		page.condition.switch1_valid = true;
		page.condition.switch1_id    = mapData.registerSwitch('s:tsOff("A")');
		page.graphic.character_name  = "";
		page.trigger                 = 3;   // Autorun
		page.list.clear;
		list = page.list;
		push_branch(list, "get_self.onEvent()");
		push_event(list, 208, [0], 1);   // Change Transparent Flag
		push_wait(list, 6, 1);          // Wait
		push_event(list, 208, [1], 1);   // Change Transparent Flag
		push_move_route_and_wait(list, -1, [MoveRoute.DOWN], 1);
		push_branch_end(list, 1);
		push_script(list, "setTempSwitchOn(\"A\")");
		push_end(list);
		event.pages.Add(page);
	}

	//-----------------------------------------------------------------------------

	public void safequote(x) {
		x = System.Text.RegularExpressions.Regex.Replace(x, "\"\#\'\\", a => "\\" + a);
		x = System.Text.RegularExpressions.Regex.Replace(x, "\t", "\\t");
		x = System.Text.RegularExpressions.Regex.Replace(x, "\r", "\\r");
		x = System.Text.RegularExpressions.Regex.Replace(x, "\n", "\\n");
		return x;
	}

	public void safequote2(x) {
		x = System.Text.RegularExpressions.Regex.Replace(x, "\"\#\'\\", a => "\\" + a);
		x = System.Text.RegularExpressions.Regex.Replace(x, "\t", "\\t");
		x = System.Text.RegularExpressions.Regex.Replace(x, "\r", "\\r");
		x = System.Text.RegularExpressions.Regex.Replace(x, "\n", " ");
		return x;
	}

	public void EventId(event) {
		list = event.pages[0].list;
		if (list.length == 0) return null;
		codes = new List<string>();
		i = 0;
		while (i < list.length) {
			codes.Add(list[i].code);
			i += 1;
		}
	}

	public void EachPage(e) {
		if (!e) return true;
		if (e.is_a(RPG.CommonEvent)) {
			yield e;
		} else {
			e.pages.each(page => yield page);
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class MapData {
		public int mapinfos		{ get { return _mapinfos; } }			protected int _mapinfos;

		public void initialize() {
			@mapinfos = LoadMapInfos;
			@system   = load_data("Data/System.rxdata");
			@tilesets = load_data("Data/Tilesets.rxdata");
			@mapxy      = new List<string>();
			@mapWidths  = new List<string>();
			@mapHeights = new List<string>();
			@maps       = new List<string>();
			@registeredSwitches = new List<string>();
		}

		public void switchName(id) {
			return @system.switches[id] || "";
		}

		public void mapFilename(mapID) {
			return string.Format("Data/Map{0:3}.rxdata", mapID);
		}

		public void getMap(mapID) {
			if (@maps[mapID]) return @maps[mapID];
			begin;
				@maps[mapID] = load_data(mapFilename(mapID));
				return @maps[mapID];
			rescue;
				return null;
			}
		}

		public void getEventFromXY(mapID, x, y) {
			if (x < 0 || y < 0) return null;
			mapPositions = @mapxy[mapID];
			if (mapPositions) return mapPositions[(y * @mapWidths[mapID]) + x];
			map = getMap(mapID);
			if (!map) return null;
			@mapWidths[mapID]  = map.width;
			@mapHeights[mapID] = map.height;
			mapPositions = new List<string>();
			width = map.width;
			foreach (var e in map.events) { //map.events.each_value do => |e|
				if (e) mapPositions[(e.y * width) + e.x] = e;
			}
			@mapxy[mapID] = mapPositions;
			return mapPositions[(y * width) + x];
		}

		public void getEventFromID(mapID, id) {
			map = getMap(mapID);
			if (!map) return null;
			return map.events[id];
		}

		public void getTilesetPassages(map, mapID) {
			begin;
				return @tilesets[map.tileset_id].passages;
			rescue;
				Debug.LogError($"Tileset data for tileset number {map.tileset_id} used on map {mapID} was not found. " +);
				//throw new Exception($"Tileset data for tileset number {map.tileset_id} used on map {mapID} was not found. " +);
							"The tileset was likely deleted, but one or more maps still use it.";
			}
		}

		public void getTilesetPriorities(map, mapID) {
			begin;
				return @tilesets[map.tileset_id].priorities;
			rescue;
				Debug.LogError($"Tileset data for tileset number {map.tileset_id} used on map {mapID} was not found. " +);
				//throw new Exception($"Tileset data for tileset number {map.tileset_id} used on map {mapID} was not found. " +);
							"The tileset was likely deleted, but one or more maps still use it.";
			}
		}

		public bool isPassable(mapID, x, y) {
			map = getMap(mapID);
			if (!map) return false;
			if (x < 0 || x >= map.width || y < 0 || y >= map.height) return false;
			passages   = getTilesetPassages(map, mapID);
			priorities = getTilesetPriorities(map, mapID);
			new {2, 1, 0}.each do |i|
				tile_id = map.data[x, y, i];
				if (tile_id.null()) return false;
				passage = passages[tile_id];
				if (!passage) {
					Debug.LogError($"The tile used on map {mapID} at coordinates ({x}, {y}) on layer {i + 1} doesn't exist in the tileset. " +);
					//throw new Exception($"The tile used on map {mapID} at coordinates ({x}, {y}) on layer {i + 1} doesn't exist in the tileset. " +);
								"It should be deleted to prevent errors.";
				}
				if (passage & 0x0f == 0x0f) return false;
				if (priorities[tile_id] == 0) return true;
			}
			return true;
		}

		public bool isCounterTile(mapID, x, y) {
			map = getMap(mapID);
			if (!map) return false;
			passages = getTilesetPassages(map, mapID);
			new {2, 1, 0}.each do |i|
				tile_id = map.data[x, y, i];
				if (tile_id.null()) return false;
				passage = passages[tile_id];
				if (!passage) {
					Debug.LogError($"The tile used on map {mapID} at coordinates ({x}, {y}) on layer {i + 1} doesn't exist in the tileset. " +);
					//throw new Exception($"The tile used on map {mapID} at coordinates ({x}, {y}) on layer {i + 1} doesn't exist in the tileset. " +);
								"It should be deleted to prevent errors.";
				}
				if (passage & 0x80 == 0x80) return true;
			}
			return false;
		}

		public void setCounterTile(mapID, x, y) {
			map = getMap(mapID);
			if (!map) return;
			passages = getTilesetPassages(map, mapID);
			new {2, 1, 0}.each do |i|
				tile_id = map.data[x, y, i];
				if (tile_id == 0) continue;
				passages[tile_id] |= 0x80;
				break;
			}
		}

		public void registerSwitch(switch) {
			if (@registeredSwitches[switch]) return @registeredSwitches[switch];
			(1..5000).each do |id|
				name = @system.switches[id];
				if (name && name != "" && name != switch) continue;
				@system.switches[id] = switch;
				@registeredSwitches[switch] = id;
				return id;
			}
			return 1;
		}

		public void saveMap(mapID) {
			save_data(getMap(mapID), mapFilename(mapID)) rescue null;
		}

		public void saveTilesets() {
			save_data(@tilesets, "Data/Tilesets.rxdata");
			save_data(@system, "Data/System.rxdata");
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class TrainerChecker {
		public void initialize() {
			@dontaskagain = false;
		}

		public void TrainerTypeCheck(trainer_type) {
			if (!Core.DEBUG || @dontaskagain) return;
			if (GameData.TrainerType.exists(trainer_type)) return;
			if (ConfirmMessage(_INTL("Add new trainer type {1}?", trainer_type.ToString()))) {
				TrainerTypeEditorNew(trainer_type.ToString());
			}
		}

		public void TrainerBattleCheck(tr_type, tr_name, tr_version) {
			if (!Core.DEBUG || @dontaskagain) return;
			// Check for existence of trainer type
			TrainerTypeCheck(tr_type);
			if (!GameData.TrainerType.exists(tr_type)) return;
			tr_type = GameData.TrainerType.get(tr_type).id;
			// Check for existence of trainer
			if (GameData.Trainer.exists(tr_type, tr_name, tr_version)) return;
			// Add new trainer
			cmd = MissingTrainer(tr_type, tr_name, tr_version);
			if (cmd == 2) {
				@dontaskagain = true;
				Graphics.update;
			}
		}
	}

	//-----------------------------------------------------------------------------

	// Convert trainer comments to trainer event.
	public void convert_to_trainer_event(event, trainerChecker) {
		if (!event || event.pages.length == 0) return null;
		list = event.pages[0].list;
		if (list.length < 2) return null;
		commands = new List<string>();
		isFirstCommand = false;
		// Find all the trainer comments in the event
		for (int i = list.length; i < list.length; i++) { //for 'list.length' times do => |i|
			if (list[i].code != 108) continue;   // Comment (first line)
			command = list[i].parameters[0];
			for (int j = (i + 1); j < list.length; j++) { //each 'list.length' do => |j|
				if (list[j].code != 408) break;   // Comment (continuation line)
				command += "\r\n" + list[j].parameters[0];
			}
			if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^(Battle\:|Type\:|Name\:|BattleID\:|DoubleBattle\:|Backdrop\:|EndSpeech\:|Outcome\:|Continue\:|EndBattle\:|EndIfSwitch\:|VanishIfSwitch\:|RegSpeech\:)",RegexOptions.IgnoreCase)) {
				commands.Add(command);
				if (i == 0) isFirstCommand = true;
			}
		}
		if (commands.length == 0) return null;
		// Found trainer comments; create a new Event object to replace this event
		ret = new RPG.Event(event.x, event.y);
		ret.name = event.name;
		ret.id   = event.id;
		firstpage = Marshal.load(Marshal.dump(event.pages[0]));   // Copy event's first page
		firstpage.trigger = 2;   // On event touch
		firstpage.list    = new List<string>();   // Clear page's commands
		// Rename the event if there's nothing above the trainer comments
		if (isFirstCommand) {
			if (!System.Text.RegularExpressions.Regex.IsMatch(event.name,@"trainer",RegexOptions.IgnoreCase)) {
				ret.name = "Trainer(3)";
			} else if (System.Text.RegularExpressions.Regex.IsMatch(event.name,@"^\s*trainer\s*\((\d+)\)\s*$",RegexOptions.IgnoreCase)) {
				ret.name = $"Trainer({Game.GameData.1})";
			}
		}
		// Compile the trainer comments
		rewriteComments = false;   // You can change this
		battles        = new List<string>();
		trtype         = null;
		trname         = null;
		battleid       = 0;
		doublebattle   = false;
		backdrop       = null;
		outcome        = 0;
		continue       = false;
		endbattles     = new List<string>();
		endifswitch    = new List<string>();
		vanishifswitch = new List<string>();
		regspeech      = null;
		common_event   = 0;
		foreach (var command in commands) { //'commands.each' do => |command|
			if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^Battle\:\s*([\s\S]+)$",RegexOptions.IgnoreCase)) {
				battles.Add($~[1]);
				if (rewriteComments) push_comment(firstpage.list, command);
			} else if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^Type\:\s*([\s\S]+)$",RegexOptions.IgnoreCase)) {
				trtype = System.Text.RegularExpressions.Regex.Replace($~[1], "^\s+", ""); trtype = System.Text.RegularExpressions.Regex.Replace(trtype, "\s+$", "");
				if (rewriteComments) push_comment(firstpage.list, command);
			} else if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^Name\:\s*([\s\S]+)$",RegexOptions.IgnoreCase)) {
				trname = System.Text.RegularExpressions.Regex.Replace($~[1], "^\s+", ""); trname = System.Text.RegularExpressions.Regex.Replace(trname, "\s+$", "");
				if (rewriteComments) push_comment(firstpage.list, command);
			} else if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^BattleID\:\s*(\d+)$",RegexOptions.IgnoreCase)) {
				battleid = $~[1].ToInt();
				if (rewriteComments) push_comment(firstpage.list, command);
			} else if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^DoubleBattle\:\s*([\s\S]+)$",RegexOptions.IgnoreCase)) {
				value = System.Text.RegularExpressions.Regex.Replace($~[1], "^\s+", ""); value = System.Text.RegularExpressions.Regex.Replace(value, "\s+$", "");
				if (value.upcase == "TRUE" || value.upcase == "YES") doublebattle = true;
				if (rewriteComments) push_comment(firstpage.list, command);
			} else if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^Continue\:\s*([\s\S]+)$",RegexOptions.IgnoreCase)) {
				value = System.Text.RegularExpressions.Regex.Replace($~[1], "^\s+", ""); value = System.Text.RegularExpressions.Regex.Replace(value, "\s+$", "");
				if (value.upcase == "TRUE" || value.upcase == "YES") continue = true;
				if (rewriteComments) push_comment(firstpage.list, command);
			} else if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^EndIfSwitch\:\s*([\s\S]+)$",RegexOptions.IgnoreCase)) {
				_ = System.Text.RegularExpressions.Regex.Replace($~[1], "^\s+", ""); _ = System.Text.RegularExpressions.Regex.Replace(_, "\s+$", ""); endifswitch.Add((_).ToInt());
				if (rewriteComments) push_comment(firstpage.list, command);
			} else if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^VanishIfSwitch\:\s*([\s\S]+)$",RegexOptions.IgnoreCase)) {
				_ = System.Text.RegularExpressions.Regex.Replace($~[1], "^\s+", ""); _ = System.Text.RegularExpressions.Regex.Replace(_, "\s+$", ""); vanishifswitch.Add((_).ToInt());
				if (rewriteComments) push_comment(firstpage.list, command);
			} else if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^Backdrop\:\s*([\s\S]+)$",RegexOptions.IgnoreCase)) {
				backdrop = System.Text.RegularExpressions.Regex.Replace($~[1], "^\s+", ""); backdrop = System.Text.RegularExpressions.Regex.Replace(backdrop, "\s+$", "");
				if (rewriteComments) push_comment(firstpage.list, command);
			} else if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^Outcome\:\s*(\d+)$",RegexOptions.IgnoreCase)) {
				outcome = $~[1].ToInt();
				if (rewriteComments) push_comment(firstpage.list, command);
			} else if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^EndBattle\:\s*([\s\S]+)$",RegexOptions.IgnoreCase)) {
				_ = System.Text.RegularExpressions.Regex.Replace($~[1], "^\s+", ""); _ = System.Text.RegularExpressions.Regex.Replace(_, "\s+$", ""); endbattles.Add(_);
				if (rewriteComments) push_comment(firstpage.list, command);
			} else if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^RegSpeech\:\s*([\s\S]+)$",RegexOptions.IgnoreCase)) {
				regspeech = System.Text.RegularExpressions.Regex.Replace($~[1], "^\s+", ""); regspeech = System.Text.RegularExpressions.Regex.Replace(regspeech, "\s+$", "");
				if (rewriteComments) push_comment(firstpage.list, command);
			} else if (System.Text.RegularExpressions.Regex.IsMatch(command,@"^CommonEvent\:\s*(\d+)$",RegexOptions.IgnoreCase)) {
				common_event = $~[1].ToInt();
				if (rewriteComments) push_comment(firstpage.list, command);
			}
		}
		if (battles.length <= 0) return null;
		if (endbattles.length == 0) endbattles.Add("...");
		// Run trainer check now, except in editor
		trainerChecker.TrainerBattleCheck(trtype, trname, battleid);
		// Set the event's charset to one depending on the trainer type if the event
		// doesn't have a charset
		if (firstpage.graphic.character_name == "" && GameData.TrainerType.exists(trtype)) {
			trainerid = GameData.TrainerType.get(trtype).id;
			filename = GameData.TrainerType.charset_filename_brief(trainerid);
			if (FileTest.image_exist("Graphics/Characters/" + filename)) {
				firstpage.graphic.character_name = filename;
			}
		}
		// Create strings that will be used repeatedly
		safetrcombo = string.Format(":{0}, \"{0}\"", trtype, safequote(trname));   // :YOUNGSTER, "Joey"
		brieftrcombo = safetrcombo;
		if (battleid > 0) safetrcombo = string.Format("{0}, {0}", safetrcombo, battleid);   // :YOUNGSTER, "Joey", 1
		introplay   = string.Format("TrainerIntro(:{0})", trtype);
		// Write first page
		push_script(firstpage.list, introplay);   // TrainerIntro
		push_script(firstpage.list, "NoticePlayer(get_self)");
		push_text(firstpage.list, battles[0]);
		if (battles.length > 1) {   // Has rematches
			if (battleid > 0) {
				push_script(firstpage.list, string.Format("TrainerCheck({0}, {0}, {0})", brieftrcombo, battles.length, battleid));
			} else {
				push_script(firstpage.list, string.Format("TrainerCheck({0}, {0})", brieftrcombo, battles.length));
			}
		}
		if (doublebattle) push_script(firstpage.list, "setBattleRule(\"double\")");
		if (backdrop) push_script(firstpage.list, string.Format("setBattleRule(\n  \"backdrop\", \"{0}\"\n)", safequote(backdrop)));
		if (outcome > 1) push_script(firstpage.list, string.Format("setBattleRule(\"outcomeVar\", {0})", outcome));
		if (continue) push_script(firstpage.list, "setBattleRule(\"canLose\")");
		battleString = string.Format("TrainerBattle.start({0})", safetrcombo);
		push_branch(firstpage.list, battleString);
		if (battles.length > 1) {   // Has rematches
			push_branch(firstpage.list, string.Format("Phone.can_add({0})", safetrcombo), 1);
			push_text(firstpage.list, regspeech, 2);
			push_choices(firstpage.list, new {"Yes", "No"}, 2, 2);
			push_choice(firstpage.list, 0, "Yes", 3);
			if (common_event > 0) {
				if (battleid > 0) {
					push_script(firstpage.list, string.Format("Phone.add(get_self,\n  {0}, {0}, {0}, {0}\n)",
																							brieftrcombo, battles.length, battleid, common_event), 3);
				} else {
					push_script(firstpage.list, string.Format("Phone.add(get_self,\n  {0}, {0}, null, {0}\n)",
																							brieftrcombo, battles.length, common_event), 3);
				}
			} else {
				if (battleid > 0) {
					push_script(firstpage.list, string.Format("Phone.add(get_self,\n  {0}, {0}, {0}\n)",
																							brieftrcombo, battles.length, battleid), 3);
				} else {
					push_script(firstpage.list, string.Format("Phone.add(get_self,\n  {0}, {0}\n)",
																							brieftrcombo, battles.length), 3);
				}
			}
			push_choice(firstpage.list, 1, "No", 3);
			push_choices_end(firstpage.list, 3);
			push_branch_end(firstpage.list, 2);
		}
		push_self_switch(firstpage.list, "A", true, 1);
		if (battles.length > 1) push_self_switch(firstpage.list, "B", true, 1);
		push_branch_end(firstpage.list, 1);
		push_script(firstpage.list, "TrainerEnd", 0);
		push_end(firstpage.list);
		// Copy first page to last page and make changes to its properties
		lastpage = Marshal.load(Marshal.dump(firstpage));
		lastpage.trigger   = 0;   // On action
		lastpage.list      = new List<string>();   // Clear page's commands
		lastpage.condition = firstpage.condition.clone;
		lastpage.condition.self_switch_valid = true;
		lastpage.condition.self_switch_ch    = "A";
		// Copy last page to rematch page
		rematchpage = Marshal.load(Marshal.dump(lastpage));
		rematchpage.list      = lastpage.list.clone;   // Copy the last page's commands
		rematchpage.condition = lastpage.condition.clone;
		rematchpage.condition.self_switch_valid = true;
		rematchpage.condition.self_switch_ch    = "B";
		// Write rematch page
		push_script(rematchpage.list, introplay, 0);   // TrainerIntro
		if (battles.length == 2) {
			push_text(rematchpage.list, battles[1], 0);
		} else {
			for (int i = 1; i < battles.length; i++) { //each 'battles.length' do => |i|
				// Run trainer check now, except in editor
				trainerChecker.TrainerBattleCheck(trtype, trname, battleid + i);
				switch (i) {
					case 1:
						push_branch(rematchpage.list, string.Format("Phone.variant({0}) <= {0}", safetrcombo, i));
						break;
					case battles.length - 1:
						push_branch(rematchpage.list, string.Format("Phone.variant({0}) >= {0}", safetrcombo, i));
						break;
					default:
						push_branch(rematchpage.list, string.Format("Phone.variant({0}) == {0}", safetrcombo, i));
						break;
				}
				push_text(rematchpage.list, battles[i], 1);
				push_branch_end(rematchpage.list, 1);
			}
		}
		if (doublebattle) push_script(rematchpage.list, "setBattleRule(\"double\")", 1);
		if (backdrop) push_script(rematchpage.list, string.Format("setBattleRule(\n  \"backdrop\", {0}\n)", safequote(backdrop)), 1);
		if (outcome > 1) push_script(rematchpage.list, string.Format("setBattleRule(\"outcomeVar\", {0})", outcome), 1);
		if (continue) push_script(rematchpage.list, "setBattleRule(\"canLose\")", 1);
		battleString = string.Format("Phone.battle({0})", safetrcombo);
		push_branch(rematchpage.list, battleString, 0);
		push_script(rematchpage.list, string.Format("Phone.reset_after_win(\n  {0}\n)", safetrcombo), 1);
		push_self_switch(rematchpage.list, "A", true, 1);
		push_script(rematchpage.list, "TrainerEnd", 1);
		push_branch_end(rematchpage.list, 1);
		push_end(rematchpage.list);
		// Write last page
		if (endbattles.length > 0) {
			if (battles.length == 1) {
				push_text(lastpage.list, endbattles[0], 0);
			} else {
				for (int i = 0; i < battles.length; i++) { //each 'battles.length' do => |i|
					if (i == battles.length - 1) {
						push_branch(lastpage.list, string.Format("Phone.variant({0}) >= {0}", safetrcombo, i));
					} else {
						push_branch(lastpage.list, string.Format("Phone.variant({0}) == {0}", safetrcombo, i));
					}
					ebattle = (endbattles[i]) ? endbattles[i] : endbattles[endbattles.length - 1];
					push_text(lastpage.list, ebattle, 1);
					push_branch_end(lastpage.list, 1);
				}
			}
		}
		if (battles.length > 1) {
			push_branch(lastpage.list, string.Format("Phone.can_add({0})", safetrcombo), 0);
			push_text(lastpage.list, regspeech, 1);
			push_choices(lastpage.list, new {"Yes", "No"}, 2, 1);
			push_choice(lastpage.list, 0, "Yes", 2);
			if (common_event > 0) {
				if (battleid > 0) {
					push_script(lastpage.list, string.Format("Phone.add(get_self,\n  {0}, {0}, {0}, {0}\n)",
																						brieftrcombo, battles.length, battleid, common_event), 2);
				} else {
					push_script(lastpage.list, string.Format("Phone.add(get_self,\n  {0}, {0}, null, {0}\n)",
																						brieftrcombo, battles.length, common_event), 2);
				}
			} else {
				if (battleid > 0) {
					push_script(lastpage.list, string.Format("Phone.add(get_self,\n  {0}, {0}, {0}\n)",
																						brieftrcombo, battles.length, battleid), 2);
				} else {
					push_script(lastpage.list, string.Format("Phone.add(get_self,\n  {0}, {0}\n)",
																						brieftrcombo, battles.length), 2);
				}
			}
			push_choice(lastpage.list, 1, "No", 2);
			push_choices_end(lastpage.list, 2);
			push_branch_end(lastpage.list, 1);
		}
		push_end(lastpage.list);
		// Add pages to the new event
		if (battles.length == 1) {   // Only one battle
			ret.pages = new {firstpage, lastpage};
		} else {   // Has rematches
			ret.pages = new {firstpage, rematchpage, lastpage};
		}
		// Copy last page to endIfSwitch page
		foreach (var endswitch in endifswitch) { //'endifswitch.each' do => |endswitch|
			endIfSwitchPage = Marshal.load(Marshal.dump(lastpage));
			endIfSwitchPage.condition = lastpage.condition.clone;
			if (endIfSwitchPage.condition.switch1_valid) {   // Add another page condition
				endIfSwitchPage.condition.switch2_valid = true;
				endIfSwitchPage.condition.switch2_id    = endswitch;
			} else {
				endIfSwitchPage.condition.switch1_valid = true;
				endIfSwitchPage.condition.switch1_id    = endswitch;
			}
			endIfSwitchPage.condition.self_switch_valid = false;
			endIfSwitchPage.list = new List<string>();   // Clear page's commands
			ebattle = (endbattles[0]) ? endbattles[0] : "...";
			push_text(endIfSwitchPage.list, ebattle);
			push_end(endIfSwitchPage.list);
			ret.pages.Add(endIfSwitchPage);
		}
		// Copy last page to vanishIfSwitch page
		foreach (var vanishswitch in vanishifswitch) { //'vanishifswitch.each' do => |vanishswitch|
			vanishIfSwitchPage = Marshal.load(Marshal.dump(lastpage));
			vanishIfSwitchPage.graphic.character_name = "";   // No charset
			vanishIfSwitchPage.condition = lastpage.condition.clone;
			if (vanishIfSwitchPage.condition.switch1_valid) {   // Add another page condition
				vanishIfSwitchPage.condition.switch2_valid = true;
				vanishIfSwitchPage.condition.switch2_id    = vanishswitch;
			} else {
				vanishIfSwitchPage.condition.switch1_valid = true;
				vanishIfSwitchPage.condition.switch1_id    = vanishswitch;
			}
			vanishIfSwitchPage.condition.self_switch_valid = false;
			vanishIfSwitchPage.list = new List<string>();   // Clear page's commands
			push_end(vanishIfSwitchPage.list);
			ret.pages.Add(vanishIfSwitchPage);
		}
		return ret;
	}

	//-----------------------------------------------------------------------------

	// Convert event name to item event.
	// Checks if the event's name is "Item:POTION" or "HiddenItem:POTION". If so,
	// rewrites the whole event into one now named "Item"/"HiddenItem" which gives
	// that item when interacted with.
	public void convert_to_item_event(event) {
		if (!event || event.pages.length == 0) return null;
		name = event.name;
		ret       = new RPG.Event(event.x, event.y);
		ret.name  = event.name;
		ret.id    = event.id;
		ret.pages = new List<string>();
		itemName = "";
		hidden = false;
		if (System.Text.RegularExpressions.Regex.IsMatch(name,@"^hiddenitem\:\s*(\w+)\s*$",RegexOptions.IgnoreCase)) {
			itemName = Game.GameData.1;
			if (!GameData.Item.exists(itemName)) return null;
			ret.name = "HiddenItem";
			hidden = true;
		} else if (System.Text.RegularExpressions.Regex.IsMatch(name,@"^item\:\s*(\w+)\s*$",RegexOptions.IgnoreCase)) {
			itemName = Game.GameData.1;
			if (!GameData.Item.exists(itemName)) return null;
			ret.name = "Item";
		} else {
			return null;
		}
		// Event page 1
		page = new RPG.Event.Page();
		if (!hidden) page.graphic.character_name = "Object ball";
		page.list = new List<string>();
		push_branch(page.list, string.Format("ItemBall(:{0})", itemName));
		push_self_switch(page.list, "A", true, 1);
		push_else(page.list, 1);
		push_branch_end(page.list, 1);
		push_end(page.list);
		ret.pages.Add(page);
		// Event page 2
		page = new RPG.Event.Page();
		page.condition.self_switch_valid = true;
		page.condition.self_switch_ch    = "A";
		ret.pages.Add(page);
		return ret;
	}

	//-----------------------------------------------------------------------------

	// Checks whether a given event is likely to be a door. If so, rewrite it to
	// include animating the event as though it was a door opening and closing as the
	// player passes through.
	public void update_door_event(event, mapData) {
		changed = false;
		if (event.is_a(RPG.CommonEvent)) return false;
		// Check if event has 2+ pages and the last page meets all of these criteria:
		//   - Has a condition of a Switch being ON
		//   - The event has a charset graphic
		//   - There are more than 5 commands in that page, the first of which is a
		//     Conditional Branch
		lastPage = event.pages[event.pages.length - 1];
		if ((event.pages.length >= 2 &&
			lastPage.condition.switch1_valid &&
			lastPage.graphic.character_name != "" &&
			lastPage.list.length > 5 &&
			lastPage.list[0].code == 111) {
			// This bit of code is just in case Switch 22 has been renamed/repurposed,
			// which is highly unlikely. It changes the Switch used in the condition to
			// whichever is named 's:tsOff("A")'.
			if ((lastPage.condition.switch1_id == 22 &&
				mapData.switchName(lastPage.condition.switch1_id) != 's:tsOff("A")';
				lastPage.condition.switch1_id = mapData.registerSwitch('s:tsOff("A")');
				changed = true;
			}
			// If the last page's Switch condition uses a Switch named 's:tsOff("A")',
			// check the penultimate page. If it contains exactly 1 "Transfer Player"
			// command and does NOT contain a "Change Transparent Flag" command, rewrite
			// both the penultimate page and the last page.
			if (mapData.switchName(lastPage.condition.switch1_id) == 's:tsOff("A")') {
				list = event.pages[event.pages.length - 2].list;
				transferCommand = list.find_all(cmd => cmd.code == 201);   // Transfer Player
				if (transferCommand.length == 1 && list.none(cmd => cmd.code == 208)) {   // Change Transparent Flag
					// Rewrite penultimate page
					list.clear;
					push_move_route_and_wait(   // Move Route for door opening
						list, 0,
						new {MoveRoute.PLAY_SE, new RPG.AudioFile("Door enter"), MoveRoute.WAIT, 2,
						MoveRoute.TURN_LEFT, MoveRoute.WAIT, 2,
						MoveRoute.TURN_RIGHT, MoveRoute.WAIT, 2,
						MoveRoute.TURN_UP, MoveRoute.WAIT, 2}
					);
					push_move_route_and_wait(   // Move Route for player entering door
						list, -1,
						new {MoveRoute.THROUGH_ON, MoveRoute.UP, MoveRoute.THROUGH_OFF}
					);
					push_event(list, 208, [0]);   // Change Transparent Flag (invisible)
					push_script(list, "Followers.follow_into_door");
					push_event(list, 210, new List<string>());   // Wait for Move's Completion
					push_move_route_and_wait(   // Move Route for door closing
						list, 0,
						new {MoveRoute.WAIT, 2,
						MoveRoute.TURN_RIGHT, MoveRoute.WAIT, 2,
						MoveRoute.TURN_LEFT, MoveRoute.WAIT, 2,
						MoveRoute.TURN_DOWN, MoveRoute.WAIT, 2}
					);
					push_event(list, 223, new {new Tone(-255, -255, -255), 6});   // Change Screen Color Tone
					push_wait(list, 8);   // Wait
					push_event(list, 208, [1]);   // Change Transparent Flag (visible)
					push_event(list, transferCommand[0].code, transferCommand[0].parameters);   // Transfer Player
					push_event(list, 223, new {new Tone(0, 0, 0), 6});   // Change Screen Color Tone
					push_end(list);
					// Rewrite last page
					list = lastPage.list;
					list.clear;
					push_branch(list, "get_self.onEvent()");   // Conditional Branch
					push_event(list, 208, [0], 1);   // Change Transparent Flag (invisible)
					push_script(list, "Followers.hide_followers", 1);
					push_move_route_and_wait(   // Move Route for setting door to open
						list, 0,
						new {MoveRoute.TURN_LEFT, MoveRoute.WAIT, 6},
						1
					);
					push_event(list, 208, [1], 1);   // Change Transparent Flag (visible)
					push_move_route_and_wait(list, -1, [MoveRoute.DOWN], 1);   // Move Route for player exiting door
					push_script(list, "Followers.put_followers_on_player", 1);
					push_move_route_and_wait(   // Move Route for door closing
						list, 0,
						new {MoveRoute.TURN_UP, MoveRoute.WAIT, 2,
						MoveRoute.TURN_RIGHT, MoveRoute.WAIT, 2,
						MoveRoute.TURN_DOWN, MoveRoute.WAIT, 2},
						1
					);
					push_branch_end(list, 1);
					push_script(list, "setTempSwitchOn(\"A\")");
					push_end(list);
					changed = true;
				}
			}
		}
		return changed;
	}

	//-----------------------------------------------------------------------------
	// Fix up standard code snippets.
	//-----------------------------------------------------------------------------
	public bool event_is_empty(e) {
		if (!e) return true;
		if (e.is_a(RPG.CommonEvent)) return false;
		return e.pages.length == 0;
	}

	// Checks if the event has exactly 1 page, said page has no graphic, it has
	// less than 12 commands and at least one is a Transfer Player, and the tiles
	// to the left/right/upper left/upper right are not passable but the event's
	// tile is. Causes a second page to be added to the event which is the "is
	// player on me?" check that occurs when the map is entered.
	public bool likely_passage(thisEvent, mapID, mapData) {
		if (!thisEvent || thisEvent.pages.length == 0) return false;
		if (thisEvent.pages.length != 1) return false;
		if ((thisEvent.pages[0].graphic.character_name == "" &&
			thisEvent.pages[0].list.length <= 12 &&
			thisEvent.pages[0].list.any(cmd => cmd.code == 201) &&   // Transfer Player
//			mapData.isPassable(mapID, thisEvent.x, thisEvent.y + 1) &&
			mapData.isPassable(mapID, thisEvent.x, thisEvent.y) &&
			!mapData.isPassable(mapID, thisEvent.x - 1, thisEvent.y) &&
			!mapData.isPassable(mapID, thisEvent.x + 1, thisEvent.y) &&
			!mapData.isPassable(mapID, thisEvent.x - 1, thisEvent.y - 1) &&
			!mapData.isPassable(mapID, thisEvent.x + 1, thisEvent.y - 1)) {
			return true;
		}
		return false;
	}

	public void fix_event_name(event) {
		if (!event) return false;
		switch (event.name.downcase) {
			case "tree":
				event.name = "CutTree";
				break;
			case "rock":
				event.name = "SmashRock";
				break;
			case "boulder":
				event.name = "StrengthBoulder";
				break;
			default:
				return false;
				break;
		}
		return true;
	}

	public void replace_scripts(script) {
		ret = false;
		SCRIPT_REPLACEMENTS.each(pair => { script = System.Text.RegularExpressions.Regex.Replace(script, pair[0], pair[1]); if (script) ret = true; });
		script = System.Text.RegularExpressions.Regex.Replace(script, "Game.GameData.game_variables\[(\d+)\](?!\s*(?:\=|\!|<|>))", m => "Get(" + $~[1] + ")");	if (script) ret = true;
		script = System.Text.RegularExpressions.Regex.Replace(script, "Game.GameData.player\.party\[\s*Get\((\d+)\)\s*\]", m => "GetPokemon(" + $~[1] + ")"); 	if (script) ret = true;
		return ret;
	}

	public void fix_event_scripts(event) {
		if (event_is_empty(event)) return false;
		ret = false;
		foreach (var page in EachPage(event)) { //EachPage(event) do => |page|
			foreach (var cmd in page.list) { //'page.list.each' do => |cmd|
				params = cmd.parameters;
				switch (cmd.code) {
					case 355: case 655:   // Script (first line, continuation line)
						if (params[0].is_a(String) && replace_scripts(params[0])) ret = true;
						break;
					case 111:   // Conditional Branch
						if (params[0] == 12 && replace_scripts(params[1])) ret = true;
						break;
				}
			}
		}
		return ret;
	}

	// Splits the given code string into an array of parameters (all strings),
	// using "," as the delimiter. It will not split in the middle of a string
	// parameter. Used to extract parameters from a script call in an event.
	public void split_string_with_quotes(str) {
		ret = new List<string>();
		new_str = "";
		in_msg = false;
		foreach (var s in str.scan(/./)) { //str.scan(/./) do => |s|
			if (s == "," && !in_msg) {
				ret.Add(new_str.strip);
				new_str = "";
			} else {
				if (s == "\"") in_msg = !in_msg;
				new_str += s;
			}
		}
		new_str.strip!;
		if (!new_str.empty()) ret.Add(new_str);
		return ret;
	}

	public void replace_old_battle_scripts(event, list, index) {
		changed = false;
		script = list[index].parameters[1];
		if (System.Text.RegularExpressions.Regex.IsMatch(script,@"^\s*WildBattle\((.+)\)\s*$")) {
			battle_params = split_string_with_quotes(Game.GameData.1);   // Split on commas
			list[index].parameters[1] = string.Format($"WildBattle.start({battle_params[0]}, {battle_params[1]})");
			old_indent = list[index].indent;
			new_events = new List<string>();
			if (battle_params[3] && System.Text.RegularExpressions.Regex.IsMatch(battle_params[3],@"false")) {
				push_script(new_events, "setBattleRule(\"cannotRun\")", old_indent);
			}
			if (battle_params[4] && System.Text.RegularExpressions.Regex.IsMatch(battle_params[4],@"true")) {
				push_script(new_events, "setBattleRule(\"canLose\")", old_indent);
			}
			if (battle_params[2] && battle_params[2] != "1") {
				push_script(new_events, "setBattleRule(\"outcome\$", {battle_params[2]})", old_indent);
			}
			if (new_events.length > 0) list[index, 0] = new_events;
			changed = true;
		} else if (System.Text.RegularExpressions.Regex.IsMatch(script,@"^\s*DoubleWildBattle\((.+)\)\s*$")) {
			battle_params = split_string_with_quotes(Game.GameData.1);   // Split on commas
			pkmn1 = $"{battle_params[0]}, {battle_params[1]}";
			pkmn2 = $"{battle_params[2]}, {battle_params[3]}";
			list[index].parameters[1] = string.Format($"WildBattle.start({pkmn1}, {pkmn2})");
			old_indent = list[index].indent;
			new_events = new List<string>();
			if (battle_params[5] && System.Text.RegularExpressions.Regex.IsMatch(battle_params[5],@"false")) {
				push_script(new_events, "setBattleRule(\"cannotRun\")", old_indent);
			}
			if (battle_params[6] && System.Text.RegularExpressions.Regex.IsMatch(battle_params[6],@"true")) {
				push_script(new_events, "setBattleRule(\"canLose\")", old_indent);
			}
			if (battle_params[4] && battle_params[4] != "1") {
				push_script(new_events, "setBattleRule(\"outcome\$", {battle_params[4]})", old_indent);
			}
			if (new_events.length > 0) list[index, 0] = new_events;
			changed = true;
		} else if (System.Text.RegularExpressions.Regex.IsMatch(script,@"^\s*TripleWildBattle\((.+)\)\s*$")) {
			battle_params = split_string_with_quotes(Game.GameData.1);   // Split on commas
			pkmn1 = $"{battle_params[0]}, {battle_params[1]}";
			pkmn2 = $"{battle_params[2]}, {battle_params[3]}";
			pkmn3 = $"{battle_params[4]}, {battle_params[5]}";
			list[index].parameters[1] = string.Format($"WildBattle.start({pkmn1}, {pkmn2}, {pkmn3})");
			old_indent = list[index].indent;
			new_events = new List<string>();
			if (battle_params[7] && System.Text.RegularExpressions.Regex.IsMatch(battle_params[7],@"false")) {
				push_script(new_events, "setBattleRule(\"cannotRun\")", old_indent);
			}
			if (battle_params[8] && System.Text.RegularExpressions.Regex.IsMatch(battle_params[8],@"true")) {
				push_script(new_events, "setBattleRule(\"canLose\")", old_indent);
			}
			if (battle_params[6] && battle_params[6] != "1") {
				push_script(new_events, "setBattleRule(\"outcome\$", {battle_params[6]})", old_indent);
			}
			if (new_events.length > 0) list[index, 0] = new_events;
			changed = true;
		} else if (System.Text.RegularExpressions.Regex.IsMatch(script,@"^\s*TrainerBattle\((.+)\)\s*$")) {
			battle_params = split_string_with_quotes(Game.GameData.1);   // Split on commas
			trainer1 = $"{battle_params[0]}, {battle_params[1]}";
			if (battle_params[4] && battle_params[4] != "null") trainer1 += $", {battle_params[4]}";
			list[index].parameters[1] = $"TrainerBattle.start({trainer1})";
			old_indent = list[index].indent;
			new_events = new List<string>();
			if (battle_params[2] && !battle_params[2].empty() && battle_params[2] != "null") {
				speech = System.Text.RegularExpressions.Regex.Replace(battle_params[2], "^\s*_I\(\s*"\s*", ""); speech = System.Text.RegularExpressions.Regex.Replace(speech, "\"\s*\)\s*$", "");
				push_comment(new_events, $"EndSpeech: {speech.strip}", old_indent);
			}
			if (battle_params[3] && System.Text.RegularExpressions.Regex.IsMatch(battle_params[3],@"true")) {
				push_script(new_events, "setBattleRule(\"double\")", old_indent);
			}
			if (battle_params[5] && System.Text.RegularExpressions.Regex.IsMatch(battle_params[5],@"true")) {
				push_script(new_events, "setBattleRule(\"canLose\")", old_indent);
			}
			if (battle_params[6] && battle_params[6] != "1") {
				push_script(new_events, "setBattleRule(\"outcome\$", {battle_params[6]})", old_indent);
			}
			if (new_events.length > 0) list[index, 0] = new_events;
			changed = true;
		} else if (System.Text.RegularExpressions.Regex.IsMatch(script,@"^\s*DoubleTrainerBattle\((.+)\)\s*$")) {
			battle_params = split_string_with_quotes(Game.GameData.1);   // Split on commas
			trainer1 = $"{battle_params[0]}, {battle_params[1]}";
			if (battle_params[2] && battle_params[2] != "null") trainer1 += $", {battle_params[2]}";
			trainer2 = $"{battle_params[4]}, {battle_params[5]}";
			if (battle_params[6] && battle_params[6] != "null") trainer2 += $", {battle_params[6]}";
			list[index].parameters[1] = $"TrainerBattle.start({trainer1}, {trainer2})";
			old_indent = list[index].indent;
			new_events = new List<string>();
			if (battle_params[3] && !battle_params[3].empty() && battle_params[3] != "null") {
				speech = System.Text.RegularExpressions.Regex.Replace(battle_params[3], "^\s*_I\(\s*\"\s*", ""); speech = System.Text.RegularExpressions.Regex.Replace(speech, "\"\s*\)\s*$", "");
				push_comment(new_events, $"EndSpeech1: {speech.strip}", old_indent);
			}
			if (battle_params[7] && !battle_params[7].empty() && battle_params[7] != "null") {
				speech = System.Text.RegularExpressions.Regex.Replace(battle_params[7], "^\s*_I\(\s*\"\s*", ""); speech = System.Text.RegularExpressions.Regex.Replace(speech, "\"\s*\)\s*$", "");
				push_comment(new_events, $"EndSpeech2: {speech.strip}", old_indent);
			}
			if (battle_params[8] && System.Text.RegularExpressions.Regex.IsMatch(battle_params[8],@"true")) {
				push_script(new_events, "setBattleRule(\"canLose\")", old_indent);
			}
			if (battle_params[9] && battle_params[9] != "1") {
				push_script(new_events, "setBattleRule(\"outcome\$", {battle_params[9]})", old_indent);
			}
			if (new_events.length > 0) list[index, 0] = new_events;
			changed = true;
		} else if (System.Text.RegularExpressions.Regex.IsMatch(script,@"^\s*TripleTrainerBattle\((.+)\)\s*$")) {
			battle_params = split_string_with_quotes(Game.GameData.1);   // Split on commas
			trainer1 = $"{battle_params[0]}, {battle_params[1]}";
			if (battle_params[2] && battle_params[2] != "null") trainer1 += $", {battle_params[2]}";
			trainer2 = $"{battle_params[4]}, {battle_params[5]}";
			if (battle_params[6] && battle_params[6] != "null") trainer2 += $", {battle_params[6]}";
			trainer3 = $"{battle_params[8]}, {battle_params[9]}";
			if (battle_params[10] && battle_params[10] != "null") trainer3 += $", {battle_params[10]}";
			list[index].parameters[1] = $"TrainerBattle.start({trainer1}, {trainer2}, {trainer3})";
			old_indent = list[index].indent;
			new_events = new List<string>();
			if (battle_params[3] && !battle_params[3].empty() && battle_params[3] != "null") {
				speech = System.Text.RegularExpressions.Regex.Replace(battle_params[3], "^\s*_I\(\s*\"\s*", ""); speech = System.Text.RegularExpressions.Regex.Replace(speech, "\"\s*\)\s*$", "");
				push_comment(new_events, $"EndSpeech1: {speech.strip}", old_indent);
			}
			if (battle_params[7] && !battle_params[7].empty() && battle_params[7] != "null") {
				speech = System.Text.RegularExpressions.Regex.Replace(battle_params[7], "^\s*_I\(\s*\"\s*", ""); speech = System.Text.RegularExpressions.Regex.Replace(speech, "\"\s*\)\s*$", "");
				push_comment(new_events, $"EndSpeech2: {speech.strip}", old_indent);
			}
			if (battle_params[11] && !battle_params[11].empty() && battle_params[11] != "null") {
				speech = System.Text.RegularExpressions.Regex.Replace(battle_params[11], "^\s*_I\(\s*\"\s*", ""); speech = System.Text.RegularExpressions.Regex.Replace(speech, "\"\s*\)\s*$", "");
				push_comment(new_events, $"EndSpeech3: {speech.strip}", old_indent);
			}
			if (battle_params[12] && System.Text.RegularExpressions.Regex.IsMatch(battle_params[12],@"true")) {
				push_script(new_events, "setBattleRule(\"canLose\")", old_indent);
			}
			if (battle_params[13] && battle_params[13] != "1") {
				push_script(new_events, "setBattleRule(\"outcome\$", {battle_params[13]})", old_indent);
			}
			if (new_events.length > 0) list[index, 0] = new_events;
			changed = true;
		}
		return changed;
	}

	public void fix_event_use(event, _mapID, mapData) {
		if (event_is_empty(event)) return null;
		changed = false;
		trainerMoneyRE = @"^\s*\Game.GameData.player\.money\s*(<|<=|>|>=)\s*(\d+)\s*$";
		itemBallRE     = @"^\s*(Kernel\.)?ItemBall";
		// Rewrite event if it looks like a door
		if (update_door_event(event, mapData)) changed = true;
		// Check through each page of the event in turn
		foreach (var page in EachPage(event)) { //EachPage(event) do => |page|
			i = 0;
			list = page.list;
			while (i < list.length) {
				params = list[i].parameters;
				switch (list[i].code) {
//					case 655:   // Script (continuation line)
					case 355:   // Script (first line)
						lastScript = i;
						if (!params[0].is_a(String)) {
							i += 1;
							continue;
						}
						// Check if the script is an old way of healing the entire party, and if
						// so, replace it with a better version that uses event commands
						if (params[0][0, 1] != "f" && params[0][0, 1] != "p" && params[0][0, 1] != "K") {
							i += 1;
							continue;
						}
						// Script begins with "f" (for...), "p" (Method) or "K" (Kernel.)
						script = " " + params[0];
						j = i + 1;
						while (j < list.length) {
							if (list[j].code != 655) break;   // Script (continuation line)
							script += list[j].parameters[0];
							lastScript = j;
							j += 1;
						}
						script = System.Text.RegularExpressions.Regex.Replace(script, "\s+", "");
						// Using old method of recovering
						switch (script) {
							case "foriinGame.GameData.player.partyi.healend":
								(lastScript - i).times(() => list.delete_at(i))
								list.insert(i,
														new RPG.EventCommand(314, list[i].indent, [0]));   // Recover All
								changed = true;
								break;
							case "FadeOutIn(99999) {foriinGame.GameData.player.partyi.healend}":
								oldIndent = list[i].indent;
								(lastScript - i).times(() => list.delete_at(i))
								list.insert(
									i,
									new RPG.EventCommand(223, oldIndent, new {new Tone(-255, -255, -255), 6}),   // Fade to black
									new RPG.EventCommand(106, oldIndent, [6]),                               // Wait
									new RPG.EventCommand(314, oldIndent, [0]),                               // Recover All
									new RPG.EventCommand(223, oldIndent, new {new Tone(0, 0, 0), 6}),            // Fade to normal
									new RPG.EventCommand(106, oldIndent, [6]);                                // Wait
								);
								changed = true;
								break;
						}
						break;
					case 108:   // Comment (first line)
						// Replace a "SellItem:POTION,200" comment with event commands that do so
						if (System.Text.RegularExpressions.Regex.IsMatch(params[0],@"SellItem\s*\(\s*(\w+)\s*\,\s*(\d+)\s*\)")) {
							itemname = Game.GameData.1;
							cost     = Game.GameData.2.ToInt();
							if (GameData.Item.exists(itemname)) {
								oldIndent = list[i].indent;
								list.delete_at(i);
								newEvents = new List<string>();
								if (cost == 0) {
									push_branch(newEvents, $"Game.GameData.bag.can_add(:{itemname})", oldIndent);
									push_text(newEvents, _INTL("Here you go!"), oldIndent + 1);
									push_script(newEvents, $"ReceiveItem(:{itemname})", oldIndent + 1);
									push_else(newEvents, oldIndent + 1);
									push_text(newEvents, _INTL("You have no room left in the Bag."), oldIndent + 1);
								} else {
									push_event(newEvents, 111, new {7, cost, 0}, oldIndent);
									push_branch(newEvents, $"Game.GameData.bag.can_add(:{itemname})", oldIndent + 1);
									push_event(newEvents, 125, new {1, 0, cost}, oldIndent + 2);
									push_text(newEvents, _INTL("\\GHere you go!"), oldIndent + 2);
									push_script(newEvents, $"ReceiveItem(:{itemname})", oldIndent + 2);
									push_else(newEvents, oldIndent + 2);
									push_text(newEvents, _INTL("\\GYou have no room left in the Bag."), oldIndent + 2);
									push_branch_end(newEvents, oldIndent + 2);
									push_else(newEvents, oldIndent + 1);
									push_text(newEvents, _INTL("\\GYou don't have enough money."), oldIndent + 1);
								}
								push_branch_end(newEvents, oldIndent + 1);
								list[i, 0] = newEvents;   // insert 'newEvents' at index 'i'
								changed = true;
							}
						}
						break;
					case 115:   // Exit Event Processing
						if (i == list.length - 2) {
							// Superfluous exit command, delete it
							list.delete_at(i);
							changed = true;
						}
						break;
					case 201:   // Transfer Player
						if (list.length <= 8) {
=begin;
							if (params[0]==0) {
								// Look for another event just above the position this Transfer
								// Player command will transfer to - it may be a door, in which case
								// this command should transfer the player onto the door instead of
								// in front of it.
								e = mapData.getEventFromXY(params[1],params[2],params[3]-1);
								// This bit of code is just in case Switch 22 has been renamed/
								// repurposed, which is highly unlikely. It changes the Switch used
								// in the found event's condition to whichever is named
								// 's:tsOff("A")'.
								if ((e && e.pages.length>=2 &&
									e.pages[e.pages.length-1].condition.switch1_valid &&
									e.pages[e.pages.length-1].condition.switch1_id==22 &&
									mapData.switchName(e.pages[e.pages.length-1].condition.switch1_id)!='s:tsOff("A")' &&
									e.pages[e.pages.length-1].list.length>5 &&
									e.pages[e.pages.length-1].list[0].code==111) {   // Conditional Branch
									e.pages[e.pages.length-1].condition.switch1_id = mapData.registerSwitch('s:tsOff("A")');
									mapData.saveMap(params[1]);
									changed = true;
								}
								// Checks if the found event is a simple Transfer Player one nestled
								// between tiles that aren't passable - it is likely a door, so give
								// it a second page with an "is player on me?" check.
								if (likely_passage(e,params[1],mapData)) {   // Checks the first page
									add_passage_list(e,mapData);
									mapData.saveMap(params[1]);
									changed = true;
								}
								// If the found event's last page's Switch condition uses a Switch
								// named 's:tsOff("A")', it really does look like a door. Make this
								// command transfer the player on top of it rather than in front of
								// it.
								if ((e && e.pages.length>=2 &&
									e.pages[e.pages.length-1].condition.switch1_valid &&
									mapData.switchName(e.pages[e.pages.length-1].condition.switch1_id)=='s:tsOff("A")') {
									// If this is really a door, move transfer target to it
									params[3] -= 1;   // Move this command's destination up 1 tile (onto the found event)
									params[5]  = 1;   // No fade (the found event should take care of that)
									changed = true;
								}
								deletedRoute = null;
								deleteMoveRouteAt = block: (list,i) => {
									arr = new List<string>();
									if (list[i] && list[i].code==209) {   // Set Move Route
										arr.Add(list[i]);
										list.delete_at(i);
										while (i<list.length) {
											if (!list[i] || list[i].code!=509) break;   // Set Move Route (continuation line)
											arr.Add(list[i]);
											list.delete_at(i);
										}
									}
									next arr;
								}
								insertMoveRouteAt = block: (list,i,route) => {
									j = route.length-1;
									while (j>=0) {
										list.insert(i,route[j]);
										j -= 1;
									}
								}
								// If the next event command is a Move Route that moves the player,
								// check whether all it does is turn the player in a direction (or
								// its first item is to move the player in a direction). If so, this
								// Transfer Player command may as well set the player's direction
								// instead; make it do so and delete that Move Route.
								if (params[4]==0 &&   // Retain direction
									i+1<list.length && list[i+1].code==209 && list[i+1].parameters[0]==-1) {   // Set Move Route
									route = list[i+1].parameters[1];
									if (route && route.list.length<=2) {
										// Delete superfluous move route command if necessary
										if (route.list[0].code==16) {      // Player Turn Down
											deleteMoveRouteAt.call(list,i+1);
											params[4] = 2;
											changed = true;
										} else if (route.list[0].code==17) {   // Player Turn Left
											deleteMoveRouteAt.call(list,i+1);
											params[4] = 4;
											changed = true;
										} else if (route.list[0].code==18) {   // Player Turn Right
											deleteMoveRouteAt.call(list,i+1);
											params[4] = 6;
											changed = true;
										} else if (route.list[0].code==19) {   // Player Turn Up
											deleteMoveRouteAt.call(list,i+1);
											params[4] = 8;
											changed = true;
										} else if ((route.list[0].code==1 || route.list[0].code==2 ||   // Player Move (4-dir)
											route.list[0].code==3 || route.list[0].code==4) && list.length==4) {
											params[4] = new {0,2,4,6,8}[route.list[0].code];
											deletedRoute = deleteMoveRouteAt.call(list,i+1);
											changed = true;
										}
									}
								// If an event command before this one is a Move Route that just
								// turns the player, delete it and make this Transfer Player command
								// set the player's direction instead.
								// (I don't know if it makes sense to do this, as there could be a
								// lot of commands between then and this Transfer Player which this
								// code can't recognise and deal with, so I've quoted this code out.)
								} else if (params[4]==0 && i>3) {   // Retain direction
//									for (int j = 0; j < i; j++) {
//									  if (list[j].code==209 && list[j].parameters[0]==-1) {   // Set Move Route
//									    route = list[j].parameters[1]
//									    if (route && route.list.length<=2) {
//									      oldlistlength = list.length
//									      // Delete superfluous move route command if necessary
//									      if (route.list[0].code==16) {      // Player Turn Down
//									        deleteMoveRouteAt.call(list,j)
//									        params[4] = 2
//									        changed = true
//									        i -= (oldlistlength-list.length)
//									      } else if (route.list[0].code==17) {   // Player Turn Left
//									        deleteMoveRouteAt.call(list,j)
//									        params[4] = 4
//									        changed = true
//									        i -= (oldlistlength-list.length)
//									      } else if (route.list[0].code==18) {   // Player Turn Right
//									        deleteMoveRouteAt.call(list,j)
//									        params[4] = 6
//									        changed = true
//									        i -= (oldlistlength-list.length)
//									      } else if (route.list[0].code==19) {   // Player Turn Up
//									        deleteMoveRouteAt.call(list,j)
//									        params[4] = 8
//									        changed = true
//									        i -= (oldlistlength-list.length)
//									      }
//									    }
//									  }
//									}
								// If the next event command changes the screen color, and the one
								// after that is a Move Route which only turns the player in a
								// direction, this Transfer Player command may as well set the
								// player's direction instead; make it do so and delete that Move
								// Route.
								} else if (params[4]==0 &&   // Retain direction
									i+2<list.length &&
									list[i+1].code==223 &&   // Change Screen Color Tone) {
									list[i+2].code==209 &&   // Set Move Route
									list[i+2].parameters[0]==-1;
									route = list[i+2].parameters[1];
									if (route && route.list.length<=2) {
										// Delete superfluous move route command if necessary
										if (route.list[0].code==16) {      // Player Turn Down
											deleteMoveRouteAt.call(list,i+2);
											params[4] = 2;
											changed = true;
										} else if (route.list[0].code==17) {   // Player Turn Left
											deleteMoveRouteAt.call(list,i+2);
											params[4] = 4;
											changed = true;
										} else if (route.list[0].code==18) {   // Player Turn Right
											deleteMoveRouteAt.call(list,i+2);
											params[4] = 6;
											changed = true;
										} else if (route.list[0].code==19) {   // Player Turn Up
											deleteMoveRouteAt.call(list,i+2);
											params[4] = 8;
											changed = true;
										}
									}
								}
							}
=end;
							// If this is the only event command, convert to a full event
							if (list.length == 2 || (list.length == 3 && (list[0].code == 250 || list[1].code == 250))) {   // Play SE
								params[5] = 1;   // No fade
								fullTransfer = list[i];
								indent = list[i].indent;
								(list.length - 1).times(() => list.delete_at(0))
								list.insert(
									0,
									new RPG.EventCommand(250, indent, new {new RPG.AudioFile("Exit Door", 80, 100)}),   // Play SE
									new RPG.EventCommand(223, indent, new {new Tone(-255, -255, -255), 6}),              // Fade to black
									new RPG.EventCommand(106, indent, [8]),                                          // Wait
									fullTransfer,                                                                     // Transfer event
									new RPG.EventCommand(223, indent, new {new Tone(0, 0, 0), 6});                        // Fade to normal
								);
								changed = true;
							}
//							if (deletedRoute) {
//								insertMoveRouteAt.call(list,list.length-1,deletedRoute)
//								changed = true
//							}
						}
						break;
					case 101:   // Show Text
						// Capitalise/decapitalise various text formatting codes
						if (list[i].parameters[0]new {0, 1} == "\\") {
							newx = list[i].parameters[0].clone;
							newx = System.Text.RegularExpressions.Regex.Replace(newx, "^\\[Bb]\s+", "\\b");
							newx = System.Text.RegularExpressions.Regex.Replace(newx, "^\\[Rr]\s+", "\\r");
							newx = System.Text.RegularExpressions.Regex.Replace(newx, "^\\[Pp][Gg]\s+", "\\pg");
							newx = System.Text.RegularExpressions.Regex.Replace(newx, "^\\[Pp][Oo][Gg]\s+", "\\pog");
							newx = System.Text.RegularExpressions.Regex.Replace(newx, "^\\[Gg]\s+", "\\G");
							newx = System.Text.RegularExpressions.Regex.Replace(newx, "^\\[Cc][Nn]\s+", "\\CN");
							if (list[i].parameters[0] != newx) {
								list[i].parameters[0] = newx;
								changed = true;
							}
						}
						// Split Show Text commands with 5+ lines into multiple Show Text
						// commands each with a maximum of 4 lines
						lines = 1;
						j = i + 1;
						while (j < list.length) {
							if (list[j].code != 401) break;   // Show Text (continuation line)
							if (lines % 4 == 0) {
								list[j].code = 101;   // Show Text
								changed = true;
							}
							lines += 1;
							j += 1;
						}
						// If this Show Text command has 2+ lines of text but not much actual
						// text in the first line, merge the second line into it
						if ((lines >= 2 && list[i].parameters[0].length > 0 && list[i].parameters[0].length <= 20 &&
							!System.Text.RegularExpressions.Regex.IsMatch(list[i].parameters[0],@"\\n")) {
							// Very short line
							list[i].parameters[0] += "\\n" + list[i + 1].parameters[0];
							list.delete_at(i + 1);
							i -= 1;   // revisit this text command
							changed = true;
						// Check whether this Show Text command has 3+ lines and the next command
						// is also a Show Text
						} else if (lines >= 3 && list[i + lines] && list[i + lines].code == 101) {   // Show Text
							// Check whether a sentence is being broken midway between two Text
							// commands (i.e. the first Show Text doesn't end in certain punctuation)
							lastLine = System.Text.RegularExpressions.Regex.Replace(list[i + lines - 1].parameters[0], "\s+$", "", count: 1);
							if lastLine.length > 0 && !System.Text.RegularExpressions.Regex.IsMatch(lastLine,@"[\\<]") && System.Text.RegularExpressions.Regex.IsMatch(lastLine,@"[^\.,\!\?\;\-\"]$")
								message = list[i].parameters[0];
								j = i + 1;
								while (j < list.length) {
									if (list[j].code != 401) break;   // Show Text (continuation line)
									message += "\n" + list[j].parameters[0];
									j += 1;
								}
								// Find a punctuation mark to split at
								punct = new {message.rindex(". "), message.rindex(".\n"),
												message.rindex("!"), message.rindex("?"), -1}.compact.max;
								if (punct == -1) {
									punct = new {message.rindex(", "), message.rindex(",\n"), -1}.compact.max;
								}
								if (punct != -1) {
									// Delete old message
									indent = list[i].indent;
									newMessage  = message[0, punct + 1].split("\n");
									nextMessage = System.Text.RegularExpressions.Regex.Replace(message[punct + 1, message.length], "^\s+", "", count: 1).split("\n");
									list[i + lines].code = 401;
									lines.times(() => list.delete_at(i));
									j = nextMessage.length - 1;
									while (j >= 0) {
										list.insert(i, new RPG.EventCommand((j == 0) ? 101 : 401, indent, [nextMessage[j]]));
										j -= 1;
									}
									j = newMessage.length - 1;
									while (j >= 0) {
										list.insert(i, new RPG.EventCommand((j == 0) ? 101 : 401, indent, [newMessage[j]]));
										j -= 1;
									}
									changed = true;
									i += 1;
									continue;
								}
							}
						}
						break;
					case 111:   // Conditional Branch
						if (list[i].parameters[0] == 12) {   // script
							script = list[i].parameters[1];
							if (replace_old_battle_scripts(event, list, i)) changed = true;
							if (script[trainerMoneyRE]) {   // Compares Game.GameData.player.money with a value
								// Checking money directly
								operator = Game.GameData.1;
								amount   = Game.GameData.2.ToInt();
								switch (operator) {
									case "<":
										params[0] = 7;   // gold
										params[2] = 1;
										params[1] = amount - 1;
										changed = true;
										break;
									case "<=":
										params[0] = 7;   // gold
										params[2] = 1;
										params[1] = amount;
										changed = true;
										break;
									case ">":
										params[0] = 7;   // gold
										params[2] = 0;
										params[1] = amount + 1;
										changed = true;
										break;
									case ">=":
										params[0] = 7;   // gold
										params[2] = 0;
										params[1] = amount;
										changed = true;
										break;
								}
							} else if (script[itemBallRE] && i > 0) {   // Contains ItemBall after another command
								// Using ItemBall on non-item events, change it
								list[i].parameters[1] = System.Text.RegularExpressions.Regex.Replace(script, "ItemBall", "ReceiveItem", count: 1);
								changed = true;
							} else if (System.Text.RegularExpressions.Regex.IsMatch(script,@"^\s*(TrainerBattle.start)")) {
								// Check if trainer battle conditional branch is empty
								j = i + 1;
								isempty = true;
								elseIndex = -1;
								// Check if page is empty
								while (j < page.list.length) {
									if (list[j].indent <= list[i].indent) {
										if (list[j].code == 411) {   // Else
											elseIndex = j;
										} else {
											break;   // Reached end of Conditional Branch
										}
									}
									if (list[j].code != 0 && list[j].code != 411) {   // Else
										isempty = false;
										break;
									}
									j += 1;
								}
								if (isempty) {
									if (elseIndex >= 0) {
										list.insert(
											elseIndex + 1,
											new RPG.EventCommand(115, list[i].indent + 1, new List<string>());   // Exit Event Processing
										);
									} else {
										list.insert(
											i + 1,
											new RPG.EventCommand(0, list[i].indent + 1, new List<string>()),    // Empty Event
											new RPG.EventCommand(411, list[i].indent, new List<string>()),      // Else
											new RPG.EventCommand(115, list[i].indent + 1, new List<string>());   // Exit Event Processing
										);
									}
									changed = true;
								}
							}
						}
						break;
				}
				i += 1;
			}
		}
		return (changed) ? event : null;
	}

	//-----------------------------------------------------------------------------
	// Convert events used as counters into proper counters.
	//-----------------------------------------------------------------------------
	// Checks if the event has just 1 page, which has no conditions and no commands
	// and whose movement type is "Fixed".
	public bool plain_event(event) {
		unless (event) return false;
		if (event.pages.length > 1) return false;
		if (event.pages[0].move_type != 0) return false;
		if ((event.pages[0].condition.switch1_valid ||
										event.pages[0].condition.switch2_valid ||
										event.pages[0].condition.variable_valid ||
										event.pages[0].condition.self_switch_valid) return false;
		if (event.pages[0].list.length <= 1) return true;
		return false;
	}

	// Checks if the event has just 1 page, which has no conditions and whose
	// movement type is "Fixed". Then checks if there are no commands, or it looks
	// like a simple Mart or a Poké Center nurse event.
	public bool plain_event_or_mart(event) {
		unless (event) return false;
		if (event.pages.length > 1) return false;
		if (event.pages[0].move_type != 0) return false;
		if ((event.pages[0].condition.switch1_valid ||
										event.pages[0].condition.switch2_valid ||
										event.pages[0].condition.variable_valid ||
										event.pages[0].condition.self_switch_valid) return false;
		// No commands in the event
		if (event.pages[0].list.length <= 1) return true;
		// PokemonMart events
		if ((event.pages[0].list.length <= 12 &&
									event.pages[0].graphic.character_name != "" &&   // Has charset
									event.pages[0].list[0].code == 355 &&   // First line is Script
									System.Text.RegularExpressions.Regex.IsMatch(event.pages[0].list[0].parameters[0],@"^PokemonMart")) return true;
		// SetPokemonCenter events
		if ((event.pages[0].list.length > 8 &&
									event.pages[0].graphic.character_name != "" &&   // Has charset) return true;
									event.pages[0].list[0].code == 355 &&   // First line is Script
									System.Text.RegularExpressions.Regex.IsMatch(event.pages[0].list[0].parameters[0],@"^SetPokemonCenter")) {
		return false;
	}

	// Given two events that are next to each other, decides whether otherEvent is
	// likely to be a "counter event", i.e. is placed on a tile with the Counter
	// flag, or is on a non-passable tile between two passable tiles (e.g. a desk)
	// where one of those two tiles is occupied by thisEvent.
	public bool likely_counter(thisEvent, otherEvent, mapID, mapData) {
		// Check whether otherEvent is on a counter tile
		if (mapData.isCounterTile(mapID, otherEvent.x, otherEvent.y)) return true;
		// Check whether otherEvent is between an event with a graphic (e.g. an NPC)
		// and a spot where the player can be
		yonderX = otherEvent.x + (otherEvent.x - thisEvent.x);
		yonderY = otherEvent.y + (otherEvent.y - thisEvent.y);
		if (thisEvent.pages[0].graphic.character_name != "" &&    // Has charset
					otherEvent.pages[0].graphic.character_name == "" &&   // Has no charset
					otherEvent.pages[0].trigger == 0 &&                   // Action trigger
					mapData.isPassable(mapID, thisEvent.x, thisEvent.y) &&
					!mapData.isPassable(mapID, otherEvent.x, otherEvent.y) &&
					mapData.isPassable(mapID, yonderX, yonderY) return;
	}

	// Checks all events in the given map to see if any look like they've been
	// placed on a desk with an NPC behind it, where the event on the desk is the
	// actual interaction with the NPC. In other words, it's not making proper use
	// of the counter flag (which lets the player interact with an event on the
	// other side of counter tiles).
	// Any events found to be like this have their contents merged into the NPC
	// event and the counter event itself is deleted. The tile below the counter
	// event gets its counter flag set (if it isn't already).
	public void check_counters(map, mapID, mapData) {
		toDelete = new List<string>();
		changed = false;
		foreach (var key in map.events) { //map.events.each_key do => |key|
			event = map.events[key];
			if (!plain_event_or_mart(event)) continue;
			// Found an event that is empty or looks like a simple Mart or a Poké
			// Center nurse. Check adjacent events to see if they are "counter events".
			neighbors = new List<string>();
			neighbors.Add(mapData.getEventFromXY(mapID, event.x, event.y - 1));
			neighbors.Add(mapData.getEventFromXY(mapID, event.x, event.y + 1));
			neighbors.Add(mapData.getEventFromXY(mapID, event.x - 1, event.y));
			neighbors.Add(mapData.getEventFromXY(mapID, event.x + 1, event.y));
			neighbors.compact!;
			foreach (var otherEvent in neighbors) { //'neighbors.each' do => |otherEvent|
				if (plain_event(otherEvent)) continue;   // Blank/cosmetic-only event
				if (!likely_counter(event, otherEvent, mapID, mapData)) continue;
				// Found an adjacent event that looks like it's supposed to be a counter.
				// Set the counter flag of the tile beneath the counter event, copy the
				// counter event's pages to the NPC event, and delete the counter event.
				mapData.setCounterTile(mapID, otherEvent.x, otherEvent.y);
				savedPage = event.pages[0];
				event.pages = otherEvent.pages;
				apply_pages(savedPage, event.pages);   // Apply NPC's visuals to new event pages
				toDelete.Add(otherEvent.id);
				changed = true;
			}
		}
		toDelete.each(key => map.events.delete(key));
		return changed;
	}

	//-----------------------------------------------------------------------------
	// Main compiler method for events.
	//-----------------------------------------------------------------------------
	public void compile_trainer_events() {
		mapData = new MapData();
		t = System.uptime;
		Graphics.update;
		trainerChecker = new TrainerChecker();
		change_record = new List<string>();
		Console.echo_li(_INTL("Processing {1} maps...", mapData.mapinfos.keys.length));
		idx = 0;
		foreach (var id in mapData.mapinfos.keys.sort) { //'mapData.mapinfos.keys.sort.each' do => |id|
			if (idx % 100 == 0) echo ".";
			idx += 1;
			if (idx % 500 == 0) Graphics.update;
			changed = false;
			map = mapData.getMap(id);
			if (!map || !mapData.mapinfos[id]) continue;
			foreach (var key in map.events) { //map.events.each_key do => |key|
				if (System.uptime - t >= 5) {
					t += 5;
					Graphics.update;
				}
				newevent = convert_to_trainer_event(map.events[key], trainerChecker);
				if (newevent) {
					map.events[key] = newevent;
					changed = true;
				}
				newevent = convert_to_item_event(map.events[key]);
				if (newevent) {
					map.events[key] = newevent;
					changed = true;
				}
				if (fix_event_name(map.events[key])) changed = true;
				if (fix_event_scripts(map.events[key])) changed = true;
				newevent = fix_event_use(map.events[key], id, mapData);
				if (newevent) {
					map.events[key] = newevent;
					changed = true;
				}
			}
			if (System.uptime - t >= 5) {
				t += 5;
				Graphics.update;
			}
			if (check_counters(map, id, mapData)) changed = true;
			if (changed) {
				mapData.saveMap(id);
				mapData.saveTilesets;
				change_record.Add(_INTL("Map {1}: '{2}' was modified and saved.", id, mapData.mapinfos[id].name));
			}
		}
		Console.echo_done(true);
		change_record.each(msg => Console.echo_warn(msg));
		changed = false;
		Graphics.update;
		commonEvents = load_data("Data/CommonEvents.rxdata");
		Console.echo_li(_INTL("Processing common events..."));
		for (int key = commonEvents.length; key < commonEvents.length; key++) { //for 'commonEvents.length' times do => |key|
			newevent = fix_event_use(commonEvents[key], 0, mapData);
			if (newevent) {
				commonEvents[key] = newevent;
				changed = true;
			}
		}
		if (changed) save_data(commonEvents, "Data/CommonEvents.rxdata");
		Console.echo_done(true);
		if (change_record.length > 0 || changed) {
			Console.echo_warn(_INTL("RMXP data was altered. Close RMXP now to ensure changes are applied."));
		}
	}
}
