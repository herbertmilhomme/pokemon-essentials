//===============================================================================
// Simple battler class for the wild Pokémon in a Safari Zone battle.
//===============================================================================
public partial class Battle.FakeBattler {
	public int battle		{ get { return _battle; } }			protected int _battle;
	public int index		{ get { return _index; } }			protected int _index;
	public int pokemon		{ get { return _pokemon; } }			protected int _pokemon;
	public int owned		{ get { return _owned; } }			protected int _owned;

	public void initialize(battle, index) {
		@battle  = battle;
		@pokemon = battle.party2[0];
		@index   = index;
	}

	public int pokemonIndex   { get { return 0;                     } }
	public int species        { get { return @pokemon.species;      } }
	public int gender         { get { return @pokemon.gender;       } }
	public int status         { get { return @pokemon.status;       } }
	public int hp             { get { return @pokemon.hp;           } }
	public int level          { get { return @pokemon.level;        } }
	public int name           { get { return @pokemon.name;         } }
	public int totalhp        { get { return @pokemon.totalhp;      } }
	public int displayGender  { get { return @pokemon.gender;       } }
	public bool shiny() {         return @pokemon.shiny();       }
	public bool super_shiny() {   return @pokemon.super_shiny(); }

	public bool isSpecies(check_species) {
		return @pokemon&.isSpecies(check_species);
	}

	public bool fainted() {       return false; }
	public bool shadowPokemon() { return false; }
	public bool hasMega() {       return false; }
	public bool mega() {          return false; }
	public bool hasPrimal() {     return false; }
	public bool primal() {        return false; }
	public int captured       { get { return false; } }
	public int captured { set {  } }

	public bool owned() {
		return Game.GameData.player.owned(pokemon.species);
	}

	public void This(lowerCase = false) {
		return (lowerCase) ? _INTL("the wild {1}", name) : _INTL("The wild {1}", name);
	}

	public bool opposes(i) {
		if (i.is_a(Battle.FakeBattler)) i = i.index;
		return (@index & 1) != (i & 1);
	}

	public void Reset() { }
}

//===============================================================================
// Data box for safari battles.
//===============================================================================
public partial class Battle.Scene.SafariDataBox : Sprite {
	public int selected		{ get { return _selected; } set { _selected = value; } }			protected int _selected;

	public override void initialize(battle, viewport = null) {
		base.initialize(viewport);
		@selected    = 0;
		@battle      = battle;
		@databox     = new AnimatedBitmap(_INTL("Graphics/UI/Battle/databox_safari"));
		self.x       = Graphics.width - 232;
		self.y       = Graphics.height - 184;
		@contents    = new Bitmap(@databox.width, @databox.height);
		self.bitmap  = @contents;
		self.visible = false;
		self.z       = 50;
		SetSystemFont(self.bitmap);
		refresh;
	}

	public void refresh() {
		self.bitmap.clear;
		self.bitmap.blt(0, 0, @databox.bitmap, new Rect(0, 0, @databox.width, @databox.height));
		base   = new Color(72, 72, 72);
		shadow = new Color(184, 184, 184);
		textpos = new List<string>();
		textpos.Add(new {_INTL("Safari Balls"), 30, 14, :left, base, shadow});
		textpos.Add(new {_INTL("Left: {1}", @battle.ballCount), 30, 44, :left, base, shadow});
		DrawTextPositions(self.bitmap, textpos);
	}
}

//===============================================================================
// Shows the player throwing bait at a wild Pokémon in a Safari battle.
//===============================================================================
public partial class Battle.Scene.Animation.ThrowBait : Battle.Scene.Animation {
	include Battle.Scene.Animation.BallAnimationMixin;

	public override void initialize(sprites, viewport, battler) {
		@battler = battler;
		@trainer = battler.battle.GetOwnerFromBattlerIndex(battler.index);
		base.initialize(sprites, viewport);
	}

	public void createProcesses() {
		// Calculate start and end coordinates for battler sprite movement
		batSprite = @sprites[$"pokemon_{@battler.index}"];
		traSprite = @sprites["player_1"];
		ballPos = Battle.Scene.BattlerPosition(@battler.index, batSprite.sideSize);
		ballStartX = traSprite.x;
		ballStartY = traSprite.y - (traSprite.bitmap.height / 2);
		ballMidX   = 0;   // Unused in arc calculation
		ballMidY   = 122;
		ballEndX   = ballPos[0] - 40;
		ballEndY   = ballPos[1] - 4;
		// Set up trainer sprite
		trainer = addSprite(traSprite, PictureOrigin.BOTTOM);
		// Set up bait sprite
		ball = addNewSprite(ballStartX, ballStartY,
												"Graphics/Battle animations/safari_bait", PictureOrigin.CENTER);
		ball.setZ(0, batSprite.z + 1);
		// Trainer animation
		if (traSprite.bitmap.width >= traSprite.bitmap.height * 2) {
			ballStartX, ballStartY = trainerThrowingFrames(ball, trainer, traSprite);
		}
		delay = ball.totalDuration;   // 0 or 7
		// Bait arc animation
		ball.setSE(delay, "Battle throw");
		createBallTrajectory(ball, delay, 12,
												ballStartX, ballStartY, ballMidX, ballMidY, ballEndX, ballEndY);
		ball.setZ(9, batSprite.z + 1);
		delay = ball.totalDuration;
		ball.moveOpacity(delay + 8, 2, 0);
		ball.setVisible(delay + 10, false);
		// Set up battler sprite
		battler = addSprite(batSprite, PictureOrigin.BOTTOM);
		// Show Pokémon jumping before eating the bait
		delay = ball.totalDuration + 3;
		2.times do;
			battler.setSE(delay, "player jump");
			battler.moveDelta(delay, 3, 0, -16);
			battler.moveDelta(delay + 4, 3, 0, 16);
			delay = battler.totalDuration + 1;
		}
		// Show Pokémon eating the bait
		delay = battler.totalDuration + 3;
		2.times do;
			battler.moveAngle(delay, 7, 5);
			battler.moveDelta(delay, 7, 0, 6);
			battler.moveAngle(delay + 7, 7, 0);
			battler.moveDelta(delay + 7, 7, 0, -6);
			delay = battler.totalDuration;
		}
	}
}

//===============================================================================
// Shows the player throwing a rock at a wild Pokémon in a Safari battle.
//===============================================================================
public partial class Battle.Scene.Animation.ThrowRock : Battle.Scene.Animation {
	include Battle.Scene.Animation.BallAnimationMixin;

	public override void initialize(sprites, viewport, battler) {
		@battler = battler;
		@trainer = battler.battle.GetOwnerFromBattlerIndex(battler.index);
		base.initialize(sprites, viewport);
	}

	public void createProcesses() {
		// Calculate start and end coordinates for battler sprite movement
		batSprite = @sprites[$"pokemon_{@battler.index}"];
		traSprite = @sprites["player_1"];
		ballStartX = traSprite.x;
		ballStartY = traSprite.y - (traSprite.bitmap.height / 2);
		ballMidX   = 0;   // Unused in arc calculation
		ballMidY   = 122;
		ballEndX   = batSprite.x;
		ballEndY   = batSprite.y - (batSprite.bitmap.height / 2);
		// Set up trainer sprite
		trainer = addSprite(traSprite, PictureOrigin.BOTTOM);
		// Set up bait sprite
		ball = addNewSprite(ballStartX, ballStartY,
												"Graphics/Battle animations/safari_rock", PictureOrigin.CENTER);
		ball.setZ(0, batSprite.z + 1);
		// Trainer animation
		if (traSprite.bitmap.width >= traSprite.bitmap.height * 2) {
			ballStartX, ballStartY = trainerThrowingFrames(ball, trainer, traSprite);
		}
		delay = ball.totalDuration;   // 0 or 7
		// Bait arc animation
		ball.setSE(delay, "Battle throw");
		createBallTrajectory(ball, delay, 12,
												ballStartX, ballStartY, ballMidX, ballMidY, ballEndX, ballEndY);
		ball.setZ(9, batSprite.z + 1);
		delay = ball.totalDuration;
		ball.setSE(delay, "Battle damage weak");
		ball.moveOpacity(delay + 2, 2, 0);
		ball.setVisible(delay + 4, false);
		// Set up anger sprite
		anger = addNewSprite(ballEndX - 42, ballEndY - 36,
												"Graphics/Battle animations/safari_anger", PictureOrigin.CENTER);
		anger.setVisible(0, false);
		anger.setZ(0, batSprite.z + 1);
		// Show anger appearing
		delay = ball.totalDuration + 5;
		2.times do;
			anger.setSE(delay, "Player jump");
			anger.setVisible(delay, true);
			anger.moveZoom(delay, 3, 130);
			anger.moveZoom(delay + 3, 3, 100);
			anger.setVisible(delay + 6, false);
			anger.setDelta(delay + 6, 96, -16);
			delay = anger.totalDuration + 3;
		}
	}
}

//===============================================================================
// Safari Zone battle scene (the visuals of the battle).
//===============================================================================
public partial class Battle.Scene {
	public void SafariStart() {
		@briefMessage = false;
		@sprites["dataBox_0"] = new SafariDataBox(@battle, @viewport);
		dataBoxAnim = new Animation.DataBoxAppear(@sprites, @viewport, 0);
		do { //loop; while (true);
			dataBoxAnim.update;
			Update;
			if (dataBoxAnim.animDone()) break;
		}
		dataBoxAnim.dispose;
		Refresh;
	}

	public void SafariCommandMenu(index) {
		CommandMenuEx(index,
										new {_INTL("What will\n{1} throw?", @battle.Player.name),
										_INTL("Ball"),
										_INTL("Bait"),
										_INTL("Rock"),
										_INTL("Run")}, 3);
	}

	public void ThrowBait() {
		@briefMessage = false;
		baitAnim = new Animation.ThrowBait(@sprites, @viewport, @battle.battlers[1]);
		do { //loop; while (true);
			baitAnim.update;
			Update;
			if (baitAnim.animDone()) break;
		}
		baitAnim.dispose;
	}

	public void ThrowRock() {
		@briefMessage = false;
		rockAnim = new Animation.ThrowRock(@sprites, @viewport, @battle.battlers[1]);
		do { //loop; while (true);
			rockAnim.update;
			Update;
			if (rockAnim.animDone()) break;
		}
		rockAnim.dispose;
	}

	unless (method_defined(:__safari__pbThrowSuccess)) alias __safari__pbThrowSuccess ThrowSuccess;

	public void ThrowSuccess() {
		__safari__pbThrowSuccess;
		if (@battle.is_a(SafariBattle)) WildBattleSuccess;
	}
}

//===============================================================================
// Safari Zone battle class.
//===============================================================================
public partial class SafariBattle {
	/// <summary>Array of fake battler objects</summary>
	public int battlers		{ get { return _battlers; } }			protected int _battlers;
	/// <summary>Array of number of battlers per side</summary>
	public int sideSizes		{ get { return _sideSizes; } set { _sideSizes = value; } }			protected int _sideSizes;
	/// <summary>Filename fragment used for background graphics</summary>
	public int backdrop		{ get { return _backdrop; } set { _backdrop = value; } }			protected int _backdrop;
	/// <summary>Filename fragment used for base graphics</summary>
	public int backdropBase		{ get { return _backdropBase; } set { _backdropBase = value; } }			protected int _backdropBase;
	/// <summary>Time of day (0=day, 1=eve, 2=night)</summary>
	public int time		{ get { return _time; } set { _time = value; } }			protected int _time;
	/// <summary>Battle surroundings (for mechanics purposes)</summary>
	public int environment		{ get { return _environment; } set { _environment = value; } }			protected int _environment;
	public int weather		{ get { return _weather; } }			protected int _weather;
	public int player		{ get { return _player; } }			protected int _player;
	public int party2		{ get { return _party2; } set { _party2 = value; } }			protected int _party2;
	/// <summary>True if player can run from battle</summary>
	public int canRun		{ get { return _canRun; } set { _canRun = value; } }			protected int _canRun;
	/// <summary>True if player won't black out if they lose</summary>
	public int canLose		{ get { return _canLose; } set { _canLose = value; } }			protected int _canLose;
	/// <summary>Switch/Set "battle style" option</summary>
	public int switchStyle		{ get { return _switchStyle; } set { _switchStyle = value; } }			protected int _switchStyle;
	/// <summary>"Battle scene" option (show anims)</summary>
	public int showAnims		{ get { return _showAnims; } set { _showAnims = value; } }			protected int _showAnims;
	/// <summary>Whether Pokémon can gain Exp/EVs</summary>
	public int expGain		{ get { return _expGain; } set { _expGain = value; } }			protected int _expGain;
	/// <summary>Whether the player can gain/lose money</summary>
	public int moneyGain		{ get { return _moneyGain; } set { _moneyGain = value; } }			protected int _moneyGain;
	public int rules		{ get { return _rules; } set { _rules = value; } }			protected int _rules;
	public int ballCount		{ get { return _ballCount; } set { _ballCount = value; } }			protected int _ballCount;

	include Battle.CatchAndStoreMixin;

	public void Random(x) {return rand(x); }

	public void initialize(scene, player, party2) {
		@scene         = scene;
		@peer          = new Battle.Peer();
		@backdrop      = "";
		@backdropBase  = null;
		@time          = 0;
		@environment   = :None;   // e.g. Tall grass, cave, still water
		@weather       = :None;
		@decision      = Battle.Outcome.UNDECIDED;
		@caughtPokemon = new List<string>();
		@player        = [player];
		@party2        = party2;
		@sideSizes     = new {1, 1};
		@battlers      = new {new Battle.FakeBattler(self, 0),
											new Battle.FakeBattler(self, 1)};
		@rules         = new List<string>();
		@ballCount     = 0;
	}

	public bool decided() {
		return Battle.Outcome.decided(@decision);
	}

	public int disablePokeBalls { set {  } }
	public int sendToBoxes { set {  } }
	public int defaultWeather { set { @weather = value; } }
	public int defaultTerrain { set {  } }

	//-----------------------------------------------------------------------------
	// Information about the type and size of the battle.
	//-----------------------------------------------------------------------------

	public bool wildBattle() {    return true;  }
	public bool trainerBattle() { return false; }

	public void setBattleMode(mode) {}

	public void SideSize(index) {
		return @sideSizes[index % 2];
	}

	//-----------------------------------------------------------------------------
	// Trainers and owner-related.
	//-----------------------------------------------------------------------------

	public int Player { get { return @player[0]; } }
	public int opponent { get { return null;        } }

	public void GetOwnerFromBattlerIndex(idxBattler) {return Player; }

	public void SetSeen(battler) {
		if (!battler) return;
		if (battler.is_a(Battle.Battler)) {
			Player.pokedex.register(battler.displaySpecies, battler.displayGender,
																battler.displayForm, battler.shiny());
		} else {
			Player.pokedex.register(battler);
		}
	}

	public void SetCaught(battler) {
		if (!battler) return;
		if (battler.is_a(Battle.Battler)) {
			Player.pokedex.register_caught(battler.displaySpecies);
		} else {
			Player.pokedex.register_caught(battler.species);
		}
	}

	//-----------------------------------------------------------------------------
	// Get party info (counts all teams on the same side).
	//-----------------------------------------------------------------------------

	public void Party(idxBattler) {
		return (opposes(idxBattler)) ? @party2 : null;
	}

	public bool AllFainted(idxBattler = 0) {  return false; }

	//-----------------------------------------------------------------------------
	// Battler-related.
	//-----------------------------------------------------------------------------

	public bool opposes(idxBattler1, idxBattler2 = 0) {
		if (idxBattler1.respond_to("index")) idxBattler1 = idxBattler1.index;
		if (idxBattler2.respond_to("index")) idxBattler2 = idxBattler2.index;
		return (idxBattler1 & 1) != (idxBattler2 & 1);
	}

	public void RemoveFromParty(idxBattler, idxParty) {}
	public void GainExp() { }

	//-----------------------------------------------------------------------------
	// Messages and animations.
	//-----------------------------------------------------------------------------

	public void Display(msg, Action block = null) {
		@scene.DisplayMessage(msg, &block);
	}

	public void DisplayPaused(msg, Action block = null) {
		@scene.DisplayPausedMessage(msg, &block);
	}

	public void DisplayBrief(msg) {
		@scene.DisplayMessage(msg, true);
	}

	public void DisplayConfirm(msg) {
		return @scene.DisplayConfirmMessage(msg);
	}

	public partial class BattleAbortedException : Exception { }

	public void Abort() {
		Debug.LogError(new BattleAbortedException("Battle aborted"));
		//throw new Exception(new BattleAbortedException("Battle aborted"));
	}

	//-----------------------------------------------------------------------------
	// Safari battle-specific methods.
	//-----------------------------------------------------------------------------

	public void EscapeRate(catch_rate) {
		if (catch_rate <= 45) return 125;   // Escape factor 9 (45%)
		if (catch_rate <= 60) return 100;   // Escape factor 7 (35%)
		if (catch_rate <= 120) return 75;   // Escape factor 5 (25%)
		if (catch_rate <= 250) return 50;   // Escape factor 3 (15%)
		return 25;                        // Escape factor 2 (10%)
	}

	public void StartBattle() {
		begin;
			pkmn = @party2[0];
			SetSeen(pkmn);
			@scene.StartBattle(self);
			DisplayPaused(_INTL("Wild {1} appeared!", pkmn.name));
			@scene.SafariStart;
			weather_data = GameData.BattleWeather.try_get(@weather);
			if (weather_data) @scene.CommonAnimation(weather_data.animation);
			safariBall = GameData.Item.get(:SAFARIBALL).id;
			catch_rate = pkmn.species_data.catch_rate;
			catchFactor  = (catch_rate * 100) / 1275;
			catchFactor  = (int)Math.Min((int)Math.Max(catchFactor, 3), 20);
			escapeFactor = (EscapeRate(catch_rate) * 100) / 1275;
			escapeFactor = (int)Math.Min((int)Math.Max(escapeFactor, 2), 20);
			do { //loop; while (true);
				cmd = @scene.SafariCommandMenu(0);
				switch (cmd) {
					case 0:   // Ball
						if (BoxesFull()) {
							Display(_INTL("The boxes are full! You can't catch any more Pokémon!"));
							continue;
						}
						@ballCount -= 1;
						@scene.Refresh;
						rare = (catchFactor * 1275) / 100;
						if (safariBall) {
							ThrowPokeBall(1, safariBall, rare, true);
							if (@caughtPokemon.length > 0) {
								RecordAndStoreCaughtPokemon;
								@decision = Battle.Outcome.CATCH;
							}
						}
						break;
					case 1:   // Bait
						DisplayBrief(_INTL("{1} threw some bait at the {2}!", self.Player.name, pkmn.name));
						@scene.ThrowBait;
						if (Random(100) < 90) catchFactor  /= 2;   // Harder to catch
						escapeFactor /= 2;                       // Less likely to escape
						break;
					case 2:   // Rock
						DisplayBrief(_INTL("{1} threw a rock at the {2}!", self.Player.name, pkmn.name));
						@scene.ThrowRock;
						catchFactor  *= 2;                       // Easier to catch
						if (Random(100) < 90) escapeFactor *= 2;   // More likely to escape
						break;
					case 3:   // Run
						SEPlay("Battle flee");
						DisplayPaused(_INTL("You got away safely!"));
						@decision = Battle.Outcome.FLEE;
						break;
					default:
						continue;
						break;
				}
				catchFactor  = (int)Math.Min((int)Math.Max(catchFactor, 3), 20);
				escapeFactor = (int)Math.Min((int)Math.Max(escapeFactor, 2), 20);
				// End of round
				if (!decided()) {
					if (@ballCount <= 0) {
						SEPlay("Safari Zone end");
						Display(_INTL("PA: You have no Safari Balls left! Game over!"));
						@decision = Battle.Outcome.LOSE;
					} else if (Random(100) < 5 * escapeFactor) {
						SEPlay("Battle flee");
						Display(_INTL("{1} fled!", pkmn.name));
						@decision = Battle.Outcome.FLEE;
					} else if (cmd == 1) {   // Bait
						Display(_INTL("{1} is eating!", pkmn.name));
					} else if (cmd == 2) {   // Rock
						Display(_INTL("{1} is angry!", pkmn.name));
					} else {
						Display(_INTL("{1} is watching carefully!", pkmn.name));
					}
					// Weather continues
					weather_data = GameData.BattleWeather.try_get(@weather);
					if (weather_data) @scene.CommonAnimation(weather_data.animation);
				}
				if (decided()) break;
			}
			@scene.EndBattle(@decision);
		rescue BattleAbortedException;
			@decision = Battle.Outcome.UNDECIDED;
			@scene.EndBattle(@decision);
		}
		return @decision;
	}
}
