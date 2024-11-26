//===============================================================================
// Base class for all three menu classes below.
//===============================================================================
public partial class Battle.Scene.MenuBase {
	public int x		{ get { return _x; } set { _x = value; } }			protected int _x;
	public int y		{ get { return _y; } set { _y = value; } }			protected int _y;
	public int z		{ get { return _z; } }			protected int _z;
	public int visible		{ get { return _visible; } }			protected int _visible;
	public int color		{ get { return _color; } }			protected int _color;
	public int index		{ get { return _index; } }			protected int _index;
	public int mode		{ get { return _mode; } }			protected int _mode;

	// NOTE: Button width is half the width of the graphic containing them all.
	public const int BUTTON_HEIGHT = 46;
	TEXT_BASE_COLOR   = Battle.Scene.MESSAGE_BASE_COLOR;
	TEXT_SHADOW_COLOR = Battle.Scene.MESSAGE_SHADOW_COLOR;

	public void initialize(viewport = null) {
		@x          = 0;
		@y          = 0;
		@z          = 0;
		@visible    = false;
		@color      = new Color(0, 0, 0, 0);
		@index      = 0;
		@mode       = 0;
		@disposed   = false;
		@sprites    = new List<string>();
		@visibility = new List<string>();
	}

	public void dispose() {
		if (disposed()) return;
		DisposeSpriteHash(@sprites);
		@disposed = true;
	}

	public bool disposed() { return @disposed; }

	public int z { set {
		@z = value;
		@sprites.each do |i|
			if (!i[1].disposed()) i[1].z = value;
			}
	}
	}

	public int visible { set {
		@visible = value;
		@sprites.each do |i|
			if (!i[1].disposed()) i[1].visible = (value && @visibility[i[0]]);
			}
	}
	}

	public int color { set {
		@color = value;
		@sprites.each do |i|
			if (!i[1].disposed()) i[1].color = value;
			}
	}
	}

	public int index { set {
		oldValue = @index;
		@index = value;
		if (@cmdWindow) @cmdWindow.index = @index;
		if (@index != oldValue) refresh;
		}
	}

	public int mode { set {
		oldValue = @mode;
		@mode = value;
		if (@mode != oldValue) refresh;
		}
	}

	public void addSprite(key, sprite) {
		@sprites[key]    = sprite;
		@visibility[key] = true;
	}

	public void setIndexAndMode(index, mode) {
		oldIndex = @index;
		oldMode  = @mode;
		@index = index;
		@mode  = mode;
		if (@cmdWindow) @cmdWindow.index = @index;
		if (@index != oldIndex || @mode != oldMode) refresh;
	}

	public void refresh() { }

	public void update() {
		UpdateSpriteHash(@sprites);
	}
}

//===============================================================================
// Command menu (Fight/Pokémon/Bag/Run).
//===============================================================================
public partial class Battle.Scene.CommandMenu : Battle.Scene.MenuBase {
	// If true, displays graphics from Graphics/UI/Battle/overlay_command.png
	//     and Graphics/UI/Battle/cursor_command.png.
	// If false, just displays text and the command window over the graphic
	//     Graphics/UI/Battle/overlay_message.png. You will need to edit def
	//     ShowWindow to make the graphic appear while the command menu is being
	//     displayed.
	public const bool USE_GRAPHICS = true;
	// Lists of which button graphics to use in different situations/types of battle.
	MODES = new {
		new {0, 2, 1, 3},   // 0 = Regular battle
		new {0, 2, 1, 9},   // 1 = Regular battle with "Cancel" instead of "Run"
		new {0, 2, 1, 4},   // 2 = Regular battle with "Call" instead of "Run"
		new {5, 7, 6, 3},   // 3 = Safari Zone
		new {0, 8, 1, 3}    // 4 = Bug-Catching Contest
	}

	public override void initialize(viewport, z) {
		base.initialize(viewport);
		self.x = 0;
		self.y = Graphics.height - 96;
		// Create message box (shows "What will X do?")
		@msgBox = Window_UnformattedTextPokemon.newWithSize(
			"", self.x + 16, self.y + 2, 220, Graphics.height - self.y, viewport
		);
		@msgBox.baseColor   = TEXT_BASE_COLOR;
		@msgBox.shadowColor = TEXT_SHADOW_COLOR;
		@msgBox.windowskin  = null;
		addSprite("msgBox", @msgBox);
		if (USE_GRAPHICS) {
			// Create background graphic
			background = new IconSprite(self.x, self.y, viewport);
			background.setBitmap("Graphics/UI/Battle/overlay_command");
			addSprite("background", background);
			// Create bitmaps
			@buttonBitmap = new AnimatedBitmap(_INTL("Graphics/UI/Battle/cursor_command"));
			// Create action buttons
			@buttons = new Array(4) do |i|   // 4 command options, therefore 4 buttons
				button = new Sprite(viewport);
				button.bitmap = @buttonBitmap.bitmap;
				button.x = self.x + Graphics.width - 260;
				button.x += (i.even() ? 0 : (@buttonBitmap.width / 2) - 4);
				button.y = self.y + 6;
				button.y += (((i / 2) == 0) ? 0 : BUTTON_HEIGHT - 4);
				button.src_rect.width  = @buttonBitmap.width / 2;
				button.src_rect.height = BUTTON_HEIGHT;
				addSprite($"button_{i}", button);
				next button;
			}
		} else {
			// Create command window (shows Fight/Bag/Pokémon/Run)
			@cmdWindow = Window_CommandPokemon.newWithSize(
				[], self.x + Graphics.width - 240, self.y, 240, Graphics.height - self.y, viewport
			);
			@cmdWindow.columns       = 2;
			@cmdWindow.columnSpacing = 4;
			@cmdWindow.ignore_input  = true;
			addSprite("cmdWindow", @cmdWindow);
		}
		self.z = z;
		refresh;
	}

	public override void dispose() {
		base.dispose();
		@buttonBitmap&.dispose;
	}

	public override int z { set {
		base.z();
		@msgBox.z    += 1;
		if (@cmdWindow) @cmdWindow.z += 1;
		}
	}

	public void setTexts(value) {
		@msgBox.text = value[0];
		if (USE_GRAPHICS) return;
		commands = new List<string>();
		(1..4).each(i => { if (value[i]) commands.Add(value[i]); })
		@cmdWindow.commands = commands;
	}

	public void refreshButtons() {
		if (!USE_GRAPHICS) return;
		@buttons.each_with_index do |button, i|
			button.src_rect.x = (i == @index) ? @buttonBitmap.width / 2 : 0;
			button.src_rect.y = MODES[@mode][i] * BUTTON_HEIGHT;
			button.z          = self.z + ((i == @index) ? 3 : 2);
		}
	}

	public void refresh() {
		@msgBox.refresh;
		@cmdWindow&.refresh;
		refreshButtons;
	}
}

//===============================================================================
// Fight menu (choose a move).
//===============================================================================
public partial class Battle.Scene.FightMenu : Battle.Scene.MenuBase {
	public int battler		{ get { return _battler; } }			protected int _battler;
	public int shiftMode		{ get { return _shiftMode; } }			protected int _shiftMode;

	public const bool GET_MOVE_TEXT_COLOR_FROM_MOVE_BUTTON = true;

	// If true, displays graphics from Graphics/UI/Battle/overlay_fight.png
	//     and Graphics/UI/Battle/cursor_fight.png.
	// If false, just displays text and the command window over the graphic
	//     Graphics/UI/Battle/overlay_message.png. You will need to edit def
	//     ShowWindow to make the graphic appear while the command menu is being
	//     displayed.
	public const bool USE_GRAPHICS     = true;
	TYPE_ICON_HEIGHT = GameData.Type.ICON_SIZE[1];
	// Text colours of PP of selected move
	PP_COLORS = new {
		new Color(248, 72, 72), new Color(136, 48, 48),    // Red, zero PP
		new Color(248, 136, 32), new Color(144, 72, 24),   // Orange, 1/4 of total PP or less
		new Color(248, 192, 0), new Color(144, 104, 0),    // Yellow, 1/2 of total PP or less
		TEXT_BASE_COLOR, TEXT_SHADOW_COLOR;                 // Black, more than 1/2 of total PP
	}

	public override void initialize(viewport, z) {
		base.initialize(viewport);
		self.x = 0;
		self.y = Graphics.height - 96;
		@battler   = null;
		@shiftMode = 0;
		// NOTE: @mode is for the display of the Mega Evolution button.
		//       0=don't show, 1=show unpressed, 2=show pressed
		if (USE_GRAPHICS) {
			// Create bitmaps
			@buttonBitmap  = new AnimatedBitmap(_INTL("Graphics/UI/Battle/cursor_fight"));
			@typeBitmap    = new AnimatedBitmap(_INTL("Graphics/UI/types"));
			@megaEvoBitmap = new AnimatedBitmap(_INTL("Graphics/UI/Battle/cursor_mega"));
			@shiftBitmap   = new AnimatedBitmap(_INTL("Graphics/UI/Battle/cursor_shift"));
			// Create background graphic
			background = new IconSprite(0, Graphics.height - 96, viewport);
			background.setBitmap("Graphics/UI/Battle/overlay_fight");
			addSprite("background", background);
			// Create move buttons
			@buttons = new Array(Pokemon.MAX_MOVES) do |i|
				button = new Sprite(viewport);
				button.bitmap = @buttonBitmap.bitmap;
				button.x = self.x + 4;
				button.x += (i.even() ? 0 : (@buttonBitmap.width / 2) - 4);
				button.y = self.y + 6;
				button.y += (((i / 2) == 0) ? 0 : BUTTON_HEIGHT - 4);
				button.src_rect.width  = @buttonBitmap.width / 2;
				button.src_rect.height = BUTTON_HEIGHT;
				addSprite($"button_{i}", button);
				next button;
			}
			// Create overlay for buttons (shows move names)
			@overlay = new BitmapSprite(Graphics.width, Graphics.height - self.y, viewport);
			@overlay.x = self.x;
			@overlay.y = self.y;
			SetNarrowFont(@overlay.bitmap);
			addSprite("overlay", @overlay);
			// Create overlay for selected move's info (shows move's PP)
			@infoOverlay = new BitmapSprite(Graphics.width, Graphics.height - self.y, viewport);
			@infoOverlay.x = self.x;
			@infoOverlay.y = self.y;
			SetNarrowFont(@infoOverlay.bitmap);
			addSprite("infoOverlay", @infoOverlay);
			// Create type icon
			@typeIcon = new Sprite(viewport);
			@typeIcon.bitmap = @typeBitmap.bitmap;
			@typeIcon.x      = self.x + 416;
			@typeIcon.y      = self.y + 20;
			@typeIcon.src_rect.height = TYPE_ICON_HEIGHT;
			addSprite("typeIcon", @typeIcon);
			// Create Mega Evolution button
			@megaButton = new Sprite(viewport);
			@megaButton.bitmap = @megaEvoBitmap.bitmap;
			@megaButton.x      = self.x + 120;
			@megaButton.y      = self.y - (@megaEvoBitmap.height / 2);
			@megaButton.src_rect.height = @megaEvoBitmap.height / 2;
			addSprite("megaButton", @megaButton);
			// Create Shift button
			@shiftButton = new Sprite(viewport);
			@shiftButton.bitmap = @shiftBitmap.bitmap;
			@shiftButton.x      = self.x + 4;
			@shiftButton.y      = self.y - @shiftBitmap.height;
			addSprite("shiftButton", @shiftButton);
		} else {
			// Create message box (shows type and PP of selected move)
			@msgBox = Window_AdvancedTextPokemon.newWithSize(
				"", self.x + 320, self.y, Graphics.width - 320, Graphics.height - self.y, viewport
			);
			@msgBox.baseColor   = TEXT_BASE_COLOR;
			@msgBox.shadowColor = TEXT_SHADOW_COLOR;
			SetNarrowFont(@msgBox.contents);
			addSprite("msgBox", @msgBox);
			// Create command window (shows moves)
			@cmdWindow = Window_CommandPokemon.newWithSize(
				[], self.x, self.y, 320, Graphics.height - self.y, viewport
			);
			@cmdWindow.columns       = 2;
			@cmdWindow.columnSpacing = 4;
			@cmdWindow.ignore_input  = true;
			SetNarrowFont(@cmdWindow.contents);
			addSprite("cmdWindow", @cmdWindow);
		}
		self.z = z;
	}

	public override void dispose() {
		base.dispose();
		@buttonBitmap&.dispose;
		@typeBitmap&.dispose;
		@megaEvoBitmap&.dispose;
		@shiftBitmap&.dispose;
	}

	public override int z { set {
		base.z();
		if (@msgBox) @msgBox.z      += 1;
		if (@cmdWindow) @cmdWindow.z   += 2;
		if (@overlay) @overlay.z     += 5;
		if (@infoOverlay) @infoOverlay.z += 6;
		if (@typeIcon) @typeIcon.z    += 1;
		}
	}

	public int battler { set {
		@battler = value;
		refresh;
		refreshButtonNames;
		}
	}

	public int shiftMode { set {
		oldValue = @shiftMode;
		@shiftMode = value;
		if (@shiftMode != oldValue) refreshShiftButton;
		}
	}

	public void refreshButtonNames() {
		moves = (@battler) ? @battler.moves : [];
		if (!USE_GRAPHICS) {
			// Fill in command window
			commands = new List<string>();
			for (int i = (int)Math.Max(4, moves.length); i < (int)Math.Max(4, moves.length); i++) { //for '(int)Math.Max(4, moves.length)' times do => |i|
				commands.Add((moves[i]) ? moves[i].name : "-");
			}
			@cmdWindow.commands = commands;
			return;
		}
		// Draw move names onto overlay
		@overlay.bitmap.clear;
		textPos = new List<string>();
		@buttons.each_with_index do |button, i|
			if (!@visibility[$"button_{i}"]) continue;
			x = button.x - self.x + (button.src_rect.width / 2);
			y = button.y - self.y + 14;
			moveNameBase = TEXT_BASE_COLOR;
			if (GET_MOVE_TEXT_COLOR_FROM_MOVE_BUTTON && moves[i].display_type(@battler)) {
				// NOTE: This takes a color from a particular pixel in the button
				//       graphic and makes the move name's base color that same color.
				//       The pixel is at coordinates 10,34 in the button box. If you
				//       change the graphic, you may want to change the below line of
				//       code to ensure the font is an appropriate color.
				moveNameBase = button.bitmap.get_pixel(10, button.src_rect.y + 34);
			}
			textPos.Add(new {moves[i].name, x, y, :center, moveNameBase, TEXT_SHADOW_COLOR});
		}
		DrawTextPositions(@overlay.bitmap, textPos);
	}

	public void refreshSelection() {
		moves = (@battler) ? @battler.moves : [];
		if (USE_GRAPHICS) {
			// Choose appropriate button graphics and z positions
			@buttons.each_with_index do |button, i|
				if (!moves[i]) {
					@visibility[$"button_{i}"] = false;
					continue;
				}
				@visibility[$"button_{i}"] = true;
				button.src_rect.x = (i == @index) ? @buttonBitmap.width / 2 : 0;
				button.src_rect.y = GameData.Type.get(moves[i].display_type(@battler)).icon_position * BUTTON_HEIGHT;
				button.z          = self.z + ((i == @index) ? 4 : 3);
			}
		}
		refreshMoveData(moves[@index]);
	}

	public void refreshMoveData(move) {
		// Write PP and type of the selected move
		if (!USE_GRAPHICS) {
			if (!move) return;
			moveType = GameData.Type.get(move.display_type(@battler)).name;
			if (move.total_pp <= 0) {
				@msgBox.text = _INTL("PP: ---<br>TYPE/{1}", moveType);
			} else {
				@msgBox.text = string.Format("PP: {1: 2d}/{2: 2d}<br>TYPE/{3:s}",
																move.pp, move.total_pp, moveType);
			}
			return;
		}
		@infoOverlay.bitmap.clear;
		if (!move) {
			@visibility["typeIcon"] = false;
			return;
		}
		@visibility["typeIcon"] = true;
		// Type icon
		type_number = GameData.Type.get(move.display_type(@battler)).icon_position;
		@typeIcon.src_rect.y = type_number * TYPE_ICON_HEIGHT;
		// PP text
		if (move.total_pp > 0) {
			ppFraction = (int)Math.Min((4.0 * move.pp / move.total_pp).ceil, 3);
			textPos = new List<string>();
			textPos.Add(new {_INTL("PP: {1}/{2}", move.pp, move.total_pp),
										448, 56, :center, PP_COLORS[ppFraction * 2], PP_COLORS[(ppFraction * 2) + 1]});
			DrawTextPositions(@infoOverlay.bitmap, textPos);
		}
	}

	public void refreshMegaEvolutionButton() {
		if (!USE_GRAPHICS) return;
		@megaButton.src_rect.y    = (@mode - 1) * @megaEvoBitmap.height / 2;
		@megaButton.x             = self.x + ((@shiftMode > 0) ? 204 : 120);
		@megaButton.z             = self.z - 1;
		@visibility["megaButton"] = (@mode > 0);
	}

	public void refreshShiftButton() {
		if (!USE_GRAPHICS) return;
		@shiftButton.src_rect.y    = (@shiftMode - 1) * @shiftBitmap.height;
		@shiftButton.z             = self.z - 1;
		@visibility["shiftButton"] = (@shiftMode > 0);
	}

	public void refresh() {
		if (!@battler) return;
		refreshSelection;
		refreshMegaEvolutionButton;
		refreshShiftButton;
	}
}

//===============================================================================
// Target menu (choose a move's target).
// NOTE: Unlike the command and fight menus, this one doesn't have a textbox-only
//       version.
//===============================================================================
public partial class Battle.Scene.TargetMenu : Battle.Scene.MenuBase {
	public int mode		{ get { return _mode; } set { _mode = value; } }			protected int _mode;

	// Lists of which button graphics to use in different situations/types of battle.
	MODES = new {
		new {0, 2, 1, 3},   // 0 = Regular battle
		new {0, 2, 1, 9},   // 1 = Regular battle with "Cancel" instead of "Run"
		new {0, 2, 1, 4},   // 2 = Regular battle with "Call" instead of "Run"
		new {5, 7, 6, 3},   // 3 = Safari Zone
		new {0, 8, 1, 3}    // 4 = Bug-Catching Contest
	}
	public const int CMD_BUTTON_WIDTH_SMALL = 170;
	TEXT_BASE_COLOR   = new Color(240, 248, 224);
	TEXT_SHADOW_COLOR = new Color(64, 64, 64);

	public override void initialize(viewport, z, sideSizes) {
		base.initialize(viewport);
		@sideSizes = sideSizes;
		maxIndex = (@sideSizes[0] > @sideSizes[1]) ? (@sideSizes[0] - 1) * 2 : (@sideSizes[1] * 2) - 1;
		@smallButtons = (@sideSizes.max > 2);
		self.x = 0;
		self.y = Graphics.height - 96;
		@texts = new List<string>();
		// NOTE: @mode is for which buttons are shown as selected.
		//       0=select 1 button (@index), 1=select all buttons with text
		// Create bitmaps
		@buttonBitmap = new AnimatedBitmap("Graphics/UI/Battle/cursor_target");
		// Create target buttons
		@buttons = new Array(maxIndex + 1) do |i|
			numButtons = @sideSizes[i % 2];
			if (numButtons <= i / 2) continue;
			// NOTE: Battler indices go from left to right from the perspective of
			//       that side's trainer, so inc is different for each side for the
			//       same value of i/2.
			inc = (i.even()) ? i / 2 : numButtons - 1 - (i / 2);
			button = new Sprite(viewport);
			button.bitmap = @buttonBitmap.bitmap;
			button.src_rect.width  = (@smallButtons) ? CMD_BUTTON_WIDTH_SMALL : @buttonBitmap.width / 2;
			button.src_rect.height = BUTTON_HEIGHT;
			if (@smallButtons) {
				button.x    = self.x + 170 - new {0, 82, 166}[numButtons - 1];
			} else {
				button.x    = self.x + 138 - new {0, 116}[numButtons - 1];
			}
			button.x += (button.src_rect.width - 4) * inc;
			button.y = self.y + 6;
			button.y += (BUTTON_HEIGHT - 4) * ((i + 1) % 2);
			addSprite($"button_{i}", button);
			next button;
		}
		// Create overlay (shows target names)
		@overlay = new BitmapSprite(Graphics.width, Graphics.height - self.y, viewport);
		@overlay.x = self.x;
		@overlay.y = self.y;
		SetNarrowFont(@overlay.bitmap);
		addSprite("overlay", @overlay);
		self.z = z;
		refresh;
	}

	public override void dispose() {
		base.dispose();
		@buttonBitmap&.dispose;
	}

	public override int z { set {
		base.z();
		if (@overlay) @overlay.z += 5;
		}
	}

	public void setDetails(texts, mode) {
		@texts = texts;
		@mode  = mode;
		refresh;
	}

	public void refreshButtons() {
		// Choose appropriate button graphics and z positions
		@buttons.each_with_index do |button, i|
			if (!button) continue;
			sel = false;
			buttonType = 0;
			if (@texts[i]) {
				sel ||= (@mode == 0 && i == @index);
				sel ||= (@mode == 1);
				buttonType = (i.even()) ? 1 : 2;
			}
			buttonType = (2 * buttonType) + ((@smallButtons) ? 1 : 0);
			button.src_rect.x = (sel) ? @buttonBitmap.width / 2 : 0;
			button.src_rect.y = buttonType * BUTTON_HEIGHT;
			button.z          = self.z + ((sel) ? 3 : 2);
		}
		// Draw target names onto overlay
		@overlay.bitmap.clear;
		textpos = new List<string>();
		@buttons.each_with_index do |button, i|
			if (!button || nil_or_empty(@texts[i])) continue;
			x = button.x - self.x + (button.src_rect.width / 2);
			y = button.y - self.y + 14;
			textpos.Add(new {@texts[i], x, y, :center, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR});
		}
		DrawTextPositions(@overlay.bitmap, textpos);
	}

	public void refresh() {
		refreshButtons;
	}
}
