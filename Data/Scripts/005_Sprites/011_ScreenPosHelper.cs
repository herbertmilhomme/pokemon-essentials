//===============================================================================
//
//===============================================================================
public static partial class ScreenPosHelper {
	@heightcache = new List<string>();

	#region Class Functions
	#endregion

	public void ScreenZoomX(ch) {
		return Game_Map.TILE_WIDTH / 32.0;
	}

	public void ScreenZoomY(ch) {
		return Game_Map.TILE_HEIGHT / 32.0;
	}

	public void ScreenX(ch) {
		return ch.screen_x;
	}

	public void ScreenY(ch) {
		return ch.screen_y;
	}

	public void bmHeight(bm) {
		h = @heightcache[bm];
		if (!h) {
			bmap = new AnimatedBitmap("Graphics/Characters/" + bm, 0);
			h = bmap.height;
			@heightcache[bm] = h;
			bmap.dispose;
		}
		return h;
	}

	public void ScreenZ(ch, height = null) {
		if (height.null()) {
			height = 0;
			if (ch.tile_id > 0) {
				height = 32;
			} else if (ch.character_name != "") {
				height = bmHeight(ch.character_name) / 4;
			}
		}
		ret = ch.screen_z(height);
		return ret;
	}
}
