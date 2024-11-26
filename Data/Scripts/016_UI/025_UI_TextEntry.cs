//===============================================================================
//
//===============================================================================
public partial class Window_CharacterEntry : Window_DrawableCommand {
	public const int XSIZE = 13;
	public const int YSIZE = 4;

	public override void initialize(charset, viewport = null) {
		@viewport = viewport;
		@charset = charset;
		@othercharset = "";
		base.initialize(0, 96, 480, 192);
		self.baseColor, self.shadowColor = getDefaultTextColors(self.windowskin);
		self.columns = XSIZE;
		refresh;
	}

	public void setOtherCharset(value) {
		@othercharset = value.clone;
		refresh;
	}

	public void setCharset(value) {
		@charset = value.clone;
		refresh;
	}

	public void character() {
		if (self.index < 0 || self.index >= @charset.length) {
			return "";
		} else {
			return @charset[self.index];
		}
	}

	public void command() {
		if (self.index == @charset.length) return -1;
		if (self.index == @charset.length + 1) return -2;
		if (self.index == @charset.length + 2) return -3;
		return self.index;
	}

	public void itemCount() {
		return @charset.length + 3;
	}

	public void drawItem(index, _count, rect) {
		rect = drawCursor(index, rect);
		if (index == @charset.length) { // -1
			DrawShadowText(self.contents, rect.x, rect.y, rect.width, rect.height, "[ ]",
											self.baseColor, self.shadowColor);
		} else if (index == @charset.length + 1) { // -2
			DrawShadowText(self.contents, rect.x, rect.y, rect.width, rect.height, @othercharset,
											self.baseColor, self.shadowColor);
		} else if (index == @charset.length + 2) { // -3
			DrawShadowText(self.contents, rect.x, rect.y, rect.width, rect.height, _INTL("OK"),
											self.baseColor, self.shadowColor);
		} else {
			DrawShadowText(self.contents, rect.x, rect.y, rect.width, rect.height, @charset[index],
											self.baseColor, self.shadowColor);
		}
	}
}

//===============================================================================
// Text entry screen - free typing.
//===============================================================================
public partial class PokemonEntryScene {
	@@Characters = new {
		new {("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz").scan(/./), "[*]"},
		new {("0123456789   !@\#$%^&*()   ~`-_+=new List<string>()[]   :;'\"<>,.?/   ").scan(/./), "[A]"};
	}
	public const bool USEKEYBOARD = true;

	public void StartScene(helptext, minlength, maxlength, initialText, subject = 0, pokemon = null) {
		@sprites = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		if (USEKEYBOARD) {
			@sprites["entry"] = new Window_TextEntry_Keyboard(
				initialText, 0, 0, 400 - 112, 96, helptext, true
			);
			Input.text_input = true;
		} else {
			@sprites["entry"] = new Window_TextEntry(initialText, 0, 0, 400, 96, helptext, true);
		}
		@sprites["entry"].x = (Graphics.width / 2) - (@sprites["entry"].width / 2) + 32;
		@sprites["entry"].viewport = @viewport;
		@sprites["entry"].visible = true;
		@minlength = minlength;
		@maxlength = maxlength;
		@symtype = 0;
		@sprites["entry"].maxlength = maxlength;
		if (!USEKEYBOARD) {
			@sprites["entry2"] = new Window_CharacterEntry(@@Characters[@symtype][0]);
			@sprites["entry2"].setOtherCharset(@@Characters[@symtype][1]);
			@sprites["entry2"].viewport = @viewport;
			@sprites["entry2"].visible = true;
			@sprites["entry2"].x = (Graphics.width / 2) - (@sprites["entry2"].width / 2);
		}
		if (minlength == 0) {
			@sprites["helpwindow"] = Window_UnformattedTextPokemon.newWithSize(
				_INTL("Enter text using the keyboard. Press\nEnter to confirm, or Esc to cancel."),
				32, Graphics.height - 96, Graphics.width - 64, 96, @viewport
			);
		} else {
			@sprites["helpwindow"] = Window_UnformattedTextPokemon.newWithSize(
				_INTL("Enter text using the keyboard.\nPress Enter to confirm."),
				32, Graphics.height - 96, Graphics.width - 64, 96, @viewport
			);
		}
		@sprites["helpwindow"].letterbyletter = false;
		@sprites["helpwindow"].viewport = @viewport;
		@sprites["helpwindow"].visible = USEKEYBOARD;
		@sprites["helpwindow"].baseColor = new Color(16, 24, 32);
		@sprites["helpwindow"].shadowColor = new Color(168, 184, 184);
		addBackgroundPlane(@sprites, "background", "Naming/bg_2", @viewport);
		switch (subject) {
			case 1:   // Player
				meta = GameData.PlayerMetadata.get(Game.GameData.player.character_ID);
				if (meta) {
					@sprites["shadow"] = new IconSprite(0, 0, @viewport);
					@sprites["shadow"].setBitmap("Graphics/UI/Naming/icon_shadow");
					@sprites["shadow"].x = 66;
					@sprites["shadow"].y = 64;
					filename = GetPlayerCharset(meta.walk_charset, null, true);
					@sprites["subject"] = new TrainerWalkingCharSprite(filename, @viewport);
					charwidth = @sprites["subject"].bitmap.width;
					charheight = @sprites["subject"].bitmap.height;
					@sprites["subject"].x = 88 - (charwidth / 8);
					@sprites["subject"].y = 76 - (charheight / 4);
				}
				break;
			case 2:   // Pokémon
				if (pokemon) {
					@sprites["shadow"] = new IconSprite(0, 0, @viewport);
					@sprites["shadow"].setBitmap("Graphics/UI/Naming/icon_shadow");
					@sprites["shadow"].x = 66;
					@sprites["shadow"].y = 64;
					@sprites["subject"] = new PokemonIconSprite(pokemon, @viewport);
					@sprites["subject"].setOffset(PictureOrigin.CENTER);
					@sprites["subject"].x = 88;
					@sprites["subject"].y = 54;
					@sprites["gender"] = new BitmapSprite(32, 32, @viewport);
					@sprites["gender"].x = 430;
					@sprites["gender"].y = 54;
					@sprites["gender"].bitmap.clear;
					SetSystemFont(@sprites["gender"].bitmap);
					textpos = new List<string>();
					if (pokemon.male()) {
						textpos.Add(new {_INTL("♂"), 0, 6, :left, new Color(0, 128, 248), new Color(168, 184, 184)});
					} else if (pokemon.female()) {
						textpos.Add(new {_INTL("♀"), 0, 6, :left, new Color(248, 24, 24), new Color(168, 184, 184)});
					}
					DrawTextPositions(@sprites["gender"].bitmap, textpos);
				}
				break;
			case 3:   // NPC
				@sprites["shadow"] = new IconSprite(0, 0, @viewport);
				@sprites["shadow"].setBitmap("Graphics/UI/Naming/icon_shadow");
				@sprites["shadow"].x = 66;
				@sprites["shadow"].y = 64;
				@sprites["subject"] = new TrainerWalkingCharSprite(pokemon.ToString(), @viewport);
				charwidth = @sprites["subject"].bitmap.width;
				charheight = @sprites["subject"].bitmap.height;
				@sprites["subject"].x = 88 - (charwidth / 8);
				@sprites["subject"].y = 76 - (charheight / 4);
				break;
			case 4:   // Storage box
				@sprites["subject"] = new TrainerWalkingCharSprite(null, @viewport);
				@sprites["subject"].altcharset = "Graphics/UI/Naming/icon_storage";
				@sprites["subject"].anim_duration = 0.4;
				charwidth = @sprites["subject"].bitmap.width;
				charheight = @sprites["subject"].bitmap.height;
				@sprites["subject"].x = 88 - (charwidth / 8);
				@sprites["subject"].y = 52 - (charheight / 2);
				break;
		}
		FadeInAndShow(@sprites);
	}

	public void Entry1() {
		ret = "";
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			if (Input.triggerex(:ESCAPE) && @minlength == 0) {
				ret = "";
				break;
			} else if (Input.triggerex(:RETURN) && @sprites["entry"].text.length >= @minlength) {
				ret = @sprites["entry"].text;
				break;
			}
			@sprites["helpwindow"].update;
			@sprites["entry"].update;
			@sprites["subject"]&.update;
		}
		Input.update;
		return ret;
	}

	public void Entry2() {
		ret = "";
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			@sprites["helpwindow"].update;
			@sprites["entry"].update;
			@sprites["entry2"].update;
			@sprites["subject"]&.update;
			if (Input.trigger(Input.USE)) {
				index = @sprites["entry2"].command;
				if (index == -3) { // Confirm text
					ret = @sprites["entry"].text;
					if (ret.length < @minlength || ret.length > @maxlength) {
						PlayBuzzerSE;
					} else {
						PlayDecisionSE;
						break;
					}
				} else if (index == -1) {   // Insert a space
					if (@sprites["entry"].insert(" ")) {
						PlayDecisionSE;
					} else {
						PlayBuzzerSE;
					}
				} else if (index == -2) {   // Change character set
					PlayDecisionSE;
					@symtype += 1;
					if (@symtype >= @@Characters.length) @symtype = 0;
					@sprites["entry2"].setCharset(@@Characters[@symtype][0]);
					@sprites["entry2"].setOtherCharset(@@Characters[@symtype][1]);
				} else {   // Insert given character
					if (@sprites["entry"].insert(@sprites["entry2"].character)) {
						PlayDecisionSE;
					} else {
						PlayBuzzerSE;
					}
				}
				continue;
			}
		}
		Input.update;
		return ret;
	}

	public void Entry() {
		return USEKEYBOARD ? Entry1 : Entry2;
	}

	public void EndScene() {
		FadeOutAndHide(@sprites);
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
		if (USEKEYBOARD) Input.text_input = false;
	}
}

//===============================================================================
// Text entry screen - arrows to select letter.
//===============================================================================
public partial class PokemonEntryScene2 {
	@@Characters = new {
		new {("ABCDEFGHIJ ,." + "KLMNOPQRST '-" + "UVWXYZ     ♂♀" + "             " + "0123456789   ").scan(/./), _INTL("UPPER")},
		new {("abcdefghij ,." + "klmnopqrst '-" + "uvwxyz     ♂♀" + "             " + "0123456789   ").scan(/./), _INTL("lower")},
		new {("ÀÁÂÄÃàáâäã Ææ" + "ÈÉÊË èéêë  Çç" + "ÌÍÎÏ ìíîï  Œœ" + "ÒÓÔÖÕòóôöõ Ññ" + "ÙÚÛÜ ùúûü  Ýý").scan(/./), _INTL("accents")},
		new {(",.:;…•!?¡¿ ♂♀" + "“”‘’﴾﴿*~_^ ΡΚ" + "@\#&%+-×÷/= ΠΜ" + "◎○□△♠♥♦♣★✨  $" + "♈♌♒♐♩♪♫☽☾    ").scan(/./), _INTL("other")};
	}
	public const int ROWS    = 13;
	public const int COLUMNS = 5;
	public const int MODE1   = -6;
	public const int MODE2   = -5;
	public const int MODE3   = -4;
	public const int MODE4   = -3;
	public const int BACK    = -2;
	public const int OK      = -1;

	public partial class NameEntryCursor {
		public void initialize(viewport) {
			@sprite = new Sprite(viewport);
			@cursortype = 0;
			@cursor1 = new AnimatedBitmap("Graphics/UI/Naming/cursor_1");
			@cursor2 = new AnimatedBitmap("Graphics/UI/Naming/cursor_2");
			@cursor3 = new AnimatedBitmap("Graphics/UI/Naming/cursor_3");
			@cursorPos = 0;
			updateInternal;
		}

		public void setCursorPos(value) {
			@cursorPos = value;
		}

		public void updateCursorPos() {
			value = @cursorPos;
			switch (value) {
				case PokemonEntryScene2.MODE1:   // Upper case
					@sprite.x = 44;
					@sprite.y = 120;
					@cursortype = 1;
					break;
				case PokemonEntryScene2.MODE2:   // Lower case
					@sprite.x = 106;
					@sprite.y = 120;
					@cursortype = 1;
					break;
				case PokemonEntryScene2.MODE3:   // Accents
					@sprite.x = 168;
					@sprite.y = 120;
					@cursortype = 1;
					break;
				case PokemonEntryScene2.MODE4:   // Other symbols
					@sprite.x = 230;
					@sprite.y = 120;
					@cursortype = 1;
					break;
				case PokemonEntryScene2.BACK:   // Back
					@sprite.x = 314;
					@sprite.y = 120;
					@cursortype = 2;
					break;
				case PokemonEntryScene2.OK:   // OK
					@sprite.x = 394;
					@sprite.y = 120;
					@cursortype = 2;
					break;
				default:
					if (value >= 0) {
						@sprite.x = 52 + (32 * (value % PokemonEntryScene2.ROWS));
						@sprite.y = 180 + (38 * (value / PokemonEntryScene2.ROWS));
						@cursortype = 0;
					}
					break;
			}
		}

		public int visible { set {
			@sprite.visible = value;
			}
		}

		public void visible() {
			@sprite.visible;
		}

		public int color { set {
			@sprite.color = value;
			}
		}

		public void color() {
			@sprite.color;
		}

		public bool disposed() {
			@sprite.disposed();
		}

		public void updateInternal() {
			@cursor1.update;
			@cursor2.update;
			@cursor3.update;
			updateCursorPos;
			switch (@cursortype) {
				case 0:  @sprite.bitmap = @cursor1.bitmap; break;
				case 1:  @sprite.bitmap = @cursor2.bitmap; break;
				case 2:  @sprite.bitmap = @cursor3.bitmap; break;
			}
		}

		public void update() {
			updateInternal;
		}

		public void dispose() {
			@cursor1.dispose;
			@cursor2.dispose;
			@cursor3.dispose;
			@sprite.dispose;
		}
	}

	public void StartScene(helptext, minlength, maxlength, initialText, subject = 0, pokemon = null) {
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@helptext = helptext;
		@helper = new CharacterEntryHelper(initialText);
		// Create bitmaps
		@bitmaps = new List<string>();
		for (int i = @@Characters.length; i < @@Characters.length; i++) { //for '@@Characters.length' times do => |i|
			@bitmaps[i] = new AnimatedBitmap(string.Format("Graphics/UI/Naming/overlay_tab_{0}", i + 1));
			b = @bitmaps[i].bitmap.clone;
			SetSystemFont(b);
			textPos = new List<string>();
			for (int y = COLUMNS; y < COLUMNS; y++) { //for 'COLUMNS' times do => |y|
				for (int x = ROWS; x < ROWS; x++) { //for 'ROWS' times do => |x|
					pos = (y * ROWS) + x;
					textPos.Add(new {@@Characters[i][0][pos], 44 + (x * 32), 24 + (y * 38), :center,
												new Color(16, 24, 32), new Color(160, 160, 160)});
				}
			}
			DrawTextPositions(b, textPos);
			@bitmaps[@@Characters.length + i] = b;
		}
		underline_bitmap = new Bitmap(24, 6);
		underline_bitmap.fill_rect(2, 2, 22, 4, new Color(168, 184, 184));
		underline_bitmap.fill_rect(0, 0, 22, 4, new Color(16, 24, 32));
		@bitmaps.Add(underline_bitmap);
		// Create sprites
		@sprites = new List<string>();
		@sprites["bg"] = new IconSprite(0, 0, @viewport);
		@sprites["bg"].setBitmap("Graphics/UI/Naming/bg");
		switch (subject) {
			case 1:   // Player
				meta = GameData.PlayerMetadata.get(Game.GameData.player.character_ID);
				if (meta) {
					@sprites["shadow"] = new IconSprite(0, 0, @viewport);
					@sprites["shadow"].setBitmap("Graphics/UI/Naming/icon_shadow");
					@sprites["shadow"].x = 66;
					@sprites["shadow"].y = 64;
					filename = GetPlayerCharset(meta.walk_charset, null, true);
					@sprites["subject"] = new TrainerWalkingCharSprite(filename, @viewport);
					charwidth = @sprites["subject"].bitmap.width;
					charheight = @sprites["subject"].bitmap.height;
					@sprites["subject"].x = 88 - (charwidth / 8);
					@sprites["subject"].y = 76 - (charheight / 4);
				}
				break;
			case 2:   // Pokémon
				if (pokemon) {
					@sprites["shadow"] = new IconSprite(0, 0, @viewport);
					@sprites["shadow"].setBitmap("Graphics/UI/Naming/icon_shadow");
					@sprites["shadow"].x = 66;
					@sprites["shadow"].y = 64;
					@sprites["subject"] = new PokemonIconSprite(pokemon, @viewport);
					@sprites["subject"].setOffset(PictureOrigin.CENTER);
					@sprites["subject"].x = 88;
					@sprites["subject"].y = 54;
					@sprites["gender"] = new BitmapSprite(32, 32, @viewport);
					@sprites["gender"].x = 430;
					@sprites["gender"].y = 54;
					@sprites["gender"].bitmap.clear;
					SetSystemFont(@sprites["gender"].bitmap);
					textpos = new List<string>();
					if (pokemon.male()) {
						textpos.Add(new {_INTL("♂"), 0, 6, :left, new Color(0, 128, 248), new Color(168, 184, 184)});
					} else if (pokemon.female()) {
						textpos.Add(new {_INTL("♀"), 0, 6, :left, new Color(248, 24, 24), new Color(168, 184, 184)});
					}
					DrawTextPositions(@sprites["gender"].bitmap, textpos);
				}
				break;
			case 3:   // NPC
				@sprites["shadow"] = new IconSprite(0, 0, @viewport);
				@sprites["shadow"].setBitmap("Graphics/UI/Naming/icon_shadow");
				@sprites["shadow"].x = 66;
				@sprites["shadow"].y = 64;
				@sprites["subject"] = new TrainerWalkingCharSprite(pokemon.ToString(), @viewport);
				charwidth = @sprites["subject"].bitmap.width;
				charheight = @sprites["subject"].bitmap.height;
				@sprites["subject"].x = 88 - (charwidth / 8);
				@sprites["subject"].y = 76 - (charheight / 4);
				break;
			case 4:   // Storage box
				@sprites["subject"] = new TrainerWalkingCharSprite(null, @viewport);
				@sprites["subject"].altcharset = "Graphics/UI/Naming/icon_storage";
				@sprites["subject"].anim_duration = 0.4;
				charwidth = @sprites["subject"].bitmap.width;
				charheight = @sprites["subject"].bitmap.height;
				@sprites["subject"].x = 88 - (charwidth / 8);
				@sprites["subject"].y = 52 - (charheight / 2);
				break;
		}
		@sprites["bgoverlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		DoUpdateOverlay;
		@blanks = new List<string>();
		@mode = 0;
		@minlength = minlength;
		@maxlength = maxlength;
		for (int i = @maxlength; i < @maxlength; i++) { //for '@maxlength' times do => |i|
			@sprites[$"blank{i}"] = new Sprite(@viewport);
			@sprites[$"blank{i}"].x = 160 + (24 * i);
			@sprites[$"blank{i}"].bitmap = @bitmaps[@bitmaps.length - 1];
			@blanks[i] = 0;
		}
		@sprites["bottomtab"] = new Sprite(@viewport);   // Current tab
		@sprites["bottomtab"].x = 22;
		@sprites["bottomtab"].y = 162;
		@sprites["bottomtab"].bitmap = @bitmaps[@@Characters.length];
		@sprites["toptab"] = new Sprite(@viewport);   // Next tab
		@sprites["toptab"].x = 22 - 504;
		@sprites["toptab"].y = 162;
		@sprites["toptab"].bitmap = @bitmaps[@@Characters.length + 1];
		@sprites["controls"] = new IconSprite(0, 0, @viewport);
		@sprites["controls"].x = 16;
		@sprites["controls"].y = 96;
		@sprites["controls"].setBitmap(_INTL("Graphics/UI/Naming/overlay_controls"));
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		DoUpdateOverlay2;
		@sprites["cursor"] = new NameEntryCursor(@viewport);
		@cursorpos = 0;
		@refreshOverlay = true;
		@sprites["cursor"].setCursorPos(@cursorpos);
		FadeInAndShow(@sprites) { Update };
	}

	public void UpdateOverlay() {
		@refreshOverlay = true;
	}

	public void DoUpdateOverlay2() {
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		modeIcon = new {_INTL("Graphics/UI/Naming/icon_mode"), 44 + (@mode * 62), 120, @mode * 60, 0, 60, 44};
		DrawImagePositions(overlay, modeIcon);
	}

	public void DoUpdateOverlay() {
		if (!@refreshOverlay) return;
		@refreshOverlay = false;
		bgoverlay = @sprites["bgoverlay"].bitmap;
		bgoverlay.clear;
		SetSystemFont(bgoverlay);
		textPositions = new {
			new {@helptext, 160, 18, :left, new Color(16, 24, 32), new Color(168, 184, 184)};
		}
		chars = @helper.textChars;
		x = 172;
		foreach (var ch in chars) { //'chars.each' do => |ch|
			textPositions.Add(new {ch, x, 54, :center, new Color(16, 24, 32), new Color(168, 184, 184)});
			x += 24;
		}
		DrawTextPositions(bgoverlay, textPositions);
	}

	public void ChangeTab(newtab = @mode + 1) {
		SEPlay("GUI naming tab swap start");
		@sprites["cursor"].visible = false;
		@sprites["toptab"].bitmap = @bitmaps[(newtab % @@Characters.length) + @@Characters.length];
		// Move bottom (old) tab down off the screen, and move top (new) tab right
		// onto the screen
		timer_start = System.uptime;
		do { //loop; while (true);
			@sprites["bottomtab"].y = lerp(162, 414, 0.5, timer_start, System.uptime);
			@sprites["toptab"].x = lerp(22 - 504, 22, 0.5, timer_start, System.uptime);
			Graphics.update;
			Input.update;
			Update;
			if (@sprites["toptab"].x >= 22 && @sprites["bottomtab"].y >= 414) break;
		}
		// Swap top and bottom tab around
		@sprites["toptab"].x, @sprites["bottomtab"].x = @sprites["bottomtab"].x, @sprites["toptab"].x;
		@sprites["toptab"].y, @sprites["bottomtab"].y = @sprites["bottomtab"].y, @sprites["toptab"].y;
		@sprites["toptab"].bitmap, @sprites["bottomtab"].bitmap = @sprites["bottomtab"].bitmap, @sprites["toptab"].bitmap;
		Graphics.update;
		Input.update;
		Update;
		// Set the current mode
		@mode = newtab % @@Characters.length;
		// Set the top tab up to be the next tab
		newtab = @bitmaps[((@mode + 1) % @@Characters.length) + @@Characters.length];
		@sprites["cursor"].visible = true;
		@sprites["toptab"].bitmap = newtab;
		@sprites["toptab"].x = 22 - 504;
		@sprites["toptab"].y = 162;
		SEPlay("GUI naming tab swap end");
		DoUpdateOverlay2;
	}

	public void Update() {
		for (int i = @@Characters.length; i < @@Characters.length; i++) { //for '@@Characters.length' times do => |i|
			@bitmaps[i].update;
		}
		// Update which inputted text's character's underline is lowered to indicate
		// which character is selected
		cursorpos = @helper.cursor.clamp(0, @maxlength - 1);
		for (int i = @maxlength; i < @maxlength; i++) { //for '@maxlength' times do => |i|
			@blanks[i] = (i == cursorpos) ? 1 : 0;
			@sprites[$"blank{i}"].y = new {78, 82}[@blanks[i]];
		}
		DoUpdateOverlay;
		UpdateSpriteHash(@sprites);
	}

	public bool ColumnEmpty(m) {
		if (m >= ROWS - 1) return false;
		chset = @@Characters[@mode][0];
		for (int i = COLUMNS; i < COLUMNS; i++) { //for 'COLUMNS' times do => |i|
			if (chset[(i * ROWS) + m] != " ") return false;
		}
		return true;
	}

	public void wrapmod(x, y) {
		result = x % y;
		if (result < 0) result += y;
		return result;
	}

	public void MoveCursor() {
		oldcursor = @cursorpos;
		cursordiv = @cursorpos / ROWS;   // The row the cursor is in
		cursormod = @cursorpos % ROWS;   // The column the cursor is in
		cursororigin = @cursorpos - cursormod;
		if (Input.repeat(Input.LEFT)) {
			if (@cursorpos < 0) {   // Controls
				@cursorpos -= 1;
				if (@cursorpos < MODE1) @cursorpos = OK;
			} else {
				do { //loop; while (true);
					cursormod = wrapmod(cursormod - 1, ROWS);
					@cursorpos = cursororigin + cursormod;
					unless (ColumnEmpty(cursormod)) break;
				}
			}
		} else if (Input.repeat(Input.RIGHT)) {
			if (@cursorpos < 0) {   // Controls
				@cursorpos += 1;
				if (@cursorpos > OK) @cursorpos = MODE1;
			} else {
				do { //loop; while (true);
					cursormod = wrapmod(cursormod + 1, ROWS);
					@cursorpos = cursororigin + cursormod;
					unless (ColumnEmpty(cursormod)) break;
				}
			}
		} else if (Input.repeat(Input.UP)) {
			if (@cursorpos < 0) {         // Controls
				switch (@cursorpos) {
					case MODE1:  @cursorpos = ROWS * (COLUMNS - 1); break;
					case MODE2:  @cursorpos = (ROWS * (COLUMNS - 1)) + 2; break;
					case MODE3:  @cursorpos = (ROWS * (COLUMNS - 1)) + 4; break;
					case MODE4:  @cursorpos = (ROWS * (COLUMNS - 1)) + 6; break;
					case BACK:   @cursorpos = (ROWS * (COLUMNS - 1)) + 9; break;
					case OK:     @cursorpos = (ROWS * (COLUMNS - 1)) + 11; break;
				}
			} else if (@cursorpos < ROWS) {   // Top row of letters
				switch (@cursorpos) {
					case 0: case 1:     @cursorpos = MODE1; break;
					case 2: case 3:     @cursorpos = MODE2; break;
					case 4: case 5:     @cursorpos = MODE3; break;
					case 6: case 7:     @cursorpos = MODE4; break;
					case 8: case 9: case 10: @cursorpos = BACK; break;
					case 11: case 12:   @cursorpos = OK; break;
				}
			} else {
				cursordiv = wrapmod(cursordiv - 1, COLUMNS);
				@cursorpos = (cursordiv * ROWS) + cursormod;
			}
		} else if (Input.repeat(Input.DOWN)) {
			if (@cursorpos < 0) {                      // Controls
				switch (@cursorpos) {
					case MODE1:  @cursorpos = 0; break;
					case MODE2:  @cursorpos = 2; break;
					case MODE3:  @cursorpos = 4; break;
					case MODE4:  @cursorpos = 6; break;
					case BACK:   @cursorpos = 9; break;
					case OK:     @cursorpos = 11; break;
				}
			} else if (@cursorpos >= ROWS * (COLUMNS - 1)) {   // Bottom row of letters
				switch (cursormod) {
					case 0: case 1:     @cursorpos = MODE1; break;
					case 2: case 3:     @cursorpos = MODE2; break;
					case 4: case 5:     @cursorpos = MODE3; break;
					case 6: case 7:     @cursorpos = MODE4; break;
					case 8: case 9: case 10: @cursorpos = BACK; break;
					default:               @cursorpos = OK; break;
				}
			} else {
				cursordiv = wrapmod(cursordiv + 1, COLUMNS);
				@cursorpos = (cursordiv * ROWS) + cursormod;
			}
		}
		if (@cursorpos != oldcursor) {   // Cursor position changed
			@sprites["cursor"].setCursorPos(@cursorpos);
			PlayCursorSE;
			return true;
		}
		return false;
	}

	public void Entry() {
		ret = "";
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (MoveCursor) continue;
			if (Input.trigger(Input.SPECIAL)) {
				ChangeTab;
			} else if (Input.trigger(Input.ACTION)) {
				@cursorpos = OK;
				@sprites["cursor"].setCursorPos(@cursorpos);
			} else if (Input.trigger(Input.BACK)) {
				@helper.delete;
				PlayCancelSE
				UpdateOverlay;
			} else if (Input.trigger(Input.USE)) {
				switch (@cursorpos) {
					case BACK:   // Backspace
						@helper.delete;
						PlayCancelSE
						UpdateOverlay;
						break;
					case OK:     // Done
						SEPlay("GUI naming confirm");
						if (@helper.length >= @minlength) {
							ret = @helper.text;
							break;
						}
						break;
					case MODE1:
						if (@mode != 0) ChangeTab(0);
						break;
					case MODE2:
						if (@mode != 1) ChangeTab(1);
						break;
					case MODE3:
						if (@mode != 2) ChangeTab(2);
						break;
					case MODE4:
						if (@mode != 3) ChangeTab(3);
						break;
					default:
						cursormod = @cursorpos % ROWS;
						cursordiv = @cursorpos / ROWS;
						charpos = (cursordiv * ROWS) + cursormod;
						chset = @@Characters[@mode][0];
						if (@helper.length >= @maxlength) @helper.delete;
						@helper.insert(chset[charpos]);
						PlayCursorSE;
						if (@helper.length >= @maxlength) {
							@cursorpos = OK;
							@sprites["cursor"].setCursorPos(@cursorpos);
						}
						UpdateOverlay;
						// Auto-switch to lowercase letters after the first uppercase letter is selected
						if (@mode == 0 && @helper.cursor == 1) ChangeTab(1);
						break;
				}
			}
		}
		Input.update;
		return ret;
	}

	public void EndScene() {
		FadeOutAndHide(@sprites) { Update };
		@bitmaps.each do |bitmap|
			bitmap&.dispose;
		}
		@bitmaps.clear;
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonEntry {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen(helptext, minlength, maxlength, initialText, mode = -1, pokemon = null) {
		@scene.StartScene(helptext, minlength, maxlength, initialText, mode, pokemon);
		ret = @scene.Entry;
		@scene.EndScene;
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public void EnterText(helptext, minlength, maxlength, initialText = "", mode = 0, pokemon = null, nofadeout = false) {
	ret = "";
	if ((Game.GameData.PokemonSystem.textinput == 1 rescue false)) {   // Keyboard
		FadeOutIn(99999, nofadeout) do;
			sscene = new PokemonEntryScene();
			sscreen = new PokemonEntry(sscene);
			ret = sscreen.StartScreen(helptext, minlength, maxlength, initialText, mode, pokemon);
		}
	} else {   // Cursor
		FadeOutIn(99999, nofadeout) do;
			sscene = new PokemonEntryScene2();
			sscreen = new PokemonEntry(sscene);
			ret = sscreen.StartScreen(helptext, minlength, maxlength, initialText, mode, pokemon);
		}
	}
	return ret;
}

public void EnterPlayerName(helptext, minlength, maxlength, initialText = "", nofadeout = false) {
	return EnterText(helptext, minlength, maxlength, initialText, 1, null, nofadeout);
}

public void EnterPokemonName(helptext, minlength, maxlength, initialText = "", pokemon = null, nofadeout = false) {
	return EnterText(helptext, minlength, maxlength, initialText, 2, pokemon, nofadeout);
}

public void EnterNPCName(helptext, minlength, maxlength, initialText = "", id = 0, nofadeout = false) {
	return EnterText(helptext, minlength, maxlength, initialText, 3, id, nofadeout);
}

public void EnterBoxName(helptext, minlength, maxlength, initialText = "", nofadeout = false) {
	return EnterText(helptext, minlength, maxlength, initialText, 4, null, nofadeout);
}
