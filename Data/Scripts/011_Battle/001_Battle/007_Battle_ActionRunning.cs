//===============================================================================
//
//===============================================================================
public partial class Battle {
	//-----------------------------------------------------------------------------
	// Running from battle
	//-----------------------------------------------------------------------------

	public bool CanRun(idxBattler) {
		if (trainerBattle()) return false;
		battler = @battlers[idxBattler];
		if (!@canRun && !battler.opposes()) return false;
		if (battler.Type == Types.GHOST && Settings.MORE_TYPE_EFFECTS) return true;
		if (battler.abilityActive() &&
									Battle.AbilityEffects.triggerCertainEscapeFromBattle(battler.ability, battler)) return true;
		if (battler.itemActive() &&
									Battle.ItemEffects.triggerCertainEscapeFromBattle(battler.item, battler)) return true;
		if (battler.trappedInBattle()) return false;
		foreach (var b in allOtherSideBattlers(idxBattler)) { //'allOtherSideBattlers(idxBattler).each' do => |b|
			if (b.abilityActive() &&
											Battle.AbilityEffects.triggerTrappingByTarget(b.ability, battler, b, self)) return false;
			if (b.itemActive() &&
											Battle.ItemEffects.triggerTrappingByTarget(b.item, battler, b, self)) return false;
		}
		return true;
	}

	// Return values:
	// -1: Chose not to end the battle via Debug means
	//  0: Couldn't end the battle via Debug means; carry on trying to run
	//  1: Ended the battle via Debug means
	public void DebugRun() {
		if (!Core.DEBUG || !Input.press(Input.CTRL)) return 0;
		commands = new {_INTL("Treat as a win"), _INTL("Treat as a loss"),
								_INTL("Treat as a draw"), _INTL("Treat as running away/forfeit")};
		if (wildBattle()) commands.Add(_INTL("Treat as a capture"));
		commands.Add(_INTL("Cancel"));
		switch (ShowCommands(_INTL("Choose the outcome of this battle."), commands)) {
			case 0:
				@decision = Outcome.WIN;
				break;
			case 1:
				@decision = Outcome.LOSE;
				break;
			case 2:
				@decision = Outcome.DRAW;
				break;
			case 3:
				SEPlay("Battle flee");
				DisplayPaused(_INTL("You got away safely!"));
				@decision = Outcome.FLEE;
				break;
			case 4:
				if (trainerBattle()) return -1;
				@decision = Outcome.CATCH;
				break;
			default:
				return -1;
		}
		return 1;
	}

	// Return values:
	// -1: Failed fleeing
	//  0: Wasn't possible to attempt fleeing, continue choosing action for the round
	//  1: Succeeded at fleeing, battle will end
	// duringBattle is true for replacing a fainted Pokémon during the End Of Round
	// phase, and false for choosing the Run command.
	public void Run(idxBattler, duringBattle = false) {
		battler = @battlers[idxBattler];
		if (battler.opposes()) {
			if (trainerBattle()) return 0;
			@choices[idxBattler].Action = :Run;
			@choices[idxBattler].Index = 0;
			@choices[idxBattler].Move = null;
			return -1;
		}
		// Debug ending the battle
		debug_ret = DebugRun;
		if (debug_ret != 0) return debug_ret;
		// Running from trainer battles
		if (trainerBattle()) {
			if (@internalBattle) {
				if (Settings.CAN_FORFEIT_TRAINER_BATTLES) {
					DisplayPaused(_INTL("Would you like to give up on this battle and quit now?"));
					if (DisplayConfirm(_INTL("Quitting the battle is the same as losing the battle."))) {
						@decision = Outcome.LOSE;   // Treated as a loss
						return 1;
					}
				} else {
					DisplayPaused(_INTL("No! There's no running from a Trainer battle!"));
				}
			} else if (DisplayConfirm(_INTL("Would you like to forfeit the match and quit now?"))) {
				SEPlay("Battle flee");
				Display(_INTL("{1} forfeited the match!", self.Player.name));
				@decision = Outcome.FLEE;
				return 1;
			}
			return 0;
		}
		if (!@canRun) {
			DisplayPaused(_INTL("You can't escape!"));
			return 0;
		}
		if (!duringBattle) {
			if (battler.Type == Types.GHOST && Settings.MORE_TYPE_EFFECTS) {
				SEPlay("Battle flee");
				DisplayPaused(_INTL("You got away safely!"));
				@decision = Outcome.FLEE;
				return 1;
			}
			// Abilities that guarantee escape
			if (battler.abilityActive() &&
				Battle.AbilityEffects.triggerCertainEscapeFromBattle(battler.ability, battler)) {
				ShowAbilitySplash(battler, true);
				HideAbilitySplash(battler);
				SEPlay("Battle flee");
				DisplayPaused(_INTL("You got away safely!"));
				@decision = Outcome.FLEE;
				return 1;
			}
			// Held items that guarantee escape
			if (battler.itemActive() &&
				Battle.ItemEffects.triggerCertainEscapeFromBattle(battler.item, battler)) {
				SEPlay("Battle flee");
				DisplayPaused(_INTL("{1} fled using its {2}!", battler.ToString(), battler.itemName));
				@decision = Outcome.FLEE;
				return 1;
			}
			// Other certain trapping effects
			if (battler.trappedInBattle()) {
				DisplayPaused(_INTL("You can't escape!"));
				return 0;
			}
			// Trapping abilities/items
			foreach (var b in allOtherSideBattlers(idxBattler)) { //'allOtherSideBattlers(idxBattler).each' do => |b|
				if (!b.abilityActive()) continue;
				if (Battle.AbilityEffects.triggerTrappingByTarget(b.ability, battler, b, self)) {
					DisplayPaused(_INTL("{1} prevents escape with {2}!", b.ToString(), b.abilityName));
					return 0;
				}
			}
			foreach (var b in allOtherSideBattlers(idxBattler)) { //'allOtherSideBattlers(idxBattler).each' do => |b|
				if (!b.itemActive()) continue;
				if (Battle.ItemEffects.triggerTrappingByTarget(b.item, battler, b, self)) {
					DisplayPaused(_INTL("{1} prevents escape with {2}!", b.ToString(), b.itemName));
					return 0;
				}
			}
		}
		// Fleeing calculation
		// Get the speeds of the Pokémon fleeing and the fastest opponent
		// NOTE: Not Speed, because using unmodified Speed.
		if (!duringBattle) @runCommand += 1;   // Make it easier to flee next time
		speedPlayer = @battlers[idxBattler].speed;
		speedEnemy = 1;
		foreach (var b in allOtherSideBattlers(idxBattler)) { //'allOtherSideBattlers(idxBattler).each' do => |b|
			speed = b.speed;
			if (speedEnemy < speed) speedEnemy = speed;
		}
		// Compare speeds and perform fleeing calculation
		if (speedPlayer > speedEnemy) {
			rate = 256;
		} else {
			rate = (speedPlayer * 128) / speedEnemy;
			rate += @runCommand * 30;
		}
		if (rate >= 256 || @battleAI.AIRandom(256) < rate) {
			SEPlay("Battle flee");
			DisplayPaused(_INTL("You got away safely!"));
			@decision = Outcome.FLEE;
			return 1;
		}
		DisplayPaused(_INTL("You couldn't get away!"));
		return -1;
	}
}
