//===============================================================================
//
//===============================================================================
public partial class TilemapRenderer {
	public int tilesets		{ get { return _tilesets; } }			protected int _tilesets;
	public int autotiles		{ get { return _autotiles; } }			protected int _autotiles;
	public int tone		{ get { return _tone; } set { _tone = value; } }			protected int _tone;
	public int color		{ get { return _color; } set { _color = value; } }			protected int _color;
	public int viewport		{ get { return _viewport; } }			protected int _viewport;
	/// <summary>Does nothing</summary>
	public int ox		{ get { return _ox; } set { _ox = value; } }			protected int _ox;
	/// <summary>Does nothing</summary>
	public int oy		{ get { return _oy; } set { _oy = value; } }			protected int _oy;
	/// <summary>Does nothing</summary>
	public int visible		{ get { return _visible; } set { _visible = value; } }			protected int _visible;

	DISPLAY_TILE_WIDTH      = Game_Map.TILE_WIDTH rescue 32;
	DISPLAY_TILE_HEIGHT     = Game_Map.TILE_HEIGHT rescue 32;
	public const int SOURCE_TILE_WIDTH       = 32;
	public const int SOURCE_TILE_HEIGHT      = 32;
	public const int ZOOM_X                  = DISPLAY_TILE_WIDTH / SOURCE_TILE_WIDTH;
	public const int ZOOM_Y                  = DISPLAY_TILE_HEIGHT / SOURCE_TILE_HEIGHT;
	public const int TILESET_TILES_PER_ROW   = 8;
	public const int AUTOTILES_COUNT         = 8;   // Counting the blank tile as an autotile
	public const int TILES_PER_AUTOTILE      = 48;
	public const int TILESET_START_ID        = AUTOTILES_COUNT * TILES_PER_AUTOTILE;
	// If an autotile's filename ends with "[x]", its frame duration will be x/20
	// seconds instead.
	public const int AUTOTILE_FRAME_DURATION = 5;   // In 1/20ths of a second

	// Filenames of extra autotiles for each tileset. Each tileset's entry is an
	// array containing two other arrays (you can leave either of those empty, but
	// they must be defined):
	//   - The first sub-array is for large autotiles, i.e. ones with 48 different
	//     tile layouts. For example, "Brick path" and "Sea".
	//   - The second is for single tile autotiles. For example, "Flowers1" and
	//     "Waterfall"
	// The top tiles of the tileset will instead use these autotiles. Large
	// autotiles come first, in the same 8x6 layout as you see when you double-
	// click on a real autotile in RMXP. After that are the single tile autotiles.
	// Extra autotiles are only useful if the tiles are animated, because otherwise
	// you just have some tiles which belong in the tileset instead.
	EXTRA_AUTOTILES = {
//   Examples:
//    1 => new {["Sand shore"], ["Flowers2"]},
//    2 => new {[], new {"Flowers2", "Waterfall", "Waterfall crest", "Waterfall bottom"}},
//    6 => new {new {"Water rock", "Sea deep"}, new List<string>()}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class TilesetBitmaps {
		public int changed		{ get { return _changed; } set { _changed = value; } }			protected int _changed;
		public int bitmaps		{ get { return _bitmaps; } set { _bitmaps = value; } }			protected int _bitmaps;

		public void initialize() {
			@bitmaps      = new List<string>();
			@bitmap_wraps = new List<string>();   // Whether each tileset is a mega texture and has multiple columns
			@load_counts  = new List<string>();
			@bridge       = 0;
			@changed      = true;
		}

		public int this[int filename] { get { 
			return @bitmaps[filename];
			}
		}

		public int this[(filename, bitmap)] { get {
			if (nil_or_empty(filename)) return;
			@bitmaps[filename] = bitmap;
			@bitmap_wraps[filename] = false;
			@changed = true;
			}
		}

		public void add(filename) {
			if (nil_or_empty(filename)) return;
			if (@bitmaps[filename]) {
				@load_counts[filename] += 1;
				return;
			}
			bitmap = GetTileset(filename);
			@bitmap_wraps[filename] = false;
			if (bitmap.mega()) {
				self[filename] = TilemapRenderer.TilesetWrapper.wrapTileset(bitmap);
				@bitmap_wraps[filename] = true;
				bitmap.dispose;
			} else {
				self[filename] = bitmap;
			}
			@load_counts[filename] = 1;
		}

		public void remove(filename) {
			if (nil_or_empty(filename) || !@bitmaps[filename]) return;
			if (@load_counts[filename] > 1) {
				@load_counts[filename] -= 1;
				return;
			}
			@bitmaps[filename].dispose;
			@bitmaps.delete(filename);
			@bitmap_wraps.delete(filename);
			@load_counts.delete(filename);
		}

		public void set_src_rect(tile, tile_id) {
			if (nil_or_empty(tile.filename)) return;
			if (!@bitmaps[tile.filename]) return;
			tile.src_rect.x = ((tile_id - TILESET_START_ID) % TILESET_TILES_PER_ROW) * SOURCE_TILE_WIDTH;
			tile.src_rect.y = ((tile_id - TILESET_START_ID) / TILESET_TILES_PER_ROW) * SOURCE_TILE_HEIGHT;
			if (@bitmap_wraps[tile.filename]) {
				height = @bitmaps[tile.filename].height;
				col = (tile_id - TILESET_START_ID) * SOURCE_TILE_HEIGHT / (TILESET_TILES_PER_ROW * height);
				tile.src_rect.x += col * TILESET_TILES_PER_ROW * SOURCE_TILE_WIDTH;
				tile.src_rect.y -= col * height;
			}
		}

		public void update() { }
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class AutotileBitmaps : TilesetBitmaps {
		public int current_frames		{ get { return _current_frames; } }			protected int _current_frames;

		public override void initialize() {
			base.initialize();
			@frame_counts    = new List<string>();   // Number of frames in each autotile
			@frame_durations = new List<string>();   // How long each frame lasts per autotile
			@current_frames  = new List<string>();   // Which frame each autotile is currently showing
			@timer_start     = System.uptime;
		}

		public int this[(filename, value)] { get {
			super;
			if (nil_or_empty(filename)) return;
			frame_count(filename, true);
			set_current_frame(filename);
			}
		}

		public void add(filename) {
			if (nil_or_empty(filename)) return;
			if (@bitmaps[filename]) {
				@load_counts[filename] += 1;
				return;
			}
			orig_bitmap = GetAutotile(filename);
			@bitmap_wraps[filename] = false;
			duration = AUTOTILE_FRAME_DURATION;
			if (System.Text.RegularExpressions.Regex.IsMatch(filename,@"\[\s*(\d+?)\s*\]\s*$")) {
				duration = $~[1].ToInt();
			}
			@frame_durations[filename] = duration.to_f / 20;
			bitmap = AutotileExpander.expand(orig_bitmap);
			self[filename] = bitmap;
			if (bitmap.height > SOURCE_TILE_HEIGHT && bitmap.height < TILES_PER_AUTOTILE * SOURCE_TILE_HEIGHT) {
				@bitmap_wraps[filename] = true;
			}
			if (orig_bitmap != bitmap) orig_bitmap.dispose;
			@load_counts[filename] = 1;
		}

		public override void remove(filename) {
			base.remove();
			if (@load_counts[filename] && @load_counts[filename] > 0) return;
			@frame_counts.delete(filename);
			@current_frames.delete(filename);
			@frame_durations.delete(filename);
		}

		public void frame_count(filename, force_recalc = false) {
			if (!@frame_counts[filename] || force_recalc) {
				if (!@bitmaps[filename]) return 0;
				bitmap = @bitmaps[filename];
				@frame_counts[filename] = (int)Math.Max(bitmap.width / SOURCE_TILE_WIDTH, 1);
				if (bitmap.height > SOURCE_TILE_HEIGHT && @bitmap_wraps[filename]) {
					@frame_counts[filename] /= 2;
				}
			}
			return @frame_counts[filename];
		}

		public bool animated(filename) {
			return frame_count(filename) > 1;
		}

		public void current_frame(filename) {
			if (!@current_frames[filename]) set_current_frame(filename);
			return @current_frames[filename];
		}

		public void set_current_frame(filename) {
			frames = frame_count(filename);
			if (frames < 2) {
				@current_frames[filename] = 0;
			} else {
				@current_frames[filename] = (int)Math.Floor((System.uptime - @timer_start) / @frame_durations[filename]) % frames;
			}
		}

		public void set_src_rect(tile, tile_id) {
			if (nil_or_empty(tile.filename)) return;
			if (!@bitmaps[tile.filename]) return;
			frame = current_frame(tile.filename);
			if (@bitmaps[tile.filename].height == SOURCE_TILE_HEIGHT) {
				tile.src_rect.x = frame * SOURCE_TILE_WIDTH;
				tile.src_rect.y = 0;
				return;
			}
			wraps = @bitmap_wraps[tile.filename];
			high_id = ((tile_id % TILES_PER_AUTOTILE) >= TILES_PER_AUTOTILE / 2);
			tile.src_rect.x = 0;
			tile.src_rect.y = (tile_id % TILES_PER_AUTOTILE) * SOURCE_TILE_HEIGHT;
			if (wraps && high_id) {
				tile.src_rect.x = SOURCE_TILE_WIDTH;
				tile.src_rect.y -= SOURCE_TILE_HEIGHT * TILES_PER_AUTOTILE / 2;
			}
			tile.src_rect.x += frame * SOURCE_TILE_WIDTH * (wraps ? 2 : 1);
		}

		public override void update() {
			base.update();
			// Update the current frame for each autotile
			@bitmaps.each_key do |filename|
				if (!@bitmaps[filename] || @bitmaps[filename].disposed()) continue;
				old_frame = @current_frames[filename];
				set_current_frame(filename);
				if (@current_frames[filename] != old_frame) @changed = true;
			}
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class TileSprite : Sprite {
		public int filename		{ get { return _filename; } set { _filename = value; } }			protected int _filename;
		public int tile_id		{ get { return _tile_id; } set { _tile_id = value; } }			protected int _tile_id;
		public int is_autotile		{ get { return _is_autotile; } set { _is_autotile = value; } }			protected int _is_autotile;
		public int animated		{ get { return _animated; } set { _animated = value; } }			protected int _animated;
		public int priority		{ get { return _priority; } set { _priority = value; } }			protected int _priority;
		public int shows_reflection		{ get { return _shows_reflection; } set { _shows_reflection = value; } }			protected int _shows_reflection;
		public int bridge		{ get { return _bridge; } set { _bridge = value; } }			protected int _bridge;
		public int need_refresh		{ get { return _need_refresh; } set { _need_refresh = value; } }			protected int _need_refresh;

		public void set_bitmap(filename, tile_id, autotile, animated, priority, bitmap) {
			self.bitmap       = bitmap;
			self.src_rect     = new Rect(0, 0, SOURCE_TILE_WIDTH, SOURCE_TILE_HEIGHT);
			self.zoom_x       = ZOOM_X;
			self.zoom_y       = ZOOM_Y;
			@filename         = filename;
			@tile_id          = tile_id;
			@is_autotile      = autotile;
			@animated         = animated;
			@priority         = priority;
			@shows_reflection = false;
			@bridge           = false;
			self.visible      = !bitmap.null();
			@need_refresh     = true;
		}
	}

	//-----------------------------------------------------------------------------

	public void initialize(viewport) {
		@tilesets               = new TilesetBitmaps();
		@autotiles              = new AutotileBitmaps();
		@tiles_horizontal_count = (Graphics.width.to_f / DISPLAY_TILE_WIDTH).ceil + 1;
		@tiles_vertical_count   = (Graphics.height.to_f / DISPLAY_TILE_HEIGHT).ceil + 1;
		@tone                   = new Tone(0, 0, 0, 0);
		@old_tone               = new Tone(0, 0, 0, 0);
		@color                  = new Color(0, 0, 0, 0);
		@old_color              = new Color(0, 0, 0, 0);
		@self_viewport          = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport               = (viewport) ? viewport : @self_viewport;
		@old_viewport_ox        = 0;
		@old_viewport_oy        = 0;
		// NOTE: The extra tiles horizontally/vertically hang off the left and top
		//       edges of the screen, because the pixel_offset values are positive
		//       and are added to the tile sprite coordinates.
		@tiles                  = new List<string>();
		for (int i = @tiles_horizontal_count; i < @tiles_horizontal_count; i++) { //for '@tiles_horizontal_count' times do => |i|
			@tiles[i] = new List<string>();
			for (int j = @tiles_vertical_count; j < @tiles_vertical_count; j++) { //for '@tiles_vertical_count' times do => |j|
				@tiles[i][j] = new Array(3) { new TileSprite(@viewport) };
			}
		}
		@current_map_id         = 0;
		@tile_offset_x          = 0;
		@tile_offset_y          = 0;
		@pixel_offset_x         = 0;
		@pixel_offset_y         = 0;
		@ox                     = 0;
		@oy                     = 0;
		@visible                = true;
		@need_refresh           = true;
		@disposed               = false;
	}

	public void dispose() {
		if (disposed()) return;
		@tiles.each do |col|
			foreach (var coord in col) { //'col.each' do => |coord|
				coord.each(tile => tile.dispose);
				coord.clear;
			}
		}
		@tiles.clear;
		@tilesets.bitmaps.each_value(bitmap => bitmap.dispose);
		@tilesets.bitmaps.clear;
		@autotiles.bitmaps.each_value(bitmap => bitmap.dispose);
		@autotiles.bitmaps.clear;
		@self_viewport.dispose;
		@self_viewport = null;
		@disposed = true;
	}

	public bool disposed() {
		return @disposed;
	}

	//-----------------------------------------------------------------------------

	public void add_tileset(filename) {
		@tilesets.add(filename);
	}

	public void remove_tileset(filename) {
		@tilesets.remove(filename);
	}

	public void add_autotile(filename) {
		@autotiles.add(filename);
	}

	public void remove_autotile(filename) {
		@autotiles.remove(filename);
	}

	public void add_extra_autotiles(tileset_id) {
		if (!EXTRA_AUTOTILES[tileset_id]) return;
		foreach (var arr in EXTRA_AUTOTILES[tileset_id]) { //'EXTRA_AUTOTILES[tileset_id].each' do => |arr|
			arr.each(filename => add_autotile(filename));
		}
	}

	public void remove_extra_autotiles(tileset_id) {
		if (!EXTRA_AUTOTILES[tileset_id]) return;
		foreach (var arr in EXTRA_AUTOTILES[tileset_id]) { //'EXTRA_AUTOTILES[tileset_id].each' do => |arr|
			arr.each(filename => remove_autotile(filename));
		}
	}

	//-----------------------------------------------------------------------------

	public void refresh() {
		@need_refresh = true;
	}

	public void refresh_tile_bitmap(tile, map, tile_id) {
		tile.tile_id = tile_id;
		if (tile_id < TILES_PER_AUTOTILE) {
			tile.set_bitmap("", tile_id, false, false, 0, null);
			tile.shows_reflection = false;
			tile.bridge           = false;
		} else {
			terrain_tag = map.terrain_tags[tile_id] || 0;
			terrain_tag_data = GameData.TerrainTag.try_get(terrain_tag);
			priority = map.priorities[tile_id] || 0;
			single_autotile_start_id = TILESET_START_ID;
			true_tileset_start_id = TILESET_START_ID;
			extra_autotile_arrays = EXTRA_AUTOTILES[map.tileset_id];
			if (extra_autotile_arrays) {
				large_autotile_count = extra_autotile_arrays[0].length;
				single_autotile_count = extra_autotile_arrays[1].length;
				single_autotile_start_id += large_autotile_count * TILES_PER_AUTOTILE;
				true_tileset_start_id += large_autotile_count * TILES_PER_AUTOTILE;
				true_tileset_start_id += single_autotile_count;
			}
			if (tile_id < true_tileset_start_id) {
				filename = "";
				if (tile_id < TILESET_START_ID) {   // Real autotiles
					filename = map.autotile_names[(tile_id / TILES_PER_AUTOTILE) - 1];
				} else if (tile_id < single_autotile_start_id) {   // Large extra autotiles
					filename = extra_autotile_arrays[0][(tile_id - TILESET_START_ID) / TILES_PER_AUTOTILE];
				} else {   // Single extra autotiles
					filename = extra_autotile_arrays[1][tile_id - single_autotile_start_id];
				}
				tile.set_bitmap(filename, tile_id, true, @autotiles.animated(filename),
												priority, @autotiles[filename]);
			} else {
				filename = map.tileset_name;
				tile.set_bitmap(filename, tile_id, false, false, priority, @tilesets[filename]);
			}
			tile.shows_reflection = terrain_tag_data&.shows_reflections;
			tile.bridge           = terrain_tag_data&.bridge;
		}
		refresh_tile_src_rect(tile, tile_id);
	}

	public void refresh_tile_src_rect(tile, tile_id) {
		if (tile.is_autotile) {
			@autotiles.set_src_rect(tile, tile_id);
		} else {
			@tilesets.set_src_rect(tile, tile_id);
		}
	}

	// For animated autotiles only
	public void refresh_tile_frame(tile, tile_id) {
		if (!tile.animated) return;
		@autotiles.set_src_rect(tile, tile_id);
	}

	// x and y are the positions of tile within @tiles, not a map x/y
	public void refresh_tile_coordinates(tile, x, y) {
		tile.x = (x * DISPLAY_TILE_WIDTH) - @pixel_offset_x;
		tile.y = (y * DISPLAY_TILE_HEIGHT) - @pixel_offset_y;
	}

	public void refresh_tile_z(tile, map, y, layer, tile_id) {
		if (tile.shows_reflection) {
			tile.z = -2000;
		} else if (tile.bridge && Game.GameData.PokemonGlobal.bridge > 0) {
			tile.z = 0;
		} else {
			priority = tile.priority;
			tile.z = (priority == 0) ? 0 : (y * SOURCE_TILE_HEIGHT) + (priority * SOURCE_TILE_HEIGHT) + SOURCE_TILE_HEIGHT + 1;
		}
	}

	public void refresh_tile(tile, x, y, map, layer, tile_id) {
		refresh_tile_bitmap(tile, map, tile_id);
		refresh_tile_coordinates(tile, x, y);
		refresh_tile_z(tile, map, y, layer, tile_id);
		tile.need_refresh = false;
	}

	//-----------------------------------------------------------------------------

	public void check_if_screen_moved() {
		ret = false;
		// Check for map change
		if (@current_map_id != Game.GameData.game_map.map_id) {
			if (MapFactoryHelper.hasConnections(@current_map_id)) {
				offsets = Game.GameData.map_factory.getRelativePos(@current_map_id, 0, 0, Game.GameData.game_map.map_id, 0, 0);
				if (offsets) {
					@tile_offset_x -= offsets[0];
					@tile_offset_y -= offsets[1];
				} else {
					ret = true;   // Need a full refresh
				}
			} else {
				ret = true;
			}
			@current_map_id = Game.GameData.game_map.map_id;
		}
		// Check for tile movement
		current_map_display_x = (int)Math.Round(Game.GameData.game_map.display_x.to_f / Game_Map.X_SUBPIXELS);
		current_map_display_y = (int)Math.Round(Game.GameData.game_map.display_y.to_f / Game_Map.Y_SUBPIXELS);
		new_tile_offset_x = (current_map_display_x / SOURCE_TILE_WIDTH) * ZOOM_X;
		new_tile_offset_y = (current_map_display_y / SOURCE_TILE_HEIGHT) * ZOOM_Y;
		if (new_tile_offset_x != @tile_offset_x) {
			if (new_tile_offset_x > @tile_offset_x) {
				// Take tile stacks off the right and insert them at the beginning (left)
				(new_tile_offset_x - @tile_offset_x).times do
					c = @tiles.shift;
					@tiles.Add(c);
					foreach (var coord in c) { //'c.each' do => |coord|
						coord.each(tile => tile.need_refresh = true);
					}
				}
			} else {
				// Take tile stacks off the beginning (left) and push them onto the end (right)
				(@tile_offset_x - new_tile_offset_x).times do
					c = @tiles.pop;
					@tiles.prepend(c);
					foreach (var coord in c) { //'c.each' do => |coord|
						coord.each(tile => tile.need_refresh = true);
					}
				}
			}
			@screen_moved = true;
			@tile_offset_x = new_tile_offset_x;
		}
		if (new_tile_offset_y != @tile_offset_y) {
			if (new_tile_offset_y > @tile_offset_y) {
				// Take tile stacks off the bottom and insert them at the beginning (top)
				@tiles.each do |col|
					(new_tile_offset_y - @tile_offset_y).times do
						c = col.shift;
						col.Add(c);
						c.each(tile => tile.need_refresh = true);
					}
				}
			} else {
				// Take tile stacks off the beginning (top) and push them onto the end (bottom)
				@tiles.each do |col|
					(@tile_offset_y - new_tile_offset_y).times do
						c = col.pop;
						col.prepend(c);
						c.each(tile => tile.need_refresh = true);
					}
				}
			}
			@screen_moved = true;
			@screen_moved_vertically = true;
			@tile_offset_y = new_tile_offset_y;
		}
		// Check for pixel movement
		new_pixel_offset_x = (current_map_display_x % SOURCE_TILE_WIDTH) * ZOOM_X;
		new_pixel_offset_y = (current_map_display_y % SOURCE_TILE_HEIGHT) * ZOOM_Y;
		if (new_pixel_offset_x != @pixel_offset_x) {
			@screen_moved = true;
			@pixel_offset_x = new_pixel_offset_x;
		}
		if (new_pixel_offset_y != @pixel_offset_y) {
			@screen_moved = true;
			@screen_moved_vertically = true;
			@pixel_offset_y = new_pixel_offset_y;
		}
		return ret;
	}

	//-----------------------------------------------------------------------------

	public void update() {
		// Update tone
		if (@old_tone != @tone) {
			@tiles.each do |col|
				foreach (var coord in col) { //'col.each' do => |coord|
					coord.each(tile => tile.tone = @tone);
				}
			}
			@old_tone = @tone.clone;
		}
		// Update color
		if (@old_color != @color) {
			@tiles.each do |col|
				foreach (var coord in col) { //'col.each' do => |coord|
					coord.each(tile => tile.color = @color);
				}
			}
			@old_color = @color.clone;
		}
		// Recalculate autotile frames
		@tilesets.update;
		@autotiles.update;
		do_full_refresh = @need_refresh;
		if (@viewport.ox != @old_viewport_ox || @viewport.oy != @old_viewport_oy) {
			@old_viewport_ox = @viewport.ox;
			@old_viewport_oy = @viewport.oy;
			do_full_refresh = true;
		}
		// Check whether the screen has moved since the last update
		@screen_moved = false;
		@screen_moved_vertically = false;
		if (Game.GameData.PokemonGlobal.bridge != @bridge) {
			@bridge = Game.GameData.PokemonGlobal.bridge;
			@screen_moved_vertically = true;   // To update bridge tiles' z values
		}
		if (check_if_screen_moved) do_full_refresh = true;
		// Update all tile sprites
		visited = new List<string>();
		for (int i = @tiles_horizontal_count; i < @tiles_horizontal_count; i++) { //for '@tiles_horizontal_count' times do => |i|
			visited[i] = new List<string>();
			@tiles_vertical_count.times(j => visited[i][j] = false);
		}
		foreach (var map in Game.GameData.map_factory.maps) { //'Game.GameData.map_factory.maps.each' do => |map|
			// Calculate x/y ranges of tile sprites that represent them
			map_display_x = (int)Math.Round(map.display_x.to_f / Game_Map.X_SUBPIXELS);
			if (ZOOM_X != 1) map_display_x = ((map_display_x + (Graphics.width / 2)) * ZOOM_X) - (Graphics.width / 2);
			map_display_y = (int)Math.Round(map.display_y.to_f / Game_Map.Y_SUBPIXELS);
			if (ZOOM_Y != 1) map_display_y = ((map_display_y + (Graphics.height / 2)) * ZOOM_Y) - (Graphics.height / 2);
			map_display_x_tile = map_display_x / DISPLAY_TILE_WIDTH;
			map_display_y_tile = map_display_y / DISPLAY_TILE_HEIGHT;
			start_x = (int)Math.Max(-map_display_x_tile, 0);
			start_y = (int)Math.Max(-map_display_y_tile, 0);
			end_x = @tiles_horizontal_count - 1;
			end_x = (int)Math.Min(end_x, map.width - map_display_x_tile - 1);
			end_y = @tiles_vertical_count - 1;
			end_y = (int)Math.Min(end_y, map.height - map_display_y_tile - 1);
			if (start_x > end_x || start_y > end_y || end_x < 0 || end_y < 0) continue;
			// Update all tile sprites representing this map
			(start_x..end_x).each do |i|
				tile_x = i + map_display_x_tile;
				(start_y..end_y).each do |j|
					tile_y = j + map_display_y_tile;
					@tiles[i][j].each_with_index do |tile, layer|
						tile_id = map.data[tile_x, tile_y, layer];
						if (do_full_refresh || tile.need_refresh || tile.tile_id != tile_id) {
							refresh_tile(tile, i, j, map, layer, tile_id);
						} else {
							if (tile.animated && @autotiles.changed) refresh_tile_frame(tile, tile_id);
							// Update tile's x/y coordinates
							if (@screen_moved) refresh_tile_coordinates(tile, i, j);
							// Update tile's z value
							if (@screen_moved_vertically) refresh_tile_z(tile, map, j, layer, tile_id);
						}
					}
					// Record x/y as visited
					visited[i][j] = true;
				}
			}
		}
		// Clear all unvisited tile sprites
		@tiles.each_with_index do |col, i|
			col.each_with_index do |coord, j|
				if (visited[i][j]) continue;
				foreach (var tile in coord) { //'coord.each' do => |tile|
					tile.set_bitmap("", 0, false, false, 0, null);
					tile.shows_reflection = false;
					tile.bridge           = false;
				}
			}
		}
		@need_refresh = false;
		@autotiles.changed = false;
	}
}
