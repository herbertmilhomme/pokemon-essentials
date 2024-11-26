//===============================================================================
//
//===============================================================================
public partial class WindowCursorRect : Rect {
	public override void initialize(window) {
		base.initialize(0, 0, 0, 0);
		@window = window;
	}

	public void empty() {
		unless (needs_update(0, 0, 0, 0)) return;
		set(0, 0, 0, 0);
	}

	public bool empty() {
		return self.x == 0 && self.y == 0 && self.width == 0 && self.height == 0;
	}

	public override void set(x, y, width, height) {
		unless (needs_update(x, y, width, height)) return;
		base.set(x, y, width, height);
		@window.width = @window.width;
	}

	public override int height { set {
		base.height = value;
		@window.width = @window.width;
		}
	}

	public override int width { set {
		base.width = value;
		@window.width = @window.width;
		}
	}

	public override int x { set {
		base.x = value;
		@window.width = @window.width;
		}
	}

	public override int y { set {
		base.y = value;
		@window.width = @window.width;
		}
	}

	//-----------------------------------------------------------------------------

	private;

	public bool needs_update(x, y, width, height) {
		return self.x != x || self.y != y || self.width != width || self.height != height;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Window {
	public int tone		{ get { return _tone; } }			protected int _tone;
	public int color		{ get { return _color; } }			protected int _color;
	public int blend_type		{ get { return _blend_type; } }			protected int _blend_type;
	public int contents_blend_type		{ get { return _contents_blend_type; } }			protected int _contents_blend_type;
	public int viewport		{ get { return _viewport; } }			protected int _viewport;
	public int contents		{ get { return _contents; } }			protected int _contents;
	public int ox		{ get { return _ox; } }			protected int _ox;
	public int oy		{ get { return _oy; } }			protected int _oy;
	public int x		{ get { return _x; } }			protected int _x;
	public int y		{ get { return _y; } }			protected int _y;
	public int z		{ get { return _z; } }			protected int _z;
	public int width		{ get { return _width; } }			protected int _width;
	public int active		{ get { return _active; } }			protected int _active;
	public int pause		{ get { return _pause; } }			protected int _pause;
	public int height		{ get { return _height; } }			protected int _height;
	public int opacity		{ get { return _opacity; } }			protected int _opacity;
	public int back_opacity		{ get { return _back_opacity; } }			protected int _back_opacity;
	public int contents_opacity		{ get { return _contents_opacity; } }			protected int _contents_opacity;
	public int visible		{ get { return _visible; } }			protected int _visible;
	public int cursor_rect		{ get { return _cursor_rect; } }			protected int _cursor_rect;
	public int openness		{ get { return _openness; } }			protected int _openness;
	public int stretch		{ get { return _stretch; } }			protected int _stretch;

	public void windowskin() {
		@_windowskin;
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
		@sidebitmaps = new {null, null, null, null};
		@cursorbitmap = null;
		@bgbitmap = null;
		@viewport = viewport;
		@spritekeys.each do |i|
			@sprites[i] = new Sprite(@viewport);
		}
		@disposed = false;
		@tone = new Tone(0, 0, 0);
		@color = new Color(0, 0, 0, 0);
		@blankcontents = new Bitmap(1, 1); // RGSS2 requires this
		@contents = @blankcontents;
		@_windowskin = null;
		@rpgvx = false; // Set to true to emulate RPGVX windows
		@x = 0;
		@y = 0;
		@width = 0;
		@openness = 255;
		@height = 0;
		@ox = 0;
		@oy = 0;
		@z = 0;
		@stretch = true;
		@visible = true;
		@active = true;
		@blend_type = 0;
		@contents_blend_type = 0;
		@opacity = 255;
		@back_opacity = 255;
		@contents_opacity = 255;
		@cursor_rect = new WindowCursorRect(self);
		@cursoropacity = 255;
		@pause = false;
		@pauseopacity = 255;
		@pauseframe = 0;
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
			@_contents = null;
			@disposed = true;
		}
	}

	public int openness { set {
		@openness = value;
		if (@openness < 0) @openness = 0;
		if (@openness > 255) @openness = 255;
		privRefresh;
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
			@sprites[i].dispose;
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
		@contents = value;
		privRefresh;
		}
	}

	public int windowskin { set {
		@_windowskin = value;
		if (value.is_a(Bitmap) && !value.disposed() && value.width == 128) {
			@rpgvx = true;
		} else {
			@rpgvx = false;
			}
	}
		privRefresh(true);
	}

	public int ox { set {
		@ox = value;
		privRefresh;
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

	public int oy { set {
		@oy = value;
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
		privRefresh;
		}
	}

	public int x { set {
		@x = value;
		privRefresh;
		}
	}

	public int y { set {
		@y = value;
		privRefresh;
		}
	}

	public int opacity { set {
		@opacity = value;
		if (@opacity < 0) @opacity = 0;
		if (@opacity > 255) @opacity = 255;
		privRefresh;
		}
	}

	public int back_opacity { set {
		@back_opacity = value;
		if (@back_opacity < 0) @back_opacity = 0;
		if (@back_opacity > 255) @back_opacity = 255;
		privRefresh;
		}
	}

	public int contents_opacity { set {
		@contents_opacity = value;
		if (@contents_opacity < 0) @contents_opacity = 0;
		if (@contents_opacity > 255) @contents_opacity = 255;
		privRefresh;
		}
	}

	public int tone { set {
		@tone = value;
		privRefresh;
		}
	}

	public int color { set {
		@color = value;
		privRefresh;
		}
	}

	public int blend_type { set {
		@blend_type = value;
		privRefresh;
		}
	}

	public void flash(color, duration) {
		if (disposed()) return;
		@sprites.each do |i|
			i[1].flash(color, duration);
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
			if (!@cursor_rect.empty()) mustchange = true;
		} else {
			if (@cursoropacity != 128) mustchange = true;
			@cursoropacity = 128;
		}
		if (@pause) {
			@pauseframe = (System.uptime * 5).ToInt() % 4;   // 4 frames, 5 frames per second
			@pauseopacity = (int)Math.Min(@pauseopacity + 64, 255);
			mustchange = true;
		}
		if (mustchange) privRefresh;
		@sprites.each do |i|
			i[1].update;
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

	public void privRefresh(changeBitmap = false) {
		if (self.disposed()) return;
		backopac = self.back_opacity * self.opacity / 255;
		contopac = self.contents_opacity;
		cursoropac = @cursoropacity * contopac / 255;
		for (int i = 4; i < 4; i++) { //for '4' times do => |i|
			@sprites[$"corner{i}"].bitmap = @_windowskin;
			@sprites[$"scroll{i}"].bitmap = @_windowskin;
		}
		@sprites["pause"].bitmap = @_windowskin;
		@sprites["contents"].bitmap = @contents;
		if (@_windowskin && !@_windowskin.disposed()) {
			for (int i = 4; i < 4; i++) { //for '4' times do => |i|
				@sprites[$"corner{i}"].opacity = @opacity;
				@sprites[$"corner{i}"].tone = @tone;
				@sprites[$"corner{i}"].color = @color;
				@sprites[$"corner{i}"].blend_type = @blend_type;
				@sprites[$"corner{i}"].visible = @visible;
				@sprites[$"side{i}"].opacity = @opacity;
				@sprites[$"side{i}"].tone = @tone;
				@sprites[$"side{i}"].color = @color;
				@sprites[$"side{i}"].blend_type = @blend_type;
				@sprites[$"side{i}"].visible = @visible;
				@sprites[$"scroll{i}"].opacity = @opacity;
				@sprites[$"scroll{i}"].tone = @tone;
				@sprites[$"scroll{i}"].blend_type = @blend_type;
				@sprites[$"scroll{i}"].color = @color;
				@sprites[$"scroll{i}"].visible = @visible;
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
			@sprites["back"].visible = @visible;
			@sprites["contents"].visible = @visible && @openness == 255;
			@sprites["pause"].visible = @visible && @pause;
			@sprites["cursor"].visible = @visible && @openness == 255;
			hascontents = (@contents && !@contents.disposed());
			@sprites["scroll0"].visible = @visible && hascontents && @oy > 0;
			@sprites["scroll1"].visible = @visible && hascontents && @ox > 0;
			@sprites["scroll2"].visible = @visible && hascontents &&
																		(@contents.width - @ox) > @width - 32
			@sprites["scroll3"].visible = @visible && hascontents &&
																		(@contents.height - @oy) > @height - 32
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
		@sprites.each do |i|
			i[1].z = @z;
		}
		if (@rpgvx) {
			@sprites["cursor"].z = @z; // For Compatibility
			@sprites["contents"].z = @z; // For Compatibility
			@sprites["pause"].z = @z; // For Compatibility
		} else {
			@sprites["cursor"].z = @z + 1; // For Compatibility
			@sprites["contents"].z = @z + 2; // For Compatibility
			@sprites["pause"].z = @z + 2; // For Compatibility
		}
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
		sideRects = new {
			new Rect(trimX + 16, trimY + 0, 32, 16),
			new Rect(trimX, trimY + 16, 16, 32),
			new Rect(trimX + 48, trimY + 16, 16, 32),
			new Rect(trimX + 16, trimY + 48, 32, 16);
		}
		if (@width > 32 && @height > 32) {
			@sprites["contents"].src_rect.set(@ox, @oy, @width - 32, @height - 32);
		} else {
			@sprites["contents"].src_rect.set(0, 0, 0, 0);
		}
		pauseRects = new {
			trimX + 32, trimY + 64,
			trimX + 48, trimY + 64,
			trimX + 32, trimY + 80,
			trimX + 48, trimY + 80;
		}
		pauseWidth = 16;
		pauseHeight = 16;
		@sprites["pause"].src_rect.set(pauseRects[@pauseframe * 2],
																	pauseRects[(@pauseframe * 2) + 1],
																	pauseWidth,
																	pauseHeight);
		@sprites["pause"].x = @x + (@width / 2) - (pauseWidth / 2);
		@sprites["pause"].y = @y + @height - 16; // 16 refers to skin margin
		@sprites["contents"].x = @x + 16;
		@sprites["contents"].y = @y + 16;
		@sprites["corner0"].x = @x;
		@sprites["corner0"].y = @y;
		@sprites["corner1"].x = @x + @width - 16;
		@sprites["corner1"].y = @y;
		@sprites["corner2"].x = @x;
		@sprites["corner2"].y = @y + @height - 16;
		@sprites["corner3"].x = @x + @width - 16;
		@sprites["corner3"].y = @y + @height - 16;
		@sprites["side0"].x = @x + 16;
		@sprites["side0"].y = @y;
		@sprites["side1"].x = @x;
		@sprites["side1"].y = @y + 16;
		@sprites["side2"].x = @x + @width - 16;
		@sprites["side2"].y = @y + 16;
		@sprites["side3"].x = @x + 16;
		@sprites["side3"].y = @y + @height - 16;
		@sprites["scroll0"].x = @x + (@width / 2) - 8;
		@sprites["scroll0"].y = @y + 8;
		@sprites["scroll1"].x = @x + 8;
		@sprites["scroll1"].y = @y + (@height / 2) - 8;
		@sprites["scroll2"].x = @x + @width - 16;
		@sprites["scroll2"].y = @y + (@height / 2) - 8;
		@sprites["scroll3"].x = @x + (@width / 2) - 8;
		@sprites["scroll3"].y = @y + @height - 16;
		@sprites["back"].x = @x + 2;
		@sprites["back"].y = @y + 2;
		@sprites["cursor"].x = @x + 16 + @cursor_rect.x;
		@sprites["cursor"].y = @y + 16 + @cursor_rect.y;
		if (changeBitmap && @_windowskin && !@_windowskin.disposed()) {
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
			for (int i = 4; i < 4; i++) { //for '4' times do => |i|
				dwidth  = new []{0, 3}.Contains(i) ? @width - 32 : 16;
				dheight = new []{0, 3}.Contains(i) ? 16 : @height - 32;
				@sidebitmaps[i] = ensureBitmap(@sidebitmaps[i], dwidth, dheight);
				@sprites[$"side{i}"].bitmap = @sidebitmaps[i];
				@sprites[$"side{i}"].src_rect.set(0, 0, dwidth, dheight);
				@sidebitmaps[i].clear;
				if (sideRects[i].width > 0 && sideRects[i].height > 0) {
					@sidebitmaps[i].stretch_blt(@sprites[$"side{i}"].src_rect, @_windowskin, sideRects[i]);
				}
			}
			backwidth = @width - 4;
			backheight = @height - 4;
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
				sprite.zoom_y = 1.0;
			}
		} else {
			opn = @openness / 255.0;
			@spritekeys.each do |k|
				sprite = @sprites[k];
				ratio = (@height <= 0) ? 0 : (sprite.y - @y) / @height.to_f;
				sprite.zoom_y = opn;
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
		}
	}
}
