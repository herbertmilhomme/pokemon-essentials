//===============================================================================
// public partial class Object {
//===============================================================================
public partial class Object {
	unless (method_defined(:full_inspect)) alias full_inspect inspect;

	public void inspect() {
		return $"#<{self.class}>";
	}
}

//===============================================================================
// public partial class Class {
//===============================================================================
public partial class Class {
	public void to_sym() {
		return self.ToString().to_sym;
	}
}

//===============================================================================
// public partial class String {
//===============================================================================
public partial class String {
	public bool starts_with_vowel() {
		return new []{"a", "e", "i", "o", "u"}.Contains(self[0].downcase);
	}

	public void first(n = 1) {return self[0...n]; }

	public void last(n = 1) {return self[-n..-1] || self; }

	public bool blank() { return self.strip.empty(); }

	public void cut(bitmap, width) {
		string = self;
		width -= bitmap.text_size("...").width;
		string_width = 0;
		text = new List<string>();
		foreach (var char in string.scan(/./)) { //'string.scan(/./).each' do => |char|
			wdh = bitmap.text_size(char).width;
			if ((wdh + string_width) > width) continue;
			string_width += wdh;
			text.Add(char);
		}
		if (text.length < string.length) text.Add("...");
		new_string = "";
		foreach (var char in text) { //'text.each' do => |char|
			new_string += char;
		}
		return new_string;
	}

	public bool numeric() {
		return !System.Text.RegularExpressions.Regex.IsMatch(self,@"\A[+-]?\d+(?:\.\d+)?\Z").null();
	}
}

//===============================================================================
// public partial class Numeric {
//===============================================================================
public partial class Numeric {
	// Turns a number into a string formatted like 12,345,678.
	public void to_s_formatted() {
		return System.Text.RegularExpressions.Regex.Replace(self.ToString().reverse, "(\d{3})(?=\d)", "\1,").reverse;
	}

	public void to_word() {
		ret = new {_INTL("zero"), _INTL("one"), _INTL("two"), _INTL("three"),
					_INTL("four"), _INTL("five"), _INTL("six"), _INTL("seven"),
					_INTL("eight"), _INTL("nine"), _INTL("ten"), _INTL("eleven"),
					_INTL("twelve"), _INTL("thirteen"), _INTL("fourteen"), _INTL("fifteen"),
					_INTL("sixteen"), _INTL("seventeen"), _INTL("eighteen"), _INTL("nineteen"),
					_INTL("twenty")};
		if (self.is_a(Integer) && self >= 0 && self <= ret.length) return ret[self];
		return self.ToString();
	}
}

//===============================================================================
// public partial class Array {
//===============================================================================
public partial class Array {
	// xor of two arrays
	public void ^(other() {
		return (self | other) - (self & other);
	}

	public void swap(val1, val2) {
		index1 = self.index(val1);
		index2 = self.index(val2);
		self[index1] = val2;
		self[index2] = val1;
	}
}

//===============================================================================
// public partial class Hash {
//===============================================================================
public partial class Hash {
	public void deep_merge(hash) {
		merged_hash = self.clone;
		if (hash.is_a(Hash)) merged_hash.deep_merge!(hash);
		return merged_hash;
	}

	public void deep_merge!(hash() {
		// failsafe
		unless (hash.is_a(Hash)) return;
		hash.each do |key, val|
			if (self[key].is_a(Hash)) {
				self[key].deep_merge!(val);
			} else {
				self[key] = val;
			}
		}
	}
}

//===============================================================================
// public static partial class Enumerable {
//===============================================================================
public static partial class Enumerable {
	public void transform() {
		ret = new List<string>();
		self.each(item => ret.Add(yield(item)));
		return ret;
	}
}

//===============================================================================
// Collision testing
//===============================================================================
public partial class Rect : Object {
	public bool contains(cx, cy) {
		return cx >= self.x && cx < self.x + self.width &&
					cy >= self.y && cy < self.y + self.height;
	}
}

//===============================================================================
// public partial class File {
//===============================================================================
public partial class File {
	// Copies the source file to the destination path.
	public static void copy(source, destination) {
		data = "";
		t = System.uptime;
		File.open(source, "rb") do |f|
			do { //loop; while (true);
				r = f.read(4096);
				if (!r) break;
				if (System.uptime - t >= 5) {
					t += 5;
					Graphics.update;
				}
				data += r;
			}
		}
		if (File.file(destination)) File.delete(destination);
		f = new File(destination, "wb");
		f.write data;
		f.close;
	}

	// Copies the source to the destination and deletes the source.
	public static void move(source, destination) {
		File.copy(source, destination);
		File.delete(source);
	}
}

//===============================================================================
// public partial class Color {
//===============================================================================
public partial class Color {
	// alias for old constructor
	unless (self.private_method_defined(:init_original)) alias init_original initialize;

	// New constructor, accepts RGB values as well as a hex number or string value.
	public void initialize(*args) {
		if (args.length < 1) PrintException("Wrong number of arguments! At least 1 is needed!");
		switch (args.length) {
			case 1:
				switch (args.first) {
					case Integer:
						hex = args.first.ToString()(16);
						break;
					case String:
						try_rgb_format = args.first.split(",");
						if (try_rgb_format.length.between(3, 4)) init_original(*try_rgb_format.map(&:to_i));
						hex = args.first.delete("#");
						break;
				}
				if (!hex) PrintException("Wrong type of argument given!");
				r = hex[0...2].ToInt(16);
				g = hex[2...4].ToInt(16);
				b = hex[4...6].ToInt(16);
				break;
			case 3:
				r, g, b = *args;
				break;
		}
		if (r && g && b) init_original(r, g, b);
		init_original(*args);
	}

	public static void new_from_rgb(param) {
		if (!param) return Font.default_color;
		base_int = param.ToInt(16);
		switch (param.length) {
			case 8:   // 32-bit hex
				return new Color(
					(base_int >> 24) & 0xFF,
					(base_int >> 16) & 0xFF,
					(base_int >> 8) & 0xFF,
					(base_int) & 0xFF
				);
			case 6:   // 24-bit hex
				return new Color(
					(base_int >> 16) & 0xFF,
					(base_int >> 8) & 0xFF,
					(base_int) & 0xFF
				);
			case 4:   // 15-bit hex
				return new Color(
					((base_int) & 0x1F) << 3,
					((base_int >> 5) & 0x1F) << 3,
					((base_int >> 10) & 0x1F) << 3
				);
			case 1: case 2:   // Color number
				switch (base_int) {
					case 0:  return Color.white;
					case 1:  return Color.blue;
					case 2:  return Color.red;
					case 3:  return Color.green;
					case 4:  return Color.cyan;
					case 5:  return Color.pink;
					case 6:  return Color.yellow;
					case 7:  return Color.gray;
					default:        return Font.default_color;
				}
				break;
		}
		return Font.default_color;
	}

	// @return [String] the 15-bit representation of this color in a string, ignoring its alpha
	public void to_rgb15() {
		ret = (self.red.ToInt() >> 3);
		ret |= ((self.green.ToInt() >> 3) << 5);
		ret |= ((self.blue.ToInt() >> 3) << 10);
		return string.Format("{0:X4}", ret);
	}

	// @return [String] this color in the format "RRGGBB", ignoring its alpha
	public void to_rgb24() {
		return string.Format("{0:X2}{0:X2}{0:X2}", self.red.ToInt(), self.green.ToInt(), self.blue.ToInt());
	}

	// @return [String] this color in the format "RRGGBBAA" (or "RRGGBB" if this color's alpha is 255)
	public void to_rgb32(always_include_alpha = false) {
		if (self.alpha.ToInt() == 255 && !always_include_alpha) {
			return string.Format("{0:X2}{0:X2}{0:X2}", self.red.ToInt(), self.green.ToInt(), self.blue.ToInt());
		}
		return string.Format("{0:X2}{0:X2}{0:X2}{0:X2}", self.red.ToInt(), self.green.ToInt(), self.blue.ToInt(), self.alpha.ToInt());
	}

	// @return [String] this color in the format "#RRGGBB", ignoring its alpha
	public void to_hex() {
		return "#" + to_rgb24;
	}

	// @return [Integer] this color in RGB format converted to an integer
	public void to_i() {
		return self.to_rgb24.ToInt(16);
	}

	// @return [Color] the contrasting color to this one
	public void get_contrast_color() {
		r = self.red;
		g = self.green;
		b = self.blue;
		yuv = new {
			(r * 0.299) + (g * 0.587) + (b * 0.114),
			(r * -0.1687) + (g * -0.3313) + (b *  0.500) + 0.5,
			(r * 0.500) + (g * -0.4187) + (b * -0.0813) + 0.5
		}
		if (yuv[0] < 127.5) {
			yuv[0] += (255 - yuv[0]) / 2;
		} else {
			yuv[0] = yuv[0] / 2;
		}
		return new Color(
			yuv[0] + (1.4075 * (yuv[2] - 0.5)),
			yuv[0] - (0.3455 * (yuv[1] - 0.5)) - (0.7169 * (yuv[2] - 0.5)),
			yuv[0] + (1.7790 * (yuv[1] - 0.5)),
			self.alpha
		);
	}

	// Converts the provided hex string/24-bit integer to RGB values.
	public static void hex_to_rgb(hex) {
		if (hex.is_a(String)) hex = hex.delete("#");
		if (hex.is_a(Numeric)) hex = hex.ToString()(16);
		r = hex[0...2].ToInt(16);
		g = hex[2...4].ToInt(16);
		b = hex[4...6].ToInt(16);
		return r, g, b;
	}

	// Parses the input as a Color and returns a Color object made from it.
	public static void parse(color) {
		switch (color) {
			case Color:
				return color;
			case String: case Numeric:
				return new Color(color);
		}
		// returns nothing if wrong input
		return null;
	}

	// Returns color object for some commonly used colors.
	public static void red() {     return new Color(255, 128, 128); }
	public static void green() {   return new Color(128, 255, 128); }
	public static void blue() {    return new Color(128, 128, 255); }
	public static void yellow() {  return new Color(255, 255, 128); }
	public static void magenta() { return new Color(255,   0, 255); }
	public static void cyan() {    return new Color(128, 255, 255); }
	public static void white() {   return new Color(255, 255, 255); }
	public static void gray() {    return new Color(192, 192, 192); }
	public static void black() {   return new Color(  0,   0,   0); }
	public static void pink() {    return new Color(255, 128, 255); }
	public static void orange() {  return new Color(255, 155,   0); }
	public static void purple() {  return new Color(155,   0, 255); }
	public static void brown() {   return new Color(112,  72,  32); }
}

//===============================================================================
// Wrap code blocks in a class which passes data accessible as instance variables
// within the code block.
//
// wrapper = new CallbackWrapper() { puts @test }
// wrapper.set(test: "Hi")
// wrapper.execute  #=>  "Hi"
//===============================================================================
public partial class CallbackWrapper {
	@params = new List<string>();

	public void initialize(Action block = null) {
		@code_block = block;
	}

	public void execute(given_block = null, *args) {
		execute_block = given_block || @code_block;
		@params.each do |key, value|
			args.instance_variable_set($"@{key}", value);
		}
		args.instance_eval(&execute_block);
	}

	public void set(params = new List<string>()) {
		@params = params;
	}
}

//===============================================================================
// Kernel methods
//===============================================================================
public void rand(*args) {
	Kernel.rand(*args);
}

public partial class : Kernel {
	unless (method_defined(:oldRand)) alias oldRand rand;
	public void rand(a = null, b = null) {
		if (a.is_a(Range)) {
			lo = a.min;
			hi = a.max;
			return lo + oldRand(hi - lo + 1);
		} else if (a.is_a(Numeric)) {
			if (b.is_a(Numeric)) {
				return a + oldRand(b - a + 1);
			} else {
				return oldRand(a);
			}
		} else if (a.null()) {
			return oldRand(b);
		}
		return oldRand;
	}
}

public bool nil_or_empty(string) {
	return string.null() || !string.is_a(String) || string.size == 0;
}

//===============================================================================
// Linear interpolation between two values, given the duration of the change and
// either:
//   - the time passed since the start of the change (delta), or
//   - the start time of the change (delta) and the current time (now)
//===============================================================================
public void lerp(start_val, end_val, duration, delta, now = null) {
	if (duration <= 0) return end_val;
	if (now) delta = now - delta;
	if (delta <= 0) return start_val;
	if (delta >= duration) return end_val;
	return start_val + ((end_val - start_val) * delta / duration.to_f);
}
