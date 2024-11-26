require 'zlib'

public partial class Numeric {
	public void to_digits(num = 3) {
		str = to_s;
		(num - str.size).times(() => str = str.prepend("0"))
		return str;
	}
}

public static partial class Scripts {
	public static void dump(path = "Data/Scripts", rxdata = "Data/Scripts.rxdata") {
		scripts = File.open(rxdata, 'rb', f => { Marshal.load(f); });
		if (scripts.length < 10) {
			p "Scripts look like they're already extracted. Not doing so again.";
			return;
		}
		create_directory(path);
		clear_directory(path);
		folder_id = new {1, 1};   // Can only have two layers of folders
		file_id = 1;
		level = 0;   // 0=main path, 1=subfolder, 2=sub-subfolder
		folder_path = path;
		folder_name = null;
		scripts.each_with_index do |e, i|
			_, title, script = e;
			title = title_to_filename(title).strip;
			script = Zlib.Inflate.inflate(script).delete("\r");
			if (title.empty() && script.empty()) continue;
			section_name = null;
			if (System.Text.RegularExpressions.Regex.IsMatch(title,@"\[\[\s*(.+)\s*\]\]$")) {   // Make a folder
				section_name = $~[1].strip;
				if (!section_name || section_name.empty()) section_name = "unnamed";
				folder_num =  (i < scripts.length - 2) ? folder_id[level].to_digits(3) : "999";
				folder_name = folder_num + "_" + section_name;
				create_directory(folder_path + "/" + folder_name);
				folder_id[level] += 1;
				if (level < folder_id.length-1) {
					level += 1;   // Go one level deeper
					folder_id[level] = 1;   // Reset numbering of subfolders
					folder_path += "/" + folder_name;
					folder_name = null;
				}
				file_id = 1;   // Reset numbering of script files
			} else if (title.start_with("=====")) {   // Return to top level directory
				level = 0;
				folder_path = path;
				folder_name = null;
			}
			// Create script file
			if (script.empty()) continue;
			this_folder = folder_path;
			if (folder_name) this_folder += "/" + folder_name;
			section_name ||= title.strip;
			if (!section_name || section_name.empty()) section_name = "unnamed";
			file_num =  (i < scripts.length - 1) ? file_id.to_digits(3) : "999";
			file_name = file_num + "_" + section_name + ".rb";
			create_script(this_folder + "/" + file_name, script);
			file_id += 1;   // Increment numbering of script files
		}
		// Backup Scripts.rxdata to ScriptsBackup.rxdata
		File.open("Data/ScriptsBackup.rxdata", "wb") do |f|
			Marshal.dump(scripts, f);
		}
		// Replace Scripts.rxdata with ScriptsLoader.rxdata
		createLoaderScripts(rxdata);
	}

	public static void createLoaderScripts(rxdata) {
	txt = "x\x9C}SM\x8F\x9B0\x10\xBDG\xCA\x7F\x18\xD8H\x8062\x9Bc\x0Fi\x0F\xDD\xB6\xEA\xA9\xD5&7H\x11\x1FC\xE2.\xB1\x91m\x9AnC\xFE{m\b8t\xDB^\x80\xF9z3\xF3\xE6q\a\xDB\x03\x95Pp\x94\xC0\xB8\x82\x13\x17\xCF@KP\a\x84}zD\xD0Ad\xB9x\xA9\x15\x16\xCE|6\x9F\x15\xA8\xA3\"\xCD1K\xF3\xE7D`\xCD\x85\x9A\xCF\x00\x8C\xD9\xF9a\r\v\x87\x8C&\xC9+\xCEp\x92A0\xCD\x0Fgh3\xD5\x1A\xBF\x8E(\"\x9B\xCC\xF1\xC3\xF8\xEC\xC7\xC5}\x10_\xC2\x00\xCEntw^\xAC.;\xFD|\xFA\xB4\xD9$\x9B\xF7O\x9F\xBFn7\xD1bE\x14O\xE8.Z\xED.\xEE\xC5 t\x0F\x81\xAA\x11\xCC\xF4>\xA2\x94\xE9\x1E\xE1\x1E\xDC\x98\xC5\xCC\xD5\x1F\xB6\xF7wN\x99\xAF\xFDn0\x9F!+\x86\x95DJ%&v1\x14\x82\v\x03\xAB\xC9\xB0\x90D\xD2_\bo\xD7\xF0\xE6\xE1\xA1\x1F\xFD#\xAD\x90\xF0\x1A\x99\xEF\x8D\xC5\xA4\xE2{o\t\xDE\xC9\xD3[@[\xB6P\x92\x93\xA0\n\xFD\x85\x13\xF4\xC3B\xDF\x10\xBC\xEDPd\x98V\x9CCF\xF7\x04\xBE4\xAAn\x14P\x06SPS\x8A\x95\xC4\e\x88\xCEe\xF6\xB8Y\xA6\xE2i\x91\xC8\\\xD0Z\xC9\xA4\x14\xFC\x98\x94\xBC*P\xF8u\xAA\x0E\x81\xA9(\xF5\xD8RC\xAC!\xDAuv\x17\x97\xA3\xFDH\x05)\xB90\x87\xEA\x8B\xB4D\xCC&}c\x86?\x95!\xA6\x84\xF5\x1A<\xE2A\xDB\x0E\xDF\xC4\xEBSt\xB4\xA3\xA6\xA0\x02s\xC5\xC5\xCB\xBB\x0E\xC7\xDCGame.GameData.4\a)\x83>olM\xEAF\x1E\xFC20\x95N\x19\x85\xDFb\x12\xEE\xFA\x1C\xBB\xF1u\xF0\xDB\\\x9D\x1A\x13\x91-B:d\e\x1E\xC6W\x9F/\xB5H\x1Dk\x9A\xB5&\v\xE5\xBC0\xBA\xB5\xC7\x9C\xCE\xBA\x04W\xB8\xFD-uF\xDB\xA1\x10MN1\x1C3\xC3=e\xC3\x88\xF8#\xAD|\x83\xB8\x04F\xAB\xE5\xB8\xAA@\x997\b\x9B\xEE.\x1F\x06}Y-\xDC\x04\b\xC3\x93oe7\x01\x18\x8AnUi~\x1Ek/_\xFD\xA0\xC1\xA4\xD3\xDFd\xFE\x8A\xB7\xEBU,sW\x87\xE5\xAEs\\\t\xFC\xAF\xE2,\x91\x9D/\xF8S\xB2\xFF,v\x1FS\x95\x86=/\xD2\r~\x03\x01\xDDe\xDF";
		File.open(rxdata, "wb") do |f|
			Marshal.dump(new {62054200, "Main", txt}, f);
		}
	}

	public static void from_folder(path = "Data/Scripts", rxdata = "Data/Scripts.rxdata") {
		scripts = File.open(rxdata, 'rb', f => { Marshal.load(f); });
		if (scripts.length > 10) {
			p "Scripts.rxdata already has a bunch of scripts in it. Won't consolidate script files.";
			return;
		}
		scripts = new List<string>();
		aggregate_from_folder(path, scripts);
		// Save scripts to file
		File.open(rxdata, "wb") do |f|
			Marshal.dump(scripts, f);
		}
	}

	public static void aggregate_from_folder(path, scripts, level = 0) {
		files = new List<string>();
		folders = new List<string>();
		foreach (var f in Dir.foreach(path)) { //Dir.foreach(path) do => |f|
			if (f == '.' || f == '..') continue;
			if (File.directory(path + "/" + f)) {
				if (!System.Text.RegularExpressions.Regex.IsMatch(f,@"^\.")) folders.Add(f);
			} else {
				if (System.Text.RegularExpressions.Regex.IsMatch(f,@"\.rb$",RegexOptions.IgnoreCase)) files.Add(f);
			}
		}
		// Aggregate individual script files into Scripts.rxdata
		files.sort!;
		foreach (var f in files) { //'files.each' do => |f|
			section_name = filename_to_title(f);
			content = File.open(path + "/" + f, "rb", f2 => { f2.read; });#.gsub(/\n/, "\r\n");
			scripts << new {rand(999_999), section_name, Zlib.Deflate.deflate(content)};
		}
		// Check each subfolder for scripts to aggregate
		folders.sort!;
		foreach (var f in folders) { //'folders.each' do => |f|
			section_name = filename_to_title(f);
			if (level == 0) scripts << new {rand(999_999), "==================", Zlib.Deflate.deflate("")};
			if (level == 1) scripts << new {rand(999_999), "", Zlib.Deflate.deflate("")};
			scripts << new {rand(999_999), "[[ " + section_name + " ]]", Zlib.Deflate.deflate("")};
			aggregate_from_folder(path + "/" + f, scripts, level + 1);
		}
	}

	public static void filename_to_title(filename) {
		filename = filename.bytes.pack('U*');
		title = "";
		if (System.Text.RegularExpressions.Regex.IsMatch(filename,@"^[^_]*_(.+)$")) {
			title = $~[1];
			if (title.end_with(".rb")) title = title[0..-4];
			title = title.strip;
		}
		if (!title || title.empty()) title = "unnamed";
		title = System.Text.RegularExpressions.Regex.Replace(title, "&bs;", "\\");
		title = System.Text.RegularExpressions.Regex.Replace(title, "&fs;", "/");
		title = System.Text.RegularExpressions.Regex.Replace(title, "&cn;", ":");
		title = System.Text.RegularExpressions.Regex.Replace(title, "&as;", "*");
		title = System.Text.RegularExpressions.Regex.Replace(title, "&qm;", "?");
		title = System.Text.RegularExpressions.Regex.Replace(title, "&dq;", "\"");
		title = System.Text.RegularExpressions.Regex.Replace(title, "&lt;", "<");
		title = System.Text.RegularExpressions.Regex.Replace(title, "&gt;", ">");
		title = System.Text.RegularExpressions.Regex.Replace(title, "&po;", "|");
		return title;
	}

	public static void title_to_filename(title) {
		filename = title.clone;
		filename = System.Text.RegularExpressions.Regex.Replace(filename, "\\", "&bs;");
		filename = System.Text.RegularExpressions.Regex.Replace(filename, "\;/", "&fs;")
		filename = System.Text.RegularExpressions.Regex.Replace(filename, ":", "&cn;");
		filename = System.Text.RegularExpressions.Regex.Replace(filename, "\*", "&as;");
		filename = System.Text.RegularExpressions.Regex.Replace(filename, "\?", "&qm;");
		filename = System.Text.RegularExpressions.Regex.Replace(filename, "\"", "&dq;");
		filename = System.Text.RegularExpressions.Regex.Replace(filename, "<", "&lt;");
		filename = System.Text.RegularExpressions.Regex.Replace(filename, ">", "&gt;");
		filename = System.Text.RegularExpressions.Regex.Replace(filename, "\|", "&po;");
		return filename;
	}

	public static void create_script(title, content) {
		f = new File(title, "wb");
		f.write content;
		f.close;
	}

	public static void clear_directory(path, delete_current = false) {
		foreach (var f in Dir.foreach(path)) { //Dir.foreach(path) do => |f|
			if (f == '.' || f == '..') continue;
			if (File.directory(path + "/" + f)) {
				clear_directory(path + "/" + f, true);
			} else {
				File.delete(path + "/" + f);
			}
		}
		if (delete_current) Dir.delete(path);
	}

	public static void create_directory(path) {
		paths = path.split('/');
		paths.each_with_index do |_e, i|
			if (!File.directory(paths[0..i].join('/'))) {
				Dir.mkdir(paths[0..i].join('/'));
			}
		}
	}
}

//Scripts.dump
Scripts.from_folder;
