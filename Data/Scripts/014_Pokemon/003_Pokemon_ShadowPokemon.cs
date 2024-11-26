//===============================================================================
// Pokémon class.
//===============================================================================
public partial class Pokemon {
	public int shadow		{ get { return _shadow; } set { _shadow = value; } }			protected int _shadow;
	public int heart_gauge		{ get { return _heart_gauge; } }			protected int _heart_gauge;
	public int hyper_mode		{ get { return _hyper_mode; } }			protected int _hyper_mode;
	public int saved_exp		{ get { return _saved_exp; } set { _saved_exp = value; } }			protected int _saved_exp;
	public int saved_ev		{ get { return _saved_ev; } set { _saved_ev = value; } }			protected int _saved_ev;
	public int shadow_moves		{ get { return _shadow_moves; } set { _shadow_moves = value; } }			protected int _shadow_moves;
	public int heart_gauge_step_counter		{ get { return _heart_gauge_step_counter; } set { _heart_gauge_step_counter = value; } }			protected int _heart_gauge_step_counter;

	unless (method_defined(:__shadow_expeq)) alias __shadow_expeq exp=;
	public int exp { set {
		if (shadowPokemon()) {
			@saved_exp += value - @exp;
		} else {
			__shadow_expeq(value);
			}	}
	}

	unless (method_defined(:__shadow_hpeq)) alias __shadow_hpeq hp=;
	public int hp { set {
		__shadow_hpeq(value);
		if (@hp <= 0) @hyper_mode = false
		}	}

	public void heart_gauge() {
		return @heart_gauge || 0;
	}

	public void shadow_data() {
		return GameData.ShadowPokemon.get_species_form(@species, form_simple);
	}

	public void max_gauge_size() {
		data = shadow_data;
		return (data) ? data.gauge_size : GameData.ShadowPokemon.HEART_GAUGE_SIZE;
	}

	public void adjustHeart(value) {
		if (!shadowPokemon()) return;
		@heart_gauge = (self.heart_gauge + value).clamp(0, max_gauge_size);
	}

	public void change_heart_gauge(method, multiplier = 1) {
		if (!shadowPokemon()) return;
		heart_amounts = {
			// new {sending into battle, call to, walking 256 steps, using scent}
			HARDY   = new {110, 300, 100,  90},
			LONELY  = new { 70, 330, 100, 130},
			BRAVE   = new {130, 270,  90,  80},
			ADAMANT = new {110, 270, 110,  80},
			NAUGHTY = new {120, 270, 110,  70},
			BOLD    = new {110, 270,  90, 100},
			DOCILE  = new {100, 360,  80, 120},
			RELAXED = new { 90, 270, 110, 100},
			IMPISH  = new {120, 300, 100,  80},
			LAX     = new {100, 270,  90, 110},
			TIMID   = new { 70, 330, 110, 120},
			HASTY   = new {130, 300,  70, 100},
			SERIOUS = new {100, 330, 110,  90},
			JOLLY   = new {120, 300,  90,  90},
			NAIVE   = new {100, 300, 120,  80},
			MODEST  = new { 70, 300, 120, 110},
			MILD    = new { 80, 270, 100, 120},
			QUIET   = new {100, 300, 100, 100},
			BASHFUL = new { 80, 300,  90, 130},
			RASH    = new { 90, 300,  90, 120},
			CALM    = new { 80, 300, 110, 110},
			GENTLE  = new { 70, 300, 130, 100},
			SASSY   = new {130, 240, 100,  70},
			CAREFUL = new { 90, 300, 100, 110},
			QUIRKY  = new {130, 270,  80,  90}
		}
		amt = 100;
		switch (method) {
			case "battle":
				amt = (heart_amounts[@nature]) ? heart_amounts[@nature][0] : 100;
				break;
			case "call":
				amt = (heart_amounts[@nature]) ? heart_amounts[@nature][1] : 300;
				break;
			case "walking":
				amt = (heart_amounts[@nature]) ? heart_amounts[@nature][2] : 100;
				break;
			case "scent":
				amt = (heart_amounts[@nature]) ? heart_amounts[@nature][3] : 100;
				amt *= multiplier;
				break;
			default:
				Debug.LogError(_INTL("Unknown heart gauge-changing method: {1}", method.ToString()));
				//throw new ArgumentException(_INTL("Unknown heart gauge-changing method: {1}", method.ToString()));
				break;
		}
		adjustHeart(-amt);
	}

	public void heartStage() {
		if (!shadowPokemon()) return 0;
		max_size = max_gauge_size;
		stage_size = max_size / 5.0;
		return ((int)Math.Min(self.heart_gauge, max_size) / stage_size).ceil;
	}

	public bool shadowPokemon() {
		return @shadow && @heart_gauge && @heart_gauge >= 0;
	}

	public void hyper_mode() {
		return (self.heart_gauge == 0 || @hp == 0) ? false : @hyper_mode;
	}

	unless (method_defined(:__shadow__changeHappiness)) alias __shadow__changeHappiness changeHappiness;
	public void changeHappiness(method) {
		if (shadowPokemon() && heartStage >= 4) return;
		__shadow__changeHappiness(method);
	}

	public void makeShadow() {
		@shadow       = true;
		@hyper_mode   = false;
		@saved_exp    = 0;
		@saved_ev     = new List<string>();
		GameData.Stat.each_main(s => @saved_ev[s.id] = 0);
		@heart_gauge  = max_gauge_size;
		@heart_gauge_step_counter = 0;
		@shadow_moves = new List<string>();
		// Retrieve Shadow moveset for this Pokémon
		data = shadow_data;
		// Record this Pokémon's Shadow moves
		if (data) {
			foreach (var m in data.moves) { //'data.moves.each' do => |m|
				if (GameData.Move.exists(m)) @shadow_moves.Add(m);
				if (@shadow_moves.length >= MAX_MOVES) break;
			}
		}
		if (@shadow_moves.empty() && GameData.Moves.exists(Moves.SHADOWRUSH)) {
			@shadow_moves.Add(:SHADOWRUSH);
		}
		// Record this Pokémon's original moves
		if (!@shadow_moves.empty()) {
			@moves.each_with_index((m, i) => @shadow_moves[MAX_MOVES + i] = m.id);
			update_shadow_moves;
		}
	}

	public void update_shadow_moves() {
		if (!@shadow_moves || @shadow_moves.empty()) return;
		// Not a Shadow Pokémon (any more); relearn all its original moves
		if (!shadowPokemon()) {
			if (@shadow_moves.length > MAX_MOVES) {
				new_moves = new List<string>();
				@shadow_moves.each_with_index((m, i) => { if (m && i >= MAX_MOVES) new_moves.Add(m); });
				replace_moves(new_moves);
			}
			@shadow_moves = null;
			return;
		}
		// Is a Shadow Pokémon; ensure it knows the appropriate moves depending on its heart stage
		// Start with all Shadow moves
		new_moves = new List<string>();
		@shadow_moves.each_with_index((m, i) => { if (m && i < MAX_MOVES) new_moves.Add(m); });
		num_shadow_moves = new_moves.length;
		// Add some original moves (skipping ones in the same slot as a Shadow Move)
		num_original_moves = new {3, 3, 2, 1, 1, 0}[self.heartStage];
		if (num_original_moves > 0) {
			relearned_count = 0;
			@shadow_moves.each_with_index do |m, i|
				if (!m || i < MAX_MOVES + num_shadow_moves) continue;
				new_moves.Add(m);
				relearned_count += 1;
				if (relearned_count >= num_original_moves) break;
			}
		}
		// Relearn Shadow moves plus some original moves (may not change anything)
		replace_moves(new_moves);
	}

	public void replace_moves(new_moves) {
		// Forget any known moves that aren't in new_moves
		@moves.each_with_index do |m, i|
			if (!new_moves.Contains(m.id)) @moves[i] = null;
		}
		@moves.compact!;
		// Learn any moves in new_moves that aren't known
		foreach (var move in new_moves) { //'new_moves.each' do => |move|
			if (!move || !GameData.Move.exists(move) || hasMove(move)) continue;
			if (numMoves >= Pokemon.MAX_MOVES) break;
			learn_move(move);
		}
	}

	public bool purifiable() {
		if (!shadowPokemon() || self.heart_gauge > 0) return false;
		if (isSpecies(Speciess.LUGIA)) return false;
		return true;
	}

	public void check_ready_to_purify() {
		if (!shadowPokemon()) return;
		update_shadow_moves;
		if (self.heart_gauge == 0) Message(_INTL("{1} can now be purified!", self.name));
	}

	public void add_evs(added_evs) {
		total = 0;
		@ev.each_value(e => total += e);
		foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
			addition = added_evs[s.id].clamp(0, Pokemon.EV_STAT_LIMIT - @ev[s.id]);
			addition = addition.clamp(0, Pokemon.EV_LIMIT - total);
			if (addition == 0) continue;
			@ev[s.id] += addition;
			total += addition;
		}
	}

	unless (method_defined(:__shadow_clone)) alias __shadow_clone clone;
	public void clone() {
		ret = __shadow_clone;
		if (@saved_ev) {
			GameData.Stat.each_main(s => ret.saved_ev[s.id] = @saved_ev[s.id]);
		}
		if (@shadow_moves) ret.shadow_moves = @shadow_moves.clone;
		return ret;
	}
}
