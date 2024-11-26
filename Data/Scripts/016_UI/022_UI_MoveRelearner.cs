//===============================================================================
// Scene class for handling appearance of the screen.
//===============================================================================
public partial class MoveRelearner_Scene {
	public const int VISIBLEMOVES = 4;

	public void Display(msg, brief = false) {
		UIHelper.Display(@sprites["msgwindow"], msg, brief) { Update };
	}

	public void Confirm(msg) {
		UIHelper.Confirm(@sprites["msgwindow"], msg) { Update };
	}

	public void Update() {
		UpdateSpriteHash(@sprites);
	}

	public void StartScene(pokemon, moves) {
		@pokemon = pokemon;
		@moves = moves;
		moveCommands = new List<string>();
		moves.each(m => moveCommands.Add(GameData.Move.get(m).name));
		// Create sprite hash
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		addBackgroundPlane(@sprites, "bg", "Move Reminder/bg", @viewport);
		@sprites["pokeicon"] = new PokemonIconSprite(@pokemon, @viewport);
		@sprites["pokeicon"].setOffset(PictureOrigin.CENTER);
		@sprites["pokeicon"].x = 320;
		@sprites["pokeicon"].y = 84;
		@sprites["background"] = new IconSprite(0, 0, @viewport);
		@sprites["background"].setBitmap("Graphics/UI/Move Reminder/cursor");
		@sprites["background"].y = 78;
		@sprites["background"].src_rect = new Rect(0, 72, 258, 72);
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		SetSystemFont(@sprites["overlay"].bitmap);
		@sprites["commands"] = new Window_CommandPokemon(moveCommands, 32);
		@sprites["commands"].height = 32 * (VISIBLEMOVES + 1);
		@sprites["commands"].visible = false;
		@sprites["msgwindow"] = new Window_AdvancedTextPokemon("");
		@sprites["msgwindow"].visible = false;
		@sprites["msgwindow"].viewport = @viewport;
		@typebitmap = new AnimatedBitmap(_INTL("Graphics/UI/types"));
		DrawMoveList;
		DeactivateWindows(@sprites);
		// Fade in all sprites
		FadeInAndShow(@sprites) { Update };
	}

	public void DrawMoveList() {
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		@pokemon.types.each_with_index do |type, i|
			type_number = GameData.Type.get(type).icon_position;
			type_rect = new Rect(0, type_number * GameData.Type.ICON_SIZE[1], *GameData.Type.ICON_SIZE);
			type_x = (@pokemon.types.length == 1) ? 400 : 366 + ((GameData.Type.ICON_SIZE[0] + 6) * i);
			overlay.blt(type_x, 70, @typebitmap.bitmap, type_rect);
		}
		textpos = new {
			new {_INTL("Teach which move?"), 16, 14, :left, new Color(88, 88, 80), new Color(168, 184, 184)};
		}
		imagepos = new List<string>();
		yPos = 88;
		for (int i = VISIBLEMOVES; i < VISIBLEMOVES; i++) { //for 'VISIBLEMOVES' times do => |i|
			moveobject = @moves[@sprites["commands"].top_item + i];
			if (moveobject) {
				moveData = GameData.Move.get(moveobject);
				type_number = GameData.Type.get(moveData.display_type(@pokemon)).icon_position;
				imagepos.Add(new {_INTL("Graphics/UI/types"), 12, yPos - 4, 0, type_number * GameData.Type.ICON_SIZE[1], *GameData.Type.ICON_SIZE});
				textpos.Add(new {moveData.name, 80, yPos, :left, new Color(248, 248, 248), Color.black});
				textpos.Add(new {_INTL("PP"), 112, yPos + 32, :left, new Color(64, 64, 64), new Color(176, 176, 176)});
				if (moveData.total_pp > 0) {
					textpos.Add(new {moveData.total_pp.ToString() + "/" + moveData.total_pp.ToString(), 230, yPos + 32, :right,
												new Color(64, 64, 64), new Color(176, 176, 176)});
				} else {
					textpos.Add(new {"--", 230, yPos + 32, :right, new Color(64, 64, 64), new Color(176, 176, 176)});
				}
			}
			yPos += 64;
		}
		imagepos.Add(new {"Graphics/UI/Move Reminder/cursor",
									0, 78 + ((@sprites["commands"].index - @sprites["commands"].top_item) * 64),
									0, 0, 258, 72});
		selMoveData = GameData.Move.get(@moves[@sprites["commands"].index]);
		power = selMoveData.display_damage(@pokemon);
		category = selMoveData.display_category(@pokemon);
		accuracy = selMoveData.display_accuracy(@pokemon);
		textpos.Add(new {_INTL("CATEGORY"), 272, 120, :left, new Color(248, 248, 248), Color.black});
		textpos.Add(new {_INTL("POWER"), 272, 152, :left, new Color(248, 248, 248), Color.black});
		textpos.Add(new {power <= 1 ? power == 1 ? "???" : "---" : power.ToString(), 468, 152, :center,
									new Color(64, 64, 64), new Color(176, 176, 176)});
		textpos.Add(new {_INTL("ACCURACY"), 272, 184, :left, new Color(248, 248, 248), Color.black});
		textpos.Add(new {accuracy == 0 ? "---" : $"{accuracy}%", 468, 184, :center,
									new Color(64, 64, 64), new Color(176, 176, 176)});
		DrawTextPositions(overlay, textpos);
		imagepos.Add(new {"Graphics/UI/category", 436, 116, 0, category * CATEGORY_ICON_SIZE[1], *CATEGORY_ICON_SIZE});
		if (@sprites["commands"].index < @moves.length - 1) {
			imagepos.Add(new {"Graphics/UI/Move Reminder/buttons", 48, 350, 0, 0, 76, 32});
		}
		if (@sprites["commands"].index > 0) {
			imagepos.Add(new {"Graphics/UI/Move Reminder/buttons", 134, 350, 76, 0, 76, 32});
		}
		DrawImagePositions(overlay, imagepos);
		drawTextEx(overlay, 272, 216, 230, 5, selMoveData.description,
							new Color(64, 64, 64), new Color(176, 176, 176));
	}

	// Processes the scene.
	public void ChooseMove() {
		oldcmd = -1;
		ActivateWindow(@sprites, "commands") do;
			do { //loop; while (true);
				oldcmd = @sprites["commands"].index;
				Graphics.update;
				Input.update;
				Update;
				if (@sprites["commands"].index != oldcmd) {
					@sprites["background"].x = 0;
					@sprites["background"].y = 78 + ((@sprites["commands"].index - @sprites["commands"].top_item) * 64);
					DrawMoveList;
				}
				if (Input.trigger(Input.BACK)) {
					return null;
				} else if (Input.trigger(Input.USE)) {
					return @moves[@sprites["commands"].index];
				}
			}
		}
	}

	// End the scene here.
	public void EndScene() {
		FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@typebitmap.dispose;
		@viewport.dispose;
	}
}

//===============================================================================
// Screen class for handling game logic.
//===============================================================================
public partial class MoveRelearnerScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void GetRelearnableMoves(pkmn) {
		if (!pkmn || pkmn.egg() || pkmn.shadowPokemon()) return [];
		moves = new List<string>();
		foreach (var m in pkmn.getMoveList) { //'pkmn.getMoveList.each' do => |m|
			if (m[0] > pkmn.level || pkmn.hasMove(m[1])) continue;
			if (!moves.Contains(m[1])) moves.Add(m[1]);
		}
		if (Settings.MOVE_RELEARNER_CAN_TEACH_MORE_MOVES && pkmn.first_moves) {
			tmoves = new List<string>();
			foreach (var i in pkmn.first_moves) { //'pkmn.first_moves.each' do => |i|
				if (!moves.Contains(i) && !pkmn.hasMove(i)) tmoves.Add(i);
			}
			moves = tmoves + moves;   // List first moves before level-up moves
		}
		return moves | [];   // remove duplicates
	}

	public void StartScreen(pkmn) {
		moves = GetRelearnableMoves(pkmn);
		@scene.StartScene(pkmn, moves);
		do { //loop; while (true);
			move = @scene.ChooseMove;
			if (move) {
				if (@scene.Confirm(_INTL("Teach {1}?", GameData.Move.get(move).name))) {
					if (LearnMove(pkmn, move)) {
						Game.GameData.stats.moves_taught_by_reminder += 1;
						@scene.EndScene;
						return true;
					}
				}
			} else if (@scene.Confirm(_INTL("Give up trying to teach a new move to {1}?", pkmn.name))) {
				@scene.EndScene;
				return false;
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public void RelearnMoveScreen(pkmn) {
	retval = true;
	FadeOutIn do;
		scene = new MoveRelearner_Scene();
		screen = new MoveRelearnerScreen(scene);
		retval = screen.StartScreen(pkmn);
	}
	return retval;
}
