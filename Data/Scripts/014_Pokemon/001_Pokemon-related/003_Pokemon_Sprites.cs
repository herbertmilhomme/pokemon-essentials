//===============================================================================
// Pokémon sprite (used out of battle).
//===============================================================================
public partial class PokemonSprite : Sprite {
	public override void initialize(viewport = null) {
		base.initialize(viewport);
		@_iconbitmap = null;
	}

	public override void dispose() {
		@_iconbitmap&.dispose;
		@_iconbitmap = null;
		if (!self.disposed()) self.bitmap = null;
		base.dispose();
	}

	public void clearBitmap() {
		@_iconbitmap&.dispose;
		@_iconbitmap = null;
		self.bitmap = null;
	}

	public void setOffset(offset = PictureOrigin.CENTER) {
		@offset = offset;
		changeOrigin;
	}

	public void changeOrigin() {
		if (!self.bitmap) return;
		if (!@offset) @offset = PictureOrigin.CENTER;
		switch (@offset) {
			case PictureOrigin.TOP_LEFT: case PictureOrigin.LEFT: case PictureOrigin.BOTTOM_LEFT:
				self.ox = 0;
				break;
			case PictureOrigin.TOP: case PictureOrigin.CENTER: case PictureOrigin.BOTTOM:
				self.ox = self.bitmap.width / 2;
				break;
			case PictureOrigin.TOP_RIGHT: case PictureOrigin.RIGHT: case PictureOrigin.BOTTOM_RIGHT:
				self.ox = self.bitmap.width;
				break;
		}
		switch (@offset) {
			case PictureOrigin.TOP_LEFT: case PictureOrigin.TOP: case PictureOrigin.TOP_RIGHT:
				self.oy = 0;
				break;
			case PictureOrigin.LEFT: case PictureOrigin.CENTER: case PictureOrigin.RIGHT:
				self.oy = self.bitmap.height / 2;
				break;
			case PictureOrigin.BOTTOM_LEFT: case PictureOrigin.BOTTOM: case PictureOrigin.BOTTOM_RIGHT:
				self.oy = self.bitmap.height;
				break;
		}
	}

	public void setPokemonBitmap(pokemon, back = false) {
		@_iconbitmap&.dispose;
		@_iconbitmap = (pokemon) ? GameData.Species.sprite_bitmap_from_pokemon(pokemon, back) : null;
		self.bitmap = (@_iconbitmap) ? @_iconbitmap.bitmap : null;
		self.color = new Color(0, 0, 0, 0);
		changeOrigin;
	}

	public void setPokemonBitmapSpecies(pokemon, species, back = false) {
		@_iconbitmap&.dispose;
		@_iconbitmap = (pokemon) ? GameData.Species.sprite_bitmap_from_pokemon(pokemon, back, species) : null;
		self.bitmap = (@_iconbitmap) ? @_iconbitmap.bitmap : null;
		changeOrigin;
	}

	public void setSpeciesBitmap(species, gender = 0, form = 0, shiny = false, shadow = false, back = false, egg = false) {
		@_iconbitmap&.dispose;
		@_iconbitmap = GameData.Species.sprite_bitmap(species, form, gender, shiny, shadow, back, egg);
		self.bitmap = (@_iconbitmap) ? @_iconbitmap.bitmap : null;
		changeOrigin;
	}

	public override void update() {
		base.update();
		if (@_iconbitmap) {
			@_iconbitmap.update;
			self.bitmap = @_iconbitmap.bitmap;
		}
	}
}

//===============================================================================
// Pokémon icon (for defined Pokémon).
//===============================================================================
public partial class PokemonIconSprite : Sprite {
	public int selected		{ get { return _selected; } set { _selected = value; } }			protected int _selected;
	public int active		{ get { return _active; } set { _active = value; } }			protected int _active;
	public int pokemon		{ get { return _pokemon; } }			protected int _pokemon;

	// Time in seconds for one animation cycle of this Pokémon icon. It is doubled
	// if (the Pokémon is at 50% HP or lower, and doubled again if it is at 25% HP) {
	// or lower. The icon doesn't animate at all if the Pokémon is fainted.
	public const int ANIMATION_DURATION = 0.25;

	public override void initialize(pokemon, viewport = null) {
		base.initialize(viewport);
		@selected      = false;
		@active        = false;
		@frames_count  = 0;
		@current_frame = 0;
		self.pokemon   = pokemon;
		@logical_x     = 0;   // Actual x coordinate
		@logical_y     = 0;   // Actual y coordinate
		@adjusted_x    = 0;   // Offset due to "jumping" animation in party screen
		@adjusted_y    = 0;   // Offset due to "jumping" animation in party screen
	}

	public override void dispose() {
		@animBitmap&.dispose;
		base.dispose();
	}

	public int x { get { return @logical_x; } }
	public int y { get { return @logical_y; } }

	public override int x { set {
		@logical_x = value;
		base.x(@logical_x + @adjusted_x);
		}
	}

	public override int y { set {
		@logical_y = value;
		base.y(@logical_y + @adjusted_y);
		}
	}

	public int pokemon { set {
		@pokemon = value;
		@animBitmap&.dispose;
		@animBitmap = null;
		if (!@pokemon) {
			self.bitmap = null;
			@current_frame = 0;
			return;
			}
	}
		@animBitmap = new AnimatedBitmap(GameData.Species.icon_filename_from_pokemon(value));
		self.bitmap = @animBitmap.bitmap;
		self.src_rect.width  = @animBitmap.height;
		self.src_rect.height = @animBitmap.height;
		@frames_count = @animBitmap.width / @animBitmap.height;
		if (@current_frame >= @frames_count) @current_frame = 0;
		changeOrigin;
	}

	public void setOffset(offset = PictureOrigin.CENTER) {
		@offset = offset;
		changeOrigin;
	}

	public void changeOrigin() {
		if (!self.bitmap) return;
		if (!@offset) @offset = PictureOrigin.TOP_LEFT;
		switch (@offset) {
			case PictureOrigin.TOP_LEFT: case PictureOrigin.LEFT: case PictureOrigin.BOTTOM_LEFT:
				self.ox = 0;
				break;
			case PictureOrigin.TOP: case PictureOrigin.CENTER: case PictureOrigin.BOTTOM:
				self.ox = self.src_rect.width / 2;
				break;
			case PictureOrigin.TOP_RIGHT: case PictureOrigin.RIGHT: case PictureOrigin.BOTTOM_RIGHT:
				self.ox = self.src_rect.width;
				break;
		}
		switch (@offset) {
			case PictureOrigin.TOP_LEFT: case PictureOrigin.TOP: case PictureOrigin.TOP_RIGHT:
				self.oy = 0;
				break;
			case PictureOrigin.LEFT: case PictureOrigin.CENTER: case PictureOrigin.RIGHT:
				// NOTE: This assumes the top quarter of the icon is blank, so oy is placed
				//       in the middle of the lower three quarters of the image.
				self.oy = self.src_rect.height * 5 / 8;
				break;
			case PictureOrigin.BOTTOM_LEFT: case PictureOrigin.BOTTOM: case PictureOrigin.BOTTOM_RIGHT:
				self.oy = self.src_rect.height;
				break;
		}
	}

	public void update_frame() {
		if (@pokemon.fainted()) {
			@current_frame = 0;
			return;
		}
		duration = ANIMATION_DURATION;
		if (@pokemon.hp <= @pokemon.totalhp / 4) {      // Red HP - 1 second
			duration *= 4;
		} else if (@pokemon.hp <= @pokemon.totalhp / 2) {   // Yellow HP - 0.5 seconds
			duration *= 2;
		}
		@current_frame = (int)Math.Floor(@frames_count * (System.uptime % duration) / duration);
	}

	public override void update() {
		if (!@animBitmap) return;
		base.update();
		@animBitmap.update;
		self.bitmap = @animBitmap.bitmap;
		// Update animation
		update_frame;
		self.src_rect.x = self.src_rect.width * @current_frame;
		// Update "jumping" animation (used in party screen)
		if (@selected) {
			@adjusted_x = 4;
			@adjusted_y = (@current_frame >= @frames_count / 2) ? -2 : 6;
		} else {
			@adjusted_x = 0;
			@adjusted_y = 0;
		}
		self.x = self.x;
		self.y = self.y;
	}
}

//===============================================================================
// Pokémon icon (for species).
//===============================================================================
public partial class PokemonSpeciesIconSprite : Sprite {
	public int species		{ get { return _species; } }			protected int _species;
	public int gender		{ get { return _gender; } }			protected int _gender;
	public int form		{ get { return _form; } }			protected int _form;
	public int shiny		{ get { return _shiny; } }			protected int _shiny;

	// Time in seconds for one animation cycle of this Pokémon icon.
	public const int ANIMATION_DURATION = 0.25;

	public override void initialize(species, viewport = null) {
		base.initialize(viewport);
		@species       = species;
		@gender        = 0;
		@form          = 0;
		@shiny         = 0;
		@frames_count  = 0;
		@current_frame = 0;
		refresh;
	}

	public override void dispose() {
		@animBitmap&.dispose;
		base.dispose();
	}

	public int species { set {
		@species = value;
		refresh;
		}
	}

	public int gender { set {
		@gender = value;
		refresh;
		}
	}

	public int form { set {
		@form = value;
		refresh;
		}
	}

	public int shiny { set {
		@shiny = value;
		refresh;
		}
	}

	public void SetParams(species, gender, form, shiny = false) {
		@species = species;
		@gender  = gender;
		@form    = form;
		@shiny   = shiny;
		refresh;
	}

	public void setOffset(offset = PictureOrigin.CENTER) {
		@offset = offset;
		changeOrigin;
	}

	public void changeOrigin() {
		if (!self.bitmap) return;
		if (!@offset) @offset = PictureOrigin.TOP_LEFT;
		switch (@offset) {
			case PictureOrigin.TOP_LEFT: case PictureOrigin.LEFT: case PictureOrigin.BOTTOM_LEFT:
				self.ox = 0;
				break;
			case PictureOrigin.TOP: case PictureOrigin.CENTER: case PictureOrigin.BOTTOM:
				self.ox = self.src_rect.width / 2;
				break;
			case PictureOrigin.TOP_RIGHT: case PictureOrigin.RIGHT: case PictureOrigin.BOTTOM_RIGHT:
				self.ox = self.src_rect.width;
				break;
		}
		switch (@offset) {
			case PictureOrigin.TOP_LEFT: case PictureOrigin.TOP: case PictureOrigin.TOP_RIGHT:
				self.oy = 0;
				break;
			case PictureOrigin.LEFT: case PictureOrigin.CENTER: case PictureOrigin.RIGHT:
				// NOTE: This assumes the top quarter of the icon is blank, so oy is placed
				//       in the middle of the lower three quarters of the image.
				self.oy = self.src_rect.height * 5 / 8;
				break;
			case PictureOrigin.BOTTOM_LEFT: case PictureOrigin.BOTTOM: case PictureOrigin.BOTTOM_RIGHT:
				self.oy = self.src_rect.height;
				break;
		}
	}

	public void refresh() {
		@animBitmap&.dispose;
		@animBitmap = null;
		bitmapFileName = GameData.Species.icon_filename(@species, @form, @gender, @shiny);
		if (!bitmapFileName) return;
		@animBitmap = new AnimatedBitmap(bitmapFileName);
		self.bitmap = @animBitmap.bitmap;
		self.src_rect.width  = @animBitmap.height;
		self.src_rect.height = @animBitmap.height;
		@frames_count = @animBitmap.width / @animBitmap.height;
		if (@current_frame >= @frames_count) @current_frame = 0;
		changeOrigin;
	}

	public void update_frame() {
		@current_frame = (int)Math.Floor(@frames_count * (System.uptime % ANIMATION_DURATION) / ANIMATION_DURATION);
	}

	public override void update() {
		if (!@animBitmap) return;
		base.update();
		@animBitmap.update;
		self.bitmap = @animBitmap.bitmap;
		// Update animation
		update_frame;
		self.src_rect.x = self.src_rect.width * @current_frame;
	}
}
