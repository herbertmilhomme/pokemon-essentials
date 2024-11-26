//===============================================================================
//
//===============================================================================
public partial class LevelAdjustment {
	public const int BOTH_TEAMS           = 0;
	public const int ENEMY_TEAM           = 1;
	public const int MY_TEAM              = 2;
	public const int BOTH_TEAMS_DIFFERENT = 3;

	public void initialize(adjustment) {
		@adjustment = adjustment;
	}

	public void type() {
		@adjustment;
	}

	public static void getNullAdjustment(thisTeam, _otherTeam) {
		ret = new List<string>();
		thisTeam.each_with_index((pkmn, i) => ret[i] = pkmn.level);
		return ret;
	}

	public void getAdjustment(thisTeam, otherTeam) {
		return self.getNullAdjustment(thisTeam, otherTeam);
	}

	public void getOldExp(team1, _team2) {
		ret = new List<string>();
		team1.each_with_index((pkmn, i) => ret[i] = pkmn.exp);
		return ret;
	}

	public void unadjustLevels(team1, team2, adjustments) {
		team1.each_with_index do |pkmn, i|
			if (!adjustments[0][i] || pkmn.exp == adjustments[0][i]) continue;
			pkmn.exp = adjustments[0][i];
			pkmn.calc_stats;
		}
		team2.each_with_index do |pkmn, i|
			if (!adjustments[1][i] || pkmn.exp == adjustments[1][i]) continue;
			pkmn.exp = adjustments[1][i];
			pkmn.calc_stats;
		}
	}

	public void adjustLevels(team1, team2) {
		adj1 = null;
		adj2 = null;
		ret = new {getOldExp(team1, team2), getOldExp(team2, team1)};
		switch (@adjustment) {
			case BOTH_TEAMS:
				adj1 = getAdjustment(team1, team2);
				adj2 = getAdjustment(team2, team1);
				break;
			case MY_TEAM:
				adj1 = getAdjustment(team1, team2);
				break;
			case ENEMY_TEAM:
				adj2 = getAdjustment(team2, team1);
				break;
			case BOTH_TEAMS_DIFFERENT:
				adj1 = getMyAdjustment(team1, team2);
				adj2 = getTheirAdjustment(team2, team1);
				break;
		}
		if (adj1) {
			team1.each_with_index do |pkmn, i|
				if (pkmn.level == adj1[i]) continue;
				pkmn.level = adj1[i];
				pkmn.calc_stats;
			}
		}
		if (adj2) {
			team2.each_with_index do |pkmn, i|
				if (pkmn.level == adj2[i]) continue;
				pkmn.level = adj2[i];
				pkmn.calc_stats;
			}
		}
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public partial class FixedLevelAdjustment : LevelAdjustment {
	public override void initialize(level) {
		base.initialize(LevelAdjustment.BOTH_TEAMS);
		@level = level.clamp(1, GameData.GrowthRate.max_level);
	}

	public void getAdjustment(thisTeam, _otherTeam) {
		ret = new List<string>();
		thisTeam.each_with_index((pkmn, i) => ret[i] = @level);
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public partial class TotalLevelAdjustment : LevelAdjustment {
	public override void initialize(minLevel, maxLevel, totalLevel) {
		base.initialize(LevelAdjustment.ENEMY_TEAM);
		@minLevel = minLevel.clamp(1, GameData.GrowthRate.max_level);
		@maxLevel = maxLevel.clamp(1, GameData.GrowthRate.max_level);
		@totalLevel = totalLevel;
	}

	public void getAdjustment(thisTeam, _otherTeam) {
		ret = new List<string>();
		total = 0;
		thisTeam.each_with_index do |pkmn, i|
			ret[i] = @minLevel;
			total += @minLevel;
		}
		do { //loop; while (true);
			work = false;
			thisTeam.each_with_index do |pkmn, i|
				if (ret[i] >= @maxLevel || total >= @totalLevel) continue;
				ret[i] += 1;
				total += 1;
				work = true;
			}
			if (!work) break;
		}
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public partial class CombinedLevelAdjustment : LevelAdjustment {
	public override void initialize(my, their) {
		base.initialize(LevelAdjustment.BOTH_TEAMS_DIFFERENT);
		@my    = my;
		@their = their;
	}

	public void getMyAdjustment(myTeam, theirTeam) {
		if (@my) return @my.getAdjustment(myTeam, theirTeam);
		return LevelAdjustment.getNullAdjustment(myTeam, theirTeam);
	}

	public void getTheirAdjustment(theirTeam, myTeam) {
		if (@their) return @their.getAdjustment(theirTeam, myTeam);
		return LevelAdjustment.getNullAdjustment(theirTeam, myTeam);
	}
}

//===============================================================================
//
//===============================================================================
public partial class SinglePlayerCappedLevelAdjustment : CombinedLevelAdjustment {
	public override void initialize(level) {
		base.initialize(new CappedLevelAdjustment(level), new FixedLevelAdjustment(level));
	}
}

//===============================================================================
//
//===============================================================================
public partial class CappedLevelAdjustment : LevelAdjustment {
	public override void initialize(level) {
		base.initialize(LevelAdjustment.BOTH_TEAMS);
		@level = level.clamp(1, GameData.GrowthRate.max_level);
	}

	public void getAdjustment(thisTeam, _otherTeam) {
		ret = new List<string>();
		thisTeam.each_with_index((pkmn, i) => ret[i] = (int)Math.Min(pkmn.level, @level));
		return ret;
	}
}

//===============================================================================
// Unused
//===============================================================================
public partial class LevelBalanceAdjustment : LevelAdjustment {
	public override void initialize(minLevel) {
		base.initialize(LevelAdjustment.BOTH_TEAMS);
		@minLevel = minLevel;
	}

	public void getAdjustment(thisTeam, _otherTeam) {
		ret = new List<string>();
		thisTeam.each_with_index do |pkmn, i|
			ret[i] = (int)Math.Round(113 - (BaseStatTotal(pkmn.species) * 0.072));
		}
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public partial class EnemyLevelAdjustment : LevelAdjustment {
	public override void initialize(level) {
		base.initialize(LevelAdjustment.ENEMY_TEAM);
		@level = level.clamp(1, GameData.GrowthRate.max_level);
	}

	public void getAdjustment(thisTeam, _otherTeam) {
		ret = new List<string>();
		thisTeam.each_with_index((pkmn, i) => ret[i] = @level);
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public partial class OpenLevelAdjustment : LevelAdjustment {
	public override void initialize(minLevel = 1) {
		base.initialize(LevelAdjustment.ENEMY_TEAM);
		@minLevel = minLevel;
	}

	public void getAdjustment(thisTeam, otherTeam) {
		maxLevel = 1;
		foreach (var pkmn in otherTeam) { //'otherTeam.each' do => |pkmn|
			level = pkmn.level;
			if (maxLevel < level) maxLevel = level;
		}
		if (maxLevel < @minLevel) maxLevel = @minLevel;
		ret = new List<string>();
		thisTeam.each_with_index((pkmn, i) => ret[i] = maxLevel);
		return ret;
	}
}
