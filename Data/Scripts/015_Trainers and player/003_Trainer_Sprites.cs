//===============================================================================
// Walking charset, for use in text entry screens and load game screen
//===============================================================================
public partial class TrainerWalkingCharSprite : Sprite {
	public int anim_duration		{ get { return _anim_duration; } set { _anim_duration = value; } }			protected int _anim_duration;

	// Default time in seconds for one animation cycle of a charset. The icon for a
	// storage box is 0.4 instead (set manually).
	public const int ANIMATION_DURATION = 0.5;

	public override void initialize(charset, viewport = null) {
		base.initialize(viewport);
		@animbitmap = null;
		self.charset = charset;
		@current_frame = 0;   // Current pattern
		@anim_duration = ANIMATION_DURATION;
	}

	public override void dispose() {
		@animbitmap&.dispose;
		base.dispose();
	}

	public int charset { set {
		@animbitmap&.dispose;
		@animbitmap = null;
		bitmapFileName = string.Format("Graphics/Characters/{0}", value);
		@charset = ResolveBitmap(bitmapFileName);
		if (@charset) {
			@animbitmap = new AnimatedBitmap(@charset);
			self.bitmap = @animbitmap.bitmap;
			self.src_rect.set(0, 0, self.bitmap.width / 4, self.bitmap.height / 4);
		} else {
			self.bitmap = null;
			}
	}
	}

	// Used for the box icon in the naming screen.
	public int altcharset { set {
		@animbitmap&.dispose;
		@animbitmap = null;
		@charset = ResolveBitmap(value);
		if (@charset) {
			@animbitmap = new AnimatedBitmap(@charset);
			self.bitmap = @animbitmap.bitmap;
			self.src_rect.set(0, 0, self.bitmap.width / 4, self.bitmap.height);
		} else {
			self.bitmap = null;
			}	}
	}

	public void update_frame() {
		@current_frame = (int)Math.Floor(4 * (System.uptime % @anim_duration) / @anim_duration);
	}

	public override void update() {
		@updating = true;
		base.update();
		if (@animbitmap) {
			@animbitmap.update;
			self.bitmap = @animbitmap.bitmap;
		}
		// Update animation
		update_frame;
		self.src_rect.x = self.src_rect.width * @current_frame;
		@updating = false;
	}
}
