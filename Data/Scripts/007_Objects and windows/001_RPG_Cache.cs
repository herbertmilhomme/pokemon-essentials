//===============================================================================
//
//===============================================================================
public partial class Hangup : Exception { };

//===============================================================================
//
//===============================================================================
public static partial class RPG {
	public static partial class Cache
		public static void debug() {
			t = Time.now;
			filename = t.strftime("%H %M %S.%L.txt");
			File.open("cache_" + filename, "wb") do |f|
				@cache.each do |key, value|
					if (!value) {
						f.write($"{key} (null)\r\n");
					} else if (value.disposed()) {
						f.write($"{key} (disposed)\r\n");
					} else {
						f.write($"{key} ({value.refcount}, {value.width}x{value.height})\r\n");
					}
				}
			}
		}

		public static void setKey(key, obj) {
			@cache[key] = obj;
		}

		public static void fromCache(i) {
			if (!@cache.Contains(i)) return null;
			obj = @cache[i];
			if (obj&.disposed()) return null;
			return obj;
		}

		public static void load_bitmap(folder_name, filename, hue = 0) {
			path = folder_name + filename;
			cached = true;
			ret = fromCache(path);
			if (!ret) {
				if (filename == "") {
					ret = new BitmapWrapper(32, 32);
				} else {
					ret = new BitmapWrapper(path);
				}
				@cache[path] = ret;
				cached = false;
			}
			if (hue == 0) {
				if (cached) ret.addRef;
				return ret;
			}
			key = new {path, hue};
			ret2 = fromCache(key);
			if (ret2) {
				ret2.addRef;
			} else {
				ret2 = ret.copy;
				ret2.hue_change(hue);
				@cache[key] = ret2;
			}
			return ret2;
		}

		public static void tileEx(filename, tile_id, hue, width = 1, height = 1) {
			key = new {filename, tile_id, hue, width, height};
			ret = fromCache(key);
			if (ret) {
				ret.addRef;
			} else {
				ret = new BitmapWrapper(32 * width, 32 * height);
				x = ((tile_id - 384) % 8) * 32;
				y = (((tile_id - 384) / 8) - height + 1) * 32;
				tileset = yield(filename);
				ret.blt(0, 0, tileset, new Rect(x, y, 32 * width, 32 * height));
				tileset.dispose;
				if (hue != 0) ret.hue_change(hue);
				@cache[key] = ret;
			}
			return ret;
		}

		public static void tile(filename, tile_id, hue) {
			return self.tileEx(filename, tile_id, hue, f => { self.tileset(f); });
		}

		public static void transition(filename) {
			self.load_bitmap("Graphics/Transitions/", filename);
		}

		public static void ui(filename) {
			self.load_bitmap("Graphics/UI/", filename);
		}

		public static void retain(folder_name, filename = "", hue = 0) {
			path = folder_name + filename;
			ret = fromCache(path);
			if (hue > 0) {
				key = new {path, hue};
				ret2 = fromCache(key);
				if (ret2) {
					ret2.never_dispose = true;
					return;
				}
			}
			if (ret) ret.never_dispose = true;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class BitmapWrapper : Bitmap {
	public int refcount		{ get { return _refcount; } }			protected int _refcount;
	public int never_dispose		{ get { return _never_dispose; } set { _never_dispose = value; } }			protected int _never_dispose;

	public override void dispose() {
		if (self.disposed()) return;
		@refcount -= 1;
		if (@refcount <= 0 && !never_dispose) base.dispose();
	}

	public override void initialize(*arg) {
		base.initialize();
		@refcount = 1;
	}

	public void resetRef() {
		@refcount = 1;
	}

	public void copy() {
		bm = self.clone;
		bm.resetRef;
		return bm;
	}

	public void addRef() {
		@refcount += 1;
	}
}
