//===============================================================================
//
//===============================================================================
public partial class PokemonPokedexInfo_Scene {
	public void StartScene(dexlist, index, region) {
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@dexlist = dexlist;
		@index   = index;
		@region  = region;
		@page = 1;
		@show_battled_count = false;
		@typebitmap = new AnimatedBitmap(_INTL("Graphics/UI/Pokedex/icon_types"));
		@sprites = new List<string>();
		@sprites["background"] = new IconSprite(0, 0, @viewport);
		@sprites["infosprite"] = new PokemonSprite(@viewport);
		@sprites["infosprite"].setOffset(PictureOrigin.CENTER);
		@sprites["infosprite"].x = 104;
		@sprites["infosprite"].y = 136;
		mappos = Game.GameData.game_map.metadata&.town_map_position;
		if (@region < 0) {                                 // Use player's current region
			@region = (mappos) ? mappos[0] : 0;                      // Region 0 default
		}
		@mapdata = GameData.TownMap.get(@region);
		@sprites["areamap"] = new IconSprite(0, 0, @viewport);
		@sprites["areamap"].setBitmap($"Graphics/UI/Town Map/{@mapdata.filename}");
		@sprites["areamap"].x += (Graphics.width - @sprites["areamap"].bitmap.width) / 2;
		@sprites["areamap"].y += (Graphics.height + 32 - @sprites["areamap"].bitmap.height) / 2;
		foreach (var hidden in Settings.REGION_MAP_EXTRAS) { //'Settings.REGION_MAP_EXTRAS.each' do => |hidden|
			if (hidden[0] != @region || hidden[1] <= 0 || !Game.GameData.game_switches[hidden[1]]) continue;
			DrawImagePositions(
				@sprites["areamap"].bitmap,
				new {$"Graphics/UI/Town Map/{hidden[4]}",
					hidden[2] * PokemonRegionMap_Scene.SQUARE_WIDTH,
					hidden[3] * PokemonRegionMap_Scene.SQUARE_HEIGHT}
			);
		}
		@sprites["areahighlight"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["areaoverlay"] = new IconSprite(0, 0, @viewport);
		@sprites["areaoverlay"].setBitmap("Graphics/UI/Pokedex/overlay_area");
		@sprites["formfront"] = new PokemonSprite(@viewport);
		@sprites["formfront"].setOffset(PictureOrigin.CENTER);
		@sprites["formfront"].x = 130;
		@sprites["formfront"].y = 158;
		@sprites["formback"] = new PokemonSprite(@viewport);
		@sprites["formback"].setOffset(PictureOrigin.BOTTOM);
		@sprites["formback"].x = 382;   // y is set below as it depends on metrics
		@sprites["formicon"] = new PokemonSpeciesIconSprite(null, @viewport);
		@sprites["formicon"].setOffset(PictureOrigin.CENTER);
		@sprites["formicon"].x = 82;
		@sprites["formicon"].y = 328;
		@sprites["uparrow"] = new AnimatedSprite("Graphics/UI/up_arrow", 8, 28, 40, 2, @viewport);
		@sprites["uparrow"].x = 242;
		@sprites["uparrow"].y = 268;
		@sprites["uparrow"].play;
		@sprites["uparrow"].visible = false;
		@sprites["downarrow"] = new AnimatedSprite("Graphics/UI/down_arrow", 8, 28, 40, 2, @viewport);
		@sprites["downarrow"].x = 242;
		@sprites["downarrow"].y = 348;
		@sprites["downarrow"].play;
		@sprites["downarrow"].visible = false;
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		SetSystemFont(@sprites["overlay"].bitmap);
		UpdateDummyPokemon;
		@available = GetAvailableForms;
		drawPage(@page);
		FadeInAndShow(@sprites) { Update };
	}

	// For standalone access, shows first page only.
	public void StartSceneBrief(species) {
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		dexnum = 0;
		dexnumshift = false;
		if (Game.GameData.player.pokedex.unlocked(-1)) {   // National Dex is unlocked
			species_data = GameData.Species.try_get(species);
			if (species_data) {
				nationalDexList = [:NONE];
				GameData.Species.each_species(s => nationalDexList.Add(s.species));
				dexnum = nationalDexList.index(species_data.species) || 0;
				if (dexnum > 0 && Settings.DEXES_WITH_OFFSETS.Contains(-1)) dexnumshift = true;
			}
		} else {
			for (int i = (Game.GameData.player.pokedex.dexes_count - 1); i < (Game.GameData.player.pokedex.dexes_count - 1); i++) { //for '(Game.GameData.player.pokedex.dexes_count - 1)' times do => |i|   // Regional Dexes
				if (!Game.GameData.player.pokedex.unlocked(i)) continue;
				num = GetRegionalNumber(i, species);
				if (num <= 0) continue;
				dexnum = num;
				if (Settings.DEXES_WITH_OFFSETS.Contains(i)) dexnumshift = true;
				break;
			}
		}
		@dexlist = new {
			species = species,
			name    = "",
			height  = 0,
			weight  = 0,
			number  = dexnum,
			shift   = dexnumshift
		};
		@index = 0;
		@page = 1;
		@brief = true;
		@typebitmap = new AnimatedBitmap(_INTL("Graphics/UI/Pokedex/icon_types"));
		@sprites = new List<string>();
		@sprites["background"] = new IconSprite(0, 0, @viewport);
		@sprites["infosprite"] = new PokemonSprite(@viewport);
		@sprites["infosprite"].setOffset(PictureOrigin.CENTER);
		@sprites["infosprite"].x = 104;
		@sprites["infosprite"].y = 136;
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		SetSystemFont(@sprites["overlay"].bitmap);
		UpdateDummyPokemon;
		drawPage(@page);
		FadeInAndShow(@sprites) { Update };
	}

	public void EndScene() {
		FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@typebitmap.dispose;
		@viewport.dispose;
	}

	public void Update() {
		if (@page == 2) {
			intensity_time = System.uptime % 1.0;   // 1 second per glow
			if (intensity_time >= 0.5) {
				intensity = lerp(64, 256 + 64, 0.5, intensity_time - 0.5);
			} else {
				intensity = lerp(256 + 64, 64, 0.5, intensity_time);
			}
			@sprites["areahighlight"].opacity = intensity;
		}
		UpdateSpriteHash(@sprites);
	}

	public void UpdateDummyPokemon() {
		@species = @dexlist[@index][:species];
		@gender, @form, _shiny = Game.GameData.player.pokedex.last_form_seen(@species);
		@shiny = false;
		metrics_data = GameData.SpeciesMetrics.get_species_form(@species, @form);
		@sprites["infosprite"].setSpeciesBitmap(@species, @gender, @form, @shiny);
		@sprites["formfront"]&.setSpeciesBitmap(@species, @gender, @form, @shiny);
		if (@sprites["formback"]) {
			@sprites["formback"].setSpeciesBitmap(@species, @gender, @form, @shiny, false, true);
			@sprites["formback"].y = 256;
			@sprites["formback"].y += metrics_data.back_sprite[1] * 2;
		}
		@sprites["formicon"]&.SetParams(@species, @gender, @form, @shiny);
	}

	public void GetAvailableForms() {
		ret = new List<string>();
		multiple_forms = false;
		gender_differences = (GameData.Species.front_sprite_filename(@species, 0) != GameData.Species.front_sprite_filename(@species, 0, 1));
		// Find all genders/forms of @species that have been seen
		foreach (var sp in GameData.Species) { //'GameData.Species.each' do => |sp|
			if (sp.species != @species) continue;
			if (sp.form != 0 && (!sp.real_form_name || sp.real_form_name.empty())) continue;
			if (sp.pokedex_form != sp.form) continue;
			if (sp.form > 0) multiple_forms = true;
			if (sp.single_gendered()) {
				real_gender = (sp.gender_ratio == :AlwaysFemale) ? 1 : 0;
				if (!Game.GameData.player.pokedex.seen_form(@species, real_gender, sp.form) && !Settings.DEX_SHOWS_ALL_FORMS) continue;
				if (sp.gender_ratio == :Genderless) real_gender = 2;
				ret.Add(new {sp.form_name, real_gender, sp.form});
			} else if (sp.form == 0 && !gender_differences) {
				for (int real_gndr = 2; real_gndr < 2; real_gndr++) { //for '2' times do => |real_gndr|
					if (!Game.GameData.player.pokedex.seen_form(@species, real_gndr, sp.form) && !Settings.DEX_SHOWS_ALL_FORMS) continue;
					ret.Add(new {sp.form_name || _INTL("One Form"), 0, sp.form});
					break;
				}
			} else {   // Both male and female
				for (int real_gndr = 2; real_gndr < 2; real_gndr++) { //for '2' times do => |real_gndr|
					if (!Game.GameData.player.pokedex.seen_form(@species, real_gndr, sp.form) && !Settings.DEX_SHOWS_ALL_FORMS) continue;
					ret.Add(new {sp.form_name, real_gndr, sp.form});
					if (sp.form_name && !sp.form_name.empty()) break;   // Only show 1 entry for each non-0 form
				}
			}
		}
		// Sort all entries
		ret.sort! { |a, b| (a[2] == b[2]) ? a[1] <=> b[1] : a[2] <=> b[2] };
		// Create form names for entries if they don't already exist
		foreach (var entry in ret) { //'ret.each' do => |entry|
			if (entry[0]) {   // Alternate forms, and form 0 if no gender differences
				if (!multiple_forms && !gender_differences) entry[0] = "";
			} else {   // Necessarily applies only to form 0
				switch (entry[1]) {
					case 0:  entry[0] = _INTL("Male"); break;
					case 1:  entry[0] = _INTL("Female"); break;
					default:
						entry[0] = (multiple_forms) ? _INTL("One Form") : _INTL("Genderless"); break;
				}
			}
			if (entry[1] == 2) entry[1] = 0;   // Genderless entries are treated as male
		}
		return ret;
	}

	public void drawPage(page) {
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		// Make certain sprites visible
		@sprites["infosprite"].visible    = (@page == 1);
		if (@sprites["areamap"]) @sprites["areamap"].visible       = (@page == 2);
		if (@sprites["areahighlight"]) @sprites["areahighlight"].visible = (@page == 2);
		if (@sprites["areaoverlay"]) @sprites["areaoverlay"].visible   = (@page == 2);
		if (@sprites["formfront"]) @sprites["formfront"].visible     = (@page == 3);
		if (@sprites["formback"]) @sprites["formback"].visible      = (@page == 3);
		if (@sprites["formicon"]) @sprites["formicon"].visible      = (@page == 3);
		// Draw page-specific information
		switch (page) {
			case 1:  drawPageInfo; break;
			case 2:  drawPageArea; break;
			case 3:  drawPageForms; break;
		}
	}

	public void drawPageInfo() {
		@sprites["background"].setBitmap(_INTL("Graphics/UI/Pokedex/bg_info"));
		overlay = @sprites["overlay"].bitmap;
		base   = new Color(88, 88, 80);
		shadow = new Color(168, 184, 184);
		imagepos = new List<string>();
		if (@brief) imagepos.Add(new {_INTL("Graphics/UI/Pokedex/overlay_info"), 0, 0});
		species_data = GameData.Species.get_species_form(@species, @form);
		// Write various bits of text
		indexText = "???";
		if (@dexlist[@index][:number] > 0) {
			indexNumber = @dexlist[@index][:number];
			if (@dexlist[@index][:shift]) indexNumber -= 1;
			indexText = string.Format("{0:3}", indexNumber);
		}
		textpos = new {
			new { _INTL("{1}{2} {3}", indexText, " ", species_data.name),
				246, 48, :left, new Color(248, 248, 248), Color.black }
		};
		if (@show_battled_count) {
			textpos.Add(new {_INTL("Number Battled"), 314, 164, :left, base, shadow});
			textpos.Add(new {Game.GameData.player.pokedex.battled_count(@species).ToString(), 452, 196, :right, base, shadow});
		} else {
			textpos.Add(new {_INTL("Height"), 314, 164, :left, base, shadow});
			textpos.Add(new {_INTL("Weight"), 314, 196, :left, base, shadow});
		}
		if (Game.GameData.player.owned(@species)) {
			// Write the category
			textpos.Add(new {_INTL("{1} Pokémon", species_data.category), 246, 80, :left, base, shadow});
			// Write the height and weight
			if (!@show_battled_count) {
				height = species_data.height;
				weight = species_data.weight;
				if (System.user_language[3..4] == "US") {   // If the user is in the United States
					inches = (int)Math.Round(height / 0.254);
					pounds = (int)Math.Round(weight / 0.45359);
					textpos.Add(new {string.Format("{1:d}'{2:02d}\"", inches / 12, inches % 12), 460, 164, :right, base, shadow});
					textpos.Add(new {string.Format("{1:4.1f} lbs.", pounds / 10.0), 494, 196, :right, base, shadow});
				} else {
					textpos.Add(new {string.Format("{1:.1f} m", height / 10.0), 470, 164, :right, base, shadow});
					textpos.Add(new {string.Format("{1:.1f} kg", weight / 10.0), 482, 196, :right, base, shadow});
				}
			}
			// Draw the Pokédex entry text
			drawTextEx(overlay, 40, 246, Graphics.width - 80, 4,   // overlay, x, y, width, num lines
								species_data.pokedex_entry, base, shadow);
			// Draw the footprint
			footprintfile = GameData.Species.footprint_filename(@species, @form);
			if (footprintfile) {
				footprint = RPG.Cache.load_bitmap("", footprintfile);
				overlay.blt(226, 138, footprint, footprint.rect);
				footprint.dispose;
			}
			// Show the owned icon
			imagepos.Add(new {"Graphics/UI/Pokedex/icon_own", 212, 44});
			// Draw the type icon(s)
			species_data.types.each_with_index do |type, i|
				type_number = GameData.Type.get(type).icon_position;
				type_rect = new Rect(0, type_number * 32, 96, 32);
				overlay.blt(296 + (100 * i), 120, @typebitmap.bitmap, type_rect);
			}
		} else {
			// Write the category
			textpos.Add(new {_INTL("????? Pokémon"), 246, 80, :left, base, shadow});
			// Write the height and weight
			if (!@show_battled_count) {
				if (System.user_language[3..4] == "US") {   // If the user is in the United States
					textpos.Add(new {_INTL("???'??\""), 460, 164, :right, base, shadow});
					textpos.Add(new {_INTL("????.? lbs."), 494, 196, :right, base, shadow});
				} else {
					textpos.Add(new {_INTL("????.? m"), 470, 164, :right, base, shadow});
					textpos.Add(new {_INTL("????.? kg"), 482, 196, :right, base, shadow});
				}
			}
		}
		// Draw all text
		DrawTextPositions(overlay, textpos);
		// Draw all images
		DrawImagePositions(overlay, imagepos);
	}

	public void FindEncounter(enc_types, species) {
		if (!enc_types) return false;
		foreach (var slots in enc_types) { //enc_types.each_value do => |slots|
			if (!slots) continue;
			slots.each(slot => { if (GameData.Species.get(slot[1]).species == species) return true; });
		}
		return false;
	}

	// Returns a 1D array of values corresponding to points on the Town Map. Each
	// value is true or false.
	public void GetEncounterPoints() {
		// Determine all visible points on the Town Map (i.e. only ones with a
		// defined point in town_map.txt, and which either have no Self Switch
		// controlling their visibility or whose Self Switch is ON)
		visible_points = new List<string>();
		@mapdata.point.each do |loc|
			if (loc[7] && !Game.GameData.game_switches[loc[7]]) continue;   // Point is not visible
			visible_points.Add(new {loc[0], loc[1]});
		}
		// Find all points with a visible area for @species
		town_map_width = 1 + PokemonRegionMap_Scene.RIGHT - PokemonRegionMap_Scene.LEFT;
		ret = new List<string>();
		foreach (var enc_data in GameData.Encounter.each_of_version(Game.GameData.PokemonGlobal.encounter_version)) { //GameData.Encounter.each_of_version(Game.GameData.PokemonGlobal.encounter_version) do => |enc_data|
			if (!FindEncounter(enc_data.types, @species)) continue;   // Species isn't in encounter table
			// Get the map belonging to the encounter table
			map_metadata = GameData.MapMetadata.try_get(enc_data.map);
			if (!map_metadata || map_metadata.has_flag("HideEncountersInPokedex")) continue;
			mappos = map_metadata.town_map_position;
			if (!mappos || mappos[0] != @region) continue;   // Map isn't in the region being shown
			// Get the size and shape of the map in the Town Map
			map_size = map_metadata.town_map_size;
			map_width = 1;
			map_height = 1;
			map_shape = "1";
			if (map_size && map_size[0] && map_size[0] > 0) {   // Map occupies multiple points
				map_width = map_size[0];
				map_shape = map_size[1];
				map_height = (map_shape.length.to_f / map_width).ceil;
			}
			// Mark each visible point covered by the map as containing the area
			for (int i = map_width; i < map_width; i++) { //for 'map_width' times do => |i|
				for (int j = map_height; j < map_height; j++) { //for 'map_height' times do => |j|
					if (map_shape[i + (j * map_width), 1].ToInt() == 0) continue;   // Point isn't part of map
					if (!visible_points.Contains(new {mappos[1] + i, mappos[2] + j})) continue;   // Point isn't visible
					ret[mappos[1] + i + ((mappos[2] + j) * town_map_width)] = true;
				}
			}
		}
		return ret;
	}

	public void drawPageArea() {
		@sprites["background"].setBitmap(_INTL("Graphics/UI/Pokedex/bg_area"));
		overlay = @sprites["overlay"].bitmap;
		base   = new Color(88, 88, 80);
		shadow = new Color(168, 184, 184);
		@sprites["areahighlight"].bitmap.clear;
		// Get all points to be shown as places where @species can be encountered
		points = GetEncounterPoints;
		// Draw coloured squares on each point of the Town Map with a nest
		pointcolor   = new Color(0, 248, 248);
		pointcolorhl = new Color(192, 248, 248);
		town_map_width = 1 + PokemonRegionMap_Scene.RIGHT - PokemonRegionMap_Scene.LEFT;
		sqwidth = PokemonRegionMap_Scene.SQUARE_WIDTH;
		sqheight = PokemonRegionMap_Scene.SQUARE_HEIGHT;
		for (int j = points.length; j < points.length; j++) { //for 'points.length' times do => |j|
			if (!points[j]) continue;
			x = (j % town_map_width) * sqwidth;
			x += (Graphics.width - @sprites["areamap"].bitmap.width) / 2;
			y = (j / town_map_width) * sqheight;
			y += (Graphics.height + 32 - @sprites["areamap"].bitmap.height) / 2;
			@sprites["areahighlight"].bitmap.fill_rect(x, y, sqwidth, sqheight, pointcolor);
			if (j - town_map_width < 0 || !points[j - town_map_width]) {
				@sprites["areahighlight"].bitmap.fill_rect(x, y - 2, sqwidth, 2, pointcolorhl);
			}
			if (j + town_map_width >= points.length || !points[j + town_map_width]) {
				@sprites["areahighlight"].bitmap.fill_rect(x, y + sqheight, sqwidth, 2, pointcolorhl);
			}
			if (j % town_map_width == 0 || !points[j - 1]) {
				@sprites["areahighlight"].bitmap.fill_rect(x - 2, y, 2, sqheight, pointcolorhl);
			}
			if ((j + 1) % town_map_width == 0 || !points[j + 1]) {
				@sprites["areahighlight"].bitmap.fill_rect(x + sqwidth, y, 2, sqheight, pointcolorhl);
			}
		}
		// Set the text
		textpos = new List<string>();
		if (points.length == 0) {
			DrawImagePositions(
				overlay,
				new {"Graphics/UI/Pokedex/overlay_areanone", 108, 188}
			);
			textpos.Add(new {_INTL("Area unknown"), Graphics.width / 2, (Graphics.height / 2) + 6, :center, base, shadow});
		}
		textpos.Add(new {@mapdata.name, 414, 50, :center, base, shadow});
		textpos.Add(new {_INTL("{1}'s area", GameData.Species.get(@species).name),
									Graphics.width / 2, 358, :center, base, shadow});
		DrawTextPositions(overlay, textpos);
	}

	public void drawPageForms() {
		@sprites["background"].setBitmap(_INTL("Graphics/UI/Pokedex/bg_forms"));
		overlay = @sprites["overlay"].bitmap;
		base   = new Color(88, 88, 80);
		shadow = new Color(168, 184, 184);
		// Write species and form name
		formname = ""
		@available.each do |i|
			if (i[1] == @gender && i[2] == @form) {
				formname = i[0]
				break;
			}
		}
		textpos = new {
			new {GameData.Species.get(@species).name, Graphics.width / 2, Graphics.height - 82, :center, base, shadow},
			new {formname, Graphics.width / 2, Graphics.height - 50, :center, base, shadow}
		}
		// Draw all text
		DrawTextPositions(overlay, textpos);
	}

	public void GoToPrevious() {
		newindex = @index;
		while (newindex > 0) {
			newindex -= 1;
			if (Game.GameData.player.seen(@dexlist[newindex][:species])) {
				@index = newindex;
				break;
			}
		}
	}

	public void GoToNext() {
		newindex = @index;
		while (newindex < @dexlist.length - 1) {
			newindex += 1;
			if (Game.GameData.player.seen(@dexlist[newindex][:species])) {
				@index = newindex;
				break;
			}
		}
	}

	public void ChooseForm() {
		index = 0;
		for (int i = @available.length; i < @available.length; i++) { //for '@available.length' times do => |i|
			if (@available[i][1] == @gender && @available[i][2] == @form) {
				index = i;
				break;
			}
		}
		oldindex = -1;
		do { //loop; while (true);
			if (oldindex != index) {
				Game.GameData.player.pokedex.set_last_form_seen(@species, @available[index][1], @available[index][2]);
				UpdateDummyPokemon;
				drawPage(@page);
				@sprites["uparrow"].visible   = (index > 0);
				@sprites["downarrow"].visible = (index < @available.length - 1);
				oldindex = index;
			}
			Graphics.update;
			Input.update;
			Update;
			if (Input.trigger(Input.UP)) {
				PlayCursorSE;
				index = (index + @available.length - 1) % @available.length;
			} else if (Input.trigger(Input.DOWN)) {
				PlayCursorSE;
				index = (index + 1) % @available.length;
			} else if (Input.trigger(Input.BACK)) {
				PlayCancelSE
				break;
			} else if (Input.trigger(Input.USE)) {
				PlayDecisionSE;
				break;
			}
		}
		@sprites["uparrow"].visible   = false;
		@sprites["downarrow"].visible = false;
	}

	public void Scene() {
		Pokemon.play_cry(@species, @form);
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			dorefresh = false;
			if (Input.trigger(Input.ACTION)) {
				SEStop;
				if (@page == 1) Pokemon.play_cry(@species, @form);
			} else if (Input.trigger(Input.BACK)) {
				PlayCloseMenuSE;
				break;
			} else if (Input.trigger(Input.USE)) {
				switch (@page) {
					case 1:   // Info
						PlayDecisionSE;
						@show_battled_count = !@show_battled_count;
						dorefresh = true;
						break;
					case 2:   // Area
//						dorefresh = true
						break;
					case 3:   // Forms
						if (@available.length > 1) {
							PlayDecisionSE;
							ChooseForm;
							dorefresh = true;
						}
						break;
				}
			} else if (Input.trigger(Input.UP)) {
				oldindex = @index;
				GoToPrevious;
				if (@index != oldindex) {
					UpdateDummyPokemon;
					@available = GetAvailableForms;
					SEStop;
					(@page == 1) ? Pokemon.play_cry(@species, @form) : PlayCursorSE
					dorefresh = true;
				}
			} else if (Input.trigger(Input.DOWN)) {
				oldindex = @index;
				GoToNext;
				if (@index != oldindex) {
					UpdateDummyPokemon;
					@available = GetAvailableForms;
					SEStop;
					(@page == 1) ? Pokemon.play_cry(@species, @form) : PlayCursorSE
					dorefresh = true;
				}
			} else if (Input.trigger(Input.LEFT)) {
				oldpage = @page;
				@page -= 1;
				if (@page < 1) @page = 1;
				if (@page > 3) @page = 3;
				if (@page != oldpage) {
					PlayCursorSE;
					dorefresh = true;
				}
			} else if (Input.trigger(Input.RIGHT)) {
				oldpage = @page;
				@page += 1;
				if (@page < 1) @page = 1;
				if (@page > 3) @page = 3;
				if (@page != oldpage) {
					PlayCursorSE;
					dorefresh = true;
				}
			}
			if (dorefresh) drawPage(@page);
		}
		return @index;
	}

	public void SceneBrief() {
		Pokemon.play_cry(@species, @form);
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (Input.trigger(Input.ACTION)) {
				SEStop;
				Pokemon.play_cry(@species, @form);
			} else if (Input.trigger(Input.BACK) || Input.trigger(Input.USE)) {
				PlayCloseMenuSE;
				break;
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPokedexInfoScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen(dexlist, index, region) {
		@scene.StartScene(dexlist, index, region);
		ret = @scene.Scene;
		@scene.EndScene;
		return ret;   // Index of last species viewed in dexlist
	}

	// For use from a Pokémon's summary screen.
	public void StartSceneSingle(species) {
		region = -1;
		if (Settings.USE_CURRENT_REGION_DEX) {
			region = GetCurrentRegion;
			if (region >= Game.GameData.player.pokedex.dexes_count - 1) region = -1;
		} else {
			region = Game.GameData.PokemonGlobal.pokedexDex;   // National Dex -1, regional Dexes 0, 1, etc.
		}
		dexnum = GetRegionalNumber(region, species);
		dexnumshift = Settings.DEXES_WITH_OFFSETS.Contains(region);
		dexlist = new {
			species = species,
			name    = GameData.Species.get(species).name,
			height  = 0,
			weight  = 0,
			number  = dexnum,
			shift   = dexnumshift
		};
		@scene.StartScene(dexlist, 0, region);
		@scene.Scene;
		@scene.EndScene;
	}

	// For use when capturing or otherwise obtaining a new species.
	public void DexEntry(species) {
		@scene.StartSceneBrief(species);
		@scene.SceneBrief;
		@scene.EndScene;
	}
}
