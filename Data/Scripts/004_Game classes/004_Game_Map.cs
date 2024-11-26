//===============================================================================
// ** Game_Map
//------------------------------------------------------------------------------
//  This class handles the map. It includes scrolling and passable determining
//  functions. Refer to "Game.GameData.game_map" for the instance of this class.
//===============================================================================
public partial class Game_Map {
	public int map_id		{ get { return _map_id; } set { _map_id = value; } }			protected int _map_id;
	/// <summary>tileset file name</summary>
	public int tileset_name		{ get { return _tileset_name; } set { _tileset_name = value; } }			protected int _tileset_name;
	/// <summary>autotile file name</summary>
	public int autotile_names		{ get { return _autotile_names; } set { _autotile_names = value; } }			protected int _autotile_names;
	/// <summary>passage table</summary>
	public int passages		{ get { return _passages; } }			protected int _passages;
	/// <summary>priority table</summary>
	public int priorities		{ get { return _priorities; } }			protected int _priorities;
	/// <summary>terrain tag table</summary>
	public int terrain_tags		{ get { return _terrain_tags; } }			protected int _terrain_tags;
	/// <summary>events</summary>
	public int events		{ get { return _events; } }			protected int _events;
	/// <summary>panorama file name</summary>
	public int panorama_name		{ get { return _panorama_name; } set { _panorama_name = value; } }			protected int _panorama_name;
	/// <summary>panorama hue</summary>
	public int panorama_hue		{ get { return _panorama_hue; } set { _panorama_hue = value; } }			protected int _panorama_hue;
	/// <summary>fog file name</summary>
	public int fog_name		{ get { return _fog_name; } set { _fog_name = value; } }			protected int _fog_name;
	/// <summary>fog hue</summary>
	public int fog_hue		{ get { return _fog_hue; } set { _fog_hue = value; } }			protected int _fog_hue;
	/// <summary>fog opacity level</summary>
	public int fog_opacity		{ get { return _fog_opacity; } set { _fog_opacity = value; } }			protected int _fog_opacity;
	/// <summary>fog blending method</summary>
	public int fog_blend_type		{ get { return _fog_blend_type; } set { _fog_blend_type = value; } }			protected int _fog_blend_type;
	/// <summary>fog zoom rate</summary>
	public int fog_zoom		{ get { return _fog_zoom; } set { _fog_zoom = value; } }			protected int _fog_zoom;
	/// <summary>fog sx</summary>
	public int fog_sx		{ get { return _fog_sx; } set { _fog_sx = value; } }			protected int _fog_sx;
	/// <summary>fog sy</summary>
	public int fog_sy		{ get { return _fog_sy; } set { _fog_sy = value; } }			protected int _fog_sy;
	/// <summary>fog x-coordinate starting point</summary>
	public int fog_ox		{ get { return _fog_ox; } }			protected int _fog_ox;
	/// <summary>fog y-coordinate starting point</summary>
	public int fog_oy		{ get { return _fog_oy; } }			protected int _fog_oy;
	/// <summary>fog color tone</summary>
	public int fog_tone		{ get { return _fog_tone; } }			protected int _fog_tone;
	/// <summary>battleback file name</summary>
	public int battleback_name		{ get { return _battleback_name; } set { _battleback_name = value; } }			protected int _battleback_name;
	/// <summary>display x-coordinate * 128</summary>
	public int display_x		{ get { return _display_x; } }			protected int _display_x;
	/// <summary>display y-coordinate * 128</summary>
	public int display_y		{ get { return _display_y; } }			protected int _display_y;
	/// <summary>refresh request flag</summary>
	public int need_refresh		{ get { return _need_refresh; } set { _need_refresh = value; } }			protected int _need_refresh;

	public const int TILE_WIDTH  = 32;
	public const int TILE_HEIGHT = 32;
	public const int X_SUBPIXELS = 4;
	public const int Y_SUBPIXELS = 4;
	public const int REAL_RES_X  = TILE_WIDTH * X_SUBPIXELS;
	public const int REAL_RES_Y  = TILE_HEIGHT * Y_SUBPIXELS;

	public void initialize() {
		@map_id = 0;
		@display_x = 0;
		@display_y = 0;
	}

	public void setup(map_id) {
		@map_id = map_id;
		@map = load_data(string.Format("Data/Map{0:3}.rxdata", map_id));
		tileset = Game.GameData.data_tilesets[@map.tileset_id];
		updateTileset;
		@fog_ox                  = 0;
		@fog_oy                  = 0;
		@fog_tone                = new Tone(0, 0, 0, 0);
		@fog_tone_target         = new Tone(0, 0, 0, 0);
		@fog_tone_duration       = 0;
		@fog_tone_timer_start    = null;
		@fog_opacity_duration    = 0;
		@fog_opacity_target      = 0;
		@fog_opacity_timer_start = null;
		self.display_x           = 0;
		self.display_y           = 0;
		@need_refresh            = false;
		EventHandlers.trigger(:on_game_map_setup, map_id, @map, tileset);
		@events                  = new List<string>();
		@map.events.each_key do |i|
			@events[i]             = new Game_Event(@map_id, @map.events[i], self);
		}
		@common_events           = new List<string>();
		for (int i = 1; i < Game.GameData.data_common_events.size; i++) { //each 'Game.GameData.data_common_events.size' do => |i|
			@common_events[i]      = new Game_CommonEvent(i);
		}
		@scroll_distance_x       = 0;
		@scroll_distance_y       = 0;
		@scroll_speed            = 4;
	}

	public void updateTileset() {
		tileset = Game.GameData.data_tilesets[@map.tileset_id];
		@tileset_name    = tileset.tileset_name;
		@autotile_names  = tileset.autotile_names;
		@panorama_name   = tileset.panorama_name;
		@panorama_hue    = tileset.panorama_hue;
		@fog_name        = tileset.fog_name;
		@fog_hue         = tileset.fog_hue;
		@fog_opacity     = tileset.fog_opacity;
		@fog_blend_type  = tileset.fog_blend_type;
		@fog_zoom        = tileset.fog_zoom;
		@fog_sx          = tileset.fog_sx;
		@fog_sy          = tileset.fog_sy;
		@battleback_name = tileset.battleback_name;
		@passages        = tileset.passages;
		@priorities      = tileset.priorities;
		@terrain_tags    = tileset.terrain_tags;
	}

	public int width          { get { return @map.width;          } }
	public int height         { get { return @map.height;         } }
	public int encounter_list { get { return @map.encounter_list; } }
	public int encounter_step { get { return @map.encounter_step; } }
	public int data           { get { return @map.data;           } }
	public int tileset_id     { get { return @map.tileset_id;     } }
	public int bgm            { get { return @map.bgm;            } }

	public void name() {
		return GetMapNameFromId(@map_id);
	}

	public void metadata() {
		return GameData.MapMetadata.try_get(@map_id);
	}

	// Returns the name of this map's BGM. If it's night time, returns the night
	// version of the BGM (if it exists).
	public void bgm_name() {
		if (DayNight.isNight() && FileTest.audio_exist("Audio/BGM/" + @map.bgm.name + "_n")) {
			return @map.bgm.name + "_n";
		}
		return @map.bgm.name;
	}

	// Autoplays background music
	// Plays music called "[normal BGM]_n" if it's night time and it exists
	public void autoplayAsCue() {
		if (@map.autoplay_bgm) CueBGM(bgm_name, 1.0, @map.bgm.volume, @map.bgm.pitch);
		if (@map.autoplay_bgs) BGSPlay(@map.bgs);
	}

	// Plays background music
	// Plays music called "[normal BGM]_n" if it's night time and it exists
	public void autoplay() {
		if (@map.autoplay_bgm) BGMPlay(bgm_name, @map.bgm.volume, @map.bgm.pitch);
		if (@map.autoplay_bgs) BGSPlay(@map.bgs);
	}

	public bool valid(x, y) {
		return x >= 0 && x < width && y >= 0 && y < height;
	}

	public bool validLax(x, y) {
		return x >= -10 && x <= width + 10 && y >= -10 && y <= height + 10;
	}

	public bool passable(x, y, dir, self_event = null) {
		if (!valid(x, y)) return false;
		bit = (1 << ((dir / 2) - 1)) & 0x0f;
		foreach (var event in events) { //events.each_value do => |event|
			if (event.tile_id <= 0) continue;
			if (event == self_event) continue;
			if (!event.at_coordinate(x, y)) continue;
			if (event.through) continue;
			if (GameData.TerrainTag.try_get(@terrain_tags[event.tile_id]).ignore_passability) continue;
			passage = @passages[event.tile_id];
			if (passage & bit != 0) return false;
			if (passage & 0x0f == 0x0f) return false;
			if (@priorities[event.tile_id] == 0) return true;
		}
		if (self_event == Game.GameData.game_player) return playerPassable(x, y, dir, self_event);
		// All other events
		newx = x;
		newy = y;
		switch (dir) {
			case 1:
				newx -= 1;
				newy += 1;
				break;
			case 2:
				newy += 1;
				break;
			case 3:
				newx += 1;
				newy += 1;
				break;
			case 4:
				newx -= 1;
				break;
			case 6:
				newx += 1;
				break;
			case 7:
				newx -= 1;
				newy -= 1;
				break;
			case 8:
				newy -= 1;
				break;
			case 9:
				newx += 1;
				newy -= 1;
				break;
		}
		if (!valid(newx, newy)) return false;
		new {2, 1, 0}.each do |i|
			tile_id = data[x, y, i];
			terrain = GameData.TerrainTag.try_get(@terrain_tags[tile_id]);
			// If already on water, only allow movement to another water tile
			if (self_event && terrain.can_surf_freely) {
				new {2, 1, 0}.each do |j|
					facing_tile_id = data[newx, newy, j];
					if (facing_tile_id == 0) continue;
					if (facing_tile_id.null()) return false;
					facing_terrain = GameData.TerrainTag.try_get(@terrain_tags[facing_tile_id]);
					if (facing_terrain.id != :None && !facing_terrain.ignore_passability) {
						return facing_terrain.can_surf_freely;
					}
				}
				return false;
			// Can't walk onto ice
			} else if (terrain.ice) {
				return false;
			} else if (self_event && self_event.x == x && self_event.y == y) {
				// Can't walk onto ledges
				new {2, 1, 0}.each do |j|
					facing_tile_id = data[newx, newy, j];
					if (facing_tile_id == 0) continue;
					if (facing_tile_id.null()) return false;
					facing_terrain = GameData.TerrainTag.try_get(@terrain_tags[facing_tile_id]);
					if (facing_terrain.ledge) return false;
					if (facing_terrain.id != :None && !facing_terrain.ignore_passability) break;
				}
			}
			if (terrain&.ignore_passability) continue;
			if (tile_id == 0) continue;
			// Regular passability checks
			passage = @passages[tile_id];
			if (passage & bit != 0 || passage & 0x0f == 0x0f) return false;
			if (@priorities[tile_id] == 0) return true;
		}
		return true;
	}

	public bool playerPassable(x, y, dir, self_event = null) {
		bit = (1 << ((dir / 2) - 1)) & 0x0f;
		new {2, 1, 0}.each do |i|
			tile_id = data[x, y, i];
			if (tile_id == 0) continue;
			terrain = GameData.TerrainTag.try_get(@terrain_tags[tile_id]);
			passage = @passages[tile_id];
			if (terrain) {
				// Ignore bridge tiles if not on a bridge
				if (terrain.bridge && Game.GameData.PokemonGlobal.bridge == 0) continue;
				// Make water tiles passable if player is surfing
				if (Game.GameData.PokemonGlobal.surfing && terrain.can_surf && !terrain.waterfall) return true;
				// Prevent cycling in really tall grass/on ice
				if (Game.GameData.PokemonGlobal.bicycle && (terrain.must_walk || terrain.must_walk_or_run)) return false;
				// Depend on passability of bridge tile if on bridge
				if (terrain.bridge && Game.GameData.PokemonGlobal.bridge > 0) {
					return (passage & bit == 0 && passage & 0x0f != 0x0f);
				}
			}
			if (terrain&.ignore_passability) continue;
			// Regular passability checks
			if (passage & bit != 0 || passage & 0x0f == 0x0f) return false;
			if (@priorities[tile_id] == 0) return true;
		}
		return true;
	}

	// Returns whether the position x,y is fully passable (there is no blocking
	// event there, and the tile is fully passable in all directions).
	public bool passableStrict(x, y, d, self_event = null) {
		if (!valid(x, y)) return false;
		foreach (var event in events) { //events.each_value do => |event|
			if (event == self_event || event.tile_id < 0 || event.through) continue;
			if (!event.at_coordinate(x, y)) continue;
			if (GameData.TerrainTag.try_get(@terrain_tags[event.tile_id]).ignore_passability) continue;
			if (@passages[event.tile_id] & 0x0f != 0) return false;
			if (@priorities[event.tile_id] == 0) return true;
		}
		new {2, 1, 0}.each do |i|
			tile_id = data[x, y, i];
			if (tile_id == 0) continue;
			if (GameData.TerrainTag.try_get(@terrain_tags[tile_id]).ignore_passability) continue;
			if (@passages[tile_id] & 0x0f != 0) return false;
			if (@priorities[tile_id] == 0) return true;
		}
		return true;
	}

	public bool bush(x, y) {
		new {2, 1, 0}.each do |i|
			tile_id = data[x, y, i];
			if (tile_id == 0) continue;
			if (GameData.TerrainTag.try_get(@terrain_tags[tile_id]).bridge &&
											Game.GameData.PokemonGlobal.bridge > 0) return false;
			if (@passages[tile_id] & 0x40 == 0x40) return true;
		}
		return false;
	}

	public bool deepBush(x, y) {
		new {2, 1, 0}.each do |i|
			tile_id = data[x, y, i];
			if (tile_id == 0) continue;
			terrain = GameData.TerrainTag.try_get(@terrain_tags[tile_id]);
			if (terrain.bridge && Game.GameData.PokemonGlobal.bridge > 0) return false;
			if (terrain.deep_bush && @passages[tile_id] & 0x40 == 0x40) return true;
		}
		return false;
	}

	public bool counter(x, y) {
		new {2, 1, 0}.each do |i|
			tile_id = data[x, y, i];
			if (tile_id == 0) continue;
			passage = @passages[tile_id];
			if (passage & 0x80 == 0x80) return true;
		}
		return false;
	}

	public void terrain_tag(x, y, countBridge = false) {
		if (valid(x, y)) {
			new {2, 1, 0}.each do |i|
				tile_id = data[x, y, i];
				if (tile_id == 0) continue;
				terrain = GameData.TerrainTag.try_get(@terrain_tags[tile_id]);
				if (terrain.id == :None || terrain.ignore_passability) continue;
				if (!countBridge && terrain.bridge && Game.GameData.PokemonGlobal.bridge == 0) continue;
				return terrain;
			}
		}
		return GameData.TerrainTag.get(:None);
	}

	// Unused.
	public void check_event(x, y) {
		foreach (var event in self.events) { //self.events.each_value do => |event|
			if (event.at_coordinate(x, y)) return event.id;
		}
	}

	public int display_x { set {
		if (@display_x == value) return;
		@display_x = value;
		if (metadata&.snap_edges) {
			max_x = (self.width - (Graphics.width.to_f / TILE_WIDTH)) * REAL_RES_X;
			@display_x = (int)Math.Max(0, (int)Math.Min(@display_x, max_x));
			}
	}
		Game.GameData.map_factory&.setMapsInRange;
	}

	public int display_y { set {
		if (@display_y == value) return;
		@display_y = value;
		if (metadata&.snap_edges) {
			max_y = (self.height - (Graphics.height.to_f / TILE_HEIGHT)) * REAL_RES_Y;
			@display_y = (int)Math.Max(0, (int)Math.Min(@display_y, max_y));
			}
	}
		Game.GameData.map_factory&.setMapsInRange;
	}

	public void scroll_up(distance) {
		self.display_y -= distance;
	}

	public void scroll_down(distance) {
		self.display_y += distance;
	}

	public void scroll_left(distance) {
		self.display_x -= distance;
	}

	public void scroll_right(distance) {
		self.display_x += distance;
	}

	// speed is:
	//   1: moves 1 tile in 1.6 seconds
	//   2: moves 1 tile in 0.8 seconds
	//   3: moves 1 tile in 0.4 seconds
	//   4: moves 1 tile in 0.2 seconds
	//   5: moves 1 tile in 0.1 seconds
	//   6: moves 1 tile in 0.05 seconds
	public void start_scroll(direction, distance, speed = 4) {
		if (direction <= 0 || direction == 5 || direction >= 10) return;
		if (new []{1, 3, 4, 6, 7, 9}.Contains(direction)) {   // horizontal
			@scroll_distance_x = distance;
			if (new []{1, 4, 7}.Contains(direction)) @scroll_distance_x *= -1;
		}
		if (new []{1, 2, 3, 7, 8, 9}.Contains(direction)) {   // vertical
			@scroll_distance_y = distance;
			if (new []{7, 8, 9}.Contains(direction)) @scroll_distance_y *= -1;
		}
		@scroll_speed = speed;
		@scroll_start_x = display_x;
		@scroll_start_y = display_y;
		@scroll_timer_start = System.uptime;
	}

	// The two distances can be positive or negative.
	public void start_scroll_custom(distance_x, distance_y, speed = 4) {
		if (distance_x == 0 && distance_y == 0) return;
		@scroll_distance_x = distance_x;
		@scroll_distance_y = distance_y;
		@scroll_speed = speed;
		@scroll_start_x = display_x;
		@scroll_start_y = display_y;
		@scroll_timer_start = System.uptime;
	}

	public bool scrolling() {
		return (@scroll_distance_x || 0) != 0 || (@scroll_distance_y || 0) != 0;
	}

	// duration is time in 1/20ths of a second.
	public void start_fog_tone_change(tone, duration) {
		if (duration == 0) {
			@fog_tone = tone.clone;
			return;
		}
		@fog_tone_initial = @fog_tone.clone;
		@fog_tone_target = tone.clone;
		@fog_tone_duration = duration / 20.0;
		@fog_tone_timer_start = Game.GameData.stats.play_time;
	}

	// duration is time in 1/20ths of a second.
	public void start_fog_opacity_change(opacity, duration) {
		if (duration == 0) {
			@fog_opacity = opacity.to_f;
			return;
		}
		@fog_opacity_initial = @fog_opacity;
		@fog_opacity_target = opacity.to_f;
		@fog_opacity_duration = duration / 20.0;
		@fog_opacity_timer_start = Game.GameData.stats.play_time;
	}

	public void set_tile(x, y, layer, id = 0) {
		self.data[x, y, layer] = id;
	}

	public void erase_tile(x, y, layer) {
		set_tile(x, y, layer, 0);
	}

	public void refresh() {
		@events.each_value do |event|
			event.refresh;
		}
		@common_events.each_value do |common_event|
			common_event.refresh;
		}
		@need_refresh = false;
	}

	public void update() {
		uptime_now = System.uptime;
		play_now = Game.GameData.stats.play_time;
		// Refresh maps if necessary
		if (Game.GameData.map_factory) {
			Game.GameData.map_factory.maps.each(i => { if (i.need_refresh) i.refresh; });
			Game.GameData.map_factory.setCurrentMap;
		}
		// If scrolling
		if ((@scroll_distance_x || 0) != 0) {
			duration = @scroll_distance_x.abs * TILE_WIDTH.to_f / (10 * (2**@scroll_speed));
			scroll_offset = lerp(0, @scroll_distance_x, duration, @scroll_timer_start, uptime_now);
			self.display_x = @scroll_start_x + (scroll_offset * REAL_RES_X);
			if (scroll_offset == @scroll_distance_x) @scroll_distance_x = 0;
		}
		if ((@scroll_distance_y || 0) != 0) {
			duration = @scroll_distance_y.abs * TILE_HEIGHT.to_f / (10 * (2**@scroll_speed));
			scroll_offset = lerp(0, @scroll_distance_y, duration, @scroll_timer_start, uptime_now);
			self.display_y = @scroll_start_y + (scroll_offset * REAL_RES_Y);
			if (scroll_offset == @scroll_distance_y) @scroll_distance_y = 0;
		}
		// Only update events that are on-screen
		if (!Game.GameData.game_temp.in_menu) {
			@events.each_value(event => event.update);
		}
		// Update common events
		@common_events.each_value(common_event => common_event.update);
		// Update fog
		if (!@fog_scroll_last_update_timer) @fog_scroll_last_update_timer = uptime_now;
		scroll_mult = (uptime_now - @fog_scroll_last_update_timer) * 5;
		@fog_ox -= @fog_sx * scroll_mult;
		@fog_oy -= @fog_sy * scroll_mult;
		@fog_scroll_last_update_timer = uptime_now;
		if (@fog_tone_timer_start) {
			@fog_tone.red = lerp(@fog_tone_initial.red, @fog_tone_target.red, @fog_tone_duration, @fog_tone_timer_start, play_now);
			@fog_tone.green = lerp(@fog_tone_initial.green, @fog_tone_target.green, @fog_tone_duration, @fog_tone_timer_start, play_now);
			@fog_tone.blue = lerp(@fog_tone_initial.blue, @fog_tone_target.blue, @fog_tone_duration, @fog_tone_timer_start, play_now);
			@fog_tone.gray = lerp(@fog_tone_initial.gray, @fog_tone_target.gray, @fog_tone_duration, @fog_tone_timer_start, play_now);
			if (play_now - @fog_tone_timer_start >= @fog_tone_duration) {
				@fog_tone_initial = null;
				@fog_tone_timer_start = null;
			}
		}
		if (@fog_opacity_timer_start) {
			@fog_opacity = lerp(@fog_opacity_initial, @fog_opacity_target, @fog_opacity_duration, @fog_opacity_timer_start, play_now);
			if (play_now - @fog_opacity_timer_start >= @fog_opacity_duration) {
				@fog_opacity_initial = null;
				@fog_opacity_timer_start = null;
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
// Scroll the map in the given direction by the given distance at the (optional)
// given speed.
public void ScrollMap(direction, distance, speed = 4) {
	if (speed == 0) {
		if (new []{1, 2, 3}.Contains(direction)) {
			Game.GameData.game_map.scroll_down(distance * Game_Map.REAL_RES_Y);
		} else if (new []{7, 8, 9}.Contains(direction)) {
			Game.GameData.game_map.scroll_up(distance * Game_Map.REAL_RES_Y);
		}
		if (new []{3, 6, 9}.Contains(direction)) {
			Game.GameData.game_map.scroll_right(distance * Game_Map.REAL_RES_X);
		} else if (new []{1, 4, 7}.Contains(direction)) {
			Game.GameData.game_map.scroll_left(distance * Game_Map.REAL_RES_X);
		}
	} else {
		Game.GameData.game_map.start_scroll(direction, distance, speed);
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			UpdateSceneMap;
			if (!Game.GameData.game_map.scrolling()) break;
		}
	}
}

// Scroll the map to center on the given coordinates at the (optional) given
// speed. The scroll can happen in up to two parts, depending on where the target
// is relative to the current location: an initial diagonal movement and a
// following cardinal (vertical/horizontal) movement.
public void ScrollMapTo(x, y, speed = 4) {
	if (!Game.GameData.game_map.valid(x, y)) {
		print "ScrollMapTo: given x,y is invalid";
		return;
	} else if (!(0..6).Contains(speed)) {
		print "ScrollMapTo: invalid speed (0-6 only)";
		return;
	}
	// Get tile coordinates that the screen is currently scrolled to
	screen_offset_x = (Graphics.width - Game_Map.TILE_WIDTH) * Game_Map.X_SUBPIXELS / 2;
	screen_offset_y = (Graphics.height - Game_Map.TILE_HEIGHT) * Game_Map.Y_SUBPIXELS / 2;
	current_tile_x = (Game.GameData.game_map.display_x + screen_offset_x) / Game_Map.REAL_RES_X;
	current_tile_y = (Game.GameData.game_map.display_y + screen_offset_y) / Game_Map.REAL_RES_Y;
	offset_x = x - current_tile_x;
	offset_y = y - current_tile_y;
	if (offset_x == 0 && offset_y == 0) return;
	if (speed == 0) {
		if (offset_y > 0) {
			Game.GameData.game_map.scroll_down(offset_y.abs * Game_Map.REAL_RES_Y);
		} else if (offset_y < 0) {
			Game.GameData.game_map.scroll_up(offset_y.abs * Game_Map.REAL_RES_Y);
		}
		if (offset_x > 0) {
			Game.GameData.game_map.scroll_right(offset_x.abs * Game_Map.REAL_RES_X);
		} else if (offset_x < 0) {
			Game.GameData.game_map.scroll_left(offset_x.abs * Game_Map.REAL_RES_X);
		}
	} else {
		Game.GameData.game_map.start_scroll_custom(offset_x, offset_y, speed);
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			UpdateSceneMap;
			if (!Game.GameData.game_map.scrolling()) break;
		}
	}
}

// Scroll the map to center on the player at the (optional) given speed.
public void ScrollMapToPlayer(speed = 4) {
	ScrollMapTo(Game.GameData.game_player.x, Game.GameData.game_player.y, speed);
}
