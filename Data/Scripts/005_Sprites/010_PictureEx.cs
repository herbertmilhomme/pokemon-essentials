//===============================================================================
//
//===============================================================================
public partial class PictureOrigin {
	public const int TOP_LEFT     = 0;
	public const int CENTER       = 1;
	public const int TOP_RIGHT    = 2;
	public const int BOTTOM_LEFT  = 3;
	public const int LOWER_LEFT   = 3;
	public const int BOTTOM_RIGHT = 4;
	public const int LOWER_RIGHT  = 4;
	public const int TOP          = 5;
	public const int BOTTOM       = 6;
	public const int LEFT         = 7;
	public const int RIGHT        = 8;
}

//===============================================================================
//
//===============================================================================
public partial class Processes {
	public const int XY          = 0;
	public const int DELTA_XY    = 1;
	public const int Z           = 2;
	public const int CURVE       = 3;
	public const int ZOOM        = 4;
	public const int ANGLE       = 5;
	public const int TONE        = 6;
	public const int COLOR       = 7;
	public const int HUE         = 8;
	public const int OPACITY     = 9;
	public const int VISIBLE     = 10;
	public const int BLEND_TYPE  = 11;
	public const int SE          = 12;
	public const int NAME        = 13;
	public const int ORIGIN      = 14;
	public const int SRC         = 15;
	public const int SRC_SIZE    = 16;
	public const int CROP_BOTTOM = 17;
}

//===============================================================================
//
//===============================================================================
public void getCubicPoint2(src, t) {
	x0  = src[0];
	y0  = src[1];
	cx0 = src[2];
	cy0 = src[3];
	cx1 = src[4];
	cy1 = src[5];
	x1  = src[6];
	y1  = src[7];

	x1 = cx1 + ((x1 - cx1) * t);
	x0 += ((cx0 - x0) * t);
	cx0 += ((cx1 - cx0) * t);
	cx1 = cx0 + ((x1 - cx0) * t);
	cx0 = x0 + ((cx0 - x0) * t);
	cx = cx0 + ((cx1 - cx0) * t);
	// a = x1 - 3 * cx1 + 3 * cx0 - x0
	// b = 3 * (cx1 - 2 * cx0 + x0)
	// c = 3 * (cx0 - x0)
	// d = x0
	// cx = a*t*t*t + b*t*t + c*t + d
	y1 = cy1 + ((y1 - cy1) * t);
	y0 += ((cy0 - y0) * t);
	cy0 += ((cy1 - cy0) * t);
	cy1 = cy0 + ((y1 - cy0) * t);
	cy0 = y0 + ((cy0 - y0) * t);
	cy = cy0 + ((cy1 - cy0) * t);
	// a = y1 - 3 * cy1 + 3 * cy0 - y0
	// b = 3 * (cy1 - 2 * cy0 + y0)
	// c = 3 * (cy0 - y0)
	// d = y0
	// cy = a*t*t*t + b*t*t + c*t + d
	return new {cx, cy};
}

//===============================================================================
// PictureEx
//===============================================================================
public partial class PictureEx {
	/// <summary>x-coordinate</summary>
	public int x		{ get { return _x; } set { _x = value; } }			protected int _x;
	/// <summary>y-coordinate</summary>
	public int y		{ get { return _y; } set { _y = value; } }			protected int _y;
	/// <summary>z value</summary>
	public int z		{ get { return _z; } set { _z = value; } }			protected int _z;
	/// <summary>x directional zoom rate</summary>
	public int zoom_x		{ get { return _zoom_x; } set { _zoom_x = value; } }			protected int _zoom_x;
	/// <summary>y directional zoom rate</summary>
	public int zoom_y		{ get { return _zoom_y; } set { _zoom_y = value; } }			protected int _zoom_y;
	/// <summary>rotation angle</summary>
	public int angle		{ get { return _angle; } set { _angle = value; } }			protected int _angle;
	/// <summary>tone</summary>
	public int tone		{ get { return _tone; } set { _tone = value; } }			protected int _tone;
	/// <summary>color</summary>
	public int color		{ get { return _color; } set { _color = value; } }			protected int _color;
	/// <summary>filename hue</summary>
	public int hue		{ get { return _hue; } set { _hue = value; } }			protected int _hue;
	/// <summary>opacity level</summary>
	public int opacity		{ get { return _opacity; } set { _opacity = value; } }			protected int _opacity;
	/// <summary>visibility boolean</summary>
	public int visible		{ get { return _visible; } set { _visible = value; } }			protected int _visible;
	/// <summary>blend method</summary>
	public int blend_type		{ get { return _blend_type; } set { _blend_type = value; } }			protected int _blend_type;
	/// <summary>file name</summary>
	public int name		{ get { return _name; } set { _name = value; } }			protected int _name;
	/// <summary>starting point</summary>
	public int origin		{ get { return _origin; } set { _origin = value; } }			protected int _origin;
	/// <summary>source rect</summary>
	public int src_rect		{ get { return _src_rect; } }			protected int _src_rect;
	/// <summary>crops sprite to above this y-coordinate</summary>
	public int cropBottom		{ get { return _cropBottom; } }			protected int _cropBottom;
	/// <summary>Array of processes updated in a frame</summary>
	public int frameUpdates		{ get { return _frameUpdates; } }			protected int _frameUpdates;

	public void move_processes() {
		ret = new List<string>();
		@processes.each do |p|
			if (!new []{Processes.XY, Processes.DELTA_XY}.Contains(p[0])) continue;
			pro = new List<string>();
			pro.Add(p[0] == Processes.XY ? "XY" : "DELTA");
			if (p[1] == 0 && p[2] == 0) {
				pro.Add("start " + p[7].ToInt().ToString() + ", " + p[8].ToInt().ToString());
			} else {
				if (p[2] > 0) pro.Add("for " + p[2].ToString());
				if (p[0] == Processes.XY) {
					pro.Add("go to " + p[7].ToInt().ToString() + ", " + p[8].ToInt().ToString());
				} else {
					pro.Add("move by " + p[7].ToInt().ToString() + ", " + p[8].ToInt().ToString());
				}
			}
			ret.Add(pro);
		}
		return ret;
	}

	public void initialize(z) {
		// process: new {type, delay, total_duration, frame_counter, cb, etc.}
		@processes     = new List<string>();
		@x             = 0.0;
		@y             = 0.0;
		@z             = z;
		@zoom_x        = 100.0;
		@zoom_y        = 100.0;
		@angle         = 0;
		@rotate_speed  = 0;
		@auto_angle    = 0;   // Cumulative angle change caused by @rotate_speed
		@tone          = new Tone(0, 0, 0, 0);
		@tone_duration = 0;
		@color         = new Color(0, 0, 0, 0);
		@hue           = 0;
		@opacity       = 255.0;
		@visible       = true;
		@blend_type    = 0;
		@name          = "";
		@origin        = PictureOrigin.TOP_LEFT;
		@src_rect      = new Rect(0, 0, -1, -1);
		@cropBottom    = -1;
		@frameUpdates  = new List<string>();
	}

	public void callback(cb) {
		switch (cb) {
			case Proc:
				cb.call(self);
				break;
			case Array:
				cb[0].method(cb[1]).call(self, *cb[2]);
				break;
			case Method:
				cb.call(self);
				break;
		}
	}

	public void setCallback(delay, cb = null) {
		delay = ensureDelayAndDuration(delay);
		@processes.Add(new {null, delay, 0, false, cb});
	}

	public bool running() {
		return @processes.length > 0;
	}

	public void totalDuration() {
		ret = 0;
		@processes.each do |process|
			dur = process[1] + process[2];
			if (dur > ret) ret = dur;
		}
		return ret;
	}

	public void ensureDelayAndDuration(delay, duration = null) {
		if (delay < 0) delay = self.totalDuration;
		if (!duration.null()) return delay, duration;
		return delay;
	}

	public void ensureDelay(delay) {
		return ensureDelayAndDuration(delay);
	}

	// speed is the angle to change by in 1/20 of a second. @rotate_speed is the
	// angle to change by per frame.
	// NOTE: This is not compatible with manually changing the angle at a certain
	//       point. If you make a sprite auto-rotate, you should not try to alter
	//       the angle another way too.
	public void rotate(speed) {
		@rotate_speed = speed * 20.0;
	}

	public void erase() {
		self.name = "";
	}

	public void clearProcesses() {
		@processes = new List<string>();
		@timer_start = null;
	}

	public void adjustPosition(xOffset, yOffset) {
		@processes.each do |process|
			if (process[0] != Processes.XY) continue;
			process[5] += xOffset;
			process[6] += yOffset;
			process[7] += xOffset;
			process[8] += yOffset;
		}
	}

	public void move(delay, duration, origin, x, y, zoom_x = 100.0, zoom_y = 100.0, opacity = 255) {
		setOrigin(delay, duration, origin);
		moveXY(delay, duration, x, y);
		moveZoomXY(delay, duration, zoom_x, zoom_y);
		moveOpacity(delay, duration, opacity);
	}

	public void moveXY(delay, duration, x, y, cb = null) {
		delay, duration = ensureDelayAndDuration(delay, duration);
		@processes.Add(new {Processes.XY, delay, duration, false, cb, @x, @y, x, y});
	}

	public void setXY(delay, x, y, cb = null) {
		moveXY(delay, 0, x, y, cb);
	}

	public void moveCurve(delay, duration, x1, y1, x2, y2, x3, y3, cb = null) {
		delay, duration = ensureDelayAndDuration(delay, duration);
		@processes.Add(new {Processes.CURVE, delay, duration, false, cb, new {@x, @y, x1, y1, x2, y2, x3, y3}});
	}

	public void moveDelta(delay, duration, x, y, cb = null) {
		delay, duration = ensureDelayAndDuration(delay, duration);
		@processes.Add(new {Processes.DELTA_XY, delay, duration, false, cb, @x, @y, x, y});
	}

	public void setDelta(delay, x, y, cb = null) {
		moveDelta(delay, 0, x, y, cb);
	}

	public void moveZ(delay, duration, z, cb = null) {
		delay, duration = ensureDelayAndDuration(delay, duration);
		@processes.Add(new {Processes.Z, delay, duration, false, cb, @z, z});
	}

	public void setZ(delay, z, cb = null) {
		moveZ(delay, 0, z, cb);
	}

	public void moveZoomXY(delay, duration, zoom_x, zoom_y, cb = null) {
		delay, duration = ensureDelayAndDuration(delay, duration);
		@processes.Add(new {Processes.ZOOM, delay, duration, false, cb, @zoom_x, @zoom_y, zoom_x, zoom_y});
	}

	public void setZoomXY(delay, zoom_x, zoom_y, cb = null) {
		moveZoomXY(delay, 0, zoom_x, zoom_y, cb);
	}

	public void moveZoom(delay, duration, zoom, cb = null) {
		moveZoomXY(delay, duration, zoom, zoom, cb);
	}

	public void setZoom(delay, zoom, cb = null) {
		moveZoomXY(delay, 0, zoom, zoom, cb);
	}

	public void moveAngle(delay, duration, angle, cb = null) {
		delay, duration = ensureDelayAndDuration(delay, duration);
		@processes.Add(new {Processes.ANGLE, delay, duration, false, cb, @angle, angle});
	}

	public void setAngle(delay, angle, cb = null) {
		moveAngle(delay, 0, angle, cb);
	}

	public void moveTone(delay, duration, tone, cb = null) {
		delay, duration = ensureDelayAndDuration(delay, duration);
		target = (tone) ? tone.clone : new Tone(0, 0, 0, 0);
		@processes.Add(new {Processes.TONE, delay, duration, false, cb, @tone.clone, target});
	}

	public void setTone(delay, tone, cb = null) {
		moveTone(delay, 0, tone, cb);
	}

	public void moveColor(delay, duration, color, cb = null) {
		delay, duration = ensureDelayAndDuration(delay, duration);
		target = (color) ? color.clone : new Color(0, 0, 0, 0);
		@processes.Add(new {Processes.COLOR, delay, duration, false, cb, @color.clone, target});
	}

	public void setColor(delay, color, cb = null) {
		moveColor(delay, 0, color, cb);
	}

	// Hue changes don't actually work.
	public void moveHue(delay, duration, hue, cb = null) {
		delay, duration = ensureDelayAndDuration(delay, duration);
		@processes.Add(new {Processes.HUE, delay, duration, false, cb, @hue, hue});
	}

	// Hue changes don't actually work.
	public void setHue(delay, hue, cb = null) {
		moveHue(delay, 0, hue, cb);
	}

	public void moveOpacity(delay, duration, opacity, cb = null) {
		delay, duration = ensureDelayAndDuration(delay, duration);
		@processes.Add(new {Processes.OPACITY, delay, duration, false, cb, @opacity, opacity});
	}

	public void setOpacity(delay, opacity, cb = null) {
		moveOpacity(delay, 0, opacity, cb);
	}

	public void setVisible(delay, visible, cb = null) {
		delay = ensureDelay(delay);
		@processes.Add(new {Processes.VISIBLE, delay, 0, false, cb, visible});
	}

	// Only values of 0 (normal), 1 (additive) and 2 (subtractive) are allowed.
	public void setBlendType(delay, blend, cb = null) {
		delay = ensureDelayAndDuration(delay);
		@processes.Add(new {Processes.BLEND_TYPE, delay, 0, false, cb, blend});
	}

	public void setSE(delay, seFile, volume = null, pitch = null, cb = null) {
		delay = ensureDelay(delay);
		@processes.Add(new {Processes.SE, delay, 0, false, cb, seFile, volume, pitch});
	}

	public void setName(delay, name, cb = null) {
		delay = ensureDelay(delay);
		@processes.Add(new {Processes.NAME, delay, 0, false, cb, name});
	}

	public void setOrigin(delay, origin, cb = null) {
		delay = ensureDelay(delay);
		@processes.Add(new {Processes.ORIGIN, delay, 0, false, cb, origin});
	}

	public void setSrc(delay, srcX, srcY, cb = null) {
		delay = ensureDelay(delay);
		@processes.Add(new {Processes.SRC, delay, 0, false, cb, srcX, srcY});
	}

	public void setSrcSize(delay, srcWidth, srcHeight, cb = null) {
		delay = ensureDelay(delay);
		@processes.Add(new {Processes.SRC_SIZE, delay, 0, false, cb, srcWidth, srcHeight});
	}

	// Used to cut Pokémon sprites off when they faint and sink into the ground.
	public void setCropBottom(delay, y, cb = null) {
		delay = ensureDelay(delay);
		@processes.Add(new {Processes.CROP_BOTTOM, delay, 0, false, cb, y});
	}

	public void update() {
		time_now = System.uptime;
		if (!@timer_start) @timer_start = time_now;
		this_frame = ((time_now - @timer_start) * 20).ToInt();   // 20 frames per second
		procEnded = false;
		@frameUpdates.clear;
		@processes.each_with_index do |process, i|
			// Skip processes that aren't due to start yet
			if (process[1] > this_frame) continue;
			// Set initial values if the process has just started
			if (!process[3]) {   // Not started yet
				process[3] = true;   // Running
				switch (process[0]) {
					case Processes.XY:
						process[5] = @x;
						process[6] = @y;
						break;
					case Processes.DELTA_XY:
						process[5] = @x;
						process[6] = @y;
						process[7] += @x;
						process[8] += @y;
						break;
					case Processes.CURVE:
						process[5][0] = @x;
						process[5][1] = @y;
						break;
					case Processes.Z:
						process[5] = @z;
						break;
					case Processes.ZOOM:
						process[5] = @zoom_x;
						process[6] = @zoom_y;
						break;
					case Processes.ANGLE:
						process[5] = @angle;
						break;
					case Processes.TONE:
						process[5] = @tone.clone;
						break;
					case Processes.COLOR:
						process[5] = @color.clone;
						break;
					case Processes.HUE:
						process[5] = @hue;
						break;
					case Processes.OPACITY:
						process[5] = @opacity;
						break;
				}
			}
			// Update process
			if (!@frameUpdates.Contains(process[0])) @frameUpdates.Add(process[0]);
			start_time = @timer_start + (process[1] / 20.0);
			duration = process[2] / 20.0;
			switch (process[0]) {
				case Processes.XY: case Processes.DELTA_XY:
					@x = lerp(process[5], process[7], duration, start_time, time_now);
					@y = lerp(process[6], process[8], duration, start_time, time_now);
					break;
				case Processes.CURVE:
					@x, @y = getCubicPoint2(process[5], (time_now - start_time) / duration);
					break;
				case Processes.Z:
					@z = lerp(process[5], process[6], duration, start_time, time_now);
					break;
				case Processes.ZOOM:
					@zoom_x = lerp(process[5], process[7], duration, start_time, time_now);
					@zoom_y = lerp(process[6], process[8], duration, start_time, time_now);
					break;
				case Processes.ANGLE:
					@angle = lerp(process[5], process[6], duration, start_time, time_now);
					break;
				case Processes.TONE:
					@tone.red = lerp(process[5].red, process[6].red, duration, start_time, time_now);
					@tone.green = lerp(process[5].green, process[6].green, duration, start_time, time_now);
					@tone.blue = lerp(process[5].blue, process[6].blue, duration, start_time, time_now);
					@tone.gray = lerp(process[5].gray, process[6].gray, duration, start_time, time_now);
					break;
				case Processes.COLOR:
					@color.red = lerp(process[5].red, process[6].red, duration, start_time, time_now);
					@color.green = lerp(process[5].green, process[6].green, duration, start_time, time_now);
					@color.blue = lerp(process[5].blue, process[6].blue, duration, start_time, time_now);
					@color.alpha = lerp(process[5].alpha, process[6].alpha, duration, start_time, time_now);
					break;
				case Processes.HUE:
					@hue = lerp(process[5], process[6], duration, start_time, time_now);
					break;
				case Processes.OPACITY:
					@opacity = lerp(process[5], process[6], duration, start_time, time_now);
					break;
				case Processes.VISIBLE:
					@visible = process[5];
					break;
				case Processes.BLEND_TYPE:
					@blend_type = process[5];
					break;
				case Processes.SE:
					SEPlay(process[5], process[6], process[7]);
					break;
				case Processes.NAME:
					@name = process[5];
					break;
				case Processes.ORIGIN:
					@origin = process[5];
					break;
				case Processes.SRC:
					@src_rect.x = process[5];
					@src_rect.y = process[6];
					break;
				case Processes.SRC_SIZE:
					@src_rect.width  = process[5];
					@src_rect.height = process[6];
					break;
				case Processes.CROP_BOTTOM:
					@cropBottom = process[5];
					break;
			}
			// Erase process if its duration has elapsed
			if (process[1] + process[2] <= this_frame) {
				if (process[4]) callback(process[4]);
				@processes[i] = null;
				procEnded = true;
			}
		}
		// Clear out empty spaces in @processes array caused by finished processes
		if (procEnded) @processes.compact!;
		if (@processes.empty() && @rotate_speed == 0) @timer_start = null;
		// Add the constant rotation speed
		if (@rotate_speed != 0) {
			if (!@frameUpdates.Contains(Processes.ANGLE)) @frameUpdates.Add(Processes.ANGLE);
			@auto_angle = @rotate_speed * (time_now - @timer_start);
			while (@auto_angle < 0) {
				@auto_angle += 360;
			}
			@auto_angle %= 360;
			@angle += @rotate_speed;
			while (@angle < 0) {
				@angle += 360;
			}
			@angle %= 360;
		}
	}
}

//===============================================================================
//
//===============================================================================
public void setPictureSprite(sprite, picture, iconSprite = false) {
	if (picture.frameUpdates.length == 0) return;
	foreach (var type in picture.frameUpdates) { //'picture.frameUpdates.each' do => |type|
		switch (type) {
			case Processes.XY: case Processes.DELTA_XY:
				sprite.x = picture.x.round;
				sprite.y = picture.y.round;
				break;
			case Processes.Z:
				sprite.z = picture.z;
				break;
			case Processes.ZOOM:
				sprite.zoom_x = picture.zoom_x / 100.0;
				sprite.zoom_y = picture.zoom_y / 100.0;
				break;
			case Processes.ANGLE:
				sprite.angle = picture.angle;
				break;
			case Processes.TONE:
				sprite.tone = picture.tone;
				break;
			case Processes.COLOR:
				sprite.color = picture.color;
				break;
			case Processes.HUE:
				// This doesn't do anything.
				break;
			case Processes.BLEND_TYPE:
				sprite.blend_type = picture.blend_type;
				break;
			case Processes.OPACITY:
				sprite.opacity = picture.opacity;
				break;
			case Processes.VISIBLE:
				sprite.visible = picture.visible;
				break;
			case Processes.NAME:
				if (iconSprite && sprite.name != picture.name) sprite.name = picture.name;
				break;
			case Processes.ORIGIN:
				switch (picture.origin) {
					case PictureOrigin.TOP_LEFT: case PictureOrigin.LEFT: case PictureOrigin.BOTTOM_LEFT:
						sprite.ox = 0;
						break;
					case PictureOrigin.TOP: case PictureOrigin.CENTER: case PictureOrigin.BOTTOM:
						sprite.ox = (sprite.bitmap && !sprite.bitmap.disposed()) ? sprite.src_rect.width / 2 : 0;
						break;
					case PictureOrigin.TOP_RIGHT: case PictureOrigin.RIGHT: case PictureOrigin.BOTTOM_RIGHT:
						sprite.ox = (sprite.bitmap && !sprite.bitmap.disposed()) ? sprite.src_rect.width : 0;
						break;
				}
				switch (picture.origin) {
					case PictureOrigin.TOP_LEFT: case PictureOrigin.TOP: case PictureOrigin.TOP_RIGHT:
						sprite.oy = 0;
						break;
					case PictureOrigin.LEFT: case PictureOrigin.CENTER: case PictureOrigin.RIGHT:
						sprite.oy = (sprite.bitmap && !sprite.bitmap.disposed()) ? sprite.src_rect.height / 2 : 0;
						break;
					case PictureOrigin.BOTTOM_LEFT: case PictureOrigin.BOTTOM: case PictureOrigin.BOTTOM_RIGHT:
						sprite.oy = (sprite.bitmap && !sprite.bitmap.disposed()) ? sprite.src_rect.height : 0;
						break;
				}
				break;
			case Processes.SRC:
				unless (iconSprite && sprite.src_rect) continue;
				sprite.src_rect.x = picture.src_rect.x;
				sprite.src_rect.y = picture.src_rect.y;
				break;
			case Processes.SRC_SIZE:
				unless (iconSprite && sprite.src_rect) continue;
				sprite.src_rect.width  = picture.src_rect.width;
				sprite.src_rect.height = picture.src_rect.height;
				break;
		}
	}
	if (iconSprite && sprite.src_rect && picture.cropBottom >= 0) {
		spriteBottom = sprite.y - sprite.oy + sprite.src_rect.height;
		if (spriteBottom > picture.cropBottom) {
			sprite.src_rect.height = (int)Math.Max(picture.cropBottom - sprite.y + sprite.oy, 0);
		}
	}
}

public void setPictureIconSprite(sprite, picture) {
	setPictureSprite(sprite, picture, true);
}
