//===============================================================================
//
//===============================================================================
public partial class PokemonJukebox_Scene {
	public void Update() {
		UpdateSpriteHash(@sprites);
	}

	public void StartScene(commands) {
		@commands = commands;
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		@sprites["background"] = new IconSprite(0, 0, @viewport);
		@sprites["background"].setBitmap(_INTL("Graphics/UI/jukebox_bg"));
		@sprites["header"] = Window_UnformattedTextPokemon.newWithSize(
			_INTL("Jukebox"), 2, -18, 128, 64, @viewport
		);
		@sprites["header"].baseColor   = new Color(248, 248, 248);
		@sprites["header"].shadowColor = Color.black;
		@sprites["header"].windowskin  = null;
		@sprites["commands"] = Window_CommandPokemon.newWithSize(
			@commands, 94, 92, 324, 224, @viewport
		);
		@sprites["commands"].windowskin = null;
		FadeInAndShow(@sprites) { Update };
	}

	public void Scene() {
		ret = -1;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (Input.trigger(Input.BACK)) {
				break;
			} else if (Input.trigger(Input.USE)) {
				ret = @sprites["commands"].index;
				break;
			}
		}
		return ret;
	}

	public void SetCommands(newcommands, newindex) {
		@sprites["commands"].commands = (!newcommands) ? @commands : newcommands;
		@sprites["commands"].index    = newindex;
	}

	public void EndScene() {
		FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonJukeboxScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen() {
		commands = new List<string>();
		cmdMarch   = -1;
		cmdLullaby = -1;
		cmdOak     = -1;
		cmdCustom  = -1;
		cmdTurnOff = -1;
		commands[cmdMarch = commands.length]   = _INTL("Play: Pokémon March");
		commands[cmdLullaby = commands.length] = _INTL("Play: Pokémon Lullaby");
		commands[cmdOak = commands.length]     = _INTL("Play: Oak");
		commands[cmdCustom = commands.length]  = _INTL("Play: Custom...");
		commands[cmdTurnOff = commands.length] = _INTL("Stop");
		commands[commands.length]              = _INTL("Exit");
		@scene.StartScene(commands);
		do { //loop; while (true);
			cmd = @scene.Scene;
			if (cmd < 0) {
				PlayCloseMenuSE;
				break;
			} else if (cmdMarch >= 0 && cmd == cmdMarch) {
				PlayDecisionSE;
				BGMPlay("Radio - March", 100, 100);
				if (Game.GameData.PokemonMap) {
					Game.GameData.PokemonMap.lower_encounter_rate = false;
					Game.GameData.PokemonMap.higher_encounter_rate = true;
				}
			} else if (cmdLullaby >= 0 && cmd == cmdLullaby) {
				PlayDecisionSE;
				BGMPlay("Radio - Lullaby", 100, 100);
				if (Game.GameData.PokemonMap) {
					Game.GameData.PokemonMap.lower_encounter_rate = true;
					Game.GameData.PokemonMap.higher_encounter_rate = false;
				}
			} else if (cmdOak >= 0 && cmd == cmdOak) {
				PlayDecisionSE;
				BGMPlay("Radio - Oak", 100, 100);
				if (Game.GameData.PokemonMap) {
					Game.GameData.PokemonMap.lower_encounter_rate = false;
					Game.GameData.PokemonMap.higher_encounter_rate = false;
				}
			} else if (cmdCustom >= 0 && cmd == cmdCustom) {
				PlayDecisionSE;
				files = new List<string>();
				Dir.chdir("Audio/BGM/") do;
					Dir.glob("*.wav", f => { files.Add(f); });
					Dir.glob("*.ogg", f => { files.Add(f); });
					Dir.glob("*.mp3", f => { files.Add(f); });
					Dir.glob("*.midi", f => { files.Add(f); });
					Dir.glob("*.mid", f => { files.Add(f); });
					Dir.glob("*.wma", f => { files.Add(f); });
				}
				files.map! { |f| File.basename(f, ".*") };
				files.uniq!;
				files.sort! { |a, b| a.downcase <=> b.downcase };
				@scene.SetCommands(files, 0);
				do { //loop; while (true);
					cmd2 = @scene.Scene;
					if (cmd2 < 0) {
						PlayCancelSE
						break;
					}
					PlayDecisionSE;
					Game.GameData.game_system.setDefaultBGM(files[cmd2]);
					if (Game.GameData.PokemonMap) {
						Game.GameData.PokemonMap.lower_encounter_rate = false;
						Game.GameData.PokemonMap.higher_encounter_rate = false;
					}
				}
				@scene.SetCommands(null, cmdCustom);
			} else if (cmdTurnOff >= 0 && cmd == cmdTurnOff) {
				PlayDecisionSE;
				Game.GameData.game_system.setDefaultBGM(null);
				BGMPlay(ResolveAudioFile(Game.GameData.game_map.bgm_name, Game.GameData.game_map.bgm.volume, Game.GameData.game_map.bgm.pitch));
				if (Game.GameData.PokemonMap) {
					Game.GameData.PokemonMap.lower_encounter_rate = false;
					Game.GameData.PokemonMap.higher_encounter_rate = false;
				}
			} else {   // Exit
				PlayCloseMenuSE;
				break;
			}
		}
		@scene.EndScene;
	}
}
