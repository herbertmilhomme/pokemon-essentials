//===============================================================================
//
//===============================================================================
public partial class Battle.Scene {
	//-----------------------------------------------------------------------------
	// Create the battle scene and its elements.
	//-----------------------------------------------------------------------------

	public void initialize() {
		@battle     = null;
		@abortable  = false;
		@aborted    = false;
		@battleEnd  = false;
		@animations = new List<string>();
	}

	// Called whenever the battle begins.
	public void StartBattle(battle) {
		@battle   = battle;
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@lastCmd  = new Array(@battle.battlers.length, 0);
		@lastMove = new Array(@battle.battlers.length, 0);
		InitSprites;
		BattleIntroAnimation;
	}

	public void InitSprites() {
		@sprites = new List<string>();
		// The background image and each side's base graphic
		CreateBackdropSprites;
		// Create message box graphic
		messageBox = AddSprite("messageBox", 0, Graphics.height - 96,
														"Graphics/UI/Battle/overlay_message", @viewport);
		messageBox.z = 195;
		// Create message window (displays the message)
		msgWindow = Window_AdvancedTextPokemon.newWithSize(
			"", 16, Graphics.height - 96 + 2, Graphics.width - 32, 96, @viewport
		);
		msgWindow.z              = 200;
		msgWindow.opacity        = 0;
		msgWindow.baseColor      = MESSAGE_BASE_COLOR;
		msgWindow.shadowColor    = MESSAGE_SHADOW_COLOR;
		msgWindow.letterbyletter = true;
		@sprites["messageWindow"] = msgWindow;
		// Create command window
		@sprites["commandWindow"] = new CommandMenu(@viewport, 200);
		// Create fight window
		@sprites["fightWindow"] = new FightMenu(@viewport, 200);
		// Create targeting window
		@sprites["targetWindow"] = new TargetMenu(@viewport, 200, @battle.sideSizes);
		ShowWindow(MESSAGE_BOX);
		// The party lineup graphics (bar and balls) for both sides
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			partyBar = AddSprite($"partyBar_{side}", 0, 0,
														"Graphics/UI/Battle/overlay_lineup", @viewport);
			partyBar.z       = 120;
			if (side == 0) partyBar.mirror  = true;   // Player's lineup bar only
			partyBar.visible = false;
			for (int i = NUM_BALLS; i < NUM_BALLS; i++) { //for 'NUM_BALLS' times do => |i|
				ball = AddSprite($"partyBall_{side}_{i}", 0, 0, null, @viewport);
				ball.z       = 121;
				ball.visible = false;
			}
			// Ability splash bars
			if (USE_ABILITY_SPLASH) {
				@sprites[$"abilityBar_{side}"] = new AbilitySplashBar(side, @viewport);
			}
		}
		// Player's and partner trainer's back sprite
		@battle.player.each_with_index do |p, i|
			CreateTrainerBackSprite(i, p.trainer_type, @battle.player.length);
		}
		// Opposing trainer(s) sprites
		if (@battle.trainerBattle()) {
			@battle.opponent.each_with_index do |p, i|
				CreateTrainerFrontSprite(i, p.trainer_type, @battle.opponent.length);
			}
		}
		// Data boxes and Pokémon sprites
		@battle.battlers.each_with_index do |b, i|
			if (!b) continue;
			@sprites[$"dataBox_{i}"] = new PokemonDataBox(b, @battle.SideSize(i), @viewport);
			CreatePokemonSprite(i);
		}
		// Wild battle, so set up the Pokémon sprite(s) accordingly
		if (@battle.wildBattle()) {
			@battle.Party(1).each_with_index do |pkmn, i|
				index = (i * 2) + 1;
				ChangePokemon(index, pkmn);
				pkmnSprite = @sprites[$"pokemon_{index}"];
				pkmnSprite.tone    = new Tone(-80, -80, -80);
				pkmnSprite.visible = true;
			}
		}
	}

	public void CreateBackdropSprites() {
		switch (@battle.time) {
			case 1:  time = "eve"; break;
			case 2:  time = "night"; break;
		}
		// Put everything together into backdrop, bases and message bar filenames
		backdropFilename = @battle.backdrop;
		baseFilename = @battle.backdrop;
		if (@battle.backdropBase) baseFilename = string.Format("{0}_{0}", baseFilename, @battle.backdropBase);
		messageFilename = @battle.backdrop;
		if (time) {
			trialName = string.Format("{0}_{0}", backdropFilename, time);
			if (ResolveBitmap(string.Format("Graphics/Battlebacks/{0}_bg", trialName))) {
				backdropFilename = trialName;
			}
			trialName = string.Format("{0}_{0}", baseFilename, time);
			if (ResolveBitmap(string.Format("Graphics/Battlebacks/{0}_base0", trialName))) {
				baseFilename = trialName;
			}
			trialName = string.Format("{0}_{0}", messageFilename, time);
			if (ResolveBitmap(string.Format("Graphics/Battlebacks/{0}_message", trialName))) {
				messageFilename = trialName;
			}
		}
		if (!ResolveBitmap(string.Format("Graphics/Battlebacks/{0}_base0", baseFilename)) &&
			@battle.backdropBase) {
			baseFilename = @battle.backdropBase;
			if (time) {
				trialName = string.Format("{0}_{0}", baseFilename, time);
				if (ResolveBitmap(string.Format("Graphics/Battlebacks/{0}_base0", trialName))) {
					baseFilename = trialName;
				}
			}
		}
		// Finalise filenames
		battleBG   = "Graphics/Battlebacks/" + backdropFilename + "_bg";
		playerBase = "Graphics/Battlebacks/" + baseFilename + "_base0";
		enemyBase  = "Graphics/Battlebacks/" + baseFilename + "_base1";
		messageBG  = "Graphics/Battlebacks/" + messageFilename + "_message";
		// Apply graphics
		bg = AddSprite("battle_bg", 0, 0, battleBG, @viewport);
		bg.z = 0;
		bg = AddSprite("battle_bg2", -Graphics.width, 0, battleBG, @viewport);
		bg.z      = 0;
		bg.mirror = true;
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			baseX, baseY = Battle.Scene.BattlerPosition(side);
			base = AddSprite($"base_{side}", baseX, baseY,
												(side == 0) ? playerBase : enemyBase, @viewport)
			base.z = 1;
			if (base.bitmap) {
				base.ox = base.bitmap.width / 2;
				base.oy = (side == 0) ? base.bitmap.height : base.bitmap.height / 2;
			}
		}
		cmdBarBG = AddSprite("cmdBar_bg", 0, Graphics.height - 96, messageBG, @viewport);
		cmdBarBG.z = 180;
	}

	public void CreateTrainerBackSprite(idxTrainer, trainerType, numTrainers = 1) {
		if (idxTrainer == 0) {   // Player's sprite
			trainerFile = GameData.TrainerType.player_back_sprite_filename(trainerType);
		} else {   // Partner trainer's sprite
			trainerFile = GameData.TrainerType.back_sprite_filename(trainerType);
		}
		spriteX, spriteY = Battle.Scene.TrainerPosition(0, idxTrainer, numTrainers);
		trainer = AddSprite($"player_{idxTrainer + 1}", spriteX, spriteY, trainerFile, @viewport);
		if (!trainer.bitmap) return;
		// Alter position of sprite
		trainer.z = 80 + idxTrainer;
		if (trainer.bitmap.width > trainer.bitmap.height * 2) {
			trainer.src_rect.x     = 0;
			trainer.src_rect.width = trainer.bitmap.width / 5;
		}
		trainer.ox = trainer.src_rect.width / 2;
		trainer.oy = trainer.bitmap.height;
	}

	public void CreateTrainerFrontSprite(idxTrainer, trainerType, numTrainers = 1) {
		trainerFile = GameData.TrainerType.front_sprite_filename(trainerType);
		spriteX, spriteY = Battle.Scene.TrainerPosition(1, idxTrainer, numTrainers);
		trainer = AddSprite($"trainer_{idxTrainer + 1}", spriteX, spriteY, trainerFile, @viewport);
		if (!trainer.bitmap) return;
		// Alter position of sprite
		trainer.z  = 7 + idxTrainer;
		trainer.ox = trainer.src_rect.width / 2;
		trainer.oy = trainer.bitmap.height;
	}

	public void CreatePokemonSprite(idxBattler) {
		sideSize = @battle.SideSize(idxBattler);
		batSprite = new BattlerSprite(@viewport, sideSize, idxBattler, @animations);
		@sprites[$"pokemon_{idxBattler}"] = batSprite;
		shaSprite = new BattlerShadowSprite(@viewport, sideSize, idxBattler);
		shaSprite.visible = false;
		@sprites[$"shadow_{idxBattler}"] = shaSprite;
	}
}
