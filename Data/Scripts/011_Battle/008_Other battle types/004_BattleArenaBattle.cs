//===============================================================================
// Success state.
//===============================================================================
public partial class Battle.SuccessState {
	public int typeMod		{ get { return _typeMod; } set { _typeMod = value; } }			protected int _typeMod;
	/// <summary>0 - not used, 1 - failed, 2 - succeeded</summary>
	public int useState		{ get { return _useState; } set { _useState = value; } }			protected int _useState;
	public int protected		{ get { return _protected; } set { _protected = value; } }			protected int _protected;
	public int skill		{ get { return _skill; } set { _skill = value; } }			protected int _skill;

	public int initialize { get { return clear; } }

	public void clear(full = true) {
		@typeMod   = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
		@useState  = 0;
		@protected = false;
		if (full) @skill     = 0;
	}

	public void updateSkill() {
		switch (@useState) {
			case 1:
				if (!@protected) @skill = -2;
				break;
			case 2:
				if (Effectiveness.super_effective(@typeMod)) {
					@skill = 2;
				} else if (Effectiveness.normal(@typeMod)) {
					@skill = 1;
				} else if (Effectiveness.not_very_effective(@typeMod)) {
					@skill = -1;
				} else {   // Ineffective
					@skill = -2;
				}
				break;
		}
		clear(false);
	}
}

//===============================================================================
//
//===============================================================================
public partial class BattleArenaBattle : Battle {
	public override void initialize(*arg) {
		base.initialize();
		@battlersChanged      = true;
		@mind                 = new {0, 0};
		@skill                = new {0, 0};
		@starthp              = new {0, 0};
		@count                = 0;
		@partyindexes         = new {0, 0};
		@battleAI.battleArena = true;
	}

	public bool CanSwitchIn(idxBattler, _idxParty, partyScene = null) {
		partyScene&.Display(_INTL("{1} can't be switched out!", @battlers[idxBattler].ToString()));
		return false;
	}

	public void EORSwitch(favorDraws = false) {
		if (favorDraws && @decision == Battle.Outcome.DRAW) return;
		if (!favorDraws && decided()) return;
		Judge;
		if (decided()) return;
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			if (!@battlers[side].fainted()) continue;
			if (@partyindexes[side] + 1 >= self.Party(side).length) continue;
			@partyindexes[side] += 1;
			newpoke = @partyindexes[side];
			MessagesOnReplace(side, newpoke);
			Replace(side, newpoke);
			OnBattlerEnteringBattle(side);
		}
	}

	public void OnAllBattlersEnteringBattle() {
		@battlersChanged = true;
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			@mind[side]    = 0;
			@skill[side]   = 0;
			@starthp[side] = battlers[side].hp;
		}
		@count           = 0;
		return super;
	}

	public void RecordBattlerAsActive(battler) {
		@battlersChanged = true;
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			@mind[side]    = 0;
			@skill[side]   = 0;
			@starthp[side] = @battlers[side].hp;
		}
		@count           = 0;
		return super;
	}

	public void MindScore(move) {
		if (move.function_code == "ProtectUser" ||   // Detect/Protect
			move.function_code == "UserEnduresFaintingThisTurn" ||   // Endure
			move.function_code == "FlinchTargetFailsIfNotUserFirstTurn") {   // Fake Out
			return -1;
		}
		if (move.function_code == "CounterPhysicalDamage" ||   // Counter
			move.function_code == "CounterSpecialDamage" ||   // Mirror Coat
			move.function_code == "MultiTurnAttackBideThenReturnDoubleDamage") {   // Bide
			return 0;
		}
		if (move.statusMove()) return 0;
		return 1;
	}

	public void CommandPhase() {
		if (@battlersChanged) {
			@scene.BattleArenaBattlers(@battlers[0], @battlers[1]);
			@battlersChanged = false;
			@count = 0;
		}
		super;
		if (decided()) return;
		// Update mind rating (asserting that a move was chosen)
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			if (@choices[side].Move && @choices[side].Action == :UseMove) {
				@mind[side] += MindScore(@choices[side].Move);
			}
		}
	}

	public override void EndOfRoundPhase() {
		base.EndOfRoundPhase();
		if (decided()) return;
		// Update skill rating
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			@skill[side] += self.successStates[side].skill;
		}
//		Debug.Log($"[Mind: {@mind.inspect}, Skill: {@skill.inspect}]")
		// Increment turn counter
		@count += 1;
		if (@count < 3) return;
		// Half all multi-turn moves
		@battlers[0].CancelMoves(true);
		@battlers[1].CancelMoves(true);
		// Calculate scores in each category
		ratings1 = new {0, 0, 0};
		ratings2 = new {0, 0, 0};
		if (@mind[0] == @mind[1]) {
			ratings1[0] = 1;
			ratings2[0] = 1;
		} else if (@mind[0] > @mind[1]) {
			ratings1[0] = 2;
		} else {
			ratings2[0] = 2;
		}
		if (@skill[0] == @skill[1]) {
			ratings1[1] = 1;
			ratings2[1] = 1;
		} else if (@skill[0] > @skill[1]) {
			ratings1[1] = 2;
		} else {
			ratings2[1] = 2;
		}
		body = new {0, 0};
		body[0] = (int)Math.Floor((@battlers[0].hp * 100) / (int)Math.Max(@starthp[0], 1));
		body[1] = (int)Math.Floor((@battlers[1].hp * 100) / (int)Math.Max(@starthp[1], 1));
		if (body[0] == body[1]) {
			ratings1[2] = 1;
			ratings2[2] = 1;
		} else if (body[0] > body[1]) {
			ratings1[2] = 2;
		} else {
			ratings2[2] = 2;
		}
		// Show scores
		@scene.BattleArenaJudgment(@battlers[0], @battlers[1], ratings1.clone, ratings2.clone);
		// Calculate total scores
		points = new {0, 0};
		ratings1.each(val => points[0] += val);
		ratings2.each(val => points[1] += val);
		// Make judgment
		if (points[0] == points[1]) {
			Display(_INTL("{1} tied the opponent {2} in a referee's decision!",
											@battlers[0].name, @battlers[1].name));
			// NOTE: Pokémon doesn't really lose HP, but the effect is mostly the
			//       same.
			@battlers[0].hp = 0;
			@battlers[0].Faint(false);
			@battlers[1].hp = 0;
			@battlers[1].Faint(false);
		} else if (points[0] > points[1]) {
			Display(_INTL("{1} defeated the opponent {2} in a referee's decision!",
											@battlers[0].name, @battlers[1].name));
			@battlers[1].hp = 0;
			@battlers[1].Faint(false);
		} else {
			Display(_INTL("{1} lost to the opponent {2} in a referee's decision!",
											@battlers[0].name, @battlers[1].name));
			@battlers[0].hp = 0;
			@battlers[0].Faint(false);
		}
		GainExp;
		EORSwitch;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Battle.AI {
	public int battleArena		{ get { return _battleArena; } set { _battleArena = value; } }			protected int _battleArena;

	unless (method_defined(:_battleArena_pbChooseToSwitchOut)) {
		alias _battleArena_pbChooseToSwitchOut ChooseToSwitchOut;
	}

	public void ChooseToSwitchOut(force_switch = false) {
		if (!@battleArena) return _battleArena_pbChooseToSwitchOut(force_switch);
		return false;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Battle.Scene {
	public void BattleArenaUpdate() {
		GraphicsUpdate;
	}

	public void updateJudgment(window, phase, battler1, battler2, ratings1, ratings2) {
		total1 = 0;
		total2 = 0;
		for (int i = phase; i < phase; i++) { //for 'phase' times do => |i|
			total1 += ratings1[i];
			total2 += ratings2[i];
		}
		window.contents.clear;
		SetSystemFont(window.contents);
		textpos = new {
			new {battler1.name, 64, 6, :center, new Color(248, 0, 0), new Color(208, 208, 200)},
			new {_INTL("VS"), 144, 6, :center, new Color(72, 72, 72), new Color(208, 208, 200)},
			new {battler2.name, 224, 6, :center, new Color(72, 72, 72), new Color(208, 208, 200)},
			new {_INTL("Mind"), 144, 54, :center, new Color(72, 72, 72), new Color(208, 208, 200)},
			new {_INTL("Skill"), 144, 86, :center, new Color(72, 72, 72), new Color(208, 208, 200)},
			new {_INTL("Body"), 144, 118, :center, new Color(72, 72, 72), new Color(208, 208, 200)},
			new {total1.ToString(), 64, 166, :center, new Color(72, 72, 72), new Color(208, 208, 200)},
			new {_INTL("Judgment"), 144, 166, :center, new Color(72, 72, 72), new Color(208, 208, 200)},
			new {total2.ToString(), 224, 166, :center, new Color(72, 72, 72), new Color(208, 208, 200)}
		}
		DrawTextPositions(window.contents, textpos);
		images = new List<string>();
		for (int i = phase; i < phase; i++) { //for 'phase' times do => |i|
			y = new {48, 80, 112}[i];
			x = (ratings1[i] == ratings2[i]) ? 64 : ((ratings1[i] > ratings2[i]) ? 0 : 32);
			images.Add(new {"Graphics/UI/Battle/judgment", 64 - 16, y, x, 0, 32, 32});
			x = (ratings1[i] == ratings2[i]) ? 64 : ((ratings1[i] < ratings2[i]) ? 0 : 32);
			images.Add(new {"Graphics/UI/Battle/judgment", 224 - 16, y, x, 0, 32, 32});
		}
		DrawImagePositions(window.contents, images);
		window.contents.fill_rect(16, 150, 256, 4, new Color(80, 80, 80));
	}

	public void BattleArenaBattlers(battler1, battler2) {
		Message(_INTL("REFEREE: {1} VS {2}!\nCommence battling!",
										battler1.name, battler2.name) + "\\wtnp[10]") { BattleArenaUpdate };
	}

	public void BattleArenaJudgment(battler1, battler2, ratings1, ratings2) {
		msgwindow  = null;
		dimmingvp  = null;
		infowindow = null;
		begin;
			msgwindow = CreateMessageWindow;
			dimmingvp = new Viewport(0, 0, Graphics.width, Graphics.height - msgwindow.height);
			MessageDisplay(msgwindow,
											_INTL("REFEREE: That's it! We will now go to judging to determine the winner!") + "\\wtnp[10]") do;
				BattleArenaUpdate;
				dimmingvp.update;
			}
			dimmingvp.z = 99999;
			infowindow = new SpriteWindow_Base(80, 0, 320, 224);
			infowindow.contents = new Bitmap(infowindow.width - infowindow.borderX,
																			infowindow.height - infowindow.borderY);
			infowindow.z        = 99999;
			infowindow.visible  = false;
			for (int i = 11; i < 11; i++) { //for '11' times do => |i|
				GraphicsUpdate;
				InputUpdate;
				msgwindow.update;
				dimmingvp.update;
				dimmingvp.color = new Color(0, 0, 0, i * 128 / 10);
			}
			updateJudgment(infowindow, 0, battler1, battler2, ratings1, ratings2);
			infowindow.visible = true;
			for (int i = 11; i < 11; i++) { //for '11' times do => |i|
				GraphicsUpdate;
				InputUpdate;
				msgwindow.update;
				dimmingvp.update;
				infowindow.update;
			}
			updateJudgment(infowindow, 1, battler1, battler2, ratings1, ratings2);
			MessageDisplay(msgwindow,
											_INTL("REFEREE: Judging category 1, Mind!\nThe Pokémon showing the most guts!") + "\\wtnp[20]") do;
				BattleArenaUpdate;
				dimmingvp.update;
				infowindow.update;
			}
			updateJudgment(infowindow, 2, battler1, battler2, ratings1, ratings2);
			MessageDisplay(msgwindow,
											_INTL("REFEREE: Judging category 2, Skill!\nThe Pokémon using moves the best!") + "\\wtnp[20]") do;
				BattleArenaUpdate;
				dimmingvp.update;
				infowindow.update;
			}
			updateJudgment(infowindow, 3, battler1, battler2, ratings1, ratings2);
			MessageDisplay(msgwindow,
											_INTL("REFEREE: Judging category 3, Body!\nThe Pokémon with the most vitality!") + "\\wtnp[20]") do;
				BattleArenaUpdate;
				dimmingvp.update;
				infowindow.update;
			}
			total1 = 0;
			total2 = 0;
			for (int i = 3; i < 3; i++) { //for '3' times do => |i|
				total1 += ratings1[i];
				total2 += ratings2[i];
			}
			if (total1 == total2) {
				MessageDisplay(msgwindow,
												_INTL("REFEREE: Judgment: {1} to {2}!\nWe have a draw!", total1, total2) + "\\wtnp[20]") do;
					BattleArenaUpdate;
					dimmingvp.update;
					infowindow.update;
				}
			} else if (total1 > total2) {
				MessageDisplay(msgwindow,
												_INTL("REFEREE: Judgment: {1} to {2}!\nThe winner is {3}'s {4}!",
															total1, total2, @battle.GetOwnerName(battler1.index), battler1.name) + "\\wtnp[20]") do;
					BattleArenaUpdate;
					dimmingvp.update;
					infowindow.update;
				}
			} else {
				MessageDisplay(msgwindow,
												_INTL("REFEREE: Judgment: {1} to {2}!\nThe winner is {3}!",
															total1, total2, battler2.name) + "\\wtnp[20]") do;
					BattleArenaUpdate;
					dimmingvp.update;
					infowindow.update;
				}
			}
			infowindow.visible = false;
			msgwindow.visible  = false;
			for (int i = 11; i < 11; i++) { //for '11' times do => |i|
				GraphicsUpdate;
				InputUpdate;
				msgwindow.update;
				dimmingvp.update;
				dimmingvp.color = new Color(0, 0, 0, (10 - i) * 128 / 10);
			}
		ensure;
			DisposeMessageWindow(msgwindow);
			dimmingvp.dispose;
			infowindow&.contents&.dispose;
			infowindow&.dispose;
		}
	}
}
