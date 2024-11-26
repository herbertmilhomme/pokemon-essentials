//===============================================================================
//
//===============================================================================
public partial class PokemonTrade_Scene {
	public void Update() {
		UpdateSpriteHash(@sprites);
	}

	public void RunPictures(pictures, sprites) {
		do { //loop; while (true);
			pictures.each(pic => pic.update);
			sprites.each_with_index do |sprite, i|
				if (sprite.is_a(IconSprite)) {
					setPictureIconSprite(sprite, pictures[i]);
				} else {
					setPictureSprite(sprite, pictures[i]);
				}
			}
			Graphics.update;
			Input.update;
			running = false;
			pictures.each(pic => { if (pic.running()) running = true; });
			if (!running) break;
		}
	}

	public void StartScreen(pokemon, pokemon2, trader1, trader2) {
		@sprites = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@pokemon  = pokemon;
		@pokemon2 = pokemon2;
		@trader1  = trader1;
		@trader2  = trader2;
		addBackgroundOrColoredPlane(@sprites, "background", "trade_bg",
																new Color(248, 248, 248), @viewport);
		@sprites["rsprite1"] = new PokemonSprite(@viewport);
		@sprites["rsprite1"].setPokemonBitmap(@pokemon, false);
		@sprites["rsprite1"].setOffset(PictureOrigin.BOTTOM);
		@sprites["rsprite1"].x = Graphics.width / 2;
		@sprites["rsprite1"].y = 264;
		@sprites["rsprite1"].z = 10;
		@pokemon.species_data.apply_metrics_to_sprite(@sprites["rsprite1"], 1);
		@sprites["rsprite2"] = new PokemonSprite(@viewport);
		@sprites["rsprite2"].setPokemonBitmap(@pokemon2, false);
		@sprites["rsprite2"].setOffset(PictureOrigin.BOTTOM);
		@sprites["rsprite2"].x = Graphics.width / 2;
		@sprites["rsprite2"].y = 264;
		@sprites["rsprite2"].z = 10;
		@pokemon2.species_data.apply_metrics_to_sprite(@sprites["rsprite2"], 1);
		@sprites["rsprite2"].visible = false;
		@sprites["msgwindow"] = CreateMessageWindow(@viewport);
		FadeInAndShow(@sprites);
	}

	public void Scene1() {
		spriteBall = new IconSprite(0, 0, @viewport);
		pictureBall = new PictureEx(0);
		picturePoke = new PictureEx(0);
		ballimage = string.Format("Graphics/Battle animations/ball_{0}", @pokemon.poke_ball);
		ballopenimage = string.Format("Graphics/Battle animations/ball_{0}_open", @pokemon.poke_ball);
		// Starting position of ball
		pictureBall.setXY(0, Graphics.width / 2, 48);
		pictureBall.setName(0, ballimage);
		pictureBall.setSrcSize(0, 32, 64);
		pictureBall.setOrigin(0, PictureOrigin.CENTER);
		pictureBall.setVisible(0, true);
		// Starting position of sprite
		picturePoke.setXY(0, @sprites["rsprite1"].x, @sprites["rsprite1"].y);
		picturePoke.setOrigin(0, PictureOrigin.BOTTOM);
		picturePoke.setVisible(0, true);
		// Change Pokémon color
		picturePoke.moveColor(2, 5, new Color(248, 176, 140));
		// Recall
		delay = picturePoke.totalDuration;
		picturePoke.setSE(delay, "Battle recall");
		pictureBall.setName(delay, ballopenimage);
		pictureBall.setSrcSize(delay, 32, 64);
		// Move sprite to ball
		picturePoke.moveZoom(delay, 8, 0);
		picturePoke.moveXY(delay, 8, Graphics.width / 2, 48);
		picturePoke.setSE(delay + 5, "Battle jump to ball");
		picturePoke.setVisible(delay + 8, false);
		delay = picturePoke.totalDuration + 1;
		pictureBall.setName(delay, ballimage);
		pictureBall.setSrcSize(delay, 32, 64);
		// Make Poké Ball go off the top of the screen
		delay = picturePoke.totalDuration + 10;
		pictureBall.moveXY(delay, 6, Graphics.width / 2, -32);
		// Play animation
		RunPictures(
			new {picturePoke, pictureBall},
			new {@sprites["rsprite1"], spriteBall}
		);
		spriteBall.dispose;
	}

	public void Scene2() {
		spriteBall = new IconSprite(0, 0, @viewport);
		pictureBall = new PictureEx(0);
		picturePoke = new PictureEx(0);
		ballimage = string.Format("Graphics/Battle animations/ball_{0}", @pokemon2.poke_ball);
		ballopenimage = string.Format("Graphics/Battle animations/ball_{0}_open", @pokemon2.poke_ball);
		// Starting position of ball
		pictureBall.setXY(0, Graphics.width / 2, -32);
		pictureBall.setName(0, ballimage);
		pictureBall.setSrcSize(0, 32, 64);
		pictureBall.setOrigin(0, PictureOrigin.CENTER);
		pictureBall.setVisible(0, true);
		// Starting position of sprite
		picturePoke.setOrigin(0, PictureOrigin.BOTTOM);
		picturePoke.setZoom(0, 0);
		picturePoke.setColor(0, new Color(248, 176, 240));
		picturePoke.setVisible(0, false);
		// Dropping ball
		y = Graphics.height - 96 - 16 - 16;   // end point of Poké Ball
		delay = picturePoke.totalDuration + 2;
		for (int i = 4; i < 4; i++) { //for '4' times do => |i|
			t = new {4, 4, 3, 2}[i];   // Time taken to rise or fall for each bounce
			d = new {1, 2, 4, 8}[i];   // Fraction of the starting height each bounce rises to
			if (i == 0) delay -= t;
			if (i > 0) {
				pictureBall.setZoomXY(delay, 100 + (5 * (5 - i)), 100 - (5 * (5 - i)));   // Squish
				pictureBall.moveZoom(delay, 2, 100);                      // Unsquish
				pictureBall.moveXY(delay, t, Graphics.width / 2, y - (100 / d));
			}
			pictureBall.moveXY(delay + t, t, Graphics.width / 2, y);
			pictureBall.setSE(delay + (2 * t), "Battle ball drop");
			delay = pictureBall.totalDuration;
		}
		picturePoke.setXY(delay, Graphics.width / 2, y);
		// Open Poké Ball
		delay = pictureBall.totalDuration + 15;
		pictureBall.setSE(delay, "Battle recall");
		pictureBall.setName(delay, ballopenimage);
		pictureBall.setSrcSize(delay, 32, 64);
		pictureBall.setVisible(delay + 5, false);
		// Pokémon appears and enlarges
		picturePoke.setVisible(delay, true);
		picturePoke.moveZoom(delay, 8, 100);
		picturePoke.moveXY(delay, 8, Graphics.width / 2, @sprites["rsprite2"].y);
		// Return Pokémon's color to normal and play cry
		delay = picturePoke.totalDuration;
		picturePoke.moveColor(delay, 5, new Color(248, 176, 240, 0));
		cry = GameData.Species.cry_filename_from_pokemon(@pokemon2);
		if (cry) picturePoke.setSE(delay, cry);
		cry_length = (GameData.Species.cry_length(@pokemon2) * 20).ceil;
		picturePoke.setVisible(delay + cry_length + 4, true);   // Time for the cry to play
		// Play animation
		RunPictures(
			new {picturePoke, pictureBall},
			new {@sprites["rsprite2"], spriteBall}
		);
		spriteBall.dispose;
	}

	public void EndScreen(need_fade_out = true) {
		if (@sprites["msgwindow"]) DisposeMessageWindow(@sprites["msgwindow"]);
		if (need_fade_out) FadeOutAndHide(@sprites);
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
		newspecies = @pokemon2.check_evolution_on_trade(@pokemon);
		if (newspecies) {
			evo = new PokemonEvolutionScene();
			evo.StartScreen(@pokemon2, newspecies);
			evo.Evolution(false);
			evo.EndScreen;
		}
	}

	public void Trade() {
		was_owned = Game.GameData.player.owned(@pokemon2.species);
		Game.GameData.player.pokedex.register(@pokemon2);
		Game.GameData.player.pokedex.set_owned(@pokemon2.species);
		BGMStop;
		@pokemon.play_cry;
		speciesname1 = GameData.Species.get(@pokemon.species).name;
		speciesname2 = GameData.Species.get(@pokemon2.species).name;
		MessageDisplay(@sprites["msgwindow"],
										string.Format("{1:s}\nID: {2:05d}   OT: {3:s}",
															@pokemon.name, @pokemon.owner.public_id, @pokemon.owner.name) + "\\wtnp[0]") { Update };
		MessageWaitForInput(@sprites["msgwindow"], 50, true) { Update };
		PlayDecisionSE;
		BGMPlay("Evolution");
		Scene1;
		MessageDisplay(@sprites["msgwindow"],
										_INTL("For {1}'s {2},\n{3} sends {4}.", @trader1, speciesname1, @trader2, speciesname2) + "\1") { Update };
		MessageDisplay(@sprites["msgwindow"],
										_INTL("{1} bids farewell to {2}.", @trader2, speciesname2)) { Update };
		Scene2;
		BGMStop;
		MEPlay("Battle capture success");
		MessageDisplay(@sprites["msgwindow"],
										string.Format("{1:s}\nID: {2:05d}   OT: {3:s}",
															@pokemon2.name, @pokemon2.owner.public_id, @pokemon2.owner.name) + "\1") { Update };
		MessageDisplay(@sprites["msgwindow"],
										_INTL("Take good care of {1}.", speciesname2)) { Update };
		// Show Pokédex entry for new species if it hasn't been owned before
		if (Settings.SHOW_NEW_SPECIES_POKEDEX_ENTRY_MORE_OFTEN && !was_owned &&
			Game.GameData.player.has_pokedex && Game.GameData.player.pokedex.species_in_unlocked_dex(@pokemon2.species)) {
			MessageDisplay(@sprites["msgwindow"],
											_INTL("{1}'s data was added to the Pokédex.", speciesname2)) { Update };
			Game.GameData.player.pokedex.register_last_seen(@pokemon2);
			FadeOutIn do;
				scene = new PokemonPokedexInfo_Scene();
				screen = new PokemonPokedexInfoScreen(scene);
				screen.DexEntry(@pokemon2.species);
				EndScreen(false);
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public void StartTrade(pokemonIndex, newpoke, nickname, trainerName, trainerGender = 0) {
	Game.GameData.stats.trade_count += 1;
	myPokemon = Game.GameData.player.party[pokemonIndex];
	yourPokemon = null;
	resetmoves = true;
	if (newpoke.is_a(Pokemon)) {
		newpoke.owner = Pokemon.Owner.new_foreign(trainerName, trainerGender);
		yourPokemon = newpoke;
		resetmoves = false;
	} else {
		species_data = GameData.Species.try_get(newpoke);
		if (!species_data) raise _INTL("Species {1} does not exist.", newpoke);
		yourPokemon = new Pokemon(species_data.id, myPokemon.level);
		yourPokemon.owner = Pokemon.Owner.new_foreign(trainerName, trainerGender);
	}
	yourPokemon.name          = nickname;
	yourPokemon.obtain_method = 2;   // traded
	if (resetmoves) yourPokemon.reset_moves;
	yourPokemon.record_first_moves;
	FadeOutInWithMusic do;
		evo = new PokemonTrade_Scene();
		evo.StartScreen(myPokemon, yourPokemon, Game.GameData.player.name, trainerName);
		evo.Trade;
		evo.EndScreen;
	}
	Game.GameData.player.party[pokemonIndex] = yourPokemon;
}
