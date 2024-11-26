//===============================================================================
//
//===============================================================================
public static partial class Translator {
	#region Class Functions
	#endregion

	public void gather_script_and_event_texts() {
		Graphics.update;
		begin;
			t = System.uptime;
			texts = new List<string>();
			// Get script texts from Scripts.rxdata
			foreach (var script in Game.GameData.RGSS_SCRIPTS) { //'Game.GameData.RGSS_SCRIPTS.each' do => |script|
				if (System.uptime - t >= 5) {
					t += 5;
					Graphics.update;
				}
				scr = Zlib.Inflate.inflate(script[2]);
				find_translatable_text_from_RGSS_script(texts, scr);
			}
			// If Scripts.rxdata only has 1 section, scripts have been extracted. Get
			// script texts from .rb files in Data/Scripts
			if (Game.GameData.RGSS_SCRIPTS.length == 1) {
				foreach (var script_file in Dir.all("Data/Scripts")) { //'Dir.all("Data/Scripts").each' do => |script_file|
					if (System.uptime - t >= 5) {
						t += 5;
						Graphics.update;
					}
					File.open(script_file, "rb") do |f|
						find_translatable_text_from_RGSS_script(texts, f.read);
					}
				}
			}
			// Get script texts from plugin script files
			if (FileTest.exist("Data/PluginScripts.rxdata")) {
				plugin_scripts = load_data("Data/PluginScripts.rxdata");
				foreach (var plugin in plugin_scripts) { //'plugin_scripts.each' do => |plugin|
					foreach (var script in plugin[2]) { //'plugin[2].each' do => |script|
						if (System.uptime - t >= 5) {
							t += 5;
							Graphics.update;
						}
						scr = Zlib.Inflate.inflate(script[1]).force_encoding(Encoding.UTF_8);
						find_translatable_text_from_RGSS_script(texts, scr);
					}
				}
			}
			MessageTypes.addMessagesAsHash(MessageTypes.SCRIPT_TEXTS, texts);
			// Find all text in common events and add them to messages
			commonevents = load_data("Data/CommonEvents.rxdata");
			items = new List<string>();
			choices = new List<string>();
			foreach (var event in commonevents.compact) { //'commonevents.compact.each' do => |event|
				if (System.uptime - t >= 5) {
					t += 5;
					Graphics.update;
				}
				begin;
					neednewline = false;
					lastitem = "";
					for (int j = event.list.size; j < event.list.size; j++) { //for 'event.list.size' times do => |j|
						list = event.list[j];
						if (neednewline && list.code != 401) {   // Continuation of 101 Show Text
							if (lastitem != "") {
								lastitem = System.Text.RegularExpressions.Regex.Replace(lastitem, "([^\.\!\?])\s\s+", m => Game.GameData.1 + " ");
								items.Add(lastitem);
								lastitem = "";
							}
							neednewline = false;
						}
						if (list.code == 101) {   // Show Text
							lastitem += list.parameters[0].ToString();
							neednewline = true;
						} else if (list.code == 102) {   // Show Choices
							for (int k = list.parameters[0].length; k < list.parameters[0].length; k++) { //for 'list.parameters[0].length' times do => |k|
								choices.Add(list.parameters[0][k]);
							}
							neednewline = false;
						} else if (list.code == 401) {   // Continuation of 101 Show Text
							if (lastitem != "") lastitem += " ";
							lastitem += list.parameters[0].ToString();
							neednewline = true;
						} else if (list.code == 355 || list.code == 655) {   // Script or script continuation line
							find_translatable_text_from_event_script(items, list.parameters[0]);
						} else if (list.code == 111 && list.parameters[0] == 12) {   // Conditional Branch
							find_translatable_text_from_event_script(items, list.parameters[1]);
						} else if (list.code == 209) {   // Set Move Route
							route = list.parameters[1];
							for (int k = route.list.size; k < route.list.size; k++) { //for 'route.list.size' times do => |k|
								if (route.list[k].code == MoveRoute.SCRIPT) {
									find_translatable_text_from_event_script(items, route.list[k].parameters[0]);
								}
							}
						}
					}
					if (neednewline && lastitem != "") {
						items.Add(lastitem);
						lastitem = "";
					}
				}
			}
			if (System.uptime - t >= 5) {
				t += 5;
				Graphics.update;
			}
			items |= new List<string>();
			choices |= new List<string>();
			items.concat(choices);
			MessageTypes.setMapMessagesAsHash(0, items);
			// Find all text in map events and add them to messages
			mapinfos = LoadMapInfos;
			foreach (var id in mapinfos) { //mapinfos.each_key do => |id|
				if (System.uptime - t >= 5) {
					t += 5;
					Graphics.update;
				}
				filename = string.Format("Data/Map{0:3}.rxdata", id);
				if (!RgssExists(filename)) continue;
				map = load_data(filename);
				items = new List<string>();
				choices = new List<string>();
				foreach (var event in map.events) { //map.events.each_value do => |event|
					if (System.uptime - t >= 5) {
						t += 5;
						Graphics.update;
					}
					begin;
						for (int i = event.pages.size; i < event.pages.size; i++) { //for 'event.pages.size' times do => |i|
							neednewline = false;
							lastitem = "";
							for (int j = event.pages[i].list.size; j < event.pages[i].list.size; j++) { //for 'event.pages[i].list.size' times do => |j|
								list = event.pages[i].list[j];
								if (neednewline && list.code != 401) {   // Continuation of 101 Show Text
									if (lastitem != "") {
										lastitem = System.Text.RegularExpressions.Regex.Replace(lastitem, "([^\.\!\?])\s\s+", m => Game.GameData.1 + " ");
										items.Add(lastitem);
										lastitem = "";
									}
									neednewline = false;
								}
								if (list.code == 101) {   // Show Text
									lastitem += list.parameters[0].ToString();
									neednewline = true;
								} else if (list.code == 102) {   // Show Choices
									for (int k = list.parameters[0].length; k < list.parameters[0].length; k++) { //for 'list.parameters[0].length' times do => |k|
										choices.Add(list.parameters[0][k]);
									}
									neednewline = false;
								} else if (list.code == 401) {   // Continuation of 101 Show Text
									if (lastitem != "") lastitem += " ";
									lastitem += list.parameters[0].ToString();
									neednewline = true;
								} else if (list.code == 355 || list.code == 655) {   // Script or script continuation line
									find_translatable_text_from_event_script(items, list.parameters[0]);
								} else if (list.code == 111 && list.parameters[0] == 12) {   // Conditional Branch
									find_translatable_text_from_event_script(items, list.parameters[1]);
								} else if (list.code == 209) {   // Set Move Route
									route = list.parameters[1];
									for (int k = route.list.size; k < route.list.size; k++) { //for 'route.list.size' times do => |k|
										if (route.list[k].code == MoveRoute.SCRIPT) {
											find_translatable_text_from_event_script(items, route.list[k].parameters[0]);
										}
									}
								}
							}
							if (neednewline && lastitem != "") {
								items.Add(lastitem);
								lastitem = "";
							}
						}
					}
				}
				if (System.uptime - t >= 5) {
					t += 5;
					Graphics.update;
				}
				items |= new List<string>();
				choices |= new List<string>();
				items.concat(choices);
				if (items.length > 0) MessageTypes.setMapMessagesAsHash(id, items);
				if (System.uptime - t >= 5) {
					t += 5;
					Graphics.update;
				}
			}
		rescue Hangup;
		}
		Graphics.update;
	}

	public void find_translatable_text_from_RGSS_script(items, script) {
		script.force_encoding(Encoding.UTF_8);
		foreach (var s in script.scan(/(?:_INTL|_ISPRINTF)\s*\(\s*\"((?:[^\\\"]*\\\"?)*[^\"]*)\"/)) { //script.scan(/(?:_INTL|_ISPRINTF)\s*\(\s*\"((?:[^\\\"]*\\\"?)*[^\"]*)\"/) do => |s|
			string = s[0];
			string = System.Text.RegularExpressions.Regex.Replace(string, "\\r", "\r");
			string = System.Text.RegularExpressions.Regex.Replace(string, "\\n", "\n");
			string = System.Text.RegularExpressions.Regex.Replace(string, "\\1", "\1");
			string = System.Text.RegularExpressions.Regex.Replace(string, "\\\"", "\"");
			string = System.Text.RegularExpressions.Regex.Replace(string, "\\\\", "\\");
			items.Add(string);
		}
	}

	public void find_translatable_text_from_event_script(items, script) {
		script.force_encoding(Encoding.UTF_8);
		foreach (var s in script.scan(/(?:_I)\s*\(\s*\"((?:[^\\\"]*\\\"?)*[^\"]*)\"/)) { //script.scan(/(?:_I)\s*\(\s*\"((?:[^\\\"]*\\\"?)*[^\"]*)\"/) do => |s|
			string = s[0];
			string = System.Text.RegularExpressions.Regex.Replace(string, "\\\"", "\"");
			string = System.Text.RegularExpressions.Regex.Replace(string, "\\\\", "\\");
			items.Add(string);
		}
	}

	//-----------------------------------------------------------------------------

	public void normalize_value(value) {
		if (System.Text.RegularExpressions.Regex.IsMatch(value,@"[\r\n\t\x01]|^[\[\]]")) {
			ret = value.dup;
			ret = System.Text.RegularExpressions.Regex.Replace(ret, "\r", "<<r>>");
			ret = System.Text.RegularExpressions.Regex.Replace(ret, "\n", "<<n>>");
			ret = System.Text.RegularExpressions.Regex.Replace(ret, "\t", "<<t>>");
			ret = System.Text.RegularExpressions.Regex.Replace(ret, "\x01", "<<1>>");
			ret = System.Text.RegularExpressions.Regex.Replace(ret, "^\[", "<<[>>");
			ret = System.Text.RegularExpressions.Regex.Replace(ret, "^\]", "<<]>>");
			return ret;
		}
		return value;
	}

	public void denormalize_value(value) {
		if (System.Text.RegularExpressions.Regex.IsMatch(value,@"<<[rnt1\[\]]>>")) {
			ret = value.dup;
			ret = System.Text.RegularExpressions.Regex.Replace(ret, "<<r>>", "\r");
			ret = System.Text.RegularExpressions.Regex.Replace(ret, "<<n>>", "\n");
			ret = System.Text.RegularExpressions.Regex.Replace(ret, "<<t>>", "\t");
			ret = System.Text.RegularExpressions.Regex.Replace(ret, "<<1>>", "\1");
			ret = System.Text.RegularExpressions.Regex.Replace(ret, "<<\[>>", "[");
			ret = System.Text.RegularExpressions.Regex.Replace(ret, "<<\]>>", "]");
			return ret;
		}
		return value;
	}

	//-----------------------------------------------------------------------------

	public void extract_text(language_name = "default", core_text = false, separate_map_files = false) {
		dir_name = string.Format("Text_{0}_{0}", language_name, (core_text) ? "core" : "game");
		msg_window = CreateMessageWindow;
		// Get text for extraction
		orig_messages = new Translation(language_name);
		if (core_text) {
			language_messages = orig_messages.core_messages;
			default_messages = orig_messages.default_core_messages;
			if (!default_messages || default_messages.length == 0) {
				MessageDisplay(msg_window, _INTL("The default core messages file \"messages_core.dat\" was not found."));
				DisposeMessageWindow(msg_window);
				return;
			}
		} else {
			language_messages = orig_messages.game_messages;
			default_messages = orig_messages.default_game_messages;
			if (!default_messages || default_messages.length == 0) {
				MessageDisplay(msg_window, _INTL("The default game messages file \"messages_game.dat\" was not found."));
				DisposeMessageWindow(msg_window);
				return;
			}
		}
		// Create folder for extracted text files, or delete existing text files from
		// existing destination folder
		if (Dir.safe(dir_name)) {
			has_files = false;
			foreach (var f in Dir.all(dir_name)) { //'Dir.all(dir_name).each' do => |f|
				has_files = true;
				break;
			}
			if (has_files && !ConfirmMessageSerious(_INTL("Replace all text files in folder '{1}'?", dir_name))) {
				DisposeMessageWindow(msg_window);
				return;
			}
			Dir.all(dir_name).each(f => File.delete(f));
		} else {
			Dir.create(dir_name);
		}
		// Create a lambda function that helps to write text files
		write_header = lambda do |f, with_line|
			f.write(0xEF.chr);
			f.write(0xBB.chr);
			f.write(0xBF.chr);
			f.write("# To localize this text for a particular language, please" + "\r\n");
			f.write("# translate every second line of this file." + "\r\n");
			if (with_line) f.write("\#-------------------------------\r\n");
		}
		// Extract the text
		MessageDisplay(msg_window, "\\ts[]" + _INTL("Extracting text, please wait.") + "\\wtnp[0]");
		// Get all the section IDs to cycle through
		max_section_id = default_messages.length;
		if (language_messages && language_messages.length > max_section_id) max_section_id = language_messages.length;
		for (int i = max_section_id; i < max_section_id; i++) { //for 'max_section_id' times do => |i|
			section_name = getConstantName(MessageTypes, i, false);
			if (!section_name) continue;
			if (i == MessageTypes.EVENT_TEXTS) {
				if (separate_map_files) {
					map_infos = LoadMapInfos;
					default_messages[i].each_with_index do |map_msgs, map_id|
						if (!map_msgs || map_msgs.length == 0) continue;
						filename = string.Format("Map{0:3}", map_id);
						if (map_infos[map_id]) filename += " " + map_infos[map_id].name;
						File.open(dir_name + "/" + filename + ".txt", "wb") do |f|
							write_header.call(f, true);
							if (language_messages && language_messages[i]) translated_msgs = language_messages[i][map_id];
							write_section_texts_to_file(f, string.Format("Map{0:3}", map_id), translated_msgs, map_msgs);
						}
					}
				} else {
					if (!default_messages[i] || default_messages[i].length == 0) continue;
					no_difference = true;
					foreach (var map_msgs in default_messages[i]) { //'default_messages[i].each' do => |map_msgs|
						if (map_msgs && map_msgs.length > 0) no_difference = false;
						if (!map_msgs) break;
					}
					if (no_difference) continue;
					File.open(dir_name + "/" + section_name + ".txt", "wb") do |f|
						write_header.call(f, false);
						default_messages[i].each_with_index do |map_msgs, map_id|
							if (!map_msgs || map_msgs.length == 0) continue;
							f.write("\#-------------------------------\r\n");
							translated_msgs = (language_messages && language_messages[i]) ? language_messages[i][map_id] : null;
							write_section_texts_to_file(f, string.Format("Map{0:3}", map_id), translated_msgs, map_msgs);
						}
					}
				}
			} else {   // MessageTypes sections
				if (!default_messages[i] || default_messages[i].length == 0) continue;
				File.open(dir_name + "/" + section_name + ".txt", "wb") do |f|
					write_header.call(f, true);
					translated_msgs = (language_messages) ? language_messages[i] : null;
					write_section_texts_to_file(f, section_name, translated_msgs, default_messages[i]);
				}
			}
		}
		msg_window.textspeed = MessageConfig.SettingToTextSpeed(Game.GameData.PokemonSystem.textspeed);
		if (core_text) {
			MessageDisplay(msg_window, _INTL("All core text was extracted to files in the folder \"{1}\".", dir_name) + "\1");
		} else {
			MessageDisplay(msg_window, _INTL("All game text was extracted to files in the folder \"{1}\".", dir_name) + "\1");
		}
		MessageDisplay(msg_window, _INTL("To localize this text, translate every second line in those files.") + "\1");
		MessageDisplay(msg_window, _INTL("After translating, choose \"Compile Translated Text\" in the Debug menu."));
		DisposeMessageWindow(msg_window);
	}

	public void write_section_texts_to_file(f, section_name, language_msgs, original_msgs = null) {
		if (!original_msgs) return;
		switch (original_msgs) {
			case Array:
				f.write($"[{section_name}]\r\n");
				for (int j = original_msgs.length; j < original_msgs.length; j++) { //for 'original_msgs.length' times do => |j|
					if (nil_or_empty(original_msgs[j])) continue;
					f.write($"{j}\r\n");
					f.write(normalize_value(original_msgs[j]) + "\r\n");
					text = (language_msgs && language_msgs[j]) ? language_msgs[j] : original_msgs[j];
					f.write(normalize_value(text) + "\r\n");
				}
				break;
			case Hash:
				f.write($"[{section_name}]\r\n");
				keys = original_msgs.keys;
				foreach (var key in keys) { //'keys.each' do => |key|
					if (nil_or_empty(original_msgs[key])) continue;
					f.write(normalize_value(key) + "\r\n");
					text = (language_msgs && language_msgs[key]) ? language_msgs[key] : original_msgs[key];
					f.write(normalize_value(text) + "\r\n");
				}
				break;
		}
	}

	//-----------------------------------------------------------------------------

	public void compile_text(dir_name, dat_filename) {
		msg_window = CreateMessageWindow;
		MessageDisplay(msg_window, "\\ts[]" + _INTL("Compiling text, please wait.") + "\\wtnp[0]");
		outfile = File.open("Data/messages_" + dat_filename + ".dat", "wb");
		all_text = new List<string>();
		begin;
			text_files = Dir.get("Text_" + dir_name, "*.txt");
			text_files.each(file => compile_text_from_file(file, all_text));
			Marshal.dump(all_text, outfile);
		rescue;
			Debug.LogError($"Exception Error Thrown on '{System.Reflection.MethodBase.GetCurrentMethod().Name}'");
			//throw new Exception();
		ensure;
			outfile.close;
		}
		msg_window.textspeed = MessageConfig.SettingToTextSpeed(Game.GameData.PokemonSystem.textspeed);
		MessageDisplay(msg_window,
			_INTL("Text files in the folder \"Text_{1}\" were successfully compiled into file \"Data/messages_{2}.dat\".", dir_name, dat_filename));
		MessageDisplay(msg_window, _INTL("You may need to close the game to see any changes to messages."));
		DisposeMessageWindow(msg_window);
	}

	public void compile_text_from_file(text_file, all_text) {
		begin;
			file = File.open(text_file, "rb");
		rescue;
			Debug.LogError(_INTL("Can't find or open '{1}'.", text_file));
			//throw new ArgumentException(_INTL("Can't find or open '{1}'.", text_file));
		}
		begin;
			Compiler.EachSection(file) do |contents, section_name|
				if (contents.length == 0) continue;
				// Get the section number and whether the section contains a map's event text
				section_id = -1;
				is_map = false;
				if (section_name.ToInt() != 0) {   // Section name is a number
					section_id = section_name.ToInt();
				} else if (hasConst(MessageTypes, section_name)) {   // Section name is a constant from MessageTypes
					section_id = getConst(MessageTypes, section_name);
				} else if (System.Text.RegularExpressions.Regex.IsMatch(section_name,@"^Map(\d+)$",RegexOptions.IgnoreCase)) {   // Section name is a map number (event text)
					is_map = true;
					section_id = $~[1].ToInt();
				}
				if (section_id < 0) raise _INTL("Invalid section name {1}", section_name);
				// Decide whether the section contains text stored in an ordered list (an
				// array) or an ordered hash
				item_length = 0;
				if (System.Text.RegularExpressions.Regex.IsMatch(contents[0],@"^\d+$")) {   // If first line is a number, text is stored in an array
					text_hash = new List<string>();
					item_length = 3;
					if (is_map) {
						Debug.LogError(_INTL("Section {1} can't be an ordered list (section was recognized as an ordered list because its first line is a number).", section_name));
						//throw new ArgumentException(_INTL("Section {1} can't be an ordered list (section was recognized as an ordered list because its first line is a number).", section_name));
					}
					if (contents.length % 3 != 0) {
						Debug.LogError(_INTL("Section {1}'s line count is not divisible by 3 (section was recognized as an ordered list because its first line is a number).", section_name));
						//throw new ArgumentException(_INTL("Section {1}'s line count is not divisible by 3 (section was recognized as an ordered list because its first line is a number).", section_name));
					}
				} else {   // Text is stored in a hash
					text_hash = new List<string>();
					item_length = 2;
					if (contents.length.odd()) {
						Debug.LogError(_INTL("Section {1} has an odd number of entries (section was recognized as a hash because its first line is not a number).", section_name));
						//throw new ArgumentException(_INTL("Section {1} has an odd number of entries (section was recognized as a hash because its first line is not a number).", section_name));
					}
				}
				// Add text in section to ordered list/hash
				i = 0;
				do { //loop; while (true);
					if (item_length == 3) {
						if (!System.Text.RegularExpressions.Regex.IsMatch(contents[i],@"^\d+$")) {
							Debug.LogError(_INTL("Expected a number in section {1}, got {2} instead", section_name, contents[i]));
							//throw new ArgumentException(_INTL("Expected a number in section {1}, got {2} instead", section_name, contents[i]));
						}
						key = contents[i].ToInt();
						i += 1;
					} else {
						key = denormalize_value(contents[i]);
						key = Translation.stringToKey(key);
					}
					text_hash[key] = denormalize_value(contents[i + 1]);
					i += 2;
					if (i >= contents.length) break;
				}
				// Add ordered list/hash (text_hash) to array of all text (all_text)
				if (is_map && !all_text[MessageTypes.EVENT_TEXTS]) all_text[MessageTypes.EVENT_TEXTS] = new List<string>();
				target_section = (is_map) ? all_text[MessageTypes.EVENT_TEXTS][section_id] : all_text[section_id];
				if (target_section) {
					if (text_hash.is_a(Hash)) {
						text_hash.each_key(key => { if (text_hash[key]) target_section[key] = text_hash[key]; });
					} else {   // text_hash is an array
						text_hash.each_with_index((line, j) => { if (line) target_section[j] = line; });
					}
				} else if (is_map) {
					all_text[MessageTypes.EVENT_TEXTS][section_id] = text_hash;
				} else {
					all_text[section_id] = text_hash;
				}
			}
		ensure;
			file.close;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class Translation {
	public int core_messages		{ get { return _core_messages; } set { _core_messages = value; } }			protected int _core_messages;
	public int game_messages		{ get { return _game_messages; } }			protected int _game_messages;

	public static void stringToKey(str) {
		if (str && System.Text.RegularExpressions.Regex.IsMatch(str,@"[\r\n\t\1]|^\s+|\s+$|\s{2,}")) {
			key = str.clone;
			key = System.Text.RegularExpressions.Regex.Replace(key, "^\s+", "");
			key = System.Text.RegularExpressions.Regex.Replace(key, "\s+$", "");
			key = System.Text.RegularExpressions.Regex.Replace(key, "\s{2,}", " ");
			return key;
		}
		return str;
	}

	public void initialize(filename = null, delay_load = false) {
		@default_core_messages = null;
		@default_game_messages = null;
		@core_messages = null;   // A translation file
		@game_messages = null;   // A translation file
		@filename = filename;
		if (@filename && !delay_load) load_message_files(@filename);
	}

	public void default_core_messages() {
		load_default_messages;
		return @default_core_messages;
	}

	public void default_game_messages() {
		load_default_messages;
		return @default_game_messages;
	}

	public void load_message_files(filename) {
		@core_messages = null;
		@game_messages = null;
		begin;
			core_filename = string.Format("Data/messages_{0}_core.dat", filename);
			if (FileTest.exist(core_filename)) {
				@core_messages = load_data(core_filename);
				if (!@core_messages.Length > 0) @core_messages = null;
			}
			game_filename = string.Format("Data/messages_{0}_game.dat", filename);
			if (FileTest.exist(game_filename)) {
				@game_messages = load_data(game_filename);
				if (!@game_messages.Length > 0) @game_messages = null;
			}
		rescue;
			@core_messages = null;
			@game_messages = null;
		}
	}

	public void load_default_messages() {
		if (@default_core_messages) return;
		begin;
			if (FileTest.exist("Data/messages_core.dat")) {
				RgssOpen("Data/messages_core.dat", "rb", f => { @default_core_messages = Marshal.load(f); });
			}
			if (!@default_core_messages.Length > 0) @default_core_messages = new List<string>();
			if (FileTest.exist("Data/messages_game.dat")) {
				RgssOpen("Data/messages_game.dat", "rb", f => { @default_game_messages = Marshal.load(f); });
			}
			if (!@default_game_messages.Length > 0) @default_game_messages = new List<string>();
		rescue;
			@default_core_messages = new List<string>();
			@default_game_messages = new List<string>();
		}
	}

	public void save_default_messages() {
		File.open("Data/messages_core.dat", "wb", f => { Marshal.dump(@default_core_messages, f); });
		File.open("Data/messages_game.dat", "wb", f => { Marshal.dump(@default_game_messages, f); });
	}

	public void setMessages(type, array) {
		load_default_messages;
		@default_game_messages[type] = priv_add_to_array(type, array, null);
	}

	public void addMessages(type, array) {
		load_default_messages;
		@default_game_messages[type] = priv_add_to_array(type, array, @default_game_messages[type]);
	}

	public void setMessagesAsHash(type, array) {
		load_default_messages;
		@default_game_messages[type] = priv_add_to_hash(type, array, null);
	}

	public void addMessagesAsHash(type, array) {
		load_default_messages;
		@default_game_messages[type] = priv_add_to_hash(type, array, @default_game_messages[type]);
	}

	public void setMapMessagesAsHash(map_id, array) {
		load_default_messages;
		@default_game_messages[MessageTypes.EVENT_TEXTS] ||= new List<string>();
		@default_game_messages[MessageTypes.EVENT_TEXTS][map_id] = priv_add_to_hash(
			MessageTypes.EVENT_TEXTS, array, null, map_id
		);
	}

	public void addMapMessagesAsHash(map_id, array) {
		load_default_messages;
		@default_game_messages[MessageTypes.EVENT_TEXTS] ||= new List<string>();
		@default_game_messages[MessageTypes.EVENT_TEXTS][map_id] = priv_add_to_hash(
			MessageTypes.EVENT_TEXTS, array, @default_game_messages[MessageTypes.EVENT_TEXTS][map_id], map_id
		);
	}

	public void get(type, id) {
		delayed_load_message_files;
		if (@game_messages && @game_messages[type] && @game_messages[type][id]) {
			return @game_messages[type][id];
		}
		if (@core_messages && @core_messages[type] && @core_messages[type][id]) {
			return @core_messages[type][id];
		}
		return "";
	}

	public void getFromHash(type, text) {
		delayed_load_message_files;
		key = Translation.stringToKey(text);
		if (nil_or_empty(key)) return text;
		if (@game_messages && @game_messages[type] && @game_messages[type][key]) {
			return @game_messages[type][key];
		}
		if (@core_messages && @core_messages[type] && @core_messages[type][key]) {
			return @core_messages[type][key];
		}
		return text;
	}

	public void getFromMapHash(map_id, text) {
		delayed_load_message_files;
		key = Translation.stringToKey(text);
		if (nil_or_empty(key)) return text;
		if (@game_messages && @game_messages[MessageTypes.EVENT_TEXTS]) {
			if (@game_messages[MessageTypes.EVENT_TEXTS][map_id] && @game_messages[MessageTypes.EVENT_TEXTS][map_id][key]) {
				return @game_messages[MessageTypes.EVENT_TEXTS][map_id][key];
			} else if (@game_messages[MessageTypes.EVENT_TEXTS][0] && @game_messages[MessageTypes.EVENT_TEXTS][0][key]) {
				return @game_messages[MessageTypes.EVENT_TEXTS][0][key];
			}
		}
		if (@core_messages && @core_messages[MessageTypes.EVENT_TEXTS]) {
			if (@core_messages[MessageTypes.EVENT_TEXTS][map_id] && @core_messages[MessageTypes.EVENT_TEXTS][map_id][key]) {
				return @core_messages[MessageTypes.EVENT_TEXTS][map_id][key];
			} else if (@core_messages[MessageTypes.EVENT_TEXTS][0] && @core_messages[MessageTypes.EVENT_TEXTS][0][key]) {
				return @core_messages[MessageTypes.EVENT_TEXTS][0][key];
			}
		}
		return text;
	}

	//-----------------------------------------------------------------------------

	private;

	public void delayed_load_message_files() {
		if (!@filename || @core_messages) return;
		load_message_files(@filename);
		@filename = null;
	}

	public void priv_add_to_array(type, array, ret) {
		@default_core_messages[type] ||= new List<string>();
		if (!ret) ret = new List<string>();
		array.each_with_index do |text, i|
			if (!nil_or_empty(text) && @default_core_messages[type][i] != text) ret[i] = text;
		}
		return ret;
	}

	public void priv_add_to_hash(type, array, ret, map_id = 0) {
		if (type == MessageTypes.EVENT_TEXTS) {
			@default_core_messages[type] ||= new List<string>();
			@default_core_messages[type][map_id] ||= new List<string>();
			default_keys = @default_core_messages[type][map_id].keys;
		} else {
			@default_core_messages[type] ||= new List<string>();
			default_keys = @default_core_messages[type].keys;
		}
		if (!ret) ret = new List<string>();
		foreach (var text in array) { //'array.each' do => |text|
			if (!text) continue;
			key = Translation.stringToKey(text);
			if (!default_keys.Contains(key)) ret[key] = text;
		}
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public static partial class MessageTypes {
	// NOTE: These constants aren't numbered in any particular order, but these
	//       numbers are retained for backwards compatibility with older extracted
	//       text files.
	public const int EVENT_TEXTS                  = 0;   // Used for text in both common events and map events
	public const int SPECIES_NAMES                = 1;
	public const int SPECIES_CATEGORIES           = 2;
	public const int POKEDEX_ENTRIES              = 3;
	public const int SPECIES_FORM_NAMES           = 4;
	public const int MOVE_NAMES                   = 5;
	public const int MOVE_DESCRIPTIONS            = 6;
	public const int ITEM_NAMES                   = 7;
	public const int ITEM_NAME_PLURALS            = 8;
	public const int ITEM_DESCRIPTIONS            = 9;
	public const int ABILITY_NAMES                = 10;
	public const int ABILITY_DESCRIPTIONS         = 11;
	public const int TYPE_NAMES                   = 12;
	public const int TRAINER_TYPE_NAMES           = 13;
	public const int TRAINER_NAMES                = 14;
	public const int FRONTIER_INTRO_SPEECHES      = 15;
	public const int FRONTIER_END_SPEECHES_WIN    = 16;
	public const int FRONTIER_END_SPEECHES_LOSE   = 17;
	public const int REGION_NAMES                 = 18;
	public const int REGION_LOCATION_NAMES        = 19;
	public const int REGION_LOCATION_DESCRIPTIONS = 20;
	public const int MAP_NAMES                    = 21;
	public const int PHONE_MESSAGES               = 22;
	public const int TRAINER_SPEECHES_LOSE        = 23;
	public const int SCRIPT_TEXTS                 = 24;
	public const int RIBBON_NAMES                 = 25;
	public const int RIBBON_DESCRIPTIONS          = 26;
	public const int STORAGE_CREATOR_NAME         = 27;
	public const int ITEM_PORTION_NAMES           = 28;
	public const int ITEM_PORTION_NAME_PLURALS    = 29;
	public const int POKEMON_NICKNAMES            = 30;
	@@messages = new Translation();

	#region Class Functions
	#endregion

	public void load_default_messages() {
		@@messages.load_default_messages;
	}

	public void load_message_files(filename) {
		@@messages.load_message_files(filename);
	}

	public void save_default_messages() {
		@@messages.save_default_messages;
	}

	public void setMessages(type, array) {
		@@messages.setMessages(type, array);
	}

	public void addMessages(type, array) {
		@@messages.addMessages(type, array);
	}

	public void setMessagesAsHash(type, array) {
		@@messages.setMessagesAsHash(type, array);
	}

	public void addMessagesAsHash(type, array) {
		@@messages.addMessagesAsHash(type, array);
	}

	public void setMapMessagesAsHash(type, array) {
		@@messages.setMapMessagesAsHash(type, array);
	}

	public void addMapMessagesAsHash(type, array) {
		@@messages.addMapMessagesAsHash(type, array);
	}

	public void get(type, id) {
		return @@messages.get(type, id);
	}

	public void getFromHash(type, key) {
		return @@messages.getFromHash(type, key);
	}

	public void getFromMapHash(type, key) {
		return @@messages.getFromMapHash(type, key);
	}
}

//===============================================================================
//
//===============================================================================
public void GetMessage(type, id) {
	return MessageTypes.get(type, id);
}

public void GetMessageFromHash(type, id) {
	return MessageTypes.getFromHash(type, id);
}

// Replaces first argument with a localized version and formats the other
// parameters by replacing {1}, {2}, etc. with those placeholders.
public void _INTL(*arg) {
	begin;
		string = MessageTypes.getFromHash(MessageTypes.SCRIPT_TEXTS, arg[0]);
	rescue;
		string = arg[0];
	}
	string = string.clone;
	for (int i = 1; i < arg.length; i++) { //each 'arg.length' do => |i|
		string = System.Text.RegularExpressions.Regex.Replace(string, $"\{{i}\}", arg[i].ToString());
	}
	return string;
}

// Replaces first argument with a localized version and formats the other
// parameters by replacing {1}, {2}, etc. with those placeholders.
// This version acts more like sprintf, supports e.g. {1:d} or {2:s}
public void string.Format(*arg) {
	begin;
		string = MessageTypes.getFromHash(MessageTypes.SCRIPT_TEXTS, arg[0]);
	rescue;
		string = arg[0];
	}
	string = string.clone;
	for (int i = 1; i < arg.length; i++) { //each 'arg.length' do => |i|
		string = System.Text.RegularExpressions.Regex.Replace(string, $"\{{i}\:([^\}]+?)\}", m => string.Format("%" + Game.GameData.1, arg[i]));
	}
	return string;
}

public void _I(str, *arg) {
	return _MAPINTL(Game.GameData.game_map.map_id, str, *arg);
}

public void _MAPINTL(mapid, *arg) {
	string = MessageTypes.getFromMapHash(mapid, arg[0]);
	string = string.clone;
	for (int i = 1; i < arg.length; i++) { //each 'arg.length' do => |i|
		string = System.Text.RegularExpressions.Regex.Replace(string, $"\{{i}\}", arg[i].ToString());
	}
	return string;
}

public void _MAPISPRINTF(mapid, *arg) {
	string = MessageTypes.getFromMapHash(mapid, arg[0]);
	string = string.clone;
	for (int i = 1; i < arg.length; i++) { //each 'arg.length' do => |i|
		string = System.Text.RegularExpressions.Regex.Replace(string, $"\{{i}\:([^\}]+?)\}", m => string.Format("%" + Game.GameData.1, arg[i]));
	}
	return string;
}
