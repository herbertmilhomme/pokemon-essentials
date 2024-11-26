//===============================================================================
//
//===============================================================================
public partial class MoveSelectionSprite : Sprite {
	public int preselected		{ get { return _preselected; } }			protected int _preselected;
	public int index		{ get { return _index; } }			protected int _index;

	public override void initialize(viewport = null, fifthmove = false) {
		base.initialize(viewport);
		@movesel = new AnimatedBitmap("Graphics/UI/Summary/cursor_move");
		@frame = 0;
		@index = 0;
		@fifthmove = fifthmove;
		@preselected = false;
		@updating = false;
		refresh;
	}

	public override void dispose() {
		@movesel.dispose;
		base.dispose();
	}

	public int index { set {
		@index = value;
		refresh;
		}
	}

	public int preselected { set {
		@preselected = value;
		refresh;
		}
	}

	public void refresh() {
		w = @movesel.width;
		h = @movesel.height / 2;
		self.x = 240;
		self.y = 92 + (self.index * 64);
		if (@fifthmove) self.y -= 76;
		if (@fifthmove && self.index == Pokemon.MAX_MOVES) self.y += 20;   // Add a gap
		self.bitmap = @movesel.bitmap;
		if (self.preselected) {
			self.src_rect.set(0, h, w, h);
		} else {
			self.src_rect.set(0, 0, w, h);
		}
	}

	public override void update() {
		@updating = true;
		base.update();
		@movesel.update;
		@updating = false;
		refresh;
	}
}

//===============================================================================
//
//===============================================================================
public partial class RibbonSelectionSprite : MoveSelectionSprite {
	public override void initialize(viewport = null) {
		base.initialize(viewport);
		@movesel = new AnimatedBitmap("Graphics/UI/Summary/cursor_ribbon");
		@frame = 0;
		@index = 0;
		@preselected = false;
		@updating = false;
		@spriteVisible = true;
		refresh;
	}

	public override int visible { set {
		base.visible();
		if (!@updating) @spriteVisible = value;
		}
	}

	public void refresh() {
		w = @movesel.width;
		h = @movesel.height / 2;
		self.x = 228 + ((self.index % 4) * 68);
		self.y = 76 + ((int)Math.Floor(self.index / 4) * 68);
		self.bitmap = @movesel.bitmap;
		if (self.preselected) {
			self.src_rect.set(0, h, w, h);
		} else {
			self.src_rect.set(0, 0, w, h);
		}
	}

	public override void update() {
		@updating = true;
		base.update();
		self.visible = @spriteVisible && @index >= 0 && @index < 12;
		@movesel.update;
		@updating = false;
		refresh;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonSummary_Scene {
	public const int MARK_WIDTH  = 16;
	public const int MARK_HEIGHT = 16;
	// Colors used for messages in this scene
	RED_TEXT_BASE     = new Color(248, 56, 32);
	RED_TEXT_SHADOW   = new Color(224, 152, 144);
	BLACK_TEXT_BASE   = new Color(64, 64, 64);
	BLACK_TEXT_SHADOW = new Color(176, 176, 176);

	public void Update() {
		UpdateSpriteHash(@sprites);
	}

	public void StartScene(party, partyindex, inbattle = false) {
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@party      = party;
		@partyindex = partyindex;
		@pokemon    = @party[@partyindex];
		@inbattle   = inbattle;
		@page = 1;
		@typebitmap    = new AnimatedBitmap(_INTL("Graphics/UI/types"));
		@markingbitmap = new AnimatedBitmap("Graphics/UI/Summary/markings");
		@sprites = new List<string>();
		@sprites["background"] = new IconSprite(0, 0, @viewport);
		@sprites["pokemon"] = new PokemonSprite(@viewport);
		@sprites["pokemon"].setOffset(PictureOrigin.CENTER);
		@sprites["pokemon"].x = 104;
		@sprites["pokemon"].y = 206;
		@sprites["pokemon"].setPokemonBitmap(@pokemon);
		@sprites["pokeicon"] = new PokemonIconSprite(@pokemon, @viewport);
		@sprites["pokeicon"].setOffset(PictureOrigin.CENTER);
		@sprites["pokeicon"].x       = 46;
		@sprites["pokeicon"].y       = 92;
		@sprites["pokeicon"].visible = false;
		@sprites["itemicon"] = new ItemIconSprite(30, 320, @pokemon.item_id, @viewport);
		@sprites["itemicon"].blankzero = true;
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		SetSystemFont(@sprites["overlay"].bitmap);
		@sprites["movepresel"] = new MoveSelectionSprite(@viewport);
		@sprites["movepresel"].visible     = false;
		@sprites["movepresel"].preselected = true;
		@sprites["movesel"] = new MoveSelectionSprite(@viewport);
		@sprites["movesel"].visible = false;
		@sprites["ribbonpresel"] = new RibbonSelectionSprite(@viewport);
		@sprites["ribbonpresel"].visible     = false;
		@sprites["ribbonpresel"].preselected = true;
		@sprites["ribbonsel"] = new RibbonSelectionSprite(@viewport);
		@sprites["ribbonsel"].visible = false;
		@sprites["uparrow"] = new AnimatedSprite("Graphics/UI/up_arrow", 8, 28, 40, 2, @viewport);
		@sprites["uparrow"].x = 350;
		@sprites["uparrow"].y = 56;
		@sprites["uparrow"].play;
		@sprites["uparrow"].visible = false;
		@sprites["downarrow"] = new AnimatedSprite("Graphics/UI/down_arrow", 8, 28, 40, 2, @viewport);
		@sprites["downarrow"].x = 350;
		@sprites["downarrow"].y = 260;
		@sprites["downarrow"].play;
		@sprites["downarrow"].visible = false;
		@sprites["markingbg"] = new IconSprite(260, 88, @viewport);
		@sprites["markingbg"].setBitmap("Graphics/UI/Summary/overlay_marking");
		@sprites["markingbg"].visible = false;
		@sprites["markingoverlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["markingoverlay"].visible = false;
		SetSystemFont(@sprites["markingoverlay"].bitmap);
		@sprites["markingsel"] = new IconSprite(0, 0, @viewport);
		@sprites["markingsel"].setBitmap("Graphics/UI/Summary/cursor_marking");
		@sprites["markingsel"].src_rect.height = @sprites["markingsel"].bitmap.height / 2;
		@sprites["markingsel"].visible = false;
		@sprites["messagebox"] = new Window_AdvancedTextPokemon("");
		@sprites["messagebox"].viewport       = @viewport;
		@sprites["messagebox"].visible        = false;
		@sprites["messagebox"].letterbyletter = true;
		BottomLeftLines(@sprites["messagebox"], 2);
		@nationalDexList = [:NONE];
		GameData.Species.each_species(s => @nationalDexList.Add(s.species));
		drawPage(@page);
		FadeInAndShow(@sprites) { Update };
	}

	public void StartForgetScene(party, partyindex, move_to_learn) {
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@party      = party;
		@partyindex = partyindex;
		@pokemon    = @party[@partyindex];
		@page = 4;
		@typebitmap = new AnimatedBitmap(_INTL("Graphics/UI/types"));
		@sprites = new List<string>();
		@sprites["background"] = new IconSprite(0, 0, @viewport);
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		SetSystemFont(@sprites["overlay"].bitmap);
		@sprites["pokeicon"] = new PokemonIconSprite(@pokemon, @viewport);
		@sprites["pokeicon"].setOffset(PictureOrigin.CENTER);
		@sprites["pokeicon"].x       = 46;
		@sprites["pokeicon"].y       = 92;
		@sprites["movesel"] = new MoveSelectionSprite(@viewport, !move_to_learn.null());
		@sprites["movesel"].visible = false;
		@sprites["movesel"].visible = true;
		@sprites["movesel"].index   = 0;
		new_move = (move_to_learn) ? new Pokemon.Move(move_to_learn) : null;
		drawSelectedMove(new_move, @pokemon.moves[0]);
		FadeInAndShow(@sprites);
	}

	public void EndScene() {
		FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@typebitmap.dispose;
		@markingbitmap&.dispose;
		@viewport.dispose;
	}

	public void Display(text) {
		@sprites["messagebox"].text = text;
		@sprites["messagebox"].visible = true;
		PlayDecisionSE;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (@sprites["messagebox"].busy()) {
				if (Input.trigger(Input.USE)) {
					if (@sprites["messagebox"].pausing()) PlayDecisionSE;
					@sprites["messagebox"].resume;
				}
			} else if (Input.trigger(Input.USE) || Input.trigger(Input.BACK)) {
				break;
			}
		}
		@sprites["messagebox"].visible = false;
	}

	public void Confirm(text) {
		ret = -1;
		@sprites["messagebox"].text    = text;
		@sprites["messagebox"].visible = true;
		using(cmdwindow = new Window_CommandPokemon(new {_INTL("Yes"), _INTL("No")})) do;
			cmdwindow.z       = @viewport.z + 1;
			cmdwindow.visible = false;
			BottomRight(cmdwindow);
			cmdwindow.y -= @sprites["messagebox"].height;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				if (!@sprites["messagebox"].busy()) cmdwindow.visible = true;
				cmdwindow.update;
				Update;
				if (!@sprites["messagebox"].busy()) {
					if (Input.trigger(Input.BACK)) {
						ret = false;
						break;
					} else if (Input.trigger(Input.USE) && @sprites["messagebox"].resume) {
						ret = (cmdwindow.index == 0);
						break;
					}
				}
			}
		}
		@sprites["messagebox"].visible = false;
		return ret;
	}

	public void ShowCommands(commands, index = 0) {
		ret = -1;
		using(cmdwindow = new Window_CommandPokemon(commands)) do;
			cmdwindow.z = @viewport.z + 1;
			cmdwindow.index = index;
			BottomRight(cmdwindow);
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				cmdwindow.update;
				Update;
				if (Input.trigger(Input.BACK)) {
					PlayCancelSE
					ret = -1;
					break;
				} else if (Input.trigger(Input.USE)) {
					PlayDecisionSE;
					ret = cmdwindow.index;
					break;
				}
			}
		}
		return ret;
	}

	public void drawMarkings(bitmap, x, y) {
		mark_variants = @markingbitmap.bitmap.height / MARK_HEIGHT;
		markings = @pokemon.markings;
		markrect = new Rect(0, 0, MARK_WIDTH, MARK_HEIGHT);
		for (int i = (@markingbitmap.bitmap.width / MARK_WIDTH); i < (@markingbitmap.bitmap.width / MARK_WIDTH); i++) { //for '(@markingbitmap.bitmap.width / MARK_WIDTH)' times do => |i|
			markrect.x = i * MARK_WIDTH;
			markrect.y = (int)Math.Min((markings[i] || 0), mark_variants - 1) * MARK_HEIGHT;
			bitmap.blt(x + (i * MARK_WIDTH), y, @markingbitmap.bitmap, markrect);
		}
	}

	public void drawPage(page) {
		if (@pokemon.egg()) {
			drawPageOneEgg;
			return;
		}
		@sprites["pokemon"].setPokemonBitmap(@pokemon);
		@sprites["pokeicon"].pokemon = @pokemon;
		@sprites["itemicon"].item = @pokemon.item_id;
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		base   = new Color(248, 248, 248);
		shadow = new Color(104, 104, 104);
		// Set background image
		@sprites["background"].setBitmap($"Graphics/UI/Summary/bg_{page}");
		imagepos = new List<string>();
		// Show the Poké Ball containing the Pokémon
		ballimage = string.Format("Graphics/UI/Summary/icon_ball_{0}", @pokemon.poke_ball);
		imagepos.Add(new {ballimage, 14, 60});
		// Show status/fainted/Pokérus infected icon
		status = -1;
		if (@pokemon.fainted()) {
			status = GameData.Status.count - 1;
		} else if (@pokemon.status != statuses.NONE) {
			status = GameData.Status.get(@pokemon.status).icon_position;
		} else if (@pokemon.pokerusStage == 1) {
			status = GameData.Status.count;
		}
		if (status >= 0) {
			imagepos.Add(new {_INTL("Graphics/UI/statuses"), 124, 100,
										0, status * GameData.Status.ICON_SIZE[1], *GameData.Status.ICON_SIZE});
		}
		// Show Pokérus cured icon
		if (@pokemon.pokerusStage == 2) {
			imagepos.Add(new {"Graphics/UI/Summary/icon_pokerus", 176, 100});
		}
		// Show shininess star
		if (@pokemon.shiny()) imagepos.Add(new {"Graphics/UI/shiny", 2, 134});
		// Draw all images
		DrawImagePositions(overlay, imagepos);
		// Write various bits of text
		pagename = new {_INTL("INFO"),
								_INTL("TRAINER MEMO"),
								_INTL("SKILLS"),
								_INTL("MOVES"),
								_INTL("RIBBONS")}[page - 1];
		textpos = new {
			new {pagename, 26, 22, :left, base, shadow},
			new {@pokemon.name, 46, 68, :left, base, shadow},
			new {@pokemon.level.ToString(), 46, 98, :left, new Color(64, 64, 64), new Color(176, 176, 176)},
			new {_INTL("Item"), 66, 324, :left, base, shadow}
		}
		// Write the held item's name
		if (@pokemon.hasItem()) {
			textpos.Add(new {@pokemon.item.name, 16, 358, :left, new Color(64, 64, 64), new Color(176, 176, 176)});
		} else {
			textpos.Add(new {_INTL("None"), 16, 358, :left, new Color(192, 200, 208), new Color(208, 216, 224)});
		}
		// Write the gender symbol
		if (@pokemon.male()) {
			textpos.Add(new {_INTL("♂"), 178, 68, :left, new Color(24, 112, 216), new Color(136, 168, 208)});
		} else if (@pokemon.female()) {
			textpos.Add(new {_INTL("♀"), 178, 68, :left, new Color(248, 56, 32), new Color(224, 152, 144)});
		}
		// Draw all text
		DrawTextPositions(overlay, textpos);
		// Draw the Pokémon's markings
		drawMarkings(overlay, 84, 292);
		// Draw page-specific information
		switch (page) {
			case 1:  drawPageOne; break;
			case 2:  drawPageTwo; break;
			case 3:  drawPageThree; break;
			case 4:  drawPageFour; break;
			case 5:  drawPageFive; break;
		}
	}

	public void drawPageOne() {
		overlay = @sprites["overlay"].bitmap;
		base   = new Color(248, 248, 248);
		shadow = new Color(104, 104, 104);
		dexNumBase   = (@pokemon.shiny()) ? new Color(248, 56, 32) : new Color(64, 64, 64);
		dexNumShadow = (@pokemon.shiny()) ? new Color(224, 152, 144) : new Color(176, 176, 176);
		// If a Shadow Pokémon, draw the heart gauge area and bar
		if (@pokemon.shadowPokemon()) {
			shadowfract = @pokemon.heart_gauge.to_f / @pokemon.max_gauge_size;
			imagepos = new {
				new {"Graphics/UI/Summary/overlay_shadow", 224, 240},
				new {"Graphics/UI/Summary/overlay_shadowbar", 242, 280, 0, 0, (int)Math.Floor(shadowfract * 248), -1}
			}
			DrawImagePositions(overlay, imagepos);
		}
		// Write various bits of text
		textpos = new {
			new {_INTL("Dex No."), 238, 86, :left, base, shadow},
			new {_INTL("Species"), 238, 118, :left, base, shadow},
			new {@pokemon.speciesName, 435, 118, :center, new Color(64, 64, 64), new Color(176, 176, 176)},
			new {_INTL("Type"), 238, 150, :left, base, shadow},
			new {_INTL("OT"), 238, 182, :left, base, shadow},
			new {_INTL("ID No."), 238, 214, :left, base, shadow}
		}
		// Write the Regional/National Dex number
		dexnum = 0;
		dexnumshift = false;
		if (Game.GameData.player.pokedex.unlocked(-1)) {   // National Dex is unlocked
			dexnum = @nationalDexList.index(@pokemon.species_data.species) || 0;
			if (Settings.DEXES_WITH_OFFSETS.Contains(-1)) dexnumshift = true;
		} else {
			for (int i = (Game.GameData.player.pokedex.dexes_count - 1); i < (Game.GameData.player.pokedex.dexes_count - 1); i++) { //for '(Game.GameData.player.pokedex.dexes_count - 1)' times do => |i|
				if (!Game.GameData.player.pokedex.unlocked(i)) continue;
				num = GetRegionalNumber(i, @pokemon.species);
				if (num <= 0) break;
				dexnum = num;
				if (Settings.DEXES_WITH_OFFSETS.Contains(i)) dexnumshift = true;
				break;
			}
		}
		if (dexnum <= 0) {
			textpos.Add(new {"???", 435, 86, :center, dexNumBase, dexNumShadow});
		} else {
			if (dexnumshift) dexnum -= 1;
			textpos.Add(new {string.Format("{0:3}", dexnum), 435, 86, :center, dexNumBase, dexNumShadow});
		}
		// Write Original Trainer's name and ID number
		if (@pokemon.owner.name.empty()) {
			textpos.Add(new {_INTL("RENTAL"), 435, 182, :center, new Color(64, 64, 64), new Color(176, 176, 176)});
			textpos.Add(new {"?????", 435, 214, :center, new Color(64, 64, 64), new Color(176, 176, 176)});
		} else {
			ownerbase   = new Color(64, 64, 64);
			ownershadow = new Color(176, 176, 176);
			switch (@pokemon.owner.gender) {
				case 0:
					ownerbase = new Color(24, 112, 216);
					ownershadow = new Color(136, 168, 208);
					break;
				case 1:
					ownerbase = new Color(248, 56, 32);
					ownershadow = new Color(224, 152, 144);
					break;
			}
			textpos.Add(new {@pokemon.owner.name, 435, 182, :center, ownerbase, ownershadow});
			textpos.Add(new {string.Format("{0:5}", @pokemon.owner.public_id), 435, 214, :center,
										new Color(64, 64, 64), new Color(176, 176, 176)});
		}
		// Write Exp text OR heart gauge message (if a Shadow Pokémon)
		if (@pokemon.shadowPokemon()) {
			textpos.Add(new {_INTL("Heart Gauge"), 238, 246, :left, base, shadow});
			black_text_tag = shadowc3tag(BLACK_TEXT_BASE, BLACK_TEXT_SHADOW);
			heartmessage = new {_INTL("The door to its heart is open! Undo the final lock!"),
											_INTL("The door to its heart is almost fully open."),
											_INTL("The door to its heart is nearly open."),
											_INTL("The door to its heart is opening wider."),
											_INTL("The door to its heart is opening up."),
											_INTL("The door to its heart is tightly shut.")}[@pokemon.heartStage];
			memo = black_text_tag + heartmessage;
			drawFormattedTextEx(overlay, 234, 308, 264, memo);
		} else {
			endexp = @pokemon.growth_rate.minimum_exp_for_level(@pokemon.level + 1);
			textpos.Add(new {_INTL("Exp. Points"), 238, 246, :left, base, shadow});
			textpos.Add(new {@pokemon.exp.to_s_formatted, 488, 278, :right, new Color(64, 64, 64), new Color(176, 176, 176)});
			textpos.Add(new {_INTL("To Next Lv."), 238, 310, :left, base, shadow});
			textpos.Add(new {(endexp - @pokemon.exp).to_s_formatted, 488, 342, :right, new Color(64, 64, 64), new Color(176, 176, 176)});
		}
		// Draw all text
		DrawTextPositions(overlay, textpos);
		// Draw Pokémon type(s)
		@pokemon.types.each_with_index do |type, i|
			type_number = GameData.Type.get(type).icon_position;
			type_rect = new Rect(0, type_number * GameData.Type.ICON_SIZE[1], *GameData.Type.ICON_SIZE);
			type_x = (@pokemon.types.length == 1) ? 402 : 370 + ((GameData.Type.ICON_SIZE[0] + 2) * i);
			overlay.blt(type_x, 146, @typebitmap.bitmap, type_rect);
		}
		// Draw Exp bar
		if (@pokemon.level < GameData.GrowthRate.max_level) {
			w = @pokemon.exp_fraction * 128;
			w = ((int)Math.Round(w / 2)) * 2;
			DrawImagePositions(overlay,
													new {"Graphics/UI/Summary/overlay_exp", 362, 372, 0, 0, w, 6});
		}
	}

	public void drawPageOneEgg() {
		@sprites["itemicon"].item = @pokemon.item_id;
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		base   = new Color(248, 248, 248);
		shadow = new Color(104, 104, 104);
		// Set background image
		@sprites["background"].setBitmap("Graphics/UI/Summary/bg_egg");
		imagepos = new List<string>();
		// Show the Poké Ball containing the Pokémon
		ballimage = string.Format("Graphics/UI/Summary/icon_ball_{0}", @pokemon.poke_ball);
		imagepos.Add(new {ballimage, 14, 60});
		// Draw all images
		DrawImagePositions(overlay, imagepos);
		// Write various bits of text
		textpos = new {
			new {_INTL("TRAINER MEMO"), 26, 22, :left, base, shadow},
			new {@pokemon.name, 46, 68, :left, base, shadow},
			new {_INTL("Item"), 66, 324, :left, base, shadow}
		}
		// Write the held item's name
		if (@pokemon.hasItem()) {
			textpos.Add(new {@pokemon.item.name, 16, 358, :left, new Color(64, 64, 64), new Color(176, 176, 176)});
		} else {
			textpos.Add(new {_INTL("None"), 16, 358, :left, new Color(192, 200, 208), new Color(208, 216, 224)});
		}
		// Draw all text
		DrawTextPositions(overlay, textpos);
		red_text_tag = shadowc3tag(RED_TEXT_BASE, RED_TEXT_SHADOW);
		black_text_tag = shadowc3tag(BLACK_TEXT_BASE, BLACK_TEXT_SHADOW);
		memo = "";
		// Write date received
		if (@pokemon.timeReceived) {
			date  = @pokemon.timeReceived.day;
			month = GetMonthName(@pokemon.timeReceived.mon);
			year  = @pokemon.timeReceived.year;
			memo += black_text_tag + _INTL("{1} {2}, {3}", date, month, year) + "\n";
		}
		// Write map name egg was received on
		mapname = GetMapNameFromId(@pokemon.obtain_map);
		if (@pokemon.obtain_text && !@pokemon.obtain_text.empty()) mapname = @pokemon.obtain_text;
		if (mapname && mapname != "") {
			mapname = red_text_tag + mapname + black_text_tag;
			memo += black_text_tag + _INTL("A mysterious Pokémon Egg received from {1}.", mapname) + "\n";
		} else {
			memo += black_text_tag + _INTL("A mysterious Pokémon Egg.") + "\n";
		}
		memo += "\n";   // Empty line
		// Write Egg Watch blurb
		memo += black_text_tag + _INTL("\"The Egg Watch\"") + "\n";
		eggstate = _INTL("It looks like this Egg will take a long time to hatch.");
		if (@pokemon.steps_to_hatch < 10_200) eggstate = _INTL("What will hatch from this? It doesn't seem close to hatching.");
		if (@pokemon.steps_to_hatch < 2550) eggstate = _INTL("It appears to move occasionally. It may be close to hatching.");
		if (@pokemon.steps_to_hatch < 1275) eggstate = _INTL("Sounds can be heard coming from inside! It will hatch soon!");
		memo += black_text_tag + eggstate;
		// Draw all text
		drawFormattedTextEx(overlay, 232, 86, 268, memo);
		// Draw the Pokémon's markings
		drawMarkings(overlay, 84, 292);
	}

	public void drawPageTwo() {
		overlay = @sprites["overlay"].bitmap;
		red_text_tag = shadowc3tag(RED_TEXT_BASE, RED_TEXT_SHADOW);
		black_text_tag = shadowc3tag(BLACK_TEXT_BASE, BLACK_TEXT_SHADOW);
		memo = "";
		// Write nature
		showNature = !@pokemon.shadowPokemon() || @pokemon.heartStage <= 3;
		if (showNature) {
			nature_name = red_text_tag + @pokemon.nature.name + black_text_tag;
			memo += _INTL("{1} nature.", nature_name) + "\n";
		}
		// Write date received
		if (@pokemon.timeReceived) {
			date  = @pokemon.timeReceived.day;
			month = GetMonthName(@pokemon.timeReceived.mon);
			year  = @pokemon.timeReceived.year;
			memo += black_text_tag + _INTL("{1} {2}, {3}", date, month, year) + "\n";
		}
		// Write map name Pokémon was received on
		mapname = GetMapNameFromId(@pokemon.obtain_map);
		if (@pokemon.obtain_text && !@pokemon.obtain_text.empty()) mapname = @pokemon.obtain_text;
		if (nil_or_empty(mapname)) mapname = _INTL("Faraway place");
		memo += red_text_tag + mapname + "\n";
		// Write how Pokémon was obtained
		mettext = new {
			_INTL("Met at Lv. {1}.", @pokemon.obtain_level),
			_INTL("Egg received."),
			_INTL("Traded at Lv. {1}.", @pokemon.obtain_level),
			"",
			_INTL("Had a fateful encounter at Lv. {1}.", @pokemon.obtain_level);
		}[@pokemon.obtain_method];
		if (mettext && mettext != "") memo += black_text_tag + mettext + "\n";
		// If Pokémon was hatched, write when and where it hatched
		if (@pokemon.obtain_method == 1) {
			if (@pokemon.timeEggHatched) {
				date  = @pokemon.timeEggHatched.day;
				month = GetMonthName(@pokemon.timeEggHatched.mon);
				year  = @pokemon.timeEggHatched.year;
				memo += black_text_tag + _INTL("{1} {2}, {3}", date, month, year) + "\n";
			}
			mapname = GetMapNameFromId(@pokemon.hatched_map);
			if (nil_or_empty(mapname)) mapname = _INTL("Faraway place");
			memo += red_text_tag + mapname + "\n";
			memo += black_text_tag + _INTL("Egg hatched.") + "\n";
		} else {
			memo += "\n";   // Empty line
		}
		// Write characteristic
		if (showNature) {
			best_stat = null;
			best_iv = 0;
			stats_order = new {:HP, :ATTACK, :DEFENSE, :SPEED, :SPECIAL_ATTACK, :SPECIAL_DEFENSE};
			start_point = @pokemon.personalID % stats_order.length;   // Tiebreaker
			for (int i = stats_order.length; i < stats_order.length; i++) { //for 'stats_order.length' times do => |i|
				stat = stats_order[(i + start_point) % stats_order.length];
				if (!best_stat || @pokemon.iv[stat] > @pokemon.iv[best_stat]) {
					best_stat = stat;
					best_iv = @pokemon.iv[best_stat];
				}
			}
			characteristics = {
				HP              = new {_INTL("Loves to eat."),
														_INTL("Takes plenty of siestas."),
														_INTL("Nods off a lot."),
														_INTL("Scatters things often."),
														_INTL("Likes to relax.")},
				ATTACK          = new {_INTL("Proud of its power."),
														_INTL("Likes to thrash about."),
														_INTL("A little quick tempered."),
														_INTL("Likes to fight."),
														_INTL("Quick tempered.")},
				DEFENSE         = new {_INTL("Sturdy body."),
														_INTL("Capable of taking hits."),
														_INTL("Highly persistent."),
														_INTL("Good endurance."),
														_INTL("Good perseverance.")},
				SPECIAL_ATTACK  = new {_INTL("Highly curious."),
														_INTL("Mischievous."),
														_INTL("Thoroughly cunning."),
														_INTL("Often lost in thought."),
														_INTL("Very finicky.")},
				SPECIAL_DEFENSE = new {_INTL("Strong willed."),
														_INTL("Somewhat vain."),
														_INTL("Strongly defiant."),
														_INTL("Hates to lose."),
														_INTL("Somewhat stubborn.")},
				SPEED           = new {_INTL("Likes to run."),
														_INTL("Alert to sounds."),
														_INTL("Impetuous and silly."),
														_INTL("Somewhat of a clown."),
														_INTL("Quick to flee.")};
			}
			memo += black_text_tag + characteristics[best_stat][best_iv % 5] + "\n";
		}
		// Write all text
		drawFormattedTextEx(overlay, 232, 86, 268, memo);
	}

	public void drawPageThree() {
		overlay = @sprites["overlay"].bitmap;
		base   = new Color(248, 248, 248);
		shadow = new Color(104, 104, 104);
		// Determine which stats are boosted and lowered by the Pokémon's nature
		statshadows = new List<string>();
		GameData.Stat.each_main(s => statshadows[s.id] = shadow);
		if (!@pokemon.shadowPokemon() || @pokemon.heartStage <= 3) {
			@pokemon.nature_for_stats.stat_changes.each do |change|
				if (change[1] > 0) statshadows[change[0]] = new Color(136, 96, 72);
				if (change[1] < 0) statshadows[change[0]] = new Color(64, 120, 152);
			}
		}
		// Write various bits of text
		textpos = new {
			new {_INTL("HP"), 292, 82, :center, base, statshadows[:HP]},
			new {string.Format("{0}/{0}", @pokemon.hp, @pokemon.totalhp), 462, 82, :right, new Color(64, 64, 64), new Color(176, 176, 176)},
			new {_INTL("Attack"), 248, 126, :left, base, statshadows[:ATTACK]},
			new {@pokemon.attack.ToString(), 456, 126, :right, new Color(64, 64, 64), new Color(176, 176, 176)},
			new {_INTL("Defense"), 248, 158, :left, base, statshadows[:DEFENSE]},
			new {@pokemon.defense.ToString(), 456, 158, :right, new Color(64, 64, 64), new Color(176, 176, 176)},
			new {_INTL("Sp. Atk"), 248, 190, :left, base, statshadows[:SPECIAL_ATTACK]},
			new {@pokemon.spatk.ToString(), 456, 190, :right, new Color(64, 64, 64), new Color(176, 176, 176)},
			new {_INTL("Sp. Def"), 248, 222, :left, base, statshadows[:SPECIAL_DEFENSE]},
			new {@pokemon.spdef.ToString(), 456, 222, :right, new Color(64, 64, 64), new Color(176, 176, 176)},
			new {_INTL("Speed"), 248, 254, :left, base, statshadows[:SPEED]},
			new {@pokemon.speed.ToString(), 456, 254, :right, new Color(64, 64, 64), new Color(176, 176, 176)},
			new {_INTL("Ability"), 224, 290, :left, base, shadow}
		}
		// Draw ability name and description
		ability = @pokemon.ability;
		if (ability) {
			textpos.Add(new {ability.name, 362, 290, :left, new Color(64, 64, 64), new Color(176, 176, 176)});
			drawTextEx(overlay, 224, 322, 282, 2, ability.description, new Color(64, 64, 64), new Color(176, 176, 176));
		}
		// Draw all text
		DrawTextPositions(overlay, textpos);
		// Draw HP bar
		if (@pokemon.hp > 0) {
			w = @pokemon.hp * 96 / @pokemon.totalhp.to_f;
			if (w < 1) w = 1;
			w = ((int)Math.Round(w / 2)) * 2;
			hpzone = 0;
			if (@pokemon.hp <= (int)Math.Floor(@pokemon.totalhp / 2)) hpzone = 1;
			if (@pokemon.hp <= (int)Math.Floor(@pokemon.totalhp / 4)) hpzone = 2;
			imagepos = new {
				new {"Graphics/UI/Summary/overlay_hp", 360, 110, 0, hpzone * 6, w, 6};
			}
			DrawImagePositions(overlay, imagepos);
		}
	}

	public void drawPageFour() {
		overlay = @sprites["overlay"].bitmap;
		moveBase   = new Color(64, 64, 64);
		moveShadow = new Color(176, 176, 176);
		ppBase   = new {moveBase,                // More than 1/2 of total PP
								new Color(248, 192, 0),    // 1/2 of total PP or less
								new Color(248, 136, 32),   // 1/4 of total PP or less
								new Color(248, 72, 72)};    // Zero PP
		ppShadow = new {moveShadow,             // More than 1/2 of total PP
								new Color(144, 104, 0),   // 1/2 of total PP or less
								new Color(144, 72, 24),   // 1/4 of total PP or less
								new Color(136, 48, 48)};   // Zero PP
		@sprites["pokemon"].visible  = true;
		@sprites["pokeicon"].visible = false;
		@sprites["itemicon"].visible = true;
		textpos  = new List<string>();
		imagepos = new List<string>();
		// Write move names, types and PP amounts for each known move
		yPos = 104;
		for (int i = Pokemon.MAX_MOVES; i < Pokemon.MAX_MOVES; i++) { //for 'Pokemon.MAX_MOVES' times do => |i|
			move = @pokemon.moves[i];
			if (move) {
				type_number = GameData.Type.get(move.display_type(@pokemon)).icon_position;
				imagepos.Add(new {_INTL("Graphics/UI/types"), 248, yPos - 4, 0, type_number * GameData.Type.ICON_SIZE[1], *GameData.Type.ICON_SIZE});
				textpos.Add(new {move.name, 316, yPos, :left, moveBase, moveShadow});
				if (move.total_pp > 0) {
					textpos.Add(new {_INTL("PP"), 342, yPos + 32, :left, moveBase, moveShadow});
					ppfraction = 0;
					if (move.pp == 0) {
						ppfraction = 3;
					} else if (move.pp * 4 <= move.total_pp) {
						ppfraction = 2;
					} else if (move.pp * 2 <= move.total_pp) {
						ppfraction = 1;
					}
					textpos.Add(new {string.Format("{0}/{0}", move.pp, move.total_pp), 460, yPos + 32, :right, ppBase[ppfraction], ppShadow[ppfraction]});
				}
			} else {
				textpos.Add(new {"-", 316, yPos, :left, moveBase, moveShadow});
				textpos.Add(new {"--", 442, yPos + 32, :right, moveBase, moveShadow});
			}
			yPos += 64;
		}
		// Draw all text and images
		DrawTextPositions(overlay, textpos);
		DrawImagePositions(overlay, imagepos);
	}

	public void drawPageFourSelecting(move_to_learn) {
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		base   = new Color(248, 248, 248);
		shadow = new Color(104, 104, 104);
		moveBase   = new Color(64, 64, 64);
		moveShadow = new Color(176, 176, 176);
		ppBase   = new {moveBase,                // More than 1/2 of total PP
								new Color(248, 192, 0),    // 1/2 of total PP or less
								new Color(248, 136, 32),   // 1/4 of total PP or less
								new Color(248, 72, 72)};    // Zero PP
		ppShadow = new {moveShadow,             // More than 1/2 of total PP
								new Color(144, 104, 0),   // 1/2 of total PP or less
								new Color(144, 72, 24),   // 1/4 of total PP or less
								new Color(136, 48, 48)};   // Zero PP
		// Set background image
		if (move_to_learn) {
			@sprites["background"].setBitmap("Graphics/UI/Summary/bg_learnmove");
		} else {
			@sprites["background"].setBitmap("Graphics/UI/Summary/bg_movedetail");
		}
		// Write various bits of text
		textpos = new {
			new {_INTL("MOVES"), 26, 22, :left, base, shadow},
			new {_INTL("CATEGORY"), 20, 128, :left, base, shadow},
			new {_INTL("POWER"), 20, 160, :left, base, shadow},
			new {_INTL("ACCURACY"), 20, 192, :left, base, shadow}
		}
		imagepos = new List<string>();
		// Write move names, types and PP amounts for each known move
		yPos = 104;
		if (move_to_learn) yPos -= 76;
		limit = (move_to_learn) ? Pokemon.MAX_MOVES + 1 : Pokemon.MAX_MOVES;
		for (int i = limit; i < limit; i++) { //for 'limit' times do => |i|
			move = @pokemon.moves[i];
			if (i == Pokemon.MAX_MOVES) {
				move = move_to_learn;
				yPos += 20;
			}
			if (move) {
				type_number = GameData.Type.get(move.display_type(@pokemon)).icon_position;
				imagepos.Add(new {_INTL("Graphics/UI/types"), 248, yPos - 4, 0, type_number * GameData.Type.ICON_SIZE[1], *GameData.Type.ICON_SIZE});
				textpos.Add(new {move.name, 316, yPos, :left, moveBase, moveShadow});
				if (move.total_pp > 0) {
					textpos.Add(new {_INTL("PP"), 342, yPos + 32, :left, moveBase, moveShadow});
					ppfraction = 0;
					if (move.pp == 0) {
						ppfraction = 3;
					} else if (move.pp * 4 <= move.total_pp) {
						ppfraction = 2;
					} else if (move.pp * 2 <= move.total_pp) {
						ppfraction = 1;
					}
					textpos.Add(new {string.Format("{0}/{0}", move.pp, move.total_pp), 460, yPos + 32, :right,
												ppBase[ppfraction], ppShadow[ppfraction]});
				}
			} else {
				textpos.Add(new {"-", 316, yPos, :left, moveBase, moveShadow});
				textpos.Add(new {"--", 442, yPos + 32, :right, moveBase, moveShadow});
			}
			yPos += 64;
		}
		// Draw all text and images
		DrawTextPositions(overlay, textpos);
		DrawImagePositions(overlay, imagepos);
		// Draw Pokémon's type icon(s)
		@pokemon.types.each_with_index do |type, i|
			type_number = GameData.Type.get(type).icon_position;
			type_rect = new Rect(0, type_number * GameData.Type.ICON_SIZE[1], *GameData.Type.ICON_SIZE);
			type_x = (@pokemon.types.length == 1) ? 130 : 96 + ((GameData.Type.ICON_SIZE[0] + 6) * i);
			overlay.blt(type_x, 78, @typebitmap.bitmap, type_rect);
		}
	}

	public void drawSelectedMove(move_to_learn, selected_move) {
		// Draw all of page four, except selected move's details
		drawPageFourSelecting(move_to_learn);
		// Set various values
		overlay = @sprites["overlay"].bitmap;
		base = new Color(64, 64, 64);
		shadow = new Color(176, 176, 176);
		if (@sprites["pokemon"]) @sprites["pokemon"].visible = false;
		@sprites["pokeicon"].pokemon = @pokemon;
		@sprites["pokeicon"].visible = true;
		if (@sprites["itemicon"]) @sprites["itemicon"].visible = false;
		textpos = new List<string>();
		// Write power and accuracy values for selected move
		switch (selected_move.display_damage(@pokemon)) {
			case 0:  textpos.Add(new {"---", 216, 160, :right, base, shadow}); break;   // Status move
			case 1:  textpos.Add(new {"???", 216, 160, :right, base, shadow}); break;   // Variable power move
			default:        textpos.Add(new {selected_move.display_damage(@pokemon).ToString(), 216, 160, :right, base, shadow}); break;
		}
		if (selected_move.display_accuracy(@pokemon) == 0) {
			textpos.Add(new {"---", 216, 192, :right, base, shadow});
		} else {
			textpos.Add(new {$"{selected_move.display_accuracy(@pokemon)}%", 216 + overlay.text_size("%").width, 192, :right, base, shadow});
		}
		// Draw all text
		DrawTextPositions(overlay, textpos);
		// Draw selected move's damage category icon
		imagepos = new {"Graphics/UI/category", 166, 124,
								0, selected_move.display_category(@pokemon) * CATEGORY_ICON_SIZE[1], *CATEGORY_ICON_SIZE};
		DrawImagePositions(overlay, imagepos);
		// Draw selected move's description
		drawTextEx(overlay, 4, 224, 230, 5, selected_move.description, base, shadow);
	}

	public void drawPageFive() {
		overlay = @sprites["overlay"].bitmap;
		@sprites["uparrow"].visible   = false;
		@sprites["downarrow"].visible = false;
		// Write various bits of text
		textpos = new {
			new {_INTL("No. of Ribbons:"), 234, 338, :left, new Color(64, 64, 64), new Color(176, 176, 176)},
			new {@pokemon.numRibbons.ToString(), 450, 338, :right, new Color(64, 64, 64), new Color(176, 176, 176)}
		}
		// Draw all text
		DrawTextPositions(overlay, textpos);
		// Show all ribbons
		imagepos = new List<string>();
		coord = 0;
		(@ribbonOffset * 4...(@ribbonOffset * 4) + 12).each do |i|
			if (!@pokemon.ribbons[i]) break;
			ribbon_data = GameData.Ribbon.get(@pokemon.ribbons[i]);
			ribn = ribbon_data.icon_position;
			imagepos.Add(new {"Graphics/UI/Summary/ribbons",
										230 + (68 * (coord % 4)), 78 + (68 * (int)Math.Floor(coord / 4)),
										64 * (ribn % 8), 64 * (int)Math.Floor(ribn / 8), 64, 64});
			coord += 1;
		}
		// Draw all images
		DrawImagePositions(overlay, imagepos);
	}

	public void drawSelectedRibbon(ribbonid) {
		// Draw all of page five
		drawPage(5);
		// Set various values
		overlay = @sprites["overlay"].bitmap;
		base   = new Color(64, 64, 64);
		shadow = new Color(176, 176, 176);
		nameBase   = new Color(248, 248, 248);
		nameShadow = new Color(104, 104, 104);
		// Get data for selected ribbon
		name = ribbonid ? GameData.Ribbon.get(ribbonid).name : "";
		desc = ribbonid ? GameData.Ribbon.get(ribbonid).description : "";
		// Draw the description box
		imagepos = new {
			new {"Graphics/UI/Summary/overlay_ribbon", 8, 280};
		}
		DrawImagePositions(overlay, imagepos);
		// Draw name of selected ribbon
		textpos = new {
			new {name, 18, 292, :left, nameBase, nameShadow};
		}
		DrawTextPositions(overlay, textpos);
		// Draw selected ribbon's description
		drawTextEx(overlay, 18, 324, 480, 2, desc, base, shadow);
	}

	public void GoToPrevious() {
		newindex = @partyindex;
		while (newindex > 0) {
			newindex -= 1;
			if (@party[newindex] && (@page == 1 || !@party[newindex].egg())) {
				@partyindex = newindex;
				break;
			}
		}
	}

	public void GoToNext() {
		newindex = @partyindex;
		while (newindex < @party.length - 1) {
			newindex += 1;
			if (@party[newindex] && (@page == 1 || !@party[newindex].egg())) {
				@partyindex = newindex;
				break;
			}
		}
	}

	public void ChangePokemon() {
		@pokemon = @party[@partyindex];
		@sprites["pokemon"].setPokemonBitmap(@pokemon);
		@sprites["itemicon"].item = @pokemon.item_id;
		SEStop;
		@pokemon.play_cry;
	}

	public void MoveSelection() {
		@sprites["movesel"].visible = true;
		@sprites["movesel"].index   = 0;
		selmove    = 0;
		oldselmove = 0;
		switching = false;
		drawSelectedMove(null, @pokemon.moves[selmove]);
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (@sprites["movepresel"].index == @sprites["movesel"].index) {
				@sprites["movepresel"].z = @sprites["movesel"].z + 1;
			} else {
				@sprites["movepresel"].z = @sprites["movesel"].z;
			}
			if (Input.trigger(Input.BACK)) {
				(switching) ? PlayCancelSE : PlayCloseMenuSE
				if (!switching) break;
				@sprites["movepresel"].visible = false;
				switching = false;
			} else if (Input.trigger(Input.USE)) {
				PlayDecisionSE;
				if (selmove == Pokemon.MAX_MOVES) {
					if (!switching) break;
					@sprites["movepresel"].visible = false;
					switching = false;
				} else if (!@pokemon.shadowPokemon()) {
					if (switching) {
						tmpmove                    = @pokemon.moves[oldselmove];
						@pokemon.moves[oldselmove] = @pokemon.moves[selmove];
						@pokemon.moves[selmove]    = tmpmove;
						@sprites["movepresel"].visible = false;
						switching = false;
						drawSelectedMove(null, @pokemon.moves[selmove]);
					} else {
						@sprites["movepresel"].index   = selmove;
						@sprites["movepresel"].visible = true;
						oldselmove = selmove;
						switching = true;
					}
				}
			} else if (Input.trigger(Input.UP)) {
				selmove -= 1;
				if (selmove < Pokemon.MAX_MOVES && selmove >= @pokemon.numMoves) {
					selmove = @pokemon.numMoves - 1;
				}
				if (selmove >= Pokemon.MAX_MOVES) selmove = 0;
				if (selmove < 0) selmove = @pokemon.numMoves - 1;
				@sprites["movesel"].index = selmove;
				PlayCursorSE;
				drawSelectedMove(null, @pokemon.moves[selmove]);
			} else if (Input.trigger(Input.DOWN)) {
				selmove += 1;
				if (selmove < Pokemon.MAX_MOVES && selmove >= @pokemon.numMoves) selmove = 0;
				if (selmove >= Pokemon.MAX_MOVES) selmove = 0;
				if (selmove < 0) selmove = Pokemon.MAX_MOVES;
				@sprites["movesel"].index = selmove;
				PlayCursorSE;
				drawSelectedMove(null, @pokemon.moves[selmove]);
			}
		}
		@sprites["movesel"].visible = false;
	}

	public void RibbonSelection() {
		@sprites["ribbonsel"].visible = true;
		@sprites["ribbonsel"].index   = 0;
		selribbon    = @ribbonOffset * 4;
		oldselribbon = selribbon;
		switching = false;
		numRibbons = @pokemon.ribbons.length;
		numRows    = (int)Math.Max((int)Math.Floor((numRibbons + 3) / 4), 3);
		drawSelectedRibbon(@pokemon.ribbons[selribbon]);
		do { //loop; while (true);
			@sprites["uparrow"].visible   = (@ribbonOffset > 0);
			@sprites["downarrow"].visible = (@ribbonOffset < numRows - 3);
			Graphics.update;
			Input.update;
			Update;
			if (@sprites["ribbonpresel"].index == @sprites["ribbonsel"].index) {
				@sprites["ribbonpresel"].z = @sprites["ribbonsel"].z + 1;
			} else {
				@sprites["ribbonpresel"].z = @sprites["ribbonsel"].z;
			}
			hasMovedCursor = false;
			if (Input.trigger(Input.BACK)) {
				(switching) ? PlayCancelSE : PlayCloseMenuSE
				if (!switching) break;
				@sprites["ribbonpresel"].visible = false;
				switching = false;
			} else if (Input.trigger(Input.USE)) {
				if (switching) {
					PlayDecisionSE;
					tmpribbon                      = @pokemon.ribbons[oldselribbon];
					@pokemon.ribbons[oldselribbon] = @pokemon.ribbons[selribbon];
					@pokemon.ribbons[selribbon]    = tmpribbon;
					if (@pokemon.ribbons[oldselribbon] || @pokemon.ribbons[selribbon]) {
						@pokemon.ribbons.compact!;
						if (selribbon >= numRibbons) {
							selribbon = numRibbons - 1;
							hasMovedCursor = true;
						}
					}
					@sprites["ribbonpresel"].visible = false;
					switching = false;
					drawSelectedRibbon(@pokemon.ribbons[selribbon]);
				} else {
					if (@pokemon.ribbons[selribbon]) {
						PlayDecisionSE;
						@sprites["ribbonpresel"].index = selribbon - (@ribbonOffset * 4);
						oldselribbon = selribbon;
						@sprites["ribbonpresel"].visible = true;
						switching = true;
					}
				}
			} else if (Input.trigger(Input.UP)) {
				selribbon -= 4;
				if (selribbon < 0) selribbon += numRows * 4;
				hasMovedCursor = true;
				PlayCursorSE;
			} else if (Input.trigger(Input.DOWN)) {
				selribbon += 4;
				if (selribbon >= numRows * 4) selribbon -= numRows * 4;
				hasMovedCursor = true;
				PlayCursorSE;
			} else if (Input.trigger(Input.LEFT)) {
				selribbon -= 1;
				if (selribbon % 4 == 3) selribbon += 4;
				hasMovedCursor = true;
				PlayCursorSE;
			} else if (Input.trigger(Input.RIGHT)) {
				selribbon += 1;
				if (selribbon % 4 == 0) selribbon -= 4;
				hasMovedCursor = true;
				PlayCursorSE;
			}
			if (!hasMovedCursor) continue;
			@ribbonOffset = (int)Math.Floor(selribbon / 4) if selribbon < @ribbonOffset * 4;
			@ribbonOffset = (int)Math.Floor(selribbon / 4) - 2 if selribbon >= (@ribbonOffset + 3) * 4;
			if (@ribbonOffset < 0) @ribbonOffset = 0;
			if (@ribbonOffset > numRows - 3) @ribbonOffset = numRows - 3;
			@sprites["ribbonsel"].index    = selribbon - (@ribbonOffset * 4);
			@sprites["ribbonpresel"].index = oldselribbon - (@ribbonOffset * 4);
			drawSelectedRibbon(@pokemon.ribbons[selribbon]);
		}
		@sprites["ribbonsel"].visible = false;
	}

	public void Marking(pokemon) {
		@sprites["markingbg"].visible      = true;
		@sprites["markingoverlay"].visible = true;
		@sprites["markingsel"].visible     = true;
		base   = new Color(248, 248, 248);
		shadow = new Color(104, 104, 104);
		ret = pokemon.markings.clone;
		markings = pokemon.markings.clone;
		mark_variants = @markingbitmap.bitmap.height / MARK_HEIGHT;
		index = 0;
		redraw = true;
		markrect = new Rect(0, 0, MARK_WIDTH, MARK_HEIGHT);
		do { //loop; while (true);
			// Redraw the markings and text
			if (redraw) {
				@sprites["markingoverlay"].bitmap.clear;
				for (int i = (@markingbitmap.bitmap.width / MARK_WIDTH); i < (@markingbitmap.bitmap.width / MARK_WIDTH); i++) { //for '(@markingbitmap.bitmap.width / MARK_WIDTH)' times do => |i|
					markrect.x = i * MARK_WIDTH;
					markrect.y = (int)Math.Min((markings[i] || 0), mark_variants - 1) * MARK_HEIGHT;
					@sprites["markingoverlay"].bitmap.blt(300 + (58 * (i % 3)), 154 + (50 * (i / 3)),
																								@markingbitmap.bitmap, markrect);
				}
				textpos = new {
					new {_INTL("Mark {1}", pokemon.name), 366, 102, :center, base, shadow},
					new {_INTL("OK"), 366, 254, :center, base, shadow},
					new {_INTL("Cancel"), 366, 304, :center, base, shadow}
				}
				DrawTextPositions(@sprites["markingoverlay"].bitmap, textpos);
				redraw = false;
			}
			// Reposition the cursor
			@sprites["markingsel"].x = 284 + (58 * (index % 3));
			@sprites["markingsel"].y = 144 + (50 * (index / 3));
			switch (index) {
				case 6:   // OK
					@sprites["markingsel"].x = 284;
					@sprites["markingsel"].y = 244;
					@sprites["markingsel"].src_rect.y = @sprites["markingsel"].bitmap.height / 2;
					break;
				case 7:   // Cancel
					@sprites["markingsel"].x = 284;
					@sprites["markingsel"].y = 294;
					@sprites["markingsel"].src_rect.y = @sprites["markingsel"].bitmap.height / 2;
					break;
				default:
					@sprites["markingsel"].src_rect.y = 0;
					break;
			}
			Graphics.update;
			Input.update;
			Update;
			if (Input.trigger(Input.BACK)) {
				PlayCloseMenuSE;
				break;
			} else if (Input.trigger(Input.USE)) {
				PlayDecisionSE;
				switch (index) {
					case 6:   // OK
						ret = markings;
						break;
						break;
					case 7:   // Cancel
						break;
						break;
					default:
						markings[index] = ((markings[index] || 0) + 1) % mark_variants;
						redraw = true;
						break;
				}
			} else if (Input.trigger(Input.ACTION)) {
				if (index < 6 && markings[index] > 0) {
					PlayDecisionSE;
					markings[index] = 0;
					redraw = true;
				}
			} else if (Input.trigger(Input.UP)) {
				if (index == 7) {
					index = 6;
				} else if (index == 6) {
					index = 4;
				} else if (index < 3) {
					index = 7;
				} else {
					index -= 3;
				}
				PlayCursorSE;
			} else if (Input.trigger(Input.DOWN)) {
				if (index == 7) {
					index = 1;
				} else if (index == 6) {
					index = 7;
				} else if (index >= 3) {
					index = 6;
				} else {
					index += 3;
				}
				PlayCursorSE;
			} else if (Input.trigger(Input.LEFT)) {
				if (index < 6) {
					index -= 1;
					if (index % 3 == 2) index += 3;
					PlayCursorSE;
				}
			} else if (Input.trigger(Input.RIGHT)) {
				if (index < 6) {
					index += 1;
					if (index % 3 == 0) index -= 3;
					PlayCursorSE;
				}
			}
		}
		@sprites["markingbg"].visible      = false;
		@sprites["markingoverlay"].visible = false;
		@sprites["markingsel"].visible     = false;
		if (pokemon.markings != ret) {
			pokemon.markings = ret;
			return true;
		}
		return false;
	}

	public void Options() {
		dorefresh = false;
		commands = new List<string>();
		cmdGiveItem = -1;
		cmdTakeItem = -1;
		cmdPokedex  = -1;
		cmdMark     = -1;
		if (!@pokemon.egg()) {
			commands[cmdGiveItem = commands.length] = _INTL("Give item");
			if (@pokemon.hasItem()) commands[cmdTakeItem = commands.length] = _INTL("Take item");
			if (Game.GameData.player.has_pokedex) commands[cmdPokedex = commands.length]  = _INTL("View Pokédex");
		}
		commands[cmdMark = commands.length]       = _INTL("Mark");
		commands[commands.length]                 = _INTL("Cancel");
		command = ShowCommands(commands);
		if (cmdGiveItem >= 0 && command == cmdGiveItem) {
			item = null;
			FadeOutIn do;
				scene = new PokemonBag_Scene();
				screen = new PokemonBagScreen(scene, Game.GameData.bag);
				item = screen.ChooseItemScreen(block: (itm) => { GameData.Item.get(itm).can_hold() });
			}
			if (item) dorefresh = GiveItemToPokemon(item, @pokemon, self, @partyindex);
		} else if (cmdTakeItem >= 0 && command == cmdTakeItem) {
			dorefresh = TakeItemFromPokemon(@pokemon, self);
		} else if (cmdPokedex >= 0 && command == cmdPokedex) {
			Game.GameData.player.pokedex.register_last_seen(@pokemon);
			FadeOutIn do;
				scene = new PokemonPokedexInfo_Scene();
				screen = new PokemonPokedexInfoScreen(scene);
				screen.StartSceneSingle(@pokemon.species);
			}
			dorefresh = true;
		} else if (cmdMark >= 0 && command == cmdMark) {
			dorefresh = Marking(@pokemon);
		}
		return dorefresh;
	}

	public void ChooseMoveToForget(move_to_learn) {
		new_move = (move_to_learn) ? new Pokemon.Move(move_to_learn) : null;
		selmove = 0;
		maxmove = (new_move) ? Pokemon.MAX_MOVES : Pokemon.MAX_MOVES - 1;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (Input.trigger(Input.BACK)) {
				selmove = Pokemon.MAX_MOVES;
				if (new_move) PlayCloseMenuSE;
				break;
			} else if (Input.trigger(Input.USE)) {
				PlayDecisionSE;
				break;
			} else if (Input.trigger(Input.UP)) {
				selmove -= 1;
				if (selmove < 0) selmove = maxmove;
				if (selmove < Pokemon.MAX_MOVES && selmove >= @pokemon.numMoves) {
					selmove = @pokemon.numMoves - 1;
				}
				@sprites["movesel"].index = selmove;
				selected_move = (selmove == Pokemon.MAX_MOVES) ? new_move : @pokemon.moves[selmove];
				drawSelectedMove(new_move, selected_move);
			} else if (Input.trigger(Input.DOWN)) {
				selmove += 1;
				if (selmove > maxmove) selmove = 0;
				if (selmove < Pokemon.MAX_MOVES && selmove >= @pokemon.numMoves) {
					selmove = (new_move) ? maxmove : 0;
				}
				@sprites["movesel"].index = selmove;
				selected_move = (selmove == Pokemon.MAX_MOVES) ? new_move : @pokemon.moves[selmove];
				drawSelectedMove(new_move, selected_move);
			}
		}
		return (selmove == Pokemon.MAX_MOVES) ? -1 : selmove;
	}

	public void Scene() {
		@pokemon.play_cry;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			dorefresh = false;
			if (Input.trigger(Input.ACTION)) {
				SEStop;
				@pokemon.play_cry;
			} else if (Input.trigger(Input.BACK)) {
				PlayCloseMenuSE;
				break;
			} else if (Input.trigger(Input.USE)) {
				if (@page == 4) {
					PlayDecisionSE;
					MoveSelection;
					dorefresh = true;
				} else if (@page == 5) {
					PlayDecisionSE;
					RibbonSelection;
					dorefresh = true;
				} else if (!@inbattle) {
					PlayDecisionSE;
					dorefresh = Options;
				}
			} else if (Input.trigger(Input.UP) && @partyindex > 0) {
				oldindex = @partyindex;
				GoToPrevious;
				if (@partyindex != oldindex) {
					ChangePokemon;
					@ribbonOffset = 0;
					dorefresh = true;
				}
			} else if (Input.trigger(Input.DOWN) && @partyindex < @party.length - 1) {
				oldindex = @partyindex;
				GoToNext;
				if (@partyindex != oldindex) {
					ChangePokemon;
					@ribbonOffset = 0;
					dorefresh = true;
				}
			} else if (Input.trigger(Input.LEFT) && !@pokemon.egg()) {
				oldpage = @page;
				@page -= 1;
				if (@page < 1) @page = 1;
				if (@page > 5) @page = 5;
				if (@page != oldpage) {   // Move to next page
					SEPlay("GUI summary change page");
					@ribbonOffset = 0;
					dorefresh = true;
				}
			} else if (Input.trigger(Input.RIGHT) && !@pokemon.egg()) {
				oldpage = @page;
				@page += 1;
				if (@page < 1) @page = 1;
				if (@page > 5) @page = 5;
				if (@page != oldpage) {   // Move to next page
					SEPlay("GUI summary change page");
					@ribbonOffset = 0;
					dorefresh = true;
				}
			}
			if (dorefresh) drawPage(@page);
		}
		return @partyindex;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonSummaryScreen {
	public void initialize(scene, inbattle = false) {
		@scene = scene;
		@inbattle = inbattle;
	}

	public void StartScreen(party, partyindex) {
		@scene.StartScene(party, partyindex, @inbattle);
		ret = @scene.Scene;
		@scene.EndScene;
		return ret;
	}

	public void StartForgetScreen(party, partyindex, move_to_learn) {
		ret = -1;
		@scene.StartForgetScene(party, partyindex, move_to_learn);
		do { //loop; while (true);
			ret = @scene.ChooseMoveToForget(move_to_learn);
			if (ret < 0 || !move_to_learn) break;
			if (Core.DEBUG || !party[partyindex].moves[ret].hidden_move()) break;
			Message(_INTL("HM moves can't be forgotten now.")) { @scene.Update };
		}
		@scene.EndScene;
		return ret;
	}

	public void StartChooseMoveScreen(party, partyindex, message) {
		ret = -1;
		@scene.StartForgetScene(party, partyindex, null);
		Message(message) { @scene.Update };
		do { //loop; while (true);
			ret = @scene.ChooseMoveToForget(null);
			if (ret >= 0) break;
			Message(_INTL("You must choose a move!")) { @scene.Update };
		}
		@scene.EndScene;
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public void ChooseMove(pokemon, variableNumber, nameVarNumber) {
	if (!pokemon) return;
	ret = -1;
	FadeOutIn do;
		scene = new PokemonSummary_Scene();
		screen = new PokemonSummaryScreen(scene);
		ret = screen.StartForgetScreen([pokemon], 0, null);
	}
	Game.GameData.game_variables[variableNumber] = ret;
	if (ret >= 0) {
		Game.GameData.game_variables[nameVarNumber] = pokemon.moves[ret].name;
	} else {
		Game.GameData.game_variables[nameVarNumber] = "";
	}
	if (Game.GameData.game_map) Game.GameData.game_map.need_refresh = true;
}
