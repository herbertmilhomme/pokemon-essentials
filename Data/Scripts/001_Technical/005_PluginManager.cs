//==============================================================================#
//                                Plugin Manager                                #
//                                   by Marin                                   #
//               Support for external plugin scripts by Luka S.J.               #
//                              Tweaked by Maruno                               #
//------------------------------------------------------------------------------#
//   Provides a simple interface that allows plugins to require dependencies    #
//   at specific versions, and to specify incompatibilities between plugins.    #
//                                                                              #
//    Supports external scripts that are in .rb files in folders within the     #
//                               Plugins folder.                                #
//------------------------------------------------------------------------------#
//                                   Usage:                                     #
//                                                                              #
// Each plugin should have its own folder in the "Plugins" folder found in the  #
// main directory. The "Plugins" folder is similar in concept to the "PBS"      #
// folder, in that its contents are compiled and recorded as existing. The      #
// plugin's script file(s) are placed in its folder - they must be .rb files.   #
//                                                                              #
// A plugin's folder must also contain a "meta.txt" file. This file is what     #
// makes Essentials recognise that the plugin exists, and contains important    #
// information about the plugin; if this file does not exist, the folder's      #
// contents are ignored. Each line in this file is a property.                  #
//                                                                              #
// Required lines:                                                              #
//                                                                              #
//     Name       = Simple Extension                          The plugin's name #
//     Version    = 1.0                                    The plugin's version #
//     Essentials = 19.1,20                 Compatible version(s) of Essentials #
//     Link       = https://reliccastle.com/link-to-the-plugin/                 #
//     Credits    = Luka S.J.,Maruno,Marin                    One or more names #
//                                                                              #
// A plugin's version should be in the format X or X.Y or X.Y.Z, where X/Y/Z    #
// are numbers. You can also use Xa, Xb, Xc, Ya, etc. What matters is that you  #
// use version numbers consistently for your plugin. A later version will be    #
// alphanumerically higher than an older version.                               #
//                                                                              #
// Plugins can interact with each other in several ways, such as requiring      #
// another one to exist or by clashing with each other. These interactions are  #
// known as dependencies and conflicts. The lines below are all optional, and   #
// go in "meta.txt" to define how your plugin works (or doesn't work) with      #
// others. You can have multiples of each of these lines.                       #
//                                                                              #
//     Requires   = Basic Plugin            Must have this plugin (any version) #
//     Requires   = Useful Utils,1.1         Must have this plugin/min. version #
//     Exact      = Scene Tweaks,2                Must have this plugin/version #
//     Optional   = Extended Windows,1.2   If this plugin exists, load it first #
//     Conflicts  = Complex Extension                       Incompatible plugin #
//                                                                              #
// A plugin that depends on another one ("Requires"/"Exact"/"Optional") will    #
// make that other plugin be loaded first. The "Optional" line is for a plugin  #
// which isn't necessary, but if it does exist in the same project, it must be  #
// at the given version or higher.                                              #
//                                                                              #
// When plugins are compiled, their scripts are stored in the file              #
// "PluginScripts.rxdata" in the "Data" folder. Dependencies defined above will #
// ensure that they are loaded in a suitable order. Scripts within a plugin are #
// loaded alphanumerically, going through subfolders depth-first.               #
//                                                                              #
// The "Plugins" folder should be deleted when the game is released. Scripts in #
// there are compiled, but any other files used by a plugin (graphics/audio)    #
// should go into other folders and not the plugin's folder.                    #
//                                                                              #
//------------------------------------------------------------------------------#
//                           The code behind plugins:                           #
//                                                                              #
// When a plugin's "meta.txt" file is read, its contents are registered in the  #
// PluginManager. A simple example of registering a plugin is as follows:       #
//                                                                              #
//     PluginManager.register({                                                 #
//       name       = "Basic Plugin",                                         #
//       version    = "1.0",                                                  #
//       essentials = "20",                                                   #
//       link       = "https://reliccastle.com/link-to-the-plugin/",          #
//       credits    = ["Marin"]                                               #
//     })                                                                       #
//                                                                              #
// The :link value is optional, but recommended. This will be shown in the      #
// message if the PluginManager detects that this plugin needs to be updated.   #
//                                                                              #
// Here is the same example but also with dependencies and conflicts:           #
//                                                                              #
//     PluginManager.register({                                                 #
//       name       = "Basic Plugin",                                         #
//       version    = "1.0",                                                  #
//       essentials = "20",                                                   #
//       link       = "https://reliccastle.com/link-to-the-plugin/",          #
//       credits    = ["Marin"],                                              #
//       dependencies = new {"Basic Plugin",                                      #
//                         new {"Useful Utils", "1.1"},                             #
//                         new {:exact, "Scene Tweaks", "2"},                       #
//                         new {:optional, "Extended Windows", "1.2"},              #
//                        },                                                    #
//       incompatibilities = ["Simple Extension"]                             #
//     })                                                                       #
//                                                                              #
// The example dependencies/conflict are the same as the examples shown above   #
// for lines in "meta.txt". :optional_exact is a combination of :exact and      #
// :optional, and there is no way to make use of its combined functionality via #
// "meta.txt".                                                                  #
//                                                                              #
//------------------------------------------------------------------------------#
//                     Please give credit when using this.                      #
//==============================================================================#
public static partial class PluginManager {
	// Holds all registered plugin data.
	@@Plugins = new List<string>();

	// Registers a plugin and tests its dependencies and incompatibilities.
	public static void register(options) {
		name         = null;
		version      = null;
		essentials   = null;
		link         = null;
		dependencies = null;
		incompats    = null;
		credits      = new List<string>();
		order = new {:name, :version, :essentials, :link, :dependencies, :incompatibilities, :credits};
		// Ensure it first reads the plugin's name, which is used in error reporting,
		// by sorting the keys
		keys = options.keys.sort do |a, b|
			idx_a = order.index(a) || order.size;
			idx_b = order.index(b) || order.size;
			next idx_a <=> idx_b;
		}
		foreach (var key in keys) { //'keys.each' do => |key|
			value = options[key];
			switch (key) {
				case :name:   // Plugin name
					if (nil_or_empty(value)) {
						self.error("Plugin name must be a non-empty string.");
					}
					if (!@@Plugins[value].null()) {
						self.error($"A plugin called '{value}' already exists.");
					}
					name = value;
					break;
				case :version:   // Plugin version
					if (nil_or_empty(value)) self.error("Plugin version must be a string.");
					version = value;
					break;
				case :essentials:
					essentials = value;
					break;
				case :link:   // Plugin website
					if (nil_or_empty(value)) {
						self.error("Plugin link must be a non-empty string.");
					}
					link = value;
					break;
				case :dependencies:   // Plugin dependencies
					dependencies = value;
					if (!dependencies.Length > 0 || !dependencies[0].Length > 0) dependencies = [dependencies];
					foreach (var dep in value) { //'value.each' do => |dep|
						switch (dep) {
							case String:   // "plugin name"
								if (!self.installed(dep)) {
									self.error($"Plugin '{name}' requires plugin '{dep}' to be installed above it.");
								}
								break;
							case Array:
								switch (dep.size) {
									case 1:   // ["plugin name"]
										if (dep[0].is_a(String)) {
											dep_name = dep[0];
											if (!self.installed(dep_name)) {
												self.error($"Plugin '{name}' requires plugin '{dep_name}' to be installed above it.");
											}
										} else {
											self.error($"Expected the plugin name as a string, but got {dep[0].inspect}.");
										}
										break;
									case 2:   // ["plugin name", "version"]
										if (dep[0].is_a(Symbol)) {
											self.error("A plugin version comparator symbol was given but no version was given.");
										} else if (dep[0].is_a(String) && dep[1].is_a(String)) {
											dep_name    = dep[0];
											dep_version = dep[1];
											if (self.installed(dep_name, dep_version)) continue;
											if (self.installed(dep_name)) {   // Have plugin but lower version
												msg = $"Plugin '{name}' requires plugin '{dep_name}' version {dep_version} or higher, " +;
															$"but the installed version is {self.version(dep_name)}.";
												dep_link = self.link(dep_name);
												if (dep_link) {
													msg += $"\r\nCheck {dep_link} for an update to plugin '{dep_name}'.";
												}
												self.error(msg);
											} else {   // Don't have plugin
												self.error($"Plugin '{name}' requires plugin '{dep_name}' version {dep_version} " +;
														"or higher to be installed above it.");
											}
										}
										break;
									case 3:   // [:optional/:exact/:optional_exact, "plugin name", "version"]
										if (!dep[0].is_a(Symbol)) {
											self.error($"Expected first dependency argument to be a symbol, but got {dep[0].inspect}.");
										}
										if (!dep[1].is_a(String)) {
											self.error($"Expected second dependency argument to be a plugin name, but got {dep[1].inspect}.");
										}
										if (!dep[2].is_a(String)) {
											self.error($"Expected third dependency argument to be the plugin version, but got {dep[2].inspect}.");
										}
										dep_arg     = dep[0];
										dep_name    = dep[1];
										dep_version = dep[2];
										optional    = false;
										exact       = false;
										switch (dep_arg) {
											case :optional:
												optional = true;
												break;
											case :exact:
												exact = true;
												break;
											case :optional_exact:
												optional = true;
												exact = true;
												break;
											default:
												self.error("Expected first dependency argument to be one of " +;
																	$":optional, :exact or :optional_exact, but got {dep_arg.inspect}.");
												break;
										}
										if (optional) {
											if (self.installed(dep_name) &&   // Have plugin but lower version
												!self.installed(dep_name, dep_version, exact)) {
												msg = $"Plugin '{name}' requires plugin '{dep_name}', if installed, to be version {dep_version}";
												if (!exact) msg << " or higher";
												msg << $", but the installed version was {self.version(dep_name)}.";
												dep_link = self.link(dep_name);
												if (dep_link) {
													msg << $"\r\nCheck {dep_link} for an update to plugin '{dep_name}'.";
												}
												self.error(msg);
											}
										} else if (!self.installed(dep_name, dep_version, exact)) {
											if (self.installed(dep_name)) {   // Have plugin but lower version
												msg = $"Plugin '{name}' requires plugin '{dep_name}' to be version {dep_version}";
												if (!exact) msg << " or later";
												msg << $", but the installed version was {self.version(dep_name)}.";
												dep_link = self.link(dep_name);
												if (dep_link) {
													msg << $"\r\nCheck {dep_link} for an update to plugin '{dep_name}'.";
												}
											} else {   // Don't have plugin
												msg = $"Plugin '{name}' requires plugin '{dep_name}' version {dep_version} ";
												if (!exact) msg << "or later ";
												msg << "to be installed above it.";
											}
											self.error(msg);
										}
										break;
								}
								break;
						}
					}
					break;
				case :incompatibilities:   // Plugin incompatibilities
					incompats = value;
					if (!incompats.Length > 0) incompats = [incompats];
					foreach (var incompat in incompats) { //'incompats.each' do => |incompat|
						if (self.installed(incompat)) {
							self.error($"Plugin '{name}' is incompatible with '{incompat}'. They cannot both be used at the same time.");
						}
					}
					break;
				case :credits: // Plugin credits
					if (value.is_a(String)) value = [value];
					if (value.Length > 0) {
						foreach (var entry in value) { //'value.each' do => |entry|
							if (entry.is_a(String)) {
								credits << entry;
							} else {
								self.error($"Plugin '{name}'s credits array contains a non-string value.");
							}
						}
					} else {
						self.error($"Plugin '{name}'s credits field must contain a string, or a string array.");
					}
					break;
				default:
					self.error($"Invalid plugin registry key '{key}'.");
					break;
			}
		}
		@@Plugins.each_value do |plugin|
			if (plugin.incompatibilities&.Contains(name)) {
				self.error($"Plugin '{plugin.name}' is incompatible with '{name}'. They cannot both be used at the same time.");
			}
		}
		// Add plugin to class variable
		@@Plugins[name] = {
			name              = name,
			version           = version,
			essentials        = essentials,
			link              = link,
			dependencies      = dependencies,
			incompatibilities = incompats,
			credits           = credits;
		}
	}

	// Throws a pure error message without stack trace or any other useless info.
	public static void error(msg) {
		Graphics.update;
		t = new Thread() do;
			Console.echo_error($"Plugin Error:\r\n{msg}");
			print($"Plugin Error:\r\n{msg}");
			Thread.exit;
		}
		while (t.status) {
			Graphics.update;
		}
		Kernel.exit! true;
	}

	// Returns true if the specified plugin is installed.
	// If the version is specified, this version is taken into account.
	// If mustequal is true, the version must be a match with the specified version.
	public static bool installed(plugin_name, plugin_version = null, mustequal = false) {
		plugin = @@Plugins[plugin_name];
		if (plugin.null()) return false;
		if (plugin_version.null()) return true;
		comparison = compare_versions(plugin.version, plugin_version);
		if (!mustequal && comparison >= 0) return true;
		if (mustequal && comparison == 0) return true;
	}

	// Returns the string names of all installed plugins.
	public static void plugins() {
		return @@Plugins.keys;
	}

	// Returns the installed version of the specified plugin.
	public static void version(plugin_name) {
		if (!installed(plugin_name)) return;
		return @@Plugins[plugin_name][:version];
	}

	// Returns the link of the specified plugin.
	public static void link(plugin_name) {
		if (!installed(plugin_name)) return;
		return @@Plugins[plugin_name][:link];
	}

	// Returns the credits of the specified plugin.
	public static void credits(plugin_name) {
		if (!installed(plugin_name)) return;
		return @@Plugins[plugin_name][:credits];
	}

	// Compares two versions given in string form. v1 should be the plugin version
	// you actually have, and v2 should be the minimum/desired plugin version.
	// Return values:
	//     1 if v1 is higher than v2
	//     0 if v1 is equal to v2
	//     -1 if v1 is lower than v2
	public static void compare_versions(v1, v2) {
		d1 = v1.chars;
		if (d1[0] == ".") d1.insert(0, "0");   // Turn ".123" into "0.123"
		while d1[-1] == ".";                 // Turn "123." into "123"
			d1 = d1[0..-2];
		}
		d2 = v2.chars;
		if (d2[0] == ".") d2.insert(0, "0");   // Turn ".123" into "0.123"
		while d2[-1] == ".";                 // Turn "123." into "123"
			d2 = d2[0..-2];
		}
		for (int i = (int)Math.Max(d1.size, d2.size); i < (int)Math.Max(d1.size, d2.size); i++) { //for '(int)Math.Max(d1.size, d2.size)' times do => |i|   // Compare each digit in turn
			c1 = d1[i];
			c2 = d2[i];
			if (c1) {
				if (!c2) return 1;
				if (c1.ToInt(16) > c2.ToInt(16)) return 1;
				if (c1.ToInt(16) < c2.ToInt(16)) return -1;
			} else if (c2) {
				return -1;
			}
		}
		return 0;
	}

	// Formats the error message
	public static void pluginErrorMsg(name, script) {
		e = $!;
		// begin message formatting
		message = $"[Pokémon Essentials version {Essentials.VERSION}]\r\n";
		message += $"{Essentials.ERROR_TEXT}\r\n";   // For third party scripts to add to
		message += $"Error in Plugin: [{name}]\r\n";
		message += $"Exception: {e.class}\r\n";
		message += "Message: ";
		message += e.message;
		// show last 10 lines of backtrace
		message += "\r\n\r\nBacktrace:\r\n";
		e.backtrace[0, 10].each { |i| message += $"{i}\r\n" };
		// output to log
		errorlog = "errorlog.txt";
		File.open(errorlog, "ab") do |f|
			f.write($"\r\n=================\r\n\r\n[{Time.now}]\r\n");
			f.write(message);
		}
		// format/censor the error log directory
		errorlogline = errorlog.gsub("/", "\\");
		errorlogline.sub!(Dir.pwd + "\\", "");
		errorlogline.sub!(GetUserName, "USERNAME");
		if (errorlogline.length > 20) errorlogline = "\r\n" + errorlogline;
		// output message
		print($"{message}\r\nThis exception was logged in {errorlogline}.\r\nHold Ctrl when closing this message to copy it to the clipboard.");
		// Give a ~500ms coyote time to start holding Control
		t = System.uptime;
		until System.uptime - t >= 0.5;
			Input.update;
			if (Input.press(Input.CTRL)) {
				Input.clipboard = message;
				break;
			}
		}
	}

	// Used to read the metadata file
	public static void readMeta(dir, file) {
		filename = $"{dir}/{file}";
		meta = new List<string>();
		// read file
		Compiler.CompilerEachPreppedLine(filename) do |line, line_no|
			// split line up into property name and values
			if (!System.Text.RegularExpressions.Regex.IsMatch(line,@"^\s*(\w+)\s*=\s*(.*)$")) {
				Debug.LogError(_INTL("Bad line syntax (expected syntax like XXX=YYY).") + "\n" + FileLineData.linereport);
				//throw new Exception(_INTL("Bad line syntax (expected syntax like XXX=YYY).") + "\n" + FileLineData.linereport);
			}
			property = $~[1].upcase;
			data = $~[2].split(",");
			data.each_with_index((value, i) => data[i] = value.strip);
			// begin formatting data hash
			switch (property) {
				case "ESSENTIALS":
					if (!meta.essentials) meta.essentials = new List<string>();
					data.each(ver => meta.essentials.Add(ver));
					break;
				case "REQUIRES":
					if (!meta.dependencies) meta.dependencies = new List<string>();
					if (data.length < 2) {   // No version given, just push name of plugin dependency
						meta.dependencies.Add(data[0]);
						continue;
					} else if (data.length == 2) {   // Push name and version of plugin dependency
						meta.dependencies.Add(new {data[0], data[1]});
					} else {   // Push dependency type, name and version of plugin dependency
						meta.dependencies.Add(new {data[2].downcase.to_sym, data[0], data[1]});
					}
					break;
				case "EXACT":
					if (data.length < 2) continue;   // Exact dependencies must have a version given; ignore if not
					if (!meta.dependencies) meta.dependencies = new List<string>();
					meta.dependencies.Add(new {:exact, data[0], data[1]});
					break;
				case "OPTIONAL":
					if (data.length < 2) continue;   // Optional dependencies must have a version given; ignore if not
					if (!meta.dependencies) meta.dependencies = new List<string>();
					meta.dependencies.Add(new {:optional, data[0], data[1]});
					break;
				case "CONFLICTS":
					if (!meta.incompatibilities) meta.incompatibilities = new List<string>();
					data.each(value => { if (value && !value.empty()) meta.incompatibilities.Add(value); });
					break;
				case "SCRIPTS":
					if (!meta.scripts) meta.scripts = new List<string>();
					data.each(scr => meta.scripts.Add(scr));
					break;
				case "CREDITS":
					meta.credits = data;
					break;
				case "LINK": case "WEBSITE":
					meta.link = data[0];
					break;
				default:
					meta[property.downcase.to_sym] = data[0];
					break;
			}
		}
		// generate a list of all script files to be loaded, in the order they are to
		// be loaded (files listed in the meta file are loaded first)
		if (!meta.scripts) meta.scripts = new List<string>();
		// get all script files from plugin Dir
		foreach (var fl in Dir.all(dir)) { //'Dir.all(dir).each' do => |fl|
			if (!fl.Contains(".rb")) continue;
			meta.scripts.Add(fl.gsub($"{dir}/", ""));
		}
		// ensure no duplicate script files are queued
		meta.scripts.uniq!;
		// return meta hash
		return meta;
	}

	// Get a list of all the plugin directories to inspect
	public static void listAll() {
		if (!Core.DEBUG || FileTest.exist("Game.rgssad") || !Dir.safe("Plugins")) return [];
		// get a list of all directories in the `Plugins/` folder
		dirs = new List<string>();
		Dir.get("Plugins").each(d => { if (Dir.safe(d)) dirs.Add(d); });
		// return all plugins
		return dirs;
	}

	// Catch any potential loop with dependencies and raise an error
	public static void validateDependencies(name, meta, og = null) {
		// exit if no registered dependency
		if (!meta[name] || !meta[name][:dependencies]) return null;
		if (!og) og = [name];
		// go through all dependencies
		foreach (var dname in meta[name][:dependencies]) { //'meta[name][:dependencies].each' do => |dname|
			// clean the name to a simple string
			if (dname.Length > 0 && dname.length == 2) dname = dname[0];
			if (dname.Length > 0 && dname.length == 3) dname = dname[1];
			// catch looping dependency issue
			if (!og.null() && og.Contains(dname)) self.error($"Plugin '{og[0]}' has looping dependencies which cannot be resolved automatically.");
			new_og = og.clone;
			new_og.Add(dname);
			self.validateDependencies(dname, meta, new_og);
		}
		return name;
	}

	// Sort load order based on dependencies (this ends up in reverse order)
	public static void sortLoadOrder(order, plugins) {
		// go through the load order
		foreach (var o in order) { //'order.each' do => |o|
			if (!plugins[o] || !plugins[o][:dependencies]) continue;
			// go through all dependencies
			foreach (var dname in plugins[o][:dependencies]) { //'plugins[o][:dependencies].each' do => |dname|
				optional = false;
				// clean the name to a simple string
				if (dname.Length > 0) {
					optional = new []{:optional, :optional_exact}.Contains(dname[0]);
					dname = dname[dname.length - 2];
				}
				// catch missing dependency
				if (!order.Contains(dname)) {
					if (optional) continue;
					self.error($"Plugin '{o}' requires plugin '{dname}' to work properly.");
				}
				// skip if already sorted
				if (order.index(dname) > order.index(o)) continue;
				// catch looping dependency issue
				order.swap(o, dname);
				order = self.sortLoadOrder(order, plugins);
			}
		}
		return order;
	}

	// Get the order in which to load plugins
	public static void getPluginOrder() {
		plugins = new List<string>();
		order = new List<string>();
		// Find all plugin folders that have a meta.txt and add them to the list of
		// plugins.
		foreach (var dir in self.listAll) { //'self.listAll.each' do => |dir|
			// skip if there is no meta file
			if (!FileTest.exist(dir + "/meta.txt")) continue;
			ndx = order.length;
			meta = self.readMeta(dir, "meta.txt");
			meta.dir = dir;
			// raise error if no name defined for plugin
			if (!meta.name) self.error($"No 'Name' metadata defined for plugin located at '{dir}'.");
			// raise error if no script defined for plugin
			if (!meta.scripts) self.error($"No 'Scripts' metadata defined for plugin located at '{dir}'.");
			plugins[meta.name] = meta;
			// raise error if a plugin with the same name already exists
			if (order.Contains(meta.name)) self.error($"A plugin called '{meta.name}' already exists in the load order.");
			order.insert(ndx, meta.name);
		}
		// validate all dependencies
		order.each(o => self.validateDependencies(o, plugins));
		// sort the load order
		return self.sortLoadOrder(order, plugins).reverse, plugins;
	}

	// Check if plugins need compiling
	public static bool needCompiling(order, plugins) {
		// fixed actions
		if (!Core.DEBUG || FileTest.exist("Game.rgssad")) return false;
		if (Game.GameData.full_compile) return true;
		if (!FileTest.exist("Data/PluginScripts.rxdata")) return true;
		Input.update;
		if (Input.press(Input.SHIFT) || Input.press(Input.CTRL)) return true;
		// analyze whether or not to push recompile
		mtime = File.mtime("Data/PluginScripts.rxdata");
		foreach (var o in order) { //'order.each' do => |o|
			// go through all the registered plugin scripts
			scr = plugins[o][:scripts];
			dir = plugins[o][:dir];
			foreach (var sc in scr) { //'scr.each' do => |sc|
				if (File.mtime($"{dir}/{sc}") > mtime) return true;
			}
			if (File.mtime($"{dir}/meta.txt") > mtime) return true;
		}
		return false;
	}

	// Check if plugins need compiling
	public static void compilePlugins(order, plugins) {
		Console.echo_li("Compiling plugin scripts...");
		scripts = new List<string>();
		// go through the entire order one by one
		foreach (var o in order) { //'order.each' do => |o|
			// save name, metadata and scripts array
			meta = plugins[o].clone;
			meta.delete(:scripts);
			meta.delete(:dir);
			dat = new {o, meta, new List<string>()};
			// iterate through each file to deflate
			foreach (var file in plugins[o][:scripts]) { //'plugins[o][:scripts].each' do => |file|
				File.open($"{plugins[o][:dir]}/{file}", "rb") do |f|
					dat[2].Add(new {file, Zlib.Deflate.deflate(f.read)});
				}
			}
			// push to the main scripts array
			scripts.Add(dat);
		}
		// save to main `PluginScripts.rxdata` file
		File.open("Data/PluginScripts.rxdata", "wb", f => { Marshal.dump(scripts, f); });
		// collect garbage
		GC.start;
		Console.echo_done(true);
	}

	// Check if plugins need compiling
	public static void runPlugins() {
		Console.echo_h1("Checking plugins");
		// get the order of plugins to interpret
		order, plugins = self.getPluginOrder;
		// compile if necessary
		if (self.needCompiling(order, plugins)) {
			self.compilePlugins(order, plugins);
		} else {
			Console.echoln_li("Plugins were not compiled");
		}
		// load plugins
		scripts = load_data("Data/PluginScripts.rxdata");
		echoed_plugins = new List<string>();
		foreach (var plugin in scripts) { //'scripts.each' do => |plugin|
			// get the required data
			name, meta, script = plugin;
			if (!meta.essentials || !meta.essentials.Contains(Essentials.VERSION)) {
				Console.echo_warn($"Plugin '{name}' may not be compatible with Essentials v{Essentials.VERSION}. Trying to load anyway.");
			}
			// register plugin
			self.register(meta);
			// go through each script and interpret
			foreach (var scr in script) { //'script.each' do => |scr|
				// turn code into plaintext
				code = Zlib.Inflate.inflate(scr[1]).force_encoding(Encoding.UTF_8);
				// get rid of tabs
				code.gsub!("\t", "  ");
				// construct filename
				sname = scr[0].gsub("\\", "/").split("/")[-1];
				fname = $"[{name}] {sname}";
				// try to run the code
				begin;
					eval(code, TOPLEVEL_BINDING, fname);
					if (!echoed_plugins.Contains(name)) Console.echoln_li($"Loaded plugin: =={name}== (ver. {meta.version})");
					echoed_plugins.Add(name);
				rescue Exception;   // format error message to display
					self.pluginErrorMsg(name, sname);
					Kernel.exit! true;
				}
			}
		}
		if (scripts.length > 0) {
			Console.echoln_li_done($"Successfully loaded {scripts.length} plugin(s)");
		} else {
			Console.echoln_li_done("No plugins found");
		}
	}

	// Get plugin dir from name based on meta entries
	public static void findDirectory(name) {
		// go through the plugins folder
		foreach (var dir in Dir.get("Plugins")) { //'Dir.get("Plugins").each' do => |dir|
			if (!Dir.safe(dir)) continue;
			if (!FileTest.exist(dir + "/meta.txt")) continue;
			// read meta
			meta = self.readMeta(dir, "meta.txt");
			if (meta.name == name) return dir;
		}
		// return null if no plugin dir found
		return null;
	}
}
