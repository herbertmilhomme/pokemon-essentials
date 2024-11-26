//===============================================================================
//
//===============================================================================
public partial class PokemonSystem {
	public int textspeed		{ get { return _textspeed; } set { _textspeed = value; } }			protected int _textspeed;
	public int battlescene		{ get { return _battlescene; } set { _battlescene = value; } }			protected int _battlescene;
	public int battlestyle		{ get { return _battlestyle; } set { _battlestyle = value; } }			protected int _battlestyle;
	public int sendtoboxes		{ get { return _sendtoboxes; } set { _sendtoboxes = value; } }			protected int _sendtoboxes;
	public int givenicknames		{ get { return _givenicknames; } set { _givenicknames = value; } }			protected int _givenicknames;
	public int frame		{ get { return _frame; } set { _frame = value; } }			protected int _frame;
	public int textskin		{ get { return _textskin; } set { _textskin = value; } }			protected int _textskin;
	public int screensize		{ get { return _screensize; } set { _screensize = value; } }			protected int _screensize;
	public int language		{ get { return _language; } set { _language = value; } }			protected int _language;
	public int runstyle		{ get { return _runstyle; } set { _runstyle = value; } }			protected int _runstyle;
	public int bgmvolume		{ get { return _bgmvolume; } set { _bgmvolume = value; } }			protected int _bgmvolume;
	public int sevolume		{ get { return _sevolume; } set { _sevolume = value; } }			protected int _sevolume;
	public int textinput		{ get { return _textinput; } set { _textinput = value; } }			protected int _textinput;

	public void initialize() {
		@textspeed     = 1;     // Text speed (0=slow, 1=medium, 2=fast, 3=instant)
		@battlescene   = 0;     // Battle effects (animations) (0=on, 1=off)
		@battlestyle   = 0;     // Battle style (0=switch, 1=set)
		@sendtoboxes   = 0;     // Send to Boxes (0=manual, 1=automatic)
		@givenicknames = 0;     // Give nicknames (0=give, 1=don't give)
		@frame         = 0;     // Default window frame (see also Settings.MENU_WINDOWSKINS)
		@textskin      = 0;     // Speech frame
		@screensize    = (int)Math.Floor(Settings.SCREEN_SCALE * 2) - 1;   // 0=half size, 1=full size, 2=full-and-a-half size, 3=double size
		@language      = 0;     // Language (see also Settings.LANGUAGES in script PokemonSystem)
		@runstyle      = 0;     // Default movement speed (0=walk, 1=run)
		@bgmvolume     = 80;    // Volume of background music and ME
		@sevolume      = 100;   // Volume of sound effects
		@textinput     = 0;     // Text input mode (0=cursor, 1=keyboard)
	}
}

//===============================================================================
//
//===============================================================================
public static partial class PropertyMixin {
	public int name		{ get { return _name; } }			protected int _name;

	public void get() {
		return @get_proc&.call;
	}

	public void set(*args) {
		@set_proc&.call(*args);
	}
}

//===============================================================================
//
//===============================================================================
public partial class EnumOption {
	include PropertyMixin;
	public int values		{ get { return _values; } }			protected int _values;

	public void initialize(name, values, get_proc, set_proc) {
		@name     = name;
		@values   = values.map(val => _INTL(val));
		@get_proc = get_proc;
		@set_proc = set_proc;
	}

	public void next(current) {
		index = current + 1;
		if (index > @values.length - 1) index = @values.length - 1;
		return index;
	}

	public void prev(current) {
		index = current - 1;
		if (index < 0) index = 0;
		return index;
	}
}

//===============================================================================
//
//===============================================================================
public partial class NumberOption {
	include PropertyMixin;
	public int lowest_value		{ get { return _lowest_value; } }			protected int _lowest_value;
	public int highest_value		{ get { return _highest_value; } }			protected int _highest_value;

	public void initialize(name, range, get_proc, set_proc) {
		@name = name;
		switch (range) {
			case Range:
				@lowest_value  = range.begin;
				@highest_value = range.end;
				break;
			case Array:
				@lowest_value  = range[0];
				@highest_value = range[1];
				break;
		}
		@get_proc = get_proc;
		@set_proc = set_proc;
	}

	public void next(current) {
		index = current + @lowest_value;
		index += 1;
		if (index > @highest_value) index = @lowest_value;
		return index - @lowest_value;
	}

	public void prev(current) {
		index = current + @lowest_value;
		index -= 1;
		if (index < @lowest_value) index = @highest_value;
		return index - @lowest_value;
	}
}

//===============================================================================
//
//===============================================================================
public partial class SliderOption {
	include PropertyMixin;
	public int lowest_value		{ get { return _lowest_value; } }			protected int _lowest_value;
	public int highest_value		{ get { return _highest_value; } }			protected int _highest_value;

	public void initialize(name, range, get_proc, set_proc) {
		@name          = name;
		@lowest_value  = range[0];
		@highest_value = range[1];
		@interval      = range[2];
		@get_proc      = get_proc;
		@set_proc      = set_proc;
	}

	public void next(current) {
		index = current + @lowest_value;
		index += @interval;
		if (index > @highest_value) index = @highest_value;
		return index - @lowest_value;
	}

	public void prev(current) {
		index = current + @lowest_value;
		index -= @interval;
		if (index < @lowest_value) index = @lowest_value;
		return index - @lowest_value;
	}
}

//===============================================================================
// Main options list.
//===============================================================================
public partial class Window_PokemonOption : Window_DrawableCommand {
	public int value_changed		{ get { return _value_changed; } }			protected int _value_changed;

	SEL_NAME_BASE_COLOR    = new Color(192, 120, 0);
	SEL_NAME_SHADOW_COLOR  = new Color(248, 176, 80);
	SEL_VALUE_BASE_COLOR   = new Color(248, 48, 24);
	SEL_VALUE_SHADOW_COLOR = new Color(248, 136, 128);

	public void initialize(options, x, y, width, height) {
		@options = options;
		@values = new List<string>();
		@options.length.times(i => @values[i] = 0);
		@value_changed = false;
		super(x, y, width, height);
	}

	public int this[int i] { get {
		return @values[i];
		}
	}

	public int this[(i, value)] { get {
		@values[i] = value;
		refresh;
		}
	}

	public void setValueNoRefresh(i, value) {
		@values[i] = value;
	}

	public void itemCount() {
		return @options.length + 1;
	}

	public void drawItem(index, _count, rect) {
		rect = drawCursor(index, rect);
		sel_index = self.index;
		// Draw option's name
		optionname = (index == @options.length) ? _INTL("Close") : @options[index].name;
		optionwidth = rect.width * 9 / 20;
		DrawShadowText(self.contents, rect.x, rect.y, optionwidth, rect.height, optionname,
										(index == sel_index) ? SEL_NAME_BASE_COLOR : self.baseColor,
										(index == sel_index) ? SEL_NAME_SHADOW_COLOR : self.shadowColor)
		if (index == @options.length) return;
		// Draw option's values
		switch (@options[index]) {
			case EnumOption:
				if (@options[index].values.length > 1) {
					totalwidth = 0;
					@options[index].values.each do |value|
						totalwidth += self.contents.text_size(value).width;
					}
					spacing = (rect.width - rect.x - optionwidth - totalwidth) / (@options[index].values.length - 1);
					if (spacing < 0) spacing = 0;
					xpos = optionwidth + rect.x;
					ivalue = 0;
					@options[index].values.each do |value|
						DrawShadowText(self.contents, xpos, rect.y, optionwidth, rect.height, value,
														(ivalue == self[index]) ? SEL_VALUE_BASE_COLOR : self.baseColor,
														(ivalue == self[index]) ? SEL_VALUE_SHADOW_COLOR : self.shadowColor)
						xpos += self.contents.text_size(value).width;
						xpos += spacing;
						ivalue += 1;
					}
				} else {
					DrawShadowText(self.contents, rect.x + optionwidth, rect.y, optionwidth, rect.height,
													optionname, self.baseColor, self.shadowColor);
				}
				break;
			case NumberOption:
				value = _INTL("Type {1}/{2}", @options[index].lowest_value + self[index],
											@options[index].highest_value - @options[index].lowest_value + 1);
				xpos = optionwidth + (rect.x * 2);
				DrawShadowText(self.contents, xpos, rect.y, optionwidth, rect.height, value,
												SEL_VALUE_BASE_COLOR, SEL_VALUE_SHADOW_COLOR, 1);
				break;
			case SliderOption:
				value = string.Format(" {0}", @options[index].highest_value);
				sliderlength = rect.width - rect.x - optionwidth - self.contents.text_size(value).width;
				xpos = optionwidth + rect.x;
				self.contents.fill_rect(xpos, rect.y - 2 + (rect.height / 2), sliderlength, 4, self.baseColor);
				self.contents.fill_rect(
					xpos + ((sliderlength - 8) * (@options[index].lowest_value + self[index]) / @options[index].highest_value),
					rect.y - 8 + (rect.height / 2),
					8, 16, SEL_VALUE_BASE_COLOR
				);
				value = (@options[index].lowest_value + self[index]).ToString();
				xpos += (rect.width - rect.x - optionwidth) - self.contents.text_size(value).width;
				DrawShadowText(self.contents, xpos, rect.y, optionwidth, rect.height, value,
												SEL_VALUE_BASE_COLOR, SEL_VALUE_SHADOW_COLOR);
			default:
				value = @options[index].values[self[index]];
				xpos = optionwidth + rect.x;
				DrawShadowText(self.contents, xpos, rect.y, optionwidth, rect.height, value,
												SEL_VALUE_BASE_COLOR, SEL_VALUE_SHADOW_COLOR);
				break;
		}
	}

	public override void update() {
		oldindex = self.index;
		@value_changed = false;
		base.update();
		dorefresh = (self.index != oldindex);
		if (self.active && self.index < @options.length) {
			if (Input.repeat(Input.LEFT)) {
				self[self.index] = @options[self.index].prev(self[self.index]);
				dorefresh = true;
				@value_changed = true;
			} else if (Input.repeat(Input.RIGHT)) {
				self[self.index] = @options[self.index].next(self[self.index]);
				dorefresh = true;
				@value_changed = true;
			}
		}
		if (dorefresh) refresh;
	}
}

//===============================================================================
// Options main screen.
//===============================================================================
public partial class PokemonOption_Scene {
	public int sprites		{ get { return _sprites; } }			protected int _sprites;
	public int in_load_screen		{ get { return _in_load_screen; } }			protected int _in_load_screen;

	public void StartScene(in_load_screen = false) {
		@in_load_screen = in_load_screen;
		// Get all options
		@options = new List<string>();
		@hashes = new List<string>();
		MenuHandlers.each_available(:options_menu) do |option, hash, name|
			@options.Add(
				hash["type"].new(name, hash["parameters"], hash["get_proc"], hash["set_proc"])
			);
			@hashes.Add(hash);
		}
		// Create sprites
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		addBackgroundOrColoredPlane(@sprites, "bg", "optionsbg", new Color(192, 200, 208), @viewport);
		@sprites["title"] = Window_UnformattedTextPokemon.newWithSize(
			_INTL("Options"), 0, -16, Graphics.width, 64, @viewport
		);
		@sprites["title"].back_opacity = 0;
		@sprites["textbox"] = CreateMessageWindow;
		SetSystemFont(@sprites["textbox"].contents);
		@sprites["option"] = new Window_PokemonOption(
			@options, 0, @sprites["title"].y + @sprites["title"].height - 16, Graphics.width,
			Graphics.height - (@sprites["title"].y + @sprites["title"].height - 16) - @sprites["textbox"].height
		);
		@sprites["option"].viewport = @viewport;
		@sprites["option"].visible  = true;
		// Get the values of each option
		@options.length.times(i => @sprites["option"].setValueNoRefresh(i, @options[i].get || 0));
		@sprites["option"].refresh;
		ChangeSelection;
		DeactivateWindows(@sprites);
		FadeInAndShow(@sprites) { Update };
	}

	public void ChangeSelection() {
		hash = @hashes[@sprites["option"].index];
		// Call selected option's "on_select" proc (if defined)
		@sprites["textbox"].letterbyletter = false;
		if (hash) hash["on_select"]&.call(self);
		// Set descriptive text
		description = "";
		if (hash) {
			if (hash["description"].is_a(Proc)) {
				description = hash["description"].call;
			} else if (!hash["description"].null()) {
				description = _INTL(hash["description"]);
			}
		} else {
			description = _INTL("Close the screen.");
		}
		@sprites["textbox"].text = description;
	}

	public void Options() {
		ActivateWindow(@sprites, "option") do;
			index = -1;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				Update;
				if (@sprites["option"].index != index) {
					ChangeSelection;
					index = @sprites["option"].index;
				}
				if (@sprites["option"].value_changed) @options[index].set(@sprites["option"][index], self);
				if (Input.trigger(Input.BACK)) {
					break;
				} else if (Input.trigger(Input.USE)) {
					if (@sprites["option"].index == @options.length) break;
				}
			}
		}
	}

	public void EndScene() {
		PlayCloseMenuSE;
		FadeOutAndHide(@sprites) { Update };
		// Set the values of each option, to make sure they're all set
		for (int i = @options.length; i < @options.length; i++) { //for '@options.length' times do => |i|
			@options[i].set(@sprites["option"][i], self);
		}
		DisposeMessageWindow(@sprites["textbox"]);
		DisposeSpriteHash(@sprites);
		UpdateSceneMap;
		@viewport.dispose;
	}

	public void Update() {
		UpdateSpriteHash(@sprites);
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonOptionScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen(in_load_screen = false) {
		@scene.StartScene(in_load_screen);
		@scene.Options;
		@scene.EndScene;
	}
}

//===============================================================================
// Options Menu commands.
//===============================================================================

MenuHandlers.add(:options_menu, :bgm_volume, {
	"name"        => _INTL("Music Volume"),
	"order"       => 10,
	"type"        => SliderOption,
	"parameters"  => new {0, 100, 5},   // new {minimum_value, maximum_value, interval}
	"description" => _INTL("Adjust the volume of the background music."),
	"get_proc"    => () => next Game.GameData.PokemonSystem.bgmvolume,
	"set_proc"    => (value, scene) => {
		if (Game.GameData.PokemonSystem.bgmvolume == value) continue;
		Game.GameData.PokemonSystem.bgmvolume = value;
		if (scene.in_load_screen || Game.GameData.game_system.playing_bgm.null()) continue;
		playingBGM = Game.GameData.game_system.getPlayingBGM;
		Game.GameData.game_system.bgm_pause;
		Game.GameData.game_system.bgm_resume(playingBGM);
	}
});

MenuHandlers.add(:options_menu, :se_volume, {
	"name"        => _INTL("SE Volume"),
	"order"       => 20,
	"type"        => SliderOption,
	"parameters"  => new {0, 100, 5},   // new {minimum_value, maximum_value, interval}
	"description" => _INTL("Adjust the volume of sound effects."),
	"get_proc"    => () => next Game.GameData.PokemonSystem.sevolume,
	"set_proc"    => (value, _scene) => {
		if (Game.GameData.PokemonSystem.sevolume == value) continue;
		Game.GameData.PokemonSystem.sevolume = value;
		if (Game.GameData.game_system.playing_bgs) {
			Game.GameData.game_system.playing_bgs.volume = value;
			playingBGS = Game.GameData.game_system.getPlayingBGS;
			Game.GameData.game_system.bgs_pause;
			Game.GameData.game_system.bgs_resume(playingBGS);
		}
		PlayCursorSE;
	}
});

MenuHandlers.add(:options_menu, :text_speed, {
	"name"        => _INTL("Text Speed"),
	"order"       => 30,
	"type"        => EnumOption,
	"parameters"  => new {_INTL("Slow"), _INTL("Mid"), _INTL("Fast"), _INTL("Inst")},
	"description" => _INTL("Choose the speed at which text appears."),
	"on_select"   => scene => { scene.sprites["textbox"].letterbyletter = true },
	"get_proc"    => () => next Game.GameData.PokemonSystem.textspeed,
	"set_proc"    => (value, scene) => {
		if (value == Game.GameData.PokemonSystem.textspeed) continue;
		Game.GameData.PokemonSystem.textspeed = value;
		MessageConfig.SetTextSpeed(MessageConfig.SettingToTextSpeed(value));
		// Display the message with the selected text speed to gauge it better.
		scene.sprites["textbox"].textspeed      = MessageConfig.GetTextSpeed;
		scene.sprites["textbox"].letterbyletter = true;
		scene.sprites["textbox"].text           = scene.sprites["textbox"].text;
	}
});

MenuHandlers.add(:options_menu, :battle_animations, {
	"name"        => _INTL("Battle Effects"),
	"order"       => 40,
	"type"        => EnumOption,
	"parameters"  => new {_INTL("On"), _INTL("Off")},
	"description" => _INTL("Choose whether you wish to see move animations in battle."),
	"get_proc"    => () => next Game.GameData.PokemonSystem.battlescene,
	"set_proc"    => (value, _scene) => Game.GameData.PokemonSystem.battlescene = value;
});

MenuHandlers.add(:options_menu, :battle_style, {
	"name"        => _INTL("Battle Style"),
	"order"       => 50,
	"type"        => EnumOption,
	"parameters"  => new {_INTL("Switch"), _INTL("Set")},
	"description" => _INTL("Choose whether you can switch Pokémon when an opponent's Pokémon faints."),
	"get_proc"    => () => next Game.GameData.PokemonSystem.battlestyle,
	"set_proc"    => (value, _scene) => Game.GameData.PokemonSystem.battlestyle = value;
});

MenuHandlers.add(:options_menu, :movement_style, {
	"name"        => _INTL("Default Movement"),
	"order"       => 60,
	"type"        => EnumOption,
	"parameters"  => new {_INTL("Walking"), _INTL("Running")},
	"description" => _INTL("Choose your movement speed. Hold Back while moving to move at the other speed."),
	"condition"   => () => next Game.GameData.player&.has_running_shoes,
	"get_proc"    => () => next Game.GameData.PokemonSystem.runstyle,
	"set_proc"    => (value, _sceme) => Game.GameData.PokemonSystem.runstyle = value;
});

MenuHandlers.add(:options_menu, :send_to_boxes, {
	"name"        => _INTL("Send to Boxes"),
	"order"       => 70,
	"type"        => EnumOption,
	"parameters"  => new {_INTL("Manual"), _INTL("Automatic")},
	"description" => _INTL("Choose whether caught Pokémon are sent to your Boxes when your party is full."),
	"condition"   => () => next Settings.NEW_CAPTURE_CAN_REPLACE_PARTY_MEMBER,
	"get_proc"    => () => next Game.GameData.PokemonSystem.sendtoboxes,
	"set_proc"    => (value, _scene) => Game.GameData.PokemonSystem.sendtoboxes = value;
});

MenuHandlers.add(:options_menu, :give_nicknames, {
	"name"        => _INTL("Give Nicknames"),
	"order"       => 80,
	"type"        => EnumOption,
	"parameters"  => new {_INTL("Give"), _INTL("Don't give")},
	"description" => _INTL("Choose whether you can give a nickname to a Pokémon when you obtain it."),
	"get_proc"    => () => next Game.GameData.PokemonSystem.givenicknames,
	"set_proc"    => (value, _scene) => Game.GameData.PokemonSystem.givenicknames = value;
});

MenuHandlers.add(:options_menu, :speech_frame, {
	"name"        => _INTL("Speech Frame"),
	"order"       => 90,
	"type"        => NumberOption,
	"parameters"  => 1..Settings.SPEECH_WINDOWSKINS.length,
	"description" => _INTL("Choose the appearance of dialogue boxes."),
	"condition"   => () => next Settings.SPEECH_WINDOWSKINS.length > 1,
	"get_proc"    => () => next Game.GameData.PokemonSystem.textskin,
	"set_proc"    => (value, scene) => {
		Game.GameData.PokemonSystem.textskin = value;
		MessageConfig.SetSpeechFrame("Graphics/Windowskins/" + Settings.SPEECH_WINDOWSKINS[value]);
		// Change the windowskin of the options text box to selected one
		scene.sprites["textbox"].setSkin(MessageConfig.GetSpeechFrame);
	}
});

MenuHandlers.add(:options_menu, :menu_frame, {
	"name"        => _INTL("Menu Frame"),
	"order"       => 100,
	"type"        => NumberOption,
	"parameters"  => 1..Settings.MENU_WINDOWSKINS.length,
	"description" => _INTL("Choose the appearance of menu boxes."),
	"condition"   => () => next Settings.MENU_WINDOWSKINS.length > 1,
	"get_proc"    => () => next Game.GameData.PokemonSystem.frame,
	"set_proc"    => (value, scene) => {
		Game.GameData.PokemonSystem.frame = value;
		MessageConfig.SetSystemFrame("Graphics/Windowskins/" + Settings.MENU_WINDOWSKINS[value]);
		// Change the windowskin of the options text box to selected one
		scene.sprites["option"].setSkin(MessageConfig.GetSystemFrame);
	}
});

MenuHandlers.add(:options_menu, :text_input_style, {
	"name"        => _INTL("Text Entry"),
	"order"       => 110,
	"type"        => EnumOption,
	"parameters"  => new {_INTL("Cursor"), _INTL("Keyboard")},
	"description" => _INTL("Choose how you want to enter text."),
	"get_proc"    => () => next Game.GameData.PokemonSystem.textinput,
	"set_proc"    => (value, _scene) => Game.GameData.PokemonSystem.textinput = value;
});

MenuHandlers.add(:options_menu, :screen_size, {
	"name"        => _INTL("Screen Size"),
	"order"       => 120,
	"type"        => EnumOption,
	"parameters"  => new {_INTL("S"), _INTL("M"), _INTL("L"), _INTL("XL"), _INTL("Full")},
	"description" => _INTL("Choose the size of the game window."),
	"get_proc"    => () => { next (int)Math.Min(Game.GameData.PokemonSystem.screensize, 4) },
	"set_proc"    => (value, _scene) => {
		if (Game.GameData.PokemonSystem.screensize == value) continue;
		Game.GameData.PokemonSystem.screensize = value;
		SetResizeFactor(Game.GameData.PokemonSystem.screensize);
	}
});
