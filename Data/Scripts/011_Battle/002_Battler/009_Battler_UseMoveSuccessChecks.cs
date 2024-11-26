//===============================================================================
//
//===============================================================================
public partial class Battle.Battler {
	// Decide whether the trainer is allowed to tell the Pokémon to use the given
	// move. Called when choosing a command for the round.
	// Also called when processing the Pokémon's action, because these effects also
	// prevent Pokémon action. Relevant because these effects can become active
	// earlier in the same round (after choosing the command but before using the
	// move) or an unusable move may be called by another move such as Metronome.
	public bool CanChooseMove(move, commandPhase, showMessages = true, specialUsage = false) {
		// Disable
		if (@effects.DisableMove == move.id && !specialUsage) {
			if (showMessages) {
				msg = _INTL("{1}'s {2} is disabled!", This, move.name);
				(commandPhase) ? @battle.DisplayPaused(msg) : @battle.Display(msg)
			}
			return false;
		}
		// Heal Block
		if (@effects.HealBlock > 0 && move.healingMove()) {
			if (showMessages) {
				msg = _INTL("{1} can't use {2} because of Heal Block!", This, move.name);
				(commandPhase) ? @battle.DisplayPaused(msg) : @battle.Display(msg)
			}
			return false;
		}
		// Gravity
		if (@battle.field.effects.Gravity > 0 && move.unusableInGravity()) {
			if (showMessages) {
				msg = _INTL("{1} can't use {2} because of gravity!", This, move.name);
				(commandPhase) ? @battle.DisplayPaused(msg) : @battle.Display(msg)
			}
			return false;
		}
		// Throat Chop
		if (@effects.ThroatChop > 0 && move.soundMove()) {
			if (showMessages) {
				msg = _INTL("{1} can't use {2} because of Throat Chop!", This, move.name);
				(commandPhase) ? @battle.DisplayPaused(msg) : @battle.Display(msg)
			}
			return false;
		}
		// Choice Band/Gorilla Tactics
		if (!HasMove(@effects.ChoiceBand)) @effects.ChoiceBand = null;
		if (@effects.ChoiceBand && move.id != @effects.ChoiceBand) {
			choiced_move = GameData.Move.try_get(@effects.ChoiceBand);
			if (choiced_move) {
				if (hasActiveItem(new {:CHOICEBAND, :CHOICESPECS, :CHOICESCARF})) {
					if (showMessages) {
						msg = _INTL("The {1} only allows the use of {2}!", itemName, choiced_move.name);
						(commandPhase) ? @battle.DisplayPaused(msg) : @battle.Display(msg)
					}
					return false;
				} else if (hasActiveAbility(Abilitys.GORILLATACTICS)) {
					if (showMessages) {
						msg = _INTL("{1} can only use {2}!", This, choiced_move.name);
						(commandPhase) ? @battle.DisplayPaused(msg) : @battle.Display(msg)
					}
					return false;
				}
			}
		}
		// Taunt
		if (@effects.Taunt > 0 && move.statusMove() && !specialUsage) {
			if (showMessages) {
				msg = _INTL("{1} can't use {2} after the taunt!", This, move.name);
				(commandPhase) ? @battle.DisplayPaused(msg) : @battle.Display(msg)
			}
			return false;
		}
		// Torment
		if (@effects.Torment && !@effects.Instructed && !specialUsage &&
			@lastMoveUsed && move.id == @lastMoveUsed && move.id != @battle.struggle.id) {
			if (showMessages) {
				msg = _INTL("{1} can't use the same move twice in a row due to the torment!", This);
				(commandPhase) ? @battle.DisplayPaused(msg) : @battle.Display(msg)
			}
			return false;
		}
		// Imprison
		if (@battle.allOtherSideBattlers(@index).any(b => b.effects.Imprison && b.HasMove(move.id))) {
			if (showMessages) {
				msg = _INTL("{1} can't use its sealed {2}!", This, move.name);
				(commandPhase) ? @battle.DisplayPaused(msg) : @battle.Display(msg)
			}
			return false;
		}
		// Assault Vest (prevents choosing status moves but doesn't prevent
		// executing them)
		if (hasActiveItem(Items.ASSAULTVEST) && move.statusMove() && move.function_code != "UseMoveTargetIsAboutToUse" && commandPhase) {
			if (showMessages) {
				msg = _INTL("The effects of the {1} prevent status moves from being used!", itemName);
				(commandPhase) ? @battle.DisplayPaused(msg) : @battle.Display(msg)
			}
			return false;
		}
		// Belch
		if (!move.CanChooseMove(self, commandPhase, showMessages)) return false;
		return true;
	}

	//-----------------------------------------------------------------------------

	// Obedience check.
	// Return true if Pokémon continues attacking (although it may have chosen to
	// use a different move in disobedience), or false if attack stops.
	public bool ObedienceCheck(choice) {
		if (usingMultiTurnAttack()) return true;
		if (choice[0] != :UseMove) return true;
		if (!@battle.internalBattle) return true;
		if (!@battle.OwnedByPlayer(@index)) return true;
		disobedient = false;
		// Pokémon may be disobedient; calculate if it is
		badge_level = 10 * (@battle.Player.badge_count + 1);
		if (@battle.Player.badge_count >= 8) badge_level = GameData.GrowthRate.max_level;
		if (Settings.ANY_HIGH_LEVEL_POKEMON_CAN_DISOBEY ||
			(Settings.FOREIGN_HIGH_LEVEL_POKEMON_CAN_DISOBEY && @pokemon.foreign(@battle.Player))) {
			if (@level > badge_level) {
				a = (int)Math.Floor((@level + badge_level) * @battle.Random(256) / 256);
				disobedient |= (a >= badge_level);
			}
		}
		disobedient |= !HyperModeObedience(choice[2]);
		if (!disobedient) return true;
		// Pokémon is disobedient; make it do something else
		return Disobey(choice, badge_level);
	}

	public void Disobey(choice, badge_level) {
		move = choice[2];
		Debug.Log($"[Disobedience] {This} disobeyed");
		@effects.Rage = false;
		// Do nothing if using Snore/Sleep Talk
		if (@status == statuses.SLEEP && move.usableWhenAsleep()) {
			@battle.Display(_INTL("{1} ignored orders and kept sleeping!", This));
			return false;
		}
		b = (int)Math.Floor((@level + badge_level) * @battle.Random(256) / 256);
		// Use another move
		if (b < badge_level) {
			@battle.Display(_INTL("{1} ignored orders!", This));
			if (!@battle.CanShowFightMenu(@index)) return false;
			otherMoves = new List<string>();
			eachMoveWithIndex do |_m, i|
				if (i == choice[1]) continue;
				if (@battle.CanChooseMove(@index, i, false)) otherMoves.Add(i);
			}
			if (otherMoves.length == 0) return false;   // No other move to use; do nothing
			newChoice = otherMoves[@battle.Random(otherMoves.length)];
			choice[1] = newChoice;
			choice[2] = @moves[newChoice];
			choice[3] = -1;
			return true;
		}
		c = @level - badge_level;
		r = @battle.Random(256);
		// Fall asleep
		if (r < c && CanSleep(self, false)) {
			SleepSelf(_INTL("{1} began to nap!", This));
			return false;
		}
		// Hurt self in confusion
		r -= c;
		if (r < c && @status != statuses.SLEEP) {
			ConfusionDamage(_INTL("{1} won't obey! It hurt itself in its confusion!", This));
			return false;
		}
		// Show refusal message and do nothing
		switch (@battle.Random(4)) {
			case 0:  @battle.Display(_INTL("{1} won't obey!", This)); break;
			case 1:  @battle.Display(_INTL("{1} turned away!", This)); break;
			case 2:  @battle.Display(_INTL("{1} is loafing around!", This)); break;
			case 3:  @battle.Display(_INTL("{1} pretended not to notice!", This)); break;
		}
		return false;
	}

	//-----------------------------------------------------------------------------

	// Check whether the user (self) is able to take action at all.
	// If this returns true, and if PP isn't a problem, the move will be considered
	// to have been used (even if it then fails for whatever reason).
	public void TryUseMove(choice, move, specialUsage, skipAccuracyCheck) {
		// Check whether it's possible for self to use the given move
		// NOTE: Encore has already changed the move being used, no need to have a
		//       check for it here.
		if (!CanChooseMove(move, false, true, specialUsage)) {
			@lastMoveFailed = true;
			return false;
		}
		// Check whether it's possible for self to do anything at all
		if (@effects.SkyDrop >= 0) {   // Intentionally no message here
			Debug.Log($"[Move failed] {This} can't use {move.name} because of being Sky Dropped");
			return false;
		}
		if (@effects.HyperBeam > 0) {   // Intentionally before Truant
			Debug.Log($"[Move failed] {This} is recharging after using {move.name}");
			@battle.Display(_INTL("{1} must recharge!", This));
			if (hasActiveAbility(Abilitys.TRUANT)) @effects.Truant = !@effects.Truant;
			return false;
		}
		if (choice[1] == -2) {   // Battle Palace
			Debug.Log($"[Move failed] {This} can't act in the Battle Palace somehow");
			@battle.Display(_INTL("{1} appears incapable of using its power!", This));
			return false;
		}
		// Skip checking all applied effects that could make self fail doing something
		if (skipAccuracyCheck) return true;
		// Check status problems and continue their effects/cure them
		switch (@status) {
			case :SLEEP:
				self.statusCount -= 1;
				if (@statusCount <= 0) {
					CureStatus;
				} else {
					ContinueStatus;
					if (!move.usableWhenAsleep()) {   // Snore/Sleep Talk
						Debug.Log($"[Move failed] {This} is asleep");
						@lastMoveFailed = true;
						return false;
					}
				}
				break;
			case :FROZEN:
				if (!move.thawsUser()) {
					if (@battle.Random(100) < 20) {
						CureStatus;
					} else {
						ContinueStatus;
						Debug.Log($"[Move failed] {This} is frozen");
						@lastMoveFailed = true;
						return false;
					}
				}
				break;
		}
		// Obedience check
		if (!ObedienceCheck(choice)) return false;
		// Truant
		if (hasActiveAbility(Abilitys.TRUANT)) {
			@effects.Truant = !@effects.Truant;
			if (!@effects.Truant) {   // True means loafing, but was just inverted
				@battle.ShowAbilitySplash(self);
				@battle.Display(_INTL("{1} is loafing around!", This));
				@lastMoveFailed = true;
				@battle.HideAbilitySplash(self);
				Debug.Log($"[Move failed] {This} can't act because of {abilityName}");
				return false;
			}
		}
		// Flinching
		if (@effects.Flinch) {
			@battle.Display(_INTL("{1} flinched and couldn't move!", This));
			Debug.Log($"[Move failed] {This} flinched");
			if (abilityActive()) {
				Battle.AbilityEffects.triggerOnFlinch(self.ability, self, @battle);
			}
			@lastMoveFailed = true;
			return false;
		}
		// Confusion
		if (@effects.Confusion > 0) {
			@effects.Confusion -= 1;
			if (@effects.Confusion <= 0) {
				CureConfusion;
				@battle.Display(_INTL("{1} snapped out of its confusion.", This));
			} else {
				@battle.CommonAnimation("Confusion", self);
				@battle.Display(_INTL("{1} is confused!", This));
				threshold = (Settings.MECHANICS_GENERATION >= 7) ? 33 : 50;   // % chance
				if (@battle.Random(100) < threshold) {
					ConfusionDamage(_INTL("It hurt itself in its confusion!"));
					Debug.Log($"[Move failed] {This} hurt itself in its confusion");
					@lastMoveFailed = true;
					return false;
				}
			}
		}
		// Paralysis
		if (@status == statuses.PARALYSIS && @battle.Random(100) < 25) {
			ContinueStatus;
			Debug.Log($"[Move failed] {This} is paralyzed");
			@lastMoveFailed = true;
			return false;
		}
		// Infatuation
		if (@effects.Attract >= 0) {
			@battle.CommonAnimation("Attract", self);
			@battle.Display(_INTL("{1} is in love with {2}!", This,
															@battle.battlers[@effects.Attract].ToString(true)));
			if (@battle.Random(100) < 50) {
				@battle.Display(_INTL("{1} is immobilized by love!", This));
				Debug.Log($"[Move failed] {This} is immobilized by love");
				@lastMoveFailed = true;
				return false;
			}
		}
		return true;
	}

	//-----------------------------------------------------------------------------

	// Initial success check against the target. Done once before the first hit.
	// Includes move-specific failure conditions, protections and type immunities.
	public void SuccessCheckAgainstTarget(move, user, target, targets) {
		show_message = move.ShowFailMessages(targets);
		typeMod = move.CalcTypeMod(move.calcType, user, target);
		target.damageState.typeMod = typeMod;
		// Two-turn attacks can't fail here in the charging turn
		if (user.effects.TwoTurnAttack) return true;
		// Semi-invulnerable target
		if (!SuccessCheckSemiInvulnerable(move, user, target)) {
			Debug.Log($"[Move failed] Target is semi-invulnerable");
			target.damageState.invulnerable = true;
			return true;   // Succeeds here but fails in def SuccessCheckPerHit
		}
		// Move-specific failures
		if (move.FailsAgainstTarget(user, target, show_message)) {
			Debug.Log(string.Format("[Move failed] In function code {0}'s def FailsAgainstTarget?", move.function_code));
			return false;
		}
		// Immunity to priority moves because of Psychic Terrain
		if (@battle.field.terrain == :Psychic && target.affectedByTerrain() && target.opposes(user) &&
			@battle.choices[user.index][4] > 0) {   // Move priority saved from CalculatePriority
			if (show_message) @battle.Display(_INTL("{1} surrounds itself with psychic terrain!", target.ToString()));
			return false;
		}
		// Crafty Shield
		if (target.OwnSide.effects.CraftyShield && user.index != target.index &&
			move.statusMove() && !move.Target(user).targets_all) {
			if (show_message) {
				@battle.CommonAnimation("CraftyShield", target);
				@battle.Display(_INTL("Crafty Shield protected {1}!", target.ToString(true)));
			}
			target.damageState.protected = true;
			@battle.successStates[user.index].protected = true;
			return false;
		}
		if (!(user.hasActiveAbility(Abilitys.UNSEENFIST) && move.ContactMove(user))) {
			// Wide Guard
			if (target.OwnSide.effects.WideGuard && user.index != target.index &&
				move.Target(user).num_targets > 1 &&
				(Settings.MECHANICS_GENERATION >= 7 || move.damagingMove())) {
				if (show_message) {
					@battle.CommonAnimation("WideGuard", target);
					@battle.Display(_INTL("Wide Guard protected {1}!", target.ToString(true)));
				}
				target.damageState.protected = true;
				@battle.successStates[user.index].protected = true;
				return false;
			}
			if (move.canProtectAgainst()) {
				// Quick Guard
				if (target.OwnSide.effects.QuickGuard &&
					@battle.choices[user.index][4] > 0) {   // Move priority saved from CalculatePriority
					if (show_message) {
						@battle.CommonAnimation("QuickGuard", target);
						@battle.Display(_INTL("Quick Guard protected {1}!", target.ToString(true)));
					}
					target.damageState.protected = true;
					@battle.successStates[user.index].protected = true;
					return false;
				}
				// Protect
				if (target.effects.Protect) {
					if (show_message) {
						@battle.CommonAnimation("Protect", target);
						@battle.Display(_INTL("{1} protected itself!", target.ToString()));
					}
					target.damageState.protected = true;
					@battle.successStates[user.index].protected = true;
					return false;
				}
				// Mat Block
				if (target.OwnSide.effects.MatBlock && move.damagingMove()) {
					// NOTE: Confirmed no common animation for this effect.
					if (show_message) @battle.Display(_INTL("{1} was blocked by the kicked-up mat!", move.name));
					target.damageState.protected = true;
					@battle.successStates[user.index].protected = true;
					return false;
				}
				// King's Shield
				if (target.effects.KingsShield && move.damagingMove()) {
					if (show_message) {
						@battle.CommonAnimation("KingsShield", target);
						@battle.Display(_INTL("{1} protected itself!", target.ToString()));
					}
					target.damageState.protected = true;
					@battle.successStates[user.index].protected = true;
					if (move.ContactMove(user) && user.affectedByContactEffect() &&
						user.CanLowerStatStage(:ATTACK, target)) {
						user.LowerStatStage(:ATTACK, (Settings.MECHANICS_GENERATION >= 8) ? 1 : 2, target);
					}
					return false;
				}
				// Obstruct
				if (target.effects.Obstruct && move.damagingMove()) {
					if (show_message) {
						@battle.CommonAnimation("Obstruct", target);
						@battle.Display(_INTL("{1} protected itself!", target.ToString()));
					}
					target.damageState.protected = true;
					@battle.successStates[user.index].protected = true;
					if (move.ContactMove(user) && user.affectedByContactEffect() &&
						user.CanLowerStatStage(:DEFENSE, target)) {
						user.LowerStatStage(:DEFENSE, 2, target);
					}
					return false;
				}
				// Silk Trap
				if (target.effects.SilkTrap && move.damagingMove()) {
					if (show_message) {
						@battle.CommonAnimation("SilkTrap", target);
						@battle.Display(_INTL("{1} protected itself!", target.ToString()));
					}
					target.damageState.protected = true;
					@battle.successStates[user.index].protected = true;
					if (move.ContactMove(user) && user.affectedByContactEffect() &&
						user.CanLowerStatStage(:SPEED, target)) {
						user.LowerStatStage(:SPEED, 1, target);
					}
					return false;
				}
				// Spiky Shield
				if (target.effects.SpikyShield) {
					if (show_message) {
						@battle.CommonAnimation("SpikyShield", target);
						@battle.Display(_INTL("{1} protected itself!", target.ToString()));
					}
					target.damageState.protected = true;
					@battle.successStates[user.index].protected = true;
					if (move.ContactMove(user) && user.affectedByContactEffect() && user.takesIndirectDamage()) {
						@battle.scene.DamageAnimation(user);
						user.ReduceHP(user.totalhp / 8, false);
						@battle.Display(_INTL("{1} was hurt!", user.ToString()));
						user.ItemHPHealCheck;
					}
					return false;
				}
				// Baneful Bunker
				if (target.effects.BanefulBunker) {
					if (show_message) {
						@battle.CommonAnimation("BanefulBunker", target);
						@battle.Display(_INTL("{1} protected itself!", target.ToString()));
					}
					target.damageState.protected = true;
					@battle.successStates[user.index].protected = true;
					if (move.ContactMove(user) && user.affectedByContactEffect() &&
						user.CanPoison(target, false)) {
						user.Poison(target);
					}
					return false;
				}
				// Burning Bulwark
				if (target.effects.BurningBulwark) {
					if (show_message) {
						@battle.CommonAnimation("BurningBulwark", target);
						@battle.Display(_INTL("{1} protected itself!", target.ToString()));
					}
					target.damageState.protected = true;
					@battle.successStates[user.index].protected = true;
					if (move.ContactMove(user) && user.affectedByContactEffect() &&
						user.CanBurn(target, false)) {
						user.Burn(target);
					}
					return false;
				}
			}
		}
		// Magic Coat/Magic Bounce
		if (move.statusMove() && move.canMagicCoat() && !target.semiInvulnerable() && target.opposes(user)) {
			if (target.effects.MagicCoat) {
				target.damageState.magicCoat = true;
				target.effects.MagicCoat = false;
				return false;
			}
			if (target.hasActiveAbility(Abilitys.MAGICBOUNCE) && !target.beingMoldBroken() &&
				!target.effects.MagicBounce) {
				target.damageState.magicBounce = true;
				target.effects.MagicBounce = true;
				return false;
			}
		}
		// Immunity because of ability (intentionally before type immunity check)
		if (move.ImmunityByAbility(user, target, show_message)) return false;
		// Type immunity
		if (move.DamagingMove() && Effectiveness.ineffective(typeMod)) {
			Debug.Log($"[Target immune] {target.ToString()}'s type immunity");
			if (show_message) @battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
			return false;
		}
		// Dark-type immunity to moves made faster by Prankster
		if (Settings.MECHANICS_GENERATION >= 7 && user.effects.Prankster &&
			target.Type == Types.DARK && target.opposes(user)) {
			Debug.Log($"[Target immune] {target.ToString()} is Dark-type and immune to Prankster-boosted moves");
			if (show_message) @battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
			return false;
		}
		// Airborne-based immunity to Ground moves
		if (move.damagingMove() && move.calcType == Types.GROUND &&
			target.airborne() && !move.hitsFlyingTargets()) {
			if (target.hasActiveAbility(Abilitys.LEVITATE) && !target.beingMoldBroken()) {
				if (show_message) {
					@battle.ShowAbilitySplash(target);
					if (Battle.Scene.USE_ABILITY_SPLASH) {
						@battle.Display(_INTL("{1} avoided the attack!", target.ToString()));
					} else {
						@battle.Display(_INTL("{1} avoided the attack with {2}!", target.ToString(), target.abilityName));
					}
					@battle.HideAbilitySplash(target);
				}
				return false;
			}
			if (target.hasActiveItem(Items.AIRBALLOON)) {
				if (show_message) @battle.Display(_INTL("{1}'s {2} makes Ground moves miss!", target.ToString(), target.itemName));
				return false;
			}
			if (target.effects.MagnetRise > 0) {
				if (show_message) @battle.Display(_INTL("{1} makes Ground moves miss with Magnet Rise!", target.ToString()));
				return false;
			}
			if (target.effects.Telekinesis > 0) {
				if (show_message) @battle.Display(_INTL("{1} makes Ground moves miss with Telekinesis!", target.ToString()));
				return false;
			}
		}
		// Immunity to powder-based moves
		if (move.powderMove()) {
			if (target.Type == Types.GRASS && Settings.MORE_TYPE_EFFECTS) {
				Debug.Log($"[Target immune] {target.ToString()} is Grass-type and immune to powder-based moves");
				if (show_message) @battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
				return false;
			}
			if (Settings.MECHANICS_GENERATION >= 6) {
				if (target.hasActiveAbility(Abilitys.OVERCOAT) && !target.beingMoldBroken()) {
					if (show_message) {
						@battle.ShowAbilitySplash(target);
						if (Battle.Scene.USE_ABILITY_SPLASH) {
							@battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
						} else {
							@battle.Display(_INTL("It doesn't affect {1} because of its {2}.", target.ToString(true), target.abilityName));
						}
						@battle.HideAbilitySplash(target);
					}
					return false;
				}
				if (target.hasActiveItem(Items.SAFETYGOGGLES)) {
					Debug.Log($"[Item triggered] {target.ToString()} has Safety Goggles and is immune to powder-based moves");
					if (show_message) @battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
					return false;
				}
			}
		}
		// Substitute
		if (target.effects.Substitute > 0 && move.statusMove() &&
			!move.ignoresSubstitute(user) && user.index != target.index) {
			Debug.Log($"[Target immune] {target.ToString()} is protected by its Substitute");
			if (show_message) @battle.Display(_INTL("{1} avoided the attack!", target.ToString(true)));
			return false;
		}
		return true;
	}

	// Returns true if the target is not semi-invulnerable, or if the user can hit
	// the target even though the target is semi-invulnerable.
	public void SuccessCheckSemiInvulnerable(move, user, target) {
		// Lock-On
		if (user.effects.LockOn > 0 &&
									user.effects.LockOnPos == target.index) return true;
		// Toxic
		if (move.OverrideSuccessCheckPerHit(user, target)) return true;
		// No Guard
		if (user.hasActiveAbility(Abilitys.NOGUARD) ||
									target.hasActiveAbility(Abilitys.NOGUARD)) return true;
		// Future Sight
		if (@battle.futureSight) return true;
		// Helping Hand
		if (move.function_code == "PowerUpAllyMove") return true;
		// Semi-invulnerable moves
		if (target.effects.TwoTurnAttack) {
			if ((target.inTwoTurnAttack("TwoTurnAttackInvulnerableInSky",
																"TwoTurnAttackInvulnerableInSkyParalyzeTarget",
																"TwoTurnAttackInvulnerableInSkyTargetCannotAct")) {
				return move.hitsFlyingTargets();
			} else if (target.inTwoTurnAttack("TwoTurnAttackInvulnerableUnderground")) {
				return move.hitsDiggingTargets();
			} else if (target.inTwoTurnAttack("TwoTurnAttackInvulnerableUnderwater")) {
				return move.hitsDivingTargets();
			} else if (target.inTwoTurnAttack("TwoTurnAttackInvulnerableRemoveProtections")) {
				return false;
			}
		}
		if (target.effects.SkyDrop >= 0 &&
			target.effects.SkyDrop != user.index && !move.hitsFlyingTargets()) {
			return false;
		}
		return true;
	}

	//-----------------------------------------------------------------------------

	// Per-hit success check against the target.
	// Includes semi-invulnerable move use and accuracy calculation.
	public void SuccessCheckPerHit(move, user, target, skipAccuracyCheck) {
		// Two-turn attacks can't fail here in the charging turn
		if (user.effects.TwoTurnAttack) return true;
		// Lock-On
		if (user.effects.LockOn > 0 &&
									user.effects.LockOnPos == target.index) return true;
		// Toxic
		if (move.OverrideSuccessCheckPerHit(user, target)) return true;
		// Semi-invulnerable target
		if (target.damageState.invulnerable) return false;
		// Called by another move
		if (skipAccuracyCheck) return true;
		// Accuracy check
		if (move.AccuracyCheck(user, target)) return true;   // Includes Counter/Mirror Coat
		Debug.Log($"[Move failed] Failed AccuracyCheck");
		return false;
	}

	//-----------------------------------------------------------------------------

	// Message shown when a move fails the per-hit success check above.
	public void MissMessage(move, user, target) {
		if (target.damageState.affection_missed) {
			@battle.Display(_INTL("{1} avoided the move in time with your shout!", target.ToString()));
		} else if (move.Target(user).num_targets > 1 || target.effects.TwoTurnAttack) {
			@battle.Display(_INTL("{1} avoided the attack!", target.ToString()));
		} else if (!move.MissMessage(user, target)) {
			@battle.Display(_INTL("{1}'s attack missed!", user.ToString()));
		}
	}
}
