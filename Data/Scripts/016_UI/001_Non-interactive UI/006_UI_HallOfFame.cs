//===============================================================================
// * Hall of Fame - by FL (Credits will be apreciated)
//-------------------------------------------------------------------------------
// This script is for Pokémon Essentials. It makes a recordable Hall of Fame
// like the Gen 3 games.
//-------------------------------------------------------------------------------
// To this scripts works, put it above main, put a 512x384 picture in
// hallfamebars and a 8x24 background picture in hallfamebg. To call this script,
// use 'HallOfFameEntry'. After you recorder the first entry, you can access
// the hall teams using a PC. You can also check the player Hall of Fame last
// number using 'Game.GameData.PokemonGlobal.hallOfFameLastNumber'.
//===============================================================================
public partial class HallOfFame_Scene {
	// When true, all pokémon will be in one line.
	// When false, all pokémon will be in two lines.
	public const bool SINGLE_ROW_OF_POKEMON = false;
	// Make the pokémon movement ON in hall entry.
	public const bool ANIMATION = true;
	// Time in seconds for a Pokémon to slide to its position from off-screen.
	public const int APPEAR_SPEED = 0.4;
	// Entry wait time (in seconds) between showing each Pokémon (and trainer).
	public const int ENTRY_WAIT_TIME = 3.0;
	// Wait time (in seconds) when showing "Welcome to the Hall of Fame!".
	public const int WELCOME_WAIT_TIME = 4.0;
	// Maximum number limit of simultaneous hall entries saved.
	// 0 = Doesn't save any hall. -1 = no limit
	// Prefer to use larger numbers (like 500 and 1000) than don't put a limit.
	// If a player exceed this limit, the first one will be removed.
	public const int HALL_ENTRIES_LIMIT = 50;
	// The entry music name. Put "" to doesn't play anything.
	public const string HALL_OF_FAME_BGM = "Hall of Fame";
	// Allow eggs to be show and saved in hall.
	public const bool ALLOW_EGGS = true;
	// Remove the hallbars when the trainer sprite appears.
	public const bool REMOVE_BARS_WHEN_SHOWING_TRAINER = true;
	// The final fade speed on entry.
	public const int FINAL_FADE_DURATION = 1.0;
	// Sprite's opacity value when it isn't selected.
	public const int OPACITY = 64;
	TEXT_BASE_COLOR   = new Color(248, 248, 248);
	TEXT_SHADOW_COLOR = new Color(0, 0, 0);

	// Placement for pokemon icons
	public void StartScene() {
		@sprites = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		// Comment the below line to doesn't use a background
		addBackgroundPlane(@sprites, "bg", "Hall of Fame/bg", @viewport);
		@sprites["hallbars"] = new IconSprite(@viewport);
		@sprites["hallbars"].setBitmap("Graphics/UI/Hall of Fame/bars");
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["overlay"].z = 10;
		SetSystemFont(@sprites["overlay"].bitmap);
		@alreadyFadedInEnd = false;
		@useMusic = false;
		@battlerIndex = 0;
		@hallEntry = new List<string>();
		@nationalDexList = [:NONE];
		GameData.Species.each_species(s => @nationalDexList.Add(s.species));
	}

	public void StartSceneEntry() {
		StartScene;
		@useMusic = (HALL_OF_FAME_BGM && HALL_OF_FAME_BGM != "");
		if (@useMusic) BGMPlay(HALL_OF_FAME_BGM);
		saveHallEntry;
		@movements = new List<string>();
		createBattlers;
		FadeInAndShow(@sprites) { Update };
	}

	public void StartScenePC() {
		StartScene;
		@hallIndex = Game.GameData.PokemonGlobal.hallOfFame.size - 1;
		@hallEntry = Game.GameData.PokemonGlobal.hallOfFame[-1];
		createBattlers(false);
		FadeInAndShow(@sprites) { Update };
		UpdatePC;
	}

	public void EndScene() {
		if (@useMusic) Game.GameData.game_map.autoplay;
		if (@sprites.Contains("msgwindow")) DisposeMessageWindow(@sprites["msgwindow"]);
		if (!@alreadyFadedInEnd) FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void slowFadeOut(duration) {
		col = new Color(0, 0, 0, 0);
		timer_start = System.uptime;
		do { //loop; while (true);
			col.alpha = lerp(0, 255, duration, timer_start, System.uptime);
			@viewport.color = col;
			Graphics.update;
			Input.update;
			Update;
			if (col.alpha == 255) break;
		}
	}

	// Dispose the sprite if the sprite exists and make it null
	public void restartSpritePosition(sprites, spritename) {
		if (sprites.Contains(spritename) && sprites[spritename]) sprites[spritename].dispose;
		sprites[spritename] = null;
	}

	// Change the pokémon sprites opacity except the index one
	public void setPokemonSpritesOpacity(index, opacity = 255) {
		for (int n = @hallEntry.size; n < @hallEntry.size; n++) { //for '@hallEntry.size' times do => |n|
			if (@sprites[$"pokemon{n}"]) @sprites[$"pokemon{n}"].opacity = (n == index) ? 255 : opacity;
		}
	}

	public void saveHallEntry() {
		foreach (var pkmn in Game.GameData.player.party) { //'Game.GameData.player.party.each' do => |pkmn|
			// Clones every pokémon object
			if (!pkmn.egg() || ALLOW_EGGS) @hallEntry.Add(pkmn.clone);
		}
		// Update the global variables
		Game.GameData.PokemonGlobal.hallOfFame.Add(@hallEntry);
		Game.GameData.PokemonGlobal.hallOfFameLastNumber += 1;
		if (HALL_ENTRIES_LIMIT >= 0 && Game.GameData.PokemonGlobal.hallOfFame.size > HALL_ENTRIES_LIMIT) {
			Game.GameData.PokemonGlobal.hallOfFame.delete_at(0);
		}
	}

	// Return the x/y point position in screen for battler index number
	// Don't use odd numbers!
	public void xpointformula(battlernumber) {
		if (SINGLE_ROW_OF_POKEMON) {
			ret = ((60 * (battlernumber / 2)) + 48) * (xpositionformula(battlernumber) - 1);   // -48, 48, -108, 108, -168, 168
			return ret + (Graphics.width / 2);                   // 208, 304, 148, 364, 88, 424
		}
		return 96 + (160 * xpositionformula(battlernumber));   // 256, 96, 456, 256, 456, 96
	}

	public void ypointformula(battlernumber) {
		if (SINGLE_ROW_OF_POKEMON) return 180 - (32 * (battlernumber / 2));   // 180, 180, 148, 148, 116, 116
		return 96 + (64 * ypositionformula(battlernumber));                 // 90, 90, 90, 160, 160, 160
	}

	// Returns 0, 1 or 2 as the x position value (left, middle, right column)
	public void xpositionformula(battlernumber) {
		if (SINGLE_ROW_OF_POKEMON) return (battlernumber % 2) * 2;       // 0, 2, 0, 2, 0, 2
		if ((battlernumber / 3).even()) return (1 - battlernumber) % 3;   // First 3 mons: 1, 0, 2
		return (1 + battlernumber) % 3;                                // Second 3 mons: 1, 2, 0
	}

	// Returns 0, 1 or 2 as the y position value (top, middle, bottom row)
	public void ypositionformula(battlernumber) {
		if (SINGLE_ROW_OF_POKEMON) return 1;      // 1, 1, 1, 1, 1, 1
		return ((battlernumber / 3) % 2) * 2;   // 0, 0, 0, 2, 2, 2
	}

	public void moveSprite(i) {
		spritename = (i > -1) ? $"pokemon{i}" : "trainer";
		if (!ANIMATION) {   // Skips animation, place directly in end position
			@sprites[spritename].x = @movements[i][1];
			@sprites[spritename].y = @movements[i][3];
			@movements[i][0] = @movements[i][1];
			@movements[i][2] = @movements[i][3];
			return;
		}
		if (!@movements[i][4]) @movements[i][4] = System.uptime;
		speed = (i > -1) ? APPEAR_SPEED : APPEAR_SPEED * 3;
		@sprites[spritename].x = lerp(@movements[i][0], @movements[i][1], speed, @movements[i][4], System.uptime);
		@sprites[spritename].y = lerp(@movements[i][2], @movements[i][3], speed, @movements[i][4], System.uptime);
		if (@sprites[spritename].x == @movements[i][1]) @movements[i][0] = @movements[i][1];
		if (@sprites[spritename].y == @movements[i][3]) @movements[i][2] = @movements[i][3];
	}

	public void createBattlers(hide = true) {
		// Movement in animation
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			// Clear all pokémon sprites and dispose the ones that exists every time
			// that this method is call
			restartSpritePosition(@sprites, $"pokemon{i}");
			if (i >= @hallEntry.size) continue;
			end_x = xpointformula(i);
			end_y = ypointformula(i);
			@sprites[$"pokemon{i}"] = new PokemonSprite(@viewport);
			@sprites[$"pokemon{i}"].setPokemonBitmap(@hallEntry[i]);
			// This method doesn't put the exact coordinates
			@sprites[$"pokemon{i}"].x = end_x;
			@sprites[$"pokemon{i}"].y = end_y;
			if (SINGLE_ROW_OF_POKEMON) @sprites[$"pokemon{i}"].z = Settings.MAX_PARTY_SIZE - i;
			if (!hide) continue;
			// Animation distance calculation
			x_direction = xpositionformula(i) - 1;
			y_direction = ypositionformula(i) - 1;
			distance = 0;
			if (y_direction == 0) {
				distance = (x_direction > 0) ? end_x : Graphics.width - end_x;
				distance += @sprites[$"pokemon{i}"].bitmap.width / 2;
			} else {
				distance = (y_direction > 0) ? end_y : Graphics.height - end_y;
				distance += @sprites[$"pokemon{i}"].bitmap.height / 2;
			}
			start_x = end_x - (x_direction * distance);
			start_y = end_y - (y_direction * distance);
			@sprites[$"pokemon{i}"].x = start_x;
			@sprites[$"pokemon{i}"].y = start_y;
			@movements[i] = new {start_x, end_x, start_y, end_y};
		}
	}

	public void createTrainerBattler() {
		@sprites["trainer"] = new IconSprite(@viewport);
		@sprites["trainer"].setBitmap(GameData.TrainerType.player_front_sprite_filename(Game.GameData.player.trainer_type));
		if (SINGLE_ROW_OF_POKEMON) {
			@sprites["trainer"].x = Graphics.width / 2;
			@sprites["trainer"].y = 208;
		} else {
			@sprites["trainer"].x = Graphics.width - 96;
			@sprites["trainer"].y = 160;
		}
		@movements.Add(new {Graphics.width / 2, @sprites["trainer"].x, @sprites["trainer"].y, @sprites["trainer"].y});
		@sprites["trainer"].z = 9;
		@sprites["trainer"].ox = @sprites["trainer"].bitmap.width / 2;
		@sprites["trainer"].oy = @sprites["trainer"].bitmap.height / 2;
		if (REMOVE_BARS_WHEN_SHOWING_TRAINER) {
			@sprites["overlay"].bitmap.clear;
			@sprites["hallbars"].visible = false;
		}
		if (ANIMATION && !SINGLE_ROW_OF_POKEMON) {   // Trainer Animation
			@sprites["trainer"].x = @movements.last[0];
		} else {
			timer_start = System.uptime;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				Update;
				if (System.uptime - timer_start >= ENTRY_WAIT_TIME) break;
			}
		}
	}

	public void writeTrainerData() {
		if (Game.GameData.PokemonGlobal.hallOfFameLastNumber == 1) {
			totalsec = Game.GameData.stats.time_to_enter_hall_of_fame.ToInt();
		} else {
			totalsec = Game.GameData.stats.play_time.ToInt();
		}
		hour = totalsec / 60 / 60;
		min = totalsec / 60 % 60;
		pubid = string.Format("{0:5}", Game.GameData.player.public_ID);
		lefttext = _INTL("Name<r>{1}", Game.GameData.player.name) + "<br>";
		lefttext += _INTL("ID No.<r>{1}", pubid) + "<br>";
		if (hour > 0) {
			lefttext += _INTL("Time<r>{1}h {2}m", hour, min) + "<br>";
		} else {
			lefttext += _INTL("Time<r>{1}m", min) + "<br>";
		}
		lefttext += _INTL("Pokédex<r>{1}/{2}",
											Game.GameData.player.pokedex.owned_count, Game.GameData.player.pokedex.seen_count) + "<br>";
		@sprites["messagebox"] = new Window_AdvancedTextPokemon(lefttext);
		@sprites["messagebox"].viewport = @viewport;
		if (@sprites["messagebox"].width < 192) @sprites["messagebox"].width = 192;
		@sprites["msgwindow"] = CreateMessageWindow(@viewport);
		MessageDisplay(@sprites["msgwindow"],
										_INTL("League champion!\nCongratulations!") + "\\^");
	}

	public void writePokemonData(pokemon, hallNumber = -1) {
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		pokename = pokemon.name;
		speciesname = pokemon.speciesName;
		if (pokemon.male()) {
			speciesname += "♂";
		} else if (pokemon.female()) {
			speciesname += "♀";
		}
		pokename += "/" + speciesname;
		if (pokemon.egg()) pokename = _INTL("Egg") + "/" + _INTL("Egg");
		idno = (pokemon.owner.name.empty() || pokemon.egg()) ? "?????" : string.Format("{0:5}", pokemon.owner.public_id);
		dexnumber = _INTL("No. ???");
		if (!pokemon.egg()) {
			number = @nationalDexList.index(pokemon.species) || 0;
			dexnumber = string.Format("No. {1:03d}", number);
		}
		textPositions = new {
			new {dexnumber, 32, Graphics.height - 74, :left, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR},
			new {pokename, Graphics.width - 192, Graphics.height - 74, :center, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR},
			new {_INTL("Lv. {1}", pokemon.egg() ? "?" : pokemon.level),
			64, Graphics.height - 42, :left, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR},
			new {_INTL("ID No. {1}", pokemon.egg() ? "?????" : idno),
			Graphics.width - 192, Graphics.height - 42, :center, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR};
		}
		if (hallNumber > -1) {
			textPositions.Add(new {_INTL("Hall of Fame No."), (Graphics.width / 2) - 104, 6, :left, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR});
			textPositions.Add(new {hallNumber.ToString(), (Graphics.width / 2) + 104, 6, :right, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR});
		}
		DrawTextPositions(overlay, textPositions);
	}

	public void writeWelcome() {
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		DrawTextPositions(overlay, new {_INTL("Welcome to the Hall of Fame!"),
																	Graphics.width / 2, Graphics.height - 68, :center, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR});
	}

	public void AnimationLoop() {
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			UpdateAnimation;
			if (@battlerIndex == @hallEntry.size + 2) break;
		}
	}

	public void PCSelection() {
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			continueScene = true;
			if (Input.trigger(Input.BACK)) break;   // Exits
			if (Input.trigger(Input.USE)) {   // Moves the selection one entry backward
				@battlerIndex += 10;
				continueScene = UpdatePC;
			}
			if (Input.trigger(Input.LEFT)) {   // Moves the selection one pokémon forward
				@battlerIndex -= 1;
				continueScene = UpdatePC;
			}
			if (Input.trigger(Input.RIGHT)) {   // Moves the selection one pokémon backward
				@battlerIndex += 1;
				continueScene = UpdatePC;
			}
			if (!continueScene) break;
		}
	}

	public void Update() {
		UpdateSpriteHash(@sprites);
	}

	public void UpdateAnimation() {
		if (@battlerIndex <= @hallEntry.size) {
			if (@movements[@battlerIndex] &&
				(@movements[@battlerIndex][0] != @movements[@battlerIndex][1] ||
				@movements[@battlerIndex][2] != @movements[@battlerIndex][3])) {
				spriteIndex = (@battlerIndex < @hallEntry.size) ? @battlerIndex : -1;
				moveSprite(spriteIndex);
			} else {
				@battlerIndex += 1;
				if (@battlerIndex <= @hallEntry.size) {
					// If it is a pokémon, write the pokémon text, wait the
					// ENTRY_WAIT_TIME and goes to the next battler
					@hallEntry[@battlerIndex - 1].play_cry;
					writePokemonData(@hallEntry[@battlerIndex - 1]);
					timer_start = System.uptime;
					do { //loop; while (true);
						Graphics.update;
						Input.update;
						Update;
						if (System.uptime - timer_start >= ENTRY_WAIT_TIME) break;
					}
					if (@battlerIndex < @hallEntry.size) {   // Preparates the next battler
						setPokemonSpritesOpacity(@battlerIndex, OPACITY);
						@sprites["overlay"].bitmap.clear;
					} else {   // Show the welcome message and prepares the trainer
						setPokemonSpritesOpacity(-1);
						writeWelcome;
						timer_start = System.uptime;
						do { //loop; while (true);
							Graphics.update;
							Input.update;
							Update;
							if (System.uptime - timer_start >= WELCOME_WAIT_TIME) break;
						}
						if (!SINGLE_ROW_OF_POKEMON) setPokemonSpritesOpacity(-1, OPACITY);
						createTrainerBattler;
					}
				}
			}
		} else if (@battlerIndex > @hallEntry.size) {
			// Write the trainer data and fade
			writeTrainerData;
			timer_start = System.uptime;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				Update;
				if (System.uptime - timer_start >= ENTRY_WAIT_TIME) break;
			}
			if (@useMusic) BGMFade(FINAL_FADE_DURATION);
			slowFadeOut(FINAL_FADE_DURATION);
			@alreadyFadedInEnd = true;
			@battlerIndex += 1;
		}
	}

	public void UpdatePC() {
		// Change the team
		if (@battlerIndex >= @hallEntry.size) {
			@hallIndex -= 1;
			if (@hallIndex == -1) return false;
			@hallEntry = Game.GameData.PokemonGlobal.hallOfFame[@hallIndex];
			@battlerIndex = 0;
			createBattlers(false);
		} else if (@battlerIndex < 0) {
			@hallIndex += 1;
			if (@hallIndex >= Game.GameData.PokemonGlobal.hallOfFame.size) return false;
			@hallEntry = Game.GameData.PokemonGlobal.hallOfFame[@hallIndex];
			@battlerIndex = @hallEntry.size - 1;
			createBattlers(false);
		}
		// Change the pokemon
		@hallEntry[@battlerIndex].play_cry;
		setPokemonSpritesOpacity(@battlerIndex, OPACITY);
		hallNumber = Game.GameData.PokemonGlobal.hallOfFameLastNumber + @hallIndex -;
								Game.GameData.PokemonGlobal.hallOfFame.size + 1;
		writePokemonData(@hallEntry[@battlerIndex], hallNumber);
		return true;
	}
}

//===============================================================================
//
//===============================================================================
public partial class HallOfFameScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreenEntry() {
		@scene.StartSceneEntry;
		@scene.AnimationLoop;
		@scene.EndScene;
	}

	public void StartScreenPC() {
		@scene.StartScenePC;
		@scene.PCSelection;
		@scene.EndScene;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonGlobalMetadata {
	public int hallOfFame		{ get { return _hallOfFame; } }			protected int _hallOfFame;
	// Number necessary if hallOfFame array reach in its size limit
	public int hallOfFameLastNumber		{ get { return _hallOfFameLastNumber; } }			protected int _hallOfFameLastNumber;

	public void hallOfFame() {
		if (!@hallOfFame) @hallOfFame = new List<string>();
		return @hallOfFame;
	}

	public void hallOfFameLastNumber() {
		return @hallOfFameLastNumber || 0;
	}
}

//===============================================================================
//
//===============================================================================
public void HallOfFameEntry() {
	scene = new HallOfFame_Scene();
	screen = new HallOfFameScreen(scene);
	screen.StartScreenEntry;
}

public void HallOfFamePC() {
	scene = new HallOfFame_Scene();
	screen = new HallOfFameScreen(scene);
	screen.StartScreenPC;
}

MenuHandlers.add(:pc_menu, :hall_of_fame, {
	"name"      => _INTL("Hall of Fame"),
	"order"     => 40,
	"condition" => () => next Game.GameData.PokemonGlobal.hallOfFameLastNumber > 0,
	"effect"    => menu => {
		Message("\\se[PC access]" + _INTL("Accessed the Hall of Fame."));
		HallOfFamePC;
		next false;
	}
});
