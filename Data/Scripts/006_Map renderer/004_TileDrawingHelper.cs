//===============================================================================
//
//===============================================================================
public partial class TileDrawingHelper {
	public int tileset		{ get { return _tileset; } set { _tileset = value; } }			protected int _tileset;
	public int autotiles		{ get { return _autotiles; } set { _autotiles = value; } }			protected int _autotiles;

	AUTOTILE_PATTERNS = new {
		new {new {27, 28, 33, 34}, new {5, 28, 33, 34}, new {27,  6, 33, 34}, new {5,  6, 33, 34},
		new {27, 28, 33, 12}, new {5, 28, 33, 12}, new {27,  6, 33, 12}, new {5,  6, 33, 12}},
		new {new {27, 28, 11, 34}, new {5, 28, 11, 34}, new {27,  6, 11, 34}, new {5,  6, 11, 34},
		new {27, 28, 11, 12}, new {5, 28, 11, 12}, new {27,  6, 11, 12}, new {5,  6, 11, 12}},
		new {new {25, 26, 31, 32}, new {25,  6, 31, 32}, new {25, 26, 31, 12}, new {25,  6, 31, 12},
		new {15, 16, 21, 22}, new {15, 16, 21, 12}, new {15, 16, 11, 22}, new {15, 16, 11, 12}},
		new {new {29, 30, 35, 36}, new {29, 30, 11, 36}, new {5, 30, 35, 36}, new {5, 30, 11, 36},
		new {39, 40, 45, 46}, new {5, 40, 45, 46}, new {39,  6, 45, 46}, new {5,  6, 45, 46}},
		new {new {25, 30, 31, 36}, new {15, 16, 45, 46}, new {13, 14, 19, 20}, new {13, 14, 19, 12},
		new {17, 18, 23, 24}, new {17, 18, 11, 24}, new {41, 42, 47, 48}, new {5, 42, 47, 48}},
		new {new {37, 38, 43, 44}, new {37,  6, 43, 44}, new {13, 18, 19, 24}, new {13, 14, 43, 44},
		new {37, 42, 43, 48}, new {17, 18, 47, 48}, new {13, 18, 43, 48}, new {1,  2,  7,  8}}
	}

	// converts neighbors returned from tableNeighbors to tile indexes
	NEIGHBORS_TO_AUTOTILE_INDEX = new {
		46, 44, 46, 44, 43, 41, 43, 40, 46, 44, 46, 44, 43, 41, 43, 40,
		42, 32, 42, 32, 35, 19, 35, 18, 42, 32, 42, 32, 34, 17, 34, 16,
		46, 44, 46, 44, 43, 41, 43, 40, 46, 44, 46, 44, 43, 41, 43, 40,
		42, 32, 42, 32, 35, 19, 35, 18, 42, 32, 42, 32, 34, 17, 34, 16,
		45, 39, 45, 39, 33, 31, 33, 29, 45, 39, 45, 39, 33, 31, 33, 29,
		37, 27, 37, 27, 23, 15, 23, 13, 37, 27, 37, 27, 22, 11, 22,  9,
		45, 39, 45, 39, 33, 31, 33, 29, 45, 39, 45, 39, 33, 31, 33, 29,
		36, 26, 36, 26, 21,  7, 21,  5, 36, 26, 36, 26, 20,  3, 20,  1,
		46, 44, 46, 44, 43, 41, 43, 40, 46, 44, 46, 44, 43, 41, 43, 40,
		42, 32, 42, 32, 35, 19, 35, 18, 42, 32, 42, 32, 34, 17, 34, 16,
		46, 44, 46, 44, 43, 41, 43, 40, 46, 44, 46, 44, 43, 41, 43, 40,
		42, 32, 42, 32, 35, 19, 35, 18, 42, 32, 42, 32, 34, 17, 34, 16,
		45, 38, 45, 38, 33, 30, 33, 28, 45, 38, 45, 38, 33, 30, 33, 28,
		37, 25, 37, 25, 23, 14, 23, 12, 37, 25, 37, 25, 22, 10, 22,  8,
		45, 38, 45, 38, 33, 30, 33, 28, 45, 38, 45, 38, 33, 30, 33, 28,
		36, 24, 36, 24, 21,  6, 21,  4, 36, 24, 36, 24, 20,  2, 20,  0;
	}

	public static void tableNeighbors(data, x, y, layer = null) {
		if (x < 0 || x >= data.xsize) return 0;
		if (y < 0 || y >= data.ysize) return 0;
		if (layer.null()) {
			t = data[x, y];
		} else {
			t = data[x, y, layer];
		}
		xp1 = (int)Math.Min(x + 1, data.xsize - 1);
		yp1 = (int)Math.Min(y + 1, data.ysize - 1);
		xm1 = (int)Math.Max(x - 1, 0);
		ym1 = (int)Math.Max(y - 1, 0);
		i = 0;
		if (layer.null()) {
			if (data[  x, ym1] == t) i |= 0x01;   // N
			if (data[xp1, ym1] == t) i |= 0x02;   // NE
			if (data[xp1,   y] == t) i |= 0x04;   // E
			if (data[xp1, yp1] == t) i |= 0x08;   // SE
			if (data[  x, yp1] == t) i |= 0x10;   // S
			if (data[xm1, yp1] == t) i |= 0x20;   // SW
			if (data[xm1,   y] == t) i |= 0x40;   // W
			if (data[xm1, ym1] == t) i |= 0x80;   // NW
		} else {
			if (data[  x, ym1, layer] == t) i |= 0x01;   // N
			if (data[xp1, ym1, layer] == t) i |= 0x02;   // NE
			if (data[xp1,   y, layer] == t) i |= 0x04;   // E
			if (data[xp1, yp1, layer] == t) i |= 0x08;   // SE
			if (data[  x, yp1, layer] == t) i |= 0x10;   // S
			if (data[xm1, yp1, layer] == t) i |= 0x20;   // SW
			if (data[xm1,   y, layer] == t) i |= 0x40;   // W
			if (data[xm1, ym1, layer] == t) i |= 0x80;   // NW
		}
		return i;
	}

	public static void fromTileset(tileset) {
		bmtileset = GetTileset(tileset.tileset_name);
		bmautotiles = new List<string>();
		for (int i = 7; i < 7; i++) { //for '7' times do => |i|
			bmautotiles.Add(GetAutotile(tileset.autotile_names[i]));
		}
		return new self(bmtileset, bmautotiles);
	}

	//-----------------------------------------------------------------------------

	public void initialize(tileset, autotiles) {
		if (tileset.mega()) {
			@tileset = TilemapRenderer.TilesetWrapper.wrapTileset(tileset);
			tileset.dispose;
			@shouldWrap = true;
		} else {
			@tileset = tileset;
			@shouldWrap = false;
		}
		@autotiles = autotiles;
	}

	public void dispose() {
		@tileset&.dispose;
		@tileset = null;
		@autotiles.each_with_index do |autotile, i|
			autotile.dispose;
			@autotiles[i] = null;
		}
	}

	public void bltSmallAutotile(bitmap, x, y, cxTile, cyTile, id, frame) {
		if (id >= 384 || frame < 0 || !@autotiles) return;
		autotile = @autotiles[(id / 48) - 1];
		if (!autotile || autotile.disposed()) return;
		cxTile = (int)Math.Max(cxTile / 2, 1);
		cyTile = (int)Math.Max(cyTile / 2, 1);
		if (autotile.height == 32) {
			anim = frame * 32;
			src_rect = new Rect(anim, 0, 32, 32);
			bitmap.stretch_blt(new Rect(x, y, cxTile * 2, cyTile * 2), autotile, src_rect);
		} else {
			anim = frame * 96;
			id %= 48;
			tiles = AUTOTILE_PATTERNS[id >> 3][id & 7];
			src = new Rect(0, 0, 0, 0);
			for (int i = 4; i < 4; i++) { //for '4' times do => |i|
				tile_position = tiles[i] - 1;
				src.set(((tile_position % 6) * 16) + anim, (tile_position / 6) * 16, 16, 16);
				bitmap.stretch_blt(new Rect((i % 2 * cxTile) + x, (i / 2 * cyTile) + y, cxTile, cyTile),
													autotile, src);
			}
		}
	}

	public void bltSmallRegularTile(bitmap, x, y, cxTile, cyTile, id) {
		if (id < 384 || !@tileset || @tileset.disposed()) return;
		rect = new Rect(((id - 384) % 8) * 32, ((id - 384) / 8) * 32, 32, 32);
		if (@shouldWrap) rect = TilemapRenderer.TilesetWrapper.getWrappedRect(rect);
		bitmap.stretch_blt(new Rect(x, y, cxTile, cyTile), @tileset, rect);
	}

	public void bltSmallTile(bitmap, x, y, cxTile, cyTile, id, frame = 0) {
		if (id >= 384) {
			bltSmallRegularTile(bitmap, x, y, cxTile, cyTile, id);
		} else if (id > 0) {
			bltSmallAutotile(bitmap, x, y, cxTile, cyTile, id, frame);
		}
	}

	public void bltAutotile(bitmap, x, y, id, frame) {
		bltSmallAutotile(bitmap, x, y, 32, 32, id, frame);
	}

	public void bltRegularTile(bitmap, x, y, id) {
		bltSmallRegularTile(bitmap, x, y, 32, 32, id);
	}

	public void bltTile(bitmap, x, y, id, frame = 0) {
		if (id >= 384) {
			bltRegularTile(bitmap, x, y, id);
		} else if (id > 0) {
			bltAutotile(bitmap, x, y, id, frame);
		}
	}
}

//===============================================================================
//
//===============================================================================
public void createMinimap(mapid) {
	map = load_data(string.Format("Data/Map{0:3}.rxdata", mapid)) rescue null;
	if (!map) return new Bitmap(32, 32);
	bitmap = new Bitmap(map.width * 4, map.height * 4);
	black = Color.black;
	tilesets = Game.GameData.data_tilesets;
	tileset = tilesets[map.tileset_id];
	if (!tileset) return bitmap;
	helper = TileDrawingHelper.fromTileset(tileset);
	for (int y = map.height; y < map.height; y++) { //for 'map.height' times do => |y|
		for (int x = map.width; x < map.width; x++) { //for 'map.width' times do => |x|
			for (int z = 3; z < 3; z++) { //for '3' times do => |z|
				id = map.data[x, y, z];
				if (!id) id = 0;
				helper.bltSmallTile(bitmap, x * 4, y * 4, 4, 4, id);
			}
		}
	}
	bitmap.fill_rect(0, 0, bitmap.width, 1, black);
	bitmap.fill_rect(0, bitmap.height - 1, bitmap.width, 1, black);
	bitmap.fill_rect(0, 0, 1, bitmap.height, black);
	bitmap.fill_rect(bitmap.width - 1, 0, 1, bitmap.height, black);
	return bitmap;
}

public void bltMinimapAutotile(dstBitmap, x, y, srcBitmap, id) {
	if (id >= 48 || !srcBitmap || srcBitmap.disposed()) return;
	anim = 0;
	cxTile = 3;
	cyTile = 3;
	tiles = TileDrawingHelper.AUTOTILE_PATTERNS[id >> 3][id & 7];
	src = new Rect(0, 0, 0, 0);
	for (int i = 4; i < 4; i++) { //for '4' times do => |i|
		tile_position = tiles[i] - 1;
		src.set((tile_position % 6 * cxTile) + anim,
						tile_position / 6 * cyTile, cxTile, cyTile);
		dstBitmap.blt((i % 2 * cxTile) + x, (i / 2 * cyTile) + y, srcBitmap, src);
	}
}

public bool passable(passages, tile_id) {
	if (tile_id.null()) return false;
	passage = passages[tile_id];
	return (passage && passage < 15);
}

// Unused
public void getPassabilityMinimap(mapid) {
	map = load_data(string.Format("Data/Map{0:3}.rxdata", mapid));
	tileset = Game.GameData.data_tilesets[map.tileset_id];
	minimap = new AnimatedBitmap("Graphics/UI/minimap_tiles");
	ret = new Bitmap(map.width * 6, map.height * 6);
	passtable = new Table(map.width, map.height);
	passages = tileset.passages;
	for (int i = map.width; i < map.width; i++) { //for 'map.width' times do => |i|
		for (int j = map.height; j < map.height; j++) { //for 'map.height' times do => |j|
			pass = true;
			new {2, 1, 0}.each do |z|
				if (!passable(passages, map.data[i, j, z])) {
					pass = false;
					break;
				}
			}
			passtable[i, j] = pass ? 1 : 0;
		}
	}
	neighbors = TileDrawingHelper.NEIGHBORS_TO_AUTOTILE_INDEX;
	for (int i = map.width; i < map.width; i++) { //for 'map.width' times do => |i|
		for (int j = map.height; j < map.height; j++) { //for 'map.height' times do => |j|
			if (passtable[i, j] != 0) continue;
			nb = TileDrawingHelper.tableNeighbors(passtable, i, j);
			tile = neighbors[nb];
			bltMinimapAutotile(ret, i * 6, j * 6, minimap.bitmap, tile);
		}
	}
	minimap.dispose;
	return ret;
}
