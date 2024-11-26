//===============================================================================
//
//===============================================================================
public partial class PokemonBox {
	public int pokemon		{ get { return _pokemon; } }			protected int _pokemon;
	public int name		{ get { return _name; } set { _name = value; } }			protected int _name;
	public int background		{ get { return _background; } set { _background = value; } }			protected int _background;

	public const int BOX_WIDTH  = 6;
	public const int BOX_HEIGHT = 5;
	public const int BOX_SIZE   = BOX_WIDTH * BOX_HEIGHT;

	public void initialize(name, maxPokemon = BOX_SIZE) {
		@name = name;
		@background = 0;
		@pokemon = new List<string>();
		maxPokemon.times(i => @pokemon[i] = null);
	}

	public void length() {
		return @pokemon.length;
	}

	public void nitems() {
		ret = 0;
		@pokemon.each(pkmn => { if (!pkmn.null()) ret += 1; });
		return ret;
	}

	public bool full() {
		return nitems == self.length;
	}

	public bool empty() {
		return nitems == 0;
	}

	public int this[int i] { get {
		return @pokemon[i];
		}
	}

	public int this[(i, value)] { get {
		@pokemon[i] = value;
		}
	}

	public void each() {
		@pokemon.each(item => yield item);
	}

	public void clear() {
		@pokemon.clear;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonStorage {
	public int boxes		{ get { return _boxes; } }			protected int _boxes;
	public int currentBox		{ get { return _currentBox; } set { _currentBox = value; } }			protected int _currentBox;
	public int unlockedWallpapers		{ get { return _unlockedWallpapers; } }			protected int _unlockedWallpapers;

	public const int BASICWALLPAPERQTY = 16;

	public void initialize(maxBoxes = Settings.NUM_STORAGE_BOXES, maxPokemon = PokemonBox.BOX_SIZE) {
		@boxes = new List<string>();
		for (int i = maxBoxes; i < maxBoxes; i++) { //for 'maxBoxes' times do => |i|
			@boxes[i] = new PokemonBox(_INTL("Box {1}", i + 1), maxPokemon);
			@boxes[i].background = i % BASICWALLPAPERQTY;
		}
		@currentBox = 0;
		@boxmode = -1;
		@unlockedWallpapers = new List<string>();
		for (int i = allWallpapers.length; i < allWallpapers.length; i++) { //for 'allWallpapers.length' times do => |i|
			@unlockedWallpapers[i] = false;
		}
	}

	public void allWallpapers() {
		return new {
			// Basic wallpapers
			_INTL("Forest"), _INTL("City"), _INTL("Desert"), _INTL("Savanna"),
			_INTL("Crag"), _INTL("Volcano"), _INTL("Snow"), _INTL("Cave"),
			_INTL("Beach"), _INTL("Seafloor"), _INTL("River"), _INTL("Sky"),
			_INTL("Poké Center"), _INTL("Machine"), _INTL("Checks"), _INTL("Simple"),
			// Special wallpapers
			_INTL("Space"), _INTL("Backyard"), _INTL("Nostalgic 1"), _INTL("Torchic"),
			_INTL("Trio 1"), _INTL("PikaPika 1"), _INTL("Legend 1"), _INTL("Team Galactic 1"),
			_INTL("Distortion"), _INTL("Contest"), _INTL("Nostalgic 2"), _INTL("Croagunk"),
			_INTL("Trio 2"), _INTL("PikaPika 2"), _INTL("Legend 2"), _INTL("Team Galactic 2"),
			_INTL("Heart"), _INTL("Soul"), _INTL("Big Brother"), _INTL("Pokéathlon"),
			_INTL("Trio 3"), _INTL("Spiky Pika"), _INTL("Kimono Girl"), _INTL("Revival");
		}
	}

	public void unlockedWallpapers() {
		if (!@unlockedWallpapers) @unlockedWallpapers = new List<string>();
		return @unlockedWallpapers;
	}

	public bool isAvailableWallpaper(i) {
		if (!@unlockedWallpapers) @unlockedWallpapers = new List<string>();
		if (i < BASICWALLPAPERQTY) return true;
		if (@unlockedWallpapers[i]) return true;
		return false;
	}

	public void availableWallpapers() {
		ret = new {[], new List<string>()};   // Names, IDs
		papers = allWallpapers;
		if (!@unlockedWallpapers) @unlockedWallpapers = new List<string>();
		for (int i = papers.length; i < papers.length; i++) { //for 'papers.length' times do => |i|
			if (!isAvailableWallpaper(i)) continue;
			ret[0].Add(papers[i]);
			ret[1].Add(i);
		}
		return ret;
	}

	public void party() {
		Game.GameData.player.party;
	}

	public int party { set {
		Debug.LogError(new ArgumentError("Not supported"));
		//throw new Exception(new ArgumentError("Not supported"));
		}
	}

	public bool party_full() {
		return Game.GameData.player.party_full();
	}

	public void maxBoxes() {
		return @boxes.length;
	}

	public void maxPokemon(box) {
		if (box >= self.maxBoxes) return 0;
		return (box < 0) ? Settings.MAX_PARTY_SIZE : self[box].length;
	}

	public bool full() {
		for (int i = self.maxBoxes; i < self.maxBoxes; i++) { //for 'self.maxBoxes' times do => |i|
			if (!@boxes[i].full()) return false;
		}
		return true;
	}

	public void FirstFreePos(box) {
		if (box == -1) {
			ret = self.party.length;
			return (ret >= Settings.MAX_PARTY_SIZE) ? -1 : ret;
		}
		for (int i = maxPokemon(box); i < maxPokemon(box); i++) { //for 'maxPokemon(box)' times do => |i|
			if (!self[box, i]) return i;
		}
		return -1;
	}

	public void [](x, y = null() {
		if (y.null()) {
			return (x == -1) ? self.party : @boxes[x];
		} else {
			@boxes.each do |i|
				if (i.is_a(Pokemon)) raise "Box is a Pokémon, not a box";
			}
			return (x == -1) ? self.party[y] : @boxes[x][y];
		}
	}

	public int this[(x, y, value)] { get {
		if (x == -1) {
			self.party[y] = value;
		} else {
			@boxes[x][y] = value;
		}
		}
	}

	public void Copy(boxDst, indexDst, boxSrc, indexSrc) {
		if (indexDst < 0 && boxDst < self.maxBoxes) {
			found = false;
			for (int i = maxPokemon(boxDst); i < maxPokemon(boxDst); i++) { //for 'maxPokemon(boxDst)' times do => |i|
				if (self[boxDst, i]) continue;
				found = true;
				indexDst = i;
				break;
			}
			if (!found) return false;
		}
		if (boxDst == -1) {   // Copying into party
			if (party_full()) return false;
			self.party[self.party.length] = self[boxSrc, indexSrc];
			self.party.compact!;
		} else {   // Copying into box
			pkmn = self[boxSrc, indexSrc];
			if (!pkmn) raise "Trying to copy null to storage";
			if (Settings.HEAL_STORED_POKEMON) {
				old_ready_evo = pkmn.ready_to_evolve;
				pkmn.heal;
				pkmn.ready_to_evolve = old_ready_evo;
			}
			self[boxDst, indexDst] = pkmn;
		}
		return true;
	}

	public void Move(boxDst, indexDst, boxSrc, indexSrc) {
		if (!Copy(boxDst, indexDst, boxSrc, indexSrc)) return false;
		Delete(boxSrc, indexSrc);
		return true;
	}

	public void MoveCaughtToParty(pkmn) {
		if (party_full()) return false;
		self.party[self.party.length] = pkmn;
	}

	public void MoveCaughtToBox(pkmn, box) {
		for (int i = maxPokemon(box); i < maxPokemon(box); i++) { //for 'maxPokemon(box)' times do => |i|
			unless (self[box, i].null()) continue;
			if (Settings.HEAL_STORED_POKEMON && box >= 0) {
				old_ready_evo = pkmn.ready_to_evolve;
				pkmn.heal;
				pkmn.ready_to_evolve = old_ready_evo;
			}
			self[box, i] = pkmn;
			return true;
		}
		return false;
	}

	public void StoreCaught(pkmn) {
		if (Settings.HEAL_STORED_POKEMON && @currentBox >= 0) {
			old_ready_evo = pkmn.ready_to_evolve;
			pkmn.heal;
			pkmn.ready_to_evolve = old_ready_evo;
		}
		for (int i = maxPokemon(@currentBox); i < maxPokemon(@currentBox); i++) { //for 'maxPokemon(@currentBox)' times do => |i|
			if (self[@currentBox, i].null()) {
				self[@currentBox, i] = pkmn;
				return @currentBox;
			}
		}
		for (int j = self.maxBoxes; j < self.maxBoxes; j++) { //for 'self.maxBoxes' times do => |j|
			for (int i = maxPokemon(j); i < maxPokemon(j); i++) { //for 'maxPokemon(j)' times do => |i|
				unless (self[j, i].null()) continue;
				self[j, i] = pkmn;
				@currentBox = j;
				return @currentBox;
			}
		}
		return -1;
	}

	public void Delete(box, index) {
		if (self[box, index]) {
			self[box, index] = null;
			if (box == -1) self.party.compact!;
		}
	}

	public void clear() {
		self.maxBoxes.times(i => @boxes[i].clear);
	}
}

//===============================================================================
// Regional Storage scripts.
//===============================================================================
public partial class RegionalStorage {
	public void initialize() {
		@storages = new List<string>();
		@lastmap = -1;
		@rgnmap = -1;
	}

	public void getCurrentStorage() {
		if (!Game.GameData.game_map) {
			Debug.LogError(_INTL("The player is not on a map, so the region could not be determined."));
			//throw new ArgumentException(_INTL("The player is not on a map, so the region could not be determined."));
		}
		if (@lastmap != Game.GameData.game_map.map_id) {
			@rgnmap = GetCurrentRegion;   // may access file IO, so caching result
			@lastmap = Game.GameData.game_map.map_id;
		}
		if (@rgnmap < 0) {
			Debug.LogError(_INTL("The current map has no region set. Please set the MapPosition metadata setting for this map."));
			//throw new ArgumentException(_INTL("The current map has no region set. Please set the MapPosition metadata setting for this map."));
		}
		if (!@storages[@rgnmap]) @storages[@rgnmap] = new PokemonStorage();
		return @storages[@rgnmap];
	}

	public void allWallpapers() {
		return getCurrentStorage.allWallpapers;
	}

	public void availableWallpapers() {
		return getCurrentStorage.availableWallpapers;
	}

	public void unlockWallpaper(index) {
		getCurrentStorage.unlockWallpaper(index);
	}

	public void boxes() {
		return getCurrentStorage.boxes;
	}

	public void party() {
		return getCurrentStorage.party;
	}

	public bool party_full() {
		return getCurrentStorage.party_full();
	}

	public void maxBoxes() {
		return getCurrentStorage.maxBoxes;
	}

	public void maxPokemon(box) {
		return getCurrentStorage.maxPokemon(box);
	}

	public bool full() {
		getCurrentStorage.full();
	}

	public void currentBox() {
		return getCurrentStorage.currentBox;
	}

	public int currentBox { set {
		getCurrentStorage.currentBox = value;
		}
	}

	public void [](x, y = null() {
		getCurrentStorage[x, y];
	}

	public int this[(x, y, value)] { get {
		getCurrentStorage[x, y] = value;
		}
	}

	public void FirstFreePos(box) {
		getCurrentStorage.FirstFreePos(box);
	}

	public void Copy(boxDst, indexDst, boxSrc, indexSrc) {
		getCurrentStorage.Copy(boxDst, indexDst, boxSrc, indexSrc);
	}

	public void Move(boxDst, indexDst, boxSrc, indexSrc) {
		getCurrentStorage.Copy(boxDst, indexDst, boxSrc, indexSrc);
	}

	public void MoveCaughtToParty(pkmn) {
		getCurrentStorage.MoveCaughtToParty(pkmn);
	}

	public void MoveCaughtToBox(pkmn, box) {
		getCurrentStorage.MoveCaughtToBox(pkmn, box);
	}

	public void StoreCaught(pkmn) {
		getCurrentStorage.StoreCaught(pkmn);
	}

	public void Delete(box, index) {
		getCurrentStorage.Delete(pkmn);
	}
}

//===============================================================================
//
//===============================================================================
public void UnlockWallpaper(index) {
	Game.GameData.PokemonStorage.unlockedWallpapers[index] = true;
}

// NOTE: I don't know why you'd want to do this, but here you go.
public void LockWallpaper(index) {
	Game.GameData.PokemonStorage.unlockedWallpapers[index] = false;
}

//===============================================================================
// Look through Pokémon in storage.
//===============================================================================
// Yields every Pokémon/egg in storage in turn.
public void EachPokemon() {
	for (int i = -1; i < Game.GameData.PokemonStorage.maxBoxes; i++) { //each 'Game.GameData.PokemonStorage.maxBoxes' do => |i|
		for (int j = Game.GameData.PokemonStorage.maxPokemon(i); j < Game.GameData.PokemonStorage.maxPokemon(i); j++) { //for 'Game.GameData.PokemonStorage.maxPokemon(i)' times do => |j|
			pkmn = Game.GameData.PokemonStorage[i][j];
			if (pkmn) yield(pkmn, i);
		}
	}
}

// Yields every Pokémon in storage in turn.
public void EachNonEggPokemon() {
	if (!pkmn.egg() }) EachPokemon { |pkmn, box| yield(pkmn, box);
}
