//===============================================================================
// To use the console, use the executable explicitly built with the console
// enabled on Windows. On Linux and macOS, just launch the executable directly
// from a terminal.
//===============================================================================
public static partial class Console {
	public static void setup_console() {
		unless (Core.DEBUG) return;
		echoln $"GPU Cache Max: {Bitmap.max_size}";
		echoln "-------------------------------------------------------------------------------";
		echoln $"{System.game_title} Output Window";
		echoln "-------------------------------------------------------------------------------";
		echoln "If you can see this window, you are running the game in Debug Mode. This means";
		echoln "that you're either playing a debug version of the game, or you're playing from";
		echoln "within RPG Maker XP.";
		echoln "";
		echoln "Closing this window will close the game. If you want to get rid of this window,";
		echoln "run the program from the Shell, or download a release version of the game.";
		echoln "-------------------------------------------------------------------------------";
		echoln "Debug Output:";
		echoln "-------------------------------------------------------------------------------";
		echoln "";
	}

	public static void readInput() {
		return gets.strip;
	}

	public static void readInput2() {
		return self.readInput;
	}

	public static void get_input() {
		echo self.readInput2;
	}
}

//===============================================================================
//
//===============================================================================
public static partial class Kernel {
	public void echo(string) {
		unless (Core.DEBUG) return;
		printf(string.is_a(String) ? string : string.inspect);
	}

	public void echoln(string) {
		echo string;
		echo "\n";
	}
}

Console.setup_console;

//===============================================================================
// Console message formatting
//===============================================================================
public static partial class Console {
	#region Class Functions
	#endregion

	//-----------------------------------------------------------------------------
	// echo string into console (example shorthand for common options)
	//-----------------------------------------------------------------------------

	// heading 1
	public void echo_h1(msg) {
		echoln markup_style($"*** {msg} ***", text: :brown);
		echoln "";
	}

	// heading 2
	public void echo_h2(msg, **options) {
		echoln markup_style(msg, **options);
		echoln "";
	}

	// heading 3
	public void echo_h3(msg) {
		echoln markup(msg);
		echoln "";
	}

	// list item
	public void echo_li(msg, pad = 0, color = :brown) {
		echo markup_style("  -> ", text: color);
		pad = (pad - msg.length) > 0 ? "." * (pad - msg.length) : "";
		echo markup(msg + pad);
	}

	// list item with line break after
	public void echoln_li(msg, pad = 0, color = :brown) {
		self.echo_li(msg, pad, color);
		echoln "";
	}

	// Same as echoln_li but text is in green
	public void echoln_li_done(msg) {
		self.echo_li(markup_style(msg, text: :green), 0, :green);
		echoln "";
		echoln "";
	}

	// paragraph with markup
	public void echo_p(msg) {
		echoln markup(msg);
	}

	// warning message
	public void echo_warn(msg) {
		echoln markup_style($"WARNING: {msg}", text: :yellow);
	}

	// error message
	public void echo_error(msg) {
		echoln markup_style($"ERROR: {msg}", text: :light_red);
	}

	// status output
	public void echo_status(status) {
		if (status) {
			echoln markup_style("OK", text: :green);
		} else {
			echoln markup_style("FAIL", text: :red);
		}
	}

	// completion output
	public void echo_done(status) {
		if (status) {
			echoln markup_style("done", text: :green);
		} else {
			echoln markup_style("error", text: :red);
		}
	}

	//-----------------------------------------------------------------------------
	// Markup options
	//-----------------------------------------------------------------------------

	public void string_colors() {
		{
			default: "38", black: "30", red: "31", green: "32", brown: "33",
			blue: "34", purple: "35", cyan: "36", gray: "37",
			dark_gray: "1;30", light_red: "1;31", light_green: "1;32", yellow: "1;33",
			light_blue: "1;34", light_purple: "1;35", light_cyan: "1;36", white: "1;37";
		}
	}

	public void background_colors() {
		{
			default: "0", black: "40", red: "41", green: "42", brown: "43",
			blue: "44", purple: "45", cyan: "46", gray: "47",
			dark_gray: "100", light_red: "101", light_green: "102", yellow: "103",
			light_blue: "104", light_purple: "105", light_cyan: "106", white: "107";
		}
	}

	public void font_options() {
		{
			bold: "1", dim: "2", italic: "3", underline: "4", reverse: "7",
			hidden: "8";
		}
	}

	// Text markup that turns text between them a certain color
	public void markup_colors() {
		{
			"`" => :cyan, '"' => :purple, "==" => :purple, "$" => :green, "~" => :red;
		}
	}

	public void markup_options() {
		{
			"__" => :underline, "*" => :bold, "|" => :italic;
		}
	}

	// apply console coloring
	public void markup_style(string, text: :default, bg: :default, **options) {
		// get colors
		code_text = string_colors[text];
		code_bg   = background_colors[bg];
		// get options
		options_pool = options.select((key, val) => font_options.key(key) && val);
		markup_pool  = options_pool.keys.map(opt => font_options[opt]).join(";").squeeze;
		// return formatted string
		return $"\e[{code_bg};{markup_pool};{code_text}m{string}\e[0m".squeeze(";");
	}

	//-----------------------------------------------------------------------------
	// Perform markup on text
	//-----------------------------------------------------------------------------

	public void markup_all_options() {
		@markup_all_options ||= markup_colors.merge(markup_options);
	}

	public void markup_component(string, component, key, options) {
		// trim inner markup content
		l = key.length;
		trimmed = component[l...-l];
		// merge markup options
		unless (options[trimmed]) options[trimmed] = new List<string>();
		foreach (var new_opt in options[trimmed].deep_merge!(new List<string>().tap) { //options[trimmed].deep_merge!(new List<string>().tap do => |new_opt|
			if (markup_colors.key(key)) new_opt.text = markup_colors[key];
			if (markup_options.key(key)) new_opt[markup_options[key]] = true;
		});
		// remove markup from input string
		string.gsub!(component, trimmed);
		// return output
		return string, options;
	}

	public void markup_breakdown(string, options = new List<string>()) {
		// iterate through all options
		foreach (var key in markup_all_options) { //markup_all_options.each_key do => |key|
			// ensure escape
			key_char = key.chars.map { |c| $"\\{c}" }.join;
			// define regex
			regex = $"{key_char}.*?{key_char}";
			// go through matches
			foreach (var component in string.scan(/#{regex}/)) { //'string.scan(/#{regex}/).each' do => |component|
				return *markup_breakdown(*markup_component(string, component, key, options));
			}
		}
		// return output
		return string, options;
	}

	public void markup(string) {
		// get a breakdown of all markup options
		string, options = markup_breakdown(string);
		// iterate through each option and apply
		options.each do |key, opt|
			string.gsub!(key, markup_style(key, **opt));
		}
		// return string
		return string;
	}
}
