//===============================================================================
//
//===============================================================================
public static partial class RecordedBattleModule {
	public int randomnums		{ get { return _randomnums; } }			protected int _randomnums;
	public int rounds		{ get { return _rounds; } }			protected int _rounds;

	public static partial class Commands {
		public const int FIGHT   = 0;
		public const int BAG     = 1;
		public const int POKEMON = 2;
		public const int RUN     = 3;
	}

	public void initialize(*arg) {
		@randomnumbers = new List<string>();
		@rounds        = new List<string>();
		@switches      = new List<string>();
		@roundindex    = -1;
		@properties    = new List<string>();
		super(*arg);
	}

	public void GetBattleType() {
		return 0;   // Battle Tower
	}

	public void GetTrainerInfo(trainer) {
		if (!trainer) return null;
		if (trainer.Length > 0) {
			ret = new List<string>();
			foreach (var tr in trainer) { //'trainer.each' do => |tr|
				if (tr.is_a(Player)) {
					ret.Add(new {tr.trainer_type, tr.name.clone, tr.id, tr.badges.clone});
				} else {   // NPCTrainer
					ret.Add(new {tr.trainer_type, tr.name.clone, tr.id, tr.lose_text || "...", tr.win_text || "..."});
				}
			}
			return ret;
		} else if (trainer[i].is_a(Player)) {
			return new {trainer.trainer_type, trainer.name.clone, trainer.id, trainer.badges.clone};
		} else {
			return new {trainer.trainer_type, trainer.name.clone, trainer.id, trainer.lose_text || "...", trainer.win_text || "..."};
		}
	}

	public void StartBattle() {
		@properties = new List<string>();
		@properties["internalBattle"]  = @internalBattle;
		@properties["player"]          = GetTrainerInfo(@player);
		@properties["opponent"]        = GetTrainerInfo(@opponent);
		@properties["party1"]          = Marshal.dump(@party1);
		@properties["party2"]          = Marshal.dump(@party2);
		@properties["party1starts"]    = Marshal.dump(@party1starts);
		@properties["party2starts"]    = Marshal.dump(@party2starts);
		@properties["weather"]         = @field.weather;
		@properties["weatherDuration"] = @field.weatherDuration;
		@properties["canRun"]          = @canRun;
		@properties["switchStyle"]     = @switchStyle;
		@properties["showAnims"]       = @showAnims;
		@properties["items"]           = Marshal.dump(@items);
		@properties["backdrop"]        = @backdrop;
		@properties["backdropBase"]    = @backdropBase;
		@properties["time"]            = @time;
		@properties["environment"]     = @environment;
		@properties["rules"]           = Marshal.dump(@rules);
		super;
	}

	public void DumpRecord() {
		return Marshal.dump(new {GetBattleType, @properties, @rounds, @randomnumbers, @switches});
	}

	public override void SwitchInBetween(idxBattler, checkLaxOnly = false, canCancel = false) {
		ret = base.SwitchInBetween();
		@switches.Add(ret);
		return ret;
	}

	public override void RegisterMove(idxBattler, idxMove, showMessages = true) {
		if (base.RegisterMove()) {
			@rounds[@roundindex][idxBattler] = new {Commands.FIGHT, idxMove};
			return true;
		}
		return false;
	}

	public override void RegisterTarget(idxBattler, idxTarget) {
		base.RegisterTarget();
		@rounds[@roundindex][idxBattler][2] = idxTarget;
	}

	public override void Run(idxBattler, duringBattle = false) {
		ret = base.Run();
		@rounds[@roundindex][idxBattler] = new {Commands.RUN, @decision};
		return ret;
	}

	public override void AutoChooseMove(idxBattler, showMessages = true) {
		ret = base.AutoChooseMove();
		@rounds[@roundindex][idxBattler] = new {Commands.FIGHT, -1};
		return ret;
	}

	public override void RegisterSwitch(idxBattler, idxParty) {
		if (base.RegisterSwitch()) {
			@rounds[@roundindex][idxBattler] = new {Commands.POKEMON, idxParty};
			return true;
		}
		return false;
	}

	public override void RegisterItem(idxBattler, item, idxTarget = null, idxMove = null) {
		if (base.RegisterItem()) {
			@rounds[@roundindex][idxBattler] = new {Commands.BAG, item, idxTarget, idxMove};
			return true;
		}
		return false;
	}

	public void CommandPhase() {
		@roundindex += 1;
		@rounds[@roundindex] = new {[], new List<string>(), new List<string>(), new List<string>()};
		super;
	}

	public void StorePokemon(pkmn) {}

	public override void Random(num) {
		ret = base.Random(num);
		@randomnumbers.Add(ret);
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public static partial class RecordedBattlePlaybackModule {
	public static partial class Commands
		public const int FIGHT   = 0;
		public const int BAG     = 1;
		public const int POKEMON = 2;
		public const int RUN     = 3;
	}

	public override void initialize(scene, battle) {
		@battletype  = battle[0];
		@properties  = battle[1];
		@rounds      = battle[2];
		@randomnums  = battle[3];
		@switches    = battle[4];
		@roundindex  = -1;
		@randomindex = 0;
		@switchindex = 0;
		base.initialize(scene,
			Marshal.restore(@properties["party1"]),
			Marshal.restore(@properties["party2"]),
			RecordedBattle.PlaybackHelper.CreateTrainerInfo(@properties["player"]),
			RecordedBattle.PlaybackHelper.CreateTrainerInfo(@properties["opponent"])
		);
	}

	public override void StartBattle() {
		@party1starts          = Marshal.restore(@properties["party1starts"]);
		@party2starts          = Marshal.restore(@properties["party2starts"]);
		@internalBattle        = @properties["internalBattle"];
		@field.weather         = @properties["weather"];
		@field.weatherDuration = @properties["weatherDuration"];
		@canRun                = @properties["canRun"];
		@switchStyle           = @properties["switchStyle"];
		@showAnims             = @properties["showAnims"];
		@backdrop              = @properties["backdrop"];
		@backdropBase          = @properties["backdropBase"];
		@time                  = @properties["time"];
		@environment           = @properties["environment"];
		@items                 = Marshal.restore(@properties["items"]);
		@rules                 = Marshal.restore(@properties["rules"]);
		base.StartBattle();
	}

	public void SwitchInBetween(_idxBattler, _checkLaxOnly = false, _canCancel = false) {
		ret = @switches[@switchindex];
		@switchindex += 1;
		return ret;
	}

	public void Random(_num) {
		ret = @randomnums[@randomindex];
		@randomindex += 1;
		return ret;
	}

	public void DisplayPaused(str) {
		Display(str);
	}

	public void CommandPhaseLoop(isPlayer) {
		if (!isPlayer) return;
		@roundindex += 1;
		for (int i = 4; i < 4; i++) { //for '4' times do => |i|
			if (@rounds[@roundindex][i].length == 0) continue;
			ClearChoice(i);
			switch (@rounds[@roundindex][i][0]) {
				case Commands.FIGHT:
					if (@rounds[@roundindex][i][1] == -1) {
						AutoChooseMove(i, false);
					} else {
						RegisterMove(i, @rounds[@roundindex][i][1]);
					}
					if (@rounds[@roundindex][i][2]) {
						RegisterTarget(i, @rounds[@roundindex][i][2]);
					}
					break;
				case Commands.BAG:
					RegisterItem(i, @rounds[@roundindex][i][1], @rounds[@roundindex][i][2], @rounds[@roundindex][i][3]);
					break;
				case Commands.POKEMON:
					RegisterSwitch(i, @rounds[@roundindex][i][1]);
					break;
				case Commands.RUN:
					@decision = @rounds[@roundindex][i][1];
					break;
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class RecordedBattle : Battle {
	include RecordedBattleModule;

	public int GetBattleType { get { return 0; } }
}

public partial class RecordedBattle.BattlePalaceBattle : BattlePalaceBattle {
	include RecordedBattleModule;

	public int GetBattleType { get { return 1; } }
}

public partial class RecordedBattle.BattleArenaBattle : BattleArenaBattle {
	include RecordedBattleModule;

	public int GetBattleType { get { return 2; } }
}

public partial class RecordedBattle.PlaybackBattle : Battle {
	include RecordedBattlePlaybackModule;
}

public partial class RecordedBattle.BattlePalacePlaybackBattle : BattlePalaceBattle {
	include RecordedBattlePlaybackModule;
}

public partial class RecordedBattle.BattleArenaPlaybackBattle : BattleArenaBattle {
	include RecordedBattlePlaybackModule;
}

//===============================================================================
//
//===============================================================================
public static partial class RecordedBattle.PlaybackHelper {
	public static void GetOpponent(battle) {
		return self.CreateTrainerInfo(battle[1]["opponent"]);
	}

	public static void GetBattleBGM(battle) {
		return self.GetTrainerBattleBGM(self.GetOpponent(battle));
	}

	public static void CreateTrainerInfo(trainer) {
		if (!trainer) return null;
		ret = new List<string>();
		foreach (var tr in trainer) { //'trainer.each' do => |tr|
			if (tr.length == 4) {   // Player
				t = new Player(tr[1], tr[0]);
				t.badges = tr[3];
			} else {   // NPCTrainer
				t = new NPCTrainer(tr[1], tr[0]);
				t.lose_text = tr[3] || "...";
				t.win_text = tr[4] || "...";
			}
			t.id = tr[2];
			ret.Add(t);
		}
		return ret;
	}
}
