//===============================================================================
// Unused.
//===============================================================================
public partial class ClippableSprite : Sprite_Character {
	public override void initialize(viewport, event, tilemap) {
		@tilemap = tilemap;
		@_src_rect = new Rect(0, 0, 0, 0);
		base.initialize(viewport, event);
	}

	public override void update() {
		base.update();
		@_src_rect = self.src_rect;
		tmright = (@tilemap.map_data.xsize * Game_Map.TILE_WIDTH) - @tilemap.ox;
		echoln $"x={self.x},ox={self.ox},tmright={tmright},tmox={@tilemap.ox}";
		if (@tilemap.ox - self.ox < -self.x) {
			// clipped on left
			diff = -self.x - @tilemap.ox + self.ox;
			self.src_rect = new Rect(@_src_rect.x + diff, @_src_rect.y,
															@_src_rect.width - diff, @_src_rect.height);
			echoln $"clipped out left: {diff} {@tilemap.ox - self.ox} {self.x}";
		} else if (tmright - self.ox < self.x) {
			// clipped on right
			diff = self.x - tmright + self.ox;
			self.src_rect = new Rect(@_src_rect.x, @_src_rect.y,
															@_src_rect.width - diff, @_src_rect.height);
			echoln $"clipped out right: {diff} {tmright + self.ox} {self.x}";
		} else {
			echoln $"-not- clipped out left: {diff} {@tilemap.ox - self.ox} {self.x}";
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class Spriteset_Map {
	public int map		{ get { return _map; } }			protected int _map;

	@@viewport0 = new Viewport(0, 0, Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT);   // Panorama
	@@viewport0.z = -100;
	@@viewport1 = new Viewport(0, 0, Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT);   // Map, events, player, fog
	@@viewport1.z = 0;
	@@viewport3 = new Viewport(0, 0, Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT);   // Flashing
	@@viewport3.z = 500;

	// For access by Spriteset_Global.
	public static void viewport() {
		return @@viewport1;
	}

	public void initialize(map = null) {
		@map = (map) ? map : Game.GameData.game_map;
		Game.GameData.scene.map_renderer.add_tileset(@map.tileset_name);
		@map.autotile_names.each(filename => Game.GameData.scene.map_renderer.add_autotile(filename));
		Game.GameData.scene.map_renderer.add_extra_autotiles(@map.tileset_id);
		@panorama = new AnimatedPlane(@@viewport0);
		@fog = new AnimatedPlane(@@viewport1);
		@fog.z = 3000;
		@character_sprites = new List<string>();
		@map.events.keys.sort.each do |i|
			sprite = new Sprite_Character(@@viewport1, @map.events[i]);
			@character_sprites.Add(sprite);
		}
		EventHandlers.trigger(:on_new_spriteset_map, self, @@viewport1);
		update;
	}

	public void dispose() {
		if (Game.GameData.scene.is_a(Scene_Map)) {
			Game.GameData.scene.map_renderer.remove_tileset(@map.tileset_name);
			@map.autotile_names.each(filename => Game.GameData.scene.map_renderer.remove_autotile(filename));
			Game.GameData.scene.map_renderer.remove_extra_autotiles(@map.tileset_id);
		}
		@panorama.dispose;
		@fog.dispose;
		@character_sprites.each(sprite => sprite.dispose);
		@panorama = null;
		@fog = null;
		@character_sprites.clear;
	}

	public void getAnimations() {
		return @usersprites;
	}

	public void restoreAnimations(anims) {
		@usersprites = anims;
	}

	public void update() {
		if (@panorama_name != @map.panorama_name || @panorama_hue != @map.panorama_hue) {
			@panorama_name = @map.panorama_name;
			@panorama_hue  = @map.panorama_hue;
			if (!@panorama.bitmap.null()) @panorama.set_panorama(null);
			if (!nil_or_empty(@panorama_name)) @panorama.set_panorama(@panorama_name, @panorama_hue);
			Graphics.frame_reset;
		}
		if (@fog_name != @map.fog_name || @fog_hue != @map.fog_hue) {
			@fog_name = @map.fog_name;
			@fog_hue = @map.fog_hue;
			if (!@fog.bitmap.null()) @fog.set_fog(null);
			if (!nil_or_empty(@fog_name)) @fog.set_fog(@fog_name, @fog_hue);
			Graphics.frame_reset;
		}
		tmox = (int)Math.Round(@map.display_x / Game_Map.X_SUBPIXELS);
		tmoy = (int)Math.Round(@map.display_y / Game_Map.Y_SUBPIXELS);
		@@viewport1.rect.set(0, 0, Graphics.width, Graphics.height);
		@@viewport1.ox = 0;
		@@viewport1.oy = 0;
		@@viewport1.ox += Game.GameData.game_screen.shake;
		@panorama.ox = tmox / 2;
		@panorama.oy = tmoy / 2;
		@fog.ox         = tmox + @map.fog_ox;
		@fog.oy         = tmoy + @map.fog_oy;
		@fog.zoom_x     = @map.fog_zoom / 100.0;
		@fog.zoom_y     = @map.fog_zoom / 100.0;
		@fog.opacity    = @map.fog_opacity;
		@fog.blend_type = @map.fog_blend_type;
		@fog.tone       = @map.fog_tone;
		@panorama.update;
		@fog.update;
		@character_sprites.each do |sprite|
			sprite.update;
		}
		@@viewport1.tone = Game.GameData.game_screen.tone;
		@@viewport3.color = Game.GameData.game_screen.flash_color;
		@@viewport1.update;
		@@viewport3.update;
	}
}
