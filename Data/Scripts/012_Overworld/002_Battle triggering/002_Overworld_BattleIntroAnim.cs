//===============================================================================
// Registers special battle transition animations which may be used instead of
// the default ones. There are examples below of how to register them.
//
// The register call has 4 arguments:
//    1) The name of the animation. Typically unused, but helps to identify the
//       registration code for a particular animation if necessary.
//    2) The animation's priority. If multiple special animations could trigger
//       for the same battle, the one with the highest priority number is used.
//    3) A condition proc which decides whether the animation should trigger.
//    4) The animation itself. Could be a bunch of code, or a call to, say,
//       CommonEvent(20) or something else. By the end of the animation, the
//       screen should be black.
// Note that you can get an image of the current game screen with
// Graphics.snap_to_bitmap.
//===============================================================================
public static partial class SpecialBattleIntroAnimations {
	// [name, priority number, "trigger if" proc, animation proc]
	@@anims = new List<string>();

	public static void register(name, priority, condition, hash) {
		@@anims.Add(new {name, priority, condition, hash});
	}

	public static void remove(name) {
		@@anims.delete_if(anim => anim[0] == name);
	}

	public static void each() {
		ret = @@anims.sort((a, b) => b[1] <=> a[1]);
		ret.each(anim => yield anim[0], anim[1], anim[2], anim[3]);
	}

	public static bool has(name) {
		return @@anims.any(anim => anim[0] == name);
	}

	public static void get(name) {
		@@anims.each(anim => { if (anim[0] == name) return anim; });
		return null;
	}
}

//===============================================================================
// Battle intro animation.
//===============================================================================
public partial class Game_Temp {
	public int transition_animation_data		{ get { return _transition_animation_data; } set { _transition_animation_data = value; } }			protected int _transition_animation_data;
}

public void SceneStandby() {
	if (Game.GameData.scene.is_a(Scene_Map)) Game.GameData.scene.disposeSpritesets;
	RPG.Cache.clear;
	Graphics.frame_reset;
	yield;
	if (Game.GameData.scene.is_a(Scene_Map)) Game.GameData.scene.createSpritesets;
}

public void BattleAnimation(bgm = null, battletype = 0, foe = null) {
	Game.GameData.game_temp.in_battle = true;
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = 99999;
	// Set up audio
	playingBGS = null;
	playingBGM = null;
	if (Game.GameData.game_system.is_a(Game_System)) {
		playingBGS = Game.GameData.game_system.getPlayingBGS;
		playingBGM = Game.GameData.game_system.getPlayingBGM;
		Game.GameData.game_system.bgm_pause;
		Game.GameData.game_system.bgs_pause;
		if (Game.GameData.game_temp.memorized_bgm) {
			playingBGM = Game.GameData.game_temp.memorized_bgm;
			Game.GameData.game_system.bgm_position = Game.GameData.game_temp.memorized_bgm_position;
		}
	}
	// Play battle music
	if (!bgm) bgm = GetWildBattleBGM(new List<string>());
	BGMPlay(bgm);
	// Determine location of battle
	location = 0;   // 0=outside, 1=inside, 2=cave, 3=water
	if (Game.GameData.PokemonGlobal.surfing || Game.GameData.PokemonGlobal.diving) {
		location = 3;
	} else if (Game.GameData.game_temp.encounter_type &&
				GameData.EncounterType.get(Game.GameData.game_temp.encounter_type).type == types.fishing) {
		location = 3;
	} else if (Game.GameData.PokemonEncounters.has_cave_encounters()) {
		location = 2;
	} else if (!Game.GameData.game_map.metadata&.outdoor_map) {
		location = 1;
	}
	// Check for custom battle intro animations
	handled = false;
	SpecialBattleIntroAnimations.each do |name, priority, condition, animation|
		if (!condition.call(battletype, foe, location)) continue;
		animation.call(viewport, battletype, foe, location);
		handled = true;
		break;
	}
	// Default battle intro animation
	if (!handled) {
		// Determine which animation is played
		anim = "";
		if (DayNight.isDay()) {
			switch (battletype) {
				case 0: case 2:   // Wild, double wild
					anim = new {"SnakeSquares", "DiagonalBubbleTL", "DiagonalBubbleBR", "RisingSplash"}[location];
					break;
				case 1:      // Trainer
					anim = new {"TwoBallPass", "ThreeBallDown", "BallDown", "WavyThreeBallUp"}[location];
					break;
				case 3:      // Double trainer
					anim = "FourBallBurst";
					break;
			}
		} else {
			switch (battletype) {
				case 0: case 2:   // Wild, double wild
					anim = new {"SnakeSquares", "DiagonalBubbleBR", "DiagonalBubbleBR", "RisingSplash"}[location];
					break;
				case 1:      // Trainer
					anim = new {"SpinBallSplit", "BallDown", "BallDown", "WavySpinBall"}[location];
					break;
				case 3:      // Double trainer
					anim = "FourBallBurst";
					break;
			}
		}
		BattleAnimationCore(anim, viewport, location);
	}
	PushFade;
	// Yield to the battle scene
	if (block_given()) yield;
	// After the battle
	PopFade;
	if (Game.GameData.game_system.is_a(Game_System)) {
		Game.GameData.game_system.bgm_resume(playingBGM);
		Game.GameData.game_system.bgs_resume(playingBGS);
	}
	Game.GameData.game_temp.memorized_bgm            = null;
	Game.GameData.game_temp.memorized_bgm_position   = 0;
	Game.GameData.PokemonGlobal.nextBattleBGM        = null;
	Game.GameData.PokemonGlobal.nextBattleVictoryBGM = null;
	Game.GameData.PokemonGlobal.nextBattleCaptureME  = null;
	Game.GameData.PokemonGlobal.nextBattleBack       = null;
	Game.GameData.PokemonEncounters.reset_step_count;
	// Fade back to the overworld in 0.4 seconds
	viewport.color = Color.black;
	timer_start = System.uptime;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		UpdateSceneMap;
		viewport.color.alpha = 255 * (1 - ((System.uptime - timer_start) / 0.4));
		if (viewport.color.alpha <= 0) break;
	}
	viewport.dispose;
	Game.GameData.game_temp.in_battle = false;
}

public void BattleAnimationCore(anim, viewport, location, num_flashes = 2) {
	// Initial screen flashing
	if (num_flashes > 0) {
		c = (location == 2 || DayNight.isNight()) ? 0 : 255;   // Dark=black, light=white
		viewport.color = new Color(c, c, c);   // Fade to black/white a few times
		half_flash_time = 0.2;   // seconds
		num_flashes.times do;   // 2 flashes
			fade_out = false;
			timer_start = System.uptime;
			do { //loop; while (true);
				if (fade_out) {
					viewport.color.alpha = lerp(255, 0, half_flash_time, timer_start, System.uptime);
				} else {
					viewport.color.alpha = lerp(0, 255, half_flash_time, timer_start, System.uptime);
				}
				Graphics.update;
				UpdateSceneMap;
				if (fade_out && viewport.color.alpha <= 0) break;
				if (!fade_out && viewport.color.alpha >= 255) {
					fade_out = true;
					timer_start = System.uptime;
				}
			}
		}
	}
	// Take screenshot of game, for use in some animations
	Game.GameData.game_temp.background_bitmap&.dispose;
	Game.GameData.game_temp.background_bitmap = Graphics.snap_to_bitmap;
	// Play main animation
	Graphics.freeze;
	viewport.color = Color.black;   // Ensure screen is black
	Graphics.transition(25, "Graphics/Transitions/" + anim);
	// Slight pause after animation before starting up the battle scene
	Wait(0.1);
}

//===============================================================================
// Play the HGSS "VSTrainer" battle transition animation for any single trainer
// battle where the following graphics exist in the Graphics/Transitions/
// folder for the opponent:
//   * "hgss_vs_TRAINERTYPE.png" and "hgss_vsBar_TRAINERTYPE.png"
// This animation makes use of Game.GameData.game_temp.transition_animation_data, and expects
// it to be an array like so: [:TRAINERTYPE, "display name"]
//===============================================================================
SpecialBattleIntroAnimations.register("vs_trainer_animation", 60,   // Priority 60
	block: (battle_type, foe, location) => {   // Condition
		if (battle_type.even() || foe.length != 1) next false;   // Trainer battle against 1 trainer
		tr_type = foe[0].trainer_type;
		next ResolveBitmap($"Graphics/Transitions/hgss_vs_{tr_type}") &&
				ResolveBitmap($"Graphics/Transitions/hgss_vsBar_{tr_type}");
	},
	block: (viewport, battle_type, foe, location) => {   // Animation
		Game.GameData.game_temp.transition_animation_data = new {foe[0].trainer_type, foe[0].name};
		BattleAnimationCore("VSTrainer", viewport, location, 1);
		Game.GameData.game_temp.transition_animation_data = null;
	}
)

//===============================================================================
// Play the "VSEliteFour" battle transition animation for any single trainer
// battle where the following graphics exist in the Graphics/Transitions/
// folder for the opponent:
//   * "vsE4_TRAINERTYPE.png" and "vsE4Bar_TRAINERTYPE.png"
// This animation makes use of Game.GameData.game_temp.transition_animation_data, and expects
// it to be an array like so:
//   [:TRAINERTYPE, "display name", "player sprite name minus 'vsE4_'"]
//===============================================================================
SpecialBattleIntroAnimations.register("vs_elite_four_animation", 60,   // Priority 60
	block: (battle_type, foe, location) => {   // Condition
		if (battle_type.even() || foe.length != 1) next false;   // Trainer battle against 1 trainer
		tr_type = foe[0].trainer_type;
		next ResolveBitmap($"Graphics/Transitions/vsE4_{tr_type}") &&
				ResolveBitmap($"Graphics/Transitions/vsE4Bar_{tr_type}");
	},
	block: (viewport, battle_type, foe, location) => {   // Animation
		tr_sprite_name = Game.GameData.player.trainer_type.ToString();
		if (ResolveBitmap($"Graphics/Transitions/vsE4_{tr_sprite_name}_{Game.GameData.player.outfit}")) {
			tr_sprite_name += $"_{Game.GameData.player.outfit}";
		}
		Game.GameData.game_temp.transition_animation_data = new {foe[0].trainer_type, foe[0].name, tr_sprite_name};
		BattleAnimationCore("VSEliteFour", viewport, location, 0);
		Game.GameData.game_temp.transition_animation_data = null;
	}
)

//===============================================================================
// Play the "VSRocketAdmin" battle transition animation for any trainer battle
// where the following graphic exists in the Graphics/Transitions/ folder for any
// of the opponents:
//   * "rocket_TRAINERTYPE.png"
// This animation makes use of Game.GameData.game_temp.transition_animation_data, and expects
// it to be an array like so: [:TRAINERTYPE, "display name"]
//===============================================================================
SpecialBattleIntroAnimations.register("vs_admin_animation", 60,   // Priority 60
	block: (battle_type, foe, location) => {   // Condition
		if (!new []{1, 3}.Contains(battle_type)) next false;   // Trainer battles only
		found = false;
		foreach (var f in foe) { //'foe.each' do => |f|
			found = ResolveBitmap($"Graphics/Transitions/rocket_{f.trainer_type}");
			if (found) break;
		}
		next found;
	},
	block: (viewport, battle_type, foe, location) => {   // Animation
		foreach (var f in foe) { //'foe.each' do => |f|
			tr_type = f.trainer_type;
			if (!ResolveBitmap($"Graphics/Transitions/rocket_{tr_type}")) continue;
			Game.GameData.game_temp.transition_animation_data = new {tr_type, f.name};
			break;
		}
		BattleAnimationCore("VSRocketAdmin", viewport, location, 0);
		Game.GameData.game_temp.transition_animation_data = null;
	}
)

//===============================================================================
// Play the original Vs. Trainer battle transition animation for any single
// trainer battle where the following graphics exist in the Graphics/Transitions/
// folder for the opponent:
//   * "vsTrainer_TRAINERTYPE.png" and "vsBar_TRAINERTYPE.png"
//===============================================================================
//#### VS. animation, by Luka S.J. #####
//#### Tweaked by Maruno           #####
SpecialBattleIntroAnimations.register("alternate_vs_trainer_animation", 50,   // Priority 50
	block: (battle_type, foe, location) => {   // Condition
		if (battle_type.even() || foe.length != 1) next false;   // Trainer battle against 1 trainer
		tr_type = foe[0].trainer_type;
		next ResolveBitmap($"Graphics/Transitions/vsTrainer_{tr_type}") &&
				ResolveBitmap($"Graphics/Transitions/vsBar_{tr_type}");
	},
	block: (viewport, battle_type, foe, location) => {   // Animation
		// Determine filenames of graphics to be used
		tr_type = foe[0].trainer_type;
		trainer_bar_graphic = string.Format("vsBar_{0}", tr_type.ToString()) rescue null;
		trainer_graphic     = string.Format("vsTrainer_{0}", tr_type.ToString()) rescue null;
		player_tr_type = Game.GameData.player.trainer_type;
		outfit = Game.GameData.player.outfit;
		player_bar_graphic = string.Format("vsBar_{0}_{0}", player_tr_type.ToString(), outfit) rescue null;
		if (!ResolveBitmap("Graphics/Transitions/" + player_bar_graphic)) {
			player_bar_graphic = string.Format("vsBar_{0}", player_tr_type.ToString()) rescue null;
		}
		player_graphic = string.Format("vsTrainer_{0}_{0}", player_tr_type.ToString(), outfit) rescue null;
		if (!ResolveBitmap("Graphics/Transitions/" + player_graphic)) {
			player_graphic = string.Format("vsTrainer_{0}", player_tr_type.ToString()) rescue null;
		}
		// Set up viewports
		viewplayer = new Viewport(0, Graphics.height / 3, Graphics.width / 2, 128);
		viewplayer.z = viewport.z;
		viewopp = new Viewport(Graphics.width / 2, Graphics.height / 3, Graphics.width / 2, 128);
		viewopp.z = viewport.z;
		viewvs = new Viewport(0, 0, Graphics.width, Graphics.height);
		viewvs.z = viewport.z;
		// Set up sprites
		fade = new Sprite(viewport);
		fade.bitmap  = RPG.Cache.transition("vsFlash");
		fade.tone    = new Tone(-255, -255, -255);
		fade.opacity = 100;
		overlay = new Sprite(viewport);
		overlay.bitmap = new Bitmap(Graphics.width, Graphics.height);
		SetSystemFont(overlay.bitmap);
		xoffset = ((Graphics.width / 2) / 10) * 10;
		bar1 = new Sprite(viewplayer);
		bar1.bitmap = RPG.Cache.transition(player_bar_graphic);
		bar1.x      = -xoffset;
		bar2 = new Sprite(viewopp);
		bar2.bitmap = RPG.Cache.transition(trainer_bar_graphic);
		bar2.x      = xoffset;
		vs_x = Graphics.width / 2;
		vs_y = Graphics.height / 1.5;
		vs = new Sprite(viewvs);
		vs.bitmap  = RPG.Cache.transition("vs");
		vs.ox      = vs.bitmap.width / 2;
		vs.oy      = vs.bitmap.height / 2;
		vs.x       = vs_x;
		vs.y       = vs_y;
		vs.visible = false;
		flash = new Sprite(viewvs);
		flash.bitmap  = RPG.Cache.transition("vsFlash");
		flash.opacity = 0;
		// Animate bars sliding in from either side
		foreach (var delta_t in Wait(0.25)) { //Wait(0.25) do => |delta_t|
			bar1.x = lerp(-xoffset, 0, 0.25, delta_t);
			bar2.x = lerp(xoffset, 0, 0.25, delta_t);
		}
		bar1.dispose;
		bar2.dispose;
		// Make whole screen flash white
		SEPlay("Vs flash");
		SEPlay("Vs sword");
		flash.opacity = 255;
		// Replace bar sprites with AnimatedPlanes, set up trainer sprites
		bar1 = new AnimatedPlane(viewplayer);
		bar1.bitmap = RPG.Cache.transition(player_bar_graphic);
		bar2 = new AnimatedPlane(viewopp);
		bar2.bitmap = RPG.Cache.transition(trainer_bar_graphic);
		player = new Sprite(viewplayer);
		player.bitmap = RPG.Cache.transition(player_graphic);
		player.x      = -xoffset;
		trainer = new Sprite(viewopp);
		trainer.bitmap = RPG.Cache.transition(trainer_graphic);
		trainer.x      = xoffset;
		trainer.tone   = new Tone(-255, -255, -255);
		// Dim the flash and make the trainer sprites appear, while animating bars
		foreach (var delta_t in Wait(1.2)) { //Wait(1.2) do => |delta_t|
			flash.opacity = lerp(255, 0, 0.25, delta_t);
			bar1.ox = lerp(0, -bar1.bitmap.width * 3, 1.2, delta_t);
			bar2.ox = lerp(0, bar2.bitmap.width * 3, 1.2, delta_t);
			player.x = lerp(-xoffset, 0, 0.25, delta_t - 0.6);
			trainer.x = lerp(xoffset, 0, 0.25, delta_t - 0.6);
		}
		player.x = 0;
		trainer.x = 0;
		// Make whole screen flash white again
		flash.opacity = 255;
		SEPlay("Vs sword");
		// Make the Vs logo and trainer names appear, and reset trainer's tone
		vs.visible = true;
		trainer.tone = new Tone(0, 0, 0);
		trainername = foe[0].name;
		textpos = new {
			new {Game.GameData.player.name, Graphics.width / 4, (Graphics.height / 1.5) + 16, :center,
			new Color(248, 248, 248), new Color(72, 72, 72)},
			new {trainername, (Graphics.width / 4) + (Graphics.width / 2), (Graphics.height / 1.5) + 16, :center,
			new Color(248, 248, 248), new Color(72, 72, 72)};
		}
		DrawTextPositions(overlay.bitmap, textpos);
		// Fade out flash, shudder Vs logo and expand it, and then fade to black
		shudder_time = 1.75;
		zoom_time = 2.5;
		foreach (var delta_t in Wait(2.8)) { //Wait(2.8) do => |delta_t|
			if (delta_t <= shudder_time) {
				flash.opacity = lerp(255, 0, 0.25, delta_t);   // Fade out the white flash
			} else if (delta_t >= zoom_time) {
				flash.tone = new Tone(-255, -255, -255);   // Make the flash black
				flash.opacity = lerp(0, 255, 0.25, delta_t - zoom_time);   // Fade to black
			}
			bar1.ox = lerp(0, -bar1.bitmap.width * 7, 2.8, delta_t);
			bar2.ox = lerp(0, bar2.bitmap.width * 7, 2.8, delta_t);
			if (delta_t <= shudder_time) {
				// +2, -2, -2, +2, repeat
				period = (delta_t / 0.025).ToInt() % 4;
				shudder_delta = new {2, 0, -2, 0}[period];
				vs.x = vs_x + shudder_delta;
				vs.y = vs_y - shudder_delta;
			} else if (delta_t <= zoom_time) {
				vs.zoom_x = lerp(1.0, 7.0, zoom_time - shudder_time, delta_t - shudder_time);
				vs.zoom_y = vs.zoom_x;
			}
		}
		// End of animation
		player.dispose;
		trainer.dispose;
		flash.dispose;
		vs.dispose;
		bar1.dispose;
		bar2.dispose;
		overlay.dispose;
		fade.dispose;
		viewvs.dispose;
		viewopp.dispose;
		viewplayer.dispose;
		viewport.color = Color.black;   // Ensure screen is black
	}
)

//===============================================================================
// Play the "RocketGrunt" battle transition animation for any trainer battle
// involving a Team Rocket Grunt. Is lower priority than the Vs. animation above.
//===============================================================================
SpecialBattleIntroAnimations.register("rocket_grunt_animation", 40,   // Priority 40
	block: (battle_type, foe, location) => {   // Condition
		unless (new []{1, 3}.Contains(battle_type)) next false;   // Only if a trainer battle
		trainer_types = new {:TEAMROCKET_M, :TEAMROCKET_F};
		next foe.any(f => trainer_types.Contains(f.trainer_type));
	},
	block: (viewport, battle_type, foe, location) => {   // Animation
		BattleAnimationCore("RocketGrunt", viewport, location);
	}
)
