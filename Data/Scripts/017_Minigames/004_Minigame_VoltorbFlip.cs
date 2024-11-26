//===============================================================================
// "Voltorb Flip" mini-game.
// By KitsuneKouta.
//-------------------------------------------------------------------------------
// Run with:      VoltorbFlip
//===============================================================================
public partial class VoltorbFlip {
	public const string GRAPHICS_DIRECTORY = "Graphics/UI/Voltorb Flip/";
	public const int NUM_ROWS           = 5;
	public const int NUM_COLUMNS        = 5;
	public const int NUM_TILES          = NUM_ROWS * NUM_COLUMNS;
	TILE_DISTRIBUTIONS = new {   // Voltorbs, Twos, Threes, MaxFreePerRowOrCol, MaxFreeTotal
		// NOTE: The MaxFree values are not inclusive. The board will only be valid
		//       if (the corresponding counts are strictly less than these values.) {
		// Level 1
		new {
			new {6, 3, 1, 3, 3},
			new {6, 0, 3, 2, 2},
			new {6, 5, 0, 3, 4},
			new {6, 2, 2, 3, 3},
			new {6, 4, 1, 3, 4}
		},
		// Level 2
		new {
			new {7, 1, 3, 2, 3},
			new {7, 6, 0, 3, 4},
			new {7, 3, 2, 2, 3},
			new {7, 0, 4, 2, 3},
			new {7, 5, 1, 3, 4},
			new {7, 1, 3, 2, 2},
			new {7, 6, 0, 3, 3},
			new {7, 3, 2, 2, 2},
			new {7, 0, 4, 2, 2},
			new {7, 5, 1, 3, 3}
		},
		// Level 3
		new {
			new {8, 2, 3, 2, 3},
			new {8, 7, 0, 3, 4},
			new {8, 4, 2, 3, 4},
			new {8, 1, 4, 2, 3},
			new {8, 6, 1, 4, 3},
			new {8, 2, 3, 2, 2},
			new {8, 7, 0, 3, 3},
			new {8, 4, 2, 3, 3},
			new {8, 1, 4, 2, 2},
			new {8, 6, 1, 3, 3}
		},
		// Level 4
		new {
			new {8, 3, 3, 4, 3},
			new {8, 0, 5, 2, 3},
			new {10, 8, 0, 4, 5},
			new {10, 5, 2, 3, 4},
			new {10, 2, 4, 3, 4},
			new {8, 3, 3, 3, 3},
			new {8, 0, 5, 2, 2},
			new {10, 8, 0, 4, 4},
			new {10, 5, 2, 3, 3},
			new {10, 2, 4, 3, 3}
		},
		// Level 5
		new {
			new {10, 7, 1, 4, 5},
			new {10, 4, 3, 3, 4},
			new {10, 1, 5, 3, 4},
			new {10, 9, 0, 4, 5},
			new {10, 6, 2, 4, 5},
			new {10, 7, 1, 4, 4},
			new {10, 4, 3, 3, 3},
			new {10, 1, 5, 3, 3},
			new {10, 9, 0, 4, 4},
			new {10, 6, 2, 4, 4}
		},
		// Level 6
		new {
			new {10, 3, 4, 3, 4},
			new {10, 0, 6, 3, 4},
			new {10, 8, 1, 4, 5},
			new {10, 5, 3, 4, 5},
			new {10, 2, 5, 3, 4},
			new {10, 3, 4, 3, 3},
			new {10, 0, 6, 3, 3},
			new {10, 8, 1, 4, 4},
			new {10, 5, 3, 4, 4},
			new {10, 2, 5, 3, 3}
		},
		// Level 7
		new {
			new {10, 7, 2, 4, 5},
			new {10, 4, 4, 4, 5},
			new {13, 1, 6, 3, 4},
			new {13, 9, 1, 5, 6},
			new {10, 6, 3, 4, 5},
			new {10, 7, 2, 4, 4},
			new {10, 4, 4, 4, 4},
			new {13, 1, 6, 3, 3},
			new {13, 9, 1, 5, 5},
			new {10, 6, 3, 4, 4}
		},
		// Level 8
		new {
			new {10, 0, 7, 3, 4},
			new {10, 8, 2, 5, 6},
			new {10, 5, 4, 4, 5},
			new {10, 2, 6, 4, 5},
			new {10, 7, 3, 5, 6},
			new {10, 0, 7, 3, 3},
			new {10, 8, 2, 5, 5},
			new {10, 5, 4, 4, 4},
			new {10, 2, 6, 4, 4},
			new {10, 7, 3, 5, 5}
		}
	}

	public void update() {
		UpdateSpriteHash(@sprites);
	}

	public void Start() {
		@level = 1;
		@firstRound = true;
		NewGame;
	}

	public void generate_board() {
		ret = new List<string>();
		for (int attempt = 1000; attempt < 1000; attempt++) { //for '1000' times do => |attempt|
			board_distro = TILE_DISTRIBUTIONS[@level - 1].sample;
			// Randomly distribute tiles
			ret = [1] * NUM_TILES;
			index = 0;
			new {0, 2, 3}.each do |value|
				qty = board_distro[(int)Math.Max(value - 1, 0)];
				for (int i = qty; i < qty; i++) { //for 'qty' times do => |i|
					ret[index] = value;
					index += 1;
				}
			}
			ret.shuffle!;
			// Find how many Voltorbs are in each row/column
			row_voltorbs = [0] * NUM_ROWS;
			col_voltorbs = [0] * NUM_COLUMNS;
			ret.each_with_index do |val, i|
				if (val != 0) continue;
				row_voltorbs[i / NUM_COLUMNS] += 1;
				col_voltorbs[i % NUM_COLUMNS] += 1;
			}
			// Count the number of x2 and x3 tiles are free (i.e. no Voltorbs in its row/column)
			free_multipliers = 0;
			free_row = [0] * NUM_ROWS;
			free_col = [0] * NUM_COLUMNS;
			ret.each_with_index do |val, i|
				if (val <= 1) continue;
				if (row_voltorbs[i / NUM_COLUMNS] > 0 && col_voltorbs[i % NUM_COLUMNS] > 0) continue;
				free_multipliers += 1;
				free_row[i / NUM_COLUMNS] += 1;
				free_col[i % NUM_COLUMNS] += 1;
			}
			// Regnerate board if there are too many free multiplier tiles
			if (free_multipliers >= board_distro[4]) continue;
			if (free_row.any(i => i >= board_distro[3])) continue;
			if (free_col.any(i => i >= board_distro[3])) continue;
			// Board is valid; use it
			break;
		}
		return ret;
	}

	public void NewGame() {
		// Initialize variables
		@sprites = new List<string>();
		@cursor = new List<string>();
		@marks = new List<string>();
		@coins = new List<string>();
		@numbers = new List<string>();
		@voltorbNumbers = new List<string>();
		@points = 0;
		@index = new {0, 0};
		@squares = new List<string>();   // Each square is new {x, y, points, revealed}
		// Generate a board
		squareValues = generate_board;
		// Apply the generated board
		squareValues.each_with_index do |val, i|
			@squares[i] = new {((i % NUM_COLUMNS) * 64) + 128, (i / NUM_COLUMNS).abs * 64, val, false};
		}
		CreateSprites;
		// Display numbers (all zeroes, as no values have been calculated yet)
		NUM_ROWS.times(i => UpdateRowNumbers(0, 0, i));
		NUM_COLUMNS.times(i => UpdateColumnNumbers(0, 0, i));
		DrawShadowText(@sprites["text"].bitmap, 8, 22, 118, 26,
										_INTL("Your coins"), new Color(60, 60, 60), new Color(150, 190, 170), 1);
		DrawShadowText(@sprites["text"].bitmap, 8, 88, 118, 26,
										_INTL("Prize coins"), new Color(60, 60, 60), new Color(150, 190, 170), 1);
		// Draw current level
		DrawShadowText(@sprites["level"].bitmap, 8, 154, 118, 28,
										_INTL("Level {1}", @level.ToString()), new Color(60, 60, 60), new Color(150, 190, 170), 1);
		// Displays total and current coins
		UpdateCoins;
		// Draw curtain effect
		if (@firstRound) {
			curtain_duration = 0.5;
			timer_start = System.uptime;
			do { //loop; while (true);
				@sprites["curtainL"].angle = lerp(-90, -180, curtain_duration, timer_start, System.uptime);
				@sprites["curtainR"].angle = lerp(0, 90, curtain_duration, timer_start, System.uptime);
				Graphics.update;
				Input.update;
				update;
				if (@sprites["curtainL"].angle <= -180) break;
			}
		}
		@sprites["curtainL"].visible = false;
		@sprites["curtainR"].visible = false;
		@sprites["curtain"].opacity = 100;
		if (Game.GameData.player.coins >= Settings.MAX_COINS) {
			Message(_INTL("You've gathered {1} Coins. You cannot gather any more.", Settings.MAX_COINS.to_s_formatted));
			Game.GameData.player.coins = Settings.MAX_COINS;   // As a precaution
			@quit = true;
//    } else if (!ConfirmMessage(_INTL("Play Voltorb Flip Lv. {1}?", @level)) && Game.GameData.player.coins < Settings.MAX_COINS) {
//      @quit = true
		} else {
			@sprites["curtain"].opacity = 0;
			// Erase 0s to prepare to replace with values
			@sprites["numbers"].bitmap.clear;
			// Reset arrays to empty
			@voltorbNumbers = new List<string>();
			@numbers = new List<string>();
			// Draw numbers for each row (precautionary)
			for (int j = NUM_ROWS; j < NUM_ROWS; j++) { //for 'NUM_ROWS' times do => |j|
				num = 0;
				voltorbs = 0;
				for (int i = NUM_COLUMNS; i < NUM_COLUMNS; i++) { //for 'NUM_COLUMNS' times do => |i|
					val = @squares[i + (j * NUM_COLUMNS)][2];
					num += val;
					if (val == 0) voltorbs += 1;
				}
				UpdateRowNumbers(num, voltorbs, j);
			}
			// Reset arrays to empty
			@voltorbNumbers = new List<string>();
			@numbers = new List<string>();
			// Draw numbers for each column
			for (int i = NUM_COLUMNS; i < NUM_COLUMNS; i++) { //for 'NUM_COLUMNS' times do => |i|
				num = 0;
				voltorbs = 0;
				for (int j = NUM_ROWS; j < NUM_ROWS; j++) { //for 'NUM_ROWS' times do => |j|
					val = @squares[i + (j * NUM_COLUMNS)][2];
					num += val;
					if (val == 0) voltorbs += 1;
				}
				UpdateColumnNumbers(num, voltorbs, i);
			}
		}
	}

	public void CreateSprites() {
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites["bg"] = new Sprite(@viewport);
		@sprites["bg"].bitmap = RPG.Cache.load_bitmap(GRAPHICS_DIRECTORY, _INTL("Voltorb Flip bg"));
		@sprites["text"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		SetSystemFont(@sprites["text"].bitmap);
		@sprites["level"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		SetSystemFont(@sprites["level"].bitmap);
		@sprites["curtain"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["curtain"].z = 99999;
		@sprites["curtain"].bitmap.fill_rect(0, 0, Graphics.width, Graphics.height, Color.black);
		@sprites["curtain"].opacity = 0;
		@sprites["curtainL"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["curtainL"].z = 99999;
		@sprites["curtainL"].x = Graphics.width / 2;
		@sprites["curtainL"].angle = -90;
		@sprites["curtainL"].bitmap.fill_rect(0, 0, Graphics.width, Graphics.height, Color.black);
		@sprites["curtainR"] = new BitmapSprite(Graphics.width, Graphics.height * 2, @viewport);
		@sprites["curtainR"].z = 99999;
		@sprites["curtainR"].x = Graphics.width / 2;
		@sprites["curtainR"].bitmap.fill_rect(0, 0, Graphics.width, Graphics.height * 2, Color.black);
		@sprites["cursor"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["cursor"].z = 99998;
		@sprites["icon"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["icon"].z = 99997;
		@sprites["mark"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["memo"] = new Sprite(@viewport);
		@sprites["memo"].bitmap = RPG.Cache.load_bitmap(GRAPHICS_DIRECTORY, _INTL("memo"));
		@sprites["memo"].x = 10;
		@sprites["memo"].y = 244;
		@sprites["memo"].visible = false;
		@sprites["numbers"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["totalCoins"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["currentCoins"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["animation"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		@sprites["animation"].z = 99999;
		for (int i = 6; i < 6; i++) { //for '6' times do => |i|
			@sprites[i] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
			@sprites[i].z = 99996;
			@sprites[i].visible = false;
		}
		// Creates images ahead of time for the display-all animation (reduces lag)
		icons = new List<string>();
		points = 0;
		for (int i = 3; i < 3; i++) { //for '3' times do => |i|
			for (int j = NUM_TILES; j < NUM_TILES; j++) { //for 'NUM_TILES' times do => |j|
				if (i == 2) points = @squares[j][2];
				icons[j] = new {GRAPHICS_DIRECTORY + "tiles", @squares[j][0], @squares[j][1], 320 + (i * 64) + (points * 64), 0, 64, 64};
			}
			icons.compact!;
			DrawImagePositions(@sprites[i].bitmap, icons);
		}
		icons = new List<string>();
		for (int i = NUM_TILES; i < NUM_TILES; i++) { //for 'NUM_TILES' times do => |i|
			icons[i] = new {GRAPHICS_DIRECTORY + "tiles", @squares[i][0], @squares[i][1], @squares[i][2] * 64, 0, 64, 64};
		}
		DrawImagePositions(@sprites[5].bitmap, icons);
		// Default cursor image
		@cursor[0] = new {GRAPHICS_DIRECTORY + "cursor", 0 + 128, 0, 0, 0, 64, 64};
	}

	public void getInput() {
		if (Input.trigger(Input.UP)) {
			PlayCursorSE;
			if (@index[1] > 0) {
				@index[1] -= 1;
				@sprites["cursor"].y -= 64;
			} else {
				@index[1] = 4;
				@sprites["cursor"].y = 256;
			}
		} else if (Input.trigger(Input.DOWN)) {
			PlayCursorSE;
			if (@index[1] < 4) {
				@index[1] += 1;
				@sprites["cursor"].y += 64;
			} else {
				@index[1] = 0;
				@sprites["cursor"].y = 0;
			}
		} else if (Input.trigger(Input.LEFT)) {
			PlayCursorSE;
			if (@index[0] > 0) {
				@index[0] -= 1;
				@sprites["cursor"].x -= 64;
			} else {
				@index[0] = 4;
				@sprites["cursor"].x = 256;
			}
		} else if (Input.trigger(Input.RIGHT)) {
			PlayCursorSE;
			if (@index[0] < 4) {
				@index[0] += 1;
				@sprites["cursor"].x += 64;
			} else {
				@index[0] = 0;
				@sprites["cursor"].x = 0;
			}
		} else if (Input.trigger(Input.USE)) {
			if (@cursor[0][3] == 64) {   // If in mark mode
				for (int i = @squares.length; i < @squares.length; i++) { //for '@squares.length' times do => |i|
					if ((@index[0] * 64) + 128 == @squares[i][0] && @index[1] * 64 == @squares[i][1] && @squares[i][3] == false) {
						SEPlay("Voltorb Flip mark");
					}
				}
				for (int i = (@marks.length + 1); i < (@marks.length + 1); i++) { //for '(@marks.length + 1)' times do => |i|
					if (@marks[i].null()) {
						@marks[i] = new {GRAPHICS_DIRECTORY + "tiles", (@index[0] * 64) + 128, @index[1] * 64, 256, 0, 64, 64};
					} else if (@marks[i][1] == (@index[0] * 64) + 128 && @marks[i][2] == @index[1] * 64) {
						@marks.delete_at(i);
						@marks.compact!;
						@sprites["mark"].bitmap.clear;
						break;
					}
				}
				DrawImagePositions(@sprites["mark"].bitmap, @marks);
				Wait(0.05);
			} else {
				// Display the tile for the selected spot
				icons = new List<string>();
				for (int i = @squares.length; i < @squares.length; i++) { //for '@squares.length' times do => |i|
					if ((@index[0] * 64) + 128 == @squares[i][0] && @index[1] * 64 == @squares[i][1] && @squares[i][3] == false) {
						AnimateTile((@index[0] * 64) + 128, @index[1] * 64, @squares[i][2]);
						@squares[i][3] = true;
						// If Voltorb (0), display all tiles on the board
						if (@squares[i][2] == 0) {
							SEPlay("Voltorb Flip explosion");
							// Play explosion animation
							// Part1
							animation = new List<string>();
							for (int j = 3; j < 3; j++) { //for '3' times do => |j|
								animation[0] = icons[0] = new {GRAPHICS_DIRECTORY + "tiles", (@index[0] * 64) + 128, @index[1] * 64,
																					704 + (64 * j), 0, 64, 64};
								DrawImagePositions(@sprites["animation"].bitmap, animation);
								Wait(0.05);
								@sprites["animation"].bitmap.clear;
							}
							// Part2
							animation = new List<string>();
							for (int j = 6; j < 6; j++) { //for '6' times do => |j|
								animation[0] = new {GRAPHICS_DIRECTORY + "explosion", (@index[0] * 64) - 32 + 128, (@index[1] * 64) - 32,
																j * 128, 0, 128, 128};
								DrawImagePositions(@sprites["animation"].bitmap, animation);
								Wait(0.1);
								@sprites["animation"].bitmap.clear;
							}
							// Unskippable text block, parameter 2 = wait time (corresponds to ME length)
							Message("\\me[Voltorb Flip game over]" + _INTL("Oh no! You get 0 Coins!") + "\\wtnp[80]");
							ShowAndDispose;
							@sprites["mark"].bitmap.clear;
							if (@level > 1) {
								// Determine how many levels to reduce by
								newLevel = @squares.count(tile => tile[3] && tile[2] > 0);
								newLevel = newLevel.clamp(@level, 1);
								if (newLevel < @level) {
									@level = newLevel;
									Message("\\se[Voltorb Flip level down]" + _INTL("Dropped to Game Lv. {1}!", @level.ToString()));
								}
							}
							// Update level text
							@sprites["level"].bitmap.clear;
							DrawShadowText(@sprites["level"].bitmap, 8, 154, 118, 28, "Level " + @level.ToString(),
															new Color(60, 60, 60), new Color(150, 190, 170), 1);
							@points = 0;
							UpdateCoins;
							// Revert numbers to 0s
							@sprites["numbers"].bitmap.clear;
							NUM_ROWS.times(j => UpdateRowNumbers(0, 0, j));
							NUM_COLUMNS.times(j => UpdateColumnNumbers(0, 0, j));
							DisposeSpriteHash(@sprites);
							@firstRound = false;
							NewGame;
						} else {
							// Play tile animation
							animation = new List<string>();
							for (int j = 4; j < 4; j++) { //for '4' times do => |j|
								animation[0] = new {GRAPHICS_DIRECTORY + "flipAnimation", (@index[0] * 64) - 14 + 128, (@index[1] * 64) - 16,
																j * 92, 0, 92, 96};
								DrawImagePositions(@sprites["animation"].bitmap, animation);
								Wait(0.05);
								@sprites["animation"].bitmap.clear;
							}
							if (@points == 0) {
								@points += @squares[i][2];
								SEPlay("Voltorb Flip point");
							} else if (@squares[i][2] > 1) {
								@points *= @squares[i][2];
								SEPlay("Voltorb Flip point");
							}
							break;
						}
					}
				}
			}
			count = 0;
			for (int i = @squares.length; i < @squares.length; i++) { //for '@squares.length' times do => |i|
				if (@squares[i][3] == false && @squares[i][2] > 1) count += 1;
			}
			UpdateCoins;
			// Game cleared
			if (count == 0) {
				@sprites["curtain"].opacity = 100;
				Message("\\me[Voltorb Flip win]" + _INTL("Game clear!") + "\\wtnp[40]");
//        Message(_INTL("You've found all of the hidden x2 and x3 cards."))
//        Message(_INTL("This means you've found all the Coins in this game, so the game is now over."))
				Message("\\se[Voltorb Flip gain coins]" + _INTL("{1} received {2} Coins!", Game.GameData.player.name, @points.to_s_formatted));
				// Update level text
				@sprites["level"].bitmap.clear;
				DrawShadowText(@sprites["level"].bitmap, 8, 154, 118, 28, _INTL("Level {1}", @level.ToString()),
												new Color(60, 60, 60), new Color(150, 190, 170), 1);
				old_coins = Game.GameData.player.coins;
				Game.GameData.player.coins += @points;
				if (Game.GameData.player.coins > old_coins) Game.GameData.stats.coins_won += Game.GameData.player.coins - old_coins;
				@points = 0;
				UpdateCoins;
				@sprites["curtain"].opacity = 0;
				ShowAndDispose;
				// Revert numbers to 0s
				@sprites["numbers"].bitmap.clear;
				NUM_ROWS.times(i => UpdateRowNumbers(0, 0, i));
				NUM_COLUMNS.times(i => UpdateColumnNumbers(0, 0, i));
				@sprites["curtain"].opacity = 100;
				if (@level < 8) {
					@level += 1;
					Message("\\se[Voltorb Flip level up]" + _INTL("Advanced to Game Lv. {1}!", @level.ToString()));
					if (@firstRound) {
//            Message(_INTL("Congratulations!"))
//            Message(_INTL("You can receive even more Coins in the next game!"))
						@firstRound = false;
					}
				}
				DisposeSpriteHash(@sprites);
				NewGame;
			}
		} else if (Input.trigger(Input.ACTION)) {
			PlayDecisionSE;
			@sprites["cursor"].bitmap.clear;
			if (@cursor[0][3] == 0) {   // If in normal mode
				@cursor[0] = new {GRAPHICS_DIRECTORY + "cursor", 128, 0, 64, 0, 64, 64};
				@sprites["memo"].visible = true;
			} else {   // Mark mode
				@cursor[0] = new {GRAPHICS_DIRECTORY + "cursor", 128, 0, 0, 0, 64, 64};
				@sprites["memo"].visible = false;
			}
		} else if (Input.trigger(Input.BACK)) {
			@sprites["curtain"].opacity = 100;
			if (@points == 0) {
				if (ConfirmMessage("You haven't found any Coins! Are you sure you want to quit?")) {
					@sprites["curtain"].opacity = 0;
					ShowAndDispose;
					@quit = true;
				}
			} else if ((ConfirmMessage(_INTL("If you quit now, you will recieve {1} Coin(s). Will you quit?",
																	@points.to_s_formatted))) {
				Message(_INTL("{1} received {2} Coin(s)!", Game.GameData.player.name, @points.to_s_formatted));
				old_coins = Game.GameData.player.coins;
				Game.GameData.player.coins += @points;
				if (Game.GameData.player.coins > old_coins) Game.GameData.stats.coins_won += Game.GameData.player.coins - old_coins;
				@points = 0;
				UpdateCoins;
				@sprites["curtain"].opacity = 0;
				ShowAndDispose;
				@quit = true;
			}
			@sprites["curtain"].opacity = 0;
		}
		// Draw cursor
		DrawImagePositions(@sprites["cursor"].bitmap, @cursor);
	}

	public void UpdateRowNumbers(num, voltorbs, i) {
		numText = string.Format("{0:2}", num);
		numText.chars.each_with_index do |digit, j|
			@numbers[j] = new {GRAPHICS_DIRECTORY + "numbersSmall", 472 + (j * 16), 8 + (i * 64), digit.ToInt() * 16, 0, 16, 16};
		}
		@voltorbNumbers[i] = new {GRAPHICS_DIRECTORY + "numbersSmall", 488, 34 + (i * 64), voltorbs * 16, 0, 16, 16};
		// Display the numbers
		DrawImagePositions(@sprites["numbers"].bitmap, @numbers);
		DrawImagePositions(@sprites["numbers"].bitmap, @voltorbNumbers);
	}

	public void UpdateColumnNumbers(num, voltorbs, i) {
		numText = string.Format("{0:2}", num);
		numText.chars.each_with_index do |digit, j|
			@numbers[j] = new {GRAPHICS_DIRECTORY + "numbersSmall", 152 + (i * 64) + (j * 16), 328, digit.ToInt() * 16, 0, 16, 16};
		}
		@voltorbNumbers[i] = new {GRAPHICS_DIRECTORY + "numbersSmall", 168 + (i * 64), 354, voltorbs * 16, 0, 16, 16};
		// Display the numbers
		DrawImagePositions(@sprites["numbers"].bitmap, @numbers);
		DrawImagePositions(@sprites["numbers"].bitmap, @voltorbNumbers);
	}

	public void CreateCoins(source, y) {
		coinText = string.Format("{0:5}", source);
		coinText.chars.each_with_index do |digit, i|
			@coins[i] = new {GRAPHICS_DIRECTORY + "numbersScore", 6 + (i * 24), y, digit.ToInt() * 24, 0, 24, 38};
		}
	}

	public void UpdateCoins() {
		// Update coins display
		@sprites["totalCoins"].bitmap.clear;
		CreateCoins(Game.GameData.player.coins, 46);
		DrawImagePositions(@sprites["totalCoins"].bitmap, @coins);
		// Update points display
		@sprites["currentCoins"].bitmap.clear;
		CreateCoins(@points, 112);
		DrawImagePositions(@sprites["currentCoins"].bitmap, @coins);
	}

	public void AnimateTile(x, y, tile) {
		icons = new List<string>();
		points = 0;
		for (int i = 3; i < 3; i++) { //for '3' times do => |i|
			if (i == 2) points = tile;
			icons[i] = new {GRAPHICS_DIRECTORY + "tiles", x, y, 320 + (i * 64) + (points * 64), 0, 64, 64};
			DrawImagePositions(@sprites["icon"].bitmap, icons);
			Wait(0.05);
		}
		icons[3] = new {GRAPHICS_DIRECTORY + "tiles", x, y, tile * 64, 0, 64, 64};
		DrawImagePositions(@sprites["icon"].bitmap, icons);
		SEPlay("Voltorb Flip tile");
	}

	public void ShowAndDispose() {
		// Make pre-rendered sprites visible (this approach reduces lag)
		for (int i = 5; i < 5; i++) { //for '5' times do => |i|
			@sprites[i].visible = true;
			if (i < 3) Wait(0.05);
			@sprites[i].bitmap.clear;
			@sprites[i].z = 99997;
		}
		SEPlay("Voltorb Flip tile");
		@sprites[5].visible = true;
		@sprites["mark"].bitmap.clear;
		Wait(0.1);
		// Wait for user input to continue
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			update;
			if (Input.trigger(Input.USE) || Input.trigger(Input.BACK)) {
				break;
			}
		}
		// "Dispose" of tiles by column
		for (int i = NUM_COLUMNS; i < NUM_COLUMNS; i++) { //for 'NUM_COLUMNS' times do => |i|
			icons = new List<string>();
			SEPlay("Voltorb Flip tile");
			for (int j = NUM_ROWS; j < NUM_ROWS; j++) { //for 'NUM_ROWS' times do => |j|
				icons[j] = new {GRAPHICS_DIRECTORY + "tiles", @squares[i + (j * NUM_COLUMNS)][0], @squares[i + (j * NUM_COLUMNS)][1],
										448 + (@squares[i + (j * NUM_COLUMNS)][2] * 64), 0, 64, 64};
			}
			DrawImagePositions(@sprites[i].bitmap, icons);
			Wait(0.05);
			for (int j = NUM_ROWS; j < NUM_ROWS; j++) { //for 'NUM_ROWS' times do => |j|
				icons[j] = new {GRAPHICS_DIRECTORY + "tiles", @squares[i + (j * NUM_COLUMNS)][0], @squares[i + (j * NUM_COLUMNS)][1],
										384, 0, 64, 64};
			}
			DrawImagePositions(@sprites[i].bitmap, icons);
			Wait(0.05);
			for (int j = NUM_ROWS; j < NUM_ROWS; j++) { //for 'NUM_ROWS' times do => |j|
				icons[j] = new {GRAPHICS_DIRECTORY + "tiles", @squares[i + (j * NUM_COLUMNS)][0], @squares[i + (j * NUM_COLUMNS)][1],
										320, 0, 64, 64};
			}
			DrawImagePositions(@sprites[i].bitmap, icons);
			Wait(0.05);
			for (int j = NUM_ROWS; j < NUM_ROWS; j++) { //for 'NUM_ROWS' times do => |j|
				icons[j] = new {GRAPHICS_DIRECTORY + "tiles", @squares[i + (j * NUM_COLUMNS)][0], @squares[i + (j * NUM_COLUMNS)][1],
										896, 0, 64, 64};
			}
			DrawImagePositions(@sprites[i].bitmap, icons);
			Wait(0.05);
		}
		@sprites["icon"].bitmap.clear;
		for (int i = 6; i < 6; i++) { //for '6' times do => |i|
			@sprites[i].bitmap.clear;
		}
		@sprites["cursor"].bitmap.clear;
	}

//  public void WaitText(msg, frames) {
//    msgwindow = CreateMessageWindow
//    MessageDisplay(msgwindow, msg)
//    Wait(frames / 20.0)
//    DisposeMessageWindow(msgwindow)
//  }

	public void EndScene() {
		@sprites["curtainL"].angle = -180;
		@sprites["curtainR"].angle = 90;
		// Draw curtain effect
		@sprites["curtainL"].visible = true;
		@sprites["curtainR"].visible = true;
		curtain_duration = 0.25;
		timer_start = System.uptime;
		do { //loop; while (true);
			@sprites["curtainL"].angle = lerp(-180, -90, curtain_duration, timer_start, System.uptime);
			@sprites["curtainR"].angle = lerp(90, 0, curtain_duration, timer_start, System.uptime);
			Graphics.update;
			Input.update;
			update;
			if (@sprites["curtainL"].angle >= -90) break;
		}
		FadeOutAndHide(@sprites) { update };
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void Scene() {
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			getInput;
			if (@quit) break;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class VoltorbFlipScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen() {
		@scene.Start;
		@scene.Scene;
		@scene.EndScene;
	}
}

//===============================================================================
//
//===============================================================================
public void VoltorbFlip() {
	if (!Game.GameData.bag.has(:COINCASE)) {
		Message(_INTL("You can't play unless you have a Coin Case."));
	} else if (Game.GameData.player.coins == Settings.MAX_COINS) {
		Message(_INTL("Your Coin Case is full!"));
	} else {
		scene = new VoltorbFlip();
		screen = new VoltorbFlipScreen(scene);
		screen.StartScreen;
	}
}
