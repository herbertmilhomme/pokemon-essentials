//===============================================================================
// "Tile Puzzle" mini-games.
// By Maruno.
// Graphics by the__end.
//-------------------------------------------------------------------------------
// Run with:      TilePuzzle(game,board,width,height)
// game = 1 (Ruins of Alph puzzle),
//        2 (Ruins of Alph puzzle plus tile rotations),
//        3 (Mystic Square),
//        4 (swap two adjacent tiles),
//        5 (swap two adjacent tiles plus tile rotations),
//        6 (Rubik's square),
//        7 (rotate selected tile plus adjacent tiles at once).
// board = The name/number of the graphics to be used.
// width,height = Optional, the number of tiles wide/high the puzzle is (0 for
//                the default value of 4).
//===============================================================================
public partial class TilePuzzleCursor : BitmapSprite {
	public int game		{ get { return _game; } set { _game = value; } }			protected int _game;
	public int position		{ get { return _position; } set { _position = value; } }			protected int _position;
	public int arrows		{ get { return _arrows; } set { _arrows = value; } }			protected int _arrows;
	public int selected		{ get { return _selected; } set { _selected = value; } }			protected int _selected;
	public int holding		{ get { return _holding; } set { _holding = value; } }			protected int _holding;

	public override void initialize(game, position, tilewidth, tileheight, boardwidth, boardheight) {
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		base.initialize(Graphics.width, Graphics.height, @viewport);
		@game = game;
		@position = position;
		@tilewidth = tilewidth;
		@tileheight = tileheight;
		@boardwidth = boardwidth;
		@boardheight = boardheight;
		@arrows = new List<string>();
		@selected = false;
		@holding = false;
		@cursorbitmap = new AnimatedBitmap("Graphics/UI/Tile Puzzle/cursor");
		update;
	}

	public void update() {
		self.bitmap.clear;
		x = (Graphics.width - (@tilewidth * @boardwidth)) / 2;
		if (@position >= @boardwidth * @boardheight) {
			x = ((x - (@tilewidth * (@boardwidth / 2).ceil)) / 2) - 10;
			if ((@position % @boardwidth) >= (@boardwidth / 2).ceil) {
				x = Graphics.width - x - (@tilewidth * @boardwidth);
			}
		}
		x += @tilewidth * (@position % @boardwidth);
		y = ((Graphics.height - (@tileheight * @boardheight)) / 2) - 32;
		y += @tileheight * ((@position % (@boardwidth * @boardheight)) / @boardwidth);
		self.tone = new Tone(0, (@holding ? 64 : 0), (@holding ? 64 : 0), 0);
		// Cursor
		if (@game != 3) {
			expand = (@holding) ? 0 : 4;
			for (int i = 4; i < 4; i++) { //for '4' times do => |i|
				self.bitmap.blt(
					x + ((i % 2) * (@tilewidth - (@cursorbitmap.width / 4))) + (expand * (((i % 2) * 2) - 1)),
					y + ((i / 2) * (@tileheight - (@cursorbitmap.height / 2))) + (expand * (((i / 2) * 2) - 1)),
					@cursorbitmap.bitmap, new Rect((i % 2) * @cursorbitmap.width / 4,
																				(i / 2) * @cursorbitmap.height / 2,
																				@cursorbitmap.width / 4, @cursorbitmap.height / 2)
				);
			}
		}
		// Arrows
		if (@selected || @game == 3) {
			expand = (@game == 3) ? 0 : 4;
			xin = new {(@tilewidth - (@cursorbitmap.width / 4)) / 2, -expand,
						@tilewidth - (@cursorbitmap.width / 4) + expand, (@tilewidth - (@cursorbitmap.width / 4)) / 2};
			yin = new {@tileheight - (@cursorbitmap.height / 2) + expand, (@tileheight - (@cursorbitmap.height / 2)) / 2,
						(@tileheight - (@cursorbitmap.height / 2)) / 2, -expand}
			for (int i = 4; i < 4; i++) { //for '4' times do => |i|
				if (!@arrows[i]) continue;
				self.bitmap.blt(x + xin[i], y + yin[i], @cursorbitmap.bitmap,
												new Rect((@cursorbitmap.width / 2) + ((i % 2) * (@cursorbitmap.width / 4)),
																(i / 2) * (@cursorbitmap.height / 2),
																@cursorbitmap.width / 4, @cursorbitmap.height / 2));
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class TilePuzzleScene {
	public void initialize(game, board, width, height) {
		@game = game;
		@board = board;
		@boardwidth = (width > 0) ? width : 4;
		@boardheight = (height > 0) ? height : 4;
	}

	public void update() {
		xtop = (Graphics.width - (@tilewidth * @boardwidth)) / 2;
		ytop = ((Graphics.height - (@tileheight * @boardheight)) / 2) + (@tileheight / 2) - 32;
		for (int i = (@boardwidth * @boardheight); i < (@boardwidth * @boardheight); i++) { //for '(@boardwidth * @boardheight)' times do => |i|
			pos = -1;
			for (int j = @tiles.length; j < @tiles.length; j++) { //for '@tiles.length' times do => |j|
				if (@tiles[j] == i) pos = j;
			}
			@sprites[$"tile{i}"].z = 0;
			@sprites[$"tile{i}"].tone = new Tone(0, 0, 0, 0);
			if (@heldtile == i) {
				pos = @sprites["cursor"].position;
				@sprites[$"tile{i}"].z = 1;
				if (@tiles[pos] >= 0) @sprites[$"tile{i}"].tone = new Tone(64, 0, 0, 0);
			}
			thisx = xtop;
			if (pos >= 0) {
				if (pos >= @boardwidth * @boardheight) {
					thisx = ((xtop - (@tilewidth * (@boardwidth / 2).ceil)) / 2) - 10;
					if ((pos % @boardwidth) >= (@boardwidth / 2).ceil) {
						thisx = Graphics.width - thisx - (@tilewidth * @boardwidth);
					}
				}
				@sprites[$"tile{i}"].x = thisx + (@tilewidth * (pos % @boardwidth)) + (@tilewidth / 2);
				@sprites[$"tile{i}"].y = ytop + (@tileheight * ((pos % (@boardwidth * @boardheight)) / @boardwidth));
				if (@game == 3) continue;
				rotatebitmaps = new {@tilebitmap, @tilebitmap1, @tilebitmap2, @tilebitmap3};
				@sprites[$"tile{i}"].bitmap.clear;
				if (rotatebitmaps[@angles[i]]) {
					@sprites[$"tile{i}"].bitmap.blt(0, 0, rotatebitmaps[@angles[i]].bitmap,
																					new Rect(@tilewidth * (i % @boardwidth), @tileheight * (i / @boardwidth), @tilewidth, @tileheight));
					@sprites[$"tile{i}"].angle = 0;
				} else {
					@sprites[$"tile{i}"].bitmap.blt(0, 0, @tilebitmap.bitmap,
																					new Rect(@tilewidth * (i % @boardwidth), @tileheight * (i / @boardwidth), @tilewidth, @tileheight));
					@sprites[$"tile{i}"].angle = @angles[i] * 90;
				}
			}
		}
		updateCursor;
		UpdateSpriteHash(@sprites);
	}

	public void updateCursor() {
		arrows = new List<string>();
		for (int i = 4; i < 4; i++) { //for '4' times do => |i|
			arrows.Add(CanMoveInDir(@sprites["cursor"].position, (i + 1) * 2, @game == 6));
		}
		@sprites["cursor"].arrows = arrows;
	}

	public void StartScene() {
		@sprites = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		if (ResolveBitmap($"Graphics/UI/Tile Puzzle/bg{@board}")) {
			addBackgroundPlane(@sprites, "bg", $"Tile Puzzle/bg{@board}", @viewport);
		} else {
			addBackgroundPlane(@sprites, "bg", "Tile Puzzle/bg", @viewport);
		}
		@tilebitmap = new AnimatedBitmap($"Graphics/UI/Tile Puzzle/tiles{@board}");
		@tilebitmap1 = null;
		@tilebitmap2 = null;
		@tilebitmap3 = null;
		if (ResolveBitmap($"Graphics/UI/Tile Puzzle/tiles{@board}_1")) {
			@tilebitmap1 = new AnimatedBitmap($"Graphics/UI/Tile Puzzle/tiles{@board}_1");
		}
		if (ResolveBitmap($"Graphics/UI/Tile Puzzle/tiles{@board}_2")) {
			@tilebitmap2 = new AnimatedBitmap($"Graphics/UI/Tile Puzzle/tiles{@board}_2");
		}
		if (ResolveBitmap($"Graphics/UI/Tile Puzzle/tiles{@board}_3")) {
			@tilebitmap3 = new AnimatedBitmap($"Graphics/UI/Tile Puzzle/tiles{@board}_3");
		}
		@tilewidth = @tilebitmap.width / @boardwidth;
		@tileheight = @tilebitmap.height / @boardheight;
		for (int i = (@boardwidth * @boardheight); i < (@boardwidth * @boardheight); i++) { //for '(@boardwidth * @boardheight)' times do => |i|
			@sprites[$"tile{i}"] = new BitmapSprite(@tilewidth, @tileheight, @viewport);
			@sprites[$"tile{i}"].ox = @tilewidth / 2;
			@sprites[$"tile{i}"].oy = @tileheight / 2;
			if (@game == 3 && i >= (@boardwidth * @boardheight) - 1) break;
			@sprites[$"tile{i}"].bitmap.blt(0, 0, @tilebitmap.bitmap,
																			new Rect(@tilewidth * (i % @boardwidth), @tileheight * (i / @boardwidth), @tilewidth, @tileheight));
		}
		@heldtile = -1;
		@angles = new List<string>();
		@tiles = ShuffleTiles;
		@sprites["cursor"] = new TilePuzzleCursor(@game, DefaultCursorPosition,
																							@tilewidth, @tileheight, @boardwidth, @boardheight);
		update;
		FadeInAndShow(@sprites);
	}

	public void ShuffleTiles() {
		ret = new List<string>();
		for (int i = (@boardwidth * @boardheight); i < (@boardwidth * @boardheight); i++) { //for '(@boardwidth * @boardheight)' times do => |i|
			ret.Add(i);
			@angles.Add(0);
		}
		switch (@game) {
			case 6:
				@tiles = ret;
				5.times do;
					ShiftLine(new {2, 4, 6, 8}[rand(4)], rand(@boardwidth * @boardheight), false);
				}
				return @tiles;
			case 7:
				@tiles = ret;
				5.times do;
					RotateTile(rand(@boardwidth * @boardheight), false);
				}
				break;
			default:
				ret.shuffle!;
				if (@game == 3) {  // Make sure only solvable Mystic Squares are allowed.
					num = 0;
					blank = -1;
					for (int i = (ret.length - 1); i < (ret.length - 1); i++) { //for '(ret.length - 1)' times do => |i|
						if (ret[i] == (@boardwidth * @boardheight) - 1) blank = i;
						for (int j = i; j < ret.length; j++) { //each 'ret.length' do => |j|
							if (ret[j] < ret[i] && ret[i] != (@boardwidth * @boardheight) - 1 &&
													ret[j] != (@boardwidth * @boardheight) - 1) num += 1;
						}
					}
					if (@boardwidth.odd()) {
						if (num.odd()) ret = ShuffleTiles;
					} else if (num.even() == (@boardheight - (blank / @boardwidth)).even()) {
						ret = ShuffleTiles;
					}
				}
				if (@game == 1 || @game == 2) {
					ret2 = new List<string>();
					for (int i = (@boardwidth * @boardheight); i < (@boardwidth * @boardheight); i++) { //for '(@boardwidth * @boardheight)' times do => |i|
						ret2.Add(-1);
					}
					ret = ret2 + ret;
				}
				if (@game == 2 || @game == 5) {
					for (int i = @angles.length; i < @angles.length; i++) { //for '@angles.length' times do => |i|
						@angles[i] = rand(4);
					}
				}
				break;
		}
		return ret;
	}

	public void DefaultCursorPosition() {
		if (@game == 3) {
			for (int i = (@boardwidth * @boardheight); i < (@boardwidth * @boardheight); i++) { //for '(@boardwidth * @boardheight)' times do => |i|
				if (@tiles[i] == (@boardwidth * @boardheight) - 1) return i;
			}
		}
		return 0;
	}

	public void MoveCursor(pos, dir) {
		switch (dir) {
			case 2:
				pos += @boardwidth;
				break;
			case 4:
				if (pos >= @boardwidth * @boardheight) {
					if (pos % @boardwidth == (@boardwidth / 2).ceil) {
						pos = (((pos % (@boardwidth * @boardheight)) / @boardwidth) * @boardwidth) + @boardwidth - 1;
					} else {
						pos -= 1;
					}
				} else if ((pos % @boardwidth) == 0) {
					pos = (((pos / @boardwidth) + @boardheight) * @boardwidth) + (@boardwidth / 2).ceil - 1;
				} else {
					pos -= 1;
				}
				break;
			case 6:
				if (pos >= @boardwidth * @boardheight) {
					if (pos % @boardwidth == (@boardwidth / 2).ceil - 1) {
						pos = ((pos % (@boardwidth * @boardheight)) / @boardwidth) * @boardwidth;
					} else {
						pos += 1;
					}
				} else if (pos % @boardwidth >= @boardwidth - 1) {
					pos = (((pos / @boardwidth) + @boardheight) * @boardwidth) + (@boardwidth / 2).ceil;
				} else {
					pos += 1;
				}
				break;
			case 8:
				pos -= @boardwidth;
				break;
		}
		return pos;
	}

	public bool CanMoveInDir(pos, dir, swapping) {
		if (@game == 6 && swapping) return true;
		switch (dir) {
			case 2:
				if ((pos / @boardwidth) % @boardheight >= @boardheight - 1) return false;
				break;
			case 4:
				if (@game == 1 || @game == 2) {
					if (pos >= @boardwidth * @boardheight && pos % @boardwidth == 0) return false;
				} else {
					if (pos % @boardwidth == 0) return false;
				}
				break;
			case 6:
				if (@game == 1 || @game == 2) {
					if (pos >= @boardwidth * @boardheight && pos % @boardwidth >= @boardwidth - 1) return false;
				} else {
					if (pos % @boardwidth >= @boardwidth - 1) return false;
				}
				break;
			case 8:
				if ((pos / @boardwidth) % @boardheight == 0) return false;
				break;
		}
		return true;
	}

	public void RotateTile(pos, anim = true) {
		if (@heldtile >= 0) {
			if (anim) {
				@sprites["cursor"].visible = false;
				@sprites[$"tile{@heldtile}"].z = 1;
				old_angle = @sprites[$"tile{@heldtile}"].angle;
				timer_start = System.uptime;
				do { //loop; while (true);
					@sprites[$"tile{@heldtile}"].angle = lerp(old_angle, old_angle - 90, 0.25, timer_start, System.uptime);
					UpdateSpriteHash(@sprites);
					Graphics.update;
					Input.update;
					if (@sprites[$"tile{@heldtile}"].angle == old_angle - 90) break;
				}
				@sprites[$"tile{@heldtile}"].z = 0;
				if (!CheckWin) @sprites["cursor"].visible = true;
			}
			@angles[@heldtile] -= 1;
			if (@angles[@heldtile] < 0) @angles[@heldtile] += 4;
		} else {
			if (@tiles[pos] < 0) return;
			group = GetNearTiles(pos);
			if (anim) {
				@sprites["cursor"].visible = false;
				old_angles = new List<string>();
				foreach (var i in group) { //'group.each' do => |i|
					@sprites[$"tile{@tiles[i]}"].z = 1;
					old_angles.Add(@sprites[$"tile{@tiles[i]}"].angle);
				}
				timer_start = System.uptime;
				do { //loop; while (true);
					group.each_with_index do |idx, i|
						@sprites[$"tile{@tiles[idx]}"].angle = lerp(old_angles[i], old_angles[i] - 90, 0.25, timer_start, System.uptime);
					}
					UpdateSpriteHash(@sprites);
					Graphics.update;
					Input.update;
					if (@sprites[$"tile{@tiles[group[0]]}"].angle == old_angles[0] - 90) break;
				}
				foreach (var i in group) { //'group.each' do => |i|
					@sprites[$"tile{@tiles[i]}"].z = 0;
				}
				if (!CheckWin) @sprites["cursor"].visible = true;
			}
			foreach (var i in group) { //'group.each' do => |i|
				tile = @tiles[i];
				@angles[tile] -= 1;
				if (@angles[tile] < 0) @angles[tile] += 4;
			}
		}
	}

	public void GetNearTiles(pos) {
		ret = [pos];
		if (@game == 7) {
			new {2, 4, 6, 8}.each do |i|
				if (CanMoveInDir(pos, i, true)) ret.Add(MoveCursor(pos, i));
			}
		}
		return ret;
	}

	public void SwapTiles(dir) {
		cursor = @sprites["cursor"].position;
		if (@game == 6) return ShiftLine(dir, cursor);
		movetile = MoveCursor(cursor, dir);
		@sprites["cursor"].visible = false;
		@sprites[$"tile{@tiles[cursor]}"].z = 1;
		duration = 0.3;
		timer_start = System.uptime;
		if (new []{2, 8}.Contains(dir)) {   // Swap vertically
			start_sprite_pos = @sprites[$"tile{@tiles[movetile]}"].y;
			start_cursor_pos = @sprites[$"tile{@tiles[cursor]}"].y;
			dist = (int)Math.Floor(dir / 4) - 1;
			do { //loop; while (true);
				delta_y = lerp(0, @tileheight * dist, duration, timer_start, System.uptime);
				@sprites[$"tile{@tiles[movetile]}"].y = start_sprite_pos + delta_y;
				@sprites[$"tile{@tiles[cursor]}"].y = start_cursor_pos - delta_y;
				UpdateSpriteHash(@sprites);
				Graphics.update;
				Input.update;
				if (@sprites[$"tile{@tiles[movetile]}"].y == start_sprite_pos + (@tileheight * dist)) break;
			}
		} else {   // Swap horizontally
			start_sprite_pos = @sprites[$"tile{@tiles[movetile]}"].x;
			start_cursor_pos = @sprites[$"tile{@tiles[cursor]}"].x;
			dist = dir - 5;
			do { //loop; while (true);
				delta_x = lerp(0, @tilewidth * dist, duration, timer_start, System.uptime);
				@sprites[$"tile{@tiles[movetile]}"].x = start_sprite_pos - delta_x;
				@sprites[$"tile{@tiles[cursor]}"].x = start_cursor_pos + delta_x;
				UpdateSpriteHash(@sprites);
				Graphics.update;
				Input.update;
				if (@sprites[$"tile{@tiles[movetile]}"].x == start_sprite_pos - (@tilewidth * dist)) break;
			}
		}
		@tiles[cursor], @tiles[movetile] = @tiles[movetile], @tiles[cursor];
		@sprites[$"tile{@tiles[cursor]}"].z = 0;
		@sprites["cursor"].position = movetile;
		@sprites["cursor"].selected = false;
		if (!CheckWin) @sprites["cursor"].visible = true;
		return true;
	}

	public void ShiftLine(dir, cursor, anim = true) {
		// Get tiles involved
		tiles = new List<string>();
		dist = 0;
		if (new []{2, 8}.Contains(dir)) {
			dist = (int)Math.Floor(dir / 4) - 1;
			while ((dist > 0 && cursor < (@boardwidth - 1) * @boardheight) ||) {
						(dist < 0 && cursor >= @boardwidth)
				cursor += (@boardwidth * dist);
			}
			for (int i = @boardheight; i < @boardheight; i++) { //for '@boardheight' times do => |i|
				tiles.Add(cursor - (i * dist * @boardwidth));
			}
		} else {
			dist = dir - 5;
			while ((dist > 0 && cursor % @boardwidth > 0) ||) {
						(dist < 0 && cursor % @boardwidth < @boardwidth - 1)
				cursor -= dist;
			}
			for (int i = @boardwidth; i < @boardwidth; i++) { //for '@boardwidth' times do => |i|
				tiles.Add(cursor + (i * dist));
			}
		}
		// Shift tiles
		fade_duration = 0.4;
		if (anim) {
			@sprites["cursor"].visible = false;
			timer_start = System.uptime;
			do { //loop; while (true);
				end_tile = @sprites[$"tile{@tiles[tiles[tiles.length - 1]]}"];
				end_tile.opacity = lerp(255, 0, fade_duration, timer_start, System.uptime);
				Graphics.update;
				Input.update;
				if (end_tile.opacity == 0) break;
			}
			duration = 0.3;
			timer_start = System.uptime;
			start_pos = new List<string>();
			if (new []{2, 8}.Contains(dir)) {
				tiles.each { |i| start_pos.Add(@sprites[$"tile{@tiles[i]}"].y) };
				do { //loop; while (true);
					tiles.each_with_index do |idx, i|
						@sprites[$"tile{@tiles[idx]}"].y = lerp(start_pos[i], start_pos[i] - (@tileheight * dist), duration, timer_start, System.uptime);
					}
					UpdateSpriteHash(@sprites);
					Graphics.update;
					Input.update;
					if (@sprites[$"tile{@tiles[tiles[0]]}"].y == start_pos[0] - (@tileheight * dist)) break;
				}
			} else {
				tiles.each { |i| start_pos.Add(@sprites[$"tile{@tiles[i]}"].x) };
				do { //loop; while (true);
					tiles.each_with_index do |idx, i|
						@sprites[$"tile{@tiles[idx]}"].x = lerp(start_pos[i], start_pos[i] + (@tilewidth * dist), duration, timer_start, System.uptime);
					}
					UpdateSpriteHash(@sprites);
					Graphics.update;
					Input.update;
					if (@sprites[$"tile{@tiles[tiles[0]]}"].x == start_pos[0] + (@tilewidth * dist)) break;
				}
			}
		}
		temp = new List<string>();
		foreach (var i in tiles) { //'tiles.each' do => |i|
			temp.Add(@tiles[i]);
		}
		for (int i = temp.length; i < temp.length; i++) { //for 'temp.length' times do => |i|
			@tiles[tiles[(i + 1) % (temp.length)]] = temp[i];
		}
		if (anim) {
			update;
			timer_start = System.uptime;
			do { //loop; while (true);
				end_tile = @sprites[$"tile{@tiles[tiles[0]]}"];
				end_tile.opacity = lerp(0, 255, fade_duration, timer_start, System.uptime);
				Graphics.update;
				Input.update;
				if (end_tile.opacity == 255) break;
			}
			@sprites["cursor"].selected = false;
			if (!CheckWin) @sprites["cursor"].visible = true;
		}
		return true;
	}

	public void GrabTile(pos) {
		@heldtile, @tiles[pos] = @tiles[pos], @heldtile;
		@sprites["cursor"].holding = (@heldtile >= 0);
		if (CheckWin) @sprites["cursor"].visible = false;
	}

	public void CheckWin() {
		for (int i = (@boardwidth * @boardheight); i < (@boardwidth * @boardheight); i++) { //for '(@boardwidth * @boardheight)' times do => |i|
			if (@tiles[i] != i) return false;
			if (@angles[i] != 0) return false;
		}
		return true;
	}

	public void Main() {
		do { //loop; while (true);
			update;
			Graphics.update;
			Input.update;
			// Check end conditions
			if (CheckWin) {
				@sprites["cursor"].visible = false;
				if (@game == 3) {
					extratile = @sprites[$"tile{(@boardwidth * @boardheight) - 1}"];
					extratile.bitmap.clear;
					extratile.bitmap.blt(0, 0, @tilebitmap.bitmap,
															new Rect(@tilewidth * (@boardwidth - 1), @tileheight * (@boardheight - 1),
																				@tilewidth, @tileheight));
					extratile.opacity = 0;
					timer_start = System.uptime;
					do { //loop; while (true);
						extratile.opacity = lerp(0, 255, 0.8, timer_start, System.uptime);
						Graphics.update;
						Input.update;
						if (extratile.opacity >= 255) break;
					}
				} else {
					Wait(0.5);
				}
				do { //loop; while (true);
					Graphics.update;
					Input.update;
					if (Input.trigger(Input.USE) || Input.trigger(Input.BACK)) break;
				}
				return true;
			}
			// Input
			@sprites["cursor"].selected = (Input.press(Input.USE) && @game >= 3 && @game <= 6);
			dir = 0;
			if (Input.trigger(Input.DOWN) || Input.repeat(Input.DOWN)) dir = 2;
			if (Input.trigger(Input.LEFT) || Input.repeat(Input.LEFT)) dir = 4;
			if (Input.trigger(Input.RIGHT) || Input.repeat(Input.RIGHT)) dir = 6;
			if (Input.trigger(Input.UP) || Input.repeat(Input.UP)) dir = 8;
			if (dir > 0) {
				if (@game == 3 || (@game != 3 && @sprites["cursor"].selected)) {
					if (CanMoveInDir(@sprites["cursor"].position, dir, true)) {
						SEPlay("Tile Game cursor");
						SwapTiles(dir);
					}
				} else {
					if (CanMoveInDir(@sprites["cursor"].position, dir, false)) {
						SEPlay("Tile Game cursor");
						@sprites["cursor"].position = MoveCursor(@sprites["cursor"].position, dir);
					}
				}
			} else if ((@game == 1 || @game == 2) && Input.trigger(Input.USE)) {
				GrabTile(@sprites["cursor"].position);
			} else if ((@game == 2 && Input.trigger(Input.ACTION)) ||
						(@game == 5 && Input.trigger(Input.ACTION)) ||
						(@game == 7 && Input.trigger(Input.USE))) {
				RotateTile(@sprites["cursor"].position);
			} else if (Input.trigger(Input.BACK)) {
				return false;
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
public partial class TilePuzzle {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen() {
		@scene.StartScene;
		ret = @scene.Main;
		@scene.EndScene;
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public void TilePuzzle(game, board, width = 0, height = 0) {
	ret = false;
	FadeOutIn do;
		scene = new TilePuzzleScene(game, board, width, height);
		screen = new TilePuzzle(scene);
		ret = screen.StartScreen;
	}
	return ret;
}
