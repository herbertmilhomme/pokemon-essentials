//===============================================================================
//
//===============================================================================
public partial class Player : Trainer {
	public int has_snag_machine		{ get { return _has_snag_machine; } set { _has_snag_machine = value; } }			protected int _has_snag_machine;
	public int seen_purify_chamber		{ get { return _seen_purify_chamber; } set { _seen_purify_chamber = value; } }			protected int _seen_purify_chamber;

	unless (private_method_defined(:__shadowPkmn__initialize)) alias __shadowPkmn__initialize initialize;
	public void initialize(name, trainer_type) {
		__shadowPkmn__initialize(name, trainer_type);
		@has_snag_machine    = false;
		@seen_purify_chamber = false;
	}
}

public partial class PokemonGlobalMetadata {
	public int purifyChamber		{ get { return _purifyChamber; } }			protected int _purifyChamber;

	public void purifyChamber() {
		if (!@purifyChamber) @purifyChamber = new PurifyChamber();
		return @purifyChamber;
	}
}

//===============================================================================
// General purpose utilities.
//===============================================================================
public void DrawGauge(bitmap, rect, color, value, maxValue) {
	if (!bitmap) return;
	bitmap.fill_rect(rect.x, rect.y, rect.width, rect.height, Color.black);
	width = (maxValue <= 0) ? 0 : (rect.width - 4) * value / maxValue;
	if (rect.width >= 4 && rect.height >= 4) {
		bitmap.fill_rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4, new Color(248, 248, 248));
		bitmap.fill_rect(rect.x + 2, rect.y + 2, width, rect.height - 4, color);
	}
}

// angle is in degrees.
public void calcPoint(x, y, distance, angle) {
	angle -= (int)Math.Floor(angle / 360.0) * 360;   // normalize
	angle = (angle / 360.0) * (2 * Math.PI);   // convert to radians
	angle = -angle % (2 * Math.PI);   // normalize radians
	point = new {distance * Math.cos(angle), distance * Math.sin(angle)};
	point[0] += x;
	point[1] += y;
	return point;
}

//===============================================================================
//
//===============================================================================
public partial class PurifyChamberSet {
	/// <summary>The Shadow Pokémon in the middle</summary>
	public int shadow		{ get { return _shadow; } }			protected int _shadow;
	/// <summary>Index in list of Pokémon the Shadow Pokémon is facing</summary>
	public int facing		{ get { return _facing; } }			protected int _facing;

	public void partialSum(x) {
		return ((x * x) + x) / 2;   // pattern: 1, 3, 6, 10, 15, 21, 28, ...
	}

	public void length() {
		return @list.length;
	}

	public void initialize() {
		@list = new List<string>();
		@facing = 0;
	}

	public int facing { set {
		if (value >= 0 && value < @list.length) @facing = value;
		}
	}

	public int shadow { set {
		if (value.null() || value.shadowPokemon()) @shadow = value;
		}
	}

	// Main component is tempo
	// Boosted if center has advantage over facing Pokemon
	// Boosted based on number of best circles
	public void flow() {
		ret = 0;
		if (!@shadow) return 0;
		for (int i = @list.length; i < @list.length; i++) { //for '@list.length' times do => |i|
			ret += (PurifyChamberSet.isSuperEffective(@list[i], @list[(i + 1) % @list.length])) ? 1 : 0;
		}
		if (@list[@facing]) {
			ret += PurifyChamberSet.isSuperEffective(@shadow, @list[@facing]) ? 1 : 0;
		}
		return ret + (@list.length / 2);
	}

	public void shadowAffinity() {
		if (@facing < 0 || @facing >= @list.length || !@shadow) return 0;
		return (PurifyChamberSet.isSuperEffective(@shadow, @list[@facing])) ? 2 : 1;
	}

	public void affinity(i) {
		if (i < 0 || i >= @list.length) return 0;
		return (PurifyChamberSet.isSuperEffective(@list[i], @list[(i + 1) % @list.length])) ? 2 : 1;
	}

	// Tempo refers to the type advantages of each Pokemon in a certain set in a
	// clockwise direction. Tempo also depends on the number of Pokemon in the set.
	public void tempo() {
		ret = 0;
		for (int i = @list.length; i < @list.length; i++) { //for '@list.length' times do => |i|
			ret += (PurifyChamberSet.isSuperEffective(@list[i], @list[(i + 1) % @list.length])) ? 1 : 0;
		}
		return partialSum(@list.length) + ret;
	}

	public void list() {
		return @list.clone;
	}

	public int this[int index] { get {
		return @list[index];
		}
	}

	public void insertAfter(index, value) {
		if (self.length >= PurifyChamber.SETSIZE) return;
		if (index < 0 || index >= PurifyChamber.SETSIZE) return;
		unless (value&.shadowPokemon()) {
			@list.insert(index + 1, value);
			@list.compact!;
			if (@facing > index && value) @facing += 1;
			@facing = (int)Math.Max((int)Math.Min(@facing, @list.length - 1), 0);
		}
	}

	public void insertAt(index, value) {
		if (index < 0 || index >= PurifyChamber.SETSIZE) return;
		unless (value&.shadowPokemon()) {
			@list[index] = value;
			@list.compact!;
			@facing = (int)Math.Max((int)Math.Min(@facing, @list.length - 1), 0);
		}
	}

	// Purify Chamber treats Normal/Normal matchup as super effective
	public static void typeAdvantage(type1, type2) {
		if (type1 == :NORMAL && type2 == :NORMAL) return true;
		return Effectiveness.super_effective_type(type1, type2);
	}

	public static void isSuperEffective(pkmn1, pkmn2) {
		foreach (var type1 in pkmn1.types) { //'pkmn1.types.each' do => |type1|
			foreach (var type2 in pkmn2.types) { //'pkmn2.types.each' do => |type2|
				if (typeAdvantage(type1, type2)) return true;
			}
		}
		return false;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PurifyChamber {
	public int sets		{ get { return _sets; } }			protected int _sets;
	public int currentSet		{ get { return _currentSet; } }			protected int _currentSet;

	public const int NUMSETS = 9;
	public const int SETSIZE = 4;

	// Calculates the maximum possible tempo.
	public static void maximumTempo() {
		x = SETSIZE + 1;
		return (((x * x) + x) / 2) - 1;
	}

	public void initialize() {
		@sets = new List<string>();
		@currentSet = 0;
		NUMSETS.times(i => @sets[i] = new PurifyChamberSet());
	}

	public int currentSet { set {
		if (value >= 0 && value < NUMSETS) @currentSet = value;
		}
	}

	// Number of regular Pokemon in a set
	public void setCount(set) {
		return @sets[set].length;
	}

	public void setList(set) {
		if (set < 0 || set >= NUMSETS) return [];
		return @sets[set].list;
	}

	// For speeding up purification.
	public void chamberFlow(chamber) {
		if (chamber < 0 || chamber >= NUMSETS) return 0;
		return @sets[chamber].flow;
	}

	public void getShadow(chamber) {
		if (chamber < 0 || chamber >= NUMSETS) return null;
		return @sets[chamber].shadow;
	}

	// Allow only Shadow Pokemon.
	public void setShadow(chamber, value) {
		if (chamber < 0 || chamber >= NUMSETS) return;
		@sets[chamber].shadow = value;
	}

	public void switch(set1, set2) {
		if (set1 < 0 || set1 >= NUMSETS) return;
		if (set2 < 0 || set2 >= NUMSETS) return;
		s = @sets[set1];
		@sets[set1] = @sets[set2];
		@sets[set2] = s;
	}

	public void insertAfter(set, index, value) {
		if (set < 0 || set >= NUMSETS) return;
		@sets[set].insertAfter(index, value);
	}

	public void insertAt(set, index, value) {
		if (set < 0 || set >= NUMSETS) return;
		@sets[set].insertAt(index, value);
	}

	public void [](chamber, slot = null() {
		if (slot.null()) return @sets[chamber];
		if (chamber < 0 || chamber >= NUMSETS) return null;
		if (slot < 0 || slot >= SETSIZE) return null;
		return @sets[chamber][slot];
	}

	public bool isPurifiableIgnoreRegular(set) {
		shadow = getShadow(set);
		if (!shadow) return false;
		if (shadow.heart_gauge != 0) return false;
		// Define an exception for Lugia
		if (shadow.isSpecies(Speciess.LUGIA)) {
			maxtempo = PurifyChamber.maximumTempo;
			for (int i = NUMSETS; i < NUMSETS; i++) { //for 'NUMSETS' times do => |i|
				if (@sets[i].tempo != maxtempo) return false;
			}
		}
		return true;
	}

	public bool isPurifiable(set) {
		isPurifiableIgnoreRegular(set) && setCount(set) > 0;
	}

	// Called upon each step taken in the overworld
	public void update() {
		for (int set = NUMSETS; set < NUMSETS; set++) { //for 'NUMSETS' times do => |set|
			if (!@sets[set].shadow || @sets[set].shadow.heart_gauge <= 0) continue;
			// If a Shadow Pokemon and a regular Pokemon are on the same set
			flow = self.chamberFlow(set);
			@sets[set].shadow.adjustHeart(-flow);
			if (!isPurifiable(set)) continue;
			Message(_INTL("Your {1} in the Purify Chamber is ready for purification!",
											@sets[set].shadow.name));
		}
	}

	public void debugAddShadow(set, species) {
		pkmn = new Pokemon(species, 1);
		pkmn.makeShadow;
		setShadow(set, pkmn);
	}

	public void debugAddNormal(set, species) {
		pkmn = new Pokemon(species, 1);
		insertAfter(set, setCount(set), pkmn);
	}

	public void debugAdd(set, shadow, type1, type2 = null) {
		pkmn = new PseudoPokemon(shadow, type1, type2 || type1);
		if (pkmn.shadowPokemon()) {
			self.setShadow(set, pkmn);
		} else {
			self.insertAfter(set, setCount(set), pkmn);
		}
	}
}

//===============================================================================
//
//===============================================================================
public static partial class PurifyChamberHelper {
	public static void GetPokemon2(chamber, set, position) {
		if (position == 0) {
			return chamber.getShadow(set);
		} else if (position > 0) {
			position -= 1;
			if (position.even()) {
				return chamber[set, position / 2];
			} else { // In between two indices
				return null;
			}
		}
		return null;
	}

	public static void GetPokemon(chamber, position) {
		if (position == 0) {
			return chamber.getShadow(chamber.currentSet);
		} else if (position > 0) {
			position -= 1;
			if (position.even()) {
				return chamber[chamber.currentSet, position / 2];
			} else { // In between two indices
				return null;
			}
		}
		return null;
	}

	public static void adjustOnInsert(position) {
		if (position > 0) {
			position -= 1;
			oldpos = position / 2;
			if (position.even()) {
				return position + 1;
			} else {
				return ((oldpos + 1) * 2) + 1;
			}
		}
		return position;
	}

	public static void SetPokemon(chamber, position, value) {
		if (position == 0) {
			chamber.setShadow(chamber.currentSet, value);
		} else if (position > 0) {
			position -= 1;
			if (position.even()) {
				chamber.insertAt(chamber.currentSet, position / 2, value);
			} else { // In between two indices
				chamber.insertAfter(chamber.currentSet, position / 2, value);
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class PurifyChamberScreen {
	public void initialize(scene) {
		@scene = scene;
		@chamber = Game.GameData.PokemonGlobal.purifyChamber;
//    for (int j = 0; j < PurifyChamber.NUMSETS; j++) {
//      @chamber.debugAddShadow(j,rand(100)+1)
//      @chamber[j].shadow.heart_gauge = 0
//      for (int i = 0; i < PurifyChamber.SETSIZE; i++) {
//        @chamber.debugAddNormal(j,rand(100)+1)
//      }
//    }
	}

	public void Place(pkmn, position) {
		if (!pkmn) return false;
		if (pkmn.egg()) {
			@scene.Display(_INTL("Can't place an egg there."));
			return false;
		}
		if (position == 0) {
			if (pkmn.shadowPokemon()) {
				// Remove from storage and place in set
				oldpkmn = PurifyChamberHelper.GetPokemon(@chamber, position);
				if (oldpkmn) {
					@scene.Shift(position, pkmn);
				} else {
					@scene.Place(position, pkmn);
				}
				PurifyChamberHelper.SetPokemon(@chamber, position, pkmn);
				@scene.Refresh;
			} else {
				@scene.Display(_INTL("Only a Shadow Pokémon can go there."));
				return false;
			}
		} else if (position >= 1) {
			if (pkmn.shadowPokemon()) {
				@scene.Display(_INTL("Can't place a Shadow Pokémon there."));
				return false;
			} else {
				oldpkmn = PurifyChamberHelper.GetPokemon(@chamber, position);
				if (oldpkmn) {
					@scene.Shift(position, pkmn);
				} else {
					@scene.Place(position, pkmn);
				}
				PurifyChamberHelper.SetPokemon(@chamber, position, pkmn);
				@scene.Refresh;
			}
		}
		return true;
	}

	public void PlacePokemon(pos, position) {
		if (!pos) return false;
		pkmn = Game.GameData.PokemonStorage[pos[0], pos[1]];
		if (Place(pkmn, position)) {
			Game.GameData.PokemonStorage.Delete(pos[0], pos[1]);
			return true;
		}
		return false;
	}

	public void OnPlace(pkmn) {
		set = @chamber.currentSet;
		if (@chamber.setCount(set) == 0 && @chamber.isPurifiableIgnoreRegular(set)) {
			pkmn = @chamber.getShadow(set);
			@scene.Display(
				_INTL("This {1} is ready to open its heart. However, there must be at least one regular Pokémon in the set to perform a purification ceremony.",
							pkmn.name)
			);
		}
	}

	public void OpenSetDetail() {
		chamber = @chamber;
		@scene.OpenSetDetail(chamber.currentSet);
		heldpkmn = null;
		do { //loop; while (true);
			// Commands
			// array[0]==0 - a position was chosen
			// array[0]==1 - a new set was chosen
			// array[0]==2 - choose Pokemon command
			cmd = @scene.SetScreen;
			switch (cmd[0]) {
				case 0:   // Place Pokemon in the set
					curpkmn = PurifyChamberHelper.GetPokemon(@chamber, cmd[1]);
					if (curpkmn || heldpkmn) {
						commands = new {_INTL("MOVE"), _INTL("SUMMARY"), _INTL("WITHDRAW")};
						if (curpkmn && heldpkmn) {
							commands[0] = _INTL("EXCHANGE");
						} else if (heldpkmn) {
							commands[0] = _INTL("PLACE");
						}
						cmdReplace = -1;
						cmdRotate = -1;
						if (!heldpkmn && curpkmn && cmd[1] == 0 &&
							@chamber[@chamber.currentSet].length > 0) {
							commands[cmdRotate = commands.length] = _INTL("ROTATE");
						}
						if (!heldpkmn && curpkmn) {
							commands[cmdReplace = commands.length] = _INTL("REPLACE");
						}
						commands.Add(_INTL("CANCEL"));
						choice = @scene.ShowCommands(
							_INTL("What shall I do with this {1}?", heldpkmn ? heldpkmn.name : curpkmn.name),
							commands
						);
						if (choice == 0) {
							if (heldpkmn) {
								if (Place(heldpkmn, cmd[1])) { // calls place or shift as appropriate
									if (curpkmn) {
										heldpkmn = curpkmn; // Pokemon was shifted
									} else {
										OnPlace(heldpkmn);
										@scene.PositionHint(PurifyChamberHelper.adjustOnInsert(cmd[1]));
										heldpkmn = null; // Pokemon was placed
									}
								}
							} else {
								@scene.Move(cmd[1]);
								PurifyChamberHelper.SetPokemon(@chamber, cmd[1], null);
								@scene.Refresh;
								heldpkmn = curpkmn;
							}
						} else if (choice == 1) {
							@scene.Summary(cmd[1], heldpkmn);
						} else if (choice == 2) {
							if (BoxesFull()) {
								@scene.Display(_INTL("All boxes are full."));
							} else if (heldpkmn) {
								@scene.Withdraw(cmd[1], heldpkmn);
								Game.GameData.PokemonStorage.StoreCaught(heldpkmn);
								heldpkmn = null;
								@scene.Refresh;
							} else {
								// Store and delete Pokemon.
								@scene.Withdraw(cmd[1], heldpkmn);
								Game.GameData.PokemonStorage.StoreCaught(curpkmn);
								PurifyChamberHelper.SetPokemon(@chamber, cmd[1], null);
								@scene.Refresh;
							}
						} else if (cmdRotate >= 0 && choice == cmdRotate) {
							count = @chamber[@chamber.currentSet].length;
							nextPos = @chamber[@chamber.currentSet].facing;
							if (count > 0) {
								@scene.Rotate((nextPos + 1) % count);
								@chamber[@chamber.currentSet].facing = (nextPos + 1) % count;
								@scene.Refresh;
							}
						} else if (cmdReplace >= 0 && choice == cmdReplace) {
							pos = @scene.ChoosePokemon;
							if (pos) {
								newpkmn = Game.GameData.PokemonStorage[pos[0], pos[1]];
								if (newpkmn) {
									if ((newpkmn.shadowPokemon()) == (curpkmn.shadowPokemon())) {
										@scene = System.Text.RegularExpressions.Regex.Replace(@scene, cmd, pos);
										PurifyChamberHelper.SetPokemon(@chamber, cmd[1], newpkmn);
										Game.GameData.PokemonStorage[pos[0], pos[1]] = curpkmn;
										@scene.Refresh;
										OnPlace(curpkmn);
									} else {
										@scene.Display(_INTL("That Pokémon can't be placed there."));
									}
								}
							}
						}
					} else {   // No current Pokemon
						pos = @scene.ChoosePokemon;
						if (PlacePokemon(pos, cmd[1])) {
							curpkmn = PurifyChamberHelper.GetPokemon(@chamber, cmd[1]);
							OnPlace(curpkmn);
							@scene.PositionHint(PurifyChamberHelper.adjustOnInsert(cmd[1]));
						}
					}
					break;
				case 1:   // Change the active set
					@scene.ChangeSet(cmd[1]);
					chamber.currentSet = cmd[1];
					break;
				case 2:   // Choose a Pokemon
					pos = @scene.ChoosePokemon;
					pkmn = pos ? Game.GameData.PokemonStorage[pos[0], pos[1]] : null;
					if (pkmn) heldpkmn = pkmn;
					break;
				default:   // cancel
					if (heldpkmn) {
						@scene.Display(_INTL("You're holding a Pokémon!"));
					} else {
						if (!@scene.Confirm(_INTL("Continue editing sets?"))) break;
					}
					break;
			}
		}
		if (CheckPurify) {
			@scene.Display(_INTL("There is a Pokémon that is ready to open its heart!") + "\1");
			@scene.CloseSetDetail;
			DoPurify;
			return false;
		} else {
			@scene.CloseSetDetail;
			return true;
		}
	}

	public void Display(msg) {
		@scene.Display(msg);
	}

	public void Confirm(msg) {
		@scene.Confirm(msg);
	}

	public void Refresh() {
		@scene.Refresh;
	}

	public void CheckPurify() {
		purifiables = new List<string>();
		for (int set = PurifyChamber.NUMSETS; set < PurifyChamber.NUMSETS; set++) { //for 'PurifyChamber.NUMSETS' times do => |set|
			if (@chamber.isPurifiable(set)) { // if ready for purification
				purifiables.Add(set);
			}
		}
		return purifiables.length > 0;
	}

	public void DoPurify() {
		purifiables = new List<string>();
		for (int set = PurifyChamber.NUMSETS; set < PurifyChamber.NUMSETS; set++) { //for 'PurifyChamber.NUMSETS' times do => |set|
			if (@chamber.isPurifiable(set)) { // if ready for purification
				purifiables.Add(set);
			}
		}
		for (int i = purifiables.length; i < purifiables.length; i++) { //for 'purifiables.length' times do => |i|
			set = purifiables[i];
			@chamber.currentSet = set;
			@scene.OpenSet(set);
			@scene.Purify;
			Purify(@chamber[set].shadow, self);
			StorePokemon(@chamber[set].shadow);
			@chamber.setShadow(set, null); // Remove shadow Pokemon from set
			if ((i + 1) != purifiables.length) {
				@scene.Display(_INTL("There is another Pokémon that is ready to open its heart!") + "\1");
				if (!@scene.Confirm(_INTL("Would you like to switch sets?"))) {
					@scene.CloseSet;
					break;
				}
			}
			@scene.CloseSet;
		}
	}

	public void StartPurify() {
		chamber = @chamber;
		@scene.Start(chamber);
		if (CheckPurify) {
			DoPurify;
			@scene.End;
			return;
		}
		@scene.OpenSet(chamber.currentSet);
		do { //loop; while (true);
			set = @scene.ChooseSet;
			if (set < 0) {
				if (!@scene.Confirm(_INTL("Continue viewing holograms?"))) break;
			} else {
				chamber.currentSet = set;
				cmd = @scene.ShowCommands(_INTL("What do you want to do?"),
																		new {_INTL("EDIT"), _INTL("SWITCH"), _INTL("CANCEL")});
				switch (cmd) {
					case 0:   // edit
						if (!OpenSetDetail) break;
						break;
					case 1:   // switch
						chamber.currentSet = set;
						newSet = @scene.Switch(set);
						chamber.switch(set, newSet);
						chamber.currentSet = newSet;
						@scene.Refresh;
						break;
				}
			}
		}
		@scene.CloseSet;
		@scene.End;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Window_PurifyChamberSets : Window_DrawableCommand {
	public int switching		{ get { return _switching; } }			protected int _switching;

	public override void initialize(chamber, x, y, width, height, viewport = null) {
		@chamber = chamber;
		@switching = -1;
		base.initialize(x, y, width, height, viewport);
	}

	public void itemCount() {
		return PurifyChamber.NUMSETS;
	}

	public int switching { set {
		@switching = value;
		refresh;
		}
	}

	public void drawItem(index, _count, rect) {
		textpos = new List<string>();
		rect = drawCursor(index, rect);
		if (index == @switching) {
			textpos.Add(new {(index + 1).ToString(), rect.x, rect.y, :left, new Color(248, 0, 0), self.shadowColor});
		} else {
			textpos.Add(new {(index + 1).ToString(), rect.x, rect.y, :left, self.baseColor, self.shadowColor});
		}
		if (@chamber.setCount(index) > 0) {
			DrawGauge(self.contents, new Rect(rect.x + 16, rect.y + 6, 48, 8),
									new Color(0, 0, 256), @chamber[index].tempo, PurifyChamber.maximumTempo);
		}
		if (@chamber.getShadow(index)) {
			DrawGauge(self.contents, new Rect(rect.x + 16, rect.y + 18, 48, 8),
									new Color(192, 0, 256),
									@chamber.getShadow(index).heart_gauge,
									@chamber.getShadow(index).max_gauge_size);
		}
		DrawTextPositions(self.contents, textpos);
	}
}

//===============================================================================
//
//===============================================================================
public partial class DirectFlowDiagram {
	// Distance travelled by a dot in 1 second.
	public const int DOT_SPEED = 80;

	public void initialize(viewport = null) {
		@points = new List<string>();
		@angles = new List<string>();
		@viewport = viewport;
		@strength = 0;
		@offset = 0;
		@x = 306;
		@y = 158;
		@distance = 96;
	}

	public void dispose() {
		@points.each(point => point.dispose);
	}

	// 0=none, 1=weak, 2=strong
	public void setFlowStrength(strength) {
		@strength = strength;
	}

	public int visible { set {
		@points.each { |point| point.visible = value 	};
	}
	}

	public int color { set {
		@points.each { |point| point.color = value 	};
	}
	}

	public void setAngle(angle1) {
		@angle = angle1 - ((int)Math.Floor(angle1 / 360) * 360);
	}

	public void ensurePoint(j) {
		if (!@points[j] || @points[j].disposed()) {
			@points[j] = new BitmapSprite(8, 8, @viewport);
			@points[j].bitmap.fill_rect(0, 0, 8, 8, Color.black);
		}
		@points[j].tone = (@strength == 2) ? new Tone(232, 232, 248) : new Tone(16, 16, 232);
		@points[j].visible = (@strength != 0);
	}

	public void update() {
		@points.each do |point|
			point.update;
			point.visible = false;
		}
		j = 0;
		i = 0;
		while (i < @distance) {
			o = (i + @offset) % @distance;
			if (o >= 0 && o < @distance) {
				ensurePoint(j);
				pt = calcPoint(@x, @y, o, @angle);
				@points[j].x = pt[0];
				@points[j].y = pt[1];
				j += 1;
			}
			i += (@strength == 2) ? 16 : 32;
		}
		offset_delta = System.uptime * DOT_SPEED;
		if (@strength == 2) offset_delta *= 1.5;
		@offset = offset_delta % @distance;
	}
}

//===============================================================================
//
//===============================================================================
public partial class FlowDiagram {
	// Distance travelled by a dot in 1 second.
	public const int DOT_SPEED = 80;

	public void initialize(viewport = null) {
		@points = new List<string>();
		@angles = new List<string>();
		@viewport = viewport;
		@strength = 0;
		@offset = 0;
		@x = 306;
		@y = 158;
		@distance = 96;
	}

	public void dispose() {
		@points.each(point => point.dispose);
	}

	public int visible { set {
		@points.each { |point| point.visible = value 	};
	}
	}

	public int color { set {
		@points.each { |point| point.color = value 	};
	}
	}

	// 0=none, 1=weak, 2=strong
	public void setFlowStrength(strength) {
		@strength = strength;
	}

	public void ensurePoint(j) {
		if (!@points[j] || @points[j].disposed()) {
			@points[j] = new BitmapSprite(8, 8, @viewport);
			@points[j].bitmap.fill_rect(0, 0, 8, 8, Color.black);
		}
		@points[j].tone = (@strength == 2) ? new Tone(232, 232, 248) : new Tone(16, 16, 232);
		@points[j].visible = (@strength != 0);
	}

	public void setRange(angle1, angle2) {
		@startAngle = angle1 - ((int)Math.Floor(angle1 / 360) * 360);
		@endAngle = angle2 - ((int)Math.Floor(angle2 / 360) * 360);
		if (@startAngle == @endAngle && angle1 != angle2) {
			@startAngle = 0;
			@endAngle = 359.99;
		}
	}

	public void withinRange(angle, startAngle, endAngle) {
		if (startAngle > endAngle) {
			return (angle >= startAngle || angle <= endAngle) &&
						(angle >= 0 && angle <= 360);
		} else {
			return (angle >= startAngle && angle <= endAngle);
		}
	}

	public void update() {
		@points.each do |point|
			point.update;
			point.visible = false;
		}
		j = 0;
		i = 0;
		while (i < 360) {
			angle = (i + @offset) % 360;
			if (withinRange(angle, @startAngle, @endAngle)) {
				ensurePoint(j);
				pt = calcPoint(@x, @y, @distance, angle);
				@points[j].x = pt[0];
				@points[j].y = pt[1];
				j += 1;
			}
			i += (@strength == 2) ? 10 : 20;
		}
		offset_delta = -System.uptime * DOT_SPEED;
		if (@strength == 2) offset_delta *= 1.5;
		@offset = offset_delta % (360 * 6);
	}
}

//===============================================================================
//
//===============================================================================
public partial class PurifyChamberSetView : Sprite {
	public int set		{ get { return _set; } }			protected int _set;
	public int cursor		{ get { return _cursor; } }			protected int _cursor;
	public int heldpkmn		{ get { return _heldpkmn; } }			protected int _heldpkmn;

	public override void initialize(chamber, set, viewport = null) {
		base.initialize(viewport);
		@set = set;
		@heldpkmn = null;
		@cursor = -1;
		@view = new BitmapSprite(64, 64, viewport);
		@view.bitmap.fill_rect(8, 8, 48, 48, Color.white);
		@view.bitmap.fill_rect(10, 10, 44, 44, new Color(255, 255, 255, 128));
		@info = new BitmapSprite(Graphics.width - 112, 48, viewport);
		@flows = new List<string>();
		@directflow = new DirectFlowDiagram(viewport);
		@directflow.setAngle(0);
		@directflow.setFlowStrength(1);
		@__sprites = new List<string>();
		@__sprites[0] = new PokemonIconSprite(null, viewport);
		@__sprites[0].setOffset;
		for (int i = (PurifyChamber.SETSIZE * 2); i < (PurifyChamber.SETSIZE * 2); i++) { //for '(PurifyChamber.SETSIZE * 2)' times do => |i|
			@__sprites[i + 1] = new PokemonIconSprite(null, viewport);
			@__sprites[i + 1].setOffset;
		}
		@__sprites[1 + (PurifyChamber.SETSIZE * 2)] = new PokemonIconSprite(null, viewport);
		@__sprites[1 + (PurifyChamber.SETSIZE * 2)].setOffset;
		@chamber = chamber;
		refresh;
	}

	public void refreshFlows() {
		@flows.each do |flow|
			flow.setFlowStrength(0);
		}
		setcount = @chamber.setCount(@set);
		for (int i = setcount; i < setcount; i++) { //for 'setcount' times do => |i|
			if (!@flows[i]) @flows[i] = new FlowDiagram(self.viewport);
			angle = 360 - (i * 360 / setcount);
			angle += 90;   // start at 12 not 3 o'clock
			endAngle = angle - (360 / setcount);
			@flows[i].setRange(endAngle, angle);
			@flows[i].setFlowStrength(@chamber[@set].affinity(i));
		}
	}

	public void moveCursor(button) {
		points = (int)Math.Max(@chamber.setCount(@set) * 2, 1);
		oldcursor = @cursor;
		if (@cursor == 0 && points > 0) {
			if (button == Input.UP) @cursor = 1;
			@cursor = (points * 1 / 4) + 1 if button == Input.RIGHT;
			@cursor = (points * 2 / 4) + 1 if button == Input.DOWN;
			@cursor = (points * 3 / 4) + 1 if button == Input.LEFT;
		} else if (@cursor > 0) {
			pos = @cursor - 1;
			if (@chamber.setCount(@set) == PurifyChamber.SETSIZE) {
				points = (int)Math.Max(points / 2, 1);
				pos /= 2;
			}
			seg = pos * 8 / points;
			switch (seg) {
				case 7: case 0:
					if (button == Input.LEFT) pos -= 1;
					if (button == Input.RIGHT) pos += 1;
					if (button == Input.DOWN) pos = null;
					break;
				case 1: case 2:
					if (button == Input.UP) pos -= 1;
					if (button == Input.DOWN) pos += 1;
					if (button == Input.LEFT) pos = null;
					break;
				case 3: case 4:
					if (button == Input.RIGHT) pos -= 1;
					if (button == Input.LEFT) pos += 1;
					if (button == Input.UP) pos = null;
					break;
				case 5: case 6:
					if (button == Input.DOWN) pos -= 1;
					if (button == Input.UP) pos += 1;
					if (button == Input.RIGHT) pos = null;
					break;
			}
			if (pos.null()) {
				@cursor = 0;
			} else {
				pos -= (int)Math.Floor(pos / points) * points;   // modulus
				if (@chamber.setCount(@set) == PurifyChamber.SETSIZE) pos *= 2;
				@cursor = pos + 1;
			}
		}
		if (@cursor != oldcursor) refresh;
	}

	public void checkCursor(index) {
		if (@cursor == index) {
			@view.x = @__sprites[index].x - (@view.bitmap.width / 2);
			@view.y = @__sprites[index].y - (@view.bitmap.height / 2);
			@view.visible = true;
		}
	}

	public void refresh() {
		pkmn = @chamber.getShadow(@set);
		@view.visible = false;
		@info.bitmap.fill_rect(0, 0, @info.bitmap.width, @info.bitmap.height, new Color(0, 248, 0));
		SetSmallFont(@info.bitmap);
		textpos = new List<string>();
		if (pkmn) {
			type_string = "";
			pkmn.types.each_with_index do |type, i|
				if (i > 0) type_string += "/";
				type_string += GameData.Type.get(type).name;
			}
			textpos.Add(new {_INTL("{1}  Lv.{2}  {3}", pkmn.name, pkmn.level, type_string),
										2, 6, :left, new Color(248, 248, 248), new Color(128, 128, 128)});
			textpos.Add(new {_INTL("FLOW"), 2 + (@info.bitmap.width / 2), 30, :left,
										new Color(248, 248, 248), new Color(128, 128, 128)});
			// draw heart gauge
			DrawGauge(@info.bitmap, new Rect(@info.bitmap.width * 3 / 4, 8, @info.bitmap.width * 1 / 4, 8),
									new Color(192, 0, 256), pkmn.heart_gauge, pkmn.max_gauge_size);
			// draw flow gauge
			DrawGauge(@info.bitmap, new Rect(@info.bitmap.width * 3 / 4, 32, @info.bitmap.width * 1 / 4, 8),
									new Color(0, 0, 248), @chamber.chamberFlow(@set), 7);
		}
		if (@chamber.setCount(@set) > 0) {
			textpos.Add(new {_INTL("TEMPO"), 2, 30, :left, new Color(248, 248, 248), new Color(128, 128, 128)});
			// draw tempo gauge
			DrawGauge(@info.bitmap, new Rect(@info.bitmap.width * 1 / 4, 32, @info.bitmap.width * 1 / 4, 8),
									new Color(0, 0, 248), @chamber[@set].tempo, PurifyChamber.maximumTempo);
		}
		DrawTextPositions(@info.bitmap, textpos);
		@info.x = Graphics.width - @info.bitmap.width;
		@info.y = Graphics.height - @info.bitmap.height;
		@__sprites[0].pokemon = pkmn;
		@__sprites[0].x = 306;
		@__sprites[0].y = 158;
		@__sprites[0].z = 2;
		@directflow.setAngle(angle);
		@directflow.setFlowStrength(0);
		checkCursor(0);
		points = (int)Math.Max(@chamber.setCount(@set) * 2, 1);
		setList = @chamber.setList(@set);
		refreshFlows;
		for (int i = (PurifyChamber.SETSIZE * 2); i < (PurifyChamber.SETSIZE * 2); i++) { //for '(PurifyChamber.SETSIZE * 2)' times do => |i|
			pkmn = (i.odd() || i >= points) ? null : setList[i / 2];
			angle = 360 - (i * 360 / points);
			angle += 90;   // start at 12 not 3 o'clock
			if (pkmn && @chamber[@set].facing == i / 2) {
				@directflow.setAngle(angle);
				@directflow.setFlowStrength(@chamber[@set].shadowAffinity);
			}
			point = calcPoint(306, 158, 96, angle);
			@__sprites[i + 1].x = point[0];
			@__sprites[i + 1].y = point[1];
			@__sprites[i + 1].z = 2;
			@__sprites[i + 1].pokemon = pkmn;
			checkCursor(i + 1);
		}
		@__sprites[1 + (PurifyChamber.SETSIZE * 2)].pokemon = @heldpkmn;
		@__sprites[1 + (PurifyChamber.SETSIZE * 2)].visible = @view.visible;
		@__sprites[1 + (PurifyChamber.SETSIZE * 2)].x = @view.x + (@view.bitmap.width / 2);
		@__sprites[1 + (PurifyChamber.SETSIZE * 2)].y = @view.y + (@view.bitmap.height / 2);
		@__sprites[1 + (PurifyChamber.SETSIZE * 2)].z = 3;
	}

	public void getCurrent() {
		return PurifyChamberHelper.GetPokemon(@chamber, @cursor);
	}

	public int cursor { set {
		@cursor = value;
		refresh;
		}
	}

	public int heldpkmn { set {
		@heldpkmn = value;
		refresh;
		}
	}

	public int set { set {
		@set = value;
		refresh;
		}
	}

	public override int visible { set {
		base.visible();
		@__sprites.each do |sprite|
			sprite.visible = value;
		}
		@flows.each do |flow|
			flow.visible = value;
		}
		@directflow.visible = value;
		@view.visible = value;
		@info.visible = value;
		}
	}

	public override int color { set {
		base.color();
		@__sprites.each do |sprite|
			sprite.color = value.clone;
		}
		@flows.each do |flow|
			flow.color = value.clone;
		}
		@directflow.color = value.clone;
		@view.color = value.clone;
		@info.color = value.clone;
		}
	}

	public override void update() {
		base.update();
		@__sprites.each do |sprite|
			sprite&.update;
		}
		@flows.each do |flow|
			flow.update;
		}
		@directflow.update;
		@view.update;
		@info.update;
	}

	public void dispose() {
		@__sprites.each do |sprite|
			sprite&.dispose;
		}
		@flows.each do |flow|
			flow.dispose;
		}
		@directflow.dispose;
		@view.dispose;
		@info.dispose;
		@__sprites.clear;
		super;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PurifyChamberScene {
	public void Update() {
		UpdateSpriteHash(@sprites);
	}

	public void Refresh() {
		if (@sprites["setview"]) {
			@sprites["setview"].refresh;
			@sprites["setwindow"].refresh;
		}
	}

	public void Start(chamber) {
		@chamber = chamber;
	}

	public void End() { }

	public void OpenSet(set) {
		@sprites = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@viewportmsg = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewportmsg.z = 99999;
		addBackgroundOrColoredPlane(@sprites, "bg", "purifychamber_bg",
																new Color(64, 48, 96), @viewport);
		@sprites["setwindow"] = new Window_PurifyChamberSets(
			@chamber, 0, 0, 112, Graphics.height, @viewport
		);
		@sprites["setview"] = new PurifyChamberSetView(@chamber, set, @viewport);
		@sprites["msgwindow"] = new Window_AdvancedTextPokemon("");
		@sprites["msgwindow"].viewport = @viewportmsg;
		@sprites["msgwindow"].visible = false;
		@sprites["setwindow"].index = set;
		DeactivateWindows(@sprites);
		FadeInAndShow(@sprites) { Update };
	}

	public void CloseSet() {
		FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
		@viewportmsg.dispose;
	}

	public void OpenSetDetail(set) {
		@sprites["setwindow"].index = set;
		@sprites["setview"].set = set;
		@sprites["setview"].cursor = 0;
	}

	public void CloseSetDetail() { }

	public void Purify() {
		Refresh;
	}

	public void Move(_pos) {
		@sprites["setview"].heldpkmn = @sprites["setview"].getCurrent;
		Refresh;
	}

	public void Shift(_pos, _heldpoke) {
		@sprites["setview"].heldpkmn = @sprites["setview"].getCurrent;
		Refresh;
	}

	public void Place(_pos, _heldpoke) {
		@sprites["setview"].heldpkmn = null;
		Refresh;
	}

	public void Replace(_pos, _storagePos) {
		@sprites["setview"].heldpkmn = null;
		Refresh;
	}

	public void Rotate(facing) {}

	public void Withdraw(_pos, _heldpoke) {
		@sprites["setview"].heldpkmn = null;
		Refresh;
	}

	public void Display(msg) {
		UIHelper.Display(@sprites["msgwindow"], msg, false) { Update };
	}

	public void Confirm(msg) {
		UIHelper.Confirm(@sprites["msgwindow"], msg) { Update };
	}

	public void ShowCommands(msg, commands) {
		UIHelper.ShowCommands(@sprites["msgwindow"], msg, commands) { Update };
	}

	public void SetScreen() {
		DeactivateWindows(@sprites) do;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				Update;
				btn = 0;
				if (Input.repeat(Input.DOWN)) btn = Input.DOWN;
				if (Input.repeat(Input.UP)) btn = Input.UP;
				if (Input.repeat(Input.RIGHT)) btn = Input.RIGHT;
				if (Input.repeat(Input.LEFT)) btn = Input.LEFT;
				if (btn != 0) {
					PlayCursorSE;
					@sprites["setview"].moveCursor(btn);
				}
				if (Input.repeat(Input.JUMPUP)) {
					nextset = (@sprites["setview"].set == 0) ? PurifyChamber.NUMSETS - 1 : @sprites["setview"].set - 1;
					PlayCursorSE;
					return new {1, nextset};
				} else if (Input.repeat(Input.JUMPDOWN)) {
					nextset = (@sprites["setview"].set == PurifyChamber.NUMSETS - 1) ? 0 : @sprites["setview"].set + 1;
					PlayCursorSE;
					return new {1, nextset};
				} else if (Input.trigger(Input.USE)) {
					PlayDecisionSE;
					return new {0, @sprites["setview"].cursor};
				} else if (Input.trigger(Input.BACK)) {
					PlayCancelSE
					return new {3, 0};
				}
			}
		}
	}

	public void ChooseSet() {
		ActivateWindow(@sprites, "setwindow") do;
			oldindex = @sprites["setwindow"].index;
			do { //loop; while (true);
				if (oldindex != @sprites["setwindow"].index) {
					oldindex = @sprites["setwindow"].index;
					@sprites["setview"].set = oldindex;
				}
				Graphics.update;
				Input.update;
				Update;
				if (Input.trigger(Input.USE)) {
					PlayDecisionSE;
					return @sprites["setwindow"].index;
				}
				if (Input.trigger(Input.BACK)) {
					PlayCancelSE
					return -1;
				}
			}
		}
	}

	public void Switch(set) {
		@sprites["setwindow"].switching = set;
		ret = ChooseSet;
		@sprites["setwindow"].switching = -1;
		return ret < 0 ? set : ret;
	}

	public void Summary(pos, heldpkmn) {
		if (heldpkmn) {
			oldsprites = FadeOutAndHide(@sprites);
			scene = new PokemonSummary_Scene();
			screen = new PokemonSummaryScreen(scene);
			screen.StartScreen([heldpkmn], 0);
			FadeInAndShow(@sprites, oldsprites);
			return;
		}
		party = new List<string>();
		indexes = new List<string>();
		startindex = 0;
		set = @sprites["setview"].set;
		for (int i = (@chamber.setCount(set) * 2); i < (@chamber.setCount(set) * 2); i++) { //for '(@chamber.setCount(set) * 2)' times do => |i|
			p = PurifyChamberHelper.GetPokemon2(@chamber, set, i);
			if (!p) continue;
			if (i == pos) startindex = party.length;
			party.Add(p);
			indexes.Add(i);
		}
		if (party.length == 0) return;
		oldsprites = FadeOutAndHide(@sprites);
		scene = new PokemonSummary_Scene();
		screen = new PokemonSummaryScreen(scene);
		selection = screen.StartScreen(party, startindex);
		@sprites["setview"].cursor = indexes[selection];
		FadeInAndShow(@sprites, oldsprites);
	}

	public void PositionHint(pos) {
		@sprites["setview"].cursor = pos;
		Refresh;
	}

	public void ChangeSet(set) {
		@sprites["setview"].set = set;
		@sprites["setwindow"].index = set;
		@sprites["setwindow"].refresh;
		Refresh;
	}

	public void ChoosePokemon() {
		visible = FadeOutAndHide(@sprites);
		scene = new PokemonStorageScene();
		screen = new PokemonStorageScreen(scene, Game.GameData.PokemonStorage);
		pos = screen.ChoosePokemon;
		Refresh;
		FadeInAndShow(@sprites, visible);
		return pos;
	}
}

//===============================================================================
//
//===============================================================================
public void PurifyChamber() {
	Game.GameData.player.seen_purify_chamber = true;
	FadeOutIn do;
		scene = new PurifyChamberScene();
		screen = new PurifyChamberScreen(scene);
		screen.StartPurify;
	}
}

//===============================================================================
//
//===============================================================================

MenuHandlers.add(:pc_menu, :purify_chamber, {
	"name"      => _INTL("Purify Chamber"),
	"order"     => 30,
	"condition" => () => next Game.GameData.player.seen_purify_chamber,
	"effect"    => menu => {
		Message("\\se[PC access]" + _INTL("Accessed the Purify Chamber."));
		PurifyChamber;
		next false;
	}
});
