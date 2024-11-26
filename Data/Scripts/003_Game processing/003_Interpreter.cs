//===============================================================================
// ** Interpreter
//-------------------------------------------------------------------------------
//  This interpreter runs event commands. This class is used within the
//  Game_System class and the Game_Event class.
//===============================================================================
public partial class Interpreter {
	// Object Initialization
	//     depth : nest depth
	//     main  : main flag
	public void initialize(depth = 0, main = false) {
		@depth = depth;
		@main  = main;
		if (depth > 100) {
			print("Common event call has exceeded maximum limit.");
			exit;
		}
		clear;
	}

	public override void inspect() {
		str = base.inspect().chop;
		str << string.Format(" @event_id: {0}>", @event_id);
		return str;
	}

	public void clear() {
		@map_id             = 0;       // map ID when starting up
		@event_id           = 0;
		@message_waiting    = false;   // waiting for message to end
		@move_route_waiting = false;   // waiting for move completion
		@wait_count         = 0;
		@wait_start         = null;
		@child_interpreter  = null;
		@branch             = new List<string>();
		@buttonInput        = false;
		@hidden_choices     = new List<string>();
		@renamed_choices    = new List<string>();
		end_follower_overrides;
	}

	// Event Setup
	//     list     : list of event commands
	//     event_id : event ID
	public void setup(list, event_id, map_id = null) {
		clear;
		@map_id = map_id || Game.GameData.game_map.map_id;
		@event_id = event_id;
		@list = list;
		@index = 0;
		@branch.clear;
	}

	public void setup_starting_event() {
		if (Game.GameData.game_map.need_refresh) Game.GameData.game_map.refresh;
		// Set up common event if one wants to start
		if (Game.GameData.game_temp.common_event_id > 0) {
			setup(Game.GameData.data_common_events[Game.GameData.game_temp.common_event_id].list, 0);
			Game.GameData.game_temp.common_event_id = 0;
			return;
		}
		// Check all map events for one that wants to start, and set it up
		foreach (var event in Game.GameData.game_map.events) { //Game.GameData.game_map.events.each_value do => |event|
			if (!event.starting) continue;
			if (event.trigger < 3) {   // Isn't autorun or parallel processing
				event.lock;
				event.clear_starting;
			}
			setup(event.list, event.id, event.map.map_id);
			return;
		}
		// Check all common events for one that is autorun, and set it up
		foreach (var common_event in Game.GameData.data_common_events.compact) { //'Game.GameData.data_common_events.compact.each' do => |common_event|
			if (common_event.trigger != 1 || !Game.GameData.game_switches[common_event.switch_id]) continue;
			setup(common_event.list, 0);
			return;
		}
	}

	public bool running() {
		return !@list.null();
	}

	public void update() {
		@loop_count = 0;
		do { //loop; while (true);
			@loop_count += 1;
			if (@loop_count > 100) {   // Call Graphics.update for freeze prevention
				Graphics.update;
				@loop_count = 0;
			}
			// If this interpreter's map isn't the current map or connected to it,
			// forget this interpreter's event ID
			if (Game.GameData.game_map.map_id != @map_id && !Game.GameData.map_factory.areConnected(Game.GameData.game_map.map_id, @map_id)) {
				@event_id = 0;
			}
			// Update child interpreter if one exists
			if (@child_interpreter) {
				@child_interpreter.update;
				if (!@child_interpreter.running()) @child_interpreter = null;
				if (@child_interpreter) return;
			}
			// Do nothing if a message is being shown
			if (@message_waiting) return;
			// Do nothing if any event or the player is in the middle of a move route
			if (@move_route_waiting) {
				if (Game.GameData.game_player.move_route_forcing) return;
				foreach (var event in Game.GameData.game_map.events) { //Game.GameData.game_map.events.each_value do => |event|
					if (event.move_route_forcing) return;
				}
				Game.GameData.game_temp.followers.each_follower do |event, follower|
					if (event.move_route_forcing) return;
				}
				@move_route_waiting = false;
			}
			// Do nothing if the player is jumping out of surfing
			if (Game.GameData.game_temp.ending_surf) return;
			// Do nothing while waiting
			if (@wait_count > 0) {
				if (System.uptime - @wait_start < @wait_count) return;
				@wait_count = 0;
				@wait_start = null;
			}
			// Do nothing if the pause menu is going to open
			if (Game.GameData.game_temp.menu_calling) return;
			// If there are no commands in the list, try to find something that wants to run
			if (@list.null()) {
				if (@main) setup_starting_event;
				if (@list.null()) return;   // Couldn't find anything that wants to run
			}
			// Execute the next command
			if (execute_command == false) return;
			// Move to the next @index
			@index += 1;
		}
	}

	public void execute_script(script) {
		begin;
			result = eval(script);
			return result;
		rescue Exception;
			e = $!;
			if (e.is_a(SystemExit) || e.class.ToString() == "Reset") raise;
			event = get_self;
			// Gather text for error message
			message = GetExceptionMessage(e);
			backtrace_text = "";
			if (e.is_a(SyntaxError)) {
				foreach (var line in script) { //script.each_line do => |line|
					line = System.Text.RegularExpressions.Regex.Replace(line, "\s+$", "");
					if (System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*\(")) {
						message += $"\r\n***Line '{line}' shouldn't begin with '('. Try putting the '('\r\n";
						message += "at the end of the previous line instead, or using 'extendtext.exe'.";
					}
				}
			} else {
				backtrace_text += "\r\n";
				backtrace_text += "Backtrace:";
				e.backtrace[0, 10].each { |i| backtrace_text += $"\r\n{i}" };
				backtrace_text = System.Text.RegularExpressions.Regex.Replace(backtrace_text, "Section(\d+)", Game.GameData.RGSS_SCRIPTS[Game.GameData.1.ToInt()][1]); //rescue null
				backtrace_text += "\r\n";
			}
			// Assemble error message
			err = "Script error in Interpreter\r\n";
			if (Game.GameData.game_map) {
				map_name = (GetBasicMapNameFromId(Game.GameData.game_map.map_id) rescue null) || "???";
				if (event) {
					err = $"Script error in event {event.id} (coords {event.x},{event.y}), map {Game.GameData.game_map.map_id} ({map_name})\r\n";
				} else {
					err = $"Script error in Common Event, map {Game.GameData.game_map.map_id} ({map_name})\r\n";
				}
			}
			err += $"Exception: {e.class}\r\n";
			err += $"Message: {message}\r\n\r\n";
			err += $"***Full script:\r\n{script}";   // \r\n"
			err += backtrace_text;
			// Raise error
			Debug.LogError(new EventScriptError(err));
			//throw new Exception(new EventScriptError(err));
		}
	}

	public void get_character(parameter = 0) {
		switch (parameter) {
			case -1:   // player
				return Game.GameData.game_player;
			case 0:    // this event
				events = Game.GameData.game_map.events;
				return (events) ? events[@event_id] : null;
			default:      // specific event
				events = Game.GameData.game_map.events;
				return (events) ? events[parameter] : null;
		}
	}

	public void get_player() {
		return get_character(-1);
	}

	public void get_self() {
		return get_character(0);
	}

	public void get_event(parameter) {
		return get_character(parameter);
	}

	// Freezes all events on the map (for use at the beginning of common events)
	public void GlobalLock() {
		Game.GameData.game_map.events.each_value(event => event.minilock);
	}

	// Unfreezes all events on the map (for use at the end of common events)
	public void GlobalUnlock() {
		Game.GameData.game_map.events.each_value(event => event.unlock);
	}

	// Gets the next index in the interpreter, ignoring certain commands between messages
	public void NextIndex(index) {
		if (!@list || @list.length == 0) return -1;
		i = index + 1;
		do { //loop; while (true);
			if (i >= @list.length - 1) return i;
			switch (@list[i].code) {
				case 118: case 108: case 408:   // Label, Comment
					i += 1;
					break;
				case 413:             // Repeat Above
					i = RepeatAbove(i);
					break;
				case 113:             // Break Loop
					i = BreakLoop(i);
					break;
				case 119:             // Jump to Label
					newI = JumpToLabel(i, @list[i].parameters[0]);
					i = (newI > i) ? newI : i + 1;
					break;
				default:
					return i;
			}
		}
	}

	public void RepeatAbove(index) {
		index = @list[index].indent;
		do { //loop; while (true);
			index -= 1;
			if (@list[index].indent == indent) return index + 1;
		}
	}

	public void BreakLoop(index) {
		indent = @list[index].indent;
		temp_index = index;
		do { //loop; while (true);
			temp_index += 1;
			if (temp_index >= @list.size - 1) return index + 1;
			if (@list[temp_index].code == 413 &&
															@list[temp_index].indent < indent) return temp_index + 1;
		}
	}

	public void JumpToLabel(index, label_name) {
		temp_index = 0;
		do { //loop; while (true);
			if (temp_index >= @list.size - 1) return index + 1;
			if (@list[temp_index].code == 118 &&
															@list[temp_index].parameters[0] == label_name) return temp_index + 1;
			temp_index += 1;
		}
	}

	public void follower_move_route(id = null) {
		@follower_move_route = true;
		@follower_move_route_id = id;
	}

	public void follower_animation(id = null) {
		@follower_animation = true;
		@follower_animation_id = id;
	}

	public void end_follower_overrides() {
		@follower_move_route = false;
		@follower_move_route_id = null;
		@follower_animation = false;
		@follower_animation_id = null;
	}

	// Helper function that shows a picture in a script.
	public void ShowPicture(number, name, origin, x, y, zoomX = 100, zoomY = 100, opacity = 255, blendType = 0) {
		number += (Game.GameData.game_temp.in_battle ? 50 : 0);
		Game.GameData.game_screen.pictures[number].show(name, origin, x, y, zoomX, zoomY, opacity, blendType);
	}

	// Erases an event and adds it to the list of erased events so that
	// it can stay erased when the game is saved then loaded again.
	public void EraseThisEvent() {
		if (Game.GameData.game_map.events[@event_id]) {
			Game.GameData.game_map.events[@event_id].erase;
			Game.GameData.PokemonMap&.addErasedEvent(@event_id);
		}
		@index += 1;
		return true;
	}

	// Runs a common event.
	public void CommonEvent(id) {
		common_event = Game.GameData.data_common_events[id];
		if (!common_event) return;
		if (Game.GameData.game_temp.in_battle) {
			Game.GameData.game_system.battle_interpreter.setup(common_event.list, 0);
		} else {
			interp = new Interpreter();
			interp.setup(common_event.list, 0);
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				interp.update;
				UpdateSceneMap;
				if (!interp.running()) break;
			}
		}
	}

	// Sets another event's self switch (eg. SetSelfSwitch(20, "A", true) ).
	public void SetSelfSwitch(eventid, switch_name, value, mapid = -1) {
		if (mapid < 0) mapid = @map_id;
		old_value = Game.GameData.game_self_switches[new {mapid, eventid, switch_name}];
		Game.GameData.game_self_switches[new {mapid, eventid, switch_name}] = value;
		if (value != old_value && Game.GameData.map_factory.hasMap(mapid)) {
			Game.GameData.map_factory.getMap(mapid, false).need_refresh = true;
		}
	}

	public bool tsOff(c) {
		return get_self.tsOff(c);
	}
	alias isTempSwitchOff() tsOff();

	public bool tsOn(c) {
		return get_self.tsOn(c);
	}
	alias isTempSwitchOn() tsOn();

	public void setTempSwitchOn(c) {
		get_self.setTempSwitchOn(c);
	}

	public void setTempSwitchOff(c) {
		get_self.setTempSwitchOff(c);
	}

	public void getVariable(*arg) {
		if (arg.length == 0) {
			if (!Game.GameData.PokemonGlobal.eventvars) return null;
			return Game.GameData.PokemonGlobal.eventvars[new {@map_id, @event_id}];
		} else {
			return Game.GameData.game_variables[arg[0]];
		}
	}

	public void setVariable(*arg) {
		if (arg.length == 1) {
			if (!Game.GameData.PokemonGlobal.eventvars) Game.GameData.PokemonGlobal.eventvars = new List<string>();
			Game.GameData.PokemonGlobal.eventvars[new {@map_id, @event_id}] = arg[0];
		} else {
			Game.GameData.game_variables[arg[0]] = arg[1];
			Game.GameData.game_map.need_refresh = true;
		}
	}

	public void GetPokemon(id) {
		return Game.GameData.player.party[Get(id)];
	}

	public void SetEventTime(*arg) {
		if (!Game.GameData.PokemonGlobal.eventvars) Game.GameData.PokemonGlobal.eventvars = new List<string>();
		time = GetTimeNow;
		time = time.ToInt();
		SetSelfSwitch(@event_id, "A", true);
		Game.GameData.PokemonGlobal.eventvars[new {@map_id, @event_id}] = time;
		foreach (var otherevt in arg) { //'arg.each' do => |otherevt|
			SetSelfSwitch(otherevt, "A", true);
			Game.GameData.PokemonGlobal.eventvars[new {@map_id, otherevt}] = time;
		}
	}

	// Used in boulder events. Allows an event to be pushed.
	public void PushThisEvent(strength = false) {
		event = get_self;
		old_x  = event.x;
		old_y  = event.y;
		// Apply strict version of passable, which treats tiles that are passable
		// only from certain directions as fully impassible
		if (!event.can_move_in_direction(Game.GameData.game_player.direction, true)) return;
		Game.GameData.stats.strength_push_count += 1;
		switch (Game.GameData.game_player.direction) {
			case 2:  event.move_down; break;
			case 4:  event.move_left; break;
			case 6:  event.move_right; break;
			case 8:  event.move_up; break;
		}
		Game.GameData.PokemonMap&.addMovedEvent(@event_id);
		if (old_x != event.x || old_y != event.y) {
			if (strength) SEPlay("Strength push");
			Game.GameData.game_player.lock;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				UpdateSceneMap;
				if (!event.moving()) break;
			}
			Game.GameData.game_player.unlock;
		}
	}

	public void PushThisBoulder() {
		if (Game.GameData.PokemonMap.strengthUsed) PushThisEvent(true);
		return true;
	}

	public void SmashThisEvent() {
		event = get_self;
		if (event) SmashEvent(event);
		@index += 1;
		return true;
	}

	public void TrainerIntro(symbol) {
		if (Core.DEBUG && !GameData.TrainerType.exists(symbol)) return true;
		tr_type = GameData.TrainerType.get(symbol).id;
		GlobalLock;
		PlayTrainerIntroBGM(tr_type);
		return true;
	}

	public void TrainerEnd() {
		GlobalUnlock;
		event = get_self;
		event&.erase_route;
	}

	public void setPrice(item, buy_price = -1, sell_price = -1) {
		item = GameData.Item.get(item).id;
		if (!Game.GameData.game_temp.mart_prices[item]) Game.GameData.game_temp.mart_prices[item] = new {-1, -1};
		if (buy_price > 0) Game.GameData.game_temp.mart_prices[item][0] = buy_price;
		if (sell_price >= 0) {   // 0=can't sell
			Game.GameData.game_temp.mart_prices[item][1] = sell_price;
		} else if (buy_price > 0) {
			Game.GameData.game_temp.mart_prices[item][1] = buy_price / Settings.ITEM_SELL_PRICE_DIVISOR;
		}
	}

	public void setSellPrice(item, sell_price) {
		setPrice(item, -1, sell_price);
	}
}
