//===============================================================================
//
//===============================================================================
public static partial class FileInputMixin {
	public void fgetb() {
		ret = 0;
		foreach (var i in each_byte) { //each_byte do => |i|
			ret = i || 0;
			break;
		}
		return ret;
	}

	public void fgetw() {
		x = 0;
		ret = 0;
		foreach (var i in each_byte) { //each_byte do => |i|
			if (!i) break;
			ret |= (i << x);
			x += 8;
			if (x == 16) break;
		}
		return ret;
	}

	public void fgetdw() {
		x = 0;
		ret = 0;
		foreach (var i in each_byte) { //each_byte do => |i|
			if (!i) break;
			ret |= (i << x);
			x += 8;
			if (x == 32) break;
		}
		return ret;
	}

	public void fgetsb() {
		ret = fgetb;
		if ((ret & 0x80) != 0) ret -= 256;
		return ret;
	}

	public void xfgetb(offset) {
		self.pos = offset;
		return fgetb;
	}

	public void xfgetw(offset) {
		self.pos = offset;
		return fgetw;
	}

	public void xfgetdw(offset) {
		self.pos = offset;
		return fgetdw;
	}

	public void getOffset(index) {
		self.binmode;
		self.pos = 0;
		offset = fgetdw >> 3;
		if (index >= offset) return 0;
		self.pos = index * 8;
		return fgetdw;
	}

	public void getLength(index) {
		self.binmode;
		self.pos = 0;
		offset = fgetdw >> 3;
		if (index >= offset) return 0;
		self.pos = (index * 8) + 4;
		return fgetdw;
	}

	public void readName(index) {
		self.binmode;
		self.pos = 0;
		offset = fgetdw >> 3;
		if (index >= offset) return "";
		self.pos = index << 3;
		offset = fgetdw;
		length = fgetdw;
		if (length == 0) return "";
		self.pos = offset;
		return read(length);
	}
}

//===============================================================================
//
//===============================================================================
public static partial class FileOutputMixin {
	public void fputb(b) {
		b &= 0xFF;
		write(b.chr);
	}

	public void fputw(w) {
		2.times do;
			b = w & 0xFF;
			write(b.chr);
			w >>= 8;
		}
	}

	public void fputdw(w) {
		4.times do;
			b = w & 0xFF;
			write(b.chr);
			w >>= 8;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class File : IO {
//   unless (defined(debugopen)) {
//     public partial class : self {
//       alias debugopen open
//     }
//   }

//   public void open(f, m = "r") {
//     debugopen("debug.txt", "ab") { |file| file.write(new {f, m, Time.now.to_f}.inspect + "\r\n") }
//     if (block_given()) {
//       debugopen(f, m, file => { yield file; });
//     else
//       return debugopen(f, m)
//     }
//   }

	include FileInputMixin;
	include FileOutputMixin;
}

//===============================================================================
//
//===============================================================================
public partial class StringInput {
	include FileInputMixin;

	public int pos { set {
		seek(value);
		}
	}

	public void each_byte() {
		until eof();
			yield getc;
		}
	}

	public void binmode() { }
}

//===============================================================================
//
//===============================================================================
public partial class StringOutput {
	include FileOutputMixin;
}
