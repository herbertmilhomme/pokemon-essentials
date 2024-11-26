//===============================================================================
// ** Interpreter
//-------------------------------------------------------------------------------
//  This interpreter runs event commands. This class is used within the
//  Game_System class and the Game_Event class.
//===============================================================================
public partial class Interpreter {
	//-----------------------------------------------------------------------------
	// * Event Command Execution
	//-----------------------------------------------------------------------------
	public void execute_command() {
		// Reached end of list of commands
		if (@index >= @list.size - 1) {
			command_end;
			return true;
		}
		// Make current command's parameters available for reference via @parameters
		@parameters = @list[@index].parameters;
		// Branch by command code
		switch (@list[@index].code) {
			case 101:  return command_101;   // Show Text
			case 102:  return command_102;   // Show Choices
			case 402:  return command_402;   // When [**]
			case 403:  return command_403;   // When Cancel
			case 103:  return command_103;   // Input Number
			case 104:  return command_104;   // Change Text Options
			case 105:  return command_105;   // Button Input Processing
			case 106:  return command_106;   // Wait
			case 111:  return command_111;   // Conditional Branch
			case 411:  return command_411;   // Else
			case 112:  return command_112;   // Loop
			case 413:  return command_413;   // Repeat Above
			case 113:  return command_113;   // Break Loop
			case 115:  return command_115;   // Exit Event Processing
			case 116:  return command_116;   // Erase Event
			case 117:  return command_117;   // Call Common Event
			case 118:  return command_118;   // Label
			case 119:  return command_119;   // Jump to Label
			case 121:  return command_121;   // Control Switches
			case 122:  return command_122;   // Control Variables
			case 123:  return command_123;   // Control Self Switch
			case 124:  return command_124;   // Control Timer
			case 125:  return command_125;   // Change Gold
			case 126:  return command_126;   // Change Items
			case 127:  return command_127;   // Change Weapons
			case 128:  return command_128;   // Change Armor
			case 129:  return command_129;   // Change Party Member
			case 131:  return command_131;   // Change Windowskin
			case 132:  return command_132;   // Change Battle BGM
			case 133:  return command_133;   // Change Battle End ME
			case 134:  return command_134;   // Change Save Access
			case 135:  return command_135;   // Change Menu Access
			case 136:  return command_136;   // Change Encounter
			case 201:  return command_201;   // Transfer Player
			case 202:  return command_202;   // Set Event Location
			case 203:  return command_203;   // Scroll Map
			case 204:  return command_204;   // Change Map Settings
			case 205:  return command_205;   // Change Fog Color Tone
			case 206:  return command_206;   // Change Fog Opacity
			case 207:  return command_207;   // Show Animation
			case 208:  return command_208;   // Change Transparent Flag
			case 209:  return command_209;   // Set Move Route
			case 210:  return command_210;   // Wait for Move's Completion
			case 221:  return command_221;   // Prepare for Transition
			case 222:  return command_222;   // Execute Transition
			case 223:  return command_223;   // Change Screen Color Tone
			case 224:  return command_224;   // Screen Flash
			case 225:  return command_225;   // Screen Shake
			case 231:  return command_231;   // Show Picture
			case 232:  return command_232;   // Move Picture
			case 233:  return command_233;   // Rotate Picture
			case 234:  return command_234;   // Change Picture Color Tone
			case 235:  return command_235;   // Erase Picture
			case 236:  return command_236;   // Set Weather Effects
			case 241:  return command_241;   // Play BGM
			case 242:  return command_242;   // Fade Out BGM
			case 245:  return command_245;   // Play BGS
			case 246:  return command_246;   // Fade Out BGS
			case 247:  return command_247;   // Memorize BGM/BGS
			case 248:  return command_248;   // Restore BGM/BGS
			case 249:  return command_249;   // Play ME
			case 250:  return command_250;   // Play SE
			case 251:  return command_251;   // Stop SE
			case 301:  return command_301;   // Battle Processing
			case 601:  return command_601;   // If Win
			case 602:  return command_602;   // If Escape
			case 603:  return command_603;   // If Lose
			case 302:  return command_302;   // Shop Processing
			case 303:  return command_303;   // Name Input Processing
			case 311:  return command_311;   // Change HP
			case 312:  return command_312;   // Change SP
			case 313:  return command_313;   // Change State
			case 314:  return command_314;   // Recover All
			case 315:  return command_315;   // Change EXP
			case 316:  return command_316;   // Change Level
			case 317:  return command_317;   // Change Parameters
			case 318:  return command_318;   // Change Skills
			case 319:  return command_319;   // Change Equipment
			case 320:  return command_320;   // Change Actor Name
			case 321:  return command_321;   // Change Actor Class
			case 322:  return command_322;   // Change Actor Graphic
			case 331:  return command_331;   // Change Enemy HP
			case 332:  return command_332;   // Change Enemy SP
			case 333:  return command_333;   // Change Enemy State
			case 334:  return command_334;   // Enemy Recover All
			case 335:  return command_335;   // Enemy Appearance
			case 336:  return command_336;   // Enemy Transform
			case 337:  return command_337;   // Show Battle Animation
			case 338:  return command_338;   // Deal Damage
			case 339:  return command_339;   // Force Action
			case 340:  return command_340;   // Abort Battle
			case 351:  return command_351;   // Call Menu Screen
			case 352:  return command_352;   // Call Save Screen
			case 353:  return command_353;   // Game Over
			case 354:  return command_354;   // Return to Title Screen
			case 355:  return command_355;   // Script
			default:          return true;          // Other
		}
	}

	public void command_dummy() {
		return true;
	}

	//-----------------------------------------------------------------------------
	// * End Event
	//-----------------------------------------------------------------------------
	public void command_end() {
		@list = null;
		end_follower_overrides;
		// If main map event and event ID are valid, unlock event
		if (@main && @event_id > 0 && Game.GameData.game_map.events[@event_id]) {
			Game.GameData.game_map.events[@event_id].unlock;
		}
	}

	//-----------------------------------------------------------------------------
	// * Command Skip
	//-----------------------------------------------------------------------------
	public void command_skip() {
		indent = @list[@index].indent;
		do { //loop; while (true);
			if (@list[@index + 1].indent == indent) return true;
			@index += 1;
		}
	}

	//-----------------------------------------------------------------------------
	// * Command If
	//-----------------------------------------------------------------------------
	public void command_if(value) {
		if (@branch[@list[@index].indent] == value) {
			@branch.delete(@list[@index].indent);
			return true;
		}
		return command_skip;
	}

	//-----------------------------------------------------------------------------
	// * Show Text
	//-----------------------------------------------------------------------------
	public void command_101() {
		if (Game.GameData.game_temp.message_window_showing) return false;
		message     = @list[@index].parameters[0];
		message_end = "";
		choices                 = null;
		number_input_variable   = null;
		number_input_max_digits = null;
		// Check the next command(s) for things to add on to this text
		do { //loop; while (true);
			next_index = NextIndex(@index);
			switch (@list[next_index].code) {
				case 401:   // Continuation of 101 Show Text
					text = @list[next_index].parameters[0];
					if (text != "" && message[message.length - 1, 1] != " ") message += " ";
					message += text;
					@index = next_index;
					continue;
					break;
				case 101:   // Show Text
					message_end = "\1";
					break;
				case 102:   // Show Choices
					@index = next_index;
					choices = setup_choices(@list[@index].parameters);
					break;
				case 103:   // Input Number
					number_input_variable   = @list[next_index].parameters[0];
					number_input_max_digits = @list[next_index].parameters[1];
					@index = next_index;
					break;
			}
			break;
		}
		// Translate the text
		message = _MAPINTL(Game.GameData.game_map.map_id, message);
		// Display the text, with choices/number choosing if appropriate
		@message_waiting = true;   // Lets parallel process events work while a message is displayed
		if (choices) {
			command = Message(message + message_end, choices[0], choices[1]);
			@branch[@list[@index].indent] = choices[2][command] || command;
		} else if (number_input_variable) {
			params = new ChooseNumberParams();
			params.setMaxDigits(number_input_max_digits);
			params.setDefaultValue(Game.GameData.game_variables[number_input_variable]);
			Game.GameData.game_variables[number_input_variable] = MessageChooseNumber(message + message_end, params);
			if (Game.GameData.game_map) Game.GameData.game_map.need_refresh = true;
		} else {
			Message(message + message_end);
		}
		@message_waiting = false;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Show Choices
	//-----------------------------------------------------------------------------
	public void command_102() {
		choices = setup_choices(@list[@index].parameters);
		@message_waiting = true;
		command = ShowCommands(null, choices[0], choices[1]);
		@message_waiting = false;
		@branch[@list[@index].indent] = choices[2][command] || command;
		Input.update;   // Must call Input.update again to avoid extra triggers
		return true;
	}

	public void setup_choices(params) {
		// Get initial options
		choices = params[0].clone;
		cancel_index = params[1];
		// Clone @list so the original isn't modified
		@list = Marshal.load(Marshal.dump(@list));
		// Get more choices
		@choice_branch_index = 4;
		ret = add_more_choices(choices, cancel_index, @index + 1, @list[@index].indent);
		// Rename choices
		ret[0].each_with_index((choice, i) => { if (@renamed_choices[i]) ret[0][i] = @renamed_choices[i]; });
		@renamed_choices.clear;
		// Remove hidden choices
		ret[2] = new Array(ret[0].length, i => { i; });
		@hidden_choices.each_with_index do |condition, i|
			if (!condition) continue;
			ret[0][i] = null;
			ret[2][i] = null;
		}
		ret[0].compact!;
		ret[2].compact!;
		@hidden_choices.clear;
		// Translate choices
		ret[0].map! { |ch| _MAPINTL(Game.GameData.game_map.map_id, ch) };
		return ret;
	}

	public void add_more_choices(choices, cancel_index, choice_index, indent) {
		// Find index of next command after the current Show Choices command
		do { //loop; while (true);
			if (@list[choice_index].indent == indent && !new []{402, 403, 404}.Contains(@list[choice_index].code)) break;
			choice_index += 1;
		}
		next_cmd = @list[choice_index];
		// If the next command isn't another Show Choices, we're done
		if (next_cmd.code != 102) return new {choices, cancel_index};
		// Add more choices
		old_length = choices.length;
		choices += next_cmd.parameters[0];
		// Update cancel option
		if (next_cmd.parameters[1] == 5) {   // Branch
			cancel_index = choices.length + 1;
			@choice_branch_index = cancel_index - 1;
		} else if (next_cmd.parameters[1] > 0) {   // A choice
			cancel_index = old_length + next_cmd.parameters[1];
			@choice_branch_index = -1;
		}
		// Update first Show Choices command to include all options and result of cancelling
		@list[@index].parameters[0] = choices;
		@list[@index].parameters[1] = cancel_index;
		// Find the "When" lines for this Show Choices command and update their index parameter
		temp_index = choice_index + 1;
		do { //loop; while (true);
			if (@list[temp_index].indent == indent && !new []{402, 403, 404}.Contains(@list[temp_index].code)) break;
			if (@list[temp_index].code == 402 && @list[temp_index].indent == indent) {
				@list[temp_index].parameters[0] += old_length;
			}
			temp_index += 1;
		}
		// Delete the "Show Choices" line
		@list.delete(next_cmd);
		// Find more choices to add
		return add_more_choices(choices, cancel_index, choice_index + 1, indent);
	}

	public void hide_choice(number, condition = true) {
		@hidden_choices[number - 1] = condition;
	}

	public void rename_choice(number, new_name, condition = true) {
		if (!condition || nil_or_empty(new_name)) return;
		@renamed_choices[number - 1] = new_name;
	}

	//-----------------------------------------------------------------------------
	// * When [**]
	//-----------------------------------------------------------------------------
	public void command_402() {
		// @parameters[0] is 0/1/2/3 for Choice 1/2/3/4 respectively
		if (@branch[@list[@index].indent] == @parameters[0]) {
			@branch.delete(@list[@index].indent);
			return true;
		}
		return command_skip;
	}

	//-----------------------------------------------------------------------------
	// * When Cancel
	//-----------------------------------------------------------------------------
	public void command_403() {
		// @parameters[0] is 4 for "Branch"
		if (@branch[@list[@index].indent] == @choice_branch_index) {
			@branch.delete(@list[@index].indent);
			return true;
		}
		return command_skip;
	}

	//-----------------------------------------------------------------------------
	// * Input Number
	//-----------------------------------------------------------------------------
	public void command_103() {
		@message_waiting = true;
		variable_number = @list[@index].parameters[0];
		params = new ChooseNumberParams();
		params.setMaxDigits(@list[@index].parameters[1]);
		params.setDefaultValue(Game.GameData.game_variables[variable_number]);
		Game.GameData.game_variables[variable_number] = ChooseNumber(null, params);
		if (Game.GameData.game_map) Game.GameData.game_map.need_refresh = true;
		@message_waiting = false;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Change Text Options
	//-----------------------------------------------------------------------------
	public void command_104() {
		if (Game.GameData.game_temp.message_window_showing) return false;
		Game.GameData.game_system.message_position = @parameters[0];
		Game.GameData.game_system.message_frame    = @parameters[1];
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Button Input Processing
	//-----------------------------------------------------------------------------
	public void ButtonInputProcessing(variable_number = 0, timeout_frames = 0) {
		ret = 0;
		timer_start = System.uptime;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			UpdateSceneMap;
			// Check for input and break if there is one
			(1..18).each(i => { if (Input.trigger(i)) ret = i; })
			if (ret != 0) break;
			// Break if the timer runs out
			if (timeout_frames > 0 && System.uptime - timer_start >= timeout_frames / 20.0) break;
		}
		Input.update;
		if (variable_number && variable_number > 0) {
			Game.GameData.game_variables[variable_number] = ret;
			if (Game.GameData.game_map) Game.GameData.game_map.need_refresh = true;
		}
		return ret;
	}

	public void command_105() {
		if (@buttonInput) return false;
		@buttonInput = true;
		ButtonInputProcessing(@list[@index].parameters[0]);
		@buttonInput = false;
		@index += 1;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Wait
	//-----------------------------------------------------------------------------
	public void command_106() {
		@wait_count = @parameters[0] / 20.0;
		@wait_start = System.uptime;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Conditional Branch
	//-----------------------------------------------------------------------------
	public void command_111() {
		result = false;
		switch (@parameters[0]) {
			case 0:   // switch
				switch_name = Game.GameData.data_system.switches[@parameters[1]];
				if (switch_name && System.Text.RegularExpressions.Regex.IsMatch(switch_name,@"^s\:")) {
					result = (eval($~.post_match) == (@parameters[2] == 0));
				} else {
					result = (Game.GameData.game_switches[@parameters[1]] == (@parameters[2] == 0));
				}
				break;
			case 1:   // variable
				value1 = Game.GameData.game_variables[@parameters[1]];
				value2 = (@parameters[2] == 0) ? @parameters[3] : Game.GameData.game_variables[@parameters[3]];
				switch (@parameters[4]) {
					case 0:  result = (value1 == value2); break;
					case 1:  result = (value1 >= value2); break;
					case 2:  result = (value1 <= value2); break;
					case 3:  result = (value1 > value2); break;
					case 4:  result = (value1 < value2); break;
					case 5:  result = (value1 != value2); break;
				}
				break;
			case 2:   // self switch
				if (@event_id > 0) {
					key = new {Game.GameData.game_map.map_id, @event_id, @parameters[1]};
					result = (Game.GameData.game_self_switches[key] == (@parameters[2] == 0));
				}
				break;
			case 3:   // timer
				if (Game.GameData.game_system.timer_start) {
					sec = Game.GameData.game_system.timer;
					result = (@parameters[2] == 0) ? (sec >= @parameters[1]) : (sec <= @parameters[1]);
				}
				break;
//			case 4: case 5   // actor, enemy
			case 6:   // character
				character = get_character(@parameters[1]);
				if (character) result = (character.direction == @parameters[2]);
				break;
			case 7:   // gold
				gold = Game.GameData.player.money;
				result = (@parameters[2] == 0) ? (gold >= @parameters[1]) : (gold <= @parameters[1]);
				break;
//			case 8: case 9: case 10:   // item, weapon, armor
			case 11:   // button
				result = Input.press(@parameters[1]);
				break;
			case 12:   // script
				result = execute_script(@parameters[1]);
				break;
		}
		// Store result in hash
		@branch[@list[@index].indent] = result;
		if (@branch[@list[@index].indent]) {
			@branch.delete(@list[@index].indent);
			return true;
		}
		return command_skip;
	}

	//-----------------------------------------------------------------------------
	// * Else
	//-----------------------------------------------------------------------------
	public void command_411() {
		if (@branch[@list[@index].indent] == false) {   // Could be null, so intentionally checks for false
			@branch.delete(@list[@index].indent);
			return true;
		}
		return command_skip;
	}

	//-----------------------------------------------------------------------------
	// * Loop
	//-----------------------------------------------------------------------------
	public void command_112() {
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Repeat Above
	//-----------------------------------------------------------------------------
	public void command_413() {
		indent = @list[@index].indent;
		do { //loop; while (true);
			@index -= 1;
			if (@list[@index].indent == indent) return true;
		}
	}

	//-----------------------------------------------------------------------------
	// * Break Loop
	//-----------------------------------------------------------------------------
	public void command_113() {
		indent = @list[@index].indent;
		temp_index = @index;
		do { //loop; while (true);
			temp_index += 1;
			if (temp_index >= @list.size - 1) return true;   // Reached end of commands
			// Skip ahead to after the [Repeat Above] end of the current loop
			if (@list[temp_index].code == 413 && @list[temp_index].indent < indent) {
				@index = temp_index;
				return true;
			}
		}
	}

	//-----------------------------------------------------------------------------
	// * Exit Event Processing
	//-----------------------------------------------------------------------------
	public void command_115() {
		command_end;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Erase Event
	//-----------------------------------------------------------------------------
	public void command_116() {
		if (@event_id > 0) {
			Game.GameData.game_map.events[@event_id]&.erase;
			Game.GameData.PokemonMap&.addErasedEvent(@event_id);
		}
		@index += 1;
		return false;
	}

	//-----------------------------------------------------------------------------
	// * Call Common Event
	//-----------------------------------------------------------------------------
	public void command_117() {
		common_event = Game.GameData.data_common_events[@parameters[0]];
		if (common_event) {
			@child_interpreter = new Interpreter(@depth + 1);
			@child_interpreter.setup(common_event.list, @event_id);
		}
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Label
	//-----------------------------------------------------------------------------
	public void command_118() {
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Jump to Label
	//-----------------------------------------------------------------------------
	public void command_119() {
		label_name = @parameters[0];
		temp_index = 0;
		do { //loop; while (true);
			if (temp_index >= @list.size - 1) return true;   // Reached end of commands
			// Check whether this command is a label with the desired name
			if (@list[temp_index].code == 118 &&
				@list[temp_index].parameters[0] == label_name) {
				@index = temp_index;
				return true;
			}
			// Command isn't the desired label, increment temp_index and keep looking
			temp_index += 1;
		}
	}

	//-----------------------------------------------------------------------------
	// * Control Switches
	//-----------------------------------------------------------------------------
	public void command_121() {
		should_refresh = false;
		(@parameters[0]..@parameters[1]).each do |i|
			if (Game.GameData.game_switches[i] == (@parameters[2] == 0)) continue;
			Game.GameData.game_switches[i] = (@parameters[2] == 0);
			should_refresh = true;
		}
		// Refresh map
		if (should_refresh) Game.GameData.game_map.need_refresh = true;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Control Variables
	//-----------------------------------------------------------------------------
	public void command_122() {
		value = 0;
		switch (@parameters[3]) {
			case 0:   // invariable (fixed value)
				value = @parameters[4];
				break;
			case 1:   // variable
				value = Game.GameData.game_variables[@parameters[4]];
				break;
			case 2:   // random number
				value = @parameters[4] + rand(@parameters[5] - @parameters[4] + 1);
				break;
//			case 3, 4, 5   // item, actor, enemy
			case 6:   // character
				character = get_character(@parameters[4]);
				if (character) {
					switch (@parameters[5]) {
						case 0:  value = character.x; break;             // x-coordinate
						case 1:  value = character.y; break;             // y-coordinate
						case 2:  value = character.direction; break;     // direction
						case 3:  value = character.screen_x; break;      // screen x-coordinate
						case 4:  value = character.screen_y; break;      // screen y-coordinate
						case 5:  value = character.terrain_tag.id_number; break;   // terrain tag
					}
				}
				break;
			case 7:   // other
				switch (@parameters[4]) {
					case 0:  value = Game.GameData.game_map.map_id; break;          // map ID
					case 1:  value = Game.GameData.player.pokemon_count; break;     // party members
					case 2:  value = Game.GameData.player.money; break;             // gold
					case 3:  value = Game.GameData.stats.distance_moved; break;     // steps
					case 4:  value = Game.GameData.stats.play_time; break;          // play time
					case 5:  value = Game.GameData.game_system.timer; break;        // timer
					case 6:  value = Game.GameData.game_system.save_count; break;   // save count
				}
		}
		// Apply value and operation to all specified game variables
		(@parameters[0]..@parameters[1]).each do |i|
			switch (@parameters[2]) {
				case 0:   // set
					if (Game.GameData.game_variables[i] == value) continue;
					Game.GameData.game_variables[i] = value;
					break;
				case 1:   // add
					if (Game.GameData.game_variables[i] >= 99_999_999) continue;
					Game.GameData.game_variables[i] += value;
					break;
				case 2:   // subtract
					if (Game.GameData.game_variables[i] <= -99_999_999) continue;
					Game.GameData.game_variables[i] -= value;
					break;
				case 3:   // multiply
					if (value == 1) continue;
					Game.GameData.game_variables[i] *= value;
					break;
				case 4:   // divide
					if (new []{0, 1}.Contains(value)) continue;
					Game.GameData.game_variables[i] /= value;
					break;
				case 5:   // remainder
					if (new []{0, 1}.Contains(value)) continue;
					Game.GameData.game_variables[i] %= value;
					break;
			}
			if (Game.GameData.game_variables[i] > 99_999_999) Game.GameData.game_variables[i] = 99_999_999;
			if (Game.GameData.game_variables[i] < -99_999_999) Game.GameData.game_variables[i] = -99_999_999;
			Game.GameData.game_map.need_refresh = true;
			break;
		}
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Control Self Switch
	//-----------------------------------------------------------------------------
	public void command_123() {
		if (@event_id > 0) {
			new_value = (@parameters[1] == 0);
			key = new {Game.GameData.game_map.map_id, @event_id, @parameters[0]};
			if (Game.GameData.game_self_switches[key] != new_value) {
				Game.GameData.game_self_switches[key] = new_value;
				Game.GameData.game_map.need_refresh = true;
			}
		}
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Control Timer
	//-----------------------------------------------------------------------------
	public void command_124() {
		Game.GameData.game_system.timer_start = (@parameters[0] == 0) ? Game.GameData.stats.play_time : null;
		if (@parameters[0] == 0) Game.GameData.game_system.timer_duration = @parameters[1];
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Change Gold
	//-----------------------------------------------------------------------------
	public void command_125() {
		value = (@parameters[1] == 0) ? @parameters[2] : Game.GameData.game_variables[@parameters[2]];
		if (@parameters[0] == 1) value = -value;   // Decrease
		Game.GameData.player.money += value;
		return true;
	}

	public int command_126 { get { return command_dummy; } }   // Change Items
	public int command_127 { get { return command_dummy; } }   // Change Weapons
	public int command_128 { get { return command_dummy; } }   // Change Armor
	public int command_129 { get { return command_dummy; } }   // Change Party Member

	//-----------------------------------------------------------------------------
	// * Change Windowskin
	//-----------------------------------------------------------------------------
	public void command_131() {
		for (int i = Settings.SPEECH_WINDOWSKINS.length; i < Settings.SPEECH_WINDOWSKINS.length; i++) { //for 'Settings.SPEECH_WINDOWSKINS.length' times do => |i|
			if (Settings.SPEECH_WINDOWSKINS[i] != @parameters[0]) continue;
			Game.GameData.PokemonSystem.textskin = i;
			MessageConfig.SetSpeechFrame("Graphics/Windowskins/" + Settings.SPEECH_WINDOWSKINS[i]);
			return true;
		}
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Change Battle BGM
	//-----------------------------------------------------------------------------
	public void command_132() {
		(Game.GameData.PokemonGlobal.nextBattleBGM = @parameters[0]) ? @parameters[0].clone : null
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Change Battle End ME
	//-----------------------------------------------------------------------------
	public int command_133 { get { return command_dummy; } }

	//-----------------------------------------------------------------------------
	// * Change Save Access
	//-----------------------------------------------------------------------------
	public void command_134() {
		Game.GameData.game_system.save_disabled = (@parameters[0] == 0);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Change Menu Access
	//-----------------------------------------------------------------------------
	public void command_135() {
		Game.GameData.game_system.menu_disabled = (@parameters[0] == 0);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Change Encounter
	//-----------------------------------------------------------------------------
	public void command_136() {
		Game.GameData.game_system.encounter_disabled = (@parameters[0] == 0);
		Game.GameData.game_player.make_encounter_count;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Transfer Player
	//-----------------------------------------------------------------------------
	public void command_201() {
		if (Game.GameData.game_temp.in_battle) return true;
		if (Game.GameData.game_temp.player_transferring ||
										Game.GameData.game_temp.message_window_showing ||
										Game.GameData.game_temp.transition_processing) return false;
		// Set up the transfer and the player's new coordinates
		Game.GameData.game_temp.player_transferring = true;
		if (@parameters[0] == 0) {   // Direct appointment
			Game.GameData.game_temp.player_new_map_id    = @parameters[1];
			Game.GameData.game_temp.player_new_x         = @parameters[2];
			Game.GameData.game_temp.player_new_y         = @parameters[3];
		} else {   // Appoint with variables
			Game.GameData.game_temp.player_new_map_id    = Game.GameData.game_variables[@parameters[1]];
			Game.GameData.game_temp.player_new_x         = Game.GameData.game_variables[@parameters[2]];
			Game.GameData.game_temp.player_new_y         = Game.GameData.game_variables[@parameters[3]];
		}
		Game.GameData.game_temp.player_new_direction = @parameters[4];
		@index += 1;
		// If transition happens with a fade, do the fade
		if (@parameters[5] == 0) {
			Graphics.freeze;
			Game.GameData.game_temp.transition_processing = true;
			Game.GameData.game_temp.transition_name       = "";
		}
		return false;
	}

	//-----------------------------------------------------------------------------
	// * Set Event Location
	//-----------------------------------------------------------------------------
	public void command_202() {
		if (Game.GameData.game_temp.in_battle) return true;
		character = get_character(@parameters[0]);
		if (character.null()) return true;
		// Move the character
		switch (@parameters[1]) {
			case 0:   // Direct appointment
				character.moveto(@parameters[2], @parameters[3]);
				break;
			case 1:   // Appoint with variables
				character.moveto(Game.GameData.game_variables[@parameters[2]], Game.GameData.game_variables[@parameters[3]]);
				break;
			default:   // Exchange with another event
				character2 = get_character(@parameters[2]);
				if (character2) {
					old_x = character.x;
					old_y = character.y;
					character.moveto(character2.x, character2.y);
					character2.moveto(old_x, old_y);
				}
				break;
		}
		// Set character's direction
		switch (@parameters[4]) {
			case 2:  character.turn_down; break;
			case 4:  character.turn_left; break;
			case 6:  character.turn_right; break;
			case 8:  character.turn_up; break;
		}
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Scroll Map
	//-----------------------------------------------------------------------------
	public void command_203() {
		if (Game.GameData.game_temp.in_battle) return true;
		if (Game.GameData.game_map.scrolling()) return false;
		Game.GameData.game_map.start_scroll(@parameters[0], @parameters[1], @parameters[2]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Change Map Settings
	//-----------------------------------------------------------------------------
	public void command_204() {
		switch (@parameters[0]) {
			case 0:   // panorama
				Game.GameData.game_map.panorama_name = @parameters[1];
				Game.GameData.game_map.panorama_hue  = @parameters[2];
				break;
			case 1:   // fog
				Game.GameData.game_map.fog_name       = @parameters[1];
				Game.GameData.game_map.fog_hue        = @parameters[2];
				Game.GameData.game_map.fog_opacity    = @parameters[3];
				Game.GameData.game_map.fog_blend_type = @parameters[4];
				Game.GameData.game_map.fog_zoom       = @parameters[5];
				Game.GameData.game_map.fog_sx         = @parameters[6];
				Game.GameData.game_map.fog_sy         = @parameters[7];
				break;
			case 2:   // battleback
				Game.GameData.game_map.battleback_name  = @parameters[1];
				Game.GameData.game_temp.battleback_name = @parameters[1];
				break;
		}
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Change Fog Color Tone
	//-----------------------------------------------------------------------------
	public void command_205() {
		Game.GameData.game_map.start_fog_tone_change(@parameters[0], @parameters[1]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Change Fog Opacity
	//-----------------------------------------------------------------------------
	public void command_206() {
		Game.GameData.game_map.start_fog_opacity_change(@parameters[0], @parameters[1]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Show Animation
	//-----------------------------------------------------------------------------
	public void command_207() {
		character = get_character(@parameters[0]);
		if (@follower_animation) {
			character = Followers.get(@follower_animation_id);
			@follower_animation = false;
			@follower_animation_id = null;
		}
		if (character.null()) return true;
		character.animation_id = @parameters[1];
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Change Transparent Flag
	//-----------------------------------------------------------------------------
	public void command_208() {
		Game.GameData.game_player.transparent = (@parameters[0] == 0);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Set Move Route
	//-----------------------------------------------------------------------------
	public void command_209() {
		character = get_character(@parameters[0]);
		if (@follower_move_route) {
			character = Followers.get(@follower_move_route_id);
			@follower_move_route = false;
			@follower_move_route_id = null;
		}
		if (character.null()) return true;
		character.force_move_route(@parameters[1]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Wait for Move's Completion
	//-----------------------------------------------------------------------------
	public void command_210() {
		if (!Game.GameData.game_temp.in_battle) @move_route_waiting = true;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Prepare for Transition
	//-----------------------------------------------------------------------------
	public void command_221() {
		if (Game.GameData.game_temp.message_window_showing) return false;
		Graphics.freeze;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Execute Transition
	//-----------------------------------------------------------------------------
	public void command_222() {
		if (Game.GameData.game_temp.transition_processing) return false;
		Game.GameData.game_temp.transition_processing = true;
		Game.GameData.game_temp.transition_name       = @parameters[0];
		@index += 1;
		return false;
	}

	//-----------------------------------------------------------------------------
	// * Change Screen Color Tone
	//-----------------------------------------------------------------------------
	public void command_223() {
		Game.GameData.game_screen.start_tone_change(@parameters[0], @parameters[1]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Screen Flash
	//-----------------------------------------------------------------------------
	public void command_224() {
		Game.GameData.game_screen.start_flash(@parameters[0], @parameters[1]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Screen Shake
	//-----------------------------------------------------------------------------
	public void command_225() {
		Game.GameData.game_screen.start_shake(@parameters[0], @parameters[1], @parameters[2]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Show Picture
	//-----------------------------------------------------------------------------
	public void command_231() {
		number = @parameters[0] + (Game.GameData.game_temp.in_battle ? 50 : 0);
		if (@parameters[3] == 0) {   // Direct appointment
			x = @parameters[4];
			y = @parameters[5];
		} else {   // Appoint with variables
			x = Game.GameData.game_variables[@parameters[4]];
			y = Game.GameData.game_variables[@parameters[5]];
		}
		Game.GameData.game_screen.pictures[number].show(@parameters[1], @parameters[2],
																			x, y, @parameters[6], @parameters[7], @parameters[8], @parameters[9]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Move Picture
	//-----------------------------------------------------------------------------
	public void command_232() {
		number = @parameters[0] + (Game.GameData.game_temp.in_battle ? 50 : 0);
		if (@parameters[3] == 0) {   // Direct appointment
			x = @parameters[4];
			y = @parameters[5];
		} else {   // Appoint with variables
			x = Game.GameData.game_variables[@parameters[4]];
			y = Game.GameData.game_variables[@parameters[5]];
		}
		Game.GameData.game_screen.pictures[number].move(@parameters[1], @parameters[2], x, y,
																			@parameters[6], @parameters[7],
																			@parameters[8], @parameters[9]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Rotate Picture
	//-----------------------------------------------------------------------------
	public void command_233() {
		number = @parameters[0] + (Game.GameData.game_temp.in_battle ? 50 : 0);
		Game.GameData.game_screen.pictures[number].rotate(@parameters[1]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Change Picture Color Tone
	//-----------------------------------------------------------------------------
	public void command_234() {
		number = @parameters[0] + (Game.GameData.game_temp.in_battle ? 50 : 0);
		Game.GameData.game_screen.pictures[number].start_tone_change(@parameters[1], @parameters[2]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Erase Picture
	//-----------------------------------------------------------------------------
	public void command_235() {
		number = @parameters[0] + (Game.GameData.game_temp.in_battle ? 50 : 0);
		Game.GameData.game_screen.pictures[number].erase;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Set Weather Effects
	//-----------------------------------------------------------------------------
	public void command_236() {
		Game.GameData.game_screen.weather(@parameters[0], @parameters[1], @parameters[2]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Play BGM
	//-----------------------------------------------------------------------------
	public void command_241() {
		BGMPlay(@parameters[0]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Fade Out BGM
	//-----------------------------------------------------------------------------
	public void command_242() {
		BGMFade(@parameters[0]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Play BGS
	//-----------------------------------------------------------------------------
	public void command_245() {
		BGSPlay(@parameters[0]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Fade Out BGS
	//-----------------------------------------------------------------------------
	public void command_246() {
		BGSFade(@parameters[0]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Memorize BGM/BGS
	//-----------------------------------------------------------------------------
	public void command_247() {
		Game.GameData.game_system.bgm_memorize;
		Game.GameData.game_system.bgs_memorize;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Restore BGM/BGS
	//-----------------------------------------------------------------------------
	public void command_248() {
		Game.GameData.game_system.bgm_restore;
		Game.GameData.game_system.bgs_restore;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Play ME
	//-----------------------------------------------------------------------------
	public void command_249() {
		MEPlay(@parameters[0]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Play SE
	//-----------------------------------------------------------------------------
	public void command_250() {
		SEPlay(@parameters[0]);
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Stop SE
	//-----------------------------------------------------------------------------
	public void command_251() {
		SEStop;
		return true;
	}

	public int command_301 { get { return command_dummy; } }   // Battle Processing
	public int command_601 { get { return command_if(0); } }   // If Win
	public int command_602 { get { return command_if(1); } }   // If Escape
	public int command_603 { get { return command_if(2); } }   // If Lose
	public int command_302 { get { return command_dummy; } }   // Shop Processing

	//-----------------------------------------------------------------------------
	// * Name Input Processing
	//-----------------------------------------------------------------------------
	public void command_303() {
		if (Game.GameData.player) {
			Game.GameData.player.name = EnterPlayerName(_INTL("Your name?"), 1, @parameters[1], Game.GameData.player.name);
			return true;
		}
		if (Game.GameData.game_actors && Game.GameData.data_actors && Game.GameData.data_actors[@parameters[0]]) {
			Game.GameData.game_temp.battle_abort = true;
			FadeOutIn do;
				sscene = new PokemonEntryScene();
				sscreen = new PokemonEntry(sscene);
				Game.GameData.game_actors[@parameters[0]].name = sscreen.StartScreen(
					_INTL("Enter {1}'s name.", Game.GameData.game_actors[@parameters[0]].name),
					1, @parameters[1], Game.GameData.game_actors[@parameters[0]].name
				);
			}
		}
		return true;
	}

	public int command_311 { get { return command_dummy; } }   // Change HP
	public int command_312 { get { return command_dummy; } }   // Change SP
	public int command_313 { get { return command_dummy; } }   // Change State

	//-----------------------------------------------------------------------------
	// * Recover All
	//-----------------------------------------------------------------------------
	public void command_314() {
		if (@parameters[0] == 0) {
			if (Settings.HEAL_STORED_POKEMON) {   // No need to heal stored Pokémon
				Game.GameData.player.heal_party;
			} else {
				EachPokemon { |pkmn, box| pkmn.heal };   // Includes party Pokémon
			}
		}
		return true;
	}

	public int command_315 { get { return command_dummy; } }   // Change EXP
	public int command_316 { get { return command_dummy; } }   // Change Level
	public int command_317 { get { return command_dummy; } }   // Change Parameters
	public int command_318 { get { return command_dummy; } }   // Change Skills
	public int command_319 { get { return command_dummy; } }   // Change Equipment
	public int command_320 { get { return command_dummy; } }   // Change Actor Name
	public int command_321 { get { return command_dummy; } }   // Change Actor Class
	public int command_322 { get { return command_dummy; } }   // Change Actor Graphic
	public int command_331 { get { return command_dummy; } }   // Change Enemy HP
	public int command_332 { get { return command_dummy; } }   // Change Enemy SP
	public int command_333 { get { return command_dummy; } }   // Change Enemy State
	public int command_334 { get { return command_dummy; } }   // Enemy Recover All
	public int command_335 { get { return command_dummy; } }   // Enemy Appearance
	public int command_336 { get { return command_dummy; } }   // Enemy Transform
	public int command_337 { get { return command_dummy; } }   // Show Battle Animation
	public int command_338 { get { return command_dummy; } }   // Deal Damage
	public int command_339 { get { return command_dummy; } }   // Force Action
	public int command_340 { get { return command_dummy; } }   // Abort Battle

	//-----------------------------------------------------------------------------
	// * Call Menu Screen
	//-----------------------------------------------------------------------------
	public void command_351() {
		Game.GameData.game_temp.menu_calling = true;
		@index += 1;
		return false;
	}

	//-----------------------------------------------------------------------------
	// * Call Save Screen
	//-----------------------------------------------------------------------------
	public void command_352() {
		scene = new PokemonSave_Scene();
		screen = new PokemonSaveScreen(scene);
		screen.SaveScreen;
		return true;
	}

	//-----------------------------------------------------------------------------
	// * Game Over
	//-----------------------------------------------------------------------------
	public void command_353() {
		BGMFade(1.0);
		BGSFade(1.0);
		FadeOutIn { StartOver(true) };
	}

	//-----------------------------------------------------------------------------
	// * Return to Title Screen
	//-----------------------------------------------------------------------------
	public void command_354() {
		Game.GameData.game_temp.title_screen_calling = true;
		return false;
	}

	//-----------------------------------------------------------------------------
	// * Script
	//-----------------------------------------------------------------------------
	public void command_355() {
		script = @list[@index].parameters[0] + "\n";
		// Look for more script commands or a continuation of one, and add them to script
		do { //loop; while (true);
			if (!new []{355, 655}.Contains(@list[@index + 1].code)) break;
			script += @list[@index + 1].parameters[0] + "\n";
			@index += 1;
		}
		// Run the script
		execute_script(script);
		return true;
	}
}
