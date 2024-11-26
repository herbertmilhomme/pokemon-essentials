//===============================================================================
// Pokémon icons.
//===============================================================================
public partial class PokemonBoxIcon : IconSprite {
	public override void initialize(pokemon, viewport = null) {
		base.initialize(0, 0, viewport);
		@pokemon = pokemon;
		@release_timer_start = null;
		refresh;
	}

	public bool releasing() {
		return !@release_timer_start.null();
	}

	public void release() {
		self.ox = self.src_rect.width / 2;    // 32
		self.oy = self.src_rect.height / 2;   // 32
		self.x += self.src_rect.width / 2;    // 32
		self.y += self.src_rect.height / 2;   // 32
		@release_timer_start = System.uptime;
	}

	public void refresh() {
		if (!@pokemon) return;
		self.setBitmap(GameData.Species.icon_filename_from_pokemon(@pokemon));
		self.src_rect = new Rect(0, 0, self.bitmap.height, self.bitmap.height);
	}

	public override void update() {
		base.update();
		self.color = new Color(0, 0, 0, 0);
		if (releasing()) {
			time_now = System.uptime;
			self.zoom_x = lerp(1.0, 0.0, 1.5, @release_timer_start, System.uptime);
			self.zoom_y = self.zoom_x;
			self.opacity = lerp(255, 0, 1.5, @release_timer_start, System.uptime);
			if (self.opacity == 0) {
				@release_timer_start = null;
				dispose;
			}
		}
	}
}

//===============================================================================
// Pokémon sprite.
//===============================================================================
public partial class MosaicPokemonSprite : PokemonSprite {
	public int mosaic		{ get { return _mosaic; } }			protected int _mosaic;

	public override void initialize(*args) {
		base.initialize(*args);
		@mosaic = 0;
		@inrefresh = false;
		@mosaicbitmap = null;
		@mosaicbitmap2 = null;
		@oldbitmap = self.bitmap;
	}

	public override void dispose() {
		base.dispose();
		@mosaicbitmap&.dispose;
		@mosaicbitmap = null;
		@mosaicbitmap2&.dispose;
		@mosaicbitmap2 = null;
	}

	public override int bitmap { set {
		base.bitmap();
		mosaicRefresh(value);
		}
	}

	public int mosaic { set {
		@mosaic = value;
		if (@mosaic < 0) @mosaic = 0;
		mosaicRefresh(@oldbitmap);
		}
	}

	public void mosaicRefresh(bitmap) {
		if (@inrefresh) return;
		@inrefresh = true;
		@oldbitmap = bitmap;
		if (@mosaic <= 0 || !@oldbitmap) {
			@mosaicbitmap&.dispose;
			@mosaicbitmap = null;
			@mosaicbitmap2&.dispose;
			@mosaicbitmap2 = null;
			self.bitmap = @oldbitmap;
		} else {
			newWidth  = (int)Math.Max((@oldbitmap.width / @mosaic), 1);
			newHeight = (int)Math.Max((@oldbitmap.height / @mosaic), 1);
			@mosaicbitmap2&.dispose;
			@mosaicbitmap = DoEnsureBitmap(@mosaicbitmap, newWidth, newHeight);
			@mosaicbitmap.clear;
			@mosaicbitmap2 = DoEnsureBitmap(@mosaicbitmap2, @oldbitmap.width, @oldbitmap.height);
			@mosaicbitmap2.clear;
			@mosaicbitmap.stretch_blt(new Rect(0, 0, newWidth, newHeight), @oldbitmap, @oldbitmap.rect);
			@mosaicbitmap2.stretch_blt(
				new Rect((-@mosaic / 2) + 1, (-@mosaic / 2) + 1, @mosaicbitmap2.width, @mosaicbitmap2.height),
				@mosaicbitmap, new Rect(0, 0, newWidth, newHeight)
			);
			self.bitmap = @mosaicbitmap2;
		}
		@inrefresh = false;
	}
}

//===============================================================================
//
//===============================================================================
public partial class AutoMosaicPokemonSprite : MosaicPokemonSprite {
	public const int INITIAL_MOSAIC = 10;   // Pixellation factor

	public int mosaic { set {
		@mosaic = value;
		if (@mosaic < 0) @mosaic = 0;
		if (!@start_mosaic) @start_mosaic = @mosaic;
		}
	}

	public int mosaic_duration { set {
		@mosaic_duration = val;
		if (@mosaic_duration < 0) @mosaic_duration = 0;
		if (@mosaic_duration > 0) @mosaic_timer_start = System.uptime;
		}
	}

	public override void update() {
		base.update();
		if (@mosaic_timer_start) {
			if (!@start_mosaic || @start_mosaic == 0) @start_mosaic = INITIAL_MOSAIC;
			new_mosaic = lerp(@start_mosaic, 0, @mosaic_duration, @mosaic_timer_start, System.uptime).ToInt();
			self.mosaic = new_mosaic;
			mosaicRefresh(@oldbitmap);
			if (new_mosaic == 0) {
				@mosaic_timer_start = null;
				@start_mosaic = null;
			}
		}
	}
}

//===============================================================================
// Cursor.
//===============================================================================
public partial class PokemonBoxArrow : Sprite {
	public int quickswap		{ get { return _quickswap; } set { _quickswap = value; } }			protected int _quickswap;

	// Time in seconds for the cursor to move down and back up to grab/drop a
	// Pokémon.
	public const int GRAB_TIME = 0.4;

	public override void initialize(viewport = null) {
		base.initialize(viewport);
		@holding    = false;
		@updating   = false;
		@quickswap  = false;
		@heldpkmn   = null;
		@handsprite = new ChangelingSprite(0, 0, viewport);
		@handsprite.addBitmap("point1", "Graphics/UI/Storage/cursor_point_1");
		@handsprite.addBitmap("point2", "Graphics/UI/Storage/cursor_point_2");
		@handsprite.addBitmap("grab", "Graphics/UI/Storage/cursor_grab");
		@handsprite.addBitmap("fist", "Graphics/UI/Storage/cursor_fist");
		@handsprite.addBitmap("point1q", "Graphics/UI/Storage/cursor_point_1_q");
		@handsprite.addBitmap("point2q", "Graphics/UI/Storage/cursor_point_2_q");
		@handsprite.addBitmap("grabq", "Graphics/UI/Storage/cursor_grab_q");
		@handsprite.addBitmap("fistq", "Graphics/UI/Storage/cursor_fist_q");
		@handsprite.changeBitmap("fist");
		@spriteX = self.x;
		@spriteY = self.y;
	}

	public override void dispose() {
		@handsprite.dispose;
		@heldpkmn&.dispose;
		base.dispose();
	}

	public override int x { set {
		base.x();
		@handsprite.x = self.x;
		if (!@updating) @spriteX = x;
		if (holding()) heldPokemon.x = self.x;
		}
	}

	public override int y { set {
		base.y();
		@handsprite.y = self.y;
		if (!@updating) @spriteY = y;
		if (holding()) heldPokemon.y = self.y + 16;
		}
	}

	public override int z { set {
		base.z();
		@handsprite.z = value;
		}
	}

	public override int visible { set {
		base.visible();
		@handsprite.visible = value;
		sprite = heldPokemon;
		if (sprite) sprite.visible = value;
		}
	}

	public override int color { set {
		base.color();
		@handsprite.color = value;
		sprite = heldPokemon;
		if (sprite) sprite.color = value;
		}
	}

	public void heldPokemon() {
		if (@heldpkmn&.disposed()) @heldpkmn = null;
		if (!@heldpkmn) @holding = false;
		return @heldpkmn;
	}

	public bool holding() {
		return self.heldPokemon && @holding;
	}

	public bool grabbing() {
		return !@grabbing_timer_start.null();
	}

	public bool placing() {
		return !@placing_timer_start.null();
	}

	public void setSprite(sprite) {
		if (holding()) {
			@heldpkmn = sprite;
			if (@heldpkmn) @heldpkmn.viewport = self.viewport;
			if (@heldpkmn) @heldpkmn.z = 1;
			if (!@heldpkmn) @holding = false;
			self.z = 2;
		}
	}

	public void deleteSprite() {
		@holding = false;
		if (@heldpkmn) {
			@heldpkmn.dispose;
			@heldpkmn = null;
		}
	}

	public void grab(sprite) {
		@grabbing_timer_start = System.uptime;
		@heldpkmn = sprite;
		@heldpkmn.viewport = self.viewport;
		@heldpkmn.z = 1;
		self.z = 2;
	}

	public void place() {
		@placing_timer_start = System.uptime;
	}

	public void release() {
		@heldpkmn&.release;
	}

	public override void update() {
		@updating = true;
		base.update();
		heldpkmn = heldPokemon;
		heldpkmn&.update;
		@handsprite.update;
		if (!heldpkmn) @holding = false;
		if (@grabbing_timer_start) {
			if (System.uptime - @grabbing_timer_start <= GRAB_TIME / 2) {
				@handsprite.changeBitmap((@quickswap) ? "grabq" : "grab");
				self.y = @spriteY + lerp(0, 16, GRAB_TIME / 2, @grabbing_timer_start, System.uptime);
			} else {
				@holding = true;
				@handsprite.changeBitmap((@quickswap) ? "fistq" : "fist");
				delta_y = lerp(16, 0, GRAB_TIME / 2, @grabbing_timer_start + (GRAB_TIME / 2), System.uptime);
				self.y = @spriteY + delta_y;
				if (delta_y == 0) @grabbing_timer_start = null;
			}
		} else if (@placing_timer_start) {
			if (System.uptime - @placing_timer_start <= GRAB_TIME / 2) {
				@handsprite.changeBitmap((@quickswap) ? "fistq" : "fist");
				self.y = @spriteY + lerp(0, 16, GRAB_TIME / 2, @placing_timer_start, System.uptime);
			} else {
				@holding = false;
				@heldpkmn = null;
				@handsprite.changeBitmap((@quickswap) ? "grabq" : "grab");
				delta_y = lerp(16, 0, GRAB_TIME / 2, @placing_timer_start + (GRAB_TIME / 2), System.uptime);
				self.y = @spriteY + delta_y;
				if (delta_y == 0) @placing_timer_start = null;
			}
		} else if (holding()) {
			@handsprite.changeBitmap((@quickswap) ? "fistq" : "fist");
		} else {   // Idling
			self.x = @spriteX;
			self.y = @spriteY;
			if ((System.uptime / 0.5).ToInt().even()) {   // Changes every 0.5 seconds
				@handsprite.changeBitmap((@quickswap) ? "point1q" : "point1");
			} else {
				@handsprite.changeBitmap((@quickswap) ? "point2q" : "point2");
			}
		}
		@updating = false;
	}
}

//===============================================================================
// Box.
//===============================================================================
public partial class PokemonBoxSprite : Sprite {
	public int refreshBox		{ get { return _refreshBox; } set { _refreshBox = value; } }			protected int _refreshBox;
	public int refreshSprites		{ get { return _refreshSprites; } set { _refreshSprites = value; } }			protected int _refreshSprites;

	public override void initialize(storage, boxnumber, viewport = null) {
		base.initialize(viewport);
		@storage = storage;
		@boxnumber = boxnumber;
		@refreshBox = true;
		@refreshSprites = true;
		@pokemonsprites = new List<string>();
		for (int i = PokemonBox.BOX_SIZE; i < PokemonBox.BOX_SIZE; i++) { //for 'PokemonBox.BOX_SIZE' times do => |i|
			@pokemonsprites[i] = null;
			pokemon = @storage[boxnumber, i];
			@pokemonsprites[i] = new PokemonBoxIcon(pokemon, viewport);
		}
		@contents = new Bitmap(324, 296);
		self.bitmap = @contents;
		self.x = 184;
		self.y = 18;
		refresh;
	}

	public void dispose() {
		if (!disposed()) {
			for (int i = PokemonBox.BOX_SIZE; i < PokemonBox.BOX_SIZE; i++) { //for 'PokemonBox.BOX_SIZE' times do => |i|
				@pokemonsprites[i]&.dispose;
				@pokemonsprites[i] = null;
			}
			@boxbitmap.dispose;
			@contents.dispose;
			super;
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
		if (@refreshSprites) {
			for (int i = PokemonBox.BOX_SIZE; i < PokemonBox.BOX_SIZE; i++) { //for 'PokemonBox.BOX_SIZE' times do => |i|
				if (@pokemonsprites[i] && !@pokemonsprites[i].disposed()) {
					@pokemonsprites[i].color = value;
				}
			}
		}
		refresh;
		}
	}

	public override int visible { set {
		base.visible();
		for (int i = PokemonBox.BOX_SIZE; i < PokemonBox.BOX_SIZE; i++) { //for 'PokemonBox.BOX_SIZE' times do => |i|
			if (@pokemonsprites[i] && !@pokemonsprites[i].disposed()) {
				@pokemonsprites[i].visible = value;
			}
		}
		refresh;
		}
	}

	public void getBoxBitmap() {
		if (!@bg || @bg != @storage[@boxnumber].background) {
			curbg = @storage[@boxnumber].background;
			if (!curbg || (curbg.is_a(String) && curbg.length == 0)) {
				@bg = @boxnumber % PokemonStorage.BASICWALLPAPERQTY;
			} else {
				if (curbg.is_a(String) && System.Text.RegularExpressions.Regex.IsMatch(curbg,@"^box(\d+)$")) {
					curbg = $~[1].ToInt();
					@storage[@boxnumber].background = curbg;
				}
				@bg = curbg;
			}
			if (!@storage.isAvailableWallpaper(@bg)) {
				@bg = @boxnumber % PokemonStorage.BASICWALLPAPERQTY;
				@storage[@boxnumber].background = @bg;
			}
			@boxbitmap&.dispose;
			@boxbitmap = new AnimatedBitmap($"Graphics/UI/Storage/box_{@bg}");
		}
	}

	public void getPokemon(index) {
		return @pokemonsprites[index];
	}

	public void setPokemon(index, sprite) {
		@pokemonsprites[index] = sprite;
		@pokemonsprites[index].refresh;
		refresh;
	}

	public void grabPokemon(index, arrow) {
		sprite = @pokemonsprites[index];
		if (sprite) {
			arrow.grab(sprite);
			@pokemonsprites[index] = null;
			refresh;
		}
	}

	public void deletePokemon(index) {
		@pokemonsprites[index].dispose;
		@pokemonsprites[index] = null;
		refresh;
	}

	public void refresh() {
		if (@refreshBox) {
			boxname = @storage[@boxnumber].name;
			getBoxBitmap;
			@contents.blt(0, 0, @boxbitmap.bitmap, new Rect(0, 0, 324, 296));
			SetSystemFont(@contents);
			widthval = @contents.text_size(boxname).width;
			xval = 162 - (widthval / 2);
			DrawShadowText(@contents, xval, 14, widthval, 32,
											boxname, new Color(248, 248, 248), new Color(40, 48, 48));
			@refreshBox = false;
		}
		yval = self.y + 30;
		for (int j = PokemonBox.BOX_HEIGHT; j < PokemonBox.BOX_HEIGHT; j++) { //for 'PokemonBox.BOX_HEIGHT' times do => |j|
			xval = self.x + 10;
			for (int k = PokemonBox.BOX_WIDTH; k < PokemonBox.BOX_WIDTH; k++) { //for 'PokemonBox.BOX_WIDTH' times do => |k|
				sprite = @pokemonsprites[(j * PokemonBox.BOX_WIDTH) + k];
				if (sprite && !sprite.disposed()) {
					sprite.viewport = self.viewport;
					sprite.x = xval;
					sprite.y = yval;
					sprite.z = 1;
				}
				xval += 48;
			}
			yval += 48;
		}
	}

	public override void update() {
		base.update();
		for (int i = PokemonBox.BOX_SIZE; i < PokemonBox.BOX_SIZE; i++) { //for 'PokemonBox.BOX_SIZE' times do => |i|
			if (@pokemonsprites[i] && !@pokemonsprites[i].disposed()) {
				@pokemonsprites[i].update;
			}
		}
	}
}

//===============================================================================
// Party pop-up panel.
//===============================================================================
public partial class PokemonBoxPartySprite : Sprite {
	public override void initialize(party, viewport = null) {
		base.initialize(viewport);
		@party = party;
		@boxbitmap = new AnimatedBitmap("Graphics/UI/Storage/overlay_party");
		@pokemonsprites = new List<string>();
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			@pokemonsprites[i] = null;
			pokemon = @party[i];
			if (pokemon) @pokemonsprites[i] = new PokemonBoxIcon(pokemon, viewport);
		}
		@contents = new Bitmap(172, 352);
		self.bitmap = @contents;
		self.x = 182;
		self.y = Graphics.height - 352;
		SetSystemFont(self.bitmap);
		refresh;
	}

	public void dispose() {
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			@pokemonsprites[i]&.dispose;
		}
		@boxbitmap.dispose;
		@contents.dispose;
		super;
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
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			if (@pokemonsprites[i] && !@pokemonsprites[i].disposed()) {
				@pokemonsprites[i].color = SrcOver(@pokemonsprites[i].color, value);
			}
		}
		}
	}

	public override int visible { set {
		base.visible();
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			if (@pokemonsprites[i] && !@pokemonsprites[i].disposed()) {
				@pokemonsprites[i].visible = value;
			}
		}
		}
	}

	public void getPokemon(index) {
		return @pokemonsprites[index];
	}

	public void setPokemon(index, sprite) {
		@pokemonsprites[index] = sprite;
		@pokemonsprites.compact!;
		refresh;
	}

	public void grabPokemon(index, arrow) {
		sprite = @pokemonsprites[index];
		if (sprite) {
			arrow.grab(sprite);
			@pokemonsprites.delete_at(index);
			refresh;
		}
	}

	public void deletePokemon(index) {
		@pokemonsprites[index].dispose;
		@pokemonsprites[index] = null;
		@pokemonsprites.compact!;
		refresh;
	}

	public void refresh() {
		@contents.blt(0, 0, @boxbitmap.bitmap, new Rect(0, 0, 172, 352));
		DrawTextPositions(
			self.bitmap,
			new {_INTL("Back"), 86, 248, :center, new Color(248, 248, 248), new Color(80, 80, 80), :outline}
		);
		xvalues = new List<string>();   // new {18, 90, 18, 90, 18, 90}
		yvalues = new List<string>();   // new {2, 18, 66, 82, 130, 146}
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			xvalues.Add(18 + (72 * (i % 2)));
			yvalues.Add(2 + (16 * (i % 2)) + (64 * (i / 2)));
		}
		@pokemonsprites.delete_if(sprite => sprite&.disposed());
		@pokemonsprites.each(sprite => sprite&.refresh);
		for (int j = Settings.MAX_PARTY_SIZE; j < Settings.MAX_PARTY_SIZE; j++) { //for 'Settings.MAX_PARTY_SIZE' times do => |j|
			sprite = @pokemonsprites[j];
			if (sprite.null() || sprite.disposed()) continue;
			sprite.viewport = self.viewport;
			sprite.x = self.x + xvalues[j];
			sprite.y = self.y + yvalues[j];
			sprite.z = 1;
		}
	}

	public override void update() {
		base.update();
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			if (@pokemonsprites[i] && !@pokemonsprites[i].disposed()) @pokemonsprites[i].update;
		}
	}
}

//===============================================================================
// Pokémon storage visuals.
//===============================================================================
public partial class PokemonStorageScene {
	public int quickswap		{ get { return _quickswap; } }			protected int _quickswap;

	public const int MARK_WIDTH  = 16;
	public const int MARK_HEIGHT = 16;

	public void initialize() {
		@command = 1;
	}

	public void StartBox(screen, command) {
		@screen = screen;
		@storage = screen.storage;
		@bgviewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@bgviewport.z = 99999;
		@boxviewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@boxviewport.z = 99999;
		@boxsidesviewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@boxsidesviewport.z = 99999;
		@arrowviewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@arrowviewport.z = 99999;
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@selection = 0;
		@quickswap = false;
		@sprites = new List<string>();
		@choseFromParty = false;
		@command = command;
		addBackgroundPlane(@sprites, "background", "Storage/bg", @bgviewport);
		@sprites["box"] = new PokemonBoxSprite(@storage, @storage.currentBox, @boxviewport);
		@sprites["boxsides"] = new IconSprite(0, 0, @boxsidesviewport);
		@sprites["boxsides"].setBitmap("Graphics/UI/Storage/overlay_main");
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @boxsidesviewport);
		SetSystemFont(@sprites["overlay"].bitmap);
		@sprites["pokemon"] = new AutoMosaicPokemonSprite(@boxsidesviewport);
		@sprites["pokemon"].setOffset(PictureOrigin.CENTER);
		@sprites["pokemon"].x = 90;
		@sprites["pokemon"].y = 134;
		@sprites["boxparty"] = new PokemonBoxPartySprite(@storage.party, @boxsidesviewport);
		if (command != 2) {   // Drop down tab only on Deposit
			@sprites["boxparty"].x = 182;
			@sprites["boxparty"].y = Graphics.height;
		}
		@markingbitmap = new AnimatedBitmap("Graphics/UI/Storage/markings");
		@sprites["markingbg"] = new IconSprite(292, 68, @boxsidesviewport);
		@sprites["markingbg"].setBitmap("Graphics/UI/Storage/overlay_marking");
		@sprites["markingbg"].z = 10;
		@sprites["markingbg"].visible = false;
		@sprites["markingoverlay"] = new BitmapSprite(Graphics.width, Graphics.height, @boxsidesviewport);
		@sprites["markingoverlay"].z = 11;
		@sprites["markingoverlay"].visible = false;
		SetSystemFont(@sprites["markingoverlay"].bitmap);
		@sprites["arrow"] = new PokemonBoxArrow(@arrowviewport);
		@sprites["arrow"].z += 1;
		if (command == 2) {
			PartySetArrow(@sprites["arrow"], @selection);
			UpdateOverlay(@selection, @storage.party);
		} else {
			SetArrow(@sprites["arrow"], @selection);
			UpdateOverlay(@selection);
		}
		SetMosaic(@selection);
		SEPlay("PC access");
		FadeInAndShow(@sprites);
	}

	public void CloseBox() {
		FadeOutAndHide(@sprites);
		DisposeSpriteHash(@sprites);
		@markingbitmap&.dispose;
		@boxviewport.dispose;
		@boxsidesviewport.dispose;
		@arrowviewport.dispose;
	}

	public void Display(message) {
		msgwindow = Window_UnformattedTextPokemon.newWithSize("", 180, 0, Graphics.width - 180, 32);
		msgwindow.viewport       = @viewport;
		msgwindow.visible        = true;
		msgwindow.letterbyletter = false;
		msgwindow.resizeHeightToFit(message, Graphics.width - 180);
		msgwindow.text = message;
		BottomRight(msgwindow);
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			if (Input.trigger(Input.BACK) || Input.trigger(Input.USE)) {
				break;
			}
			msgwindow.update;
			self.update;
		}
		msgwindow.dispose;
		Input.update;
	}

	public void ShowCommands(message, commands, index = 0) {
		ret = -1;
		msgwindow = Window_UnformattedTextPokemon.newWithSize("", 180, 0, Graphics.width - 180, 32);
		msgwindow.viewport       = @viewport;
		msgwindow.visible        = true;
		msgwindow.letterbyletter = false;
		msgwindow.text           = message;
		msgwindow.resizeHeightToFit(message, Graphics.width - 180);
		BottomRight(msgwindow);
		cmdwindow = new Window_CommandPokemon(commands);
		cmdwindow.viewport = @viewport;
		cmdwindow.visible  = true;
		cmdwindow.resizeToFit(cmdwindow.commands);
		if (cmdwindow.height > Graphics.height - msgwindow.height) cmdwindow.height = Graphics.height - msgwindow.height;
		BottomRight(cmdwindow);
		cmdwindow.y -= msgwindow.height;
		cmdwindow.index = index;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			msgwindow.update;
			cmdwindow.update;
			if (Input.trigger(Input.BACK)) {
				ret = -1;
				break;
			} else if (Input.trigger(Input.USE)) {
				ret = cmdwindow.index;
				break;
			}
			self.update;
		}
		msgwindow.dispose;
		cmdwindow.dispose;
		Input.update;
		return ret;
	}

	public void SetArrow(arrow, selection) {
		switch (selection) {
			case -1: case -4: case -5:   // Box name, move left, move right
				arrow.x = 314;
				arrow.y = -24;
				break;
			case -2:   // Party Pokémon
				arrow.x = 238;
				arrow.y = 278;
				break;
			case -3:   // Close Box
				arrow.x = 414;
				arrow.y = 278;
				break;
			default:
				arrow.x = (97 + (24 * (selection % PokemonBox.BOX_WIDTH))) * 2;
				arrow.y = (8 + (24 * (selection / PokemonBox.BOX_WIDTH))) * 2;
				break;
		}
	}

	public void ChangeSelection(key, selection) {
		switch (key) {
			case Input.UP:
				switch (selection) {
					case -1:   // Box name
						selection = -2;
						break;
					case -2:   // Party
						selection = PokemonBox.BOX_SIZE - 1 - (PokemonBox.BOX_WIDTH * 2 / 3);   // 25
						break;
					case -3:   // Close Box
						selection = PokemonBox.BOX_SIZE - (PokemonBox.BOX_WIDTH / 3);   // 28
						break;
					default:
						selection -= PokemonBox.BOX_WIDTH;
						if (selection < 0) selection = -1;
						break;
				}
				break;
			case Input.DOWN:
				switch (selection) {
					case -1:   // Box name
						selection = PokemonBox.BOX_WIDTH / 3;   // 2
						break;
					case -2:   // Party
						selection = -1;
						break;
					case -3:   // Close Box
						selection = -1;
						break;
					default:
						selection += PokemonBox.BOX_WIDTH;
						if (selection >= PokemonBox.BOX_SIZE) {
							if (selection < PokemonBox.BOX_SIZE + (PokemonBox.BOX_WIDTH / 2)) {
								selection = -2;   // Party
							} else {
								selection = -3;   // Close Box
							}
						}
						break;
				}
				break;
			case Input.LEFT:
				if (selection == -1) {   // Box name
					selection = -4;   // Move to previous box
				} else if (selection == -2) {
					selection = -3;
				} else if (selection == -3) {
					selection = -2;
				} else if ((selection % PokemonBox.BOX_WIDTH) == 0) {   // Wrap around
					selection += PokemonBox.BOX_WIDTH - 1;
				} else {
					selection -= 1;
				}
				break;
			case Input.RIGHT:
				if (selection == -1) {   // Box name
					selection = -5;   // Move to next box
				} else if (selection == -2) {
					selection = -3;
				} else if (selection == -3) {
					selection = -2;
				} else if ((selection % PokemonBox.BOX_WIDTH) == PokemonBox.BOX_WIDTH - 1) {   // Wrap around
					selection -= PokemonBox.BOX_WIDTH - 1;
				} else {
					selection += 1;
				}
				break;
		}
		return selection;
	}

	public void PartySetArrow(arrow, selection) {
		if (selection < 0) return;
		xvalues = new List<string>();   // new {200, 272, 200, 272, 200, 272, 236}
		yvalues = new List<string>();   // new {2, 18, 66, 82, 130, 146, 220}
		for (int i = Settings.MAX_PARTY_SIZE; i < Settings.MAX_PARTY_SIZE; i++) { //for 'Settings.MAX_PARTY_SIZE' times do => |i|
			xvalues.Add(200 + (72 * (i % 2)));
			yvalues.Add(2 + (16 * (i % 2)) + (64 * (i / 2)));
		}
		xvalues.Add(236);
		yvalues.Add(220);
		arrow.angle = 0;
		arrow.mirror = false;
		arrow.ox = 0;
		arrow.oy = 0;
		arrow.x = xvalues[selection];
		arrow.y = yvalues[selection];
	}

	public void PartyChangeSelection(key, selection) {
		switch (key) {
			case Input.LEFT:
				selection -= 1;
				if (selection < 0) selection = Settings.MAX_PARTY_SIZE;
				break;
			case Input.RIGHT:
				selection += 1;
				if (selection > Settings.MAX_PARTY_SIZE) selection = 0;
				break;
			case Input.UP:
				if (selection == Settings.MAX_PARTY_SIZE) {
					selection = Settings.MAX_PARTY_SIZE - 1;
				} else {
					selection -= 2;
					if (selection < 0) selection = Settings.MAX_PARTY_SIZE;
				}
				break;
			case Input.DOWN:
				if (selection == Settings.MAX_PARTY_SIZE) {
					selection = 0;
				} else {
					selection += 2;
					if (selection > Settings.MAX_PARTY_SIZE) selection = Settings.MAX_PARTY_SIZE;
				}
				break;
		}
		return selection;
	}

	public void SelectBoxInternal(_party) {
		selection = @selection;
		SetArrow(@sprites["arrow"], selection);
		UpdateOverlay(selection);
		SetMosaic(selection);
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			key = -1;
			if (Input.repeat(Input.DOWN)) key = Input.DOWN;
			if (Input.repeat(Input.RIGHT)) key = Input.RIGHT;
			if (Input.repeat(Input.LEFT)) key = Input.LEFT;
			if (Input.repeat(Input.UP)) key = Input.UP;
			if (key >= 0) {
				PlayCursorSE;
				selection = ChangeSelection(key, selection);
				SetArrow(@sprites["arrow"], selection);
				switch (selection) {
					case -4:
						nextbox = (@storage.currentBox + @storage.maxBoxes - 1) % @storage.maxBoxes;
						SwitchBoxToLeft(nextbox);
						@storage.currentBox = nextbox;
						break;
					case -5:
						nextbox = (@storage.currentBox + 1) % @storage.maxBoxes;
						SwitchBoxToRight(nextbox);
						@storage.currentBox = nextbox;
						break;
				}
				if (new []{-4, -5}.Contains(selection)) selection = -1;
				UpdateOverlay(selection);
				SetMosaic(selection);
			}
			self.update;
			if (Input.trigger(Input.JUMPUP)) {
				PlayCursorSE;
				nextbox = (@storage.currentBox + @storage.maxBoxes - 1) % @storage.maxBoxes;
				SwitchBoxToLeft(nextbox);
				@storage.currentBox = nextbox;
				UpdateOverlay(selection);
				SetMosaic(selection);
			} else if (Input.trigger(Input.JUMPDOWN)) {
				PlayCursorSE;
				nextbox = (@storage.currentBox + 1) % @storage.maxBoxes;
				SwitchBoxToRight(nextbox);
				@storage.currentBox = nextbox;
				UpdateOverlay(selection);
				SetMosaic(selection);
			} else if (Input.trigger(Input.SPECIAL)) {   // Jump to box name
				if (selection != -1) {
					PlayCursorSE;
					selection = -1;
					SetArrow(@sprites["arrow"], selection);
					UpdateOverlay(selection);
					SetMosaic(selection);
				}
			} else if (Input.trigger(Input.ACTION) && @command == 0) {   // Organize only
				PlayDecisionSE;
				SetQuickSwap(!@quickswap);
			} else if (Input.trigger(Input.BACK)) {
				@selection = selection;
				return null;
			} else if (Input.trigger(Input.USE)) {
				@selection = selection;
				if (selection >= 0) {
					return new {@storage.currentBox, selection};
				} else if (selection == -1) {   // Box name
					return new {-4, -1};
				} else if (selection == -2) {   // Party Pokémon
					return new {-2, -1};
				} else if (selection == -3) {   // Close Box
					return new {-3, -1};
				}
			}
		}
	}

	public void SelectBox(party) {
		if (@command == 1) return SelectBoxInternal(party);   // Withdraw
		ret = null;
		do { //loop; while (true);
			if (!@choseFromParty) ret = SelectBoxInternal(party);
			if (@choseFromParty || (ret && ret[0] == -2)) {   // Party Pokémon
				if (!@choseFromParty) {
					ShowPartyTab;
					@selection = 0;
				}
				ret = SelectPartyInternal(party, false);
				if (ret < 0) {
					HidePartyTab;
					@selection = -2;
					@choseFromParty = false;
				} else {
					@choseFromParty = true;
					return new {-1, ret};
				}
			} else {
				@choseFromParty = false;
				return ret;
			}
		}
	}

	public void SelectPartyInternal(party, depositing) {
		selection = @selection;
		PartySetArrow(@sprites["arrow"], selection);
		UpdateOverlay(selection, party);
		SetMosaic(selection);
		lastsel = 1;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			key = -1;
			if (Input.repeat(Input.DOWN)) key = Input.DOWN;
			if (Input.repeat(Input.RIGHT)) key = Input.RIGHT;
			if (Input.repeat(Input.LEFT)) key = Input.LEFT;
			if (Input.repeat(Input.UP)) key = Input.UP;
			if (key >= 0) {
				PlayCursorSE;
				newselection = PartyChangeSelection(key, selection);
				switch (newselection) {
					case -1:
						if (!depositing) return -1;
						break;
					case -2:
						selection = lastsel;
						break;
					default:
						selection = newselection;
						break;
				}
				PartySetArrow(@sprites["arrow"], selection);
				if (selection > 0) lastsel = selection;
				UpdateOverlay(selection, party);
				SetMosaic(selection);
			}
			self.update;
			if (Input.trigger(Input.ACTION) && @command == 0) {   // Organize only
				PlayDecisionSE;
				SetQuickSwap(!@quickswap);
			} else if (Input.trigger(Input.BACK)) {
				@selection = selection;
				return -1;
			} else if (Input.trigger(Input.USE)) {
				if (selection >= 0 && selection < Settings.MAX_PARTY_SIZE) {
					@selection = selection;
					return selection;
				} else if (selection == Settings.MAX_PARTY_SIZE) {   // Close Box
					@selection = selection;
					return (depositing) ? -3 : -1;
				}
			}
		}
	}

	public void SelectParty(party) {
		return SelectPartyInternal(party, true);
	}

	public void ChangeBackground(wp) {
		duration = 0.2;   // Time in seconds to fade out or fade in
		@sprites["box"].refreshSprites = false;
		Graphics.update;
		self.update;
		// Fade old background to white
		timer_start = System.uptime;
		do { //loop; while (true);
			alpha = lerp(0, 255, duration, timer_start, System.uptime);
			@sprites["box"].color = new Color(248, 248, 248, alpha);
			Graphics.update;
			self.update;
			if (alpha >= 255) break;
		}
		// Fade in new background from white
		@sprites["box"].refreshBox = true;
		@storage[@storage.currentBox].background = wp;
		timer_start = System.uptime;
		do { //loop; while (true);
			alpha = lerp(255, 0, duration, timer_start, System.uptime);
			@sprites["box"].color = new Color(248, 248, 248, alpha);
			Graphics.update;
			self.update;
			if (alpha <= 0) break;
		}
		@sprites["box"].refreshSprites = true;
		Input.update;
	}

	public void SwitchBoxToRight(new_box_number) {
		start_x = @sprites["box"].x;
		newbox = new PokemonBoxSprite(@storage, new_box_number, @boxviewport);
		newbox.x = start_x + 336;
		timer_start = System.uptime;
		do { //loop; while (true);
			@sprites["box"].x = lerp(start_x, start_x - 336, 0.25, timer_start, System.uptime);
			newbox.x = @sprites["box"].x + 336;
			self.update;
			Graphics.update;
			if (newbox.x == start_x) break;
		}
		@sprites["box"].dispose;
		@sprites["box"] = newbox;
		Input.update;
	}

	public void SwitchBoxToLeft(new_box_number) {
		start_x = @sprites["box"].x;
		newbox = new PokemonBoxSprite(@storage, new_box_number, @boxviewport);
		newbox.x = start_x - 336;
		timer_start = System.uptime;
		do { //loop; while (true);
			@sprites["box"].x = lerp(start_x, start_x + 336, 0.25, timer_start, System.uptime);
			newbox.x = @sprites["box"].x - 336;
			self.update;
			Graphics.update;
			if (newbox.x == start_x) break;
		}
		@sprites["box"].dispose;
		@sprites["box"] = newbox;
		Input.update;
	}

	public void JumpToBox(newbox) {
		if (@storage.currentBox == newbox) return;
		if (newbox > @storage.currentBox) {
			SwitchBoxToRight(newbox);
		} else {
			SwitchBoxToLeft(newbox);
		}
		@storage.currentBox = newbox;
	}

	public void SetMosaic(selection) {
		if (@screen.HeldPokemon) return;
		if (@boxForMosaic == @storage.currentBox && @selectionForMosaic == selection) return;
		@sprites["pokemon"].mosaic_duration = 0.25;   // In seconds
		@boxForMosaic = @storage.currentBox;
		@selectionForMosaic = selection;
	}

	public void SetQuickSwap(value) {
		@quickswap = value;
		@sprites["arrow"].quickswap = value;
	}

	public void ShowPartyTab() {
		@sprites["arrow"].visible = false;
		if (!@screen.HeldPokemon) {
			UpdateOverlay(-1);
			SetMosaic(-1);
		}
		SEPlay("GUI storage show party panel");
		start_y = @sprites["boxparty"].y;   // Graphics.height
		timer_start = System.uptime;
		do { //loop; while (true);
			@sprites["boxparty"].y = lerp(start_y, start_y - @sprites["boxparty"].height,
																		0.4, timer_start, System.uptime);
			self.update;
			Graphics.update;
			if (@sprites["boxparty"].y == start_y - @sprites["boxparty"].height) break;
		}
		Input.update;
		@sprites["arrow"].visible = true;
	}

	public void HidePartyTab() {
		@sprites["arrow"].visible = false;
		if (!@screen.HeldPokemon) {
			UpdateOverlay(-1);
			SetMosaic(-1);
		}
		SEPlay("GUI storage hide party panel");
		start_y = @sprites["boxparty"].y;   // Graphics.height - @sprites["boxparty"].height
		timer_start = System.uptime;
		do { //loop; while (true);
			@sprites["boxparty"].y = lerp(start_y, start_y + @sprites["boxparty"].height,
																		0.4, timer_start, System.uptime);
			self.update;
			Graphics.update;
			if (@sprites["boxparty"].y == start_y + @sprites["boxparty"].height) break;
		}
		Input.update;
		@sprites["arrow"].visible = true;
	}

	public void Hold(selected) {
		SEPlay("GUI storage pick up");
		if (selected[0] == -1) {
			@sprites["boxparty"].grabPokemon(selected[1], @sprites["arrow"]);
		} else {
			@sprites["box"].grabPokemon(selected[1], @sprites["arrow"]);
		}
		while (@sprites["arrow"].grabbing()) {
			Graphics.update;
			Input.update;
			self.update;
		}
	}

	public void Swap(selected, _heldpoke) {
		SEPlay("GUI storage pick up");
		heldpokesprite = @sprites["arrow"].heldPokemon;
		boxpokesprite = null;
		if (selected[0] == -1) {
			boxpokesprite = @sprites["boxparty"].getPokemon(selected[1]);
		} else {
			boxpokesprite = @sprites["box"].getPokemon(selected[1]);
		}
		if (selected[0] == -1) {
			@sprites["boxparty"].setPokemon(selected[1], heldpokesprite);
		} else {
			@sprites["box"].setPokemon(selected[1], heldpokesprite);
		}
		@sprites["arrow"].setSprite(boxpokesprite);
		@sprites["pokemon"].mosaic_duration = 0.25;   // In seconds
		@boxForMosaic = @storage.currentBox;
		@selectionForMosaic = selected[1];
	}

	public void Place(selected, _heldpoke) {
		SEPlay("GUI storage put down");
		heldpokesprite = @sprites["arrow"].heldPokemon;
		@sprites["arrow"].place;
		while (@sprites["arrow"].placing()) {
			Graphics.update;
			Input.update;
			self.update;
		}
		if (selected[0] == -1) {
			@sprites["boxparty"].setPokemon(selected[1], heldpokesprite);
		} else {
			@sprites["box"].setPokemon(selected[1], heldpokesprite);
		}
		@boxForMosaic = @storage.currentBox;
		@selectionForMosaic = selected[1];
	}

	public void Withdraw(selected, heldpoke, partyindex) {
		if (!heldpoke) Hold(selected);
		ShowPartyTab;
		PartySetArrow(@sprites["arrow"], partyindex);
		Place(new {-1, partyindex}, heldpoke);
		HidePartyTab;
	}

	public void Store(selected, heldpoke, destbox, firstfree) {
		if (heldpoke) {
			if (destbox == @storage.currentBox) {
				heldpokesprite = @sprites["arrow"].heldPokemon;
				@sprites["box"].setPokemon(firstfree, heldpokesprite);
				@sprites["arrow"].setSprite(null);
			} else {
				@sprites["arrow"].deleteSprite;
			}
		} else {
			sprite = @sprites["boxparty"].getPokemon(selected[1]);
			if (destbox == @storage.currentBox) {
				@sprites["box"].setPokemon(firstfree, sprite);
				@sprites["boxparty"].setPokemon(selected[1], null);
			} else {
				@sprites["boxparty"].deletePokemon(selected[1]);
			}
		}
	}

	public void Release(selected, heldpoke) {
		box = selected[0];
		index = selected[1];
		if (heldpoke) {
			sprite = @sprites["arrow"].heldPokemon;
		} else if (box == -1) {
			sprite = @sprites["boxparty"].getPokemon(index);
		} else {
			sprite = @sprites["box"].getPokemon(index);
		}
		if (sprite) {
			sprite.release;
			while (sprite.releasing()) {
				Graphics.update;
				sprite.update;
				self.update;
			}
		}
	}

	public void ChooseBox(msg) {
		commands = new List<string>();
		for (int i = @storage.maxBoxes; i < @storage.maxBoxes; i++) { //for '@storage.maxBoxes' times do => |i|
			box = @storage[i];
			if (box) {
				commands.Add(_INTL("{1} ({2}/{3})", box.name, box.nitems, box.length));
			}
		}
		return ShowCommands(msg, commands, @storage.currentBox);
	}

	public void BoxName(helptext, minchars, maxchars) {
		oldsprites = FadeOutAndHide(@sprites);
		ret = EnterBoxName(helptext, minchars, maxchars);
		if (ret.length > 0) @storage[@storage.currentBox].name = ret;
		@sprites["box"].refreshBox = true;
		Refresh;
		FadeInAndShow(@sprites, oldsprites);
	}

	public void ChooseItem(bag) {
		ret = null;
		FadeOutIn do;
			scene = new PokemonBag_Scene();
			screen = new PokemonBagScreen(scene, bag);
			ret = screen.ChooseItemScreen(block: (item) => { GameData.Item.get(item).can_hold() });
		}
		return ret;
	}

	public void Summary(selected, heldpoke) {
		oldsprites = FadeOutAndHide(@sprites);
		scene = new PokemonSummary_Scene();
		screen = new PokemonSummaryScreen(scene);
		if (heldpoke) {
			screen.StartScreen([heldpoke], 0);
		} else if (selected[0] == -1) {
			@selection = screen.StartScreen(@storage.party, selected[1]);
			PartySetArrow(@sprites["arrow"], @selection);
			UpdateOverlay(@selection, @storage.party);
		} else {
			@selection = screen.StartScreen(@storage.boxes[selected[0]], selected[1]);
			SetArrow(@sprites["arrow"], @selection);
			UpdateOverlay(@selection);
		}
		FadeInAndShow(@sprites, oldsprites);
	}

	public void MarkingSetArrow(arrow, selection) {
		if (selection >= 0) {
			xvalues = new {162, 191, 220, 162, 191, 220, 184, 184};
			yvalues = new {24, 24, 24, 49, 49, 49, 77, 109};
			arrow.angle = 0;
			arrow.mirror = false;
			arrow.ox = 0;
			arrow.oy = 0;
			arrow.x = xvalues[selection] * 2;
			arrow.y = yvalues[selection] * 2;
		}
	}

	public void MarkingChangeSelection(key, selection) {
		switch (key) {
			case Input.LEFT:
				if (selection < 6) {
					selection -= 1;
					if (selection % 3 == 2) selection += 3;
				}
				break;
			case Input.RIGHT:
				if (selection < 6) {
					selection += 1;
					if (selection % 3 == 0) selection -= 3;
				}
				break;
			case Input.UP:
				if (selection == 7) {
					selection = 6;
				} else if (selection == 6) {
					selection = 4;
				} else if (selection < 3) {
					selection = 7;
				} else {
					selection -= 3;
				}
				break;
			case Input.DOWN:
				if (selection == 7) {
					selection = 1;
				} else if (selection == 6) {
					selection = 7;
				} else if (selection >= 3) {
					selection = 6;
				} else {
					selection += 3;
				}
				break;
		}
		return selection;
	}

	public void Mark(selected, heldpoke) {
		@sprites["markingbg"].visible      = true;
		@sprites["markingoverlay"].visible = true;
		msg = _INTL("Mark your Pokémon.");
		msgwindow = Window_UnformattedTextPokemon.newWithSize("", 180, 0, Graphics.width - 180, 32);
		msgwindow.viewport       = @viewport;
		msgwindow.visible        = true;
		msgwindow.letterbyletter = false;
		msgwindow.text           = msg;
		msgwindow.resizeHeightToFit(msg, Graphics.width - 180);
		BottomRight(msgwindow);
		base   = new Color(248, 248, 248);
		shadow = new Color(80, 80, 80);
		pokemon = heldpoke;
		if (heldpoke) {
			pokemon = heldpoke;
		} else if (selected[0] == -1) {
			pokemon = @storage.party[selected[1]];
		} else {
			pokemon = @storage.boxes[selected[0]][selected[1]];
		}
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
					@sprites["markingoverlay"].bitmap.blt(336 + (58 * (i % 3)), 106 + (50 * (i / 3)),
																								@markingbitmap.bitmap, markrect);
				}
				textpos = new {
					new {_INTL("OK"), 402, 216, :center, base, shadow, :outline},
					new {_INTL("Cancel"), 402, 280, :center, base, shadow, :outline}
				}
				DrawTextPositions(@sprites["markingoverlay"].bitmap, textpos);
				MarkingSetArrow(@sprites["arrow"], index);
				redraw = false;
			}
			Graphics.update;
			Input.update;
			key = -1;
			if (Input.repeat(Input.DOWN)) key = Input.DOWN;
			if (Input.repeat(Input.RIGHT)) key = Input.RIGHT;
			if (Input.repeat(Input.LEFT)) key = Input.LEFT;
			if (Input.repeat(Input.UP)) key = Input.UP;
			if (key >= 0) {
				oldindex = index;
				index = MarkingChangeSelection(key, index);
				if (index != oldindex) PlayCursorSE;
				MarkingSetArrow(@sprites["arrow"], index);
			}
			self.update;
			if (Input.trigger(Input.BACK)) {
				PlayCancelSE
				break;
			} else if (Input.trigger(Input.USE)) {
				PlayDecisionSE;
				switch (index) {
					case 6:   // OK
						pokemon.markings = markings;
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
			}
		}
		@sprites["markingbg"].visible      = false;
		@sprites["markingoverlay"].visible = false;
		msgwindow.dispose;
	}

	public void Refresh() {
		@sprites["box"].refresh;
		@sprites["boxparty"].refresh;
	}

	public void HardRefresh() {
		oldPartyY = @sprites["boxparty"].y;
		@sprites["box"].dispose;
		@sprites["box"] = new PokemonBoxSprite(@storage, @storage.currentBox, @boxviewport);
		@sprites["boxparty"].dispose;
		@sprites["boxparty"] = new PokemonBoxPartySprite(@storage.party, @boxsidesviewport);
		@sprites["boxparty"].y = oldPartyY;
	}

	public void drawMarkings(bitmap, x, y, _width, _height, markings) {
		mark_variants = @markingbitmap.bitmap.height / MARK_HEIGHT;
		markrect = new Rect(0, 0, MARK_WIDTH, MARK_HEIGHT);
		for (int i = (@markingbitmap.bitmap.width / MARK_WIDTH); i < (@markingbitmap.bitmap.width / MARK_WIDTH); i++) { //for '(@markingbitmap.bitmap.width / MARK_WIDTH)' times do => |i|
			markrect.x = i * MARK_WIDTH;
			markrect.y = (int)Math.Min((markings[i] || 0), mark_variants - 1) * MARK_HEIGHT;
			bitmap.blt(x + (i * MARK_WIDTH), y, @markingbitmap.bitmap, markrect);
		}
	}

	public void UpdateOverlay(selection, party = null) {
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		buttonbase = new Color(248, 248, 248);
		buttonshadow = new Color(80, 80, 80);
		DrawTextPositions(
			overlay,
			new {_INTL("Party: {1}", (@storage.party.length rescue 0)), 270, 334, :center, buttonbase, buttonshadow, :outline},
			new {_INTL("Exit"), 446, 334, :center, buttonbase, buttonshadow, :outline}
		);
		pokemon = null;
		if (@screen.HeldPokemon) {
			pokemon = @screen.HeldPokemon;
		} else if (selection >= 0) {
			pokemon = (party) ? party[selection] : @storagenew {@storage.currentBox, selection};
		}
		if (!pokemon) {
			@sprites["pokemon"].visible = false;
			return;
		}
		@sprites["pokemon"].visible = true;
		base   = new Color(88, 88, 80);
		shadow = new Color(168, 184, 184);
		nonbase   = new Color(208, 208, 208);
		nonshadow = new Color(224, 224, 224);
		pokename = pokemon.name;
		textstrings = new {
			new {pokename, 10, 14, :left, base, shadow};
		}
		if (!pokemon.egg()) {
			imagepos = new List<string>();
			if (pokemon.male()) {
				textstrings.Add(new {_INTL("♂"), 148, 14, :left, new Color(24, 112, 216), new Color(136, 168, 208)});
			} else if (pokemon.female()) {
				textstrings.Add(new {_INTL("♀"), 148, 14, :left, new Color(248, 56, 32), new Color(224, 152, 144)});
			}
			imagepos.Add(new {_INTL("Graphics/UI/Storage/overlay_lv"), 6, 246});
			textstrings.Add(new {pokemon.level.ToString(), 28, 240, :left, base, shadow});
			if (pokemon.ability) {
				textstrings.Add(new {pokemon.ability.name, 86, 312, :center, base, shadow});
			} else {
				textstrings.Add(new {_INTL("No ability"), 86, 312, :center, nonbase, nonshadow});
			}
			if (pokemon.item) {
				textstrings.Add(new {pokemon.item.name, 86, 348, :center, base, shadow});
			} else {
				textstrings.Add(new {_INTL("No item"), 86, 348, :center, nonbase, nonshadow});
			}
			if (pokemon.shiny()) imagepos.Add(new {"Graphics/UI/shiny", 156, 198});
			typebitmap = new AnimatedBitmap(_INTL("Graphics/UI/types"));
			pokemon.types.each_with_index do |type, i|
				type_number = GameData.Type.get(type).icon_position;
				type_rect = new Rect(0, type_number * GameData.Type.ICON_SIZE[1], *GameData.Type.ICON_SIZE);
				type_x = (pokemon.types.length == 1) ? 52 : 18 + ((GameData.Type.ICON_SIZE[0] + 6) * i);
				overlay.blt(type_x, 272, typebitmap.bitmap, type_rect);
			}
			typebitmap.dispose;
			drawMarkings(overlay, 70, 240, 128, 20, pokemon.markings);
			DrawImagePositions(overlay, imagepos);
		}
		DrawTextPositions(overlay, textstrings);
		@sprites["pokemon"].setPokemonBitmap(pokemon);
	}

	public void update() {
		UpdateSpriteHash(@sprites);
	}
}

//===============================================================================
// Pokémon storage mechanics.
//===============================================================================
public partial class PokemonStorageScreen {
	public int scene		{ get { return _scene; } }			protected int _scene;
	public int storage		{ get { return _storage; } }			protected int _storage;
	public int heldpkmn		{ get { return _heldpkmn; } set { _heldpkmn = value; } }			protected int _heldpkmn;

	public void initialize(scene, storage) {
		@scene = scene;
		@storage = storage;
		@HeldPokemon = null;
	}

	public void StartScreen(command) {
		Game.GameData.game_temp.in_storage = true;
		@heldpkmn = null;
		switch (command) {
			case 0:   // Organise
				@scene.StartBox(self, command);
				do { //loop; while (true);
					selected = @scene.SelectBox(@storage.party);
					if (selected.null()) {
						if (HeldPokemon) {
							Display(_INTL("You're holding a Pokémon!"));
							continue;
						}
						if (Confirm(_INTL("Continue Box operations?"))) continue;
						break;
					} else if (selected[0] == -3) {   // Close box
						if (HeldPokemon) {
							Display(_INTL("You're holding a Pokémon!"));
							continue;
						}
						if (Confirm(_INTL("Exit from the Box?"))) {
							SEPlay("PC close");
							break;
						}
						continue;
					} else if (selected[0] == -4) {   // Box name
						BoxCommands;
					} else {
						pokemon = @storage[selected[0], selected[1]];
						heldpoke = HeldPokemon;
						if (!pokemon && !heldpoke) continue;
						if (@scene.quickswap) {
							if (@heldpkmn) {
								(pokemon) ? Swap(selected) : Place(selected)
							} else {
								Hold(selected);
							}
						} else {
							commands = new List<string>();
							cmdMove     = -1;
							cmdSummary  = -1;
							cmdWithdraw = -1;
							cmdItem     = -1;
							cmdMark     = -1;
							cmdRelease  = -1;
							cmdDebug    = -1;
							if (heldpoke) {
								helptext = _INTL("{1} is selected.", heldpoke.name);
								commands[cmdMove = commands.length] = (pokemon) ? _INTL("Shift") : _INTL("Place");
							} else if (pokemon) {
								helptext = _INTL("{1} is selected.", pokemon.name);
								commands[cmdMove = commands.length] = _INTL("Move");
							}
							commands[cmdSummary = commands.length]  = _INTL("Summary");
							commands[cmdWithdraw = commands.length] = (selected[0] == -1) ? _INTL("Store") : _INTL("Withdraw");
							commands[cmdItem = commands.length]     = _INTL("Item");
							commands[cmdMark = commands.length]     = _INTL("Mark");
							commands[cmdRelease = commands.length]  = _INTL("Release");
							if (Core.DEBUG) commands[cmdDebug = commands.length]    = _INTL("Debug");
							commands[commands.length]               = _INTL("Cancel");
							command = ShowCommands(helptext, commands);
							if (cmdMove >= 0 && command == cmdMove) {   // Move/Shift/Place
								if (@heldpkmn) {
									(pokemon) ? Swap(selected) : Place(selected)
								} else {
									Hold(selected);
								}
							} else if (cmdSummary >= 0 && command == cmdSummary) {   // Summary
								Summary(selected, @heldpkmn);
							} else if (cmdWithdraw >= 0 && command == cmdWithdraw) {   // Store/Withdraw
								(selected[0] == -1) ? Store(selected, @heldpkmn) : Withdraw(selected, @heldpkmn)
							} else if (cmdItem >= 0 && command == cmdItem) {   // Item
								Item(selected, @heldpkmn);
							} else if (cmdMark >= 0 && command == cmdMark) {   // Mark
								Mark(selected, @heldpkmn);
							} else if (cmdRelease >= 0 && command == cmdRelease) {   // Release
								Release(selected, @heldpkmn);
							} else if (cmdDebug >= 0 && command == cmdDebug) {   // Debug
								PokemonDebug((@heldpkmn) ? @heldpkmn : pokemon, selected, heldpoke);
							}
						}
					}
				}
				@scene.CloseBox;
				break;
			case 1:   // Withdraw
				@scene.StartBox(self, command);
				do { //loop; while (true);
					selected = @scene.SelectBox(@storage.party);
					if (selected.null()) {
						if (Confirm(_INTL("Continue Box operations?"))) continue;
						break;
					} else {
						switch (selected[0]) {
							case -2:   // Party Pokémon
								Display(_INTL("Which one will you take?"));
								continue;
								break;
							case -3:   // Close box
								if (Confirm(_INTL("Exit from the Box?"))) {
									SEPlay("PC close");
									break;
								}
								continue;
								break;
							case -4:   // Box name
								BoxCommands;
								continue;
								break;
						}
						pokemon = @storage[selected[0], selected[1]];
						if (!pokemon) continue;
						command = ShowCommands(_INTL("{1} is selected.", pokemon.name),
																		new {_INTL("Withdraw"),
																			_INTL("Summary"),
																			_INTL("Mark"),
																			_INTL("Release"),
																			_INTL("Cancel")});
						switch (command) {
							case 0:  Withdraw(selected, null); break;
							case 1:  Summary(selected, null); break;
							case 2:  Mark(selected, null); break;
							case 3:  Release(selected, null); break;
						}
					}
				}
				@scene.CloseBox;
				break;
			case 2:   // Deposit
				@scene.StartBox(self, command);
				do { //loop; while (true);
					selected = @scene.SelectParty(@storage.party);
					if (selected == -3) {   // Close box
						if (Confirm(_INTL("Exit from the Box?"))) {
							SEPlay("PC close");
							break;
						}
						continue;
					} else if (selected < 0) {
						if (Confirm(_INTL("Continue Box operations?"))) continue;
						break;
					} else {
						pokemon = @storagenew {-1, selected};
						if (!pokemon) continue;
						command = ShowCommands(_INTL("{1} is selected.", pokemon.name),
																		new {_INTL("Store"),
																			_INTL("Summary"),
																			_INTL("Mark"),
																			_INTL("Release"),
																			_INTL("Cancel")});
						switch (command) {
							case 0:  Store(new {-1, selected}, null); break;
							case 1:  Summary(new {-1, selected}, null); break;
							case 2:  Mark(new {-1, selected}, null); break;
							case 3:  Release(new {-1, selected}, null); break;
						}
					}
				}
				@scene.CloseBox;
				break;
			case 3:
				@scene.StartBox(self, command);
				@scene.CloseBox;
				break;
		}
		Game.GameData.game_temp.in_storage = false;
	}

	// For debug purposes.
	public void Update() {
		@scene.update;
	}

	// For debug purposes.
	public void HardRefresh() {
		@scene.HardRefresh;
	}

	// For debug purposes.
	public void RefreshSingle(i) {
		@scene.UpdateOverlay(i[1], (i[0] == -1) ? @storage.party : null);
		@scene.HardRefresh;
	}

	public void Display(message) {
		@scene.Display(message);
	}

	public void Confirm(str) {
		return ShowCommands(str, new {_INTL("Yes"), _INTL("No")}) == 0;
	}

	public void ShowCommands(msg, commands, index = 0) {
		return @scene.ShowCommands(msg, commands, index);
	}

	public bool Able(pokemon) {
		pokemon && !pokemon.egg() && pokemon.hp > 0;
	}

	public void AbleCount() {
		count = 0;
		@storage.party.each do |p|
			if (Able(p)) count += 1;
		}
		return count;
	}

	public void HeldPokemon() {
		return @heldpkmn;
	}

	public void Withdraw(selected, heldpoke) {
		box = selected[0];
		index = selected[1];
		if (box == -1) raise _INTL("Can't withdraw from party...");
		if (@storage.party_full()) {
			Display(_INTL("Your party's full!"));
			return false;
		}
		@scene.Withdraw(selected, heldpoke, @storage.party.length);
		if (heldpoke) {
			@storage.MoveCaughtToParty(heldpoke);
			@heldpkmn = null;
		} else {
			@storage.Move(-1, -1, box, index);
		}
		@scene.Refresh;
		return true;
	}

	public void Store(selected, heldpoke) {
		box = selected[0];
		index = selected[1];
		if (box != -1) raise _INTL("Can't deposit from box...");
		if (AbleCount <= 1 && Able(@storage[box, index]) && !heldpoke) {
			PlayBuzzerSE;
			Display(_INTL("That's your last Pokémon!"));
		} else if (heldpoke&.mail) {
			Display(_INTL("Please remove the Mail."));
		} else if (!heldpoke && @storage[box, index].mail) {
			Display(_INTL("Please remove the Mail."));
		} else if (heldpoke&.cannot_store) {
			Display(_INTL("{1} refuses to go into storage!", heldpoke.name));
		} else if (!heldpoke && @storage[box, index].cannot_store) {
			Display(_INTL("{1} refuses to go into storage!", @storage[box, index].name));
		} else {
			do { //loop; while (true);
				destbox = @scene.ChooseBox(_INTL("Deposit in which Box?"));
				if (destbox >= 0) {
					firstfree = @storage.FirstFreePos(destbox);
					if (firstfree < 0) {
						Display(_INTL("The Box is full."));
						continue;
					}
					if (heldpoke || selected[0] == -1) {
						p = (heldpoke) ? heldpoke : @storage[-1, index];
						if (Settings.HEAL_STORED_POKEMON) {
							old_ready_evo = p.ready_to_evolve;
							p.heal;
							p.ready_to_evolve = old_ready_evo;
						}
					}
					@scene.Store(selected, heldpoke, destbox, firstfree);
					if (heldpoke) {
						@storage.MoveCaughtToBox(heldpoke, destbox);
						@heldpkmn = null;
					} else {
						@storage.Move(destbox, -1, -1, index);
					}
				}
				break;
			}
			@scene.Refresh;
		}
	}

	public void Hold(selected) {
		box = selected[0];
		index = selected[1];
		if (box == -1 && Able(@storage[box, index]) && AbleCount <= 1) {
			PlayBuzzerSE;
			Display(_INTL("That's your last Pokémon!"));
			return;
		}
		@scene.Hold(selected);
		@heldpkmn = @storage[box, index];
		@storage.Delete(box, index);
		@scene.Refresh;
	}

	public void Place(selected) {
		box = selected[0];
		index = selected[1];
		if (@storage[box, index]) {
			Debug.LogError(_INTL("Position {1},{2} is not empty...", box, index));
			//throw new ArgumentException(_INTL("Position {1},{2} is not empty...", box, index));
		} else if (box != -1) {
			if (index >= @storage.maxPokemon(box)) {
				Display("Can't place that there.");
				return;
			} else if (@heldpkmn.mail) {
				Display("Please remove the mail.");
				return;
			} else if (@heldpkmn.cannot_store) {
				Display(_INTL("{1} refuses to go into storage!", @heldpkmn.name));
				return;
			}
		}
		if (Settings.HEAL_STORED_POKEMON && box >= 0) {
			old_ready_evo = @heldpkmn.ready_to_evolve;
			@heldpkmn.heal;
			@heldpkmn.ready_to_evolve = old_ready_evo;
		}
		@scene.Place(selected, @heldpkmn);
		@storage[box, index] = @heldpkmn;
		if (box == -1) @storage.party.compact!;
		@scene.Refresh;
		@heldpkmn = null;
	}

	public void Swap(selected) {
		box = selected[0];
		index = selected[1];
		if (!@storage[box, index]) {
			Debug.LogError(_INTL("Position {1},{2} is empty...", box, index));
			//throw new ArgumentException(_INTL("Position {1},{2} is empty...", box, index));
		}
		if (@heldpkmn.cannot_store && box != -1) {
			PlayBuzzerSE;
			Display(_INTL("{1} refuses to go into storage!", @heldpkmn.name));
			return false;
		} else if (box == -1 && Able(@storage[box, index]) && AbleCount <= 1 && !Able(@heldpkmn)) {
			PlayBuzzerSE;
			Display(_INTL("That's your last Pokémon!"));
			return false;
		}
		if (box != -1 && @heldpkmn.mail) {
			Display("Please remove the mail.");
			return false;
		}
		if (Settings.HEAL_STORED_POKEMON && box >= 0) {
			old_ready_evo = @heldpkmn.ready_to_evolve;
			@heldpkmn.heal;
			@heldpkmn.ready_to_evolve = old_ready_evo;
		}
		@scene.Swap(selected, @heldpkmn);
		tmp = @storage[box, index];
		@storage[box, index] = @heldpkmn;
		@heldpkmn = tmp;
		@scene.Refresh;
		return true;
	}

	public void Release(selected, heldpoke) {
		box = selected[0];
		index = selected[1];
		pokemon = (heldpoke) ? heldpoke : @storage[box, index];
		if (!pokemon) return;
		if (pokemon.egg()) {
			Display(_INTL("You can't release an Egg."));
			return false;
		} else if (pokemon.mail) {
			Display(_INTL("Please remove the mail."));
			return false;
		} else if (pokemon.cannot_release) {
			Display(_INTL("{1} refuses to leave you!", pokemon.name));
			return false;
		}
		if (box == -1 && AbleCount <= 1 && Able(pokemon) && !heldpoke) {
			PlayBuzzerSE;
			Display(_INTL("That's your last Pokémon!"));
			return;
		}
		command = ShowCommands(_INTL("Release this Pokémon?"), new {_INTL("No"), _INTL("Yes")});
		if (command == 1) {
			pkmnname = pokemon.name;
			@scene.Release(selected, heldpoke);
			if (heldpoke) {
				@heldpkmn = null;
			} else {
				@storage.Delete(box, index);
			}
			@scene.Refresh;
			Display(_INTL("{1} was released.", pkmnname));
			Display(_INTL("Bye-bye, {1}!", pkmnname));
			@scene.Refresh;
		}
		return;
	}

	public void ChooseMove(pkmn, helptext, index = 0) {
		movenames = new List<string>();
		foreach (var i in pkmn.moves) { //'pkmn.moves.each' do => |i|
			if (i.total_pp <= 0) {
				movenames.Add(_INTL("{1} (PP: ---)", i.name));
			} else {
				movenames.Add(_INTL("{1} (PP: {2}/{3})", i.name, i.pp, i.total_pp));
			}
		}
		return @scene.ShowCommands(helptext, movenames, index);
	}

	public void Summary(selected, heldpoke) {
		@scene.Summary(selected, heldpoke);
	}

	public void Mark(selected, heldpoke) {
		@scene.Mark(selected, heldpoke);
	}

	public void Item(selected, heldpoke) {
		box = selected[0];
		index = selected[1];
		pokemon = (heldpoke) ? heldpoke : @storage[box, index];
		if (pokemon.egg()) {
			Display(_INTL("Eggs can't hold items."));
			return;
		} else if (pokemon.mail) {
			Display(_INTL("Please remove the mail."));
			return;
		}
		if (pokemon.item) {
			itemname = pokemon.item.portion_name;
			if (Confirm(_INTL("Take the {1}?", itemname))) {
				if (Game.GameData.bag.add(pokemon.item)) {
					Display(_INTL("Took the {1}.", itemname));
					pokemon.item = null;
					@scene.HardRefresh;
				} else {
					Display(_INTL("Can't store the {1}.", itemname));
				}
			}
		} else {
			item = scene.ChooseItem(Game.GameData.bag);
			if (item) {
				itemname = GameData.Item.get(item).name;
				pokemon.item = item;
				Game.GameData.bag.remove(item);
				Display(_INTL("{1} is now being held.", itemname));
				@scene.HardRefresh;
			}
		}
	}

	public void BoxCommands() {
		commands = new {
			_INTL("Jump"),
			_INTL("Wallpaper"),
			_INTL("Name"),
			_INTL("Cancel");
		}
		command = ShowCommands(_INTL("What do you want to do?"), commands);
		switch (command) {
			case 0:
				destbox = @scene.ChooseBox(_INTL("Jump to which Box?"));
				if (destbox >= 0) @scene.JumpToBox(destbox);
				break;
			case 1:
				papers = @storage.availableWallpapers;
				index = 0;
				for (int i = papers[1].length; i < papers[1].length; i++) { //for 'papers[1].length' times do => |i|
					if (papers[1][i] == @storage[@storage.currentBox].background) {
						index = i;
						break;
					}
				}
				wpaper = ShowCommands(_INTL("Pick the wallpaper."), papers[0], index);
				if (wpaper >= 0) @scene.ChangeBackground(papers[1][wpaper]);
				break;
			case 2:
				@scene.BoxName(_INTL("Box name?"), 0, 12);
				break;
		}
	}

	public void ChoosePokemon(_party = null) {
		Game.GameData.game_temp.in_storage = true;
		@heldpkmn = null;
		@scene.StartBox(self, 1);
		retval = null;
		do { //loop; while (true);
			selected = @scene.SelectBox(@storage.party);
			if (selected && selected[0] == -3) {   // Close box
				if (Confirm(_INTL("Exit from the Box?"))) {
					SEPlay("PC close");
					break;
				}
				continue;
			}
			if (selected.null()) {
				if (Confirm(_INTL("Continue Box operations?"))) continue;
				break;
			} else if (selected[0] == -4) {   // Box name
				BoxCommands;
			} else {
				pokemon = @storage[selected[0], selected[1]];
				if (!pokemon) continue;
				commands = new {
					_INTL("Select"),
					_INTL("Summary"),
					_INTL("Withdraw"),
					_INTL("Item"),
					_INTL("Mark");
				}
				commands.Add(_INTL("Cancel"));
				if (selected[0] == -1) commands[2] = _INTL("Store");
				helptext = _INTL("{1} is selected.", pokemon.name);
				command = ShowCommands(helptext, commands);
				switch (command) {
					case 0:   // Select
						if (pokemon) {
							retval = selected;
							break;
						}
						break;
					case 1:
						Summary(selected, null);
						break;
					case 2:   // Store/Withdraw
						if (selected[0] == -1) {
							Store(selected, null);
						} else {
							Withdraw(selected, null);
						}
						break;
					case 3:
						Item(selected, null);
						break;
					case 4:
						Mark(selected, null);
						break;
				}
			}
		}
		@scene.CloseBox;
		Game.GameData.game_temp.in_storage = false;
		return retval;
	}
}
