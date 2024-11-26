//===============================================================================
//
//===============================================================================
public partial class Sprite_SurfBase {
	public int visible		{ get { return _visible; } }			protected int _visible;

	public void initialize(parent_sprite, viewport = null) {
		@parent_sprite = parent_sprite;
		@sprite = null;
		@viewport = viewport;
		@disposed = false;
		@surfbitmap = new AnimatedBitmap("Graphics/Characters/base_surf");
		@divebitmap = new AnimatedBitmap("Graphics/Characters/base_dive");
		RPG.Cache.retain("Graphics/Characters/base_surf");
		RPG.Cache.retain("Graphics/Characters/base_dive");
		@cws = @surfbitmap.width / 4;
		@chs = @surfbitmap.height / 4;
		@cwd = @divebitmap.width / 4;
		@chd = @divebitmap.height / 4;
		update;
	}

	public void dispose() {
		if (@disposed) return;
		@sprite&.dispose;
		@sprite = null;
		@parent_sprite = null;
		@surfbitmap.dispose;
		@divebitmap.dispose;
		@disposed = true;
	}

	public bool disposed() {
		return @disposed;
	}

	public void event() {
		return @parent_sprite.character;
	}

	public int visible { set {
		@visible = value;
		if (@sprite && !@sprite.disposed()) @sprite.visible = value;
		}
	}

	public void update() {
		if (disposed()) return;
		if (!Game.GameData.PokemonGlobal.surfing && !Game.GameData.PokemonGlobal.diving) {
			// Just-in-time disposal of sprite
			if (@sprite) {
				@sprite.dispose;
				@sprite = null;
			}
			return;
		}
		// Just-in-time creation of sprite
		if (!@sprite) @sprite = new Sprite(@viewport);
		if (!@sprite) return;
		if (Game.GameData.PokemonGlobal.surfing) {
			@sprite.bitmap = @surfbitmap.bitmap;
			cw = @cws;
			ch = @chs;
		} else if (Game.GameData.PokemonGlobal.diving) {
			@sprite.bitmap = @divebitmap.bitmap;
			cw = @cwd;
			ch = @chd;
		}
		sx = event.pattern_surf * cw;
		sy = ((event.direction - 2) / 2) * ch;
		@sprite.src_rect.set(sx, sy, cw, ch);
		if (Game.GameData.game_temp.surf_base_coords) {
			spr_x = (int)Math.Round(((Game.GameData.game_temp.surf_base_coords[0] * Game_Map.REAL_RES_X) - event.map.display_x).to_f / Game_Map.X_SUBPIXELS);
			spr_x += (Game_Map.TILE_WIDTH / 2);
			if (TilemapRenderer.ZOOM_X != 1) spr_x = ((spr_x - (Graphics.width / 2)) * TilemapRenderer.ZOOM_X) + (Graphics.width / 2);
			@sprite.x = spr_x;
			spr_y = (int)Math.Round(((Game.GameData.game_temp.surf_base_coords[1] * Game_Map.REAL_RES_Y) - event.map.display_y).to_f / Game_Map.Y_SUBPIXELS);
			spr_y += (Game_Map.TILE_HEIGHT / 2) + 16;
			if (TilemapRenderer.ZOOM_Y != 1) spr_y = ((spr_y - (Graphics.height / 2)) * TilemapRenderer.ZOOM_Y) + (Graphics.height / 2);
			@sprite.y = spr_y;
		} else {
			@sprite.x = @parent_sprite.x;
			@sprite.y = @parent_sprite.y;
		}
		@sprite.ox      = cw / 2;
		@sprite.oy      = ch - 16;   // Assume base needs offsetting
		@sprite.oy      -= event.bob_height;
		@sprite.z       = event.screen_z(ch) - 1;
		@sprite.zoom_x  = @parent_sprite.zoom_x;
		@sprite.zoom_y  = @parent_sprite.zoom_y;
		@sprite.tone    = @parent_sprite.tone;
		@sprite.color   = @parent_sprite.color;
		@sprite.opacity = @parent_sprite.opacity;
	}
}
