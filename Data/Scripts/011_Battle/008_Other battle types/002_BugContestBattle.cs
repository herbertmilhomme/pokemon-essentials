//===============================================================================
// Bug-Catching Contest battle scene (the visuals of the battle).
//===============================================================================
public partial class Battle.Scene {
	unless (method_defined(:_bugContest_pbInitSprites)) alias _bugContest_pbInitSprites InitSprites;

	public void InitSprites() {
		_bugContest_pbInitSprites;
		// "helpwindow" shows the currently caught Pokémon's details when asking if
		// you want to replace it with a newly caught Pokémon.
		@sprites["helpwindow"] = Window_UnformattedTextPokemon.newWithSize("", 0, 0, 32, 32, @viewport);
		@sprites["helpwindow"].z       = 90;
		@sprites["helpwindow"].visible = false;
	}

	public void ShowHelp(text) {
		@sprites["helpwindow"].resizeToFit(text, Graphics.width);
		@sprites["helpwindow"].y       = 0;
		@sprites["helpwindow"].x       = 0;
		@sprites["helpwindow"].text    = text;
		@sprites["helpwindow"].visible = true;
	}

	public void HideHelp() {
		@sprites["helpwindow"].visible = false;
	}
}

//===============================================================================
// Bug-Catching Contest battle class.
//===============================================================================
public partial class BugContestBattle : Battle {
	public int ballCount		{ get { return _ballCount; } set { _ballCount = value; } }			protected int _ballCount;

	public override void initialize(*arg) {
		@ballCount = 0;
		@ballConst = GameData.Item.get(:SPORTBALL).id;
		base.initialize(*arg);
	}

	public void ItemMenu(idxBattler, _firstAction) {
		return RegisterItem(idxBattler, @ballConst, 1);
	}

	public void CommandMenu(idxBattler, _firstAction) {
		return @scene.CommandMenuEx(idxBattler,
																	new {_INTL("Sport Balls: {1}", @ballCount),
																	_INTL("Fight"),
																	_INTL("Ball"),
																	_INTL("Pokémon"),
																	_INTL("Run")}, 4);
	}

	public void ConsumeItemInBag(_item, _idxBattler) {
		if (@ballCount > 0) @ballCount -= 1;
	}

	public void StorePokemon(pkmn) {
		if (BugContestState.lastPokemon) {
			lastPokemon = BugContestState.lastPokemon;
			DisplayPaused(_INTL("You already caught a {1}.", lastPokemon.name));
			helptext = _INTL("Stock Pokémon:\n{1} Lv.{2} Max HP: {3}\nThis Pokémon:\n{4} Lv.{5} Max HP: {6}",
											lastPokemon.name, lastPokemon.level, lastPokemon.totalhp,
											pkmn.name, pkmn.level, pkmn.totalhp);
			@scene.ShowHelp(helptext);
			if (DisplayConfirm(_INTL("Switch Pokémon?"))) {
				BugContestState.lastPokemon = pkmn;
				@scene.HideHelp;
			} else {
				@scene.HideHelp;
				return;
			}
		} else {
			BugContestState.lastPokemon = pkmn;
		}
		Display(_INTL("Caught {1}!", pkmn.name));
	}

	public override void EndOfRoundPhase() {
		base.EndOfRoundPhase();
		if (@ballCount <= 0 && !decided()) @decision = Battle.Outcome.FLEE;
	}
}
