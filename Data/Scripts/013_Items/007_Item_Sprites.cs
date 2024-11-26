//===============================================================================
// Item icon.
//===============================================================================
public partial class ItemIconSprite : Sprite {
	public int item		{ get { return _item; } }			protected int _item;

	// Height in pixels the item's icon graphic must be for it to be animated by
	// being a horizontal set of frames.
	public const int ANIM_ICON_SIZE = 48;
	// Time in seconds for one animation cycle of this item icon.
	public const int ANIMATION_DURATION = 1.0;

	public override void initialize(x, y, item, viewport = null) {
		base.initialize(viewport);
		@animbitmap = null;
		@frames_count = 1;
		@current_frame = 0;
		self.x = x;
		self.y = y;
		@blankzero = false;
		@forceitemchange = true;
		self.item = item;
		@forceitemchange = false;
	}

	public override void dispose() {
		@animbitmap&.dispose;
		base.dispose();
	}

	public void width() {
		if (!self.bitmap || self.bitmap.disposed()) return 0;
		return (@frames_count == 1) ? self.bitmap.width : ANIM_ICON_SIZE;
	}

	public void height() {
		return (self.bitmap && !self.bitmap.disposed()) ? self.bitmap.height : 0;
	}

	public int blankzero { set {
		@blankzero = val;
		@forceitemchange = true;
		self.item = @item;
		@forceitemchange = false;
		}
	}

	public void setOffset(offset = PictureOrigin.CENTER) {
		@offset = offset;
		changeOrigin;
	}

	public void changeOrigin() {
		if (!@offset) @offset = PictureOrigin.CENTER;
		switch (@offset) {
			case PictureOrigin.TOP_LEFT: case PictureOrigin.TOP: case PictureOrigin.TOP_RIGHT:
				self.oy = 0;
				break;
			case PictureOrigin.LEFT: case PictureOrigin.CENTER: case PictureOrigin.RIGHT:
				self.oy = self.height / 2;
				break;
			case PictureOrigin.BOTTOM_LEFT: case PictureOrigin.BOTTOM: case PictureOrigin.BOTTOM_RIGHT:
				self.oy = self.height;
				break;
		}
		switch (@offset) {
			case PictureOrigin.TOP_LEFT: case PictureOrigin.LEFT: case PictureOrigin.BOTTOM_LEFT:
				self.ox = 0;
				break;
			case PictureOrigin.TOP: case PictureOrigin.CENTER: case PictureOrigin.BOTTOM:
				self.ox = self.width / 2;
				break;
			case PictureOrigin.TOP_RIGHT: case PictureOrigin.RIGHT: case PictureOrigin.BOTTOM_RIGHT:
				self.ox = self.width;
				break;
		}
	}

	public int item { set {
		if (@item == value && !@forceitemchange) return;
		@item = value;
		@animbitmap&.dispose;
		@animbitmap = null;
		if (@item || !@blankzero) {
			@animbitmap = new AnimatedBitmap(GameData.Item.icon_filename(@item));
			self.bitmap = @animbitmap.bitmap;
			if (self.bitmap.height == ANIM_ICON_SIZE) {
				@frames_count = (int)Math.Max((int)Math.Floor(self.bitmap.width / ANIM_ICON_SIZE), 1);
				self.src_rect = new Rect(0, 0, ANIM_ICON_SIZE, ANIM_ICON_SIZE);
			} else {
				@frames_count = 1;
				self.src_rect = new Rect(0, 0, self.bitmap.width, self.bitmap.height);
			}
			@current_frame = 0;
		} else {
			self.bitmap = null;
		}
		changeOrigin;
		}
	}

	public void update_frame() {
		@current_frame = (int)Math.Floor(@frames_count * (System.uptime % ANIMATION_DURATION) / ANIMATION_DURATION);
	}

	public override void update() {
		@updating = true;
		base.update();
		if (@animbitmap) {
			@animbitmap.update;
			self.bitmap = @animbitmap.bitmap;
			if (@frames_count > 1) {
				update_frame;
				self.src_rect.x = @current_frame * ANIM_ICON_SIZE;
			}
		}
		@updating = false;
	}
}

//===============================================================================
// Item held icon (used in the party screen).
//===============================================================================
public partial class HeldItemIconSprite : Sprite {
	public override void initialize(x, y, pokemon, viewport = null) {
		base.initialize(viewport);
		self.x = x;
		self.y = y;
		@pokemon = pokemon;
		@item = null;
		self.item = @pokemon.item_id;
	}

	public override void dispose() {
		@animbitmap&.dispose;
		base.dispose();
	}

	public int pokemon { set {
		@pokemon = value;
		self.item = @pokemon.item_id;
		}
	}

	public int item { set {
		if (@item == value) return;
		@item = value;
		@animbitmap&.dispose;
		@animbitmap = null;
		if (@item) {
			@animbitmap = new AnimatedBitmap(GameData.Item.held_icon_filename(@item));
			self.bitmap = @animbitmap.bitmap;
		} else {
			self.bitmap = null;
		}
		}
	}

	public override void update() {
		base.update();
		self.item = @pokemon.item_id;
		if (@animbitmap) {
			@animbitmap.update;
			self.bitmap = @animbitmap.bitmap;
		}
	}
}
