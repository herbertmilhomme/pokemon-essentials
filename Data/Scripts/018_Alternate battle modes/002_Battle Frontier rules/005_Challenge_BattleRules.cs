// NOTE: The following clauses have battle code implementing them, but no class
//       below to apply them:
//         "drawclause"
//         "modifiedselfdestructclause"
//         "suddendeath"

//===============================================================================
//
//===============================================================================
public partial class BattleRule {
	public void setRule(battle) {}
}

//===============================================================================
//
//===============================================================================
public partial class DoubleBattle : BattleRule {
	public void setRule(battle) {battle.setBattleMode("double"); }
}

//===============================================================================
//
//===============================================================================
public partial class SingleBattle : BattleRule {
	public void setRule(battle) {battle.setBattleMode("single"); }
}

//===============================================================================
//
//===============================================================================
public partial class SoulDewBattleClause : BattleRule {
	public void setRule(battle) {battle.rules["souldewclause"] = true; }
}

//===============================================================================
//
//===============================================================================
public partial class SleepClause : BattleRule {
	public void setRule(battle) {battle.rules["sleepclause"] = true; }
}

//===============================================================================
//
//===============================================================================
public partial class FreezeClause : BattleRule {
	public void setRule(battle) {battle.rules["freezeclause"] = true; }
}

//===============================================================================
//
//===============================================================================
public partial class EvasionClause : BattleRule {
	public void setRule(battle) {battle.rules["evasionclause"] = true; }
}

//===============================================================================
//
//===============================================================================
public partial class OHKOClause : BattleRule {
	public void setRule(battle) {battle.rules["ohkoclause"] = true; }
}

//===============================================================================
//
//===============================================================================
public partial class PerishSongClause : BattleRule {
	public void setRule(battle) {battle.rules["perishsongclause"] = true; }
}

//===============================================================================
//
//===============================================================================
public partial class SelfKOClause : BattleRule {
	public void setRule(battle) {battle.rules["selfkoclause"] = true; }
}

//===============================================================================
//
//===============================================================================
public partial class SelfdestructClause : BattleRule {
	public void setRule(battle) {battle.rules["selfdestructclause"] = true; }
}

//===============================================================================
//
//===============================================================================
public partial class SonicBoomClause : BattleRule {
	public void setRule(battle) {battle.rules["sonicboomclause"] = true; }
}

//===============================================================================
//
//===============================================================================
public partial class ModifiedSleepClause : BattleRule {
	public void setRule(battle) {battle.rules["modifiedsleepclause"] = true; }
}

//===============================================================================
//
//===============================================================================
public partial class SkillSwapClause : BattleRule {
	public void setRule(battle) {battle.rules["skillswapclause"] = true; }
}
