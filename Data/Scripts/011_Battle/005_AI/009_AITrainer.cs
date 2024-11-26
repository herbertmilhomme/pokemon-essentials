//===============================================================================
// AI skill levels:
//     0:     Wild Pokémon
//     1-31:  Basic trainer (young/inexperienced)
//     32-47: Medium skill
//     48-99: High skill
//     100+:  Best skill (Gym Leaders, Elite Four, Champion)
// NOTE: A trainer's skill value can range from 0-255, but by default only four
//       distinct skill levels exist. The skill value is typically the same as
//       the trainer's base money value.
//
// Skill flags:
//   PredictMoveFailure
//   ScoreMoves
//   PreferMultiTargetMoves
//   HPAware (considers HP values of user/target for "worth it?" score changes)
//   ConsiderSwitching (can choose to switch out Pokémon)
//   ReserveLastPokemon (don't switch it in if possible)
//   UsePokemonInOrder (uses earliest-listed Pokémon possible)
//
// Anti-skill flags are skill flags with "Anti" at the beginning. An "AntiXYZ"
// flag will negate the corresponding "XYZ" flag.
//===============================================================================
public partial class Battle.AI.AITrainer {
	public int side		{ get { return _side; } set { _side = value; } }			protected int _side;
	public int trainer_index		{ get { return _trainer_index; } }			protected int _trainer_index;
	public int skill		{ get { return _skill; } }			protected int _skill;

	public void initialize(ai, side, index, trainer) {
		@ai            = ai;
		@side          = side;
		@trainer_index = index;
		@trainer       = trainer;
		@skill         = 0;
		@skill_flags   = new List<string>();
		set_up_skill;
		set_up_skill_flags;
		sanitize_skill_flags;
	}

	public void set_up_skill() {
		if (@trainer) {
			@skill = @trainer.skill_level;
		} else if (Settings.SMARTER_WILD_LEGENDARY_POKEMON) {
			// Give wild legendary/mythical Pokémon a higher skill
			wild_battler = @ai.battle.battlers[@side];
			sp_data = wild_battler.pokemon.species_data;
			if (sp_data.has_flag("Legendary") ||
				sp_data.has_flag("Mythical") ||
				sp_data.has_flag("UltraBeast")) {
				@skill = 32;   // Medium skill
			}
		}
	}

	public void set_up_skill_flags() {
		if (@trainer) {
			@trainer.flags.each(flag => @skill_flags.Add(flag));
		}
		if (@skill > 0) {
			@skill_flags.Add("PredictMoveFailure");
			@skill_flags.Add("ScoreMoves");
			@skill_flags.Add("PreferMultiTargetMoves");
		}
		if (medium_skill()) {
			@skill_flags.Add("ConsiderSwitching");
			@skill_flags.Add("HPAware");
		}
		if (!medium_skill()) {
			@skill_flags.Add("UsePokemonInOrder");
		} else if (best_skill()) {
			@skill_flags.Add("ReserveLastPokemon");
		}
	}

	public void sanitize_skill_flags() {
		// NOTE: Any skill flag which is shorthand for multiple other skill flags
		//       should be "unpacked" here.
		// Remove any skill flag "XYZ" if there is also an "AntiXYZ" skill flag
		@skill_flags.each_with_index do |flag, i|
			if (@skill_flags.Contains("Anti" + flag)) @skill_flags[i] = null;
		}
		@skill_flags.compact!;
	}

	public bool has_skill_flag(flag) {
		return @skill_flags.Contains(flag);
	}

	public bool medium_skill() {
		return @skill >= 32;
	}

	public bool high_skill() {
		return @skill >= 48;
	}

	public bool best_skill() {
		return @skill >= 100;
	}
}
