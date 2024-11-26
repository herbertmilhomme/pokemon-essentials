//===============================================================================
// Code that generates a random dungeon layout, and implements it in a given map.
//===============================================================================
public static partial class RandomDungeon {
	//=============================================================================
	// Bitwise values used to keep track of the generation of node connections.
	//=============================================================================
	public static partial class EdgeMasks {
		public const int NORTH = 1;
		public const int EAST  = 2;
		public const int SOUTH = 4;
		public const int WEST  = 8;
	}

	//=============================================================================
	// A node in a randomly generated dungeon. There is one node per cell, and
	// nodes are connected to each other.
	//=============================================================================
	public partial class MazeNode {
		public void initialize() {
			@visitable = false;
			@visited   = false;
			@room      = false;
			block_all_edges;   // A bit being 1 means its edge is NOT connected to the adjacent node
		}

		public int edge_pattern       { get { return @edges;            } }
		public void block_edge(e)      {@edges |= e;              }
		public void connect_edge(e)    {@edges &= ~e;             }
		public int block_all_edges    { get { return @edges = 15;              } }
		public int connect_all_edges  { get { return @edges = 0;               } }
		public bool edge_blocked(e) {    return (@edges & e) != 0; }
		public bool all_edges_blocked() { return @edges != 0;       }
		public bool visitable() {         return @visitable;        }
		public int set_visitable      { get { return @visitable = true;        } }
		public bool visited() {           return @visited;          }
		public int set_visited        { get { return @visited = true;          } }
		public bool room() {              return @room;             }
		public int set_room           { get { return @room = true;             } }
	}

	//=============================================================================
	// Maze generator. Given the number of nodes horizontally and vertically in a
	// map, connects all the nodes together.
	//=============================================================================
	public partial class Maze {
		public int node_count_x		{ get { return _node_count_x; } set { _node_count_x = value; } }			protected int _node_count_x;
		public int node_count_y		{ get { return _node_count_y; } set { _node_count_y = value; } }			protected int _node_count_y;

		DIRECTIONS = new {EdgeMasks.NORTH, EdgeMasks.SOUTH, EdgeMasks.EAST, EdgeMasks.WEST};

		public void initialize(cw, ch, parameters) {
			if (cw == 0 || ch == 0) raise new ArgumentError();
			@node_count_x = cw;
			@node_count_y = ch;
			@parameters = parameters;
			@nodes = new Array(@node_count_x * @node_count_y) { new MazeNode() };
		}

		public bool valid_node(x, y) {
			return x >= 0 && x < @node_count_x && y >= 0 && y < @node_count_y;
		}

		public void get_node(x, y) {
			if (valid_node(x, y)) return @nodes[(y * @node_count_x) + x];
			return null;
		}

		public bool node_visited(x, y) {
			if (!valid_node(x, y) || !@nodes[(y * @node_count_x) + x].visitable()) return true;
			return @nodes[(y * @node_count_x) + x].visited();
		}

		public void set_node_visited(x, y) {
			if (valid_node(x, y)) @nodes[(y * @node_count_x) + x].set_visited;
		}

		public bool node_edge_blocked(x, y, edge) {
			if (!valid_node(x, y)) return false;
			return @nodes[(y * @node_count_x) + x].edge_blocked(edge);
		}

		public void connect_node_edges(x, y, edge) {
			if (!valid_node(x, y)) return;
			@nodes[(y * @node_count_x) + x].connect_edge(edge);
			new_x, new_y, new_edge = get_coords_in_direction(x, y, edge, true);
			if (new_edge == 0) raise new ArgumentError();
			if (valid_node(new_x, new_y)) @nodes[(new_y * @node_count_x) + new_x].connect_edge(new_edge);
		}

		public void room_count() {
			ret = 0;
			@nodes.each(node => { if (node.room()) ret += 1; });
			return ret;
		}

		public void get_coords_in_direction(x, y, dir, include_direction = false) {
			new_x = x;
			new_y = y;
			new_dir = 0;
			switch (dir) {
				case EdgeMasks.NORTH:
					new_dir = EdgeMasks.SOUTH;
					new_y -= 1;
					break;
				case EdgeMasks.SOUTH:
					new_dir = EdgeMasks.NORTH;
					new_y += 1;
					break;
				case EdgeMasks.WEST:
					new_dir = EdgeMasks.EAST;
					new_x -= 1;
					break;
				case EdgeMasks.EAST:
					new_dir = EdgeMasks.WEST;
					new_x += 1;
					break;
			}
			if (include_direction) return new_x, new_y, new_dir;
			return new_x, new_y;
		}

		//---------------------------------------------------------------------------

		public void generate_layout() {
			// Set visitable nodes
			visitable_nodes = set_visitable_nodes;
			// Generate connections between all nodes
			generate_depth_first_maze(visitable_nodes);
			add_more_connections;
			// Spawn rooms in some nodes
			spawn_rooms(visitable_nodes);
		}

		// Returns whether the node at (x, y) is active in the given layout.
		public void check_active_node(x, y, layout) {
			switch (layout) {
				case :no_corners:
					if (new []{0, @node_count_x - 1}.Contains(x) && new []{0, @node_count_y - 1}.Contains(y)) return false;
					break;
				case :ring:
					if (x > 0 && x < @node_count_x - 1 && y > 0 && y < @node_count_y - 1) return false;
					break;
				case :antiring:
					if (x == 0 || x == @node_count_x - 1 || y == 0 || y == @node_count_y - 1) return false;
					break;
				case :plus:
					if (x != @node_count_x / 2 && y != @node_count_y / 2) return false;
					break;
				case :diagonal_up:
					if ((x + y - @node_count_y + 1).abs >= 2) return false;
					break;
				case :diagonal_down:
					if ((x - y).abs >= 2) return false;
					break;
				case :cross:
					if ((x - y).abs >= 2 && (x + y - @node_count_y + 1).abs >= 2) return false;
					break;
				case :quadrants:
					if ((x == 0 || x == @node_count_x - 1) && y >= 2 && y < @node_count_y - 2) return false;
					if ((y == 0 || y == @node_count_y - 1) && x >= 2 && x < @node_count_x - 2) return false;
					break;
			}
			return true;
		}

		public void set_visitable_nodes() {
			visitable_nodes = new List<string>();
			for (int y = @node_count_y; y < @node_count_y; y++) { //for '@node_count_y' times do => |y|
				for (int x = @node_count_x; x < @node_count_x; x++) { //for '@node_count_x' times do => |x|
					if (!check_active_node(x, y, @parameters.node_layout)) continue;
					@nodes[(y * @node_count_x) + x].set_visitable;
					visitable_nodes.Add(new {x, y});
				}
			}
			return visitable_nodes;
		}

		public void generate_depth_first_maze(visitable_nodes) {
			// Pick a cell to start in
			start = visitable_nodes.sample;
			sx = start[0];
			sy = start[1];
			// Generate a maze
			connect_nodes_and_recurse_depth_first(sx, sy, 0);
		}

		public void connect_nodes_and_recurse_depth_first(x, y, depth) {
			set_node_visited(x, y);
			dirs = DIRECTIONS.shuffle;
			for (int c = 4; c < 4; c++) { //for '4' times do => |c|
				dir = dirs[c];
				cx, cy = get_coords_in_direction(x, y, dir);
				if (node_visited(cx, cy)) continue;
				connect_node_edges(x, y, dir);
				connect_nodes_and_recurse_depth_first(cx, cy, depth + 1);
			}
		}

		public void add_more_connections() {
			if (@parameters.extra_connections_count == 0) return;
			possible_conns = new List<string>();
			for (int x = @node_count_x; x < @node_count_x; x++) { //for '@node_count_x' times do => |x|
				for (int y = @node_count_y; y < @node_count_y; y++) { //for '@node_count_y' times do => |y|
					node = @nodes[(y * @node_count_x) + x];
					if (!node.visitable()) continue;
					foreach (var dir in DIRECTIONS) { //'DIRECTIONS.each' do => |dir|
						if (!node.edge_blocked(dir)) continue;
						cx, cy, cdir = get_coords_in_direction(x, y, dir, true);
						new_node = get_node(cx, cy);
						if (!new_node || !new_node.visitable() || !new_node.edge_blocked(cdir)) continue;
						possible_conns.Add(new {x, y, dir});
					}
				}
			}
			foreach (var conn in possible_conns.sample(@parameters.extra_connections_count)) { //'possible_conns.sample(@parameters.extra_connections_count).each' do => |conn|
				connect_node_edges(*conn);
			}
		}

		public void spawn_rooms(visitable_nodes) {
			roomable_nodes = new List<string>();
			visitable_nodes.each(coord => { if (check_active_node(*coord, @parameters.room_layout)) roomable_nodes.Add(coord); });
			room_count = (int)Math.Max(roomable_nodes.length * @parameters.room_chance / 100, 1);
			if (room_count == 0) return;
			rooms = roomable_nodes.sample(room_count);
			rooms.each(coords => @nodes[(coords[1] * @node_count_x) + coords[0]].set_room);
		}
	}

	//=============================================================================
	// Arrays of tile types in the dungeon map.
	//=============================================================================
	public partial class DungeonLayout {
		public int width		{ get { return _width; } set { _width = value; } }			protected int _width;
		public int height		{ get { return _height; } set { _height = value; } }			protected int _height;
		alias xsize width;
		alias ysize height;

		// Used for debugging when printing out an ASCII image of the dungeon
		TEXT_SYMBOLS = {
			void                   = "#",
			room                   = " ",
			corridor               = " ",
			void_decoration        = "#",
			void_decoration_large  = "#",
			floor_decoration       = " ",
			floor_decoration_large = " ",
			floor_patch            = " ",
			wall_top               = " ",
			wall_1                 = Console.markup_style("=", bg: :brown),
			wall_2                 = Console.markup_style("=", bg: :brown),
			wall_3                 = Console.markup_style("=", bg: :brown),
			wall_4                 = Console.markup_style("=", bg: :brown),
			wall_6                 = Console.markup_style("=", bg: :brown),
			wall_7                 = Console.markup_style("=", bg: :brown),
			wall_8                 = Console.markup_style("=", bg: :brown),
			wall_9                 = Console.markup_style("=", bg: :brown),
			wall_in_1              = Console.markup_style("=", bg: :brown),
			wall_in_3              = Console.markup_style("=", bg: :brown),
			wall_in_7              = Console.markup_style("=", bg: :brown),
			wall_in_9              = Console.markup_style("=", bg: :brown),
			upper_wall_1           = Console.markup_style("~", bg: :gray),
			upper_wall_2           = Console.markup_style("~", bg: :gray),
			upper_wall_3           = Console.markup_style("~", bg: :gray),
			upper_wall_4           = Console.markup_style("~", bg: :gray),
			upper_wall_6           = Console.markup_style("~", bg: :gray),
			upper_wall_7           = Console.markup_style("~", bg: :gray),
			upper_wall_8           = Console.markup_style("~", bg: :gray),
			upper_wall_9           = Console.markup_style("~", bg: :gray),
			upper_wall_in_1        = Console.markup_style("~", bg: :gray),
			upper_wall_in_3        = Console.markup_style("~", bg: :gray),
			upper_wall_in_7        = Console.markup_style("~", bg: :gray),
			upper_wall_in_9        = Console.markup_style("~", bg: :gray);
		}

		public void initialize(width, height) {
			@width  = width;
			@height = height;
			@array  = new {[], new List<string>(), new List<string>()};
			clear;
		}

		public void [](x, y, layer() {
			return @array[layer][(y * @width) + x];
		}

		public int this[(x, y, layer, value)] { get {
			@array[layer][(y * @width) + x] = value;
			}
		}

		public void value(x, y) {
			if (x < 0 || x >= @width || y < 0 || y >= @height) return :void;
			ret = :void;
			new {2, 1, 0}.each do |layer|
				if (@array[layer][(y * @width) + x] != :none) return @array[layer][(y * @width) + x];
			}
			return ret;
		}

		public void clear() {
			@array.each_with_index do |arr, layer|
				(@width * @height).times(i => arr[i] = (layer == 0) ? :void : :none)
			}
		}

		public void set_wall(x, y, value) {
			@array[0][(y * @width) + x] = :room;
			@array[1][(y * @width) + x] = value;
		}

		public void set_ground(x, y, value) {
			@array[0][(y * @width) + x] = value;
			@array[1][(y * @width) + x] = :none;
		}

		public void write() {
			ret = "";
			for (int y = @height; y < @height; y++) { //for '@height' times do => |y|
				for (int x = @width; x < @width; x++) { //for '@width' times do => |x|
					ret += TEXT_SYMBOLS[value(x, y)] || "\e[30m\e[41m?\e[0m";
				}
				ret += "\n";
			}
			return ret;
		}
	}

	//=============================================================================
	// The main dungeon generator class.
	//=============================================================================
	public partial class Dungeon {
		public int width		{ get { return _width; } set { _width = value; } }			protected int _width;
		public int height		{ get { return _height; } set { _height = value; } }			protected int _height;
		alias xsize width;
		alias ysize height;
		public int parameters		{ get { return _parameters; } set { _parameters = value; } }			protected int _parameters;
		public int rng_seed		{ get { return _rng_seed; } set { _rng_seed = value; } }			protected int _rng_seed;
		public int tileset		{ get { return _tileset; } set { _tileset = value; } }			protected int _tileset;

		// 0 is none (index 0 only) or corridor/floor
		// -1 are tile combinations that need special attention
		// Other numbers correspond to tile types (see def get_wall_tile_for_coord)
		FLOOR_NEIGHBOURS_TO_WALL = new {
			0, 2, 1, 2, 4, 11, 4, 11, 7, 9, 4, 11, 4, 11, 4, 11,
			8, 0, 17, 0, 17, 0, 17, 0, 8, 0, 17, 0, 17, 0, 17, 0,
			9, 13, -1, 13, 17, 0, 17, 0, 8, 0, 17, 0, 17, 0, 17, 0,
			8, 0, 17, 0, 17, 0, 17, 0, 8, 0, 17, 0, 17, 0, 17, 0,
			6, 13, 13, 13, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0,
			19, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0,
			6, 13, 13, 13, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0,
			19, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0,
			3, 2, 2, 2, 11, 11, 11, 11, -1, 11, 11, 11, 11, 11, 11, 11,
			19, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0,
			6, 13, 13, 13, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0,
			19, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0,
			6, 13, 13, 13, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0,
			19, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0,
			6, 13, 13, 13, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0,
			19, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0;
		}

		public void initialize(width, height, tileset, parameters = null) {
			@tileset     = tileset;
			@buffer_x    = ((Graphics.width.to_f / Game_Map.TILE_WIDTH) / 2).ceil;
			@buffer_y    = ((Graphics.height.to_f / Game_Map.TILE_HEIGHT) / 2).ceil;
			if (@tileset.snap_to_large_grid) {
				if (@buffer_x.odd()) @buffer_x += 1;
				if (@buffer_x.odd()) @buffer_y += 1;
			}
			@parameters = parameters || new GameData.DungeonParameters(new List<string>());
			if (@tileset.snap_to_large_grid) {
				if (@parameters.cell_width.odd()) @parameters.cell_width -= 1;
				if (@parameters.cell_height.odd()) @parameters.cell_height -= 1;
				if (@parameters.corridor_width.odd()) @parameters.corridor_width += (@parameters.corridor_width == 1) ? 1 : -1;
			}
			if (width >= 20) {
				@width = width;
			} else {
				@width = (width * @parameters.cell_width) + (2 * @buffer_x);
				if (@tileset.snap_to_large_grid && @width.odd()) @width += 1;
			}
			if (height >= 20) {
				@height = height;
			} else {
				@height = (height * @parameters.cell_height) + (2 * @buffer_y);
				if (@tileset.snap_to_large_grid && @height.odd()) @height += 1;
			}
			@usable_width = @width;
			@usable_height = @height;
			if (@tileset.snap_to_large_grid) {
				if (@usable_width.odd()) @usable_width  -= 1;
				if (@usable_height.odd()) @usable_height -= 1;
			}
			@room_rects = new List<string>();
			@map_data    = new DungeonLayout(@width, @height);
			@need_redraw = false;
		}

		public void [](x, y, layer = null() {
			if (layer.null()) return @map_data.value(x, y);
			return @map_data[x, y, layer];
		}

		public int this[(x, y, layer, value)] { get {
			@map_data[x, y, layer] = value;
			}
		}

		public void write() {
			return @map_data.write;
		}

		//---------------------------------------------------------------------------

		// Returns whether the given coordinates are a room floor that isn't too
		// close to a corridor. For positioning events/the player upon entering.
		public bool isRoom(x, y) {
			if (@map_data.value(x, y) != :room) return false;
			(-1..1).each do |i|
				(-1..1).each do |j|
					if (i == 0 && j == 0) continue;
					if (@map_data.value(x + i, y + j) == :corridor) return false;
				}
			}
			return true;   // No surrounding tiles are corridor floor
		}

		public bool tile_is_ground(value) {
			return new []{:room, :corridor}.Contains(value);
		}

		// Lower wall tiles only.
		public bool tile_is_wall(value) {
			return new []{:wall_1, :wall_2, :wall_3, :wall_4, :wall_6, :wall_7, :wall_8, :wall_9,
							:wall_in_1, :wall_in_3, :wall_in_7, :wall_in_9}.Contains(value);
		}

		public bool coord_is_ground(x, y) {
			return tile_is_ground(@map_data[x, y, 0]) && !tile_is_wall(@map_data[x, y, 1]);
		}

		//---------------------------------------------------------------------------

		public void generate() {
			@rng_seed = @parameters.rng_seed || Game.GameData.PokemonGlobal.dungeon_rng_seed || Random.new_seed;
			Game.GameData.PokemonGlobal.dungeon_rng_seed = null;
			Random.srand(@rng_seed);
			maxWidth = @usable_width - (@buffer_x * 2);
			maxHeight = @usable_height - (@buffer_y * 2);
			if (maxWidth < 0 || maxHeight < 0) return;
			do { //loop; while (true);
				@need_redraw = false;
				@map_data.clear;
				// Generate the basic layout of the map
				generate_layout(maxWidth, maxHeight);
				if (@need_redraw) continue;
				// Draw walls
				generate_walls(maxWidth, maxHeight);
				if (@need_redraw) continue;
				// Draw decorations
				paint_decorations(maxWidth, maxHeight);
				// Draw wall top tiles
				paint_wall_top_tiles(maxWidth, maxHeight);
				break;   // if !@need_redraw
			}
		}

		public void generate_layout(maxWidth, maxHeight) {
			cellWidth = @parameters.cell_width;
			cellHeight = @parameters.cell_height;
			// Map is too small, make the whole map a room
			if (maxWidth < cellWidth || maxHeight < cellHeight) {
				paint_ground_rect(@buffer_x, @buffer_y, maxWidth, maxHeight, :room);
				return;
			}
			// Generate connections between cells
			maze = new Maze(maxWidth / cellWidth, maxHeight / cellHeight, @parameters);
			maze.generate_layout;
			// If no rooms were generated, make the whole map a room
			if (maze.room_count == 0) {
				paint_ground_rect(@buffer_x, @buffer_y, maxWidth, maxHeight, :room);
				return;
			}
			// Draw each cell's contents in turn (room and corridors)
			for (int y = (maxHeight / cellHeight); y < (maxHeight / cellHeight); y++) { //for '(maxHeight / cellHeight)' times do => |y|
				for (int x = (maxWidth / cellWidth); x < (maxWidth / cellWidth); x++) { //for '(maxWidth / cellWidth)' times do => |x|
					paint_node_contents(@buffer_x + (x * cellWidth), @buffer_y + (y * cellHeight), maze.get_node(x, y));
				}
			}
			check_for_isolated_rooms;
		}

		public void generate_walls(maxWidth, maxHeight) {
			// Lower layer
			errors = new List<string>();
			for (int y = maxHeight; y < maxHeight; y++) { //for 'maxHeight' times do => |y|
				for (int x = maxWidth; x < maxWidth; x++) { //for 'maxWidth' times do => |x|
					if (!coord_is_ground(@buffer_x + x, @buffer_y + y)) continue;
					paint_walls_around_ground(@buffer_x + x, @buffer_y + y, 0, errors);
				}
			}
			// Check for error tiles
			foreach (var coord in errors) { //'errors.each' do => |coord|
				resolve_wall_error(coord[0], coord[1], 0);
				if (@need_redraw) break;
			}
			if (@need_redraw) return;
			if (!@tileset.double_walls) return;
			// Upper layer
			errors = new List<string>();
			for (int y = (maxHeight + 2); y < (maxHeight + 2); y++) { //for '(maxHeight + 2)' times do => |y|
				for (int x = (maxWidth + 2); x < (maxWidth + 2); x++) { //for '(maxWidth + 2)' times do => |x|
					if (!tile_is_wall(@map_data[@buffer_x + x - 1, @buffer_y + y - 1, 1])) continue;
					paint_walls_around_ground(@buffer_x + x - 1, @buffer_y + y - 1, 1, errors);
				}
			}
			// Check for error tiles
			foreach (var coord in errors) { //'errors.each' do => |coord|
				resolve_wall_error(coord[0], coord[1], 1);
				if (@need_redraw) break;
			}
		}

		//---------------------------------------------------------------------------

		// Determines whether all floor tiles are contiguous. Sets @need_redraw if
		// there are 2+ floor regions that are isolated from each other.
		public void check_for_isolated_rooms() {
			// Get a floor tile as a starting position
			start = null;
			maxWidth = @usable_width - (@buffer_x * 2);
			maxHeight = @usable_height - (@buffer_y * 2);
			for (int y = maxHeight; y < maxHeight; y++) { //for 'maxHeight' times do => |y|
				for (int x = maxWidth; x < maxWidth; x++) { //for 'maxWidth' times do => |x|
					if (!tile_is_ground(@map_data[x + @buffer_x, y + @buffer_y, 0])) continue;
					start = new {x, y};
					break;
				}
				if (start) break;
			}
			if (!start) {
				@need_redraw = true;
				return;
			}
			// Flood fill (https://en.wikipedia.org/wiki/Flood_fill#Span_Filling)
			to_check = new {
				new {start[0], start[0], start[1], 1},
				new {start[0], start[0], start[1] - 1, -1}
			}
			visited = new List<string>();
			do { //loop; while (true);
				if (to_check.empty()) break;
				checking = to_check.shift;
				x1, x2, y, dy = checking;
				x = x1;
				if (!visited[(y * maxWidth) + x] && tile_is_ground(@map_data[x + @buffer_x, y + @buffer_y, 0])) {
					do { //loop; while (true);
						if (visited[(y * maxWidth) + x - 1] || !tile_is_ground(@map_data[x - 1 + @buffer_x, y + @buffer_y, 0])) break;
						visited[(y * maxWidth) + x - 1] = true;
						x -= 1;
					}
				}
				if (x < x1) to_check.Add(new {x, x1 - 1, y - dy, -dy});
				do { //loop; while (true);
					if (x1 > x2) break;
					do { //loop; while (true);
						if (visited[(y * maxWidth) + x1] || !tile_is_ground(@map_data[x1 + @buffer_x, y + @buffer_y, 0])) break;
						visited[(y * maxWidth) + x1] = true;
						to_check.Add(new {x, x1, y + dy, dy});
						if (x1 > x2) to_check.Add(new {x2 + 1, x1, y - dy, -dy});
						x1 += 1;
					}
					x1 += 1;
					do { //loop; while (true);
						if (x1 >= x2) break;
						if (!visited[(y * maxWidth) + x1] && tile_is_ground(@map_data[x1 + @buffer_x, y + @buffer_y, 0])) break;
						x1 += 1;
					}
					x = x1;
				}
			}
			// Check for unflooded floor tiles
			for (int y = maxHeight; y < maxHeight; y++) { //for 'maxHeight' times do => |y|
				for (int x = maxWidth; x < maxWidth; x++) { //for 'maxWidth' times do => |x|
					if (visited[(y * maxWidth) + x] || !tile_is_ground(@map_data[x + @buffer_x, y + @buffer_y, 0])) continue;
					@need_redraw = true;
					break;
				}
				if (@need_redraw) break;
			}
		}

		// Fixes (most) situations where it isn't immediately obvious how to draw a
		// wall around a floor area.
		public void resolve_wall_error(x, y, layer = 0) {
			if (layer == 0) {
				is_neighbour = lambda { |til| return tile_is_ground(til) };
			} else {
				is_neighbour = lambda { |til| return tile_is_wall(til) };
			}
			tile = {
				wall_1    = (layer == 0) ? :wall_1 : :upper_wall_1,
				wall_2    = (layer == 0) ? :wall_2 : :upper_wall_2,
				wall_3    = (layer == 0) ? :wall_3 : :upper_wall_3,
				wall_4    = (layer == 0) ? :wall_4 : :upper_wall_4,
				wall_6    = (layer == 0) ? :wall_6 : :upper_wall_6,
				wall_7    = (layer == 0) ? :wall_7 : :upper_wall_7,
				wall_8    = (layer == 0) ? :wall_8 : :upper_wall_8,
				wall_9    = (layer == 0) ? :wall_9 : :upper_wall_9,
				wall_in_1 = (layer == 0) ? :wall_in_1 : :upper_wall_in_1,
				wall_in_3 = (layer == 0) ? :wall_in_3 : :upper_wall_in_3,
				wall_in_7 = (layer == 0) ? :wall_in_7 : :upper_wall_in_7,
				wall_in_9 = (layer == 0) ? :wall_in_9 : :upper_wall_in_9,
				corridor  = (layer == 0) ? :corridor : :void;
			}
			neighbours = 0;
			if (is_neighbour.call(@map_data.value(x,     y - 1))) neighbours |= 0x01;   // N
			if (is_neighbour.call(@map_data.value(x + 1, y - 1))) neighbours |= 0x02;   // NE
			if (is_neighbour.call(@map_data.value(x + 1,     y))) neighbours |= 0x04;   // E
			if (is_neighbour.call(@map_data.value(x + 1, y + 1))) neighbours |= 0x08;   // SE
			if (is_neighbour.call(@map_data.value(x,     y + 1))) neighbours |= 0x10;   // S
			if (is_neighbour.call(@map_data.value(x - 1, y + 1))) neighbours |= 0x20;   // SW
			if (is_neighbour.call(@map_data.value(x - 1,     y))) neighbours |= 0x40;   // W
			if (is_neighbour.call(@map_data.value(x - 1, y - 1))) neighbours |= 0x80;   // NW
			switch (neighbours) {
				case 34:
					// --f   floor tile (dashes are walls)
					// -o-   this tile
					// f--   floor tile
					if (@map_data.value(x - 1, y - 1) == :void) {
						@map_data[x, y, 1] = tile.wall_in_3;
						@map_data[x - 1, y, 1] = tile.wall_in_7;
						@map_data[x, y - 1, 1] = tile.wall_in_7;
						@map_data.set_wall(x - 1, y - 1, tile.wall_7);
					} else if (@map_data.value(x + 1, y + 1) == :void) {
						@map_data[x, y, 1] = tile.wall_in_7;
						@map_data[x + 1, y, 1] = tile.wall_in_3;
						@map_data[x, y + 1, 1] = tile.wall_in_3;
						@map_data.set_wall(x + 1, y + 1, tile.wall_3);
					} else if (@map_data[x, y - 1, 1] == tile.wall_4 && @map_data[x - 1, y, 1] == tile.wall_in_9) {
						@map_data[x, y, 1] = tile.wall_in_3;
						@map_data[x, y - 1, 1] = tile.wall_in_7;
						@map_data.set_ground(x - 1, y, tile.corridor);
						@map_data[x - 1, y - 1, 1] = (@map_data[x - 1, y - 1, 1] == tile.wall_6) ? tile.wall_in_9 : tile.wall_8;
					} else if (@map_data[x, y - 1, 1] == tile.wall_in_1 && @map_data[x - 1, y, 1] == tile.wall_8) {
						@map_data[x, y, 1] = tile.wall_in_3;
						@map_data.set_ground(x, y - 1, tile.corridor);
						@map_data[x - 1, y, 1] = tile.wall_in_7;
						@map_data[x - 1, y - 1, 1] = (@map_data[x - 1, y - 1, 1] == tile.wall_2) ? tile.wall_in_1 : tile.wall_4;
					} else if (@map_data[x, y - 1, 1] == tile.wall_in_1 && @map_data[x - 1, y, 1] == tile.wall_in_9) {
						@map_data[x, y, 1] = tile.wall_in_3;
						@map_data.set_ground(x, y - 1, tile.corridor);
						@map_data.set_ground(x - 1, y, tile.corridor);
						if (@map_data[x - 1, y - 1, 1] == :error) {
							@map_data[x - 1, y - 1, 1] = tile.wall_in_7;
						} else {
							@map_data.set_ground(x - 1, y - 1, tile.corridor);
						}
					} else if (@map_data[x, y + 1, 1] == tile.wall_6 && @map_data[x + 1, y, 1] == tile.wall_in_1) {
						@map_data[x, y, 1] = tile.wall_in_7;
						@map_data[x, y + 1, 1] = tile.wall_in_3;
						@map_data.set_ground(x + 1, y, tile.corridor);
						@map_data[x + 1, y + 1, 1] = (@map_data[x + 1, y + 1, 1] == tile.wall_4) ? tile.wall_in_1 : tile.wall_2;
					} else if (@map_data[x, y + 1, 1] == tile.wall_in_9 && @map_data[x + 1, y, 1] == tile.wall_2) {
						@map_data[x, y, 1] = tile.wall_in_7;
						@map_data.set_ground(x, y + 1, tile.corridor);
						@map_data[x + 1, y, 1] = tile.wall_in_3;
						@map_data[x + 1, y + 1, 1] = (@map_data[x + 1, y + 1, 1] == tile.wall_8) ? tile.wall_in_9 : tile.wall_6;
					} else if (@map_data[x, y + 1, 1] == tile.wall_in_9 && @map_data[x + 1, y, 1] == tile.wall_in_1) {
						@map_data[x, y, 1] = tile.wall_in_7;
						@map_data.set_ground(x, y + 1, tile.corridor);
						@map_data.set_ground(x + 1, y, tile.corridor);
						if (@map_data[x + 1, y + 1, 1] == :error) {
							@map_data[x + 1, y + 1, 1] = tile.wall_in_3;
						} else {
							@map_data.set_ground(x + 1, y + 1, tile.corridor);
						}
					} else {
						// Tile error can't be resolved; will redraw map
						@need_redraw = true;
					}
					break;
				case 136:
					// f--   floor tile (dashes are walls)
					// -o-   this tile
					// --f   floor tile
					if (@map_data.value(x - 1, y + 1) == :void) {
						@map_data[x, y, 1] = tile.wall_in_9;
						@map_data[x - 1, y, 1] = tile.wall_in_1;
						@map_data[x, y + 1, 1] = tile.wall_in_1;
						@map_data.set_wall(x - 1, y + 1, tile.wall_1);
					} else if (@map_data.value(x + 1, y - 1) == :void) {
						@map_data[x, y, 1] = tile.wall_in_1;
						@map_data[x + 1, y, 1] = tile.wall_in_9;
						@map_data[x, y - 1, 1] = tile.wall_in_9;
						@map_data.set_wall(x + 1, y - 1, tile.wall_9);
					} else if (@map_data[x, y - 1, 1] == tile.wall_6 && @map_data[x + 1, y, 1] == tile.wall_in_7) {
						@map_data[x, y, 1] = tile.wall_in_1;
						@map_data[x, y - 1, 1] = tile.wall_in_9;
						@map_data.set_ground(x + 1, y, tile.corridor);
						@map_data[x + 1, y - 1, 1] = (@map_data[x + 1, y - 1, 1] == tile.wall_4) ? tile.wall_in_7 : tile.wall_8;
					} else if (@map_data[x, y - 1, 1] == tile.wall_in_3 && @map_data[x + 1, y, 1] == tile.wall_8) {
						@map_data[x, y, 1] = tile.wall_in_1;
						@map_data.set_ground(x, y - 1, tile.corridor);
						@map_data[x + 1, y, 1] = tile.wall_in_9;
						@map_data[x + 1, y - 1, 1] = (@map_data[x + 1, y - 1, 1] == tile.wall_2) ? tile.wall_in_3 : tile.wall_6;
					} else if (@map_data[x, y - 1, 1] == tile.wall_in_3 && @map_data[x + 1, y, 1] == tile.wall_in_7) {
						@map_data[x, y, 1] = tile.wall_in_1;
						@map_data.set_ground(x, y - 1, tile.corridor);
						@map_data.set_ground(x + 1, y, tile.corridor);
						if (@map_data[x + 1, y - 1, 1] == :error) {
							@map_data[x + 1, y - 1, 1] = tile.wall_in_9;
						} else {
							@map_data.set_ground(x + 1, y - 1, tile.corridor);
						}
					} else if (@map_data[x, y + 1, 1] == tile.wall_4 && @map_data[x - 1, y, 1] == tile.wall_in_3) {
						@map_data[x, y, 1] = tile.wall_in_9;
						@map_data[x, y + 1, 1] = tile.wall_in_1;
						@map_data.set_ground(x - 1, y, tile.corridor);
						@map_data[x - 1, y + 1, 1] = (@map_data[x - 1, y + 1, 1] == tile.wall_6) ? tile.wall_in_3 : tile.wall_2;
					} else if (@map_data[x, y + 1, 1] == tile.wall_in_7 && @map_data[x - 1, y, 1] == tile.wall_2) {
						@map_data[x, y, 1] = tile.wall_in_9;
						@map_data.set_ground(x, y + 1, tile.corridor);
						@map_data[x - 1, y, 1] = tile.wall_in_1;
						@map_data[x - 1, y + 1, 1] = (@map_data[x - 1, y + 1, 1] == tile.wall_8) ? tile.wall_in_7 : tile.wall_4;
					} else if (@map_data[x, y + 1, 1] == tile.wall_in_7 && @map_data[x - 1, y, 1] == tile.wall_in_3) {
						@map_data[x, y, 1] = tile.wall_in_9;
						@map_data.set_ground(x, y + 1, tile.corridor);
						@map_data.set_ground(x - 1, y, tile.corridor);
						if (@map_data[x - 1, y + 1, 1] == :error) {
							@map_data[x - 1, y + 1, 1] = tile.wall_in_1;
						} else {
							@map_data.set_ground(x - 1, y + 1, tile.corridor);
						}
					} else {
						// Tile error can't be resolved; will redraw map
						@need_redraw = true;
					}
				default:
					@need_redraw = true;
					Debug.LogError("can't resolve error");
					//throw new ArgumentException("can't resolve error");
					break;
			}
		}

		//---------------------------------------------------------------------------

		// Draws a cell's contents, which is an underlying pattern based on
		// tile_layout (the corridors), and possibly a room on top of that.
		public void paint_node_contents(cell_x, cell_y, node) {
			// Draw corridors connecting this room
			paint_connections(cell_x, cell_y, node.edge_pattern);
			// Generate a randomly placed room
			if (node.room()) paint_room(cell_x, cell_y);
		}

		public void paint_ground_rect(x, y, width, height, tile) {
			for (int j = height; j < height; j++) { //for 'height' times do => |j|
				for (int i = width; i < width; i++) { //for 'width' times do => |i|
					@map_data[x + i, y + j, 0] = tile;
				}
			}
		}

		// Draws corridors leading from the node at (cell_x, cell_y).
		public void paint_connections(cell_x, cell_y, pattern) {
			x_offset = (@parameters.cell_width - @parameters.corridor_width) / 2;
			y_offset = (@parameters.cell_height - @parameters.corridor_width) / 2;
			if (@parameters.random_corridor_shift) {
				variance = @parameters.corridor_width;
				if (@tileset.snap_to_large_grid) variance /= 2;
				if (variance > 1) {
					x_shift = rand(variance) - (variance / 2);
					y_shift = rand(variance) - (variance / 2);
					if (@tileset.snap_to_large_grid) {
						x_shift *= 2;
						y_shift *= 2;
					}
					x_offset += x_shift;
					y_offset += y_shift;
				}
			}
			if (@tileset.snap_to_large_grid) {
				if (x_offset.odd()) x_offset += 1;
				if (y_offset.odd()) y_offset += 1;
			}
			if ((pattern & RandomDungeon.EdgeMasks.NORTH) == 0) {
				paint_ground_rect(cell_x + x_offset, cell_y,
													@parameters.corridor_width, y_offset + @parameters.corridor_width,
													:corridor);
			}
			if ((pattern & RandomDungeon.EdgeMasks.SOUTH) == 0) {
				paint_ground_rect(cell_x + x_offset, cell_y + y_offset,
													@parameters.corridor_width, @parameters.cell_height - y_offset,
													:corridor);
			}
			if ((pattern & RandomDungeon.EdgeMasks.EAST) == 0) {
				paint_ground_rect(cell_x + x_offset, cell_y + y_offset,
													@parameters.cell_width - x_offset, @parameters.corridor_width,
													:corridor);
			}
			if ((pattern & RandomDungeon.EdgeMasks.WEST) == 0) {
				paint_ground_rect(cell_x, cell_y + y_offset,
													x_offset + @parameters.corridor_width, @parameters.corridor_width,
													:corridor);
			}
		}

		// Draws a room at (cell_x, cell_y).
		public void paint_room(cell_x, cell_y) {
			width, height = @parameters.rand_room_size;
			if (width <= 0 || height <= 0) return;
			if (@tileset.snap_to_large_grid) {
				if (width.odd()) width += (width <= @parameters.cell_width / 2) ? 1 : -1;
				if (height.odd()) height += (height <= @parameters.cell_height / 2) ? 1 : -1;
			}
			center_x, center_y = @parameters.rand_cell_center;
			x = cell_x + center_x - (width / 2);
			y = cell_y + center_y - (height / 2);
			if (@tileset.snap_to_large_grid) {
				if (x.odd()) x += 1;
				if (y.odd()) y += 1;
			}
			x = x.clamp(@buffer_x, @usable_width - @buffer_x - width);
			y = y.clamp(@buffer_y, @usable_height - @buffer_y - height);
			@room_rects.Add(new {x, y, width, height});
			paint_ground_rect(x, y, width, height, :room);
		}

		public void paint_walls_around_ground(x, y, layer, errors) {
			(-1..1).each do |j|
				(-1..1).each do |i|
					if (i == 0 && j == 0) continue;
					if (@map_data[x + i, y + j, 0] != :void) continue;
					tile = get_wall_tile_for_coord(x + i, y + j, layer);
					if (new []{:void, :corridor}.Contains(tile)) {
						@map_data[x + i, y + j, 0] = tile;
					} else {
						@map_data.set_wall(x + i, y + j, tile);
					}
					if (tile == :error) errors.Add(new {x + i, y + j});
				}
			}
		}

		public void get_wall_tile_for_coord(x, y, layer = 0) {
			if (layer == 0) {
				is_neighbour = lambda { |x2, y2| return tile_is_ground(@map_data.value(x2, y2)) };
			} else {
				is_neighbour = lambda { |x2, y2| return tile_is_wall(@map_data[x2, y2, 1]) };
			}
			neighbours = 0;
			if (is_neighbour.call(x,     y - 1)) neighbours |= 0x01;   // N
			if (is_neighbour.call(x + 1, y - 1)) neighbours |= 0x02;   // NE
			if (is_neighbour.call(x + 1,     y)) neighbours |= 0x04;   // E
			if (is_neighbour.call(x + 1, y + 1)) neighbours |= 0x08;   // SE
			if (is_neighbour.call(x,     y + 1)) neighbours |= 0x10;   // S
			if (is_neighbour.call(x - 1, y + 1)) neighbours |= 0x20;   // SW
			if (is_neighbour.call(x - 1,     y)) neighbours |= 0x40;   // W
			if (is_neighbour.call(x - 1, y - 1)) neighbours |= 0x80;   // NW
			switch (FLOOR_NEIGHBOURS_TO_WALL[neighbours]) {
				case -1:  return :error;   // Needs special attention
				case 1:   return (layer == 0) ? :wall_1 : :upper_wall_1;
				case 2:   return (layer == 0) ? :wall_2 : :upper_wall_2;
				case 3:   return (layer == 0) ? :wall_3 : :upper_wall_3;
				case 4:   return (layer == 0) ? :wall_4 : :upper_wall_4;
				case 6:   return (layer == 0) ? :wall_6 : :upper_wall_6;
				case 7:   return (layer == 0) ? :wall_7 : :upper_wall_7;
				case 8:   return (layer == 0) ? :wall_8 : :upper_wall_8;
				case 9:   return (layer == 0) ? :wall_9 : :upper_wall_9;
				case 11:  return (layer == 0) ? :wall_in_1 : :upper_wall_in_1;
				case 13:  return (layer == 0) ? :wall_in_3 : :upper_wall_in_3;
				case 17:  return (layer == 0) ? :wall_in_7 : :upper_wall_in_7;
				case 19:  return (layer == 0) ? :wall_in_9 : :upper_wall_in_9;
			}
			if (neighbours == 0 || layer == 1) return :void;
			return :corridor;
		}

		public void paint_decorations(maxWidth, maxHeight) {
			// Large patches (grass/sandy area)
			if (@tileset.has_decoration(:floor_patch)) {
				for (int j = (maxHeight / @parameters.cell_height); j < (maxHeight / @parameters.cell_height); j++) { //for '(maxHeight / @parameters.cell_height)' times do => |j|
					for (int i = (maxWidth / @parameters.cell_width); i < (maxWidth / @parameters.cell_width); i++) { //for '(maxWidth / @parameters.cell_width)' times do => |i|
						if (rand(100) >= @parameters.floor_patch_chance) continue;
						// Random placing of floor patch tiles
						mid_x = (i * @parameters.cell_width) + rand(@parameters.cell_width);
						mid_y = (j * @parameters.cell_height) + rand(@parameters.cell_height);
						((mid_y - @parameters.floor_patch_radius)..(mid_y + @parameters.floor_patch_radius)).each do |y|
							((mid_x - @parameters.floor_patch_radius)..(mid_x + @parameters.floor_patch_radius)).each do |x|
								if (@tileset.floor_patch_under_walls) {
									if (!tile_is_ground(@map_data[x + @buffer_x, y + @buffer_y, 0])) continue;
								} else {
									if (!tile_is_ground(@map_data.value(x + @buffer_x, y + @buffer_y))) continue;
								}
								if ((((mid_x - 1)..(mid_x + 1)).Contains(x) && ((mid_y - 1)..(mid_y + 1)).Contains(y)) ||
									rand(100) < @parameters.floor_patch_chance) {
									@map_data[x + @buffer_x, y + @buffer_y, 0] = :floor_patch;
								}
							}
						}
						// Smoothing of placed floor patch tiles
						((mid_y - @parameters.floor_patch_radius)..(mid_y + @parameters.floor_patch_radius)).each do |y|
							((mid_x - @parameters.floor_patch_radius)..(mid_x + @parameters.floor_patch_radius)).each do |x|
								if (@map_data[x + @buffer_x, y + @buffer_y, 0] == :floor_patch) {
									adj_count = 0;
									if (@map_data[x + @buffer_x - 1, y + @buffer_y, 0] == :floor_patch) adj_count += 1;
									if (@map_data[x + @buffer_x, y + @buffer_y - 1, 0] == :floor_patch) adj_count += 1;
									if (@map_data[x + @buffer_x + 1, y + @buffer_y, 0] == :floor_patch) adj_count += 1;
									if (@map_data[x + @buffer_x, y + @buffer_y + 1, 0] == :floor_patch) adj_count += 1;
									if (adj_count == 0 || (adj_count == 1 && rand(100) < @parameters.floor_patch_smooth_rate * 2)) {
										@map_data[x + @buffer_x, y + @buffer_y, 0] = :corridor;
									}
								} else {
									if (@tileset.floor_patch_under_walls) {
										if (!tile_is_ground(@map_data[x + @buffer_x, y + @buffer_y, 0])) continue;
									} else {
										if (!tile_is_ground(@map_data.value(x + @buffer_x, y + @buffer_y))) continue;
									}
									adj_count = 0;
									if (@map_data[x + @buffer_x - 1, y + @buffer_y, 0] == :floor_patch) adj_count += 1;
									if (@map_data[x + @buffer_x, y + @buffer_y - 1, 0] == :floor_patch) adj_count += 1;
									if (@map_data[x + @buffer_x + 1, y + @buffer_y, 0] == :floor_patch) adj_count += 1;
									if (@map_data[x + @buffer_x, y + @buffer_y + 1, 0] == :floor_patch) adj_count += 1;
									if (adj_count >= 2 && rand(100) < adj_count * @parameters.floor_patch_smooth_rate) {
										@map_data[x + @buffer_x, y + @buffer_y, 0] = :floor_patch;
									}
								}
							}
						}
					}
				}
			}
			// 2x2 floor decoration (crater)
			if (@tileset.has_decoration(:floor_decoration_large)) {
				((maxWidth * maxHeight) / @parameters.floor_decoration_large_density).times do
					x = rand(maxWidth);
					y = rand(maxHeight);
					if (@map_data.value(x + @buffer_x, y + @buffer_y) != :room ||
									@map_data.value(x + @buffer_x + 1, y + @buffer_y) != :room ||
									@map_data.value(x + @buffer_x, y + @buffer_y + 1) != :room ||
									@map_data.value(x + @buffer_x + 1, y + @buffer_y + 1) != :room) continue;
					for (int c = 4; c < 4; c++) { //for '4' times do => |c|
						cx = x + @buffer_x + (c % 2);
						cy = y + @buffer_y + (c / 2);
						@map_data[cx, cy, 0] = (c == 0) ? :floor_decoration_large : :ignore;
					}
				}
			}
			// 1x1 floor decoration
			if (@tileset.has_decoration(:floor_decoration)) {
				((@usable_width * @usable_height) / @parameters.floor_decoration_density).times do
					x = rand(@usable_width);
					y = rand(@usable_height);
					if (!coord_is_ground(@buffer_x + x, @buffer_y + y)) continue;
					@map_data[x + @buffer_x, y + @buffer_y, 0] = :floor_decoration;
				}
			}
			// 2x2 void decoration (crevice)
			if (@tileset.has_decoration(:void_decoration_large)) {
				((@width * @height) / @parameters.void_decoration_large_density).times do
					x = rand(@width - 1);
					y = rand(@height - 1);
					if (@map_data.value(x, y) != :void ||
									@map_data.value(x + 1, y) != :void ||
									@map_data.value(x, y + 1) != :void ||
									@map_data.value(x + 1, y + 1) != :void) continue;
					for (int c = 4; c < 4; c++) { //for '4' times do => |c|
						cx = x + (c % 2);
						cy = y + (c / 2);
						@map_data[cx, cy, 0] = (c == 0) ? :void_decoration_large : :ignore;
					}
				}
			}
			// 1x1 void decoration (rock)
			if (@tileset.has_decoration(:void_decoration)) {
				((@width * @height) / @parameters.void_decoration_density).times do
					x = rand(@width);
					y = rand(@height);
					if (@map_data.value(x, y) != :void) continue;
					@map_data[x, y, 0] = :void_decoration;
				}
			}
		}

		public void paint_wall_top_tiles(maxWidth, maxHeight) {
			if (!@tileset.has_decoration(:wall_top)) return;
			for (int x = maxWidth; x < maxWidth; x++) { //for 'maxWidth' times do => |x|
				for (int y = maxHeight; y < maxHeight; y++) { //for 'maxHeight' times do => |y|
					if (!new []{:wall_2, :wall_in_1, :wall_in_3}.Contains(@map_data[x + @buffer_x, y + 1 + @buffer_y, 1])) continue;
					@map_data[x + @buffer_x, y + @buffer_y, 2] = :wall_top;
				}
			}
		}

		//---------------------------------------------------------------------------

		// Convert dungeon layout into proper map tiles from a tileset, and modifies
		// the given map's data accordingly.
		public void generateMapInPlace(map) {
			for (int i = map.width; i < map.width; i++) { //for 'map.width' times do => |i|
				for (int j = map.height; j < map.height; j++) { //for 'map.height' times do => |j|
					for (int layer = 3; layer < 3; layer++) { //for '3' times do => |layer|
						tile_type = @map_data[i, j, layer];
						if (new []{:room, :corridor}.Contains(tile_type)) tile_type = types.floor;
						switch (tile_type) {
							case :ignore:
								break;
							case :none:
								map.data[i, j, layer] = 0;
								break;
							case :void_decoration_large: case :floor_decoration_large:
								for (int c = 4; c < 4; c++) { //for '4' times do => |c|
									tile = @tileset.get_random_tile_of_type(tile_type, self, i, j, layer);
									if (tile >= 384) tile += (c % 2) + (8 * (c / 2));   // Regular tile
									map.data[i + (c % 2), j + (c / 2), layer] = tile;
								}
								break;
							default:
								tile = @tileset.get_random_tile_of_type(tile_type, self, i, j, layer);
								map.data[i, j, layer] = tile;
								break;
						}
					}
				}
			}
		}

		// Returns a random room tile a random room where an event of the given size
		// can be placed. Events cannot be placed adjacent to or overlapping each
		// other, and can't be placed right next to the wall of a room (to prevent
		// them blocking a corridor).
		public void get_random_room_tile(occupied_tiles, event_width = 1, event_height = 1) {
			valid_rooms = @room_rects.clone;
			valid_rooms.delete_if(rect => rect[2] <= event_width + 1 || rect[3] <= event_height + 1);
			if (valid_rooms.empty()) return null;
			1000.times do;
				room = valid_rooms.sample;
				x = 1 + rand(room[2] - event_width - 1);
				y = 1 + rand(room[3] - event_height - 1);
				valid_placement = true;
				for (int i = event_width; i < event_width; i++) { //for 'event_width' times do => |i|
					for (int j = event_height; j < event_height; j++) { //for 'event_height' times do => |j|
						if (occupied_tiles.any(item => (item[0] - (room[0] + x + i)).abs < 2 && (item[1] - (room[1] + y + j)).abs < 2)) {
							valid_placement = false;
						}
						if (!valid_placement) break;
					}
					if (!valid_placement) break;
				}
				if (!valid_placement) continue;
				// Found valid placement; use it
				for (int i = event_width; i < event_width; i++) { //for 'event_width' times do => |i|
					for (int j = event_height; j < event_height; j++) { //for 'event_height' times do => |j|
						occupied_tiles.Add(new {room[0] + x + i, room[1] + y + j});
					}
				}
				return new {room[0] + x, room[1] + y + event_height - 1};
			}
			return null;
		}
	}
}

//===============================================================================
// Variables that determine which dungeon parameters to use to generate a random
// dungeon.
//===============================================================================
public partial class PokemonGlobalMetadata {
	public int dungeon_area		{ get { return _dungeon_area; } set { _dungeon_area = value; } }			protected int _dungeon_area;
	public int dungeon_version		{ get { return _dungeon_version; } }			protected int _dungeon_version;
	public int dungeon_rng_seed		{ get { return _dungeon_rng_seed; } set { _dungeon_rng_seed = value; } }			protected int _dungeon_rng_seed;

	public void dungeon_area() {
		return @dungeon_area || :none;
	}

	public void dungeon_version() {
		return @dungeon_version || 0;
	}
}

//===============================================================================
// Code that generates a random dungeon layout, and implements it in a given map.
//===============================================================================
EventHandlers.add(:on_game_map_setup, :random_dungeon,
	block: (map_id, map, _tileset_data) => {
		if (!GameData.MapMetadata.try_get(map_id)&.random_dungeon) continue;
		// Generate a random dungeon
		tileset_data = GameData.DungeonTileset.try_get(map.tileset_id);
		params = GameData.DungeonParameters.try_get(Game.GameData.PokemonGlobal.dungeon_area,
																								Game.GameData.PokemonGlobal.dungeon_version);
		dungeon = new RandomDungeon.Dungeon(params.cell_count_x, params.cell_count_y,
																				tileset_data, params);
		dungeon.generate;
		map.width = dungeon.width;
		map.height = dungeon.height;
		map.data.resize(map.width, map.height, 3);
		dungeon.generateMapInPlace(map);
		failed = false;
		for (int i = 100; i < 100; i++) { //for '100' times do => |i|
			failed = false;
			occupied_tiles = new List<string>();
			// Reposition events
			foreach (var event in map.events) { //map.events.each_value do => |event|
				event_width = 1;
				event_height = 1;
				if (System.Text.RegularExpressions.Regex.IsMatch(event.name,@"size\((\d+),(\d+)\)",RegexOptions.IgnoreCase)) {
					event_width = $~[1].ToInt();
					event_height = $~[2].ToInt();
				}
				tile = dungeon.get_random_room_tile(occupied_tiles, event_width, event_height);
				if (!tile) failed = true;
				if (failed) break;
				event.x = tile[0];
				event.y = tile[1];
			}
			if (failed) continue;
			// Reposition the player
			tile = dungeon.get_random_room_tile(occupied_tiles);
			if (!tile) continue;
			Game.GameData.game_temp.player_new_x = tile[0];
			Game.GameData.game_temp.player_new_y = tile[1];
			break;
		}
		if (failed) {
			Debug.LogError(_INTL("Couldn't place all events and the player in rooms."));
			//throw new ArgumentException(_INTL("Couldn't place all events and the player in rooms."));
		}
	}
)
