//===============================================================================
//
//===============================================================================
public partial class SpriteAnimation {
	@@_animations      = new List<string>();
	@@_reference_count = new List<string>();

	public void initialize(sprite) {
		@sprite = sprite;
	}

	public void x(*arg)        {@sprite.x(*arg);        }
	public void y(*arg)        {@sprite.y(*arg);        }
	public void ox(*arg)       {@sprite.ox(*arg);       }
	public void oy(*arg)       {@sprite.oy(*arg);       }
	public void viewport(*arg) {@sprite.viewport(*arg); }
	public void flash(*arg)    {@sprite.flash(*arg);    }
	public void src_rect(*arg) {@sprite.src_rect(*arg); }
	public void opacity(*arg)  {@sprite.opacity(*arg);  }
	public void tone(*arg)     {@sprite.tone(*arg);     }

	public static void clear() {
		@@_animations.clear;
	}

	public void dispose() {
		dispose_animation;
		dispose_loop_animation;
	}

	public void animation(animation, hit, height = 3, no_tone = false) {
		dispose_animation;
		@_animation = animation;
		if (@_animation.null()) return;
		@_animation_hit      = hit;
		@_animation_height   = height;
		@_animation_no_tone  = no_tone;
		@_animation_duration = @_animation.frame_max;
		@_animation_index    = -1;
		fr = 20;
		if (System.Text.RegularExpressions.Regex.IsMatch(@_animation.name,@"\[\s*(\d+?)\s*\]\s*$")) {
			fr = $~[1].ToInt();
		}
		@_animation_time_per_frame = 1.0 / fr;
		@_animation_timer_start = System.uptime;
		animation_name = @_animation.animation_name;
		animation_hue  = @_animation.animation_hue;
		bitmap = GetAnimation(animation_name, animation_hue);
		if (@@_reference_count.Contains(bitmap)) {
			@@_reference_count[bitmap] += 1;
		} else {
			@@_reference_count[bitmap] = 1;
		}
		@_animation_sprites = new List<string>();
		if (@_animation.position != 3 || !@@_animations.Contains(animation)) {
			16.times do;
				sprite = ::new Sprite(self.viewport);
				sprite.bitmap = bitmap;
				sprite.visible = false;
				@_animation_sprites.Add(sprite);
			}
			unless (@@_animations.Contains(animation)) @@_animations.Add(animation);
		}
		update_animation;
	}

	public void loop_animation(animation) {
		if (animation == @_loop_animation) return;
		dispose_loop_animation;
		@_loop_animation = animation;
		if (@_loop_animation.null()) return;
		@_loop_animation_duration = @_animation.frame_max;
		@_loop_animation_index = -1;
		fr = 20;
		if (System.Text.RegularExpressions.Regex.IsMatch(@_animation.name,@"\[\s*(\d+?)\s*\]\s*$")) {
			fr = $~[1].ToInt();
		}
		@_loop_animation_time_per_frame = 1.0 / fr;
		@_loop_animation_timer_start = System.uptime;
		animation_name = @_loop_animation.animation_name;
		animation_hue  = @_loop_animation.animation_hue;
		bitmap = GetAnimation(animation_name, animation_hue);
		if (@@_reference_count.Contains(bitmap)) {
			@@_reference_count[bitmap] += 1;
		} else {
			@@_reference_count[bitmap] = 1;
		}
		@_loop_animation_sprites = new List<string>();
		16.times do;
			sprite = ::new Sprite(self.viewport);
			sprite.bitmap = bitmap;
			sprite.visible = false;
			@_loop_animation_sprites.Add(sprite);
		}
		update_loop_animation;
	}

	public void dispose_animation() {
		if (@_animation_sprites.null()) return;
		sprite = @_animation_sprites[0];
		if (sprite) {
			@@_reference_count[sprite.bitmap] -= 1;
			if (@@_reference_count[sprite.bitmap] == 0) sprite.bitmap.dispose;
		}
		@_animation_sprites.each(s => s.dispose);
		@_animation_sprites = null;
		@_animation = null;
		@_animation_duration = 0;
	}

	public void dispose_loop_animation() {
		if (@_loop_animation_sprites.null()) return;
		sprite = @_loop_animation_sprites[0];
		if (sprite) {
			@@_reference_count[sprite.bitmap] -= 1;
			if (@@_reference_count[sprite.bitmap] == 0) sprite.bitmap.dispose;
		}
		@_loop_animation_sprites.each(s => s.dispose);
		@_loop_animation_sprites = null;
		@_loop_animation = null;
	}

	public bool active() {
		return @_loop_animation_sprites || @_animation_sprites;
	}

	public bool effect() {
		return @_animation_duration > 0;
	}

	public void update() {
		if (@_animation) update_animation;
		if (@_loop_animation) update_loop_animation;
	}

	public void update_animation() {
		new_index = ((System.uptime - @_animation_timer_start) / @_animation_time_per_frame).ToInt();
		if (new_index >= @_animation_duration) {
			dispose_animation;
			return;
		}
		quick_update = (@_animation_index == new_index);
		@_animation_index = new_index;
		frame_index = @_animation_index;
		cell_data   = @_animation.frames[frame_index].cell_data;
		position    = @_animation.position;
		animation_set_sprites(@_animation_sprites, cell_data, position, quick_update);
		if (quick_update) return;
		@_animation.timings.each do |timing|
			if (timing.frame != frame_index) continue;
			animation_process_timing(timing, @_animation_hit);
		}
	}

	public void update_loop_animation() {
		new_index = ((System.uptime - @_loop_animation_timer_start) / @_loop_animation_time_per_frame).ToInt();
		new_index %= @_loop_animation_duration;
		quick_update = (@_loop_animation_index == new_index);
		@_loop_animation_index = new_index;
		frame_index = @_loop_animation_index;
		cell_data   = @_loop_animation.frames[frame_index].cell_data;
		position    = @_loop_animation.position;
		animation_set_sprites(@_loop_animation_sprites, cell_data, position, quick_update);
		if (quick_update) return;
		@_loop_animation.timings.each do |timing|
			if (timing.frame != frame_index) continue;
			animation_process_timing(timing, true);
		}
	}

	public void animation_set_sprites(sprites, cell_data, position, quick_update = false) {
		sprite_x = 320;
		sprite_y = 240;
		if (position == 3) {   // Screen
			if (self.viewport) {
				sprite_x = self.viewport.rect.width / 2;
				sprite_y = self.viewport.rect.height - 160;
			}
		} else {
			sprite_x = self.x - self.ox + (self.src_rect.width / 2);
			sprite_y = self.y - self.oy;
			if (self.src_rect.height > 1) {
				if (position == 1) sprite_y += self.src_rect.height / 2;   // Middle
				if (position == 2) sprite_y += self.src_rect.height;   // Bottom
			}
		}
		for (int i = 16; i < 16; i++) { //for '16' times do => |i|
			sprite = sprites[i];
			pattern = cell_data[i, 0];
			if (sprite.null() || pattern.null() || pattern == -1) {
				if (sprite) sprite.visible = false;
				continue;
			}
			sprite.x = sprite_x + cell_data[i, 1];
			sprite.y = sprite_y + cell_data[i, 2];
			if (quick_update) continue;
			sprite.visible = true;
			sprite.src_rect.set((pattern % 5) * 192, (pattern / 5) * 192, 192, 192);
			switch (@_animation_height) {
				case -1:  sprite.z = -25; break;
				case 0:   sprite.z = 1; break;
				case 1:   sprite.z = sprite.y + (Game_Map.TILE_HEIGHT * 3 / 2) + 1; break;
				case 2:   sprite.z = sprite.y + (Game_Map.TILE_HEIGHT * 3) + 1; break;
				default:         sprite.z = 2000;
			}
			sprite.ox         = 96;
			sprite.oy         = 96;
			sprite.zoom_x     = cell_data[i, 3] / 100.0;
			sprite.zoom_y     = cell_data[i, 3] / 100.0;
			sprite.angle      = cell_data[i, 4];
			sprite.mirror     = (cell_data[i, 5] == 1);
			if (!@_animation_no_tone) sprite.tone       = self.tone;
			sprite.opacity    = cell_data[i, 6] * self.opacity / 255.0;
			sprite.blend_type = cell_data[i, 7];
		}
	}

	public void animation_process_timing(timing, hit) {
		if (timing.condition == 0 ||
			(timing.condition == 1 && hit == true) ||
			(timing.condition == 2 && hit == false)) {
			if (timing.se.name != "") {
				se = timing.se;
				SEPlay(se);
			}
			switch (timing.flash_scope) {
				case 1:
					self.flash(timing.flash_color, timing.flash_duration * 2);
					break;
				case 2:
					if (self.viewport) self.viewport.flash(timing.flash_color, timing.flash_duration * 2);
					break;
				case 3:
					self.flash(null, timing.flash_duration * 2);
					break;
			}
		}
	}

	public int x { set {
		sx = x - self.x;
		if (sx == 0) return;
		if (@_animation_sprites) {
			16.times(i => @_animation_sprites[i].x += sx);
		}
		if (@_loop_animation_sprites) {
			16.times(i => @_loop_animation_sprites[i].x += sx);
		}
		}
	}

	public int y { set {
		sy = y - self.y;
		if (sy == 0) return;
		if (@_animation_sprites) {
			16.times(i => @_animation_sprites[i].y += sy);
		}
		if (@_loop_animation_sprites) {
			16.times(i => @_loop_animation_sprites[i].y += sy);
		}
		}
	}
}

//===============================================================================
// A sprite whose sole purpose is to display an animation (a SpriteAnimation).
// This sprite can be displayed anywhere on the map and is disposed automatically
// when its animation is finished. Used for grass rustling and so forth.
//===============================================================================
public partial class AnimationContainerSprite : RPG.Sprite {
	public override void initialize(animID, map, tileX, tileY, viewport = null, tinting = false, height = 3) {
		base.initialize(viewport);
		@tileX = tileX;
		@tileY = tileY;
		@map = map;
		setCoords;
		if (tinting) DayNightTint(self);
		self.animation(Game.GameData.data_animations[animID], true, height);
	}

	public void setCoords() {
		self.x = (((@tileX * Game_Map.REAL_RES_X) - @map.display_x) / Game_Map.X_SUBPIXELS).ceil;
		self.x += Game_Map.TILE_WIDTH / 2;
		self.y = (((@tileY * Game_Map.REAL_RES_Y) - @map.display_y) / Game_Map.Y_SUBPIXELS).ceil;
		self.y += Game_Map.TILE_HEIGHT;
	}

	public override void update() {
		if (disposed()) return;
		setCoords;
		base.update();
		if (!effect()) dispose;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Spriteset_Map {
	public int usersprites		{ get { return _usersprites; } }			protected int _usersprites;

	unless (private_method_defined(:_animationSprite_initialize)) alias _animationSprite_initialize initialize;
	unless (method_defined(:_animationSprite_update)) alias _animationSprite_update update;
	unless (method_defined(:_animationSprite_dispose)) alias _animationSprite_dispose dispose;

	public void initialize(map = null) {
		@usersprites = new List<string>();
		_animationSprite_initialize(map);
	}

	// Used to display animations that remain in the same location on the map.
	// Typically for grass rustling and dust clouds, and other animations that
	// aren't relative to an event.
	public void addUserAnimation(animID, x, y, tinting = false, height = 3) {
		sprite = new AnimationContainerSprite(animID, self.map, x, y, @@viewport1, tinting, height);
		addUserSprite(sprite);
		return sprite;
	}

	public void addUserSprite(new_sprite) {
		@usersprites.each_with_index do |sprite, i|
			if (sprite && !sprite.disposed()) continue;
			@usersprites[i] = new_sprite;
			return;
		}
		@usersprites.Add(new_sprite);
	}

	public void dispose() {
		_animationSprite_dispose;
		@usersprites.each(sprite => sprite.dispose);
		@usersprites.clear;
	}

	public void update() {
		@@viewport3.tone.set(0, 0, 0, 0);
		_animationSprite_update;
		@usersprites.each(sprite => { if (!sprite.disposed()) sprite.update; });
		@usersprites.delete_if(sprite => sprite.disposed());
	}
}
