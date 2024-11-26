//===============================================================================
//
//===============================================================================
public partial class Sprite_Reflection {
	public int visible		{ get { return _visible; } }			protected int _visible;

	public void initialize(parent_sprite, viewport = null) {
		@parent_sprite = parent_sprite;
		@sprite = null;
		@height = 0;
		@fixedheight = false;
		if (@parent_sprite.character && @parent_sprite.character != Game.GameData.game_player &&
			System.Text.RegularExpressions.Regex.IsMatch(@parent_sprite.character.name,@"reflection\((\d+)\)",RegexOptions.IgnoreCase)) {
			@height = $~[1].ToInt() || 0;
			@fixedheight = true;
		}
		@viewport = viewport;
		@disposed = false;
		update;
	}

	public void dispose() {
		if (@disposed) return;
		@sprite&.dispose;
		@sprite = null;
		@parent_sprite = null;
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
		shouldShow = @parent_sprite.visible;
		if (!shouldShow) {
			// Just-in-time disposal of sprite
			if (@sprite) {
				@sprite.dispose;
				@sprite = null;
			}
			return;
		}
		// Just-in-time creation of sprite
		if (!@sprite) @sprite = new Sprite(@viewport);
		if (@sprite) {
			x = @parent_sprite.x - (@parent_sprite.ox * TilemapRenderer.ZOOM_X);
			y = @parent_sprite.y - (@parent_sprite.oy * TilemapRenderer.ZOOM_Y);
			if (System.Text.RegularExpressions.Regex.IsMatch(event.character_name,@"offset",RegexOptions.IgnoreCase)) y -= Game_Map.TILE_HEIGHT * TilemapRenderer.ZOOM_Y;
			if (!@fixedheight) @height = Game.GameData.PokemonGlobal.bridge;
			y += @height * TilemapRenderer.ZOOM_Y * Game_Map.TILE_HEIGHT / 2;
			width  = @parent_sprite.src_rect.width;
			height = @parent_sprite.src_rect.height;
			@sprite.x        = x + ((width / 2) * TilemapRenderer.ZOOM_X);
			@sprite.y        = y + ((height + (height / 2)) * TilemapRenderer.ZOOM_Y);
			@sprite.ox       = width / 2;
			@sprite.oy       = (height / 2) - 2;   // Hard-coded 2 pixel shift up
			@sprite.oy       -= event.bob_height * 2;
			@sprite.z        = @parent_sprite.groundY - (Graphics.height / 2);
			@sprite.z        -= 1000;   // Still water is -2000, map is 0 and above
			if (event == Game.GameData.game_player) @sprite.z        += 1;
			@sprite.zoom_x   = @parent_sprite.zoom_x;
			if (Settings.ANIMATE_REFLECTIONS && !GameData.MapMetadata.try_get(event.map_id)&.still_reflections) {
				@sprite.zoom_x   += 0.05 * @sprite.zoom_x * Math.sin(2 * Math.PI * System.uptime);
			}
			@sprite.zoom_y   = @parent_sprite.zoom_y;
			@sprite.angle    = 180.0;
			@sprite.mirror   = true;
			@sprite.bitmap   = @parent_sprite.bitmap;
			@sprite.tone     = @parent_sprite.tone;
			if (@height > 0) {
				@sprite.color   = new Color(48, 96, 160, 255);   // Dark still water
				@sprite.opacity = @parent_sprite.opacity;
				@sprite.visible = !Settings.TIME_SHADING;   // Can't time-tone a colored sprite
			} else {
				@sprite.color   = new Color(224, 224, 224, 96);
				@sprite.opacity = @parent_sprite.opacity * 3 / 4;
				@sprite.visible = true;
			}
			@sprite.src_rect = @parent_sprite.src_rect;
		}
	}
}
