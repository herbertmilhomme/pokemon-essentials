//===============================================================================
// Instances of this are stored in @realEvents.
//===============================================================================
public partial class Game_Follower : Game_Event {
	public int map		{ get { return _map; } }			protected int _map;

	public void initialize(event_data) {
		// Create RPG.Event to base self on
		rpg_event = new RPG.Event(event_data.x, event_data.y);
		rpg_event.id = event_data.event_id;
		rpg_event.name = event_data.event_name;
		if (event_data.common_event_id) {
			// Must setup common event list here and now
			common_event = new Game_CommonEvent(event_data.common_event_id);
			rpg_event.pages[0].list = common_event.list;
		}
		// Create self
		super(event_data.original_map_id, rpg_event, Game.GameData.map_factory.getMap(event_data.current_map_id));
		// Modify self
		self.character_name = event_data.character_name;
		self.character_hue  = event_data.character_hue;
		switch (event_data.direction) {
			case 2:  turn_down; break;
			case 4:  turn_left; break;
			case 6:  turn_right; break;
			case 8:  turn_up; break;
		}
	}

	public void map_id() {
		return @map.map_id;
	}

	//-----------------------------------------------------------------------------

	public void move_through(direction) {
		old_through = @through;
		@through = true;
		switch (direction) {
			case 2:  move_down; break;
			case 4:  move_left; break;
			case 6:  move_right; break;
			case 8:  move_up; break;
		}
		@through = old_through;
	}

	public void move_fancy(direction) {
		delta_x = (direction == 6) ? 1 : (direction == 4) ? -1 : 0;
		delta_y = (direction == 2) ? 1 : (direction == 8) ? -1 : 0;
		new_x = self.x + delta_x;
		new_y = self.y + delta_y;
		// Move if new position is the player's, or the new position is passable,
		// or self's current position is not passable
		if ((Game.GameData.game_player.x == new_x && Game.GameData.game_player.y == new_y) ||
			location_passable(new_x, new_y, 10 - direction) ||
			!location_passable(self.x, self.y, direction)) {
			move_through(direction);
		}
	}

	public void jump_fancy(direction, leader) {
		delta_x = (direction == 6) ? 2 : (direction == 4) ? -2 : 0;
		delta_y = (direction == 2) ? 2 : (direction == 8) ? -2 : 0;
		half_delta_x = delta_x / 2;
		half_delta_y = delta_y / 2;
		if (location_passable(self.x + half_delta_x, self.y + half_delta_y, 10 - direction)) {
			// Can walk over the middle tile normally; just take two steps
			move_fancy(direction);
			move_fancy(direction);
		} else if (location_passable(self.x + delta_x, self.y + delta_y, 10 - direction)) {
			// Can't walk over the middle tile, but can walk over the end tile; jump over
			if (location_passable(self.x, self.y, direction)) {
				if (leader.jumping()) {
					self.jump_speed = leader.jump_speed || 3;
				} else {
					self.jump_speed = leader.move_speed || 3;
					// This is halved because self has to jump 2 tiles in the time it takes
					// the leader to move one tile
					@jump_time /= 2;
				}
				jump(delta_x, delta_y);
			} else {
				// self's current tile isn't passable; just take two steps ignoring passability
				move_through(direction);
				move_through(direction);
			}
		}
	}

	public void fancy_moveto(new_x, new_y, leader) {
		if (self.x - new_x == 1 && self.y == new_y) {
			move_fancy(4);
		} else if (self.x - new_x == -1 && self.y == new_y) {
			move_fancy(6);
		} else if (self.x == new_x && self.y - new_y == 1) {
			move_fancy(8);
		} else if (self.x == new_x && self.y - new_y == -1) {
			move_fancy(2);
		} else if (self.x - new_x == 2 && self.y == new_y) {
			jump_fancy(4, leader);
		} else if (self.x - new_x == -2 && self.y == new_y) {
			jump_fancy(6, leader);
		} else if (self.x == new_x && self.y - new_y == 2) {
			jump_fancy(8, leader);
		} else if (self.x == new_x && self.y - new_y == -2) {
			jump_fancy(2, leader);
		} else if (self.x != new_x || self.y != new_y) {
			moveto(new_x, new_y);
		}
	}

	// Ceases all movement immediately. Used when the leader wants to move another
	// tile but self hasn't quite finished its previous movement yet.
	public void end_movement() {
		@x = x % self.map.width;
		@y = y % self.map.height;
		@real_x = @x * Game_Map.REAL_RES_X;
		@real_y = @y * Game_Map.REAL_RES_Y;
		@move_timer = null;
		@jump_timer = null;
		@jump_peak = 0;
		@jump_distance = 0;
		@jump_fraction = 0;
		@jumping_on_spot = false;
	}

	//-----------------------------------------------------------------------------

	public void turn_towards_leader(leader) {
		TurnTowardEvent(self, leader);
	}

	public void follow_leader(leader, instant = false, leaderIsTrueLeader = true) {
		if (@move_route_forcing) return;
		end_movement;
		maps_connected = Game.GameData.map_factory.areConnected(leader.map.map_id, self.map.map_id);
		target = null;
		// Get the target tile that self wants to move to
		if (maps_connected) {
			behind_direction = 10 - leader.direction;
			target = Game.GameData.map_factory.getFacingTile(behind_direction, leader);
			if (target && Game.GameData.map_factory.getTerrainTag(target[0], target[1], target[2]).ledge) {
				// Get the tile above the ledge (where the leader jumped from)
				target = Game.GameData.map_factory.getFacingTileFromPos(target[0], target[1], target[2], behind_direction);
			}
			if (!target) target = new {leader.map.map_id, leader.x, leader.y};
		} else {
			// Map transfer to an unconnected map
			target = new {leader.map.map_id, leader.x, leader.y};
		}
		// Move self to the target
		if (self.map.map_id != target[0]) {
			vector = Game.GameData.map_factory.getRelativePos(target[0], 0, 0, self.map.map_id, @x, @y);
			@map = Game.GameData.map_factory.getMap(target[0]);
			// NOTE: Can't use moveto because vector is outside the boundaries of the
			//       map, and moveto doesn't allow setting invalid coordinates.
			@x = vector[0];
			@y = vector[1];
			@real_x = @x * Game_Map.REAL_RES_X;
			@real_y = @y * Game_Map.REAL_RES_Y;
		}
		if (instant || !maps_connected) {
			moveto(target[1], target[2]);
		} else {
			fancy_moveto(target[1], target[2], leader);
		}
	}

	//-----------------------------------------------------------------------------

	private;

	public bool location_passable(x, y, direction) {
		this_map = self.map;
		if (!this_map || !this_map.valid(x, y)) return false;
		if (@through) return true;
		passed_tile_checks = false;
		bit = (1 << ((direction / 2) - 1)) & 0x0f;
		// Check all events for ones using tiles as graphics, and see if they're passable
		foreach (var event in this_map.events) { //this_map.events.each_value do => |event|
			if (event.tile_id < 0 || event.through || !event.at_coordinate(x, y)) continue;
			tile_data = GameData.TerrainTag.try_get(this_map.terrain_tags[event.tile_id]);
			if (tile_data.ignore_passability) continue;
			if (tile_data.bridge && Game.GameData.PokemonGlobal.bridge == 0) continue;
			if (tile_data.ledge) return false;
			passage = this_map.passages[event.tile_id] || 0;
			if (passage & bit != 0) return false;
			if ((tile_data.bridge && Game.GameData.PokemonGlobal.bridge > 0) ||
																	(this_map.priorities[event.tile_id] || -1) == 0) passed_tile_checks = true;
			if (passed_tile_checks) break;
		}
		// Check if tiles at (x, y) allow passage for followe
		if (!passed_tile_checks) {
			new {2, 1, 0}.each do |i|
				tile_id = this_map.data[x, y, i] || 0;
				if (tile_id == 0) continue;
				tile_data = GameData.TerrainTag.try_get(this_map.terrain_tags[tile_id]);
				if (tile_data.ignore_passability) continue;
				if (tile_data.bridge && Game.GameData.PokemonGlobal.bridge == 0) continue;
				if (tile_data.ledge) return false;
				passage = this_map.passages[tile_id] || 0;
				if (passage & bit != 0) return false;
				if (tile_data.bridge && Game.GameData.PokemonGlobal.bridge > 0) break;
				if ((this_map.priorities[tile_id] || -1) == 0) break;
			}
		}
		// Check all events on the map to see if any are in the way
		foreach (var event in this_map.events) { //this_map.events.each_value do => |event|
			if (!event.at_coordinate(x, y)) continue;
			if (!event.through && event.character_name != "") return false;
		}
		return true;
	}
}
