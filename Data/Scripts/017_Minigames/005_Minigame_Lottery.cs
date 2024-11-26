//===============================================================================
// "Lottery" mini-game.
// By Maruno.
//===============================================================================
public void SetLotteryNumber(variable = 1) {
	t = GetTimeNow;
	hash = t.day + (t.month << 5) + (t.year << 9);
	srand(hash);                      // seed RNG with fixed value depending on date
	lottery = rand(65_536);           // get a number
	srand;                            // reseed RNG
	Set(variable, string.Format("{0:5}", lottery));
}

public void Lottery(winnum, nameVar = 2, positionVar = 3, matchedVar = 4) {
	winnum = winnum.ToInt();
	winpoke = null;
	winpos = 0;
	winmatched = 0;
	foreach (var i in Game.GameData.player.party) { //'Game.GameData.player.party.each' do => |i|
		thismatched = 0;
		id = i.owner.public_id;
		for (int j = 5; j < 5; j++) { //for '5' times do => |j|
			if ((id / (10**j)) % 10 != (winnum / (10**j)) % 10) break;
			thismatched += 1;
		}
		if (thismatched <= winmatched) continue;
		winpoke = i.name;
		winpos = 1;    // Party
		winmatched = thismatched;
	}
	EachPokemon do |poke, _box|
		thismatched = 0;
		id = poke.owner.public_id;
		for (int j = 5; j < 5; j++) { //for '5' times do => |j|
			if ((id / (10**j)) % 10 != (winnum / (10**j)) % 10) break;
			thismatched += 1;
		}
		if (thismatched <= winmatched) continue;
		winpoke = poke.name;
		winpos = 2;    // Storage
		winmatched = thismatched;
	}
	Game.GameData.game_variables[nameVar] = winpoke;
	Game.GameData.game_variables[positionVar] = winpos;
	Game.GameData.game_variables[matchedVar] = winmatched;
}
