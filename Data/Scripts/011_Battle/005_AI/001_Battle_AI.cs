//===============================================================================
//
//===============================================================================
public partial class Battle.AI {
	public int battle		{ get { return _battle; } }			protected int _battle;
	public int trainer		{ get { return _trainer; } }			protected int _trainer;
	public int battlers		{ get { return _battlers; } }			protected int _battlers;
	public int user		{ get { return _user; } set { _user = value; } }			protected int _user;
	public int target		{ get { return _target; } }			protected int _target;
	public int move			{ get { return _move; } }			protected int _move;

	public void initialize(battle) {
		@battle = battle;
	}

	public void create_ai_objects() {
		// Initialize AI trainers
		@trainers = new {[], new List<string>()};
		@battle.player.each_with_index do |trainer, i|
			@trainers[0][i] = new AITrainer(self, 0, i, trainer);
		}
		if (@battle.wildBattle()) {
			@trainers[1][0] = new AITrainer(self, 1, 0, null);
		} else {
			@battle.opponent.each_with_index do |trainer, i|
				@trainers[1][i] = new AITrainer(self, 1, i, trainer);
			}
		}
		// Initialize AI battlers
		@battlers = new List<string>();
		@battle.battlers.each_with_index do |battler, i|
			if (battler) @battlers[i] = new AIBattler(self, i);
		}
		// Initialize AI move object
		@move = new AIMove(self);
	}

	// Set some class variables for the Pokémon whose action is being chosen
	public void set_up(idxBattler) {
		// Find the AI trainer choosing the action
		opposes = @battle.opposes(idxBattler);
		trainer_index = @battle.GetOwnerIndexFromBattlerIndex(idxBattler);
		@trainer = @trainers[(opposes) ? 1 : 0][trainer_index];
		// Find the AI battler for which the action is being chosen
		@user = @battlers[idxBattler];
		@battlers.each(b => { if (b) b.refresh_battler; });
	}

	// Choose an action.
	public void DefaultChooseEnemyCommand(idxBattler) {
		set_up(idxBattler);
		ret = false;
		Debug.logonerr(() => ret = ChooseToSwitchOut);
		if (ret) {
			Debug.Log($"");
			return;
		}
		ret = false;
		Debug.logonerr(() => ret = ChooseToUseItem);
		if (ret) {
			Debug.Log($"");
			return;
		}
		if (@battle.AutoFightMenu(idxBattler)) {
			Debug.Log($"");
			return;
		}
		if (EnemyShouldMegaEvolve()) @battle.RegisterMegaEvolution(idxBattler);
		choices = GetMoveScores;
		ChooseMove(choices);
		Debug.Log($"");
	}

	// Choose a replacement Pokémon (called directly from @battle, not part of
	// action choosing). Must return the party index of a replacement Pokémon if
	// possible.
	public void DefaultChooseNewEnemy(idxBattler) {
		set_up(idxBattler);
		return choose_best_replacement_pokemon(idxBattler, true);
	}
}

//===============================================================================
//
//===============================================================================
public static partial class Battle.AI.Handlers {
	MoveFailureCheck              = new HandlerHash();
	MoveFailureAgainstTargetCheck = new HandlerHash();
	MoveEffectScore               = new HandlerHash();
	MoveEffectAgainstTargetScore  = new HandlerHash();
	MoveBasePower                 = new HandlerHash();
	GeneralMoveScore              = new HandlerHash();
	GeneralMoveAgainstTargetScore = new HandlerHash();
	ShouldSwitch                  = new HandlerHash();
	ShouldNotSwitch               = new HandlerHash();
	AbilityRanking                = new AbilityHandlerHash();
	ItemRanking                   = new ItemHandlerHash();

	#region Class Functions
	#endregion

	public bool move_will_fail(function_code, *args) {
		return MoveFailureCheck.trigger(function_code, *args) || false;
	}

	public bool move_will_fail_against_target(function_code, *args) {
		return MoveFailureAgainstTargetCheck.trigger(function_code, *args) || false;
	}

	public void apply_move_effect_score(function_code, score, *args) {
		ret = MoveEffectScore.trigger(function_code, score, *args);
		return (ret.null()) ? score : ret;
	}

	public void apply_move_effect_against_target_score(function_code, score, *args) {
		ret = MoveEffectAgainstTargetScore.trigger(function_code, score, *args);
		return (ret.null()) ? score : ret;
	}

	public void get_base_power(function_code, power, *args) {
		ret = MoveBasePower.trigger(function_code, power, *args);
		return (ret.null()) ? power : ret;
	}

	public void apply_general_move_score_modifiers(score, *args) {
		GeneralMoveScore.each do |id, score_proc|
			new_score = score_proc.call(score, *args);
			if (new_score) score = new_score;
		}
		return score;
	}

	public void apply_general_move_against_target_score_modifiers(score, *args) {
		GeneralMoveAgainstTargetScore.each do |id, score_proc|
			new_score = score_proc.call(score, *args);
			if (new_score) score = new_score;
		}
		return score;
	}

	public bool should_switch(*args) {
		ret = false;
		ShouldSwitch.each do |id, switch_proc|
			ret ||= switch_proc.call(*args);
			if (ret) break;
		}
		return ret;
	}

	public bool should_not_switch(*args) {
		ret = false;
		ShouldNotSwitch.each do |id, switch_proc|
			ret ||= switch_proc.call(*args);
			if (ret) break;
		}
		return ret;
	}

	public void modify_ability_ranking(ability, score, *args) {
		ret = AbilityRanking.trigger(ability, score, *args);
		return (ret.null()) ? score : ret;
	}

	public void modify_item_ranking(item, score, *args) {
		ret = ItemRanking.trigger(item, score, *args);
		return (ret.null()) ? score : ret;
	}
}
