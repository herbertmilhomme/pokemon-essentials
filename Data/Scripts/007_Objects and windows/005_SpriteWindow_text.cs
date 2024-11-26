//===============================================================================
// Represents a window with no formatting capabilities. Its text color can be set,
// though, and line breaks are supported, but the text is generally unformatted.
//===============================================================================
public partial class Window_UnformattedTextPokemon : SpriteWindow_Base {
	public int text		{ get { return _text; } }			protected int _text;
	public int baseColor		{ get { return _baseColor; } }			protected int _baseColor;
	public int shadowColor		{ get { return _shadowColor; } }			protected int _shadowColor;
	// Letter-by-letter mode.  This mode is not supported in this class.
	public int letterbyletter		{ get { return _letterbyletter; } set { _letterbyletter = value; } }			protected int _letterbyletter;

	public int text { set {
		@text = value;
		refresh;
		}
	}

	public int baseColor { set {
		@baseColor = value;
		refresh;
		}
	}

	public int shadowColor { set {
		@shadowColor = value;
		refresh;
		}
	}

	public override void initialize(text = "") {
		base.initialize(0, 0, 33, 33);
		self.contents = new Bitmap(1, 1);
		SetSystemFont(self.contents);
		@text = text;
		@letterbyletter = false; // Not supported in this class
		@baseColor, @shadowColor = getDefaultTextColors(self.windowskin);
		resizeToFit(text);
	}

	public static void newWithSize(text, x, y, width, height, viewport = null) {
		ret = new self(text);
		ret.x = x;
		ret.y = y;
		ret.width = width;
		ret.height = height;
		ret.viewport = viewport;
		ret.refresh;
		return ret;
	}

	// maxwidth is maximum acceptable window width.
	public void resizeToFitInternal(text, maxwidth) {
		dims = new {0, 0};
		cwidth = maxwidth < 0 ? Graphics.width : maxwidth;
		getLineBrokenChunks(self.contents, text,
												cwidth - self.borderX - SpriteWindow_Base.TEXT_PADDING, dims, true);
		return dims;
	}

	public void setTextToFit(text, maxwidth = -1) {
		resizeToFit(text, maxwidth);
		self.text = text;
	}

	// maxwidth is maximum acceptable window width.
	public void resizeToFit(text, maxwidth = -1) {
		dims = resizeToFitInternal(text, maxwidth);
		self.width = dims[0] + self.borderX + SpriteWindow_Base.TEXT_PADDING;
		self.height = dims[1] + self.borderY;
		refresh;
	}

	// width is current window width.
	public void resizeHeightToFit(text, width = -1) {
		dims = resizeToFitInternal(text, width);
		self.width  = (width < 0) ? Graphics.width : width;
		self.height = dims[1] + self.borderY;
		refresh;
	}

	public override void setSkin(skin) {
		base.setSkin(skin);
		privRefresh(true);
		oldbaser = @baseColor.red;
		oldbaseg = @baseColor.green;
		oldbaseb = @baseColor.blue;
		oldbasea = @baseColor.alpha;
		oldshadowr = @shadowColor.red;
		oldshadowg = @shadowColor.green;
		oldshadowb = @shadowColor.blue;
		oldshadowa = @shadowColor.alpha;
		@baseColor, @shadowColor = getDefaultTextColors(self.windowskin);
		if ((oldbaser != @baseColor.red || oldbaseg != @baseColor.green ||
			oldbaseb != @baseColor.blue || oldbasea != @baseColor.alpha ||
			oldshadowr != @shadowColor.red || oldshadowg != @shadowColor.green ||
			oldshadowb != @shadowColor.blue || oldshadowa != @shadowColor.alpha) {
			self.text = self.text;
		}
	}

	public void refresh() {
		self.contents = DoEnsureBitmap(self.contents, self.width - self.borderX,
																		self.height - self.borderY);
		self.contents.clear;
		drawTextEx(self.contents, 0, -2, self.contents.width, 0,   // TEXT OFFSET
							System.Text.RegularExpressions.Regex.Replace(@text, "\r", ""), @baseColor, @shadowColor);
	}
}

//===============================================================================
//
//===============================================================================
public partial class Window_AdvancedTextPokemon : SpriteWindow_Base {
	public int text		{ get { return _text; } }			protected int _text;
	public int baseColor		{ get { return _baseColor; } }			protected int _baseColor;
	public int shadowColor		{ get { return _shadowColor; } }			protected int _shadowColor;
	public int letterbyletter		{ get { return _letterbyletter; } set { _letterbyletter = value; } }			protected int _letterbyletter;
	public int waitcount		{ get { return _waitcount; } }			protected int _waitcount;

	public override void initialize(text = "") {
		@cursorMode         = MessageConfig.CURSOR_POSITION;
		@endOfText          = null;
		@scrollstate        = 0;
		@scrollY            = 0;
		@scroll_timer_start = null;
		@realframes         = 0;
		@nodraw             = false;
		@lineHeight         = 32;
		@linesdrawn         = 0;
		@bufferbitmap       = null;
		@letterbyletter     = false;
		@starting           = true;
		@displaying         = false;
		@lastDrawnChar      = -1;
		@fmtchars           = new List<string>();
		@text_delay_changed = false;
		@text_delay         = MessageConfig.GetTextSpeed;
		base.initialize(0, 0, 33, 33);
		@pausesprite        = null;
		@text               = "";
		self.contents = new Bitmap(1, 1);
		SetSystemFont(self.contents);
		self.resizeToFit(text, Graphics.width);
		@baseColor, @shadowColor = getDefaultTextColors(self.windowskin);
		self.text           = text;
		@starting           = false;
	}

	public static void newWithSize(text, x, y, width, height, viewport = null) {
		ret = new self(text);
		ret.x        = x;
		ret.y        = y;
		ret.width    = width;
		ret.height   = height;
		ret.viewport = viewport;
		return ret;
	}

	public override void dispose() {
		if (disposed()) return;
		@pausesprite&.dispose;
		@pausesprite = null;
		base.dispose();
	}

	public int waitcount { set {
		@waitcount = (value <= 0) ? 0 : value;
		if (!@wait_timer_start && value > 0) @wait_timer_start = System.uptime;
		}
	}

	public int cursorMode		{ get { return _cursorMode; } }			protected int _cursorMode;

	public int cursorMode { set {
		@cursorMode = value;
		moveCursor;
		}
	}

	public int lineHeight { set {
		@lineHeight = value;
		self.text = self.text;
		}
	}

	public int baseColor { set {
		@baseColor = value;
		refresh;
		}
	}

	public int shadowColor { set {
		@shadowColor = value;
		refresh;
		}
	}

	// Delay in seconds between two adjacent characters appearing.
	public void textspeed() {
		return @text_delay;
	}

	public int textspeed { set {
		if (@text_delay != value) @text_delay_changed = true;
		@text_delay = value;
		}
	}

	public override int width { set {
		base.width();
		if (!@starting) self.text = self.text;
		}
	}

	public override int height { set {
		base.height();
		if (!@starting) self.text = self.text;
		}
	}

	public void resizeToFit(text, maxwidth = -1) {
		dims = resizeToFitInternal(text, maxwidth);
		oldstarting = @starting;
		@starting = true;
		self.width  = dims[0] + self.borderX + SpriteWindow_Base.TEXT_PADDING;
		self.height = dims[1] + self.borderY;
		@starting = oldstarting;
		redrawText;
	}

	public void resizeToFit2(text, maxwidth, maxheight) {
		dims = resizeToFitInternal(text, maxwidth);
		oldstarting = @starting;
		@starting = true;
		self.width  = (int)Math.Min(dims[0] + self.borderX + SpriteWindow_Base.TEXT_PADDING, maxwidth);
		self.height = (int)Math.Min(dims[1] + self.borderY, maxheight);
		@starting = oldstarting;
		redrawText;
	}

	public void resizeToFitInternal(text, maxwidth) {
		dims = new {0, 0};
		cwidth = (maxwidth < 0) ? Graphics.width : maxwidth;
		chars = getFormattedTextForDims(self.contents, 0, 0,
																		cwidth - self.borderX - 2 - 6, -1, text, @lineHeight, true);
		foreach (var ch in chars) { //'chars.each' do => |ch|
			dims[0] = (int)Math.Max(dims[0], ch[1] + ch[3]);
			dims[1] = (int)Math.Max(dims[1], ch[2] + ch[4]);
		}
		return dims;
	}

	public void resizeHeightToFit(text, width = -1) {
		dims = resizeToFitInternal(text, width);
		oldstarting = @starting;
		@starting = true;
		self.width  = (width < 0) ? Graphics.width : width;
		self.height = dims[1] + self.borderY;
		@starting = oldstarting;
		redrawText;
	}

	public override void setSkin(skin, redrawText = true) {
		base.setSkin(skin);
		privRefresh(true);
		oldbaser = @baseColor.red;
		oldbaseg = @baseColor.green;
		oldbaseb = @baseColor.blue;
		oldbasea = @baseColor.alpha;
		oldshadowr = @shadowColor.red;
		oldshadowg = @shadowColor.green;
		oldshadowb = @shadowColor.blue;
		oldshadowa = @shadowColor.alpha;
		@baseColor, @shadowColor = getDefaultTextColors(self.windowskin);
		if ((redrawText &&
			(oldbaser != @baseColor.red || oldbaseg != @baseColor.green ||
			oldbaseb != @baseColor.blue || oldbasea != @baseColor.alpha ||
			oldshadowr != @shadowColor.red || oldshadowg != @shadowColor.green ||
			oldshadowb != @shadowColor.blue || oldshadowa != @shadowColor.alpha)) {
			setText(self.text);
		}
	}

	public void setTextToFit(text, maxwidth = -1) {
		resizeToFit(text, maxwidth);
		self.text = text;
	}

	public int text { set {
		setText(value);
		}
	}

	public void setText(value) {
		@waitcount = 0;
		@wait_timer_start = null;
		@display_timer = 0.0;
		@display_last_updated = System.uptime;
		@curchar = 0;
		@drawncurchar = -1;
		@lastDrawnChar = -1;
		@text = value;
		@textlength = unformattedTextLength(value);
		@scrollstate = 0;
		@scrollY = 0;
		@scroll_timer_start = null;
		@linesdrawn = 0;
		@realframes = 0;
		@textchars = new List<string>();
		width = 1;
		height = 1;
		numlines = 0;
		visiblelines = (self.height - self.borderY) / @lineHeight;
		if (value.length == 0) {
			@fmtchars = new List<string>();
			@bitmapwidth = width;
			@bitmapheight = height;
			@numtextchars = 0;
		} else {
			if (@letterbyletter) {
				@fmtchars = new List<string>();
				fmt = getFormattedText(self.contents, 0, 0,
															self.width - self.borderX - SpriteWindow_Base.TEXT_PADDING, -1,
															shadowc3tag(@baseColor, @shadowColor) + value, @lineHeight, true);
				@oldfont = self.contents.font.clone;
				foreach (var ch in fmt) { //'fmt.each' do => |ch|
					chx = ch[1] + ch[3];
					chy = ch[2] + ch[4];
					if (width < chx) width  = chx;
					if (height < chy) height = chy;
					if (!ch[5] && ch[0] == "\n") {
						numlines += 1;
						if (numlines >= visiblelines) {
							fclone = ch.clone;
							fclone[0] = "\1";
							@fmtchars.Add(fclone);
							@textchars.Add("\1");
						}
					}
					// Don't add newline characters, since they
					// can slow down letter-by-letter display
					if (ch[5] || (ch[0] != "\r")) {
						@fmtchars.Add(ch);
						@textchars.Add(ch[5] ? "" : ch[0]);
					}
				}
				fmt.clear;
			} else {
				@fmtchars = getFormattedText(self.contents, 0, 0,
																		self.width - self.borderX - SpriteWindow_Base.TEXT_PADDING, -1,
																		shadowc3tag(@baseColor, @shadowColor) + value, @lineHeight, true);
				@oldfont = self.contents.font.clone;
				@fmtchars.each do |ch|
					chx = ch[1] + ch[3];
					chy = ch[2] + ch[4];
					if (width < chx) width = chx;
					if (height < chy) height = chy;
					@textchars.Add(ch[5] ? "" : ch[0]);
				}
			}
			@bitmapwidth = width;
			@bitmapheight = height;
			@numtextchars = @textchars.length;
		}
		stopPause;
		@displaying = @letterbyletter;
		@needclear = true;
		@nodraw = @letterbyletter;
		refresh;
	}

	public bool busy() {
		return @displaying;
	}

	public bool pausing() {
		return @pausing && @displaying;
	}

	public void resume() {
		if (!busy()) {
			self.stopPause;
			return true;
		}
		if (@pausing) {
			@pausing = false;
			self.stopPause;
			return false;
		}
		return true;
	}

	public void position() {
		if (@lastDrawnChar < 0) return 0;
		if (@lastDrawnChar >= @fmtchars.length) return @numtextchars;
		// index after the last character's index
		return @fmtchars[@lastDrawnChar][14] + 1;
	}

	public void maxPosition() {
		pos = 0;
		@fmtchars.each do |ch|
			// index after the last character's index
			if (pos < ch[14] + 1) pos = ch[14] + 1;
		}
		return pos;
	}

	public void skipAhead() {
		if (!busy()) return;
		if (@textchars[@curchar] == "\n") return;
		resume;
		if (curcharSkip(true)) {
			visiblelines = (self.height - self.borderY) / @lineHeight;
			if (@textchars[@curchar] == "\n" && @linesdrawn >= visiblelines - 1) {
				@scroll_timer_start = System.uptime;
			} else if (@textchars[@curchar] == "\1") {
				if (@curchar < @numtextchars - 1) @pausing = true;
				self.startPause;
				refresh;
			}
		}
	}

	public void allocPause() {
		if (@pausesprite) return;
		@pausesprite = AnimatedSprite.create("Graphics/UI/pause_arrow", 4, 3);
		@pausesprite.z       = 100000;
		@pausesprite.visible = false;
	}

	public void startPause() {
		allocPause;
		@pausesprite.visible = true;
		@pausesprite.frame   = 0;
		@pausesprite.start;
		moveCursor;
	}

	public void stopPause() {
		if (!@pausesprite) return;
		@pausesprite.stop;
		@pausesprite.visible = false;
	}

	public void moveCursor() {
		if (!@pausesprite) return;
		cursor = @cursorMode;
		if (cursor == 0 && !@endOfText) cursor = 2;
		switch (cursor) {
			case 0:   // End of text
				@pausesprite.x = self.x + self.startX + @endOfText.x + @endOfText.width - 2;
				@pausesprite.y = self.y + self.startY + @endOfText.y - @scrollY;
				break;
			case 1:   // Lower right
				pauseWidth  = @pausesprite.bitmap ? @pausesprite.framewidth : 16;
				pauseHeight = @pausesprite.bitmap ? @pausesprite.frameheight : 16;
				@pausesprite.x = self.x + self.width - 40 + (pauseWidth / 2);
				@pausesprite.y = self.y + self.height - 60 + (pauseHeight / 2);
				break;
			case 2:   // Lower middle
				pauseWidth  = @pausesprite.bitmap ? @pausesprite.framewidth : 16;
				pauseHeight = @pausesprite.bitmap ? @pausesprite.frameheight : 16;
				@pausesprite.x = self.x + (self.width / 2) - (pauseWidth / 2);
				@pausesprite.y = self.y + self.height - 36 + (pauseHeight / 2);
				break;
		}
	}

	public void refresh() {
		oldcontents = self.contents;
		self.contents = DoEnsureBitmap(oldcontents, @bitmapwidth, @bitmapheight);
		self.oy       = @scrollY;
		numchars = @numtextchars;
		if (self.letterbyletter) numchars = (int)Math.Min(@curchar, @numtextchars);
		if (busy() && @drawncurchar == @curchar && !@scroll_timer_start) return;
		if (!self.letterbyletter || !oldcontents.equal(self.contents)) {
			@drawncurchar = -1;
			@needclear    = true;
		}
		if (@needclear) {
			if (@oldfont) self.contents.font = @oldfont;
			self.contents.clear;
			@needclear = false;
		}
		if (@nodraw) {
			@nodraw = false;
			return;
		}
		maxX = self.width - self.borderX;
		maxY = self.height - self.borderY;
		(@drawncurchar + 1..numchars).each do |i|
			if (i >= @fmtchars.length) continue;
			if (!self.letterbyletter) {
				if (@fmtchars[i][1] >= maxX) continue;
				if (@fmtchars[i][2] >= maxY) continue;
			}
			drawSingleFormattedChar(self.contents, @fmtchars[i]);
			@lastDrawnChar = i;
		}
		// all characters were drawn, reset old font
		if (!self.letterbyletter && @oldfont) self.contents.font = @oldfont;
		if (numchars > 0 && numchars != @numtextchars) {
			fch = @fmtchars[numchars - 1];
			if (fch) {
				rcdst = new Rect(fch[1], fch[2], fch[3], fch[4]);
				if (@textchars[numchars] == "\1") {
					@endOfText = rcdst;
					allocPause;
					moveCursor;
				} else {
					@endOfText = new Rect(rcdst.x + rcdst.width, rcdst.y, 8, 1);
				}
			}
		}
		@drawncurchar = @curchar;
	}

	public void redrawText() {
		if (@letterbyletter) {
			oldPosition = self.position;
			self.text = self.text;   // Clears the text already drawn
			if (oldPosition > @numtextchars) oldPosition = @numtextchars;
			while (self.position != oldPosition) {
				refresh;
				updateInternal;
			}
		} else {
			self.text = self.text;
		}
	}

	public void updateInternal() {
		time_now = System.uptime;
		if (!@display_last_updated) @display_last_updated = time_now;
		delta_t = time_now - @display_last_updated;
		@display_last_updated = time_now;
		visiblelines = (self.height - self.borderY) / @lineHeight;
		if (!@lastchar) @lastchar = -1;
		show_more_characters = false;
		// Pauses and new lines
		if (@textchars[@curchar] == "\1") {   // Waiting
			if (!@pausing) show_more_characters = true;
		} else if (@textchars[@curchar] == "\n") {   // Move to new line
			if (@linesdrawn >= visiblelines - 1) {   // Need to scroll text to show new line
				if (@scroll_timer_start) {
					old_y = @scrollstate;
					new_y = lerp(0, @lineHeight, 0.1, @scroll_timer_start, time_now);
					@scrollstate = new_y;
					@scrollY += new_y - old_y;
					if (@scrollstate >= @lineHeight) {
						@scrollstate = 0;
						@scroll_timer_start = null;
						@linesdrawn += 1;
						show_more_characters = true;
					}
				} else {
					show_more_characters = true;
				}
			} else {   // New line but the next line can be shown without scrolling to it
				if (@lastchar < @curchar) @linesdrawn += 1;
				show_more_characters = true;
			}
		} else if (@curchar <= @numtextchars) {   // Displaying more text
			show_more_characters = true;
		} else {
			@displaying = false;
			@scrollstate = 0;
			@scrollY = 0;
			@scroll_timer_start = null;
			@linesdrawn = 0;
		}
		@lastchar = @curchar;
		// Keep displaying more text
		if (show_more_characters) {
			@display_timer += delta_t;
			if (curcharSkip) {
				if (@textchars[@curchar] == "\n" && @linesdrawn >= visiblelines - 1) {
					@scroll_timer_start = time_now;
				} else if (@textchars[@curchar] == "\1") {
					if (@curchar < @numtextchars - 1) @pausing = true;
					self.startPause;
					refresh;
				}
			}
		}
	}

	public override void update() {
		base.update();
		if (@pausesprite&.visible) @pausesprite.update;
		if (@wait_timer_start) {
			if (System.uptime - @wait_timer_start >= @waitcount) {
				@wait_timer_start = null;
				@waitcount = 0;
				@display_last_updated = null;
			}
			if (@wait_timer_start) return;
		}
		if (busy()) {
			if (!@text_delay_changed) refresh;
			updateInternal;
			if (@text_delay_changed) refresh;   // Needed to allow "textspeed=0" to work seamlessly
		}
		@text_delay_changed = false;
	}

	//-----------------------------------------------------------------------------

	private;

	public void curcharSkip(instant = false) {
		ret = false;
		do { //loop; while (true);
			if (@display_timer < @text_delay && !instant) break;
			if (!instant) @display_timer -= @text_delay;
			ret = true;
			@curchar += 1;
			if (@textchars[@curchar] == "\n" || // newline
							@textchars[@curchar] == "\1" ||   // pause
							@textchars[@curchar] == "\2" ||   // letter-by-letter break
							@textchars[@curchar].null()) break;
		}
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Window_InputNumberPokemon : SpriteWindow_Base {
	public int sign		{ get { return _sign; } }			protected int _sign;

	public override void initialize(digits_max) {
		@digits_max = digits_max;
		@number = 0;
		@cursor_timer_start = System.uptime;
		@cursor_shown = true;
		@sign = false;
		@negative = false;
		base.initialize(0, 0, 32, 32);
		self.width = (digits_max * 24) + 8 + self.borderX;
		self.height = 32 + self.borderY;
		@baseColor, @shadowColor = getDefaultTextColors(self.windowskin);
		@index = digits_max - 1;
		self.active = true;
		refresh;
	}

	public override int active { set {
		base.active();
		refresh;
		}
	}

	public void number() {
		@number * (@sign && @negative ? -1 : 1);
	}

	public int number { set {
		if (!value.is_a(Numeric)) value = 0;
		if (@sign) {
			@negative = (value < 0);
			@number = (int)Math.Min(value.abs, (10**@digits_max) - 1);
		} else {
			@number = (int)Math.Min((int)Math.Max(value, 0), (10**@digits_max) - 1);
			}
	}
		refresh;
	}

	public int sign { set {
		@sign = value;
		self.width = (@digits_max * 24) + 8 + self.borderX + (@sign ? 24 : 0);
		@index = (@digits_max - 1) + (@sign ? 1 : 0);
		refresh;
		}
	}

	public void refresh() {
		self.contents = DoEnsureBitmap(self.contents,
																		self.width - self.borderX, self.height - self.borderY);
		SetSystemFont(self.contents);
		self.contents.clear;
		s = string.Format("{0:"+@digits_max+"}", @number.abs);
		if (@sign) {
			textHelper(0, 0, @negative ? "-" : "+", 0);
		}
		for (int i = @digits_max; i < @digits_max; i++) { //for '@digits_max' times do => |i|
			index = i + (@sign ? 1 : 0);
			textHelper(index * 24, 0, s[i, 1], index);
		}
	}

	public override void update() {
		base.update();
		digits = @digits_max + (@sign ? 1 : 0);
		cursor_to_show = ((System.uptime - @cursor_timer_start) / 0.35).ToInt().even();
		if (cursor_to_show != @cursor_shown) {
			@cursor_shown = cursor_to_show;
			refresh;
		}
		if (self.active) {
			if (Input.repeat(Input.UP) || Input.repeat(Input.DOWN)) {
				PlayCursorSE;
				if (@index == 0 && @sign) {
					@negative = !@negative;
				} else {
					place = 10**(digits - 1 - @index);
					n = @number / place % 10;
					@number -= n * place;
					if (Input.repeat(Input.UP)) {
						n = (n + 1) % 10;
					} else if (Input.repeat(Input.DOWN)) {
						n = (n + 9) % 10;
					}
					@number += n * place;
				}
				refresh;
			} else if (Input.repeat(Input.RIGHT)) {
				if (digits >= 2) {
					PlayCursorSE;
					@index = (@index + 1) % digits;
					@cursor_timer_start = System.uptime;
					@cursor_shown = true;
					refresh;
				}
			} else if (Input.repeat(Input.LEFT)) {
				if (digits >= 2) {
					PlayCursorSE;
					@index = (@index + digits - 1) % digits;
					@cursor_timer_start = System.uptime;
					@cursor_shown = true;
					refresh;
				}
			}
		}
	}

	//-----------------------------------------------------------------------------

	private;

	public void textHelper(x, y, text, i) {
		textwidth = self.contents.text_size(text).width;
		DrawShadowText(self.contents,
										x + (12 - (textwidth / 2)),
										y - 2 + (self.contents.text_offset_y || 0),   // TEXT OFFSET (the - 2)
										textwidth + 4, 32, text, @baseColor, @shadowColor);
		// Draw cursor
		if (@index == i && @active && @cursor_shown) {
			self.contents.fill_rect(x + (12 - (textwidth / 2)), y + 28, textwidth, 4, @shadowColor);
			self.contents.fill_rect(x + (12 - (textwidth / 2)), y + 28, textwidth - 2, 2, @baseColor);
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class SpriteWindow_Selectable : SpriteWindow_Base {
	public int index		{ get { return _index; } }			protected int _index;
	public int ignore_input		{ get { return _ignore_input; } }			protected int _ignore_input;

	public override void initialize(x, y, width, height) {
		base.initialize(x, y, width, height);
		@item_max = 1;
		@column_max = 1;
		@virtualOy = 2;   // TEXT OFFSET
		@index = -1;
		@row_height = 32;
		@column_spacing = 32;
		@ignore_input = false;
	}

	public void itemCount() {
		return @item_max || 0;
	}

	public int index { set {
		if (@index != index) {
			@index = index;
			priv_update_cursor_rect(true);
		}
		}
	}

	public void rowHeight() {
		return @row_height || 32;
	}

	public int rowHeight { set {
		if (@row_height != value) {
			oldTopRow = self.top_row;
			@row_height = (int)Math.Max(1, value);
			self.top_row = oldTopRow;
			update_cursor_rect;
		}
		}
	}

	public void columns() {
		return @column_max || 1;
	}

	public int columns { set {
		if (@column_max != value) {
			@column_max = (int)Math.Max(1, value);
			update_cursor_rect;
		}
		}
	}

	public void columnSpacing() {
		return @column_spacing || 32;
	}

	public int columnSpacing { set {
		if (@column_spacing != value) {
			@column_spacing = (int)Math.Max(0, value);
			update_cursor_rect;
		}
		}
	}

	public void count() {
		return @item_max;
	}

	public void row_max() {
		return ((@item_max + @column_max - 1) / @column_max).ToInt();
	}

	public void top_row() {
		return (@virtualOy / (@row_height || 32)).ToInt();
	}

	public int top_row { set {
		if (row > row_max - 1) row = row_max - 1;
		if (row < 0) row = 0;
		@virtualOy = (row * @row_height) + 2;   // TEXT OFFSET (the + 2)
		}
	}

	public void top_item() {
		return top_row * @column_max;
	}

	public void page_row_max() {
		return priv_page_row_max.ToInt();
	}

	public void page_item_max() {
		return priv_page_item_max.ToInt();
	}

	public void itemRect(item) {
		if ((item < 0 || item >= @item_max || item < self.top_item ||
			item > self.top_item + self.page_item_max) {
			return new Rect(0, 0, 0, 0);
		} else {
			cursor_width = (self.width - self.borderX - ((@column_max - 1) * @column_spacing)) / @column_max;
			x = item % @column_max * (cursor_width + @column_spacing);
			y = (item / @column_max * @row_height) - @virtualOy;
			return new Rect(x, y, cursor_width, @row_height);
		}
	}

	public void refresh() { }

	public void update_cursor_rect() {
		priv_update_cursor_rect;
	}

	public override void update() {
		base.update();
		if (self.active && @item_max > 0 && @index >= 0 && !@ignore_input) {
			if (Input.repeat(Input.UP)) {
				if ((@index >= @column_max ||
					(Input.trigger(Input.UP) && (@item_max % @column_max) == 0)) {
					oldindex = @index;
					@index = (@index - @column_max + @item_max) % @item_max;
					if (@index != oldindex) {
						PlayCursorSE;
						update_cursor_rect;
					}
				}
			} else if (Input.repeat(Input.DOWN)) {
				if ((@index < @item_max - @column_max ||
					(Input.trigger(Input.DOWN) && (@item_max % @column_max) == 0)) {
					oldindex = @index;
					@index = (@index + @column_max) % @item_max;
					if (@index != oldindex) {
						PlayCursorSE;
						update_cursor_rect;
					}
				}
			} else if (Input.repeat(Input.LEFT)) {
				if (@column_max >= 2 && @index > 0) {
					oldindex = @index;
					@index -= 1;
					if (@index != oldindex) {
						PlayCursorSE;
						update_cursor_rect;
					}
				}
			} else if (Input.repeat(Input.RIGHT)) {
				if (@column_max >= 2 && @index < @item_max - 1) {
					oldindex = @index;
					@index += 1;
					if (@index != oldindex) {
						PlayCursorSE;
						update_cursor_rect;
					}
				}
			} else if (Input.repeat(Input.JUMPUP)) {
				if (@index > 0) {
					oldindex = @index;
					@index = (int)Math.Max(self.index - self.page_item_max, 0);
					if (@index != oldindex) {
						PlayCursorSE;
						self.top_row -= self.page_row_max;
						update_cursor_rect;
					}
				}
			} else if (Input.repeat(Input.JUMPDOWN)) {
				if (@index < @item_max - 1) {
					oldindex = @index;
					@index = (int)Math.Min(self.index + self.page_item_max, @item_max - 1);
					if (@index != oldindex) {
						PlayCursorSE;
						self.top_row += self.page_row_max;
						update_cursor_rect;
					}
				}
			}
		}
	}

	//-----------------------------------------------------------------------------

	private;

	public void priv_page_row_max() {
		return (self.height - self.borderY) / @row_height;
	}

	public void priv_page_item_max() {
		return (self.height - self.borderY) / @row_height * @column_max;
	}

	public void priv_update_cursor_rect(force = false) {
		if (@index < 0) {
			self.cursor_rect.empty;
			self.refresh;
			return;
		}
		dorefresh = false;
		row = @index / @column_max;
		// This code makes lists scroll only when the cursor hits the top and bottom
		// of the visible list.
//    if (row < self.top_row) {
//      self.top_row = row
//      dorefresh=true
//    }
//    if (row > self.top_row + (self.page_row_max - 1)) {
//      self.top_row = row - (self.page_row_max - 1)
//      dorefresh=true
//    }
//    if (oldindex-self.top_item>=((self.page_item_max - 1)/2)) {
//      self.top_row+=1
//    }
//    self.top_row = (int)Math.Min(self.top_row, self.row_max - self.page_row_max)
		// This code makes the cursor stay in the middle of the visible list as much
		// as possible.
		new_top_row = row - (int)Math.Floor((self.page_row_max - 1) / 2);
		new_top_row = (int)Math.Max((int)Math.Min(new_top_row, self.row_max - self.page_row_max), 0);
		if (self.top_row != new_top_row) {
			self.top_row = new_top_row;
//      dorefresh = true
		}
		// End of code
		cursor_width = (self.width - self.borderX) / @column_max;
		x = self.index % @column_max * (cursor_width + @column_spacing);
		y = (self.index / @column_max * @row_height) - @virtualOy;
		self.cursor_rect.set(x, y, cursor_width, @row_height);
		if (dorefresh || force) self.refresh;
	}
}

//===============================================================================
//
//===============================================================================
public static partial class UpDownArrowMixin {
	public void initUpDownArrow() {
		@uparrow   = AnimatedSprite.create("Graphics/UI/up_arrow", 8, 2, self.viewport);
		@downarrow = AnimatedSprite.create("Graphics/UI/down_arrow", 8, 2, self.viewport);
		RPG.Cache.retain("Graphics/UI/up_arrow");
		RPG.Cache.retain("Graphics/UI/down_arrow");
		@uparrow.z   = 99998;
		@downarrow.z = 99998;
		@uparrow.visible   = false;
		@downarrow.visible = false;
		@uparrow.play;
		@downarrow.play;
	}

	public override void dispose() {
		@uparrow.dispose;
		@downarrow.dispose;
		base.dispose();
	}

	public override int viewport { set {
		base.viewport();
		@uparrow.viewport   = self.viewport;
		@downarrow.viewport = self.viewport;
		}
	}

	public override int color { set {
		base.color();
		@uparrow.color   = value;
		@downarrow.color = value;
		}
	}

	public void adjustForZoom(sprite) {
		sprite.zoom_x = self.zoom_x;
		sprite.zoom_y = self.zoom_y;
		sprite.x = (sprite.x * self.zoom_x) + (self.offset_x / self.zoom_x);
		sprite.y = (sprite.y * self.zoom_y) + (self.offset_y / self.zoom_y);
	}

	public override void update() {
		base.update();
		@uparrow.x   = self.x + (self.width / 2) - (@uparrow.framewidth / 2);
		@downarrow.x = self.x + (self.width / 2) - (@downarrow.framewidth / 2);
		@uparrow.y   = self.y;
		@downarrow.y = self.y + self.height - @downarrow.frameheight;
		@uparrow.visible = self.visible && self.active && (self.top_item != 0 &&
											@item_max > self.page_item_max);
		@downarrow.visible = self.visible && self.active &&
												(self.top_item + self.page_item_max < @item_max && @item_max > self.page_item_max)
		@uparrow.z   = self.z + 1;
		@downarrow.z = self.z + 1;
		adjustForZoom(@uparrow);
		adjustForZoom(@downarrow);
		@uparrow.viewport   = self.viewport;
		@downarrow.viewport = self.viewport;
		@uparrow.update;
		@downarrow.update;
	}
}

//===============================================================================
//
//===============================================================================
public partial class SpriteWindow_SelectableEx : SpriteWindow_Selectable {
	include UpDownArrowMixin;

	public override void initialize(*arg) {
		base.initialize(*arg);
		initUpDownArrow;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Window_DrawableCommand : SpriteWindow_SelectableEx {
	public int baseColor		{ get { return _baseColor; } }			protected int _baseColor;
	public int shadowColor		{ get { return _shadowColor; } }			protected int _shadowColor;

	public override void initialize(x, y, width, height, viewport = null) {
		base.initialize(x, y, width, height);
		if (viewport) self.viewport = viewport;
		if (isDarkWindowskin(self.windowskin)) {
			@selarrow = new AnimatedBitmap("Graphics/UI/sel_arrow_white");
			RPG.Cache.retain("Graphics/UI/sel_arrow_white");
		} else {
			@selarrow = new AnimatedBitmap("Graphics/UI/sel_arrow");
			RPG.Cache.retain("Graphics/UI/sel_arrow");
		}
		@index = 0;
		@baseColor, @shadowColor = getDefaultTextColors(self.windowskin);
		refresh;
	}

	public override void dispose() {
		@selarrow.dispose;
		base.dispose();
	}

	public int baseColor { set {
		@baseColor = value;
		refresh;
		}
	}

	public int shadowColor { set {
		@shadowColor = value;
		refresh;
		}
	}

	public void textWidth(bitmap, text) {
		return bitmap.text_size(text).width;
	}

	public void getAutoDims(commands, dims, width = null) {
		rowMax = ((commands.length + self.columns - 1) / self.columns).ToInt();
		windowheight = (rowMax * self.rowHeight);
		windowheight += self.borderY;
		if (!width || width < 0) {
			width = 0;
			tmpbitmap = new Bitmap(1, 1);
			SetSystemFont(tmpbitmap);
			foreach (var i in commands) { //'commands.each' do => |i|
				width = (int)Math.Max(width, tmpbitmap.text_size(i).width);
			}
			// one 16 to allow cursor
			width += 16 + 16 + SpriteWindow_Base.TEXT_PADDING;
			tmpbitmap.dispose;
		}
		// Store suggested width and height of window
		dims[0] = (int)Math.Max(self.borderX + 1,
							(width * self.columns) + self.borderX + ((self.columns - 1) * self.columnSpacing))
		dims[1] = (int)Math.Max(self.borderY + 1, windowheight);
		dims[1] = (int)Math.Min(dims[1], Graphics.height);
	}

	public override void setSkin(skin) {
		base.setSkin(skin);
		privRefresh(true);
		@baseColor, @shadowColor = getDefaultTextColors(self.windowskin);
	}

	public void drawCursor(index, rect) {
		if (self.index == index) {
			CopyBitmap(self.contents, @selarrow.bitmap, rect.x, rect.y + 2);   // TEXT OFFSET (counters the offset above)
		}
		return new Rect(rect.x + 16, rect.y, rect.width - 16, rect.height);
	}

	// To be implemented by derived classes.
	public void itemCount() {
		return 0;
	}

	// To be implemented by derived classes.
	public void drawItem(index, count, rect) {}

	public void refresh() {
		@item_max = itemCount;
		dwidth  = self.width - self.borderX;
		dheight = self.height - self.borderY;
		self.contents = DoEnsureBitmap(self.contents, dwidth, dheight);
		self.contents.clear;
		for (int i = @item_max; i < @item_max; i++) { //for '@item_max' times do => |i|
			if (i < self.top_item || i > self.top_item + self.page_item_max) continue;
			drawItem(i, @item_max, itemRect(i));
		}
	}

	public override void update() {
		oldindex = self.index;
		base.update();
		if (self.index != oldindex) refresh;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Window_CommandPokemon : Window_DrawableCommand {
	public int commands		{ get { return _commands; } }			protected int _commands;

	public override void initialize(commands, width = null) {
		@starting = true;
		@commands = new List<string>();
		dims = new List<string>();
		base.initialize(0, 0, 32, 32);
		getAutoDims(commands, dims, width);
		self.width = dims[0];
		self.height = dims[1];
		@commands = commands;
		self.active = true;
		@baseColor, @shadowColor = getDefaultTextColors(self.windowskin);
		refresh;
		@starting = false;
	}

	public static void newWithSize(commands, x, y, width, height, viewport = null) {
		ret = new self(commands, width);
		ret.x = x;
		ret.y = y;
		ret.width = width;
		ret.height = height;
		ret.viewport = viewport;
		return ret;
	}

	public static void newEmpty(x, y, width, height, viewport = null) {
		ret = new self(new List<string>(), width);
		ret.x = x;
		ret.y = y;
		ret.width = width;
		ret.height = height;
		ret.viewport = viewport;
		return ret;
	}

	public override int index { set {
		base.index();
		if (!@starting) refresh;
		}
	}

	public int commands { set {
		@commands = value;
		@item_max = commands.length;
		self.update_cursor_rect;
		self.refresh;
		}
	}

	public override int width { set {
		base.width();
		if (!@starting) {
			self.index = self.index;
			self.update_cursor_rect;
		}
		}
	}

	public override int height { set {
		base.height();
		if (!@starting) {
			self.index = self.index;
			self.update_cursor_rect;
		}
		}
	}

	public void resizeToFit(commands, width = null) {
		dims = new List<string>();
		getAutoDims(commands, dims, width);
		self.width = dims[0];
		self.height = dims[1];
	}

	public void itemCount() {
		return @commands ? @commands.length : 0;
	}

	public void drawItem(index, _count, rect) {
		if (@starting) SetSystemFont(self.contents);
		rect = drawCursor(index, rect);
		DrawShadowText(self.contents, rect.x, rect.y + (self.contents.text_offset_y || 0),
										rect.width, rect.height, @commands[index], self.baseColor, self.shadowColor);
	}
}

//===============================================================================
//
//===============================================================================
public partial class Window_CommandPokemonEx : Window_CommandPokemon {
}

//===============================================================================
//
//===============================================================================
public partial class Window_AdvancedCommandPokemon : Window_DrawableCommand {
	public int commands		{ get { return _commands; } }			protected int _commands;

	public void textWidth(bitmap, text) {
		dims = new {null, 0};
		chars = getFormattedText(bitmap, 0, 0,
														Graphics.width - self.borderX - SpriteWindow_Base.TEXT_PADDING - 16,
														-1, text, self.rowHeight, true, true);
		foreach (var ch in chars) { //'chars.each' do => |ch|
			dims[0] = dims[0] ? (int)Math.Min(dims[0], ch[1]) : ch[1];
			dims[1] = (int)Math.Max(dims[1], ch[1] + ch[3]);
		}
		if (!dims[0]) dims[0] = 0;
		return dims[1] - dims[0];
	}

	public override void initialize(commands, width = null) {
		@starting = true;
		@commands = new List<string>();
		dims = new List<string>();
		base.initialize(0, 0, 32, 32);
		getAutoDims(commands, dims, width);
		self.width = dims[0];
		self.height = dims[1];
		@commands = commands;
		self.active = true;
		@baseColor, @shadowColor = getDefaultTextColors(self.windowskin);
		refresh;
		@starting = false;
	}

	public static void newWithSize(commands, x, y, width, height, viewport = null) {
		ret = new self(commands, width);
		ret.x = x;
		ret.y = y;
		ret.width = width;
		ret.height = height;
		ret.viewport = viewport;
		return ret;
	}

	public static void newEmpty(x, y, width, height, viewport = null) {
		ret = new self(new List<string>(), width);
		ret.x = x;
		ret.y = y;
		ret.width = width;
		ret.height = height;
		ret.viewport = viewport;
		return ret;
	}

	public override int index { set {
		base.index();
		if (!@starting) refresh;
		}
	}

	public int commands { set {
		@commands = value;
		@item_max = commands.length;
		self.update_cursor_rect;
		self.refresh;
		}
	}

	public override int width { set {
		oldvalue = self.width;
		base.width();
		if (!@starting && oldvalue != value) {
			self.index = self.index;
			self.update_cursor_rect;
		}
		}
	}

	public override int height { set {
		oldvalue = self.height;
		base.height();
		if (!@starting && oldvalue != value) {
			self.index = self.index;
			self.update_cursor_rect;
		}
		}
	}

	public void resizeToFit(commands, width = null) {
		dims = new List<string>();
		getAutoDims(commands, dims, width);
		self.width = dims[0];
		self.height = dims[1] - 6;
	}

	public void itemCount() {
		return @commands ? @commands.length : 0;
	}

	public void drawItem(index, _count, rect) {
		SetSystemFont(self.contents);
		rect = drawCursor(index, rect);
		if (toUnformattedText(@commands[index]).gsub(/\n/, "") == @commands[index]) {
			// Use faster alternative for unformatted text without line breaks
			DrawShadowText(self.contents, rect.x, rect.y + (self.contents.text_offset_y || 0),
											rect.width, rect.height, @commands[index], self.baseColor, self.shadowColor);
		} else {
			chars = getFormattedText(self.contents, rect.x, rect.y + (self.contents.text_offset_y || 0),
															rect.width, rect.height, @commands[index], rect.height, true, true);
			drawFormattedChars(self.contents, chars);
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class Window_AdvancedCommandPokemonEx : Window_AdvancedCommandPokemon {
}
