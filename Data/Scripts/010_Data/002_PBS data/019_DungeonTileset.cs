//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class DungeonTileset {
		public int id		{ get { return _id; } }			protected int _id;
		public int tile_type_ids		{ get { return _tile_type_ids; } }			protected int _tile_type_ids;
		/// <summary>"large" means 2x2 tiles</summary>
		public int snap_to_large_grid		{ get { return _snap_to_large_grid; } }			protected int _snap_to_large_grid;
		/// <summary>"large" means 2x2 tiles</summary>
		public int large_void_tiles		{ get { return _large_void_tiles; } }			protected int _large_void_tiles;
		/// <summary>"large" means 1x2 or 2x1 tiles depending on side</summary>
		public int large_wall_tiles		{ get { return _large_wall_tiles; } }			protected int _large_wall_tiles;
		/// <summary>"large" means 2x2 tiles</summary>
		public int large_floor_tiles		{ get { return _large_floor_tiles; } }			protected int _large_floor_tiles;
		public int double_walls		{ get { return _double_walls; } }			protected int _double_walls;
		public int floor_patch_under_walls		{ get { return _floor_patch_under_walls; } }			protected int _floor_patch_under_walls;
		public int thin_north_wall_offset		{ get { return _thin_north_wall_offset; } }			protected int _thin_north_wall_offset;
		public int flags		{ get { return _flags; } }			protected int _flags;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "dungeon_tilesets.dat";
		public const string PBS_BASE_FILENAME = "dungeon_tilesets";
		SCHEMA = {
			"SectionName"          => new {:id,                      "u"},
			"Autotile"             => new {:autotile,                "^um"},
			"Tile"                 => new {:tile,                    "^um"},
			"SnapToLargeGrid"      => new {:snap_to_large_grid,      "b"},
			"LargeVoidTiles"       => new {:large_void_tiles,        "b"},
			"LargeWallTiles"       => new {:large_wall_tiles,        "b"},
			"LargeFloorTiles"      => new {:large_floor_tiles,       "b"},
			"DoubleWalls"          => new {:double_walls,            "b"},
			"FloorPatchUnderWalls" => new {:floor_patch_under_walls, "b"},
			"ThinNorthWallOffset"  => new {:thin_north_wall_offset,  "i"},
			"Flags"                => new {:flags,                   "*s"}
		}

		extend ClassMethodsIDNumbers;
		include InstanceMethods;

		/// <param name="other"> | self, Integer</param>
		// @return [self]
		public static void try_get(self other) {
			validate other => [Integer, self];
			if (other.is_a(self)) return other;
			return (self.DATA.has_key(other)) ? self.DATA[other] : self.get(self.DATA.keys.first);
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id                      = hash.id;
			@snap_to_large_grid      = hash.snap_to_large_grid      || false;
			@large_void_tiles        = hash.large_void_tiles        || false;
			@large_wall_tiles        = hash.large_wall_tiles        || false;
			@large_floor_tiles       = hash.large_floor_tiles       || false;
			@double_walls            = hash.double_walls            || false;
			@floor_patch_under_walls = hash.floor_patch_under_walls || false;
			@thin_north_wall_offset  = hash.thin_north_wall_offset  || 0;
			@flags                   = hash.flags                   || [];
			@tile_type_ids           = new List<string>();
			set_tile_type_ids(hash);
			@s_file_suffix         = hash.s_file_suffix         || "";
		}

		public void set_tile_type_ids(hash) {
			new {hash.autotile, hash.tile}.each_with_index do |array, i|
				foreach (var tile_info in array) { //'array.each' do => |tile_info|
					if (!tile_info) continue;
					tile_type = tile_info[1];
					if (tile_type == types.walls) {
						if (@double_walls) {
							if (@large_wall_tiles) {
								push_tile(:wall_1, 384 + tile_info[0] + 33);
								push_tile(:wall_2, 384 + tile_info[0] + 34);
								push_tile(:wall_3, 384 + tile_info[0] + 36);
								push_tile(:wall_4, 384 + tile_info[0] + 17);
								push_tile(:wall_6, 384 + tile_info[0] + 20);
								push_tile(:wall_7, 384 + tile_info[0] + 9);
								push_tile(:wall_8, 384 + tile_info[0] + 10);
								push_tile(:wall_9, 384 + tile_info[0] + 12);
								push_tile(:wall_in_1, 384 + tile_info[0] + 23);
								push_tile(:wall_in_3, 384 + tile_info[0] + 22);
								push_tile(:wall_in_7, 384 + tile_info[0] + 31);
								push_tile(:wall_in_9, 384 + tile_info[0] + 30);
								push_tile(:upper_wall_1, 384 + tile_info[0] + 40);
								push_tile(:upper_wall_2, 384 + tile_info[0] + 42);
								push_tile(:upper_wall_3, 384 + tile_info[0] + 45);
								push_tile(:upper_wall_4, 384 + tile_info[0] + 16);
								push_tile(:upper_wall_6, 384 + tile_info[0] + 21);
								push_tile(:upper_wall_7, 384 + tile_info[0] + 0);
								push_tile(:upper_wall_8, 384 + tile_info[0] + 2);
								push_tile(:upper_wall_9, 384 + tile_info[0] + 5);
								push_tile(:upper_wall_in_1, 384 + tile_info[0] + 7);
								push_tile(:upper_wall_in_3, 384 + tile_info[0] + 6);
								push_tile(:upper_wall_in_7, 384 + tile_info[0] + 15);
								push_tile(:upper_wall_in_9, 384 + tile_info[0] + 14);
							} else {
								push_tile(:wall_1, 384 + tile_info[0] + 25);
								push_tile(:wall_2, 384 + tile_info[0] + 26);
								push_tile(:wall_3, 384 + tile_info[0] + 27);
								push_tile(:wall_4, 384 + tile_info[0] + 17);
								push_tile(:wall_6, 384 + tile_info[0] + 19);
								push_tile(:wall_7, 384 + tile_info[0] + 9);
								push_tile(:wall_8, 384 + tile_info[0] + 10);
								push_tile(:wall_9, 384 + tile_info[0] + 11);
								push_tile(:wall_in_1, 384 + tile_info[0] + 22);
								push_tile(:wall_in_3, 384 + tile_info[0] + 21);
								push_tile(:wall_in_7, 384 + tile_info[0] + 30);
								push_tile(:wall_in_9, 384 + tile_info[0] + 29);
								push_tile(:upper_wall_1, 384 + tile_info[0] + 32);
								push_tile(:upper_wall_2, 384 + tile_info[0] + 34);
								push_tile(:upper_wall_3, 384 + tile_info[0] + 36);
								push_tile(:upper_wall_4, 384 + tile_info[0] + 16);
								push_tile(:upper_wall_6, 384 + tile_info[0] + 20);
								push_tile(:upper_wall_7, 384 + tile_info[0] + 0);
								push_tile(:upper_wall_8, 384 + tile_info[0] + 2);
								push_tile(:upper_wall_9, 384 + tile_info[0] + 4);
								push_tile(:upper_wall_in_1, 384 + tile_info[0] + 6);
								push_tile(:upper_wall_in_3, 384 + tile_info[0] + 5);
								push_tile(:upper_wall_in_7, 384 + tile_info[0] + 14);
								push_tile(:upper_wall_in_9, 384 + tile_info[0] + 13);
							}
						} else if (@large_wall_tiles) {
							push_tile(:wall_1, 384 + tile_info[0] + 24);
							push_tile(:wall_2, 384 + tile_info[0] + 25);
							push_tile(:wall_3, 384 + tile_info[0] + 27);
							push_tile(:wall_4, 384 + tile_info[0] + 8);
							push_tile(:wall_6, 384 + tile_info[0] + 11);
							push_tile(:wall_7, 384 + tile_info[0] + 0);
							push_tile(:wall_8, 384 + tile_info[0] + 1);
							push_tile(:wall_9, 384 + tile_info[0] + 3);
							push_tile(:wall_in_1, 384 + tile_info[0] + 5);
							push_tile(:wall_in_3, 384 + tile_info[0] + 4);
							push_tile(:wall_in_7, 384 + tile_info[0] + 13);
							push_tile(:wall_in_9, 384 + tile_info[0] + 12);
						} else {
							push_tile(:wall_1, 384 + tile_info[0] + 16);
							push_tile(:wall_2, 384 + tile_info[0] + 17);
							push_tile(:wall_3, 384 + tile_info[0] + 18);
							push_tile(:wall_4, 384 + tile_info[0] + 8);
							push_tile(:wall_6, 384 + tile_info[0] + 10);
							push_tile(:wall_7, 384 + tile_info[0] + 0);
							push_tile(:wall_8, 384 + tile_info[0] + 1);
							push_tile(:wall_9, 384 + tile_info[0] + 2);
							push_tile(:wall_in_1, 384 + tile_info[0] + 4);
							push_tile(:wall_in_3, 384 + tile_info[0] + 3);
							push_tile(:wall_in_7, 384 + tile_info[0] + 12);
							push_tile(:wall_in_9, 384 + tile_info[0] + 11);
						}
					}
					id = (i == 0) ? tile_info[0] * 48 : 384 + tile_info[0];
					push_tile(tile_type, id, false);
				}
			}
		}

		public void push_tile(tile_type, id, auto = true) {
			@tile_type_ids[tile_type] ||= new List<string>();
			@tile_type_ids[tile_type].Add(new {id, auto});
		}

		public bool has_flag(flag) {
			return @flags.any(f => f.downcase == flag.downcase);
		}

		public bool has_decoration(deco) {
			return @tile_type_ids.Contains(deco) && @tile_type_ids[deco].length > 0;
		}

		public void get_random_tile_of_type(tile_type, dungeon, x, y, layer) {
			tiles = @tile_type_ids[tile_type];
			if (!tiles || tiles.empty()) return 0;
			ret = tiles.sample[0];
			if (ret < 384) {   // Autotile
				nb = TileDrawingHelper.tableNeighbors(dungeon, x, y, layer);
				variant = TileDrawingHelper.NEIGHBORS_TO_AUTOTILE_INDEX[nb];
				ret += variant;
			} else {
				switch (tile_type) {
					case :void:
						if (@large_void_tiles) {
							if (x.odd()) ret += 1;
							if (y.odd()) ret += 8;
						}
						break;
					case :floor:
						if (large_floor_tiles) {
							if (x.odd()) ret += 1;
							if (y.odd()) ret += 8;
						}
						break;
					case :wall_2, :wall_8, :wall_top:
						if (@large_wall_tiles && x.odd()) ret += 1;
						break;
					case :wall_4, :wall_6:
						if (@large_wall_tiles && y.odd()) ret += 8;
						break;
				}
				// Different wall tiles for northern walls if there's another wall directly
				// north of them (i.e. tree tiles that shouldn't have shaded grass because
				// there isn't a tree-enclosed area there)
				if (@thin_north_wall_offset != 0 && new []{:wall_7, :wall_8, :wall_9}.Contains(tile_type)) {
					if (dungeon.tile_is_wall(dungeon[x, y - 1, 1])) ret += @thin_north_wall_offset;
				}
			}
			return ret;
		}

		unless (method_defined(:__orig__get_property_for_PBS)) alias __orig__get_property_for_PBS get_property_for_PBS;
		public void get_property_for_PBS(key) {
			ret = __orig__get_property_for_PBS(key);
			switch (key) {
				case "ThinNorthWallOffset":
					if (ret == 0) ret = null;
					break;
				case "Tile": case "Autotile":
					ret = new List<string>();
					@tile_type_ids.each do |tile_type, tile_ids|
						foreach (var tile in tile_ids) { //'tile_ids.each' do => |tile|
							switch (key) {
								case "Tile":
									if (!tile[1] && tile[0] >= 384) ret.Add(new {tile[0] - 384, tile_type});
									break;
								case "Autotile":
									if (!tile[1] && tile[0] < 384) ret.Add(new {tile[0] / 48, tile_type});
									break;
							}
						}
					}
					if (ret.length == 0) ret = null;
					break;
			}
			return ret;
		}
	}
}
