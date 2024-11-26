//===============================================================================
// Edits the terrain tags of tiles in tilesets.
//===============================================================================
public partial class PokemonTilesetScene {
	public const int TILE_SIZE            = 32;   // in pixels
	public const int TILES_PER_ROW        = 8;
	public const int TILESET_WIDTH        = TILES_PER_ROW * TILE_SIZE;
	public const int TILES_PER_AUTOTILE   = 48;
	public const int TILESET_START_ID     = TILES_PER_ROW * TILES_PER_AUTOTILE;
	TILE_BACKGROUND      = Color.magenta;
	CURSOR_COLOR         = new Color(255, 0, 0);   // Red
	CURSOR_OUTLINE_COLOR = Color.white;
	TEXT_COLOR           = Color.white;
	TEXT_SHADOW_COLOR    = Color.black;

	public void initialize() {
		@tilesets_data = load_data("Data/Tilesets.rxdata");
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		@sprites["title"] = Window_UnformattedTextPokemon.newWithSize(
			_INTL("Tileset Editor\nA/S: SCROLL\nZ: MENU"),
			TILESET_WIDTH, 0, Graphics.width - TILESET_WIDTH, 128, @viewport
		);
		@sprites["background"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["tileset"] = new BitmapSprite(TILESET_WIDTH, Graphics.height, @viewport);
		@sprites["tileset"].z = 10;
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["overlay"].z = 20;
		SetSystemFont(@sprites["overlay"].bitmap);
		@visible_height = @sprites["tileset"].bitmap.height / TILE_SIZE;
		load_tileset(1);
	}

	public void open_screen() {
		FadeInAndShow(@sprites);
	}

	public void close_screen() {
		FadeOutAndHide(@sprites);
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
		@tilehelper.dispose;
		if (Game.GameData.game_map && Game.GameData.map_factory) {
			Game.GameData.map_factory.setup(Game.GameData.game_map.map_id);
			Game.GameData.game_player.center(Game.GameData.game_player.x, Game.GameData.game_player.y);
			if (Game.GameData.scene.is_a(Scene_Map)) {
				Game.GameData.scene.dispose;
				Game.GameData.scene.createSpritesets;
			}
		}
	}

	public void load_tileset(id) {
		@tileset = @tilesets_data[id];
		@tilehelper&.dispose;
		@tilehelper = TileDrawingHelper.fromTileset(@tileset);
		@x = 0;
		@y = 0;
		@top_y = 0;
		@height = ((@tileset.terrain_tags.xsize - TILESET_START_ID) / TILES_PER_ROW) + 1;
		draw_tiles;
		draw_overlay;
	}

	public void choose_tileset() {
		commands = new List<string>();
		for (int i = 1; i < @tilesets_data.length; i++) { //each '@tilesets_data.length' do => |i|
			commands.Add(string.Format("{0:3} {0}", i, @tilesets_data[i].name));
		}
		ret = ShowCommands(null, commands, -1);
		if (ret >= 0) load_tileset(ret + 1);
	}

	public void draw_tiles() {
		@sprites["background"].bitmap.clear;
		@sprites["tileset"].bitmap.clear;
		for (int yy = @visible_height; yy < @visible_height; yy++) { //for '@visible_height' times do => |yy|
			autotile_row = (@top_y == 0 && yy == 0);   // Autotiles
			id_y_offset = (autotile_row) ? 0 : TILESET_START_ID + ((@top_y + yy - 1) * TILES_PER_ROW);
			if (@top_y + yy >= @height) break;
			for (int xx = TILES_PER_ROW; xx < TILES_PER_ROW; xx++) { //for 'TILES_PER_ROW' times do => |xx|
				@sprites["background"].bitmap.fill_rect(xx * TILE_SIZE, yy * TILE_SIZE, TILE_SIZE, TILE_SIZE, TILE_BACKGROUND);
				id_x_offset = (autotile_row) ? xx * TILES_PER_AUTOTILE : xx;
				@tilehelper.bltTile(@sprites["tileset"].bitmap, xx * TILE_SIZE, yy * TILE_SIZE,
														id_y_offset + id_x_offset);
			}
		}
	}

	public void draw_overlay() {
		@sprites["overlay"].bitmap.clear;
		// Draw all text over tiles (terrain tag numbers)
		textpos = new List<string>();
		for (int yy = @visible_height; yy < @visible_height; yy++) { //for '@visible_height' times do => |yy|
			for (int xx = TILES_PER_ROW; xx < TILES_PER_ROW; xx++) { //for 'TILES_PER_ROW' times do => |xx|
				tile_id = tile_ID_from_coordinates(xx, @top_y + yy);
				terr = @tileset.terrain_tags[tile_id];
				if (terr == 0) continue;
				textpos.Add(new {terr.ToString(), (xx * TILE_SIZE) + (TILE_SIZE / 2), (yy * TILE_SIZE) + 6,
											:center, TEXT_COLOR, TEXT_SHADOW_COLOR, :outline});
			}
		}
		DrawTextPositions(@sprites["overlay"].bitmap, textpos);
		// Draw cursor
		draw_cursor;
		// Draw information about selected tile on right side
		draw_tile_details;
	}

	public void draw_cursor() {
		cursor_x = @x * TILE_SIZE;
		cursor_y = (@y - @top_y) * TILE_SIZE;
		cursor_width = TILE_SIZE;
		cursor_height = TILE_SIZE;
		bitmap = @sprites["overlay"].bitmap;
		bitmap.fill_rect(cursor_x - 2,                cursor_y - 2,                 cursor_width + 4, 8,  CURSOR_OUTLINE_COLOR);
		bitmap.fill_rect(cursor_x - 2,                cursor_y - 2,                 8, cursor_height + 4, CURSOR_OUTLINE_COLOR);
		bitmap.fill_rect(cursor_x - 2,                cursor_y + cursor_height - 6, cursor_width + 4, 8,  CURSOR_OUTLINE_COLOR);
		bitmap.fill_rect(cursor_x + cursor_width - 6, cursor_y - 2,                 8, cursor_height + 4, CURSOR_OUTLINE_COLOR);
		bitmap.fill_rect(cursor_x,                    cursor_y,                     cursor_width, 4,      CURSOR_COLOR);
		bitmap.fill_rect(cursor_x,                    cursor_y,                     4, cursor_height,     CURSOR_COLOR);
		bitmap.fill_rect(cursor_x,                    cursor_y + cursor_height - 4, cursor_width, 4,      CURSOR_COLOR);
		bitmap.fill_rect(cursor_x + cursor_width - 4, cursor_y,                     4, cursor_height,     CURSOR_COLOR);
	}

	public void draw_tile_details() {
		overlay = @sprites["overlay"].bitmap;
		tile_size = 4;   // Size multiplier
		tile_x = (Graphics.width * 3 / 4) - (TILE_SIZE * tile_size / 2);
		tile_y = (Graphics.height / 2) - (TILE_SIZE * tile_size / 2);
		tile_id = tile_ID_from_coordinates(@x, @y) || 0;
		// Draw tile (at 400% size)
		@sprites["background"].bitmap.fill_rect(tile_x, tile_y, TILE_SIZE * tile_size, TILE_SIZE * tile_size, TILE_BACKGROUND);
		@tilehelper.bltSmallTile(overlay, tile_x, tile_y, TILE_SIZE * tile_size, TILE_SIZE * tile_size, tile_id);
		// Draw box around tile image
		overlay.fill_rect(tile_x - 1,                       tile_y - 1,                       (TILE_SIZE * tile_size) + 2, 1, Color.white);
		overlay.fill_rect(tile_x - 1,                       tile_y - 1,                       1, (TILE_SIZE * tile_size) + 2, Color.white);
		overlay.fill_rect(tile_x - 1,                       tile_y + (TILE_SIZE * tile_size), (TILE_SIZE * tile_size) + 2, 1, Color.white);
		overlay.fill_rect(tile_x + (TILE_SIZE * tile_size), tile_y - 1,                       1, (TILE_SIZE * tile_size) + 2, Color.white);
		// Write terrain tag info about selected tile
		terrain_tag = @tileset.terrain_tags[tile_id] || 0;
		if (GameData.TerrainTag.exists(terrain_tag)) {
			terrain_tag_name = string.Format("{0}: {0}", terrain_tag, GameData.TerrainTag.get(terrain_tag).real_name);
		} else {
			terrain_tag_name = terrain_tag.ToString();
		}
		textpos = new {
			new {_INTL("Terrain Tag:"), Graphics.width * 3 / 4, tile_y + (TILE_SIZE * tile_size) + 22,
			:center, new Color(248, 248, 248), new Color(40, 40, 40)},
			new {terrain_tag_name, Graphics.width * 3 / 4, tile_y + (TILE_SIZE * tile_size) + 54,
			:center, new Color(248, 248, 248), new Color(40, 40, 40)};
		}
		// Draw all text
		DrawTextPositions(overlay, textpos);
	}

	public void tile_ID_from_coordinates(x, y) {
		if (y == 0) return x * TILES_PER_AUTOTILE;   // Autotile
		return TILESET_START_ID + ((y - 1) * TILES_PER_ROW) + x;
	}

	public void set_terrain_tag_for_tile_ID(i, value) {
		if (i < TILESET_START_ID) {
			TILES_PER_AUTOTILE.times(j => @tileset.terrain_tags[i + j] = value);
		} else {
			@tileset.terrain_tags[i] = value;
		}
	}

	public void update_cursor_position(x_offset, y_offset) {
		old_x = @x;
		old_y = @y;
		old_top_y = @top_y;
		if (x_offset != 0) {
			@x += x_offset;
			@x = @x.clamp(0, TILES_PER_ROW - 1);
		}
		if (y_offset != 0) {
			@y += y_offset;
			@y = @y.clamp(0, @height - 1);
			if (@y < @top_y) @top_y = @y;
			if (@y >= @top_y + @visible_height) @top_y = @y - @visible_height + 1;
			if (@top_y < 0) @top_y = 0;
		}
		if (@top_y != old_top_y) draw_tiles;
		if (@x != old_x || @y != old_y) draw_overlay;
	}

	public void StartScene() {
		open_screen;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			if (Input.repeat(Input.UP)) {
				update_cursor_position(0, -1);
			} else if (Input.repeat(Input.DOWN)) {
				update_cursor_position(0, 1);
			} else if (Input.repeat(Input.LEFT)) {
				update_cursor_position(-1, 0);
			} else if (Input.repeat(Input.RIGHT)) {
				update_cursor_position(1, 0);
			} else if (Input.repeat(Input.JUMPUP)) {
				update_cursor_position(0, -@visible_height / 2);
			} else if (Input.repeat(Input.JUMPDOWN)) {
				update_cursor_position(0, @visible_height / 2);
			} else if (Input.trigger(Input.ACTION)) {
				commands = new {
					_INTL("Go to bottom"),
					_INTL("Go to top"),
					_INTL("Change tileset"),
					_INTL("Cancel");
				}
				switch (ShowCommands(null, commands, -1)) {
					case 0:
						update_cursor_position(0, 99_999);
						break;
					case 1:
						update_cursor_position(0, -99_999);
						break;
					case 2:
						choose_tileset;
						break;
				}
			} else if (Input.trigger(Input.BACK)) {
				if (ConfirmMessage(_INTL("Save changes?"))) {
					save_data(@tilesets_data, "Data/Tilesets.rxdata");
					Game.GameData.data_tilesets = @tilesets_data;
					Message(_INTL("To ensure that the changes remain, close and reopen RPG Maker XP."));
				}
				if (ConfirmMessage(_INTL("Exit from the editor?"))) break;
			} else if (Input.trigger(Input.USE)) {
				selected = tile_ID_from_coordinates(@x, @y);
				old_tag = @tileset.terrain_tags[selected];
				cmds = new List<string>();
				ids = new List<string>();
				old_idx = 0;
				foreach (var tag in GameData.TerrainTag) { //'GameData.TerrainTag.each' do => |tag|
					if (tag.id_number == old_tag) old_idx = cmds.length;
					cmds.Add($"{tag.id_number}: {tag.real_name}");
					ids.Add(tag.id_number);
				}
				val = Message("\\l[1]\\ts[]" + _INTL("Set the terrain tag."), cmds, -1, null, old_idx);
				if (val >= 0 && val != old_tag) {
					set_terrain_tag_for_tile_ID(selected, ids[val]);
					draw_overlay;
				}
			}
		}
		close_screen;
	}
}

//===============================================================================
//
//===============================================================================
public void TilesetScreen() {
	FadeOutIn do;
		Graphics.resize_screen(Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT * 2);
		SetResizeFactor(1);
		scene = new PokemonTilesetScene();
		scene.StartScene;
		Graphics.resize_screen(Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT);
		SetResizeFactor(Game.GameData.PokemonSystem.screensize);
	}
}
