//===============================================================================
//
//===============================================================================
public partial class AnimFrame {
	public const int X          = 0;
	public const int Y          = 1;
	public const int ZOOMX      = 2;
	public const int ANGLE      = 3;
	public const int MIRROR     = 4;
	public const int BLENDTYPE  = 5;
	public const int VISIBLE    = 6;
	public const int PATTERN    = 7;
	public const int OPACITY    = 8;
	public const int ZOOMY      = 11;
	public const int COLORRED   = 12;
	public const int COLORGREEN = 13;
	public const int COLORBLUE  = 14;
	public const int COLORALPHA = 15;
	public const int TONERED    = 16;
	public const int TONEGREEN  = 17;
	public const int TONEBLUE   = 18;
	public const int TONEGRAY   = 19;
	public const int LOCKED     = 20;
	public const int FLASHRED   = 21;
	public const int FLASHGREEN = 22;
	public const int FLASHBLUE  = 23;
	public const int FLASHALPHA = 24;
	public const int PRIORITY   = 25;
	public const int FOCUS      = 26;
}

//===============================================================================
//
//===============================================================================
public void yaxisIntersect(x1, y1, x2, y2, px, py) {
	dx = x2 - x1;
	dy = y2 - y1;
	x = (dx == 0) ? 0.0 : (px - x1).to_f / dx;
	y = (dy == 0) ? 0.0 : (py - y1).to_f / dy;
	return new {x, y};
}

public void repositionY(x1, y1, x2, y2, tx, ty) {
	dx = x2 - x1;
	dy = y2 - y1;
	x = x1 + (tx * dx.to_f);
	y = y1 + (ty * dy.to_f);
	return new {x, y};
}

public void transformPoint(x1, y1, x2, y2,  // Source line
									x3, y3, x4, y4,  // Destination line
									px, py)        {// Source point
	ret = yaxisIntersect(x1, y1, x2, y2, px, py);
	ret2 = repositionY(x3, y3, x4, y4, ret[0], ret[1]);
	return ret2;
}

public void getSpriteCenter(sprite) {
	if (!sprite || sprite.disposed()) return new {0, 0};
	if (!sprite.bitmap || sprite.bitmap.disposed()) return new {sprite.x, sprite.y};
	centerX = sprite.src_rect.width / 2;
	centerY = sprite.src_rect.height / 2;
	offsetX = (centerX - sprite.ox) * sprite.zoom_x;
	offsetY = (centerY - sprite.oy) * sprite.zoom_y;
	return new {sprite.x + offsetX, sprite.y + offsetY};
}

public void isReversed(src0, src1, dst0, dst1) {
	if (src0 == src1) return false;
	if (src0 < src1) return (dst0 > dst1);
	return (dst0 < dst1);
}

public void CreateCel(x, y, pattern, focus = 4) {
	frame = new List<string>();
	frame[AnimFrame.X]       = x;
	frame[AnimFrame.Y]       = y;
	frame[AnimFrame.PATTERN] = pattern;
	frame[AnimFrame.FOCUS]   = focus;   // 1=target, 2=user, 3=user and target, 4=screen
	frame[AnimFrame.LOCKED]  = 0;
	ResetCel(frame);
	return frame;
}

public void ResetCel(frame) {
	if (!frame) return;
	frame[AnimFrame.ZOOMX]      = 100;
	frame[AnimFrame.ZOOMY]      = 100;
	frame[AnimFrame.BLENDTYPE]  = 0;
	frame[AnimFrame.VISIBLE]    = 1;
	frame[AnimFrame.ANGLE]      = 0;
	frame[AnimFrame.MIRROR]     = 0;
	frame[AnimFrame.OPACITY]    = 255;
	frame[AnimFrame.COLORRED]   = 0;
	frame[AnimFrame.COLORGREEN] = 0;
	frame[AnimFrame.COLORBLUE]  = 0;
	frame[AnimFrame.COLORALPHA] = 0;
	frame[AnimFrame.TONERED]    = 0;
	frame[AnimFrame.TONEGREEN]  = 0;
	frame[AnimFrame.TONEBLUE]   = 0;
	frame[AnimFrame.TONEGRAY]   = 0;
	frame[AnimFrame.FLASHRED]   = 0;
	frame[AnimFrame.FLASHGREEN] = 0;
	frame[AnimFrame.FLASHBLUE]  = 0;
	frame[AnimFrame.FLASHALPHA] = 0;
	frame[AnimFrame.PRIORITY]   = 1;   // 0=back, 1=front, 2=behind focus, 3=before focus
}

//===============================================================================
//
//===============================================================================
public void ConvertRPGAnimation(animation) {
	Anim = new Animation();
	Anim.id       = animation.id;
	Anim.name     = animation.name.clone;
	Anim.graphic  = animation.animation_name;
	Anim.hue      = animation.animation_hue;
	Anim.array.clear;
	yOffset = 0;
	Anim.position = animation.position;
	if (animation.position == 0) yOffset = -64;
	if (animation.position == 2) yOffset = 64;
	for (int i = animation.frames.length; i < animation.frames.length; i++) { //for 'animation.frames.length' times do => |i|
		frame = Anim.addFrame;
		animFrame = animation.frames[i];
		for (int j = animFrame.cell_max; j < animFrame.cell_max; j++) { //for 'animFrame.cell_max' times do => |j|
			data = animFrame.cell_data;
			if (data[j, 0] == -1) {
				frame.Add(null);
				continue;
			}
			if (animation.position == 3) {   // Screen
				point = transformPoint(
					-160, 80, 160, -80,
					Battle.Scene.FOCUSUSER_X, Battle.Scene.FOCUSUSER_Y,
					Battle.Scene.FOCUSTARGET_X, Battle.Scene.FOCUSTARGET_Y,
					data[j, 1], data[j, 2]
				);
				cel = CreateCel(point[0], point[1], data[j, 0]);
			} else {
				cel = CreateCel(data[j, 1], data[j, 2] + yOffset, data[j, 0]);
			}
			cel[AnimFrame.ZOOMX]     = data[j, 3];
			cel[AnimFrame.ZOOMY]     = data[j, 3];
			cel[AnimFrame.ANGLE]     = data[j, 4];
			cel[AnimFrame.MIRROR]    = data[j, 5];
			cel[AnimFrame.OPACITY]   = data[j, 6];
			cel[AnimFrame.BLENDTYPE] = 0;
			frame.Add(cel);
		}
	}
	foreach (var timing in animation.timings) { //'animation.timings.each' do => |timing|
		newTiming = new AnimTiming();
		newTiming.frame         = timing.frame;
		newTiming.name          = timing.se.name;
		newTiming.volume        = timing.se.volume;
		newTiming.pitch         = timing.se.pitch;
		newTiming.flashScope    = timing.flash_scope;
		newTiming.flashColor    = timing.flash_color.clone;
		newTiming.flashDuration = timing.flash_duration;
		Anim.timing.Add(newTiming);
	}
	return Anim;
}

//===============================================================================
//
//===============================================================================
public partial class RPG.Animation {
	public static void fromOther(otherAnim, id) {
		ret = new RPG.Animation();
		ret.id             = id;
		ret.name           = otherAnim.name.clone;
		ret.animation_name = otherAnim.animation_name.clone;
		ret.animation_hue  = otherAnim.animation_hue;
		ret.position       = otherAnim.position;
		return ret;
	}

	public void addSound(frame, se) {
		timing = new RPG.Animation.Timing();
		timing.frame = frame;
		timing.se    = new RPG.AudioFile(se, 100);
		self.timings.Add(timing);
	}

	// frame is zero-based.
	public void addAnimation(otherAnim, frame, x, y) {
		if (frame + otherAnim.frames.length >= self.frames.length) {
			totalframes = frame + otherAnim.frames.length + 1;
			(totalframes - self.frames.length).times do
				self.frames.Add(new RPG.Animation.Frame());
			}
		}
		self.frame_max = self.frames.length;
		for (int i = otherAnim.frame_max; i < otherAnim.frame_max; i++) { //for 'otherAnim.frame_max' times do => |i|
			thisframe = self.frames[frame + i];
			otherframe = otherAnim.frames[i];
			cellStart = thisframe.cell_max;
			thisframe.cell_max += otherframe.cell_max;
			thisframe.cell_data.resize(thisframe.cell_max, 8);
			for (int j = otherframe.cell_max; j < otherframe.cell_max; j++) { //for 'otherframe.cell_max' times do => |j|
				thisframe.cell_data[cellStart + j, 0] = otherframe.cell_data[j, 0];
				thisframe.cell_data[cellStart + j, 1] = otherframe.cell_data[j, 1] + x;
				thisframe.cell_data[cellStart + j, 2] = otherframe.cell_data[j, 2] + y;
				thisframe.cell_data[cellStart + j, 3] = otherframe.cell_data[j, 3];
				thisframe.cell_data[cellStart + j, 4] = otherframe.cell_data[j, 4];
				thisframe.cell_data[cellStart + j, 5] = otherframe.cell_data[j, 5];
				thisframe.cell_data[cellStart + j, 6] = otherframe.cell_data[j, 6];
				thisframe.cell_data[cellStart + j, 7] = otherframe.cell_data[j, 7];
			}
		}
		foreach (var othertiming in otherAnim.timings) { //'otherAnim.timings.each' do => |othertiming|
			timing = new RPG.Animation.Timing();
			timing.frame          = frame + othertiming.frame;
			timing.se             = new RPG.AudioFile(othertiming.se.name.clone,
																								othertiming.se.volume,
																								othertiming.se.pitch);
			timing.flash_scope    = othertiming.flash_scope;
			timing.flash_color    = othertiming.flash_color.clone;
			timing.flash_duration = othertiming.flash_duration;
			timing.condition      = othertiming.condition;
			self.timings.Add(timing);
		}
		self.timings.sort! { |a, b| a.frame <=> b.frame };
	}
}

//===============================================================================
//
//===============================================================================
public partial class AnimTiming {
	public int frame		{ get { return _frame; } set { _frame = value; } }			protected int _frame;
	/// <summary>0=play SE, 1=set bg, 2=bg mod</summary>
	public int timingType		{ get { return _timingType; } }			protected int _timingType;
	/// <summary>Name of SE file or BG file</summary>
	public int name		{ get { return _name; } set { _name = value; } }			protected int _name;
	public int volume		{ get { return _volume; } set { _volume = value; } }			protected int _volume;
	public int pitch		{ get { return _pitch; } set { _pitch = value; } }			protected int _pitch;
	/// <summary>x coordinate of bg (or to move bg to)</summary>
	public int bgX		{ get { return _bgX; } set { _bgX = value; } }			protected int _bgX;
	/// <summary>y coordinate of bg (or to move bg to)</summary>
	public int bgY		{ get { return _bgY; } set { _bgY = value; } }			protected int _bgY;
	/// <summary>Opacity of bg (or to change bg to)</summary>
	public int opacity		{ get { return _opacity; } set { _opacity = value; } }			protected int _opacity;
	/// <summary>Color of bg (or to change bg to)</summary>
	public int colorRed		{ get { return _colorRed; } set { _colorRed = value; } }			protected int _colorRed;
	/// <summary>Color of bg (or to change bg to)</summary>
	public int colorGreen		{ get { return _colorGreen; } set { _colorGreen = value; } }			protected int _colorGreen;
	/// <summary>Color of bg (or to change bg to)</summary>
	public int colorBlue		{ get { return _colorBlue; } set { _colorBlue = value; } }			protected int _colorBlue;
	/// <summary>Color of bg (or to change bg to)</summary>
	public int colorAlpha		{ get { return _colorAlpha; } set { _colorAlpha = value; } }			protected int _colorAlpha;
	/// <summary>How long to spend changing to the new bg coords/color</summary>
	public int duration		{ get { return _duration; } }			protected int _duration;
	public int flashScope		{ get { return _flashScope; } set { _flashScope = value; } }			protected int _flashScope;
	public int flashColor		{ get { return _flashColor; } set { _flashColor = value; } }			protected int _flashColor;
	public int flashDuration		{ get { return _flashDuration; } set { _flashDuration = value; } }			protected int _flashDuration;

	public void initialize(type = 0) {
		@frame         = 0;
		@timingType    = type;
		@name          = "";
		@volume        = 80;
		@pitch         = 100;
		@bgX           = null;
		@bgY           = null;
		@opacity       = null;
		@colorRed      = null;
		@colorGreen    = null;
		@colorBlue     = null;
		@colorAlpha    = null;
		@duration      = 5;
		@flashScope    = 0;
		@flashColor    = Color.white;
		@flashDuration = 5;
	}

	public void timingType() {
		return @timingType || 0;
	}

	public void duration() {
		return @duration || 5;
	}

	public void to_s() {
		switch (self.timingType) {
			case 0:
				return $"[{@frame + 1}] Play SE: {name} (volume {@volume}, pitch {@pitch})";
			case 1:
				text = string.Format("[{0}] Set BG: \"{0}\"", @frame + 1, name);
				text += string.Format(" (color={0},{0},{0},{0})",
												@colorRed || "-",
												@colorGreen || "-",
												@colorBlue || "-",
												@colorAlpha || "-");
				text += string.Format(" (opacity={0})", @opacity);
				text += string.Format(" (coords={0},{0})", @bgX || "-", @bgY || "-");
				return text;
			case 2:
				text = string.Format("[{0}] Change BG: @{0}", @frame + 1, duration);
				if (@colorRed || @colorGreen || @colorBlue || @colorAlpha) {
					text += string.Format(" (color={0},{0},{0},{0})",
													@colorRed || "-",
													@colorGreen || "-",
													@colorBlue || "-",
													@colorAlpha || "-");
				}
				if (@opacity) text += string.Format(" (opacity={0})", @opacity);
				if (@bgX || @bgY) text += string.Format(" (coords={0},{0})", @bgX || "-", @bgY || "-");
				return text;
			case 3:
				text = string.Format("[{0}] Set FG: \"{0}\"", @frame + 1, name);
				text += string.Format(" (color={0},{0},{0},{0})",
												@colorRed || "-",
												@colorGreen || "-",
												@colorBlue || "-",
												@colorAlpha || "-");
				text += string.Format(" (opacity={0})", @opacity);
				text += string.Format(" (coords={0},{0})", @bgX || "-", @bgY || "-");
				return text;
			case 4:
				text = string.Format("[{0}] Change FG: @{0}", @frame + 1, duration);
				if (@colorRed || @colorGreen || @colorBlue || @colorAlpha) {
					text += string.Format(" (color={0},{0},{0},{0})",
													@colorRed || "-",
													@colorGreen || "-",
													@colorBlue || "-",
													@colorAlpha || "-");
				}
				if (@opacity) text += string.Format(" (opacity={0})", @opacity);
				if (@bgX || @bgY) text += string.Format(" (coords={0},{0})", @bgX || "-", @bgY || "-");
				return text;
		}
		return "";
	}
}

//===============================================================================
//
//===============================================================================
public partial class Animations : Array {
	include Enumerable;
	public int array		{ get { return _array; } }			protected int _array;
	public int selected		{ get { return _selected; } set { _selected = value; } }			protected int _selected;

	public void initialize(size = 1) {
		@array = new List<string>();
		@selected = 0;
		if (size < 1) size = 1;   // Always create at least one animation
		size.times do;
			@array.Add(new Animation());
		}
	}

	public void length() {
		return @array.length;
	}

	public void each() {
		@array.each(i => yield i);
	}

	public int this[int i] { get {
		return @array[i];
		}
	}

	public int this[(i, value)] { get {
		@array[i] = value;
		}
	}

	public void get_from_name(name) {
		@array.each(i => { if (i&.name == name) return i; });
		return null;
	}

	public void compact() {
		@array.compact!;
	}

	public void insert(index, val) {
		@array.insert(index, val);
	}

	public void delete_at(index) {
		@array.delete_at(index);
	}

	public void resize(len) {
		idxStart = @array.length;
		idxEnd   = len;
		if (idxStart > idxEnd) {
			(idxStart - idxEnd).times(() => @array.pop)
		} else {
			(idxEnd - idxStart).times(() => @array.Add(new Animation()))
		}
		if (self.selected >= len) self.selected = len;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Animation : Array {
	include Enumerable;
	public int id		{ get { return _id; } set { _id = value; } }			protected int _id;
	public int name		{ get { return _name; } set { _name = value; } }			protected int _name;
	public int graphic		{ get { return _graphic; } set { _graphic = value; } }			protected int _graphic;
	public int hue		{ get { return _hue; } set { _hue = value; } }			protected int _hue;
	public int position		{ get { return _position; } set { _position = value; } }			protected int _position;
	public int speed		{ get { return _speed; } }			protected int _speed;
	public int array		{ get { return _array; } }			protected int _array;
	public int timing		{ get { return _timing; } }			protected int _timing;

	public const int MAX_SPRITES = 60;

	public void speed() {
		return @speed || 20;
	}

	public void initialize(size = 1) {
		@id       = -1;
		@name     = "";
		@graphic  = "";
		@hue      = 0;
		@position = 4;             // 1=target, 2=user, 3=user and target, 4=screen
		@array    = new List<string>();
		if (size < 1) size      = 1;   // Always create at least one frame
		size.times(() => addFrame);
		@timing   = new List<string>();
		@scope    = 0;
	}

	public void length() {
		return @array.length;
	}

	public void each() {
		@array.each(i => yield i);
	}

	public int this[int i] { get {
		return @array[i];
		}
	}

	public int this[(i, value)] { get {
		@array[i] = value;
		}
	}

	public void insert(*arg) {
		return @array.insert(*arg);
	}

	public void delete_at(*arg) {
		return @array.delete_at(*arg);
	}

	public void resize(len) {
		if (len < @array.length) {
			@array[len, @array.length - len] = new List<string>();
		} else if (len > @array.length) {
			(len - @array.length).times do
				addFrame;
			}
		}
	}

	public void addFrame() {
		pos = @array.length;
		@array[pos] = new List<string>();
		// Move's user
		@array[pos][0] = CreateCel(Battle.Scene.FOCUSUSER_X, Battle.Scene.FOCUSUSER_Y, -1);
		@array[pos][0][AnimFrame.FOCUS]  = 2;
		@array[pos][0][AnimFrame.LOCKED] = 1;
		// Move's target
		@array[pos][1] = CreateCel(Battle.Scene.FOCUSTARGET_X, Battle.Scene.FOCUSTARGET_Y, -2);
		@array[pos][1][AnimFrame.FOCUS]  = 1;
		@array[pos][1][AnimFrame.LOCKED] = 1;
		return @array[pos];
	}

	public void playTiming(frame, bgGraphic, bgColor, foGraphic, foColor, oldbg = new List<string>(), oldfo = new List<string>(), user = null) {
		@timing.each do |i|
			if (!i.duration || i.duration <= 0) continue;
			if (i.frame + i.duration < frame || i.frame >= frame) continue;
			fraction = (frame - i.frame).to_f / i.duration;
			switch (i.timingType) {
				case 2:
					if (bgGraphic.bitmap.null()) {
						if (i.opacity) bgColor.opacity = oldbg[2] + ((i.opacity - oldbg[2]) * fraction);
						cr = (i.colorRed) ? oldbg[3].red + ((i.colorRed - oldbg[3].red) * fraction) : oldbg[3].red;
						cg = (i.colorGreen) ? oldbg[3].green + ((i.colorGreen - oldbg[3].green) * fraction) : oldbg[3].green;
						cb = (i.colorBlue) ? oldbg[3].blue + ((i.colorBlue - oldbg[3].blue) * fraction) : oldbg[3].blue;
						ca = (i.colorAlpha) ? oldbg[3].alpha + ((i.colorAlpha - oldbg[3].alpha) * fraction) : oldbg[3].alpha;
						bgColor.color = new Color(cr, cg, cb, ca);
					} else {
						if (i.bgX) bgGraphic.ox      = oldbg[0] - ((i.bgX - oldbg[0]) * fraction);
						if (i.bgY) bgGraphic.oy      = oldbg[1] - ((i.bgY - oldbg[1]) * fraction);
						if (i.opacity) bgGraphic.opacity = oldbg[2] + ((i.opacity - oldbg[2]) * fraction);
						cr = (i.colorRed) ? oldbg[3].red + ((i.colorRed - oldbg[3].red) * fraction) : oldbg[3].red;
						cg = (i.colorGreen) ? oldbg[3].green + ((i.colorGreen - oldbg[3].green) * fraction) : oldbg[3].green;
						cb = (i.colorBlue) ? oldbg[3].blue + ((i.colorBlue - oldbg[3].blue) * fraction) : oldbg[3].blue;
						ca = (i.colorAlpha) ? oldbg[3].alpha + ((i.colorAlpha - oldbg[3].alpha) * fraction) : oldbg[3].alpha;
						bgGraphic.color = new Color(cr, cg, cb, ca);
					}
					break;
				case 4:
					if (foGraphic.bitmap.null()) {
						if (i.opacity) foColor.opacity = oldfo[2] + ((i.opacity - oldfo[2]) * fraction);
						cr = (i.colorRed) ? oldfo[3].red + ((i.colorRed - oldfo[3].red) * fraction) : oldfo[3].red;
						cg = (i.colorGreen) ? oldfo[3].green + ((i.colorGreen - oldfo[3].green) * fraction) : oldfo[3].green;
						cb = (i.colorBlue) ? oldfo[3].blue + ((i.colorBlue - oldfo[3].blue) * fraction) : oldfo[3].blue;
						ca = (i.colorAlpha) ? oldfo[3].alpha + ((i.colorAlpha - oldfo[3].alpha) * fraction) : oldfo[3].alpha;
						foColor.color = new Color(cr, cg, cb, ca);
					} else {
						if (i.bgX) foGraphic.ox      = oldfo[0] - ((i.bgX - oldfo[0]) * fraction);
						if (i.bgY) foGraphic.oy      = oldfo[1] - ((i.bgY - oldfo[1]) * fraction);
						if (i.opacity) foGraphic.opacity = oldfo[2] + ((i.opacity - oldfo[2]) * fraction);
						cr = (i.colorRed) ? oldfo[3].red + ((i.colorRed - oldfo[3].red) * fraction) : oldfo[3].red;
						cg = (i.colorGreen) ? oldfo[3].green + ((i.colorGreen - oldfo[3].green) * fraction) : oldfo[3].green;
						cb = (i.colorBlue) ? oldfo[3].blue + ((i.colorBlue - oldfo[3].blue) * fraction) : oldfo[3].blue;
						ca = (i.colorAlpha) ? oldfo[3].alpha + ((i.colorAlpha - oldfo[3].alpha) * fraction) : oldfo[3].alpha;
						foGraphic.color = new Color(cr, cg, cb, ca);
					}
					break;
			}
		}
		@timing.each do |i|
			if (i.frame != frame) continue;
			switch (i.timingType) {
				case 0:   // Play SE
					if (i.name && i.name != "") {
						SEPlay("Anim/" + i.name, i.volume, i.pitch);
					} else if (user&.pokemon) {
						name = GameData.Species.cry_filename_from_pokemon(user.pokemon);
						if (name) SEPlay(name, i.volume, i.pitch);
					}
//					if (sprite) {
//						sprite.flash(i.flashColor, i.flashDuration * 2) if i.flashScope == 1
//						sprite.flash(null, i.flashDuration * 2) if i.flashScope == 3
//					}
					break;
				case 1:   // Set background graphic (immediate)
					if (i.name && i.name != "") {
						bgGraphic.setBitmap("Graphics/Animations/" + i.name);
						bgGraphic.ox      = -i.bgX || 0;
						bgGraphic.oy      = -i.bgY || 0;
						bgGraphic.color   = new Color(i.colorRed || 0, i.colorGreen || 0, i.colorBlue || 0, i.colorAlpha || 0);
						bgGraphic.opacity = i.opacity || 0;
						bgColor.opacity = 0;
					} else {
						bgGraphic.setBitmap(null);
						bgGraphic.opacity = 0;
						bgColor.color   = new Color(i.colorRed || 0, i.colorGreen || 0, i.colorBlue || 0, i.colorAlpha || 0);
						bgColor.opacity = i.opacity || 0;
					}
					break;
				case 2:   // Move/recolour background graphic
					if (bgGraphic.bitmap.null()) {
						oldbg[0] = 0;
						oldbg[1] = 0;
						oldbg[2] = bgColor.opacity || 0;
						oldbg[3] = bgColor.color.clone || new Color(0, 0, 0, 0);
					} else {
						oldbg[0] = bgGraphic.ox || 0;
						oldbg[1] = bgGraphic.oy || 0;
						oldbg[2] = bgGraphic.opacity || 0;
						oldbg[3] = bgGraphic.color.clone || new Color(0, 0, 0, 0);
					}
					break;
				case 3:   // Set foreground graphic (immediate)
					if (i.name && i.name != "") {
						foGraphic.setBitmap("Graphics/Animations/" + i.name);
						foGraphic.ox      = -i.bgX || 0;
						foGraphic.oy      = -i.bgY || 0;
						foGraphic.color   = new Color(i.colorRed || 0, i.colorGreen || 0, i.colorBlue || 0, i.colorAlpha || 0);
						foGraphic.opacity = i.opacity || 0;
						foColor.opacity = 0;
					} else {
						foGraphic.setBitmap(null);
						foGraphic.opacity = 0;
						foColor.color   = new Color(i.colorRed || 0, i.colorGreen || 0, i.colorBlue || 0, i.colorAlpha || 0);
						foColor.opacity = i.opacity || 0;
					}
					break;
				case 4:   // Move/recolour foreground graphic
					if (foGraphic.bitmap.null()) {
						oldfo[0] = 0;
						oldfo[1] = 0;
						oldfo[2] = foColor.opacity || 0;
						oldfo[3] = foColor.color.clone || new Color(0, 0, 0, 0);
					} else {
						oldfo[0] = foGraphic.ox || 0;
						oldfo[1] = foGraphic.oy || 0;
						oldfo[2] = foGraphic.opacity || 0;
						oldfo[3] = foGraphic.color.clone || new Color(0, 0, 0, 0);
					}
					break;
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public void SpriteSetAnimFrame(sprite, frame, user = null, target = null, inEditor = false) {
	if (!sprite) return;
	if (!frame) {
		sprite.visible  = false;
		sprite.src_rect = new Rect(0, 0, 1, 1);
		return;
	}
	sprite.blend_type = frame[AnimFrame.BLENDTYPE];
	sprite.angle      = frame[AnimFrame.ANGLE];
	sprite.mirror     = (frame[AnimFrame.MIRROR] > 0);
	sprite.opacity    = frame[AnimFrame.OPACITY];
	sprite.visible    = true;
	if (!frame[AnimFrame.VISIBLE] == 1 && inEditor) {
		sprite.opacity /= 2;
	} else {
		sprite.visible = (frame[AnimFrame.VISIBLE] == 1);
	}
	pattern = frame[AnimFrame.PATTERN];
	if (pattern >= 0) {
		animwidth = 192;
		sprite.src_rect.set((pattern % 5) * animwidth, (pattern / 5) * animwidth,
												animwidth, animwidth);
	} else {
		sprite.src_rect.set(0, 0,
												(sprite.bitmap) ? sprite.bitmap.width : 128,
												(sprite.bitmap) ? sprite.bitmap.height : 128)
	}
	sprite.zoom_x = frame[AnimFrame.ZOOMX] / 100.0;
	sprite.zoom_y = frame[AnimFrame.ZOOMY] / 100.0;
	sprite.color.set(
		frame[AnimFrame.COLORRED],
		frame[AnimFrame.COLORGREEN],
		frame[AnimFrame.COLORBLUE],
		frame[AnimFrame.COLORALPHA]
	);
	sprite.tone.set(
		frame[AnimFrame.TONERED],
		frame[AnimFrame.TONEGREEN],
		frame[AnimFrame.TONEBLUE],
		frame[AnimFrame.TONEGRAY]
	);
	sprite.ox = sprite.src_rect.width / 2;
	sprite.oy = sprite.src_rect.height / 2;
	sprite.x  = frame[AnimFrame.X];
	sprite.y  = frame[AnimFrame.Y];
	if (sprite != user && sprite != target) {
		switch (frame[AnimFrame.PRIORITY]) {
			case 0:   // Behind everything
				sprite.z = 10;
				break;
			case 1:   // In front of everything
				sprite.z = 80;
				break;
			case 2:   // Just behind focus
				switch (frame[AnimFrame.FOCUS]) {
					case 1:   // Focused on target
						sprite.z = (target) ? target.z - 1 : 20;
						break;
					case 2:   // Focused on user
						sprite.z = (user) ? user.z - 1 : 20;
						break;
					default:     // Focused on user and target, or screen
						sprite.z = 20;
						break;
				}
				break;
			case 3:   // Just in front of focus
				switch (frame[AnimFrame.FOCUS]) {
					case 1:   // Focused on target
						sprite.z = (target) ? target.z + 1 : 80;
						break;
					case 2:   // Focused on user
						sprite.z = (user) ? user.z + 1 : 80;
						break;
					default:     // Focused on user and target, or screen
						sprite.z = 80;
						break;
				}
			default:
				sprite.z = 80;
				break;
		}
	}
}

//===============================================================================
// Animation player.
//===============================================================================
public partial class AnimationPlayerX {
	public int looping		{ get { return _looping; } set { _looping = value; } }			protected int _looping;

	public const int MAX_SPRITES = 60;

	public void initialize(animation, user, target, scene = null, oppMove = false, inEditor = false) {
		@animation     = animation;
		@user          = (oppMove) ? target : user;   // Just used for playing user's cry
		@usersprite    = (user) ? scene.sprites[$"pokemon_{user.index}"] : null;
		@targetsprite  = (target) ? scene.sprites[$"pokemon_{target.index}"] : null;
		@userbitmap    = @usersprite&.bitmap; // not to be disposed
		@targetbitmap  = @targetsprite&.bitmap; // not to be disposed
		@scene         = scene;
		@viewport      = scene&.viewport;
		@inEditor      = inEditor;
		@looping       = false;
		@animbitmap    = null;   // Animation sheet graphic
		@old_frame     = -1;
		@frame         = -1;
		@timer_start   = null;
		@srcLine       = null;
		@dstLine       = null;
		@userOrig      = getSpriteCenter(@usersprite);
		@targetOrig    = getSpriteCenter(@targetsprite);
		@oldbg         = new List<string>();
		@oldfo         = new List<string>();
		initializeSprites;
	}

	public void initializeSprites() {
		// Create animation sprites (0=user's sprite, 1=target's sprite)
		@animsprites = new List<string>();
		@animsprites[0] = @usersprite;
		@animsprites[1] = @targetsprite;
		for (int i = 2; i < MAX_SPRITES; i++) { //each 'MAX_SPRITES' do => |i|
			@animsprites[i] = new Sprite(@viewport);
			@animsprites[i].bitmap  = null;
			@animsprites[i].visible = false;
		}
		// Create background colour sprite
		@bgColor = new ColoredPlane(Color.black, @viewport);
		@bgColor.z       = 5;
		@bgColor.opacity = 0;
		@bgColor.refresh;
		// Create background graphic sprite
		@bgGraphic = new AnimatedPlane(@viewport);
		@bgGraphic.setBitmap(null);
		@bgGraphic.z       = 5;
		@bgGraphic.opacity = 0;
		@bgGraphic.refresh;
		// Create foreground colour sprite
		@foColor = new ColoredPlane(Color.black, @viewport);
		@foColor.z       = 85;
		@foColor.opacity = 0;
		@foColor.refresh;
		// Create foreground graphic sprite
		@foGraphic = new AnimatedPlane(@viewport);
		@foGraphic.setBitmap(null);
		@foGraphic.z       = 85;
		@foGraphic.opacity = 0;
		@foGraphic.refresh;
	}

	public void dispose() {
		@animbitmap&.dispose;
		(2...MAX_SPRITES).each(i => @animsprites[i]&.dispose)
		@bgGraphic.dispose;
		@bgColor.dispose;
		@foGraphic.dispose;
		@foColor.dispose;
	}

	// Makes the original user and target sprites be uninvolved with the animation.
	// The animation shows just its particles.
	public void discard_user_and_target_sprites() {
		@animsprites[0] = null;
		@animsprites[1] = null;
	}

	public void set_target_origin(x, y) {
		@targetOrig = new {x, y};
	}

	public void start() {
		@frame = 0;
		@timer_start = System.uptime;
	}

	public bool animDone() {
		return @frame < 0;
	}

	public void setLineTransform(x1, y1, x2, y2, x3, y3, x4, y4) {
		@srcLine = new {x1, y1, x2, y2};
		@dstLine = new {x3, y3, x4, y4};
	}

	public void update() {
		if (@frame < 0) return;
		@frame = ((System.uptime - @timer_start) * 20).ToInt();
		// Loop or end the animation if the animation has reached the end
		if (@frame >= @animation.length) {
			if (@looping) {
				@frame %= @animation.length;
				@timer_start += @animation.length / 20.0;
			} else {
				@frame = -1;
				@animbitmap&.dispose;
				@animbitmap = null;
				return;
			}
		}
		// Load the animation's spritesheet and assign it to all the sprites
		if (!@animbitmap || @animbitmap.disposed()) {
			@animbitmap = new AnimatedBitmap("Graphics/Animations/" + @animation.graphic,
																			@animation.hue).deanimate;
			for (int i = MAX_SPRITES; i < MAX_SPRITES; i++) { //for 'MAX_SPRITES' times do => |i|
				if (@animsprites[i]) @animsprites[i].bitmap = @animbitmap;
			}
		}
		// Update background and foreground graphics
		@bgGraphic.update;
		@bgColor.update;
		@foGraphic.update;
		@foColor.update;
		// Update all the sprites to depict the animation's next frame
		if (@frame == @old_frame) return;
		@old_frame = @frame;
		thisframe = @animation[@frame];
		// Make all cel sprites invisible
		if (@animsprites[i] })) MAX_SPRITES.times(i => { @animsprites[i].visible = false;
		// Set each cel sprite acoordingly
		for (int i = thisframe.length; i < thisframe.length; i++) { //for 'thisframe.length' times do => |i|
			cel = thisframe[i];
			if (!cel) continue;
			sprite = @animsprites[i];
			if (!sprite) continue;
			// Set cel sprite's graphic
			switch (cel[AnimFrame.PATTERN]) {
				case -1:
					sprite.bitmap = @userbitmap;
					break;
				case -2:
					sprite.bitmap = @targetbitmap;
					break;
				default:
					sprite.bitmap = @animbitmap;
					break;
			}
			// Apply settings to the cel sprite
			SpriteSetAnimFrame(sprite, cel, @usersprite, @targetsprite);
			switch (cel[AnimFrame.FOCUS]) {
				case 1:   // Focused on target
					sprite.x = cel[AnimFrame.X] + @targetOrig[0] - Battle.Scene.FOCUSTARGET_X;
					sprite.y = cel[AnimFrame.Y] + @targetOrig[1] - Battle.Scene.FOCUSTARGET_Y;
					break;
				case 2:   // Focused on user
					sprite.x = cel[AnimFrame.X] + @userOrig[0] - Battle.Scene.FOCUSUSER_X;
					sprite.y = cel[AnimFrame.Y] + @userOrig[1] - Battle.Scene.FOCUSUSER_Y;
					break;
				case 3:   // Focused on user and target
					if (!@srcLine || !@dstLine) continue;
					point = transformPoint(@srcLine[0], @srcLine[1], @srcLine[2], @srcLine[3],
																@dstLine[0], @dstLine[1], @dstLine[2], @dstLine[3],
																sprite.x, sprite.y);
					sprite.x = point[0];
					sprite.y = point[1];
					if (isReversed(@srcLine[0], @srcLine[2], @dstLine[0], @dstLine[2]) &&
						cel[AnimFrame.PATTERN] >= 0) {
						// Reverse direction
						sprite.mirror = !sprite.mirror;
					}
					break;
			}
			if (@inEditor) sprite.x += 64;
			if (@inEditor) sprite.y += 64;
		}
		// Play timings
		@animation.playTiming(@frame, @bgGraphic, @bgColor, @foGraphic, @foColor, @oldbg, @oldfo, @user);
	}
}
