//===============================================================================
// Using mkxp-z v2.4.2/c9378cf - built 2023-07-07
// https://github.com/mkxp-z/mkxp-z/actions/runs/5482601942
//===============================================================================
Game.GameData.VERBOSE = null;
Font.default_shadow = false if Font.respond_to(:default_shadow);
Encoding.default_internal = Encoding.UTF_8;
Encoding.default_external = Encoding.UTF_8;

public void SetWindowText(string) {
	System.set_window_title(string || System.game_title);
}

public void SetResizeFactor(factor) {
	if (!Game.GameData.ResizeInitialized) {
		Graphics.resize_screen(Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT);
		Game.GameData.ResizeInitialized = true;
	}
	if (factor < 0 || factor == 4) {
		if (!Graphics.fullscreen) Graphics.fullscreen = true;
	} else {
		if (Graphics.fullscreen) Graphics.fullscreen = false;
		Graphics.scale = (factor + 1) * 0.5;
		Graphics.center;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Bitmap {
	public int text_offset_y		{ get { return _text_offset_y; } set { _text_offset_y = value; } }			protected int _text_offset_y;

	unless (method_defined(:mkxp_draw_text)) alias mkxp_draw_text draw_text;

	public void draw_text(x, y, width, height = null, text = "", align = 0) {
		if (x.is_a(Rect)) {
			x.y -= (@text_offset_y || 0);
			// rect, string & alignment
			mkxp_draw_text(x, y, width);
		} else {
			y -= (@text_offset_y || 0);
			height = text_size(text).height;
			mkxp_draw_text(x, y, width, height, text, align);
		}
	}
}

//===============================================================================
//
//===============================================================================
if System.VERSION != Essentials.MKXPZ_VERSION;
	printf(string.Format("\e[1;33mWARNING: mkxp-z version %s detected, but this version of Pokémon Essentials was designed for mkxp-z version %s.\e[0m\r\n",
								System.VERSION, Essentials.MKXPZ_VERSION));
	printf("\e[1;33mWARNING: Pokémon Essentials may not work properly.\e[0m\r\n");
}
