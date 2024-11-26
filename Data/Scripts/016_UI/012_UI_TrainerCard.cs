//===============================================================================
//
//===============================================================================
public partial class PokemonTrainerCard_Scene {
	public void Update() {
		UpdateSpriteHash(@sprites);
	}

	public void StartScene() {
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		background = ResolveBitmap("Graphics/UI/Trainer Card/bg_f");
		if (Game.GameData.player.female() && background) {
			addBackgroundPlane(@sprites, "bg", "Trainer Card/bg_f", @viewport);
		} else {
			addBackgroundPlane(@sprites, "bg", "Trainer Card/bg", @viewport);
		}
		cardexists = ResolveBitmap(_INTL("Graphics/UI/Trainer Card/card_f"));
		@sprites["card"] = new IconSprite(0, 0, @viewport);
		if (Game.GameData.player.female() && cardexists) {
			@sprites["card"].setBitmap(_INTL("Graphics/UI/Trainer Card/card_f"));
		} else {
			@sprites["card"].setBitmap(_INTL("Graphics/UI/Trainer Card/card"));
		}
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		SetSystemFont(@sprites["overlay"].bitmap);
		@sprites["trainer"] = new IconSprite(336, 112, @viewport);
		@sprites["trainer"].setBitmap(GameData.TrainerType.player_front_sprite_filename(Game.GameData.player.trainer_type));
		if (!@sprites["trainer"].bitmap) {
			Debug.LogError(_INTL("No trainer front sprite exists for the player character, expected a file at {1}.",);
			//throw new Exception(_INTL("No trainer front sprite exists for the player character, expected a file at {1}.",);
									"Graphics/Trainers/" + Game.GameData.player.trainer_type.ToString() + ".png");
		}
		@sprites["trainer"].x -= (@sprites["trainer"].bitmap.width - 128) / 2;
		@sprites["trainer"].y -= (@sprites["trainer"].bitmap.height - 128);
		@sprites["trainer"].z = 2;
		DrawTrainerCardFront;
		FadeInAndShow(@sprites) { Update };
	}

	public void DrawTrainerCardFront() {
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		baseColor   = new Color(72, 72, 72);
		shadowColor = new Color(160, 160, 160);
		totalsec = Game.GameData.stats.play_time.ToInt();
		hour = totalsec / 60 / 60;
		min = totalsec / 60 % 60;
		time = (hour > 0) ? _INTL("{1}h {2}m", hour, min) : _INTL("{1}m", min);
		if (!Game.GameData.PokemonGlobal.startTime) Game.GameData.PokemonGlobal.startTime = Time.now;
		starttime = _INTL("{1} {2}, {3}",
											GetAbbrevMonthName(Game.GameData.PokemonGlobal.startTime.mon),
											Game.GameData.PokemonGlobal.startTime.day,
											Game.GameData.PokemonGlobal.startTime.year);
		textPositions = new {
			new {_INTL("Name"), 34, 70, :left, baseColor, shadowColor},
			new {Game.GameData.player.name, 302, 70, :right, baseColor, shadowColor},
			new {_INTL("ID No."), 332, 70, :left, baseColor, shadowColor},
			new {string.Format("{0:5}", Game.GameData.player.public_ID), 468, 70, :right, baseColor, shadowColor},
			new {_INTL("Money"), 34, 118, :left, baseColor, shadowColor},
			new {_INTL("${1}", Game.GameData.player.money.to_s_formatted), 302, 118, :right, baseColor, shadowColor},
			new {_INTL("Pokédex"), 34, 166, :left, baseColor, shadowColor},
			new {string.Format("{0}/{0}", Game.GameData.player.pokedex.owned_count, Game.GameData.player.pokedex.seen_count), 302, 166, :right, baseColor, shadowColor},
			new {_INTL("Time"), 34, 214, :left, baseColor, shadowColor},
			new {time, 302, 214, :right, baseColor, shadowColor},
			new {_INTL("Started"), 34, 262, :left, baseColor, shadowColor},
			new {starttime, 302, 262, :right, baseColor, shadowColor}
		}
		DrawTextPositions(overlay, textPositions);
		x = 72;
		region = GetCurrentRegion(0); // Get the current region
		imagePositions = new List<string>();
		for (int i = 8; i < 8; i++) { //for '8' times do => |i|
			if (Game.GameData.player.badges[i + (region * 8)]) {
				imagePositions.Add(new {"Graphics/UI/Trainer Card/icon_badges", x, 310, i * 32, region * 32, 32, 32});
			}
			x += 48;
		}
		DrawImagePositions(overlay, imagePositions);
	}

	public void TrainerCard() {
		SEPlay("GUI trainer card open");
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (Input.trigger(Input.BACK)) {
				PlayCloseMenuSE;
				break;
			}
		}
	}

	public void EndScene() {
		FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonTrainerCardScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen() {
		@scene.StartScene;
		@scene.TrainerCard;
		@scene.EndScene;
	}
}
