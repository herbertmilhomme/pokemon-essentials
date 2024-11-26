//===============================================================================
//
//===============================================================================
public partial class Battle.Scene {
	// Animates the battle intro.
	public void BattleIntroAnimation() {
		// Make everything appear
		introAnim = new Animation.Intro(@sprites, @viewport, @battle);
		do { //loop; while (true);
			introAnim.update;
			Update;
			if (introAnim.animDone()) break;
		}
		introAnim.dispose;
		// Post-appearance activities
		// Trainer battle: get ready to show the party lineups (they are brought
		// on-screen by a separate animation)
		if (@battle.trainerBattle()) {
			// NOTE: Here is where you'd make trainer sprites animate if they had an
			//       entrance animation. Be sure to set it up like a Pokémon entrance
			//       animation, i.e. add them to @animations so that they can play out
			//       while party lineups appear and messages show.
			ShowPartyLineup(0, true);
			ShowPartyLineup(1, true);
			return;
		}
		// Wild battle: play wild Pokémon's intro animations (including cry), show
		// data box(es), return the wild Pokémon's sprite(s) to normal colour, show
		// shiny animation(s)
		// Set up data box animation
		for (int i = @battle.sideSizes[1]; i < @battle.sideSizes[1]; i++) { //for '@battle.sideSizes[1]' times do => |i|
			idxBattler = (2 * i) + 1;
			if (!@battle.battlers[idxBattler]) continue;
			dataBoxAnim = new Animation.DataBoxAppear(@sprites, @viewport, idxBattler);
			@animations.Add(dataBoxAnim);
		}
		// Set up wild Pokémon returning to normal colour and playing intro
		// animations (including cry)
		@animations.Add(new Animation.Intro2(@sprites, @viewport, @battle.sideSizes[1]));
		// Play all the animations
		while (inPartyAnimation()) {
			Update;
		}
		// Show shiny animation for wild Pokémon
		if (@battle.showAnims) {
			for (int i = @battle.sideSizes[1]; i < @battle.sideSizes[1]; i++) { //for '@battle.sideSizes[1]' times do => |i|
				idxBattler = (2 * i) + 1;
				if (!@battle.battlers[idxBattler] || !@battle.battlers[idxBattler].shiny()) continue;
				if (Settings.SUPER_SHINY && @battle.battlers[idxBattler].super_shiny()) {
					CommonAnimation("SuperShiny", @battle.battlers[idxBattler]);
				} else {
					CommonAnimation("Shiny", @battle.battlers[idxBattler]);
				}
			}
		}
	}

	// Animates a party lineup appearing for the given side.
	public void ShowPartyLineup(side, fullAnim = false) {
		@animations.Add(
			new Animation.LineupAppear(@sprites, @viewport, side,
																	@battle.Party(side), @battle.PartyStarts(side),
																	fullAnim)
		);
		if (fullAnim) return;
		while (inPartyAnimation()) {
			Update;
		}
	}

	// Animates an opposing trainer sliding in from off-screen. Will animate a
	// previous trainer that is already on-screen slide off first. Used at the end
	// of battle.
	public void ShowOpponent(idxTrainer) {
		// Set up trainer appearing animation
		appearAnim = new Animation.TrainerAppear(@sprites, @viewport, idxTrainer);
		@animations.Add(appearAnim);
		// Play the animation
		while (inPartyAnimation()) {
			Update;
		}
	}

	// Animates a trainer's sprite and party lineup hiding (if they are visible).
	// Animates a Pokémon being sent out into battle, then plays the shiny
	// animation for it if relevant.
	// sendOuts is an array; each element is itself an array: [idxBattler,pkmn]
	public void SendOutBattlers(sendOuts, startBattle = false) {
		if (sendOuts.length == 0) return;
		// If party balls are still appearing, wait for them to finish showing up, as
		// the FadeAnimation will make them disappear.
		while (inPartyAnimation()) {
			Update;
		}
		@briefMessage = false;
		// Make all trainers and party lineups disappear (player-side trainers may
		// animate throwing a Poké Ball)
		if (@battle.opposes(sendOuts[0][0])) {
			fadeAnim = new Animation.TrainerFade(@sprites, @viewport, startBattle);
		} else {
			fadeAnim = new Animation.PlayerFade(@sprites, @viewport, startBattle);
		}
		// For each battler being sent out, set the battler's sprite and create two
		// animations (the Poké Ball moving and battler appearing from it, and its
		// data box appearing)
		sendOutAnims = new List<string>();
		sendOuts.each_with_index do |b, i|
			pkmn = @battle.battlers[b[0]].effects.Illusion || b[1];
			ChangePokemon(b[0], pkmn);
			Refresh;
			if (@battle.opposes(b[0])) {
				sendOutAnim = new Animation.PokeballTrainerSendOut(
					@sprites, @viewport, @battle.GetOwnerIndexFromBattlerIndex(b[0]) + 1,
					@battle.battlers[b[0]], startBattle, i
				);
			} else {
				sendOutAnim = new Animation.PokeballPlayerSendOut(
					@sprites, @viewport, @battle.GetOwnerIndexFromBattlerIndex(b[0]) + 1,
					@battle.battlers[b[0]], startBattle, i
				);
			}
			dataBoxAnim = new Animation.DataBoxAppear(@sprites, @viewport, b[0]);
			sendOutAnims.Add(new {sendOutAnim, dataBoxAnim, false});
		}
		// Play all animations
		do { //loop; while (true);
			fadeAnim.update;
			foreach (var a in sendOutAnims) { //'sendOutAnims.each' do => |a|
				if (a[2]) continue;
				a[0].update;
				if (a[0].animDone()) a[1].update;
				if (a[1].animDone()) a[2] = true;
			}
			Update;
			if (!inPartyAnimation() && sendOutAnims.none(a => !a[2])) break;
		}
		fadeAnim.dispose;
		foreach (var a in sendOutAnims) { //'sendOutAnims.each' do => |a|
			a[0].dispose;
			a[1].dispose;
		}
		// Play shininess animations for shiny Pokémon
		foreach (var b in sendOuts) { //'sendOuts.each' do => |b|
			if (!@battle.showAnims || !@battle.battlers[b[0]].shiny()) continue;
			if (Settings.SUPER_SHINY && @battle.battlers[b[0]].super_shiny()) {
				CommonAnimation("SuperShiny", @battle.battlers[b[0]]);
			} else {
				CommonAnimation("Shiny", @battle.battlers[b[0]]);
			}
		}
	}

	// Animates a Pokémon being recalled into its Poké Ball and its data box hiding.
	public void Recall(idxBattler) {
		@briefMessage = false;
		// Recall animation
		recallAnim = new Animation.BattlerRecall(@sprites, @viewport, @battle.battlers[idxBattler]);
		do { //loop; while (true);
			recallAnim&.update;
			Update;
			if (recallAnim.animDone()) break;
		}
		recallAnim.dispose;
		// Data box disappear animation
		dataBoxAnim = new Animation.DataBoxDisappear(@sprites, @viewport, idxBattler);
		do { //loop; while (true);
			dataBoxAnim.update;
			Update;
			if (dataBoxAnim.animDone()) break;
		}
		dataBoxAnim.dispose;
	}

	//-----------------------------------------------------------------------------
	// Ability splash bar animations.
	//-----------------------------------------------------------------------------

	public void ShowAbilitySplash(battler) {
		if (!USE_ABILITY_SPLASH) return;
		side = battler.index % 2;
		if (@sprites[$"abilityBar_{side}"].visible) HideAbilitySplash(battler);
		@sprites[$"abilityBar_{side}"].battler = battler;
		abilitySplashAnim = new Animation.AbilitySplashAppear(@sprites, @viewport, side);
		do { //loop; while (true);
			abilitySplashAnim.update;
			Update;
			if (abilitySplashAnim.animDone()) break;
		}
		abilitySplashAnim.dispose;
	}

	public void HideAbilitySplash(battler) {
		if (!USE_ABILITY_SPLASH) return;
		side = battler.index % 2;
		if (!@sprites[$"abilityBar_{side}"].visible) return;
		abilitySplashAnim = new Animation.AbilitySplashDisappear(@sprites, @viewport, side);
		do { //loop; while (true);
			abilitySplashAnim.update;
			Update;
			if (abilitySplashAnim.animDone()) break;
		}
		abilitySplashAnim.dispose;
	}

	public void ReplaceAbilitySplash(battler) {
		if (!USE_ABILITY_SPLASH) return;
		ShowAbilitySplash(battler);
	}

	//-----------------------------------------------------------------------------
	// HP change animations.
	//-----------------------------------------------------------------------------

	// Shows a HP-changing common animation and animates a data box's HP bar.
	// Called by def ReduceHP, def RecoverHP.
	public void HPChanged(battler, oldHP, showAnim = false) {
		@briefMessage = false;
		if (battler.hp > oldHP) {
			if (showAnim && @battle.showAnims) CommonAnimation("HealthUp", battler);
		} else if (battler.hp < oldHP) {
			if (showAnim && @battle.showAnims) CommonAnimation("HealthDown", battler);
		}
		@sprites[$"dataBox_{battler.index}"].animate_hp(oldHP, battler.hp);
		while (@sprites[$"dataBox_{battler.index}"].animating_hp()) {
			Update;
		}
	}

	public void DamageAnimation(battler, effectiveness = 0) {
		@briefMessage = false;
		// Damage animation
		damageAnim = new Animation.BattlerDamage(@sprites, @viewport, battler.index, effectiveness);
		do { //loop; while (true);
			damageAnim.update;
			Update;
			if (damageAnim.animDone()) break;
		}
		damageAnim.dispose;
	}

	// Animates battlers flashing and data boxes' HP bars because of damage taken
	// by an attack. targets is an array, which are all animated simultaneously.
	// Each element in targets is also an array: new {battler, old HP, effectiveness}
	public void HitAndHPLossAnimation(targets) {
		@briefMessage = false;
		// Set up animations
		damageAnims = new List<string>();
		foreach (var t in targets) { //'targets.each' do => |t|
			anim = new Animation.BattlerDamage(@sprites, @viewport, t[0].index, t[2]);
			damageAnims.Add(anim);
			@sprites[$"dataBox_{t[0].index}"].animate_hp(t[1], t[0].hp);
		}
		// Update loop
		do { //loop; while (true);
			damageAnims.each(a => a.update);
			Update;
			allDone = true;
			foreach (var t in targets) { //'targets.each' do => |t|
				if (!@sprites[$"dataBox_{t[0].index}"].animating_hp()) continue;
				allDone = false;
				break;
			}
			if (!allDone) continue;
			foreach (var a in damageAnims) { //'damageAnims.each' do => |a|
				if (a.animDone()) continue;
				allDone = false;
				break;
			}
			if (!allDone) continue;
			break;
		}
		damageAnims.each(a => a.dispose);
	}

	//-----------------------------------------------------------------------------

	// Animates a data box's Exp bar.
	public void EXPBar(battler, startExp, endExp, tempExp1, tempExp2) {
		if (!battler || endExp == startExp) return;
		startExpLevel = tempExp1 - startExp;
		endExpLevel   = tempExp2 - startExp;
		expRange      = endExp - startExp;
		dataBox = @sprites[$"dataBox_{battler.index}"];
		dataBox.animate_exp(startExpLevel, endExpLevel, expRange);
		while (dataBox.animating_exp()) {
			Update;
		}
	}

	// Shows stats windows upon a Pokémon levelling up.
	public void LevelUp(pkmn, _battler, oldTotalHP, oldAttack, oldDefense, oldSpAtk, oldSpDef, oldSpeed) {
		TopRightWindow(
			_INTL("Max. HP<r>+{1}\nAttack<r>+{2}\nDefense<r>+{3}\nSp. Atk<r>+{4}\nSp. Def<r>+{5}\nSpeed<r>+{6}",
						pkmn.totalhp - oldTotalHP, pkmn.attack - oldAttack, pkmn.defense - oldDefense,
						pkmn.spatk - oldSpAtk, pkmn.spdef - oldSpDef, pkmn.speed - oldSpeed)
		);
		TopRightWindow(
			_INTL("Max. HP<r>{1}\nAttack<r>{2}\nDefense<r>{3}\nSp. Atk<r>{4}\nSp. Def<r>{5}\nSpeed<r>{6}",
						pkmn.totalhp, pkmn.attack, pkmn.defense, pkmn.spatk, pkmn.spdef, pkmn.speed)
		);
	}

	// Animates a Pokémon fainting.
	public void FaintBattler(battler) {
		@briefMessage = false;
		old_height = @sprites[$"pokemon_{battler.index}"].src_rect.height;
		// Pokémon plays cry and drops down, data box disappears
		faintAnim   = new Animation.BattlerFaint(@sprites, @viewport, battler.index, @battle);
		dataBoxAnim = new Animation.DataBoxDisappear(@sprites, @viewport, battler.index);
		do { //loop; while (true);
			faintAnim.update;
			dataBoxAnim.update;
			Update;
			if (faintAnim.animDone() && dataBoxAnim.animDone()) break;
		}
		faintAnim.dispose;
		dataBoxAnim.dispose;
		@sprites[$"pokemon_{battler.index}"].src_rect.height = old_height;
	}

	//-----------------------------------------------------------------------------
	// Animates throwing a Poké Ball at a Pokémon in an attempt to catch it.
	//-----------------------------------------------------------------------------

	public void Throw(ball, shakes, critical, targetBattler, showPlayer = false) {
		@briefMessage = false;
		captureAnim = new Animation.PokeballThrowCapture(
			@sprites, @viewport, ball, shakes, critical, @battle.battlers[targetBattler], showPlayer
		);
		do { //loop; while (true);
			captureAnim.update;
			Update;
			if (captureAnim.animDone() && !inPartyAnimation()) break;
		}
		captureAnim.dispose;
	}

	public void ThrowSuccess() {
		if (@battle.opponent) return;
		@briefMessage = false;
		MEPlay(GetWildCaptureME);
		timer_start = System.uptime;
		do { //loop; while (true);
			Update;
			if (System.uptime - timer_start >= 3.5) break;
		}
		MEStop;
	}

	public void HideCaptureBall(idxBattler) {
		// NOTE: It's not really worth writing a whole Battle.Scene.Animation class
		//       for making the capture ball fade out.
		ball = @sprites["captureBall"];
		if (!ball) return;
		// Data box disappear animation
		dataBoxAnim = new Animation.DataBoxDisappear(@sprites, @viewport, idxBattler);
		timer_start = System.uptime;
		do { //loop; while (true);
			dataBoxAnim.update;
			ball.opacity = lerp(255, 0, 1.0, timer_start, System.uptime);
			Update;
			if (dataBoxAnim.animDone() && ball.opacity <= 0) break;
		}
		dataBoxAnim.dispose;
	}

	public void ThrowAndDeflect(ball, idxBattler) {
		@briefMessage = false;
		throwAnim = new Animation.PokeballThrowDeflect(
			@sprites, @viewport, ball, @battle.battlers[idxBattler]
		);
		do { //loop; while (true);
			throwAnim.update;
			Update;
			if (throwAnim.animDone()) break;
		}
		throwAnim.dispose;
	}

	//=============================================================================

	// Hides all battler shadows before yielding to a move animation, and then
	// restores the shadows afterwards.
	public void SaveShadows() {
		// Remember which shadows were visible
		shadows = new Array(@battle.battlers.length) do |i|
			shadow = @sprites[$"shadow_{i}"];
			ret = (shadow) ? shadow.visible : false;
			if (shadow) shadow.visible = false;
			next ret;
		}
		// Yield to other code, i.e. playing an animation
		yield;
		// Restore shadow visibility
		for (int i = @battle.battlers.length; i < @battle.battlers.length; i++) { //for '@battle.battlers.length' times do => |i|
			shadow = @sprites[$"shadow_{i}"];
			if (shadow) shadow.visible = shadows[i];
		}
	}

	//-----------------------------------------------------------------------------
	// Loads a move/common animation.
	//-----------------------------------------------------------------------------

	// Returns the animation ID to use for a given move/user. Returns null if that
	// move has no animations defined for it.
	public void FindMoveAnimDetails(move2anim, moveID, idxUser, hitNum = 0) {
		real_move_id = GameData.Move.try_get(moveID)&.id || moveID;
		noFlip = false;
		if ((idxUser & 1) == 0) {   // On player's side
			anim = move2anim[0][real_move_id];
		} else {                // On opposing side
			anim = move2anim[1][real_move_id];
			if (anim) noFlip = true;
			if (!anim) anim = move2anim[0][real_move_id];
		}
		if (anim) return new {anim + hitNum, noFlip};
		return null;
	}

	// Returns the animation ID to use for a given move. If the move has no
	// animations, tries to use a default move animation depending on the move's
	// type. If that default move animation doesn't exist, trues to use Tackle's
	// move animation. Returns null if it can't find any of these animations to use.
	public void FindMoveAnimation(moveID, idxUser, hitNum) {
		begin;
			move2anim = LoadMoveToAnim;
			// Find actual animation requested (an opponent using the animation first
			// looks for an OppMove version then a Move version)
			anim = FindMoveAnimDetails(move2anim, moveID, idxUser, hitNum);
			if (anim) return anim;
			// Actual animation not found, get the default animation for the move's type
			moveData = GameData.Move.get(moveID);
			target_data = GameData.Target.get(moveData.target);
			moveType = moveData.type;
			moveKind = moveData.category;
			if (target_data.num_targets > 1 || target_data.affects_foe_side) moveKind += 3;
			if (moveData.status() && target_data.num_targets > 0) moveKind += 3;
			// [one target physical, one target special, user status,
			//  multiple targets physical, multiple targets special, non-user status]
			typeDefaultAnim = {
				NORMAL   = new {:TACKLE,       :SONICBOOM,    :DEFENSECURL, :EXPLOSION,  :SWIFT,        :TAILWHIP},
				FIGHTING = new {:MACHPUNCH,    :AURASPHERE,   :DETECT,      null,         null,           null},
				FLYING   = new {:WINGATTACK,   :GUST,         :ROOST,       null,         :AIRCUTTER,    :FEATHERDANCE},
				POISON   = new {:POISONSTING,  :SLUDGE,       :ACIDARMOR,   null,         :ACID,         :POISONPOWDER},
				GROUND   = new {:SANDTOMB,     :MUDSLAP,      null,          :EARTHQUAKE, :EARTHPOWER,   :MUDSPORT},
				ROCK     = new {:ROCKTHROW,    :POWERGEM,     :ROCKPOLISH,  :ROCKSLIDE,  null,           :SANDSTORM},
				BUG      = new {:TWINEEDLE,    :BUGBUZZ,      :QUIVERDANCE, null,         :STRUGGLEBUG,  :STRINGSHOT},
				GHOST    = new {:LICK,         :SHADOWBALL,   :GRUDGE,      null,         null,           :CONFUSERAY},
				STEEL    = new {:IRONHEAD,     :MIRRORSHOT,   :IRONDEFENSE, null,         null,           :METALSOUND},
				FIRE     = new {:FIREPUNCH,    :EMBER,        :SUNNYDAY,    null,         :INCINERATE,   :WILLOWISP},
				WATER    = new {:CRABHAMMER,   :WATERGUN,     :AQUARING,    null,         :SURF,         :WATERSPORT},
				GRASS    = new {:VINEWHIP,     :MEGADRAIN,    :COTTONGUARD, :RAZORLEAF,  null,           :SPORE},
				ELECTRIC = new {:THUNDERPUNCH, :THUNDERSHOCK, :CHARGE,      null,         :DISCHARGE,    :THUNDERWAVE},
				PSYCHIC  = new {:ZENHEADBUTT,  :CONFUSION,    :CALMMIND,    null,         :SYNCHRONOISE, :MIRACLEEYE},
				ICE      = new {:ICEPUNCH,     :ICEBEAM,      :MIST,        null,         :POWDERSNOW,   :HAIL},
				DRAGON   = new {:DRAGONCLAW,   :DRAGONRAGE,   :DRAGONDANCE, null,         :TWISTER,      null},
				DARK     = new {:PURSUIT,      :DARKPULSE,    :HONECLAWS,   null,         :SNARL,        :EMBARGO},
				FAIRY    = new {:TACKLE,       :FAIRYWIND,    :MOONLIGHT,   null,         :SWIFT,        :SWEETKISS}
			}
			if (typeDefaultAnim[moveType]) {
				anims = typeDefaultAnim[moveType];
				if (GameData.Move.exists(anims[moveKind])) {
					anim = FindMoveAnimDetails(move2anim, anims[moveKind], idxUser);
				}
				if (!anim && moveKind >= 3 && GameData.Move.exists(anims[moveKind - 3])) {
					anim = FindMoveAnimDetails(move2anim, anims[moveKind - 3], idxUser);
				}
				if (!anim && GameData.Move.exists(anims[2])) {
					anim = FindMoveAnimDetails(move2anim, anims[2], idxUser);
				}
			}
			if (anim) return anim;
			// Default animation for the move's type not found, use Tackle's animation
			if (GameData.Moves.exists(Moves.TACKLE)) {
				return FindMoveAnimDetails(move2anim, :TACKLE, idxUser);
			}
		rescue;
		}
		return null;
	}

	//-----------------------------------------------------------------------------
	// Plays a move/common animation.
	//-----------------------------------------------------------------------------

	// Plays a move animation.
	public void Animation(moveID, user, targets, hitNum = 0) {
		animID = FindMoveAnimation(moveID, user.index, hitNum);
		if (!animID) return;
		anim = animID[0];
		target = (targets.Length > 0) ? targets[0] : targets;
		animations = LoadBattleAnimations;
		if (!animations) return;
		SaveShadows do;
			if (animID[1]) {   // On opposing side and using OppMove animation
				AnimationCore(animations[anim], target, user, true);
			} else {           // On player's side, and/or using Move animation
				AnimationCore(animations[anim], user, target);
			}
		}
	}

	// Plays a common animation.
	public void CommonAnimation(animName, user = null, target = null) {
		if (nil_or_empty(animName)) return;
		if (target.Length > 0) target = target[0];
		animations = LoadBattleAnimations;
		if (!animations) return;
		foreach (var a in animations) { //'animations.each' do => |a|
			if (!a || a.name != "Common:" + animName) continue;
			AnimationCore(a, user, target || user);
			return;
		}
	}

	public void AnimationCore(animation, user, target, oppMove = false) {
		if (!animation) return;
		@briefMessage = false;
		userSprite   = (user) ? @sprites[$"pokemon_{user.index}"] : null;
		targetSprite = (target) ? @sprites[$"pokemon_{target.index}"] : null;
		// Remember the original positions of Pokémon sprites
		oldUserX = (userSprite) ? userSprite.x : 0;
		oldUserY = (userSprite) ? userSprite.y : 0;
		oldTargetX = (targetSprite) ? targetSprite.x : oldUserX;
		oldTargetY = (targetSprite) ? targetSprite.y : oldUserY;
		// Create the animation player
		animPlayer = new AnimationPlayerX(animation, user, target, self, oppMove);
		// Apply a transformation to the animation based on where the user and target
		// actually are. Get the centres of each sprite.
		userHeight = (userSprite&.bitmap && !userSprite.bitmap.disposed()) ? userSprite.bitmap.height : 128;
		if (targetSprite) {
			targetHeight = (targetSprite.bitmap && !targetSprite.bitmap.disposed()) ? targetSprite.bitmap.height : 128;
		} else {
			targetHeight = userHeight;
		}
		animPlayer.setLineTransform(
			FOCUSUSER_X, FOCUSUSER_Y, FOCUSTARGET_X, FOCUSTARGET_Y,
			oldUserX, oldUserY - (userHeight / 2), oldTargetX, oldTargetY - (targetHeight / 2)
		);
		// Play the animation
		animPlayer.start;
		do { //loop; while (true);
			animPlayer.update;
			Update;
			if (animPlayer.animDone()) break;
		}
		animPlayer.dispose;
		// Return Pokémon sprites to their original positions
		if (userSprite) {
			userSprite.x = oldUserX;
			userSprite.y = oldUserY;
			userSprite.SetOrigin;
		}
		if (targetSprite) {
			targetSprite.x = oldTargetX;
			targetSprite.y = oldTargetY;
			targetSprite.SetOrigin;
		}
	}

	// Ball burst common animations should have a focus of "Target" and a priority
	// of "Front".
	public void BallBurstCommonAnimation(_picture_ex, anim_name, battler, target_x, target_y) {
		if (nil_or_empty(anim_name)) return;
		animations = LoadBattleAnimations;
		anim = animations&.get_from_name("Common:" + anim_name);
		if (!anim) return;
		animPlayer = new AnimationPlayerX(anim, battler, null, self);
		animPlayer.discard_user_and_target_sprites;   // Don't involve user/target in animation
		animPlayer.set_target_origin(target_x, target_y);
		animPlayer.start;
		@animations.Add(animPlayer);
	}
}
