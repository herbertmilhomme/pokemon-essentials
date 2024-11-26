//===============================================================================
// Battle scene (the visuals of the battle)
//===============================================================================
public partial class Battle.Scene {
	/// <summary>For non-interactive battles, can quit immediately</summary>
	public int abortable		{ get { return _abortable; } set { _abortable = value; } }			protected int _abortable;
	public int viewport		{ get { return _viewport; } }			protected int _viewport;
	public int sprites		{ get { return _sprites; } }			protected int _sprites;

	USE_ABILITY_SPLASH   = (Settings.MECHANICS_GENERATION >= 5);
	public const int MESSAGE_PAUSE_TIME   = 1.0;   // In seconds
	// Text colors
	MESSAGE_BASE_COLOR   = new Color(80, 80, 88);
	MESSAGE_SHADOW_COLOR = new Color(160, 160, 168);
	// The number of party balls to show in each side's lineup.
	NUM_BALLS            = Settings.MAX_PARTY_SIZE;
	// Centre bottom of the player's side base graphic
	public const int PLAYER_BASE_X        = 128;
	PLAYER_BASE_Y        = Settings.SCREEN_HEIGHT - 80;
	// Centre middle of the foe's side base graphic
	FOE_BASE_X           = Settings.SCREEN_WIDTH - 128;
	FOE_BASE_Y           = (Settings.SCREEN_HEIGHT * 3 / 4) - 112;
	// Default focal points of user and target in animations - do not change!
	// Is the centre middle of each sprite
	public const int FOCUSUSER_X          = 128;
	public const int FOCUSUSER_Y          = 224;
	public const int FOCUSTARGET_X        = 384;
	public const int FOCUSTARGET_Y        = 96;
	// Menu types
	public const int BLANK                = 0;
	public const int MESSAGE_BOX          = 1;
	public const int COMMAND_BOX          = 2;
	public const int FIGHT_BOX            = 3;
	public const int TARGET_BOX           = 4;

	// Returns where the centre bottom of a battler's sprite should be, given its
	// index and the number of battlers on its side, assuming the battler has
	// metrics of 0 (those are added later).
	public static void BattlerPosition(index, sideSize = 1) {
		// Start at the centre of the base for the appropriate side
		if ((index & 1) == 0) {
			ret = new {PLAYER_BASE_X, PLAYER_BASE_Y};
		} else {
			ret = new {FOE_BASE_X, FOE_BASE_Y};
		}
		// Shift depending on index (no shifting needed for sideSize of 1)
		switch (sideSize) {
			case 2:
				ret[0] += new {-48, 48, 32, -32}[index];
				ret[1] += new {  0,  0, 16, -16}[index];
				break;
			case 3:
				ret[0] += new {-80, 80,  0,  0, 80, -80}[index];
				ret[1] += new {  0,  0,  8, -8, 16, -16}[index];
				break;
		}
		return ret;
	}

	// Returns where the centre bottom of a trainer's sprite should be, given its
	// side (0/1), index and the number of trainers on its side.
	public static void TrainerPosition(side, index = 0, sideSize = 1) {
		// Start at the centre of the base for the appropriate side
		if (side == 0) {
			ret = new {PLAYER_BASE_X, PLAYER_BASE_Y - 16};
		} else {
			ret = new {FOE_BASE_X, FOE_BASE_Y + 6};
		}
		// Shift depending on index (no shifting needed for sideSize of 1)
		switch (sideSize) {
			case 2:
				ret[0] += new {-48, 48, 32, -32}[(2 * index) + side];
				ret[1] += new {  0,  0,  0, -16}[(2 * index) + side];
				break;
			case 3:
				ret[0] += new {-80, 80,  0,  0, 80, -80}[(2 * index) + side];
				ret[1] += new {  0,  0,  0, -8,  0, -16}[(2 * index) + side];
				break;
		}
		return ret;
	}

	//-----------------------------------------------------------------------------
	// Updating and refreshing.
	//-----------------------------------------------------------------------------

	public void Update(cw = null) {
		GraphicsUpdate;
		InputUpdate;
		FrameUpdate(cw);
	}

	public void GraphicsUpdate() {
		// Update lineup animations
		if (@animations.length > 0) {
			shouldCompact = false;
			@animations.each_with_index do |a, i|
				a.update;
				if (!a.animDone()) continue;
				a.dispose;
				@animations[i] = null;
				shouldCompact = true;
			}
			if (shouldCompact) @animations.compact!;
		}
		// Update other graphics
		if (@sprites["battle_bg"].respond_to("update")) @sprites["battle_bg"].update;
		Graphics.update;
	}

	public void InputUpdate() {
		Input.update;
		if (Input.trigger(Input.BACK) && @abortable && !@aborted) {
			@aborted = true;
			@battle.Abort;
		}
	}

	public void FrameUpdate(cw = null) {
		cw&.update;
		@battle.battlers.each_with_index do |b, i|
			if (!b) continue;
			@sprites[$"dataBox_{i}"]&.update;
			@sprites[$"pokemon_{i}"]&.update;
			@sprites[$"shadow_{i}"]&.update;
		}
	}

	public void Refresh() {
		@battle.battlers.each_with_index do |b, i|
			if (!b) continue;
			@sprites[$"dataBox_{i}"]&.refresh;
		}
	}

	public void RefreshOne(idxBattler) {
		@sprites[$"dataBox_{idxBattler}"]&.refresh;
	}

	public void RefreshEverything() {
		CreateBackdropSprites;
		@battle.battlers.each_with_index do |battler, i|
			if (!battler) continue;
			ChangePokemon(i, @sprites[$"pokemon_{i}"].pkmn);
			@sprites[$"dataBox_{i}"].initializeDataBoxGraphic(@battle.SideSize(i));
			@sprites[$"dataBox_{i}"].refresh;
		}
	}

	//-----------------------------------------------------------------------------
	// Party lineup.
	//-----------------------------------------------------------------------------

	// Returns whether the party line-ups are currently coming on-screen
	public bool inPartyAnimation() {
		return @animations.length > 0;
	}

	//-----------------------------------------------------------------------------
	// Window displays.
	//-----------------------------------------------------------------------------

	public void ShowWindow(windowType) {
		// NOTE: If you are not using fancy graphics for the command/fight menus, you
		//       will need to make "messageBox" also visible if the windowtype if
		//       COMMAND_BOX/FIGHT_BOX respectively.
		@sprites["messageBox"].visible    = (windowType == MESSAGE_BOX);
		@sprites["messageWindow"].visible = (windowType == MESSAGE_BOX);
		@sprites["commandWindow"].visible = (windowType == COMMAND_BOX);
		@sprites["fightWindow"].visible   = (windowType == FIGHT_BOX);
		@sprites["targetWindow"].visible  = (windowType == TARGET_BOX);
	}

	// This is for the end of brief messages, which have been lingering on-screen
	// while other things happened. This is only called when another message wants
	// to be shown, and makes the brief message linger for one more second first.
	// Some animations skip this extra second by setting @briefMessage to false
	// despite not having any other messages to show.
	public void WaitMessage() {
		if (!@briefMessage) return;
		ShowWindow(MESSAGE_BOX);
		cw = @sprites["messageWindow"];
		timer_start = System.uptime;
		while (System.uptime - timer_start < MESSAGE_PAUSE_TIME) {
			Update(cw);
		}
		cw.text    = "";
		cw.visible = false;
		@briefMessage = false;
	}

	// NOTE: A regular message is displayed for 1 second after it fully appears (or
	//       less if Back/Use is pressed). Disappears automatically after that time.
	public void DisplayMessage(msg, brief = false) {
		WaitMessage;
		ShowWindow(MESSAGE_BOX);
		cw = @sprites["messageWindow"];
		cw.setText(msg);
		Debug.log_message(msg);
		yielded = false;
		timer_start = null;
		do { //loop; while (true);
			Update(cw);
			if (!cw.busy()) {
				if (!yielded) {
					if (block_given()) yield;   // For playing SE as soon as the message is all shown
					yielded = true;
				}
				if (brief) {
					// NOTE: A brief message lingers on-screen while other things happen. A
					//       regular message has to end before the game can continue.
					@briefMessage = true;
					break;
				}
				if (!timer_start) timer_start = System.uptime;
				if (System.uptime - timer_start >= MESSAGE_PAUSE_TIME) {   // Autoclose after 1 second
					cw.text = "";
					cw.visible = false;
					break;
				}
			}
			if (Input.trigger(Input.BACK) || Input.trigger(Input.USE) || @abortable) {
				if (cw.busy()) {
					if (cw.pausing() && !@abortable) PlayDecisionSE;
					cw.skipAhead;
				} else if (!@abortable) {
					cw.text = "";
					cw.visible = false;
					break;
				}
			}
		}
	}
	alias Display DisplayMessage;

	// NOTE: A paused message has the arrow in the bottom corner indicating there
	//       is another message immediately afterward. It is displayed for 3
	//       seconds after it fully appears (or less if B/C is pressed) and
	//       disappears automatically after that time, except at the end of battle.
	public void DisplayPausedMessage(msg) {
		WaitMessage;
		ShowWindow(MESSAGE_BOX);
		cw = @sprites["messageWindow"];
		cw.text = msg + "\1";
		Debug.log_message(msg);
		yielded = false;
		timer_start = null;
		do { //loop; while (true);
			Update(cw);
			if (!cw.busy()) {
				if (!yielded) {
					if (block_given()) yield;   // For playing SE as soon as the message is all shown
					yielded = true;
				}
				if (!@battleEnd) {
					if (!timer_start) timer_start = System.uptime;
					if (System.uptime - timer_start >= MESSAGE_PAUSE_TIME * 3) {   // Autoclose after 3 seconds
						cw.text = "";
						cw.visible = false;
						break;
					}
				}
			}
			if (Input.trigger(Input.BACK) || Input.trigger(Input.USE) || @abortable) {
				if (cw.busy()) {
					if (cw.pausing() && !@abortable) PlayDecisionSE;
					cw.skipAhead;
				} else if (!@abortable) {
					cw.text = "";
					PlayDecisionSE;
					break;
				}
			}
		}
	}

	public void DisplayConfirmMessage(msg) {
		return ShowCommands(msg, new {_INTL("Yes"), _INTL("No")}, 1) == 0;
	}

	public void ShowCommands(msg, commands, defaultValue) {
		WaitMessage;
		ShowWindow(MESSAGE_BOX);
		dw = @sprites["messageWindow"];
		dw.text = msg;
		cw = new Window_CommandPokemon(commands);
		if (cw.height > Graphics.height - dw.height) cw.height   = Graphics.height - dw.height;
		cw.x        = Graphics.width - cw.width;
		cw.y        = Graphics.height - cw.height - dw.height;
		cw.z        = dw.z + 1;
		cw.index    = 0;
		cw.viewport = @viewport;
		Debug.log_message(msg);
		do { //loop; while (true);
			cw.visible = (!dw.busy());
			Update(cw);
			dw.update;
			if (Input.trigger(Input.BACK) && defaultValue >= 0) {
				if (dw.busy()) {
					if (dw.pausing()) PlayDecisionSE;
					dw.resume;
				} else {
					cw.dispose;
					dw.text = "";
					return defaultValue;
				}
			} else if (Input.trigger(Input.USE)) {
				if (dw.busy()) {
					if (dw.pausing()) PlayDecisionSE;
					dw.resume;
				} else {
					cw.dispose;
					dw.text = "";
					return cw.index;
				}
			}
		}
	}

	//-----------------------------------------------------------------------------
	// Sprites.
	//-----------------------------------------------------------------------------

	public void AddSprite(id, x, y, filename, viewport) {
		sprite = @sprites[id] || new IconSprite(x, y, viewport);
		if (filename) {
			sprite.setBitmap(filename) rescue null;
		}
		@sprites[id] = sprite;
		return sprite;
	}

	public void AddPlane(id, filename, viewport) {
		sprite = new AnimatedPlane(viewport);
		if (filename) {
			sprite.setBitmap(filename);
		}
		@sprites[id] = sprite;
		return sprite;
	}

	public void DisposeSprites() {
		DisposeSpriteHash(@sprites);
	}

	// Used by Ally Switch.
	public void SwapBattlerSprites(idxA, idxB) {
		@sprites[$"pokemon_{idxA}"], @sprites[$"pokemon_{idxB}"] = @sprites[$"pokemon_{idxB}"], @sprites[$"pokemon_{idxA}"];
		@sprites[$"shadow_{idxA}"], @sprites[$"shadow_{idxB}"] = @sprites[$"shadow_{idxB}"], @sprites[$"shadow_{idxA}"];
		@lastCmd[idxA], @lastCmd[idxB] = @lastCmd[idxB], @lastCmd[idxA];
		@lastMove[idxA], @lastMove[idxB] = @lastMove[idxB], @lastMove[idxA];
		new {idxA, idxB}.each do |i|
			@sprites[$"pokemon_{i}"].index = i;
			@sprites[$"pokemon_{i}"].SetPosition;
			@sprites[$"shadow_{i}"].index = i;
			@sprites[$"shadow_{i}"].SetPosition;
			@sprites[$"dataBox_{i}"].battler = @battle.battlers[i];
		}
		Refresh;
	}

	//-----------------------------------------------------------------------------
	// Phases.
	//-----------------------------------------------------------------------------

	public void BeginCommandPhase() {
		@sprites["messageWindow"].text = "";
	}

	public void BeginAttackPhase() {
		SelectBattler(-1);
		ShowWindow(MESSAGE_BOX);
	}

	public void BeginEndOfRoundPhase() { }

	public void EndBattle(_result) {
		@abortable = false;
		ShowWindow(BLANK);
		// Fade out all sprites
		BGMFade(1.0);
		FadeOutAndHide(@sprites);
		DisposeSprites;
	}

	//-----------------------------------------------------------------------------
	//
	//-----------------------------------------------------------------------------

	public void SelectBattler(idxBattler, selectMode = 1) {
		numWindows = @battle.sideSizes.max * 2;
		for (int i = numWindows; i < numWindows; i++) { //for 'numWindows' times do => |i|
			sel = (idxBattler.Length > 0) ? !idxBattler[i].null() : i == idxBattler;
			selVal = (sel) ? selectMode : 0;
			if (@sprites[$"dataBox_{i}"]) @sprites[$"dataBox_{i}"].selected = selVal;
			if (@sprites[$"pokemon_{i}"]) @sprites[$"pokemon_{i}"].selected = selVal;
		}
	}

	public void ChangePokemon(idxBattler, pkmn) {
		if (idxBattler.respond_to("index")) idxBattler = idxBattler.index;
		pkmnSprite   = @sprites[$"pokemon_{idxBattler}"];
		shadowSprite = @sprites[$"shadow_{idxBattler}"];
		back = !@battle.opposes(idxBattler);
		pkmnSprite.setPokemonBitmap(pkmn, back);
		shadowSprite.setPokemonBitmap(pkmn);
		// Set visibility of battler's shadow
		if (shadowSprite && !back) shadowSprite.visible = pkmn.species_data.shows_shadow();
		@sprites[$"dataBox_{idxBattler}"].refresh;
	}

	public void ResetCommandsIndex(idxBattler) {
		@lastCmd[idxBattler] = 0;
		@lastMove[idxBattler] = 0;
	}

	//-----------------------------------------------------------------------------
	//
	//-----------------------------------------------------------------------------

	// This method is called when the player wins a wild Pokémon battle.
	// This method can change the battle's music for example.
	public void WildBattleSuccess() {
		@battleEnd = true;
		BGMPlay(GetWildVictoryBGM);
	}

	// This method is called when the player wins a trainer battle.
	// This method can change the battle's music for example.
	public void TrainerBattleSuccess() {
		@battleEnd = true;
		BGMPlay(GetTrainerVictoryBGM(@battle.opponent));
	}
}
