//===============================================================================
// "Mining" mini-game.
// By Maruno.
//-------------------------------------------------------------------------------
// Run with:      MiningGame
//===============================================================================
public partial class MiningGameCounter : BitmapSprite {
	public int hits		{ get { return _hits; } set { _hits = value; } }			protected int _hits;

	public override void initialize(x, y, viewport) {
		@viewport = viewport;
		@x = x;
		@y = y;
		base.initialize(416, 60, @viewport);
		@hits = 0;
		@image = new AnimatedBitmap("Graphics/UI/Mining/cracks");
		update;
	}

	public void update() {
		self.bitmap.clear;
		value = @hits;
		startx = 416 - 48;
		while (value > 6) {
			self.bitmap.blt(startx, 0, @image.bitmap, new Rect(0, 0, 48, 52));
			startx -= 48;
			value -= 6;
		}
		startx -= 48;
		if (value > 0) {
			self.bitmap.blt(startx, 0, @image.bitmap, new Rect(0, value * 52, 96, 52));
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class MiningGameTile : BitmapSprite {
	public int layer		{ get { return _layer; } }			protected int _layer;

	public override void initialize(viewport) {
		@viewport = viewport;
		base.initialize(32, 32, @viewport);
		r = rand(100);
		if (r < 10) {
			@layer = 2;   // 10%
		} else if (r < 25) {
			@layer = 3;   // 15%
		} else if (r < 60) {
			@layer = 4;   // 35%
		} else if (r < 85) {
			@layer = 5;   // 25%
		} else {
			@layer = 6;   // 15%
		}
		@image = new AnimatedBitmap("Graphics/UI/Mining/tiles");
		update;
	}

	public int layer { set {
		@layer = value;
		if (@layer < 0) @layer = 0;
		}
	}

	public void update() {
		self.bitmap.clear;
		if (@layer > 0) {
			self.bitmap.blt(0, 0, @image.bitmap, new Rect(0, 32 * (@layer - 1), 32, 32));
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class MiningGameCursor : BitmapSprite {
	public int position		{ get { return _position; } set { _position = value; } }			protected int _position;
	public int mode		{ get { return _mode; } set { _mode = value; } }			protected int _mode;

	public const int HIT_FRAME_DURATION = 0.05;   // In seconds
	TOOL_POSITIONS = new {new {1, 0}, new {1, 1}, new {1, 1}, new {0, 0}, new {0, 0},
										new {0, 2}, new {0, 2}, new {0, 0}, new {0, 0}, new {0, 2}, new {0, 2}}   // Graphic, position

	// mode: 0=pick, 1=hammer.
	public override void initialize(position, mode, viewport) {
		@viewport = viewport;
		base.initialize(Graphics.width, Graphics.height, @viewport);
		@position = position;
		@mode     = mode;
		@hit      = 0;   // 0=regular, 1=hit item, 2=hit iron
		@cursorbitmap = new AnimatedBitmap("Graphics/UI/Mining/cursor");
		@toolbitmap   = new AnimatedBitmap("Graphics/UI/Mining/tools");
		@hitsbitmap   = new AnimatedBitmap("Graphics/UI/Mining/hits");
		update;
	}

	public void animate(hit) {
		@hit = hit;
		@hit_timer_start = System.uptime;
	}

	public bool isAnimating() {
		return !@hit_timer_start.null();
	}

	public void update() {
		self.bitmap.clear;
		x = 32 * (@position % MiningGameScene.BOARD_WIDTH);
		y = 32 * (@position / MiningGameScene.BOARD_WIDTH);
		if (@hit_timer_start) {
			hit_frame = ((System.uptime - @hit_timer_start) / HIT_FRAME_DURATION).ToInt();
			if (hit_frame >= TOOL_POSITIONS.length) @hit_timer_start = null;
			if (@hit_timer_start) {
				toolx = x;
				tooly = y;
				switch (TOOL_POSITIONS[hit_frame][1]) {
					case 1:
						toolx -= 8;
						tooly += 8;
						break;
					case 2:
						toolx += 6;
						break;
				}
				self.bitmap.blt(toolx, tooly, @toolbitmap.bitmap,
												new Rect(96 * TOOL_POSITIONS[hit_frame][0], 96 * @mode, 96, 96));
				if (hit_frame < 5 && hit_frame.even()) {
					if (@hit == 2) {
						self.bitmap.blt(x - 64, y, @hitsbitmap.bitmap, new Rect(160 * 2, 0, 160, 160));
					} else {
						self.bitmap.blt(x - 64, y, @hitsbitmap.bitmap, new Rect(160 * @mode, 0, 160, 160));
					}
				}
				if (@hit == 1 && hit_frame < 3) {
					self.bitmap.blt(x - 64, y, @hitsbitmap.bitmap, new Rect(160 * hit_frame, 160, 160, 160));
				}
			}
		}
		if (!@hit_timer_start) {
			self.bitmap.blt(x, y + 64, @cursorbitmap.bitmap, new Rect(32 * @mode, 0, 32, 32));
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class MiningGameScene {
	public const int BOARD_WIDTH  = 13;
	public const int BOARD_HEIGHT = 10;
	ITEMS = new {   // Item, probability, graphic x, graphic y, width, height, pattern
		new {:DOMEFOSSIL, 20, 0, 3, 5, 4, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0}},
		new {:HELIXFOSSIL, 5, 5, 3, 4, 4, new {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}},
		new {:HELIXFOSSIL, 5, 9, 3, 4, 4, new {1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1}},
		new {:HELIXFOSSIL, 5, 13, 3, 4, 4, new {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}},
		new {:HELIXFOSSIL, 5, 17, 3, 4, 4, new {1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1}},
		new {:OLDAMBER, 10, 21, 3, 4, 4, new {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}},
		new {:OLDAMBER, 10, 25, 3, 4, 4, new {1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1}},
		new {:ROOTFOSSIL, 5, 0, 7, 5, 5, new {1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0}},
		new {:ROOTFOSSIL, 5, 5, 7, 5, 5, new {0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0}},
		new {:ROOTFOSSIL, 5, 10, 7, 5, 5, new {0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1}},
		new {:ROOTFOSSIL, 5, 15, 7, 5, 5, new {0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0}},
		new {:SKULLFOSSIL, 20, 20, 7, 4, 4, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0}},
		new {:ARMORFOSSIL, 20, 24, 7, 5, 4, new {0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0}},
		new {:CLAWFOSSIL, 5, 0, 12, 4, 5, new {0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 0}},
		new {:CLAWFOSSIL, 5, 4, 12, 5, 4, new {1, 1, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1}},
		new {:CLAWFOSSIL, 5, 9, 12, 4, 5, new {0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 0, 0}},
		new {:CLAWFOSSIL, 5, 13, 12, 5, 4, new {1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1}},
		new {:FIRESTONE, 20, 20, 11, 3, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:WATERSTONE, 20, 23, 11, 3, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 0}},
		new {:THUNDERSTONE, 20, 26, 11, 3, 3, new {0, 1, 1, 1, 1, 1, 1, 1, 0}},
		new {:LEAFSTONE, 10, 18, 14, 3, 4, new {0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0}},
		new {:LEAFSTONE, 10, 21, 14, 4, 3, new {0, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0}},
		new {:MOONSTONE, 10, 25, 14, 4, 2, new {0, 1, 1, 1, 1, 1, 1, 0}},
		new {:MOONSTONE, 10, 27, 16, 2, 4, new {1, 0, 1, 1, 1, 1, 0, 1}},
		new {:SUNSTONE, 20, 21, 17, 3, 3, new {0, 1, 0, 1, 1, 1, 1, 1, 1}},
		new {:OVALSTONE, 150, 24, 17, 3, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:EVERSTONE, 150, 21, 20, 4, 2, new {1, 1, 1, 1, 1, 1, 1, 1}},
		new {:STARPIECE, 100, 0, 17, 3, 3, new {0, 1, 0, 1, 1, 1, 0, 1, 0}},
		new {:REVIVE, 100, 0, 20, 3, 3, new {0, 1, 0, 1, 1, 1, 0, 1, 0}},
		new {:MAXREVIVE, 50, 0, 23, 3, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:RAREBONE, 50, 3, 17, 6, 3, new {1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1}},
		new {:RAREBONE, 50, 3, 20, 3, 6, new {1, 1, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 1, 1}},
		new {:LIGHTCLAY, 100, 6, 20, 4, 4, new {1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1}},
		new {:HARDSTONE, 200, 6, 24, 2, 2, new {1, 1, 1, 1}},
		new {:HEARTSCALE, 200, 8, 24, 2, 2, new {1, 0, 1, 1}},
		new {:IRONBALL, 100, 9, 17, 3, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:ODDKEYSTONE, 100, 10, 20, 4, 4, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:HEATROCK, 50, 12, 17, 4, 3, new {1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:DAMPROCK, 50, 14, 20, 3, 3, new {1, 1, 1, 1, 1, 1, 1, 0, 1}},
		new {:SMOOTHROCK, 50, 17, 18, 4, 4, new {0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 1, 0, 0}},
		new {:ICYROCK, 50, 17, 22, 4, 4, new {0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1}},
		new {:REDSHARD, 100, 21, 22, 3, 3, new {1, 1, 1, 1, 1, 0, 1, 1, 1}},
		new {:GREENSHARD, 100, 25, 20, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1}},
		new {:YELLOWSHARD, 100, 25, 23, 4, 3, new {1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1}},
		new {:BLUESHARD, 100, 26, 26, 3, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 0}},
		new {:INSECTPLATE, 10, 0, 26, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:DREADPLATE, 10, 4, 26, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:DRACOPLATE, 10, 8, 26, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:ZAPPLATE, 10, 12, 26, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:FISTPLATE, 10, 16, 26, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:FLAMEPLATE, 10, 20, 26, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:MEADOWPLATE, 10, 0, 29, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:EARTHPLATE, 10, 4, 29, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:ICICLEPLATE, 10, 8, 29, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:TOXICPLATE, 10, 12, 29, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:MINDPLATE, 10, 16, 29, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:STONEPLATE, 10, 20, 29, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:SKYPLATE, 10, 0, 32, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:SPOOKYPLATE, 10, 4, 32, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:IRONPLATE, 10, 8, 32, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {:SPLASHPLATE, 10, 12, 32, 4, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}}
	}
	IRON = new {   // Graphic x, graphic y, width, height, pattern
		new {0, 0, 1, 4, new {1, 1, 1, 1}},
		new {1, 0, 2, 4, new {1, 1, 1, 1, 1, 1, 1, 1}},
		new {3, 0, 4, 2, new {1, 1, 1, 1, 1, 1, 1, 1}},
		new {3, 2, 4, 1, new {1, 1, 1, 1}},
		new {7, 0, 3, 3, new {1, 1, 1, 1, 1, 1, 1, 1, 1}},
		new {0, 5, 3, 2, new {1, 1, 0, 0, 1, 1}},
		new {0, 7, 3, 2, new {0, 1, 0, 1, 1, 1}},
		new {3, 5, 3, 2, new {0, 1, 1, 1, 1, 0}},
		new {3, 7, 3, 2, new {1, 1, 1, 0, 1, 0}},
		new {6, 3, 2, 3, new {1, 0, 1, 1, 0, 1}},
		new {8, 3, 2, 3, new {0, 1, 1, 1, 1, 0}},
		new {6, 6, 2, 3, new {1, 0, 1, 1, 1, 0}},
		new {8, 6, 2, 3, new {0, 1, 1, 1, 0, 1}}
	}

	public void update() {
		UpdateSpriteHash(@sprites);
	}

	public void StartScene() {
		@sprites = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		addBackgroundPlane(@sprites, "bg", "Mining/bg", @viewport);
		@sprites["itemlayer"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["itemlayer"].z = 10;
		@itembitmap = new AnimatedBitmap("Graphics/UI/Mining/items");
		@ironbitmap = new AnimatedBitmap("Graphics/UI/Mining/irons");
		@items = new List<string>();
		@itemswon = new List<string>();
		@iron = new List<string>();
		DistributeItems;
		DistributeIron;
		for (int i = BOARD_HEIGHT; i < BOARD_HEIGHT; i++) { //for 'BOARD_HEIGHT' times do => |i|
			for (int j = BOARD_WIDTH; j < BOARD_WIDTH; j++) { //for 'BOARD_WIDTH' times do => |j|
				@sprites[$"tile{j + (i * BOARD_WIDTH)}"] = new MiningGameTile(@viewport);
				@sprites[$"tile{j + (i * BOARD_WIDTH)}"].x = 32 * j;
				@sprites[$"tile{j + (i * BOARD_WIDTH)}"].y = 64 + (32 * i);
				@sprites[$"tile{j + (i * BOARD_WIDTH)}"].z = 20;
			}
		}
		@sprites["crack"] = new MiningGameCounter(0, 4, @viewport);
		@sprites["cursor"] = new MiningGameCursor(58, 0, @viewport);   // central position, pick
		@sprites["cursor"].z = 50;
		@sprites["tool"] = new IconSprite(434, 254, @viewport);
		@sprites["tool"].setBitmap("Graphics/UI/Mining/toolicons");
		@sprites["tool"].src_rect.set(0, 0, 68, 100);
		@sprites["tool"].z = 100;
		update;
		FadeInAndShow(@sprites);
	}

	public void DistributeItems() {
		// Set items to be buried (index in ITEMS, x coord, y coord)
		ptotal = 0;
		foreach (var i in ITEMS) { //'ITEMS.each' do => |i|
			ptotal += i[1];
		}
		numitems = rand(2..4);
		tries = 0;
		while (numitems > 0) {
			rnd = rand(ptotal);
			added = false;
			for (int i = ITEMS.length; i < ITEMS.length; i++) { //for 'ITEMS.length' times do => |i|
				rnd -= ITEMS[i][1];
				if (rnd < 0) {
					if (NoDuplicateItems(ITEMS[i][0])) {
						until added;
							provx = rand(BOARD_WIDTH - ITEMS[i][4] + 1);
							provy = rand(BOARD_HEIGHT - ITEMS[i][5] + 1);
							if (CheckOverlaps(false, provx, provy, ITEMS[i][4], ITEMS[i][5], ITEMS[i][6])) {
								@items.Add(new {i, provx, provy});
								numitems -= 1;
								added = true;
							}
						}
					} else {
						break;
					}
				}
				if (added) break;
			}
			tries += 1;
			if (tries >= 500) break;
		}
		// Draw items on item layer
		layer = @sprites["itemlayer"].bitmap;
		@items.each do |i|
			ox = ITEMS[i[0]][2];
			oy = ITEMS[i[0]][3];
			rectx = ITEMS[i[0]][4];
			recty = ITEMS[i[0]][5];
			layer.blt(32 * i[1], 64 + (32 * i[2]), @itembitmap.bitmap, new Rect(32 * ox, 32 * oy, 32 * rectx, 32 * recty));
		}
	}

	public void DistributeIron() {
		// Set iron to be buried (index in IRON, x coord, y coord)
		numitems = rand(4..6);
		tries = 0;
		while (numitems > 0) {
			rnd = rand(IRON.length);
			provx = rand(BOARD_WIDTH - IRON[rnd][2] + 1);
			provy = rand(BOARD_HEIGHT - IRON[rnd][3] + 1);
			if (CheckOverlaps(true, provx, provy, IRON[rnd][2], IRON[rnd][3], IRON[rnd][4])) {
				@iron.Add(new {rnd, provx, provy});
				numitems -= 1;
			}
			tries += 1;
			if (tries >= 500) break;
		}
		// Draw items on item layer
		layer = @sprites["itemlayer"].bitmap;
		@iron.each do |i|
			ox = IRON[i[0]][0];
			oy = IRON[i[0]][1];
			rectx = IRON[i[0]][2];
			recty = IRON[i[0]][3];
			layer.blt(32 * i[1], 64 + (32 * i[2]), @ironbitmap.bitmap, new Rect(32 * ox, 32 * oy, 32 * rectx, 32 * recty));
		}
	}

	public void NoDuplicateItems(newitem) {
		if (newitem == items.HEARTSCALE) return true;   // Allow multiple Heart Scales
		fossils = new {:DOMEFOSSIL, :HELIXFOSSIL, :OLDAMBER, :ROOTFOSSIL,
							:SKULLFOSSIL, :ARMORFOSSIL, :CLAWFOSSIL};
		plates = new {:INSECTPLATE, :DREADPLATE, :DRACOPLATE, :ZAPPLATE, :FISTPLATE,
							:FLAMEPLATE, :MEADOWPLATE, :EARTHPLATE, :ICICLEPLATE, :TOXICPLATE,
							:MINDPLATE, :STONEPLATE, :SKYPLATE, :SPOOKYPLATE, :IRONPLATE, :SPLASHPLATE};
		@items.each do |i|
			preitem = ITEMS[i[0]][0];
			if (preitem == newitem) return false;   // No duplicate items
			if (fossils.Contains(preitem) && fossils.Contains(newitem)) return false;
			if (plates.Contains(preitem) && plates.Contains(newitem)) return false;
		}
		return true;
	}

	public void CheckOverlaps(checkiron, provx, provy, provwidth, provheight, provpattern) {
		@items.each do |i|
			prex = i[1];
			prey = i[2];
			prewidth = ITEMS[i[0]][4];
			preheight = ITEMS[i[0]][5];
			prepattern = ITEMS[i[0]][6];
			if (provx + provwidth <= prex || provx >= prex + prewidth ||
							provy + provheight <= prey || provy >= prey + preheight) continue;
			for (int j = prepattern.length; j < prepattern.length; j++) { //for 'prepattern.length' times do => |j|
				if (prepattern[j] == 0) continue;
				xco = prex + (j % prewidth);
				yco = prey + (int)Math.Floor(j / prewidth);
				if (provx + provwidth <= xco || provx > xco ||
								provy + provheight <= yco || provy > yco) continue;
				if (provpattern[xco - provx + ((yco - provy) * provwidth)] == 1) return false;
			}
		}
		if (checkiron) {   // Check other irons as well
			@iron.each do |i|
				prex = i[1];
				prey = i[2];
				prewidth = IRON[i[0]][2];
				preheight = IRON[i[0]][3];
				prepattern = IRON[i[0]][4];
				if (provx + provwidth <= prex || provx >= prex + prewidth ||
								provy + provheight <= prey || provy >= prey + preheight) continue;
				for (int j = prepattern.length; j < prepattern.length; j++) { //for 'prepattern.length' times do => |j|
					if (prepattern[j] == 0) continue;
					xco = prex + (j % prewidth);
					yco = prey + (int)Math.Floor(j / prewidth);
					if (provx + provwidth <= xco || provx > xco ||
									provy + provheight <= yco || provy > yco) continue;
					if (provpattern[xco - provx + ((yco - provy) * provwidth)] == 1) return false;
				}
			}
		}
		return true;
	}

	public void Hit() {
		hittype = 0;
		position = @sprites["cursor"].position;
		if (@sprites["cursor"].mode == 1) {   // Hammer
			pattern = new {1, 2, 1,
								2, 2, 2,
								1, 2, 1};
			if (!(Core.DEBUG && Input.press(Input.CTRL))) @sprites["crack"].hits += 2;
		} else {                            // Pick
			pattern = new {0, 1, 0,
								1, 2, 1,
								0, 1, 0};
			if (!(Core.DEBUG && Input.press(Input.CTRL))) @sprites["crack"].hits += 1;
		}
		if (@sprites[$"tile) {{position}"].layer <= pattern[4] && IsIronThere(position)
			@sprites[$"tile{position}"].layer -= pattern[4];
			SEPlay("Mining iron");
			hittype = 2;
		} else {
			for (int i = 3; i < 3; i++) { //for '3' times do => |i|
				ytile = i - 1 + (position / BOARD_WIDTH);
				if (ytile < 0 || ytile >= BOARD_HEIGHT) continue;
				for (int j = 3; j < 3; j++) { //for '3' times do => |j|
					xtile = j - 1 + (position % BOARD_WIDTH);
					if (xtile < 0 || xtile >= BOARD_WIDTH) continue;
					@sprites[$"tile{xtile + (ytile * BOARD_WIDTH)}"].layer -= pattern[j + (i * 3)];
				}
			}
			if (@sprites["cursor"].mode == 1) {   // Hammer
				SEPlay("Mining hammer");
			} else {
				SEPlay("Mining pick");
			}
		}
		update;
		Graphics.update;
		hititem = (@sprites[$"tile{position}"].layer == 0 && IsItemThere(position));
		if (hititem) hittype = 1;
		@sprites["cursor"].animate(hittype);
		revealed = CheckRevealed;
		if (revealed.length > 0) {
			SEPlay("Mining reveal full");
			FlashItems(revealed);
		} else if (hititem) {
			SEPlay("Mining reveal");
		}
	}

	public bool IsItemThere(position) {
		posx = position % BOARD_WIDTH;
		posy = position / BOARD_WIDTH;
		@items.each do |i|
			index = i[0];
			width = ITEMS[index][4];
			height = ITEMS[index][5];
			pattern = ITEMS[index][6];
			if (posx < i[1] || posx >= (i[1] + width)) continue;
			if (posy < i[2] || posy >= (i[2] + height)) continue;
			dx = posx - i[1];
			dy = posy - i[2];
			if (pattern[dx + (dy * width)] > 0) return true;
		}
		return false;
	}

	public bool IsIronThere(position) {
		posx = position % BOARD_WIDTH;
		posy = position / BOARD_WIDTH;
		@iron.each do |i|
			index = i[0];
			width = IRON[index][2];
			height = IRON[index][3];
			pattern = IRON[index][4];
			if (posx < i[1] || posx >= (i[1] + width)) continue;
			if (posy < i[2] || posy >= (i[2] + height)) continue;
			dx = posx - i[1];
			dy = posy - i[2];
			if (pattern[dx + (dy * width)] > 0) return true;
		}
		return false;
	}

	public void CheckRevealed() {
		ret = new List<string>();
		for (int i = @items.length; i < @items.length; i++) { //for '@items.length' times do => |i|
			if (@items[i][3]) continue;
			revealed = true;
			index = @items[i][0];
			width = ITEMS[index][4];
			height = ITEMS[index][5];
			pattern = ITEMS[index][6];
			for (int j = height; j < height; j++) { //for 'height' times do => |j|
				for (int k = width; k < width; k++) { //for 'width' times do => |k|
					layer = @sprites[$"tile{@items[i][1] + k + ((@items[i][2] + j) * BOARD_WIDTH)}"].layer;
					if (layer > 0 && pattern[k + (j * width)] > 0) revealed = false;
					if (!revealed) break;
				}
				if (!revealed) break;
			}
			if (revealed) ret.Add(i);
		}
		return ret;
	}

	public void FlashItems(revealed) {
		if (revealed.length <= 0) return;
		revealeditems = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		revealeditems.z = 15;
		revealeditems.color = new Color(255, 255, 255, 0);
		flash_duration = 0.25;
		for (int i = 2; i < 2; i++) { //for '2' times do => |i|
			alpha_start = (i == 0) ? 0 : 255;
			alpha_end = (i == 0) ? 255 : 0;
			timer_start = System.uptime;
			do { //loop; while (true);
				foreach (var index in revealed) { //'revealed.each' do => |index|
					burieditem = @items[index];
					revealeditems.bitmap.blt(32 * burieditem[1], 64 + (32 * burieditem[2]),
																	@itembitmap.bitmap,
																	new Rect(32 * ITEMS[burieditem[0]][2], 32 * ITEMS[burieditem[0]][3],
																						32 * ITEMS[burieditem[0]][4], 32 * ITEMS[burieditem[0]][5]));
				}
				flash_alpha = lerp(alpha_start, alpha_end, flash_duration / 2, timer_start, System.uptime);
				revealeditems.color.alpha = flash_alpha;
				update;
				Graphics.update;
				if (flash_alpha == alpha_end) break;
			}
		}
		revealeditems.dispose;
		foreach (var index in revealed) { //'revealed.each' do => |index|
			@items[index][3] = true;
			item = ITEMS[@items[index][0]][0];
			@itemswon.Add(item);
		}
	}

	public void Main() {
		SEPlay("Mining ping");
		Message(_INTL("Something pinged in the wall!\n{1} confirmed!", @items.length));
		do { //loop; while (true);
			update;
			Graphics.update;
			Input.update;
			if (@sprites["cursor"].isAnimating()) continue;
			// Check end conditions
			if (@sprites["crack"].hits >= 49) {
				@sprites["cursor"].visible = false;
				SEPlay("Mining collapse");
				@sprites["collapse"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
				@sprites["collapse"].z = 999;
				timer_start = System.uptime;
				do { //loop; while (true);
					collapse_height = lerp(0, Graphics.height, 0.8, timer_start, System.uptime);
					@sprites["collapse"].bitmap.fill_rect(0, 0, Graphics.width, collapse_height, Color.black);
					Graphics.update;
					if (collapse_height == Graphics.height) break;
				}
				Message(_INTL("The wall collapsed!"));
				break;
			}
			foundall = true;
			@items.each do |i|
				if (!i[3]) foundall = false;
				if (!foundall) break;
			}
			if (foundall) {
				@sprites["cursor"].visible = false;
				Wait(0.75);
				SEPlay("Mining found all");
				Message(_INTL("Everything was dug up!"));
				break;
			}
			// Input
			if (Input.trigger(Input.UP) || Input.repeat(Input.UP)) {
				if (@sprites["cursor"].position >= BOARD_WIDTH) {
					SEPlay("Mining cursor");
					@sprites["cursor"].position -= BOARD_WIDTH;
				}
			} else if (Input.trigger(Input.DOWN) || Input.repeat(Input.DOWN)) {
				if (@sprites["cursor"].position < (BOARD_WIDTH * (BOARD_HEIGHT - 1))) {
					SEPlay("Mining cursor");
					@sprites["cursor"].position += BOARD_WIDTH;
				}
			} else if (Input.trigger(Input.LEFT) || Input.repeat(Input.LEFT)) {
				if (@sprites["cursor"].position % BOARD_WIDTH > 0) {
					SEPlay("Mining cursor");
					@sprites["cursor"].position -= 1;
				}
			} else if (Input.trigger(Input.RIGHT) || Input.repeat(Input.RIGHT)) {
				if (@sprites["cursor"].position % BOARD_WIDTH < (BOARD_WIDTH - 1)) {
					SEPlay("Mining cursor");
					@sprites["cursor"].position += 1;
				}
			} else if (Input.trigger(Input.ACTION)) {   // Change tool mode
				SEPlay("Mining tool change");
				newmode = (@sprites["cursor"].mode + 1) % 2;
				@sprites["cursor"].mode = newmode;
				@sprites["tool"].src_rect.set(newmode * 68, 0, 68, 100);
				@sprites["tool"].y = 254 - (144 * newmode);
			} else if (Input.trigger(Input.USE)) {   // Hit
				Hit;
			} else if (Input.trigger(Input.BACK)) {   // Quit
				if (ConfirmMessage(_INTL("Are you sure you want to give up?"))) break;
			}
		}
		GiveItems;
	}

	public void GiveItems() {
		if (@itemswon.length > 0) {
			@itemswon.each do |i|
				if (Game.GameData.bag.add(i)) {
					Message(_INTL("One {1} was obtained.", GameData.Item.get(i).name) + "\\se[Mining item get]\\wtnp[30]");
				} else {
					Message(_INTL("One {1} was found, but you have no room for it.",
													GameData.Item.get(i).name));
				}
			}
		}
	}

	public void EndScene() {
		FadeOutAndHide(@sprites);
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}
}

//===============================================================================
//
//===============================================================================
public partial class MiningGame {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen() {
		@scene.StartScene;
		@scene.Main;
		@scene.EndScene;
	}
}

//===============================================================================
//
//===============================================================================
public void MiningGame() {
	FadeOutIn do;
		scene = new MiningGameScene();
		screen = new MiningGame(scene);
		screen.StartScreen;
	}
}
