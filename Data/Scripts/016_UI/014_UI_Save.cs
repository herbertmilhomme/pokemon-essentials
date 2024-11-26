//===============================================================================
//
//===============================================================================
public partial class PokemonSave_Scene {
	LOCATION_TEXT_BASE   = new Color(32, 152, 8);   // Green
	LOCATION_TEXT_SHADOW = new Color(144, 240, 144);
	MALE_TEXT_BASE       = new Color(0, 112, 248);   // Blue
	MALE_TEXT_SHADOW     = new Color(120, 184, 232);
	FEMALE_TEXT_BASE     = new Color(232, 32, 16);   // Red
	FEMALE_TEXT_SHADOW   = new Color(248, 168, 184);
	OTHER_TEXT_BASE      = new Color(0, 112, 248);   // Blue
	OTHER_TEXT_SHADOW    = new Color(120, 184, 232);

	public void StartScreen() {
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		totalsec = Game.GameData.stats.play_time.ToInt();
		hour = totalsec / 60 / 60;
		min = totalsec / 60 % 60;
		mapname = Game.GameData.game_map.name;
		if (Game.GameData.player.male()) {
			text_tag = shadowc3tag(MALE_TEXT_BASE, MALE_TEXT_SHADOW);
		} else if (Game.GameData.player.female()) {
			text_tag = shadowc3tag(FEMALE_TEXT_BASE, FEMALE_TEXT_SHADOW);
		} else {
			text_tag = shadowc3tag(OTHER_TEXT_BASE, OTHER_TEXT_SHADOW);
		}
		location_tag = shadowc3tag(LOCATION_TEXT_BASE, LOCATION_TEXT_SHADOW);
		loctext = location_tag + "<ac>" + mapname + "</ac></c3>";
		loctext += _INTL("Player") + "<r>" + text_tag + Game.GameData.player.name + "</c3><br>";
		if (hour > 0) {
			loctext += _INTL("Time") + "<r>" + text_tag + _INTL("{1}h {2}m", hour, min) + "</c3><br>";
		} else {
			loctext += _INTL("Time") + "<r>" + text_tag + _INTL("{1}m", min) + "</c3><br>";
		}
		loctext += _INTL("Badges") + "<r>" + text_tag + Game.GameData.player.badge_count.ToString() + "</c3><br>";
		if (Game.GameData.player.has_pokedex) {
			loctext += _INTL("Pokédex") + "<r>" + text_tag + Game.GameData.player.pokedex.owned_count.ToString() + "/" + Game.GameData.player.pokedex.seen_count.ToString() + "</c3>";
		}
		@sprites["locwindow"] = new Window_AdvancedTextPokemon(loctext);
		@sprites["locwindow"].viewport = @viewport;
		@sprites["locwindow"].x = 0;
		@sprites["locwindow"].y = 0;
		if (@sprites["locwindow"].width < 228) @sprites["locwindow"].width = 228;
		@sprites["locwindow"].visible = true;
	}

	public void EndScreen() {
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonSaveScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void Display(text, brief = false) {
		@scene.Display(text, brief);
	}

	public void DisplayPaused(text) {
		@scene.DisplayPaused(text);
	}

	public void Confirm(text) {
		return @scene.Confirm(text);
	}

	public void SaveScreen() {
		ret = false;
		@scene.StartScreen;
		if (ConfirmMessage(_INTL("Would you like to save the game?"))) {
			if (SaveData.exists() && Game.GameData.game_temp.begun_new_game) {
				Message(_INTL("WARNING!") + "\1");
				Message(_INTL("There is a different game file that is already saved.") + "\1");
				Message(_INTL("If you save now, the other file's adventure, including items and Pokémon, will be entirely lost.") + "\1");
				if (!ConfirmMessageSerious(_INTL("Are you sure you want to save now and overwrite the other save file?"))) {
					SEPlay("GUI save choice");
					@scene.EndScreen;
					return false;
				}
			}
			Game.GameData.game_temp.begun_new_game = false;
			SEPlay("GUI save choice");
			if (Game.save) {
				Message("\\se[]" + _INTL("{1} saved the game.", Game.GameData.player.name) + "\\me[GUI save game]\\wtnp[20]");
				ret = true;
			} else {
				Message("\\se[]" + _INTL("Save failed.") + "\\wtnp[30]");
				ret = false;
			}
		} else {
			SEPlay("GUI save choice");
		}
		@scene.EndScreen;
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public void SaveScreen() {
	scene = new PokemonSave_Scene();
	screen = new PokemonSaveScreen(scene);
	ret = screen.SaveScreen;
	return ret;
}

public void EmergencySave() {
	oldscene = Game.GameData.scene;
	Game.GameData.scene = null;
	Message(_INTL("The script is taking too long. The game will restart."));
	if (!Game.GameData.player) return;
	if (SaveData.exists()) {
		File.open(SaveData.FILE_PATH, "rb") do |r|
			File.open(SaveData.FILE_PATH + ".bak", "wb") do |w|
				do { //loop; while (true);
					s = r.read(4096);
					if (!s) break;
					w.write(s);
				}
			}
		}
	}
	if (Game.save) {
		Message("\\se[]" + _INTL("The game was saved.") + "\\me[GUI save game]\\wtnp[20]");
		Message("\\se[]" + _INTL("The previous save file has been backed up.") + "\\wtnp[20]");
	} else {
		Message("\\se[]" + _INTL("Save failed.") + "\\wtnp[30]");
	}
	Game.GameData.scene = oldscene;
}
