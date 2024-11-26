//===============================================================================
//
//===============================================================================
public partial class Window_Pokedex : Window_DrawableCommand {
	public override void initialize(x, y, width, height, viewport) {
		@commands = new List<string>();
		base.initialize(x, y, width, height, viewport);
		@selarrow     = new AnimatedBitmap("Graphics/UI/Pokedex/cursor_list");
		@pokeballOwn  = new AnimatedBitmap("Graphics/UI/Pokedex/icon_own");
		@pokeballSeen = new AnimatedBitmap("Graphics/UI/Pokedex/icon_seen");
		self.baseColor   = new Color(88, 88, 80);
		self.shadowColor = new Color(168, 184, 184);
		self.windowskin  = null;
	}

	public int commands { set {
		@commands = value;
		refresh;
		}
	}

	public override void dispose() {
		@pokeballOwn.dispose;
		@pokeballSeen.dispose;
		base.dispose();
	}

	public void species() {
		return (@commands.length == 0) ? 0 : @commands[self.index][:species];
	}

	public void itemCount() {
		return @commands.length;
	}

	public void drawItem(index, _count, rect) {
		if (index >= self.top_row + self.page_item_max) return;
		rect = new Rect(rect.x + 16, rect.y, rect.width - 16, rect.height);
		species     = @commands[index][:species];
		indexNumber = @commands[index][:number];
		if (@commands[index][:shift]) indexNumber -= 1;
		if (Game.GameData.player.seen(species)) {
			if (Game.GameData.player.owned(species)) {
				CopyBitmap(self.contents, @pokeballOwn.bitmap, rect.x - 6, rect.y + 10);
			} else {
				CopyBitmap(self.contents, @pokeballSeen.bitmap, rect.x - 6, rect.y + 10);
			}
			num_text = string.Format("{0:3}", indexNumber);
			name_text = @commands[index][:name];
		} else {
			num_text = string.Format("{0:3}", indexNumber);
			name_text = "----------";
		}
		DrawShadowText(self.contents, rect.x + 36, rect.y + 6, rect.width, rect.height,
										num_text, self.baseColor, self.shadowColor);
		DrawShadowText(self.contents, rect.x + 84, rect.y + 6, rect.width, rect.height,
										name_text, self.baseColor, self.shadowColor);
	}

	public void refresh() {
		@item_max = itemCount;
		dwidth  = self.width - self.borderX;
		dheight = self.height - self.borderY;
		self.contents = DoEnsureBitmap(self.contents, dwidth, dheight);
		self.contents.clear;
		for (int i = @item_max; i < @item_max; i++) { //for '@item_max' times do => |i|
			if (i < self.top_item || i > self.top_item + self.page_item_max) continue;
			drawItem(i, @item_max, itemRect(i));
		}
		drawCursor(self.index, itemRect(self.index));
	}

	public override void update() {
		base.update();
		@uparrow.visible   = false;
		@downarrow.visible = false;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokedexSearchSelectionSprite : Sprite {
	public int index		{ get { return _index; } }			protected int _index;
	public int cmds		{ get { return _cmds; } set { _cmds = value; } }			protected int _cmds;
	public int minmax		{ get { return _minmax; } set { _minmax = value; } }			protected int _minmax;

	public override void initialize(viewport = null) {
		base.initialize(viewport);
		@selbitmap = new AnimatedBitmap("Graphics/UI/Pokedex/cursor_search");
		self.bitmap = @selbitmap.bitmap;
		self.mode = -1;
		@index = 0;
		refresh;
	}

	public override void dispose() {
		@selbitmap.dispose;
		base.dispose();
	}

	public int index { set {
		@index = value;
		refresh;
		}
	}

	public int mode { set {
		@mode = value;
		switch (@mode) {
			case 0:     // Order
				@xstart = 46;
				@ystart = 128;
				@xgap = 236;
				@ygap = 64;
				@cols = 2;
				break;
			case 1:     // Name
				@xstart = 78;
				@ystart = 114;
				@xgap = 52;
				@ygap = 52;
				@cols = 7;
				break;
			case 2:     // Type
				@xstart = 8;
				@ystart = 104;
				@xgap = 124;
				@ygap = 44;
				@cols = 4;
				break;
			case 3: case 4:   // Height, weight
				@xstart = 44;
				@ystart = 110;
				@xgap = 8;
				@ygap = 112;
				break;
			case 5:     // Color
				@xstart = 62;
				@ystart = 114;
				@xgap = 132;
				@ygap = 52;
				@cols = 3;
				break;
			case 6:     // Shape
				@xstart = 82;
				@ystart = 116;
				@xgap = 70;
				@ygap = 70;
				@cols = 5;
				break;
		}
		}
	}

	public void refresh() {
		// Size and position cursor
		if (@mode == -1) {   // Main search screen
			switch (@index) {
				case 0:     // Order
					self.src_rect.y = 0;
					self.src_rect.height = 44;
					break;
				case 1: case 5:   // Name, color
					self.src_rect.y = 44;
					self.src_rect.height = 44;
					break;
				case 2:     // Type
					self.src_rect.y = 88;
					self.src_rect.height = 44;
					break;
				case 3: case 4:   // Height, weight
					self.src_rect.y = 132;
					self.src_rect.height = 44;
					break;
				case 6:     // Shape
					self.src_rect.y = 176;
					self.src_rect.height = 68;
					break;
				default:       // Reset/start/cancel
					self.src_rect.y = 244;
					self.src_rect.height = 40;
					break;
			}
			switch (@index) {
				case 0:         // Order
					self.x = 252;
					self.y = 52;
					break;
				case 1: case 2, 3, 4:   // Name, type, height, weight
					self.x = 114;
					self.y = 110 + ((@index - 1) * 52);
					break;
				case 5:         // Color
					self.x = 382;
					self.y = 110;
					break;
				case 6:         // Shape
					self.x = 420;
					self.y = 214;
					break;
				case 7: case 8: case 9:     // Reset, start, cancel
					self.x = 4 + ((@index - 7) * 176);
					self.y = 334;
					break;
			}
		} else {   // Parameter screen
			switch (@index) {
				case -2: case -3:   // OK, Cancel
					self.src_rect.y = 244;
					self.src_rect.height = 40;
					break;
				default:
					switch (@mode) {
						case 0:     // Order
							self.src_rect.y = 0;
							self.src_rect.height = 44;
							break;
						case 1:     // Name
							self.src_rect.y = 284;
							self.src_rect.height = 44;
							break;
						case 2: case 5:   // Type, color
							self.src_rect.y = 44;
							self.src_rect.height = 44;
							break;
						case 3: case 4:   // Height, weight
							self.src_rect.y = (@minmax == 1) ? 328 : 424;
							self.src_rect.height = 96;
							break;
						case 6:     // Shape
							self.src_rect.y = 176;
							self.src_rect.height = 68;
							break;
					}
					break;
			}
			switch (@index) {
				case -1:   // Blank option
					if (@mode == 3 || @mode == 4) {   // Height/weight range
						self.x = @xstart + ((@cmds + 1) * @xgap * (@minmax % 2));
						self.y = @ystart + (@ygap * ((@minmax + 1) % 2));
					} else {
						self.x = @xstart + ((@cols - 1) * @xgap);
						self.y = @ystart + ((int)Math.Floor(@cmds / @cols) * @ygap);
					}
					break;
				case -2:   // OK
					self.x = 4;
					self.y = 334;
					break;
				case -3:   // Cancel
					self.x = 356;
					self.y = 334;
					break;
				default:
					switch (@mode) {
						case 0: case 1, 2, 5, 6:   // Order, name, type, color, shape
							if (@index >= @cmds) {
								self.x = @xstart + ((@cols - 1) * @xgap);
								self.y = @ystart + ((int)Math.Floor(@cmds / @cols) * @ygap);
							} else {
								self.x = @xstart + ((@index % @cols) * @xgap);
								self.y = @ystart + ((int)Math.Floor(@index / @cols) * @ygap);
							}
							break;
						case 3: case 4:         // Height, weight
							if (@index >= @cmds) {
								self.x = @xstart + ((@cmds + 1) * @xgap * ((@minmax + 1) % 2));
							} else {
								self.x = @xstart + ((@index + 1) * @xgap);
							}
							self.y = @ystart + (@ygap * ((@minmax + 1) % 2));
							break;
					}
					break;
			}
		}
	}
}

//===============================================================================
// Pokédex main screen
//===============================================================================
public partial class PokemonPokedex_Scene {
	public const int MODENUMERICAL = 0;
	public const int MODEATOZ      = 1;
	public const int MODETALLEST   = 2;
	public const int MODESMALLEST  = 3;
	public const int MODEHEAVIEST  = 4;
	public const int MODELIGHTEST  = 5;

	public void Update() {
		UpdateSpriteHash(@sprites);
	}

	public void StartScene() {
		@sliderbitmap       = new AnimatedBitmap("Graphics/UI/Pokedex/icon_slider");
		@typebitmap         = new AnimatedBitmap(_INTL("Graphics/UI/Pokedex/icon_types"));
		@shapebitmap        = new AnimatedBitmap("Graphics/UI/Pokedex/icon_shapes");
		@hwbitmap           = new AnimatedBitmap(_INTL("Graphics/UI/Pokedex/icon_hw"));
		@selbitmap          = new AnimatedBitmap("Graphics/UI/Pokedex/icon_searchsel");
		@searchsliderbitmap = new AnimatedBitmap(_INTL("Graphics/UI/Pokedex/icon_searchslider"));
		@sprites = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		addBackgroundPlane(@sprites, "background", "Pokedex/bg_list", @viewport);
		// Suggestion for changing the background depending on region. You can
		// comment out the line above and uncomment the following lines:
//    if (GetPokedexRegion == -1) {   // Using national Pokédex
//      addBackgroundPlane(@sprites, "background", "Pokedex/bg_national", @viewport)
//    } else if (GetPokedexRegion == 0) {   // Using first regional Pokédex
//      addBackgroundPlane(@sprites, "background", "Pokedex/bg_regional", @viewport)
//    }
		addBackgroundPlane(@sprites, "searchbg", "Pokedex/bg_search", @viewport);
		@sprites["searchbg"].visible = false;
		@sprites["pokedex"] = new Window_Pokedex(206, 30, 276, 364, @viewport);
		@sprites["icon"] = new PokemonSprite(@viewport);
		@sprites["icon"].setOffset(PictureOrigin.CENTER);
		@sprites["icon"].x = 112;
		@sprites["icon"].y = 196;
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		SetSystemFont(@sprites["overlay"].bitmap);
		@sprites["searchcursor"] = new PokedexSearchSelectionSprite(@viewport);
		@sprites["searchcursor"].visible = false;
		@searchResults = false;
		@searchParams  = new {Game.GameData.PokemonGlobal.pokedexMode, -1, -1, -1, -1, -1, -1, -1, -1, -1};
		RefreshDexList(Game.GameData.PokemonGlobal.pokedexIndex[GetSavePositionIndex]);
		DeactivateWindows(@sprites);
		FadeInAndShow(@sprites);
	}

	public void EndScene() {
		FadeOutAndHide(@sprites);
		DisposeSpriteHash(@sprites);
		@sliderbitmap.dispose;
		@typebitmap.dispose;
		@shapebitmap.dispose;
		@hwbitmap.dispose;
		@selbitmap.dispose;
		@searchsliderbitmap.dispose;
		@viewport.dispose;
	}

	// Gets the region used for displaying Pokédex entries. Species will be listed
	// according to the given region's numbering and the returned region can have
	// any value defined in the town map data file. It is currently set to the
	// return value of GetCurrentRegion, and thus will change according to the
	// current map's MapPosition metadata setting.
	public void GetPokedexRegion() {
		if (Settings.USE_CURRENT_REGION_DEX) {
			region = GetCurrentRegion;
			if (region >= Game.GameData.player.pokedex.dexes_count - 1) region = -1;
			return region;
		} else {
			return Game.GameData.PokemonGlobal.pokedexDex;   // National Dex -1, regional Dexes 0, 1, etc.
		}
	}

	// Determines which index of the array Game.GameData.PokemonGlobal.pokedexIndex to save the
	// "last viewed species" in. All regional dexes come first in order, then the
	// National Dex at the end.
	public void GetSavePositionIndex() {
		index = GetPokedexRegion;
		if (index == -1) {   // National Dex (comes after regional Dex indices)
			index = Game.GameData.player.pokedex.dexes_count - 1;
		}
		return index;
	}

	public bool CanAddForModeList(mode, species) {
		switch (mode) {
			case MODEATOZ:
				return Game.GameData.player.seen(species);
				break;
			case MODEHEAVIEST: case MODELIGHTEST: case MODETALLEST: case MODESMALLEST:
				return Game.GameData.player.owned(species);
				break;
		}
		return true;   // For MODENUMERICAL
	}

	public void GetDexList() {
		region = GetPokedexRegion;
		regionalSpecies = AllRegionalSpecies(region);
		if (!regionalSpecies || regionalSpecies.length == 0) {
			// If no Regional Dex defined for the given region, use the National Pokédex
			regionalSpecies = new List<string>();
			GameData.Species.each_species(s => regionalSpecies.Add(s.id));
		}
		shift = Settings.DEXES_WITH_OFFSETS.Contains(region);
		ret = new List<string>();
		regionalSpecies.each_with_index do |species, i|
			if (!species) continue;
			if (!CanAddForModeList(Game.GameData.PokemonGlobal.pokedexMode, species)) continue;
			_gender, form, _shiny = Game.GameData.player.pokedex.last_form_seen(species);
			species_data = GameData.Species.get_species_form(species, form);
			ret.Add({
				species = species,
				name    = species_data.name,
				height  = species_data.height,
				weight  = species_data.weight,
				number  = i + 1,
				shift   = shift,
				types   = species_data.types,
				color   = species_data.color,
				shape   = species_data.shape;
			});
		}
		return ret;
	}

	public void RefreshDexList(index = 0) {
		dexlist = GetDexList;
		switch (Game.GameData.PokemonGlobal.pokedexMode) {
			case MODENUMERICAL:
				// Hide the Dex number 0 species if unseen
				if (dexlist[0][:shift] && !Game.GameData.player.seen(dexlist[0][:species])) dexlist[0] = null;
				// Remove unseen species from the end of the list
				i = dexlist.length - 1;
				do { //loop; while (true);
					if (i < 0 || !dexlist[i] || Game.GameData.player.seen(dexlist[i][:species])) break;
					dexlist[i] = null;
					i -= 1;
				}
				dexlist.compact!;
				// Sort species in ascending order by Regional Dex number
				dexlist.sort! { |a, b| a.number <=> b.number };
				break;
			case MODEATOZ:
				dexlist.sort! { |a, b| (a.name == b.name) ? a.number <=> b.number : a.name <=> b.name };
				break;
			case MODEHEAVIEST:
				dexlist.sort! { |a, b| (a.weight == b.weight) ? a.number <=> b.number : b.weight <=> a.weight };
				break;
			case MODELIGHTEST:
				dexlist.sort! { |a, b| (a.weight == b.weight) ? a.number <=> b.number : a.weight <=> b.weight };
				break;
			case MODETALLEST:
				dexlist.sort! { |a, b| (a.height == b.height) ? a.number <=> b.number : b.height <=> a.height };
				break;
			case MODESMALLEST:
				dexlist.sort! { |a, b| (a.height == b.height) ? a.number <=> b.number : a.height <=> b.height };
				break;
		}
		@dexlist = dexlist;
		@sprites["pokedex"].commands = @dexlist;
		@sprites["pokedex"].index    = index;
		@sprites["pokedex"].refresh;
		if (@searchResults) {
			@sprites["background"].setBitmap("Graphics/UI/Pokedex/bg_listsearch");
		} else {
			@sprites["background"].setBitmap("Graphics/UI/Pokedex/bg_list");
		}
		Refresh;
	}

	public void Refresh() {
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		base   = new Color(88, 88, 80);
		shadow = new Color(168, 184, 184);
		iconspecies = @sprites["pokedex"].species;
		if (!Game.GameData.player.seen(iconspecies)) iconspecies = null;
		// Write various bits of text
		dexname = _INTL("Pokédex");
		if (Game.GameData.player.pokedex.dexes_count > 1) {
			thisdex = Settings.pokedex_names[GetSavePositionIndex];
			if (thisdex) {
				dexname = (thisdex.Length > 0) ? thisdex[0] : thisdex;
			}
		}
		textpos = new {
			new {dexname, Graphics.width / 2, 10, :center, new Color(248, 248, 248), Color.black};
		}
		if (iconspecies) textpos.Add(new {GameData.Species.get(iconspecies).name, 112, 58, :center, base, shadow});
		if (@searchResults) {
			textpos.Add(new {_INTL("Search results"), 112, 314, :center, base, shadow});
			textpos.Add(new {@dexlist.length.ToString(), 112, 346, :center, base, shadow});
		} else {
			textpos.Add(new {_INTL("Seen:"), 42, 314, :left, base, shadow});
			textpos.Add(new {Game.GameData.player.pokedex.seen_count(GetPokedexRegion).ToString(), 182, 314, :right, base, shadow});
			textpos.Add(new {_INTL("Owned:"), 42, 346, :left, base, shadow});
			textpos.Add(new {Game.GameData.player.pokedex.owned_count(GetPokedexRegion).ToString(), 182, 346, :right, base, shadow});
		}
		// Draw all text
		DrawTextPositions(overlay, textpos);
		// Set Pokémon sprite
		setIconBitmap(iconspecies);
		// Draw slider arrows
		itemlist = @sprites["pokedex"];
		showslider = false;
		if (itemlist.top_row > 0) {
			overlay.blt(468, 48, @sliderbitmap.bitmap, new Rect(0, 0, 40, 30));
			showslider = true;
		}
		if (itemlist.top_item + itemlist.page_item_max < itemlist.itemCount) {
			overlay.blt(468, 346, @sliderbitmap.bitmap, new Rect(0, 30, 40, 30));
			showslider = true;
		}
		// Draw slider box
		if (showslider) {
			sliderheight = 268;
			boxheight = (int)Math.Floor(sliderheight * itemlist.page_row_max / itemlist.row_max);
			boxheight += (int)Math.Min((sliderheight - boxheight) / 2, sliderheight / 6);
			boxheight = (int)Math.Max(boxheight.floor, 40);
			y = 78;
			y += (int)Math.Floor((sliderheight - boxheight) * itemlist.top_row / (itemlist.row_max - itemlist.page_row_max));
			overlay.blt(468, y, @sliderbitmap.bitmap, new Rect(40, 0, 40, 8));
			i = 0;
			while (i * 16 < boxheight - 8 - 16) {
				height = (int)Math.Min(boxheight - 8 - 16 - (i * 16), 16);
				overlay.blt(468, y + 8 + (i * 16), @sliderbitmap.bitmap, new Rect(40, 8, 40, height));
				i += 1;
			}
			overlay.blt(468, y + boxheight - 16, @sliderbitmap.bitmap, new Rect(40, 24, 40, 16));
		}
	}

	public void RefreshDexSearch(params, _index) {
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		base   = new Color(248, 248, 248);
		shadow = new Color(72, 72, 72);
		// Write various bits of text
		textpos = new {
			new {_INTL("Search Mode"), Graphics.width / 2, 10, :center, base, shadow},
			new {_INTL("Order"), 136, 64, :center, base, shadow},
			new {_INTL("Name"), 58, 122, :center, base, shadow},
			new {_INTL("Type"), 58, 174, :center, base, shadow},
			new {_INTL("Height"), 58, 226, :center, base, shadow},
			new {_INTL("Weight"), 58, 278, :center, base, shadow},
			new {_INTL("Color"), 326, 122, :center, base, shadow},
			new {_INTL("Shape"), 454, 174, :center, base, shadow},
			new {_INTL("Reset"), 80, 346, :center, base, shadow, 1},
			new {_INTL("Start"), Graphics.width / 2, 346, :center, base, shadow, :outline},
			new {_INTL("Cancel"), Graphics.width - 80, 346, :center, base, shadow, :outline}
		}
		// Write order, name and color parameters
		textpos.Add(new {@orderCommands[params[0]], 344, 66, :center, base, shadow, :outline});
		textpos.Add(new {(params[1] < 0) ? "----" : @nameCommands[params[1]], 176, 124, :center, base, shadow, :outline});
		textpos.Add(new {(params[8] < 0) ? "----" : @colorCommands[params[8]].name, 444, 124, :center, base, shadow, :outline});
		// Draw type icons
		if (params[2] >= 0) {
			type_number = @typeCommands[params[2]].icon_position;
			typerect = new Rect(0, type_number * 32, 96, 32);
			overlay.blt(128, 168, @typebitmap.bitmap, typerect);
		} else {
			textpos.Add(new {"----", 176, 176, :center, base, shadow, :outline});
		}
		if (params[3] >= 0) {
			type_number = @typeCommands[params[3]].icon_position;
			typerect = new Rect(0, type_number * 32, 96, 32);
			overlay.blt(256, 168, @typebitmap.bitmap, typerect);
		} else {
			textpos.Add(new {"----", 304, 176, :center, base, shadow, :outline});
		}
		// Write height and weight limits
		ht1 = (params[4] < 0) ? 0 : (params[4] >= @heightCommands.length) ? 999 : @heightCommands[params[4]];
		ht2 = (params[5] < 0) ? 999 : (params[5] >= @heightCommands.length) ? 0 : @heightCommands[params[5]];
		wt1 = (params[6] < 0) ? 0 : (params[6] >= @weightCommands.length) ? 9999 : @weightCommands[params[6]];
		wt2 = (params[7] < 0) ? 9999 : (params[7] >= @weightCommands.length) ? 0 : @weightCommands[params[7]];
		hwoffset = false;
		if (System.user_language[3..4] == "US") {   // If the user is in the United States
			ht1 = (params[4] >= @heightCommands.length) ? 99 * 12 : (int)Math.Round(ht1 / 0.254);
			ht2 = (params[5] < 0) ? 99 * 12 : (int)Math.Round(ht2 / 0.254);
			wt1 = (params[6] >= @weightCommands.length) ? 99_990 : (int)Math.Round(wt1 / 0.254);
			wt2 = (params[7] < 0) ? 99_990 : (int)Math.Round(wt2 / 0.254);
			textpos.Add(new {string.Format("{0}'{0:2}''", ht1 / 12, ht1 % 12), 166, 228, :center, base, shadow, :outline});
			textpos.Add(new {string.Format("{0}'{0:2}''", ht2 / 12, ht2 % 12), 294, 228, :center, base, shadow, :outline});
			textpos.Add(new {string.Format("%.1f", wt1 / 10.0), 166, 280, :center, base, shadow, :outline});
			textpos.Add(new {string.Format("%.1f", wt2 / 10.0), 294, 280, :center, base, shadow, :outline});
			hwoffset = true;
		} else {
			textpos.Add(new {string.Format("%.1f", ht1 / 10.0), 166, 228, :center, base, shadow, :outline});
			textpos.Add(new {string.Format("%.1f", ht2 / 10.0), 294, 228, :center, base, shadow, :outline});
			textpos.Add(new {string.Format("%.1f", wt1 / 10.0), 166, 280, :center, base, shadow, :outline});
			textpos.Add(new {string.Format("%.1f", wt2 / 10.0), 294, 280, :center, base, shadow, :outline});
		}
		overlay.blt(344, 214, @hwbitmap.bitmap, new Rect(0, (hwoffset) ? 44 : 0, 32, 44));
		overlay.blt(344, 266, @hwbitmap.bitmap, new Rect(32, (hwoffset) ? 44 : 0, 32, 44));
		// Draw shape icon
		if (params[9] >= 0) {
			shape_number = @shapeCommands[params[9]].icon_position;
			shaperect = new Rect(0, shape_number * 60, 60, 60);
			overlay.blt(424, 218, @shapebitmap.bitmap, shaperect);
		}
		// Draw all text
		DrawTextPositions(overlay, textpos);
	}

	public void RefreshDexSearchParam(mode, cmds, sel, _index) {
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		base   = new Color(248, 248, 248);
		shadow = new Color(72, 72, 72);
		// Write various bits of text
		textpos = new {
			new {_INTL("Search Mode"), Graphics.width / 2, 10, :center, base, shadow},
			new {_INTL("OK"), 80, 346, :center, base, shadow, :outline},
			new {_INTL("Cancel"), Graphics.width - 80, 346, :center, base, shadow, :outline}
		}
		title = new {_INTL("Order"), _INTL("Name"), _INTL("Type"), _INTL("Height"),
						_INTL("Weight"), _INTL("Color"), _INTL("Shape")}[mode];
		textpos.Add(new {title, 102, (mode == 6) ? 70 : 64, :left, base, shadow});
		switch (mode) {
			case 0:   // Order
				xstart = 46;
				ystart = 128;
				xgap = 236;
				ygap = 64;
				halfwidth = 92;
				cols = 2;
				selbuttony = 0;
				selbuttonheight = 44;
				break;
			case 1:   // Name
				xstart = 78;
				ystart = 114;
				xgap = 52;
				ygap = 52;
				halfwidth = 22;
				cols = 7;
				selbuttony = 156;
				selbuttonheight = 44;
				break;
			case 2:   // Type
				xstart = 8;
				ystart = 104;
				xgap = 124;
				ygap = 44;
				halfwidth = 62;
				cols = 4;
				selbuttony = 44;
				selbuttonheight = 44;
				break;
			case 3: case 4:   // Height, weight
				xstart = 44;
				ystart = 110;
				xgap = 304 / (cmds.length + 1);
				ygap = 112;
				halfwidth = 60;
				cols = cmds.length + 1;
				break;
			case 5:   // Color
				xstart = 62;
				ystart = 114;
				xgap = 132;
				ygap = 52;
				halfwidth = 62;
				cols = 3;
				selbuttony = 44;
				selbuttonheight = 44;
				break;
			case 6:   // Shape
				xstart = 82;
				ystart = 116;
				xgap = 70;
				ygap = 70;
				halfwidth = 0;
				cols = 5;
				selbuttony = 88;
				selbuttonheight = 68;
				break;
		}
		// Draw selected option(s) text in top bar
		switch (mode) {
			case 2:   // Type icons
				for (int i = 2; i < 2; i++) { //for '2' times do => |i|
					if (!sel[i] || sel[i] < 0) {
						textpos.Add(new {"----", 298 + (128 * i), 66, :center, base, shadow, :outline});
					} else {
						type_number = @typeCommands[sel[i]].icon_position;
						typerect = new Rect(0, type_number * 32, 96, 32);
						overlay.blt(250 + (128 * i), 58, @typebitmap.bitmap, typerect);
					}
				}
				break;
			case 3:   // Height range
				ht1 = (sel[0] < 0) ? 0 : (sel[0] >= @heightCommands.length) ? 999 : @heightCommands[sel[0]];
				ht2 = (sel[1] < 0) ? 999 : (sel[1] >= @heightCommands.length) ? 0 : @heightCommands[sel[1]];
				hwoffset = false;
				if (System.user_language[3..4] == "US") {    // If the user is in the United States
					ht1 = (sel[0] >= @heightCommands.length) ? 99 * 12 : (int)Math.Round(ht1 / 0.254);
					ht2 = (sel[1] < 0) ? 99 * 12 : (int)Math.Round(ht2 / 0.254);
					txt1 = string.Format("{0}'{0:2}''", ht1 / 12, ht1 % 12);
					txt2 = string.Format("{0}'{0:2}''", ht2 / 12, ht2 % 12);
					hwoffset = true;
				} else {
					txt1 = string.Format("%.1f", ht1 / 10.0);
					txt2 = string.Format("%.1f", ht2 / 10.0);
				}
				textpos.Add(new {txt1, 286, 66, :center, base, shadow, :outline});
				textpos.Add(new {txt2, 414, 66, :center, base, shadow, :outline});
				overlay.blt(462, 52, @hwbitmap.bitmap, new Rect(0, (hwoffset) ? 44 : 0, 32, 44));
				break;
			case 4:   // Weight range
				wt1 = (sel[0] < 0) ? 0 : (sel[0] >= @weightCommands.length) ? 9999 : @weightCommands[sel[0]];
				wt2 = (sel[1] < 0) ? 9999 : (sel[1] >= @weightCommands.length) ? 0 : @weightCommands[sel[1]];
				hwoffset = false;
				if (System.user_language[3..4] == "US") {   // If the user is in the United States
					wt1 = (sel[0] >= @weightCommands.length) ? 99_990 : (int)Math.Round(wt1 / 0.254);
					wt2 = (sel[1] < 0) ? 99_990 : (int)Math.Round(wt2 / 0.254);
					txt1 = string.Format("%.1f", wt1 / 10.0);
					txt2 = string.Format("%.1f", wt2 / 10.0);
					hwoffset = true;
				} else {
					txt1 = string.Format("%.1f", wt1 / 10.0);
					txt2 = string.Format("%.1f", wt2 / 10.0);
				}
				textpos.Add(new {txt1, 286, 66, :center, base, shadow, :outline});
				textpos.Add(new {txt2, 414, 66, :center, base, shadow, :outline});
				overlay.blt(462, 52, @hwbitmap.bitmap, new Rect(32, (hwoffset) ? 44 : 0, 32, 44));
				break;
			case 5:   // Color
				if (sel[0] < 0) {
					textpos.Add(new {"----", 362, 66, :center, base, shadow, :outline});
				} else {
					textpos.Add(new {cmds[sel[0]].name, 362, 66, :center, base, shadow, :outline});
				}
				break;
			case 6:   // Shape icon
				if (sel[0] >= 0) {
					shaperect = new Rect(0, @shapeCommands[sel[0]].icon_position * 60, 60, 60);
					overlay.blt(332, 50, @shapebitmap.bitmap, shaperect);
				}
				break;
			default:
				if (sel[0] < 0) {
					text = new {"----", "-", "----", "", "", "----", ""}[mode];
					textpos.Add(new {text, 362, 66, :center, base, shadow, :outline});
				} else {
					textpos.Add(new {cmds[sel[0]], 362, 66, :center, base, shadow, :outline});
				}
				break;
		}
		// Draw selected option(s) button graphic
		if (new []{3, 4}.Contains(mode)) {   // Height, weight
			xpos1 = xstart + ((sel[0] + 1) * xgap);
			if (sel[0] < -1) xpos1 = xstart;
			xpos2 = xstart + ((sel[1] + 1) * xgap);
			if (sel[1] < 0) xpos2 = xstart + (cols * xgap);
			if (sel[1] >= cols - 1) xpos2 = xstart;
			ypos1 = ystart + 180;
			ypos2 = ystart + 36;
			if (sel[1] < cols - 1) overlay.blt(16, 120, @searchsliderbitmap.bitmap, new Rect(0, 192, 32, 44));
			if (sel[1] >= 0) overlay.blt(464, 120, @searchsliderbitmap.bitmap, new Rect(32, 192, 32, 44));
			if (sel[0] >= 0) overlay.blt(16, 264, @searchsliderbitmap.bitmap, new Rect(0, 192, 32, 44));
			if (sel[0] < cols - 1) overlay.blt(464, 264, @searchsliderbitmap.bitmap, new Rect(32, 192, 32, 44));
			hwrect = new Rect(0, 0, 120, 96);
			overlay.blt(xpos2, ystart, @searchsliderbitmap.bitmap, hwrect);
			hwrect.y = 96;
			overlay.blt(xpos1, ystart + ygap, @searchsliderbitmap.bitmap, hwrect);
			textpos.Add(new {txt1, xpos1 + halfwidth, ypos1, :center, base});
			textpos.Add(new {txt2, xpos2 + halfwidth, ypos2, :center, base});
		} else {
			for (int i = sel.length; i < sel.length; i++) { //for 'sel.length' times do => |i|
				selrect = new Rect(0, selbuttony, @selbitmap.bitmap.width, selbuttonheight);
				if (sel[i] >= 0) {
					overlay.blt(xstart + ((sel[i] % cols) * xgap),
											ystart + ((int)Math.Floor(sel[i] / cols) * ygap),
											@selbitmap.bitmap, selrect);
				} else {
					overlay.blt(xstart + ((cols - 1) * xgap),
											ystart + ((int)Math.Floor(cmds.length / cols) * ygap),
											@selbitmap.bitmap, selrect);
				}
			}
		}
		// Draw options
		switch (mode) {
			case 0: case 1:   // Order, name
				for (int i = cmds.length; i < cmds.length; i++) { //for 'cmds.length' times do => |i|
					x = xstart + halfwidth + ((i % cols) * xgap);
					y = ystart + 14 + ((int)Math.Floor(i / cols) * ygap);
					textpos.Add(new {cmds[i], x, y, :center, base, shadow, :outline});
				}
				if (mode != 0) {
					textpos.Add(new {(mode == 1) ? "-" : "----",
												xstart + halfwidth + ((cols - 1) * xgap),
												ystart + 14 + ((int)Math.Floor(cmds.length / cols) * ygap),
												:center, base, shadow, :outline});
				}
				break;
			case 2:   // Type
				typerect = new Rect(0, 0, 96, 32);
				for (int i = cmds.length; i < cmds.length; i++) { //for 'cmds.length' times do => |i|
					typerect.y = @typeCommands[i].icon_position * 32;
					overlay.blt(xstart + 14 + ((i % cols) * xgap),
											ystart + 6 + ((int)Math.Floor(i / cols) * ygap),
											@typebitmap.bitmap, typerect);
				}
				textpos.Add(new {"----",
											xstart + halfwidth + ((cols - 1) * xgap),
											ystart + 14 + ((int)Math.Floor(cmds.length / cols) * ygap),
											:center, base, shadow, :outline});
				break;
			case 5:   // Color
				for (int i = cmds.length; i < cmds.length; i++) { //for 'cmds.length' times do => |i|
					x = xstart + halfwidth + ((i % cols) * xgap);
					y = ystart + 14 + ((int)Math.Floor(i / cols) * ygap);
					textpos.Add(new {cmds[i].name, x, y, :center, base, shadow, :outline});
				}
				textpos.Add(new {"----",
											xstart + halfwidth + ((cols - 1) * xgap),
											ystart + 14 + ((int)Math.Floor(cmds.length / cols) * ygap),
											:center, base, shadow, :outline});
				break;
			case 6:   // Shape
				shaperect = new Rect(0, 0, 60, 60);
				for (int i = cmds.length; i < cmds.length; i++) { //for 'cmds.length' times do => |i|
					shaperect.y = @shapeCommands[i].icon_position * 60;
					overlay.blt(xstart + 4 + ((i % cols) * xgap),
											ystart + 4 + ((int)Math.Floor(i / cols) * ygap),
											@shapebitmap.bitmap, shaperect);
				}
				break;
		}
		// Draw all text
		DrawTextPositions(overlay, textpos);
	}

	public void setIconBitmap(species) {
		gender, form, _shiny = Game.GameData.player.pokedex.last_form_seen(species);
		@sprites["icon"].setSpeciesBitmap(species, gender, form, false);
	}

	public void SearchDexList(params) {
		Game.GameData.PokemonGlobal.pokedexMode = params[0];
		dexlist = GetDexList;
		// Filter by name
		if (params[1] >= 0) {
			scanNameCommand = @nameCommands[params[1]].scan(/./);
			dexlist = dexlist.find_all do |item|
				if (!Game.GameData.player.seen(item.species)) next false;
				firstChar = item.namenew {0, 1};
				next scanNameCommand.any(v => v == firstChar);
			}
		}
		// Filter by type
		if (params[2] >= 0 || params[3] >= 0) {
			stype1 = (params[2] >= 0) ? @typeCommands[params[2]].id : null;
			stype2 = (params[3] >= 0) ? @typeCommands[params[3]].id : null;
			dexlist = dexlist.find_all do |item|
				if (!Game.GameData.player.owned(item.species)) next false;
				types = item.types;
				if (stype1 && stype2) {
					// Find species that match both types
					next types.Contains(stype1) && types.Contains(stype2);
				} else if (stype1) {
					// Find species that match first type entered
					next types.Contains(stype1);
				} else if (stype2) {
					// Find species that match second type entered
					next types.Contains(stype2);
				} else {
					next false;
				}
			}
		}
		// Filter by height range
		if (params[4] >= 0 || params[5] >= 0) {
			minh = (params[4] < 0) ? 0 : (params[4] >= @heightCommands.length) ? 999 : @heightCommands[params[4]];
			maxh = (params[5] < 0) ? 999 : (params[5] >= @heightCommands.length) ? 0 : @heightCommands[params[5]];
			dexlist = dexlist.find_all do |item|
				if (!Game.GameData.player.owned(item.species)) next false;
				height = item.height;
				next height >= minh && height <= maxh;
			}
		}
		// Filter by weight range
		if (params[6] >= 0 || params[7] >= 0) {
			minw = (params[6] < 0) ? 0 : (params[6] >= @weightCommands.length) ? 9999 : @weightCommands[params[6]];
			maxw = (params[7] < 0) ? 9999 : (params[7] >= @weightCommands.length) ? 0 : @weightCommands[params[7]];
			dexlist = dexlist.find_all do |item|
				if (!Game.GameData.player.owned(item.species)) next false;
				weight = item.weight;
				next weight >= minw && weight <= maxw;
			}
		}
		// Filter by color
		if (params[8] >= 0) {
			scolor = @colorCommands[params[8]].id;
			dexlist = dexlist.find_all do |item|
				next Game.GameData.player.seen(item.species) && item.color == scolor;
			}
		}
		// Filter by shape
		if (params[9] >= 0) {
			sshape = @shapeCommands[params[9]].id;
			dexlist = dexlist.find_all do |item|
				next Game.GameData.player.seen(item.species) && item.shape == sshape;
			}
		}
		// Remove all unseen species from the results
		dexlist = dexlist.find_all(item => next Game.GameData.player.seen(item.species));
		switch (Game.GameData.PokemonGlobal.pokedexMode) {
			case MODENUMERICAL:  dexlist.sort! { |a, b| a.number <=> b.number }; break;
			case MODEATOZ:       dexlist.sort! { |a, b| a.name <=> b.name }; break;
			case MODEHEAVIEST:   dexlist.sort! { |a, b| b.weight <=> a.weight }; break;
			case MODELIGHTEST:   dexlist.sort! { |a, b| a.weight <=> b.weight }; break;
			case MODETALLEST:    dexlist.sort! { |a, b| b.height <=> a.height }; break;
			case MODESMALLEST:   dexlist.sort! { |a, b| a.height <=> b.height }; break;
		}
		return dexlist;
	}

	public void CloseSearch() {
		oldsprites = FadeOutAndHide(@sprites);
		oldspecies = @sprites["pokedex"].species;
		@searchResults = false;
		Game.GameData.PokemonGlobal.pokedexMode = MODENUMERICAL;
		@searchParams = new {Game.GameData.PokemonGlobal.pokedexMode, -1, -1, -1, -1, -1, -1, -1, -1, -1};
		RefreshDexList(Game.GameData.PokemonGlobal.pokedexIndex[GetSavePositionIndex]);
		for (int i = @dexlist.length; i < @dexlist.length; i++) { //for '@dexlist.length' times do => |i|
			if (@dexlist[i][:species] != oldspecies) continue;
			@sprites["pokedex"].index = i;
			Refresh;
			break;
		}
		Game.GameData.PokemonGlobal.pokedexIndex[GetSavePositionIndex] = @sprites["pokedex"].index;
		FadeInAndShow(@sprites, oldsprites);
	}

	public void DexEntry(index) {
		oldsprites = FadeOutAndHide(@sprites);
		region = -1;
		if (!Settings.USE_CURRENT_REGION_DEX) {
			dexnames = Settings.pokedex_names;
			if (dexnames[GetSavePositionIndex].Length > 0) {
				region = dexnames[GetSavePositionIndex][1];
			}
		}
		scene = new PokemonPokedexInfo_Scene();
		screen = new PokemonPokedexInfoScreen(scene);
		ret = screen.StartScreen(@dexlist, index, region);
		if (@searchResults) {
			dexlist = SearchDexList(@searchParams);
			@dexlist = dexlist;
			@sprites["pokedex"].commands = @dexlist;
			if (ret >= @dexlist.length) ret = @dexlist.length - 1;
			if (ret < 0) ret = 0;
		} else {
			RefreshDexList(Game.GameData.PokemonGlobal.pokedexIndex[GetSavePositionIndex]);
			Game.GameData.PokemonGlobal.pokedexIndex[GetSavePositionIndex] = ret;
		}
		@sprites["pokedex"].index = ret;
		@sprites["pokedex"].refresh;
		Refresh;
		FadeInAndShow(@sprites, oldsprites);
	}

	public void DexSearchCommands(mode, selitems, mainindex) {
		cmds = new {@orderCommands, @nameCommands, @typeCommands, @heightCommands,
						@weightCommands, @colorCommands, @shapeCommands}[mode];
		cols = new {2, 7, 4, 1, 1, 3, 5}[mode];
		ret = null;
		// Set background
		switch (mode) {
			case 0:     @sprites["searchbg"].setBitmap("Graphics/UI/Pokedex/bg_search_order"); break;
			case 1:     @sprites["searchbg"].setBitmap("Graphics/UI/Pokedex/bg_search_name"); break;
			case 2:
				count = 0;
				if (!t.pseudo_type && t.id != :SHADOW })) GameData.Type.each(t => { count += 1;
				if (count == 18) {
					@sprites["searchbg"].setBitmap("Graphics/UI/Pokedex/bg_search_type_18");
				} else {
					@sprites["searchbg"].setBitmap("Graphics/UI/Pokedex/bg_search_type");
				}
				break;
			case 3: case 4: @sprites["searchbg"].setBitmap("Graphics/UI/Pokedex/bg_search_size"); break;
			case 5:     @sprites["searchbg"].setBitmap("Graphics/UI/Pokedex/bg_search_color"); break;
			case 6:     @sprites["searchbg"].setBitmap("Graphics/UI/Pokedex/bg_search_shape"); break;
		}
		selindex = selitems.clone;
		index     = selindex[0];
		oldindex  = index;
		minmax    = 1;
		oldminmax = minmax;
		if (new []{3, 4}.Contains(mode)) index = oldindex = selindex[minmax];
		@sprites["searchcursor"].mode   = mode;
		@sprites["searchcursor"].cmds   = cmds.length;
		@sprites["searchcursor"].minmax = minmax;
		@sprites["searchcursor"].index  = index;
		nextparam = cmds.length % 2;
		RefreshDexSearchParam(mode, cmds, selindex, index);
		do { //loop; while (true);
			Update;
			if (index != oldindex || minmax != oldminmax) {
				@sprites["searchcursor"].minmax = minmax;
				@sprites["searchcursor"].index  = index;
				oldindex  = index;
				oldminmax = minmax;
			}
			Graphics.update;
			Input.update;
			if (new []{3, 4}.Contains(mode)) {
				if (Input.trigger(Input.UP)) {
					if (index < -1) {   // From OK/Cancel
						minmax = 0;
						index = selindex[minmax];
					} else if (minmax == 0) {
						minmax = 1;
						index = selindex[minmax];
					}
					if (index != oldindex || minmax != oldminmax) {
						PlayCursorSE;
						RefreshDexSearchParam(mode, cmds, selindex, index);
					}
				} else if (Input.trigger(Input.DOWN)) {
					switch (minmax) {
						case 1:
							minmax = 0;
							index = selindex[minmax];
							break;
						case 0:
							minmax = -1;
							index = -2;
							break;
					}
					if (index != oldindex || minmax != oldminmax) {
						PlayCursorSE;
						RefreshDexSearchParam(mode, cmds, selindex, index);
					}
				} else if (Input.repeat(Input.LEFT)) {
					if (index == -3) {
						index = -2;
					} else if (index >= -1) {
						if (minmax == 1 && index == -1) {
							if (selindex[0] < cmds.length - 1) index = cmds.length - 1;
						} else if (minmax == 1 && index == 0) {
							if (selindex[0] < 0) index = cmds.length;
						} else if (index > -1 && !(minmax == 1 && index >= cmds.length)) {
							if (minmax == 0 || selindex[0] <= index - 1) index -= 1;
						}
					}
					if (index != oldindex) {
						if (minmax >= 0) selindex[minmax] = index;
						PlayCursorSE;
						RefreshDexSearchParam(mode, cmds, selindex, index);
					}
				} else if (Input.repeat(Input.RIGHT)) {
					if (index == -2) {
						index = -3;
					} else if (index >= -1) {
						if (minmax == 1 && index >= cmds.length) {
							index = 0;
						} else if (minmax == 1 && index == cmds.length - 1) {
							index = -1;
						} else if (index < cmds.length && !(minmax == 1 && index < 0)) {
							if (minmax == 1 || selindex[1] == -1 ||
														(selindex[1] < cmds.length && selindex[1] >= index + 1)) index += 1;
						}
					}
					if (index != oldindex) {
						if (minmax >= 0) selindex[minmax] = index;
						PlayCursorSE;
						RefreshDexSearchParam(mode, cmds, selindex, index);
					}
				}
			} else {
				if (Input.trigger(Input.UP)) {
					if (index == -1) {   // From blank
						index = cmds.length - 1 - ((cmds.length - 1) % cols) - 1;
					} else if (index == -2) {   // From OK
						index = (int)Math.Floor((cmds.length - 1) / cols) * cols;
					} else if (index == -3 && mode == 0) {   // From Cancel
						index = cmds.length - 1;
					} else if (index == -3) {   // From Cancel
						index = -1;
					} else if (index >= cols) {
						index -= cols;
					}
					if (index != oldindex) PlayCursorSE;
				} else if (Input.trigger(Input.DOWN)) {
					if (index == -1) {   // From blank
						index = -3;
					} else if (index >= 0) {
						if (index + cols < cmds.length) {
							index += cols;
						} else if ((int)Math.Floor(index / cols) < (int)Math.Floor((cmds.length - 1) / cols)) {
							index = (index % cols < cols / 2.0) ? cmds.length - 1 : -1;
						} else {
							index = (index % cols < cols / 2.0) ? -2 : -3;
						}
					}
					if (index != oldindex) PlayCursorSE;
				} else if (Input.trigger(Input.LEFT)) {
					if (index == -3) {
						index = -2;
					} else if (index == -1) {
						index = cmds.length - 1;
					} else if (index > 0 && index % cols != 0) {
						index -= 1;
					}
					if (index != oldindex) PlayCursorSE;
				} else if (Input.trigger(Input.RIGHT)) {
					if (index == -2) {
						index = -3;
					} else if (index == cmds.length - 1 && mode != 0) {
						index = -1;
					} else if (index >= 0 && index % cols != cols - 1) {
						index += 1;
					}
					if (index != oldindex) PlayCursorSE;
				}
			}
			if (Input.trigger(Input.ACTION)) {
				index = -2;
				if (index != oldindex) PlayCursorSE;
			} else if (Input.trigger(Input.BACK)) {
				PlayCloseMenuSE;
				ret = null;
				break;
			} else if (Input.trigger(Input.USE)) {
				if (index == -2) {      // OK
					SEPlay("GUI pokedex open");
					ret = selindex;
					break;
				} else if (index == -3) {   // Cancel
					PlayCloseMenuSE;
					ret = null;
					break;
				} else if (selindex != index && mode != 3 && mode != 4) {
					if (mode == 2) {
						if (index == -1) {
							nextparam = (selindex[1] >= 0) ? 1 : 0;
						} else if (index >= 0) {
							nextparam = (selindex[0] < 0) ? 0 : (selindex[1] < 0) ? 1 : nextparam;
						}
						if (index < 0 || selindex[(nextparam + 1) % 2] != index) {
							PlayDecisionSE;
							selindex[nextparam] = index;
							nextparam = (nextparam + 1) % 2;
						}
					} else {
						PlayDecisionSE;
						selindex[0] = index;
					}
					RefreshDexSearchParam(mode, cmds, selindex, index);
				}
			}
		}
		Input.update;
		// Set background image
		@sprites["searchbg"].setBitmap("Graphics/UI/Pokedex/bg_search");
		@sprites["searchcursor"].mode = -1;
		@sprites["searchcursor"].index = mainindex;
		return ret;
	}

	public void DexSearch() {
		oldsprites = FadeOutAndHide(@sprites);
		params = @searchParams.clone;
		@orderCommands = new List<string>();
		@orderCommands[MODENUMERICAL] = _INTL("Numerical");
		@orderCommands[MODEATOZ]      = _INTL("A to Z");
		@orderCommands[MODEHEAVIEST]  = _INTL("Heaviest");
		@orderCommands[MODELIGHTEST]  = _INTL("Lightest");
		@orderCommands[MODETALLEST]   = _INTL("Tallest");
		@orderCommands[MODESMALLEST]  = _INTL("Smallest");
		@nameCommands = new {_INTL("A"), _INTL("B"), _INTL("C"), _INTL("D"), _INTL("E"),
										_INTL("F"), _INTL("G"), _INTL("H"), _INTL("I"), _INTL("J"),
										_INTL("K"), _INTL("L"), _INTL("M"), _INTL("N"), _INTL("O"),
										_INTL("P"), _INTL("Q"), _INTL("R"), _INTL("S"), _INTL("T"),
										_INTL("U"), _INTL("V"), _INTL("W"), _INTL("X"), _INTL("Y"),
										_INTL("Z")};
		@typeCommands = new List<string>();
		GameData.Type.each(t => { if (!t.pseudo_type) @typeCommands.Add(t); });
		@heightCommands = new {1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
											11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
											21, 22, 23, 24, 25, 30, 35, 40, 45, 50,
											55, 60, 65, 70, 80, 90, 100};
		@weightCommands = new {5, 10, 15, 20, 25, 30, 35, 40, 45, 50,
											55, 60, 70, 80, 90, 100, 110, 120, 140, 160,
											180, 200, 250, 300, 350, 400, 500, 600, 700, 800,
											900, 1000, 1250, 1500, 2000, 3000, 5000};
		@colorCommands = new List<string>();
		GameData.BodyColor.each(c => { if (c.id != :None) @colorCommands.Add(c); });
		@shapeCommands = new List<string>();
		GameData.BodyShape.each(s => { if (s.id != :None) @shapeCommands.Add(s); });
		@sprites["searchbg"].visible     = true;
		@sprites["overlay"].visible      = true;
		@sprites["searchcursor"].visible = true;
		index = 0;
		oldindex = index;
		@sprites["searchcursor"].mode    = -1;
		@sprites["searchcursor"].index   = index;
		RefreshDexSearch(params, index);
		FadeInAndShow(@sprites);
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (index != oldindex) {
				@sprites["searchcursor"].index = index;
				oldindex = index;
			}
			if (Input.trigger(Input.UP)) {
				if (index >= 7) {
					index = 4;
				} else if (index == 5) {
					index = 0;
				} else if (index > 0) {
					index -= 1;
				}
				if (index != oldindex) PlayCursorSE;
			} else if (Input.trigger(Input.DOWN)) {
				if (new []{4, 6}.Contains(index)) {
					index = 8;
				} else if (index < 7) {
					index += 1;
				}
				if (index != oldindex) PlayCursorSE;
			} else if (Input.trigger(Input.LEFT)) {
				if (index == 5) {
					index = 1;
				} else if (index == 6) {
					index = 3;
				} else if (index > 7) {
					index -= 1;
				}
				if (index != oldindex) PlayCursorSE;
			} else if (Input.trigger(Input.RIGHT)) {
				if (index == 1) {
					index = 5;
				} else if (index >= 2 && index <= 4) {
					index = 6;
				} else if (new []{7, 8}.Contains(index)) {
					index += 1;
				}
				if (index != oldindex) PlayCursorSE;
			} else if (Input.trigger(Input.ACTION)) {
				index = 8;
				if (index != oldindex) PlayCursorSE;
			} else if (Input.trigger(Input.BACK)) {
				PlayCloseMenuSE;
				break;
			} else if (Input.trigger(Input.USE)) {
				if (index != 9) SEPlay("GUI pokedex open");
				switch (index) {
					case 0:   // Choose sort order
						newparam = DexSearchCommands(0, [params[0]], index);
						if (newparam) params[0] = newparam[0];
						RefreshDexSearch(params, index);
						break;
					case 1:   // Filter by name
						newparam = DexSearchCommands(1, [params[1]], index);
						if (newparam) params[1] = newparam[0];
						RefreshDexSearch(params, index);
						break;
					case 2:   // Filter by type
						newparam = DexSearchCommands(2, new {params[2], params[3]}, index);
						if (newparam) {
							params[2] = newparam[0];
							params[3] = newparam[1];
						}
						RefreshDexSearch(params, index);
						break;
					case 3:   // Filter by height range
						newparam = DexSearchCommands(3, new {params[4], params[5]}, index);
						if (newparam) {
							params[4] = newparam[0];
							params[5] = newparam[1];
						}
						RefreshDexSearch(params, index);
						break;
					case 4:   // Filter by weight range
						newparam = DexSearchCommands(4, new {params[6], params[7]}, index);
						if (newparam) {
							params[6] = newparam[0];
							params[7] = newparam[1];
						}
						RefreshDexSearch(params, index);
						break;
					case 5:   // Filter by color filter
						newparam = DexSearchCommands(5, [params[8]], index);
						if (newparam) params[8] = newparam[0];
						RefreshDexSearch(params, index);
						break;
					case 6:   // Filter by shape
						newparam = DexSearchCommands(6, [params[9]], index);
						if (newparam) params[9] = newparam[0];
						RefreshDexSearch(params, index);
						break;
					case 7:   // Clear filters
						for (int i = 10; i < 10; i++) { //for '10' times do => |i|
							params[i] = (i == 0) ? MODENUMERICAL : -1;
						}
						RefreshDexSearch(params, index);
						break;
					case 8:   // Start search (filter)
						dexlist = SearchDexList(params);
						if (dexlist.length == 0) {
							Message(_INTL("No matching Pokémon were found."));
						} else {
							@dexlist = dexlist;
							@sprites["pokedex"].commands = @dexlist;
							@sprites["pokedex"].index    = 0;
							@sprites["pokedex"].refresh;
							@searchResults = true;
							@searchParams = params;
							break;
						}
						break;
					case 9:   // Cancel
						PlayCloseMenuSE;
						break;
						break;
				}
			}
		}
		FadeOutAndHide(@sprites);
		if (@searchResults) {
			@sprites["background"].setBitmap("Graphics/UI/Pokedex/bg_listsearch");
		} else {
			@sprites["background"].setBitmap("Graphics/UI/Pokedex/bg_list");
		}
		Refresh;
		FadeInAndShow(@sprites, oldsprites);
		Input.update;
		return 0;
	}

	public void Pokedex() {
		ActivateWindow(@sprites, "pokedex") do;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				oldindex = @sprites["pokedex"].index;
				Update;
				if (oldindex != @sprites["pokedex"].index) {
					if (!@searchResults) Game.GameData.PokemonGlobal.pokedexIndex[GetSavePositionIndex] = @sprites["pokedex"].index;
					Refresh;
				}
				if (Input.trigger(Input.ACTION)) {
					SEPlay("GUI pokedex open");
					@sprites["pokedex"].active = false;
					DexSearch;
					@sprites["pokedex"].active = true;
				} else if (Input.trigger(Input.BACK)) {
					PlayCloseMenuSE;
					if (@searchResults) {
						CloseSearch;
					} else {
						break;
					}
				} else if (Input.trigger(Input.USE)) {
					if (Game.GameData.player.seen(@sprites["pokedex"].species)) {
						SEPlay("GUI pokedex open");
						DexEntry(@sprites["pokedex"].index);
					}
				}
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPokedexScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen() {
		@scene.StartScene;
		@scene.Pokedex;
		@scene.EndScene;
	}
}
