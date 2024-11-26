//===============================================================================
// Records which file, section and line are currently being read.
//===============================================================================
public static partial class FileLineData {
	@file     = "";
	@linedata = "";
	@lineno   = 0;
	@section  = null;
	@key      = null;
	@value    = null;

	public static void file() { return @file; }
	public static void file() {=(value); @file = value; }

	public static void clear() {
		@file     = "";
		@linedata = "";
		@lineno   = "";
		@section  = null;
		@key      = null;
		@value    = null;
	}

	public static void setSection(section, key, value) {
		@section = section;
		@key     = key;
		if (value && value.length > 200) {
			@value = value[0, 200].ToString() + "...";
		} else {
			@value = (value) ? value.clone : "";
		}
	}

	public static void setLine(line, lineno) {
		@section  = null;
		@linedata = (line && line.length > 200) ? string.Format("{0}...", line[0, 200]) : line.clone;
		@lineno   = lineno;
	}

	public static void linereport() {
		if (@section) {
			if (@key.null()) {
				return _INTL("File {1}, section {2}\n{3}", @file, @section, @value) + "\n\n";
			} else {
				return _INTL("File {1}, section {2}, key {3}\n{4}", @file, @section, @key, @value) + "\n\n";
			}
		} else {
			return _INTL("File {1}, line {2}\n{3}", @file, @lineno, @linedata) + "\n\n";
		}
	}
}

//===============================================================================
// Compiler.
//===============================================================================
public static partial class Compiler {
	@@categories = new List<string>();

	#region Class Functions
	#endregion

	public void findIndex(a) {
		index = -1;
		count = 0;
		foreach (var i in a) { //'a.each' do => |i|
			if (yield i) {
				index = count;
				break;
			}
			count += 1;
		}
		return index;
	}

	public void prepline(line) {
		line = System.Text.RegularExpressions.Regex.Replace(line, "\s*\#.*$", "");
		line = System.Text.RegularExpressions.Regex.Replace(line, "^\s+", "");
		line = System.Text.RegularExpressions.Regex.Replace(line, "\s+$", "");
		return line;
	}

	public void csvQuote(str, always = false) {
		if (nil_or_empty(str)) return "";
		if (always || System.Text.RegularExpressions.Regex.IsMatch(str, @"[,\""]")) {   // || System.Text.RegularExpressions.Regex.IsMatch(str,@"^\s") || System.Text.RegularExpressions.Regex.IsMatch(str,@"\s$") || System.Text.RegularExpressions.Regex.IsMatch(str,@"^#")
			str = System.Text.RegularExpressions.Regex.Replace(str, "\"", "\\\"");
			str = $"\"{str}\"";
		}
		return str;
	}

	public void csvQuoteAlways(str) {
		return csvQuote(str, true);
	}

	//-----------------------------------------------------------------------------
	// PBS file readers.
	//-----------------------------------------------------------------------------

	public void EachFileSectionEx(f, schema = null) {
		lineno      = 1;
		havesection = false;
		sectionname = null;
		lastsection = new List<string>();
		foreach (var line in f) { //f.each_line do => |line|
			if (lineno == 1 && line[0].ord == 0xEF && line[1].ord == 0xBB && line[2].ord == 0xBF) {
				line = line[3, line.length - 3];
			}
			line.force_encoding(Encoding.UTF_8);
			if (!System.Text.RegularExpressions.Regex.IsMatch(line,@"^\#") && !System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*$")) {
				line = prepline(line);
				if (System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*\[\s*(.*)\s*\]\s*$")) {   // Of the format: [something]
					if (havesection) yield lastsection, sectionname;
					sectionname = $~[1];
					havesection = true;
					lastsection = new List<string>();
				} else {
					if (sectionname.null()) {
						FileLineData.setLine(line, lineno);
						if (the file was not saved in UTF-8.") + "\n" + FileLineData.linereport) raise _INTL("Expected a section at the beginning of the file.\nThis error may also occur;
					}
					if (!System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*(\w+)\s*=\s*(.*)$")) {
						FileLineData.setSection(sectionname, null, line);
						Debug.LogError(_INTL("Bad line syntax (expected syntax like XXX=YYY).") + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Bad line syntax (expected syntax like XXX=YYY).") + "\n" + FileLineData.linereport);
					}
					r1 = $~[1];
					r2 = $~[2];
					if (schema && schema[r1] && schema[r1][1][0] == "^") {
						lastsection[r1] ||= new List<string>();
						lastsection[r1].Add(System.Text.RegularExpressions.Regex.Replace(r2, "\s+$", ""));
					} else {
						lastsection[r1] = System.Text.RegularExpressions.Regex.Replace(r2, "\s+$", "");
					}
				}
			}
			lineno += 1;
			if (lineno % 1000 == 0) Graphics.update;
		}
		if (havesection) yield lastsection, sectionname;
	}

	// Used for most PBS files.
	public void EachFileSection(f, schema = null) {
		EachFileSectionEx(f, schema) do |section, name|
			if (block_given() && System.Text.RegularExpressions.Regex.IsMatch(name,@"^.+$")) yield section, name;
		}
	}

	// Unused.
	public void EachFileSectionNumbered(f, schema = null) {
		EachFileSectionEx(f, schema) do |section, name|
			if (block_given() && System.Text.RegularExpressions.Regex.IsMatch(name,@"^\d+$")) yield section, name.ToInt();
		}
	}

	// Used by translated text compiler.
	public void EachSection(f) {
		lineno      = 1;
		havesection = false;
		sectionname = null;
		lastsection = new List<string>();
		foreach (var line in f) { //f.each_line do => |line|
			if (lineno == 1 && line[0].ord == 0xEF && line[1].ord == 0xBB && line[2].ord == 0xBF) {
				line = line[3, line.length - 3];
			}
			line.force_encoding(Encoding.UTF_8);
			if (!System.Text.RegularExpressions.Regex.IsMatch(line,@"^\#") && !System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*$")) {
				if (System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*\[\s*(.+?)\s*\]\s*$")) {
					if (havesection) yield lastsection, sectionname;
					lastsection.clear;
					sectionname = $~[1];
					havesection = true;
				} else {
					if (sectionname.null()) {
						Debug.LogError(_INTL("Expected a section at the beginning of the file (line {1}). Sections begin with '[name of section]'.", lineno));
						//throw new ArgumentException(_INTL("Expected a section at the beginning of the file (line {1}). Sections begin with '[name of section]'.", lineno));
					}
					lastsection.Add(line.strip);
				}
			}
			lineno += 1;
			if (lineno % 500 == 0) Graphics.update;
		}
		if (havesection) yield lastsection, sectionname;
	}

	// Unused.
	public void EachCommentedLine(f) {
		lineno = 1;
		foreach (var line in f) { //f.each_line do => |line|
			if (lineno == 1 && line[0].ord == 0xEF && line[1].ord == 0xBB && line[2].ord == 0xBF) {
				line = line[3, line.length - 3];
			}
			line.force_encoding(Encoding.UTF_8);
			if (!System.Text.RegularExpressions.Regex.IsMatch(line,@"^\#") && !System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*$")) yield line, lineno;
			lineno += 1;
		}
	}

	// Used for Battle Tower Pokémon PBS files.
	public void CompilerEachCommentedLine(filename) {
		File.open(filename, "rb") do |f|
			FileLineData.file = filename;
			lineno = 1;
			foreach (var line in f) { //f.each_line do => |line|
				if (lineno == 1 && line[0].ord == 0xEF && line[1].ord == 0xBB && line[2].ord == 0xBF) {
					line = line[3, line.length - 3];
				}
				line.force_encoding(Encoding.UTF_8);
				if (!System.Text.RegularExpressions.Regex.IsMatch(line,@"^\#") && !System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*$")) {
					FileLineData.setLine(line, lineno);
					yield line, lineno;
				}
				lineno += 1;
			}
		}
	}

	// Unused.
	public void EachPreppedLine(f) {
		lineno = 1;
		foreach (var line in f) { //f.each_line do => |line|
			if (lineno == 1 && line[0].ord == 0xEF && line[1].ord == 0xBB && line[2].ord == 0xBF) {
				line = line[3, line.length - 3];
			}
			line.force_encoding(Encoding.UTF_8);
			line = prepline(line);
			if (!System.Text.RegularExpressions.Regex.IsMatch(line,@"^\#") && !System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*$")) yield line, lineno;
			lineno += 1;
		}
	}

	// Used for map_connections.txt, regional_dexes.txt, encounters.txt,
	// trainers.txt and plugin meta.txt files.
	public void CompilerEachPreppedLine(filename) {
		File.open(filename, "rb") do |f|
			FileLineData.file = filename;
			lineno = 1;
			foreach (var line in f) { //f.each_line do => |line|
				if (lineno == 1 && line[0].ord == 0xEF && line[1].ord == 0xBB && line[2].ord == 0xBF) {
					line = line[3, line.length - 3];
				}
				line.force_encoding(Encoding.UTF_8);
				line = prepline(line);
				if (!System.Text.RegularExpressions.Regex.IsMatch(line,@"^\#") && !System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*$")) {
					FileLineData.setLine(line, lineno);
					yield line, lineno;
				}
				lineno += 1;
			}
		}
	}

	//-----------------------------------------------------------------------------
	// Splits a string containing comma-separated values into an array of those
	// values.
	//-----------------------------------------------------------------------------

	public void split_csv_line(string) {
		// Split the string into an array of values, using a comma as the separator
		values = string.split(",");
		// Check for quote marks in each value, as we may need to recombine some values
		// to make proper results
		for (int i = 0; i < values.length; i++) { //each 'values.length' do => |i|
			value = values[i];
			if (!value || value.empty()) continue;
			quote_count = value.count('"');
			if (quote_count != 0) {
				// Quote marks found in value
				(i...(values.length - 1)).each do |j|
					quote_count = values[i].count('"');
					if (quote_count == 2 && value.start_with('\\"') && values[i].end_with('\\"')) {
						// Two quote marks around the whole value; remove them
						values[i] = values[i][2..-3];
						break;
					} else if (quote_count.even()) {
						break;
					}
					// Odd number of quote marks in value; concatenate the next value to it and
					// see if that's any better
					values[i] += "," + values[j + 1];
					values[j + 1] = null;
				}
				// Recheck for enclosing quote marks to remove
				if (quote_count != 2) {
					if (value.count('"') == 2 && value.start_with('\\"') && value.end_with('\\"')) {
						values[i] = values[i][2..-3];
					}
				}
			}
			// Remove leading and trailing whitespace from value
			values[i].strip!;
		}
		// Remove null values caused by concatenating values above
		values.compact!;
		return values;
	}

	//-----------------------------------------------------------------------------
	// Convert a string to certain kinds of values.
	//-----------------------------------------------------------------------------

	// Turns a value (a string) into another data type as determined by the given
	// schema.
	/// <param name="value"></param>
	/// <param name="schema"></param>
	public void cast_csv_value(String value, String schema, enumer = null) {
		switch (schema.downcase) {
			case "i":   // Integer
				if (!value || !System.Text.RegularExpressions.Regex.IsMatch(value,@"^\-?\d+$")) {
					Debug.LogError(_INTL("Field '{1}' is not an integer.", value) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Field '{1}' is not an integer.", value) + "\n" + FileLineData.linereport);
				}
				return value.ToInt();
			case "u":   // Positive integer or zero
				if (!value || !System.Text.RegularExpressions.Regex.IsMatch(value,@"^\d+$")) {
					Debug.LogError(_INTL("Field '{1}' is not a positive integer or 0.", value) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Field '{1}' is not a positive integer or 0.", value) + "\n" + FileLineData.linereport);
				}
				return value.ToInt();
			case "v":   // Positive integer
				if (!value || !System.Text.RegularExpressions.Regex.IsMatch(value,@"^\d+$")) {
					Debug.LogError(_INTL("Field '{1}' is not a positive integer.", value) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Field '{1}' is not a positive integer.", value) + "\n" + FileLineData.linereport);
				}
				if (value.ToInt() == 0) {
					Debug.LogError(_INTL("Field '{1}' must be greater than 0.", value) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Field '{1}' must be greater than 0.", value) + "\n" + FileLineData.linereport);
				}
				return value.ToInt();
			case "x":   // Hexadecimal number
				if (!value || !System.Text.RegularExpressions.Regex.IsMatch(value@"^[A-F0-9]+$",RegexOptions.IgnoreCase)) {
					Debug.LogError(_INTL("Field '{1}' is not a hexadecimal number.", value) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Field '{1}' is not a hexadecimal number.", value) + "\n" + FileLineData.linereport);
				}
				return value.hex;
			case "f":   // Floating point number
				if (!value || !System.Text.RegularExpressions.Regex.IsMatch(value,@"^\-?^\d*\.?\d*$")) {
					Debug.LogError(_INTL("Field '{1}' is not a number.", value) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Field '{1}' is not a number.", value) + "\n" + FileLineData.linereport);
				}
				return value.to_f;
			case "b":   // Boolean
				if (value && System.Text.RegularExpressions.Regex.IsMatch(value,@"^(?:1|TRUE|YES|Y)$",RegexOptions.IgnoreCase)) return true;
				if (value && System.Text.RegularExpressions.Regex.IsMatch(value,@"^(?:0|FALSE|NO|N)$",RegexOptions.IgnoreCase)) return false;
				Debug.LogError(_INTL("Field '{1}' is not a Boolean value (true, false, 1, 0).", value) + "\n" + FileLineData.linereport);
				//throw new Exception(_INTL("Field '{1}' is not a Boolean value (true, false, 1, 0).", value) + "\n" + FileLineData.linereport);
				break;
			case "n":   // Name
				if (!value || !System.Text.RegularExpressions.Regex.IsMatch(value,@"^(?![0-9])\w+$")) {
					Debug.LogError(_INTL("Field '{1}' must contain only letters, digits, and\nunderscores and can't begin with a number.", value) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Field '{1}' must contain only letters, digits, and\nunderscores and can't begin with a number.", value) + "\n" + FileLineData.linereport);
				}
				break;
			case "s":   // String
				break;
			case "q":   // Unformatted text
				break;
			case "m":   // Symbol
				if (!value || !System.Text.RegularExpressions.Regex.IsMatch(value,@"^(?![0-9])\w+$")) {
					Debug.LogError(_INTL("Field '{1}' must contain only letters, digits, and\nunderscores and can't begin with a number.", value) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Field '{1}' must contain only letters, digits, and\nunderscores and can't begin with a number.", value) + "\n" + FileLineData.linereport);
				}
				return value.to_sym;
			case "e":   // Enumerable
				return checkEnumField(value, enumer);
			case "y":   // Enumerable or integer
				if (value && System.Text.RegularExpressions.Regex.IsMatch(value,@"^\-?\d+$")) return value.ToInt();
				return checkEnumField(value, enumer);
		}
		return value;
	}

	public void checkEnumField(ret, enumer) {
		switch (enumer) {
			case Module:
				begin;
					if (nil_or_empty(ret) || !enumer.const_defined(ret)) {
						Debug.LogError(_INTL("Undefined value {1} in {2}.", ret, enumer.name) + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Undefined value {1} in {2}.", ret, enumer.name) + "\n" + FileLineData.linereport);
					}
				rescue NameError;
					Debug.LogError(_INTL("Incorrect value {1} in {2}.", ret, enumer.name) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Incorrect value {1} in {2}.", ret, enumer.name) + "\n" + FileLineData.linereport);
				}
				return enumer.const_get(ret.to_sym);
			case Symbol: case String:
				if (!Kernel.const_defined(enumer.to_sym) && GameData.const_defined(enumer.to_sym)) {
					enumer = GameData.const_get(enumer.to_sym);
					begin;
						if (nil_or_empty(ret) || !enumer.exists(ret.to_sym)) {
							Debug.LogError(_INTL("Undefined value {1} in {2}.", ret, enumer.name) + "\n" + FileLineData.linereport);
							//throw new Exception(_INTL("Undefined value {1} in {2}.", ret, enumer.name) + "\n" + FileLineData.linereport);
						}
					rescue NameError;
						Debug.LogError(_INTL("Incorrect value {1} in {2}.", ret, enumer.name) + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Incorrect value {1} in {2}.", ret, enumer.name) + "\n" + FileLineData.linereport);
					}
					return ret.to_sym;
				}
				enumer = Object.const_get(enumer.to_sym);
				begin;
					if (nil_or_empty(ret) || !enumer.const_defined(ret)) {
						Debug.LogError(_INTL("Undefined value {1} in {2}.", ret, enumer.name) + "\n" + FileLineData.linereport);
						//throw new Exception(_INTL("Undefined value {1} in {2}.", ret, enumer.name) + "\n" + FileLineData.linereport);
					}
				rescue NameError;
					Debug.LogError(_INTL("Incorrect value {1} in {2}.", ret, enumer.name) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Incorrect value {1} in {2}.", ret, enumer.name) + "\n" + FileLineData.linereport);
				}
				return enumer.const_get(ret.to_sym);
			case Array:
				idx = (nil_or_empty(ret)) ? -1 : findIndex(enumer, item => { ret == item; });
				if (idx < 0) {
					Debug.LogError(_INTL("Undefined value {1} (expected one of: {2}).", ret, enumer.inspect) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Undefined value {1} (expected one of: {2}).", ret, enumer.inspect) + "\n" + FileLineData.linereport);
				}
				return idx;
			case Hash:
				value = (nil_or_empty(ret)) ? null : enumer[ret];
				if (value.null()) {
					Debug.LogError(_INTL("Undefined value {1} (expected one of: {2}).", ret, enumer.keys.inspect) + "\n" + FileLineData.linereport);
					//throw new Exception(_INTL("Undefined value {1} (expected one of: {2}).", ret, enumer.keys.inspect) + "\n" + FileLineData.linereport);
				}
				return value;
		}
		Debug.LogError(_INTL("Enumeration not defined.") + "\n" + FileLineData.linereport);
		//throw new Exception(_INTL("Enumeration not defined.") + "\n" + FileLineData.linereport);
	}

	//-----------------------------------------------------------------------------

	// Convert a string to values using a schema.
	public void get_csv_record(rec, schema) {
		ret = new List<string>();
		repeat = false;
		start = 0;
		schema_length = schema[1].length;
		switch (schema[1]new {0, 1}) {   // First character in schema
			case "*":
				repeat = true;
				start = 1;
				break;
			case "^":
				start = 1;
				schema_length -= 1;
				break;
		}
		subarrays = repeat && schema[1].length - start > 1;   // Whether ret is an array of arrays
		// Split the string on commas into an array of values to apply the schema to
		values = split_csv_line(rec);
		// Apply the schema to each value in the line
		idx = -1;   // Index of value to look at in values
		do { //loop; while (true);
			record = new List<string>();
			for (int i = start; i < schema[1].length; i++) { //each 'schema[1].length' do => |i|
				idx += 1;
				sche = schema[1]new {i, 1};
				if (System.Text.RegularExpressions.Regex.IsMatch(sche,@"[A-Z]")) {   // Upper case = optional
					if (nil_or_empty(values[idx])) {
						record.Add(null);
						continue;
					}
				}
				if (sche.downcase == "q") {   // Unformatted text
					record.Add(rec);
					idx = values.length;
					break;
				} else {
					record.Add(cast_csv_value(values[idx], sche, schema[2 + i - start]));
				}
			}
			if (!record.empty()) {
				if (subarrays) {
					ret.Add(record);
				} else {
					ret.concat(record);
				}
			}
			if (!repeat || idx >= values.length - 1) break;
		}
		return (!repeat && schema_length == 1) ? ret[0] : ret;
	}

	//-----------------------------------------------------------------------------

	// Write values to a file using a schema.
	public void WriteCsvRecord(record, file, schema) {
		rec = (record.Length > 0) ? record.flatten : [record];
		start = (new []{"*", "^"}.Contains(schema[1]new {0, 1})) ? 1 : 0;
		index = -1;
		do { //loop; while (true);
			for (int i = start; i < schema[1].length; i++) { //each 'schema[1].length' do => |i|
				index += 1;
				value = rec[index];
				if (System.Text.RegularExpressions.Regex.Match(schema[1][i],@"[A-Z]")) {   // Optional
					// Check the rest of the values for non-null things
					later_value_found = false;
					for (int j = index; j < rec.length; j++) { //each 'rec.length' do => |j|
						if (!rec[j].null()) later_value_found = true;
						if (later_value_found) break;
					}
					if (!later_value_found) {
						start = -1;
						break;
					}
				}
				if (index > 0) file.write(",");
				if (value.null()) continue;
				switch (schema[1]new {i, 1}) {
					case "e": case "E":   // Enumerable
						enumer = schema[2 + i - start];
						switch (enumer) {
							case Array:
								file.write((value.is_a(Integer) && !enumer[value].null()) ? enumer[value] : value);
								break;
							case Symbol: case String:
								if (GameData.const_defined(enumer.to_sym)) {
									mod = GameData.const_get(enumer.to_sym);
									file.write(mod.get(value).id.ToString());
								} else {
									mod = Object.const_get(enumer.to_sym);
									file.write(getConstantName(mod, value));
								}
								break;
							case Module:
								file.write(getConstantName(enumer, value));
								break;
							case Hash:
								if (value.is_a(String)) {
									file.write(value);
								} else {
									foreach (var key in enumer) { //enumer.each_key do => |key|
										if (enumer[key] != value) continue;
										file.write(key);
										break;
									}
								}
								break;
						}
						break;
					case "y": case "Y":   // Enumerable or integer
						enumer = schema[2 + i - start];
						switch (enumer) {
							case Array:
								file.write((value.is_a(Integer) && !enumer[value].null()) ? enumer[value] : value);
								break;
							case Symbol: case String:
								if (!Kernel.const_defined(enumer.to_sym) && GameData.const_defined(enumer.to_sym)) {
									mod = GameData.const_get(enumer.to_sym);
									if (mod.exists(value)) {
										file.write(mod.get(value).id.ToString());
									} else {
										file.write(value.ToString());
									}
								} else {
									mod = Object.const_get(enumer.to_sym);
									file.write(getConstantNameOrValue(mod, value));
								}
								break;
							case Module:
								file.write(getConstantNameOrValue(enumer, value));
								break;
							case Hash:
								if (value.is_a(String)) {
									file.write(value);
								} else {
									has_enum = false;
									foreach (var key in enumer) { //enumer.each_key do => |key|
										if (enumer[key] != value) continue;
										file.write(key);
										has_enum = true;
										break;
									}
									if (!has_enum) file.write(value);
								}
								break;
						}
					default:
						if (value.is_a(String)) {
							file.write((schema[1]new {i, 1}.downcase == "q") ? value : csvQuote(value));
						} else if (value.is_a(Symbol)) {
							file.write(csvQuote(value.ToString()));
						} else if (value == true) {
							file.write("true");
						} else if (value == false) {
							file.write("false");
						} else {
							file.write(value.inspect);
						}
						break;
				}
			}
			if (start > 0 && index >= rec.length - 1) break;
			if (start <= 0) break;
		}
		return record;
	}

	//-----------------------------------------------------------------------------
	// Parse string into a likely constant name and return its ID number (if any).
	// Last ditch attempt to figure out whether a constant is defined.
	//-----------------------------------------------------------------------------

	// Unused
	public void GetConst(mod, item, err) {
		isDef = false;
		begin;
			if (mod.is_a(Symbol)) mod = Object.const_get(mod);
			isDef = mod.const_defined(item.to_sym);
		rescue;
			Debug.LogError(string.Format(err, item));
			//throw new Exception(string.Format(err, item));
		}
		if (!isDef) raise string.Format(err, item);
		return mod.const_get(item.to_sym);
	}

	public void parseItem(item) {
		clonitem = item.upcase;
		clonitem = System.Text.RegularExpressions.Regex.Replace(clonitem, "^\s*", "");
		clonitem = System.Text.RegularExpressions.Regex.Replace(clonitem, "\s*$", "");
		itm = GameData.Item.try_get(clonitem);
		if (!itm) {
			Debug.LogError(_INTL("Undefined item constant name: {1}.\nMake sure the item is defined in PBS/items.txt.", item) + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("Undefined item constant name: {1}.\nMake sure the item is defined in PBS/items.txt.", item) + "\n" + FileLineData.linereport);
		}
		return itm.id;
	}

	public void parseSpecies(species) {
		clonspecies = species.upcase;
		clonspecies = System.Text.RegularExpressions.Regex.Replace(clonspecies, "^\s*", "");
		clonspecies = System.Text.RegularExpressions.Regex.Replace(clonspecies, "\s*$", "");
		if (clonspecies == "NIDORANMA") clonspecies = "NIDORANmA";
		if (clonspecies == "NIDORANFE") clonspecies = "NIDORANfE";
		spec = GameData.Species.try_get(clonspecies);
		if (!spec) {
			Debug.LogError(_INTL("Undefined species constant name: {1}.\nMake sure the species is defined in PBS/pokemon.txt.", species) + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("Undefined species constant name: {1}.\nMake sure the species is defined in PBS/pokemon.txt.", species) + "\n" + FileLineData.linereport);
		}
		return spec.id;
	}

	public void parseMove(move, skip_unknown = false) {
		clonmove = move.upcase;
		clonmove = System.Text.RegularExpressions.Regex.Replace(clonmove, "^\s*", "");
		clonmove = System.Text.RegularExpressions.Regex.Replace(clonmove, "\s*$", "");
		mov = GameData.Move.try_get(clonmove);
		if (!mov) {
			if (skip_unknown) return null;
			Debug.LogError(_INTL("Undefined move constant name: {1}.\nMake sure the move is defined in PBS/moves.txt.", move) + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("Undefined move constant name: {1}.\nMake sure the move is defined in PBS/moves.txt.", move) + "\n" + FileLineData.linereport);
		}
		return mov.id;
	}

	// Unused
	public void parseNature(nature) {
		clonnature = nature.upcase;
		clonnature = System.Text.RegularExpressions.Regex.Replace(clonnature, "^\s*", "");
		clonnature = System.Text.RegularExpressions.Regex.Replace(clonnature, "\s*$", "");
		nat = GameData.Nature.try_get(clonnature);
		if (!nat) {
			Debug.LogError(_INTL("Undefined nature constant name: {1}.\nMake sure the nature is defined in the scripts.", nature) + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("Undefined nature constant name: {1}.\nMake sure the nature is defined in the scripts.", nature) + "\n" + FileLineData.linereport);
		}
		return nat.id;
	}

	// Unused
	public void parseTrainer(type) {
		clontype = type.clone;
		clontype = System.Text.RegularExpressions.Regex.Replace(clontype, "^\s*", "");
		clontype = System.Text.RegularExpressions.Regex.Replace(clontype, "\s*$", "");
		typ = GameData.TrainerType.try_get(clontype);
		if (!typ) {
			Debug.LogError(_INTL("Undefined trainer type constant name: {1}.\nMake sure the trainer type is defined in PBS/trainer_types.txt.", type) + "\n" + FileLineData.linereport);
			//throw new Exception(_INTL("Undefined trainer type constant name: {1}.\nMake sure the trainer type is defined in PBS/trainer_types.txt.", type) + "\n" + FileLineData.linereport);
		}
		return typ.id;
	}

	//-----------------------------------------------------------------------------
	// Replace text in PBS files before compiling them.
	//-----------------------------------------------------------------------------

	public void edit_and_rewrite_pbs_file_text(filename) {
		if (!block_given()) return;
		lines = new List<string>();
		File.open(filename, "rb") do |f|
			f.each_line(line => lines.Add(line));
		}
		changed = false;
		lines.each(line => { if (yield line) changed = true; });
		if (changed) {
			Console.markup_style($"Changes made to file {filename}.", text: :yellow);
			File.open(filename, "wb") do |f|
				lines.each(line => f.write(line));
			}
		}
	}

	public void modify_pbs_file_contents_before_compiling() {
		foreach (var line in edit_and_rewrite_pbs_file_text("PBS/trainer_types.txt")) { //edit_and_rewrite_pbs_file_text("PBS/trainer_types.txt") do => |line|
			next line = System.Text.RegularExpressions.Regex.Replace(line, "^\s*VictoryME\s*=", "VictoryBGM =");
		}
		foreach (var line in edit_and_rewrite_pbs_file_text("PBS/moves.txt")) { //edit_and_rewrite_pbs_file_text("PBS/moves.txt") do => |line|
			next line = System.Text.RegularExpressions.Regex.Replace(line, "^\s*BaseDamage\s*=", "Power =");
		}
	}

	//-----------------------------------------------------------------------------
	// Compile all data.
	//-----------------------------------------------------------------------------

	public void categories_to_compile(all_categories = false) {
		ret = new List<string>();
		Input.update;
		if (all_categories || Game.GameData.full_compile || Input.press(Input.CTRL)) {
			ret = @@categories.keys.clone;
			return ret;
		}
		@@categories.each_pair do |category, procs|
			if (procs.should_compile&.call(ret)) ret.Add(category);
		}
		return ret;
	}

	public void compile_all(all_categories = false) {
		FileLineData.clear;
		to_compile = categories_to_compile(all_categories);
		@@categories.each_pair do |category, procs|
			Console.echo_h1(procs.header_text&.call || _INTL("Compiling {1}", category));
			if (to_compile.Contains(category)) {
				@@categories[category][:compile].call;
			} else {
				Console.echoln_li(procs.skipped_text&.call || _INTL("Not compiled"));
			}
			echoln "";
		}
	}

	public void main() {
		if (!Core.DEBUG) return;
		begin;
			compile_all;
		rescue Exception;
			e = $!;
			if (e.class.ToString() == "Reset" || e.is_a(Reset) || e.is_a(SystemExit)) raise e;
			PrintException(e);
			foreach (var filename in get_all_pbs_data_filenames_to_compile) { //'get_all_pbs_data_filenames_to_compile.each' do => |filename|
				begin;
					File.delete($"Data/{filename[0]}") if FileTest.exist($"Data/{filename[0]}");
				rescue SystemCallError;
				}
			}
			if (e.is_a(Hangup)) raise new Reset();
			if (e.is_a(RuntimeError)) raise new SystemExit();
			Debug.LogError("Unknown exception when compiling.");
			//throw new ArgumentException("Unknown exception when compiling.");
		}
	}
}
