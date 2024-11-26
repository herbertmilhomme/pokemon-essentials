//===============================================================================
// "Slot Machine" mini-game.
// By Maruno.
//-------------------------------------------------------------------------------
// Run with:      SlotMachine(1)
// - The number is either 0 (easy), 1 (default) or 2 (hard).
//===============================================================================
public partial class SlotMachineReel : BitmapSprite {
	public const int SCROLL_SPEED = 640;   // Pixels moved per second
	ICONS_SETS = new {new {3, 2, 7, 6, 3, 1, 5, 2, 3, 0, 6, 4, 7, 5, 1, 3, 2, 3, 6, 0, 4, 5},   // Reel 1
								new {0, 4, 1, 2, 7, 4, 6, 0, 1, 5, 4, 0, 1, 3, 4, 0, 1, 6, 7, 0, 1, 5},   // Reel 2
								new {6, 2, 1, 4, 3, 2, 1, 4, 7, 3, 2, 1, 4, 3, 7, 2, 4, 3, 1, 2, 4, 5}}   // Reel 3
	SLIPPING = new {0, 0, 0, 0, 0, 0, 1, 1, 1, 2, 2, 3};

	public override void initialize(x, y, reel_num, difficulty = 1) {
		@viewport = new Viewport(x, y, 64, 144);
		@viewport.z = 99999;
		base.initialize(64, 144, @viewport);
		@reel_num = reel_num;
		@difficulty = difficulty;
		@reel = ICONS_SETS[reel_num - 1].clone;
		@toppos = 0;
		@current_y_pos = -1;
		@spin_speed = SCROLL_SPEED;
		@spin_speed /= 1.5 if difficulty == 0;
		@spinning = false;
		@stopping = false;
		@slipping = 0;
		@index = rand(@reel.length);
		@images = new AnimatedBitmap(_INTL("Graphics/UI/Slot Machine/images"));
		@shading = new AnimatedBitmap("Graphics/UI/Slot Machine/ReelOverlay");
		update;
	}

	public void startSpinning() {
		@spinning = true;
		@spin_timer_start = System.uptime;
		@initial_index = @index + 1;
		@current_y_pos = -1;
	}

	public bool spinning() {
		return @spinning;
	}

	public void stopSpinning(noslipping = false) {
		@stopping = true;
		@slipping = SLIPPING.sample;
		switch (@difficulty) {
			case 0:   // Easy
				second_slipping = SLIPPING.sample;
				@slipping = (int)Math.Min(@slipping, second_slipping);
				break;
			case 2:   // Hard
				second_slipping = SLIPPING.sample;
				@slipping = (int)Math.Max(@slipping, second_slipping);
				break;
		}
		if (noslipping) @slipping = 0;
	}

	public void showing() {
		array = new List<string>();
		for (int i = 3; i < 3; i++) { //for '3' times do => |i|
			num = @index - i;
			if (num < 0) num += @reel.length;
			array.Add(@reel[num]);
		}
		return array;   // [0] = top, [1] = middle, [2] = bottom
	}

	public void update() {
		self.bitmap.clear;
		if (@spinning) {
			new_y_pos = (System.uptime - @spin_timer_start) * @spin_speed;
			new_index = (new_y_pos / @images.height).ToInt();
			old_index = (@current_y_pos / @images.height).ToInt();
			@current_y_pos = new_y_pos;
			@toppos = new_y_pos;
			while (@toppos > 0) {
				@toppos -= @images.height;
			}
			if (new_index != old_index) {
				if (@stopping) {
					if (@slipping == 0) {
						@spinning = false;
						@stopping = false;
						@toppos = 0;
					} else {
						@slipping = (int)Math.Max(@slipping - new_index + old_index, 0);
					}
				}
				if (@spinning) {
					@index = (new_index + @initial_index) % @reel.length;
				}
			}
		}
		for (int i = 4; i < 4; i++) { //for '4' times do => |i|
			num = @index - i;
			if (num < 0) num += @reel.length;
			self.bitmap.blt(0, @toppos + (i * 48), @images.bitmap, new Rect(@reel[num] * 64, 0, 64, 48));
		}
		self.bitmap.blt(0, 0, @shading.bitmap, new Rect(0, 0, 64, 144));
	}
}

//===============================================================================
//
//===============================================================================
public partial class SlotMachineScore : BitmapSprite {
	public int score		{ get { return _score; } }			protected int _score;

	public override void initialize(x, y, score = 0) {
		@viewport = new Viewport(x, y, 70, 22);
		@viewport.z = 99999;
		base.initialize(70, 22, @viewport);
		@numbers = new AnimatedBitmap("Graphics/UI/Slot Machine/numbers");
		self.score = score;
	}

	public int score { set {
		@score = value;
		if (@score > Settings.MAX_COINS) @score = Settings.MAX_COINS;
		refresh;
		}
	}

	public void refresh() {
		self.bitmap.clear;
		for (int i = 5; i < 5; i++) { //for '5' times do => |i|
			digit = (@score / (10**i)) % 10; // Least significant digit first
			self.bitmap.blt(14 * (4 - i), 0, @numbers.bitmap, new Rect(digit * 14, 0, 14, 22));
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class SlotMachineScene {
	public int gameRunning		{ get { return _gameRunning; } set { _gameRunning = value; } }			protected int _gameRunning;
	public int gameEnd		{ get { return _gameEnd; } set { _gameEnd = value; } }			protected int _gameEnd;
	public int wager		{ get { return _wager; } set { _wager = value; } }			protected int _wager;
	public int replay		{ get { return _replay; } set { _replay = value; } }			protected int _replay;

	public void update() {
		UpdateSpriteHash(@sprites);
	}

	public void Payout() {
		@replay = false;
		payout = 0;
		bonus = 0;
		wonRow = new List<string>();
		// Get reel pictures
		reel1 = @sprites["reel1"].showing;
		reel2 = @sprites["reel2"].showing;
		reel3 = @sprites["reel3"].showing;
		combinations = new {new {reel1[1], reel2[1], reel3[1]},   // Centre row
										new {reel1[0], reel2[0], reel3[0]},   // Top row
										new {reel1[2], reel2[2], reel3[2]},   // Bottom row
										new {reel1[0], reel2[1], reel3[2]},   // Diagonal top left -> bottom right
										new {reel1[2], reel2[1], reel3[0]}}   // Diagonal bottom left -> top right
		for (int i = combinations.length; i < combinations.length; i++) { //for 'combinations.length' times do => |i|
			if (i >= 1 && @wager <= 1) break; // One coin = centre row only
			if (i >= 3 && @wager <= 2) break; // Two coins = three rows only
			wonRow[i] = true;
			switch (combinations[i]) {
				case new {1, 1, 1}:   // Three Magnemites
					payout += 8;
					break;
				case new {2, 2, 2}:   // Three Shellders
					payout += 8;
					break;
				case new {3, 3, 3}:   // Three Pikachus
					payout += 15;
					break;
				case new {4, 4, 4}:   // Three Psyducks
					payout += 15;
					break;
				case new {5, 5, 6}: case new {5, 6, 5}: case new {6, 5, 5}: case new {6, 6, 5}: case new {6, 5, 6}: case new {5, 6, 6}:   // 777 multi-colored
					payout += 90;
					if (bonus < 1) bonus = 1;
					break;
				case new {5, 5, 5}: case new {6, 6, 6}:   // Red 777, blue 777
					payout += 300;
					if (bonus < 2) bonus = 2;
					break;
				case new {7, 7, 7}:   // Three replays
					@replay = true;
					break;
				default:
					if (combinations[i][0] == 0) {   // Left cherry
						if (combinations[i][1] == 0) {   // Centre cherry as well
							payout += 4;
						} else {
							payout += 2;
						}
					} else {
						wonRow[i] = false;
					}
					break;
			}
		}
		@sprites["payout"].score = payout;
		if (payout > 0 || @replay) {
			if (bonus > 0) {
				MEPlay("Slots big win");
			} else {
				MEPlay("Slots win");
			}
			// Show winning animation
			timer_start = System.uptime;
			do { //loop; while (true);
				frame = ((System.uptime - timer_start) / 0.125).ToInt();
				@sprites["window2"].bitmap&.clear;
				@sprites["window1"].setBitmap(_INTL("Graphics/UI/Slot Machine/win"));
				@sprites["window1"].src_rect.set(152 * (frame % 4), 0, 152, 208);
				if (bonus > 0) {
					@sprites["window2"].setBitmap(_INTL("Graphics/UI/Slot Machine/bonus"));
					@sprites["window2"].src_rect.set(152 * (bonus - 1), 0, 152, 208);
				}
				@sprites["light1"].visible = true;
				@sprites["light1"].src_rect.set(0, 26 * (frame % 4), 96, 26);
				@sprites["light2"].visible = true;
				@sprites["light2"].src_rect.set(0, 26 * (frame % 4), 96, 26);
				(1..5).each do |i|
					if (wonRow[i - 1]) {
						@sprites[$"row{i}"].visible = frame.even();
					} else {
						@sprites[$"row{i}"].visible = false;
					}
				}
				Graphics.update;
				Input.update;
				update;
				if (System.uptime - timer_start >= 3.0) break;
			}
			@sprites["light1"].visible = false;
			@sprites["light2"].visible = false;
			@sprites["window1"].src_rect.set(0, 0, 152, 208);
			// Pay out
			timer_start = System.uptime;
			last_paid_tick = -1;
			do { //loop; while (true);
				if (@sprites["payout"].score <= 0) break;
				Graphics.update;
				Input.update;
				update;
				this_tick = ((System.uptime - timer_start) * 20).ToInt();   // Pay out 1 coin every 1/20 seconds
				if (this_tick != last_paid_tick) {
					@sprites["payout"].score -= 1;
					@sprites["credit"].score += 1;
					this_tick = last_paid_tick;
				}
				if (Input.trigger(Input.USE) || @sprites["credit"].score == Settings.MAX_COINS) {
					@sprites["credit"].score += @sprites["payout"].score;
					@sprites["payout"].score = 0;
				}
			}
			// Wait
			timer_start = System.uptime;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				update;
				if (System.uptime - timer_start >= 0.5) break;
			}
		} else {
			// Show losing animation
			timer_start = System.uptime;
			do { //loop; while (true);
				frame = ((System.uptime - timer_start) / 0.25).ToInt();
				@sprites["window2"].bitmap&.clear;
				@sprites["window1"].setBitmap(_INTL("Graphics/UI/Slot Machine/lose"));
				@sprites["window1"].src_rect.set(152 * (frame % 2), 0, 152, 208);
				Graphics.update;
				Input.update;
				update;
				if (System.uptime - timer_start >= 2.0) break;
			}
		}
		@wager = 0;
	}

	public void StartScene(difficulty) {
		@sprites = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		addBackgroundPlane(@sprites, "bg", "Slot Machine/bg", @viewport);
		@sprites["reel1"] = new SlotMachineReel(64, 112, 1, difficulty);
		@sprites["reel2"] = new SlotMachineReel(144, 112, 2, difficulty);
		@sprites["reel3"] = new SlotMachineReel(224, 112, 3, difficulty);
		(1..3).each do |i|
			@sprites[$"button{i}"] = new IconSprite(68 + (80 * (i - 1)), 260, @viewport);
			@sprites[$"button{i}"].setBitmap("Graphics/UI/Slot Machine/button");
			@sprites[$"button{i}"].visible = false;
		}
		(1..5).each do |i|
			y = new {170, 122, 218, 82, 82}[i - 1];
			@sprites[$"row{i}"] = new IconSprite(2, y, @viewport);
			@sprites[$"row{i}"].setBitmap(string.Format("Graphics/UI/Slot Machine/line{0:1}{0}",
																						1 + (i / 2), (i >= 4) ? ((i == 4) ? "a" : "b") : ""));
			@sprites[$"row{i}"].visible = false;
		}
		@sprites["light1"] = new IconSprite(16, 32, @viewport);
		@sprites["light1"].setBitmap("Graphics/UI/Slot Machine/lights");
		@sprites["light1"].visible = false;
		@sprites["light2"] = new IconSprite(240, 32, @viewport);
		@sprites["light2"].setBitmap("Graphics/UI/Slot Machine/lights");
		@sprites["light2"].mirror = true;
		@sprites["light2"].visible = false;
		@sprites["window1"] = new IconSprite(358, 96, @viewport);
		@sprites["window1"].setBitmap(_INTL("Graphics/UI/Slot Machine/insert"));
		@sprites["window1"].src_rect.set(0, 0, 152, 208);
		@sprites["window2"] = new IconSprite(358, 96, @viewport);
		@sprites["credit"] = new SlotMachineScore(360, 66, Game.GameData.player.coins);
		@sprites["payout"] = new SlotMachineScore(438, 66, 0);
		@wager = 0;
		update;
		FadeInAndShow(@sprites);
	}

	public void Main() {
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			update;
			@sprites["window1"].bitmap&.clear;
			@sprites["window2"].bitmap&.clear;
			if (@sprites["credit"].score == Settings.MAX_COINS) {
				Message(_INTL("You've got {1} Coins.", Settings.MAX_COINS.to_s_formatted));
				break;
			} else if (Game.GameData.player.coins == 0) {
				Message(_INTL("You've run out of Coins.\nGame over!"));
				break;
			} else if (@gameRunning) {   // Reels are spinning
				@sprites["window1"].setBitmap(_INTL("Graphics/UI/Slot Machine/stop"));
				timer_start = System.uptime;
				do { //loop; while (true);
					frame = ((System.uptime - timer_start) / 0.25).ToInt();
					@sprites["window1"].src_rect.set(152 * (frame % 4), 0, 152, 208);
					Graphics.update;
					Input.update;
					update;
					if (Input.trigger(Input.USE)) {
						SEPlay("Slots stop");
						if (@sprites["reel1"].spinning()) {
							@sprites["reel1"].stopSpinning(@replay);
							@sprites["button1"].visible = true;
						} else if (@sprites["reel2"].spinning()) {
							@sprites["reel2"].stopSpinning(@replay);
							@sprites["button2"].visible = true;
						} else if (@sprites["reel3"].spinning()) {
							@sprites["reel3"].stopSpinning(@replay);
							@sprites["button3"].visible = true;
						}
					}
					if (!@sprites["reel3"].spinning()) {
						@gameEnd = true;
						@gameRunning = false;
					}
					if (!@gameRunning) break;
				}
			} else if (@gameEnd) {   // Reels have been stopped
				Payout;
				// Reset graphics
				@sprites["button1"].visible = false;
				@sprites["button2"].visible = false;
				@sprites["button3"].visible = false;
				(1..5).each do |i|
					@sprites[$"row{i}"].visible = false;
				}
				@gameEnd = false;
			} else {   // Awaiting coins for the next spin
				@sprites["window1"].setBitmap(_INTL("Graphics/UI/Slot Machine/insert"));
				timer_start = System.uptime;
				do { //loop; while (true);
					frame = ((System.uptime - timer_start) / 0.4).ToInt();
					@sprites["window1"].src_rect.set(152 * (frame % 2), 0, 152, 208);
					if (@wager > 0) {
						@sprites["window2"].setBitmap(_INTL("Graphics/UI/Slot Machine/press"));
						@sprites["window2"].src_rect.set(152 * (frame % 2), 0, 152, 208);
					}
					Graphics.update;
					Input.update;
					update;
					if (Input.trigger(Input.DOWN) && @wager < 3 && @sprites["credit"].score > 0) {
						SEPlay("Slots coin");
						@wager += 1;
						@sprites["credit"].score -= 1;
						if (@wager >= 3) {
							@sprites["row5"].visible = true;
							@sprites["row4"].visible = true;
						} else if (@wager >= 2) {
							@sprites["row3"].visible = true;
							@sprites["row2"].visible = true;
						} else if (@wager >= 1) {
							@sprites["row1"].visible = true;
						}
					} else if (@wager >= 3 || (@wager > 0 && @sprites["credit"].score == 0) ||
								(Input.trigger(Input.USE) && @wager > 0) || @replay) {
						if (@replay) {
							@wager = 3;
							(1..5).each { |i| @sprites[$"row{i}"].visible = true }
						}
						@sprites["reel1"].startSpinning;
						@sprites["reel2"].startSpinning;
						@sprites["reel3"].startSpinning;
						@gameRunning = true;
					} else if (Input.trigger(Input.BACK) && @wager == 0) {
						break;
					}
					if (@gameRunning) break;
				}
				if (!@gameRunning) break;
			}
		}
		old_coins = Game.GameData.player.coins;
		Game.GameData.player.coins = @sprites["credit"].score;
		if (Game.GameData.player.coins > old_coins) {
			Game.GameData.stats.coins_won += Game.GameData.player.coins - old_coins;
		} else if (Game.GameData.player.coins < old_coins) {
			Game.GameData.stats.coins_lost += old_coins - Game.GameData.player.coins;
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
public partial class SlotMachine {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen(difficulty) {
		@scene.StartScene(difficulty);
		@scene.Main;
		@scene.EndScene;
	}
}

//===============================================================================
//
//===============================================================================
public void SlotMachine(difficulty = 1) {
	if (!Game.GameData.bag.has(:COINCASE)) {
		Message(_INTL("It's a Slot Machine."));
	} else if (Game.GameData.player.coins == 0) {
		Message(_INTL("You don't have any Coins to play!"));
	} else if (Game.GameData.player.coins == Settings.MAX_COINS) {
		Message(_INTL("Your Coin Case is full!"));
	} else {
		FadeOutIn do;
			scene = new SlotMachineScene();
			screen = new SlotMachine(scene);
			screen.StartScreen(difficulty);
		}
	}
}
