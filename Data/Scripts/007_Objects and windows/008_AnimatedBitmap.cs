//===============================================================================
//
//===============================================================================
public partial class AnimatedBitmap {
	public void initialize(file, hue = 0) {
		if (file.null()) raise "Filename is null (missing graphic).";
		path     = file;
		filename = "";
		if (file.last != "/") {   // Isn't just a directory
			split_file = file.Split(@"\\\"));
			filename = split_file.pop;
			path = split_file.join("/") + "/";
		}
		if (System.Text.RegularExpressions.Regex.IsMatch(filename,@"^\[\d+(?:,\d+)?\]")) {   // Starts with 1 or 2 numbers in square brackets
			@bitmap = new PngAnimatedBitmap(path, filename, hue);
		} else {
			@bitmap = new GifBitmap(path, filename, hue);
		}
	}

	public int this[int index] { get { return     @bitmap[index];                     } }
	public int width        { get { return @bitmap.bitmap.width;               } }
	public int height       { get { return @bitmap.bitmap.height;              } }
	public int length       { get { return @bitmap.length;                     } }
	public int each         { get { return @bitmap.each(item => yield item);   } }
	public int bitmap       { get { return @bitmap.bitmap;                     } }
	public int currentIndex { get { return @bitmap.currentIndex;               } }
	public int totalFrames  { get { return @bitmap.totalFrames;                } }
	public bool disposed() {    @bitmap.disposed();                  }
	public int update       { get { return @bitmap.update;                     } }
	public int dispose      { get { return @bitmap.dispose;                    } }
	public int deanimate    { get { return @bitmap.deanimate;                  } }
	public int copy         { get { return @bitmap.copy;                       } }
}

//===============================================================================
//
//===============================================================================
public partial class PngAnimatedBitmap {
	public int frames		{ get { return _frames; } set { _frames = value; } }			protected int _frames;

	// Creates an animated bitmap from a PNG file.
	public void initialize(dir, filename, hue = 0) {
		@frames       = new List<string>();
		@currentFrame = 0;
		@timer_start  = System.uptime;
		panorama = RPG.Cache.load_bitmap(dir, filename, hue);
		if (System.Text.RegularExpressions.Regex.IsMatch(filename,@"^\[(\d+)(?:,(\d+))?\]")) {   // Starts with 1 or 2 numbers in brackets
			// File has a frame count
			numFrames = Game.GameData.1.ToInt();
			duration  = Game.GameData.2.ToInt();   // In 1/20ths of a second
			if (duration == 0) duration  = 5;
			if (numFrames <= 0) raise $"Invalid frame count in {filename}";
			if (duration <= 0) raise $"Invalid frame duration in {filename}";
			if (panorama.width % numFrames != 0) {
				Debug.LogError($"Bitmap's width ({panorama.width}) is not divisible by frame count: {filename}");
				//throw new ArgumentException($"Bitmap's width ({panorama.width}) is not divisible by frame count: {filename}");
			}
			@frame_duration = duration / 20.0;
			subWidth = panorama.width / numFrames;
			for (int i = numFrames; i < numFrames; i++) { //for 'numFrames' times do => |i|
				subBitmap = new Bitmap(subWidth, panorama.height);
				subBitmap.blt(0, 0, panorama, new Rect(subWidth * i, 0, subWidth, panorama.height));
				@frames.Add(subBitmap);
			}
			panorama.dispose;
		} else {
			@frames = [panorama];
		}
	}

	public void dispose() {
		if (@disposed) return;
		@frames.each(f => f.dispose);
		@disposed = true;
	}

	public bool disposed() {
		return @disposed;
	}

	public int this[int index] { get {
		return @frames[index];
		}
	}

	public int width  { get { return self.bitmap.width;  } }
	public int height { get { return self.bitmap.height; } }

	public void bitmap() {
		return @frames[@currentFrame];
	}

	public void currentIndex() {
		return @currentFrame;
	}

	public void length() {
		return @frames.length;
	}

	// Actually returns the total number of 1/20ths of a second this animation lasts.
	public void totalFrames() {
		return (@frame_duration * @frames.length * 20).ToInt();
	}

	public void each() {
		@frames.each(item => yield item);
	}

	public void deanimate() {
		for (int i = 1; i < @frames.length; i++) { //each '@frames.length' do => |i|
			@frames[i].dispose;
		}
		@frames = [@frames[0]];
		@currentFrame = 0;
		@frame_duration = 0;
		return @frames[0];
	}

	public void copy() {
		x = self.clone;
		x.frames = x.frames.clone;
		x.frames.each_with_index((frame, i) => x.frames[i] = frame.copy);
		return x;
	}

	public void update() {
		if (disposed()) return;
		if (@frames.length > 1) {
			@currentFrame = ((System.uptime - @timer_start) / @frame_duration).ToInt() % @frames.length;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class GifBitmap {
	public int bitmap		{ get { return _bitmap; } set { _bitmap = value; } }			protected int _bitmap;

	// Creates a bitmap from a GIF file. Can also load non-animated bitmaps.
	public void initialize(dir, filename, hue = 0) {
		@bitmap   = null;
		@disposed = false;
		if (!filename) filename  = "";
		begin;
			@bitmap = RPG.Cache.load_bitmap(dir, filename, hue);
		rescue;
			@bitmap = null;
		}
		if (@bitmap.null()) @bitmap = new Bitmap(32, 32);
		if (@bitmap&.animated()) @bitmap.play;
	}

	public int this[int _index] { get {
		return @bitmap;
		}
	}

	public void deanimate() {
		if (@bitmap&.animated()) @bitmap&.goto_and_stop(0);
		return @bitmap;
	}

	public void currentIndex() {
		return @bitmap&.current_frame || 0;
	}

	public void length() {
		return @bitmap&.frame_count || 1;
	}

	public void each() {
		yield @bitmap;
	}

	public void totalFrames() {
		f_rate = @bitmap.frame_rate;
		if (f_rate.null() || f_rate == 0) f_rate = 1;
		return (@bitmap) ? (int)Math.Floor(@bitmap.frame_count / f_rate) : 1;
	}

	public bool disposed() {
		return @disposed;
	}

	public void width() {
		return @bitmap&.width || 0;
	}

	public void height() {
		return @bitmap&.height || 0;
	}

	// Gifs are animated automatically by mkxp-z. This function does nothing.
	public void update() { }

	public void dispose() {
		if (@disposed) return;
		@bitmap.dispose;
		@disposed = true;
	}

	public void copy() {
		x = self.clone;
		if (@bitmap) x.bitmap = @bitmap.copy;
		return x;
	}
}

//===============================================================================
//
//===============================================================================
public void GetTileBitmap(filename, tile_id, hue, width = 1, height = 1) {
	return RPG.Cache.tileEx(filename, tile_id, hue, width, height) do |f|
		new AnimatedBitmap("Graphics/Tilesets/" + filename).deanimate;
	}
}

public void GetTileset(name, hue = 0) {
	return new AnimatedBitmap("Graphics/Tilesets/" + name, hue).deanimate;
}

public void GetAutotile(name, hue = 0) {
	return new AnimatedBitmap("Graphics/Autotiles/" + name, hue).deanimate;
}

public void GetAnimation(name, hue = 0) {
	return new AnimatedBitmap("Graphics/Animations/" + name, hue).deanimate;
}
