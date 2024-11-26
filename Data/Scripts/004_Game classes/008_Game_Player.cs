//===============================================================================
// ** Game_Player
//-------------------------------------------------------------------------------
//  This class handles the player. Its functions include event starting
//  determinants and map scrolling. Refer to "Game.GameData.game_player" for the one
//  instance of this class.
//===============================================================================
public partial class Game_Player : Game_Character {
	public int charsetData		{ get { return _charsetData; } set { _charsetData = value; } }			protected int _charsetData;
	public int encounter_count		{ get { return _encounter_count; } set { _encounter_count = value; } }			protected int _encounter_count;

	SCREEN_CENTER_X = ((Settings.SCREEN_WIDTH / 2) - (Game_Map.TILE_WIDTH / 2)) * Game_Map.X_SUBPIXELS;
	SCREEN_CENTER_Y = ((Settings.SCREEN_HEIGHT / 2) - (Game_Map.TILE_HEIGHT / 2)) * Game_Map.Y_SUBPIXELS;
	// Time in seconds for one cycle of bobbing (playing 4 charset frames) while
	// surfing or diving.
	public const int SURF_BOB_DURATION = 1.5;

	public override void initialize(*arg) {
		base.initialize(*arg);
		@lastdir = 0;
		@lastdirframe = 0;
	}

	public void map() {
		@map = null;
		return Game.GameData.game_map;
	}

	public void map_id() {
		return Game.GameData.game_map.map_id;
	}

	public override void screen_z(height = 0) {
		ret = base.screen_z();
		return ret + 1;
	}

	public bool has_follower() {
		return Game.GameData.PokemonGlobal.followers.length > 0;
	}

	public bool can_map_transfer_with_follower() {
		return Game.GameData.PokemonGlobal.followers.length == 0;
	}

	public bool can_ride_vehicle_with_follower() {
		return Game.GameData.PokemonGlobal.followers.length == 0;
	}

	//-----------------------------------------------------------------------------

	public bool can_run() {
		if (@move_route_forcing) return @move_speed > 3;
		if (@bumping) return false;
		if (Game.GameData.game_temp.in_menu || Game.GameData.game_temp.in_battle ||
										Game.GameData.game_temp.message_window_showing || MapInterpreterRunning()) return false;
		if (!Game.GameData.player.has_running_shoes && !Game.GameData.PokemonGlobal.diving &&
										!Game.GameData.PokemonGlobal.surfing && !Game.GameData.PokemonGlobal.bicycle) return false;
		if (jumping()) return false;
		if (TerrainTag.must_walk) return false;
		return (Game.GameData.PokemonSystem.runstyle == 1) ^ Input.press(Input.BACK);
	}

	public void set_movement_type(type) {
		meta = GameData.PlayerMetadata.get(Game.GameData.player&.character_ID || 1);
		new_charset = null;
		switch (type) {
			case :fishing:
				new_charset = GetPlayerCharset(meta.fish_charset);
				break;
			case :surf_fishing:
				new_charset = GetPlayerCharset(meta.surf_fish_charset);
				break;
			case :diving: case :diving_fast: case :diving_jumping: case :diving_stopped:
				if (!@move_route_forcing) self.move_speed = 3;
				new_charset = GetPlayerCharset(meta.dive_charset);
				break;
			case :surfing: case :surfing_fast: case :surfing_jumping: case :surfing_stopped:
				if (!@move_route_forcing) {
					self.move_speed = (type == types.surfing_jumping) ? 3 : 4;
				}
				new_charset = GetPlayerCharset(meta.surf_charset);
				break;
			case :descending_waterfall: case :ascending_waterfall:
				if (!@move_route_forcing) self.move_speed = 2;
				new_charset = GetPlayerCharset(meta.surf_charset);
				break;
			case :cycling: case :cycling_fast: case :cycling_jumping: case :cycling_stopped:
				if (!@move_route_forcing) {
					self.move_speed = (type == types.cycling_jumping) ? 3 : 5;
				}
				new_charset = GetPlayerCharset(meta.cycle_charset);
				break;
			case :running:
				if (!@move_route_forcing) self.move_speed = 4;
				new_charset = GetPlayerCharset(meta.run_charset);
				break;
			case :ice_sliding:
				if (!@move_route_forcing) self.move_speed = 4;
				new_charset = GetPlayerCharset(meta.walk_charset);
				break;
			default:   // :walking, :jumping, :walking_stopped
				if (!@move_route_forcing) self.move_speed = 3;
				new_charset = GetPlayerCharset(meta.walk_charset);
				break;
		}
		if (@bumping) self.move_speed = 3;
		if (new_charset) @character_name = new_charset;
	}

	// Called when the player's character or outfit changes. Assumes the player
	// isn't moving.
	public void refresh_charset() {
		meta = GameData.PlayerMetadata.get(Game.GameData.player&.character_ID || 1);
		new_charset = null;
		if (Game.GameData.PokemonGlobal&.diving) {
			new_charset = GetPlayerCharset(meta.dive_charset);
		} else if (Game.GameData.PokemonGlobal&.surfing) {
			new_charset = GetPlayerCharset(meta.surf_charset);
		} else if (Game.GameData.PokemonGlobal&.bicycle) {
			new_charset = GetPlayerCharset(meta.cycle_charset);
		} else {
			new_charset = GetPlayerCharset(meta.walk_charset);
		}
		if (new_charset) @character_name = new_charset;
	}

	//-----------------------------------------------------------------------------

	public void bump_into_object() {
		if (!@move_route_forcing) SEPlay("Player bump");
		Game.GameData.stats.bump_count += 1;
		@move_initial_x = @x;
		@move_initial_y = @y;
		@move_timer = 0.0;
		@bumping = true;
	}

	public void add_move_distance_to_stats(distance = 1) {
		if (Game.GameData.PokemonGlobal&.diving || Game.GameData.PokemonGlobal&.surfing) {
			Game.GameData.stats.distance_surfed += distance;
		} else if (Game.GameData.PokemonGlobal&.bicycle) {
			Game.GameData.stats.distance_cycled += distance;
		} else {
			Game.GameData.stats.distance_walked += distance;
		}
		if (Game.GameData.PokemonGlobal.ice_sliding) Game.GameData.stats.distance_slid_on_ice += distance;
	}

	public void move_generic(dir, turn_enabled = true) {
		if (turn_enabled) turn_generic(dir, true);
		if (!Game.GameData.game_temp.encounter_triggered) {
			if (can_move_in_direction(dir)) {
				x_offset = (dir == 4) ? -1 : (dir == 6) ? 1 : 0;
				y_offset = (dir == 8) ? -1 : (dir == 2) ? 1 : 0;
				// Jump over ledges
				if (FacingTerrainTag.ledge) {
					if (jumpForward(2)) {
						SEPlay("Player jump");
						increase_steps;
					}
					return;
				} else if (FacingTerrainTag.waterfall_crest && dir == 2) {
					Game.GameData.PokemonGlobal.descending_waterfall = true;
					Game.GameData.game_player.through = true;
					Game.GameData.stats.waterfalls_descended += 1;
				}
				// Jumping out of surfing back onto land
				if (EndSurf(x_offset, y_offset)) return;
				// General movement
				turn_generic(dir, true);
				if (!Game.GameData.game_temp.encounter_triggered) {
					@move_initial_x = @x;
					@move_initial_y = @y;
					@x += x_offset;
					@y += y_offset;
					@move_timer = 0.0;
					add_move_distance_to_stats(x_offset.abs + y_offset.abs);
					increase_steps;
				}
			} else if (!check_event_trigger_touch(dir)) {
				bump_into_object;
			}
		}
		Game.GameData.game_temp.encounter_triggered = false;
	}

	public override void turn_generic(dir, keep_enc_indicator = false) {
		old_direction = @direction;
		base.turn_generic(dir);
		if (@direction != old_direction && !@move_route_forcing && !MapInterpreterRunning()) {
			EventHandlers.trigger(:on_player_change_direction);
			if (!keep_enc_indicator) Game.GameData.game_temp.encounter_triggered = false;
		}
	}

	public override void jump(x_plus, y_plus) {
		old_x = @x;
		old_y = @y;
		base.jump();
		if (@x != old_x || @y != old_y) add_move_distance_to_stats(x_plus.abs + y_plus.abs);
	}

	//-----------------------------------------------------------------------------

	public void TerrainTag(countBridge = false) {
		if (Game.GameData.map_factory) return Game.GameData.map_factory.getTerrainTagFromCoords(self.map.map_id, @x, @y, countBridge);
		return Game.GameData.game_map.terrain_tag(@x, @y, countBridge);
	}

	public void FacingEvent(ignoreInterpreter = false) {
		if (Game.GameData.game_system.map_interpreter.running() && !ignoreInterpreter) return null;
		// Check the tile in front of the player for events
		new_x = @x + (@direction == 6 ? 1 : @direction == 4 ? -1 : 0);
		new_y = @y + (@direction == 2 ? 1 : @direction == 8 ? -1 : 0);
		if (!Game.GameData.game_map.valid(new_x, new_y)) return null;
		foreach (var event in Game.GameData.game_map.events) { //Game.GameData.game_map.events.each_value do => |event|
			if (!event.at_coordinate(new_x, new_y)) continue;
			if (event.jumping() || event.over_trigger()) continue;
			return event;
		}
		// If the tile in front is a counter, check one tile beyond that for events
		if (Game.GameData.game_map.counter(new_x, new_y)) {
			new_x += (@direction == 6 ? 1 : @direction == 4 ? -1 : 0);
			new_y += (@direction == 2 ? 1 : @direction == 8 ? -1 : 0);
			foreach (var event in Game.GameData.game_map.events) { //Game.GameData.game_map.events.each_value do => |event|
				if (!event.at_coordinate(new_x, new_y)) continue;
				if (event.jumping() || event.over_trigger()) continue;
				return event;
			}
		}
		return null;
	}

	public void FacingTerrainTag(dir = null) {
		if (!dir) dir = self.direction;
		if (Game.GameData.map_factory) return Game.GameData.map_factory.getFacingTerrainTag(dir, self);
		facing = FacingTile(dir, self);
		return Game.GameData.game_map.terrain_tag(facing[1], facing[2]);
	}

	// Passable Determinants
	//     x : x-coordinate
	//     y : y-coordinate
	//     d : direction (0, 2, 4, 6, 8)
	//         * 0 = Determines if all directions are impassable (for jumping)
	public bool passable(x, y, dir, strict = false) {
		// Get new coordinates
		new_x = x + (dir == 6 ? 1 : dir == 4 ? -1 : 0);
		new_y = y + (dir == 2 ? 1 : dir == 8 ? -1 : 0);
		// If coordinates are outside of map
		if (!Game.GameData.game_map.validLax(new_x, new_y)) return false;
		if (!Game.GameData.game_map.valid(new_x, new_y)) {
			if (!Game.GameData.map_factory) return false;
			return Game.GameData.map_factory.isPassableFromEdge(new_x, new_y, 10 - dir);
		}
		// If debug mode is ON and Ctrl key was pressed
		if (Core.DEBUG && Input.press(Input.CTRL)) return true;
		return super;
	}

	// Set Map Display Position to Center of Screen
	public void center(x, y) {
		self.map.display_x = (x * Game_Map.REAL_RES_X) - SCREEN_CENTER_X;
		self.map.display_y = (y * Game_Map.REAL_RES_Y) - SCREEN_CENTER_Y;
	}

	// Move to Designated Position
	//     x : x-coordinate
	//     y : y-coordinate
	public override void moveto(x, y) {
		base.moveto();
		center(x, y);
		make_encounter_count;
	}

	// Make Encounter Count
	public void make_encounter_count() {
		// Image of two dice rolling
		if (Game.GameData.game_map.map_id != 0) {
			n = Game.GameData.game_map.encounter_step;
			@encounter_count = rand(n) + rand(n) + 1;
		}
	}

	public void refresh() {
		@opacity    = 255;
		@blend_type = 0;
	}

	//-----------------------------------------------------------------------------

	public void TriggeredTrainerEvents(triggers, checkIfRunning = true, trainer_only = false) {
		result = new List<string>();
		// If event is running
		if (checkIfRunning && Game.GameData.game_system.map_interpreter.running()) return result;
		// All event loops
		foreach (var event in Game.GameData.game_map.events) { //Game.GameData.game_map.events.each_value do => |event|
			if (!triggers.Contains(event.trigger)) continue;
			if (!System.Text.RegularExpressions.Regex.IsMatch(event.name,@"trainer\((\d+)\)",RegexOptions.IgnoreCase) && (trainer_only || !System.Text.RegularExpressions.Regex.IsMatch(event.name,@"sight\((\d+)\)",RegexOptions.IgnoreCase))) continue;
			distance = $~[1].ToInt();
			if (!EventCanReachPlayer(event, self, distance)) continue;
			if (event.jumping() || event.over_trigger()) continue;
			result.Add(event);
		}
		return result;
	}

	public void TriggeredCounterEvents(triggers, checkIfRunning = true) {
		result = new List<string>();
		// If event is running
		if (checkIfRunning && Game.GameData.game_system.map_interpreter.running()) return result;
		// All event loops
		foreach (var event in Game.GameData.game_map.events) { //Game.GameData.game_map.events.each_value do => |event|
			if (!triggers.Contains(event.trigger)) continue;
			if (!System.Text.RegularExpressions.Regex.IsMatch(event.name,@"counter\((\d+)\)",RegexOptions.IgnoreCase)) continue;
			distance = $~[1].ToInt();
			if (!EventFacesPlayer(event, self, distance)) continue;
			if (event.jumping() || event.over_trigger()) continue;
			result.Add(event);
		}
		return result;
	}

	public void check_event_trigger_after_turning() { }

	public void CheckEventTriggerFromDistance(triggers) {
		events = TriggeredTrainerEvents(triggers);
		events.concat(TriggeredCounterEvents(triggers));
		if (events.length == 0) return false;
		ret = false;
		foreach (var event in events) { //'events.each' do => |event|
			event.start;
			if (event.starting) ret = true;
		}
		return ret;
	}

	// Trigger event(s) at the same coordinates as self with the appropriate
	// trigger(s) that can be triggered
	public void check_event_trigger_here(triggers) {
		result = false;
		// If event is running
		if (Game.GameData.game_system.map_interpreter.running()) return result;
		// All event loops
		foreach (var event in Game.GameData.game_map.events) { //Game.GameData.game_map.events.each_value do => |event|
			// If event coordinates and triggers are consistent
			if (!event.at_coordinate(@x, @y)) continue;
			if (!triggers.Contains(event.trigger)) continue;
			// If starting determinant is same position event (other than jumping)
			if (event.jumping() || !event.over_trigger()) continue;
			event.start;
			if (event.starting) result = true;
		}
		return result;
	}

	// Front Event Starting Determinant
	public void check_event_trigger_there(triggers) {
		result = false;
		// If event is running
		if (Game.GameData.game_system.map_interpreter.running()) return result;
		// Calculate front event coordinates
		new_x = @x + (@direction == 6 ? 1 : @direction == 4 ? -1 : 0);
		new_y = @y + (@direction == 2 ? 1 : @direction == 8 ? -1 : 0);
		if (!Game.GameData.game_map.valid(new_x, new_y)) return false;
		// All event loops
		foreach (var event in Game.GameData.game_map.events) { //Game.GameData.game_map.events.each_value do => |event|
			if (!triggers.Contains(event.trigger)) continue;
			// If event coordinates and triggers are consistent
			if (!event.at_coordinate(new_x, new_y)) continue;
			// If starting determinant is front event (other than jumping)
			if (event.jumping() || event.over_trigger()) continue;
			event.start;
			if (event.starting) result = true;
		}
		// If fitting event is not found
		if (result == false && Game.GameData.game_map.counter(new_x, new_y)) {
			// Calculate coordinates of 1 tile further away
			new_x += (@direction == 6 ? 1 : @direction == 4 ? -1 : 0);
			new_y += (@direction == 2 ? 1 : @direction == 8 ? -1 : 0);
			if (!Game.GameData.game_map.valid(new_x, new_y)) return false;
			// All event loops
			foreach (var event in Game.GameData.game_map.events) { //Game.GameData.game_map.events.each_value do => |event|
				if (!triggers.Contains(event.trigger)) continue;
				// If event coordinates and triggers are consistent
				if (!event.at_coordinate(new_x, new_y)) continue;
				// If starting determinant is front event (other than jumping)
				if (event.jumping() || event.over_trigger()) continue;
				event.start;
				if (event.starting) result = true;
			}
		}
		return result;
	}

	// Touch Event Starting Determinant
	public void check_event_trigger_touch(dir) {
		result = false;
		if (Game.GameData.game_system.map_interpreter.running()) return result;
		// All event loops
		x_offset = (dir == 4) ? -1 : (dir == 6) ? 1 : 0;
		y_offset = (dir == 8) ? -1 : (dir == 2) ? 1 : 0;
		foreach (var event in Game.GameData.game_map.events) { //Game.GameData.game_map.events.each_value do => |event|
			if (!new []{1, 2}.Contains(event.trigger)) continue;   // Player touch, event touch
			// If event coordinates and triggers are consistent
			if (!event.at_coordinate(@x + x_offset, @y + y_offset)) continue;
			if (System.Text.RegularExpressions.Regex.IsMatch(event.name,@"(?:sight|trainer)\((\d+)\)",RegexOptions.IgnoreCase)) {
				distance = $~[1].ToInt();
				if (!EventCanReachPlayer(event, self, distance)) continue;
			} else if (System.Text.RegularExpressions.Regex.IsMatch(event.name,@"counter\((\d+)\)",RegexOptions.IgnoreCase)) {
				distance = $~[1].ToInt();
				if (!EventFacesPlayer(event, self, distance)) continue;
			}
			// If starting determinant is front event (other than jumping)
			if (event.jumping() || event.over_trigger()) continue;
			event.start;
			if (event.starting) result = true;
		}
		return result;
	}

	//-----------------------------------------------------------------------------

	public override void update() {
		last_real_x = @real_x;
		last_real_y = @real_y;
		@last_terrain_tag = TerrainTag;
		base.update();
		if (Game.GameData.game_temp.in_menu && @stopped_last_frame) update_stop;
		update_screen_position(last_real_x, last_real_y);
		// Update dependent events
		if ((!@moved_last_frame || @stopped_last_frame) && (moving() || jumping()) && !@bumping) {
			Game.GameData.game_temp.followers.move_followers;
		}
		Game.GameData.game_temp.followers.update;
		update_event_triggering;
	}

	public void update_command_new() {
		dir = Input.dir4;
		if (Game.GameData.PokemonGlobal.forced_movement()) {
			move_forward;
		} else if (!MapInterpreterRunning() && !Game.GameData.game_temp.message_window_showing &&
					!Game.GameData.game_temp.in_mini_update && !Game.GameData.game_temp.in_menu) {
			// Move player in the direction the directional button is being pressed
			if (@moved_last_frame ||
				(dir > 0 && dir == @lastdir && System.uptime - @lastdirframe >= 0.075)) {
				switch (dir) {
					case 2:  move_down; break;
					case 4:  move_left; break;
					case 6:  move_right; break;
					case 8:  move_up; break;
				}
			} else if (dir != @lastdir) {
				switch (dir) {
					case 2:  turn_down; break;
					case 4:  turn_left; break;
					case 6:  turn_right; break;
					case 8:  turn_up; break;
				}
			}
			// Record last direction input
			if (dir != @lastdir) @lastdirframe = System.uptime;
			@lastdir = dir;
		}
	}

	public void update_move() {
		if (!@moved_last_frame || @stopped_last_frame) {   // Started a new step
			if (Game.GameData.PokemonGlobal.ice_sliding || @last_terrain_tag.ice) {
				set_movement_type(MovementTypes.ice_sliding);
			} else if (Game.GameData.PokemonGlobal.descending_waterfall) {
				set_movement_type(MovementTypes.descending_waterfall);
			} else if (Game.GameData.PokemonGlobal.ascending_waterfall) {
				set_movement_type(MovementTypes.ascending_waterfall);
			} else {
				faster = can_run();
				if (Game.GameData.PokemonGlobal&.diving) {
					set_movement_type((faster) ? MovementTypes.diving_fast : MovementTypes.diving);
				} else if (Game.GameData.PokemonGlobal&.surfing) {
					set_movement_type((faster) ? MovementTypes.surfing_fast : MovementTypes.surfing);
				} else if (Game.GameData.PokemonGlobal&.bicycle) {
					set_movement_type((faster) ? MovementTypes.cycling_fast : MovementTypes.cycling);
				} else {
					set_movement_type((faster) ? MovementTypes.running : MovementTypes.walking);
				}
			}
			if (jumping()) {
				if (Game.GameData.PokemonGlobal&.diving) {
					set_movement_type(MovementTypes.diving_jumping);
				} else if (Game.GameData.PokemonGlobal&.surfing) {
					set_movement_type(MovementTypes.surfing_jumping);
				} else if (Game.GameData.PokemonGlobal&.bicycle) {
					set_movement_type(MovementTypes.cycling_jumping);
				} else {
					set_movement_type(MovementTypes.jumping);   // Walking speed/charset while jumping
				}
			}
		}
		was_jumping = jumping();
		super;
		if (was_jumping && !jumping() && !@transparent && (@tile_id > 0 || @character_name != "")) {
			if (!Game.GameData.PokemonGlobal.surfing || Game.GameData.game_temp.ending_surf) {
				spriteset = Game.GameData.scene.spriteset(map_id);
				spriteset&.addUserAnimation(Settings.DUST_ANIMATION_ID, self.x, self.y, true, 1);
			}
		}
	}

	public void update_stop() {
		if (@stopped_last_frame) {
			if (Game.GameData.PokemonGlobal&.diving) {
				set_movement_type(MovementTypes.diving_stopped);
			} else if (Game.GameData.PokemonGlobal&.surfing) {
				set_movement_type(MovementTypes.surfing_stopped);
			} else if (Game.GameData.PokemonGlobal&.bicycle) {
				set_movement_type(MovementTypes.cycling_stopped);
			} else {
				set_movement_type(MovementTypes.walking_stopped);
			}
		}
		super;
	}

	public override void update_pattern() {
		if (Game.GameData.PokemonGlobal&.surfing || Game.GameData.PokemonGlobal&.diving) {
			bob_pattern = (4 * System.uptime / SURF_BOB_DURATION).ToInt() % 4;
			if (!@lock_pattern) @pattern = bob_pattern;
			@pattern_surf = bob_pattern;
			@bob_height = (bob_pattern >= 2) ? 2 : 0;
			@anime_count = 0;
		} else {
			@bob_height = 0;
			base.update_pattern();
		}
	}

	// Track the player on-screen as they move.
	public void update_screen_position(last_real_x, last_real_y) {
		if (self.map.scrolling() || !(@moved_last_frame || @moved_this_frame)) return;
		if ((@real_x < last_real_x && @real_x < Game.GameData.game_map.display_x + SCREEN_CENTER_X) ||
			(@real_x > last_real_x && @real_x > Game.GameData.game_map.display_x + SCREEN_CENTER_X)) {
			self.map.display_x += @real_x - last_real_x;
		}
		if ((@real_y < last_real_y && @real_y < Game.GameData.game_map.display_y + SCREEN_CENTER_Y) ||
			(@real_y > last_real_y && @real_y > Game.GameData.game_map.display_y + SCREEN_CENTER_Y)) {
			self.map.display_y += @real_y - last_real_y;
		}
	}

	public void update_event_triggering() {
		if (moving() || jumping() || Game.GameData.PokemonGlobal.forced_movement()) return;
		// Try triggering events upon walking into them/in front of them
		if (@moved_this_frame) {
			Game.GameData.game_temp.followers.turn_followers;
			result = CheckEventTriggerFromDistance([2]);
			// Event determinant is via touch of same position event
			result |= check_event_trigger_here(new {1, 2});
			// No events triggered, try other event triggers upon finishing a step
			OnStepTaken(result);
		}
	}
}

//===============================================================================
//
//===============================================================================
public void GetPlayerCharset(charset, trainer = null, force = false) {
	if (!trainer) trainer = Game.GameData.player;
	outfit = (trainer) ? trainer.outfit : 0;
	if (!force && Game.GameData.game_player&.charsetData &&
								Game.GameData.game_player.charsetData[0] == trainer.character_ID &&
								Game.GameData.game_player.charsetData[1] == charset &&
								Game.GameData.game_player.charsetData[2] == outfit) return null;
	if (Game.GameData.game_player) Game.GameData.game_player.charsetData = new {trainer.character_ID, charset, outfit};
	ret = charset;
	if (ResolveBitmap("Graphics/Characters/" + ret + "_" + outfit.ToString())) {
		ret = ret + "_" + outfit.ToString();
	}
	return ret;
}

public void UpdateVehicle() {
	if (Game.GameData.PokemonGlobal&.diving) {
		Game.GameData.game_player.set_movement_type(MovementTypes.diving_stopped);
	} else if (Game.GameData.PokemonGlobal&.surfing) {
		Game.GameData.game_player.set_movement_type(MovementTypes.surfing_stopped);
	} else if (Game.GameData.PokemonGlobal&.bicycle) {
		Game.GameData.game_player.set_movement_type(MovementTypes.cycling_stopped);
	} else {
		Game.GameData.game_player.set_movement_type(MovementTypes.walking_stopped);
	}
}

public void CancelVehicles(destination = null, cancel_swimming = true) {
	if (cancel_swimming) Game.GameData.PokemonGlobal.surfing = false;
	if (cancel_swimming) Game.GameData.PokemonGlobal.diving  = false;
	if (!destination || !CanUseBike(destination)) Game.GameData.PokemonGlobal.bicycle = false;
	UpdateVehicle;
}

public bool CanUseBike(map_id) {
	map_metadata = GameData.MapMetadata.try_get(map_id);
	if (!map_metadata) return false;
	return map_metadata.always_bicycle || map_metadata.can_bicycle || map_metadata.outdoor_map;
}

public void MountBike() {
	if (Game.GameData.PokemonGlobal.bicycle) return;
	Game.GameData.PokemonGlobal.bicycle = true;
	Game.GameData.stats.cycle_count += 1;
	UpdateVehicle;
	bike_bgm = GameData.Metadata.get.bicycle_BGM;
	if (bike_bgm) CueBGM(bike_bgm, 0.4);
	SEPlay("Bicycle");
	PokeRadarCancel;
}

public void DismountBike() {
	if (!Game.GameData.PokemonGlobal.bicycle) return;
	Game.GameData.PokemonGlobal.bicycle = false;
	UpdateVehicle;
	Game.GameData.game_map.autoplayAsCue;
}
