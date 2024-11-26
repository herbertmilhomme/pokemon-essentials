//===============================================================================
// Used when generating new trainers for battle challenges.
//===============================================================================
public partial class Battle.DebugSceneNoVisuals {
	public void initialize(log_messages = false) {
		@battle       = null;
		@lastCmd      = new {0, 0, 0, 0};
		@lastMove     = new {0, 0, 0, 0};
		@log_messages = log_messages;
	}

	// Called whenever the battle begins.
	public void StartBattle(battle) {
		@battle   = battle;
		@lastCmd  = new {0, 0, 0, 0};
		@lastMove = new {0, 0, 0, 0};
	}

	public void Blitz(keys) {
		return rand(30);
	}

	// Called whenever a new round begins.
	public void BeginCommandPhase() { }
	public void BeginAttackPhase() { }
	public void BeginEndOfRoundPhase() { }
	public void ShowOpponent(idxTrainer) {}
	public void DamageAnimation(battler, effectiveness = 0) {}
	public void CommonAnimation(animName, user = null, target = null) {}
	public void Animation(moveID, user, targets, hitNum = 0) {}
	public void HitAndHPLossAnimation(targets) {}
	public void ShowPartyLineup(side, fullAnim = false) {}
	public void ShowAbilitySplash(battler, delay = false, logTrigger = true) {}
	public void ReplaceAbilitySplash(battler) {}
	public void HideAbilitySplash(battler) {}
	public void EndBattle(result) {}
	public void WildBattleSuccess() { }
	public void TrainerBattleSuccess() { }
	public void BattleArenaJudgment(b1, b2, r1, r2) {}
	public void BattleArenaBattlers(b1, b2) {}

	public void Update(cw = null) {}
	public void Refresh() { }
	public void RefreshOne(idxBattler) {}

	public void DisplayMessage(msg, brief = false) {
		if (@log_messages) Debug.log_message(msg);
	}
	alias Display DisplayMessage;

	public void DisplayPausedMessage(msg) {
		if (@log_messages) Debug.log_message(msg);
	}

	public void DisplayConfirmMessage(msg) {
		if (@log_messages) Debug.log_message(msg);
		return true;
	}

	public void ShowCommands(msg, commands, defaultValue) {
		if (@log_messages) Debug.log_message(msg);
		return 0;
	}

	public void SendOutBattlers(sendOuts, startBattle = false) {}
	public void Recall(idxBattler) {}
	public void ItemMenu(idxBattler, firstAction) {return -1; }
	public void ResetCommandsIndex(idxBattler) {}

	public void HPChanged(battler, oldHP, showAnim = false) {}
	public void ChangePokemon(idxBattler, pkmn) {}
	public void FaintBattler(battler) {}
	public void EXPBar(battler, startExp, endExp, tempExp1, tempExp2) {}
	public void LevelUp(pkmn, battler, oldTotalHP, oldAttack, oldDefense, oldSpAtk, oldSpDef, oldSpeed) {}
	public void ForgetMove(pkmn, moveToLearn) {return 0; }   // Always forget first move

	public void CommandMenu(idxBattler, firstAction) {
		if (rand(15) == 0) return 1;   // Bag
		if (rand(10) == 0) return 4;   // Call
		return 0;                    // Fight
	}

	public void FightMenu(idxBattler, megaEvoPossible = false) {
		battler = @battle.battlers[idxBattler];
		50.times do;
			if (yield rand(battler.move.length)) break;
		}
	}

	public void ChooseTarget(idxBattler, target_data, visibleSprites = null) {
		targets = @battle.allOtherSideBattlers(idxBattler).map(b => b.index);
		if (targets.length == 0) return -1;
		return targets.sample;
	}

	public void PartyScreen(idxBattler, canCancel = false) {
		replacements = new List<string>();
		@battle.eachInTeamFromBattlerIndex(idxBattler) do |_b, idxParty|
			if (!@battle.FindBattler(idxParty, idxBattler)) replacements.Add(idxParty);
		}
		if (replacements.length == 0) return;
		50.times do;
			if (yield replacements[rand(replacements.length)], self) break;
		}
	}
}
