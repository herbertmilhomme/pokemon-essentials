//===============================================================================
// All weather particles are assumed to start at the top/right and move to the
// bottom/left. Particles are only reset if they are off-screen to the left or
// bottom.
//===============================================================================
public static partial class RPG {
	public partial class Weather {
		public int type		{ get { return _type; } }			protected int _type;
		public int max		{ get { return _max; } }			protected int _max;
		public int ox		{ get { return _ox; } set { _ox = value; } }			protected int _ox;
		public int oy		{ get { return _oy; } }			protected int _oy;
		public int ox_offset		{ get { return _ox_offset; } set { _ox_offset = value; } }			protected int _ox_offset;
		public int oy_offset		{ get { return _oy_offset; } set { _oy_offset = value; } }			protected int _oy_offset;

		public const int MAX_SPRITES              = 60;
		public const int FADE_OLD_TILES_START     = 0;
		public const int FADE_OLD_TILES_END       = 1;
		public const int FADE_OLD_TONE_START      = 0;
		public const int FADE_OLD_TONE_END        = 2;
		public const int FADE_OLD_PARTICLES_START = 1;
		public const int FADE_OLD_PARTICLES_END   = 3;
		public const int FADE_NEW_PARTICLES_START = 2;
		public const int FADE_NEW_PARTICLES_END   = 4;
		public const int FADE_NEW_TONE_START      = 3;   // Shouldn't be sooner than FADE_OLD_TONE_END + 1
		public const int FADE_NEW_TONE_END        = 5;
		public const int FADE_NEW_TILES_START     = 4;   // Shouldn't be sooner than FADE_OLD_TILES_END
		public const int FADE_NEW_TILES_END       = 5;

		public void initialize(viewport = null) {
			@viewport         = new Viewport(0, 0, Graphics.width, Graphics.height);
			@viewport.z       = viewport.z + 1;
			@origViewport     = viewport;
			// [array of particle bitmaps, array of tile bitmaps,
			//  +x per second (particle), +y per second (particle), +opacity per second (particle),
			//  +x per second (tile), +y per second (tile)]
			@weatherTypes = new List<string>();
			@type                 = :None;
			@max                  = 0;
			@ox                   = 0;
			@oy                   = 0;
			@ox_offset            = 0;
			@oy_offset            = 0;
			@tiles_wide           = 0;
			@tiles_tall           = 0;
			@tile_x               = 0.0;
			@tile_y               = 0.0;
			@sun_magnitude        = 0;   // +/- maximum addition to sun tone
			@sun_strength         = 0;   // Current addition to sun tone (0 to @sun_magnitude)
			@time_until_flash     = 0;
			@sprites              = new List<string>();
			@sprite_lifetimes     = new List<string>();
			@tiles                = new List<string>();
			@new_sprites          = new List<string>();
			@new_sprite_lifetimes = new List<string>();
			@fading               = false;
		}

		public void dispose() {
			@sprites.each(sprite => sprite&.dispose);
			@new_sprites.each(sprite => sprite&.dispose);
			@tiles.each(sprite => sprite&.dispose);
			@viewport.dispose;
			@weatherTypes.each_value do |weather|
				if (!weather) continue;
				weather[1].each(bitmap => bitmap&.dispose);
				weather[2].each(bitmap => bitmap&.dispose);
			}
		}

		public void fade_in(new_type, new_max, duration = 1) {
			if (@fading) return;
			new_type = GameData.Weather.get(new_type).id;
			if (new_type == types.None) new_max = 0;
			if (@type == new_type && @max == new_max) return;
			if (duration > 0) {
				@target_type = new_type;
				@target_max = new_max;
				prepare_bitmaps(@target_type);
				@old_max = @max;
				@new_max = 0;   // Current number of new particles
				@old_tone = new Tone(@viewport.tone.red, @viewport.tone.green,
														@viewport.tone.blue, @viewport.tone.gray);
				@target_tone = get_weather_tone(@target_type, @target_max);
				@fade_time = 0.0;
				@time_shift = 0;
				if (@type == types.None) {
					@time_shift += 2;   // No previous weather to fade out first
				} else if (!GameData.Weather.get(@type).has_tiles()) {
					@time_shift += 1;   // No previous tiles to fade out first
				}
				@fading = true;
				@new_sprites.each(sprite => sprite&.dispose);
				@new_sprites.clear;
				ensureSprites;
				@new_sprites.each_with_index((sprite, i) => set_sprite_bitmap(sprite, i, @target_type));
			} else {
				self.type = new_type;
				self.max = new_max;
			}
		}

		public int type { set {
			type = GameData.Weather.get(type).id;
			if (@type == type) return;
			if (@fading) {
				@max = @target_max;
				@fading = false;
			}
			@type = type;
			prepare_bitmaps(@type);
			if (GameData.Weather.get(@type).has_tiles()) {
				w = @weatherTypes[@type][2][0].width;
				h = @weatherTypes[@type][2][0].height;
				@tiles_wide = (Graphics.width.to_f / w).ceil + 1;
				@tiles_tall = (Graphics.height.to_f / h).ceil + 1;
			} else {
				@tiles_wide = @tiles_tall = 0;
			}
			ensureSprites;
			@sprites.each_with_index((sprite, i) => set_sprite_bitmap(sprite, i, @type));
			ensureTiles;
			@tiles.each_with_index((sprite, i) => set_tile_bitmap(sprite, i, @type));
			}
		}

		public int max { set {
			if (@max == value) return;
			@max = value.clamp(0, MAX_SPRITES);
			ensureSprites;
			for (int i = MAX_SPRITES; i < MAX_SPRITES; i++) { //for 'MAX_SPRITES' times do => |i|
				if (@sprites[i]) @sprites[i].visible = (i < @max);
			}
			}
		}

		public int ox { set {
			if (value == @ox) return;
			@ox = value;
			@sprites.each { |sprite| if (sprite) sprite.ox = @ox + @ox_offset; 	};
			@new_sprites.each(sprite => { if (sprite) sprite.ox = @ox + @ox_offset; });
			@tiles.each(sprite => { if (sprite) sprite.ox = @ox + @ox_offset; });
			}
		}

		public int oy { set {
			if (value == @oy) return;
			@oy = value;
			@sprites.each { |sprite| if (sprite) sprite.oy = @oy + @oy_offset; 	};
			@new_sprites.each(sprite => { if (sprite) sprite.oy = @oy + @oy_offset; });
			@tiles.each(sprite => { if (sprite) sprite.oy = @oy + @oy_offset; });
			}
		}

		public void get_weather_tone(weather_type, maximum) {
			return GameData.Weather.get(weather_type).tone(maximum);
		}

		public void prepare_bitmaps(new_type) {
			weather_data = GameData.Weather.get(new_type);
			bitmap_names = weather_data.graphics;
			@weatherTypes[new_type] = new {weather_data, new List<string>(), new List<string>()};
			for (int i = 2; i < 2; i++) { //for '2' times do => |i|   // 0=particles, 1=tiles
				if (!bitmap_names[i]) continue;
				foreach (var name in bitmap_names[i]) { //'bitmap_names[i].each' do => |name|
					bitmap = RPG.Cache.load_bitmap("Graphics/Weather/", name);
					@weatherTypes[new_type][i + 1].Add(bitmap);
				}
			}
		}

		public void ensureSprites() {
			if (@sprites.length < MAX_SPRITES && @weatherTypes[@type] && @weatherTypes[@type][1].length > 0) {
				for (int i = MAX_SPRITES; i < MAX_SPRITES; i++) { //for 'MAX_SPRITES' times do => |i|
					if (!@sprites[i]) {
						sprite = new FloatSprite(@origViewport);
						sprite.z       = 1000;
						sprite.ox      = @ox + @ox_offset;
						sprite.oy      = @oy + @oy_offset;
						sprite.opacity = 0;
						@sprites[i] = sprite;
					}
					@sprites[i].visible = (i < @max);
					@sprite_lifetimes[i] = 0;
				}
			}
			if (@fading && @new_sprites.length < MAX_SPRITES && @weatherTypes[@target_type] &&
				@weatherTypes[@target_type][1].length > 0) {
				for (int i = MAX_SPRITES; i < MAX_SPRITES; i++) { //for 'MAX_SPRITES' times do => |i|
					if (!@new_sprites[i]) {
						sprite = new FloatSprite(@origViewport);
						sprite.z       = 1000;
						sprite.ox      = @ox + @ox_offset;
						sprite.oy      = @oy + @oy_offset;
						sprite.opacity = 0;
						@new_sprites[i] = sprite;
					}
					@new_sprites[i].visible = (i < @new_max);
					@new_sprite_lifetimes[i] = 0;
				}
			}
		}

		public void ensureTiles() {
			if (@tiles.length >= @tiles_wide * @tiles_tall) return;
			for (int i = (@tiles_wide * @tiles_tall); i < (@tiles_wide * @tiles_tall); i++) { //for '(@tiles_wide * @tiles_tall)' times do => |i|
				if (!@tiles[i]) {
					sprite = new FloatSprite(@origViewport);
					sprite.z       = 1000;
					sprite.ox      = @ox + @ox_offset;
					sprite.oy      = @oy + @oy_offset;
					sprite.opacity = 0;
					@tiles[i] = sprite;
				}
				@tiles[i].visible = true;
			}
		}

		public void set_sprite_bitmap(sprite, index, weather_type) {
			if (!sprite) return;
			weatherBitmaps = (@weatherTypes[weather_type]) ? @weatherTypes[weather_type][1] : null;
			if (!weatherBitmaps || weatherBitmaps.length == 0) {
				sprite.bitmap = null;
				return;
			}
			if (@weatherTypes[weather_type][0].category == :Rain) {
				last_index = weatherBitmaps.length - 1;   // Last sprite is a splash
				if (index.even()) {
					sprite.bitmap = weatherBitmaps[index % last_index];
				} else {
					sprite.bitmap = weatherBitmaps[last_index];
				}
			} else {
				sprite.bitmap = weatherBitmaps[index % weatherBitmaps.length];
			}
		}

		public void set_tile_bitmap(sprite, index, weather_type) {
			if (!sprite || !weather_type) return;
			weatherBitmaps = (@weatherTypes[weather_type]) ? @weatherTypes[weather_type][2] : null;
			if (weatherBitmaps && weatherBitmaps.length > 0) {
				sprite.bitmap = weatherBitmaps[index % weatherBitmaps.length];
			} else {
				sprite.bitmap = null;
			}
		}

		public void reset_sprite_position(sprite, index, is_new_sprite = false) {
			weather_type = (is_new_sprite) ? @target_type : @type;
			lifetimes = (is_new_sprite) ? @new_sprite_lifetimes : @sprite_lifetimes;
			if (index < (is_new_sprite ? @new_max : @max)) {
				sprite.visible = true;
			} else {
				sprite.visible = false;
				lifetimes[index] = 0;
				return;
			}
			if (@weatherTypes[weather_type][0].category == :Rain && index.odd()) {   // Splash
				sprite.x = @ox + @ox_offset - sprite.bitmap.width + rand(Graphics.width + (sprite.bitmap.width * 2));
				sprite.y = @oy + @oy_offset - sprite.bitmap.height + rand(Graphics.height + (sprite.bitmap.height * 2));
				lifetimes[index] = (rand(30...50)) * 0.01;   // 0.3-0.5 seconds
			} else {
				x_speed = @weatherTypes[weather_type][0].particle_delta_x;
				y_speed = @weatherTypes[weather_type][0].particle_delta_y;
				gradient = x_speed.to_f / y_speed;
				if (gradient.abs >= 1) {
					// Position sprite to the right of the screen
					sprite.x = @ox + @ox_offset + Graphics.width + rand(Graphics.width);
					sprite.y = @oy + @oy_offset + Graphics.height - rand(Graphics.height + sprite.bitmap.height - (Graphics.width / gradient));
					distance_to_cover = sprite.x - @ox - @ox_offset - (Graphics.width / 2) + sprite.bitmap.width + rand(Graphics.width * 8 / 5);
					lifetimes[index] = (distance_to_cover.to_f / x_speed).abs;
				} else {
					// Position sprite to the top of the screen
					sprite.x = @ox + @ox_offset - sprite.bitmap.width + rand(Graphics.width + sprite.bitmap.width - (gradient * Graphics.height));
					sprite.y = @oy + @oy_offset - sprite.bitmap.height - rand(Graphics.height);
					distance_to_cover = @oy + @oy_offset - sprite.y + (Graphics.height / 2) + rand(Graphics.height * 8 / 5);
					lifetimes[index] = (distance_to_cover.to_f / y_speed).abs;
				}
			}
			sprite.opacity = 255;
		}

		public void update_sprite_position(sprite, index, is_new_sprite = false) {
			if (!sprite || !sprite.bitmap || !sprite.visible) return;
			delta_t = Graphics.delta;
			lifetimes = (is_new_sprite) ? @new_sprite_lifetimes : @sprite_lifetimes;
			if (lifetimes[index] >= 0) {
				lifetimes[index] -= delta_t;
				if (lifetimes[index] <= 0) {
					reset_sprite_position(sprite, index, is_new_sprite);
					return;
				}
			}
			// Determine which weather type this sprite is representing
			weather_type = (is_new_sprite) ? @target_type : @type;
			// Update visibility/position/opacity of sprite
			if (@weatherTypes[weather_type][0].category == :Rain && index.odd()) {   // Splash
				sprite.opacity = (lifetimes[index] < 0.2) ? 255 : 0;   // 0.2 seconds
			} else {
				dist_x = @weatherTypes[weather_type][0].particle_delta_x * delta_t;
				dist_y = @weatherTypes[weather_type][0].particle_delta_y * delta_t;
				sprite.x += dist_x;
				sprite.y += dist_y;
				if (weather_type == types.Snow) {
					sprite.x += dist_x * (sprite.y - @oy - @oy_offset) / (Graphics.height * 3);   // Faster when further down screen
					sprite.x += new {2, 1, 0, -1}[rand(4)] * dist_x / 8;   // Random movement
					sprite.y += new {2, 1, 1, 0, 0, -1}[index % 6] * dist_y / 10;   // Variety
				}
				if (sprite.x - @ox - @ox_offset > Graphics.width) sprite.x -= Graphics.width;
				if (sprite.x - @ox - @ox_offset < -sprite.width) sprite.x += Graphics.width;
				if (sprite.y - @oy - @oy_offset > Graphics.height) sprite.y -= Graphics.height;
				if (sprite.y - @oy - @oy_offset < -sprite.height) sprite.y += Graphics.height;
				sprite.opacity += @weatherTypes[weather_type][0].particle_delta_opacity * delta_t;
				x = sprite.x - @ox - @ox_offset;
				y = sprite.y - @oy - @oy_offset;
				// Check if sprite is off-screen; if so, reset it
				if (sprite.opacity < 64 || x < -sprite.bitmap.width || y > Graphics.height) {
					reset_sprite_position(sprite, index, is_new_sprite);
				}
			}
		}

		public void recalculate_tile_positions() {
			delta_t = Graphics.delta;
			weather_type = @type;
			if (@fading && @fade_time >= (int)Math.Max(FADE_OLD_TONE_END - @time_shift, 0)) {
				weather_type = @target_type;
			}
			@tile_x += @weatherTypes[weather_type][0].tile_delta_x * delta_t;
			@tile_y += @weatherTypes[weather_type][0].tile_delta_y * delta_t;
			while (@tile_x < @ox + @ox_offset - @weatherTypes[weather_type][2][0].width) {
				@tile_x += @weatherTypes[weather_type][2][0].width;
			}
			while (@tile_x > @ox + @ox_offset) {
				@tile_x -= @weatherTypes[weather_type][2][0].width;
			}
			while (@tile_y < @oy + @oy_offset - @weatherTypes[weather_type][2][0].height) {
				@tile_y += @weatherTypes[weather_type][2][0].height;
			}
			while (@tile_y > @oy + @oy_offset) {
				@tile_y -= @weatherTypes[weather_type][2][0].height;
			}
		}

		public void update_tile_position(sprite, index) {
			if (!sprite || !sprite.bitmap || !sprite.visible) return;
			sprite.x = @tile_x.round + ((index % @tiles_wide) * sprite.bitmap.width);
			sprite.y = @tile_y.round + ((index / @tiles_wide) * sprite.bitmap.height);
			if (sprite.x - @ox - @ox_offset < -sprite.bitmap.width) sprite.x += @tiles_wide * sprite.bitmap.width;
			if (sprite.y - @oy - @oy_offset > Graphics.height) sprite.y -= @tiles_tall * sprite.bitmap.height;
			sprite.visible = true;
			if (@fading && @type != @target_type) {
				if (@fade_time >= FADE_OLD_TILES_START && @fade_time < FADE_OLD_TILES_END &&
					@time_shift == 0) {   // There were old tiles to fade out
					fraction = (@fade_time - (int)Math.Max(FADE_OLD_TILES_START - @time_shift, 0)) / (FADE_OLD_TILES_END - FADE_OLD_TILES_START);
					sprite.opacity = 255 * (1 - fraction);
				} else if (@fade_time >= (int)Math.Max(FADE_NEW_TILES_START - @time_shift, 0) &&
							@fade_time < (int)Math.Max(FADE_NEW_TILES_END - @time_shift, 0)) {
					fraction = (@fade_time - (int)Math.Max(FADE_NEW_TILES_START - @time_shift, 0)) / (FADE_NEW_TILES_END - FADE_NEW_TILES_START);
					sprite.opacity = 255 * fraction;
				} else {
					sprite.opacity = 0;
				}
			} else {
				sprite.opacity = (@max > 0) ? 255 : 0;
			}
		}

		// Set tone of viewport (general screen brightening/darkening)
		public void update_screen_tone() {
			weather_type = @type;
			weather_max = @max;
			fraction = 1;
			tone_red = 0;
			tone_green = 0;
			tone_blue = 0;
			tone_gray = 0;
			// Get base tone
			if (@fading) {
				if (@type == @target_type) {   // Just changing max
					if (@fade_time >= (int)Math.Max(FADE_NEW_TONE_START - @time_shift, 0) &&
						@fade_time < (int)Math.Max(FADE_NEW_TONE_END - @time_shift, 0)) {
						weather_max = @target_max;
						fract = (@fade_time - (int)Math.Max(FADE_NEW_TONE_START - @time_shift, 0)) / (FADE_NEW_TONE_END - FADE_NEW_TONE_START);
						tone_red = @target_tone.red + ((1 - fract) * (@old_tone.red - @target_tone.red));
						tone_green = @target_tone.green + ((1 - fract) * (@old_tone.green - @target_tone.green));
						tone_blue = @target_tone.blue + ((1 - fract) * (@old_tone.blue - @target_tone.blue));
						tone_gray = @target_tone.gray + ((1 - fract) * (@old_tone.gray - @target_tone.gray));
					} else {
						tone_red = @viewport.tone.red;
						tone_green = @viewport.tone.green;
						tone_blue = @viewport.tone.blue;
						tone_gray = @viewport.tone.gray;
					}
				} else if (@time_shift < 2 && @fade_time >= FADE_OLD_TONE_START && @fade_time < FADE_OLD_TONE_END) {
					weather_max = @old_max;
					fraction = ((@fade_time - FADE_OLD_TONE_START) / (FADE_OLD_TONE_END - FADE_OLD_TONE_START)).clamp(0, 1);
					fraction = 1 - fraction;
					tone_red = @old_tone.red;
					tone_green = @old_tone.green;
					tone_blue = @old_tone.blue;
					tone_gray = @old_tone.gray;
				} else if (@fade_time >= (int)Math.Max(FADE_NEW_TONE_START - @time_shift, 0)) {
					weather_type = @target_type;
					weather_max = @target_max;
					fraction = ((@fade_time - (int)Math.Max(FADE_NEW_TONE_START - @time_shift, 0)) / (FADE_NEW_TONE_END - FADE_NEW_TONE_START)).clamp(0, 1);
					tone_red = @target_tone.red;
					tone_green = @target_tone.green;
					tone_blue = @target_tone.blue;
					tone_gray = @target_tone.gray;
				}
			} else {
				base_tone = get_weather_tone(weather_type, weather_max);
				tone_red = base_tone.red;
				tone_green = base_tone.green;
				tone_blue = base_tone.blue;
				tone_gray = base_tone.gray;
			}
			// Modify base tone
			if (weather_type == types.Sun) {
				if (@sun_magnitude != weather_max && @sun_magnitude != -weather_max) @sun_magnitude = weather_max;
				if ((@sun_magnitude > 0 && @sun_strength > @sun_magnitude) ||
																(@sun_magnitude < 0 && @sun_strength < 0)) @sun_magnitude *= -1;
				@sun_strength += @sun_magnitude.to_f * Graphics.delta / 0.8;   // 0.8 seconds per half flash
				tone_red += @sun_strength;
				tone_green += @sun_strength;
				tone_blue += @sun_strength / 2;
			}
			// Apply screen tone
			@viewport.tone.set(tone_red * fraction, tone_green * fraction,
												tone_blue * fraction, tone_gray * fraction);
		}

		public void update_fading() {
			if (!@fading) return;
			old_fade_time = @fade_time;
			@fade_time += Graphics.delta;
			// Change tile bitmaps
			if (@type != @target_type) {
				tile_change_threshold = (int)Math.Max(FADE_OLD_TONE_END - @time_shift, 0);
				if (old_fade_time <= tile_change_threshold && @fade_time > tile_change_threshold) {
					@tile_x = @tile_y = 0.0;
					if (@weatherTypes[@target_type] && @weatherTypes[@target_type][2].length > 0) {
						w = @weatherTypes[@target_type][2][0].width;
						h = @weatherTypes[@target_type][2][0].height;
						@tiles_wide = (Graphics.width.to_f / w).ceil + 1;
						@tiles_tall = (Graphics.height.to_f / h).ceil + 1;
						ensureTiles;
						@tiles.each_with_index((sprite, i) => set_tile_bitmap(sprite, i, @target_type));
					} else {
						@tiles_wide = @tiles_tall = 0;
					}
				}
			}
			// Reduce the number of old weather particles
			if (@max > 0 && @fade_time >= (int)Math.Max(FADE_OLD_PARTICLES_START - @time_shift, 0)) {
				fraction = (@fade_time - (int)Math.Max(FADE_OLD_PARTICLES_START - @time_shift, 0)) / (FADE_OLD_PARTICLES_END - FADE_OLD_PARTICLES_START);
				@max = @old_max * (1 - fraction);
				// NOTE: Sprite visibilities aren't updated here; a sprite is allowed to
				//       die off naturally in def reset_sprite_position.
			}
			// Increase the number of new weather particles
			if (@new_max < @target_max && @fade_time >= (int)Math.Max(FADE_NEW_PARTICLES_START - @time_shift, 0)) {
				fraction = (@fade_time - (int)Math.Max(FADE_NEW_PARTICLES_START - @time_shift, 0)) / (FADE_NEW_PARTICLES_END - FADE_NEW_PARTICLES_START);
				@new_max = (int)Math.Floor(@target_max * fraction);
				@new_sprites.each_with_index((sprite, i) => { if (sprite) sprite.visible = (i < @new_max); });
			}
			// End fading
			if (@fade_time >= ((@target_type == types.None) ? FADE_OLD_PARTICLES_END : FADE_NEW_TILES_END) - @time_shift &&
				@sprites.none(sprite => sprite.visible)) {
				@type                 = @target_type;
				@max                  = @target_max;
				@target_type          = null;
				@target_max           = null;
				@old_max              = null;
				@new_max              = null;
				@old_tone             = null;
				@target_tone          = null;
				@fade_time            = 0.0;
				@time_shift           = 0;
				@sprites.each(sprite => sprite&.dispose);
				@sprites              = @new_sprites;
				@new_sprites          = new List<string>();
				@sprite_lifetimes     = @new_sprite_lifetimes;
				@new_sprite_lifetimes = new List<string>();
				@fading               = false;
			}
		}

		public void update() {
			update_fading;
			update_screen_tone;
			// Storm flashes
			if (@type == types.Storm && !@fading) {
				if (@time_until_flash > 0) {
					@time_until_flash -= Graphics.delta;
					if (@time_until_flash <= 0) {
						@viewport.flash(new Color(255, 255, 255, 230), rand(2..4) * 20);
					}
				}
				if (@time_until_flash <= 0) {
					@time_until_flash = rand(1..12) * 0.5;   // 0.5-6 seconds
				}
			}
			@viewport.update;
			// Update weather particles (raindrops, snowflakes, etc.)
			if (@weatherTypes[@type] && @weatherTypes[@type][1].length > 0) {
				ensureSprites;
				for (int i = MAX_SPRITES; i < MAX_SPRITES; i++) { //for 'MAX_SPRITES' times do => |i|
					update_sprite_position(@sprites[i], i, false);
				}
			} else if (@sprites.length > 0) {
				@sprites.each(sprite => sprite&.dispose);
				@sprites.clear;
			}
			// Update new weather particles (while fading in only)
			if (@fading && @weatherTypes[@target_type] && @weatherTypes[@target_type][1].length > 0) {
				ensureSprites;
				for (int i = MAX_SPRITES; i < MAX_SPRITES; i++) { //for 'MAX_SPRITES' times do => |i|
					update_sprite_position(@new_sprites[i], i, true);
				}
			} else if (@new_sprites.length > 0) {
				@new_sprites.each(sprite => sprite&.dispose);
				@new_sprites.clear;
			}
			// Update weather tiles (sandstorm/blizzard/fog tiled overlay)
			if (@tiles_wide > 0 && @tiles_tall > 0) {
				ensureTiles;
				recalculate_tile_positions;
				@tiles.each_with_index((sprite, i) => update_tile_position(sprite, i));
			} else if (@tiles.length > 0) {
				@tiles.each(sprite => sprite&.dispose);
				@tiles.clear;
			}
		}
	}
}
