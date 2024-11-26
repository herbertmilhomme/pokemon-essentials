//===============================================================================
//
//===============================================================================
public partial class BattleSwapScene {
	RED_TEXT_BASE   = new Color(232, 32, 16);
	RED_TEXT_SHADOW = new Color(248, 168, 184);

	public void StartRentScene(rentals) {
		@rentals = rentals;
		@mode = 0;   // rental (pick 3 out of 6 initial Pokémon)
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		addBackgroundPlane(@sprites, "bg", "rentbg", @viewport);
		@sprites["title"] = Window_UnformattedTextPokemon.newWithSize(
			_INTL("RENTAL POKéMON"), 0, 0, Graphics.width, 64, @viewport
		);
		@sprites["list"] = Window_AdvancedCommandPokemonEx.newWithSize(
			[], 0, 64, Graphics.width, Graphics.height - 128, @viewport
		);
		@sprites["help"] = Window_UnformattedTextPokemon.newWithSize(
			"", 0, Graphics.height - 64, Graphics.width, 64, @viewport
		);
		@sprites["msgwindow"] = Window_AdvancedTextPokemon.newWithSize(
			"", 0, Graphics.height - 64, Graphics.height, 64, @viewport
		);
		@sprites["msgwindow"].visible = false;
		UpdateChoices(new List<string>());
		DeactivateWindows(@sprites);
		FadeInAndShow(@sprites) { Update };
	}

	public void StartSwapScene(currentPokemon, newPokemon) {
		@currentPokemon = currentPokemon;
		@newPokemon = newPokemon;
		@mode = 1;   // swap (pick 1 out of 3 opponent's Pokémon to take)
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		addBackgroundPlane(@sprites, "bg", "swapbg", @viewport);
		@sprites["title"] = Window_UnformattedTextPokemon.newWithSize(
			_INTL("POKéMON SWAP"), 0, 0, Graphics.width, 64, @viewport
		);
		@sprites["list"] = Window_AdvancedCommandPokemonEx.newWithSize(
			[], 0, 64, Graphics.width, Graphics.height - 128, @viewport
		);
		@sprites["help"] = Window_UnformattedTextPokemon.newWithSize(
			"", 0, Graphics.height - 64, Graphics.width, 64, @viewport
		);
		@sprites["msgwindow"] = Window_AdvancedTextPokemon.newWithSize(
			"", 0, Graphics.height - 64, Graphics.width, 64, @viewport
		);
		@sprites["msgwindow"].visible = false;
		InitSwapScreen;
		DeactivateWindows(@sprites);
		FadeInAndShow(@sprites) { Update };
	}

	public void InitSwapScreen() {
		commands = GetCommands(@currentPokemon, new List<string>());
		commands.Add(_INTL("CANCEL"));
		@sprites["help"].text = _INTL("Select Pokémon to swap.");
		@sprites["list"].commands = commands;
		@sprites["list"].index = 0;
		@mode = 1;
	}

	// End the scene here.
	public void EndScene() {
		FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void ShowCommands(commands) {
		UIHelper.ShowCommands(@sprites["msgwindow"], null, commands) { Update };
	}

	public void Confirm(message) {
		UIHelper.Confirm(@sprites["msgwindow"], message) { Update };
	}

	public void GetCommands(list, choices) {
		red_text_tag = shadowc3tag(RED_TEXT_BASE, RED_TEXT_SHADOW);
		commands = new List<string>();
		for (int i = list.length; i < list.length; i++) { //for 'list.length' times do => |i|
			pkmn = list[i];
			category = pkmn.species_data.category;
			cmd = _INTL("{1} - {2} Pokémon", pkmn.speciesName, category);
			if (choices.Contains(i)) cmd = red_text_tag + cmd;   // Red text
			commands.Add(cmd);
		}
		return commands;
	}

	// Processes the scene.
	public void ChoosePokemon(canCancel) {
		ActivateWindow(@sprites, "list") do;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				Update;
				if (Input.trigger(Input.BACK) && canCancel) {
					return -1;
				} else if (Input.trigger(Input.USE)) {
					index = @sprites["list"].index;
					if (index == @sprites["list"].commands.length - 1 && canCancel) {
						return -1;
					} else if (index == @sprites["list"].commands.length - 2 && canCancel && @mode == 2) {
						return -2;
					} else {
						return index;
					}
				}
			}
		}
	}

	public void UpdateChoices(choices) {
		commands = GetCommands(@rentals, choices);
		@choices = choices;
		switch (choices.length) {
			case 0:
				@sprites["help"].text = _INTL("Choose the first Pokémon.");
				break;
			case 1:
				@sprites["help"].text = _INTL("Choose the second Pokémon.");
				break;
			default:
				@sprites["help"].text = _INTL("Choose the third Pokémon.");
				break;
		}
		@sprites["list"].commands = commands;
	}

	public void SwapChosen(_pkmnindex) {
		commands = GetCommands(@newPokemon, new List<string>());
		commands.Add(_INTL("PKMN FOR SWAP"));
		commands.Add(_INTL("CANCEL"));
		@sprites["help"].text = _INTL("Select Pokémon to accept.");
		@sprites["list"].commands = commands;
		@sprites["list"].index = 0;
		@mode = 2;
	}

	public void SwapCanceled() {
		InitSwapScreen;
	}

	public void Summary(list, index) {
		visibleSprites = FadeOutAndHide(@sprites) { Update };
		scene = new PokemonSummary_Scene();
		screen = new PokemonSummaryScreen(scene);
		@sprites["list"].index = screen.StartScreen(list, index);
		FadeInAndShow(@sprites, visibleSprites) { Update };
	}

	public void Update() {
		UpdateSpriteHash(@sprites);
	}
}

//===============================================================================
//
//===============================================================================
public partial class BattleSwapScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartRent(rentals) {
		@scene.StartRentScene(rentals);
		chosen = new List<string>();
		do { //loop; while (true);
			index = @scene.ChoosePokemon(false);
			commands = new List<string>();
			commands.Add(_INTL("SUMMARY"));
			if (chosen.Contains(index)) {
				commands.Add(_INTL("DESELECT"));
			} else {
				commands.Add(_INTL("RENT"));
			}
			commands.Add(_INTL("OTHERS"));
			command = @scene.ShowCommands(commands);
			switch (command) {
				case 0:
					@scene.Summary(rentals, index);
					break;
				case 1:
					if (chosen.Contains(index)) {
						chosen.delete(index);
						@scene.UpdateChoices(chosen.clone);
					} else {
						chosen.Add(index);
						@scene.UpdateChoices(chosen.clone);
						if (chosen.length == 3) {
							if (@scene.Confirm(_INTL("Are these three Pokémon OK?"))) {
								retval = new List<string>();
								chosen.each(i => retval.Add(rentals[i]));
								@scene.EndScene;
								return retval;
							} else {
								chosen.delete(index);
								@scene.UpdateChoices(chosen.clone);
							}
						}
					}
					break;
			}
		}
	}

	public void StartSwap(currentPokemon, newPokemon) {
		@scene.StartSwapScene(currentPokemon, newPokemon);
		do { //loop; while (true);
			pkmn = @scene.ChoosePokemon(true);
			if (pkmn >= 0) {
				commands = new {_INTL("SUMMARY"), _INTL("SWAP"), _INTL("RECHOOSE")};
				command = @scene.ShowCommands(commands);
				switch (command) {
					case 0:
						@scene.Summary(currentPokemon, pkmn);
						break;
					case 1:
						@scene.SwapChosen(pkmn);
						yourPkmn = pkmn;
						do { //loop; while (true);
							pkmn = @scene.ChoosePokemon(true);
							if (pkmn >= 0) {
								if (@scene.Confirm(_INTL("Accept this Pokémon?"))) {
									@scene.EndScene;
									currentPokemon[yourPkmn] = newPokemon[pkmn];
									return true;
								}
							} else if (pkmn == -2) {
								@scene.SwapCanceled;
								break;   // Back to first screen
							} else if (pkmn == -1) {
								if (@scene.Confirm(_INTL("Quit swapping?"))) {
									@scene.EndScene;
									return false;
								}
							}
						}
						break;
				}
			} else if (@scene.Confirm(_INTL("Quit swapping?"))) {
				// Canceled
				@scene.EndScene;
				return false;
			}
		}
	}
}
