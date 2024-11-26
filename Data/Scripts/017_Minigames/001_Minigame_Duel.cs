//===============================================================================
// "Duel" mini-game.
// Based on the Duel minigame by Alael.
//===============================================================================
public partial class DuelWindow : Window_AdvancedTextPokemon {
	public int hp		{ get { return _hp; } }			protected int _hp;
	public int name		{ get { return _name; } }			protected int _name;
	public int is_enemy		{ get { return _is_enemy; } }			protected int _is_enemy;

	PLAYER_TEXT_BASE   = new Color(48, 80, 200);   // Blue
	PLAYER_TEXT_SHADOW = new Color(160, 192, 240);
	ENEMY_TEXT_BASE    = new Color(224, 8, 8);   // Red
	ENEMY_TEXT_SHADOW  = new Color(248, 184, 112);
	HP_TEXT_BASE       = new Color(32, 152, 8);   // Green
	HP_TEXT_SHADOW     = new Color(144, 240, 144);

	public override void initialize(name, is_enemy) {
		@hp       = 10;
		@name     = name;
		@is_enemy = is_enemy;
		base.initialize();
		self.width  = 160;
		self.height = 96;
		duel_refresh;
	}

	public int hp { set {
		@hp = value;
		duel_refresh;
		}
	}

	public int name { set {
		@name = value;
		duel_refresh;
		}
	}

	public int is_enemy { set {
		@is_enemy = value;
		duel_refresh;
		}
	}

	public void duel_refresh() {
		if (@is_enemy) {
			name_tag = shadowc3tag(ENEMY_TEXT_BASE, ENEMY_TEXT_SHADOW);
		} else {
			name_tag = shadowc3tag(PLAYER_TEXT_BASE, PLAYER_TEXT_SHADOW);
		}
		hp_tag = shadowc3tag(HP_TEXT_BASE, HP_TEXT_SHADOW);
		self.text = name_tag + fmtEscape(@name) + "\n" + hp_tag + _INTL("HP: {1}", @hp);
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonDuel {
	public void StartDuel(opponent, event) {
		@event = event;
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		@sprites["player"] = new IconSprite(-160, 96, @viewport);
		@sprites["player"].setBitmap(GameData.TrainerType.player_front_sprite_filename(Game.GameData.player.trainer_type));
		@sprites["opponent"] = new IconSprite(Graphics.width + 32, 96, @viewport);
		@sprites["opponent"].setBitmap(GameData.TrainerType.front_sprite_filename(opponent.trainer_type));
		@sprites["playerwindow"] = new DuelWindow(Game.GameData.player.name, false);
		@sprites["playerwindow"].x        = -@sprites["playerwindow"].width;
		@sprites["playerwindow"].viewport = @viewport;
		@sprites["opponentwindow"] = new DuelWindow(opponent.name, true);
		@sprites["opponentwindow"].x        = Graphics.width;
		@sprites["opponentwindow"].viewport = @viewport;
		Wait(0.5);
		foreach (var delta_t in Wait(0.5)) { //Wait(0.5) do => |delta_t|
			@sprites["player"].x = lerp(-160, 0, 0.4, delta_t);
			@sprites["playerwindow"].x = lerp(-@sprites["playerwindow"].width, 160 - @sprites["playerwindow"].width, 0.4, delta_t);
			@sprites["opponent"].x = lerp(Graphics.width + 32, Graphics.width - 128, 0.4, delta_t);
			@sprites["opponentwindow"].x = lerp(Graphics.width, Graphics.width - 160, 0.4, delta_t);
		}
		@sprites["player"].x = 0;
		@sprites["playerwindow"].x = 160 - @sprites["playerwindow"].width;
		@sprites["opponent"].x = Graphics.width - 128;
		@sprites["opponentwindow"].x = Graphics.width - 160;
		@oldmovespeed = Game.GameData.game_player.move_speed;
		@oldeventspeed = event.move_speed;
		MoveRoute(Game.GameData.game_player,
								new {MoveRoute.CHANGE_SPEED, 2,
								MoveRoute.DIRECTION_FIX_ON});
		MoveRoute(event,
								new {MoveRoute.CHANGE_SPEED, 2,
								MoveRoute.DIRECTION_FIX_ON});
		Wait(0.75);
	}

	public void Duel(opponent, event, speeches) {
		StartDuel(opponent, event);
		@hp = new {10, 10};
		@special = new {false, false};
		decision = null;
		do { //loop; while (true);
			if (@hp[0] < 0) @hp[0] = 0;
			if (@hp[1] < 0) @hp[1] = 0;
			Refresh;
			if (@hp[0] <= 0) {
				decision = false;
				break;
			} else if (@hp[1] <= 0) {
				decision = true;
				break;
			}
			action = 0;
			scores = new {3, 4, 4, 2};
			choices = (@special[1]) ? 3 : 4;
			if (@special[1]) scores[3] = 0;
			total = scores[0] + scores[1] + scores[2] + scores[3];
			if (total <= 0) {
				action = rand(choices);
			} else {
				num = rand(total);
				cumtotal = 0;
				for (int i = 4; i < 4; i++) { //for '4' times do => |i|
					cumtotal += scores[i];
					if (num < cumtotal) {
						action = i;
						break;
					}
				}
			}
			if (action == 3) @special[1] = true;
			Message(_INTL("{1}: {2}", opponent.name, speeches[(action * 3) + rand(3)]));
			list = new {
				_INTL("DEFEND"),
				_INTL("PRECISE ATTACK"),
				_INTL("FIERCE ATTACK");
			}
			if (!@special[0]) list.Add(_INTL("SPECIAL ATTACK"));
			command = Message(_INTL("Choose a command."), list, 0);
			if (command == 3) @special[0] = true;
			if (action == 0 && command == 0) {
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.SCRIPT_ASYNC, "moveRight90",
										MoveRoute.SCRIPT_ASYNC, "moveLeft90",
										MoveRoute.SCRIPT_ASYNC, "moveLeft90",
										MoveRoute.SCRIPT_ASYNC, "moveRight90"});
				MoveRoute(event,
										new {MoveRoute.SCRIPT_ASYNC, "moveLeft90",
										MoveRoute.SCRIPT_ASYNC, "moveRight90",
										MoveRoute.SCRIPT_ASYNC, "moveRight90",
										MoveRoute.SCRIPT_ASYNC, "moveLeft90"});
				Wait(0.5);
				Message(_INTL("You study each other's movements..."));
			} else if (action == 0 && command == 1) {
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.FORWARD});
				Wait(0.4);
				Shake(9, 9, 8);
				FlashScreens(false, true);
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 2,
										MoveRoute.BACKWARD});
				@hp[1] -= 1;
				Message(_INTL("Your attack was not blocked!"));
			} else if (action == 0 && command == 2) {
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.SCRIPT_ASYNC, "jumpForward"});
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.BACKWARD});
				Wait(1.0);
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 2,
										MoveRoute.BACKWARD});
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 2,
										MoveRoute.FORWARD});
				Message(_INTL("Your attack was evaded!"));
			} else if (new []{0, 1, 2}.Contains(action) && command == 3) {
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.SCRIPT_ASYNC, "jumpForward"});
				Wait(0.4);
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 5,
										MoveRoute.BACKWARD,
										MoveRoute.CHANGE_SPEED, 2});
				Wait(0.5);
				Shake(9, 9, 8);
				FlashScreens(false, true);
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 2,
										MoveRoute.BACKWARD});
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 2,
										MoveRoute.FORWARD});
				@hp[1] -= 3;
				Message(_INTL("You pierce through the opponent's defenses!"));
			} else if (action == 1 && command == 0) {
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.FORWARD});
				Wait(0.4);
				Shake(9, 9, 8);
				FlashScreens(true, false);
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 2,
										MoveRoute.BACKWARD});
				@hp[0] -= 1;
				Message(_INTL("You fail to block the opponent's attack!"));
			} else if (action == 1 && command == 1) {
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.FORWARD});
				Wait(0.6);
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 2,
										MoveRoute.BACKWARD});
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 2,
										MoveRoute.FORWARD});
				Wait(0.6);
				MoveRoute(event, [MoveRoute.BACKWARD]);
				MoveRoute(Game.GameData.game_player, [MoveRoute.FORWARD]);
				Wait(0.6);
				MoveRoute(Game.GameData.game_player, [MoveRoute.BACKWARD]);
				Message(_INTL("You cross blades with the opponent!"));
			} else if ((action == 1 && command == 2) ||
						(action == 2 && command == 1) ||
						(action == 2 && command == 2)) {
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.BACKWARD,
										MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.SCRIPT_ASYNC, "jumpForward"});
				Wait(0.8);
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.FORWARD});
				Wait(0.9);
				Shake(9, 9, 8);
				FlashScreens(true, true);
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.BACKWARD,
										MoveRoute.CHANGE_SPEED, 2});
				MoveRoute(event,
										new {MoveRoute.BACKWARD,
										MoveRoute.BACKWARD,
										MoveRoute.CHANGE_SPEED, 2});
				Wait(1.0);
				MoveRoute(event, [MoveRoute.FORWARD]);
				MoveRoute(Game.GameData.game_player, [MoveRoute.FORWARD]);
				@hp[0] -= action;    // Enemy action
				@hp[1] -= command;   // Player command
				Message(_INTL("You hit each other!"));
			} else if (action == 2 && command == 0) {
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.FORWARD});
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.SCRIPT_ASYNC, "jumpBackward"});
				Wait(1.0);
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 2,
										MoveRoute.FORWARD});
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 2,
										MoveRoute.BACKWARD});
				Message(_INTL("You evade the opponent's attack!"));
			} else if (action == 3 && new []{0, 1, 2}.Contains(command)) {
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.SCRIPT_ASYNC, "jumpForward"});
				Wait(0.4);
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 5,
										MoveRoute.BACKWARD,
										MoveRoute.CHANGE_SPEED, 2});
				Wait(0.5);
				Shake(9, 9, 8);
				FlashScreens(true, false);
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 2,
										MoveRoute.FORWARD});
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 2,
										MoveRoute.BACKWARD});
				@hp[0] -= 3;
				Message(_INTL("The opponent pierces through your defenses!"));
			} else if (action == 3 && command == 3) {
				MoveRoute(Game.GameData.game_player, [MoveRoute.BACKWARD]);
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.SCRIPT_ASYNC, "jumpForward"});
				MoveRoute(event,
										new {MoveRoute.WAIT, 15,
										MoveRoute.CHANGE_SPEED, 4,
										MoveRoute.SCRIPT_ASYNC, "jumpForward"});
				Wait(1.0);
				MoveRoute(event,
										new {MoveRoute.CHANGE_SPEED, 5,
										MoveRoute.BACKWARD,
										MoveRoute.CHANGE_SPEED, 2});
				MoveRoute(Game.GameData.game_player,
										new {MoveRoute.CHANGE_SPEED, 5,
										MoveRoute.BACKWARD,
										MoveRoute.CHANGE_SPEED, 2});
				Shake(9, 9, 8);
				Flash(new Color(255, 255, 255, 255), 20);
				FlashScreens(true, true);
				MoveRoute(Game.GameData.game_player, [MoveRoute.FORWARD]);
				@hp[0] -= 4;
				@hp[1] -= 4;
				Message(_INTL("Your special attacks collide!"));
			}
		}
		EndDuel;
		return decision;
	}

	public void EndDuel() {
		Wait(0.75);
		MoveRoute(Game.GameData.game_player,
								new {MoveRoute.DIRECTION_FIX_OFF,
								MoveRoute.CHANGE_SPEED, @oldmovespeed});
		MoveRoute(@event,
								new {MoveRoute.DIRECTION_FIX_OFF,
								MoveRoute.CHANGE_SPEED, @oldeventspeed});
		foreach (var delta_t in Wait(0.4)) { //Wait(0.4) do => |delta_t|
			new_opacity = lerp(255, 0, 0.4, delta_t);
			@sprites["player"].opacity = new_opacity;
			@sprites["opponent"].opacity = new_opacity;
			@sprites["playerwindow"].contents_opacity = new_opacity;
			@sprites["opponentwindow"].contents_opacity = new_opacity;
			@sprites["playerwindow"].opacity = new_opacity;
			@sprites["opponentwindow"].opacity = new_opacity;
		}
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void FlashScreens(player, opponent) {
		foreach (var delta_t in Wait(0.2)) { //Wait(0.2) do => |delta_t|
			new_alpha = lerp(0, 255, 0.2, delta_t);
			if (player) {
				@sprites["player"].color = new Color(255, 255, 255, new_alpha);
				@sprites["playerwindow"].color = new Color(255, 255, 255, new_alpha);
			}
			if (opponent) {
				@sprites["opponent"].color = new Color(255, 255, 255, new_alpha);
				@sprites["opponentwindow"].color = new Color(255, 255, 255, new_alpha);
			}
		}
		foreach (var delta_t in Wait(0.2)) { //Wait(0.2) do => |delta_t|
			new_alpha = lerp(255, 0, 0.2, delta_t);
			if (player) {
				@sprites["player"].color = new Color(255, 255, 255, new_alpha);
				@sprites["playerwindow"].color = new Color(255, 255, 255, new_alpha);
			}
			if (opponent) {
				@sprites["opponent"].color = new Color(255, 255, 255, new_alpha);
				@sprites["opponentwindow"].color = new Color(255, 255, 255, new_alpha);
			}
		}
		@sprites["player"].color.alpha = 0;
		@sprites["playerwindow"].color.alpha = 0;
		@sprites["opponent"].color.alpha = 0;
		@sprites["opponentwindow"].color.alpha = 0;
		if (!player || !opponent) Wait(0.4);
	}

	public void Refresh() {
		@sprites["playerwindow"].hp   = @hp[0];
		@sprites["opponentwindow"].hp = @hp[1];
		Wait(0.25);
	}
}

//===============================================================================
// Starts a duel.
// trainer_id - ID or symbol of the opponent's trainer type.
// trainer_name - Name of the opponent
// event - Game_Event object for the character's event
// speeches - Array of 12 speeches
//===============================================================================
public void Duel(trainer_id, trainer_name, event, speeches) {
	trainer_id = GameData.TrainerType.get(trainer_id).id;
	duel = new PokemonDuel();
	opponent = new NPCTrainer(
		GetMessageFromHash(MessageTypes.TRAINER_NAMES, trainer_name), trainer_id
	);
	speech_texts = new List<string>();
	for (int i = 12; i < 12; i++) { //for '12' times do => |i|
		speech_texts.Add(_I(speeches[i]));
	}
	duel.Duel(opponent, event, speech_texts);
}
