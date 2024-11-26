//===============================================================================
// Pokémon party buttons and menu.
//===============================================================================
public partial class PokemonPartyConfirmCancelSprite : Sprite {
	public int selected		{ get { return _selected; } }			protected int _selected;

	public override void initialize(text, x, y, narrowbox = false, viewport = null) {
		base.initialize(viewport);
		@refreshBitmap = true;
		@bgsprite = new ChangelingSprite(0, 0, viewport);
		if (narrowbox) {
			@bgsprite.addBitmap("desel", "Graphics/UI/Party/icon_cancel_narrow");
			@bgsprite.addBitmap("sel", "Graphics/UI/Party/icon_cancel_narrow_sel");
		} else {
			@bgsprite.addBitmap("desel", "Graphics/UI/Party/icon_cancel");
			@bgsprite.addBitmap("sel", "Graphics/UI/Party/icon_cancel_sel");
		}
		@bgsprite.changeBitmap("desel");
		@overlaysprite = new BitmapSprite(@bgsprite.bitmap.width, @bgsprite.bitmap.height, viewport);
		@overlaysprite.z = self.z + 1;
		SetSystemFont(@overlaysprite.bitmap);
		textpos = new {text, 56, (narrowbox) ? 8 : 14, :center, new Color(248, 248, 248), new Color(40, 40, 40)};
		DrawTextPositions(@overlaysprite.bitmap, textpos);
		self.x = x;
		self.y = y;
	}

	public override void dispose() {
		@bgsprite.dispose;
		@overlaysprite.bitmap.dispose;
		@overlaysprite.dispose;
		base.dispose();
	}

	public override int viewport { set {
		base.viewport();
		refresh;
		}
	}

	public override int x { set {
		base.x();
		refresh;
		}
	}

	public override int y { set {
		base.y();
		refresh;
		}
	}

	public override int color { set {
		base.color();
		refresh;
		}
	}

	public int selected { set {
		if (@selected != value) {
			@selected = value;
			refresh;
			}
	}
	}

	public void refresh() {
		if (@bgsprite && !@bgsprite.disposed()) {
			@bgsprite.changeBitmap((@selected) ? "sel" : "desel");
			@bgsprite.x     = self.x;
			@bgsprite.y     = self.y;
			@bgsprite.color = self.color;
		}
		if (@overlaysprite && !@overlaysprite.disposed()) {
			@overlaysprite.x     = self.x;
			@overlaysprite.y     = self.y;
			@overlaysprite.color = self.color;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPartyCancelSprite : PokemonPartyConfirmCancelSprite {
	public override void initialize(viewport = null) {
		base.initialize(_INTL("CANCEL"), 398, 328, false, viewport);
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPartyConfirmSprite : PokemonPartyConfirmCancelSprite {
	public override void initialize(viewport = null) {
		base.initialize(_INTL("CONFIRM"), 398, 308, true, viewport);
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPartyCancelSprite2 : PokemonPartyConfirmCancelSprite {
	public override void initialize(viewport = null) {
		base.initialize(_INTL("CANCEL"), 398, 346, true, viewport);
	}
}

//===============================================================================
//
//===============================================================================
public partial class Window_CommandPokemonColor : Window_CommandPokemon {
	public void initialize(commands, width = null) {
		@colorKey = new List<string>();
		for (int i = commands.length; i < commands.length; i++) { //for 'commands.length' times do => |i|
			if (commands[i].Length > 0) {
				@colorKey[i] = commands[i][1];
				commands[i] = commands[i][0];
			}
		}
		super(commands, width);
	}

	public void drawItem(index, _count, rect) {
		if (@starting) SetSystemFont(self.contents);
		rect = drawCursor(index, rect);
		base   = self.baseColor;
		shadow = self.shadowColor;
		if (@colorKey[index] && @colorKey[index] == 1) {
			base   = new Color(0, 80, 160);
			shadow = new Color(128, 192, 240);
		}
		DrawShadowText(self.contents, rect.x, rect.y + (self.contents.text_offset_y || 0),
										rect.width, rect.height, @commands[index], base, shadow);
	}
}

//===============================================================================
// Blank party panel.
//===============================================================================
public partial class PokemonPartyBlankPanel : Sprite {
	public int text		{ get { return _text; } set { _text = value; } }			protected int _text;

	public override void initialize(_pokemon, index, viewport = null) {
		base.initialize(viewport);
		self.x = (index % 2) * Graphics.width / 2;
		self.y = (16 * (index % 2)) + (96 * (index / 2));
		@panelbgsprite = new AnimatedBitmap("Graphics/UI/Party/panel_blank");
		self.bitmap = @panelbgsprite.bitmap;
		@text = null;
	}

	public override void dispose() {
		@panelbgsprite.dispose;
		base.dispose();
	}

	public int selected { get { return false; } }
	public int selected { set {  } }
	public int preselected { get { return false; } }
	public int preselected { set {  } }
	public int switching { get { return false; } }
	public int switching { set {  } }
	public void refresh() { }
}

//===============================================================================
// Pokémon party panel.
//===============================================================================
public partial class PokemonPartyPanel : Sprite {
	public int pokemon		{ get { return _pokemon; } }			protected int _pokemon;
	public int active		{ get { return _active; } }			protected int _active;
	public int selected		{ get { return _selected; } }			protected int _selected;
	public int preselected		{ get { return _preselected; } }			protected int _preselected;
	public int switching		{ get { return _switching; } }			protected int _switching;
	public int text		{ get { return _text; } }			protected int _text;

	TEXT_BASE_COLOR    = new Color(248, 248, 248);
	TEXT_SHADOW_COLOR  = new Color(40, 40, 40);
	public const int HP_BAR_WIDTH       = 96;

	public override void initialize(pokemon, index, viewport = null) {
		base.initialize(viewport);
		@pokemon = pokemon;
		@active = (index == 0);   // true = rounded panel, false = rectangular panel
		@refreshing = true;
		self.x = (index % 2) * Graphics.width / 2;
		self.y = (16 * (index % 2)) + (96 * (index / 2));
		@panelbgsprite = new ChangelingSprite(0, 0, viewport);
		@panelbgsprite.z = self.z;
		if (@active) {   // Rounded panel
			@panelbgsprite.addBitmap("able", "Graphics/UI/Party/panel_round");
			@panelbgsprite.addBitmap("ablesel", "Graphics/UI/Party/panel_round_sel");
			@panelbgsprite.addBitmap("fainted", "Graphics/UI/Party/panel_round_faint");
			@panelbgsprite.addBitmap("faintedsel", "Graphics/UI/Party/panel_round_faint_sel");
			@panelbgsprite.addBitmap("swap", "Graphics/UI/Party/panel_round_swap");
			@panelbgsprite.addBitmap("swapsel", "Graphics/UI/Party/panel_round_swap_sel");
			@panelbgsprite.addBitmap("swapsel2", "Graphics/UI/Party/panel_round_swap_sel2");
		} else {   // Rectangular panel
			@panelbgsprite.addBitmap("able", "Graphics/UI/Party/panel_rect");
			@panelbgsprite.addBitmap("ablesel", "Graphics/UI/Party/panel_rect_sel");
			@panelbgsprite.addBitmap("fainted", "Graphics/UI/Party/panel_rect_faint");
			@panelbgsprite.addBitmap("faintedsel", "Graphics/UI/Party/panel_rect_faint_sel");
			@panelbgsprite.addBitmap("swap", "Graphics/UI/Party/panel_rect_swap");
			@panelbgsprite.addBitmap("swapsel", "Graphics/UI/Party/panel_rect_swap_sel");
			@panelbgsprite.addBitmap("swapsel2", "Graphics/UI/Party/panel_rect_swap_sel2");
		}
		@hpbgsprite = new ChangelingSprite(0, 0, viewport);
		@hpbgsprite.z = self.z + 1;
		@hpbgsprite.addBitmap("able", _INTL("Graphics/UI/Party/overlay_hp_back"));
		@hpbgsprite.addBitmap("fainted", _INTL("Graphics/UI/Party/overlay_hp_back_faint"));
		@hpbgsprite.addBitmap("swap", _INTL("Graphics/UI/Party/overlay_hp_back_swap"));
		@ballsprite = new ChangelingSprite(0, 0, viewport);
		@ballsprite.z = self.z + 1;
		@ballsprite.addBitmap("desel", "Graphics/UI/Party/icon_ball");
		@ballsprite.addBitmap("sel", "Graphics/UI/Party/icon_ball_sel");
		@pkmnsprite = new PokemonIconSprite(pokemon, viewport);
		@pkmnsprite.setOffset(PictureOrigin.CENTER);
		@pkmnsprite.active = @active;
		@pkmnsprite.z      = self.z + 2;
		@helditemsprite = new HeldItemIconSprite(0, 0, @pokemon, viewport);
		@helditemsprite.z = self.z + 3;
		@overlaysprite = new BitmapSprite(Graphics.width, Graphics.height, viewport);
		@overlaysprite.z = self.z + 4;
		SetSystemFont(@overlaysprite.bitmap);
		@hpbar    = new AnimatedBitmap("Graphics/UI/Party/overlay_hp");
		@statuses = new AnimatedBitmap(_INTL("Graphics/UI/statuses"));
		@selected      = false;
		@preselected   = false;
		@switching     = false;
		@text          = null;
		@refreshBitmap = true;
		@refreshing    = false;
		refresh;
	}

	public override void dispose() {
		@panelbgsprite.dispose;
		@hpbgsprite.dispose;
		@ballsprite.dispose;
		@pkmnsprite.dispose;
		@helditemsprite.dispose;
		@overlaysprite.bitmap.dispose;
		@overlaysprite.dispose;
		@hpbar.dispose;
		@statuses.dispose;
		base.dispose();
	}

	public override int x { set {
		base.x();
		refresh;
		}
	}

	public override int y { set {
		base.y();
		refresh;
		}
	}

	public override int color { set {
		base.color();
		refresh;
		}
	}

	public int text { set {
		if (@text == value) return;
		@text = value;
		@refreshBitmap = true;
		refresh;
		}
	}

	public int pokemon { set {
		@pokemon = value;
		if (@pkmnsprite && !@pkmnsprite.disposed()) @pkmnsprite.pokemon = value;
		if (@helditemsprite && !@helditemsprite.disposed()) @helditemsprite.pokemon = value;
		@refreshBitmap = true;
		refresh;
		}
	}

	public int selected { set {
		if (@selected == value) return;
		@selected = value;
		refresh;
		}
	}

	public int preselected { set {
		if (@preselected == value) return;
		@preselected = value;
		refresh;
		}
	}

	public int switching { set {
		if (@switching == value) return;
		@switching = value;
		refresh;
		}
	}

	public int hp { get { return @pokemon.hp; } }

	public void refresh_panel_graphic() {
		if (!@panelbgsprite || @panelbgsprite.disposed()) return;
		if (self.selected) {
			if (self.preselected) {
				@panelbgsprite.changeBitmap("swapsel2");
			} else if (@switching) {
				@panelbgsprite.changeBitmap("swapsel");
			} else if (@pokemon.fainted()) {
				@panelbgsprite.changeBitmap("faintedsel");
			} else {
				@panelbgsprite.changeBitmap("ablesel");
			}
		} else {
			if (self.preselected) {
				@panelbgsprite.changeBitmap("swap");
			} else if (@pokemon.fainted()) {
				@panelbgsprite.changeBitmap("fainted");
			} else {
				@panelbgsprite.changeBitmap("able");
			}
		}
		@panelbgsprite.x     = self.x;
		@panelbgsprite.y     = self.y;
		@panelbgsprite.color = self.color;
	}

	public void refresh_hp_bar_graphic() {
		if (!@hpbgsprite || @hpbgsprite.disposed()) return;
		@hpbgsprite.visible = (!@pokemon.egg() && !(@text && @text.length > 0));
		if (!@hpbgsprite.visible) return;
		if (self.preselected || (self.selected && @switching)) {
			@hpbgsprite.changeBitmap("swap");
		} else if (@pokemon.fainted()) {
			@hpbgsprite.changeBitmap("fainted");
		} else {
			@hpbgsprite.changeBitmap("able");
		}
		@hpbgsprite.x     = self.x + 96;
		@hpbgsprite.y     = self.y + 50;
		@hpbgsprite.color = self.color;
	}

	public void refresh_ball_graphic() {
		if (!@ballsprite || @ballsprite.disposed()) return;
		@ballsprite.changeBitmap((self.selected) ? "sel" : "desel");
		@ballsprite.x     = self.x + 10;
		@ballsprite.y     = self.y;
		@ballsprite.color = self.color;
	}

	public void refresh_pokemon_icon() {
		if (!@pkmnsprite || @pkmnsprite.disposed()) return;
		@pkmnsprite.x        = self.x + 60;
		@pkmnsprite.y        = self.y + 40;
		@pkmnsprite.color    = self.color;
		@pkmnsprite.selected = self.selected;
	}

	public void refresh_held_item_icon() {
		if (!@helditemsprite || @helditemsprite.disposed() || !@helditemsprite.visible) return;
		@helditemsprite.x     = self.x + 62;
		@helditemsprite.y     = self.y + 48;
		@helditemsprite.color = self.color;
	}

	public void refresh_overlay_information() {
		if (!@refreshBitmap) return;
		@overlaysprite.bitmap&.clear;
		draw_name;
		draw_level;
		draw_gender;
		draw_hp;
		draw_status;
		draw_shiny_icon;
		draw_annotation;
	}

	public void draw_name() {
		DrawTextPositions(@overlaysprite.bitmap,
												new {@pokemon.name, 96, 22, :left, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR});
	}

	public void draw_level() {
		if (@pokemon.egg()) return;
		// "Lv" graphic
		DrawImagePositions(@overlaysprite.bitmap,
												new {_INTL("Graphics/UI/Party/overlay_lv"), 20, 70, 0, 0, 22, 14});
		// Level number
		SetSmallFont(@overlaysprite.bitmap);
		DrawTextPositions(@overlaysprite.bitmap,
												new {@pokemon.level.ToString(), 42, 68, :left, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR});
		SetSystemFont(@overlaysprite.bitmap);
	}

	public void draw_gender() {
		if (@pokemon.egg() || @pokemon.genderless()) return;
		gender_text  = (@pokemon.male()) ? _INTL("♂") : _INTL("♀");
		base_color   = (@pokemon.male()) ? new Color(0, 112, 248) : new Color(232, 32, 16);
		shadow_color = (@pokemon.male()) ? new Color(120, 184, 232) : new Color(248, 168, 184);
		DrawTextPositions(@overlaysprite.bitmap,
												new {gender_text, 224, 22, :left, base_color, shadow_color});
	}

	public void draw_hp() {
		if (@pokemon.egg() || (@text && @text.length > 0)) return;
		// HP numbers
		hp_text = string.Format("% 3d /% 3d", @pokemon.hp, @pokemon.totalhp);
		DrawTextPositions(@overlaysprite.bitmap,
												new {hp_text, 224, 66, :right, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR});
		// HP bar
		if (@pokemon.able()) {
			w = @pokemon.hp * HP_BAR_WIDTH / @pokemon.totalhp.to_f;
			if (w < 1) w = 1;
			w = ((int)Math.Round(w / 2)) * 2;   // Round to the nearest 2 pixels
			hpzone = 0;
			if (@pokemon.hp <= (int)Math.Floor(@pokemon.totalhp / 2)) hpzone = 1;
			if (@pokemon.hp <= (int)Math.Floor(@pokemon.totalhp / 4)) hpzone = 2;
			hprect = new Rect(0, hpzone * 8, w, 8);
			@overlaysprite.bitmap.blt(128, 52, @hpbar.bitmap, hprect);
		}
	}

	public void draw_status() {
		if (@pokemon.egg() || (@text && @text.length > 0)) return;
		status = -1;
		if (@pokemon.fainted()) {
			status = GameData.Status.count - 1;
		} else if (@pokemon.status != statuses.NONE) {
			status = GameData.Status.get(@pokemon.status).icon_position;
		} else if (@pokemon.pokerusStage == 1) {
			status = GameData.Status.count;
		}
		if (status < 0) return;
		statusrect = new Rect(0, status * GameData.Status.ICON_SIZE[1], *GameData.Status.ICON_SIZE);
		@overlaysprite.bitmap.blt(78, 68, @statuses.bitmap, statusrect);
	}

	public void draw_shiny_icon() {
		if (@pokemon.egg() || !@pokemon.shiny()) return;
		DrawImagePositions(@overlaysprite.bitmap,
												new {"Graphics/UI/shiny", 80, 48, 0, 0, 16, 16});
	}

	public void draw_annotation() {
		if (!@text || @text.length == 0) return;
		DrawTextPositions(@overlaysprite.bitmap,
												new {@text, 96, 62, :left, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR});
	}

	public void refresh() {
		if (disposed()) return;
		if (@refreshing) return;
		@refreshing = true;
		refresh_panel_graphic;
		refresh_hp_bar_graphic;
		refresh_ball_graphic;
		refresh_pokemon_icon;
		refresh_held_item_icon;
		if (@overlaysprite && !@overlaysprite.disposed()) {
			@overlaysprite.x     = self.x;
			@overlaysprite.y     = self.y;
			@overlaysprite.color = self.color;
		}
		refresh_overlay_information;
		@refreshBitmap = false;
		@refreshing = false;
	}

	public override void update() {
		base.update();
		if (@panelbgsprite && !@panelbgsprite.disposed()) @panelbgsprite.update;
		if (@hpbgsprite && !@hpbgsprite.disposed()) @hpbgsprite.update;
		if (@ballsprite && !@ballsprite.disposed()) @ballsprite.update;
		if (@pkmnsprite && !@pkmnsprite.disposed()) @pkmnsprite.update;
		if (@helditemsprite && !@helditemsprite.disposed()) @helditemsprite.update;
	}
}

//===============================================================================
// Pokémon party visuals.
//===============================================================================
public partial class PokemonParty_Scene {
	public void StartScene(party, starthelptext, annotations = null, multiselect = false, can_access_storage = false) {
		@sprites = new List<string>();
		@party = party;
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@multiselect = multiselect;
		@can_access_storage = can_access_storage;
		addBackgroundPlane(@sprites, "partybg", "Party/bg", @viewport);
		@sprites["messagebox"] = new Window_AdvancedTextPokemon("");
		@sprites["messagebox"].z              = 50;
		@sprites["messagebox"].viewport       = @viewport;
		@sprites["messagebox"].visible        = false;
		@sprites["messagebox"].letterbyletter = true;
		BottomLeftLines(@sprites["messagebox"], 2);
		@sprites["storagetext"] = new Window_UnformattedTextPokemon(
			@can_access_storage ? _INTL("[Special]: To Boxes") : ""
		);
		@sprites["storagetext"].x           = 32;
		@sprites["storagetext"].y           = Graphics.height - @sprites["messagebox"].height - 16;
		@sprites["storagetext"].z           = 10;
		@sprites["storagetext"].viewport    = @viewport;
		@sprites["storagetext"].baseColor   = new Color(248, 248, 248);
		@sprites["storagetext"].shadowColor = Color.black;
		@sprites["storagetext"].windowskin  = null;
		@sprites["helpwindow"] = new Window_UnformattedTextPokemon(starthelptext);
		@sprites["helpwindow"].viewport = @viewport;
		@sprites["helpwindow"].visible  = true;
		BottomLeftLines(@sprites["helpwindow"], 1);
		SetHelpText(starthelptext);
		// Add party Pokémon sprites
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			if (@party[i]) {
				@sprites[$"pokemon{i}"] = new PokemonPartyPanel(@party[i], i, @viewport);
			} else {
				@sprites[$"pokemon{i}"] = new PokemonPartyBlankPanel(@party[i], i, @viewport);
			}
			if (annotations) @sprites[$"pokemon{i}"].text = annotations[i];
		}
		if (@multiselect) {
			@sprites[$"pokemon{Settings.MAX_PARTY_SIZE}"] = new PokemonPartyConfirmSprite(@viewport);
			@sprites[$"pokemon{Settings.MAX_PARTY_SIZE + 1}"] = new PokemonPartyCancelSprite2(@viewport);
		} else {
			@sprites[$"pokemon{Settings.MAX_PARTY_SIZE}"] = new PokemonPartyCancelSprite(@viewport);
		}
		// Select first Pokémon
		@activecmd = 0;
		@sprites["pokemon0"].selected = true;
		FadeInAndShow(@sprites) { update };
	}

	public void EndScene() {
		FadeOutAndHide(@sprites) { update };
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void Display(text) {
		@sprites["messagebox"].text    = text;
		@sprites["messagebox"].visible = true;
		@sprites["helpwindow"].visible = false;
		PlayDecisionSE;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			self.update;
			if (@sprites["messagebox"].busy()) {
				if (Input.trigger(Input.USE)) {
					if (@sprites["messagebox"].pausing()) PlayDecisionSE;
					@sprites["messagebox"].resume;
				}
			} else if (Input.trigger(Input.BACK) || Input.trigger(Input.USE)) {
				break;
			}
		}
		@sprites["messagebox"].visible = false;
		@sprites["helpwindow"].visible = true;
	}

	public void DisplayConfirm(text) {
		ret = -1;
		@sprites["messagebox"].text    = text;
		@sprites["messagebox"].visible = true;
		@sprites["helpwindow"].visible = false;
		using(cmdwindow = new Window_CommandPokemon(new {_INTL("Yes"), _INTL("No")})) do;
			cmdwindow.visible = false;
			BottomRight(cmdwindow);
			cmdwindow.y -= @sprites["messagebox"].height;
			cmdwindow.z = @viewport.z + 1;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				if (!@sprites["messagebox"].busy()) cmdwindow.visible = true;
				cmdwindow.update;
				self.update;
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
		@sprites["helpwindow"].visible = true;
		return ret;
	}

	public void ShowCommands(helptext, commands, index = 0) {
		ret = -1;
		helpwindow = @sprites["helpwindow"];
		helpwindow.visible = true;
		using(cmdwindow = new Window_CommandPokemonColor(commands)) do;
			cmdwindow.z     = @viewport.z + 1;
			cmdwindow.index = index;
			BottomRight(cmdwindow);
			helpwindow.resizeHeightToFit(helptext, Graphics.width - cmdwindow.width);
			helpwindow.text = helptext;
			BottomLeft(helpwindow);
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				cmdwindow.update;
				self.update;
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

	public void ChooseNumber(helptext, maximum, initnum = 1) {
		return UIHelper.ChooseNumber(@sprites["helpwindow"], helptext, maximum, initnum) { update };
	}

	public void SetHelpText(helptext) {
		helpwindow = @sprites["helpwindow"];
		BottomLeftLines(helpwindow, 1);
		helpwindow.text = helptext;
		helpwindow.width = 398;
		helpwindow.visible = true;
	}

	public bool HasAnnotations() {
		return !@sprites["pokemon0"].text.null();
	}

	public void Annotate(annot) {
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			@sprites[$"pokemon{i}"].text = (annot) ? annot[i] : null;
		}
	}

	public void Select(item) {
		@activecmd = item;
		numsprites = Settings.MAX_PARTY_SIZE + ((@multiselect) ? 2 : 1);
		for (int i = numsprites; i < numsprites; i++) { //for 'numsprites' times do => |i|
			@sprites[$"pokemon{i}"].selected = (i == @activecmd);
		}
	}

	public void PreSelect(item) {
		@activecmd = item;
	}

	public void SwitchBegin(oldid, newid) {
		SEPlay("GUI party switch");
		oldsprite = @sprites[$"pokemon{oldid}"];
		newsprite = @sprites[$"pokemon{newid}"];
		old_start_x = oldsprite.x;
		new_start_x = newsprite.x;
		old_mult = oldid.even() ? -1 : 1;
		new_mult = newid.even() ? -1 : 1;
		timer_start = System.uptime;
		do { //loop; while (true);
			oldsprite.x = lerp(old_start_x, old_start_x + (old_mult * Graphics.width / 2), 0.4, timer_start, System.uptime);
			newsprite.x = lerp(new_start_x, new_start_x + (new_mult * Graphics.width / 2), 0.4, timer_start, System.uptime);
			Graphics.update;
			Input.update;
			self.update;
			if (oldsprite.x == old_start_x + (old_mult * Graphics.width / 2)) break;
		}
	}

	public void SwitchEnd(oldid, newid) {
		SEPlay("GUI party switch");
		oldsprite = @sprites[$"pokemon{oldid}"];
		newsprite = @sprites[$"pokemon{newid}"];
		oldsprite.pokemon = @party[oldid];
		newsprite.pokemon = @party[newid];
		old_start_x = oldsprite.x;
		new_start_x = newsprite.x;
		old_mult = oldid.even() ? -1 : 1;
		new_mult = newid.even() ? -1 : 1;
		timer_start = System.uptime;
		do { //loop; while (true);
			oldsprite.x = lerp(old_start_x, old_start_x - (old_mult * Graphics.width / 2), 0.4, timer_start, System.uptime);
			newsprite.x = lerp(new_start_x, new_start_x - (new_mult * Graphics.width / 2), 0.4, timer_start, System.uptime);
			Graphics.update;
			Input.update;
			self.update;
			if (oldsprite.x == old_start_x - (old_mult * Graphics.width / 2)) break;
		}
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			@sprites[$"pokemon{i}"].preselected = false;
			@sprites[$"pokemon{i}"].switching   = false;
		}
		Refresh;
	}

	public void ClearSwitching() {
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			@sprites[$"pokemon{i}"].preselected = false;
			@sprites[$"pokemon{i}"].switching   = false;
		}
	}

	public void Summary(pkmnid, inbattle = false) {
		oldsprites = FadeOutAndHide(@sprites);
		scene = new PokemonSummary_Scene();
		screen = new PokemonSummaryScreen(scene, inbattle);
		screen.StartScreen(@party, pkmnid);
		if (block_given()) yield;
		Refresh;
		FadeInAndShow(@sprites, oldsprites);
	}

	public void ChooseItem(bag) {
		ret = null;
		FadeOutIn do;
			scene = new PokemonBag_Scene();
			screen = new PokemonBagScreen(scene, bag);
			ret = screen.ChooseItemScreen(block: (item) => { GameData.Item.get(item).can_hold() });
			if (block_given()) yield;
		}
		return ret;
	}

	public void UseItem(bag, pokemon) {
		ret = null;
		FadeOutIn do;
			scene = new PokemonBag_Scene();
			screen = new PokemonBagScreen(scene, bag);
			ret = screen.ChooseItemScreen(block: (item) => {
				itm = GameData.Item.get(item);
				if (!CanUseOnPokemon(itm)) next false;
				if (pokemon.hyper_mode && !GameData.Item.get(item)&.is_scent()) next false;
				if (itm.is_machine()) {
					move = itm.move;
					if (pokemon.hasMove(move) || !pokemon.compatible_with_move(move)) next false;
				}
				next true;
			});
			if (block_given()) yield;
		}
		return ret;
	}

	public void ChoosePokemon(switching = false, initialsel = -1, canswitch = 0) {
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			@sprites[$"pokemon{i}"].preselected = (switching && i == @activecmd);
			@sprites[$"pokemon{i}"].switching   = switching;
		}
		if (initialsel >= 0) @activecmd = initialsel;
		Refresh;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			self.update;
			oldsel = @activecmd;
			key = -1;
			if (Input.repeat(Input.DOWN)) key = Input.DOWN;
			if (Input.repeat(Input.RIGHT)) key = Input.RIGHT;
			if (Input.repeat(Input.LEFT)) key = Input.LEFT;
			if (Input.repeat(Input.UP)) key = Input.UP;
			if (key >= 0) {
				@activecmd = ChangeSelection(key, @activecmd);
			}
			if (@activecmd != oldsel) {   // Changing selection
				PlayCursorSE;
				numsprites = Settings.MAX_PARTY_SIZE + ((@multiselect) ? 2 : 1);
				for (int i = numsprites; i < numsprites; i++) { //for 'numsprites' times do => |i|
					@sprites[$"pokemon{i}"].selected = (i == @activecmd);
				}
			}
			cancelsprite = Settings.MAX_PARTY_SIZE + ((@multiselect) ? 1 : 0);
			if (Input.trigger(Input.SPECIAL) && @can_access_storage && canswitch != 2) {
				PlayDecisionSE;
				FadeOutIn do;
					scene = new PokemonStorageScene();
					screen = new PokemonStorageScreen(scene, Game.GameData.PokemonStorage);
					screen.StartScreen(0);
					HardRefresh;
				}
			} else if (Input.trigger(Input.ACTION) && canswitch == 1 && @activecmd != cancelsprite) {
				PlayDecisionSE;
				return new {1, @activecmd};
			} else if (Input.trigger(Input.ACTION) && canswitch == 2) {
				return -1;
			} else if (Input.trigger(Input.BACK)) {
				if (!switching) PlayCloseMenuSE;
				return -1;
			} else if (Input.trigger(Input.USE)) {
				if (@activecmd == cancelsprite) {
					(switching) ? PlayDecisionSE : PlayCloseMenuSE
					return -1;
				} else {
					PlayDecisionSE;
					return @activecmd;
				}
			}
		}
	}

	public void ChangeSelection(key, currentsel) {
		numsprites = Settings.MAX_PARTY_SIZE + ((@multiselect) ? 2 : 1);
		switch (key) {
			case Input.LEFT:
				do { //loop; while (true);
					currentsel -= 1;
					unless (currentsel > 0 && currentsel < Settings.MAX_PARTY_SIZE && !@party[currentsel]) break;
				}
				if (currentsel >= @party.length && currentsel < Settings.MAX_PARTY_SIZE) {
					currentsel = @party.length - 1;
				}
				if (currentsel < 0) currentsel = numsprites - 1;
				break;
			case Input.RIGHT:
				do { //loop; while (true);
					currentsel += 1;
					unless (currentsel < Settings.MAX_PARTY_SIZE && !@party[currentsel]) break;
				}
				if (currentsel == numsprites) {
					currentsel = (@party.length == 0) ? Settings.MAX_PARTY_SIZE : 0;
				}
				break;
			case Input.UP:
				if (currentsel >= Settings.MAX_PARTY_SIZE) {
					currentsel -= 1;
					while (currentsel > 0 && currentsel < Settings.MAX_PARTY_SIZE && !@party[currentsel]) {
						currentsel -= 1;
					}
					if (currentsel < Settings.MAX_PARTY_SIZE && currentsel >= @party.length) currentsel = numsprites - 1;
				} else {
					do { //loop; while (true);
						currentsel -= 2;
						unless (currentsel > 0 && !@party[currentsel]) break;
					}
				}
				if (currentsel >= @party.length && currentsel < Settings.MAX_PARTY_SIZE) {
					currentsel = @party.length - 1;
				}
				if (currentsel < 0) currentsel = numsprites - 1;
				break;
			case Input.DOWN:
				if (currentsel >= Settings.MAX_PARTY_SIZE - 1) {
					currentsel += 1;
				} else {
					currentsel += 2;
					if (currentsel < Settings.MAX_PARTY_SIZE && !@party[currentsel]) currentsel = Settings.MAX_PARTY_SIZE;
				}
				if (currentsel >= @party.length && currentsel < Settings.MAX_PARTY_SIZE) {
					currentsel = Settings.MAX_PARTY_SIZE;
				} else if (currentsel >= numsprites) {
					currentsel = (@party.length == 0) ? Settings.MAX_PARTY_SIZE : 0;
				}
				break;
		}
		return currentsel;
	}

	public void HardRefresh() {
		oldtext = new List<string>();
		lastselected = -1;
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			oldtext.Add(@sprites[$"pokemon{i}"].text);
			if (@sprites[$"pokemon{i}"].selected) lastselected = i;
			@sprites[$"pokemon{i}"].dispose;
		}
		if (lastselected >= @party.length) lastselected = @party.length - 1;
		if (lastselected < 0) lastselected = Settings.MAX_PARTY_SIZE;
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			if (@party[i]) {
				@sprites[$"pokemon{i}"] = new PokemonPartyPanel(@party[i], i, @viewport);
			} else {
				@sprites[$"pokemon{i}"] = new PokemonPartyBlankPanel(@party[i], i, @viewport);
			}
			@sprites[$"pokemon{i}"].text = oldtext[i];
		}
		Select(lastselected);
	}

	public void Refresh() {
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			sprite = @sprites[$"pokemon{i}"];
			if (sprite) {
				if (sprite.is_a(PokemonPartyPanel)) {
					sprite.pokemon = sprite.pokemon;
				} else {
					sprite.refresh;
				}
			}
		}
	}

	public void RefreshSingle(i) {
		sprite = @sprites[$"pokemon{i}"];
		if (sprite) {
			if (sprite.is_a(PokemonPartyPanel)) {
				sprite.pokemon = sprite.pokemon;
			} else {
				sprite.refresh;
			}
		}
	}

	public void update() {
		UpdateSpriteHash(@sprites);
	}
}

//===============================================================================
// Pokémon party mechanics.
//===============================================================================
public partial class PokemonPartyScreen {
	public int scene		{ get { return _scene; } }			protected int _scene;
	public int party		{ get { return _party; } }			protected int _party;

	public void initialize(scene, party) {
		@scene = scene;
		@party = party;
	}

	public void StartScene(helptext, _numBattlersOut, annotations = null) {
		@scene.StartScene(@party, helptext, annotations);
	}

	public void ChoosePokemon(helptext = null) {
		if (helptext) @scene.SetHelpText(helptext);
		return @scene.ChoosePokemon;
	}

	public void PokemonGiveScreen(item) {
		@scene.StartScene(@party, _INTL("Give to which Pokémon?"));
		pkmnid = @scene.ChoosePokemon;
		ret = false;
		if (pkmnid >= 0) {
			ret = GiveItemToPokemon(item, @party[pkmnid], self, pkmnid);
		}
		RefreshSingle(pkmnid);
		@scene.EndScene;
		return ret;
	}

	public void PokemonGiveMailScreen(mailIndex) {
		@scene.StartScene(@party, _INTL("Give to which Pokémon?"));
		pkmnid = @scene.ChoosePokemon;
		if (pkmnid >= 0) {
			pkmn = @party[pkmnid];
			if (pkmn.hasItem() || pkmn.mail) {
				Display(_INTL("This Pokémon is holding an item. It can't hold mail."));
			} else if (pkmn.egg()) {
				Display(_INTL("Eggs can't hold mail."));
			} else {
				Display(_INTL("Mail was transferred from the Mailbox."));
				pkmn.mail = Game.GameData.PokemonGlobal.mailbox[mailIndex];
				pkmn.item = pkmn.mail.item;
				Game.GameData.PokemonGlobal.mailbox.delete_at(mailIndex);
				RefreshSingle(pkmnid);
			}
		}
		@scene.EndScene;
	}

	public void EndScene() {
		@scene.EndScene;
	}

	public void Update() {
		@scene.update;
	}

	public void HardRefresh() {
		@scene.HardRefresh;
	}

	public void Refresh() {
		@scene.Refresh;
	}

	public void RefreshSingle(i) {
		@scene.RefreshSingle(i);
	}

	public void Display(text) {
		@scene.Display(text);
	}

	public void Confirm(text) {
		return @scene.DisplayConfirm(text);
	}

	public void ShowCommands(helptext, commands, index = 0) {
		return @scene.ShowCommands(helptext, commands, index);
	}

	// Checks for identical species.
	// Unused.
	public void CheckSpecies(array) {
		for (int i = array.length; i < array.length; i++) { //for 'array.length' times do => |i|
			for (int j = i + 1; j < array.length; j++) { //each 'array.length' do => |j|
				if (array[i].species == array[j].species) return false;
			}
		}
		return true;
	}

	// Checks for identical held items.
	// Unused.
	public void CheckItems(array) {
		for (int i = array.length; i < array.length; i++) { //for 'array.length' times do => |i|
			if (!array[i].hasItem()) continue;
			for (int j = i + 1; j < array.length; j++) { //each 'array.length' do => |j|
				if (array[i].item == array[j].item) return false;
			}
		}
		return true;
	}

	public void Switch(oldid, newid) {
		if (oldid != newid) {
			@scene.SwitchBegin(oldid, newid);
			tmp = @party[oldid];
			@party[oldid] = @party[newid];
			@party[newid] = tmp;
			@scene.SwitchEnd(oldid, newid);
		}
	}

	public void ChooseMove(pokemon, helptext, index = 0) {
		movenames = new List<string>();
		foreach (var i in pokemon.moves) { //'pokemon.moves.each' do => |i|
			if (!i || !i.id) continue;
			if (i.total_pp <= 0) {
				movenames.Add(_INTL("{1} (PP: ---)", i.name));
			} else {
				movenames.Add(_INTL("{1} (PP: {2}/{3})", i.name, i.pp, i.total_pp));
			}
		}
		return @scene.ShowCommands(helptext, movenames, index);
	}

	// For after using an evolution stone.
	public void RefreshAnnotations(ableProc) {
		if (!@scene.HasAnnotations()) return;
		annot = new List<string>();
		@party.each do |pkmn|
			elig = ableProc.call(pkmn);
			annot.Add((elig) ? _INTL("ABLE") : _INTL("NOT ABLE"));
		}
		@scene.Annotate(annot);
	}

	public void ClearAnnotations() {
		@scene.Annotate(null);
	}

	public void PokemonMultipleEntryScreenEx(ruleset) {
		annot = new List<string>();
		statuses = new List<string>();
		ordinals = new {_INTL("INELIGIBLE"), _INTL("NOT ENTERED"), _INTL("BANNED")};
		positions = new {_INTL("FIRST"), _INTL("SECOND"), _INTL("THIRD"), _INTL("FOURTH"),
								_INTL("FIFTH"), _INTL("SIXTH"), _INTL("SEVENTH"), _INTL("EIGHTH"),
								_INTL("NINTH"), _INTL("TENTH"), _INTL("ELEVENTH"), _INTL("TWELFTH")};
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			if (i < positions.length) {
				ordinals.Add(positions[i]);
			} else {
				ordinals.Add($"{i + 1}th");
			}
		}
		if (!ruleset.hasValidTeam(@party)) return null;
		ret = null;
		addedEntry = false;
		for (int i = @party.length; i < @party.length; i++) { //for '@party.length' times do => |i|
			statuses[i] = (ruleset.isPokemonValid(@party[i])) ? 1 : 2;
			annot[i] = ordinals[statuses[i]];
		}
		@scene.StartScene(@party, _INTL("Choose Pokémon and confirm."), annot, true);
		do { //loop; while (true);
			realorder = new List<string>();
			for (int i = @party.length; i < @party.length; i++) { //for '@party.length' times do => |i|
				for (int j = @party.length; j < @party.length; j++) { //for '@party.length' times do => |j|
					if (statuses[j] == i + 3) {
						realorder.Add(j);
						break;
					}
				}
			}
			for (int i = realorder.length; i < realorder.length; i++) { //for 'realorder.length' times do => |i|
				statuses[realorder[i]] = i + 3;
			}
			for (int i = @party.length; i < @party.length; i++) { //for '@party.length' times do => |i|
				annot[i] = ordinals[statuses[i]];
			}
			@scene.Annotate(annot);
			if (realorder.length == ruleset.number && addedEntry) {
				@scene.Select(Settings.MAX_PARTY_SIZE);
			}
			@scene.SetHelpText(_INTL("Choose Pokémon and confirm."));
			pkmnid = @scene.ChoosePokemon;
			addedEntry = false;
			if (pkmnid == Settings.MAX_PARTY_SIZE) {   // Confirm was chosen
				ret = new List<string>();
				foreach (var i in realorder) { //'realorder.each' do => |i|
					ret.Add(@party[i]);
				}
				error = new List<string>();
				if (ruleset.isValid(ret, error)) break;
				Display(error[0]);
				ret = null;
			}
			if (pkmnid < 0) break;   // Cancelled
			cmdEntry   = -1;
			cmdNoEntry = -1;
			cmdSummary = -1;
			commands = new List<string>();
			if ((statuses[pkmnid] || 0) == 1) {
				commands[cmdEntry = commands.length]   = _INTL("Entry");
			} else if ((statuses[pkmnid] || 0) > 2) {
				commands[cmdNoEntry = commands.length] = _INTL("No Entry");
			}
			pkmn = @party[pkmnid];
			commands[cmdSummary = commands.length]   = _INTL("Summary");
			commands[commands.length]                = _INTL("Cancel");
			if (pkmn) command = @scene.ShowCommands(_INTL("Do what with {1}?", pkmn.name), commands);
			if (cmdEntry >= 0 && command == cmdEntry) {
				if (realorder.length >= ruleset.number && ruleset.number > 0) {
					Display(_INTL("No more than {1} Pokémon may enter.", ruleset.number));
				} else {
					statuses[pkmnid] = realorder.length + 3;
					addedEntry = true;
					RefreshSingle(pkmnid);
				}
			} else if (cmdNoEntry >= 0 && command == cmdNoEntry) {
				statuses[pkmnid] = 1;
				RefreshSingle(pkmnid);
			} else if (cmdSummary >= 0 && command == cmdSummary) {
				@scene.Summary(pkmnid) do;
					@scene.SetHelpText((@party.length > 1) ? _INTL("Choose a Pokémon.") : _INTL("Choose Pokémon or cancel."));
				}
			}
		}
		@scene.EndScene;
		return ret;
	}

	public void ChooseAblePokemon(ableProc, allowIneligible = false) {
		annot = new List<string>();
		eligibility = new List<string>();
		@party.each do |pkmn|
			elig = ableProc.call(pkmn);
			eligibility.Add(elig);
			annot.Add((elig) ? _INTL("ABLE") : _INTL("NOT ABLE"));
		}
		ret = -1;
		@scene.StartScene(
			@party,
			(@party.length > 1) ? _INTL("Choose a Pokémon.") : _INTL("Choose Pokémon or cancel."),
			annot
		);
		do { //loop; while (true);
			@scene.SetHelpText(
				(@party.length > 1) ? _INTL("Choose a Pokémon.") : _INTL("Choose Pokémon or cancel.")
			);
			pkmnid = @scene.ChoosePokemon;
			if (pkmnid < 0) break;
			if (!eligibility[pkmnid] && !allowIneligible) {
				Display(_INTL("This Pokémon can't be chosen."));
			} else {
				ret = pkmnid;
				break;
			}
		}
		@scene.EndScene;
		return ret;
	}

	public void ChooseTradablePokemon(ableProc, allowIneligible = false) {
		annot = new List<string>();
		eligibility = new List<string>();
		@party.each do |pkmn|
			elig = ableProc.call(pkmn);
			if (pkmn.egg() || pkmn.shadowPokemon() || pkmn.cannot_trade) elig = false;
			eligibility.Add(elig);
			annot.Add((elig) ? _INTL("ABLE") : _INTL("NOT ABLE"));
		}
		ret = -1;
		@scene.StartScene(
			@party,
			(@party.length > 1) ? _INTL("Choose a Pokémon.") : _INTL("Choose Pokémon or cancel."),
			annot
		);
		do { //loop; while (true);
			@scene.SetHelpText(
				(@party.length > 1) ? _INTL("Choose a Pokémon.") : _INTL("Choose Pokémon or cancel.")
			);
			pkmnid = @scene.ChoosePokemon;
			if (pkmnid < 0) break;
			if (!eligibility[pkmnid] && !allowIneligible) {
				Display(_INTL("This Pokémon can't be chosen."));
			} else {
				ret = pkmnid;
				break;
			}
		}
		@scene.EndScene;
		return ret;
	}

	public void PokemonScreen() {
		can_access_storage = false;
		if ((Game.GameData.player.has_box_link || Game.GameData.bag.has(:POKEMONBOXLINK)) &&
			!Game.GameData.game_switches[Settings.DISABLE_BOX_LINK_SWITCH] &&
			!Game.GameData.game_map.metadata&.has_flag("DisableBoxLink")) {
			can_access_storage = true;
		}
		@scene.StartScene(@party,
												(@party.length > 1) ? _INTL("Choose a Pokémon.") : _INTL("Choose Pokémon or cancel."),
												null, false, can_access_storage);
		// Main loop
		do { //loop; while (true);
			// Choose a Pokémon or cancel or press Action to quick switch
			@scene.SetHelpText((@party.length > 1) ? _INTL("Choose a Pokémon.") : _INTL("Choose Pokémon or cancel."));
			party_idx = @scene.ChoosePokemon(false, -1, 1);
			if ((party_idx.is_a(Numeric) && party_idx < 0) || (party_idx.Length > 0 && party_idx[1] < 0)) break;
			// Quick switch
			if (party_idx.Length > 0 && party_idx[0] == 1) {   // Switch
				@scene.SetHelpText(_INTL("Move to where?"));
				old_party_idx = party_idx[1];
				party_idx = @scene.ChoosePokemon(true, -1, 2);
				if (party_idx >= 0 && party_idx != old_party_idx) Switch(old_party_idx, party_idx);
				continue;
			}
			// Chose a Pokémon
			pkmn = @party[party_idx];
			// Get all commands
			command_list = new List<string>();
			commands = new List<string>();
			MenuHandlers.each_available(:party_menu, self, @party, party_idx) do |option, hash, name|
				command_list.Add(name);
				commands.Add(hash);
			}
			command_list.Add(_INTL("Cancel"));
			// Add field move commands
			if (!pkmn.egg()) {
				insert_index = (Core.DEBUG) ? 2 : 1;
				pkmn.moves.each_with_index do |move, i|
					if (!HiddenMoveHandlers.hasHandler(move.id) &&
									!new []{:MILKDRINK, :SOFTBOILED}.Contains(move.id)) continue;
					command_list.insert(insert_index, new {move.name, 1});
					commands.insert(insert_index, i);
					insert_index += 1;
				}
			}
			// Choose a menu option
			choice = @scene.ShowCommands(_INTL("Do what with {1}?", pkmn.name), command_list);
			if (choice < 0 || choice >= commands.length) continue;
			// Effect of chosen menu option
			switch (commands[choice]) {
				case Hash:   // Option defined via a MenuHandler below
					commands[choice]["effect"].call(self, @party, party_idx);
					break;
				case Integer:   // Hidden move's index
					move = pkmn.moves[commands[choice]];
					if (new []{:MILKDRINK, :SOFTBOILED}.Contains(move.id)) {
						amt = (int)Math.Max((int)Math.Floor(pkmn.totalhp / 5), 1);
						if (pkmn.hp <= amt) {
							Display(_INTL("Not enough HP..."));
							continue;
						}
						@scene.SetHelpText(_INTL("Use on which Pokémon?"));
						old_party_idx = party_idx;
						do { //loop; while (true);
							@scene.PreSelect(old_party_idx);
							party_idx = @scene.ChoosePokemon(true, party_idx);
							if (party_idx < 0) break;
							newpkmn = @party[party_idx];
							movename = move.name;
							if (party_idx == old_party_idx) {
								Display(_INTL("{1} can't use {2} on itself!", pkmn.name, movename));
							} else if (newpkmn.egg()) {
								Display(_INTL("{1} can't be used on an Egg!", movename));
							} else if (newpkmn.fainted() || newpkmn.hp == newpkmn.totalhp) {
								Display(_INTL("{1} can't be used on that Pokémon.", movename));
							} else {
								pkmn.hp -= amt;
								hpgain = ItemRestoreHP(newpkmn, amt);
								@scene.Display(_INTL("{1}'s HP was restored by {2} points.", newpkmn.name, hpgain));
								Refresh;
							}
							if (pkmn.hp <= amt) break;
						}
						@scene.Select(old_party_idx);
						Refresh;
					} else if (CanUseHiddenMove(pkmn, move.id)) {
						if (ConfirmUseHiddenMove(pkmn, move.id)) {
							@scene.EndScene;
							if (move.id == moves.FLY) {
								scene = new PokemonRegionMap_Scene(-1, false);
								screen = new PokemonRegionMapScreen(scene);
								ret = screen.StartFlyScreen;
								if (ret) {
									Game.GameData.game_temp.fly_destination = ret;
									return new {pkmn, move.id};
								}
								@scene.StartScene(
									@party, (@party.length > 1) ? _INTL("Choose a Pokémon.") : _INTL("Choose Pokémon or cancel.")
								);
								continue;
							}
							return new {pkmn, move.id};
						}
					}
					break;
			}
		}
		@scene.EndScene;
		return null;
	}
}

//===============================================================================
// Party screen menu commands.
// Note that field moves are inserted into the list of commands after the first
// command, which is usually "Summary". If playing in Debug mode, they are
// inserted after the second command instead, which is usually "Debug". See
// insert_index above if you need to change this.
//===============================================================================

MenuHandlers.add(:party_menu, :summary, {
	"name"      => _INTL("Summary"),
	"order"     => 10,
	"effect"    => (screen, party, party_idx) => {
		screen.scene.Summary(party_idx) do;
			screen.scene.SetHelpText((party.length > 1) ? _INTL("Choose a Pokémon.") : _INTL("Choose Pokémon or cancel."));
		}
	}
});

MenuHandlers.add(:party_menu, :debug, {
	"name"      => _INTL("Debug"),
	"order"     => 20,
	"condition" => (screen, party, party_idx) => next Core.DEBUG,
	"effect"    => (screen, party, party_idx) => {
		screen.PokemonDebug(party[party_idx], party_idx);
	}
});

MenuHandlers.add(:party_menu, :switch, {
	"name"      => _INTL("Switch"),
	"order"     => 30,
	"condition" => (screen, party, party_idx) => next party.length > 1,
	"effect"    => (screen, party, party_idx) => {
		screen.scene.SetHelpText(_INTL("Move to where?"));
		old_party_idx = party_idx;
		party_idx = screen.scene.ChoosePokemon(true);
		if (party_idx >= 0 && party_idx != old_party_idx) screen.Switch(old_party_idx, party_idx);
	}
});

MenuHandlers.add(:party_menu, :mail, {
	"name"      => _INTL("Mail"),
	"order"     => 40,
	"condition" => (screen, party, party_idx) => next !party[party_idx].egg() && party[party_idx].mail,
	"effect"    => (screen, party, party_idx) => {
		pkmn = party[party_idx];
		command = screen.scene.ShowCommands(_INTL("Do what with the mail?"),
																					new {_INTL("Read"), _INTL("Take"), _INTL("Cancel")});
		switch (command) {
			case 0:   // Read
				FadeOutIn do;
					DisplayMail(pkmn.mail, pkmn);
					screen.scene.SetHelpText((party.length > 1) ? _INTL("Choose a Pokémon.") : _INTL("Choose Pokémon or cancel."));
				}
				break;
			case 1:   // Take
				if (TakeItemFromPokemon(pkmn, screen)) {
					screen.RefreshSingle(party_idx);
				}
				break;
		}
	}
});

MenuHandlers.add(:party_menu, :item, {
	"name"      => _INTL("Item"),
	"order"     => 50,
	"condition" => (screen, party, party_idx) => next !party[party_idx].egg() && !party[party_idx].mail,
	"effect"    => (screen, party, party_idx) => {
		// Get all commands
		command_list = new List<string>();
		commands = new List<string>();
		MenuHandlers.each_available(:party_menu_item, screen, party, party_idx) do |option, hash, name|
			command_list.Add(name);
			commands.Add(hash);
		}
		command_list.Add(_INTL("Cancel"));
		// Choose a menu option
		choice = screen.scene.ShowCommands(_INTL("Do what with an item?"), command_list);
		if (choice < 0 || choice >= commands.length) continue;
		commands[choice]["effect"].call(screen, party, party_idx);
	}
});

MenuHandlers.add(:party_menu_item, :use, {
	"name"      => _INTL("Use"),
	"order"     => 10,
	"effect"    => (screen, party, party_idx) => {
		pkmn = party[party_idx];
		item = screen.scene.UseItem(Game.GameData.bag, pkmn) do;
			screen.scene.SetHelpText((party.length > 1) ? _INTL("Choose a Pokémon.") : _INTL("Choose Pokémon or cancel."));
		}
		if (!item) continue;
		UseItemOnPokemon(item, pkmn, screen);
		screen.RefreshSingle(party_idx);
	}
});

MenuHandlers.add(:party_menu_item, :give, {
	"name"      => _INTL("Give"),
	"order"     => 20,
	"effect"    => (screen, party, party_idx) => {
		pkmn = party[party_idx];
		item = screen.scene.ChooseItem(Game.GameData.bag) do;
			screen.scene.SetHelpText((party.length > 1) ? _INTL("Choose a Pokémon.") : _INTL("Choose Pokémon or cancel."));
		}
		if (!item || !GiveItemToPokemon(item, pkmn, screen, party_idx)) continue;
		screen.RefreshSingle(party_idx);
	}
});

MenuHandlers.add(:party_menu_item, :take, {
	"name"      => _INTL("Take"),
	"order"     => 30,
	"condition" => (screen, party, party_idx) => { next party[party_idx].hasItem() },
	"effect"    => (screen, party, party_idx) => {
		pkmn = party[party_idx];
		if (!TakeItemFromPokemon(pkmn, screen)) continue;
		screen.RefreshSingle(party_idx);
	}
});

MenuHandlers.add(:party_menu_item, :move, {
	"name"      => _INTL("Move"),
	"order"     => 40,
	"condition" => (screen, party, party_idx) => { next party[party_idx].hasItem() && !party[party_idx].item.is_mail() },
	"effect"    => (screen, party, party_idx) => {
		pkmn = party[party_idx];
		item = pkmn.item;
		itemname = item.name;
		portionitemname = item.portion_name;
		screen.scene.SetHelpText(_INTL("Move {1} to where?", itemname));
		old_party_idx = party_idx;
		moved = false;
		do { //loop; while (true);
			screen.scene.PreSelect(old_party_idx);
			party_idx = screen.scene.ChoosePokemon(true, party_idx);
			if (party_idx < 0) break;
			newpkmn = party[party_idx];
			if (party_idx == old_party_idx) break;
			if (newpkmn.egg()) {
				screen.Display(_INTL("Eggs can't hold items."));
				continue;
			} else if (!newpkmn.hasItem()) {
				newpkmn.item = item;
				pkmn.item = null;
				screen.scene.ClearSwitching;
				screen.Refresh;
				screen.Display(_INTL("{1} was given the {2} to hold.", newpkmn.name, portionitemname));
				moved = true;
				break;
			} else if (newpkmn.item.is_mail()) {
				screen.Display(_INTL("{1}'s mail must be removed before giving it an item.", newpkmn.name));
				continue;
			}
			// New Pokémon is also holding an item; ask what to do with it
			newitem = newpkmn.item;
			newitemname = newitem.portion_name;
			if (newitemname.starts_with_vowel()) {
				screen.Display(_INTL("{1} is already holding an {2}.", newpkmn.name, newitemname) + "\1");
			} else {
				screen.Display(_INTL("{1} is already holding a {2}.", newpkmn.name, newitemname) + "\1");
			}
			if (!screen.Confirm(_INTL("Would you like to switch the two items?"))) continue;
			newpkmn.item = item;
			pkmn.item = newitem;
			screen.scene.ClearSwitching;
			screen.Refresh;
			screen.Display(_INTL("{1} was given the {2} to hold.", newpkmn.name, portionitemname) + "\1");
			screen.Display(_INTL("{1} was given the {2} to hold.", pkmn.name, newitemname));
			moved = true;
			break;
		}
		if (!moved) screen.scene.Select(old_party_idx);
	}
});

//===============================================================================
// Open the party screen.
//===============================================================================
public void PokemonScreen() {
	FadeOutIn do;
		sscene = new PokemonParty_Scene();
		sscreen = new PokemonPartyScreen(sscene, Game.GameData.player.party);
		sscreen.PokemonScreen;
	}
}

//===============================================================================
// Choose a Pokémon in the party.
//===============================================================================
// Choose a Pokémon/egg from the party.
// Stores result in variable _variableNumber_ and the chosen Pokémon's name in
// variable _nameVarNumber_; result is -1 if no Pokémon was chosen
public void ChoosePokemon(variableNumber, nameVarNumber, ableProc = null, allowIneligible = false) {
	chosen = 0;
	FadeOutIn do;
		scene = new PokemonParty_Scene();
		screen = new PokemonPartyScreen(scene, Game.GameData.player.party);
		if (ableProc) {
			chosen = screen.ChooseAblePokemon(ableProc, allowIneligible);
		} else {
			screen.StartScene(_INTL("Choose a Pokémon."), false);
			chosen = screen.ChoosePokemon;
			screen.EndScene;
		}
	}
	Set(variableNumber, chosen);
	if (chosen >= 0) {
		Set(nameVarNumber, Game.GameData.player.party[chosen].name);
	} else {
		Set(nameVarNumber, "");
	}
}

public void ChooseNonEggPokemon(variableNumber, nameVarNumber) {
	ChoosePokemon(variableNumber, nameVarNumber, block: (pkmn) => { !pkmn.egg() });
}

public void ChooseAblePokemon(variableNumber, nameVarNumber) {
	ChoosePokemon(variableNumber, nameVarNumber, block: pkmn => { !pkmn.egg() && pkmn.hp > 0 });
}

// Same as ChoosePokemon, but prevents choosing an egg or a Shadow Pokémon.
public void ChooseTradablePokemon(variableNumber, nameVarNumber, ableProc = null, allowIneligible = false) {
	chosen = 0;
	FadeOutIn do;
		scene = new PokemonParty_Scene();
		screen = new PokemonPartyScreen(scene, Game.GameData.player.party);
		if (ableProc) {
			chosen = screen.ChooseTradablePokemon(ableProc, allowIneligible);
		} else {
			screen.StartScene(_INTL("Choose a Pokémon."), false);
			chosen = screen.ChoosePokemon;
			screen.EndScene;
		}
	}
	Set(variableNumber, chosen);
	if (chosen >= 0) {
		Set(nameVarNumber, Game.GameData.player.party[chosen].name);
	} else {
		Set(nameVarNumber, "");
	}
}

public void ChoosePokemonForTrade(variableNumber, nameVarNumber, wanted) {
	wanted = GameData.Species.get(wanted).species;
	ChooseTradablePokemon(variableNumber, nameVarNumber, block: (pkmn) => {
		next pkmn.species == wanted;
	});
}
