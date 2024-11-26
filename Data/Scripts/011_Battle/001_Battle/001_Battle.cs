//===============================================================================
// Results of battle (see module Outcome):
//    0 - Undecided or aborted
//    1 - Player won
//    2 - Player lost
//    3 - Player or wild Pokémon ran from battle, or player forfeited the match
//    4 - Wild Pokémon was caught
//    5 - Draw
// Possible actions a battler can take in a round:
//    :None
//    :UseMove
//    :SwitchOut
//    :UseItem
//    :Call
//    :Run
//    :Shift
// NOTE: If you want to have more than 3 Pokémon on a side at once, you will need
//       to edit some code. Mainly this is to change/add coordinates for the
//       sprites, describe the relationships between Pokémon and trainers, and to
//       change messages. The methods that will need editing are as follows:
//           public partial class Battle {
//             public void setBattleMode() {
//             public void GetOwnerIndexFromBattlerIndex() {
//             public void GetOpposingIndicesInOrder() {
//             public bool nearBattlers() {
//             public void StartBattleSendOut() {
//             public void EORShiftDistantBattlers() {
//             public bool CanShift() {
//             public void EndOfRoundPhase() {
//           public partial class Battle.Scene.TargetMenu {
//             public void initialize() {
//           public partial class Battle.Scene.PokemonDataBox {
//             public void initializeDataBoxGraphic() {
//           public partial class Battle.Scene {
//             public static void BattlerPosition() {
//             public static void TrainerPosition() {
//           public partial class Game_Temp {
//             public void add_battle_rule() {
//       (There is no guarantee that this list is complete.)
//===============================================================================
public partial class Battle {
	public static partial class Outcome {
		public const int UNDECIDED = 0;
		public const int WIN       = 1;
		public const int LOSE      = 2;   // Also used when player forfeits a trainer battle
		public const int FLEE      = 3;   // Player or wild Pokémon ran away, count as a win
		public const int CATCH     = 4;   // Counts as a win
		public const int DRAW      = 5;

		public static bool decided(decision) {
			return decision != UNDECIDED;
		}

		public static bool should_black_out(decision) {
			return decision == LOSE || decision == DRAW;
		}

		public static bool success(decision) {
			return !self.should_black_out(decision);
		}
	}

	//-----------------------------------------------------------------------------

	/// <summary>Scene object for this battle</summary>
	public int scene		{ get { return _scene; } }			protected int _scene;
	public int peer		{ get { return _peer; } }			protected int _peer;
	/// <summary>Effects common to the whole of a battle</summary>
	public int field		{ get { return _field; } }			protected int _field;
	/// <summary>Effects common to each side of a battle</summary>
	public int sides		{ get { return _sides; } }			protected int _sides;
	/// <summary>Effects that apply to a battler position</summary>
	public int positions		{ get { return _positions; } }			protected int _positions;
	/// <summary>Currently active Pokémon</summary>
	public int battlers		{ get { return _battlers; } }			protected int _battlers;
	/// <summary>Array of number of battlers per side</summary>
	public int sideSizes		{ get { return _sideSizes; } }			protected int _sideSizes;
	/// <summary>Filename fragment used for background graphics</summary>
	public int backdrop		{ get { return _backdrop; } set { _backdrop = value; } }			protected int _backdrop;
	/// <summary>Filename fragment used for base graphics</summary>
	public int backdropBase		{ get { return _backdropBase; } set { _backdropBase = value; } }			protected int _backdropBase;
	/// <summary>Time of day (0=day, 1=eve, 2=night)</summary>
	public int time		{ get { return _time; } set { _time = value; } }			protected int _time;
	/// <summary>Battle surroundings (for mechanics purposes)</summary>
	public int environment		{ get { return _environment; } set { _environment = value; } }			protected int _environment;
	public int turnCount		{ get { return _turnCount; } }			protected int _turnCount;
	/// <summary>Outcome of battle</summary>
	public int decision		{ get { return _decision; } set { _decision = value; } }			protected int _decision;
	/// <summary>Player trainer (or array of trainers)</summary>
	public int player		{ get { return _player; } }			protected int _player;
	/// <summary>Opponent trainer (or array of trainers)</summary>
	public int opponent		{ get { return _opponent; } }			protected int _opponent;
	/// <summary>Items held by opponents</summary>
	public int items		{ get { return _items; } set { _items = value; } }			protected int _items;
	/// <summary>Items held by allies</summary>
	public int ally_items		{ get { return _ally_items; } set { _ally_items = value; } }			protected int _ally_items;
	/// <summary>Array of start indexes for each player-side trainer's party</summary>
	public int party1starts		{ get { return _party1starts; } set { _party1starts = value; } }			protected int _party1starts;
	/// <summary>Array of start indexes for each opponent-side trainer's party</summary>
	public int party2starts		{ get { return _party2starts; } set { _party2starts = value; } }			protected int _party2starts;
	/// <summary>Internal battle flag</summary>
	public int internalBattle		{ get { return _internalBattle; } set { _internalBattle = value; } }			protected int _internalBattle;
	/// <summary>Debug flag</summary>
	public int debug		{ get { return _debug; } set { _debug = value; } }			protected int _debug;
	/// <summary>True if player can run from battle</summary>
	public int canRun		{ get { return _canRun; } set { _canRun = value; } }			protected int _canRun;
	/// <summary>True if player won't black out if they lose</summary>
	public int canLose		{ get { return _canLose; } set { _canLose = value; } }			protected int _canLose;
	/// <summary>True if player is allowed to switch Pokémon</summary>
	public int canSwitch		{ get { return _canSwitch; } set { _canSwitch = value; } }			protected int _canSwitch;
	/// <summary>Switch/Set "battle style" option</summary>
	public int switchStyle		{ get { return _switchStyle; } set { _switchStyle = value; } }			protected int _switchStyle;
	/// <summary>"Battle Effects" option</summary>
	public int showAnims		{ get { return _showAnims; } set { _showAnims = value; } }			protected int _showAnims;
	/// <summary>Whether player's Pokémon are AI controlled</summary>
	public int controlPlayer		{ get { return _controlPlayer; } set { _controlPlayer = value; } }			protected int _controlPlayer;
	/// <summary>Whether Pokémon can gain Exp/EVs</summary>
	public int expGain		{ get { return _expGain; } set { _expGain = value; } }			protected int _expGain;
	/// <summary>Whether the player can gain/lose money</summary>
	public int moneyGain		{ get { return _moneyGain; } set { _moneyGain = value; } }			protected int _moneyGain;
	/// <summary>Whether Poké Balls cannot be thrown at all</summary>
	public int disablePokeBalls		{ get { return _disablePokeBalls; } set { _disablePokeBalls = value; } }			protected int _disablePokeBalls;
	/// <summary>Send to Boxes (0=ask, 1=don't ask, 2=must add to party)</summary>
	public int sendToBoxes		{ get { return _sendToBoxes; } set { _sendToBoxes = value; } }			protected int _sendToBoxes;
	public int rules		{ get { return _rules; } set { _rules = value; } }			protected int _rules;
	/// <summary>Choices made by each Pokémon this round</summary>
	public int choices		{ get { return _choices; } set { _choices = value; } }			protected int _choices;
	/// <summary>Battle index of each trainer's Pokémon to Mega Evolve</summary>
	public int megaEvolution		{ get { return _megaEvolution; } set { _megaEvolution = value; } }			protected int _megaEvolution;
	public int initialItems		{ get { return _initialItems; } }			protected int _initialItems;
	public int recycleItems		{ get { return _recycleItems; } }			protected int _recycleItems;
	public int belch		{ get { return _belch; } }			protected int _belch;
	public int battleBond		{ get { return _battleBond; } }			protected int _battleBond;
	public int corrosiveGas		{ get { return _corrosiveGas; } }			protected int _corrosiveGas;
	/// <summary>Whether each Pokémon was used in battle (for Burmy)</summary>
	public int usedInBattle		{ get { return _usedInBattle; } }			protected int _usedInBattle;
	/// <summary>Success states</summary>
	public int successStates		{ get { return _successStates; } }			protected int _successStates;
	/// <summary>Last move used</summary>
	public int lastMoveUsed		{ get { return _lastMoveUsed; } set { _lastMoveUsed = value; } }			protected int _lastMoveUsed;
	/// <summary>Last move user</summary>
	public int lastMoveUser		{ get { return _lastMoveUser; } set { _lastMoveUser = value; } }			protected int _lastMoveUser;
	/// <summary>ID of the first thrown Poké Ball that failed</summary>
	public int first_poke_ball		{ get { return _first_poke_ball; } set { _first_poke_ball = value; } }			protected int _first_poke_ball;
	/// <summary>Set after first_poke_ball to prevent it being set again</summary>
	public int poke_ball_failed		{ get { return _poke_ball_failed; } set { _poke_ball_failed = value; } }			protected int _poke_ball_failed;
	/// <summary>True if during the switching phase of the round</summary>
	public int switching		{ get { return _switching; } }			protected int _switching;
	/// <summary>True if Future Sight is hitting</summary>
	public int futureSight		{ get { return _futureSight; } }			protected int _futureSight;
	public int command_phase		{ get { return _command_phase; } }			protected int _command_phase;
	/// <summary>True during the end of round</summary>
	public int endOfRound		{ get { return _endOfRound; } }			protected int _endOfRound;
	/// <summary>True if Mold Breaker applies</summary>
	public int moldBreaker		{ get { return _moldBreaker; } set { _moldBreaker = value; } }			protected int _moldBreaker;
	/// <summary>The Struggle move</summary>
	public int struggle		{ get { return _struggle; } }			protected int _struggle;

	public void Random(x) {return rand(x); }

	//-----------------------------------------------------------------------------

	public void initialize(scene, p1, p2, player, opponent) {
		if (p1.length == 0) {
			Debug.LogError(new ArgumentError(_INTL("Party 1 has no Pokémon.")));
			//throw new ArgumentException(new ArgumentError(_INTL("Party 1 has no Pokémon.")));
		} else if (p2.length == 0) {
			Debug.LogError(new ArgumentError(_INTL("Party 2 has no Pokémon.")));
			//throw new ArgumentException(new ArgumentError(_INTL("Party 2 has no Pokémon.")));
		}
		@scene             = scene;
		@peer              = new Peer();
		@field             = new ActiveField();    // Whole field (gravity/rooms)
		@sides             = new {new ActiveSide(),   // Player's side
													new ActiveSide()};   // Foe's side
		@positions         = new List<string>();                 // Battler positions
		@battlers          = new List<string>();
		@sideSizes         = new {1, 1};   // Single battle, 1v1
		@backdrop          = "";
		@backdropBase      = null;
		@time              = 0;
		@environment       = :None;   // e.g. Tall grass, cave, still water
		@turnCount         = 0;
		@decision          = Outcome.UNDECIDED;
		@caughtPokemon     = new List<string>();
		if (!player.null() && !player.Length > 0) player   = [player];
		if (!opponent.null() && !opponent.Length > 0) opponent = [opponent];
		@player            = player;     // Array of Player/NPCTrainer objects, or null
		@opponent          = opponent;   // Array of NPCTrainer objects, or null
		@items             = null;
		@ally_items        = null;        // Array of items held by ally. This is just used for Mega Evolution for now.
		@party1            = p1;
		@party2            = p2;
		@party1order       = new Array(@party1.length, i => { i; });
		@party2order       = new Array(@party2.length, i => { i; });
		@party1starts      = [0];
		@party2starts      = [0];
		@internalBattle    = true;
		@debug             = false;
		@canRun            = true;
		@canLose           = false;
		@canSwitch         = true;
		@switchStyle       = true;
		@showAnims         = true;
		@controlPlayer     = false;
		@expGain           = true;
		@moneyGain         = true;
		@disablePokeBalls  = false;
		@sendToBoxes       = 1;
		@rules             = new List<string>();
		@priority          = new List<string>();
		@priorityTrickRoom = false;
		@choices           = new List<string>();
		@megaEvolution     = new {
			[-1] * (@player ? @player.length : 1),
			[-1] * (@opponent ? @opponent.length : 1);
		}
		@initialItems      = new {
			new Array(@party1.length, i => { (@party1[i]) ? @party1[i].item_id : null; });,
			new Array(@party2.length, i => { (@party2[i]) ? @party2[i].item_id : null; });
		}
		@recycleItems      = new {new Array(@party1.length, null),   new Array(@party2.length, null)};
		@belch             = new {new Array(@party1.length, false), new Array(@party2.length, false)};
		@battleBond        = new {new Array(@party1.length, false), new Array(@party2.length, false)};
		@corrosiveGas      = new {new Array(@party1.length, false), new Array(@party2.length, false)};
		@usedInBattle      = new {new Array(@party1.length, false), new Array(@party2.length, false)};
		@successStates     = new List<string>();
		@lastMoveUsed      = null;
		@lastMoveUser      = -1;
		@switching         = false;
		@futureSight       = false;
		@command_phase     = false;
		@endOfRound        = false;
		@moldBreaker       = false;
		@runCommand        = 0;
		@nextPickupUse     = 0;
		@struggle          = new Move.Struggle(self, null);
		@mega_rings        = new List<string>();
		GameData.Item.each(item => { if (item.has_flag("MegaRing")) @mega_rings.Add(item.id); });
		@battleAI          = new AI(self);
	}

	public bool decided() {
		return Outcome.decided(@decision);
	}

	//-----------------------------------------------------------------------------
	// Information about the type and size of the battle.
	//-----------------------------------------------------------------------------

	public bool wildBattle() {    return @opponent.null();  }
	public bool trainerBattle() { return !@opponent.null(); }

	// Sets the number of battler slots on each side of the field independently.
	// For "1v2" names, the first number is for the player's side and the second
	// number is for the opposing side.
	public void setBattleMode(mode) {
		@sideSizes =;
			switch (mode) {
				case "triple": case "3v3": new {3, 3}; break;
				case "3v2":            new {3, 2}; break;
				case "3v1":            new {3, 1}; break;
				case "2v3":            new {2, 3}; break;
				case "double": case "2v2": new {2, 2}; break;
				case "2v1":            new {2, 1}; break;
				case "1v3":            new {1, 3}; break;
				case "1v2":            new {1, 2}; break;
				default                      new {1, 1}; break;   // Single, 1v1 (default)
			}
	}

	public bool singleBattle() {
		return SideSize(0) == 1 && SideSize(1) == 1;
	}

	public void SideSize(index) {
		return @sideSizes[index % 2];
	}

	public void maxBattlerIndex() {
		return (SideSize(0) > SideSize(1)) ? (SideSize(0) - 1) * 2 : (SideSize(1) * 2) - 1;
	}

	//-----------------------------------------------------------------------------
	// Trainers and owner-related methods.
	//-----------------------------------------------------------------------------

	public int Player { get { return @player[0]; } }

	// Given a battler index, returns the index within @player/@opponent of the
	// trainer that controls that battler index.
	// NOTE: You shouldn't ever have more trainers on a side than there are battler
	//       positions on that side. This method doesn't account for if you do.
	public void GetOwnerIndexFromBattlerIndex(idxBattler) {
		trainer = (opposes(idxBattler)) ? @opponent : @player;
		if (!trainer) return 0;
		switch (trainer.length) {
			case 2:
				n = SideSize(idxBattler % 2);
				if (n == 3) return new {0, 0, 1}[idxBattler / 2];
				return idxBattler / 2;   // Same as new {0,1}[idxBattler/2], i.e. 2 battler slots
			case 3:
				return idxBattler / 2;
		}
		return 0;
	}

	public void GetOwnerFromBattlerIndex(idxBattler) {
		idxTrainer = GetOwnerIndexFromBattlerIndex(idxBattler);
		trainer = (opposes(idxBattler)) ? @opponent : @player;
		return (trainer.null()) ? null : trainer[idxTrainer];
	}

	public void GetOwnerIndexFromPartyIndex(idxBattler, idxParty) {
		ret = -1;
		PartyStarts(idxBattler).each_with_index do |start, i|
			if (start > idxParty) break;
			ret = i;
		}
		return ret;
	}

	// Only used for the purpose of an error message when one trainer tries to
	// switch another trainer's Pokémon.
	public void GetOwnerFromPartyIndex(idxBattler, idxParty) {
		idxTrainer = GetOwnerIndexFromPartyIndex(idxBattler, idxParty);
		trainer = (opposes(idxBattler)) ? @opponent : @player;
		return (trainer.null()) ? null : trainer[idxTrainer];
	}

	public void GetOwnerName(idxBattler) {
		idxTrainer = GetOwnerIndexFromBattlerIndex(idxBattler);
		if (opposes(idxBattler)) return @opponent[idxTrainer].full_name;   // Opponent
		if (idxTrainer > 0) return @player[idxTrainer].full_name;   // Ally trainer
		return @player[idxTrainer].name;   // Player
	}

	public void GetOwnerItems(idxBattler) {
		if (opposes(idxBattler)) {
			if (!@items) return [];
			return @items[GetOwnerIndexFromBattlerIndex(idxBattler)];
		}
		if (!@ally_items) return [];
		return @ally_items[GetOwnerIndexFromBattlerIndex(idxBattler)];
	}

	// Returns whether the battler in position idxBattler is owned by the same
	// trainer that owns the Pokémon in party slot idxParty. This assumes that
	// both the battler position and the party slot are from the same side.
	public bool IsOwner(idxBattler, idxParty) {
		idxTrainer1 = GetOwnerIndexFromBattlerIndex(idxBattler);
		idxTrainer2 = GetOwnerIndexFromPartyIndex(idxBattler, idxParty);
		return idxTrainer1 == idxTrainer2;
	}

	public bool OwnedByPlayer(idxBattler) {
		if (opposes(idxBattler)) return false;
		return GetOwnerIndexFromBattlerIndex(idxBattler) == 0;
	}

	// Returns the number of Pokémon positions controlled by the given trainerIndex
	// on the given side of battle.
	public void NumPositions(side, idxTrainer) {
		ret = 0;
		for (int i = SideSize(side); i < SideSize(side); i++) { //for 'SideSize(side)' times do => |i|
			t = GetOwnerIndexFromBattlerIndex((i * 2) + side);
			if (t != idxTrainer) continue;
			ret += 1;
		}
		return ret;
	}

	//-----------------------------------------------------------------------------
	// Get party information (counts all teams on the same side).
	//-----------------------------------------------------------------------------

	public void Party(idxBattler) {
		return (opposes(idxBattler)) ? @party2 : @party1;
	}

	public void OpposingParty(idxBattler) {
		return (opposes(idxBattler)) ? @party1 : @party2;
	}

	public void PartyOrder(idxBattler) {
		return (opposes(idxBattler)) ? @party2order : @party1order;
	}

	public void PartyStarts(idxBattler) {
		return (opposes(idxBattler)) ? @party2starts : @party1starts;
	}

	// Returns the player's team in its display order. Used when showing the party
	// screen.
	public void PlayerDisplayParty(idxBattler = 0) {
		partyOrders = PartyOrder(idxBattler);
		idxStart, _idxEnd = TeamIndexRangeFromBattlerIndex(idxBattler);
		ret = new List<string>();
		eachInTeamFromBattlerIndex(idxBattler, (pkmn, i) => { ret[partyOrders[i] - idxStart] = pkmn; });
		return ret;
	}

	public void AbleCount(idxBattler = 0) {
		party = Party(idxBattler);
		count = 0;
		party.each(pkmn => { if (pkmn&.able()) count += 1; });
		return count;
	}

	public void AbleNonActiveCount(idxBattler = 0) {
		party = Party(idxBattler);
		inBattleIndices = allSameSideBattlers(idxBattler).map(b => b.pokemonIndex);
		count = 0;
		party.each_with_index do |pkmn, idxParty|
			if (!pkmn || !pkmn.able()) continue;
			if (inBattleIndices.Contains(idxParty)) continue;
			count += 1;
		}
		return count;
	}

	public bool AllFainted(idxBattler = 0) {
		return AbleCount(idxBattler) == 0;
	}

	public void TeamAbleNonActiveCount(idxBattler = 0) {
		inBattleIndices = allSameSideBattlers(idxBattler).map(b => b.pokemonIndex);
		count = 0;
		eachInTeamFromBattlerIndex(idxBattler) do |pkmn, i|
			if (!pkmn || !pkmn.able()) continue;
			if (inBattleIndices.Contains(i)) continue;
			count += 1;
		}
		return count;
	}

	// For the given side of the field (0=player's, 1=opponent's), returns an array
	// containing the number of able Pokémon in each team.
	public void AbleTeamCounts(side) {
		party = Party(side);
		partyStarts = PartyStarts(side);
		ret = new List<string>();
		idxTeam = -1;
		nextStart = 0;
		party.each_with_index do |pkmn, i|
			if (i >= nextStart) {
				idxTeam += 1;
				nextStart = (idxTeam < partyStarts.length - 1) ? partyStarts[idxTeam + 1] : party.length;
			}
			if (!pkmn || !pkmn.able()) continue;
			if (!ret[idxTeam]) ret[idxTeam] = 0;
			ret[idxTeam] += 1;
		}
		return ret;
	}

	//-----------------------------------------------------------------------------
	// Get team information (a team is only the Pokémon owned by a particular
	// trainer).
	//-----------------------------------------------------------------------------

	public void TeamIndexRangeFromBattlerIndex(idxBattler) {
		partyStarts = PartyStarts(idxBattler);
		idxTrainer = GetOwnerIndexFromBattlerIndex(idxBattler);
		idxPartyStart = partyStarts[idxTrainer];
		idxPartyEnd   = (idxTrainer < partyStarts.length - 1) ? partyStarts[idxTrainer + 1] : Party(idxBattler).length;
		return idxPartyStart, idxPartyEnd;
	}

	public void TeamLengthFromBattlerIndex(idxBattler) {
		idxPartyStart, idxPartyEnd = TeamIndexRangeFromBattlerIndex(idxBattler);
		return idxPartyEnd - idxPartyStart;
	}

	public void eachInTeamFromBattlerIndex(idxBattler) {
		party = Party(idxBattler);
		idxPartyStart, idxPartyEnd = TeamIndexRangeFromBattlerIndex(idxBattler);
		party.each_with_index((pkmn, i) => { if (pkmn && i >= idxPartyStart && i < idxPartyEnd) yield pkmn, i; });
	}

	public void eachInTeam(side, idxTrainer) {
		party       = Party(side);
		partyStarts = PartyStarts(side);
		idxPartyStart = partyStarts[idxTrainer];
		idxPartyEnd   = (idxTrainer < partyStarts.length - 1) ? partyStarts[idxTrainer + 1] : party.length;
		party.each_with_index((pkmn, i) => { if (pkmn && i >= idxPartyStart && i < idxPartyEnd) yield pkmn, i; });
	}

	// Used for Illusion.
	// NOTE: This cares about the temporary rearranged order of the team. That is,
	//       if (you do some switching, the last Pokémon in the team could change) {
	//       and the Illusion could be a different Pokémon.
	public void LastInTeam(idxBattler) {
		party       = Party(idxBattler);
		partyOrders = PartyOrder(idxBattler);
		idxPartyStart, idxPartyEnd = TeamIndexRangeFromBattlerIndex(idxBattler);
		ret = -1;
		party.each_with_index do |pkmn, i|
			if (i < idxPartyStart || i >= idxPartyEnd) continue;   // Check the team only
			if (!pkmn || !pkmn.able()) continue;   // Can't copy a non-fainted Pokémon or egg
			if (ret < 0 || partyOrders[i] > partyOrders[ret]) ret = i;
		}
		return ret;
	}

	// Used to calculate money gained/lost after winning/losing a battle.
	public void MaxLevelInTeam(side, idxTrainer) {
		ret = 1;
		eachInTeam(side, idxTrainer) do |pkmn, _i|
			if (pkmn.level > ret) ret = pkmn.level;
		}
		return ret;
	}

	//-----------------------------------------------------------------------------
	// Iterate through battlers.
	//-----------------------------------------------------------------------------

	// Unused
	public void eachBattler() {
		@battlers.each(b => { if (b && !b.fainted()) yield b; });
	}

	public void allBattlers() {
		return @battlers.select(b => b && !b.fainted());
	}

	// Unused
	public void eachSameSideBattler(idxBattler = 0) {
		if (idxBattler.respond_to("index")) idxBattler = idxBattler.index;
		@battlers.each(b => { if (b && !b.fainted() && !b.opposes(idxBattler)) yield b; });
	}

	public void allSameSideBattlers(idxBattler = 0) {
		if (idxBattler.respond_to("index")) idxBattler = idxBattler.index;
		return @battlers.select(b => b && !b.fainted() && !b.opposes(idxBattler));
	}

	// Unused
	public void eachOtherSideBattler(idxBattler = 0) {
		if (idxBattler.respond_to("index")) idxBattler = idxBattler.index;
		@battlers.each(b => { if (b && !b.fainted() && b.opposes(idxBattler)) yield b; });
	}

	public void allOtherSideBattlers(idxBattler = 0) {
		if (idxBattler.respond_to("index")) idxBattler = idxBattler.index;
		return @battlers.select(b => b && !b.fainted() && b.opposes(idxBattler));
	}

	public void SideBattlerCount(idxBattler = 0) {
		return allSameSideBattlers(idxBattler).length;
	}

	public void OpposingBattlerCount(idxBattler = 0) {
		return allOtherSideBattlers(idxBattler).length;
	}

	// This method only counts the player's Pokémon, not a partner trainer's.
	public void PlayerBattlerCount() {
		return allSameSideBattlers.select(b => b.OwnedByPlayer()).length;
	}

	public void CheckGlobalAbility(abil, check_mold_breaker = false) {
		foreach (var b in allBattlers) { //'allBattlers.each' do => |b|
			if (b.hasActiveAbility(abil) && (!check_mold_breaker || !b.beingMoldBroken())) return b;
		}
		return null;
	}

	public void CheckOpposingAbility(abil, idxBattler = 0, nearOnly = false) {
		foreach (var b in allOtherSideBattlers(idxBattler)) { //'allOtherSideBattlers(idxBattler).each' do => |b|
			if (nearOnly && !b.near(idxBattler)) continue;
			if (b.hasActiveAbility(abil)) return b;
		}
		return null;
	}

	// Returns an array containing the IDs of all active abilities.
	public void AllActiveAbilities() {
		ret = new List<string>();
		allBattlers.each(b => { if (b.abilityActive()) ret.Add(b.ability_id); });
		return ret;
	}

	// Given a battler index, and using battle side sizes, returns an array of
	// battler indices from the opposing side that are in order of most "opposite".
	// Used when choosing a target and pressing up/down to move the cursor to the
	// opposite side, and also when deciding which target to select first for some
	// moves.
	public void GetOpposingIndicesInOrder(idxBattler) {
		switch (SideSize(0)) {
			case 1:
				switch (SideSize(1)) {
					case 1:   // 1v1 single
						if (opposes(idxBattler)) return [0];
						return [1];
					case 2:   // 1v2
						if (opposes(idxBattler)) return [0];
						return new {3, 1};
					case 3:   // 1v3
						if (opposes(idxBattler)) return [0];
						return new {3, 5, 1};
				}
				break;
			case 2:
				switch (SideSize(1)) {
					case 1:   // 2v1
						if (opposes(idxBattler)) return new {0, 2};
						return [1];
					case 2:   // 2v2 double
						return new {new {3, 1}, new {2, 0}, new {1, 3}, new {0, 2}}[idxBattler];
					case 3:   // 2v3
						if (idxBattler < 3) return new {new {5, 3, 1}, new {2, 0}, new {3, 1, 5}}[idxBattler];
						return new {0, 2};
				}
				break;
			case 3:
				switch (SideSize(1)) {
					case 1:   // 3v1
						if (opposes(idxBattler)) return new {2, 0, 4};
						return [1];
					case 2:   // 3v2
						return new {new {3, 1}, new {2, 4, 0}, new {3, 1}, new {2, 0, 4}, new {1, 3}}[idxBattler];
					case 3:   // 3v3 triple
						return new {new {5, 3, 1}, new {4, 2, 0}, new {3, 5, 1}, new {2, 0, 4}, new {1, 3, 5}, new {0, 2, 4}}[idxBattler];
				}
				break;
		}
		return [idxBattler];
	}

	//-----------------------------------------------------------------------------
	// Comparing the positions of two battlers.
	//-----------------------------------------------------------------------------

	public bool opposes(idxBattler1, idxBattler2 = 0) {
		if (idxBattler1.respond_to("index")) idxBattler1 = idxBattler1.index;
		if (idxBattler2.respond_to("index")) idxBattler2 = idxBattler2.index;
		return (idxBattler1 & 1) != (idxBattler2 & 1);
	}

	public bool nearBattlers(idxBattler1, idxBattler2) {
		if (idxBattler1 == idxBattler2) return false;
		if (SideSize(0) <= 2 && SideSize(1) <= 2) return true;
		// Get all pairs of battler positions that are not close to each other
		pairsArray = new {new {0, 4}, new {1, 5}}   // Covers 3v1 and 1v3
		switch (SideSize(0)) {
			case 3:
				switch (SideSize(1)) {
					case 3:   // 3v3 (triple)
						pairsArray.Add(new {0, 1});
						pairsArray.Add(new {4, 5});
						break;
					case 2:   // 3v2
						pairsArray.Add(new {0, 1});
						pairsArray.Add(new {3, 4});
						break;
				}
				break;
			case 2:       // 2v3
				pairsArray.Add(new {0, 1});
				pairsArray.Add(new {2, 5});
				break;
		}
		// See if any pair matches the two battlers being assessed
		foreach (var pair in pairsArray) { //'pairsArray.each' do => |pair|
			if (pair.Contains(idxBattler1) && pair.Contains(idxBattler2)) return false;
		}
		return true;
	}

	//-----------------------------------------------------------------------------
	// Altering a party or rearranging battlers.
	//-----------------------------------------------------------------------------

	public void RemoveFromParty(idxBattler, idxParty) {
		party = Party(idxBattler);
		// Erase the Pokémon from the party
		party[idxParty] = null;
		// Rearrange the display order of the team to place the erased Pokémon last
		// in it (to avoid gaps)
		partyOrders = PartyOrder(idxBattler);
		partyStarts = PartyStarts(idxBattler);
		idxTrainer = GetOwnerIndexFromPartyIndex(idxBattler, idxParty);
		idxPartyStart = partyStarts[idxTrainer];
		idxPartyEnd   = (idxTrainer < partyStarts.length - 1) ? partyStarts[idxTrainer + 1] : party.length;
		origPartyPos = partyOrders[idxParty];   // Position of erased Pokémon initially
		partyOrders[idxParty] = idxPartyEnd;   // Put erased Pokémon last in the team
		party.each_with_index do |_pkmn, i|
			if (i < idxPartyStart || i >= idxPartyEnd) continue;   // Only check the team
			if (partyOrders[i] < origPartyPos) continue;   // Appeared before erased Pokémon
			partyOrders[i] -= 1;   // Appeared after erased Pokémon; bump it up by 1
		}
	}

	public void SwapBattlers(idxA, idxB) {
		if (!@battlers[idxA] || !@battlers[idxB]) return false;
		// Can't swap if battlers aren't owned by the same trainer
		if (opposes(idxA, idxB)) return false;
		if (GetOwnerIndexFromBattlerIndex(idxA) != GetOwnerIndexFromBattlerIndex(idxB)) return false;
		@battlers[idxA],       @battlers[idxB]       = @battlers[idxB],       @battlers[idxA];
		@battlers[idxA].index, @battlers[idxB].index = @battlers[idxB].index, @battlers[idxA].index;
		@choices[idxA],        @choices[idxB]        = @choices[idxB],        @choices[idxA];
		@scene.SwapBattlerSprites(idxA, idxB);
		// Swap the target of any battlers' effects that point at either of the
		// swapped battlers, to ensure they still point at the correct target
		// NOTE: LeechSeed is not swapped, because drained HP goes to whichever
		//       Pokémon is in the position that Leech Seed was used from.
		// NOTE: PerishSongUser doesn't need to change, as it's only used to
		//       determine which side the Perish Song user was on, and a battler
		//       can't change sides.
		effectsToSwap = new {Effects.Attract,
										Effects.BideTarget,
										Effects.CounterTarget,
										Effects.JawLock,
										Effects.LockOnPos,
										Effects.MeanLook,
										Effects.MirrorCoatTarget,
										Effects.Octolock,
										Effects.SkyDrop,
										Effects.TrappingUser};
		foreach (var b in allBattlers) { //'allBattlers.each' do => |b|
			foreach (var i in effectsToSwap) { //'effectsToSwap.each' do => |i|
				if (b.effects[i] != idxA && b.effects[i] != idxB) continue;
				b.effects[i] = (b.effects[i] == idxA) ? idxB : idxA;
			}
		}
		return true;
	}

	//-----------------------------------------------------------------------------
	//
	//-----------------------------------------------------------------------------

	// Returns the battler representing the Pokémon at index idxParty in its party,
	// on the same side as a battler with battler index of idxBattlerOther.
	public void FindBattler(idxParty, idxBattlerOther = 0) {
		allSameSideBattlers(idxBattlerOther).each(b => { if (b.pokemonIndex == idxParty) return b; });
		return null;
	}

	// Only used for Wish, as the Wishing Pokémon will no longer be in battle.
	public void ThisEx(idxBattler, idxParty) {
		party = Party(idxBattler);
		if (opposes(idxBattler)) {
			if (trainerBattle()) return _INTL("The opposing {1}", party[idxParty].name);
			return _INTL("The wild {1}", party[idxParty].name);
		}
		if (!OwnedByPlayer(idxBattler)) return _INTL("The ally {1}", party[idxParty].name);
		return party[idxParty].name;
	}

	public void SetSeen(battler) {
		if (!battler || !@internalBattle) return;
		if (battler.is_a(Battler)) {
			Player.pokedex.register(battler.displaySpecies, battler.displayGender,
																battler.displayForm, battler.shiny());
		} else {
			Player.pokedex.register(battler);
		}
	}

	public void SetCaught(battler) {
		if (!battler || !@internalBattle) return;
		if (battler.is_a(Battler)) {
			Player.pokedex.register_caught(battler.displaySpecies);
		} else {
			Player.pokedex.register_caught(battler.species);
		}
	}

	public void SetDefeated(battler) {
		if (!battler || !@internalBattle) return;
		if (battler.is_a(Battler)) {
			Player.pokedex.register_defeated(battler.displaySpecies);
		} else {
			Player.pokedex.register_defeated(battler.species);
		}
	}

	public void nextPickupUse() {
		@nextPickupUse += 1;
		return @nextPickupUse;
	}

	//-----------------------------------------------------------------------------
	// Weather.
	//-----------------------------------------------------------------------------

	public int defaultWeather { set {
		@field.defaultWeather  = value;
		@field.weather         = value;
		@field.weatherDuration = -1;
		}
	}

	// Returns the effective weather (note that weather effects can be negated)
	public void Weather() {
		if (allBattlers.any(b => b.hasActiveAbility(new {:CLOUDNINE, :AIRLOCK}))) return :None;
		return @field.weather;
	}

	// Used for causing weather by a move or by an ability.
	public void StartWeather(user, newWeather, fixedDuration = false, showAnim = true) {
		if (@field.weather == newWeather) return;
		@field.weather = newWeather;
		duration = (fixedDuration) ? 5 : -1;
		if (duration > 0 && user && user.itemActive()) {
			duration = Battle.ItemEffects.triggerWeatherExtender(user.item, @field.weather,
																														duration, user, self);
		}
		@field.weatherDuration = duration;
		weather_data = GameData.BattleWeather.try_get(@field.weather);
		if (showAnim && weather_data) CommonAnimation(weather_data.animation);
		if (user) HideAbilitySplash(user);
		switch (@field.weather) {
			case :Sun:          Display(_INTL("The sunlight turned harsh!")); break;
			case :Rain:         Display(_INTL("It started to rain!")); break;
			case :Sandstorm:    Display(_INTL("A sandstorm brewed!")); break;
			case :Hail:         Display(_INTL("It started to hail!")); break;
			case :Snowstorm:    Display(_INTL("It started to snow!")); break;
			case :HarshSun:     Display(_INTL("The sunlight turned extremely harsh!")); break;
			case :HeavyRain:    Display(_INTL("A heavy rain began to fall!")); break;
			case :StrongWinds:  Display(_INTL("Mysterious strong winds are protecting Flying-type Pokémon!")); break;
			case :ShadowSky:    Display(_INTL("A shadow sky appeared!")); break;
		}
		// Check for end of primordial weather, and weather-triggered form changes
		allBattlers.each(b => b.CheckFormOnWeatherChange);
		EndPrimordialWeather;
	}

	public void EndPrimordialWeather() {
		if (@field.weather == @field.defaultWeather) return;
		oldWeather = @field.weather;
		// End Primordial Sea, Desolate Land, Delta Stream
		switch (@field.weather) {
			case :HarshSun:
				if (!CheckGlobalAbility(Abilities.DESOLATELAND)) {
					@field.weather = weathers.None;
					Display("The harsh sunlight faded!");
				}
				break;
			case :HeavyRain:
				if (!CheckGlobalAbility(Abilities.PRIMORDIALSEA)) {
					@field.weather = weathers.None;
					Display("The heavy rain has lifted!");
				}
				break;
			case :StrongWinds:
				if (!CheckGlobalAbility(Abilities.DELTASTREAM)) {
					@field.weather = weathers.None;
					Display("The mysterious air current has dissipated!");
				}
				break;
		}
		if (@field.weather != oldWeather) {
			// Check for form changes caused by the weather changing
			allBattlers.each(b => b.CheckFormOnWeatherChange);
			// Start up the default weather
			if (@field.defaultWeather != Weathers.None) StartWeather(null, @field.defaultWeather);
		}
	}

	public void StartWeatherAbility(new_weather, battler, ignore_primal = false) {
		if (!ignore_primal && new []{:HarshSun, :HeavyRain, :StrongWinds}.Contains(@field.weather)) return;
		if (@field.weather == new_weather) return;
		ShowAbilitySplash(battler);
		if (!Scene.USE_ABILITY_SPLASH) {
			Display(_INTL("{1}'s {2} activated!", battler.ToString(), battler.abilityName));
		}
		fixed_duration = false;
		if (Settings.FIXED_DURATION_WEATHER_FROM_ABILITY &&
														!new []{:HarshSun, :HeavyRain, :StrongWinds}.Contains(new_weather)) fixed_duration = true;
		StartWeather(battler, new_weather, fixed_duration);
		// NOTE: The ability splash is hidden again in def StartWeather.
	}

	//-----------------------------------------------------------------------------
	// Terrain.
	//-----------------------------------------------------------------------------

	public int defaultTerrain { set {
		@field.defaultTerrain  = value;
		@field.terrain         = value;
		@field.terrainDuration = -1;
		}
	}

	public void StartTerrain(user, newTerrain, fixedDuration = true) {
		if (@field.terrain == newTerrain) return;
		@field.terrain = newTerrain;
		duration = (fixedDuration) ? 5 : -1;
		if (duration > 0 && user && user.itemActive()) {
			duration = Battle.ItemEffects.triggerTerrainExtender(user.item, newTerrain,
																														duration, user, self);
		}
		@field.terrainDuration = duration;
		terrain_data = GameData.BattleTerrain.try_get(@field.terrain);
		if (terrain_data) CommonAnimation(terrain_data.animation);
		if (user) HideAbilitySplash(user);
		switch (@field.terrain) {
			case :Electric:
				Display(_INTL("An electric current runs across the battlefield!"));
				break;
			case :Grassy:
				Display(_INTL("Grass grew to cover the battlefield!"));
				break;
			case :Misty:
				Display(_INTL("Mist swirled about the battlefield!"));
				break;
			case :Psychic:
				Display(_INTL("The battlefield got weird!"));
				break;
		}
		// Check for abilities/items that trigger upon the terrain changing
		allBattlers.each(b => b.AbilityOnTerrainChange);
		allBattlers.each(b => b.ItemTerrainStatBoostCheck);
	}

	//-----------------------------------------------------------------------------
	// Messages and animations.
	//-----------------------------------------------------------------------------

	public void Display(msg, Action block = null) {
		@scene.DisplayMessage(msg, &block);
	}

	public void DisplayBrief(msg) {
		@scene.DisplayMessage(msg, true);
	}

	public void DisplayPaused(msg, Action block = null) {
		@scene.DisplayPausedMessage(msg, &block);
	}

	public void DisplayConfirm(msg) {
		return @scene.DisplayConfirmMessage(msg);
	}

	// defaultValue of -1 means "can't cancel". If it's 0 or greater, returns that
	// value when pressing the "Back" button.
	public void ShowCommands(msg, commands, defaultValue = -1) {
		return @scene.ShowCommands(msg, commands, defaultValue);
	}

	public void Animation(move, user, targets, hitNum = 0) {
		if (@showAnims) @scene.Animation(move, user, targets, hitNum);
	}

	public void CommonAnimation(name, user = null, targets = null) {
		if (@showAnims) @scene.CommonAnimation(name, user, targets);
	}

	public void ShowAbilitySplash(battler, delay = false, logTrigger = true) {
		if (logTrigger) Debug.Log($"[Ability triggered] {battler.ToString()}'s {battler.abilityName}");
		if (!Scene.USE_ABILITY_SPLASH) return;
		@scene.ShowAbilitySplash(battler);
		if (delay) {
			timer_start = System.uptime;
			until System.uptime - timer_start >= 1;   // 1 second
				@scene.Update;
			}
		}
	}

	public void HideAbilitySplash(battler) {
		if (!Scene.USE_ABILITY_SPLASH) return;
		@scene.HideAbilitySplash(battler);
	}

	public void ReplaceAbilitySplash(battler) {
		if (!Scene.USE_ABILITY_SPLASH) return;
		@scene.ReplaceAbilitySplash(battler);
	}
}
