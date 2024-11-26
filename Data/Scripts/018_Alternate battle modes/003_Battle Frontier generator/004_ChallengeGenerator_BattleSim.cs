//===============================================================================
//
//===============================================================================
public partial class RuledTeam {
	public int team		{ get { return _team; } set { _team = value; } }			protected int _team;

	public void initialize(party, rule) {
		count = rule.ruleset.suggestedNumber;
		@team = new List<string>();
		retnum = new List<string>();
		do { //loop; while (true);
			for (int i = count; i < count; i++) { //for 'count' times do => |i|
				retnum[i] = rand(party.length);
				@team[i] = party[retnum[i]];
				party.delete_at(retnum[i]);
			}
			if (rule.ruleset.isValid(@team)) break;
		}
		@totalGames = 0;
		@rating = new PlayerRating();
		@history = new MatchHistory(@rating);
	}

	public int this[int i] { get {
		@team[i];
		}
	}

	public void length() {
		return @team.length;
	}

	public void rating() {
		@rating.winChancePercent;
	}

	public void ratingData() {
		@rating;
	}

	public void ratingRaw() {
		new {@rating.rating, @rating.deviation, @rating.volatility, @rating.winChancePercent};
	}

	public void compare(other) {
		@rating.compare(other.ratingData);
	}

	public void totalGames() {
		(@totalGames || 0) + self.games
	}

	public void addMatch(other, score) {
		@history.addMatch(other.ratingData, score);
	}

	public void games() {
		@history.length;
	}

	public void updateRating() {
		if (!@totalGames) @totalGames = 0;
		oldgames = self.games;
		@history.updateAndClear;
		newgames = self.games;
		@totalGames += (oldgames - newgames);
	}

	public void toStr() {
		return "new {" + @rating.ToInt().ToString() + "," + @games.ToInt().ToString() + "}";
	}

	public void load(party) {
		ret = new List<string>();
		for (int i = team.length; i < team.length; i++) { //for 'team.length' times do => |i|
			ret.Add(party[team[i]]);
		}
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public partial class SingleMatch {
	public int opponentRating		{ get { return _opponentRating; } }			protected int _opponentRating;
	public int opponentDeviation		{ get { return _opponentDeviation; } }			protected int _opponentDeviation;
	public int score		{ get { return _score; } }			protected int _score;
	public int kValue		{ get { return _kValue; } }			protected int _kValue;

	public void initialize(opponentRating, opponentDev, score, kValue = 16) {
		@opponentRating    = opponentRating;
		@opponentDeviation = opponentDev;
		@score             = score;   // -1=draw, 0=lose, 1=win
		@kValue            = kValue;
	}
}

//===============================================================================
//
//===============================================================================
public partial class MatchHistory {
	include Enumerable;

	public void initialize(thisPlayer) {
		@matches    = new List<string>();
		@thisPlayer = thisPlayer;
	}

	public int this[int i] { get {
		@matches[i];
		}
	}

	public void length() {
		@matches.length;
	}

	public void each() {
		@matches.each(item => yield item);
	}

	public void addMatch(otherPlayer, result) {
		// 1=I won; 0=Other player won; -1: Draw
		@matches.Add(new SingleMatch(otherPlayer.rating, otherPlayer.deviation, result));
	}

	public void updateAndClear() {
		@thisPlayer.update(@matches);
		@matches.clear;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PlayerRatingElo {
	public int rating		{ get { return _rating; } }			protected int _rating;

	public const int K_VALUE = 16;

	public void initialize() {
		@rating          = 1600.0;
		@deviation       = 0;
		@volatility      = 0;
		@estimatedRating = null;
	}

	public void winChancePercent() {
		if (@estimatedRating) return @estimatedRating;
		x = (1 + (10.0**((@rating - 1600.0) / 400.0)));
		@estimatedRating = (x == 0 ? 1.0 : 1.0 / x);
		return @estimatedRating;
	}

	public void update(matches) {
		if (matches.length == 0) return;
		stake = 0;
		matches.length.times do;
			score = (match.score == -1) ? 0.5 : match.score;
			e = (1 + (10.0**((@rating - match.opponentRating) / 400.0)));
			stake += match.kValue * (score - e);
		}
		@rating += stake;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PlayerRating {
	public int volatility		{ get { return _volatility; } }			protected int _volatility;
	public int deviation		{ get { return _deviation; } }			protected int _deviation;
	public int rating		{ get { return _rating; } }			protected int _rating;

	public void initialize() {
		@rating          = 1500.0;
		@deviation       = 350.0;
		@volatility      = 0.9;
		@estimatedRating = null;
	}

	public void winChancePercent() {
		if (@estimatedRating) return @estimatedRating;
		if (@deviation > 100) {
			// https://www.smogon.com/forums/threads/make-sense-of-your-shoddy-battle-rating.55764/
			otherRating = 1500.0;
			otherDeviation = 350.0;
			s = Math.sqrt(100_000.0 + (@deviation * @deviation) + (otherDeviation * otherDeviation));
			g = 10.0**((otherRating - @rating) * 0.79 / s);
			@estimatedRating = (1.0 / (1.0 + g)) * 100.0;   // Percent chance that I win against opponent
		} else {
			// GLIXARE method
			rds = @deviation * @deviation;
			sqr = Math.sqrt(15.905694331435 * (rds + 221_781.21786254));
			inner = (1500.0 - @rating) * Math.PI / sqr;
			@estimatedRating = ((10_000.0 / (1.0 + (10.0**inner))) + 0.5) / 100.0;
		}
		return @estimatedRating;
	}

	public void update(matches, system = 1.2) {
		volatility = volatility2;
		deviation = deviation2;
		rating = rating2;
		if (matches.length == 0) {
			setDeviation2(Math.sqrt((deviation * deviation) + (volatility * volatility)));
			return;
		}
		g = new List<string>();
		e = new List<string>();
		score = new List<string>();
		for (int i = matches.length; i < matches.length; i++) { //for 'matches.length' times do => |i|
			match = matches[i];
			g[i] = getGFactor(match.opponentDeviation);
			e[i] = getEFactor(rating, match.opponentRating, g[i]);
			score[i] = match.score;
		}
		// Estimated variance
		variance = 0.0;
		for (int i = matches.length; i < matches.length; i++) { //for 'matches.length' times do => |i|
			variance += g[i] * g[i] * e[i] * (1 - e[i]);
		}
		variance = 1.0 / variance;
		// Improvement sum
		sum = 0.0;
		for (int i = matches.length; i < matches.length; i++) { //for 'matches.length' times do => |i|
			v = score[i];
			if (v != -1) sum += g[i] * (v.to_f - e[i]);
		}
		volatility = getUpdatedVolatility(volatility, deviation, variance, sum, system);
		// Update deviation
		t = (deviation * deviation) + (volatility * volatility);
		deviation = 1.0 / Math.sqrt((1.0 / t) + (1.0 / variance));
		// Update rating
		rating += deviation * deviation * sum;
		setRating2(rating);
		setDeviation2(deviation);
		setVolatility2(volatility);
	}

	//-----------------------------------------------------------------------------

	private;

	public int volatility		{ get { return _volatility } set { _volatility = value; } }			private int _volatility;
	public int volatility2		{ get { return _volatility } set { _volatility = value; } }			private int _volatility;


	public void rating2() {
		return (@rating - 1500.0) / 173.7178;
	}

	public void deviation2() {
		return @deviation / 173.7178;
	}

	public void getGFactor(deviation) {
		// deviation is not yet in glicko2
		deviation /= 173.7178;
		return 1.0 / Math.sqrt(1.0 + ((3.0 * deviation * deviation) / (Math.PI * Math.PI)));
	}

	public void getEFactor(rating, opponentRating, g) {
		// rating is already in glicko2
		// opponentRating is not yet in glicko2
		opponentRating = (opponentRating - 1500.0) / 173.7178;
		return 1.0 / (1.0 + Math.exp(-g * (rating - opponentRating)));
	}

	public void setVolatility2(value) {
		@volatility = value;
	}

	public void setRating2(value) {
		@estimatedRating = null;
		@rating = (value * 173.7178) + 1500.0;
	}

	public void setDeviation2(value) {
		@estimatedRating = null;
		@deviation = value * 173.7178;
	}

	public void getUpdatedVolatility(volatility, deviation, variance, improvementSum, system) {
		improvement = improvementSum * variance;
		a = Math.log(volatility * volatility);
		squSystem = system * system;
		squDeviation = deviation * deviation;
		squVariance = variance + variance;
		squDevplusVar = squDeviation + variance;
		x0 = a;
		100.times do;   // Up to 100 iterations to avoid potentially infinite loops
			e = Math.exp(x0);
			d = squDevplusVar + e;
			squD = d * d;
			i = improvement / d;
			h1 = (-(x0 - a) / squSystem) - (0.5 * e * i * i);
			h2 = (-1.0 / squSystem) - (0.5 * e * squDevplusVar / squD);
			h2 += 0.5 * squVariance * e * (squDevplusVar - e) / (squD * d);
			x1 = x0;
			x0 -= h1 / h2;
			if ((x1 - x0).abs < 0.000001) break;
		}
		return Math.exp(x0 / 2.0);
	}
}

//===============================================================================
//
//===============================================================================
public void DecideWinnerEffectiveness(move, otype1, otype2, ability, scores) {
	data = GameData.Move.get(move);
	if (data.power == 0) return 0;
	atype = data.type;
	typemod = 1.0;
	if (ability != abilitys.LEVITATE || data.type != types.GROUND) {
		mod1 = Effectiveness.calculate(atype, otype1);
		mod2 = (otype1 == otype2) ? 1.0 : Effectiveness.calculate(atype, otype2);
		if (ability == abilitys.WONDERGUARD) {
			if (!Effectiveness.super_effective(mod1)) mod1 = 1.0;
			if (!Effectiveness.super_effective(mod2)) mod2 = 1.0;
		}
		typemod = mod1 * mod2;
	}
	typemod *= 4;   // Because dealing with 2 types
	if (typemod == 0) return scores[0];    // Ineffective
	if (typemod == 1) return scores[1];    // Doubly not very effective
	if (typemod == 2) return scores[2];    // Not very effective
	if (typemod == 4) return scores[3];    // Normal effective
	if (typemod == 8) return scores[4];    // Super effective
	if (typemod == 16) return scores[5];   // Doubly super effective
	return 0;
}

public void DecideWinnerScore(party0, party1, rating) {
	score = 0;
	types1 = new List<string>();
	types2 = new List<string>();
	abilities = new List<string>();
	for (int j = party1.length; j < party1.length; j++) { //for 'party1.length' times do => |j|
		types1.Add(party1[j].types[0]);
		types2.Add(party1[j].types[1] || party1[j].types[0]);
		abilities.Add(party1[j].ability_id);
	}
	for (int i = party0.length; i < party0.length; i++) { //for 'party0.length' times do => |i|
		foreach (var move in party0[i].moves) { //'party0[i].moves.each' do => |move|
			if (!move) continue;
			for (int j = party1.length; j < party1.length; j++) { //for 'party1.length' times do => |j|
				score += DecideWinnerEffectiveness(
					move.id, types1[j], types2[j], abilities[j], new {-16, -8, 0, 4, 12, 20}
				);
			}
		}
		basestatsum = baseStatTotal(party0[i].species);
		score += basestatsum / 10;
		if (party0[i].item) score += 10;   // Not in Battle Dome ranking
	}
	score += rating + rand(32);
	return score;
}

public void DecideWinner(party0, party1, rating0, rating1) {
	rating0 = (int)Math.Round(rating0 * 15.0 / 100);
	rating1 = (int)Math.Round(rating1 * 15.0 / 100);
	score0 = DecideWinnerScore(party0, party1, rating0);
	score1 = DecideWinnerScore(party1, party0, rating1);
	if (score0 == score1) {
		if (rating0 == rating1) return Battle.Outcome.DRAW;
		return (rating0 > rating1) ? Battle.Outcome.WIN : Battle.Outcome.LOSE;
	} else {
		return (score0 > score1) ? Battle.Outcome.WIN : Battle.Outcome.LOSE;
	}
}

//===============================================================================
//
//===============================================================================
public void RuledBattle(team1, team2, rule) {
	outcome = Battle.Outcome.UNDECIDED;
	if (rand(100) == 0) {
		level = rule.ruleset.suggestedLevel;
		t_type = GameData.TrainerType.keys.first;
		trainer1 = new NPCTrainer("PLAYER1", t_type);
		trainer2 = new NPCTrainer("PLAYER2", t_type);
		items1 = new List<string>();
		items2 = new List<string>();
		team1.team.each_with_index do |p, i|
			if (!p) continue;
			if (p.level != level) {
				p.level = level;
				p.calc_stats;
			}
			items1[i] = p.item_id;
			trainer1.party.Add(p);
		}
		team2.team.each_with_index do |p, i|
			if (!p) continue;
			if (p.level != level) {
				p.level = level;
				p.calc_stats;
			}
			items2[i] = p.item_id;
			trainer2.party.Add(p);
		}
		scene = new Battle.DebugSceneNoVisuals();
		battle = rule.createBattle(scene, trainer1, trainer2);
		battle.debug = true;
		battle.controlPlayer = true;
		battle.internalBattle = false;
		outcome = battle.StartBattle;
		team1.team.each_with_index do |p, i|
			if (!p) continue;
			p.heal;
			p.item = items1[i];
		}
		team2.team.each_with_index do |p, i|
			if (!p) continue;
			p.heal;
			p.item = items2[i];
		}
	} else {
		party1 = new List<string>();
		party2 = new List<string>();
		team1.length.times(i => party1.Add(team1[i]));
		team2.length.times(i => party2.Add(team2[i]));
		outcome = DecideWinner(party1, party2, team1.rating, team2.rating);
	}
	switch (outcome) {
		case Battle.Outcome.WIN:   // Team 1 wins
			team1.addMatch(team2, 1);
			team2.addMatch(team1, 0);
			break;
		case Battle.Outcome.LOSE:   // Team 2 wins
			team1.addMatch(team2, 0);
			team2.addMatch(team1, 1);
			break;
		default:
			team1.addMatch(team2, -1);
			team2.addMatch(team1, -1);
			break;
	}
}
