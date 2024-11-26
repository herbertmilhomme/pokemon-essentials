//===============================================================================
// * Egg Hatch Animation - by FL (Credits will be apreciated)
//                         Tweaked by Maruno
//-------------------------------------------------------------------------------
// This script is for Pokémon Essentials. It's an egg hatch animation that
// works even with special eggs like Manaphy egg.
//-------------------------------------------------------------------------------
// To this script works, put it above Main and put a picture (a 5 frames
// sprite sheet) with egg sprite height and 5 times the egg sprite width at
// Graphics/Battlers/eggCracks.
//===============================================================================
public partial class PokemonEggHatch_Scene {
	public void StartScene(pokemon) {
		@sprites = new List<string>();
		@pokemon = pokemon;
		@nicknamed = false;
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		// Create background image
		addBackgroundOrColoredPlane(@sprites, "background", "hatch_bg",
																new Color(248, 248, 248), @viewport);
		// Create egg sprite/Pokémon sprite
		@sprites["pokemon"] = new PokemonSprite(@viewport);
		@sprites["pokemon"].setOffset(PictureOrigin.BOTTOM);
		@sprites["pokemon"].x = Graphics.width / 2;
		@sprites["pokemon"].y = 264 + 56;   // 56 to offset the egg sprite
		@sprites["pokemon"].setSpeciesBitmap(@pokemon.species, @pokemon.gender,
																				@pokemon.form, @pokemon.shiny(),
																				false, false, true);   // Egg sprite
		// Load egg cracks bitmap
		crackfilename = GameData.Species.egg_cracks_sprite_filename(@pokemon.species, @pokemon.form);
		@hatchSheet = new AnimatedBitmap(crackfilename);
		// Create egg cracks sprite
		@sprites["hatch"] = new Sprite(@viewport);
		@sprites["hatch"].x = @sprites["pokemon"].x;
		@sprites["hatch"].y = @sprites["pokemon"].y;
		@sprites["hatch"].ox = @sprites["pokemon"].ox;
		@sprites["hatch"].oy = @sprites["pokemon"].oy;
		@sprites["hatch"].bitmap = @hatchSheet.bitmap;
		@sprites["hatch"].src_rect = new Rect(0, 0, @hatchSheet.width / 5, @hatchSheet.height);
		@sprites["hatch"].visible = false;
		// Create flash overlay
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["overlay"].z = 200;
		@sprites["overlay"].bitmap = new Bitmap(Graphics.width, Graphics.height);
		@sprites["overlay"].bitmap.fill_rect(0, 0, Graphics.width, Graphics.height, Color.white);
		@sprites["overlay"].opacity = 0;
		// Start up scene
		FadeInAndShow(@sprites);
	}

	public void Main() {
		BGMPlay("Evolution");
		// Egg animation
		updateScene(1.5);
		PositionHatchMask(0);
		SEPlay("Battle ball shake");
		swingEgg(4);
		updateScene(0.2);
		PositionHatchMask(1);
		SEPlay("Battle ball shake");
		swingEgg(4);
		updateScene(0.4);
		PositionHatchMask(2);
		SEPlay("Battle ball shake");
		swingEgg(8, 2);
		updateScene(0.4);
		PositionHatchMask(3);
		SEPlay("Battle ball shake");
		swingEgg(16, 4);
		updateScene(0.2);
		PositionHatchMask(4);
		SEPlay("Battle recall");
		// Fade and change the sprite
		timer_start = System.uptime;
		do { //loop; while (true);
			tone_val = lerp(0, 255, 0.4, timer_start, System.uptime);
			@sprites["pokemon"].tone = new Tone(tone_val, tone_val, tone_val);
			@sprites["overlay"].opacity = tone_val;
			updateScene;
			if (tone_val >= 255) break;
		}
		updateScene(0.75);
		@sprites["pokemon"].setPokemonBitmap(@pokemon); // Pokémon sprite
		@sprites["pokemon"].x = Graphics.width / 2;
		@sprites["pokemon"].y = 264;
		@pokemon.species_data.apply_metrics_to_sprite(@sprites["pokemon"], 1);
		@sprites["hatch"].visible = false;
		timer_start = System.uptime;
		do { //loop; while (true);
			tone_val = lerp(255, 0, 0.4, timer_start, System.uptime);
			@sprites["pokemon"].tone = new Tone(tone_val, tone_val, tone_val);
			@sprites["overlay"].opacity = tone_val;
			updateScene;
			if (tone_val <= 0) break;
		}
		// Finish scene
		cry_duration = GameData.Species.cry_length(@pokemon);
		@pokemon.play_cry;
		updateScene(cry_duration + 0.1);
		BGMStop;
		MEPlay("Evolution success");
		@pokemon.name = null;
		Message("\\se[]" + _INTL("{1} hatched from the Egg!", @pokemon.name) + "\\wt[80]") { update };
		// Record the Pokémon's species as owned in the Pokédex
		was_owned = Game.GameData.player.owned(@pokemon.species);
		Game.GameData.player.pokedex.register(@pokemon);
		Game.GameData.player.pokedex.set_owned(@pokemon.species);
		Game.GameData.player.pokedex.set_seen_egg(@pokemon.species);
		// Show Pokédex entry for new species if it hasn't been owned before
		if (Settings.SHOW_NEW_SPECIES_POKEDEX_ENTRY_MORE_OFTEN && !was_owned &&
			Game.GameData.player.has_pokedex && Game.GameData.player.pokedex.species_in_unlocked_dex(@pokemon.species)) {
			Message(_INTL("{1}'s data was added to the Pokédex.", @pokemon.name)) { update };
			Game.GameData.player.pokedex.register_last_seen(@pokemon);
			FadeOutIn do;
				scene = new PokemonPokedexInfo_Scene();
				screen = new PokemonPokedexInfoScreen(scene);
				screen.DexEntry(@pokemon.species);
			}
		}
		// Nickname the Pokémon
		if (Game.GameData.PokemonSystem.givenicknames == 0 &&
			ConfirmMessage(
				_INTL("Would you like to nickname the newly hatched {1}?", @pokemon.name),
				() => update())) {
			nickname = EnterPokemonName(_INTL("{1}'s nickname?", @pokemon.name),
																		0, Pokemon.MAX_NAME_SIZE, "", @pokemon, true);
			@pokemon.name = nickname;
			@nicknamed = true;
		}
	}

	public void EndScene() {
		if (!@nicknamed) FadeOutAndHide(@sprites) { update };
		DisposeSpriteHash(@sprites);
		@hatchSheet.dispose;
		@viewport.dispose;
	}

	public void PositionHatchMask(index) {
		@sprites["hatch"].src_rect.x = index * @sprites["hatch"].src_rect.width;
	}

	public void swingEgg(speed, swingTimes = 1) {
		@sprites["hatch"].visible = true;
		amplitude = 8;
		duration = 0.05 * amplitude / speed;
		targets = new List<string>();
		swingTimes.times do;
			targets.Add(@sprites["pokemon"].x + amplitude);
			targets.Add(@sprites["pokemon"].x - amplitude);
		}
		targets.Add(@sprites["pokemon"].x);
		targets.each_with_index do |target, i|
			timer_start = System.uptime;
			start_x = @sprites["pokemon"].x;
			do { //loop; while (true);
				if (i.even() && @sprites["pokemon"].x >= target) break;
				if (i.odd() && @sprites["pokemon"].x <= target) break;
				@sprites["pokemon"].x = lerp(start_x, target, duration, timer_start, System.uptime);
				@sprites["hatch"].x = @sprites["pokemon"].x;
				updateScene;
			}
		}
		@sprites["pokemon"].x = targets[targets.length - 1];
		@sprites["hatch"].x   = @sprites["pokemon"].x;
	}

	// Can be used for "wait" effect.
	public void updateScene(duration = 0.01) {
		timer_start = System.uptime;
		while (System.uptime - timer_start < duration) {
			Graphics.update;
			Input.update;
			self.update;
		}
	}

	public void update() {
		UpdateSpriteHash(@sprites);
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonEggHatchScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen(pokemon) {
		@scene.StartScene(pokemon);
		@scene.Main;
		@scene.EndScene;
	}
}

//===============================================================================
//
//===============================================================================
public void HatchAnimation(pokemon) {
	Message(_INTL("Huh?") + "\1");
	FadeOutInWithMusic do;
		scene = new PokemonEggHatch_Scene();
		screen = new PokemonEggHatchScreen(scene);
		screen.StartScreen(pokemon);
	}
	return true;
}

public void Hatch(pokemon) {
	Game.GameData.stats.eggs_hatched += 1;
	speciesname = pokemon.speciesName;
	pokemon.name           = null;
	pokemon.owner          = Pokemon.Owner.new_from_trainer(Game.GameData.player);
	pokemon.happiness      = 120;
	pokemon.steps_to_hatch = 0;
	pokemon.timeEggHatched = Time.now.ToInt();
	pokemon.obtain_method  = 1;   // hatched from egg
	pokemon.hatched_map    = Game.GameData.game_map.map_id;
	pokemon.record_first_moves;
	if (!HatchAnimation(pokemon)) {
		Message(_INTL("Huh?") + "\1");
		Message(_INTL("...") + "\1");
		Message(_INTL("... .... .....") + "\1");
		Message(_INTL("{1} hatched from the Egg!", speciesname));
		was_owned = Game.GameData.player.owned(pokemon.species);
		Game.GameData.player.pokedex.register(pokemon);
		Game.GameData.player.pokedex.set_owned(pokemon.species);
		Game.GameData.player.pokedex.set_seen_egg(pokemon.species);
		// Show Pokédex entry for new species if it hasn't been owned before
		if (Settings.SHOW_NEW_SPECIES_POKEDEX_ENTRY_MORE_OFTEN && !was_owned &&
			Game.GameData.player.has_pokedex && Game.GameData.player.pokedex.species_in_unlocked_dex(pokemon.species)) {
			Message(_INTL("{1}'s data was added to the Pokédex.", speciesname));
			Game.GameData.player.pokedex.register_last_seen(pokemon);
			FadeOutIn do;
				scene = new PokemonPokedexInfo_Scene();
				screen = new PokemonPokedexInfoScreen(scene);
				screen.DexEntry(pokemon.species);
			}
		}
		// Nickname the Pokémon
		if (Game.GameData.PokemonSystem.givenicknames == 0 &&
			ConfirmMessage(_INTL("Would you like to nickname the newly hatched {1}?", speciesname))) {
			nickname = EnterPokemonName(_INTL("{1}'s nickname?", speciesname),
																		0, Pokemon.MAX_NAME_SIZE, "", pokemon);
			pokemon.name = nickname;
		}
	}
}

EventHandlers.add(:on_player_step_taken, :hatch_eggs,
	block: () => {
		foreach (var egg in Game.GameData.player.party) { //'Game.GameData.player.party.each' do => |egg|
			if (egg.steps_to_hatch <= 0) continue;
			egg.steps_to_hatch -= 1;
			foreach (var pkmn in Game.GameData.player.pokemon_party) { //'Game.GameData.player.pokemon_party.each' do => |pkmn|
				if (!pkmn.ability&.has_flag("FasterEggHatching")) continue;
				egg.steps_to_hatch -= 1;
				break;
			}
			if (egg.steps_to_hatch <= 0) {
				egg.steps_to_hatch = 0;
				Hatch(egg);
			}
		}
	}
)
