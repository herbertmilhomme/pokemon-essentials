//===============================================================================
//
//===============================================================================
public partial class Game_Character {
	public int id		{ get { return _id; } }			protected int _id;
	public int original_x		{ get { return _original_x; } }			protected int _original_x;
	public int original_y		{ get { return _original_y; } }			protected int _original_y;
	public int x		{ get { return _x; } }			protected int _x;
	public int y		{ get { return _y; } }			protected int _y;
	public int real_x		{ get { return _real_x; } }			protected int _real_x;
	public int real_y		{ get { return _real_y; } }			protected int _real_y;
	/// <summary>In pixels, positive shifts sprite to the right</summary>
	public int x_offset		{ get { return _x_offset; } }			protected int _x_offset;
	/// <summary>In pixels, positive shifts sprite down</summary>
	public int y_offset		{ get { return _y_offset; } }			protected int _y_offset;
	public int width		{ get { return _width; } set { _width = value; } }			protected int _width;
	public int height		{ get { return _height; } set { _height = value; } }			protected int _height;
	public int sprite_size		{ get { return _sprite_size; } set { _sprite_size = value; } }			protected int _sprite_size;
	public int tile_id		{ get { return _tile_id; } }			protected int _tile_id;
	public int character_name		{ get { return _character_name; } set { _character_name = value; } }			protected int _character_name;
	public int character_hue		{ get { return _character_hue; } set { _character_hue = value; } }			protected int _character_hue;
	public int opacity		{ get { return _opacity; } set { _opacity = value; } }			protected int _opacity;
	public int blend_type		{ get { return _blend_type; } }			protected int _blend_type;
	public int direction		{ get { return _direction; } set { _direction = value; } }			protected int _direction;
	public int pattern		{ get { return _pattern; } set { _pattern = value; } }			protected int _pattern;
	public int pattern_surf		{ get { return _pattern_surf; } set { _pattern_surf = value; } }			protected int _pattern_surf;
	public int lock_pattern		{ get { return _lock_pattern; } set { _lock_pattern = value; } }			protected int _lock_pattern;
	public int move_route_forcing		{ get { return _move_route_forcing; } }			protected int _move_route_forcing;
	public int through		{ get { return _through; } set { _through = value; } }			protected int _through;
	public int animation_id		{ get { return _animation_id; } }			protected int _animation_id;
	public int animation_height		{ get { return _animation_height; } set { _animation_height = value; } }			protected int _animation_height;
	public int animation_regular_tone		{ get { return _animation_regular_tone; } set { _animation_regular_tone = value; } }			protected int _animation_regular_tone;
	public int transparent		{ get { return _transparent; } set { _transparent = value; } }			protected int _transparent;
	public int move_speed		{ get { return _move_speed; } }			protected int _move_speed;
	public int jump_speed		{ get { return _jump_speed; } }			protected int _jump_speed;
	public int walk_anime		{ get { return _walk_anime; } set { _walk_anime = value; } }			protected int _walk_anime;
	public int bob_height		{ get { return _bob_height; } }			protected int _bob_height;

	public void initialize(map = null) {
		@map                       = map;
		@id                        = 0;
		@original_x                = 0;
		@original_y                = 0;
		@x                         = 0;
		@y                         = 0;
		@real_x                    = 0;
		@real_y                    = 0;
		@x_offset                  = 0;
		@y_offset                  = 0;
		@width                     = 1;
		@height                    = 1;
		@sprite_size               = new {Game_Map.TILE_WIDTH, Game_Map.TILE_HEIGHT};
		@tile_id                   = 0;
		@character_name            = "";
		@character_hue             = 0;
		@opacity                   = 255;
		@blend_type                = 0;
		@direction                 = 2;
		@pattern                   = 0;
		@pattern_surf              = 0;
		@lock_pattern              = false;
		@move_route_forcing        = false;
		@through                   = false;
		animation_id               = 0;
		@transparent               = false;
		@original_direction        = 2;
		@original_pattern          = 0;
		@move_type                 = 0;
		self.move_speed            = 3;
		self.move_frequency        = 6;
		self.jump_speed            = 3;
		@move_route                = null;
		@move_route_index          = 0;
		@original_move_route       = null;
		@original_move_route_index = 0;
		@walk_anime                = true;    // Whether character should animate while moving
		@step_anime                = false;   // Whether character should animate while still
		@direction_fix             = false;
		@always_on_top             = false;
		@anime_count               = 0;   // Time since pattern was last changed
		@stop_count                = 0;   // Time since character last finished moving
		@bumping                   = false;   // Used by the player only when walking into something
		@jump_peak                 = 0;   // Max height while jumping
		@jump_distance             = 0;   // Total distance of jump
		@jump_fraction             = 0;   // How far through a jump we currently are (0-1)
		@jumping_on_spot           = false;
		@bob_height                = 0;
		@wait_count                = 0;
		@wait_start                = null;
		@moved_this_frame          = false;
		@moveto_happened           = false;
		@locked                    = false;
		@prelock_direction         = 0;
	}

	public int animation_id { set {
		@animation_id = value;
		if (value == 0) {
			@animation_height = 3;
			@animation_regular_tone = false;
			}
	}
	}

	public int x_offset { get { return @x_offset || 0; } }
	public int y_offset { get { return @y_offset || 0; } }

	public bool at_coordinate(check_x, check_y) {
		return check_x >= @x && check_x < @x + @width &&
					check_y > @y - @height && check_y <= @y;
	}

	public bool in_line_with_coordinate(check_x, check_y) {
		return (check_x >= @x && check_x < @x + @width) ||
					(check_y > @y - @height && check_y <= @y);
	}

	public void each_occupied_tile() {
		(@x...(@x + @width)).each do |i|
			((@y - @height + 1)..@y).each do |j|
				yield i, j;
			}
		}
	}

	public int move_speed { set {
		@move_speed = val;
		// Time taken to traverse one tile (in seconds) for each speed:
		//   1 => 1.0
		//   2 => 0.5
		//   3 => 0.25    // Walking speed
		//   4 => 0.125   // Running speed (2x walking speed)
		//   5 => 0.1     // Cycling speed (1.25x running speed)
		//   6 => 0.05
		switch (val) {
			case 6:  @move_time = 0.05; break;
			case 5:  @move_time = 0.1; break;
			default:        @move_time = 2.0 / (2**val); break;
		}
		}
	}

	// Takes the same values as move_speed above.
	public int jump_speed { set {
		@jump_speed = val;
		switch (val) {
			case 6:  @jump_time = 0.05; break;
			case 5:  @jump_time = 0.1; break;
			default:        @jump_time = 2.0 / (2**val); break;
		}
		}
	}

	// Returns time in seconds for one full cycle (4 frames) of an animating
	// charset to show. Two frames are shown per movement across one tile.
	public void pattern_update_speed() {
		if (jumping()) return @jump_time * 2;
		ret = @move_time * 2;
		if (@move_speed >= 5) ret *= 2;   // Cycling speed or faster; slower animation
		return ret;
	}

	public int move_frequency { set {
		if (val == @move_frequency) return;
		@move_frequency = val;
		// Time in seconds to wait between each action in a move route (not forced).
		// Specifically, this is the time to wait after the character stops moving
		// because of the previous action.
		//   1 => 4.75 seconds
		//   2 => 3.6 seconds
		//   3 => 2.55 seconds
		//   4 => 1.6 seconds
		//   5 => 0.75 seconds
		//   6 => 0 seconds, i.e. continuous movement
		@command_delay = (40 - (val * 2)) * (6 - val) / 40.0;
		}
	}

	public void bob_height() {
		if (!@bob_height) @bob_height = 0;
		return @bob_height;
	}

	public void lock() {
		if (@locked) return;
		@prelock_direction = 0;   // Was @direction but disabled
		turn_toward_player;
		@locked = true;
	}

	public void minilock() {
		@prelock_direction = 0;   // Was @direction but disabled
		@locked = true;
	}

	public bool lock() {
		return @locked;
	}

	public void unlock() {
		unless (@locked) return;
		@locked = false;
		if (!@direction_fix && @prelock_direction != 0) @direction = @prelock_direction;
	}

	//-----------------------------------------------------------------------------
	// Information from map data
	//-----------------------------------------------------------------------------

	public void map() {
		return (@map) ? @map : Game.GameData.game_map;
	}

	public void terrain_tag() {
		return self.map.terrain_tag(@x, @y);
	}

	public void bush_depth() {
		if (respond_to("name") && System.Text.RegularExpressions.Regex.IsMatch(name,@"airborne",RegexOptions.IgnoreCase)) return 0;
		return @bush_depth || 0;
	}

	public void calculate_bush_depth() {
		if (@tile_id > 0 || @always_on_top || jumping() || (respond_to("name") && System.Text.RegularExpressions.Regex.IsMatch(name,@"airborne",RegexOptions.IgnoreCase))) {
			@bush_depth = 0;
			return;
		}
		this_map = (self.map.valid(@x, @y)) ? new {self.map, @x, @y} : Game.GameData.map_factory&.getNewMap(@x, @y, self.map.map_id);
		if (this_map && this_map[0].deepBush(this_map[1], this_map[2])) {
			xbehind = @x + (@direction == 4 ? 1 : @direction == 6 ? -1 : 0);
			ybehind = @y + (@direction == 8 ? 1 : @direction == 2 ? -1 : 0);
			if (moving()) {
				behind_map = (self.map.valid(xbehind, ybehind)) ? new {self.map, xbehind, ybehind} : Game.GameData.map_factory&.getNewMap(xbehind, ybehind, self.map.map_id);
				if (behind_map[0].deepBush(behind_map[1], behind_map[2])) @bush_depth = Game_Map.TILE_HEIGHT;
			} else {
				@bush_depth = Game_Map.TILE_HEIGHT;
			}
		} else if (this_map && this_map[0].bush(this_map[1], this_map[2]) && !moving()) {
			@bush_depth = 12;
		} else {
			@bush_depth = 0;
		}
	}

	public void fullPattern() {
		switch (self.direction) {
			case 2:  return self.pattern;
			case 4:  return self.pattern + 4;
			case 6:  return self.pattern + 8;
			case 8:  return self.pattern + 12;
		}
		return 0;
	}

	//-----------------------------------------------------------------------------
	// Passability
	//-----------------------------------------------------------------------------

	public bool passable(x, y, dir, strict = false) {
		new_x = x + (dir == 6 ? 1 : dir == 4 ? -1 : 0);
		new_y = y + (dir == 2 ? 1 : dir == 8 ? -1 : 0);
		unless (self.map.valid(new_x, new_y)) return false;
		if (@through) return true;
		if (strict) {
			unless (self.map.passableStrict(x, y, dir, self)) return false;
			unless (self.map.passableStrict(new_x, new_y, 10 - dir, self)) return false;
		} else {
			unless (self.map.passable(x, y, dir, self)) return false;
			unless (self.map.passable(new_x, new_y, 10 - dir, self)) return false;
		}
		foreach (var event in self.map.events) { //self.map.events.each_value do => |event|
			if (self == event || !event.at_coordinate(new_x, new_y) || event.through) continue;
			if (self != Game.GameData.game_player || event.character_name != "") return false;
		}
		if (Game.GameData.game_player.x == new_x && Game.GameData.game_player.y == new_y &&
			!Game.GameData.game_player.through && @character_name != "") {
			return false;
		}
		return true;
	}

	public bool can_move_from_coordinate(start_x, start_y, dir, strict = false) {
		switch (dir) {
			case 2: case 8:   // Down, up
				y_diff = (dir == 8) ? @height - 1 : 0;
				(start_x...(start_x + @width)).each do |i|
					if (!passable(i, start_y - y_diff, dir, strict)) return false;
				}
				return true;
			case 4: case 6:   // Left, right
				x_diff = (dir == 6) ? @width - 1 : 0;
				((start_y - @height + 1)..start_y).each do |i|
					if (!passable(start_x + x_diff, i, dir, strict)) return false;
				}
				return true;
			case 1: case 3:   // Down diagonals
				// Treated as moving down first and then horizontally, because that
				// describes which tiles the character's feet touch
				(start_x...(start_x + @width)).each do |i|
					if (!passable(i, start_y, 2, strict)) return false;
				}
				x_diff = (dir == 3) ? @width - 1 : 0;
				((start_y - @height + 1)..start_y).each do |i|
					if (!passable(start_x + x_diff, i + 1, dir + 3, strict)) return false;
				}
				return true;
			case 7: case 9:   // Up diagonals
				// Treated as moving horizontally first and then up, because that describes
				// which tiles the character's feet touch
				x_diff = (dir == 9) ? @width - 1 : 0;
				((start_y - @height + 1)..start_y).each do |i|
					if (!passable(start_x + x_diff, i, dir - 3, strict)) return false;
				}
				x_tile_offset = (dir == 9) ? 1 : -1;
				(start_x...(start_x + @width)).each do |i|
					if (!passable(i + x_tile_offset, start_y - @height + 1, 8, strict)) return false;
				}
				return true;
		}
		return false;
	}

	public bool can_move_in_direction(dir, strict = false) {
		return can_move_from_coordinate(@x, @y, dir, strict);
	}

	//-----------------------------------------------------------------------------
	// Screen position of the character
	//-----------------------------------------------------------------------------

	public void screen_x() {
		ret = (int)Math.Round((@real_x.to_f - self.map.display_x) / Game_Map.X_SUBPIXELS);
		ret += @width * Game_Map.TILE_WIDTH / 2;
		ret += self.x_offset;
		return ret;
	}

	public void screen_y_ground() {
		ret = (int)Math.Round((@real_y.to_f - self.map.display_y) / Game_Map.Y_SUBPIXELS);
		ret += Game_Map.TILE_HEIGHT;
		return ret;
	}

	public void screen_y() {
		ret = screen_y_ground;
		if (jumping()) {
			jump_progress = (@jump_fraction - 0.5).abs;   // 0.5 to 0 to 0.5
			ret += @jump_peak * ((4 * (jump_progress**2)) - 1);
		}
		ret += self.y_offset;
		return ret;
	}

	public void screen_z(height = 0) {
		if (@always_on_top) return 999;
		z = screen_y_ground;
		if (@tile_id > 0) {
			begin;
				return z + (self.map.priorities[@tile_id] * 32);
			rescue;
				Debug.LogError($"Event's graphic is an out-of-range tile (event {@id}, map {self.map.map_id})");
				//throw new ArgumentException($"Event's graphic is an out-of-range tile (event {@id}, map {self.map.map_id})");
			}
		}
		// Add z if height exceeds 32
		return z + ((height > Game_Map.TILE_HEIGHT) ? Game_Map.TILE_HEIGHT - 1 : 0);
	}

	//-----------------------------------------------------------------------------
	// Movement
	//-----------------------------------------------------------------------------

	public bool moving() {
		return !@move_timer.null();
	}

	public bool jumping() {
		return !@jump_timer.null();
	}

	public void straighten() {
		if (@walk_anime || @step_anime) @pattern = 0;
		@anime_count = 0;
		@prelock_direction = 0;
	}

	public void force_move_route(move_route) {
		if (@original_move_route.null()) {
			@original_move_route       = @move_route;
			@original_move_route_index = @move_route_index;
		}
		@move_route         = move_route;
		@move_route_index   = 0;
		@move_route_forcing = true;
		@prelock_direction  = 0;
		@wait_count         = 0;
		@wait_start         = null;
		move_type_custom;
	}

	public void moveto(x, y) {
		@x = x % self.map.width;
		@y = y % self.map.height;
		@real_x = @x * Game_Map.REAL_RES_X;
		@real_y = @y * Game_Map.REAL_RES_Y;
		@prelock_direction = 0;
		@moveto_happened = true;
		calculate_bush_depth;
		triggerLeaveTile;
	}

	public void triggerLeaveTile() {
		if (@oldX && @oldY && @oldMap &&
			(@oldX != self.x || @oldY != self.y || @oldMap != self.map.map_id)) {
			EventHandlers.trigger(:on_leave_tile, self, @oldMap, @oldX, @oldY);
		}
		@oldX = self.x;
		@oldY = self.y;
		@oldMap = self.map.map_id;
	}

	public void increase_steps() {
		@stop_count = 0;
		triggerLeaveTile;
	}

	//-----------------------------------------------------------------------------
	// Movement commands
	//-----------------------------------------------------------------------------

	public void move_type_random() {
		switch (rand(6)) {
			case 0..3:  move_random; break;
			case 4:     move_forward; break;
			case 5:     @stop_count = 0; break;
		}
	}

	public void move_type_toward_player() {
		sx = @x + (@width / 2.0) - (Game.GameData.game_player.x + (Game.GameData.game_player.width / 2.0));
		sy = @y - (@height / 2.0) - (Game.GameData.game_player.y - (Game.GameData.game_player.height / 2.0));
		if (sx.abs + sy.abs >= 20) {
			move_random;
			return;
		}
		switch (rand(6)) {
			case 0..3:  move_toward_player; break;
			case 4:     move_random; break;
			case 5:     move_forward; break;
		}
	}

	public void move_type_custom() {
		if (jumping() || moving()) return;
		if (@move_route.list.size <= 1) return;   // Empty move route
		start_index = @move_route_index;
		(@move_route.list.size - 1).times do
			command = @move_route.list[@move_route_index];
			if (command.code == 0) {
				if (@move_route.repeat) {
					@move_route_index = 0;
					command = @move_route.list[@move_route_index];
				} else {
					if (@move_route_forcing) {
						@move_route_forcing = false;
						@move_route       = @original_move_route;
						@move_route_index = @original_move_route_index;
						@original_move_route = null;
					}
					@stop_count = 0;
					return;
				}
			}
			done_one_command = true;
			// The below move route commands wait for a frame (i.e. return) after
			// executing them
			if (command.code <= 14) {
				switch (command.code) {
					case 1:   move_down; break;
					case 2:   move_left; break;
					case 3:   move_right; break;
					case 4:   move_up; break;
					case 5:   move_lower_left; break;
					case 6:   move_lower_right; break;
					case 7:   move_upper_left; break;
					case 8:   move_upper_right; break;
					case 9:   move_random; break;
					case 10:  move_toward_player; break;
					case 11:  move_away_from_player; break;
					case 12:  move_forward; break;
					case 13:  move_backward; break;
					case 14:  jump(command.parameters[0], command.parameters[1]); break;
				}
				if (@move_route.skippable || moving() || jumping()) @move_route_index += 1;
				return;
			}
			// The below move route commands wait for a frame (i.e. return) after
			// executing them
			if (command.code >= 15 && command.code <= 26) {
				switch (command.code) {
					case 15:   // Wait
						@wait_count = command.parameters[0] / 20.0;
						@wait_start = System.uptime;
						break;
					case 16:  turn_down; break;
					case 17:  turn_left; break;
					case 18:  turn_right; break;
					case 19:  turn_up; break;
					case 20:  turn_right_90; break;
					case 21:  turn_left_90; break;
					case 22:  turn_180; break;
					case 23:  turn_right_or_left_90; break;
					case 24:  turn_random; break;
					case 25:  turn_toward_player; break;
					case 26:  turn_away_from_player; break;
				}
				@move_route_index += 1;
				return;
			}
			// The below move route commands don't wait for a frame (i.e. return) after
			// executing them
			if (command.code >= 27) {
				switch (command.code) {
					case 27:
						Game.GameData.game_switches[command.parameters[0]] = true;
						self.map.need_refresh = true;
						break;
					case 28:
						Game.GameData.game_switches[command.parameters[0]] = false;
						self.map.need_refresh = true;
						break;
					case 29:  self.move_speed = command.parameters[0]; break;
					case 30:  self.move_frequency = command.parameters[0]; break;
					case 31:  @walk_anime = true; break;
					case 32:  @walk_anime = false; break;
					case 33:  @step_anime = true; break;
					case 34:  @step_anime = false; break;
					case 35:  @direction_fix = true; break;
					case 36:  @direction_fix = false; break;
					case 37:  @through = true; break;
					case 38:  @through = false; break;
					case 39:
						old_always_on_top = @always_on_top;
						@always_on_top = true;
						if (@always_on_top != old_always_on_top) calculate_bush_depth;
						break;
					case 40:
						old_always_on_top = @always_on_top;
						@always_on_top = false;
						if (@always_on_top != old_always_on_top) calculate_bush_depth;
						break;
					case 41:
						old_tile_id = @tile_id;
						@tile_id = 0;
						@character_name = command.parameters[0];
						@character_hue = command.parameters[1];
						if (@original_direction != command.parameters[2]) {
							@direction = command.parameters[2];
							@original_direction = @direction;
							@prelock_direction = 0;
						}
						if (@original_pattern != command.parameters[3]) {
							@pattern = command.parameters[3];
							@original_pattern = @pattern;
						}
						if (@tile_id != old_tile_id) calculate_bush_depth;
						break;
					case 42:  @opacity = command.parameters[0]; break;
					case 43:  @blend_type = command.parameters[0]; break;
					case 44:  SEPlay(command.parameters[0]); break;
					case 45:
						eval(command.parameters[0]);
						if (System.Text.RegularExpressions.Regex.IsMatch(command.parameters[0],@"^move_random_range") ||
							System.Text.RegularExpressions.Regex.IsMatch(command.parameters[0],@"^move_random_UD") ||
							System.Text.RegularExpressions.Regex.IsMatch(command.parameters[0],@"^move_random_LR")) {
							@move_route_index += 1;
							return;
						}
						break;
				}
				@move_route_index += 1;
			}
		}
	}

	public void move_generic(dir, turn_enabled = true) {
		if (turn_enabled) turn_generic(dir);
		if (can_move_in_direction(dir)) {
			turn_generic(dir);
			@move_initial_x = @x;
			@move_initial_y = @y;
			@x += (dir == 4) ? -1 : (dir == 6) ? 1 : 0;
			@y += (dir == 8) ? -1 : (dir == 2) ? 1 : 0;
			@move_timer = 0.0;
			increase_steps;
		} else {
			check_event_trigger_touch(dir);
		}
	}

	public void move_down(turn_enabled = true) {
		move_generic(2, turn_enabled);
	}

	public void move_left(turn_enabled = true) {
		move_generic(4, turn_enabled);
	}

	public void move_right(turn_enabled = true) {
		move_generic(6, turn_enabled);
	}

	public void move_up(turn_enabled = true) {
		move_generic(8, turn_enabled);
	}

	public void move_upper_left() {
		unless (@direction_fix) {
			@direction = (@direction == 6 ? 4 : @direction == 2 ? 8 : @direction);
		}
		if (can_move_in_direction(7)) {
			@move_initial_x = @x;
			@move_initial_y = @y;
			@x -= 1;
			@y -= 1;
			@move_timer = 0.0;
			increase_steps;
		}
	}

	public void move_upper_right() {
		unless (@direction_fix) {
			@direction = (@direction == 4 ? 6 : @direction == 2 ? 8 : @direction);
		}
		if (can_move_in_direction(9)) {
			@move_initial_x = @x;
			@move_initial_y = @y;
			@x += 1;
			@y -= 1;
			@move_timer = 0.0;
			increase_steps;
		}
	}

	public void move_lower_left() {
		unless (@direction_fix) {
			@direction = (@direction == 6 ? 4 : @direction == 8 ? 2 : @direction);
		}
		if (can_move_in_direction(1)) {
			@move_initial_x = @x;
			@move_initial_y = @y;
			@x -= 1;
			@y += 1;
			@move_timer = 0.0;
			increase_steps;
		}
	}

	public void move_lower_right() {
		unless (@direction_fix) {
			@direction = (@direction == 4 ? 6 : @direction == 8 ? 2 : @direction);
		}
		if (can_move_in_direction(3)) {
			@move_initial_x = @x;
			@move_initial_y = @y;
			@x += 1;
			@y += 1;
			@move_timer = 0.0;
			increase_steps;
		}
	}

	// Anticlockwise.
	public void moveLeft90() {
		switch (self.direction) {
			case 2:  move_right; break;   // down
			case 4:  move_down; break;    // left
			case 6:  move_up; break;      // right
			case 8:  move_left; break;    // up
		}
	}

	// Clockwise.
	public void moveRight90() {
		switch (self.direction) {
			case 2:  move_left; break;    // down
			case 4:  move_up; break;      // left
			case 6:  move_down; break;    // right
			case 8:  move_right; break;   // up
		}
	}

	public void move_random() {
		switch (rand(4)) {
			case 0:  move_down(false); break;
			case 1:  move_left(false); break;
			case 2:  move_right(false); break;
			case 3:  move_up(false); break;
		}
	}

	public void move_random_range(xrange = -1, yrange = -1) {
		dirs = new List<string>();   // 0=down, 1=left, 2=right, 3=up
		if (xrange < 0) {
			dirs.Add(1);
			dirs.Add(2);
		} else if (xrange > 0) {
			if (@x > @original_x - xrange) dirs.Add(1);
			if (@x < @original_x + xrange) dirs.Add(2);
		}
		if (yrange < 0) {
			dirs.Add(0);
			dirs.Add(3);
		} else if (yrange > 0) {
			if (@y < @original_y + yrange) dirs.Add(0);
			if (@y > @original_y - yrange) dirs.Add(3);
		}
		if (dirs.length == 0) return;
		switch (dirs[rand(dirs.length)]) {
			case 0:  move_down(false); break;
			case 1:  move_left(false); break;
			case 2:  move_right(false); break;
			case 3:  move_up(false); break;
		}
	}

	public void move_random_UD(range = -1) {
		move_random_range(0, range);
	}

	public void move_random_LR(range = -1) {
		move_random_range(range, 0);
	}

	public void move_toward_player() {
		sx = @x + (@width / 2.0) - (Game.GameData.game_player.x + (Game.GameData.game_player.width / 2.0));
		sy = @y - (@height / 2.0) - (Game.GameData.game_player.y - (Game.GameData.game_player.height / 2.0));
		if (sx == 0 && sy == 0) return;
		abs_sx = sx.abs;
		abs_sy = sy.abs;
		if (abs_sx == abs_sy) {
			(rand(2) == 0) ? abs_sx += 1 : abs_sy += 1
		}
		if (abs_sx > abs_sy) {
			if (abs_sx >= 1) {
				(sx > 0) ? move_left : move_right
			}
			if (!moving() && sy != 0) {
				if (abs_sy >= 1) {
					(sy > 0) ? move_up : move_down
				}
			}
		} else {
			if (abs_sy >= 1) {
				(sy > 0) ? move_up : move_down
			}
			if (!moving() && sx != 0) {
				if (abs_sx >= 1) {
					(sx > 0) ? move_left : move_right
				}
			}
		}
	}

	public void move_away_from_player() {
		sx = @x + (@width / 2.0) - (Game.GameData.game_player.x + (Game.GameData.game_player.width / 2.0));
		sy = @y - (@height / 2.0) - (Game.GameData.game_player.y - (Game.GameData.game_player.height / 2.0));
		if (sx == 0 && sy == 0) return;
		abs_sx = sx.abs;
		abs_sy = sy.abs;
		if (abs_sx == abs_sy) {
			(rand(2) == 0) ? abs_sx += 1 : abs_sy += 1
		}
		if (abs_sx > abs_sy) {
			(sx > 0) ? move_right : move_left
			if (!moving() && sy != 0) {
				(sy > 0) ? move_down : move_up
			}
		} else {
			(sy > 0) ? move_down : move_up
			if (!moving() && sx != 0) {
				(sx > 0) ? move_right : move_left
			}
		}
	}

	public void move_forward() {
		switch (@direction) {
			case 2:  move_down(false); break;
			case 4:  move_left(false); break;
			case 6:  move_right(false); break;
			case 8:  move_up(false); break;
		}
	}

	public void move_backward() {
		last_direction_fix = @direction_fix;
		@direction_fix = true;
		switch (@direction) {
			case 2:  move_up(false); break;
			case 4:  move_right(false); break;
			case 6:  move_left(false); break;
			case 8:  move_down(false); break;
		}
		@direction_fix = last_direction_fix;
	}

	public void jump(x_plus, y_plus) {
		if (x_plus != 0 || y_plus != 0) {
			if (x_plus.abs > y_plus.abs) {
				(x_plus < 0) ? turn_left : turn_right
			} else {
				(y_plus < 0) ? turn_up : turn_down
			}
			if (!passable(i + x_plus, j + y_plus, 0) }) each_occupied_tile { |i, j| return;
		}
		@jump_initial_x = @x;
		@jump_initial_y = @y;
		@x += x_plus;
		@y += y_plus;
		@jump_timer = 0.0;
		real_distance = Math.sqrt((x_plus**2) + (y_plus**2));
		distance = (int)Math.Max(1, real_distance);
		@jump_peak = distance * Game_Map.TILE_HEIGHT * 3 / 8;   // 3/4 of tile for ledge jumping
		@jump_distance = (int)Math.Max(x_plus.abs * Game_Map.REAL_RES_X, y_plus.abs * Game_Map.REAL_RES_Y);
		@jumping_on_spot = (real_distance == 0);
		increase_steps;
	}

	public void jumpForward(distance = 1) {
		if (distance == 0) return false;
		old_x = @x;
		old_y = @y;
		switch (self.direction) {
			case 2:  jump(0, distance); break;    // down
			case 4:  jump(-distance, 0); break;   // left
			case 6:  jump(distance, 0); break;    // right
			case 8:  jump(0, -distance); break;   // up
		}
		return @x != old_x || @y != old_y;
	}

	public void jumpBackward(distance = 1) {
		if (distance == 0) return false;
		old_x = @x;
		old_y = @y;
		switch (self.direction) {
			case 2:  jump(0, -distance); break;   // down
			case 4:  jump(distance, 0); break;    // left
			case 6:  jump(-distance, 0); break;   // right
			case 8:  jump(0, distance); break;    // up
		}
		return @x != old_x || @y != old_y;
	}

	public void turn_generic(dir) {
		if (@direction_fix) return;
		oldDirection = @direction;
		@direction = dir;
		@stop_count = 0;
		if (dir != oldDirection) check_event_trigger_after_turning;
	}

	public int turn_down  { get { return turn_generic(2); } }
	public int turn_left  { get { return turn_generic(4); } }
	public int turn_right { get { return turn_generic(6); } }
	public int turn_up    { get { return turn_generic(8); } }

	public void turn_right_90() {
		switch (@direction) {
			case 2:  turn_left; break;
			case 4:  turn_up; break;
			case 6:  turn_down; break;
			case 8:  turn_right; break;
		}
	}

	public void turn_left_90() {
		switch (@direction) {
			case 2:  turn_right; break;
			case 4:  turn_down; break;
			case 6:  turn_up; break;
			case 8:  turn_left; break;
		}
	}

	public void turn_180() {
		switch (@direction) {
			case 2:  turn_up; break;
			case 4:  turn_right; break;
			case 6:  turn_left; break;
			case 8:  turn_down; break;
		}
	}

	public void turn_right_or_left_90() {
		(rand(2) == 0) ? turn_right_90 : turn_left_90
	}

	public void turn_random() {
		switch (rand(4)) {
			case 0:  turn_up; break;
			case 1:  turn_right; break;
			case 2:  turn_left; break;
			case 3:  turn_down; break;
		}
	}

	public void turn_toward_player() {
		sx = @x + (@width / 2.0) - (Game.GameData.game_player.x + (Game.GameData.game_player.width / 2.0));
		sy = @y - (@height / 2.0) - (Game.GameData.game_player.y - (Game.GameData.game_player.height / 2.0));
		if (sx == 0 && sy == 0) return;
		if (sx.abs > sy.abs) {
			(sx > 0) ? turn_left : turn_right
		} else {
			(sy > 0) ? turn_up : turn_down
		}
	}

	public void turn_away_from_player() {
		sx = @x + (@width / 2.0) - (Game.GameData.game_player.x + (Game.GameData.game_player.width / 2.0));
		sy = @y - (@height / 2.0) - (Game.GameData.game_player.y - (Game.GameData.game_player.height / 2.0));
		if (sx == 0 && sy == 0) return;
		if (sx.abs > sy.abs) {
			(sx > 0) ? turn_right : turn_left
		} else {
			(sy > 0) ? turn_down : turn_up
		}
	}

	//-----------------------------------------------------------------------------
	// Updating
	//-----------------------------------------------------------------------------

	public void update() {
		if (Game.GameData.game_temp.in_menu) return;
		time_now = System.uptime;
		if (!@last_update_time || @last_update_time > time_now) @last_update_time = time_now;
		@delta_t = time_now - @last_update_time;
		@last_update_time = time_now;
		if (@delta_t > 0.25) return;   // Was in a menu; delay movement
		@moved_last_frame = @moved_this_frame;
		@stopped_last_frame = @stopped_this_frame;
		@moved_this_frame = false;
		@stopped_this_frame = false;
		// Update command
		update_command;
		// Update movement
		(moving() || jumping()) ? update_move : update_stop
		// Update animation
		update_pattern;
	}

	public void update_command() {
		if (@wait_count > 0) {
			if (System.uptime - @wait_start < @wait_count) return;
			@wait_count = 0;
			@wait_start = null;
		}
		if (@move_route_forcing) {
			move_type_custom;
		} else if (!@starting && !lock() && !moving() && !jumping()) {
			update_command_new;
		}
	}

	public void update_command_new() {
		if (@stop_count >= @command_delay) {
			switch (@move_type) {
				case 1:  move_type_random; break;
				case 2:  move_type_toward_player; break;
				case 3:  move_type_custom; break;
			}
		}
	}

	public void update_move() {
		if (@move_timer) {
			@move_timer += @delta_t;
			// Move horizontally
			if (@x != @move_initial_x) {
				dist = (@move_initial_x - @x).abs;
				@real_x = lerp(@move_initial_x, @x, @move_time * dist, @move_timer) * Game_Map.REAL_RES_X;
			}
			// Move vertically
			if (@y != @move_initial_y) {
				dist = (@move_initial_y - @y).abs;
				@real_y = lerp(@move_initial_y, @y, @move_time * dist, @move_timer) * Game_Map.REAL_RES_Y;
			}
		} else if (@jump_timer) {
			if (!@jump_time) self.jump_speed = 3;
			@jump_timer += @delta_t;
			dist = (int)Math.Max((@x - @jump_initial_x).abs, (@y - @jump_initial_y).abs);
			if (dist == 0) dist = 1;   // Jumping on spot
			// Move horizontally
			if (@x != @jump_initial_x) {
				@real_x = lerp(@jump_initial_x, @x, @jump_time * dist, @jump_timer) * Game_Map.REAL_RES_X;
			}
			// Move vertically
			if (@y != @jump_initial_y) {
				@real_y = lerp(@jump_initial_y, @y, @jump_time * dist, @jump_timer) * Game_Map.REAL_RES_Y;
			}
			// Calculate how far through the jump we are (from 0 to 1)
			@jump_fraction = @jump_timer / (@jump_time * dist);
		}
		// Snap to end position if close enough
		if ((@real_x - (@x * Game_Map.REAL_RES_X)).abs < Game_Map.X_SUBPIXELS / 2) @real_x = @x * Game_Map.REAL_RES_X;
		if ((@real_y - (@y * Game_Map.REAL_RES_Y)).abs < Game_Map.Y_SUBPIXELS / 2) @real_y = @y * Game_Map.REAL_RES_Y;
		// End of move
		if (moving() && @move_timer >= @move_time &&
			@real_x == @x * Game_Map.REAL_RES_X && @real_y == @y * Game_Map.REAL_RES_Y) {
			@move_timer = null;
			@bumping = false;
		}
		// End of jump
		if (jumping() && @jump_fraction >= 1) {
			@jump_timer = null;
			@jump_peak = 0;
			@jump_distance = 0;
			@jump_fraction = 0;
			@jumping_on_spot = false;
		}
		// End of a step, so perform events that happen at this time
		if (!jumping() && !moving()) {
			EventHandlers.trigger(:on_step_taken, self);
			calculate_bush_depth;
			@stopped_this_frame = true;
		} else if (!@moved_last_frame || @stopped_last_frame) {   // Started a new step
			calculate_bush_depth;
		}
		// Increment animation counter
		if (@walk_anime || @step_anime) @anime_count += @delta_t;
		@moved_this_frame = true;
	}

	public void update_stop() {
		if (@step_anime) @anime_count += @delta_t;
		if (!@starting && !lock()) @stop_count += @delta_t;
	}

	public void update_pattern() {
		if (@lock_pattern) return;
//    return if @jumping_on_spot   // Don't animate if jumping on the spot
		// Character has stopped moving, return to original pattern
		if (@moved_last_frame && !@moved_this_frame && !@step_anime) {
			@pattern = @original_pattern;
			@anime_count = 0;
			return;
		}
		// Character has started to move, change pattern immediately
		if (!@moved_last_frame && @moved_this_frame && !@step_anime) {
			if (@walk_anime) @pattern = (@pattern + 1) % 4;
			@anime_count = 0;
			return;
		}
		// Calculate how many frames each pattern should display for, i.e. the time
		// it takes to move half a tile (or a whole tile if cycling). We assume the
		// game uses square tiles.
		pattern_time = pattern_update_speed / 4;   // 4 frames per cycle in a charset
		if (@anime_count < pattern_time) return;
		// Advance to the next animation frame
		@pattern = (@pattern + 1) % 4;
		@anime_count -= pattern_time;
	}
}
