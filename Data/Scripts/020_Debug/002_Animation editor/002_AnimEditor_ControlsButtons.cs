//===============================================================================
//
//===============================================================================
public static partial class BattleAnimationEditor {
	//=============================================================================
	//
	//=============================================================================
	public static partial class ShadowText {
		public void shadowtext(bitmap, x, y, w, h, t, disabled = false, align = 0) {
			width = bitmap.text_size(t).width;
			switch (align) {
				case 2:
					x += (w - width);
					break;
				case 1:
					x += (w / 2) - (width / 2);
					break;
			}
			DrawShadowText(bitmap, x, y + 6, w, h, t,
											disabled ? new Color(208, 208, 200) : new Color(96, 96, 96),
											new Color(208, 208, 200));
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class UIControl {
		include ShadowText;
		public int bitmap		{ get { return _bitmap; } set { _bitmap = value; } }			protected int _bitmap;
		public int label		{ get { return _label; } set { _label = value; } }			protected int _label;
		public int x		{ get { return _x; } set { _x = value; } }			protected int _x;
		public int y		{ get { return _y; } set { _y = value; } }			protected int _y;
		public int width		{ get { return _width; } set { _width = value; } }			protected int _width;
		public int height		{ get { return _height; } set { _height = value; } }			protected int _height;
		public int changed		{ get { return _changed; } set { _changed = value; } }			protected int _changed;
		public int parent		{ get { return _parent; } set { _parent = value; } }			protected int _parent;
		public int disabled		{ get { return _disabled; } set { _disabled = value; } }			protected int _disabled;

		public void text() {
			return self.label;
		}

		public int text { set {
			self.label = value;
			}
		}

		public void initialize(label) {
			@label = label;
			@x = 0;
			@y = 0;
			@width = 0;
			@height = 0;
			@changed = false;
			@disabled = false;
			@invalid = true;
		}

		public void toAbsoluteRect(rc) {
			return new Rect(rc.x + self.parentX, rc.y + self.parentY, rc.width, rc.height);
		}

		public void parentX() {
			if (!self.parent) return 0;
			if (self.parent.is_a(SpriteWindow)) return self.parent.x + self.parent.leftEdge;
			if (self.parent.is_a(Window)) return self.parent.x + 16;
			return self.parent.x;
		}

		public void parentY() {
			if (!self.parent) return 0;
			if (self.parent.is_a(SpriteWindow)) return self.parent.y + self.parent.topEdge;
			if (self.parent.is_a(Window)) return self.parent.y + 16;
			return self.parent.y;
		}

		public bool invalid() {
			return @invalid;
		}

		// Marks that the control must be redrawn to reflect current logic.
		public void invalidate() {
			@invalid = true;
		}

		// Updates the logic on the control, invalidating it if necessary.
		public void update() { }

		// Redraws the control.
		public void refresh() { }

		// Makes the control no longer invalid.
		public void validate() {
			@invalid = false;
		}

		// Redraws the control only if it is invalid.
		public void repaint() {
			if (!self.invalid()) return;
			self.refresh;
			self.validate;
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class Label : UIControl {
		public int text { set {
			self.label = value;
			refresh;
			}		}

		public void refresh() {
			bitmap = self.bitmap;
			bitmap.fill_rect(self.x, self.y, self.width, self.height, new Color(0, 0, 0, 0));
			size = bitmap.text_size(self.label).width;
			shadowtext(bitmap, self.x + 4, self.y, size, self.height, self.label, @disabled);
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class Button : UIControl {
		public int label		{ get { return _label; } set { _label = value; } }			protected int _label;

		public override void initialize(label) {
			base.initialize();
			@captured = false;
			@label = label;
		}

		public void update() {
			mousepos = Mouse.getMousePos;
			self.changed = false;
			if (!mousepos) return;
			rect = new Rect(self.x + 1, self.y + 1, self.width - 2, self.height - 2);
			rect = toAbsoluteRect(rect);
			if (Input.trigger(Input.MOUSELEFT) &&
				rect.contains(mousepos[0], mousepos[1]) && !@captured) {
				@captured = true;
				self.invalidate;
			}
			if (Input.release(Input.MOUSELEFT) && @captured) {
				if (rect.contains(mousepos[0], mousepos[1])) self.changed = true;
				@captured = false;
				self.invalidate;
			}
		}

		public void refresh() {
			bitmap = self.bitmap;
			x = self.x;
			y = self.y;
			width = self.width;
			height = self.height;
			// Draw background
			if (@captured) {
				bitmap.fill_rect(x + 2, y + 2, width - 4, height - 4, new Color(120, 120, 120, 80));
			} else {
				bitmap.fill_rect(x + 2, y + 2, width - 4, height - 4, new Color(0, 0, 0, 0));
			}
			// Draw text
			size = bitmap.text_size(self.label).width;
			shadowtext(bitmap, x + 4, y, size, height, self.label, @disabled);
			// Draw outline
			color = new Color(120, 120, 120);
			bitmap.fill_rect(x + 1, y + 1, width - 2, 1, color);
			bitmap.fill_rect(x + 1, y + 1, 1, height - 2, color);
			bitmap.fill_rect(x + 1, y + height - 2, width - 2, 1, color);
			bitmap.fill_rect(x + width - 2, y + 1, 1, height - 2, color);
			// Return the control's clickable area
			ret = new Rect(x + 1, y + 1, width - 2, height - 2);
			return ret;
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class Checkbox : Button {
		public int checked		{ get { return _checked; } }			protected int _checked;

		public void curvalue() {
			return self.checked;
		}

		public int curvalue { set {
			self.checked = value;
			}
		}

		public int checked { set {
			@checked = value;
			invalidate;
			}
		}

		public override void initialize(label) {
			base.initialize();
			@checked = false;
		}

		public override void update() {
			base.update();
			if (self.changed) {
				@checked = !@checked;
				self.invalidate;
			}
		}

		public void refresh() {
			bitmap = self.bitmap;
			x = self.x;
			y = self.y;
			width = (int)Math.Min(self.width, 32);
			height = (int)Math.Min(self.height, 32);
			// Draw background
			bitmap.fill_rect(x + 2, y + 2, self.width - 4, self.height - 4, new Color(0, 0, 0, 0));
			if (@captured) {
				bitmap.fill_rect(x + 2, y + 2, width - 4, height - 4, new Color(120, 120, 120, 80));
			} else {
				bitmap.fill_rect(x + 2, y + 2, width - 4, height - 4, new Color(0, 0, 0, 0));
			}
			// Draw text
			if (self.checked) shadowtext(bitmap, x, y, 32, 32, "X", @disabled, 1);
			size = bitmap.text_size(self.label).width;
			shadowtext(bitmap, x + 36, y, size, height, self.label, @disabled);
			// Draw outline
			color = new Color(120, 120, 120);
			bitmap.fill_rect(x + 1, y + 1, width - 2, 1, color);
			bitmap.fill_rect(x + 1, y + 1, 1, height - 2, color);
			bitmap.fill_rect(x + 1, y + height - 2, width - 2, 1, color);
			bitmap.fill_rect(x + width - 2, y + 1, 1, height - 2, color);
			// Return the control's clickable area
			ret = new Rect(x + 1, y + 1, width - 2, height - 2);
			return ret;
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class TextField : UIControl {
		public int label		{ get { return _label; } set { _label = value; } }			protected int _label;
		public int text		{ get { return _text; } }			protected int _text;

		public int text { set {
			@text = value;
			@cursor_shown = true;
			self.invalidate;
			}
		}

		public override void initialize(label, text) {
			base.initialize(label);
			@cursor_timer_start = System.uptime;
			@cursor_shown = true;
			@label = label;
			@text = text;
			@cursor = text.scan(/./m).length;
		}

		public void insert(ch) {
			chars = self.text.scan(/./m);
			chars.insert(@cursor, ch);
			@text = "";
			chars.each(char => @text += char);
			@cursor += 1;
			@cursor_timer_start = System.uptime;
			@cursor_shown = true;
			self.changed = true;
			self.invalidate;
		}

		public void delete() {
			chars = self.text.scan(/./m);
			chars.delete_at(@cursor - 1);
			@text = "";
			chars.each(char => @text += char);
			@cursor -= 1;
			@cursor_timer_start = System.uptime;
			@cursor_shown = true;
			self.changed = true;
			self.invalidate;
		}

		public void update() {
			cursor_to_show = ((System.uptime - @cursor_timer_start) / 0.35).ToInt().even();
			self.changed = false;
			if (cursor_to_show != @cursor_shown) {
				@cursor_shown = cursor_to_show;
				self.invalidate;
			}
			// Moving cursor
			if (Input.triggerex(:LEFT) || Input.repeatex(:LEFT)) {
				if (@cursor > 0) {
					@cursor -= 1;
					@cursor_timer_start = System.uptime;
					@cursor_shown = true;
					self.invalidate;
				}
				return;
			}
			if (Input.triggerex(:RIGHT) || Input.repeatex(:RIGHT)) {
				if (@cursor < self.text.scan(/./m).length) {
					@cursor += 1;
					@cursor_timer_start = System.uptime;
					@cursor_shown = true;
					self.invalidate;
				}
				return;
			}
			// Backspace
			if (Input.triggerex(:BACKSPACE) || Input.repeatex(:BACKSPACE) ||
				Input.triggerex(:DELETE) || Input.repeatex(:DELETE)) {
				if (@cursor > 0) self.delete;
				return;
			}
			// Letter & Number keys
			Input.gets.each_char(c => insert(c));
		}

		public void refresh() {
			bitmap = self.bitmap;
			x = self.x;
			y = self.y;
			width = self.width;
			height = self.height;
			color = new Color(120, 120, 120);
			bitmap.font.color = color;
			bitmap.fill_rect(x, y, width, height, new Color(0, 0, 0, 0));
			size = bitmap.text_size(self.label).width;
			shadowtext(bitmap, x, y, size, height, self.label);
			x += size;
			width -= size;
			outline_x = x;
			outline_y = y;
			// Draw background
			if (@captured) {
				bitmap.fill_rect(x + 2, y + 2, width - 4, height - 4, new Color(120, 120, 120, 80));
			} else {
				bitmap.fill_rect(x + 2, y + 2, width - 4, height - 4, new Color(0, 0, 0, 0));
			}
			// Draw text
			x += 4;
			textscan = self.text.scan(/./m);
			scanlength = textscan.length;
			if (@cursor > scanlength) @cursor = scanlength;
			if (@cursor < 0) @cursor = 0;
			startpos = @cursor;
			fromcursor = 0;
			while (startpos > 0) {
				c = textscan[startpos - 1];
				fromcursor += bitmap.text_size(c).width;
				if (fromcursor > width - 4) break;
				startpos -= 1;
			}
			for (int i = startpos; i < scanlength; i++) { //each 'scanlength' do => |i|
				c = textscan[i];
				textwidth = bitmap.text_size(c).width;
				if (c == "\n") continue;
				// Draw text
				shadowtext(bitmap, x, y, textwidth + 4, 32, c);
				// Draw cursor if necessary
				if (i == @cursor && @cursor_shown) {
					bitmap.fill_rect(x, y + 4, 2, 24, new Color(120, 120, 120));
				}
				// Add x to drawn text width
				x += textwidth;
			}
			if (textscan.length == @cursor && @cursor_shown) {
				bitmap.fill_rect(x, y + 4, 2, 24, new Color(120, 120, 120));
			}
			// Draw outline
			bitmap.fill_rect(outline_x + 1, outline_y + 1, width - 2, 1, color);
			bitmap.fill_rect(outline_x + 1, outline_y + 1, 1, height - 2, color);
			bitmap.fill_rect(outline_x + 1, outline_y + height - 2, width - 2, 1, color);
			bitmap.fill_rect(outline_x + width - 2, outline_y + 1, 1, height - 2, color);
			// Return the control's clickable area
			ret = new Rect(x + 1, y + 1, width - 2, height - 2);
			return ret;
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class Slider : UIControl {
		public int minvalue		{ get { return _minvalue; } }			protected int _minvalue;
		public int maxvalue		{ get { return _maxvalue; } }			protected int _maxvalue;
		public int curvalue		{ get { return _curvalue; } }			protected int _curvalue;
		public int label		{ get { return _label; } set { _label = value; } }			protected int _label;

		public int curvalue { set {
			@curvalue = value;
			if (self.minvalue && @curvalue < self.minvalue) @curvalue = self.minvalue;
			if (self.maxvalue && @curvalue > self.maxvalue) @curvalue = self.maxvalue;
			self.invalidate;
			}
		}

		public int minvalue { set {
			@minvalue = value;
			if (self.minvalue && @curvalue < self.minvalue) @curvalue = self.minvalue;
			if (self.maxvalue && @curvalue > self.maxvalue) @curvalue = self.maxvalue;
			self.invalidate;
			}
		}

		public int maxvalue { set {
			@maxvalue = value;
			if (self.minvalue && @curvalue < self.minvalue) @curvalue = self.minvalue;
			if (self.maxvalue && @curvalue > self.maxvalue) @curvalue = self.maxvalue;
			self.invalidate;
			}
		}

		public override void initialize(label, minvalue, maxvalue, curval) {
			base.initialize(label);
			@minvalue = minvalue;
			@maxvalue = maxvalue;
			@curvalue = curval;
			@label = label;
			@leftarrow = new Rect(0, 0, 0, 0);
			@rightarrow = new Rect(0, 0, 0, 0);
			self.minvalue = minvalue;
			self.maxvalue = maxvalue;
			self.curvalue = curval;
		}

		public void update() {
			mousepos = Mouse.getMousePos;
			self.changed = false;
			if (self.minvalue < self.maxvalue && self.curvalue < self.minvalue) {
				self.curvalue = self.minvalue;
			}
			if (self.disabled) return false;
			if (!Input.repeat(Input.MOUSELEFT)) return false;
			if (!mousepos) return false;
			left = toAbsoluteRect(@leftarrow);
			right = toAbsoluteRect(@rightarrow);
			oldvalue = self.curvalue;
			repeattime = Input.time(Input.MOUSELEFT);
			// Left arrow
			if (left.contains(mousepos[0], mousepos[1])) {
				if (repeattime > 3.0) {
					self.curvalue -= 10;
				} else if (repeattime > 1.5) {
					self.curvalue -= 5;
				} else {
					self.curvalue -= 1;
				}
				self.curvalue = self.curvalue.floor;
				self.changed = (self.curvalue != oldvalue);
				self.invalidate;
			}
			// Right arrow
			if (right.contains(mousepos[0], mousepos[1])) {
				if (repeattime > 3.0) {
					self.curvalue += 10;
				} else if (repeattime > 1.5) {
					self.curvalue += 5;
				} else {
					self.curvalue += 1;
				}
				self.curvalue = self.curvalue.floor;
				self.changed = (self.curvalue != oldvalue);
				self.invalidate;
			}
		}

		public void refresh() {
			bitmap = self.bitmap;
			x = self.x;
			y = self.y;
			width = self.width;
			height = self.height;
			color = new Color(120, 120, 120);
			bitmap.fill_rect(x, y, width, height, new Color(0, 0, 0, 0));
			size = bitmap.text_size(self.label).width;
			leftarrows = bitmap.text_size(" <<");
			numbers = bitmap.text_size(" XXXX ").width;
			rightarrows = bitmap.text_size(">> ");
			bitmap.font.color = color;
			shadowtext(bitmap, x, y, size, height, self.label);
			x += size;
			shadowtext(bitmap, x, y, leftarrows.width, height, " <<",
								self.disabled || self.curvalue == self.minvalue);
			@leftarrow = new Rect(x, y, leftarrows.width, height);
			x += leftarrows.width;
			if (!self.disabled) {
				bitmap.font.color = color;
				shadowtext(bitmap, x, y, numbers, height, $" {self.curvalue} ", false, 1);
			}
			x += numbers;
			shadowtext(bitmap, x, y, rightarrows.width, height, ">> ",
								self.disabled || self.curvalue == self.maxvalue);
			@rightarrow = new Rect(x, y, rightarrows.width, height);
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class OptionalSlider : Slider {
		public void initialize(label, minvalue, maxvalue, curvalue) {
			@slider = new Slider(label, minvalue, maxvalue, curvalue);
			@checkbox = new Checkbox("");
		}

		public void curvalue() {
			return @checkbox.checked ? @slider.curvalue : null;
		}

		public int curvalue { set {
			slider.curvalue = value;
			}
		}

		public void checked() {
			return @checkbox.checked;
		}

		public int checked { set {
			@checkbox.checked = value;
			}
		}

		public bool invalid() {
			return @slider.invalid() || @checkbox.invalid();
		}

		public void invalidate() {
			@slider.invalidate;
			@checkbox.invalidate;
		}

		public bool validate() {
			@slider.validate;
			@checkbox.validate;
		}

		public void changed() {
			return @slider.changed || @checkbox.changed;
		}

		public void minvalue() {
			return @slider.minvalue;
		}

		public int minvalue { set {
			slider.minvalue = value;
			}
		}

		public void maxvalue() {
			return @slider.maxvalue;
		}

		public int maxvalue { set {
			slider.maxvalue = value;
			}
		}

		public void update() {
			updatedefs;
			@slider.update;
			@checkbox.update;
		}

		public void refresh() {
			updatedefs;
			@slider.refresh;
			@checkbox.refresh;
		}

		//---------------------------------------------------------------------------

		private;

		public void updatedefs() {
			checkboxwidth = 32;
			@slider.bitmap = self.bitmap;
			@slider.parent = self.parent;
			@checkbox.x = self.x;
			@checkbox.y = self.y;
			@checkbox.width = checkboxwidth;
			@checkbox.height = self.height;
			@checkbox.bitmap = self.bitmap;
			@checkbox.parent = self.parent;
			@slider.x = self.x + checkboxwidth + 4;
			@slider.y = self.y;
			@slider.width = self.width - checkboxwidth;
			@slider.height = self.height;
			@slider.disabled = !@checkbox.checked;
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class ArrayCountSlider : Slider {
		public void maxvalue() {
			return @array.length - 1;
		}

		public override void initialize(array, label) {
			@array = array;
			base.initialize(label, 0, canvas.animation.length - 1, 0);
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class FrameCountSlider : Slider {
		public void maxvalue() {
			return @canvas.animation.length;
		}

		public override void initialize(canvas) {
			@canvas = canvas;
			base.initialize(_INTL("Frame:"), 1, canvas.animation.length, 0);
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class FrameCountButton : Button {
		public void label() {
			return _INTL("Total Frames: {1}", @canvas.animation.length);
		}

		public override void initialize(canvas) {
			@canvas = canvas;
			base.initialize(self.label);
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class TextSlider : UIControl {
		public int minvalue		{ get { return _minvalue; } }			protected int _minvalue;
		public int maxvalue		{ get { return _maxvalue; } }			protected int _maxvalue;
		public int curvalue		{ get { return _curvalue; } }			protected int _curvalue;
		public int label		{ get { return _label; } set { _label = value; } }			protected int _label;
		public int options		{ get { return _options; } set { _options = value; } }			protected int _options;
		public int maxoptionwidth		{ get { return _maxoptionwidth; } set { _maxoptionwidth = value; } }			protected int _maxoptionwidth;

		public int curvalue { set {
			@curvalue = value;
			if (self.minvalue && @curvalue < self.minvalue) @curvalue = self.minvalue;
			if (self.maxvalue && @curvalue > self.maxvalue) @curvalue = self.maxvalue;
			self.invalidate;
			}
		}

		public int minvalue { set {
			@minvalue = value;
			if (self.minvalue && @curvalue < self.minvalue) @curvalue = self.minvalue;
			if (self.maxvalue && @curvalue > self.maxvalue) @curvalue = self.maxvalue;
			self.invalidate;
			}
		}

		public int maxvalue { set {
			@maxvalue = value;
			if (self.minvalue && @curvalue < self.minvalue) @curvalue = self.minvalue;
			if (self.maxvalue && @curvalue > self.maxvalue) @curvalue = self.maxvalue;
			self.invalidate;
			}
		}

		public override void initialize(label, options, curval) {
			base.initialize(label);
			@label = label;
			@options = options;
			@minvalue = 0;
			@maxvalue = options.length - 1;
			@curvalue = curval;
			@leftarrow = new Rect(0, 0, 0, 0);
			@rightarrow = new Rect(0, 0, 0, 0);
			self.minvalue = @minvalue;
			self.maxvalue = @maxvalue;
			self.curvalue = @curvalue;
		}

		public void update() {
			mousepos = Mouse.getMousePos;
			self.changed = false;
			if (self.minvalue < self.maxvalue && self.curvalue < self.minvalue) {
				self.curvalue = self.minvalue;
			}
			if (self.disabled) return false;
			if (!Input.repeat(Input.MOUSELEFT)) return false;
			if (!mousepos) return false;
			left = toAbsoluteRect(@leftarrow);
			right = toAbsoluteRect(@rightarrow);
			oldvalue = self.curvalue;
			repeattime = Input.time(Input.MOUSELEFT);
			// Left arrow
			if (left.contains(mousepos[0], mousepos[1])) {
				if (repeattime > 3.0) {
					self.curvalue -= 10;
				} else if (repeattime > 1.5) {
					self.curvalue -= 5;
				} else {
					self.curvalue -= 1;
				}
				self.changed = (self.curvalue != oldvalue);
				self.invalidate;
			}
			// Right arrow
			if (right.contains(mousepos[0], mousepos[1])) {
				if (repeattime > 3.0) {
					self.curvalue += 10;
				} else if (repeattime > 1.5) {
					self.curvalue += 5;
				} else {
					self.curvalue += 1;
				}
				self.changed = (self.curvalue != oldvalue);
				self.invalidate;
			}
		}

		public void refresh() {
			bitmap = self.bitmap;
			if (@maxoptionwidth.null()) {
				for (int i = @options.length; i < @options.length; i++) { //for '@options.length' times do => |i|
					w = self.bitmap.text_size(" " + @options[i] + " ").width;
					if (!@maxoptionwidth || @maxoptionwidth < w) @maxoptionwidth = w;
				}
			}
			x = self.x;
			y = self.y;
			width = self.width;
			height = self.height;
			color = new Color(120, 120, 120);
			bitmap.fill_rect(x, y, width, height, new Color(0, 0, 0, 0));
			size = bitmap.text_size(self.label).width;
			leftarrows = bitmap.text_size(" <<");
			rightarrows = bitmap.text_size(">> ");
			bitmap.font.color = color;
			shadowtext(bitmap, x, y, size, height, self.label);
			x += size;
			shadowtext(bitmap, x, y, leftarrows.width, height, " <<",
								self.disabled || self.curvalue == self.minvalue);
			@leftarrow = new Rect(x, y, leftarrows.width, height);
			x += leftarrows.width;
			if (!self.disabled) {
				bitmap.font.color = color;
				shadowtext(bitmap, x, y, @maxoptionwidth, height, $" {@options[self.curvalue]} ", false, 1);
			}
			x += @maxoptionwidth;
			shadowtext(bitmap, x, y, rightarrows.width, height, ">> ",
								self.disabled || self.curvalue == self.maxvalue);
			@rightarrow = new Rect(x, y, rightarrows.width, height);
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class OptionalTextSlider : TextSlider {
		public void initialize(label, options, curval) {
			@slider = new TextSlider(label, options, curval);
			@checkbox = new Checkbox("");
		}

		public void curvalue() {
			return @checkbox.checked ? @slider.curvalue : null;
		}

		public int curvalue { set {
			slider.curvalue = value;
			}
		}

		public void checked() {
			return @checkbox.checked;
		}

		public int checked { set {
			@checkbox.checked = value;
			}
		}

		public bool invalid() {
			return @slider.invalid() || @checkbox.invalid();
		}

		public void invalidate() {
			@slider.invalidate;
			@checkbox.invalidate;
		}

		public bool validate() {
			@slider.validate;
			@checkbox.validate;
		}

		public void changed() {
			return @slider.changed || @checkbox.changed;
		}

		public void minvalue() {
			return @slider.minvalue;
		}

		public int minvalue { set {
			slider.minvalue = value;
			}
		}

		public void maxvalue() {
			return @slider.maxvalue;
		}

		public int maxvalue { set {
			slider.maxvalue = value;
			}
		}

		public void update() {
			updatedefs;
			@slider.update;
			@checkbox.update;
		}

		public void refresh() {
			updatedefs;
			@slider.refresh;
			@checkbox.refresh;
		}

		//---------------------------------------------------------------------------

		private;

		public void updatedefs() {
			checkboxwidth = 32;
			@slider.bitmap = self.bitmap;
			@slider.parent = self.parent;
			@checkbox.x = self.x;
			@checkbox.y = self.y;
			@checkbox.width = checkboxwidth;
			@checkbox.height = self.height;
			@checkbox.bitmap = self.bitmap;
			@checkbox.parent = self.parent;
			@slider.x = self.x + checkboxwidth + 4;
			@slider.y = self.y;
			@slider.width = self.width - checkboxwidth;
			@slider.height = self.height;
			@slider.disabled = !@checkbox.checked;
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class ControlWindow : SpriteWindow_Base {
		public int controls		{ get { return _controls; } }			protected int _controls;

		public override void initialize(x, y, width, height) {
			base.initialize(x, y, width, height);
			self.contents = new Bitmap(width - 32, height - 32);
			SetNarrowFont(self.contents);
			@controls = new List<string>();
		}

		public override void dispose() {
			self.contents.dispose;
			base.dispose();
		}

		public void refresh() {
			@controls.each(ctrl => ctrl.refresh);
		}

		public void repaint() {
			@controls.each(ctrl => ctrl.repaint);
		}

		public void invalidate() {
			@controls.each(ctrl => ctrl.invalidate);
		}

		public bool hittest(i) {
			mousepos = Mouse.getMousePos;
			if (!mousepos) return false;
			if (i < 0 || i >= @controls.length) return false;
			rc = new Rect(@controls[i].parentX, @controls[i].parentY,
										@controls[i].width, @controls[i].height);
			return rc.contains(mousepos[0], mousepos[1]);
		}

		public void addControl(control) {
			i = @controls.length;
			@controls[i] = control;
			@controls[i].x = 0;
			@controls[i].y = i * 32;
			@controls[i].width = self.contents.width;
			@controls[i].height = 32;
			@controls[i].parent = self;
			@controls[i].bitmap = self.contents;
			@controls[i].invalidate;
			refresh;
			return i;
		}

		public void addLabel(label) {
			return addControl(new Label(label));
		}

		public void addButton(label) {
			return addControl(new Button(label));
		}

		public void addSlider(label, minvalue, maxvalue, curvalue) {
			return addControl(new Slider(label, minvalue, maxvalue, curvalue));
		}

		public void addOptionalSlider(label, minvalue, maxvalue, curvalue) {
			return addControl(new OptionalSlider(label, minvalue, maxvalue, curvalue));
		}

		public void addTextSlider(label, options, curvalue) {
			return addControl(new TextSlider(label, options, curvalue));
		}

		public void addOptionalTextSlider(label, options, curvalue) {
			return addControl(new OptionalTextSlider(label, options, curvalue));
		}

		public void addCheckbox(label) {
			return addControl(new Checkbox(label));
		}

		public void addSpace() {
			return addControl(new UIControl(""));
		}

		public override void update() {
			base.update();
			@controls.each(ctrl => ctrl.update);
			repaint;
		}

		public bool changed(i) {
			if (i < 0) return false;
			return @controls[i].changed;
		}

		public void value(i) {
			if (i < 0) return false;
			return @controls[i].curvalue;
		}
	}
}
