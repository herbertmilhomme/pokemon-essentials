//===============================================================================
// Displays an icon bitmap in a window. Supports animated images.
//===============================================================================
public partial class IconWindow : SpriteWindow_Base {
	public int name		{ get { return _name; } }			protected int _name;

	public override void initialize(x, y, width, height, viewport = null) {
		base.initialize(x, y, width, height);
		self.viewport = viewport;
		self.contents = null;
		@name = "";
		@_iconbitmap = null;
	}

	public override void dispose() {
		clearBitmaps;
		base.dispose();
	}

	public override void update() {
		base.update();
		if (@_iconbitmap) {
			@_iconbitmap.update;
			self.contents = @_iconbitmap.bitmap;
		}
	}

	public void clearBitmaps() {
		@_iconbitmap&.dispose;
		@_iconbitmap = null;
		if (!self.disposed()) self.contents = null;
	}

	// Sets the icon's filename.  Alias for setBitmap.
	public int name { set {
		setBitmap(value);
		}	}

	// Sets the icon's filename.
	public void setBitmap(file, hue = 0) {
		clearBitmaps;
		@name = file;
		if (file.null()) return;
		if (file == "") {
			@_iconbitmap = null;
		} else {
			@_iconbitmap = new AnimatedBitmap(file, hue);
			// for compatibility
			self.contents = @_iconbitmap ? @_iconbitmap.bitmap : null;
		}
	}
}

//===============================================================================
// Displays an icon bitmap in a window. Supports animated images.
// Accepts bitmaps and paths to bitmap files in its constructor.
//===============================================================================
public partial class PictureWindow : SpriteWindow_Base {
	public override void initialize(pathOrBitmap) {
		base.initialize(0, 0, 32, 32);
		self.viewport = viewport;
		self.contents = null;
		@_iconbitmap = null;
		setBitmap(pathOrBitmap);
	}

	public override void dispose() {
		clearBitmaps;
		base.dispose();
	}

	public override void update() {
		base.update();
		if (@_iconbitmap) {
			if (@_iconbitmap.is_a(Bitmap)) {
				self.contents = @_iconbitmap;
			} else {
				@_iconbitmap.update;
				self.contents = @_iconbitmap.bitmap;
			}
		}
	}

	public void clearBitmaps() {
		@_iconbitmap&.dispose;
		@_iconbitmap = null;
		if (!self.disposed()) self.contents = null;
	}

	// Sets the icon's bitmap or filename. (hue parameter
	// is ignored unless pathOrBitmap is a filename)
	public void setBitmap(pathOrBitmap, hue = 0) {
		clearBitmaps;
		if (pathOrBitmap && pathOrBitmap != "") {
			switch (pathOrBitmap) {
				case Bitmap:
					@_iconbitmap = pathOrBitmap;
					self.contents = @_iconbitmap;
					self.width = @_iconbitmap.width + self.borderX;
					self.height = @_iconbitmap.height + self.borderY;
					break;
				case AnimatedBitmap:
					@_iconbitmap = pathOrBitmap;
					self.contents = @_iconbitmap.bitmap;
					self.width = @_iconbitmap.bitmap.width + self.borderX;
					self.height = @_iconbitmap.bitmap.height + self.borderY;
					break;
				default:
					@_iconbitmap = new AnimatedBitmap(pathOrBitmap, hue);
					self.contents = @_iconbitmap&.bitmap;
					self.width = self.borderX + (@_iconbitmap&.bitmap&.width || 32);
					self.height = self.borderY + (@_iconbitmap&.bitmap&.height || 32);
					break;
			}
		} else {
			@_iconbitmap = null;
			self.width = 32 + self.borderX;
			self.height = 32 + self.borderY;
		}
	}
}
