//===============================================================================
//
//===============================================================================
public partial class BushBitmap {
	public void initialize(bitmap, isTile, depth) {
		@bitmaps  = new List<string>();
		@bitmap   = bitmap;
		@isTile   = isTile;
		@isBitmap = @bitmap.is_a(Bitmap);
		@depth    = depth;
	}

	public void dispose() {
		@bitmaps.each(b => b&.dispose);
	}

	public void bitmap() {
		thisBitmap = (@isBitmap) ? @bitmap : @bitmap.bitmap;
		current = (@isBitmap) ? 0 : @bitmap.currentIndex;
		if (!@bitmaps[current]) {
			if (@isTile) {
				@bitmaps[current] = BushDepthTile(thisBitmap, @depth);
			} else {
				@bitmaps[current] = BushDepthBitmap(thisBitmap, @depth);
			}
		}
		return @bitmaps[current];
	}

	public void BushDepthBitmap(bitmap, depth) {
		ret = new Bitmap(bitmap.width, bitmap.height);
		charheight = ret.height / 4;
		cy = charheight - depth - 2;
		for (int i = 4; i < 4; i++) { //for '4' times do => |i|
			y = i * charheight;
			if (cy >= 0) {
				ret.blt(0, y, bitmap, new Rect(0, y, ret.width, cy));
				ret.blt(0, y + cy, bitmap, new Rect(0, y + cy, ret.width, 2), 170);
			}
			if (cy + 2 >= 0) ret.blt(0, y + cy + 2, bitmap, new Rect(0, y + cy + 2, ret.width, 2), 85);
		}
		return ret;
	}

	public void BushDepthTile(bitmap, depth) {
		ret = new Bitmap(bitmap.width, bitmap.height);
		charheight = ret.height;
		cy = charheight - depth - 2;
		y = charheight;
		if (cy >= 0) {
			ret.blt(0, y, bitmap, new Rect(0, y, ret.width, cy));
			ret.blt(0, y + cy, bitmap, new Rect(0, y + cy, ret.width, 2), 170);
		}
		if (cy + 2 >= 0) ret.blt(0, y + cy + 2, bitmap, new Rect(0, y + cy + 2, ret.width, 2), 85);
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Sprite_Character : RPG.Sprite {
	public int character		{ get { return _character; } set { _character = value; } }			protected int _character;

	public override void initialize(viewport, character = null) {
		base.initialize(viewport);
		@character    = character;
		@oldbushdepth = 0;
		@spriteoffset = false;
		if (!character || character == Game.GameData.game_player || (System.Text.RegularExpressions.Regex.IsMatch(character.name,@"reflection",RegexOptions.IgnoreCase) rescue false)) {
			@reflection = new Sprite_Reflection(self, viewport);
		}
		if (character == Game.GameData.game_player) @surfbase = new Sprite_SurfBase(self, viewport);
		self.zoom_x = TilemapRenderer.ZOOM_X;
		self.zoom_y = TilemapRenderer.ZOOM_Y;
		update;
	}

	public void groundY() {
		return @character.screen_y_ground;
	}

	public override int visible { set {
		base.visible = value;
		if (@reflection) @reflection.visible = value;
		}
	}

	public override void dispose() {
		@bushbitmap&.dispose;
		@bushbitmap = null;
		@charbitmap&.dispose;
		@charbitmap = null;
		@reflection&.dispose;
		@reflection = null;
		@surfbase&.dispose;
		@surfbase = null;
		@character = null;
		base.dispose();
	}

	public void refresh_graphic() {
		if (@tile_id == @character.tile_id &&
							@character_name == @character.character_name &&
							@character_hue == @character.character_hue &&
							@oldbushdepth == @character.bush_depth) return;
		@tile_id        = @character.tile_id;
		@character_name = @character.character_name;
		@character_hue  = @character.character_hue;
		@oldbushdepth   = @character.bush_depth;
		@charbitmap&.dispose;
		@charbitmap = null;
		@bushbitmap&.dispose;
		@bushbitmap = null;
		if (@tile_id >= 384) {
			@charbitmap = GetTileBitmap(@character.map.tileset_name, @tile_id,
																		@character_hue, @character.width, @character.height);
			@charbitmapAnimated = false;
			@spriteoffset = false;
			@cw = Game_Map.TILE_WIDTH * @character.width;
			@ch = Game_Map.TILE_HEIGHT * @character.height;
			self.src_rect.set(0, 0, @cw, @ch);
			self.ox = @cw / 2;
			self.oy = @ch;
		} else if (@character_name != "") {
			@charbitmap = new AnimatedBitmap(
				"Graphics/Characters/" + @character_name, @character_hue
			);
			if (@character == Game.GameData.game_player) RPG.Cache.retain("Graphics/Characters/", @character_name, @character_hue);
			@charbitmapAnimated = true;
			@spriteoffset = System.Text.RegularExpressions.Regex.IsMatch(@character_name,@"offset",RegexOptions.IgnoreCase);
			@cw = @charbitmap.width / 4;
			@ch = @charbitmap.height / 4;
			self.ox = @cw / 2;
		} else {
			self.bitmap = null;
			@cw = 0;
			@ch = 0;
			@reflection&.update;
		}
		@character.sprite_size = new {@cw, @ch};
	}

	public override void update() {
		if (@character.is_a(Game_Event) && !@character.should_update()) return;
		base.update();
		refresh_graphic;
		if (!@charbitmap) return;
		if (@charbitmapAnimated) @charbitmap.update;
		bushdepth = @character.bush_depth;
		if (bushdepth == 0) {
			self.bitmap = (@charbitmapAnimated) ? @charbitmap.bitmap : @charbitmap;
		} else {
			if (!@bushbitmap) @bushbitmap = new BushBitmap(@charbitmap, (@tile_id >= 384), bushdepth);
			self.bitmap = @bushbitmap.bitmap;
		}
		self.visible = !@character.transparent;
		if (@tile_id == 0) {
			sx = @character.pattern * @cw;
			sy = ((@character.direction - 2) / 2) * @ch;
			self.src_rect.set(sx, sy, @cw, @ch);
			self.oy = (@spriteoffset rescue false) ? @ch - 16 : @ch;
			self.oy -= @character.bob_height;
		}
		if (self.visible) {
			if (@character.is_a(Game_Event) && System.Text.RegularExpressions.Regex.IsMatch(@character.name,@"regulartone",RegexOptions.IgnoreCase)) {
				self.tone.set(0, 0, 0, 0);
			} else {
				DayNightTint(self);
			}
		}
		this_x = @character.screen_x;
		if (TilemapRenderer.ZOOM_X != 1) this_x = ((this_x - (Graphics.width / 2)) * TilemapRenderer.ZOOM_X) + (Graphics.width / 2);
		self.x = this_x;
		this_y = @character.screen_y;
		if (TilemapRenderer.ZOOM_Y != 1) this_y = ((this_y - (Graphics.height / 2)) * TilemapRenderer.ZOOM_Y) + (Graphics.height / 2);
		self.y = this_y;
		self.z = @character.screen_z(@ch);
		self.opacity = @character.opacity;
		self.blend_type = @character.blend_type;
		if (@character.animation_id && @character.animation_id != 0) {
			animation = Game.GameData.data_animations[@character.animation_id];
			animation(animation, true, @character.animation_height || 3, @character.animation_regular_tone || false);
			@character.animation_id = 0;
		}
		@reflection&.update;
		@surfbase&.update;
	}
}
