//===============================================================================
// This script modifies the battle system to implement battle rules.
//===============================================================================
public partial class Battle {
	unless (@__clauses__aliased) {
		alias __clauses__pbDecisionOnDraw DecisionOnDraw;
		alias __clauses__pbEndOfRoundPhase EndOfRoundPhase;
		@__clauses__aliased = true;
	}

	public void DecisionOnDraw() {
		if (@rules["selfkoclause"]) {
			if (self.lastMoveUser < 0) {
				// in extreme cases there may be no last move user
				return Outcome.DRAW;
			} else if (opposes(self.lastMoveUser)) {
				return Outcome.LOSE;
			} else {
				return Outcome.WIN;
			}
		}
		return __clauses__pbDecisionOnDraw;
	}

	public void JudgeCheckpoint(user, move = null) {
		if (AllFainted(0) && AllFainted(1)) {
			if (@rules["drawclause"]) {   // NOTE: Also includes Life Orb (not implemented)
				if (!(move && move.function_code == "HealUserByHalfOfDamageDone")) {
					// Not a draw if fainting occurred due to Liquid Ooze
					@decision = (user.opposes()) ? Outcome.WIN : Outcome.LOSE;
				}
			} else if (@rules["modifiedselfdestructclause"]) {
				if (move && move.function_code == "UserFaintsExplosive") {   // Self-Destruct
					@decision = (user.opposes()) ? Outcome.WIN : Outcome.LOSE;
				}
			}
		}
	}

	public void EndOfRoundPhase() {
		__clauses__pbEndOfRoundPhase;
		if (@rules["suddendeath"] && !decided()) {
			p1able = AbleCount(0);
			p2able = AbleCount(1);
			if (p1able > p2able) {
				@decision = Outcome.WIN;
			} else if (p1able < p2able) {
				@decision = Outcome.LOSE;
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class Battle.Battler {
	unless (@__clauses__aliased) {
		alias __clauses__pbCanSleep() CanSleep();
		alias __clauses__pbCanSleepYawn() CanSleepYawn();
		alias __clauses__pbCanFreeze() CanFreeze();
		alias __clauses__pbUseMove UseMove;
		@__clauses__aliased = true;
	}

	public bool CanSleep(user, showMessages, move = null, ignoreStatus = false) {
		selfsleep = (user && user.index == @index);
		if (((@battle.rules["modifiedsleepclause"]) || (!selfsleep && @battle.rules["sleepclause"])) &&
			HasStatusPokemon(:SLEEP)) {
			if (showMessages) {
				@battle.Display(_INTL("But {1} couldn't sleep!", This(true)));
			}
			return false;
		}
		return __clauses__pbCanSleep(user, showMessages, move, ignoreStatus);
	}

	public bool CanSleepYawn() {
		if ((@battle.rules["sleepclause"] || @battle.rules["modifiedsleepclause"]) &&
			HasStatusPokemon(:SLEEP)) {
			return false;
		}
		return __clauses__pbCanSleepYawn();
	}

	public bool CanFreeze(*arg) {
		if (@battle.rules["freezeclause"] && HasStatusPokemon(:FROZEN)) {
			return false;
		}
		return __clauses__pbCanFreeze(*arg);
	}

	public bool HasStatusPokemon(status) {
		count = 0;
		@battle.Party(@index).each do |pkmn|
			if (!pkmn || pkmn.egg()) continue;
			if (pkmn.status != status) continue;
			count += 1;
		}
		return count > 0;
	}
}

//===============================================================================
// Double Team.
//===============================================================================
public partial class Battle.Move.RaiseUserEvasion1 {
	unless (method_defined(:__clauses__pbMoveFailed())) {
		alias __clauses__pbMoveFailed() MoveFailed();
	}

	public bool MoveFailed(user, targets) {
		if (!damagingMove() && @battle.rules["evasionclause"]) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return __clauses__pbMoveFailed(user, targets);
	}
}

//===============================================================================
// Minimize.
//===============================================================================
public partial class Battle.Move.RaiseUserEvasion2MinimizeUser {
	unless (method_defined(:__clauses__pbMoveFailed())) {
		alias __clauses__pbMoveFailed() MoveFailed();
	}

	public bool MoveFailed(user, targets) {
		if (!damagingMove() && @battle.rules["evasionclause"]) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return __clauses__pbMoveFailed(user, targets);
	}
}

//===============================================================================
// Skill Swap.
//===============================================================================
public partial class Battle.Move.UserTargetSwapAbilities {
	unless (method_defined(:__clauses__pbFailsAgainstTarget())) {
		alias __clauses__pbFailsAgainstTarget() FailsAgainstTarget();
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@battle.rules["skillswapclause"]) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return __clauses__pbFailsAgainstTarget(user, target, show_message);
	}
}

//===============================================================================
// Sonic Boom.
//===============================================================================
public partial class Battle.Move.FixedDamage20 {
	unless (method_defined(:__clauses__pbFailsAgainstTarget())) {
		alias __clauses__pbFailsAgainstTarget() FailsAgainstTarget();
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@battle.rules["sonicboomclause"]) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return __clauses__pbFailsAgainstTarget(user, target, show_message);
	}
}

//===============================================================================
// Dragon Rage.
//===============================================================================
public partial class Battle.Move.FixedDamage40 {
	unless (method_defined(:__clauses__pbFailsAgainstTarget())) {
		alias __clauses__pbFailsAgainstTarget() FailsAgainstTarget();
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@battle.rules["sonicboomclause"]) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return __clauses__pbFailsAgainstTarget(user, target, show_message);
	}
}

//===============================================================================
//
//===============================================================================
public partial class Battle.Move.OHKO {
	unless (method_defined(:__clauses__pbFailsAgainstTarget())) {
		alias __clauses__pbFailsAgainstTarget() FailsAgainstTarget();
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@battle.rules["ohkoclause"]) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return __clauses__pbFailsAgainstTarget(user, target, show_message);
	}
}

//===============================================================================
//
//===============================================================================
public partial class Battle.Move.OHKOIce {
	unless (method_defined(:__clauses__pbFailsAgainstTarget())) {
		alias __clauses__pbFailsAgainstTarget() FailsAgainstTarget();
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@battle.rules["ohkoclause"]) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return __clauses__pbFailsAgainstTarget(user, target, show_message);
	}
}

//===============================================================================
//
//===============================================================================
public partial class Battle.Move.OHKOHitsUndergroundTarget {
	unless (method_defined(:__clauses__pbFailsAgainstTarget())) {
		alias __clauses__pbFailsAgainstTarget() FailsAgainstTarget();
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@battle.rules["ohkoclause"]) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return __clauses__pbFailsAgainstTarget(user, target, show_message);
	}
}

//===============================================================================
// Self-Destruct.
//===============================================================================
public partial class Battle.Move.UserFaintsExplosive {
	unless (method_defined(:__clauses__pbMoveFailed())) {
		alias __clauses__pbMoveFailed() MoveFailed();
	}

	public bool MoveFailed(user, targets) {
		if (@battle.rules["selfkoclause"]) {
			// Check whether no unfainted Pokemon remain in either party
			count = @battle.AbleNonActiveCount(user.idxOwnSide);
			count += @battle.AbleNonActiveCount(user.idxOpposingSide);
			if (count == 0) {
				@battle.Display("But it failed!");
				return false;
			}
		}
		if (@battle.rules["selfdestructclause"]) {
			// Check whether no unfainted Pokemon remain in either party
			count = @battle.AbleNonActiveCount(user.idxOwnSide);
			count += @battle.AbleNonActiveCount(user.idxOpposingSide);
			if (count == 0) {
				@battle.Display(_INTL("{1}'s team was disqualified!", user.ToString()));
				@battle.decision = (user.opposes()) ? Outcome.WIN : Outcome.LOSE;
				return false;
			}
		}
		return __clauses__pbMoveFailed(user, targets);
	}
}

//===============================================================================
// Perish Song.
//===============================================================================
public partial class Battle.Move.StartPerishCountsForAllBattlers {
	unless (method_defined(:__clauses__pbFailsAgainstTarget())) {
		alias __clauses__pbFailsAgainstTarget() FailsAgainstTarget();
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@battle.rules["perishsongclause"] &&
			@battle.AbleNonActiveCount(user.idxOwnSide) == 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return __clauses__pbFailsAgainstTarget(user, target, show_message);
	}
}

//===============================================================================
// Destiny Bond.
//===============================================================================
public partial class Battle.Move.AttackerFaintsIfUserFaints {
	unless (method_defined(:__clauses__pbFailsAgainstTarget())) {
		alias __clauses__pbFailsAgainstTarget() FailsAgainstTarget();
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@battle.rules["perishsongclause"] &&
			@battle.AbleNonActiveCount(user.idxOwnSide) == 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return __clauses__pbFailsAgainstTarget(user, target, show_message);
	}
}
