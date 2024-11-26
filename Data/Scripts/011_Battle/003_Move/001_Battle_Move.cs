//===============================================================================
//
//===============================================================================
public partial class Battle.Move {
	public int battle		{ get { return _battle; } }			protected int _battle;
	public int realMove		{ get { return _realMove; } }			protected int _realMove;
	public int id		{ get { return _id; } set { _id = value; } }			protected int _id;
	public int name		{ get { return _name; } }			protected int _name;
	public int function_code		{ get { return _function_code; } }			protected int _function_code;
	public int power		{ get { return _power; } }			protected int _power;
	public int type		{ get { return _type; } }			protected int _type;
	public int category		{ get { return _category; } }			protected int _category;
	public int accuracy		{ get { return _accuracy; } }			protected int _accuracy;
	public int pp		{ get { return _pp; } set { _pp = value; } }			protected int _pp;
	public int total_pp		{ get { return _total_pp; } }			protected int _total_pp;
	public int addlEffect		{ get { return _addlEffect; } }			protected int _addlEffect;
	public int target		{ get { return _target; } }			protected int _target;
	public int priority		{ get { return _priority; } }			protected int _priority;
	public int flags		{ get { return _flags; } }			protected int _flags;
	public int calcType		{ get { return _calcType; } set { _calcType = value; } }			protected int _calcType;
	public int powerBoost		{ get { return _powerBoost; } set { _powerBoost = value; } }			protected int _powerBoost;
	public int snatched		{ get { return _snatched; } set { _snatched = value; } }			protected int _snatched;

	CRITICAL_HIT_RATIOS = (Settings.NEW_CRITICAL_HIT_RATE_MECHANICS) ? new {24, 8, 2, 1} : new {16, 8, 4, 3, 2};

	//-----------------------------------------------------------------------------
	// Creating a move.
	//-----------------------------------------------------------------------------

	public void initialize(battle, move) {
		@battle        = battle;
		@realMove      = move;
		@id            = move.id;
		@name          = move.name;   // Get the move's name
		// Get data on the move
		@function_code = move.function_code;
		@power         = move.power;
		@type          = move.type;
		@category      = move.category;
		@accuracy      = move.accuracy;
		@pp            = move.pp;   // Can be changed with Mimic/Transform
		@target        = move.target;
		@priority      = move.priority;
		@flags         = move.flags.clone;
		@addlEffect    = move.effect_chance;
		@powerBoost    = false;   // For Aerilate, Pixilate, Refrigerate, Galvanize
		@snatched      = false;
	}

	// This is the code actually used to generate a Battle.Move object. The
	// object generated is a subclass of this one which depends on the move's
	// function code.
	public static void from_pokemon_move(battle, move) {
		validate move => Pokemon.Move;
		code = move.function_code || "None";
		if (System.Text.RegularExpressions.Regex.IsMatch(code,@"^\d")) {   // Begins with a digit
			class_name = string.Format("Battle.Move.Effect{0}", code)
		} else {
			class_name = string.Format("Battle.Move.{0}", code)
		}
		if (Object.const_defined(class_name)) {
			return Object.const_get(class_name).new(battle, move);
		}
		return new Battle.Move.Unimplemented(battle, move);
	}

	//-----------------------------------------------------------------------------
	// About the move.
	//-----------------------------------------------------------------------------

	public void Target(_user) {return GameData.Target.get(@target); }

	public void total_pp() {
		if (@total_pp && @total_pp > 0) return @total_pp;   // Usually undefined
		if (@realMove) return @realMove.total_pp;
		return 0;
	}

	// NOTE: This method is only ever called while using a move (and also by the
	//       AI), so using @calcType here is acceptable.
	public bool physicalMove(thisType = null) {
		if (Settings.MOVE_CATEGORY_PER_MOVE) return (@category == 0);
		thisType ||= @calcType;
		thisType ||= @type;
		if (!thisType) return true;
		return GameData.Type.get(thisType).physical();
	}

	// NOTE: This method is only ever called while using a move (and also by the
	//       AI), so using @calcType here is acceptable.
	public bool specialMove(thisType = null) {
		if (Settings.MOVE_CATEGORY_PER_MOVE) return (@category == 1);
		thisType ||= @calcType;
		thisType ||= @type;
		if (!thisType) return false;
		return GameData.Type.get(thisType).special();
	}

	public bool damagingMove() { return @category != 2; }
	public bool statusMove() {   return @category == 2; }

	public void Priority(user) {return @priority; }

	public bool usableWhenAsleep() {    return false; }
	public bool unusableInGravity() {   return false; }
	public bool healingMove() {         return false; }
	public bool recoilMove() {          return false; }
	public bool flinchingMove() {       return false; }
	public bool callsAnotherMove() {    return false; }
	// Whether the move can/will hit more than once in the same turn (including
	// Beat Up which may instead hit just once). Not the same as NumHits>1.
	public bool multiHitMove() {        return false; }
	public bool chargingTurnMove() {    return false; }
	public bool successCheckPerHit() {  return false; }
	public bool hitsFlyingTargets() {   return false; }
	public bool hitsDiggingTargets() {  return false; }
	public bool hitsDivingTargets() {   return false; }
	public bool ignoresReflect() {      return false; }   // For Brick Break
	public bool targetsPosition() {     return false; }   // For Future Sight/Doom Desire
	public bool cannotRedirect() {      return false; }   // For Snipe Shot
	public bool worksWithNoTargets() {  return false; }   // For Explosion
	public bool damageReducedByBurn() { return true;  }   // For Facade
	public bool triggersHyperMode() {   return false; }
	public bool canSnatch() {           return false; }
	public bool canMagicCoat() {        return false; }

	public bool contactMove() {       return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^Contact$",RegexOptions.IgnoreCase));             }
	public bool canProtectAgainst() { return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^CanProtect$",RegexOptions.IgnoreCase));          }
	public bool canMirrorMove() {     return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^CanMirrorMove$",RegexOptions.IgnoreCase));       }
	public bool thawsUser() {         return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^ThawsUser$",RegexOptions.IgnoreCase));           }
	public bool highCriticalRate() {  return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^HighCriticalHitRate$",RegexOptions.IgnoreCase)); }
	public bool bitingMove() {        return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^Biting$",RegexOptions.IgnoreCase));              }
	public bool punchingMove() {      return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^Punching$",RegexOptions.IgnoreCase));            }
	public bool soundMove() {         return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^Sound$",RegexOptions.IgnoreCase));               }
	public bool powderMove() {        return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^Powder$",RegexOptions.IgnoreCase));              }
	public bool pulseMove() {         return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^Pulse$",RegexOptions.IgnoreCase));               }
	public bool bombMove() {          return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^Bomb$",RegexOptions.IgnoreCase));                }
	public bool danceMove() {         return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^Dance$",RegexOptions.IgnoreCase));               }
	public bool slicingMove() {       return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^Slicing$",RegexOptions.IgnoreCase));             }
	public bool windMove() {          return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^Wind$",RegexOptions.IgnoreCase));                }
	// Causes perfect accuracy and double damage if target used Minimize. Perfect accuracy only with Gen 6+ mechanics.
	public bool tramplesMinimize() {  return @flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^TramplesMinimize$",RegexOptions.IgnoreCase));    }

	public bool nonLethal(_user, _target) {  return false; }   // For False Swipe
	public bool preventsBattlerConsumingHealingBerry(battler, targets) {  return false; }   // For Bug Bite/Pluck

	// user is the Pokémon using this move.
	public bool ignoresSubstitute(user) {
		if (Settings.MECHANICS_GENERATION >= 6) {
			if (soundMove()) return true;
			if (user&.hasActiveAbility(Abilitys.INFILTRATOR)) return true;
		}
		return false;
	}

	public void display_type(battler) {
		switch (@function_code) {
			case "TypeDependsOnUserMorpekoFormRaiseUserSpeed1":
				if (battler.isSpecies(Speciess.MORPEKO) || battler.effects.TransformSpecies == :MORPEKO) {
					return BaseType(battler);
				}
				break;
=begin;
			case "TypeDependsOnUserPlate": case "TypeDependsOnUserMemory":
					case "TypeDependsOnUserDrive": case "TypeAndPowerDependOnUserBerry":
					case "TypeIsUserFirstType": case "TypeAndPowerDependOnWeather":
					case "TypeAndPowerDependOnTerrain":
				return BaseType(battler);
=end;
		}
		return @realMove.display_type(battler.pokemon);
	}

	public void display_damage(battler) {
=begin;
		switch (@function_code) {
			case "TypeAndPowerDependOnUserBerry":
				return NaturalGiftBaseDamage(battler.item_id);
			case "TypeAndPowerDependOnWeather": case "TypeAndPowerDependOnTerrain":
					case "PowerHigherWithUserHP": case "PowerLowerWithUserHP":
					case "PowerHigherWithUserHappiness": case "PowerLowerWithUserHappiness":
					case "PowerHigherWithUserPositiveStatStages": case "PowerDependsOnUserStockpile":
				return BaseType(@power, battler, null);
		}
=end;
		return @realMove.display_damage(battler.pokemon);
	}

	public void display_category(battler) {
=begin;
		switch (@function_code) {
			case "CategoryDependsOnHigherDamageIgnoreTargetAbility":
				OnStartUse(user, null);
				return @calcCategory;
		}
=end;
		return @realMove.display_category(battler.pokemon);
	}

	public void display_accuracy(battler) {return @realMove.display_accuracy(battler.pokemon); }
}
