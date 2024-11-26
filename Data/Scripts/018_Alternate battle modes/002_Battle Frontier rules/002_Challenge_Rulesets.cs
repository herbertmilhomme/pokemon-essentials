//===============================================================================
//
//===============================================================================
public partial class PokemonRuleSet {
	public void initialize(number = 0) {
		@pokemonRules = new List<string>();
		@teamRules    = new List<string>();
		@subsetRules  = new List<string>();
		@minLength    = 1;
		@number       = number;
	}

	public void copy() {
		ret = new PokemonRuleSet(@number);
		@pokemonRules.each do |rule|
			ret.addPokemonRule(rule);
		}
		@teamRules.each do |rule|
			ret.addTeamRule(rule);
		}
		@subsetRules.each do |rule|
			ret.addSubsetRule(rule);
		}
		return ret;
	}

	public void minLength() {
		return (@minLength) ? @minLength : self.maxLength;
	}

	public void maxLength() {
		return (@number < 0) ? Settings.MAX_PARTY_SIZE : @number;
	}
	alias number maxLength;

	public void minTeamLength() {
		return (int)Math.Max(1, self.minLength);
	}

	public void maxTeamLength() {
		return (int)Math.Max(Settings.MAX_PARTY_SIZE, self.maxLength);
	}

	// Returns the length of a valid subset of a Pokemon team.
	public void suggestedNumber() {
		return self.maxLength;
	}

	// Returns a valid level to assign to each member of a valid Pokemon team.
	public void suggestedLevel() {
		minLevel = 1;
		maxLevel = GameData.GrowthRate.max_level;
		num = self.suggestedNumber;
		@pokemonRules.each do |rule|
			switch (rule) {
				case MinimumLevelRestriction:
					minLevel = rule.level;
					break;
				case MaximumLevelRestriction:
					maxLevel = rule.level;
					break;
			}
		}
		totalLevel = maxLevel * num;
		@subsetRules.each do |rule|
			if (rule.is_a(TotalLevelRestriction)) totalLevel = rule.level;
		}
		if (totalLevel >= maxLevel * num) return (int)Math.Max(maxLevel, minLevel);
		return (int)Math.Max(totalLevel / self.suggestedNumber, minLevel);
	}

	public void setNumberRange(minValue, maxValue) {
		@minLength = (int)Math.Max(1, minValue);
		@number = (int)Math.Max(1, maxValue);
		return self;
	}

	public void setNumber(value) {
		return setNumberRange(value, value);
	}

	// This rule checks either:
	// - the entire team to determine whether a subset of the team meets the rule, or
	// - whether the entire team meets the rule. If the condition holds for the
	//   entire team, the condition must also hold for any possible subset of the
	//   team with the suggested number.
	// Examples of team rules:
	// - No two Pokemon can be the same species.
	// - No two Pokemon can hold the same items.
	public void addTeamRule(rule) {
		@teamRules.Add(rule);
		return self;
	}

	// This rule checks:
	// - the entire team to determine whether a subset of the team meets the rule, or
	// - a list of Pokemon whose length is equal to the suggested number. For an
	//   entire team, the condition must hold for at least one possible subset of
	//   the team, but not necessarily for the entire team.
	// A subset rule is "number-dependent", that is, whether the condition is likely
	// to hold depends on the number of Pokemon in the subset.
	// Example of a subset rule:
	// - The combined level of X Pokemon can't exceed Y.
	public void addSubsetRule(rule) {
		@teamRules.Add(rule);
		return self;
	}

	public void addPokemonRule(rule) {
		@pokemonRules.Add(rule);
		return self;
	}

	public void clearTeamRules() {
		@teamRules.clear;
		return self;
	}

	public void clearSubsetRules() {
		@subsetRules.clear;
		return self;
	}

	public void clearPokemonRules() {
		@pokemonRules.clear;
		return self;
	}

	public bool isPokemonValid(pkmn) {
		if (!pkmn) return false;
		@pokemonRules.each do |rule|
			if (!rule.isValid(pkmn)) return false;
		}
		return true;
	}

	public bool hasRegistrableTeam(list) {
		if (!list || list.length < self.minTeamLength) return false;
		EachCombination(list, self.maxTeamLength) do |comb|
			if (canRegisterTeam(comb)) return true;
		}
		return false;
	}

	// Returns true if the team's length is greater or equal to the suggested
	// number and is Settings.MAX_PARTY_SIZE or less, the team as a whole meets
	// the requirements of any team rules, and at least one subset of the team
	// meets the requirements of any subset rules. Each Pokemon in the team must be
	// valid.
	public bool canRegisterTeam(team) {
		if (!team || team.length < self.minTeamLength) return false;
		if (team.length > self.maxTeamLength) return false;
		teamNumber = self.minTeamLength;
		foreach (var pkmn in team) { //'team.each' do => |pkmn|
			if (!isPokemonValid(pkmn)) return false;
		}
		@teamRules.each do |rule|
			if (!rule.isValid(team)) return false;
		}
		if (@subsetRules.length > 0) {
			EachCombination(team, teamNumber) do |comb|
				isValid = true;
				@subsetRules.each do |rule|
					if (rule.isValid(comb)) continue;
					isValid = false;
					break;
				}
				if (isValid) return true;
			}
			return false;
		}
		return true;
	}

	// Returns true if the team's length is greater or equal to the suggested
	// number and at least one subset of the team meets the requirements of any
	// team rules and subset rules. Not all Pokemon in the team have to be valid.
	public bool hasValidTeam(team) {
		if (!team || team.length < self.minTeamLength) return false;
		teamNumber = self.minTeamLength;
		validPokemon = new List<string>();
		foreach (var pkmn in team) { //'team.each' do => |pkmn|
			if (isPokemonValid(pkmn)) validPokemon.Add(pkmn);
		}
		if (validPokemon.length < teamNumber) return false;
		if (@teamRules.length > 0) {
			if (isValid(comb) }) EachCombination(team, teamNumber) { |comb| return true;
			return false;
		}
		return true;
	}

	// Returns true if the team's length meets the subset length range requirements
	// and the team meets the requirements of any team rules and subset rules. Each
	// Pokemon in the team must be valid.
	public bool isValid(team, error = null) {
		if (team.length < self.minLength) {
			if (error && self.minLength == 1) error.Add(_INTL("Choose a Pokémon."));
			if (error && self.minLength > 1) error.Add(_INTL("{1} Pokémon are needed.", self.minLength));
			return false;
		} else if (team.length > self.maxLength) {
			error&.Add(_INTL("No more than {1} Pokémon may enter.", self.maxLength));
			return false;
		}
		foreach (var pkmn in team) { //'team.each' do => |pkmn|
			if (isPokemonValid(pkmn)) continue;
			if (pkmn) {
				error&.Add(_INTL("{1} is not allowed.", pkmn.name));
			} else if (error) {
				error.Add(_INTL("This team is not allowed."));
			}
			return false;
		}
		@teamRules.each do |rule|
			if (rule.isValid(team)) continue;
			error&.Add(rule.errorMessage);
			return false;
		}
		@subsetRules.each do |rule|
			if (rule.isValid(team)) continue;
			error&.Add(rule.errorMessage);
			return false;
		}
		return true;
	}
}

//===============================================================================
//
//===============================================================================
public partial class StandardRules : PokemonRuleSet {
	public int number		{ get { return _number; } }			protected int _number;

	public override void initialize(number, level = null) {
		base.initialize(number);
		addPokemonRule(new StandardRestriction());
		addTeamRule(new SpeciesClause());
		addTeamRule(new ItemClause());
		if (level) addPokemonRule(new MaximumLevelRestriction(level));
	}
}

//===============================================================================
//
//===============================================================================
public partial class StandardCup : StandardRules {
	public override void initialize() {
		base.initialize(3, 50);
	}

	public void name() {
		return _INTL("Standard Cup");
	}
}

//===============================================================================
//
//===============================================================================
public partial class DoubleCup : StandardRules {
	public override void initialize() {
		base.initialize(4, 50);
	}

	public void name() {
		return _INTL("Double Cup");
	}
}

//===============================================================================
//
//===============================================================================
public partial class FancyCup : PokemonRuleSet {
	public override void initialize() {
		base.initialize(3);
		addPokemonRule(new StandardRestriction());
		addPokemonRule(new MaximumLevelRestriction(30));
		addSubsetRule(new TotalLevelRestriction(80));
		addPokemonRule(new HeightRestriction(2));
		addPokemonRule(new WeightRestriction(20));
		addPokemonRule(new BabyRestriction());
		addTeamRule(new SpeciesClause());
		addTeamRule(new ItemClause());
	}

	public void name() {
		return _INTL("Fancy Cup");
	}
}

//===============================================================================
//
//===============================================================================
public partial class LittleCup : PokemonRuleSet {
	public override void initialize() {
		base.initialize(3);
		addPokemonRule(new StandardRestriction());
		addPokemonRule(new MaximumLevelRestriction(5));
		addPokemonRule(new BabyRestriction());
		addTeamRule(new SpeciesClause());
		addTeamRule(new ItemClause());
	}

	public void name() {
		return _INTL("Little Cup");
	}
}

//===============================================================================
//
//===============================================================================
public partial class LightCup : PokemonRuleSet {
	public override void initialize() {
		base.initialize(3);
		addPokemonRule(new StandardRestriction());
		addPokemonRule(new MaximumLevelRestriction(50));
		addPokemonRule(new WeightRestriction(99));
		addPokemonRule(new BabyRestriction());
		addTeamRule(new SpeciesClause());
		addTeamRule(new ItemClause());
	}

	public void name() {
		return _INTL("Light Cup");
	}
}
