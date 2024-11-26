//===============================================================================
//
//===============================================================================
public partial class PokemonChallengeRules {
	public int ruleset		{ get { return _ruleset; } }			protected int _ruleset;
	public int battletype		{ get { return _battletype; } }			protected int _battletype;
	public int levelAdjustment		{ get { return _levelAdjustment; } }			protected int _levelAdjustment;

	public void initialize(ruleset = null) {
		@ruleset         = (ruleset) ? ruleset : new PokemonRuleSet();
		@battletype      = new BattleTower();
		@levelAdjustment = null;
		@battlerules     = new List<string>();
	}

	public void copy() {
		ret = new PokemonChallengeRules(@ruleset.copy);
		ret.setBattleType(@battletype);
		ret.setLevelAdjustment(@levelAdjustment);
		@battlerules.each do |rule|
			ret.addBattleRule(rule);
		}
		return ret;
	}

	public void setRuleset(rule) {
		@ruleset = rule;
		return self;
	}

	public void setBattleType(rule) {
		@battletype = rule;
		return self;
	}

	public void setLevelAdjustment(rule) {
		@levelAdjustment = rule;
		return self;
	}

	public void number() {
		return self.ruleset.number;
	}

	public void setNumber(number) {
		self.ruleset.setNumber(number);
		return self;
	}

	public void setDoubleBattle(value) {
		if (value) {
			self.ruleset.setNumber(4);
			self.addBattleRule(new DoubleBattle());
		} else {
			self.ruleset.setNumber(3);
			self.addBattleRule(new SingleBattle());
		}
		return self;
	}

	public void adjustLevels(party1, party2) {
		if (@levelAdjustment) return @levelAdjustment.adjustLevels(party1, party2);
		return null;
	}

	public void unadjustLevels(party1, party2, adjusts) {
		if (@levelAdjustment && adjusts) @levelAdjustment.unadjustLevels(party1, party2, adjusts);
	}

	public void adjustLevelsBilateral(party1, party2) {
		if (@levelAdjustment && @levelAdjustment.type == LevelAdjustment.BOTH_TEAMS) {
			return @levelAdjustment.adjustLevels(party1, party2);
		}
		return null;
	}

	public void unadjustLevelsBilateral(party1, party2, adjusts) {
		if (@levelAdjustment && adjusts && @levelAdjustment.type == LevelAdjustment.BOTH_TEAMS) {
			@levelAdjustment.unadjustLevels(party1, party2, adjusts);
		}
	}

	public void addPokemonRule(rule) {
		self.ruleset.addPokemonRule(rule);
		return self;
	}

	public void addLevelRule(minLevel, maxLevel, totalLevel) {
		self.addPokemonRule(new MinimumLevelRestriction(minLevel));
		self.addPokemonRule(new MaximumLevelRestriction(maxLevel));
		self.addSubsetRule(new TotalLevelRestriction(totalLevel));
		self.setLevelAdjustment(new TotalLevelAdjustment(minLevel, maxLevel, totalLevel));
		return self;
	}

	public void addSubsetRule(rule) {
		self.ruleset.addSubsetRule(rule);
		return self;
	}

	public void addTeamRule(rule) {
		self.ruleset.addTeamRule(rule);
		return self;
	}

	public void addBattleRule(rule) {
		@battlerules.Add(rule);
		return self;
	}

	public void createBattle(scene, trainer1, trainer2) {
		battle = @battletype.CreateBattle(scene, trainer1, trainer2);
		@battlerules.each do |p|
			p.setRule(battle);
		}
		return battle;
	}
}

//===============================================================================
// Stadium Cups rules.
//===============================================================================
public void PikaCupRules(double) {
	ret = new PokemonChallengeRules();
	ret.addPokemonRule(new StandardRestriction());
	ret.addLevelRule(15, 20, 50);
	ret.addTeamRule(new SpeciesClause());
	ret.addTeamRule(new ItemClause());
	ret.addBattleRule(new SleepClause());
	ret.addBattleRule(new FreezeClause());
	ret.addBattleRule(new SelfKOClause());
	ret.setDoubleBattle(double);
	ret.setNumber(3);
	return ret;
}

public void PokeCupRules(double) {
	ret = new PokemonChallengeRules();
	ret.addPokemonRule(new StandardRestriction());
	ret.addLevelRule(50, 55, 155);
	ret.addTeamRule(new SpeciesClause());
	ret.addTeamRule(new ItemClause());
	ret.addBattleRule(new SleepClause());
	ret.addBattleRule(new FreezeClause());
	ret.addBattleRule(new SelfdestructClause());
	ret.setDoubleBattle(double);
	ret.setNumber(3);
	return ret;
}

public void PrimeCupRules(double) {
	ret = new PokemonChallengeRules();
	ret.setLevelAdjustment(new OpenLevelAdjustment(GameData.GrowthRate.max_level));
	ret.addTeamRule(new SpeciesClause());
	ret.addTeamRule(new ItemClause());
	ret.addBattleRule(new SleepClause());
	ret.addBattleRule(new FreezeClause());
	ret.addBattleRule(new SelfdestructClause());
	ret.setDoubleBattle(double);
	return ret;
}

public void FancyCupRules(double) {
	ret = new PokemonChallengeRules();
	ret.addPokemonRule(new StandardRestriction());
	ret.addLevelRule(25, 30, 80);
	ret.addPokemonRule(new HeightRestriction(2));
	ret.addPokemonRule(new WeightRestriction(20));
	ret.addPokemonRule(new BabyRestriction());
	ret.addTeamRule(new SpeciesClause());
	ret.addTeamRule(new ItemClause());
	ret.addBattleRule(new SleepClause());
	ret.addBattleRule(new FreezeClause());
	ret.addBattleRule(new PerishSongClause());
	ret.addBattleRule(new SelfdestructClause());
	ret.setDoubleBattle(double);
	ret.setNumber(3);
	return ret;
}

public void LittleCupRules(double) {
	ret = new PokemonChallengeRules();
	ret.addPokemonRule(new StandardRestriction());
	ret.addPokemonRule(new UnevolvedFormRestriction());
	ret.setLevelAdjustment(new EnemyLevelAdjustment(5));
	ret.addPokemonRule(new MaximumLevelRestriction(5));
	ret.addTeamRule(new SpeciesClause());
	ret.addTeamRule(new ItemClause());
	ret.addBattleRule(new SleepClause());
	ret.addBattleRule(new FreezeClause());
	ret.addBattleRule(new SelfdestructClause());
	ret.addBattleRule(new PerishSongClause());
	ret.addBattleRule(new SonicBoomClause());
	ret.setDoubleBattle(double);
	return ret;
}

public void StrictLittleCupRules(double) {
	ret = new PokemonChallengeRules();
	ret.addPokemonRule(new StandardRestriction());
	ret.addPokemonRule(new UnevolvedFormRestriction());
	ret.setLevelAdjustment(new EnemyLevelAdjustment(5));
	ret.addPokemonRule(new MaximumLevelRestriction(5));
	ret.addPokemonRule(new LittleCupRestriction());
	ret.addTeamRule(new SpeciesClause());
	ret.addBattleRule(new SleepClause());
	ret.addBattleRule(new EvasionClause());
	ret.addBattleRule(new OHKOClause());
	ret.addBattleRule(new SelfKOClause());
	ret.setDoubleBattle(double);
	ret.setNumber(3);
	return ret;
}

//===============================================================================
// Battle Frontier rules.
//===============================================================================
public void BattleTowerRules(double, openlevel) {
	ret = new PokemonChallengeRules();
	if (openlevel) {
		ret.setLevelAdjustment(new OpenLevelAdjustment(60));
	} else {
		ret.setLevelAdjustment(new CappedLevelAdjustment(50));
	}
	ret.addPokemonRule(new StandardRestriction());
	ret.addTeamRule(new SpeciesClause());
	ret.addTeamRule(new ItemClause());
	ret.addBattleRule(new SoulDewBattleClause());
	ret.setDoubleBattle(double);
	return ret;
}

public void BattlePalaceRules(double, openlevel) {
	return BattleTowerRules(double, openlevel).setBattleType(new BattlePalace());
}

public void BattleArenaRules(openlevel) {
	return BattleTowerRules(false, openlevel).setBattleType(new BattleArena());
}

public void BattleFactoryRules(double, openlevel) {
	ret = new PokemonChallengeRules();
	if (openlevel) {
		ret.setLevelAdjustment(new FixedLevelAdjustment(100));
		ret.addPokemonRule(new MaximumLevelRestriction(100));
	} else {
		ret.setLevelAdjustment(new FixedLevelAdjustment(50));
		ret.addPokemonRule(new MaximumLevelRestriction(50));
	}
	ret.addTeamRule(new SpeciesClause());
	ret.addPokemonRule(new BannedSpeciesRestriction(:UNOWN));
	ret.addTeamRule(new ItemClause());
	ret.addBattleRule(new SoulDewBattleClause());
	ret.setDoubleBattle(double);
	return ret;
}

//===============================================================================
// Other Interesting Rulesets.
//===============================================================================
=begin;
// Official Species Restriction
.addPokemonRule(new BannedSpeciesRestriction(
	:MEWTWO, :MEW,
	:LUGIA, :HOOH, :CELEBI,
	:KYOGRE, :GROUDON, :RAYQUAZA, :JIRACHI, :DEOXYS,
	:DIALGA, :PALKIA, :GIRATINA, :MANAPHY, :PHIONE,
	:DARKRAI, :SHAYMIN, :ARCEUS));
.addBattleRule(new SoulDewBattleClause());

// New Official Species Restriction
.addPokemonRule(new BannedSpeciesRestriction(
	:MEW,
	:CELEBI,
	:JIRACHI, :DEOXYS,
	:MANAPHY, :PHIONE, :DARKRAI, :SHAYMIN, :ARCEUS));
.addBattleRule(new SoulDewBattleClause());

// Pocket Monsters Stadium
new PokemonChallengeRules();
.addPokemonRule(new SpeciesRestriction(
	:VENUSAUR, :CHARIZARD, :BLASTOISE, :BEEDRILL, :FEAROW,
	:PIKACHU, :NIDOQUEEN, :NIDOKING, :DUGTRIO, :PRIMEAPE,
	:ARCANINE, :ALAKAZAM, :MACHAMP, :GOLEM, :MAGNETON,
	:CLOYSTER, :GENGAR, :ONIX, :HYPNO, :ELECTRODE,
	:EXEGGUTOR, :CHANSEY, :KANGASKHAN, :STARMIE, :SCYTHER,
	:JYNX, :PINSIR, :TAUROS, :GYARADOS, :LAPRAS,
	:DITTO, :VAPOREON, :JOLTEON, :FLAREON, :AERODACTYL,
	:SNORLAX, :ARTICUNO, :ZAPDOS, :MOLTRES, :DRAGONITE
));

// 1999 Tournament Rules
new PokemonChallengeRules();
.addTeamRule(new SpeciesClause());
.addPokemonRule(new ItemsDisallowedClause());
.addBattleRule(new SleepClause());
.addBattleRule(new FreezeClause());
.addBattleRule(new SelfdestructClause());
.setDoubleBattle(false);
.setLevelRule(1, 50, 150);
.addPokemonRule(new BannedSpeciesRestriction(
	:VENUSAUR, :DUGTRIO, :ALAKAZAM, :GOLEM, :MAGNETON,
	:GENGAR, :HYPNO, :ELECTRODE, :EXEGGUTOR, :CHANSEY,
	:KANGASKHAN, :STARMIE, :JYNX, :TAUROS, :GYARADOS,
	:LAPRAS, :DITTO, :VAPOREON, :JOLTEON, :SNORLAX,
	:ARTICUNO, :ZAPDOS, :DRAGONITE, :MEWTWO, :MEW));

// 2005 Tournament Rules
new PokemonChallengeRules();
.addPokemonRule(new BannedSpeciesRestriction(
	:DRAGONITE, :MEW, :MEWTWO,
	:TYRANITAR, :LUGIA, :CELEBI, :HOOH,
	:GROUDON, :KYOGRE, :RAYQUAZA, :JIRACHI, :DEOXYS));
.setDoubleBattle(true);
.addLevelRule(1, 50, 200);
.addTeamRule(new ItemClause());
.addPokemonRule(new BannedItemRestriction(:SOULDEW, :ENIGMABERRY));
.addBattleRule(new SleepClause());
.addBattleRule(new FreezeClause());
.addBattleRule(new SelfdestructClause());
.addBattleRule(new PerishSongClause());

// 2008 Tournament Rules
new PokemonChallengeRules();
.addPokemonRule(new BannedSpeciesRestriction(
	:MEWTWO, :MEW,
	:TYRANITAR, :LUGIA, :HOOH, :CELEBI,
	:GROUDON, :KYOGRE, :RAYQUAZA, :JIRACHI, :DEOXYS,
	:PALKIA, :DIALGA, :PHIONE, :MANAPHY, :ROTOM, :SHAYMIN, :DARKRAI));
.setDoubleBattle(true);
.addLevelRule(1, 50, 200);
.addTeamRule(new NicknameClause());
.addTeamRule(new ItemClause());
.addBattleRule(new SoulDewBattleClause());

// 2010 Tournament Rules
new PokemonChallengeRules();
.addPokemonRule(new BannedSpeciesRestriction(
	:MEW,
	:CELEBI,
	:JIRACHI, :DEOXYS,
	:PHIONE, :MANAPHY, :SHAYMIN, :DARKRAI, :ARCEUS));
.addSubsetRule(new RestrictedSpeciesSubsetRestriction(
	:MEWTWO,
	:LUGIA, :HOOH,
	:GROUDON, :KYOGRE, :RAYQUAZA,
	:PALKIA, :DIALGA, :GIRATINA));
.setDoubleBattle(true);
.addLevelRule(1, 100, 600);
.setLevelAdjustment(new CappedLevelAdjustment(50));
.addTeamRule(new NicknameClause());
.addTeamRule(new ItemClause());
.addPokemonRule(new SoulDewClause());

// Pokemon Colosseum -- Anything Goes
new PokemonChallengeRules();
.addLevelRule(1, 100, 600);
.addBattleRule(new SleepClause());
.addBattleRule(new FreezeClause());
.addBattleRule(new SelfdestructClause());
.addBattleRule(new PerishSongClause());

// Pokemon Colosseum -- Max Lv. 50
new PokemonChallengeRules();
.addLevelRule(1, 50, 300);
.addTeamRule(new SpeciesClause());
.addTeamRule(new ItemClause());
.addBattleRule(new SleepClause());
.addBattleRule(new FreezeClause());
.addBattleRule(new SelfdestructClause());
.addBattleRule(new PerishSongClause());

// Pokemon Colosseum -- Max Lv. 100
new PokemonChallengeRules();
.addLevelRule(1, 100, 600);
.addTeamRule(new SpeciesClause());
.addTeamRule(new ItemClause());
.addBattleRule(new SleepClause());
.addBattleRule(new FreezeClause());
.addBattleRule(new SelfdestructClause());
.addBattleRule(new PerishSongClause());
=end;
