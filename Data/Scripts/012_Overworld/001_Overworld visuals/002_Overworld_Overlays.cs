//===============================================================================
// Location signpost
//===============================================================================
public partial class LocationWindow {
	public const int APPEAR_TIME = 0.4;   // In seconds; is also the disappear time
	public const int LINGER_TIME = 1.6;   // In seconds; time during which self is fully visible

	public void initialize(name) {
		@window = new Window_AdvancedTextPokemon(name);
		@window.resizeToFit(name, Graphics.width);
		@window.x        = 0;
		@window.y        = -@window.height;
		@window.viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@window.viewport.z = 99999;
		@currentmap = Game.GameData.game_map.map_id;
		@timer_start = System.uptime;
		@delayed = !Game.GameData.game_temp.fly_destination.null();
	}

	public bool disposed() {
		return @window.disposed();
	}

	public void dispose() {
		@window.dispose;
	}

	public void update() {
		if (@window.disposed() || Game.GameData.game_temp.fly_destination) return;
		if (@delayed) {
			@timer_start = System.uptime;
			@delayed = false;
		}
		@window.update;
		if (Game.GameData.game_temp.message_window_showing || @currentmap != Game.GameData.game_map.map_id) {
			@window.dispose;
			return;
		}
		if (System.uptime - @timer_start >= APPEAR_TIME + LINGER_TIME) {
			@window.y = lerp(0, -@window.height, APPEAR_TIME, @timer_start + APPEAR_TIME + LINGER_TIME, System.uptime);
			if (@window.y + @window.height <= 0) @window.dispose;
		} else {
			@window.y = lerp(-@window.height, 0, APPEAR_TIME, @timer_start, System.uptime);
		}
	}
}

//===============================================================================
// Visibility circle in dark maps
//===============================================================================
public partial class DarknessSprite : Sprite {
	public int radius		{ get { return _radius; } }			protected int _radius;

	public override void initialize(viewport = null) {
		base.initialize(viewport);
		@darkness = new Bitmap(Graphics.width, Graphics.height);
		@radius = radiusMin;
		self.bitmap = @darkness;
		self.z      = 99998;
		refresh;
	}

	public override void dispose() {
		@darkness.dispose;
		base.dispose();
	}

	public int radiusMin { get { return 64;  } }   // Before using Flash
	public int radiusMax { get { return 176; } }   // After using Flash

	public int radius { set {
		@radius = value.round;
		refresh;
		}
	}

	public void refresh() {
		@darkness.fill_rect(0, 0, Graphics.width, Graphics.height, Color.black);
		cx = Graphics.width / 2;
		cy = Graphics.height / 2;
		cradius = @radius;
		numfades = 5;
		(1..numfades).each do |i|
			(cx - cradius..cx + cradius).each do |j|
				diff2 = (cradius * cradius) - ((j - cx) * (j - cx));
				diff = Math.sqrt(diff2);
				@darkness.fill_rect(j, cy - diff, 1, diff * 2, new Color(0, 0, 0, 255.0 * (numfades - i) / numfades));
			}
			cradius = (int)Math.Floor(cradius * 0.9);
		}
	}
}

//===============================================================================
// Light effects
//===============================================================================
public partial class LightEffect {
	public void initialize(event, viewport = null, map = null, filename = null) {
		@light = new IconSprite(0, 0, viewport);
		if (!nil_or_empty(filename) && ResolveBitmap("Graphics/Pictures/" + filename)) {
			@light.setBitmap("Graphics/Pictures/" + filename);
		} else {
			@light.setBitmap("Graphics/Pictures/LE");
		}
		@light.z = 1000;
		@event = event;
		@map = (map) ? map : Game.GameData.game_map;
		@disposed = false;
	}

	public bool disposed() {
		return @disposed;
	}

	public void dispose() {
		@light.dispose;
		@map = null;
		@event = null;
		@disposed = true;
	}

	public void update() {
		@light.update;
	}
}

//===============================================================================
//
//===============================================================================
public partial class LightEffect_Lamp : LightEffect {
	public void initialize(event, viewport = null, map = null) {
		lamp = new AnimatedBitmap("Graphics/Pictures/LE");
		@light = new Sprite(viewport);
		@light.bitmap = new Bitmap(128, 64);
		src_rect = new Rect(0, 0, 64, 64);
		@light.bitmap.blt(0, 0, lamp.bitmap, src_rect);
		@light.bitmap.blt(20, 0, lamp.bitmap, src_rect);
		@light.visible = true;
		@light.z       = 1000;
		lamp.dispose;
		@map = (map) ? map : Game.GameData.game_map;
		@event = event;
	}
}

//===============================================================================
//
//===============================================================================
public partial class LightEffect_Basic : LightEffect {
	public override void initialize(event, viewport = null, map = null, filename = null) {
		base.initialize();
		@light.ox = @light.bitmap.width / 2;
		@light.oy = @light.bitmap.height / 2;
		@light.opacity = 100;
	}

	public override void update() {
		if (!@light || !@event) return;
		base.update();
		if ((Object.const_defined(:ScreenPosHelper) rescue false)) {
			@light.x      = ScreenPosHelper.ScreenX(@event);
			@light.y      = ScreenPosHelper.ScreenY(@event) - (@event.height * Game_Map.TILE_HEIGHT / 2);
			@light.zoom_x = ScreenPosHelper.ScreenZoomX(@event);
			@light.zoom_y = @light.zoom_x;
		} else {
			@light.x = @event.screen_x;
			@light.y = @event.screen_y - (Game_Map.TILE_HEIGHT / 2);
		}
		@light.tone = Game.GameData.game_screen.tone;
	}
}

//===============================================================================
//
//===============================================================================
public partial class LightEffect_DayNight : LightEffect {
	public override void initialize(event, viewport = null, map = null, filename = null) {
		base.initialize();
		@light.ox = @light.bitmap.width / 2;
		@light.oy = @light.bitmap.height / 2;
	}

	public override void update() {
		if (!@light || !@event) return;
		base.update();
		shade = DayNight.getShade;
		if (shade >= 144) {   // If light enough, call it fully day
			shade = 255;
		} else if (shade <= 64) {   // If dark enough, call it fully night
			shade = 0;
		} else {
			shade = 255 - (255 * (144 - shade) / (144 - 64));
		}
		@light.opacity = 255 - shade;
		if (@light.opacity > 0) {
			if ((Object.const_defined(:ScreenPosHelper) rescue false)) {
				@light.x      = ScreenPosHelper.ScreenX(@event);
				@light.y      = ScreenPosHelper.ScreenY(@event) - (@event.height * Game_Map.TILE_HEIGHT / 2);
				@light.zoom_x = ScreenPosHelper.ScreenZoomX(@event);
				@light.zoom_y = ScreenPosHelper.ScreenZoomY(@event);
			} else {
				@light.x = @event.screen_x;
				@light.y = @event.screen_y - (Game_Map.TILE_HEIGHT / 2);
			}
			@light.tone.set(Game.GameData.game_screen.tone.red,
											Game.GameData.game_screen.tone.green,
											Game.GameData.game_screen.tone.blue,
											Game.GameData.game_screen.tone.gray);
		}
	}
}

//===============================================================================
//
//===============================================================================
EventHandlers.add(:on_new_spriteset_map, :add_light_effects,
	block: (spriteset, viewport) => {
		map = spriteset.map;   // Map associated with the spriteset (not necessarily the current map)
		foreach (var i in map.events) { //map.events.each_key do => |i|
			if (System.Text.RegularExpressions.Regex.IsMatch(map.events[i].name,@"^outdoorlight\((\w+)\)$",RegexOptions.IgnoreCase)) {
				filename = $~[1].ToString();
				spriteset.addUserSprite(new LightEffect_DayNight(map.events[i], viewport, map, filename));
			} else if (System.Text.RegularExpressions.Regex.IsMatch(map.events[i].name,@"^outdoorlight$",RegexOptions.IgnoreCase)) {
				spriteset.addUserSprite(new LightEffect_DayNight(map.events[i], viewport, map));
			} else if (System.Text.RegularExpressions.Regex.IsMatch(map.events[i].name,@"^light\((\w+)\)$",RegexOptions.IgnoreCase)) {
				filename = $~[1].ToString();
				spriteset.addUserSprite(new LightEffect_Basic(map.events[i], viewport, map, filename));
			} else if (System.Text.RegularExpressions.Regex.IsMatch(map.events[i].name,@"^light$",RegexOptions.IgnoreCase)) {
				spriteset.addUserSprite(new LightEffect_Basic(map.events[i], viewport, map));
			}
		}
	}
)
