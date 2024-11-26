//===============================================================================
//
//===============================================================================
public partial class Game_Event : Game_Character {
	public int map_id		{ get { return _map_id; } }			protected int _map_id;
	public int trigger		{ get { return _trigger; } }			protected int _trigger;
	public int list		{ get { return _list; } }			protected int _list;
	public int starting		{ get { return _starting; } }			protected int _starting;
	/// <summary>Temporary self-switches</summary>
	public int tempSwitches		{ get { return _tempSwitches; } }			protected int _tempSwitches;
	public int need_refresh		{ get { return _need_refresh; } set { _need_refresh = value; } }			protected int _need_refresh;

	public override void initialize(map_id, event, map = null) {
		base.initialize(map);
		@map_id       = map_id;
		@event        = event;
		@id           = @event.id;
		@original_x   = @event.x;
		@original_y   = @event.y;
		if (System.Text.RegularExpressions.Regex.IsMatch(@event.name,@"size\((\d+),(\d+)\)",RegexOptions.IgnoreCase)) {
			@width = $~[1].ToInt();
			@height = $~[2].ToInt();
		}
		@erased       = false;
		@starting     = false;
		@need_refresh = false;
		@route_erased = false;
		@through      = true;
		@to_update    = true;
		@tempSwitches = new List<string>();
		if (map) moveto(@event.x, @event.y);
		refresh;
	}

	public int id   { get { return @event.id;   } }
	public int name { get { return @event.name; } }

	public void set_starting() {
		@starting = true;
	}

	public void clear_starting() {
		@starting = false;
	}

	public void start() {
		if (@list.size > 1) @starting = true;
	}

	public void erase() {
		@erased = true;
		refresh;
	}

	public void erase_route() {
		@route_erased = true;
		refresh;
	}

	public bool tsOn(c) {
		return @tempSwitches && @tempSwitches[c] == true;
	}

	public bool tsOff(c) {
		return !@tempSwitches || !@tempSwitches[c];
	}

	public void setTempSwitchOn(c) {
		@tempSwitches[c] = true;
		refresh;
	}

	public void setTempSwitchOff(c) {
		@tempSwitches[c] = false;
		refresh;
	}

	public bool isOff(c) {
		return !Game.GameData.game_self_switches[new {@map_id, @event.id, c}];
	}

	public bool switchIsOn(id) {
		switchname = Game.GameData.data_system.switches[id];
		if (!switchname) return false;
		if (System.Text.RegularExpressions.Regex.IsMatch(switchname,@"^s\:")) {
			return eval($~.post_match);
		} else {
			return Game.GameData.game_switches[id];
		}
	}

	public void variable() {
		if (!Game.GameData.PokemonGlobal.eventvars) return null;
		return Game.GameData.PokemonGlobal.eventvars[new {@map_id, @event.id}];
	}

	public void setVariable(variable) {
		Game.GameData.PokemonGlobal.eventvars[new {@map_id, @event.id}] = variable;
	}

	public void varAsInt() {
		if (!Game.GameData.PokemonGlobal.eventvars) return 0;
		return Game.GameData.PokemonGlobal.eventvars[new {@map_id, @event.id}].ToInt();
	}

	public bool expired(secs = 86_400) {
		ontime = self.variable;
		time = GetTimeNow;
		return ontime && (time.ToInt() > ontime + secs);
	}

	public bool expiredDays(days = 1) {
		ontime = self.variable.ToInt();
		if (!ontime) return false;
		now = GetTimeNow;
		elapsed = (now.ToInt() - ontime) / 86_400;
		if ((now.ToInt() - ontime) % 86_400 > ((now.hour * 3600) + (now.min * 60) + now.sec)) elapsed += 1;
		return elapsed >= days;
	}

	public bool cooledDown(seconds) {
		if (expired(seconds) && tsOff("A")) return true;
		self.need_refresh = true;
		return false;
	}

	public bool cooledDownDays(days) {
		if (expiredDays(days) && tsOff("A")) return true;
		self.need_refresh = true;
		return false;
	}

	public bool onEvent() {
		return @map_id == Game.GameData.game_player.map_id && at_coordinate(Game.GameData.game_player.x, Game.GameData.game_player.y);
	}

	public bool over_trigger() {
		if (@map_id != Game.GameData.game_player.map_id) return false;
		if (@character_name != "" && !@through) return false;
		if (System.Text.RegularExpressions.Regex.IsMatch(@event.name,@"hiddenitem",RegexOptions.IgnoreCase)) return false;
		each_occupied_tile do |i, j|
			if (self.map.passable(i, j, 0, Game.GameData.game_player)) return true;
		}
		return false;
	}

	public void check_event_trigger_touch(dir) {
		if (@map_id != Game.GameData.game_player.map_id) return;
		if (@trigger != 2) return;   // Event touch
		if (Game.GameData.game_system.map_interpreter.running()) return;
		switch (dir) {
			case 2:
				if (Game.GameData.game_player.y != @y + 1) return;
				break;
			case 4:
				if (Game.GameData.game_player.x != @x - 1) return;
				break;
			case 6:
				if (Game.GameData.game_player.x != @x + @width) return;
				break;
			case 8:
				if (Game.GameData.game_player.y != @y - @height) return;
				break;
		}
		if (!in_line_with_coordinate(Game.GameData.game_player.x, Game.GameData.game_player.y)) return;
		if (jumping() || over_trigger()) return;
		start;
	}

	public void check_event_trigger_after_turning() {
		if (@map_id != Game.GameData.game_player.map_id) return;
		if (@trigger != 2) return;   // Not Event Touch
		if (Game.GameData.game_system.map_interpreter.running() || @starting) return;
		if (!System.Text.RegularExpressions.Regex.IsMatch(self.name,@"(?:sight|trainer)\((\d+)\)",RegexOptions.IgnoreCase)) return;
		distance = $~[1].ToInt();
		if (!EventCanReachPlayer(self, Game.GameData.game_player, distance)) return;
		if (jumping() || over_trigger()) return;
		start;
	}

	public void check_event_trigger_after_moving() {
		if (@map_id != Game.GameData.game_player.map_id) return;
		if (@trigger != 2) return;   // Not Event Touch
		if (Game.GameData.game_system.map_interpreter.running() || @starting) return;
		if (System.Text.RegularExpressions.Regex.IsMatch(self.name,@"(?:sight|trainer)\((\d+)\)",RegexOptions.IgnoreCase)) {
			distance = $~[1].ToInt();
			if (!EventCanReachPlayer(self, Game.GameData.game_player, distance)) return;
		} else if (System.Text.RegularExpressions.Regex.IsMatch(self.name,@"counter\((\d+)\)",RegexOptions.IgnoreCase)) {
			distance = $~[1].ToInt();
			if (!EventFacesPlayer(self, Game.GameData.game_player, distance)) return;
		} else {
			return;
		}
		if (jumping() || over_trigger()) return;
		start;
	}

	public void check_event_trigger_auto() {
		switch (@trigger) {
			case 2:   // Event touch
				if (at_coordinate(Game.GameData.game_player.x, Game.GameData.game_player.y) && !jumping() && over_trigger()) {
					start;
				}
				break;
			case 3:   // Autorun
				start;
				break;
		}
	}

	public void refresh() {
		new_page = null;
		unless (@erased) {
			@event.pages.reverse.each do |page|
				c = page.condition;
				if (c.switch1_valid && !switchIsOn(c.switch1_id)) continue;
				if (c.switch2_valid && !switchIsOn(c.switch2_id)) continue;
				if (c.variable_valid && Game.GameData.game_variables[c.variable_id] < c.variable_value) continue;
				if (c.self_switch_valid) {
					key = new {@map_id, @event.id, c.self_switch_ch};
					if (Game.GameData.game_self_switches[key] != true) continue;
				}
				new_page = page;
				break;
			}
		}
		if (new_page == @page) return;
		@page = new_page;
		clear_starting;
		if (@page.null()) {
			@tile_id        = 0;
			@character_name = "";
			@character_hue  = 0;
			@move_type      = 0;
			@through        = true;
			@trigger        = null;
			@list           = null;
			@interpreter    = null;
			return;
		}
		@tile_id              = @page.graphic.tile_id;
		@character_name       = @page.graphic.character_name;
		@character_hue        = @page.graphic.character_hue;
		if (@original_direction != @page.graphic.direction) {
			@direction          = @page.graphic.direction;
			@original_direction = @direction;
			@prelock_direction  = 0;
		}
		if (@original_pattern != @page.graphic.pattern) {
			@pattern            = @page.graphic.pattern;
			@original_pattern   = @pattern;
		}
		@opacity              = @page.graphic.opacity;
		@blend_type           = @page.graphic.blend_type;
		@move_type            = @page.move_type;
		self.move_speed       = @page.move_speed;
		self.move_frequency   = @page.move_frequency;
		@move_route           = (@route_erased) ? new RPG.MoveRoute() : @page.move_route;
		@move_route_index     = 0;
		@move_route_forcing   = false;
		@walk_anime           = @page.walk_anime;
		@step_anime           = @page.step_anime;
		@direction_fix        = @page.direction_fix;
		@through              = @page.through;
		@always_on_top        = @page.always_on_top;
		calculate_bush_depth;
		@trigger              = @page.trigger;
		@list                 = @page.list;
		@interpreter          = null;
		if (@trigger == 4) @interpreter          = new Interpreter();   // Parallel Process
		check_event_trigger_auto;
	}

	public bool should_update(recalc = false) {
		if (!recalc) return @to_update;
		if (@updated_last_frame) return true;
		if (@trigger && (@trigger == 3 || @trigger == 4)) return true;
		if (@move_route_forcing || @moveto_happened) return true;
		if (System.Text.RegularExpressions.Regex.IsMatch(@event.name,@"update",RegexOptions.IgnoreCase)) return true;
		range = 2;   // Number of tiles
		if (self.screen_x - (@sprite_size[0] / 2) > Graphics.width + (range * Game_Map.TILE_WIDTH)) return false;
		if (self.screen_x + (@sprite_size[0] / 2) < -range * Game_Map.TILE_WIDTH) return false;
		if (self.screen_y_ground - @sprite_size[1] > Graphics.height + (range * Game_Map.TILE_HEIGHT)) return false;
		if (self.screen_y_ground < -range * Game_Map.TILE_HEIGHT) return false;
		return true;
	}

	public override void update() {
		@to_update = should_update(true);
		@updated_last_frame = false;
		if (!@to_update) return;
		@updated_last_frame = true;
		@moveto_happened = false;
		last_moving = moving();
		base.update();
		if (!moving() && last_moving) check_event_trigger_after_moving;
		if (@need_refresh) {
			@need_refresh = false;
			refresh;
		}
		check_event_trigger_auto;
		if (@interpreter) {
			if (!@interpreter.running()) @interpreter.setup(@list, @event.id, @map_id);
			@interpreter.update;
		}
	}

	public override void update_move() {
		was_jumping = jumping();
		base.update_move();
		if (was_jumping && !jumping() && !@transparent && (@tile_id > 0 || @character_name != "")) {
			spriteset = Game.GameData.scene.spriteset(map_id);
			spriteset&.addUserAnimation(Settings.DUST_ANIMATION_ID, self.x, self.y, true, 1);
		}
	}
}
