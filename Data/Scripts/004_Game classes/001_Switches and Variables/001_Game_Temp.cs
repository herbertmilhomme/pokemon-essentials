//===============================================================================
// ** Game_Temp
//-------------------------------------------------------------------------------
//  This class handles temporary data that is not included with save data.
//  Refer to "Game.GameData.game_temp" for the instance of this class.
//===============================================================================
public partial class Game_Temp {
	// Flags requesting something to happen
	/// <summary>menu calling flag</summary>
	public int menu_calling		{ get { return _menu_calling; } set { _menu_calling = value; } }			protected int _menu_calling;
	/// <summary>ready menu calling flag</summary>
	public int ready_menu_calling		{ get { return _ready_menu_calling; } set { _ready_menu_calling = value; } }			protected int _ready_menu_calling;
	/// <summary>debug calling flag</summary>
	public int debug_calling		{ get { return _debug_calling; } set { _debug_calling = value; } }			protected int _debug_calling;
	/// <summary>EventHandlers.trigger(:on_player_interact) flag</summary>
	public int interact_calling		{ get { return _interact_calling; } set { _interact_calling = value; } }			protected int _interact_calling;
	/// <summary>battle flag: interrupt (unused)</summary>
	public int battle_abort		{ get { return _battle_abort; } set { _battle_abort = value; } }			protected int _battle_abort;
	/// <summary>return to title screen flag</summary>
	public int title_screen_calling		{ get { return _title_screen_calling; } set { _title_screen_calling = value; } }			protected int _title_screen_calling;
	/// <summary>common event ID to start</summary>
	public int common_event_id		{ get { return _common_event_id; } set { _common_event_id = value; } }			protected int _common_event_id;
	// Flags indicating something is happening
	/// <summary>menu is open</summary>
	public int in_menu		{ get { return _in_menu; } set { _in_menu = value; } }			protected int _in_menu;
	/// <summary>in-Pokémon storage flag</summary>
	public int in_storage		{ get { return _in_storage; } set { _in_storage = value; } }			protected int _in_storage;
	/// <summary>in-battle flag</summary>
	public int in_battle		{ get { return _in_battle; } set { _in_battle = value; } }			protected int _in_battle;
	/// <summary>message window showing</summary>
	public int message_window_showing		{ get { return _message_window_showing; } set { _message_window_showing = value; } }			protected int _message_window_showing;
	/// <summary>jumping off surf base flag</summary>
	public int ending_surf		{ get { return _ending_surf; } set { _ending_surf = value; } }			protected int _ending_surf;
	/// <summary>[x, y] while jumping on/off, or null</summary>
	public int surf_base_coords		{ get { return _surf_base_coords; } set { _surf_base_coords = value; } }			protected int _surf_base_coords;
	/// <summary>performing mini update flag</summary>
	public int in_mini_update		{ get { return _in_mini_update; } set { _in_mini_update = value; } }			protected int _in_mini_update;
	// Battle
	/// <summary>battleback file name</summary>
	public int battleback_name		{ get { return _battleback_name; } set { _battleback_name = value; } }			protected int _battleback_name;
	/// <summary>force next battle to be 1v1 flag</summary>
	public int force_single_battle		{ get { return _force_single_battle; } set { _force_single_battle = value; } }			protected int _force_single_battle;
	/// <summary>[trainer, event ID] or null</summary>
	public int waiting_trainer		{ get { return _waiting_trainer; } set { _waiting_trainer = value; } }			protected int _waiting_trainer;
	/// <summary>record of actions in last recorded battle</summary>
	public int last_battle_record		{ get { return _last_battle_record; } set { _last_battle_record = value; } }			protected int _last_battle_record;
	// Player transfers
	/// <summary>player place movement flag</summary>
	public int player_transferring		{ get { return _player_transferring; } set { _player_transferring = value; } }			protected int _player_transferring;
	/// <summary>player destination: map ID</summary>
	public int player_new_map_id		{ get { return _player_new_map_id; } set { _player_new_map_id = value; } }			protected int _player_new_map_id;
	/// <summary>player destination: x-coordinate</summary>
	public int player_new_x		{ get { return _player_new_x; } set { _player_new_x = value; } }			protected int _player_new_x;
	/// <summary>player destination: y-coordinate</summary>
	public int player_new_y		{ get { return _player_new_y; } set { _player_new_y = value; } }			protected int _player_new_y;
	/// <summary>player destination: direction</summary>
	public int player_new_direction		{ get { return _player_new_direction; } set { _player_new_direction = value; } }			protected int _player_new_direction;
	/// <summary>[map ID, x, y] or null</summary>
	public int fly_destination		{ get { return _fly_destination; } set { _fly_destination = value; } }			protected int _fly_destination;
	// Transitions
	/// <summary>transition processing flag</summary>
	public int transition_processing		{ get { return _transition_processing; } set { _transition_processing = value; } }			protected int _transition_processing;
	/// <summary>transition file name</summary>
	public int transition_name		{ get { return _transition_name; } set { _transition_name = value; } }			protected int _transition_name;
	public int background_bitmap		{ get { return _background_bitmap; } set { _background_bitmap = value; } }			protected int _background_bitmap;
	/// <summary>for sprite hashes</summary>
	public int fadestate		{ get { return _fadestate; } set { _fadestate = value; } }			protected int _fadestate;
	// Other
	/// <summary>new game flag (true fron new game until saving)</summary>
	public int begun_new_game		{ get { return _begun_new_game; } set { _begun_new_game = value; } }			protected int _begun_new_game;
	/// <summary>menu: play sound effect flag</summary>
	public int menu_beep		{ get { return _menu_beep; } set { _menu_beep = value; } }			protected int _menu_beep;
	/// <summary>pause menu: index of last selection</summary>
	public int menu_last_choice		{ get { return _menu_last_choice; } set { _menu_last_choice = value; } }			protected int _menu_last_choice;
	/// <summary>set when trainer intro BGM is played</summary>
	public int memorized_bgm		{ get { return _memorized_bgm; } set { _memorized_bgm = value; } }			protected int _memorized_bgm;
	/// <summary>set when trainer intro BGM is played</summary>
	public int memorized_bgm_position		{ get { return _memorized_bgm_position; } set { _memorized_bgm_position = value; } }			protected int _memorized_bgm_position;
	/// <summary>DarknessSprite or null</summary>
	public int darkness_sprite		{ get { return _darkness_sprite; } set { _darkness_sprite = value; } }			protected int _darkness_sprite;
	public int mart_prices		{ get { return _mart_prices; } set { _mart_prices = value; } }			protected int _mart_prices;

	public void initialize() {
		// Flags requesting something to happen
		@menu_calling           = false;
		@ready_menu_calling     = false;
		@debug_calling          = false;
		@interact_calling       = false;
		@battle_abort           = false;
		@title_screen_calling   = false;
		@common_event_id        = 0;
		// Flags indicating something is happening
		@in_menu                = false;
		@in_storage             = false;
		@in_battle              = false;
		@message_window_showing = false;
		@ending_surf            = false;
		@in_mini_update         = false;
		// Battle
		@battleback_name        = "";
		@force_single_battle    = false;
		// Player transfers
		@player_transferring    = false;
		@player_new_map_id      = 0;
		@player_new_x           = 0;
		@player_new_y           = 0;
		@player_new_direction   = 0;
		// Transitions
		@transition_processing  = false;
		@transition_name        = "";
		@fadestate              = 0;
		// Other
		@begun_new_game         = false;
		@menu_beep              = false;
		@memorized_bgm          = null;
		@memorized_bgm_position = 0;
		@menu_last_choice       = 0;
		@mart_prices            = new List<string>();
	}

	public void clear_mart_prices() {
		@mart_prices = new List<string>();
	}
}
