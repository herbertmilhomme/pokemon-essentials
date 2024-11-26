//===============================================================================
// Trainer class for the player
//===============================================================================
public partial class Player : Trainer {
	/// <summary>the character ID of the player</summary>
	public Integer character_ID		{ get { return _character_ID; } }			private Integer _character_ID;
	/// <summary>the player's outfit</summary>
	public Integer outfit		{ get { return _outfit; } }			private Integer _outfit;
	/// <summary>the player's Gym Badges (true if owned)</summary>
	public Array<Boolean> badges		{ get { return _badges; } set { _badges = value; } }			protected Array<Boolean> _badges;
	/// <summary>the player's money</summary>
	public Integer money		{ get { return _money; } }			private Integer _money;
	/// <summary>the player's Game Corner coins</summary>
	public Integer coins		{ get { return _coins; } }			private Integer _coins;
	/// <summary>the player's battle points</summary>
	public Integer battle_points		{ get { return _battle_points; } }			private Integer _battle_points;
	/// <summary>the player's soot</summary>
	public Integer soot		{ get { return _soot; } }			private Integer _soot;
	/// <summary>the player's Pokédex</summary>
	public Pokedex pokedex		{ get { return _pokedex; } }			private Pokedex _pokedex;
	/// <summary>whether the Pokédex has been obtained</summary>
	public Boolean has_pokedex		{ get { return _has_pokedex; } set { _has_pokedex = value; } }			protected Boolean _has_pokedex;
	/// <summary>whether the Pokégear has been obtained</summary>
	public Boolean has_pokegear		{ get { return _has_pokegear; } set { _has_pokegear = value; } }			protected Boolean _has_pokegear;
	/// <summary>whether the player has running shoes (i.e. can run)</summary>
	public Boolean has_running_shoes		{ get { return _has_running_shoes; } set { _has_running_shoes = value; } }			protected Boolean _has_running_shoes;
	/// <summary>whether the player has an innate ability to access Pokémon storage</summary>
	public Boolean has_box_link		{ get { return _has_box_link; } set { _has_box_link = value; } }			protected Boolean _has_box_link;
	/// <summary>whether the creator of the Pokémon Storage System has been seen</summary>
	public Boolean seen_storage_creator		{ get { return _seen_storage_creator; } set { _seen_storage_creator = value; } }			protected Boolean _seen_storage_creator;
	/// <summary>whether the effect of Exp All applies innately</summary>
	public Boolean has_exp_all		{ get { return _has_exp_all; } set { _has_exp_all = value; } }			protected Boolean _has_exp_all;
	/// <summary>whether Mystery Gift can be used from the load screen</summary>
	public Boolean mystery_gift_unlocked		{ get { return _mystery_gift_unlocked; } set { _mystery_gift_unlocked = value; } }			protected Boolean _mystery_gift_unlocked;
	/// <summary>downloaded Mystery Gift data</summary>
	public Array<Array> mystery_gifts		{ get { return _mystery_gifts; } set { _mystery_gifts = value; } }			protected Array<Array> _mystery_gifts;

	public override void initialize(name, trainer_type) {
		base.initialize();
		@character_ID          = 0;
		@outfit                = 0;
		@badges                = [false] * 8;
		@money                 = GameData.Metadata.get.start_money;
		@coins                 = 0;
		@battle_points         = 0;
		@soot                  = 0;
		@pokedex               = new Pokedex();
		@has_pokedex           = false;
		@has_pokegear          = false;
		@has_running_shoes     = false;
		@has_box_link          = false;
		@seen_storage_creator  = false;
		@has_exp_all           = false;
		@mystery_gift_unlocked = false;
		@mystery_gifts         = new List<string>();
	}

	//-----------------------------------------------------------------------------

	public int character_ID { set {
		if (@character_ID == value) return;
		@character_ID = value;
		Game.GameData.game_player&.refresh_charset;
		}
	}

	public int outfit { set {
		if (@outfit == value) return;
		@outfit = value;
		Game.GameData.game_player&.refresh_charset;
		}
	}

	public void trainer_type() {
		return GameData.PlayerMetadata.get(@character_ID || 1).trainer_type;
	}

	// Sets the player's money. It can not exceed {Settings.MAX_MONEY}.
	// @param value [Integer] new money value
	public int money { set {
		validate value => Integer;
		@money = value.clamp(0, Settings.MAX_MONEY);
		}	}

	// Sets the player's coins amount. It can not exceed {Settings.MAX_COINS}.
	// @param value [Integer] new coins value
	public int coins { set {
		validate value => Integer;
		@coins = value.clamp(0, Settings.MAX_COINS);
		}	}

	// Sets the player's Battle Points amount. It can not exceed
	// {Settings.MAX_BATTLE_POINTS}.
	// @param value [Integer] new Battle Points value
	public int battle_points { set {
		validate value => Integer;
		@battle_points = value.clamp(0, Settings.MAX_BATTLE_POINTS);
		}	}

	// Sets the player's soot amount. It can not exceed {Settings.MAX_SOOT}.
	// @param value [Integer] new soot value
	public int soot { set {
		validate value => Integer;
		@soot = value.clamp(0, Settings.MAX_SOOT);
		}	}

	// @return [Integer] the number of Gym Badges owned by the player
	public void badge_count() {
		return @badges.count(badge => badge == true);
	}

	//-----------------------------------------------------------------------------

	// (see Pokedex#seen())
	// Shorthand for +self.pokedex.seen()+.
	public bool seen(species) {
		return @pokedex.seen(species);
	}

	// (see Pokedex#owned())
	// Shorthand for +self.pokedex.owned()+.
	public bool owned(species) {
		return @pokedex.owned(species);
	}
}
