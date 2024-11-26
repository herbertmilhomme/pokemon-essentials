//===============================================================================
//
//===============================================================================
public partial class CharacterEntryHelper {
	public int text		{ get { return _text; } set { _text = value; } }			protected int _text;
	public int maxlength		{ get { return _maxlength; } set { _maxlength = value; } }			protected int _maxlength;
	public int passwordChar		{ get { return _passwordChar; } }			protected int _passwordChar;
	public int cursor		{ get { return _cursor; } set { _cursor = value; } }			protected int _cursor;

	public void initialize(text) {
		@maxlength = -1;
		@text = text;
		@passwordChar = "";
		@cursor = text.scan(/./m).length;
	}

	public void textChars() {
		chars = text.scan(/./m);
		if (@passwordChar != "") {
			chars.length.times(i => chars[i] = @passwordChar);
		}
		return chars;
	}

	public int passwordChar { set {
		@passwordChar = value || "";
		}
	}

	public void length() {
		return self.text.scan(/./m).length;
	}

	public bool canInsert() {
		chars = self.text.scan(/./m);
		if (@maxlength >= 0 && chars.length >= @maxlength) return false;
		return true;
	}

	public void insert(ch) {
		chars = self.text.scan(/./m);
		if (@maxlength >= 0 && chars.length >= @maxlength) return false;
		chars.insert(@cursor, ch);
		@text = "";
		chars.each(char => { if (char) @text += char; });
		@cursor += 1;
		return true;
	}

	public bool canDelete() {
		chars = self.text.scan(/./m);
		if (chars.length <= 0 || @cursor <= 0) return false;
		return true;
	}

	public void delete() {
		chars = self.text.scan(/./m);
		if (chars.length <= 0 || @cursor <= 0) return false;
		chars.delete_at(@cursor - 1);
		@text = "";
		foreach (var ch in chars) { //'chars.each' do => |ch|
			if (ch) @text += ch;
		}
		@cursor -= 1;
		return true;
	}

	//-----------------------------------------------------------------------------

	private;

	public void ensure() {
		if (@maxlength < 0) return;
		chars = self.text.scan(/./m);
		if (chars.length > @maxlength && @maxlength >= 0) chars = chars[0, @maxlength];
		@text = "";
		foreach (var ch in chars) { //'chars.each' do => |ch|
			if (ch) @text += ch;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class Window_TextEntry : SpriteWindow_Base {
	public override void initialize(text, x, y, width, height, heading = null, usedarkercolor = false) {
		base.initialize(x, y, width, height);
		@baseColor, @shadowColor = getDefaultTextColors(self.windowskin);
		if (usedarkercolor) {
			@baseColor = new Color(16, 24, 32);
			@shadowColor = new Color(168, 184, 184);
		}
		@helper = new CharacterEntryHelper(text);
		@heading = heading;
		@cursor_timer_start = System.uptime;
		@cursor_shown = true;
		self.active = true;
		refresh;
	}

	public void text() {
		@helper.text;
	}

	public void maxlength() {
		@helper.maxlength;
	}

	public void passwordChar() {
		@helper.passwordChar;
	}

	public int text { set {
		@helper.text = value;
		self.refresh;
		}
	}

	public int passwordChar { set {
		@helper.passwordChar = value;
		refresh;
		}
	}

	public int maxlength { set {
		@helper.maxlength = value;
		self.refresh;
		}
	}

	public void insert(ch) {
		if (@helper.insert(ch)) {
			@cursor_timer_start = System.uptime;
			@cursor_shown = true;
			self.refresh;
			return true;
		}
		return false;
	}

	public void delete() {
		if (@helper.delete) {
			@cursor_timer_start = System.uptime;
			@cursor_shown = true;
			self.refresh;
			return true;
		}
		return false;
	}

	public void update() {
		cursor_to_show = ((System.uptime - @cursor_timer_start) / 0.35).ToInt().even();
		if (cursor_to_show != @cursor_shown) {
			@cursor_shown = cursor_to_show;
			refresh;
		}
		if (!self.active) return;
		// Moving cursor
		if (Input.repeat(Input.LEFT) && Input.press(Input.ACTION)) {
			if (@helper.cursor > 0) {
				@helper.cursor -= 1;
				@cursor_timer_start = System.uptime;
				@cursor_shown = true;
				self.refresh;
			}
		} else if (Input.repeat(Input.RIGHT) && Input.press(Input.ACTION)) {
			if (@helper.cursor < self.text.scan(/./m).length) {
				@helper.cursor += 1;
				@cursor_timer_start = System.uptime;
				@cursor_shown = true;
				self.refresh;
			}
		} else if (Input.repeat(Input.BACK)) {   // Backspace
			if (@helper.cursor > 0) self.delete;
		}
	}

	public void refresh() {
		self.contents = DoEnsureBitmap(self.contents, self.width - self.borderX,
																		self.height - self.borderY);
		bitmap = self.contents;
		bitmap.clear;
		x = 0;
		y = 0;
		if (@heading) {
			textwidth = bitmap.text_size(@heading).width;
			DrawShadowText(bitmap, x, y, textwidth + 4, 32, @heading, @baseColor, @shadowColor);
			y += 32;
		}
		x += 4;
		width = self.width - self.borderX;
		cursorcolor = new Color(16, 24, 32);
		textscan = self.text.scan(/./m);
		scanlength = textscan.length;
		if (@helper.cursor > scanlength) @helper.cursor = scanlength;
		if (@helper.cursor < 0) @helper.cursor = 0;
		startpos = @helper.cursor;
		fromcursor = 0;
		while (startpos > 0) {
			c = (@helper.passwordChar != "") ? @helper.passwordChar : textscan[startpos - 1];
			fromcursor += bitmap.text_size(c).width;
			if (fromcursor > width - 4) break;
			startpos -= 1;
		}
		for (int i = startpos; i < scanlength; i++) { //each 'scanlength' do => |i|
			c = (@helper.passwordChar != "") ? @helper.passwordChar : textscan[i];
			textwidth = bitmap.text_size(c).width;
			if (c == "\n") continue;
			// Draw text
			DrawShadowText(bitmap, x, y, textwidth + 4, 32, c, @baseColor, @shadowColor);
			// Draw cursor if necessary
			if (i == @helper.cursor && @cursor_shown) {
				bitmap.fill_rect(x, y + 4, 2, 24, cursorcolor);
			}
			// Add x to drawn text width
			x += textwidth;
		}
		if (textscan.length == @helper.cursor && @cursor_shown) {
			bitmap.fill_rect(x, y + 4, 2, 24, cursorcolor);
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class Window_TextEntry_Keyboard : Window_TextEntry {
	public void update() {
		cursor_to_show = ((System.uptime - @cursor_timer_start) / 0.35).ToInt().even();
		if (cursor_to_show != @cursor_shown) {
			@cursor_shown = cursor_to_show;
			refresh;
		}
		if (!self.active) return;
		// Moving cursor
		if (Input.triggerex(:LEFT) || Input.repeatex(:LEFT)) {
			if (@helper.cursor > 0) {
				@helper.cursor -= 1;
				@cursor_timer_start = System.uptime;
				@cursor_shown = true;
				self.refresh;
			}
			return;
		} else if (Input.triggerex(:RIGHT) || Input.repeatex(:RIGHT)) {
			if (@helper.cursor < self.text.scan(/./m).length) {
				@helper.cursor += 1;
				@cursor_timer_start = System.uptime;
				@cursor_shown = true;
				self.refresh;
			}
			return;
		} else if (Input.triggerex(:BACKSPACE) || Input.repeatex(:BACKSPACE)) {
			if (@helper.cursor > 0) self.delete;
			return;
		} else if (Input.triggerex(:RETURN) || Input.triggerex(:ESCAPE)) {
			return;
		}
		Input.gets.each_char(c => insert(c));
	}
}

//===============================================================================
//
//===============================================================================
public partial class Window_MultilineTextEntry : SpriteWindow_Base {
	public override void initialize(text, x, y, width, height) {
		base.initialize(x, y, width, height);
		@baseColor, @shadowColor = getDefaultTextColors(self.windowskin);
		@helper = new CharacterEntryHelper(text);
		@firstline = 0;
		@cursorLine = 0;
		@cursorColumn = 0;
		@cursor_timer_start = System.uptime;
		@cursor_shown = true;
		self.active = true;
		refresh;
	}

	public int baseColor		{ get { return _baseColor; } }			protected int _baseColor;
	public int shadowColor		{ get { return _shadowColor; } }			protected int _shadowColor;

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

	public void text() {
		@helper.text;
	}

	public void maxlength() {
		@helper.maxlength;
	}

	public int text { set {
		@helper.text = value;
		@textchars = null;
		self.refresh;
		}
	}

	public int maxlength { set {
		@helper.maxlength = value;
		@textchars = null;
		self.refresh;
		}
	}

	public void insert(ch) {
		@helper.cursor = getPosFromLineAndColumn(@cursorLine, @cursorColumn);
		if (@helper.insert(ch)) {
			@cursor_timer_start = System.uptime;
			@cursor_shown = true;
			@textchars = null;
			moveCursor(0, 1);
			self.refresh;
			return true;
		}
		return false;
	}

	public void delete() {
		@helper.cursor = getPosFromLineAndColumn(@cursorLine, @cursorColumn);
		if (@helper.delete) {
			@cursor_timer_start = System.uptime;
			@cursor_shown = true;
			moveCursor(0, -1); // use old textchars
			@textchars = null;
			self.refresh;
			return true;
		}
		return false;
	}

	public void getTextChars() {
		if (!@textchars) {
			@textchars = getLineBrokenText(self.contents, @helper.text,
																		self.contents.width, null);
		}
		return @textchars;
	}

	public void getTotalLines() {
		textchars = getTextChars;
		if (textchars.length == 0) return 1;
		tchar = textchars[textchars.length - 1];
		return tchar[5] + 1;
	}

	public void getLineY(line) {
		textchars = getTextChars;
		if (textchars.length == 0) return 0;
		totallines = getTotalLines;
		if (line < 0) line = 0;
		if (line >= totallines) line = totallines - 1;
		maximumY = 0;
		foreach (var text in textchars) { //'textchars.each' do => |text|
			thisline = text[5];
			y = text[2];
			if (thisline == line) return y;
			if (maximumY < y) maximumY = y;
		}
		return maximumY;
	}

	public void getColumnsInLine(line) {
		textchars = getTextChars;
		if (textchars.length == 0) return 0;
		totallines = getTotalLines;
		if (line < 0) line = 0;
		if (line >= totallines) line = totallines - 1;
		endpos = 0;
		foreach (var text in textchars) { //'textchars.each' do => |text|
			thisline = text[5];
			thislength = text[8];
			if (thisline == line) endpos += thislength;
		}
		return endpos;
	}

	public void getPosFromLineAndColumn(line, column) {
		textchars = getTextChars;
		if (textchars.length == 0) return 0;
		totallines = getTotalLines;
		if (line < 0) line = 0;
		if (line >= totallines) line = totallines - 1;
		endpos = 0;
		foreach (var text in textchars) { //'textchars.each' do => |text|
			thisline = text[5];
			thispos = text[6];
			thiscolumn = text[7];
			thislength = text[8];
			if (thisline != line) continue;
			endpos = thispos + thislength;
			if (column < thiscolumn || column > thiscolumn + thislength || thislength == 0) continue;
			return thispos + column - thiscolumn;
		}
		return endpos;
	}

	public void getLastVisibleLine() {
		getTextChars;
		textheight = (int)Math.Max(1, self.contents.text_size("X").height);
		lastVisible = @firstline + ((self.height - self.borderY) / textheight) - 1;
		return lastVisible;
	}

	public void updateCursorPos(doRefresh) {
		// Calculate new cursor position
		@helper.cursor = getPosFromLineAndColumn(@cursorLine, @cursorColumn);
		if (doRefresh) {
			@cursor_timer_start = System.uptime;
			@cursor_shown = true;
			self.refresh;
		}
		if (@cursorLine < @firstline) @firstline = @cursorLine;
		lastVisible = getLastVisibleLine;
		if (@cursorLine > lastVisible) @firstline += (@cursorLine - lastVisible);
	}

	public void moveCursor(lineOffset, columnOffset) {
		// Move column offset first, then lines (since column offset
		// can affect line offset)
//		echoln new {"beforemoving",@cursorLine,@cursorColumn}
		totalColumns = getColumnsInLine(@cursorLine); // check current line
		totalLines = getTotalLines;
		oldCursorLine = @cursorLine;
		oldCursorColumn = @cursorColumn;
		@cursorColumn += columnOffset;
		if (@cursorColumn < 0 && @cursorLine > 0) {
			// Will happen if cursor is moved left from the beginning of a line
			@cursorLine -= 1;
			@cursorColumn = getColumnsInLine(@cursorLine);
		} else if (@cursorColumn > totalColumns && @cursorLine < totalLines - 1) {
			// Will happen if cursor is moved right from the end of a line
			@cursorLine += 1;
			@cursorColumn = 0;
		}
		// Ensure column bounds
		totalColumns = getColumnsInLine(@cursorLine);
		if (@cursorColumn > totalColumns) @cursorColumn = totalColumns;
		if (@cursorColumn < 0) @cursorColumn = 0; // totalColumns can be 0
		// Move line offset
		@cursorLine += lineOffset;
		if (@cursorLine < 0) @cursorLine = 0;
		if (@cursorLine >= totalLines) @cursorLine = totalLines - 1;
		// Ensure column bounds again
		totalColumns = getColumnsInLine(@cursorLine);
		if (@cursorColumn > totalColumns) @cursorColumn = totalColumns;
		if (@cursorColumn < 0) @cursorColumn = 0; // totalColumns can be 0
		updateCursorPos(oldCursorLine != @cursorLine || oldCursorColumn != @cursorColumn);
//		echoln new {"aftermoving",@cursorLine,@cursorColumn}
	}

	public void update() {
		cursor_to_show = ((System.uptime - @cursor_timer_start) / 0.35).ToInt().even();
		if (cursor_to_show != @cursor_shown) {
			@cursor_shown = cursor_to_show;
			refresh;
		}
		if (!self.active) return;
		// Moving cursor
		if (Input.triggerex(:UP) || Input.repeatex(:UP)) {
			moveCursor(-1, 0);
			return;
		} else if (Input.triggerex(:DOWN) || Input.repeatex(:DOWN)) {
			moveCursor(1, 0);
			return;
		} else if (Input.triggerex(:LEFT) || Input.repeatex(:LEFT)) {
			moveCursor(0, -1);
			return;
		} else if (Input.triggerex(:RIGHT) || Input.repeatex(:RIGHT)) {
			moveCursor(0, 1);
			return;
		}
		if (Input.press(Input.CTRL) && Input.triggerex(:HOME)) {
			// Move cursor to beginning
			@cursorLine = 0;
			@cursorColumn = 0;
			updateCursorPos(true);
			return;
		} else if (Input.press(Input.CTRL) && Input.triggerex(:END)) {
			// Move cursor to end
			@cursorLine = getTotalLines - 1;
			@cursorColumn = getColumnsInLine(@cursorLine);
			updateCursorPos(true);
			return;
		} else if (Input.triggerex(:RETURN) || Input.repeatex(:RETURN)) {
			self.insert("\n");
			return;
		} else if (Input.triggerex(:BACKSPACE) || Input.repeatex(:BACKSPACE)) {   // Backspace
			self.delete;
			return;
		}
		Input.gets.each_char(c => insert(c));
	}

	public void refresh() {
		newContents = DoEnsureBitmap(self.contents, self.width - self.borderX,
																	self.height - self.borderY);
		if (self.contents != newContents) @textchars = null;
		self.contents = newContents;
		bitmap = self.contents;
		bitmap.clear;
		getTextChars;
		height = self.height - self.borderY;
		cursorcolor = Color.black;
		textchars = getTextChars;
		startY = getLineY(@firstline);
		foreach (var text in textchars) { //'textchars.each' do => |text|
			thisline = text[5];
			thislength = text[8];
			textY = text[2] - startY;
			// Don't draw lines before the first or zero-length segments
			if (thisline < @firstline || thislength == 0) continue;
			// Don't draw lines beyond the window's height
			if (textY >= height) break;
			c = text[0];
			// Don't draw spaces
			if (c == " ") continue;
			textwidth = text[3] + 4;   // add 4 to prevent draw_text from stretching text
			textheight = text[4];
			// Draw text
			DrawShadowText(bitmap, text[1], textY, textwidth, textheight, c, @baseColor, @shadowColor);
		}
		// Draw cursor
		if (@cursor_shown) {
			textheight = bitmap.text_size("X").height;
			cursorY = (textheight * @cursorLine) - startY;
			cursorX = 0;
			foreach (var text in textchars) { //'textchars.each' do => |text|
				thisline = text[5];
				thiscolumn = text[7];
				thislength = text[8];
				if (thisline != @cursorLine || @cursorColumn < thiscolumn ||
								@cursorColumn > thiscolumn + thislength) continue;
				cursorY = text[2] - startY;
				cursorX = text[1];
				textheight = text[4];
				posToCursor = @cursorColumn - thiscolumn;
				if (posToCursor >= 0) {
					partialString = text[0].scan(/./m)[0, posToCursor].join;
					cursorX += bitmap.text_size(partialString).width;
				}
				break;
			}
			cursorY += 4;
			cursorHeight = (int)Math.Max(4, textheight - 4, bitmap.text_size("X").height - 4);
			bitmap.fill_rect(cursorX, cursorY, 2, cursorHeight, cursorcolor);
		}
	}
}
