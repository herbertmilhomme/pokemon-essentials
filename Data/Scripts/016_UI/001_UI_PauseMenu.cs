//===============================================================================
//
//===============================================================================
public partial class PokemonPauseMenu_Scene {
	public void StartScene() {
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		@sprites["cmdwindow"] = new Window_CommandPokemon(new List<string>());
		@sprites["cmdwindow"].visible = false;
		@sprites["cmdwindow"].viewport = @viewport;
		@sprites["infowindow"] = Window_UnformattedTextPokemon.newWithSize("", 0, 0, 32, 32, @viewport);
		@sprites["infowindow"].visible = false;
		@sprites["helpwindow"] = Window_UnformattedTextPokemon.newWithSize("", 0, 0, 32, 32, @viewport);
		@sprites["helpwindow"].visible = false;
		@infostate = false;
		@helpstate = false;
		SEPlay("GUI menu open");
	}

	public void ShowInfo(text) {
		@sprites["infowindow"].resizeToFit(text, Graphics.height);
		@sprites["infowindow"].text    = text;
		@sprites["infowindow"].visible = true;
		@infostate = true;
	}

	public void ShowHelp(text) {
		@sprites["helpwindow"].resizeToFit(text, Graphics.height);
		@sprites["helpwindow"].text    = text;
		@sprites["helpwindow"].visible = true;
		BottomLeft(@sprites["helpwindow"]);
		@helpstate = true;
	}

	public void ShowMenu() {
		@sprites["cmdwindow"].visible = true;
		@sprites["infowindow"].visible = @infostate;
		@sprites["helpwindow"].visible = @helpstate;
	}

	public void HideMenu() {
		@sprites["cmdwindow"].visible = false;
		@sprites["infowindow"].visible = false;
		@sprites["helpwindow"].visible = false;
	}

	public void ShowCommands(commands) {
		ret = -1;
		cmdwindow = @sprites["cmdwindow"];
		cmdwindow.commands = commands;
		cmdwindow.index    = Game.GameData.game_temp.menu_last_choice;
		cmdwindow.resizeToFit(commands);
		cmdwindow.x        = Graphics.width - cmdwindow.width;
		cmdwindow.y        = 0;
		cmdwindow.visible  = true;
		do { //loop; while (true);
			cmdwindow.update;
			Graphics.update;
			Input.update;
			UpdateSceneMap;
			if (Input.trigger(Input.BACK) || Input.trigger(Input.ACTION)) {
				ret = -1;
				break;
			} else if (Input.trigger(Input.USE)) {
				ret = cmdwindow.index;
				Game.GameData.game_temp.menu_last_choice = ret;
				break;
			}
		}
		return ret;
	}

	public void EndScene() {
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void Refresh() { }
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPauseMenu {
	public void initialize(scene) {
		@scene = scene;
	}

	public void ShowMenu() {
		@scene.Refresh;
		@scene.ShowMenu;
	}

	public void ShowInfo() { }

	public void StartPokemonMenu() {
		if (!Game.GameData.player) {
			if (Core.DEBUG) {
				Message(_INTL("The player trainer was not defined, so the pause menu can't be displayed."));
				Message(_INTL("Please see the documentation to learn how to set up the trainer player."));
			}
			return;
		}
		@scene.StartScene;
		// Show extra info window if relevant
		ShowInfo;
		// Get all commands
		command_list = new List<string>();
		commands = new List<string>();
		MenuHandlers.each_available(:pause_menu) do |option, hash, name|
			command_list.Add(name);
			commands.Add(hash);
		}
		// Main loop
		end_scene = false;
		do { //loop; while (true);
			choice = @scene.ShowCommands(command_list);
			if (choice < 0) {
				PlayCloseMenuSE;
				end_scene = true;
				break;
			}
			if (commands[choice]["effect"].call(@scene)) break;
		}
		if (end_scene) @scene.EndScene;
	}
}

//===============================================================================
// Pause menu commands.
//===============================================================================

MenuHandlers.add(:pause_menu, :pokedex, {
	"name"      => _INTL("Pokédex"),
	"order"     => 10,
	"condition" => () => next Game.GameData.player.has_pokedex && Game.GameData.player.pokedex.accessible_dexes.length > 0,
	"effect"    => menu => {
		PlayDecisionSE;
		if (Settings.USE_CURRENT_REGION_DEX) {
			FadeOutIn do;
				scene = new PokemonPokedex_Scene();
				screen = new PokemonPokedexScreen(scene);
				screen.StartScreen;
				menu.Refresh;
			}
		} else if (Game.GameData.player.pokedex.accessible_dexes.length == 1) {
			Game.GameData.PokemonGlobal.pokedexDex = Game.GameData.player.pokedex.accessible_dexes[0];
			FadeOutIn do;
				scene = new PokemonPokedex_Scene();
				screen = new PokemonPokedexScreen(scene);
				screen.StartScreen;
				menu.Refresh;
			}
		} else {
			FadeOutIn do;
				scene = new PokemonPokedexMenu_Scene();
				screen = new PokemonPokedexMenuScreen(scene);
				screen.StartScreen;
				menu.Refresh;
			}
		}
		next false;
	}
});

MenuHandlers.add(:pause_menu, :party, {
	"name"      => _INTL("Pokémon"),
	"order"     => 20,
	"condition" => () => next Game.GameData.player.party_count > 0,
	"effect"    => menu => {
		PlayDecisionSE;
		hidden_move = null;
		FadeOutIn do;
			sscene = new PokemonParty_Scene();
			sscreen = new PokemonPartyScreen(sscene, Game.GameData.player.party);
			hidden_move = sscreen.PokemonScreen;
			(hidden_move) ? menu.EndScene : menu.Refresh
		}
		if (!hidden_move) next false;
		Game.GameData.game_temp.in_menu = false;
		UseHiddenMove(hidden_move[0], hidden_move[1]);
		next true;
	}
});

MenuHandlers.add(:pause_menu, :bag, {
	"name"      => _INTL("Bag"),
	"order"     => 30,
	"condition" => () => { next !InBugContest() },
	"effect"    => menu => {
		PlayDecisionSE;
		item = null;
		FadeOutIn do;
			scene = new PokemonBag_Scene();
			screen = new PokemonBagScreen(scene, Game.GameData.bag);
			item = screen.StartScreen;
			(item) ? menu.EndScene : menu.Refresh
		}
		if (!item) next false;
		Game.GameData.game_temp.in_menu = false;
		UseKeyItemInField(item);
		next true;
	}
});

MenuHandlers.add(:pause_menu, :pokegear, {
	"name"      => _INTL("Pokégear"),
	"order"     => 40,
	"condition" => () => next Game.GameData.player.has_pokegear,
	"effect"    => menu => {
		PlayDecisionSE;
		FadeOutIn do;
			scene = new PokemonPokegear_Scene();
			screen = new PokemonPokegearScreen(scene);
			screen.StartScreen;
			(Game.GameData.game_temp.fly_destination) ? menu.EndScene : menu.Refresh
		}
		next FlyToNewLocation;
	}
});

MenuHandlers.add(:pause_menu, :town_map, {
	"name"      => _INTL("Town Map"),
	"order"     => 40,
	"condition" => () => { next !Game.GameData.player.has_pokegear && Game.GameData.bag.has(:TOWNMAP) },
	"effect"    => menu => {
		PlayDecisionSE;
		FadeOutIn do;
			scene = new PokemonRegionMap_Scene(-1, false);
			screen = new PokemonRegionMapScreen(scene);
			ret = screen.StartScreen;
			if (ret) Game.GameData.game_temp.fly_destination = ret;
			(Game.GameData.game_temp.fly_destination) ? menu.EndScene : menu.Refresh
		}
		next FlyToNewLocation;
	}
});

MenuHandlers.add(:pause_menu, :trainer_card, {
	"name"      => () => next Game.GameData.player.name,
	"order"     => 50,
	"effect"    => menu => {
		PlayDecisionSE;
		FadeOutIn do;
			scene = new PokemonTrainerCard_Scene();
			screen = new PokemonTrainerCardScreen(scene);
			screen.StartScreen;
			menu.Refresh;
		}
		next false;
	}
});

MenuHandlers.add(:pause_menu, :save, {
	"name"      => _INTL("Save"),
	"order"     => 60,
	"condition" => () => {
		next Game.GameData.game_system && !Game.GameData.game_system.save_disabled && !InSafari() && !InBugContest();
	},
	"effect"    => menu => {
		menu.HideMenu;
		scene = new PokemonSave_Scene();
		screen = new PokemonSaveScreen(scene);
		if (screen.SaveScreen) {
			menu.EndScene;
			next true;
		}
		menu.Refresh;
		menu.ShowMenu;
		next false;
	}
});

MenuHandlers.add(:pause_menu, :options, {
	"name"      => _INTL("Options"),
	"order"     => 70,
	"effect"    => menu => {
		PlayDecisionSE;
		FadeOutIn do;
			scene = new PokemonOption_Scene();
			screen = new PokemonOptionScreen(scene);
			screen.StartScreen;
			UpdateSceneMap;
			menu.Refresh;
		}
		next false;
	}
});

MenuHandlers.add(:pause_menu, :debug, {
	"name"      => _INTL("Debug"),
	"order"     => 80,
	"condition" => () => next Core.DEBUG,
	"effect"    => menu => {
		PlayDecisionSE;
		FadeOutIn do;
			DebugMenu;
			menu.Refresh;
		}
		next false;
	}
});

MenuHandlers.add(:pause_menu, :quit_game, {
	"name"      => _INTL("Quit Game"),
	"order"     => 90,
	"effect"    => menu => {
		menu.HideMenu;
		if (ConfirmMessage(_INTL("Are you sure you want to quit the game?"))) {
			scene = new PokemonSave_Scene();
			screen = new PokemonSaveScreen(scene);
			screen.SaveScreen;
			menu.EndScene;
			Game.GameData.scene = null;
			next true;
		}
		menu.Refresh;
		menu.ShowMenu;
		next false;
	}
});
