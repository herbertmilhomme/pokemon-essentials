//===============================================================================
//
//===============================================================================
public static partial class MessageConfig {
	LIGHT_TEXT_MAIN_COLOR    = new Color(248, 248, 248);
	LIGHT_TEXT_SHADOW_COLOR  = new Color(72, 80, 88);
	DARK_TEXT_MAIN_COLOR     = new Color(80, 80, 88);
	DARK_TEXT_SHADOW_COLOR   = new Color(160, 160, 168);
	MALE_TEXT_MAIN_COLOR     = new Color(48, 80, 200);   // Used by message tag "\b"
	MALE_TEXT_SHADOW_COLOR   = new Color(208, 208, 200);
	FEMALE_TEXT_MAIN_COLOR   = new Color(224, 8, 8);   // Used by message tag "\r"
	FEMALE_TEXT_SHADOW_COLOR = new Color(208, 208, 200);
	public const string FONT_NAME                = "Power Green";
	public const int FONT_SIZE                = 27;
	public const int FONT_Y_OFFSET            = 8;
	public const string SMALL_FONT_NAME          = "Power Green Small";
	public const int SMALL_FONT_SIZE          = 21;
	public const int SMALL_FONT_Y_OFFSET      = 8;
	public const string NARROW_FONT_NAME         = "Power Green Narrow";
	public const int NARROW_FONT_SIZE         = 27;
	public const int NARROW_FONT_Y_OFFSET     = 8;
	// 0 = Pause cursor is displayed at end of text
	// 1 = Pause cursor is displayed at bottom right
	// 2 = Pause cursor is displayed at lower middle side
	public const int CURSOR_POSITION          = 1;
	public const int WINDOW_OPACITY           = 255;
	TEXT_SPEED               = null;   // Time in seconds between two characters
	@@systemFrame     = null;
	@@defaultTextSkin = null;
	@@textSpeed       = null;
	@@systemFont      = null;
	@@smallFont       = null;
	@@narrowFont      = null;

	#region Class Functions
	#endregion

	public void DefaultSystemFrame() {
		if (Game.GameData.PokemonSystem) {
			return ResolveBitmap("Graphics/Windowskins/" + Settings.MENU_WINDOWSKINS[Game.GameData.PokemonSystem.frame]) || "";
		} else {
			return ResolveBitmap("Graphics/Windowskins/" + Settings.MENU_WINDOWSKINS[0]) || "";
		}
	}

	public void DefaultSpeechFrame() {
		if (Game.GameData.PokemonSystem) {
			return ResolveBitmap("Graphics/Windowskins/" + Settings.SPEECH_WINDOWSKINS[Game.GameData.PokemonSystem.textskin]) || "";
		} else {
			return ResolveBitmap("Graphics/Windowskins/" + Settings.SPEECH_WINDOWSKINS[0]) || "";
		}
	}

	public void DefaultWindowskin() {
		skin = (Game.GameData.data_system) ? Game.GameData.data_system.windowskin_name : null;
		if (skin && skin != "") {
			skin = ResolveBitmap("Graphics/Windowskins/" + skin) || "";
		}
		if (nil_or_empty(skin)) skin = ResolveBitmap("Graphics/System/Window");
		if (nil_or_empty(skin)) skin = ResolveBitmap("Graphics/Windowskins/001-Blue01");
		return skin || "";
	}

	public void GetSystemFrame() {
		if (!@@systemFrame) {
			skin = MessageConfig.DefaultSystemFrame;
			if (nil_or_empty(skin)) skin = MessageConfig.DefaultWindowskin;
			@@systemFrame = skin || "";
		}
		return @@systemFrame;
	}

	public void GetSpeechFrame() {
		if (!@@defaultTextSkin) {
			skin = MessageConfig.DefaultSpeechFrame;
			if (nil_or_empty(skin)) skin = MessageConfig.DefaultWindowskin;
			@@defaultTextSkin = skin || "";
		}
		return @@defaultTextSkin;
	}

	public void SetSystemFrame(value) {
		@@systemFrame = ResolveBitmap(value) || "";
	}

	public void SetSpeechFrame(value) {
		@@defaultTextSkin = ResolveBitmap(value) || "";
	}

	//-----------------------------------------------------------------------------

	public void DefaultTextSpeed() {
		return (Game.GameData.PokemonSystem) ? SettingToTextSpeed(Game.GameData.PokemonSystem.textspeed) : SettingToTextSpeed(null);
	}

	public void GetTextSpeed() {
		if (!@@textSpeed) @@textSpeed = DefaultTextSpeed;
		return @@textSpeed;
	}

	public void SetTextSpeed(value) {
		@@textSpeed = value;
	}

	// Text speed is the delay in seconds between two adjacent characters being
	// shown.
	public void SettingToTextSpeed(speed) {
		switch (speed) {
			case 0:  return 4 / 80.0;    // Slow
			case 1:  return 2 / 80.0;    // Medium
			case 2:  return 1 / 80.0;    // Fast
			case 3:  return 0;           // Instant
		}
		return TEXT_SPEED || (2 / 80.0);   // Normal
	}

	//-----------------------------------------------------------------------------

	public void DefaultSystemFontName() {
		return MessageConfig.TryFonts(FONT_NAME);
	}

	public void DefaultSmallFontName() {
		return MessageConfig.TryFonts(SMALL_FONT_NAME);
	}

	public void DefaultNarrowFontName() {
		return MessageConfig.TryFonts(NARROW_FONT_NAME);
	}

	public void GetSystemFontName() {
		if (!@@systemFont) @@systemFont = DefaultSystemFontName;
		return @@systemFont;
	}

	public void GetSmallFontName() {
		if (!@@smallFont) @@smallFont = DefaultSmallFontName;
		return @@smallFont;
	}

	public void GetNarrowFontName() {
		if (!@@narrowFont) @@narrowFont = DefaultNarrowFontName;
		return @@narrowFont;
	}

	public void SetSystemFontName(value) {
		@@systemFont = MessageConfig.TryFonts(value);
		if (@@systemFont == "") @@systemFont = MessageConfig.DefaultSystemFontName;
	}

	public void SetSmallFontName(value) {
		@@smallFont = MessageConfig.TryFonts(value);
		if (@@smallFont == "") @@smallFont = MessageConfig.DefaultSmallFontName;
	}

	public void SetNarrowFontName(value) {
		@@narrowFont = MessageConfig.TryFonts(value);
		if (@@narrowFont == "") @@narrowFont = MessageConfig.DefaultNarrowFontName;
	}

	public void TryFonts(*args) {
		foreach (var a in args) { //'args.each' do => |a|
			if (!a) continue;
			switch (a) {
				case String:
					if (Font.exist(a)) return a;
					break;
				case Array:
					foreach (var aa in a) { //'a.each' do => |aa|
						ret = MessageConfig.TryFonts(aa);
						if (ret != "") return ret;
					}
					break;
			}
		}
		return "";
	}
}

//===============================================================================
// Position a window.
//===============================================================================
public void BottomRight(window) {
	window.x = Graphics.width - window.width;
	window.y = Graphics.height - window.height;
}

public void BottomLeft(window) {
	window.x = 0;
	window.y = Graphics.height - window.height;
}

public void BottomLeftLines(window, lines, width = null) {
	window.x = 0;
	window.width = width || Graphics.width;
	window.height = (window.borderY rescue 32) + (lines * 32);
	window.y = Graphics.height - window.height;
}

public void PositionFaceWindow(facewindow, msgwindow) {
	if (!facewindow) return;
	if (msgwindow) {
		if (facewindow.height <= msgwindow.height) {
			facewindow.y = msgwindow.y;
		} else {
			facewindow.y = msgwindow.y + msgwindow.height - facewindow.height;
		}
		facewindow.x = Graphics.width - facewindow.width;
		msgwindow.x = 0;
		msgwindow.width = Graphics.width - facewindow.width;
	} else {
		if (facewindow.height > Graphics.height) facewindow.height = Graphics.height;
		facewindow.x = 0;
		facewindow.y = 0;
	}
}

public void PositionNearMsgWindow(cmdwindow, msgwindow, side) {
	if (!cmdwindow) return;
	if (msgwindow) {
		height = (int)Math.Min(cmdwindow.height, Graphics.height - msgwindow.height);
		if (cmdwindow.height != height) cmdwindow.height = height;
		cmdwindow.y = msgwindow.y - cmdwindow.height;
		if (cmdwindow.y < 0) {
			cmdwindow.y = msgwindow.y + msgwindow.height;
			if (cmdwindow.y + cmdwindow.height > Graphics.height) {
				cmdwindow.y = msgwindow.y - cmdwindow.height;
			}
		}
		switch (side) {
			case :left:
				cmdwindow.x = msgwindow.x;
				break;
			case :right:
				cmdwindow.x = msgwindow.x + msgwindow.width - cmdwindow.width;
				break;
			default:
				cmdwindow.x = msgwindow.x + msgwindow.width - cmdwindow.width;
				break;
		}
	} else {
		if (cmdwindow.height > Graphics.height) cmdwindow.height = Graphics.height;
		cmdwindow.x = 0;
		cmdwindow.y = 0;
	}
}

// internal function
public void RepositionMessageWindow(msgwindow, linecount = 2) {
	msgwindow.height = (32 * linecount) + msgwindow.borderY;
	msgwindow.y = (Graphics.height) - (msgwindow.height);
	if (Game.GameData.game_system) {
		switch (Game.GameData.game_system.message_position) {
			case 0:  // up
				msgwindow.y = 0;
				break;
			case 1:  // middle
				msgwindow.y = (Graphics.height / 2) - (msgwindow.height / 2);
				break;
			case 2:
				msgwindow.y = (Graphics.height) - (msgwindow.height);
				break;
		}
		if (Game.GameData.game_system.message_frame != 0) msgwindow.opacity = 0;
	}
}

// internal function
public void UpdateMsgWindowPos(msgwindow, event, eventChanged = false) {
	if (event) {
		if (eventChanged) {
			msgwindow.resizeToFit2(msgwindow.text, Graphics.width * 2 / 3, msgwindow.height);
		}
		msgwindow.y = event.screen_y - 48 - msgwindow.height;
		if (msgwindow.y < 0) msgwindow.y = event.screen_y + 24;
		msgwindow.x = event.screen_x - (msgwindow.width / 2);
		if (msgwindow.x < 0) msgwindow.x = 0;
		if (msgwindow.x > Graphics.width - msgwindow.width) {
			msgwindow.x = Graphics.width - msgwindow.width;
		}
	} else {
		curwidth = msgwindow.width;
		if (curwidth != Graphics.width) {
			msgwindow.width = Graphics.width;
			msgwindow.width = Graphics.width;
		}
	}
}

//===============================================================================
// Determine the colour of a background.
//===============================================================================
public void isDarkBackground(background, rect = null) {
	if (!background || background.disposed()) return true;
	if (!rect) rect = background.rect;
	if (rect.width <= 0 || rect.height <= 0) return true;
	xSeg = (rect.width / 16);
	xLoop = (xSeg == 0) ? 1 : 16;
	xStart = (xSeg == 0) ? rect.x + (rect.width / 2) : rect.x + (xSeg / 2);
	ySeg = (rect.height / 16);
	yLoop = (ySeg == 0) ? 1 : 16;
	yStart = (ySeg == 0) ? rect.y + (rect.height / 2) : rect.y + (ySeg / 2);
	count = 0;
	y = yStart;
	r = g = b = 0;
	yLoop.times do;
		x = xStart;
		xLoop.times do;
			clr = background.get_pixel(x, y);
			if (clr.alpha != 0) {
				r += clr.red;
				g += clr.green;
				b += clr.blue;
				count += 1;
			}
			x += xSeg;
		}
		y += ySeg;
	}
	if (count == 0) return true;
	r /= count;
	g /= count;
	b /= count;
	return ((r * 0.299) + (g * 0.587) + (b * 0.114)) < 160;
}

public void isDarkWindowskin(windowskin) {
	if (!windowskin || windowskin.disposed()) return true;
	if (windowskin.width == 192 && windowskin.height == 128) {
		return isDarkBackground(windowskin, new Rect(0, 0, 128, 128));
	} else if (windowskin.width == 128 && windowskin.height == 128) {
		return isDarkBackground(windowskin, new Rect(0, 0, 64, 64));
	} else if (windowskin.width == 96 && windowskin.height == 48) {
		return isDarkBackground(windowskin, new Rect(32, 16, 16, 16));
	} else {
		clr = windowskin.get_pixel(windowskin.width / 2, windowskin.height / 2);
		return ((clr.red * 0.299) + (clr.green * 0.587) + (clr.blue * 0.114)) < 160;
	}
}

//===============================================================================
// Determine which text colours to use based on the darkness of the background.
//===============================================================================
public void get_text_colors_for_windowskin(windowskin, color, isDarkSkin) {
	// VX windowskin
	if (windowskin && !windowskin.disposed() && windowskin.width == 128 && windowskin.height == 128) {
		if (color >= 32) color = 0;
		x = 64 + ((color % 8) * 8);
		y = 96 + ((color / 8) * 8);
		pixel = windowskin.get_pixel(x, y);
		return pixel, pixel.get_contrast_color;
	}
	// No windowskin or not a VX windowskin
	// Base color, shadow color (these are reversed on dark windowskins)
	// Values in arrays are RGB numbers
	textcolors = new {
		new {  0, 112, 248}, new {120, 184, 232},   // 1  Blue
		new {232,  32,  16}, new {248, 168, 184},   // 2  Red
		new { 96, 176,  72}, new {174, 208, 144},   // 3  Green
		new { 72, 216, 216}, new {168, 224, 224},   // 4  Cyan
		new {208,  56, 184}, new {232, 160, 224},   // 5  Magenta
		new {232, 208,  32}, new {248, 232, 136},   // 6  Yellow
		new {160, 160, 168}, new {208, 208, 216},   // 7  Gray
		new {240, 240, 248}, new {200, 200, 208},   // 8  White
		new {114,  64, 232}, new {184, 168, 224},   // 9  Purple
		new {248, 152,  24}, new {248, 200, 152},   // 10 Orange
		MessageConfig.DARK_TEXT_MAIN_COLOR,
		MessageConfig.DARK_TEXT_SHADOW_COLOR,   // 11 Dark default
		MessageConfig.LIGHT_TEXT_MAIN_COLOR,
		MessageConfig.LIGHT_TEXT_SHADOW_COLOR;   // 12 Light default
	}
	if (color == 0 || color > textcolors.length / 2) {   // No special colour, use default
		if (isDarkSkin) {   // Dark background, light text
			return MessageConfig.LIGHT_TEXT_MAIN_COLOR, MessageConfig.LIGHT_TEXT_SHADOW_COLOR;
		}
		// Light background, dark text
		return MessageConfig.DARK_TEXT_MAIN_COLOR, MessageConfig.DARK_TEXT_SHADOW_COLOR;
	}
	// Special colour as listed above
	if (isDarkSkin && color != 12) {   // Dark background, light text
		return new Color(*textcolors[(2 * (color - 1)) + 1]), new Color(*textcolors[2 * (color - 1)]);
	}
	// Light background, dark text
	return new Color(*textcolors[2 * (color - 1)]), new Color(*textcolors[(2 * (color - 1)) + 1]);
}

public void getDefaultTextColors(windowskin) {
	// VX windowskin
	if (windowskin && !windowskin.disposed() && windowskin.width == 128 && windowskin.height == 128) {
		color = windowskin.get_pixel(64, 96);
		shadow = null;
		isDark = (color.red + color.green + color.blue) / 3 < 128;
		if (isDark) {
			shadow = new Color(color.red + 64, color.green + 64, color.blue + 64);
		} else {
			shadow = new Color(color.red - 64, color.green - 64, color.blue - 64);
		}
		return color, shadow;
	}
	// No windowskin or not a VX windowskin
	if (isDarkWindowskin(windowskin)) {
		return MessageConfig.LIGHT_TEXT_MAIN_COLOR, MessageConfig.LIGHT_TEXT_SHADOW_COLOR;   // White
	}
	return MessageConfig.DARK_TEXT_MAIN_COLOR, MessageConfig.DARK_TEXT_SHADOW_COLOR;   // Dark gray
}

//===============================================================================
// Makes sure a bitmap exists.
//===============================================================================
public void DoEnsureBitmap(bitmap, dwidth, dheight) {
	if (!bitmap || bitmap.disposed() || bitmap.width < dwidth || bitmap.height < dheight) {
		oldfont = (bitmap && !bitmap.disposed()) ? bitmap.font : null;
		bitmap&.dispose;
		bitmap = new Bitmap((int)Math.Max(1, dwidth), (int)Math.Max(1, dheight));
		(oldfont) ? bitmap.font = oldfont : SetSystemFont(bitmap)
		if (bitmap.font.respond_to("shadow")) bitmap.font.shadow = false;
	}
	return bitmap;
}

//===============================================================================
// Set a bitmap's font.
//===============================================================================
// Sets a bitmap's font to the system font.
public void SetSystemFont(bitmap) {
	bitmap.font.name = MessageConfig.GetSystemFontName;
	bitmap.font.size = MessageConfig.FONT_SIZE;
	bitmap.text_offset_y = MessageConfig.FONT_Y_OFFSET;
}

// Sets a bitmap's font to the system small font.
public void SetSmallFont(bitmap) {
	bitmap.font.name = MessageConfig.GetSmallFontName;
	bitmap.font.size = MessageConfig.SMALL_FONT_SIZE;
	bitmap.text_offset_y = MessageConfig.SMALL_FONT_Y_OFFSET;
}

// Sets a bitmap's font to the system narrow font.
public void SetNarrowFont(bitmap) {
	bitmap.font.name = MessageConfig.GetNarrowFontName;
	bitmap.font.size = MessageConfig.NARROW_FONT_SIZE;
	bitmap.text_offset_y = MessageConfig.NARROW_FONT_Y_OFFSET;
}

//===============================================================================
// Blend colours, set the colour of all bitmaps in a sprite hash.
//===============================================================================
public void AlphaBlend(dstColor, srcColor) {
	r = (255 * (srcColor.red - dstColor.red) / 255) + dstColor.red;
	g = (255 * (srcColor.green - dstColor.green) / 255) + dstColor.green;
	b = (255 * (srcColor.blue - dstColor.blue) / 255) + dstColor.blue;
	a = (255 * (srcColor.alpha - dstColor.alpha) / 255) + dstColor.alpha;
	return new Color(r, g, b, a);
}

public void SrcOver(dstColor, srcColor) {
	er = srcColor.red * srcColor.alpha / 255;
	eg = srcColor.green * srcColor.alpha / 255;
	eb = srcColor.blue * srcColor.alpha / 255;
	iea = 255 - srcColor.alpha;
	cr = dstColor.red * dstColor.alpha / 255;
	cg = dstColor.green * dstColor.alpha / 255;
	cb = dstColor.blue * dstColor.alpha / 255;
	ica = 255 - dstColor.alpha;
	a = 255 - ((iea * ica) / 255);
	r = ((iea * cr) / 255) + er;
	g = ((iea * cg) / 255) + eg;
	b = ((iea * cb) / 255) + eb;
	r = (a == 0) ? 0 : r * 255 / a;
	g = (a == 0) ? 0 : g * 255 / a;
	b = (a == 0) ? 0 : b * 255 / a;
	return new Color(r, g, b, a);
}

public void SetSpritesToColor(sprites, color) {
	if (!sprites || !color) return;
	colors = new List<string>();
	foreach (var i in sprites) { //'sprites.each' do => |i|
		if (!i[1] || Disposed(i[1])) continue;
		colors[i[0]] = i[1].color.clone;
		i[1].color = SrcOver(i[1].color, color);
	}
	Graphics.update;
	Input.update;
	foreach (var i in colors) { //'colors.each' do => |i|
		if (!sprites[i[0]]) continue;
		sprites[i[0]].color = i[1];
	}
}

//===============================================================================
// Update and dispose sprite hashes.
//===============================================================================
public void using(window) {
	begin;
		if (block_given()) yield;
	ensure;
		window.dispose;
	}
}

public void UpdateSpriteHash(windows) {
	foreach (var i in windows) { //'windows.each' do => |i|
		window = i[1];
		if (window) {
			if (window.is_a(Sprite) || window.is_a(Window)) {
				if (!Disposed(window)) window.update;
			} else if (window.is_a(Plane)) {
				begin;
					if (!window.disposed()) window.update;
				rescue NoMethodError;
				}
			} else if (window.respond_to("update")) {
				begin;
					window.update;
				rescue RGSSError;
				}
			}
		}
	}
}

// Disposes all objects in the specified hash.
public void DisposeSpriteHash(sprites) {
	if (!sprites) return;
	foreach (var i in sprites) { //sprites.each_key do => |i|
		DisposeSprite(sprites, i);
	}
	sprites.clear;
}

// Disposes the specified graphics object within the specified hash. Basically
// like:   sprites[id].dispose
public void DisposeSprite(sprites, id) {
	sprite = sprites[id];
	if (sprite && !Disposed(sprite)) sprite.dispose;
	sprites[id] = null;
}

public bool Disposed(x) {
	if (!x) return true;
	if (!x.is_a(Viewport)) return x.disposed();
	begin;
		x.rect = x.rect;
	rescue;
		return true;
	}
	return false;
}

//===============================================================================
// Fades and window activations for sprite hashes.
//===============================================================================
public void PushFade() {
	if (Game.GameData.game_temp) Game.GameData.game_temp.fadestate = (int)Math.Max(Game.GameData.game_temp.fadestate + 1, 0);
}

public void PopFade() {
	if (Game.GameData.game_temp) Game.GameData.game_temp.fadestate = (int)Math.Max(Game.GameData.game_temp.fadestate - 1, 0);
}

public bool IsFaded() {
	return (Game.GameData.game_temp) ? Game.GameData.game_temp.fadestate > 0 : false;
}

// FadeOutIn(z) { block }
// Fades out the screen before a block is run and fades it back in after the
// block exits.  z indicates the z-coordinate of the viewport used for this effect
public void FadeOutIn(z = 99999, nofadeout = false) {
	duration = 0.4;   // In seconds
	col = new Color(0, 0, 0, 0);
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = z;
	timer_start = System.uptime;
	do { //loop; while (true);
		col.set(0, 0, 0, lerp(0, 255, duration, timer_start, System.uptime));
		viewport.color = col;
		Graphics.update;
		Input.update;
		if (col.alpha == 255) break;
	}
	PushFade;
	begin;
		val = 0;
		if (block_given()) val = yield;
		if (val == 99999) nofadeout = true;   // Ugly hack used by Town Map in the Bag/Pokégear
	ensure;
		PopFade;
		if (!nofadeout) {
			timer_start = System.uptime;
			do { //loop; while (true);
				col.set(0, 0, 0, lerp(255, 0, duration, timer_start, System.uptime));
				viewport.color = col;
				Graphics.update;
				Input.update;
				if (col.alpha == 0) break;
			}
		}
		viewport.dispose;
	}
}

public void FadeOutInWithUpdate(z, sprites, nofadeout = false) {
	duration = 0.4;   // In seconds
	col = new Color(0, 0, 0, 0);
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = z;
	timer_start = System.uptime;
	do { //loop; while (true);
		col.set(0, 0, 0, lerp(0, 255, duration, timer_start, System.uptime));
		viewport.color = col;
		UpdateSpriteHash(sprites);
		Graphics.update;
		Input.update;
		if (col.alpha == 255) break;
	}
	PushFade;
	begin;
		if (block_given()) yield;
	ensure;
		PopFade;
		if (!nofadeout) {
			timer_start = System.uptime;
			do { //loop; while (true);
				col.set(0, 0, 0, lerp(255, 0, duration, timer_start, System.uptime));
				viewport.color = col;
				UpdateSpriteHash(sprites);
				Graphics.update;
				Input.update;
				if (col.alpha == 0) break;
			}
		}
		viewport.dispose;
	}
}

// Similar to FadeOutIn, but pauses the music as it fades out.
// Requires scripts "Audio" (for bgm_pause) and "SpriteWindow" (for FadeOutIn).
public void FadeOutInWithMusic(zViewport = 99999) {
	playingBGS = Game.GameData.game_system.getPlayingBGS;
	playingBGM = Game.GameData.game_system.getPlayingBGM;
	Game.GameData.game_system.bgm_pause(1.0);
	Game.GameData.game_system.bgs_pause(1.0);
	pos = Game.GameData.game_system.bgm_position;
	FadeOutIn(zViewport) do;
		yield;
		Game.GameData.game_system.bgm_position = pos;
		Game.GameData.game_system.bgm_resume(playingBGM);
		Game.GameData.game_system.bgs_resume(playingBGS);
	}
}

public void FadeOutAndHide(sprites) {
	duration = 0.4;   // In seconds
	col = new Color(0, 0, 0, 0);
	visiblesprites = new List<string>();
	DeactivateWindows(sprites) do;
		timer_start = System.uptime;
		do { //loop; while (true);
			col.alpha = lerp(0, 255, duration, timer_start, System.uptime);
			SetSpritesToColor(sprites, col);
			(block_given()) ? yield : UpdateSpriteHash(sprites)
			if (col.alpha == 255) break;
		}
	}
	foreach (var i in sprites) { //'sprites.each' do => |i|
		if (!i[1]) continue;
		if (Disposed(i[1])) continue;
		if (i[1].visible) visiblesprites[i[0]] = true;
		i[1].visible = false;
	}
	return visiblesprites;
}

public void FadeInAndShow(sprites, visiblesprites = null) {
	duration = 0.4;   // In seconds
	col = new Color(0, 0, 0, 0);
	if (visiblesprites) {
		foreach (var i in visiblesprites) { //'visiblesprites.each' do => |i|
			if (i[1] && sprites[i[0]] && !Disposed(sprites[i[0]])) {
				sprites[i[0]].visible = true;
			}
		}
	}
	DeactivateWindows(sprites) do;
		timer_start = System.uptime;
		do { //loop; while (true);
			col.alpha = lerp(255, 0, duration, timer_start, System.uptime);
			SetSpritesToColor(sprites, col);
			(block_given()) ? yield : UpdateSpriteHash(sprites)
			if (col.alpha == 0) break;
		}
	}
}

// Restores which windows are active for the given sprite hash.
// _activeStatuses_ is the result of a previous call to ActivateWindows
public void RestoreActivations(sprites, activeStatuses) {
	if (!sprites || !activeStatuses) return;
	foreach (var k in activeStatuses) { //activeStatuses.each_key do => |k|
		if (sprites[k].is_a(Window) && !Disposed(sprites[k])) {
			sprites[k].active = activeStatuses[k] ? true : false;
		}
	}
}

// Deactivates all windows. If a code block is given, deactivates all windows,
// runs the code in the block, and reactivates them.
public void DeactivateWindows(sprites) {
	if (block_given()) {
		ActivateWindow(sprites, null) { yield };
	} else {
		ActivateWindow(sprites, null);
	}
}

// Activates a specific window of a sprite hash. _key_ is the key of the window
// in the sprite hash. If a code block is given, deactivates all windows except
// the specified window, runs the code in the block, and reactivates them.
public void ActivateWindow(sprites, key) {
	if (!sprites) return;
	activeStatuses = new List<string>();
	foreach (var i in sprites) { //'sprites.each' do => |i|
		if (i[1].is_a(Window) && !Disposed(i[1])) {
			activeStatuses[i[0]] = i[1].active;
			i[1].active = (i[0] == key);
		}
	}
	if (block_given()) {
		begin;
			yield;
		ensure;
			RestoreActivations(sprites, activeStatuses);
		}
		return {};
	} else {
		return activeStatuses;
	}
}

//===============================================================================
// Create background planes for a sprite hash.
//===============================================================================
// Adds a background to the sprite hash.
// _planename_ is the hash key of the background.
// _background_ is a filename within the Graphics/UI/ folder and can be
//     an animated image.
// _viewport_ is a viewport to place the background in.
public void addBackgroundPlane(sprites, planename, background, viewport = null) {
	sprites[planename] = new AnimatedPlane(viewport);
	bitmapName = ResolveBitmap($"Graphics/UI/{background}");
	if (bitmapName.null()) {
		// Plane should exist in any case
		sprites[planename].bitmap = null;
		sprites[planename].visible = false;
	} else {
		sprites[planename].setBitmap(bitmapName);
		foreach (var spr in sprites) { //sprites.each_value do => |spr|
			if (spr.is_a(Window)) spr.windowskin = null;
		}
	}
}

// Adds a background to the sprite hash.
// _planename_ is the hash key of the background.
// _background_ is a filename within the Graphics/UI/ folder and can be
//       an animated image.
// _color_ is the color to use if the background can't be found.
// _viewport_ is a viewport to place the background in.
public void addBackgroundOrColoredPlane(sprites, planename, background, color, viewport = null) {
	bitmapName = ResolveBitmap($"Graphics/UI/{background}");
	if (bitmapName.null()) {
		// Plane should exist in any case
		sprites[planename] = new ColoredPlane(color, viewport);
	} else {
		sprites[planename] = new AnimatedPlane(viewport);
		sprites[planename].setBitmap(bitmapName);
		foreach (var spr in sprites) { //sprites.each_value do => |spr|
			if (spr.is_a(Window)) spr.windowskin = null;
		}
	}
}

//===============================================================================
// Ensure required method definitions.
//===============================================================================
public static partial class Graphics {
	if (!self.respond_to("width")) {
		public static void width() { return 640; }
	}

	if (!self.respond_to("height")) {
		public static void height() { return 480; }
	}
}

//===============================================================================
// Ensure required method definitions.
//===============================================================================
if !defined(_INTL);
	public void _INTL(*args) {
		string = args[0].clone;
		for (int i = 1; i < args.length; i++) { string = System.Text.RegularExpressions.Regex.Replace(string, $"\{{i}\}", args[i].ToString()); }
		return string;
	}
}

if !defined(_ISPRINTF);
	public void string.Format(*args) {
		string = args[0].clone;
		for (int i = 1; i < args.length; i++) { //each 'args.length' do => |i|
			string = System.Text.RegularExpressions.Regex.Replace(string, $"\{{i}\:([^\}]+?)\}",  m => string.Format("%" + Game.GameData.1, args[i]));
		}
		return string;
	}
}

if !defined(_MAPINTL);
	public void _MAPINTL(*args) {
		string = args[1].clone;
		for (int i = 2; i < args.length; i++) { string = System.Text.RegularExpressions.Regex.Replace(string, $"\{{i}\}", args[i + 1].ToString()); }
		return string;
	}
}
