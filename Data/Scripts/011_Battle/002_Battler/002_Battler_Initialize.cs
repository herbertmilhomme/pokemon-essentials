//===============================================================================
//
//===============================================================================
public partial class Battle.Battler {
	//-----------------------------------------------------------------------------
	// Creating a battler.
	//-----------------------------------------------------------------------------

	public void initialize(btl, idxBattler) {
		@battle      = btl;
		@index       = idxBattler;
		@captured    = false;
		@dummy       = false;
		@stages      = new List<string>();
		@effects     = new List<string>();
		@damageState = new Battle.DamageState();
		InitBlank;
		InitEffects(false);
	}

	public void InitBlank() {
		@name           = "";
		@species        = 0;
		@form           = 0;
		@level          = 0;
		@hp = @totalhp  = 0;
		@types          = new List<string>();
		@ability_id     = null;
		@item_id        = null;
		@attack = @defense = @spatk = @spdef = @speed = 0;
		@status         = :NONE;
		@statusCount    = 0;
		@pokemon        = null;
		@pokemonIndex   = -1;
		@participants   = new List<string>();
		@moves          = new List<string>();
	}

	// Used by Future Sight only, when Future Sight's user is no longer in battle.
	public void InitDummyPokemon(pkmn, idxParty) {
		if (pkmn.egg()) raise _INTL("An egg can't be an active Pokémon.");
		@name         = pkmn.name;
		@species      = pkmn.species;
		@form         = pkmn.form;
		@level        = pkmn.level;
		@hp           = pkmn.hp;
		@totalhp      = pkmn.totalhp;
		@types        = pkmn.types;
		// ability and item intentionally not copied across here
		@attack       = pkmn.attack;
		@defense      = pkmn.defense;
		@spatk        = pkmn.spatk;
		@spdef        = pkmn.spdef;
		@speed        = pkmn.speed;
		@status       = pkmn.status;
		@statusCount  = pkmn.statusCount;
		@pokemon      = pkmn;
		@pokemonIndex = idxParty;
		@participants = new List<string>();
		// moves intentionally not copied across here
		@dummy        = true;
	}

	public void Initialize(pkmn, idxParty, batonPass = false) {
		InitPokemon(pkmn, idxParty);
		InitEffects(batonPass);
		@damageState.reset;
	}

	public void InitPokemon(pkmn, idxParty) {
		if (pkmn.egg()) raise _INTL("An egg can't be an active Pokémon.");
		@name         = pkmn.name;
		@species      = pkmn.species;
		@form         = pkmn.form;
		@level        = pkmn.level;
		@hp           = pkmn.hp;
		@totalhp      = pkmn.totalhp;
		@types        = pkmn.types;
		@ability_id   = pkmn.ability_id;
		@item_id      = pkmn.item_id;
		@attack       = pkmn.attack;
		@defense      = pkmn.defense;
		@spatk        = pkmn.spatk;
		@spdef        = pkmn.spdef;
		@speed        = pkmn.speed;
		@status       = pkmn.status;
		@statusCount  = pkmn.statusCount;
		@pokemon      = pkmn;
		@pokemonIndex = idxParty;
		@participants = new List<string>();   // Participants earn Exp. if this battler is defeated
		@moves        = new List<string>();
		pkmn.moves.each_with_index do |m, i|
			@moves[i] = Battle.Move.from_pokemon_move(@battle, m);
		}
	}

	public void InitEffects(batonPass) {
		if (batonPass) {
			// These effects are passed on if Baton Pass is used, but they need to be
			// reapplied
			@effects.LaserFocus = (@effects.LaserFocus > 0) ? 2 : 0;
			@effects.LockOn     = (@effects.LockOn > 0) ? 2 : 0;
			if (@effects.PowerTrick) {
				@attack, @defense = @defense, @attack;
			}
			// These effects are passed on if Baton Pass is used, but they need to be
			// cancelled in certain circumstances anyway
			if (isSpecies(Speciess.GENGAR) && mega()) @effects.Telekinesis = 0;
			if (unstoppableAbility()) @effects.GastroAcid  = false;
		} else {
			// These effects are passed on if Baton Pass is used
			GameData.Stat.each_battle(stat => @stages[stat.id] = 0);
			@effects.AquaRing          = false;
			@effects.Confusion         = 0;
			@effects.Curse             = false;
			@effects.Embargo           = 0;
			@effects.FocusEnergy       = 0;
			@effects.GastroAcid        = false;
			@effects.HealBlock         = 0;
			@effects.Ingrain           = false;
			@effects.LaserFocus        = 0;
			@effects.LeechSeed         = -1;
			@effects.LockOn            = 0;
			@effects.LockOnPos         = -1;
			@effects.MagnetRise        = 0;
			@effects.PerishSong        = 0;
			@effects.PerishSongUser    = -1;
			@effects.PowerTrick        = false;
			@effects.Substitute        = 0;
			@effects.Telekinesis       = 0;
		}
		@fainted                 = (@hp == 0);
		@lastAttacker            = new List<string>();
		@lastFoeAttacker         = new List<string>();
		@lastHPLost              = 0;
		@lastHPLostFromFoe       = 0;
		@droppedBelowHalfHP      = false;
		@statsDropped            = false;
		@tookMoveDamageThisRound = false;
		@tookDamageThisRound     = false;
		@tookPhysicalHit         = false;
		@statsRaisedThisRound    = false;
		@statsLoweredThisRound   = false;
		@canRestoreIceFace       = false;
		@lastMoveUsed            = null;
		@lastMoveUsedType        = null;
		@lastRegularMoveUsed     = null;
		@lastRegularMoveTarget   = -1;
		@lastRoundMoved          = -1;
		@lastMoveFailed          = false;
		@lastRoundMoveFailed     = false;
		@movesUsed               = new List<string>();
		@turnCount               = 0;
		@effects.Attract             = -1;
		@battle.allBattlers.each do |b|   // Other battlers no longer attracted to self
			if (b.effects.Attract == @index) b.effects.Attract = -1;
		}
		@effects.BanefulBunker       = false;
		@effects.BurningBulwark      = false;
		@effects.BeakBlast           = false;
		@effects.Bide                = 0;
		@effects.BideDamage          = 0;
		@effects.BideTarget          = -1;
		@effects.BurnUp              = false;
		@effects.Charge              = 0;
		@effects.ChoiceBand          = null;
		@effects.Counter             = -1;
		@effects.CounterTarget       = -1;
		@effects.Dancer              = false;
		@effects.DefenseCurl         = false;
		@effects.DestinyBond         = false;
		@effects.DestinyBondPrevious = false;
		@effects.DestinyBondTarget   = -1;
		@effects.Disable             = 0;
		@effects.DisableMove         = null;
		@effects.DoubleShock         = false;
		@effects.Electrify           = false;
		@effects.Encore              = 0;
		@effects.EncoreMove          = null;
		@effects.Endure              = false;
		@effects.ExtraType           = null;
		@effects.FirstPledge         = null;
		@effects.FlashFire           = false;
		@effects.Flinch              = false;
		@effects.FocusPunch          = false;
		@effects.FollowMe            = 0;
		@effects.Foresight           = false;
		@effects.FuryCutter          = 0;
		@effects.GemConsumed         = null;
		@effects.Grudge              = false;
		@effects.HelpingHand         = false;
		@effects.HyperBeam           = 0;
		@effects.Illusion            = null;
		if (hasActiveAbility(Abilitys.ILLUSION)) {
			idxLastParty = @battle.LastInTeam(@index);
			if (idxLastParty >= 0 && idxLastParty != @pokemonIndex) {
				@effects.Illusion        = @battle.Party(@index)[idxLastParty];
			}
		}
		@effects.Imprison            = false;
		@effects.Instruct            = false;
		@effects.Instructed          = false;
		@effects.JawLock             = -1;
		@battle.allBattlers.each do |b|   // Other battlers no longer blocked by self
			if (b.effects.JawLock == @index) b.effects.JawLock = -1;
		}
		@effects.KingsShield         = false;
		@battle.allBattlers.each do |b|   // Other battlers lose their lock-on against self
			if (b.effects.LockOn == 0) continue;
			if (b.effects.LockOnPos != @index) continue;
			b.effects.LockOn    = 0;
			b.effects.LockOnPos = -1;
		}
		@effects.MagicBounce         = false;
		@effects.MagicCoat           = false;
		@effects.MeanLook            = -1;
		@battle.allBattlers.each do |b|   // Other battlers no longer blocked by self
			if (b.effects.MeanLook == @index) b.effects.MeanLook = -1;
		}
		@effects.MeFirst             = false;
		@effects.Metronome           = 0;
		@effects.MicleBerry          = false;
		@effects.Minimize            = false;
		@effects.MiracleEye          = false;
		@effects.MirrorCoat          = -1;
		@effects.MirrorCoatTarget    = -1;
		@effects.MoveNext            = false;
		@effects.MudSport            = false;
		@effects.Nightmare           = false;
		@effects.NoRetreat           = false;
		@effects.Obstruct            = false;
		@effects.Octolock            = -1;
		@battle.allBattlers.each do |b|   // Other battlers no longer locked by self
			if (b.effects.Octolock == @index) b.effects.Octolock = -1;
		}
		@effects.Outrage             = 0;
		@effects.ParentalBond        = 0;
		@effects.PickupItem          = null;
		@effects.PickupUse           = 0;
		@effects.Pinch               = false;
		@effects.Powder              = false;
		@effects.Prankster           = false;
		@effects.PriorityAbility     = false;
		@effects.PriorityItem        = false;
		@effects.Protect             = false;
		@effects.ProtectRate         = 1;
		@effects.Quash               = 0;
		@effects.Rage                = false;
		@effects.RagePowder          = false;
		@effects.Rollout             = 0;
		@effects.Roost               = false;
		@effects.SilkTrap            = false;
		@effects.SkyDrop             = -1;
		@battle.allBattlers.each do |b|   // Other battlers no longer Sky Dropped by self
			if (b.effects.SkyDrop == @index) b.effects.SkyDrop = -1;
		}
		@effects.SlowStart           = 0;
		@effects.SmackDown           = false;
		@effects.Snatch              = 0;
		@effects.SpikyShield         = false;
		@effects.Spotlight           = 0;
		@effects.Stockpile           = 0;
		@effects.StockpileDef        = 0;
		@effects.StockpileSpDef      = 0;
		@effects.TarShot             = false;
		@effects.Taunt               = 0;
		@effects.ThroatChop          = 0;
		@effects.Torment             = false;
		@effects.Toxic               = 0;
		@effects.Transform           = false;
		@effects.TransformSpecies    = null;
		@effects.Trapping            = 0;
		@effects.TrappingMove        = null;
		@effects.TrappingUser        = -1;
		@battle.allBattlers.each do |b|   // Other battlers no longer trapped by self
			if (b.effects.TrappingUser != @index) continue;
			b.effects.Trapping     = 0;
			b.effects.TrappingUser = -1;
		}
		@effects.Truant              = false;
		@effects.TwoTurnAttack       = null;
		@effects.Unburden            = false;
		@effects.Uproar              = 0;
		@effects.WaterSport          = false;
		@effects.WeightChange        = 0;
		@effects.Yawn                = 0;
	}

	//-----------------------------------------------------------------------------
	// Refreshing a battler's properties.
	//-----------------------------------------------------------------------------

	public void Update(fullChange = false) {
		if (!@pokemon) return;
		@pokemon.calc_stats;
		@level          = @pokemon.level;
		@hp             = @pokemon.hp;
		@totalhp        = @pokemon.totalhp;
		if (!@effects.Transform) {
			@attack       = @pokemon.attack;
			@defense      = @pokemon.defense;
			@spatk        = @pokemon.spatk;
			@spdef        = @pokemon.spdef;
			@speed        = @pokemon.speed;
			if (fullChange) {
				@types      = @pokemon.types;
				@ability_id = @pokemon.ability_id;
			}
		}
	}

	// Used to erase the battler of a Pokémon that has been caught.
	public void Reset() {
		@pokemon      = null;
		@pokemonIndex = -1;
		@hp           = 0;
		InitEffects(false);
		@participants = new List<string>();
		// Reset status
		@status       = :NONE;
		@statusCount  = 0;
		// Reset choice
		@battle.ClearChoice(@index);
	}

	// Update which Pokémon will gain Exp if this battler is defeated.
	public void UpdateParticipants() {
		if (fainted() || !@battle.opposes(@index)) return;
		foreach (var b in allOpposing) { //'allOpposing.each' do => |b|
			if (!@participants.Contains(b.pokemonIndex)) @participants.Add(b.pokemonIndex);
		}
	}
}
