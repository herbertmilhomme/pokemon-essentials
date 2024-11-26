//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class DungeonParameters {
		public int id		{ get { return _id; } set { _id = value; } }			protected int _id;
		public int area		{ get { return _area; } }			protected int _area;
		public int version	{ get { return _version; } }			protected int _version;
		public int cell_count_x		{ get { return _cell_count_x; } set { _cell_count_x = value; } }			protected int _cell_count_x;
		public int cell_count_y		{ get { return _cell_count_y; } }			protected int _cell_count_y;
		public int cell_width		{ get { return _cell_width; } set { _cell_width = value; } }			protected int _cell_width;
		public int cell_height		{ get { return _cell_height; } }			protected int _cell_height;
		public int room_min_width		{ get { return _room_min_width; } set { _room_min_width = value; } }			protected int _room_min_width;
		public int room_min_height		{ get { return _room_min_height; } }			protected int _room_min_height;
		public int room_max_width		{ get { return _room_max_width; } set { _room_max_width = value; } }			protected int _room_max_width;
		public int room_max_height		{ get { return _room_max_height; } }			protected int _room_max_height;
		public int corridor_width		{ get { return _corridor_width; } set { _corridor_width = value; } }			protected int _corridor_width;
		public int random_corridor_shift		{ get { return _random_corridor_shift; } }			protected int _random_corridor_shift;
		// Layouts:
		//   :full          - every node in the map
		//   :no_corners    - every node except for one in each corner
		//   :ring          - every node around the edge of the map
		//   :antiring      - every node except one that touches an edge of the map
		//   :plus          - every node in a plus (+) shape
		//   :diagonal_up   - every node in a line from bottom left to top right (/)
		//   :diagonal_down - every node in a line from top left to bottom right (\)
		//   :cross         - every node in a cross (x) shape
		//   :quadrants     - every node except the middles of each edge (i.e. each corner bulges out)
		public int node_layout		{ get { return _node_layout; } set { _node_layout = value; } }			protected int _node_layout;
		public int room_layout		{ get { return _room_layout; } }			protected int _room_layout;
		/// <summary>Percentage of active roomable nodes that will become rooms</summary>
		public int room_chance		{ get { return _room_chance; } }			protected int _room_chance;
		public int extra_connections_count		{ get { return _extra_connections_count; } }			protected int _extra_connections_count;
		public int floor_patch_radius		{ get { return _floor_patch_radius; } set { _floor_patch_radius = value; } }			protected int _floor_patch_radius;
		public int floor_patch_chance		{ get { return _floor_patch_chance; } }			protected int _floor_patch_chance;
		public int floor_patch_smooth_rate	{ get { return _floor_patch_smooth_rate; } }			protected int _floor_patch_smooth_rate;
		public int floor_decoration_density		{ get { return _floor_decoration_density; } set { _floor_decoration_density = value; } }			protected int _floor_decoration_density;
		public int floor_decoration_large_density		{ get { return _floor_decoration_large_density; } }			protected int _floor_decoration_large_density;
		public int void_decoration_density		{ get { return _void_decoration_density; } set { _void_decoration_density = value; } }			protected int _void_decoration_density;
		public int void_decoration_large_density		{ get { return _void_decoration_large_density; } }			protected int _void_decoration_large_density;
		public int rng_seed		{ get { return _rng_seed; } }			protected int _rng_seed;
		public int flags		{ get { return _flags; } }			protected int _flags;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "dungeon_parameters.dat";
		public const string PBS_BASE_FILENAME = "dungeon_parameters";
		SCHEMA = {
			"SectionName"      => new {:id,                      "mV"},
			"DungeonSize"      => new {:dungeon_size,            "vv"},
			"CellSize"         => new {:cell_size,               "vv"},
			"MinRoomSize"      => new {:min_room_size,           "vv"},
			"MaxRoomSize"      => new {:max_room_size,           "vv"},
			"CorridorWidth"    => new {:corridor_width,          "v"},
			"ShiftCorridors"   => new {:random_corridor_shift,   "b"},
			"NodeLayout"       => new {:node_layout,             "m"},
			"RoomLayout"       => new {:room_layout,             "m"},
			"RoomChance"       => new {:room_chance,             "v"},
			"ExtraConnections" => new {:extra_connections_count, "u"},
			"FloorPatches"     => new {:floor_patches,           "vvu"},
			"FloorDecorations" => new {:floor_decorations,       "uu"},
			"VoidDecorations"  => new {:void_decorations,        "uu"},
			"RNGSeed"          => new {:rng_seed,                "u"},
			"Flags"            => new {:flags,                   "*s"}
		}

		extend ClassMethodsSymbols;
		include InstanceMethods;

		/// <param name="area"> | Symbol, String, self</param>
		/// <param name="version"></param>
		// @return [self]
		public static void try_get(Symbol area, Integer version = 0) {
			validate area => [Symbol, self, String];
			validate version => Integer;
			if (area.is_a(self)) area = area.id;
			if (area.is_a(String)) area = area.to_sym;
			trial = string.Format("{0}_{0}", area, version).to_sym;
			area_version = (DATA[trial].null()) ? area : trial;
			return (DATA.has_key(area_version)) ? DATA[area_version] : new self(new List<string>());
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id                             = hash.id;
			@area                           = hash.area;
			@version                        = hash.version                 || 0;
			@cell_count_x                   = (hash.dungeon_size) ? hash.dungeon_size[0] : 5;
			@cell_count_y                   = (hash.dungeon_size) ? hash.dungeon_size[1] : 5;
			@cell_width                     = (hash.cell_size) ? hash.cell_size[0] : 10;
			@cell_height                    = (hash.cell_size) ? hash.cell_size[1] : 10;
			@room_min_width                 = (hash.min_room_size) ? hash.min_room_size[0] : 5;
			@room_min_height                = (hash.min_room_size) ? hash.min_room_size[1] : 5;
			@room_max_width                 = (hash.max_room_size) ? hash.max_room_size[0] : @cell_width - 1;
			@room_max_height                = (hash.max_room_size) ? hash.max_room_size[1] : @cell_height - 1;
			@corridor_width                 = hash.corridor_width          || 2;
			@random_corridor_shift          = hash.random_corridor_shift;
			@node_layout                    = hash.node_layout             || :full;
			@room_layout                    = hash.room_layout             || :full;
			@room_chance                    = hash.room_chance             || 70;
			@extra_connections_count        = hash.extra_connections_count || 2;
			@floor_patch_radius             = (hash.floor_patches) ? hash.floor_patches[0] : 3;
			@floor_patch_chance             = (hash.floor_patches) ? hash.floor_patches[1] : 75;
			@floor_patch_smooth_rate        = (hash.floor_patches) ? hash.floor_patches[2] : 25;
			@floor_decoration_density       = (hash.floor_decorations) ? hash.floor_decorations[0] : 50;
			@floor_decoration_large_density = (hash.floor_decorations) ? hash.floor_decorations[1] : 200;
			@void_decoration_density        = (hash.void_decorations) ? hash.void_decorations[0] : 50;
			@void_decoration_large_density  = (hash.void_decorations) ? hash.void_decorations[1] : 200;
			@rng_seed                       = hash.rng_seed;
			@flags                          = hash.flags                   || [];
			@s_file_suffix                = hash.s_file_suffix         || "";
		}

		public bool has_flag(flag) {
			return @flags.any(f => f.downcase == flag.downcase);
		}

		public void rand_cell_center() {
			x = (@cell_width / 2) + rand(-2..2);
			y = (@cell_height / 2) + rand(-2..2);
			return x, y;
		}

		public void rand_room_size() {
			width = @room_min_width;
			if (@room_max_width > @room_min_width) {
				width = rand(@room_min_width..@room_max_width);
			}
			height = @room_min_height;
			if (@room_max_height > @room_min_height) {
				height = rand(@room_min_height..@room_max_height);
			}
			return width, height;
		}

		unless (method_defined(:__orig__get_property_for_PBS)) alias __orig__get_property_for_PBS get_property_for_PBS;
		public void get_property_for_PBS(key) {
			switch (key) {
				case "SectionName":       return new {@area, (@version > 0) ? @version : null};
				case "DungeonSize":       return new {@cell_count_x, @cell_count_y};
				case "CellSize":          return new {@cell_width, @cell_height};
				case "MinRoomSize":       return new {@room_min_width, @room_min_height};
				case "MaxRoomSize":       return new {@room_max_width, @room_max_height};
				case "FloorPatches":      return new {@floor_patch_radius, @floor_patch_chance, @floor_patch_smooth_rate};
				case "FloorDecorations":  return new {@floor_decoration_density, @floor_decoration_large_density};
				case "VoidDecorations":   return new {@void_decoration_density, @void_decoration_large_density};
			}
			return __orig__get_property_for_PBS(key);
		}
	}
}
