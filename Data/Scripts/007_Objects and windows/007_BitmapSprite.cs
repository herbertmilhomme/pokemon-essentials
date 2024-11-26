//===============================================================================
// Sprite class that maintains a bitmap of its own.
// This bitmap can't be changed to a different one.
//===============================================================================
public partial class BitmapSprite : Sprite {
	public override void initialize(width, height, viewport = null) {
		base.initialize(viewport);
		self.bitmap = new Bitmap(width, height);
		@initialized = true;
	}

	public override int bitmap { set {
		if (!@initialized) base.bitmap = value;;
		}
	}

	public override void dispose() {
		if (!self.disposed()) self.bitmap.dispose;
		base.dispose();
	}
}

//===============================================================================
//
//===============================================================================
public partial class AnimatedSprite : Sprite {
	public int frame		{ get { return _frame; } }			protected int _frame;
	public int framewidth		{ get { return _framewidth; } }			protected int _framewidth;
	public int frameheight		{ get { return _frameheight; } }			protected int _frameheight;
	public int framecount		{ get { return _framecount; } }			protected int _framecount;
	public int animname		{ get { return _animname; } }			protected int _animname;

	// frameskip is in 1/20ths of a second, and is the time between frame changes.
	public void initializeLong(animname, framecount, framewidth, frameheight, frameskip) {
		@animname = BitmapName(animname);
		@time_per_frame = (int)Math.Max(1, frameskip) / 20.0;
		if (framewidth == 0) raise _INTL("Frame width is 0");
		if (frameheight == 0) raise _INTL("Frame height is 0");
		begin;
			@animbitmap = new AnimatedBitmap(animname).deanimate;
		rescue;
			@animbitmap = new Bitmap(framewidth, frameheight);
		}
		if (@animbitmap.width % framewidth != 0) {
			Debug.LogError(_INTL("Bitmap's width ({1}) is not a multiple of frame width ({2}) [Bitmap={3}]",);
			//throw new Exception(_INTL("Bitmap's width ({1}) is not a multiple of frame width ({2}) [Bitmap={3}]",);
									@animbitmap.width, framewidth, animname);
		}
		if (@animbitmap.height % frameheight != 0) {
			Debug.LogError(_INTL("Bitmap's height ({1}) is not a multiple of frame height ({2}) [Bitmap={3}]",);
			//throw new Exception(_INTL("Bitmap's height ({1}) is not a multiple of frame height ({2}) [Bitmap={3}]",);
									@animbitmap.height, frameheight, animname);
		}
		@framecount = framecount;
		@framewidth = framewidth;
		@frameheight = frameheight;
		@framesperrow = @animbitmap.width / @framewidth;
		@playing = false;
		self.bitmap = @animbitmap;
		self.src_rect.width = @framewidth;
		self.src_rect.height = @frameheight;
		self.frame = 0;
	}

	// Shorter version of AnimatedSprite. All frames are placed on a single row
	// of the bitmap, so that the width and height need not be defined beforehand.
	// frameskip is in 1/20ths of a second, and is the time between frame changes.
	public void initializeShort(animname, framecount, frameskip) {
		@animname = BitmapName(animname);
		@time_per_frame = (int)Math.Max(1, frameskip) / 20.0;
		begin;
			@animbitmap = new AnimatedBitmap(animname).deanimate;
		rescue;
			@animbitmap = new Bitmap(framecount * 4, 32);
		}
		if (@animbitmap.width % framecount != 0) {
			Debug.LogError(_INTL("Bitmap's width ({1}) is not a multiple of frame count ({2}) [Bitmap={3}]",);
			//throw new Exception(_INTL("Bitmap's width ({1}) is not a multiple of frame count ({2}) [Bitmap={3}]",);
									@animbitmap.width, framewidth, animname);
		}
		@framecount = framecount;
		@framewidth = @animbitmap.width / @framecount;
		@frameheight = @animbitmap.height;
		@framesperrow = framecount;
		@playing = false;
		self.bitmap = @animbitmap;
		self.src_rect.width = @framewidth;
		self.src_rect.height = @frameheight;
		self.frame = 0;
	}

	public override void override initialize(*args() {
		if (args.length == 1) {
			base.override()(args[0][3]);
			initializeShort(args[0][0], args[0][1], args[0][2]);
		} else {
			base.initialize(args[5]);
			initializeLong(args[0], args[1], args[2], args[3], args[4]);
		}
	}

	public static void create(animname, framecount, frameskip, viewport = null) {
		return new self(new {animname, framecount, frameskip, viewport});
	}

	public override void dispose() {
		if (disposed()) return;
		@animbitmap.dispose;
		@animbitmap = null;
		base.dispose();
	}

	public bool playing() {
		return @playing;
	}

	public int frame { set {
		@frame = value;
		self.src_rect.x = @frame % @framesperrow * @framewidth;
		self.src_rect.y = @frame / @framesperrow * @frameheight;
		}
	}

	public void start() {
		@playing = true;
	}

	alias play start;

	public void stop() {
		@playing = false;
	}

	public override void update() {
		base.update();
		if (@playing) {
			new_frame = (System.uptime / @time_per_frame).ToInt() % self.framecount;
			if (self.frame != new_frame) self.frame = new_frame;
		}
	}
}

//===============================================================================
// Displays an icon bitmap in a sprite. Supports animated images.
//===============================================================================
public partial class IconSprite : Sprite {
	public int name		{ get { return _name; } }			protected int _name;

	public override void override initialize(*args() {
		switch (args.length) {
			case 0:
				super(null);
				self.bitmap = null;
				break;
			case 1:
				super(args[0]);
				self.bitmap = null;
				break;
			case 2:
				base.override()(null);
				self.x = args[0];
				self.y = args[1];
				break;
			default:
				base.initialize(args[2]);
				self.x = args[0];
				self.y = args[1];
				break;
		}
		@name = "";
		@_iconbitmap = null;
	}

	public override void dispose() {
		clearBitmaps;
		base.dispose();
	}

	// Sets the icon's filename.  Alias for setBitmap.
	public int name { set {
		setBitmap(value);
		}	}

	// Sets the icon's filename.
	public void setBitmap(file, hue = 0) {
		oldrc = self.src_rect;
		clearBitmaps;
		@name = file;
		if (file.null()) return;
		if (file == "") {
			@_iconbitmap = null;
		} else {
			@_iconbitmap = new AnimatedBitmap(file, hue);
			// for compatibility
			self.bitmap = @_iconbitmap ? @_iconbitmap.bitmap : null;
			self.src_rect = oldrc;
		}
	}

	public void clearBitmaps() {
		@_iconbitmap&.dispose;
		@_iconbitmap = null;
		if (!self.disposed()) self.bitmap = null;
	}

	public override void update() {
		base.update();
		if (!@_iconbitmap) return;
		@_iconbitmap.update;
		if (self.bitmap != @_iconbitmap.bitmap) {
			oldrc = self.src_rect;
			self.bitmap = @_iconbitmap.bitmap;
			self.src_rect = oldrc;
		}
	}
}

//===============================================================================
// Sprite class that stores multiple bitmaps, and displays only one at once.
//===============================================================================
public partial class ChangelingSprite : Sprite {
	public override void initialize(x = 0, y = 0, viewport = null) {
		base.initialize(viewport);
		self.x = x;
		self.y = y;
		@bitmaps = new List<string>();
		@currentBitmap = null;
	}

	public void addBitmap(key, path) {
		@bitmaps[key]&.dispose;
		@bitmaps[key] = new AnimatedBitmap(path);
	}

	public void changeBitmap(key) {
		@currentBitmap = @bitmaps[key];
		self.bitmap = (@currentBitmap) ? @currentBitmap.bitmap : null;
	}

	public void dispose() {
		if (disposed()) return;
		@bitmaps.each_value(bm => bm.dispose);
		@bitmaps.clear;
		super;
	}

	public void update() {
		if (disposed()) return;
		@bitmaps.each_value(bm => bm.update);
		self.bitmap = (@currentBitmap) ? @currentBitmap.bitmap : null;
	}
}
