//===============================================================================
// Data box for regular battles.
//===============================================================================
public partial class Battle.Scene.PokemonDataBox : Sprite {
	public int battler		{ get { return _battler; } }			protected int _battler;
	public int selected		{ get { return _selected; } set { _selected = value; } }			protected int _selected;

	// Time in seconds to fully fill the Exp bar (from empty).
	public const int EXP_BAR_FILL_TIME = 1.75;
	// Time in seconds for this data box to flash when the Exp fully fills.
	public const int EXP_FULL_FLASH_DURATION = 0.2;
	// Maximum time in seconds to make a change to the HP bar.
	public const int HP_BAR_CHANGE_TIME = 1.0;
	// Time (in seconds) for one complete sprite bob cycle (up and down) while
	// choosing a command for this battler or when this battler is being chosen as
	// a target. Set to null to prevent bobbing.
	public const int BOBBING_DURATION = 0.6;
	// Height in pixels of a status icon
	public const int STATUS_ICON_HEIGHT = 16;
	// Text colors
	NAME_BASE_COLOR     = new Color(72, 72, 72);
	NAME_SHADOW_COLOR   = new Color(184, 184, 184);
	MALE_BASE_COLOR     = new Color(48, 96, 216);
	public const int MALE_SHADOW_COLOR   = NAME_SHADOW_COLOR;
	FEMALE_BASE_COLOR   = new Color(248, 88, 40);
	public const int FEMALE_SHADOW_COLOR = NAME_SHADOW_COLOR;

	public override void initialize(battler, sideSize, viewport = null) {
		base.initialize(viewport);
		@battler         = battler;
		@sprites         = new List<string>();
		@spriteX         = 0;
		@spriteY         = 0;
		@spriteBaseX     = 0;
		@selected        = 0;
		@show_hp_numbers = false;
		@show_exp_bar    = false;
		initializeDataBoxGraphic(sideSize);
		initializeOtherGraphics(viewport);
		refresh;
	}

	public void initializeDataBoxGraphic(sideSize) {
		onPlayerSide = @battler.index.even();
		// Get the data box graphic and set whether the HP numbers/Exp bar are shown
		if (sideSize == 1) {   // One Pokémon on side, use the regular dara box BG
			bgFilename = new {_INTL("Graphics/UI/Battle/databox_normal"),
										_INTL("Graphics/UI/Battle/databox_normal_foe")}[@battler.index % 2];
			if (onPlayerSide) {
				@show_hp_numbers = true;
				@show_exp_bar    = true;
			}
		} else {   // Multiple Pokémon on side, use the thin dara box BG
			bgFilename = new {_INTL("Graphics/UI/Battle/databox_thin"),
										_INTL("Graphics/UI/Battle/databox_thin_foe")}[@battler.index % 2];
		}
		@databoxBitmap&.dispose;
		@databoxBitmap = new AnimatedBitmap(bgFilename);
		// Determine the co-ordinates of the data box and the left edge padding width
		if (onPlayerSide) {
			@spriteX = Graphics.width - 244;
			@spriteY = Graphics.height - 192;
			@spriteBaseX = 34;
		} else {
			@spriteX = -16;
			@spriteY = 36;
			@spriteBaseX = 16;
		}
		switch (sideSize) {
			case 2:
				@spriteX += new {-12,  12,  0,  0}[@battler.index];
				@spriteY += new {-20, -34, 34, 20}[@battler.index];
				break;
			case 3:
				@spriteX += new {-12,  12, -6,  6,  0,  0}[@battler.index];
				@spriteY += new {-42, -46,  4,  0, 50, 46}[@battler.index];
				break;
		}
	}

	public void initializeOtherGraphics(viewport) {
		// Create other bitmaps
		@numbersBitmap = new AnimatedBitmap("Graphics/UI/Battle/icon_numbers");
		@hpBarBitmap   = new AnimatedBitmap("Graphics/UI/Battle/overlay_hp");
		@expBarBitmap  = new AnimatedBitmap("Graphics/UI/Battle/overlay_exp");
		// Create sprite to draw HP numbers on
		@hpNumbers = new BitmapSprite(124, 16, viewport);
//    SetSmallFont(@hpNumbers.bitmap)
		@sprites["hpNumbers"] = @hpNumbers;
		// Create sprite wrapper that displays HP bar
		@hpBar = new Sprite(viewport);
		@hpBar.bitmap = @hpBarBitmap.bitmap;
		@hpBar.src_rect.height = @hpBarBitmap.height / 3;
		@sprites["hpBar"] = @hpBar;
		// Create sprite wrapper that displays Exp bar
		@expBar = new Sprite(viewport);
		@expBar.bitmap = @expBarBitmap.bitmap;
		@sprites["expBar"] = @expBar;
		// Create sprite wrapper that displays everything except the above
		@contents = new Bitmap(@databoxBitmap.width, @databoxBitmap.height);
		self.bitmap  = @contents;
		self.visible = false;
		self.z       = 150 + ((@battler.index / 2) * 5);
		SetSystemFont(self.bitmap);
	}

	public override void dispose() {
		DisposeSpriteHash(@sprites);
		@databoxBitmap.dispose;
		@numbersBitmap.dispose;
		@hpBarBitmap.dispose;
		@expBarBitmap.dispose;
		@contents.dispose;
		base.dispose();
	}

	public override int x { set {
		base.x();
		@hpBar.x     = value + @spriteBaseX + 102;
		@expBar.x    = value + @spriteBaseX + 6;
		@hpNumbers.x = value + @spriteBaseX + 80;
		}
	}

	public override int y { set {
		base.y();
		@hpBar.y     = value + 40;
		@expBar.y    = value + 74;
		@hpNumbers.y = value + 52;
		}
	}

	public override int z { set {
		base.z();
		@hpBar.z     = value + 1;
		@expBar.z    = value + 1;
		@hpNumbers.z = value + 2;
		}
	}

	public override int opacity { set {
		base.opacity();
		@sprites.each do |i|
			if (!i[1].disposed()) i[1].opacity = value;
			}
	}
	}

	public override int visible { set {
		base.visible();
		@sprites.each do |i|
			if (!i[1].disposed()) i[1].visible = value;
			}
	}
		@expBar.visible = (value && @show_exp_bar);
	}

	public override int color { set {
		base.color();
		@sprites.each do |i|
			if (!i[1].disposed()) i[1].color = value;
			}
	}
	}

	public int battler { set {
		@battler = b;
		self.visible = (@battler && !@battler.fainted());
		}
	}

	public void hp() {
		return (animating_hp()) ? @anim_hp_current : @battler.hp;
	}

	public void exp_fraction() {
		if (animating_exp()) {
			if (@anim_exp_range == 0) return 0.0;
			return @anim_exp_current.to_f / @anim_exp_range;
		}
		return @battler.pokemon.exp_fraction;
	}

	// NOTE: A change in HP takes the same amount of time to animate, no matter how
	//       big a change it is.
	public void animate_hp(old_val, new_val) {
		if (old_val == new_val) return;
		@anim_hp_start = old_val;
		@anim_hp_end = new_val;
		@anim_hp_current = old_val;
		@anim_hp_timer_start = System.uptime;
	}

	public bool animating_hp() {
		return @anim_hp_timer_start != null;
	}

	// NOTE: Filling the Exp bar from empty to full takes EXP_BAR_FILL_TIME seconds
	//       no matter what. Filling half of it takes half as long, etc.
	public void animate_exp(old_val, new_val, range) {
		if (old_val == new_val || range == 0 || !@show_exp_bar) return;
		@anim_exp_start = old_val;
		@anim_exp_end = new_val;
		@anim_exp_range = range;
		@anim_exp_duration_mult = (new_val - old_val).abs / range.to_f;
		@anim_exp_current = old_val;
		@anim_exp_timer_start = System.uptime;
		if (@show_exp_bar) SEPlay("Pkmn exp gain");
	}

	public bool animating_exp() {
		return @anim_exp_timer_start != null;
	}

	public void DrawNumber(number, btmp, startX, startY, align = :left) {
		// -1 means draw the / character
		n = (number == -1) ? [10] : number.ToInt().digits.reverse;
		charWidth  = @numbersBitmap.width / 11;
		charHeight = @numbersBitmap.height;
		if (align == :right) startX -= charWidth * n.length;
		foreach (var i in n) { //'n.each' do => |i|
			btmp.blt(startX, startY, @numbersBitmap.bitmap, new Rect(i * charWidth, 0, charWidth, charHeight));
			startX += charWidth;
		}
	}

	public void draw_background() {
		self.bitmap.blt(0, 0, @databoxBitmap.bitmap, new Rect(0, 0, @databoxBitmap.width, @databoxBitmap.height));
	}

	public void draw_name() {
		nameWidth = self.bitmap.text_size(@battler.name).width;
		nameOffset = 0;
		if (nameWidth > 116) nameOffset = nameWidth - 116;
		DrawTextPositions(self.bitmap, new {@battler.name, @spriteBaseX + 8 - nameOffset, 12, :left,
																			NAME_BASE_COLOR, NAME_SHADOW_COLOR}
		);
	}

	public void draw_level() {
		// "Lv" graphic
		DrawImagePositions(self.bitmap, new {_INTL("Graphics/UI/Battle/overlay_lv"), @spriteBaseX + 140, 16});
		// Level number
		DrawNumber(@battler.level, self.bitmap, @spriteBaseX + 162, 16);
	}

	public void draw_gender() {
		gender = @battler.displayGender;
		if (!new []{0, 1}.Contains(gender)) return;
		gender_text  = (gender == 0) ? _INTL("♂") : _INTL("♀");
		base_color   = (gender == 0) ? MALE_BASE_COLOR : FEMALE_BASE_COLOR;
		shadow_color = (gender == 0) ? MALE_SHADOW_COLOR : FEMALE_SHADOW_COLOR;
		DrawTextPositions(self.bitmap, new {gender_text, @spriteBaseX + 126, 12, :left, base_color, shadow_color});
	}

	public void draw_status() {
		if (@battler.status == statuses.NONE) return;
		if (@battler.status == statuses.POISON && @battler.statusCount > 0) {   // Badly poisoned
			s = GameData.Status.count - 1;
		} else {
			s = GameData.Status.get(@battler.status).icon_position;
		}
		if (s < 0) return;
		DrawImagePositions(self.bitmap, new {_INTL("Graphics/UI/Battle/icon_statuses"), @spriteBaseX + 24, 36,
																				0, s * STATUS_ICON_HEIGHT, -1, STATUS_ICON_HEIGHT});
	}

	public void draw_shiny_icon() {
		if (!@battler.shiny()) return;
		shiny_x = (@battler.opposes(0)) ? 206 : -6;   // Foe's/player's
		DrawImagePositions(self.bitmap, new {"Graphics/UI/shiny", @spriteBaseX + shiny_x, 36});
	}

	public void draw_special_form_icon() {
		// Mega Evolution/Primal Reversion icon
		if (@battler.mega()) {
			DrawImagePositions(self.bitmap, new {"Graphics/UI/Battle/icon_mega", @spriteBaseX + 8, 34});
		} else if (@battler.primal()) {
			filename = null;
			if (@battler.isSpecies(Speciess.GROUDON)) {
				filename = "Graphics/UI/Battle/icon_primal_Groudon";
			} else if (@battler.isSpecies(Speciess.KYOGRE)) {
				filename = "Graphics/UI/Battle/icon_primal_Kyogre";
			}
			primalX = (@battler.opposes()) ? 208 : -28;   // Foe's/player's
			if (filename) DrawImagePositions(self.bitmap, new {filename, @spriteBaseX + primalX, 4});
		}
	}

	public void draw_owned_icon() {
		if (!@battler.owned() || !@battler.opposes(0)) return;   // Draw for foe Pokémon only
		DrawImagePositions(self.bitmap, new {"Graphics/UI/Battle/icon_own", @spriteBaseX + 8, 36});
	}

	public void refresh() {
		self.bitmap.clear;
		if (!@battler.pokemon) return;
		draw_background;
		draw_name;
		draw_level;
		draw_gender;
		draw_status;
		draw_shiny_icon;
		draw_special_form_icon;
		draw_owned_icon;
		refresh_hp;
		refresh_exp;
	}

	public void refresh_hp() {
		@hpNumbers.bitmap.clear;
		if (!@battler.pokemon) return;
		// Show HP numbers
		if (@show_hp_numbers) {
			DrawNumber(self.hp, @hpNumbers.bitmap, 54, 2, :right);
			DrawNumber(-1, @hpNumbers.bitmap, 54, 2);   // / char
			DrawNumber(@battler.totalhp, @hpNumbers.bitmap, 70, 2);
		}
		// Resize HP bar
		w = 0;
		if (self.hp > 0) {
			w = @hpBarBitmap.width.to_f * self.hp / @battler.totalhp;
			if (w < 1) w = 1;
			// NOTE: The line below snaps the bar's width to the nearest 2 pixels, to
			//       fit in with the rest of the graphics which are doubled in size.
			w = ((int)Math.Round(w / 2.0)) * 2;
		}
		@hpBar.src_rect.width = w;
		hpColor = 0;                                      // Green bar
		if (self.hp <= @battler.totalhp / 2) hpColor = 1;   // Yellow bar
		if (self.hp <= @battler.totalhp / 4) hpColor = 2;   // Red bar
		@hpBar.src_rect.y = hpColor * @hpBarBitmap.height / 3;
	}

	public void refresh_exp() {
		if (!@show_exp_bar) return;
		w = exp_fraction * @expBarBitmap.width;
		// NOTE: The line below snaps the bar's width to the nearest 2 pixels, to
		//       fit in with the rest of the graphics which are doubled in size.
		w = ((int)Math.Round(w / 2)) * 2;
		@expBar.src_rect.width = w;
	}

	public void update_hp_animation() {
		if (!animating_hp()) return;
		@anim_hp_current = lerp(@anim_hp_start, @anim_hp_end, HP_BAR_CHANGE_TIME,
														@anim_hp_timer_start, System.uptime);
		// Refresh the HP bar/numbers
		refresh_hp;
		// End the HP bar filling animation
		if (@anim_hp_current == @anim_hp_end) {
			@anim_hp_start = null;
			@anim_hp_end = null;
			@anim_hp_timer_start = null;
			@anim_hp_current = null;
		}
	}

	public void update_exp_animation() {
		if (!animating_exp()) return;
		if (!@show_exp_bar) {   // Not showing the Exp bar, no need to waste time animating it
			@anim_exp_timer_start = null;
			return;
		}
		duration = EXP_BAR_FILL_TIME * @anim_exp_duration_mult;
		@anim_exp_current = lerp(@anim_exp_start, @anim_exp_end, duration,
														@anim_exp_timer_start, System.uptime);
		// Refresh the Exp bar
		refresh_exp;
		if (@anim_exp_current != @anim_exp_end) return;   // Exp bar still has more to animate
		// End the Exp bar filling animation
		if (@anim_exp_current >= @anim_exp_range) {
			if (@anim_exp_flash_timer_start) {
				// Waiting for Exp full flash to finish
				if (System.uptime - @anim_exp_flash_timer_start < EXP_FULL_FLASH_DURATION) return;
			} else {
				// Show the Exp full flash
				@anim_exp_flash_timer_start = System.uptime;
				SEStop;
				SEPlay("Pkmn exp full");
				flash_duration = EXP_FULL_FLASH_DURATION * Graphics.frame_rate;   // Must be in frames, not seconds
				self.flash(new Color(64, 200, 248, 192), flash_duration);
				@sprites.each do |i|
					if (!i[1].disposed()) i[1].flash(new Color(64, 200, 248, 192), flash_duration);
				}
				return;
			}
		}
		if (!@anim_exp_flash_timer_start) SEStop;
		@anim_exp_start = null;
		@anim_exp_end = null;
		@anim_exp_duration_mult = null;
		@anim_exp_current = null;
		@anim_exp_timer_start = null;
		@anim_exp_flash_timer_start = null;
	}

	public void update_positions() {
		self.x = @spriteX;
		self.y = @spriteY;
		// Data box bobbing while Pokémon is selected
		if ((@selected == 1 || @selected == 2) && BOBBING_DURATION) {   // Choosing commands/targeted
			bob_delta = System.uptime % BOBBING_DURATION;   // 0-BOBBING_DURATION
			bob_frame = (int)Math.Floor(4 * bob_delta / BOBBING_DURATION);
			switch (bob_frame) {
				case 1:  self.y = @spriteY - 2; break;
				case 3:  self.y = @spriteY + 2; break;
			}
		}
	}

	public override void update() {
		base.update();
		// Animate HP bar
		update_hp_animation;
		// Animate Exp bar
		update_exp_animation;
		// Update coordinates of the data box
		update_positions;
		UpdateSpriteHash(@sprites);
	}
}

//===============================================================================
// Splash bar to announce a triggered ability.
//===============================================================================
public partial class Battle.Scene.AbilitySplashBar : Sprite {
	public int battler		{ get { return _battler; } }			protected int _battler;

	TEXT_BASE_COLOR   = new Color(0, 0, 0);
	TEXT_SHADOW_COLOR = new Color(248, 248, 248);

	public override void initialize(side, viewport = null) {
		base.initialize(viewport);
		@side    = side;
		@battler = null;
		// Create sprite wrapper that displays background graphic
		@bgBitmap = new AnimatedBitmap("Graphics/UI/Battle/ability_bar");
		@bgSprite = new Sprite(viewport);
		@bgSprite.bitmap = @bgBitmap.bitmap;
		@bgSprite.src_rect.y      = (side == 0) ? 0 : @bgBitmap.height / 2;
		@bgSprite.src_rect.height = @bgBitmap.height / 2;
		// Create bitmap that displays the text
		@contents = new Bitmap(@bgBitmap.width, @bgBitmap.height / 2);
		self.bitmap = @contents;
		SetSystemFont(self.bitmap);
		// Position the bar
		self.x       = (side == 0) ? -Graphics.width / 2 : Graphics.width;
		self.y       = (side == 0) ? 180 : 80;
		self.z       = 120;
		self.visible = false;
	}

	public override void dispose() {
		@bgSprite.dispose;
		@bgBitmap.dispose;
		@contents.dispose;
		base.dispose();
	}

	public override int x { set {
		base.x();
		@bgSprite.x = value;
		}
	}

	public override int y { set {
		base.y();
		@bgSprite.y = value;
		}
	}

	public override int z { set {
		base.z();
		@bgSprite.z = value - 1;
		}
	}

	public override int opacity { set {
		base.opacity();
		@bgSprite.opacity = value;
		}
	}

	public override int visible { set {
		base.visible();
		@bgSprite.visible = value;
		}
	}

	public override int color { set {
		base.color();
		@bgSprite.color = value;
		}
	}

	public int battler { set {
		@battler = value;
		refresh;
		}
	}

	public void refresh() {
		self.bitmap.clear;
		if (!@battler) return;
		textPos = new List<string>();
		textX = (@side == 0) ? 10 : self.bitmap.width - 8;
		align = (@side == 0) ? :left : :right;
		// Draw Pokémon's name
		textPos.Add(new {_INTL("{1}'s", @battler.name), textX, 8, align,
									TEXT_BASE_COLOR, TEXT_SHADOW_COLOR, :outline});
		// Draw Pokémon's ability
		textPos.Add(new {@battler.abilityName, textX, 38, align,
									TEXT_BASE_COLOR, TEXT_SHADOW_COLOR, :outline});
		DrawTextPositions(self.bitmap, textPos);
	}

	public override void update() {
		base.update();
		@bgSprite.update;
	}
}

//===============================================================================
// Pokémon sprite (used in battle).
//===============================================================================
public partial class Battle.Scene.BattlerSprite : RPG.Sprite {
	public int pkmn		{ get { return _pkmn; } }			protected int _pkmn;
	public int index		{ get { return _index; } set { _index = value; } }			protected int _index;
	public int selected		{ get { return _selected; } set { _selected = value; } }			protected int _selected;
	public int sideSize		{ get { return _sideSize; } }			protected int _sideSize;

	// Time (in seconds) for one complete sprite bob cycle (up and down) while
	// choosing a command for this battler. Set to null to prevent bobbing.
	public const int COMMAND_BOBBING_DURATION = 0.6;
	// Time (in seconds) for one complete blinking cycle while this battler is
	// being chosen as a target. Set to null to prevent blinking.
	public const int TARGET_BLINKING_DURATION = 0.3;

	public override void initialize(viewport, sideSize, index, battleAnimations) {
		base.initialize(viewport);
		@pkmn             = null;
		@sideSize         = sideSize;
		@index            = index;
		@battleAnimations = battleAnimations;
		// @selected: 0 = not selected, 1 = choosing action bobbing for this Pokémon,
		//            2 = flashing when targeted
		@selected         = 0;
		@updating         = false;
		@spriteX          = 0;   // Actual x coordinate
		@spriteY          = 0;   // Actual y coordinate
		@spriteXExtra     = 0;   // Offset due to "bobbing" animation
		@spriteYExtra     = 0;   // Offset due to "bobbing" animation
		@_iconBitmap      = null;
		self.visible      = false;
	}

	public override void dispose() {
		@_iconBitmap&.dispose;
		@_iconBitmap = null;
		if (!self.disposed()) self.bitmap = null;
		base.dispose();
	}

	public int x { get { return @spriteX; } }
	public int y { get { return @spriteY; } }

	public override int x { set {
		@spriteX = value;
		base.x(value + @spriteXExtra);
		}
	}

	public override int y { set {
		@spriteY = value;
		base.y(value + @spriteYExtra);
		}
	}

	public int width  { get { return (self.bitmap) ? self.bitmap.width : 0;  } }
	public int height { get { return (self.bitmap) ? self.bitmap.height : 0; } }

	public override int visible { set {
		if (!@updating) @spriteVisible = value;   // For selection/targeting flashing
		base.visible();
		}
	}

	// Set sprite's origin to bottom middle
	public void SetOrigin() {
		if (!@_iconBitmap) return;
		self.ox = @_iconBitmap.width / 2;
		self.oy = @_iconBitmap.height;
	}

	public void SetPosition() {
		if (!@_iconBitmap) return;
		SetOrigin;
		if (@index.even()) {
			self.z = 50 + (5 * @index / 2);
		} else {
			self.z = 50 - (5 * (@index + 1) / 2);
		}
		// Set original position
		p = Battle.Scene.BattlerPosition(@index, @sideSize);
		@spriteX = p[0];
		@spriteY = p[1];
		// Apply metrics
		@pkmn.species_data.apply_metrics_to_sprite(self, @index);
	}

	public void setPokemonBitmap(pkmn, back = false) {
		@pkmn = pkmn;
		@_iconBitmap&.dispose;
		@_iconBitmap = GameData.Species.sprite_bitmap_from_pokemon(@pkmn, back);
		self.bitmap = (@_iconBitmap) ? @_iconBitmap.bitmap : null;
		SetPosition;
	}

	// This method plays the battle entrance animation of a Pokémon. By default
	// this is just playing the Pokémon's cry, but you can expand on it. The
	// recommendation is to create a PictureEx animation and push it into
	// the @battleAnimations array.
	public void PlayIntroAnimation(pictureEx = null) {
		@pkmn&.play_cry;
	}

	public void update() {
		if (!@_iconBitmap) return;
		@updating = true;
		// Update bitmap
		@_iconBitmap.update;
		self.bitmap = @_iconBitmap.bitmap;
		// Pokémon sprite bobbing while Pokémon is selected
		@spriteYExtra = 0;
		if (@selected == 1 && COMMAND_BOBBING_DURATION) {    // When choosing commands for this Pokémon
			bob_delta = System.uptime % COMMAND_BOBBING_DURATION;   // 0-COMMAND_BOBBING_DURATION
			bob_frame = (int)Math.Floor(4 * bob_delta / COMMAND_BOBBING_DURATION);
			switch (bob_frame) {
				case 1:  @spriteYExtra = 2; break;
				case 3:  @spriteYExtra = -2; break;
			}
		}
		self.x       = self.x;
		self.y       = self.y;
		self.visible = @spriteVisible;
		// Pokémon sprite blinking when targeted
		if (@selected == 2 && @spriteVisible && TARGET_BLINKING_DURATION) {
			blink_delta = System.uptime % TARGET_BLINKING_DURATION;   // 0-TARGET_BLINKING_DURATION
			blink_frame = (int)Math.Floor(3 * blink_delta / TARGET_BLINKING_DURATION);
			self.visible = (blink_frame != 0);
		}
		@updating = false;
	}
}

//===============================================================================
// Shadow sprite for Pokémon (used in battle).
//===============================================================================
public partial class Battle.Scene.BattlerShadowSprite : RPG.Sprite {
	public int pkmn		{ get { return _pkmn; } }			protected int _pkmn;
	public int index		{ get { return _index; } set { _index = value; } }			protected int _index;
	public int selected		{ get { return _selected; } set { _selected = value; } }			protected int _selected;

	public override void initialize(viewport, sideSize, index) {
		base.initialize(viewport);
		@pkmn        = null;
		@sideSize    = sideSize;
		@index       = index;
		@_iconBitmap = null;
		self.visible = false;
	}

	public override void dispose() {
		@_iconBitmap&.dispose;
		@_iconBitmap = null;
		if (!self.disposed()) self.bitmap = null;
		base.dispose();
	}

	public int width  { get { return (self.bitmap) ? self.bitmap.width : 0;  } }
	public int height { get { return (self.bitmap) ? self.bitmap.height : 0; } }

	// Set sprite's origin to centre
	public void SetOrigin() {
		if (!@_iconBitmap) return;
		self.ox = @_iconBitmap.width / 2;
		self.oy = @_iconBitmap.height / 2;
	}

	public void SetPosition() {
		if (!@_iconBitmap) return;
		SetOrigin;
		self.z = 3;
		// Set original position
		p = Battle.Scene.BattlerPosition(@index, @sideSize);
		self.x = p[0];
		self.y = p[1];
		// Apply metrics
		@pkmn.species_data.apply_metrics_to_sprite(self, @index, true);
	}

	public void setPokemonBitmap(pkmn) {
		@pkmn = pkmn;
		@_iconBitmap&.dispose;
		@_iconBitmap = GameData.Species.shadow_bitmap_from_pokemon(@pkmn);
		self.bitmap = (@_iconBitmap) ? @_iconBitmap.bitmap : null;
		SetPosition;
	}

	public void update() {
		if (!@_iconBitmap) return;
		// Update bitmap
		@_iconBitmap.update;
		self.bitmap = @_iconBitmap.bitmap;
	}
}
