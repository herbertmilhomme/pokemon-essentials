//===============================================================================
// Evolution screen
//===============================================================================
public partial class PokemonEvolutionScene {
	public static void DuplicatePokemon(pkmn, new_species) {
		new_pkmn = pkmn.clone;
		new_pkmn.species   = new_species;
		new_pkmn.name      = null;
		new_pkmn.markings  = new List<string>();
		new_pkmn.poke_ball = :POKEBALL;
		new_pkmn.item      = null;
		new_pkmn.clearAllRibbons;
		new_pkmn.calc_stats;
		new_pkmn.heal;
		// Add duplicate Pokémon to party
		Game.GameData.player.party.Add(new_pkmn);
		// See and own duplicate Pokémon
		Game.GameData.player.pokedex.register(new_pkmn);
		Game.GameData.player.pokedex.set_owned(new_species);
	}

	public void StartScreen(pokemon, newspecies) {
		@pokemon = pokemon;
		@newspecies = newspecies;
		@sprites = new List<string>();
		@bgviewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@bgviewport.z = 99999;
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@msgviewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@msgviewport.z = 99999;
		addBackgroundOrColoredPlane(@sprites, "background", "evolution_bg",
																new Color(248, 248, 248), @bgviewport);
		rsprite1 = new PokemonSprite(@viewport);
		rsprite1.setOffset(PictureOrigin.CENTER);
		rsprite1.setPokemonBitmap(@pokemon, false);
		rsprite1.x = Graphics.width / 2;
		rsprite1.y = (Graphics.height - 64) / 2;
		rsprite2 = new PokemonSprite(@viewport);
		rsprite2.setOffset(PictureOrigin.CENTER);
		rsprite2.setPokemonBitmapSpecies(@pokemon, @newspecies, false);
		rsprite2.x       = rsprite1.x;
		rsprite2.y       = rsprite1.y;
		rsprite2.visible = false;
		@sprites["rsprite1"] = rsprite1;
		@sprites["rsprite2"] = rsprite2;
		@sprites["msgwindow"] = CreateMessageWindow(@msgviewport);
		set_up_animation;
		FadeInAndShow(@sprites) { Update };
	}

	public void set_up_animation() {
		sprite = new PictureEx(0);
		sprite.setVisible(0, true);
		sprite.setColor(0, new Color(255, 255, 255, 0));
		sprite2 = new PictureEx(0);
		sprite2.setVisible(0, true);
		sprite2.setZoom(0, 0.0);
		sprite2.setColor(0, new Color(255, 255, 255, 255));
		// Make sprite turn white
		sprite.moveColor(0, 25, new Color(255, 255, 255, 255));
		total_duration = 9 * 20;   // 9 seconds
		duration = 25 + 15;
		zoom_duration = 12;
		do { //loop; while (true);
			// Shrink prevo sprite, enlarge evo sprite
			sprite.moveZoom(duration, zoom_duration, 0);
			sprite2.moveZoom(duration, zoom_duration, 110);
			duration += zoom_duration;
			// If animation has played for long enough, end it now while the evo sprite is large
			if (duration >= total_duration) break;
			// Enlarge prevo sprite, shrink evo sprite
			sprite.moveZoom(duration, zoom_duration, 110);
			sprite2.moveZoom(duration, zoom_duration, 0);
			duration += zoom_duration;
			// Shorten the duration of zoom changes for the next cycle
			zoom_duration = (int)Math.Max((int)Math.Round(zoom_duration / 1.2), 2);
		}
		@picture1 = sprite;
		@picture2 = sprite2;
	}

	// Opens the evolution screen
	public void Evolution(cancancel = true) {
		BGMStop;
		MessageDisplay(@sprites["msgwindow"], "\\se[]" + _INTL("What?") + "\1") { Update };
		PlayDecisionSE;
		@pokemon.play_cry;
		@sprites["msgwindow"].text = _INTL("{1} is evolving!", @pokemon.name);
		timer_start = System.uptime;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (System.uptime - timer_start >= 1) break;
		}
		MEPlay("Evolution start");
		BGMPlay("Evolution");
		canceled = false;
		timer_start = System.uptime;
		do { //loop; while (true);
			UpdateNarrowScreen(timer_start);
			@picture1.update;
			setPictureSprite(@sprites["rsprite1"], @picture1);
			if (@sprites["rsprite1"].zoom_x > 1.0) {
				@sprites["rsprite1"].zoom_x = 1.0;
				@sprites["rsprite1"].zoom_y = 1.0;
			}
			@picture2.update;
			setPictureSprite(@sprites["rsprite2"], @picture2);
			if (@sprites["rsprite2"].zoom_x > 1.0) {
				@sprites["rsprite2"].zoom_x = 1.0;
				@sprites["rsprite2"].zoom_y = 1.0;
			}
			Graphics.update;
			Input.update;
			Update(true);
			if (Input.trigger(Input.BACK) && cancancel) {
				BGMStop;
				PlayCancelSE
				canceled = true;
				break;
			}
			if (!@picture1.running() && !@picture2.running()) break;
		}
		FlashInOut(canceled);
		if (canceled) {
			Game.GameData.stats.evolutions_cancelled += 1;
			MessageDisplay(@sprites["msgwindow"],
											_INTL("Huh? {1} stopped evolving!", @pokemon.name)) { Update };
		} else {
			EvolutionSuccess;
		}
	}

	public void UpdateNarrowScreen(timer_start) {
		if (@bgviewport.rect.y >= 80) return;
		buffer = 80;
		@bgviewport.rect.height = Graphics.height - lerp(0, 64 + (buffer * 2), 0.7, timer_start, System.uptime).ToInt();
		@bgviewport.rect.y = lerp(0, buffer, 0.5, timer_start + 0.2, System.uptime).ToInt();
		@sprites["background"].oy = @bgviewport.rect.y;
	}

	public void UpdateExpandScreen(timer_start) {
		if (@bgviewport.rect.height >= Graphics.height) return;
		buffer = 80;
		@bgviewport.rect.height = Graphics.height - lerp(64 + (buffer * 2), 0, 0.7, timer_start, System.uptime).ToInt();
		@bgviewport.rect.y = lerp(buffer, 0, 0.5, timer_start, System.uptime).ToInt();
		@sprites["background"].oy = @bgviewport.rect.y;
	}

	public void FlashInOut(canceled) {
		timer_start = System.uptime;
		do { //loop; while (true);
			Graphics.update;
			Update(true);
			UpdateExpandScreen(timer_start);
			tone = lerp(0, 255, 0.7, timer_start, System.uptime);
			@viewport.tone.set(tone, tone, tone, 0);
			if (tone >= 255) break;
		}
		@bgviewport.rect.y      = 0;
		@bgviewport.rect.height = Graphics.height;
		@sprites["background"].oy = 0;
		if (canceled) {
			@sprites["rsprite1"].visible     = true;
			@sprites["rsprite1"].zoom_x      = 1.0;
			@sprites["rsprite1"].zoom_y      = 1.0;
			@sprites["rsprite1"].color.alpha = 0;
			@sprites["rsprite2"].visible     = false;
		} else {
			@sprites["rsprite1"].visible     = false;
			@sprites["rsprite2"].visible     = true;
			@sprites["rsprite2"].zoom_x      = 1.0;
			@sprites["rsprite2"].zoom_y      = 1.0;
			@sprites["rsprite2"].color.alpha = 0;
		}
		timer_start = System.uptime;
		do { //loop; while (true);
			Graphics.update;
			Update(true);
			if (System.uptime - timer_start >= 0.25) break;
		}
		timer_start = System.uptime;
		do { //loop; while (true);
			Graphics.update;
			Update;
			UpdateExpandScreen(timer_start);
			tone = lerp(255, 0, 0.4, timer_start, System.uptime);
			@viewport.tone.set(tone, tone, tone, 0);
			if (tone <= 0) break;
		}
	}

	public void EvolutionSuccess() {
		Game.GameData.stats.evolution_count += 1;
		// Play cry of evolved species
		cry_time = GameData.Species.cry_length(@newspecies, @pokemon.form);
		Pokemon.play_cry(@newspecies, @pokemon.form);
		timer_start = System.uptime;
		do { //loop; while (true);
			Graphics.update;
			Update;
			if (System.uptime - timer_start >= cry_time) break;
		}
		BGMStop;
		// Success jingle/message
		MEPlay("Evolution success");
		newspeciesname = GameData.Species.get(@newspecies).name;
		MessageDisplay(@sprites["msgwindow"],
										"\\se[]" + _INTL("Congratulations! Your {1} evolved into {2}!",
																			@pokemon.name, newspeciesname) + "\\wt[80]") { Update };
		@sprites["msgwindow"].text = "";
		// Check for consumed item and check if Pokémon should be duplicated
		EvolutionMethodAfterEvolution;
		// Modify Pokémon to make it evolved
		was_fainted = @pokemon.fainted();
		@pokemon.species = @newspecies;
		if (was_fainted) @pokemon.hp = 0;
		@pokemon.calc_stats;
		@pokemon.ready_to_evolve = false;
		// See and own evolved species
		was_owned = Game.GameData.player.owned(@newspecies);
		Game.GameData.player.pokedex.register(@pokemon);
		Game.GameData.player.pokedex.set_owned(@newspecies);
		moves_to_learn = new List<string>();
		movelist = @pokemon.getMoveList;
		foreach (var i in movelist) { //'movelist.each' do => |i|
			if (i[0] != 0 && i[0] != @pokemon.level) continue;   // 0 is "learn upon evolution"
			moves_to_learn.Add(i[1]);
		}
		// Show Pokédex entry for new species if it hasn't been owned before
		if (Settings.SHOW_NEW_SPECIES_POKEDEX_ENTRY_MORE_OFTEN && !was_owned &&
			Game.GameData.player.has_pokedex && Game.GameData.player.pokedex.species_in_unlocked_dex(@pokemon.species)) {
			MessageDisplay(@sprites["msgwindow"],
											_INTL("{1}'s data was added to the Pokédex.", newspeciesname)) { Update };
			Game.GameData.player.pokedex.register_last_seen(@pokemon);
			FadeOutIn do;
				scene = new PokemonPokedexInfo_Scene();
				screen = new PokemonPokedexInfoScreen(scene);
				screen.DexEntry(@pokemon.species);
				if (moves_to_learn.length > 0) @sprites["msgwindow"].text = "";
				if (moves_to_learn.length == 0) EndScreen(false);
			}
		}
		// Learn moves upon evolution for evolved species
		foreach (var move in moves_to_learn) { //'moves_to_learn.each' do => |move|
			LearnMove(@pokemon, move, true) { Update };
		}
	}

	public void EvolutionMethodAfterEvolution() {
		@pokemon.action_after_evolution(@newspecies);
	}

	public void Update(animating = false) {
		if (animating) {      // Pokémon shouldn't animate during the evolution animation
			@sprites["background"].update;
			@sprites["msgwindow"].update;
		} else {
			UpdateSpriteHash(@sprites);
		}
	}

	// Closes the evolution screen.
	public void EndScreen(need_fade_out = true) {
		if (@sprites["msgwindow"]) DisposeMessageWindow(@sprites["msgwindow"]);
		if (need_fade_out) {
			FadeOutAndHide(@sprites) { Update };
		}
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
		@bgviewport.dispose;
		@msgviewport.dispose;
	}
}
