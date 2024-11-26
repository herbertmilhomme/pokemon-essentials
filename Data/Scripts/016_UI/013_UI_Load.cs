//===============================================================================
//
//===============================================================================
public partial class PokemonLoadPanel : Sprite {
	public int selected		{ get { return _selected; } }			protected int _selected;

	TEXT_COLOR               = new Color(232, 232, 232);
	TEXT_SHADOW_COLOR        = new Color(136, 136, 136);
	MALE_TEXT_COLOR          = new Color(56, 160, 248);
	MALE_TEXT_SHADOW_COLOR   = new Color(56, 104, 168);
	FEMALE_TEXT_COLOR        = new Color(240, 72, 88);
	FEMALE_TEXT_SHADOW_COLOR = new Color(160, 64, 64);

	public override void initialize(index, title, isContinue, trainer, stats, mapid, viewport = null) {
		base.initialize(viewport);
		@index = index;
		@title = title;
		@isContinue = isContinue;
		@trainer = trainer;
		@totalsec = stats&.play_time.ToInt() || 0;
		@mapid = mapid;
		@selected = (index == 0);
		@bgbitmap = new AnimatedBitmap("Graphics/UI/Load/panels");
		@refreshBitmap = true;
		@refreshing = false;
		refresh;
	}

	public override void dispose() {
		@bgbitmap.dispose;
		self.bitmap.dispose;
		base.dispose();
	}

	public int selected { set {
		if (@selected == value) return;
		@selected = value;
		@refreshBitmap = true;
		refresh;
		}
	}

	public void Refresh() {
		@refreshBitmap = true;
		refresh;
	}

	public void refresh() {
		if (@refreshing) return;
		if (disposed()) return;
		@refreshing = true;
		if (!self.bitmap || self.bitmap.disposed()) {
			self.bitmap = new Bitmap(@bgbitmap.width, 222);
			SetSystemFont(self.bitmap);
		}
		if (@refreshBitmap) {
			@refreshBitmap = false;
			self.bitmap&.clear;
			if (@isContinue) {
				self.bitmap.blt(0, 0, @bgbitmap.bitmap, new Rect(0, (@selected) ? 222 : 0, @bgbitmap.width, 222));
			} else {
				self.bitmap.blt(0, 0, @bgbitmap.bitmap, new Rect(0, 444 + ((@selected) ? 46 : 0), @bgbitmap.width, 46));
			}
			textpos = new List<string>();
			if (@isContinue) {
				textpos.Add(new {@title, 32, 16, :left, TEXT_COLOR, TEXT_SHADOW_COLOR});
				textpos.Add(new {_INTL("Badges:"), 32, 118, :left, TEXT_COLOR, TEXT_SHADOW_COLOR});
				textpos.Add(new {@trainer.badge_count.ToString(), 206, 118, :right, TEXT_COLOR, TEXT_SHADOW_COLOR});
				textpos.Add(new {_INTL("Pokédex:"), 32, 150, :left, TEXT_COLOR, TEXT_SHADOW_COLOR});
				textpos.Add(new {@trainer.pokedex.seen_count.ToString(), 206, 150, :right, TEXT_COLOR, TEXT_SHADOW_COLOR});
				textpos.Add(new {_INTL("Time:"), 32, 182, :left, TEXT_COLOR, TEXT_SHADOW_COLOR});
				hour = @totalsec / 60 / 60;
				min  = @totalsec / 60 % 60;
				if (hour > 0) {
					textpos.Add(new {_INTL("{1}h {2}m", hour, min), 206, 182, :right, TEXT_COLOR, TEXT_SHADOW_COLOR});
				} else {
					textpos.Add(new {_INTL("{1}m", min), 206, 182, :right, TEXT_COLOR, TEXT_SHADOW_COLOR});
				}
				if (@trainer.male()) {
					textpos.Add(new {@trainer.name, 112, 70, :left, MALE_TEXT_COLOR, MALE_TEXT_SHADOW_COLOR});
				} else if (@trainer.female()) {
					textpos.Add(new {@trainer.name, 112, 70, :left, FEMALE_TEXT_COLOR, FEMALE_TEXT_SHADOW_COLOR});
				} else {
					textpos.Add(new {@trainer.name, 112, 70, :left, TEXT_COLOR, TEXT_SHADOW_COLOR});
				}
				mapname = GetMapNameFromId(@mapid);
				mapname = System.Text.RegularExpressions.Regex.Replace(mapname, "\\PN", @trainer.name);
				textpos.Add(new {mapname, 386, 16, :right, TEXT_COLOR, TEXT_SHADOW_COLOR});
			} else {
				textpos.Add(new {@title, 32, 14, :left, TEXT_COLOR, TEXT_SHADOW_COLOR});
			}
			DrawTextPositions(self.bitmap, textpos);
		}
		@refreshing = false;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonLoad_Scene {
	public void StartScene(commands, show_continue, trainer, stats, map_id) {
		@commands = commands;
		@sprites = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99998;
		addBackgroundOrColoredPlane(@sprites, "background", "Load/bg", new Color(248, 248, 248), @viewport);
		y = 32;
		for (int i = commands.length; i < commands.length; i++) { //for 'commands.length' times do => |i|
			@sprites[$"panel{i}"] = new PokemonLoadPanel(
				i, commands[i], (show_continue) ? (i == 0) : false, trainer, stats, map_id, @viewport
			);
			@sprites[$"panel{i}"].x = 48;
			@sprites[$"panel{i}"].y = y;
			@sprites[$"panel{i}"].Refresh;
			y += (show_continue && i == 0) ? 224 : 48;
		}
		@sprites["cmdwindow"] = new Window_CommandPokemon(new List<string>());
		@sprites["cmdwindow"].viewport = @viewport;
		@sprites["cmdwindow"].visible  = false;
		@max_party_index = 0;
	}

	public void StartScene2() {
		FadeInAndShow(@sprites) { Update };
	}

	public void StartDeleteScene() {
		@sprites = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99998;
		addBackgroundOrColoredPlane(@sprites, "background", "Load/bg", new Color(248, 248, 248), @viewport);
	}

	public void Update() {
		oldi = @sprites["cmdwindow"].index rescue 0;
		UpdateSpriteHash(@sprites);
		newi = @sprites["cmdwindow"].index rescue 0;
		if (oldi != newi) {
			@sprites[$"panel{oldi}"].selected = false;
			@sprites[$"panel{oldi}"].Refresh;
			@sprites[$"panel{newi}"].selected = true;
			@sprites[$"panel{newi}"].Refresh;
			while (@sprites[$"panel{newi}"].y > Graphics.height - 80) {
				for (int i = @commands.length; i < @commands.length; i++) { //for '@commands.length' times do => |i|
					@sprites[$"panel{i}"].y -= 48;
				}
				for (int i = (@max_party_index + 1); i < (@max_party_index + 1); i++) { //for '(@max_party_index + 1)' times do => |i|
					if (!@sprites[$"party{i}"]) break;
					@sprites[$"party{i}"].y -= 48;
				}
				if (@sprites["player"]) @sprites["player"].y -= 48;
			}
			while (@sprites[$"panel{newi}"].y < 32) {
				for (int i = @commands.length; i < @commands.length; i++) { //for '@commands.length' times do => |i|
					@sprites[$"panel{i}"].y += 48;
				}
				for (int i = (@max_party_index + 1); i < (@max_party_index + 1); i++) { //for '(@max_party_index + 1)' times do => |i|
					if (!@sprites[$"party{i}"]) break;
					@sprites[$"party{i}"].y += 48;
				}
				if (@sprites["player"]) @sprites["player"].y += 48;
			}
		}
	}

	public void SetParty(trainer) {
		if (!trainer || !trainer.party) return;
		meta = GameData.PlayerMetadata.get(trainer.character_ID);
		if (meta) {
			filename = GetPlayerCharset(meta.walk_charset, trainer, true);
			@sprites["player"] = new TrainerWalkingCharSprite(filename, @viewport);
			if (!@sprites["player"].bitmap) {
				Debug.LogError(_INTL("Player character {1}'s walking charset was not found (filename: \"{2}\").", trainer.character_ID, filename));
				//throw new ArgumentException(_INTL("Player character {1}'s walking charset was not found (filename: \"{2}\").", trainer.character_ID, filename));
			}
			charwidth  = @sprites["player"].bitmap.width;
			charheight = @sprites["player"].bitmap.height;
			@sprites["player"].x = 112 - (charwidth / 8);
			@sprites["player"].y = 112 - (charheight / 8);
			@sprites["player"].z = 99999;
		}
		trainer.party.each_with_index do |pkmn, i|
			@sprites[$"party{i}"] = new PokemonIconSprite(pkmn, @viewport);
			@sprites[$"party{i}"].setOffset(PictureOrigin.CENTER);
			@sprites[$"party{i}"].x = 334 + (66 * (i % 2));
			@sprites[$"party{i}"].y = 112 + (50 * (i / 2));
			@sprites[$"party{i}"].z = 99999;
			@max_party_index = i;
		}
	}

	public void Choose(commands) {
		@sprites["cmdwindow"].commands = commands;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (Input.trigger(Input.USE)) {
				return @sprites["cmdwindow"].index;
			}
		}
	}

	public void EndScene() {
		FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void CloseScene() {
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonLoadScreen {
	public void initialize(scene) {
		@scene = scene;
		if (SaveData.exists()) {
			@save_data = load_save_file(SaveData.FILE_PATH);
		} else {
			@save_data = new List<string>();
		}
	}

	/// <param name="file_path">file to load save data from</param>
	// @return [Hash] save data
	public void load_save_file(String file_path) {
		save_data = SaveData.read_from_file(file_path);
		unless (SaveData.valid(save_data)) {
			if (File.file(file_path + ".bak")) {
				Message(_INTL("The save file is corrupt. A backup will be loaded."));
				save_data = load_save_file(file_path + ".bak");
			} else {
				self.prompt_save_deletion;
				return {};
			}
		}
		return save_data;
	}

	// Called if all save data is invalid.
	// Prompts the player to delete the save files.
	public void prompt_save_deletion() {
		Message(_INTL("The save file is corrupt, or is incompatible with this game.") + "\1");
		unless (ConfirmMessageSerious(
			_INTL("Do you want to delete the save file and start anew?")
		)) exit;
		self.delete_save_data;
		Game.GameData.game_system   = new Game_System();
		Game.GameData.PokemonSystem = new PokemonSystem();
	}

	public void StartDeleteScreen() {
		@scene.StartDeleteScene;
		@scene.StartScene2;
		if (SaveData.exists()) {
			if (ConfirmMessageSerious(_INTL("Delete all saved data?"))) {
				Message(_INTL("Once data has been deleted, there is no way to recover it.") + "\1");
				if (ConfirmMessageSerious(_INTL("Delete the saved data anyway?"))) {
					Message(_INTL("Deleting all data. Don't turn off the power.") + "\\wtnp[0]");
					self.delete_save_data;
				}
			}
		} else {
			Message(_INTL("No save file was found."));
		}
		@scene.EndScene;
		Game.GameData.scene = CallTitle;
	}

	public void delete_save_data() {
		begin;
			SaveData.delete_file;
			Message(_INTL("The saved data was deleted."));
		rescue SystemCallError;
			Message(_INTL("All saved data could not be deleted."));
		}
	}

	public void StartLoadScreen() {
		if (Core.DEBUG && !FileTest.exist("Game.rgssad") && Settings.SKIP_CONTINUE_SCREEN) {
			if (@save_data.empty()) {
				Game.start_new;
			} else {
				Game.load(@save_data);
			}
			return;
		}
		commands = new List<string>();
		cmd_continue     = -1;
		cmd_new_game     = -1;
		cmd_options      = -1;
		cmd_language     = -1;
		cmd_mystery_gift = -1;
		cmd_debug        = -1;
		cmd_quit         = -1;
		show_continue = !@save_data.empty();
		if (show_continue) {
			commands[cmd_continue = commands.length] = _INTL("Continue");
			if (@save_data.player.mystery_gift_unlocked) {
				commands[cmd_mystery_gift = commands.length] = _INTL("Mystery Gift");
			}
		}
		commands[cmd_new_game = commands.length]  = _INTL("New Game");
		commands[cmd_options = commands.length]   = _INTL("Options");
		if (Settings.LANGUAGES.length >= 2) commands[cmd_language = commands.length]  = _INTL("Language");
		if (Core.DEBUG) commands[cmd_debug = commands.length]     = _INTL("Debug");
		commands[cmd_quit = commands.length]      = _INTL("Quit Game");
		map_id = show_continue ? @save_data.map_factory.map.map_id : 0;
		@scene.StartScene(commands, show_continue, @save_data.player, @save_data.stats, map_id);
		if (show_continue) @scene.SetParty(@save_data.player);
		@scene.StartScene2;
		do { //loop; while (true);
			command = @scene.Choose(commands);
			if (command != cmd_quit) PlayDecisionSE;
			switch (command) {
				case cmd_continue:
					@scene.EndScene;
					Game.load(@save_data);
					return;
				case cmd_new_game:
					@scene.EndScene;
					Game.start_new;
					return;
				case cmd_mystery_gift:
					FadeOutIn { DownloadMysteryGift(@save_data.player) };
					break;
				case cmd_options:
					FadeOutIn do;
						scene = new PokemonOption_Scene();
						screen = new PokemonOptionScreen(scene);
						screen.StartScreen(true);
					}
					break;
				case cmd_language:
					@scene.EndScene;
					Game.GameData.PokemonSystem.language = ChooseLanguage;
					MessageTypes.load_message_files(Settings.LANGUAGES[Game.GameData.PokemonSystem.language][1]);
					if (show_continue) {
						@save_data.pokemon_system = Game.GameData.PokemonSystem;
						File.open(SaveData.FILE_PATH, "wb", file => { Marshal.dump(@save_data, file); });
					}
					Game.GameData.scene = CallTitle;
					return;
				case cmd_debug:
					FadeOutIn { DebugMenu(false) };
					break;
				case cmd_quit:
					PlayCloseMenuSE;
					@scene.EndScene;
					Game.GameData.scene = null;
					return;
				default:
					PlayBuzzerSE;
					break;
			}
		}
	}
}
