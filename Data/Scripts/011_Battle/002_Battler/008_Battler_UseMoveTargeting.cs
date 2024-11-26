//===============================================================================
//
//===============================================================================
public partial class Battle.Battler {
	//-----------------------------------------------------------------------------
	// Get move's user.
	//-----------------------------------------------------------------------------

	public void FindUser(_choice, _move) {
		return self;
	}

	public void ChangeUser(choice, move, user) {
		// Snatch
		move.snatched = false;
		if (move.statusMove() && move.canSnatch()) {
			newUser = null;
			strength = 100;
			@battle.allBattlers.each do |b|
				if (b.effects.Snatch == 0 ||
								b.effects.Snatch >= strength) continue;
				if (b.effects.SkyDrop >= 0) continue;
				newUser = b;
				strength = b.effects.Snatch;
			}
			if (newUser) {
				user = newUser;
				user.effects.Snatch = 0;
				move.snatched = true;
				@battle.moldBreaker = user.hasMoldBreaker();
				choice[3] = -1;   // Clear pre-chosen target
			}
		}
		return user;
	}

	//-----------------------------------------------------------------------------
	// Get move's default target(s).
	//-----------------------------------------------------------------------------

	public void FindTargets(choice, move, user) {
		preTarget = choice[3];   // A target that was already chosen
		targets = new List<string>();
		// Get list of targets
		switch (move.Target(user).id) {   // Curse can change its target type
			case :NearAlly:
				targetBattler = (preTarget >= 0) ? @battle.battlers[preTarget] : null;
				if (!AddTarget(targets, user, targetBattler, move)) {
					AddTargetRandomAlly(targets, user, move);
				}
				break;
			case :UserOrNearAlly:
				targetBattler = (preTarget >= 0) ? @battle.battlers[preTarget] : null;
				if (!AddTarget(targets, user, targetBattler, move, true, true)) {
					AddTarget(targets, user, user, move, true, true);
				}
				break;
			case :AllAllies:
				@battle.allSameSideBattlers(user.index).each do |b|
					if (b.index != user.index) AddTarget(targets, user, b, move, false, true);
				}
				break;
			case :UserAndAllies:
				AddTarget(targets, user, user, move, true, true);
				@battle.allSameSideBattlers(user.index).each(b => AddTarget(targets, user, b, move, false, true));
				break;
			case :NearFoe: case :NearOther:
				targetBattler = (preTarget >= 0) ? @battle.battlers[preTarget] : null;
				if (!AddTarget(targets, user, targetBattler, move)) {
					if (preTarget >= 0 && !user.opposes(preTarget)) {
						AddTargetRandomAlly(targets, user, move);
					} else {
						AddTargetRandomFoe(targets, user, move);
					}
				}
				break;
			case :RandomNearFoe:
				AddTargetRandomFoe(targets, user, move);
				break;
			case :AllNearFoes:
				@battle.allOtherSideBattlers(user.index).each(b => AddTarget(targets, user, b, move));
				break;
			case :Foe: case :Other:
				targetBattler = (preTarget >= 0) ? @battle.battlers[preTarget] : null;
				if (!AddTarget(targets, user, targetBattler, move, false)) {
					if (preTarget >= 0 && !user.opposes(preTarget)) {
						AddTargetRandomAlly(targets, user, move, false);
					} else {
						AddTargetRandomFoe(targets, user, move, false);
					}
				}
				break;
			case :AllFoes:
				@battle.allOtherSideBattlers(user.index).each(b => AddTarget(targets, user, b, move, false));
				break;
			case :AllNearOthers:
				@battle.allBattlers.each(b => AddTarget(targets, user, b, move));
				break;
			case :AllBattlers:
				@battle.allBattlers.each(b => AddTarget(targets, user, b, move, false, true));
				break;
			default:
				// Used by Counter/Mirror Coat/Metal Burst/Bide
				move.AddTarget(targets, user);   // Move-specific AddTarget, not the def below
				break;
		}
		return targets;
	}

	//-----------------------------------------------------------------------------
	// Redirect attack to another target.
	//-----------------------------------------------------------------------------

	public void ChangeTargets(move, user, targets) {
		target_data = move.Target(user);
		if (@battle.switching) return targets;   // For Pursuit interrupting a switch
		if (move.cannotRedirect() || move.targetsPosition()) return targets;
		if (!target_data.can_target_one_foe() || targets.length != 1) return targets;
		move.ModifyTargets(targets, user);   // For Dragon Darts
		if (user.hasActiveAbility(new {:PROPELLERTAIL, :STALWART})) return targets;
		priority = @battle.Priority(true);
		nearOnly = !target_data.can_choose_distant_target();
		// Spotlight (takes priority over Follow Me/Rage Powder/Lightning Rod/Storm Drain)
		newTarget = null;
		strength = 100;   // Lower strength takes priority
		foreach (var b in priority) { //'priority.each' do => |b|
			if (b.fainted() || b.effects.SkyDrop >= 0) continue;
			if (b.effects.Spotlight == 0 ||
							b.effects.Spotlight >= strength) continue;
			if (!b.opposes(user)) continue;
			if (nearOnly && !b.near(user)) continue;
			newTarget = b;
			strength = b.effects.Spotlight;
		}
		if (newTarget) {
			Debug.Log($"[Move target changed] {newTarget.ToString()}'s Spotlight made it the target");
			targets = new List<string>();
			AddTarget(targets, user, newTarget, move, nearOnly);
			return targets;
		}
		// Follow Me/Rage Powder (takes priority over Lightning Rod/Storm Drain)
		newTarget = null;
		strength = 100;   // Lower strength takes priority
		foreach (var b in priority) { //'priority.each' do => |b|
			if (b.fainted() || b.effects.SkyDrop >= 0) continue;
			if (b.effects.RagePowder && !user.affectedByPowder()) continue;
			if (b.effects.FollowMe == 0 ||
							b.effects.FollowMe >= strength) continue;
			if (!b.opposes(user)) continue;
			if (nearOnly && !b.near(user)) continue;
			newTarget = b;
			strength = b.effects.FollowMe;
		}
		if (newTarget) {
			Debug.Log($"[Move target changed] {newTarget.ToString()}'s Follow Me/Rage Powder made it the target");
			targets = new List<string>();
			AddTarget(targets, user, newTarget, move, nearOnly);
			return targets;
		}
		// Lightning Rod
		targets = ChangeTargetByAbility(:LIGHTNINGROD, :ELECTRIC, move, user, targets, priority, nearOnly);
		// Storm Drain
		targets = ChangeTargetByAbility(:STORMDRAIN, :WATER, move, user, targets, priority, nearOnly);
		return targets;
	}

	public void ChangeTargetByAbility(drawingAbility, drawnType, move, user, targets, priority, nearOnly) {
		if (move.calcType != drawnType) return targets;
		if (targets[0].hasActiveAbility(drawingAbility)) return targets;
		foreach (var b in priority) { //'priority.each' do => |b|
			if (b.index == user.index || b.index == targets[0].index) continue;
			if (!b.hasActiveAbility(drawingAbility)) continue;
			if (nearOnly && !b.near(user)) continue;
			@battle.ShowAbilitySplash(b);
			targets.clear;
			AddTarget(targets, user, b, move, nearOnly);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				@battle.Display(_INTL("{1} took the attack!", b.ToString()));
			} else {
				@battle.Display(_INTL("{1} took the attack with its {2}!", b.ToString(), b.abilityName));
			}
			@battle.HideAbilitySplash(b);
			break;
		}
		return targets;
	}

	//-----------------------------------------------------------------------------
	// Register target.
	//-----------------------------------------------------------------------------

	public void AddTarget(targets, user, target, move, nearOnly = true, allowUser = false) {
		if (!target || (target.fainted() && !move.targetsPosition())) return false;
		if (!allowUser && target == user) return false;
		if (nearOnly && !user.near(target) && target != user) return false;
		targets.each(b => { if (b.index == target.index) return true; });   // Already added
		targets.Add(target);
		return true;
	}

	public void AddTargetRandomAlly(targets, user, move, nearOnly = true) {
		choices = new List<string>();
		foreach (var b in user.allAllies) { //'user.allAllies.each' do => |b|
			if (nearOnly && !user.near(b)) continue;
			AddTarget(choices, user, b, move, nearOnly);
		}
		if (choices.length > 0) {
			AddTarget(targets, user, choices[@battle.Random(choices.length)], move, nearOnly);
		}
	}

	public void AddTargetRandomFoe(targets, user, move, nearOnly = true) {
		choices = new List<string>();
		foreach (var b in user.allOpposing) { //'user.allOpposing.each' do => |b|
			if (nearOnly && !user.near(b)) continue;
			AddTarget(choices, user, b, move, nearOnly);
		}
		if (choices.length > 0) {
			AddTarget(targets, user, choices[@battle.Random(choices.length)], move, nearOnly);
		}
	}
}
