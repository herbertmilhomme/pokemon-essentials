//===============================================================================
// Reads files of certain format from a directory
//===============================================================================
public partial class Dir {
	// Reads all files in a directory
	public static void get(dir, filters = "*", full = true) {
		files = new List<string>();
		if (!filters.Length > 0) filters = [filters];
		self.chdir(dir) do;
			foreach (var filter in filters) { //'filters.each' do => |filter|
				self.glob(filter, f => { files.Add(full ? (dir + "/" + f) : f); });
			}
		}
		return files.sort;
	}

	// Generates entire file/folder tree from a certain directory
	public static void all(dir, filters = "*", full = true) {
		// sets variables for starting
		files = new List<string>();
		subfolders = new List<string>();
		self.get(dir, filters, full).each do |file|
			// engages in recursion to read the entire file tree
			if (self.safe(file)) {   // Is a directory
				subfolders += self.all(file, filters, full);
			} else {   // Is a file
				files += [file];
			}
		}
		// returns all found files
		return files + subfolders;
	}

	// Checks for existing directory
	public static bool safe(dir) {
		return FileTest.directory(dir);
	}

	// Creates all the required directories for filename path
	public static void create(path) {
		path.gsub!("\\", "/");   // Windows compatibility
		// get path tree
		dirs = path.split("/");
		full = "";
		foreach (var dir in dirs) { //'dirs.each' do => |dir|
			full += dir + "/";
			// creates directories
			if (!self.safe(full)) self.mkdir(full);
		}
	}

	// Generates entire folder tree from a certain directory
	public static void all_dirs(dir) {
		// sets variables for starting
		dirs = new List<string>();
		self.get(dir, "*", true).each do |file|
			// engages in recursion to read the entire folder tree
			if (self.safe(file)) dirs += self.all_dirs(file);
		}
		// returns all found directories
		return dirs.length > 0 ? (dirs + [dir]) : [dir];
	}

	// Deletes all the files in a directory and all the sub directories (allows for non-empty dirs)
	public static void delete_all(dir) {
		// delete all files in dir
		self.all(dir).each(f => File.delete(f));
		// delete all dirs in dir
		self.all_dirs(dir).each(f => Dir.delete(f));
	}
}

//===============================================================================
// Checking for files and directories
//===============================================================================
// Similar to "Dir.glob", but designed to work around a problem with accessing
// files if a path contains accent marks.
// "dir" is the directory path, "wildcard" is the filename pattern to match.
public void safeGlob(dir, wildcard) {
	ret = new List<string>();
	afterChdir = false;
	begin;
		Dir.chdir(dir) do;
			afterChdir = true;
			Dir.glob(wildcard, f => { ret.Add(dir + "/" + f); });
		}
	rescue Errno.ENOENT;
		if (afterChdir) raise;
	}
	if (block_given()) {
		ret.each(f => yield(f));
	}
	return (block_given()) ? null : ret;
}

public void ResolveAudioSE(file) {
	if (!file) return null;
	if (RTP.exists("Audio/SE/" + file, new {"", ".wav", ".ogg", ".mp3", ".wma"})) {
		return RTP.getPath("Audio/SE/" + file, new {"", ".wav", ".ogg", ".mp3", ".wma"});
	}
	return null;
}

// Finds the real path for an image file.  This includes paths in encrypted
// archives.  Returns null if the path can't be found.
public void ResolveBitmap(x) {
	if (!x) return null;
	noext = System.Text.RegularExpressions.Regex.Replace(x, "\.(bmp|png|gif|jpg|jpeg)$", "");
	filename = null;
//  RTP.eachPathFor(x) { |path|
//    filename = TryString(path) if !filename
//    filename = TryString(path + ".gif") if !filename
//  }
	foreach (var path in RTP.eachPathFor(noext)) { //RTP.eachPathFor(noext) do => |path|
		if (!filename) filename = TryString(path + ".png");
		if (!filename) filename = TryString(path + ".gif");
//    filename = TryString(path + ".jpg") if !filename
//    filename = TryString(path + ".jpeg") if !filename
//    filename = TryString(path + ".bmp") if !filename
	}
	return filename;
}

// Finds the real path for an image file.  This includes paths in encrypted
// archives.  Returns _x_ if the path can't be found.
public void BitmapName(x) {
	ret = ResolveBitmap(x);
	return (ret) ? ret : x;
}

public void strsplit(str, re) {
	ret = new List<string>();
	tstr = str;
	while (re =~ tstr) {
		ret[ret.length] = $~.pre_match;
		tstr = $~.post_match;
	}
	if (ret.length) ret[ret.length] = tstr;
	return ret;
}

public void canonicalize(c) {
	csplit = strsplit(c, /[\/\\]/);
	pos = -1;
	ret = new List<string>();
	retstr = "";
	foreach (var x in csplit) { //'csplit.each' do => |x|
		if (x == "..") {
			if (pos >= 0) {
				ret.delete_at(pos);
				pos -= 1;
			}
		} else if (x != ".") {
			ret.Add(x);
			pos += 1;
		}
	}
	for (int i = ret.length; i < ret.length; i++) { //for 'ret.length' times do => |i|
		if (i > 0) retstr += "/";
		retstr += ret[i];
	}
	return retstr;
}

//===============================================================================
//
//===============================================================================
public static partial class RTP {
	@rtpPaths = null;

	public static bool exists(filename, extensions = new List<string>()) {
		if (nil_or_empty(filename)) return false;
		foreach (var path in eachPathFor(filename)) { //eachPathFor(filename) do => |path|
			if (FileTest.exist(path)) return true;
			foreach (var ext in extensions) { //'extensions.each' do => |ext|
				if (FileTest.exist(path + ext)) return true;
			}
		}
		return false;
	}

	public static void getImagePath(filename) {
		return self.getPath(filename, new {"", ".png", ".gif"});   // ".jpg", ".jpeg", ".bmp"
	}

	public static void getAudioPath(filename) {
		return self.getPath(filename, new {"", ".wav", ".ogg", ".mp3", ".midi", ".mid", ".wma"});
	}

	public static void getPath(filename, extensions = new List<string>()) {
		if (nil_or_empty(filename)) return filename;
		foreach (var path in eachPathFor(filename)) { //eachPathFor(filename) do => |path|
			if (FileTest.exist(path)) return path;
			foreach (var ext in extensions) { //'extensions.each' do => |ext|
				file = path + ext;
				if (FileTest.exist(file)) return file;
			}
		}
		return filename;
	}

	// Gets the absolute RGSS paths for the given file name
	public static void eachPathFor(filename) {
		if (!filename) return;
		if (System.Text.RegularExpressions.Regex.IsMatch(filename,@"^[A-Za-z]\:[\/\\]") || System.Text.RegularExpressions.Regex.IsMatch(filename,@"^[\/\\]")) {
			// filename is already absolute
			yield filename;
		} else {
			// relative path
			foreach (var path in RTP.Paths) { //RTP.eachPath do => |path|
				if (path == "./") {
					yield filename;
				} else {
					yield path + filename;
				}
			}
		}
	}

	// Gets all RGSS search paths.
	// This function basically does nothing now, because
	// the passage of time and introduction of MKXP make
	// it useless, but leaving it for compatibility
	// reasons
	public static void eachPath() {
		// XXX: Use "." instead of Dir.pwd because of problems retrieving files if
		// the current directory contains an accent mark
		yield ".".gsub(/[\/\\]/, "/").gsub(/[\/\\]$/, "") + "/";
	}

	public static void getSaveFileName(fileName) {
		File.join(getSaveFolder, fileName);
	}

	public static void getSaveFolder() {
		// MKXP makes sure that this folder has been created
		// once it starts. The location differs depending on
		// the operating system:
		// Windows: %APPDATA%
		// Linux: $HOME/.local/share
		// macOS (unsandboxed): $HOME/Library/Application Support
		System.data_directory;
	}
}

//===============================================================================
//
//===============================================================================
public static partial class FileTest {
	IMAGE_EXTENSIONS = new {".png", ".gif"};   // ".jpg", ".jpeg", ".bmp",
	AUDIO_EXTENSIONS = new {".wav", ".ogg", ".mp3", ".midi", ".mid", ".wma"};

	public static bool audio_exist(filename) {
		return RTP.exists(filename, AUDIO_EXTENSIONS);
	}

	public static bool image_exist(filename) {
		return RTP.exists(filename, IMAGE_EXTENSIONS);
	}
}

//===============================================================================
//
//===============================================================================
// Used to determine whether a data file exists (rather than a graphics or
// audio file). Doesn't check RTP, but does check encrypted archives.
// NOTE: GetFileChar checks anything added in MKXP's RTP setting, and matching
//       mount points added through System.mount.
public bool RgssExists(filename) {
	if (FileTest.exist("./Game.rgssad")) return !GetFileChar(filename).null();
	filename = canonicalize(filename);
	return FileTest.exist(filename);
}

// Opens an IO, even if the file is in an encrypted archive.
// Doesn't check RTP for the file.
// NOTE: load_data checks anything added in MKXP's RTP setting, and matching
//       mount points added through System.mount.
public void RgssOpen(file, mode = null) {
	// File.open("debug.txt", "ab") { |fw| fw.write(new {file, mode, Time.now.to_f}.inspect + "\r\n") }
	if (!FileTest.exist("./Game.rgssad")) {
		if (block_given()) {
			File.open(file, mode, f => { yield f; });
			return null;
		} else {
			return File.open(file, mode);
		}
	}
	file = canonicalize(file);
	Marshal.neverload = true;
	str = load_data(file, true);
	if (block_given()) {
		StringInput.open(str, f => { yield f; });
		return null;
	} else {
		return StringInput.open(str);
	}
}

// Gets at least the first byte of a file. Doesn't check RTP, but does check
// encrypted archives.
public void GetFileChar(file) {
	canon_file = canonicalize(file);
	if (!FileTest.exist("./Game.rgssad")) {
		if (!FileTest.exist(canon_file)) return null;
		if (file.last == "/") return null;   // Is a directory
		begin;
			File.open(canon_file, "rb", f => { return f.read(1); });   // read one byte
		rescue Errno.ENOENT, Errno.EINVAL, Errno.EACCES, Errno.EISDIR;
			return null;
		}
	}
	str = null;
	begin;
		str = load_data(canon_file, true);
	rescue Errno.ENOENT, Errno.EINVAL, Errno.EACCES, Errno.EISDIR, RGSSError, MKXPError;
		str = null;
	}
	return str;
}

public void TryString(x) {
	ret = GetFileChar(x);
	return nil_or_empty(ret) ? null : x;
}

// Gets the contents of a file. Doesn't check RTP, but does check
// encrypted archives.
// NOTE: load_data will check anything added in MKXP's RTP setting, and matching
//       mount points added through System.mount.
public void GetFileString(file) {
	file = canonicalize(file);
	if (!FileTest.exist("./Game.rgssad")) {
		if (!FileTest.exist(file)) return null;
		begin;
			File.open(file, "rb", f => { return f.read; });   // read all data
		rescue Errno.ENOENT, Errno.EINVAL, Errno.EACCES;
			return null;
		}
	}
	str = null;
	begin;
		str = load_data(file, true);
	rescue Errno.ENOENT, Errno.EINVAL, Errno.EACCES, RGSSError, MKXPError;
		str = null;
	}
	return str;
}

//===============================================================================
//
//===============================================================================
public partial class StringInput {
	include Enumerable;

	public int lineno		{ get { return _lineno; } set { _lineno = value; } }			protected int _lineno;
	public int string		{ get { return _string; } }			protected int _string;

	public partial class : self {
		public override void new(str) {
			if (block_given()) {
				begin;
					f = base.new();
					yield f;
				ensure;
					f&.close;
				}
			} else {
				super;
			}
		}
		alias open new;
	}

	public void initialize(str) {
		@string = str;
		@pos = 0;
		@closed = false;
		@lineno = 0;
	}

	public void inspect() {
		return $"#<{self.class}:{@closed ? 'closed' : 'open'},src={@string[0, 30].inspect}>";
	}

	public void close() {
		if (@closed) raise IOError, "closed stream";
		@pos = null;
		@closed = true;
	}

	public bool closed() { @closed; }

	public void pos() {
		if (@closed) raise IOError, "closed stream";
		(int)Math.Min(@pos, @string.size)
	}

	alias tell pos;

	public int rewind { get { return seek(0); } }

	public int pos { set { seek(value); } }

	public void seek(offset, whence = IO.SEEK_SET) {
		if (@closed) raise IOError, "closed stream";
		switch (whence) {
			case IO.SEEK_SET:  @pos = offset; break;
			case IO.SEEK_CUR:  @pos += offset; break;
			case IO.SEEK_END:  @pos = @string.size - offset; break;
			default:
				Debug.LogError($"unknown seek flag: {whence}");
				//throw new Exception($"unknown seek flag: {whence}");
				break;
		}
		if (@pos < 0) @pos = 0;
		@pos = (int)Math.Min(@pos, @string.size + 1);
		offset;
	}

	public bool eof() {
		if (@closed) raise IOError, "closed stream";
		@pos > @string.size;
	}

	public void each(Action block = null) {
		if (@closed) raise IOError, "closed stream";
		begin;
			@string.each(&block);
		ensure;
			@pos = 0;
		}
	}

	public void gets() {
		if (@closed) raise IOError, "closed stream";
		idx = @string.index("\n", @pos);
		if (idx) {
			idx += 1;  // "\n".size
			line = @string[@pos...idx];
			@pos = idx;
			if (@pos == @string.size) @pos += 1;
		} else {
			line = @string[@pos..-1];
			@pos = @string.size + 1;
		}
		@lineno += 1;
		line;
	}

	public void getc() {
		if (@closed) raise IOError, "closed stream";
		ch = @string[@pos];
		@pos += 1;
		if (@pos == @string.size) @pos += 1;
		ch;
	}

	public void read(len = null) {
		if (@closed) raise IOError, "closed stream";
		if (!len) {
			if (eof()) return null;
			rest = @string[@pos...@string.size];
			@pos = @string.size + 1;
			return rest;
		}
		str = @string[@pos, len];
		@pos += len;
		if (@pos == @string.size) @pos += 1;
		str;
	}
	alias read_all read;
	alias sysread read;
}
