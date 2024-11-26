//===============================================================================
//
//===============================================================================
public partial class StandardRestriction {
	public bool isValid(pkmn) {
		if (!pkmn || pkmn.egg()) return false;
		// Species with disadvantageous abilities are not banned
		foreach (var a in pkmn.species_data.abilities) { //'pkmn.species_data.abilities.each' do => |a|
			if (new []{:TRUANT, :SLOWSTART}.Contains(a)) return true;
		}
		// Certain named species are not banned
		if (new []{:DRAGONITE, :SALAMENCE, :TYRANITAR}.Contains(pkmn.species)) return true;
		// Certain named species are banned
		if (new []{:WYNAUT, :WOBBUFFET}.Contains(pkmn.species)) return false;
		// Species with total base stat 600 or more are banned
		bst = 0;
		pkmn.baseStats.each_value(s => bst += s);
		if (bst >= 600) return false;
		// Is valid
		return true;
	}
}

//===============================================================================
//
//===============================================================================
public partial class HeightRestriction {
	public void initialize(maxHeightInMeters) {
		@level = maxHeightInMeters;
	}

	public bool isValid(pkmn) {
		height = (pkmn.is_a(Pokemon)) ? pkmn.height : GameData.Species.get(pkmn).height;
		return height <= (int)Math.Round(@level * 10);
	}
}

//===============================================================================
//
//===============================================================================
public partial class WeightRestriction {
	public void initialize(maxWeightInKg) {
		@level = maxWeightInKg;
	}

	public bool isValid(pkmn) {
		weight = (pkmn.is_a(Pokemon)) ? pkmn.weight : GameData.Species.get(pkmn).weight;
		return weight <= (int)Math.Round(@level * 10);
	}
}

//===============================================================================
// Unused
//===============================================================================
public partial class NegativeExtendedGameClause {
	public bool isValid(pkmn) {
		if (pkmn.isSpecies(Speciess.ARCEUS)) return false;
		if (pkmn.hasItem(Items.MICLEBERRY)) return false;
		if (pkmn.hasItem(Items.CUSTAPBERRY)) return false;
		if (pkmn.hasItem(Items.JABOCABERRY)) return false;
		if (pkmn.hasItem(Items.ROWAPBERRY)) return false;
	}
}

//===============================================================================
//
//===============================================================================
Game.GameData.babySpeciesData = new List<string>();

public partial class BabyRestriction {
	public bool isValid(pkmn) {
		if (!Game.GameData.babySpeciesData[pkmn.species]) {
			Game.GameData.babySpeciesData[pkmn.species] = pkmn.species_data.get_baby_species;
		}
		return pkmn.species == Game.GameData.babySpeciesData[pkmn.species];
	}
}

//===============================================================================
//
//===============================================================================
Game.GameData.canEvolve = new List<string>();

public partial class UnevolvedFormRestriction {
	public bool isValid(pkmn) {
		if (!Game.GameData.babySpeciesData[pkmn.species]) {
			Game.GameData.babySpeciesData[pkmn.species] = pkmn.species_data.get_baby_species;
		}
		if (pkmn.species != Game.GameData.babySpeciesData[pkmn.species]) return false;
		if (Game.GameData.canEvolve[pkmn.species].null()) {
			Game.GameData.canEvolve[pkmn.species] = (pkmn.species_data.get_evolutions(true).length > 0);
		}
		return Game.GameData.canEvolve[pkmn.species];
	}
}

//===============================================================================
//
//===============================================================================
public static partial class NicknameChecker {
	@@names = new List<string>();

	public void getName(species) {
		n = @@names[species];
		if (n) return n;
		n = GameData.Species.get(species).name;
		@@names[species] = n.upcase;
		return n;
	}

	public void check(name, species) {
		name = name.upcase;
		if (name == getName(species)) return true;
		if (@@names.values.Contains(name)) return false;
		foreach (var species_data in GameData.Species) { //GameData.Species.each_species do => |species_data|
			if (species_data.species != species && getName(species_data.id) == name) return false;
		}
		return true;
	}
}

//===============================================================================
// No two Pokemon can have the same nickname.
// No nickname can be the same as the (real) name of another Pokemon character.
//===============================================================================
public partial class NicknameClause {
	public bool isValid(team) {
		for (int i = (team.length - 1); i < (team.length - 1); i++) { //for '(team.length - 1)' times do => |i|
			for (int j = i + 1; j < team.length; j++) { //each 'team.length' do => |j|
				if (team[i].name == team[j].name) return false;
				if (!NicknameChecker.check(team[i].name, team[i].species)) return false;
			}
		}
		return true;
	}

	public void errorMessage() {
		return _INTL("No identical nicknames.");
	}
}

//===============================================================================
//
//===============================================================================
public partial class NonEggRestriction {
	public bool isValid(pkmn) {
		return pkmn && !pkmn.egg();
	}
}

//===============================================================================
//
//===============================================================================
public partial class AblePokemonRestriction {
	public bool isValid(pkmn) {
		return pkmn&.able();
	}
}

//===============================================================================
//
//===============================================================================
public partial class SpeciesRestriction {
	public void initialize(*specieslist) {
		@specieslist = specieslist.clone;
	}

	public bool isSpecies(species, specieslist) {
		return specieslist.Contains(species);
	}

	public bool isValid(pkmn) {
		return isSpecies(pkmn.species, @specieslist);
	}
}

//===============================================================================
//
//===============================================================================
public partial class BannedSpeciesRestriction {
	public void initialize(*specieslist) {
		@specieslist = specieslist.clone;
	}

	public bool isSpecies(species, specieslist) {
		return specieslist.Contains(species);
	}

	public bool isValid(pkmn) {
		return !isSpecies(pkmn.species, @specieslist);
	}
}

//===============================================================================
//
//===============================================================================
public partial class RestrictedSpeciesRestriction {
	public void initialize(maxValue, *specieslist) {
		@specieslist = specieslist.clone;
		@maxValue = maxValue;
	}

	public bool isSpecies(species, specieslist) {
		return specieslist.Contains(species);
	}

	public bool isValid(team) {
		count = 0;
		foreach (var pkmn in team) { //'team.each' do => |pkmn|
			if (pkmn && isSpecies(pkmn.species, @specieslist)) count += 1;
		}
		return count <= @maxValue;
	}
}

//===============================================================================
//
//===============================================================================
public partial class RestrictedSpeciesTeamRestriction : RestrictedSpeciesRestriction {
	public override void initialize(*specieslist) {
		base.initialize(4, *specieslist);
	}
}

//===============================================================================
//
//===============================================================================
public partial class RestrictedSpeciesSubsetRestriction : RestrictedSpeciesRestriction {
	public override void initialize(*specieslist) {
		base.initialize(2, *specieslist);
	}
}

//===============================================================================
//
//===============================================================================
public partial class SameSpeciesClause {
	public bool isValid(team) {
		species = new List<string>();
		foreach (var pkmn in team) { //'team.each' do => |pkmn|
			if (pkmn && !species.Contains(pkmn.species)) species.Add(pkmn.species);
		}
		return species.length == 1;
	}

	public void errorMessage() {
		return _INTL("Pokémon must be the same species.");
	}
}

//===============================================================================
//
//===============================================================================
public partial class SpeciesClause {
	public bool isValid(team) {
		species = new List<string>();
		foreach (var pkmn in team) { //'team.each' do => |pkmn|
			if (!pkmn) continue;
			if (species.Contains(pkmn.species)) return false;
			species.Add(pkmn.species);
		}
		return true;
	}

	public void errorMessage() {
		return _INTL("Pokémon can't be the same species.");
	}
}

//===============================================================================
//
//===============================================================================
public partial class MinimumLevelRestriction {
	public int level		{ get { return _level; } }			protected int _level;

	public void initialize(minLevel) {
		@level = minLevel;
	}

	public bool isValid(pkmn) {
		return pkmn.level >= @level;
	}
}

//===============================================================================
//
//===============================================================================
public partial class MaximumLevelRestriction {
	public int level		{ get { return _level; } }			protected int _level;

	public void initialize(maxLevel) {
		@level = maxLevel;
	}

	public bool isValid(pkmn) {
		return pkmn.level <= @level;
	}
}

//===============================================================================
//
//===============================================================================
public partial class TotalLevelRestriction {
	public int level		{ get { return _level; } }			protected int _level;

	public void initialize(level) {
		@level = level;
	}

	public bool isValid(team) {
		totalLevel = 0;
		team.each(pkmn => { if (pkmn) totalLevel += pkmn.level; });
		return totalLevel <= @level;
	}

	public void errorMessage() {
		return _INTL("The combined levels exceed {1}.", @level);
	}
}

//===============================================================================
//
//===============================================================================
public partial class BannedItemRestriction {
	public void initialize(*itemlist) {
		@itemlist = itemlist.clone;
	}

	public bool isSpecies(item, itemlist) {
		return itemlist.Contains(item);
	}

	public bool isValid(pkmn) {
		return !pkmn.item_id || !isSpecies(pkmn.item_id, @itemlist);
	}
}

//===============================================================================
//
//===============================================================================
public partial class ItemsDisallowedClause {
	public bool isValid(pkmn) {
		return !pkmn.hasItem();
	}
}

//===============================================================================
//
//===============================================================================
public partial class SoulDewClause {
	public bool isValid(pkmn) {
		return !pkmn.hasItem(Items.SOULDEW);
	}
}

//===============================================================================
//
//===============================================================================
public partial class ItemClause {
	public bool isValid(team) {
		items = new List<string>();
		foreach (var pkmn in team) { //'team.each' do => |pkmn|
			if (!pkmn || !pkmn.hasItem()) continue;
			if (items.Contains(pkmn.item_id)) return false;
			items.Add(pkmn.item_id);
		}
		return true;
	}

	public void errorMessage() {
		return _INTL("No identical hold items.");
	}
}

//===============================================================================
//
//===============================================================================
public partial class LittleCupRestriction {
	public bool isValid(pkmn) {
		if (pkmn.hasItem(Items.BERRYJUICE)) return false;
		if (pkmn.hasItem(Items.DEEPSEATOOTH)) return false;
		if (pkmn.hasMove(Moves.SONICBOOM)) return false;
		if (pkmn.hasMove(Moves.DRAGONRAGE)) return false;
		if (pkmn.isSpecies(Speciess.SCYTHER)) return false;
		if (pkmn.isSpecies(Speciess.SNEASEL)) return false;
		if (pkmn.isSpecies(Speciess.MEDITITE)) return false;
		if (pkmn.isSpecies(Speciess.YANMA)) return false;
		if (pkmn.isSpecies(Speciess.TANGELA)) return false;
		if (pkmn.isSpecies(Speciess.MURKROW)) return false;
		return true;
	}
}
