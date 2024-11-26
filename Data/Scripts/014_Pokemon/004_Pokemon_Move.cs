//===============================================================================
// Move objects known by Pokémon.
//===============================================================================
public partial class Pokemon {
	public partial class Move {
		// This move's ID.
		public int id		{ get { return _id; } }			protected int _id;
		// The amount of PP remaining for this move.
		public int pp		{ get { return _pp; } }			protected int _pp;
		// The number of PP Ups used on this move (each one adds 20% to the total PP).
		public int ppup		{ get { return _ppup; } }			protected int _ppup;

		// Creates a new Move object.
		/// <param name="move_id">move ID | Symbol, String, GameData.Move</param>
		public void initialize(Symbol move_id) {
			@id   = GameData.Move.get(move_id).id;
			@ppup = 0;
			@pp   = total_pp;
		}

		// Sets this move's ID, and caps the PP amount if it is now greater than this
		// move's total PP.
		// @param value [Symbol, String, GameData.Move] the new move ID
		public int id { set {
			@id = GameData.Move.get(value).id;
			@pp = @pp.clamp(0, total_pp);
			}		}

		// Sets this move's PP, capping it at this move's total PP.
		// @param value [Integer] the new PP amount
		public int pp { set {
			@pp = value.clamp(0, total_pp);
			}		}

		// Sets this move's PP Up count, and caps the PP if necessary.
		// @param value [Integer] the new PP Up value
		public int ppup { set {
			@ppup = value;
			@pp = @pp.clamp(0, total_pp);
			}		}

		// Returns the total PP of this move, taking PP Ups into account.
		// @return [Integer] total PP
		public void total_pp() {
			max_pp = GameData.Move.get(@id).total_pp;
			return max_pp + (max_pp * @ppup / 5);
		}
		alias totalpp total_pp;

		public int function_code  { get { return GameData.Move.get(@id).function_code; } }
		public int power          { get { return GameData.Move.get(@id).power;         } }
		public int type           { get { return GameData.Move.get(@id).type;          } }
		public int category       { get { return GameData.Move.get(@id).category;      } }
		public bool physical_move() { return GameData.Move.get(@id).physical();     }
		public bool special_move() {  return GameData.Move.get(@id).special();      }
		public bool status_move() {   return GameData.Move.get(@id).status();       }
		public int accuracy       { get { return GameData.Move.get(@id).accuracy;      } }
		public int effect_chance  { get { return GameData.Move.get(@id).effect_chance; } }
		public int target         { get { return GameData.Move.get(@id).target;        } }
		public int priority       { get { return GameData.Move.get(@id).priority;      } }
		public int flags          { get { return GameData.Move.get(@id).flags;         } }
		public int name           { get { return GameData.Move.get(@id).name;          } }
		public int description    { get { return GameData.Move.get(@id).description;   } }
		public bool hidden_move() {   return GameData.Move.get(@id).hidden_move();  }

		public void display_type(pkmn)     {return GameData.Move.get(@id).display_type(pkmn, self);     }
		public void display_category(pkmn) {return GameData.Move.get(@id).display_category(pkmn, self); }
		public void display_damage(pkmn)   {return GameData.Move.get(@id).display_damage(pkmn, self);   }
		public void display_accuracy(pkmn) {return GameData.Move.get(@id).display_accuracy(pkmn, self); }
	}
}
