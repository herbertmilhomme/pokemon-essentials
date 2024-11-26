//===============================================================================
//
//===============================================================================
public partial class Battle.AI {
	public void AIRandom(x) {return rand(x); }

	//-----------------------------------------------------------------------------

	public void each_battler() {
		@battlers.each_with_index do |battler, i|
			if (!battler || battler.fainted()) continue;
			yield battler, i;
		}
	}

	public void each_foe_battler(side) {
		@battlers.each_with_index do |battler, i|
			if (!battler || battler.fainted()) continue;
			if (i.even() != side.even()) yield battler, i;
		}
	}

	public void each_same_side_battler(side) {
		@battlers.each_with_index do |battler, i|
			if (!battler || battler.fainted()) continue;
			if (i.even() == side.even()) yield battler, i;
		}
	}

	public void each_ally(index) {
		@battlers.each_with_index do |battler, i|
			if (!battler || battler.fainted()) continue;
			if (i != index && i.even() == index.even()) yield battler, i;
		}
	}

	//-----------------------------------------------------------------------------

	// Assumes that pkmn's ability is not negated by a global effect (e.g.
	// Neutralizing Gas).
	// pkmn is either a Battle.AI.AIBattler or a Pokemon.
	// move is a Battle.Move or a Pokemon.Move.
	public bool pokemon_can_absorb_move(pkmn, move, move_type) {
		if (pkmn.is_a(Battle.AI.AIBattler) && !pkmn.ability_active()) return false;
		// Check pkmn's ability
		// Anything with a Battle.AbilityEffects.MoveImmunity handler
		switch (pkmn.ability_id) {
			case :BULLETPROOF:
				move_data = GameData.Move.get(move.id);
				return move_data.has_flag("Bomb");
			case :FLASHFIRE:
				return move_type == types.FIRE;
			case :LIGHTNINGROD: case :MOTORDRIVE: case :VOLTABSORB:
				return move_type == types.ELECTRIC;
			case :SAPSIPPER:
				return move_type == types.GRASS;
			case :SOUNDPROOF:
				move_data = GameData.Move.get(move.id);
				return move_data.has_flag("Sound");
			case :STORMDRAIN: case :WATERABSORB: case :DRYSKIN:
				return move_type == types.WATER;
			case :TELEPATHY:
				// NOTE: The move is being used by a foe of pkmn.
				return false;
			case :WONDERGUARD:
				types = pkmn.types;
				if (pkmn.is_a(Battle.AI.AIBattler)) types = pkmn.Types(true);
				return !Effectiveness.super_effective_type(move_type, *types);
		}
		return false;
	}

	// Used by Toxic Spikes.
	public bool pokemon_can_be_poisoned(pkmn) {
		// Check pkmn's immunity to being poisoned
		if (@battle.field.terrain == :Misty) return false;
		if (pkmn.hasType(Types.POISON)) return false;
		if (pkmn.hasType(Types.STEEL)) return false;
		if (pkmn.hasAbility(Abilitys.IMMUNITY)) return false;
		if (pkmn.hasAbility(Abilitys.PASTELVEIL)) return false;
		if (pkmn.hasAbility(Abilitys.FLOWERVEIL) && pkmn.hasType(Types.GRASS)) return false;
		if (pkmn.hasAbility(Abilitys.LEAFGUARD) && new []{:Sun, :HarshSun}.Contains(@battle.Weather)) return false;
		if (pkmn.hasAbility(Abilitys.COMATOSE) && pkmn.isSpecies(Speciess.KOMALA)) return false;
		if (pkmn.hasAbility(Abilitys.SHIELDSDOWN) && pkmn.isSpecies(Speciess.MINIOR) && pkmn.form < 7) return false;
		return true;
	}

	public bool pokemon_airborne(pkmn) {
		if (pkmn.hasItem(Items.IRONBALL)) return false;
		if (@battle.field.effects.Gravity > 0) return false;
		if (pkmn.hasType(Types.FLYING)) return true;
		if (pkmn.hasAbility(Abilitys.LEVITATE)) return true;
		if (pkmn.hasItem(Items.AIRBALLOON)) return true;
		return false;
	}

	//-----------------------------------------------------------------------------

	// These values are taken from the Complete-Fire-Red-Upgrade decomp here:
	// https://github.com/Skeli789/Complete-Fire-Red-Upgrade/blob/f7f35becbd111c7e936b126f6328fc52d9af68c8/src/ability_battle_effects.c#L41
	BASE_ABILITY_RATINGS = {
		10 => new {:DELTASTREAM, :DESOLATELAND, :HUGEPOWER, :MOODY, :PARENTALBOND,
					:POWERCONSTRUCT, :PRIMORDIALSEA, :PUREPOWER, :SHADOWTAG,
					:STANCECHANGE, :WONDERGUARD},
		9  => new {:ARENATRAP, :DRIZZLE, :DROUGHT, :IMPOSTER, :MAGICBOUNCE, :MAGICGUARD,
					:MAGNETPULL, :SANDSTREAM, :SPEEDBOOST},
		8  => new {:ADAPTABILITY, :AERILATE, :CONTRARY, :DISGUISE, :DRAGONSMAW,
					:ELECTRICSURGE, :GALVANIZE, :GRASSYSURGE, :ILLUSION, :LIBERO,
					:MISTYSURGE, :MULTISCALE, :MULTITYPE, :NOGUARD, :POISONHEAL,
					:PIXILATE, :PRANKSTER, :PROTEAN, :PSYCHICSURGE, :REFRIGERATE,
					:REGENERATOR, :RKSSYSTEM, :SERENEGRACE, :SHADOWSHIELD, :SHEERFORCE,
					:SIMPLE, :SNOWWARNING, :TECHNICIAN, :TRANSISTOR, :WATERBUBBLE},
		7  => new {:BEASTBOOST, :BULLETPROOF, :COMPOUNDEYES, :DOWNLOAD, :FURCOAT,
					:HUSTLE, :ICESCALES, :INTIMIDATE, :LEVITATE, :LIGHTNINGROD,
					:MEGALAUNCHER, :MOLDBREAKER, :MOXIE, :NATURALCURE, :SAPSIPPER,
					:SHEDSKIN, :SKILLLINK, :SOULHEART, :STORMDRAIN, :TERAVOLT, :THICKFAT,
					:TINTEDLENS, :TOUGHCLAWS, :TRIAGE, :TURBOBLAZE, :UNBURDEN,
					:VOLTABSORB, :WATERABSORB},
		6  => new {:BATTLEBOND, :CHLOROPHYLL, :COMATOSE, :DARKAURA, :DRYSKIN,
					:FAIRYAURA, :FILTER, :FLASHFIRE, :FORECAST, :GALEWINGS, :GUTS,
					:INFILTRATOR, :IRONBARBS, :IRONFIST, :MIRRORARMOR, :MOTORDRIVE,
					:NEUROFORCE, :PRISMARMOR, :QUEENLYMAJESTY, :RECKLESS, :ROUGHSKIN,
					:SANDRUSH, :SCHOOLING, :SCRAPPY, :SHIELDSDOWN, :SOLIDROCK, :STAKEOUT,
					:STAMINA, :STEELWORKER, :STRONGJAW, :STURDY, :SWIFTSWIM, :TOXICBOOST,
					:TRACE, :UNAWARE, :VICTORYSTAR},
		5  => new {:AFTERMATH, :AIRLOCK, :ANALYTIC, :BERSERK, :BLAZE, :CLOUDNINE,
					:COMPETITIVE, :CORROSION, :DANCER, :DAZZLING, :DEFIANT, :FLAREBOOST,
					:FLUFFY, :GOOEY, :HARVEST, :HEATPROOF, :INNARDSOUT, :LIQUIDVOICE,
					:MARVELSCALE, :MUMMY, :NEUTRALIZINGGAS, :OVERCOAT, :OVERGROW,
					:PRESSURE, :QUICKFEET, :ROCKHEAD, :SANDSPIT, :SHIELDDUST, :SLUSHRUSH,
					:SWARM, :TANGLINGHAIR, :TORRENT},
		4  => new {:ANGERPOINT, :BADDREAMS, :CHEEKPOUCH, :CLEARBODY, :CURSEDBODY,
					:EARLYBIRD, :EFFECTSPORE, :FLAMEBODY, :FLOWERGIFT, :FULLMETALBODY,
					:GORILLATACTICS, :HYDRATION, :ICEFACE, :IMMUNITY, :INSOMNIA,
					:JUSTIFIED, :MERCILESS, :PASTELVEIL, :POISONPOINT, :POISONTOUCH,
					:RIPEN, :SANDFORCE, :SOUNDPROOF, :STATIC, :SURGESURFER, :SWEETVEIL,
					:SYNCHRONIZE, :VITALSPIRIT, :WATERCOMPACTION, :WATERVEIL,
					:WHITESMOKE, :WONDERSKIN},
		3  => new {:AROMAVEIL, :AURABREAK, :COTTONDOWN, :DAUNTLESSSHIELD,
					:EMERGENCYEXIT, :GLUTTONY, :GULPMISSLE, :HYPERCUTTER, :ICEBODY,
					:INTREPIDSWORD, :LIMBER, :LIQUIDOOZE, :LONGREACH, :MAGICIAN,
					:OWNTEMPO, :PICKPOCKET, :RAINDISH, :RATTLED, :SANDVEIL,
					:SCREENCLEANER, :SNIPER, :SNOWCLOAK, :SOLARPOWER, :STEAMENGINE,
					:STICKYHOLD, :SUPERLUCK, :UNNERVE, :WIMPOUT},
		2  => new {:BATTLEARMOR, :COLORCHANGE, :CUTECHARM, :DAMP, :GRASSPELT,
					:HUNGERSWITCH, :INNERFOCUS, :LEAFGUARD, :LIGHTMETAL, :MIMICRY,
					:OBLIVIOUS, :POWERSPOT, :PROPELLORTAIL, :PUNKROCK, :SHELLARMOR,
					:STALWART, :STEADFAST, :STEELYSPIRIT, :SUCTIONCUPS, :TANGLEDFEET,
					:WANDERINGSPIRIT, :WEAKARMOR},
		1  => new {:BIGPECKS, :KEENEYE, :MAGMAARMOR, :PICKUP, :RIVALRY, :STENCH},
		0  => new {:ANTICIPATION, :ASONECHILLINGNEIGH, :ASONEGRIMNEIGH, :BALLFETCH,
					:BATTERY, :CHILLINGNEIGH, :CURIOUSMEDICINE, :FLOWERVEIL, :FOREWARN,
					:FRIENDGUARD, :FRISK, :GRIMNEIGH, :HEALER, :HONEYGATHER, :ILLUMINATE,
					:MINUS, :PLUS, :POWEROFALCHEMY, :QUICKDRAW, :RECEIVER, :RUNAWAY,
					:SYMBIOSIS, :TELEPATHY, :UNSEENFIST},
		-1 => new {:DEFEATIST, :HEAVYMETAL, :KLUTZ, :NORMALIZE, :PERISHBODY, :STALL,
					:ZENMODE},
		-2 => new {:SLOWSTART, :TRUANT}
	}

	//-----------------------------------------------------------------------------

	BASE_ITEM_RATINGS = {
		10 => new {:EVIOLITE, :FOCUSSASH, :LIFEORB, :THICKCLUB},
		9  => new {:ASSAULTVEST, :BLACKSLUDGE, :CHOICEBAND, :CHOICESCARF, :CHOICESPECS,
					:DEEPSEATOOTH, :LEFTOVERS},
		8  => new {:LEEK, :STICK, :THROATSPRAY, :WEAKNESSPOLICY},
		7  => new {:EXPERTBELT, :LIGHTBALL, :LUMBERRY, :POWERHERB, :ROCKYHELMET,
					:SITRUSBERRY},
		6  => new {:KINGSROCK, :LIECHIBERRY, :LIGHTCLAY, :PETAYABERRY, :RAZORFANG,
					:REDCARD, :SALACBERRY, :SHELLBELL, :WHITEHERB,
					// Type-resisting berries
					:BABIRIBERRY, :CHARTIBERRY, :CHILANBERRY, :CHOPLEBERRY, :COBABERRY,
					:COLBURBERRY, :HABANBERRY, :KASIBBERRY, :KEBIABERRY, :OCCABERRY,
					:PASSHOBERRY, :PAYAPABERRY, :RINDOBERRY, :ROSELIBERRY, :SHUCABERRY,
					:TANGABERRY, :WACANBERRY, :YACHEBERRY,
					// Gems
					:BUGGEM, :DARKGEM, :DRAGONGEM, :ELECTRICGEM, :FAIRYGEM, :FIGHTINGGEM,
					:FIREGEM, :FLYINGGEM, :GHOSTGEM, :GRASSGEM, :GROUNDGEM, :ICEGEM,
					:NORMALGEM, :POISONGEM, :PSYCHICGEM, :ROCKGEM, :STEELGEM, :WATERGEM,
					// Legendary Orbs
					:ADAMANTORB, :GRISEOUSORB, :LUSTROUSORB, :SOULDEW,
					// Berries that heal HP and may confuse
					:AGUAVBERRY, :FIGYBERRY, :IAPAPABERRY, :MAGOBERRY, :WIKIBERRY},
		5  => new {:CUSTAPBERRY, :DEEPSEASCALE, :EJECTBUTTON, :FOCUSBAND, :JABOCABERRY,
					:KEEBERRY, :LANSATBERRY, :MARANGABERRY, :MENTALHERB, :METRONOME,
					:MUSCLEBAND, :QUICKCLAW, :RAZORCLAW, :ROWAPBERRY, :SCOPELENS,
					:WISEGLASSES,
					// Type power boosters
					:BLACKBELT, :BLACKGLASSES, :CHARCOAL, :DRAGONFANG, :HARDSTONE,
					:MAGNET, :METALCOAT, :MIRACLESEED, :MYSTICWATER, :NEVERMELTICE,
					:POISONBARB, :SHARPBEAK, :SILKSCARF, :SILVERPOWDER, :SOFTSAND,
					:SPELLTAG, :TWISTEDSPOON,
					:ODDINCENSE, :ROCKINCENSE, :ROSEINCENSE, :SEAINCENSE, :WAVEINCENSE,
					// Plates
					:DRACOPLATE, :DREADPLATE, :EARTHPLATE, :FISTPLATE, :FLAMEPLATE,
					:ICICLEPLATE, :INSECTPLATE, :IRONPLATE, :MEADOWPLATE, :MINDPLATE,
					:PIXIEPLATE, :SKYPLATE, :SPLASHPLATE, :SPOOKYPLATE, :STONEPLATE,
					:TOXICPLATE, :ZAPPLATE,
					// Weather/terrain extenders
					:DAMPROCK, :HEATROCK, :ICYROCK, :SMOOTHROCK, :TERRAINEXTENDER},
		4  => new {:ADRENALINEORB, :APICOTBERRY, :BLUNDERPOLICY, :CHESTOBERRY,
					:EJECTPACK, :ENIGMABERRY, :GANLONBERRY, :HEAVYDUTYBOOTS,
					:ROOMSERVICE, :SAFETYGOGGLES, :SHEDSHELL, :STARFBERRY},
		3  => new {:BIGROOT, :BRIGHTPOWDER, :LAXINCENSE, :LEPPABERRY, :PERSIMBERRY,
					:PROTECTIVEPADS, :UTILITYUMBRELLA,
					// Status problem-curing berries (except Chesto which is in 4)
					:ASPEARBERRY, :CHERIBERRY, :PECHABERRY, :RAWSTBERRY},
		2  => new {:ABSORBBULB, :BERRYJUICE, :CELLBATTERY, :GRIPCLAW, :LUMINOUSMOSS,
					:MICLEBERRY, :ORANBERRY, :SNOWBALL, :WIDELENS, :ZOOMLENS,
					// Terrain seeds
					:ELECTRICSEED, :GRASSYSEED, :MISTYSEED, :PSYCHICSEED},
		1  => new {:AIRBALLOON, :BINDINGBAND, :DESTINYKNOT, :FLOATSTONE, :LUCKYPUNCH,
					:METALPOWDER, :QUICKPOWDER,
					// Drives
					:BURNDRIVE, :CHILLDRIVE, :DOUSEDRIVE, :SHOCKDRIVE,
					// Memories
					:BUGMEMORY, :DARKMEMORY, :DRAGONMEMORY, :ELECTRICMEMORY,
					:FAIRYMEMORY, :FIGHTINGMEMORY, :FIREMEMORY, :FLYINGMEMORY,
					:GHOSTMEMORY, :GRASSMEMORY, :GROUNDMEMORY, :ICEMEMORY, :POISONMEMORY,
					:PSYCHICMEMORY, :ROCKMEMORY, :STEELMEMORY, :WATERMEMORY},
		0  => [:SMOKEBALL],
		-5 => new {:FULLINCENSE, :LAGGINGTAIL, :RINGTARGET},
		-6 => new {:MACHOBRACE, :POWERANKLET, :POWERBAND, :POWERBELT, :POWERBRACER,
					:POWERLENS, :POWERWEIGHT},
		-7 => new {:FLAMEORB, :IRONBALL, :TOXICORB},
		-9 => [:STICKYBARB];
	}
}

//===============================================================================
//
//===============================================================================

Battle.AI.Handlers.AbilityRanking.add(:BLAZE,
	block: (ability, score, battler, ai) => {
		if (battler.has_damaging_move_of_type(types.FIRE)) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:CUTECHARM,
	block: (ability, score, battler, ai) => {
		if (battler.gender == 2) next 0;
		next score;
	}
)

Battle.AI.Handlers.AbilityRanking.copy(:CUTECHARM, :RIVALRY);

Battle.AI.Handlers.AbilityRanking.add(:FRIENDGUARD,
	block: (ability, score, battler, ai) => {
		has_ally = false;
		ai.each_ally(battler.side, (b, i) => { has_ally = true; });
		if (has_ally) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.copy(:FRIENDGUARD, :HEALER, :SYMBOISIS, :TELEPATHY);

Battle.AI.Handlers.AbilityRanking.add(:GALEWINGS,
	block: (ability, score, battler, ai) => {
		if (battler.check_for_move(m => m.type == types.FLYING)) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:HUGEPOWER,
	block: (ability, score, battler, ai) => {
		if (ai.stat_raise_worthwhile(battler, :ATTACK, true)) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.copy(:HUGEPOWER, :PUREPOWER);

Battle.AI.Handlers.AbilityRanking.add(:IRONFIST,
	block: (ability, score, battler, ai) => {
		if (battler.check_for_move(m => m.punchingMove())) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:LIQUIDVOICE,
	block: (ability, score, battler, ai) => {
		if (battler.check_for_move(m => m.soundMove())) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:MEGALAUNCHER,
	block: (ability, score, battler, ai) => {
		if (battler.check_for_move(m => m.pulseMove())) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:OVERGROW,
	block: (ability, score, battler, ai) => {
		if (battler.has_damaging_move_of_type(types.GRASS)) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:PRANKSTER,
	block: (ability, score, battler, ai) => {
		if (battler.check_for_move(m => m.statusMove())) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:PUNKROCK,
	block: (ability, score, battler, ai) => {
		if (battler.check_for_move(m => m.damagingMove() && m.soundMove())) next score;
		next 1;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:RECKLESS,
	block: (ability, score, battler, ai) => {
		if (battler.check_for_move(m => m.recoilMove())) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:ROCKHEAD,
	block: (ability, score, battler, ai) => {
		if (battler.check_for_move(m => m.recoilMove() && !m.is_a(Battle.Move.CrashDamageIfFailsUnusableInGravity))) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:RUNAWAY,
	block: (ability, score, battler, ai) => {
		if (battler.wild()) next 0;
		next score;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:SANDFORCE,
	block: (ability, score, battler, ai) => {
		if (battler.has_damaging_move_of_type(:GROUND, :ROCK, :STEEL)) next score;
		next 2;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:SKILLLINK,
	block: (ability, score, battler, ai) => {
		if (battler.check_for_move(m => m.is_a(Battle.Move.HitTwoToFiveTimes))) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:STEELWORKER,
	block: (ability, score, battler, ai) => {
		if (battler.has_damaging_move_of_type(types.STEEL)) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:SWARM,
	block: (ability, score, battler, ai) => {
		if (battler.has_damaging_move_of_type(types.BUG)) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:TORRENT,
	block: (ability, score, battler, ai) => {
		if (battler.has_damaging_move_of_type(types.WATER)) next score;
		next 0;
	}
)

Battle.AI.Handlers.AbilityRanking.add(:TRIAGE,
	block: (ability, score, battler, ai) => {
		if (battler.check_for_move(m => m.healingMove())) next score;
		next 0;
	}
)

//===============================================================================
//
//===============================================================================

Battle.AI.Handlers.ItemRanking.add(:ADAMANTORB,
	block: (item, score, battler, ai) => {
		if (battler.battler.isSpecies(Speciess.DIALGA) &&
									battler.has_damaging_move_of_type(:DRAGON, :STEEL)) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:AGUAVBERRY,
	block: (item, score, battler, ai) => {
		if (Settings.MECHANICS_GENERATION == 7) {   // Heals 50%
			score += 2;
		} else if (Settings.MECHANICS_GENERATION <= 6) {   // Heals 12.5%
			score -= 3;
		}
		if (ai.trainer.high_skill()) {
			if (battler.battler.nature.stat_changes.any(val => val[0] == :SPECIAL_DEFENSE && val[1] < 0)) {
				score -= 2;   // Will confuse
			}
		}
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.add(:ASSAULTVEST,
	block: (item, score, battler, ai) => {
		if (ai.trainer.high_skill()) {
			if (!battler.check_for_move(m => m.statusMove() && !m.is_a(Battle.Move.UseMoveTargetIsAboutToUse))) score += 1;
		}
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.add(:BERRYJUICE,
	block: (item, score, battler, ai) => {
		next (int)Math.Max(10 - (battler.totalhp / 15), 1);
	}
)

Battle.AI.Handlers.ItemRanking.add(:BIGROOT,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move do |m|
			m.is_a(Battle.Move.HealUserByHalfOfDamageDone) ||
			m.is_a(Battle.Move.HealUserByHalfOfDamageDoneIfTargetAsleep) ||
			m.is_a(Battle.Move.HealUserByThreeQuartersOfDamageDone) ||
			m.is_a(Battle.Move.HealUserByTargetAttackLowerTargetAttack1) ||
			m.is_a(Battle.Move.StartLeechSeedTarget)) next score;
		}
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:BINDINGBAND,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move(m => m.is_a(Battle.Move.BindTarget))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.copy(:BINDINGBAND, :GRIPCLAW);

Battle.AI.Handlers.ItemRanking.add(:BLACKSLUDGE,
	block: (item, score, battler, ai) => {
		if (battler.has_type(types.POISON)) next score;
		next -9;
	}
)

Battle.AI.Handlers.ItemRanking.add(:CHESTOBERRY,
	block: (item, score, battler, ai) => {
		if (ai.trainer.high_skill()) {
			if (battler.has_move_with_function("HealUserFullyAndFallAsleep")) score += 1;
		}
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.add(:CHOICEBAND,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move(m => m.physicalMove(m.type))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.copy(:CHOICEBAND, :MUSCLEBAND);

Battle.AI.Handlers.ItemRanking.add(:CHOICESPECS,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move(m => m.specialMove(m.type))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.copy(:CHOICESPECS, :WISEGLASSES);

Battle.AI.Handlers.ItemRanking.add(:DEEPSEASCALE,
	block: (item, score, battler, ai) => {
		if (battler.battler.isSpecies(Speciess.CLAMPERL)) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:DAMPROCK,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move(m => m.is_a(Battle.Move.StartRainWeather))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:DEEPSEATOOTH,
	block: (item, score, battler, ai) => {
		if (battler.battler.isSpecies(Speciess.CLAMPERL) &&
									battler.check_for_move(m => m.specialMove(m.type))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:ELECTRICSEED,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move(m => m.is_a(Battle.Move.StartElectricTerrain))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:EVIOLITE,
	block: (item, score, battler, ai) => {
		if (battler.battler.pokemon.species_data.get_evolutions(true).length > 0) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:FIGYBERRY,
	block: (item, score, battler, ai) => {
		if (Settings.MECHANICS_GENERATION == 7) {   // Heals 50%
			score += 2;
		} else if (Settings.MECHANICS_GENERATION <= 6) {   // Heals 12.5%
			score -= 3;
		}
		if (ai.trainer.high_skill()) {
			if (battler.battler.nature.stat_changes.any(val => val[0] == :ATTACK && val[1] < 0)) {
				score -= 2;   // Will confuse
			}
		}
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.add(:FLAMEORB,
	block: (item, score, battler, ai) => {
		if (battler.status != statuses.NONE) next 0;
		if (battler.wants_status_problem(:BURN)) next 7;
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.add(:FULLINCENSE,
	block: (item, score, battler, ai) => {
		if (ai.trainer.high_skill()) {
			if (battler.has_active_ability(abilitys.ANALYTIC)) score = 7;
		}
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.copy(:FULLINCENSE, :LAGGINGTAIL);

Battle.AI.Handlers.ItemRanking.add(:GRASSYSEED,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move(m => m.is_a(Battle.Move.StartGrassyTerrain))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:GRISEOUSORB,
	block: (item, score, battler, ai) => {
		if (battler.battler.isSpecies(Speciess.GIRATINA) &&
									battler.has_damaging_move_of_type(:DRAGON, :GHOST)) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:HEATROCK,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move(m => m.is_a(Battle.Move.StartSunWeather))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:IAPAPABERRY,
	block: (item, score, battler, ai) => {
		if (Settings.MECHANICS_GENERATION == 7) {   // Heals 50%
			score += 2;
		} else if (Settings.MECHANICS_GENERATION <= 6) {   // Heals 12.5%
			score -= 3;
		}
		if (ai.trainer.high_skill()) {
			if (battler.battler.nature.stat_changes.any(val => val[0] == :DEFENSE && val[1] < 0)) {
				score -= 2;   // Will confuse
			}
		}
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.add(:ICYROCK,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move(m => m.is_a(Battle.Move.StartHailWeather))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:IRONBALL,
	block: (item, score, battler, ai) => {
		if (battler.has_move_with_function("ThrowUserItemAtTarget")) next 0;
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.add(:KINGSROCK,
	block: (item, score, battler, ai) => {
		if (ai.trainer.high_skill()) {
			if (battler.check_for_move(m => m.multiHitMove())) score += 1;
		}
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.copy(:KINGSROCK, :RAZORFANG);

Battle.AI.Handlers.ItemRanking.add(:LEEK,
	block: (item, score, battler, ai) => {
		if ((battler.battler.isSpecies(Speciess.FARFETCHD) || battler.battler.isSpecies(Speciess.SIRFETCHD)) &&
									battler.check_for_move(m => m.damagingMove())) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.copy(:LEEK, :STICK);

Battle.AI.Handlers.ItemRanking.add(:LIGHTBALL,
	block: (item, score, battler, ai) => {
		if (battler.battler.isSpecies(Speciess.PIKACHU) &&
									battler.check_for_move(m => m.damagingMove())) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:LIGHTCLAY,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move do |m|
			m.is_a(Battle.Move.StartWeakenPhysicalDamageAgainstUserSide) ||
			m.is_a(Battle.Move.StartWeakenSpecialDamageAgainstUserSide) ||
			m.is_a(Battle.Move.StartWeakenDamageAgainstUserSideIfHail)) next score;
		}
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:LUCKYPUNCH,
	block: (item, score, battler, ai) => {
		if (battler.battler.isSpecies(Speciess.CHANSEY)) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:LUSTROUSORB,
	block: (item, score, battler, ai) => {
		if (battler.battler.isSpecies(Speciess.PALKIA) &&
									battler.has_damaging_move_of_type(:DRAGON, :WATER)) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:MAGOBERRY,
	block: (item, score, battler, ai) => {
		if (Settings.MECHANICS_GENERATION == 7) {   // Heals 50%
			score += 2;
		} else if (Settings.MECHANICS_GENERATION <= 6) {   // Heals 12.5%
			score -= 3;
		}
		if (ai.trainer.high_skill()) {
			if (battler.battler.nature.stat_changes.any(val => val[0] == :SPEED && val[1] < 0)) {
				score -= 2;   // Will confuse
			}
		}
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.add(:METALPOWDER,
	block: (item, score, battler, ai) => {
		if (battler.battler.isSpecies(Speciess.DITTO) && !battler.effects.Transform) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.copy(:METALPOWDER, :QUICKPOWDER);

Battle.AI.Handlers.ItemRanking.add(:MISTYSEED,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move(m => m.is_a(Battle.Move.StartMistyTerrain))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:ORANBERRY,
	block: (item, score, battler, ai) => {
		next (int)Math.Max(10 - (battler.totalhp / 8), 1);
	}
)

Battle.AI.Handlers.ItemRanking.add(:POWERHERB,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move do |m|
			m.is_a(Battle.Move.TwoTurnMove) &&
			!m.is_a(Battle.Move.TwoTurnAttackInvulnerableInSkyTargetCannotAct)) next score;
		}
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:PSYCHICSEED,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move(m => m.is_a(Battle.Move.StartPsychicTerrain))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:RINGTARGET,
	block: (item, score, battler, ai) => {
		has_immunity = false;
		foreach (var type in battler.Types(true)) { //'battler.Types(true).each' do => |type|
			has_immunity = GameData.Type.get(type).immunities.length > 0;
			if (has_immunity) break;
		}
		if (has_immunity) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:SMOOTHROCK,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move(m => m.is_a(Battle.Move.StartSandstormWeather))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:SOULDEW,
	block: (item, score, battler, ai) => {
		if (!battler.battler.isSpecies(Speciess.LATIAS) && !battler.battler.isSpecies(Speciess.LATIOS)) next 0;
		if (Settings.SOUL_DEW_POWERS_UP_TYPES) {
			if (!battler.has_damaging_move_of_type(:PSYCHIC, :DRAGON)) next 0;
		} else if (battler.check_for_move(m => m.specialMove(m.type))) {
			next 10;
		} else {
			next 6;   // Boosts SpDef
		}
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.add(:TERRAINEXTENDER,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move do |m|
			m.is_a(Battle.Move.StartElectricTerrain) ||
			m.is_a(Battle.Move.StartGrassyTerrain) ||
			m.is_a(Battle.Move.StartMistyTerrain) ||
			m.is_a(Battle.Move.StartPsychicTerrain)) next score;
		}
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:THICKCLUB,
	block: (item, score, battler, ai) => {
		if ((battler.battler.isSpecies(Speciess.CUBONE) || battler.battler.isSpecies(Speciess.MAROWAK)) &&
									battler.check_for_move(m => m.physicalMove(m.type))) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:THROATSPRAY,
	block: (item, score, battler, ai) => {
		if (battler.check_for_move(m => m.soundMove())) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.add(:TOXICORB,
	block: (item, score, battler, ai) => {
		if (battler.status != statuses.NONE) next 0;
		if (battler.wants_status_problem(:POISON)) next 7;
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.add(:WHITEHERB,
	block: (item, score, battler, ai) => {
		if (ai.trainer.high_skill()) {
			if (battler.has_move_with_function("LowerUserDefSpDef1RaiseUserAtkSpAtkSpd2")) score += 1;
		}
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.add(:WIKIBERRY,
	block: (item, score, battler, ai) => {
		if (Settings.MECHANICS_GENERATION == 7) {   // Heals 50%
			score += 2;
		} else if (Settings.MECHANICS_GENERATION <= 6) {   // Heals 12.5%
			score -= 3;
		}
		if (ai.trainer.high_skill()) {
			if (battler.battler.nature.stat_changes.any(val => val[0] == :SPECIAL_ATTACK && val[1] < 0)) {
				score -= 2;   // Will confuse
			}
		}
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.add(:ZOOMLENS,
	block: (item, score, battler, ai) => {
		if (ai.trainer.high_skill()) {
			if (battler.stages[:ACCURACY] < 0) score += 1;
			foreach (var m in battler.battler.Moves) { //battler.battler.eachMove do => |m|
				if (m.accuracy == 0 || m.is_a(Battle.Move.OHKO)) continue;
				if (m.accuracy > 70) continue;
				score += 1;
				break;
			}
		}
		next score;
	}
)

Battle.AI.Handlers.ItemRanking.addIf(:type_boosting_items,
	block: (item) => {
		next new []{:BLACKBELT, :BLACKGLASSES, :CHARCOAL, :DRAGONFANG, :HARDSTONE,
					:MAGNET, :METALCOAT, :MIRACLESEED, :MYSTICWATER, :NEVERMELTICE,
					:POISONBARB, :SHARPBEAK, :SILKSCARF, :SILVERPOWDER, :SOFTSAND,
					:SPELLTAG, :TWISTEDSPOON,
					:DRACOPLATE, :DREADPLATE, :EARTHPLATE, :FISTPLATE, :FLAMEPLATE,
					:ICICLEPLATE, :INSECTPLATE, :IRONPLATE, :MEADOWPLATE, :MINDPLATE,
					:PIXIEPLATE, :SKYPLATE, :SPLASHPLATE, :SPOOKYPLATE, :STONEPLATE,
					:TOXICPLATE, :ZAPPLATE,
					:ODDINCENSE, :ROCKINCENSE, :ROSEINCENSE, :SEAINCENSE, :WAVEINCENSE}.Contains(item);
	},
	block: (item, score, battler, ai) => {
		boosters = {
			BUG      = new {:SILVERPOWDER, :INSECTPLATE},
			DARK     = new {:BLACKGLASSES, :DREADPLATE},
			DRAGON   = new {:DRAGONFANG, :DRACOPLATE},
			ELECTRIC = new {:MAGNET, :ZAPPLATE},
			FAIRY    = [:PIXIEPLATE],
			FIGHTING = new {:BLACKBELT, :FISTPLATE},
			FIRE     = new {:CHARCOAL, :FLAMEPLATE},
			FLYING   = new {:SHARPBEAK, :SKYPLATE},
			GHOST    = new {:SPELLTAG, :SPOOKYPLATE},
			GRASS    = new {:MIRACLESEED, :MEADOWPLATE, :ROSEINCENSE},
			GROUND   = new {:SOFTSAND, :EARTHPLATE},
			ICE      = new {:NEVERMELTICE, :ICICLEPLATE},
			NORMAL   = [:SILKSCARF],
			POISON   = new {:POISONBARB, :TOXICPLATE},
			PSYCHIC  = new {:TWISTEDSPOON, :MINDPLATE, :ODDINCENSE},
			ROCK     = new {:HARDSTONE, :STONEPLATE, :ROCKINCENSE},
			STEEL    = new {:METALCOAT, :IRONPLATE},
			WATER    = new {:MYSTICWATER, :SPLASHPLATE, :SEAINCENSE, :WAVEINCENSE}
		}
		boosted_type = null;
		boosters.each_pair do |type, items|
			if (!items.Contains(item)) continue;
			boosted_type = type;
			break;
		}
		if (boosted_type && battler.has_damaging_move_of_type(boosted_type)) next score;
		next 0;
	}
)

Battle.AI.Handlers.ItemRanking.addIf(:gems,
	block: (item) => {
		next new []{:FIREGEM, :WATERGEM, :ELECTRICGEM, :GRASSGEM, :ICEGEM, :FIGHTINGGEM,
					:POISONGEM, :GROUNDGEM, :FLYINGGEM, :PSYCHICGEM, :BUGGEM, :ROCKGEM,
					:GHOSTGEM, :DRAGONGEM, :DARKGEM, :STEELGEM, :FAIRYGEM, :NORMALGEM}.Contains(item);
	},
	block: (item, score, battler, ai) => {
		if (Settings.MECHANICS_GENERATION <= 5) score += 2;   // 1.5x boost rather than 1.3x
		boosted_type = {
			BUGGEM      = :BUG,
			DARKGEM     = :DARK,
			DRAGONGEM   = :DRAGON,
			ELECTRICGEM = :ELECTRIC,
			FAIRYGEM    = :FAIRY,
			FIGHTINGGEM = :FIGHTING,
			FIREGEM     = :FIRE,
			FLYINGGEM   = :FLYING,
			GHOSTGEM    = :GHOST,
			GRASSGEM    = :GRASS,
			GROUNDGEM   = :GROUND,
			ICEGEM      = :ICE,
			NORMALGEM   = :NORMAL,
			POISONGEM   = :POISON,
			PSYCHICGEM  = :PSYCHIC,
			ROCKGEM     = :ROCK,
			STEELGEM    = :STEEL,
			WATERGEM    = :WATER;
		}[item];
		if (boosted_type && battler.has_damaging_move_of_type(boosted_type)) next score;
		next 0;
	}
)
