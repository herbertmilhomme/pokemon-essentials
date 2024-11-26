//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Item {
		public int id		{ get { return _id; } }			protected int _id;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int real_name_plural		{ get { return _real_name_plural; } }			protected int _real_name_plural;
		public int real_portion_name		{ get { return _real_portion_name; } }			protected int _real_portion_name;
		public int real_portion_name_plural		{ get { return _real_portion_name_plural; } }			protected int _real_portion_name_plural;
		public int pocket		{ get { return _pocket; } }			protected int _pocket;
		public int price		{ get { return _price; } }			protected int _price;
		public int sell_price		{ get { return _sell_price; } }			protected int _sell_price;
		public int bp_price		{ get { return _bp_price; } }			protected int _bp_price;
		public int field_use		{ get { return _field_use; } }			protected int _field_use;
		public int battle_use		{ get { return _battle_use; } }			protected int _battle_use;
		public int flags		{ get { return _flags; } }			protected int _flags;
		public int consumable		{ get { return _consumable; } }			protected int _consumable;
		public int show_quantity		{ get { return _show_quantity; } }			protected int _show_quantity;
		public int move		{ get { return _move; } }			protected int _move;
		public int real_description		{ get { return _real_description; } }			protected int _real_description;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "items.dat";
		public const string PBS_BASE_FILENAME = "items";
		SCHEMA = {
			"SectionName"       => new {:id,                       "m"},
			"Name"              => new {:real_name,                "s"},
			"NamePlural"        => new {:real_name_plural,         "s"},
			"PortionName"       => new {:real_portion_name,        "s"},
			"PortionNamePlural" => new {:real_portion_name_plural, "s"},
			"Pocket"            => new {:pocket,                   "v"},
			"Price"             => new {:price,                    "u"},
			"SellPrice"         => new {:sell_price,               "u"},
			"BPPrice"           => new {:bp_price,                 "u"},
			"FieldUse"          => new {:field_use,                "e", {"OnPokemon" => 1, "Direct" => 2,
																															"TM" => 3, "HM" => 4, "TR" => 5}},
			"BattleUse"         => new {:battle_use,               "e", {"OnPokemon" => 1, "OnMove" => 2,
																															"OnBattler" => 3, "OnFoe" => 4, "Direct" => 5}},
			"Flags"             => new {:flags,                    "*s"},
			"Consumable"        => new {:consumable,               "b"},
			"ShowQuantity"      => new {:show_quantity,            "b"},
			"Move"              => new {:move,                     "e", :Move},
			"Description"       => new {:real_description,         "q"}
		}

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void editor_properties() {
			field_use_array = [_INTL("Can't use in field")];
			self.schema["FieldUse"][2].each((key, value) => { if (!field_use_array[value]) field_use_array[value] = key; });
			battle_use_array = [_INTL("Can't use in battle")];
			self.schema["BattleUse"][2].each((key, value) => { if (!battle_use_array[value]) battle_use_array[value] = key; });
			return new {
				new {"ID",                ReadOnlyProperty,                        _INTL("ID of this item (used as a symbol like :XXX).")},
				new {"Name",              ItemNameProperty,                        _INTL("Name of this item as displayed by the game.")},
				new {"NamePlural",        ItemNameProperty,                        _INTL("Plural name of this item as displayed by the game.")},
				new {"PortionName",       ItemNameProperty,                        _INTL("Name of a portion of this item as displayed by the game.")},
				new {"PortionNamePlural", ItemNameProperty,                        _INTL("Name of 2 or more portions of this item as displayed by the game.")},
				new {"Pocket",            PocketProperty,                          _INTL("Pocket in the Bag where this item is stored.")},
				new {"Price",             new LimitProperty(Settings.MAX_MONEY),  _INTL("Purchase price of this item.")},
				new {"SellPrice",         new LimitProperty2(Settings.MAX_MONEY), _INTL("Sell price of this item. If blank, is usually half the purchase price.")},
				new {"BPPrice",           new LimitProperty(Settings.MAX_BATTLE_POINTS), _INTL("Purchase price of this item in Battle Points (BP).")},
				new {"FieldUse",          new EnumProperty(field_use_array),       _INTL("How this item can be used outside of battle.")},
				new {"BattleUse",         new EnumProperty(battle_use_array),      _INTL("How this item can be used within a battle.")},
				new {"Flags",             StringListProperty,                      _INTL("Words/phrases that can be used to group certain kinds of items.")},
				new {"Consumable",        BooleanProperty,                         _INTL("Whether this item is consumed after use.")},
				new {"ShowQuantity",      BooleanProperty,                         _INTL("Whether the Bag shows how many of this item are in there.")},
				new {"Move",              MoveProperty,                            _INTL("Move taught by this HM, TM or TR.")},
				new {"Description",       StringProperty,                          _INTL("Description of this item.")}
			}
		}

		public static void icon_filename(item) {
			if (item.null()) return "Graphics/Items/back";
			item_data = self.try_get(item);
			if (item_data.null()) return "Graphics/Items/000";
			// Check for files
			ret = string.Format("Graphics/Items/{0}", item_data.id);
			if (ResolveBitmap(ret)) return ret;
			// Check for TM/HM type icons
			if (item_data.is_machine()) {
				prefix = "machine";
				if (item_data.is_HM()) {
					prefix = "machine_hm";
				} else if (item_data.is_TR()) {
					prefix = "machine_tr";
				}
				move_type = GameData.Move.get(item_data.move).type;
				type_data = GameData.Type.get(move_type);
				ret = string.Format("Graphics/Items/{0}_{0}", prefix, type_data.id);
				if (ResolveBitmap(ret)) return ret;
				if (!item_data.is_TM()) {
					ret = string.Format("Graphics/Items/machine_{0}", type_data.id);
					if (ResolveBitmap(ret)) return ret;
				}
			}
			return "Graphics/Items/000";
		}

		public static void held_icon_filename(item) {
			item_data = self.try_get(item);
			if (!item_data) return null;
			name_base = (item_data.is_mail()) ? "mail" : "item";
			// Check for files
			ret = string.Format("Graphics/UI/Party/icon_{0}_{0}", name_base, item_data.id);
			if (ResolveBitmap(ret)) return ret;
			return string.Format("Graphics/UI/Party/icon_{0}", name_base);
		}

		public static void mail_filename(item) {
			item_data = self.try_get(item);
			if (!item_data) return null;
			// Check for files
			ret = string.Format("Graphics/UI/Mail/mail_{0}", item_data.id);
			return ResolveBitmap(ret) ? ret : null;
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id                       = hash.id;
			@real_name                = hash.real_name        || "Unnamed";
			@real_name_plural         = hash.real_name_plural || "Unnamed";
			@real_portion_name        = hash.real_portion_name;
			@real_portion_name_plural = hash.real_portion_name_plural;
			@pocket                   = hash.pocket           || 1;
			@price                    = hash.price            || 0;
			@sell_price               = hash.sell_price       || (@price / Settings.ITEM_SELL_PRICE_DIVISOR);
			@bp_price                 = hash.bp_price         || 1;
			@field_use                = hash.field_use        || 0;
			@battle_use               = hash.battle_use       || 0;
			@flags                    = hash.flags            || [];
			@consumable               = hash.consumable;
			if (@consumable.null()) @consumable               = !is_important();
			@show_quantity            = hash.show_quantity;
			@move                     = hash.move;
			@real_description         = hash.real_description || "???";
			@s_file_suffix          = hash.s_file_suffix  || "";
		}

		// @return [String] the translated name of this item
		public void name() {
			return GetMessageFromHash(MessageTypes.ITEM_NAMES, @real_name);
		}

		// @return [String] the translated plural version of the name of this item
		public void name_plural() {
			return GetMessageFromHash(MessageTypes.ITEM_NAME_PLURALS, @real_name_plural);
		}

		// @return [String] the translated portion name of this item
		public void portion_name() {
			if (@real_portion_name) return GetMessageFromHash(MessageTypes.ITEM_PORTION_NAMES, @real_portion_name);
			return name;
		}

		// @return [String] the translated plural version of the portion name of this item
		public void portion_name_plural() {
			if (@real_portion_name_plural) return GetMessageFromHash(MessageTypes.ITEM_PORTION_NAME_PLURALS, @real_portion_name_plural);
			return name_plural;
		}

		// @return [String] the translated description of this item
		public void description() {
			return GetMessageFromHash(MessageTypes.ITEM_DESCRIPTIONS, @real_description);
		}

		public bool has_flag(flag) {
			return @flags.any(f => f.downcase == flag.downcase);
		}

		public bool is_TM() {              return @field_use == 3; }
		public bool is_HM() {              return @field_use == 4; }
		public bool is_TR() {              return @field_use == 5; }
		public bool is_machine() {         return is_TM() || is_HM() || is_TR(); }
		public bool is_mail() {            return has_flag("Mail") || has_flag("IconMail"); }
		public bool is_icon_mail() {       return has_flag("IconMail"); }
		public bool is_poke_ball() {       return has_flag("PokeBall") || has_flag("SnagBall"); }
		public bool is_snag_ball() {       return has_flag("SnagBall") || (is_poke_ball() && Game.GameData.player.has_snag_machine); }
		public bool is_berry() {           return has_flag("Berry"); }
		public bool is_key_item() {        return has_flag("KeyItem"); }
		public bool is_evolution_stone() { return has_flag("EvolutionStone"); }
		public bool is_fossil() {          return has_flag("Fossil"); }
		public bool is_apricorn() {        return has_flag("Apricorn"); }
		public bool is_gem() {             return has_flag("TypeGem"); }
		public bool is_mulch() {           return has_flag("Mulch"); }
		public bool is_mega_stone() {      return has_flag("MegaStone"); }   // Does NOT include Red Orb/Blue Orb
		public bool is_scent() {           return has_flag("Scent"); }

		public bool is_important() {
			if (is_key_item() || is_HM() || is_TM()) return true;
			return false;
		}

		public bool can_hold() { return !is_important(); }

		public bool consumed_after_use() {
			return !is_important() && @consumable;
		}

		public bool show_quantity() {
			return @show_quantity || !is_important();
		}

		public bool unlosable(species, ability) {
			if (species == speciess.ARCEUS && ability != abilitys.MULTITYPE) return false;
			if (species == speciess.SILVALLY && ability != abilitys.RKSSYSTEM) return false;
			combos = {
				ARCEUS    = new {:FISTPLATE,   :FIGHTINIUMZ,
											:SKYPLATE,    :FLYINIUMZ,
											:TOXICPLATE,  :POISONIUMZ,
											:EARTHPLATE,  :GROUNDIUMZ,
											:STONEPLATE,  :ROCKIUMZ,
											:INSECTPLATE, :BUGINIUMZ,
											:SPOOKYPLATE, :GHOSTIUMZ,
											:IRONPLATE,   :STEELIUMZ,
											:FLAMEPLATE,  :FIRIUMZ,
											:SPLASHPLATE, :WATERIUMZ,
											:MEADOWPLATE, :GRASSIUMZ,
											:ZAPPLATE,    :ELECTRIUMZ,
											:MINDPLATE,   :PSYCHIUMZ,
											:ICICLEPLATE, :ICIUMZ,
											:DRACOPLATE,  :DRAGONIUMZ,
											:DREADPLATE,  :DARKINIUMZ,
											:PIXIEPLATE,  :FAIRIUMZ},
				SILVALLY  = new {:FIGHTINGMEMORY,
											:FLYINGMEMORY,
											:POISONMEMORY,
											:GROUNDMEMORY,
											:ROCKMEMORY,
											:BUGMEMORY,
											:GHOSTMEMORY,
											:STEELMEMORY,
											:FIREMEMORY,
											:WATERMEMORY,
											:GRASSMEMORY,
											:ELECTRICMEMORY,
											:PSYCHICMEMORY,
											:ICEMEMORY,
											:DRAGONMEMORY,
											:DARKMEMORY,
											:FAIRYMEMORY},
				DIALGA    = [:ADAMANTCRYSTAL],
				PALKIA    = [:LUSTROUSGLOBE],
				GIRATINA  = new {:GRISEOUSORB, :GRISEOUSCORE},
				GENESECT  = new {:BURNDRIVE, :CHILLDRIVE, :DOUSEDRIVE, :SHOCKDRIVE},
				KYOGRE    = [:BLUEORB],
				GROUDON   = [:REDORB],
				ZACIAN    = [:RUSTEDSWORD],
				ZAMAZENTA = [:RUSTEDSHIELD],
				OGERPON   = new {:WELLSPRINGMASK, :HEARTHFLAMEMASK, :CORNERSTONEMASK};
			}
			if (Settings.MECHANICS_GENERATION >= 9) combos[:GIRATINA].delete(:GRISEOUSORB);
			return combos[species]&.Contains(@id);
		}

		unless (method_defined(:__orig__get_property_for_PBS)) alias __orig__get_property_for_PBS get_property_for_PBS;
		public void get_property_for_PBS(key) {
			if (key == "ID") key = "SectionName";
			ret = __orig__get_property_for_PBS(key);
			switch (key) {
				case "SellPrice":
					if (ret == @price / Settings.ITEM_SELL_PRICE_DIVISOR) ret = null;
					break;
				case "BPPrice":
					if (ret == 1) ret = null;
					break;
				case "FieldUse": case "BattleUse":
					if (ret == 0) ret = null;
					break;
				case "Consumable":
					ret = @consumable;
					if (ret || is_important()) ret = null;   // Only return false, only for non-important items
					break;
			}
			return ret;
		}
	}
}
