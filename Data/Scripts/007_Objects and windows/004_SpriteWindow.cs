//===============================================================================
// SpriteWindow is a class based on Window which emulates Window's functionality.
// This class is necessary in order to change the viewport of windows (with
// viewport=) and to make windows fade in and out (with tone=).
//===============================================================================
public partial class SpriteWindow : Window {
	public int tone		{ get { return _tone; } }			protected int _tone;
	public int color		{ get { return _color; } }			protected int _color;
	public int viewport		{ get { return _viewport; } }			protected int _viewport;
	public int contents		{ get { return _contents; } }			protected int _contents;
	public int ox		{ get { return _ox; } }			protected int _ox;
	public int oy		{ get { return _oy; } }			protected int _oy;
	public int x		{ get { return _x; } }			protected int _x;
	public int y		{ get { return _y; } }			protected int _y;
	public int z		{ get { return _z; } }			protected int _z;
	public int zoom_x		{ get { return _zoom_x; } }			protected int _zoom_x;
	public int zoom_y		{ get { return _zoom_y; } }			protected int _zoom_y;
	public int offset_x		{ get { return _offset_x; } }			protected int _offset_x;
	public int offset_y		{ get { return _offset_y; } }			protected int _offset_y;
	public int width		{ get { return _width; } }			protected int _width;
	public int active		{ get { return _active; } }			protected int _active;
	public int pause		{ get { return _pause; } }			protected int _pause;
	public int height		{ get { return _height; } }			protected int _height;
	public int opacity		{ get { return _opacity; } }			protected int _opacity;
	public int back_opacity		{ get { return _back_opacity; } }			protected int _back_opacity;
	public int contents_opacity		{ get { return _contents_opacity; } }			protected int _contents_opacity;
	public int visible		{ get { return _visible; } }			protected int _visible;
	public int cursor_rect		{ get { return _cursor_rect; } }			protected int _cursor_rect;
	public int contents_blend_type		{ get { return _contents_blend_type; } }			protected int _contents_blend_type;
	public int blend_type		{ get { return _blend_type; } }			protected int _blend_type;
	public int openness		{ get { return _openness; } }			protected int _openness;

	public void windowskin() {
		@_windowskin;
	}

	// Flags used to preserve compatibility with RGSS/RGSS2's version of Window.
	public static partial class CompatBits {
		public const int CORRECT_Z          = 1;
		public const int EXPAND_BACK        = 2;
		public const int SHOW_SCROLL_ARROWS = 4;
		public const int STRETCH_SIDES      = 8;
		public const int SHOW_PAUSE         = 16;
		public const int SHOW_CURSOR        = 32;
	}

	public int compat		{ get { return _compat; } }			protected int _compat;
	public int skinformat		{ get { return _skinformat; } }			protected int _skinformat;
	public int skinrect		{ get { return _skinrect; } }			protected int _skinrect;

	public int compat { set {
		@compat = value;
		privRefresh(true);
		}
	}

	public void initialize(viewport = null) {
		@sprites = new List<string>();
		@spritekeys = new {
			"back",
			"corner0", "side0", "scroll0",
			"corner1", "side1", "scroll1",
			"corner2", "side2", "scroll2",
			"corner3", "side3", "scroll3",
			"cursor", "contents", "pause";
		}
		@viewport = viewport;
		@sidebitmaps = new {null, null, null, null};
		@cursorbitmap = null;
		@bgbitmap = null;
		@spritekeys.each do |i|
			@sprites[i] = new Sprite(@viewport);
		}
		@disposed = false;
		@tone = new Tone(0, 0, 0);
		@color = new Color(0, 0, 0, 0);
		@blankcontents = new Bitmap(1, 1); // RGSS2 requires this
		@contents = @blankcontents;
		@_windowskin = null;
		@rpgvx = false;
		@compat = CompatBits.EXPAND_BACK | CompatBits.STRETCH_SIDES;
		@x = 0;
		@y = 0;
		@width = 0;
		@height = 0;
		@offset_x = 0;
		@offset_y = 0;
		@zoom_x = 1.0;
		@zoom_y = 1.0;
		@ox = 0;
		@oy = 0;
		@z = 0;
		@stretch = true;
		@visible = true;
		@active = true;
		@openness = 255;
		@opacity = 255;
		@back_opacity = 255;
		@blend_type = 0;
		@contents_blend_type = 0;
		@contents_opacity = 255;
		@cursor_rect = new WindowCursorRect(self);
		@cursoropacity = 255;
		@pause = false;
		@pauseframe = 0;
		@flash_duration = 0;
		@pauseopacity = 0;
		@skinformat = 0;
		@skinrect = new Rect(0, 0, 0, 0);
		@trim = new {16, 16, 16, 16};
		privRefresh(true);
	}

	public void dispose() {
		if (!self.disposed()) {
			@sprites.each do |i|
				i[1]&.dispose;
				@sprites[i[0]] = null;
			}
			@sidebitmaps.each_with_index do |bitmap, i|
				bitmap&.dispose;
				@sidebitmaps[i] = null;
			}
			@blankcontents.dispose;
			@cursorbitmap&.dispose;
			@backbitmap&.dispose;
			@sprites.clear;
			@sidebitmaps.clear;
			@_windowskin = null;
			@disposed = true;
		}
	}

	public int stretch { set {
		@stretch = value;
		privRefresh(true);
		}
	}

	public int visible { set {
		@visible = value;
		privRefresh;
		}
	}

	public int viewport { set {
		@viewport = value;
		@spritekeys.each do |i|
			@sprites[i]&.dispose;
			if (@sprites[i].is_a(Sprite)) {
				@sprites[i] = new Sprite(@viewport);
			} else {
				@sprites[i] = null;
				}
	}
		}
		privRefresh(true);
	}

	public int z { set {
		@z = value;
		privRefresh;
		}
	}

	public bool disposed() {
		return @disposed;
	}

	public int contents { set {
		if (@contents != value) {
			@contents = value;
			if (@visible) privRefresh;
			}
	}
	}

	public int ox { set {
		if (@ox != value) {
			@ox = value;
			if (@visible) privRefresh;
			}
	}
	}

	public int oy { set {
		if (@oy != value) {
			@oy = value;
			if (@visible) privRefresh;
			}
	}
	}

	public int active { set {
		@active = value;
		privRefresh(true);
		}
	}

	public int cursor_rect { set {
		if (value) {
			@cursor_rect.set(value.x, value.y, value.width, value.height);
		} else {
			@cursor_rect.empty;
			}
	}
	}

	public int openness { set {
		@openness = value;
		if (@openness < 0) @openness = 0;
		if (@openness > 255) @openness = 255;
		privRefresh;
		}
	}

	public int width { set {
		@width = value;
		privRefresh(true);
		}
	}

	public int height { set {
		@height = value;
		privRefresh(true);
		}
	}

	public int pause { set {
		@pause = value;
		if (!value) @pauseopacity = 0;
		if (@visible) privRefresh;
		}
	}

	public int x { set {
		@x = value;
		if (@visible) privRefresh;
		}
	}

	public int y { set {
		@y = value;
		if (@visible) privRefresh;
		}
	}

	public int zoom_x { set {
		@zoom_x = value;
		if (@visible) privRefresh;
		}
	}

	public int zoom_y { set {
		@zoom_y = value;
		if (@visible) privRefresh;
		}
	}

	public int offset_x { set {
		@x = value;
		if (@visible) privRefresh;
		}
	}

	public int offset_y { set {
		@y = value;
		if (@visible) privRefresh;
		}
	}

	public int opacity { set {
		@opacity = value;
		if (@opacity < 0) @opacity = 0;
		if (@opacity > 255) @opacity = 255;
		if (@visible) privRefresh;
		}
	}

	public int back_opacity { set {
		@back_opacity = value;
		if (@back_opacity < 0) @back_opacity = 0;
		if (@back_opacity > 255) @back_opacity = 255;
		if (@visible) privRefresh;
		}
	}

	public int contents_opacity { set {
		@contents_opacity = value;
		if (@contents_opacity < 0) @contents_opacity = 0;
		if (@contents_opacity > 255) @contents_opacity = 255;
		if (@visible) privRefresh;
		}
	}

	public int tone { set {
		@tone = value;
		if (@visible) privRefresh;
		}
	}

	public int color { set {
		@color = value;
		if (@visible) privRefresh;
		}
	}

	public int blend_type { set {
		@blend_type = value;
		if (@visible) privRefresh;
		}
	}

	// duration is in 1/20ths of a second
	public void flash(color, duration) {
		if (disposed()) return;
		@flash_duration = duration / 20.0;
		@flash_timer_start = System.uptime;
		@sprites.each do |i|
			i[1].flash(color, (@flash_duration * Graphics.frame_rate).ToInt());   // Must be in frames
		}
	}

	public void update() {
		if (disposed()) return;
		mustchange = false;
		if (@active) {
			cursor_time = System.uptime / 0.4;
			if (cursor_time.ToInt().even()) {
				@cursoropacity = lerp(255, 128, 0.4, cursor_time % 2);
			} else {
				@cursoropacity = lerp(128, 255, 0.4, (cursor_time - 1) % 2);
			}
		} else {
			@cursoropacity = 128;
		}
		privRefreshCursor;
		if (@pause) {
			oldpauseframe = @pauseframe;
			oldpauseopacity = @pauseopacity;
			@pauseframe = (System.uptime * 5).ToInt() % 4;   // 4 frames, 5 frames per second
			@pauseopacity = (int)Math.Min(@pauseopacity + 64, 255);
			mustchange = @pauseframe != oldpauseframe || @pauseopacity != oldpauseopacity;
		}
		if (mustchange) privRefresh;
		if (@flash_timer_start) {
			@sprites.each_value(i => i.update);
			if (System.uptime - @flash_timer_start >= @flash_duration) @flash_timer_start = null;
		}
	}

	public void loadSkinFile(_file) {
		if (((self.windowskin.width == 80 || self.windowskin.width == 96) &&
			self.windowskin.height == 48) {
			// Body = X, Y, width, height of body rectangle within windowskin
			@skinrect.set(32, 16, 16, 16);
			// Trim = X, Y, width, height of trim rectangle within windowskin
			@trim = new {32, 16, 16, 16};
		} else if (self.windowskin.width == 80 && self.windowskin.height == 80) {
			@skinrect.set(32, 32, 16, 16);
			@trim = new {32, 16, 16, 48};
		}
	}

	public int windowskin { set {
		oldSkinWidth = (@_windowskin && !@_windowskin.disposed()) ? @_windowskin.width : -1;
		oldSkinHeight = (@_windowskin && !@_windowskin.disposed()) ? @_windowskin.height : -1;
		@_windowskin = value;
		if (@skinformat == 1) {
			@rpgvx = false;
			if (@_windowskin && !@_windowskin.disposed()) {
				if (@_windowskin.width != oldSkinWidth || @_windowskin.height != oldSkinHeight) {
					// Update skinrect and trim if windowskin's dimensions have changed
					@skinrect.set((@_windowskin.width - 16) / 2, (@_windowskin.height - 16) / 2, 16, 16);
					@trim = new {@skinrect.x, @skinrect.y, @skinrect.x, @skinrect.y	};
	}
				}
			} else {
				@skinrect.set(16, 16, 16, 16);
				@trim = new {16, 16, 16, 16};
			}
		} else {
			if (value.is_a(Bitmap) && !value.disposed() && value.width == 128) {
				@rpgvx = true;
			} else {
				@rpgvx = false;
			}
			@trim = new {16, 16, 16, 16};
		}
		privRefresh(true);
	}

	public int skinrect { set {
		@skinrect = value;
		privRefresh;
		}
	}

	public int skinformat { set {
		if (@skinformat != value) {
			@skinformat = value;
			privRefresh(true);
			}
	}
	}

	public void borderX() {
		if (!@trim || skinformat == 0) return 32;
		if (@_windowskin && !@_windowskin.disposed()) {
			return @trim[0] + (@_windowskin.width - @trim[2] - @trim[0]);
		}
		return 32;
	}

	public void borderY() {
		if (!@trim || skinformat == 0) return 32;
		if (@_windowskin && !@_windowskin.disposed()) {
			return @trim[1] + (@_windowskin.height - @trim[3] - @trim[1]);
		}
		return 32;
	}

	public int leftEdge { get { return self.startX; } }
	public int topEdge { get { return self.startY; } }
	public int rightEdge { get { return self.borderX - self.leftEdge; } }
	public int bottomEdge { get { return self.borderY - self.topEdge; } }

	public void startX() {
		return !@trim || skinformat == 0  ? 16 : @trim[0];
	}

	public void startY() {
		return !@trim || skinformat == 0  ? 16 : @trim[1];
	}

	public void endX() {
		return !@trim || skinformat == 0  ? 16 : @trim[2];
	}

	public void endY() {
		return !@trim || skinformat == 0  ? 16 : @trim[3];
	}

	public int startX { set {
		@trim[0] = value;
		privRefresh;
		}
	}

	public int startY { set {
		@trim[1] = value;
		privRefresh;
		}
	}

	public int endX { set {
		@trim[2] = value;
		privRefresh;
		}
	}

	public int endY { set {
		@trim[3] = value;
		privRefresh;
		}
	}

	//-----------------------------------------------------------------------------

	private;

	public void ensureBitmap(bitmap, dwidth, dheight) {
		if (!bitmap || bitmap.disposed() || bitmap.width < dwidth || bitmap.height < dheight) {
			bitmap&.dispose;
			bitmap = new Bitmap((int)Math.Max(1, dwidth), (int)Math.Max(1, dheight));
		}
		return bitmap;
	}

	public void tileBitmap(dstbitmap, dstrect, srcbitmap, srcrect) {
		if (!srcbitmap || srcbitmap.disposed()) return;
		left = dstrect.x;
		top = dstrect.y;
		y = 0;
		do { //loop; while (true);
			unless (y < dstrect.height) break;
			x = 0;
			do { //loop; while (true);
				unless (x < dstrect.width) break;
				dstbitmap.blt(x + left, y + top, srcbitmap, srcrect);
				x += srcrect.width;
			}
			y += srcrect.height;
		}
	}

	public void privRefreshCursor() {
		contopac = self.contents_opacity;
		cursoropac = @cursoropacity * contopac / 255;
		@sprites["cursor"].opacity = cursoropac;
	}

	public void privRefresh(changeBitmap = false) {
		if (!self || self.disposed()) return;
		backopac = self.back_opacity * self.opacity / 255;
		contopac = self.contents_opacity;
		cursoropac = @cursoropacity * contopac / 255;
		haveskin = @_windowskin && !@_windowskin.disposed();
		for (int i = 4; i < 4; i++) { //for '4' times do => |i|
			@sprites[$"corner{i}"].bitmap = @_windowskin;
			@sprites[$"scroll{i}"].bitmap = @_windowskin;
		}
		@sprites["pause"].bitmap = @_windowskin;
		@sprites["contents"].bitmap = @contents;
		if (haveskin) {
			for (int i = 4; i < 4; i++) { //for '4' times do => |i|
				@sprites[$"corner{i}"].opacity = backopac;
				@sprites[$"corner{i}"].tone = @tone;
				@sprites[$"corner{i}"].color = @color;
				@sprites[$"corner{i}"].visible = @visible;
				@sprites[$"corner{i}"].blend_type = @blend_type;
				@sprites[$"side{i}"].opacity = backopac;
				@sprites[$"side{i}"].tone = @tone;
				@sprites[$"side{i}"].color = @color;
				@sprites[$"side{i}"].blend_type = @blend_type;
				@sprites[$"side{i}"].visible = @visible;
				@sprites[$"scroll{i}"].opacity = @opacity;
				@sprites[$"scroll{i}"].tone = @tone;
				@sprites[$"scroll{i}"].color = @color;
				@sprites[$"scroll{i}"].visible = @visible;
				@sprites[$"scroll{i}"].blend_type = @blend_type;
			}
			new {"back", "cursor", "pause", "contents"}.each do |i|
				@sprites[i].color = @color;
				@sprites[i].tone = @tone;
				@sprites[i].blend_type = @blend_type;
			}
			@sprites["contents"].blend_type = @contents_blend_type;
			@sprites["back"].opacity = backopac;
			@sprites["contents"].opacity = contopac;
			@sprites["cursor"].opacity = cursoropac;
			@sprites["pause"].opacity = @pauseopacity;
			supported = (@skinformat == 0);
			hascontents = (@contents && !@contents.disposed());
			@sprites["back"].visible = @visible;
			@sprites["contents"].visible = @visible && @openness == 255;
			@sprites["pause"].visible = supported && @visible && @pause &&
																	(@combat & CompatBits.SHOW_PAUSE)
			@sprites["cursor"].visible = supported && @visible && @openness == 255 &&
																	(@combat & CompatBits.SHOW_CURSOR)
			@sprites["scroll0"].visible = false;
			@sprites["scroll1"].visible = false;
			@sprites["scroll2"].visible = false;
			@sprites["scroll3"].visible = false;
		} else {
			for (int i = 4; i < 4; i++) { //for '4' times do => |i|
				@sprites[$"corner{i}"].visible = false;
				@sprites[$"side{i}"].visible = false;
				@sprites[$"scroll{i}"].visible = false;
			}
			@sprites["contents"].visible = @visible && @openness == 255;
			@sprites["contents"].color = @color;
			@sprites["contents"].tone = @tone;
			@sprites["contents"].blend_type = @contents_blend_type;
			@sprites["contents"].opacity = contopac;
			@sprites["back"].visible = false;
			@sprites["pause"].visible = false;
			@sprites["cursor"].visible = false;
		}
		@spritekeys.each do |i|
			@sprites[i].z = @z;
		}
		if ((@compat & CompatBits.CORRECT_Z) > 0 && @skinformat == 0 && !@rpgvx) {
			// Compatibility Mode: Cursor, pause, and contents have higher Z
			@sprites["cursor"].z = @z + 1;
			@sprites["contents"].z = @z + 2;
			@sprites["pause"].z = @z + 2;
		}
		if (@skinformat == 0) {
			startX = 16;
			startY = 16;
			endX = 16;
			endY = 16;
			trimStartX = 16;
			trimStartY = 16;
			trimWidth = 32;
			trimHeight = 32;
			if (@rpgvx) {
				trimX = 64;
				trimY = 0;
				backRect = new Rect(0, 0, 64, 64);
				blindsRect = new Rect(0, 64, 64, 64);
			} else {
				trimX = 128;
				trimY = 0;
				backRect = new Rect(0, 0, 128, 128);
				blindsRect = null;
			}
			if (@_windowskin && !@_windowskin.disposed()) {
				@sprites["corner0"].src_rect.set(trimX, trimY + 0, 16, 16);
				@sprites["corner1"].src_rect.set(trimX + 48, trimY + 0, 16, 16);
				@sprites["corner2"].src_rect.set(trimX, trimY + 48, 16, 16);
				@sprites["corner3"].src_rect.set(trimX + 48, trimY + 48, 16, 16);
				@sprites["scroll0"].src_rect.set(trimX + 24, trimY + 16, 16, 8); // up
				@sprites["scroll3"].src_rect.set(trimX + 24, trimY + 40, 16, 8); // down
				@sprites["scroll1"].src_rect.set(trimX + 16, trimY + 24, 8, 16); // left
				@sprites["scroll2"].src_rect.set(trimX + 40, trimY + 24, 8, 16); // right
				cursorX = trimX;
				cursorY = trimY + 64;
				sideRects = new {new Rect(trimX + 16, trimY + 0, 32, 16),
										new Rect(trimX, trimY + 16, 16, 32),
										new Rect(trimX + 48, trimY + 16, 16, 32),
										new Rect(trimX + 16, trimY + 48, 32, 16)};
				pauseRects = new {trimX + 32, trimY + 64,
											trimX + 48, trimY + 64,
											trimX + 32, trimY + 80,
											trimX + 48, trimY + 80};
				pauseWidth = 16;
				pauseHeight = 16;
				@sprites["pause"].src_rect.set(
					pauseRects[@pauseframe * 2],
					pauseRects[(@pauseframe * 2) + 1],
					pauseWidth, pauseHeight
				);
			}
		} else {
			trimStartX = @trim[0];
			trimStartY = @trim[1];
			trimWidth = @trim[0] + (@skinrect.width - @trim[2] + @trim[0]);
			trimHeight = @trim[1] + (@skinrect.height - @trim[3] + @trim[1]);
			if (@_windowskin && !@_windowskin.disposed()) {
				// width of left end of window
				startX = @skinrect.x;
				// width of top end of window
				startY = @skinrect.y;
				cx = @skinrect.x + @skinrect.width; // right side of BODY rect
				cy = @skinrect.y + @skinrect.height; // bottom side of BODY rect
				// width of right end of window
				endX = (!@_windowskin || @_windowskin.disposed()) ? @skinrect.x : @_windowskin.width - cx;
				// height of bottom end of window
				endY = (!@_windowskin || @_windowskin.disposed()) ? @skinrect.y : @_windowskin.height - cy;
				@sprites["corner0"].src_rect.set(0, 0, startX, startY);
				@sprites["corner1"].src_rect.set(cx, 0, endX, startY);
				@sprites["corner2"].src_rect.set(0, cy, startX, endY);
				@sprites["corner3"].src_rect.set(cx, cy, endX, endY);
				backRect = new Rect(@skinrect.x, @skinrect.y, @skinrect.width, @skinrect.height);
				blindsRect = null;
				sideRects = new {
					new Rect(startX, 0, @skinrect.width, startY),  // side0 (top)
					new Rect(0, startY, startX, @skinrect.height), // side1 (left)
					new Rect(cx, startY, endX, @skinrect.height),  // side2 (right)
					new Rect(startX, cy, @skinrect.width, endY);    // side3 (bottom)
				}
			}
		}
		if (@width > trimWidth && @height > trimHeight) {
			@sprites["contents"].src_rect.set(@ox, @oy, @width - trimWidth, @height - trimHeight);
		} else {
			@sprites["contents"].src_rect.set(0, 0, 0, 0);
		}
		@sprites["contents"].x = @x + trimStartX;
		@sprites["contents"].y = @y + trimStartY;
		if (((@compat & CompatBits.SHOW_SCROLL_ARROWS) > 0 && @skinformat == 0 &&
			@_windowskin && !@_windowskin.disposed() &&
			@contents && !@contents.disposed()) {
			@sprites["scroll0"].visible = @visible && hascontents && @oy > 0;
			@sprites["scroll1"].visible = @visible && hascontents && @ox > 0;
			@sprites["scroll2"].visible = @visible && (@contents.width - @ox) > @width - trimWidth;
			@sprites["scroll3"].visible = @visible && (@contents.height - @oy) > @height - trimHeight;
		}
		if (@_windowskin && !@_windowskin.disposed()) {
			borderX = startX + endX;
			borderY = startY + endY;
			@sprites["corner0"].x = @x;
			@sprites["corner0"].y = @y;
			@sprites["corner1"].x = @x + @width - endX;
			@sprites["corner1"].y = @y;
			@sprites["corner2"].x = @x;
			@sprites["corner2"].y = @y + @height - endY;
			@sprites["corner3"].x = @x + @width - endX;
			@sprites["corner3"].y = @y + @height - endY;
			@sprites["side0"].x = @x + startX;
			@sprites["side0"].y = @y;
			@sprites["side1"].x = @x;
			@sprites["side1"].y = @y + startY;
			@sprites["side2"].x = @x + @width - endX;
			@sprites["side2"].y = @y + startY;
			@sprites["side3"].x = @x + startX;
			@sprites["side3"].y = @y + @height - endY;
			@sprites["scroll0"].x = @x + (@width / 2) - 8;
			@sprites["scroll0"].y = @y + 8;
			@sprites["scroll1"].x = @x + 8;
			@sprites["scroll1"].y = @y + (@height / 2) - 8;
			@sprites["scroll2"].x = @x + @width - 16;
			@sprites["scroll2"].y = @y + (@height / 2) - 8;
			@sprites["scroll3"].x = @x + (@width / 2) - 8;
			@sprites["scroll3"].y = @y + @height - 16;
			@sprites["cursor"].x = @x + startX + @cursor_rect.x;
			@sprites["cursor"].y = @y + startY + @cursor_rect.y;
			if ((@compat & CompatBits.EXPAND_BACK) > 0 && @skinformat == 0) {
				// Compatibility mode: Expand background
				@sprites["back"].x = @x + 2;
				@sprites["back"].y = @y + 2;
			} else {
				@sprites["back"].x = @x + startX;
				@sprites["back"].y = @y + startY;
			}
		}
		if (changeBitmap && @_windowskin && !@_windowskin.disposed()) {
			if (@skinformat == 0) {
				@sprites["cursor"].x = @x + startX + @cursor_rect.x;
				@sprites["cursor"].y = @y + startY + @cursor_rect.y;
				width = @cursor_rect.width;
				height = @cursor_rect.height;
				if (width > 0 && height > 0) {
					cursorrects = new {
						// sides
						new Rect(cursorX + 2, cursorY + 0, 28, 2),
						new Rect(cursorX + 0, cursorY + 2, 2, 28),
						new Rect(cursorX + 30, cursorY + 2, 2, 28),
						new Rect(cursorX + 2, cursorY + 30, 28, 2),
						// corners
						new Rect(cursorX + 0, cursorY + 0, 2, 2),
						new Rect(cursorX + 30, cursorY + 0, 2, 2),
						new Rect(cursorX + 0, cursorY + 30, 2, 2),
						new Rect(cursorX + 30, cursorY + 30, 2, 2),
						// back
						new Rect(cursorX + 2, cursorY + 2, 28, 28);
					}
					margin = 2;
					fullmargin = 4;
					@cursorbitmap = ensureBitmap(@cursorbitmap, width, height);
					@cursorbitmap.clear;
					@sprites["cursor"].bitmap = @cursorbitmap;
					@sprites["cursor"].src_rect.set(0, 0, width, height);
					rect = new Rect(margin, margin, width - fullmargin, height - fullmargin);
					@cursorbitmap.stretch_blt(rect, @_windowskin, cursorrects[8]);
					@cursorbitmap.blt(0, 0, @_windowskin, cursorrects[4]);   // top left
					@cursorbitmap.blt(width - margin, 0, @_windowskin, cursorrects[5]);   // top right
					@cursorbitmap.blt(0, height - margin, @_windowskin, cursorrects[6]);   // bottom right
					@cursorbitmap.blt(width - margin, height - margin, @_windowskin, cursorrects[7]);   // bottom left
					rect = new Rect(margin, 0, width - fullmargin, margin);
					@cursorbitmap.stretch_blt(rect, @_windowskin, cursorrects[0]);
					rect = new Rect(0, margin, margin, height - fullmargin);
					@cursorbitmap.stretch_blt(rect, @_windowskin, cursorrects[1]);
					rect = new Rect(width - margin, margin, margin, height - fullmargin);
					@cursorbitmap.stretch_blt(rect, @_windowskin, cursorrects[2]);
					rect = new Rect(margin, height - margin, width - fullmargin, margin);
					@cursorbitmap.stretch_blt(rect, @_windowskin, cursorrects[3]);
				} else {
					@sprites["cursor"].visible = false;
					@sprites["cursor"].src_rect.set(0, 0, 0, 0);
				}
			}
			for (int i = 4; i < 4; i++) { //for '4' times do => |i|
				switch (i) {
					case 0:
						dwidth  = @width - startX - endX;
						dheight = startY;
						break;
					case 1:
						dwidth  = startX;
						dheight = @height - startY - endY;
						break;
					case 2:
						dwidth  = endX;
						dheight = @height - startY - endY;
						break;
					case 3:
						dwidth  = @width - startX - endX;
						dheight = endY;
						break;
				}
				@sidebitmaps[i] = ensureBitmap(@sidebitmaps[i], dwidth, dheight);
				@sprites[$"side{i}"].bitmap = @sidebitmaps[i];
				@sprites[$"side{i}"].src_rect.set(0, 0, dwidth, dheight);
				@sidebitmaps[i].clear;
				if (sideRects[i].width > 0 && sideRects[i].height > 0) {
					if ((@compat & CompatBits.STRETCH_SIDES) > 0 && @skinformat == 0) {
						// Compatibility mode: Stretch sides
						@sidebitmaps[i].stretch_blt(@sprites[$"side{i}"].src_rect,
																				@_windowskin, sideRects[i]);
					} else {
						tileBitmap(@sidebitmaps[i], @sprites[$"side{i}"].src_rect,
											@_windowskin, sideRects[i]);
					}
				}
			}
			if ((@compat & CompatBits.EXPAND_BACK) > 0 && @skinformat == 0) {
				// Compatibility mode: Expand background
				backwidth = @width - 4;
				backheight = @height - 4;
			} else {
				backwidth = @width - borderX;
				backheight = @height - borderY;
			}
			if (backwidth > 0 && backheight > 0) {
				@backbitmap = ensureBitmap(@backbitmap, backwidth, backheight);
				@sprites["back"].bitmap = @backbitmap;
				@sprites["back"].src_rect.set(0, 0, backwidth, backheight);
				@backbitmap.clear;
				if (@stretch) {
					@backbitmap.stretch_blt(@sprites["back"].src_rect, @_windowskin, backRect);
				} else {
					tileBitmap(@backbitmap, @sprites["back"].src_rect, @_windowskin, backRect);
				}
				if (blindsRect) {
					tileBitmap(@backbitmap, @sprites["back"].src_rect, @_windowskin, blindsRect);
				}
			} else {
				@sprites["back"].visible = false;
				@sprites["back"].src_rect.set(0, 0, 0, 0);
			}
		}
		if (@openness == 255) {
			@spritekeys.each do |k|
				sprite = @sprites[k];
				sprite.zoom_x = 1.0;
				sprite.zoom_y = 1.0;
			}
		} else {
			opn = @openness / 255.0;
			@spritekeys.each do |k|
				sprite = @sprites[k];
				ratio = (@height <= 0) ? 0 : (sprite.y - @y) / @height.to_f;
				sprite.zoom_y = opn;
				sprite.zoom_x = 1.0;
				sprite.oy = 0;
				sprite.y = (int)Math.Floor(@y + (@height / 2.0) + (@height * ratio * opn) - (@height / 2 * opn));
			}
		}
		i = 0;
		// Ensure Z order
		@spritekeys.each do |k|
			sprite = @sprites[k];
			y = sprite.y;
			sprite.y = i;
			sprite.oy = (sprite.zoom_y <= 0) ? 0 : (i - y) / sprite.zoom_y;
			sprite.zoom_x *= @zoom_x;
			sprite.zoom_y *= @zoom_y;
			sprite.x *= @zoom_x;
			sprite.y *= @zoom_y;
			sprite.x += (@offset_x / sprite.zoom_x);
			sprite.y += (@offset_y / sprite.zoom_y);
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class SpriteWindow_Base : SpriteWindow {
	public const int TEXT_PADDING = 4;   // In pixels

	public override void initialize(x, y, width, height) {
		base.initialize();
		self.x = x;
		self.y = y;
		self.width = width;
		self.height = height;
		self.z = 100;
		@curframe = MessageConfig.GetSystemFrame;
		@curfont = MessageConfig.GetSystemFontName;
		@sysframe = new AnimatedBitmap(@curframe);
		if (@curframe && !@curframe.empty()) RPG.Cache.retain(@curframe);
		@customskin = null;
		__setWindowskin(@sysframe.bitmap);
		__resolveSystemFrame;
		if (self.contents) SetSystemFont(self.contents);
	}

	public void __setWindowskin(skin) {
		if (skin && ((skin.width == 192 && skin.height == 128) ||   // RPGXP Windowskin
			(skin.width == 128 && skin.height == 128))) {              // RPGVX Windowskin
			self.skinformat = 0;
		} else {
			self.skinformat = 1;
		}
		self.windowskin = skin;
	}

	public void __resolveSystemFrame() {
		if (self.skinformat == 1) {
			if (!@resolvedFrame) {
				@resolvedFrame = MessageConfig.GetSystemFrame;
				@resolvedFrame = System.Text.RegularExpressions.Regex.Replace(@resolvedFrame, "\.[^\.\/\\]+$", "");
			}
			if (@resolvedFrame != "") self.loadSkinFile($"{@resolvedFrame}.txt");
		}
	}

	// Filename of windowskin to apply. Supports XP, VX, and animated skins.
	public void setSkin(skin) {
		@customskin&.dispose;
		@customskin = null;
		resolvedName = ResolveBitmap(skin);
		if (nil_or_empty(resolvedName)) return;
		@customskin = new AnimatedBitmap(resolvedName);
		RPG.Cache.retain(resolvedName);
		__setWindowskin(@customskin.bitmap);
		if (self.skinformat == 1) {
			skinbase = System.Text.RegularExpressions.Regex.Replace(resolvedName, "\.[^\.\/\\]+$", "", count: 1);
			self.loadSkinFile($"{skinbase}.txt");
		}
	}

	public void setSystemFrame() {
		@customskin&.dispose;
		@customskin = null;
		__setWindowskin(@sysframe.bitmap);
		__resolveSystemFrame;
	}

	public override void update() {
		base.update();
		if (self.windowskin) {
			if (@customskin) {
				if (@customskin.totalFrames > 1) {
					@customskin.update;
					__setWindowskin(@customskin.bitmap);
				}
			} else if (@sysframe) {
				if (@sysframe.totalFrames > 1) {
					@sysframe.update;
					__setWindowskin(@sysframe.bitmap);
				}
			}
		}
		if (@curframe != MessageConfig.GetSystemFrame) {
			@curframe = MessageConfig.GetSystemFrame;
			if (@sysframe && !@customskin) {
				@sysframe&.dispose;
				@sysframe = new AnimatedBitmap(@curframe);
				if (@curframe && !@curframe.empty()) RPG.Cache.retain(@curframe);
				@resolvedFrame = null;
				__setWindowskin(@sysframe.bitmap);
				__resolveSystemFrame;
			}
			begin;
				refresh;
			rescue NoMethodError;
			}
		}
		if (@curfont != MessageConfig.GetSystemFontName) {
			@curfont = MessageConfig.GetSystemFontName;
			if (self.contents && !self.contents.disposed()) {
				SetSystemFont(self.contents);
			}
			begin;
				refresh;
			rescue NoMethodError;
			}
		}
	}

	public override void dispose() {
		self.contents&.dispose;
		@sysframe.dispose;
		@customskin&.dispose;
		base.dispose();
	}
}
