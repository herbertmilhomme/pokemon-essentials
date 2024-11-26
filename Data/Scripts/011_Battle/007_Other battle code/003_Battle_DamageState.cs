//===============================================================================
//
//===============================================================================
public partial class Battle.DamageState {
	/// <summary>Type effectiveness</summary>
	public int typeMod		{ get { return _typeMod; } set { _typeMod = value; } }			protected int _typeMod;
	public int unaffected		{ get { return _unaffected; } set { _unaffected = value; } }			protected int _unaffected;
	public int protected		{ get { return _protected; } set { _protected = value; } }			protected int _protected;
	public int magicCoat		{ get { return _magicCoat; } set { _magicCoat = value; } }			protected int _magicCoat;
	public int magicBounce		{ get { return _magicBounce; } set { _magicBounce = value; } }			protected int _magicBounce;
	/// <summary>Like hpLost, but cumulative over all hits</summary>
	public int totalHPLost		{ get { return _totalHPLost; } set { _totalHPLost = value; } }			protected int _totalHPLost;
	/// <summary>Whether battler was knocked out by the move</summary>
	public int fainted		{ get { return _fainted; } set { _fainted = value; } }			protected int _fainted;

	/// <summary>Whether the move failed the accuracy check</summary>
	public int missed		{ get { return _missed; } set { _missed = value; } }			protected int _missed;
	public int affection_missed		{ get { return _affection_missed; } set { _affection_missed = value; } }			protected int _affection_missed;
	/// <summary>If the move missed due to two turn move invulnerability</summary>
	public int invulnerable		{ get { return _invulnerable; } set { _invulnerable = value; } }			protected int _invulnerable;
	/// <summary>Calculated damage</summary>
	public int calcDamage		{ get { return _calcDamage; } set { _calcDamage = value; } }			protected int _calcDamage;
	/// <summary>HP lost by opponent, inc. HP lost by a substitute</summary>
	public int hpLost		{ get { return _hpLost; } set { _hpLost = value; } }			protected int _hpLost;
	/// <summary>Critical hit flag</summary>
	public int critical		{ get { return _critical; } set { _critical = value; } }			protected int _critical;
	public int affection_critical		{ get { return _affection_critical; } set { _affection_critical = value; } }			protected int _affection_critical;
	/// <summary>Whether a substitute took the damage</summary>
	public int substitute		{ get { return _substitute; } set { _substitute = value; } }			protected int _substitute;
	/// <summary>Focus Band used</summary>
	public int focusBand		{ get { return _focusBand; } set { _focusBand = value; } }			protected int _focusBand;
	/// <summary>Focus Sash used</summary>
	public int focusSash		{ get { return _focusSash; } set { _focusSash = value; } }			protected int _focusSash;
	/// <summary>Sturdy ability used</summary>
	public int sturdy		{ get { return _sturdy; } set { _sturdy = value; } }			protected int _sturdy;
	/// <summary>Disguise ability used</summary>
	public int disguise		{ get { return _disguise; } set { _disguise = value; } }			protected int _disguise;
	/// <summary>Ice Face ability used</summary>
	public int iceFace		{ get { return _iceFace; } set { _iceFace = value; } }			protected int _iceFace;
	/// <summary>Damage was endured</summary>
	public int endured		{ get { return _endured; } set { _endured = value; } }			protected int _endured;
	public int affection_endured		{ get { return _affection_endured; } set { _affection_endured = value; } }			protected int _affection_endured;
	/// <summary>Whether a type-resisting berry was used</summary>
	public int berryWeakened		{ get { return _berryWeakened; } set { _berryWeakened = value; } }			protected int _berryWeakened;

	public int initialize { get { return reset; } }

	public void reset() {
		@typeMod          = Effectiveness.INEFFECTIVE_MULTIPLIER;
		@unaffected       = false;
		@protected        = false;
		@missed           = false;
		@affection_missed = false;
		@invulnerable     = false;
		@magicCoat        = false;
		@magicBounce      = false;
		@totalHPLost      = 0;
		@fainted          = false;
		resetPerHit;
	}

	public void resetPerHit() {
		@calcDamage         = 0;
		@hpLost             = 0;
		@critical           = false;
		@affection_critical = false;
		@substitute         = false;
		@focusBand          = false;
		@focusSash          = false;
		@sturdy             = false;
		@disguise           = false;
		@iceFace            = false;
		@endured            = false;
		@affection_endured  = false;
		@berryWeakened      = false;
	}
}
