//===============================================================================
// The Bag object, which actually contains all the items.
//===============================================================================
public partial class PokemonBag {
	public int pockets		{ get { return _pockets; } }			protected int _pockets;
	public int last_viewed_pocket		{ get { return _last_viewed_pocket; } set { _last_viewed_pocket = value; } }			protected int _last_viewed_pocket;
	public int last_pocket_selections		{ get { return _last_pocket_selections; } set { _last_pocket_selections = value; } }			protected int _last_pocket_selections;
	public int registered_items		{ get { return _registered_items; } }			protected int _registered_items;
	public int ready_menu_selection		{ get { return _ready_menu_selection; } }			protected int _ready_menu_selection;

	public static void pocket_names() {
		return Settings.bag_pocket_names;
	}

	public static void pocket_count() {
		return self.pocket_names.length;
	}

	public void initialize() {
		@pockets = new List<string>();
		(0..PokemonBag.pocket_count).each(i => @pockets[i] = new List<string>())
		reset_last_selections;
		@registered_items = new List<string>();
		@ready_menu_selection = new {0, 0, 1};   // Used by the Ready Menu to remember cursor positions
	}

	public void reset_last_selections() {
		@last_viewed_pocket = 1;
		@last_pocket_selections ||= new List<string>();
		(0..PokemonBag.pocket_count).each(i => @last_pocket_selections[i] = 0)
	}

	public void clear() {
		@pockets.each(pocket => pocket.clear);
		(PokemonBag.pocket_count + 1).times(i => @last_pocket_selections[i] = 0)
	}

	//-----------------------------------------------------------------------------

	// Gets the index of the current selected item in the pocket
	public void last_viewed_index(pocket) {
		if (pocket <= 0 || pocket > PokemonBag.pocket_count) {
			Debug.LogError(new ArgumentError(_INTL("Invalid pocket: {1}", pocket.inspect)));
			//throw new ArgumentException(new ArgumentError(_INTL("Invalid pocket: {1}", pocket.inspect)));
		}
		return (int)Math.Min(@last_pocket_selections[pocket], @pockets[pocket].length) || 0;
	}

	// Sets the index of the current selected item in the pocket
	public void set_last_viewed_index(pocket, value) {
		if (pocket <= 0 || pocket > PokemonBag.pocket_count) {
			Debug.LogError(new ArgumentError(_INTL("Invalid pocket: {1}", pocket.inspect)));
			//throw new ArgumentException(new ArgumentError(_INTL("Invalid pocket: {1}", pocket.inspect)));
		}
		if (value <= @pockets[pocket].length) @last_pocket_selections[pocket] = value;
	}

	//-----------------------------------------------------------------------------

	public void quantity(item) {
		item_data = GameData.Item.try_get(item);
		if (!item_data) return 0;
		pocket = item_data.pocket;
		return ItemStorageHelper.quantity(@pockets[pocket], item_data.id);
	}

	public bool has(item, qty = 1) {
		return quantity(item) >= qty;
	}
	alias can_remove() has();

	public bool can_add(item, qty = 1) {
		item_data = GameData.Item.try_get(item);
		if (!item_data) return false;
		pocket = item_data.pocket;
		max_size = max_pocket_size(pocket);
		if (max_size < 0) max_size = @pockets[pocket].length + 1;   // Infinite size
		return ItemStorageHelper.can_add(
			@pockets[pocket], max_size, Settings.BAG_MAX_PER_SLOT, item_data.id, qty
		);
	}

	public void add(item, qty = 1) {
		item_data = GameData.Item.try_get(item);
		if (!item_data) return false;
		pocket = item_data.pocket;
		max_size = max_pocket_size(pocket);
		if (max_size < 0) max_size = @pockets[pocket].length + 1;   // Infinite size
		ret = ItemStorageHelper.add(@pockets[pocket],
																max_size, Settings.BAG_MAX_PER_SLOT, item_data.id, qty);
		if (ret && Settings.BAG_POCKET_AUTO_SORT[pocket - 1]) {
			@pockets[pocket].sort! { |a, b| GameData.Item.keys.index(a[0]) <=> GameData.Item.keys.index(b[0]) };
		}
		return ret;
	}

	// Adds qty number of item. Doesn't add anything if it can't add all of them.
	public void add_all(item, qty = 1) {
		if (!can_add(item, qty)) return false;
		return add(item, qty);
	}

	// Deletes as many of item as possible (up to qty), and returns whether it
	// managed to delete qty of them.
	public void remove(item, qty = 1) {
		item_data = GameData.Item.try_get(item);
		if (!item_data) return false;
		pocket = item_data.pocket;
		return ItemStorageHelper.remove(@pockets[pocket], item_data.id, qty);
	}

	// Deletes qty number of item. Doesn't delete anything if there are less than
	// qty of the item in the Bag.
	public void remove_all(item, qty = 1) {
		if (!can_remove(item, qty)) return false;
		return remove(item, qty);
	}

	// This only works if the old and new items are in the same pocket. Used for
	// switching on/off certain Key Items. Replaces all old_item in its pocket with
	// new_item.
	public void replace_item(old_item, new_item) {
		old_item_data = GameData.Item.try_get(old_item);
		new_item_data = GameData.Item.try_get(new_item);
		if (!old_item_data || !new_item_data) return false;
		pocket = old_item_data.pocket;
		old_id = old_item_data.id;
		new_id = new_item_data.id;
		ret = false;
		@pockets[pocket].each do |item|
			if (!item || item[0] != old_id) continue;
			item[0] = new_id;
			ret = true;
		}
		return ret;
	}

	//-----------------------------------------------------------------------------

	// Returns whether item has been registered for quick access in the Ready Menu.
	public bool registered(item) {
		item_data = GameData.Item.try_get(item);
		if (!item_data) return false;
		return @registered_items.Contains(item_data.id);
	}

	// Registers the item in the Ready Menu.
	public void register(item) {
		item_data = GameData.Item.try_get(item);
		if (!item_data) return;
		if (!@registered_items.Contains(item_data.id)) @registered_items.Add(item_data.id);
	}

	// Unregisters the item from the Ready Menu.
	public void unregister(item) {
		item_data = GameData.Item.try_get(item);
		if (item_data) @registered_items.delete(item_data.id);
	}

	//-----------------------------------------------------------------------------

	private;

	public void max_pocket_size(pocket) {
		return Settings.BAG_MAX_POCKET_SIZE[pocket - 1] || -1;
	}
}

//===============================================================================
// The PC item storage object, which actually contains all the items.
//===============================================================================
public partial class PCItemStorage {
	public int items		{ get { return _items; } }			protected int _items;

	public const int MAX_SIZE     = 999;   // Number of different slots in storage
	public const int MAX_PER_SLOT = 999;   // Max. number of items per slot

	public void initialize() {
		@items = new List<string>();
		// Start storage with initial items (e.g. a Potion)
		foreach (var item in GameData.Metadata.get.start_item_storage) { //'GameData.Metadata.get.start_item_storage.each' do => |item|
			if (GameData.Item.exists(item)) add(item);
		}
	}

	public int this[int i] { get { 
		return @items[i];
		}
	}

	public void length() {
		return @items.length;
	}

	public bool empty() {
		return @items.length == 0;
	}

	public void clear() {
		@items.clear;
	}

	// Unused
	public void get_item(index) {
		return (index < 0 || index >= @items.length) ? null : @items[index][0];
	}

	// Number of the item in the given index
	// Unused
	public void get_item_count(index) {
		return (index < 0 || index >= @items.length) ? 0 : @items[index][1];
	}

	public void quantity(item) {
		item = GameData.Item.get(item).id;
		return ItemStorageHelper.quantity(@items, item);
	}

	public bool can_add(item, qty = 1) {
		item = GameData.Item.get(item).id;
		return ItemStorageHelper.can_add(@items, MAX_SIZE, MAX_PER_SLOT, item, qty);
	}

	public void add(item, qty = 1) {
		item = GameData.Item.get(item).id;
		return ItemStorageHelper.add(@items, MAX_SIZE, MAX_PER_SLOT, item, qty);
	}

	public void remove(item, qty = 1) {
		item = GameData.Item.get(item).id;
		return ItemStorageHelper.remove(@items, item, qty);
	}
}

//===============================================================================
// Implements methods that act on arrays of items. Each element in an item array
// is itself an array of [itemID, itemCount].
// Used by the Bag, PC item storage, and Triple Triad.
//===============================================================================
public static partial class ItemStorageHelper {
	#region Class Functions
	#endregion

	// Returns the quantity of item in items.
	public void quantity(items, item) {
		ret = 0;
		items.each(i => { if (i && i[0] == item) ret += i[1]; });
		return ret;
	}

	public bool can_add(items, max_slots, max_per_slot, item, qty) {
		if (qty < 0) raise $"Invalid value for qty: {qty}";
		if (qty == 0) return true;
		for (int i = max_slots; i < max_slots; i++) { //for 'max_slots' times do => |i|
			item_slot = items[i];
			if (!item_slot) {
				qty -= (int)Math.Min(qty, max_per_slot);
				if (qty == 0) return true;
			} else if (item_slot[0] == item && item_slot[1] < max_per_slot) {
				new_amt = item_slot[1];
				new_amt = (int)Math.Min(new_amt + qty, max_per_slot);
				qty -= (new_amt - item_slot[1]);
				if (qty == 0) return true;
			}
		}
		return false;
	}

	public void add(items, max_slots, max_per_slot, item, qty) {
		if (qty < 0) raise $"Invalid value for qty: {qty}";
		if (qty == 0) return true;
		for (int i = max_slots; i < max_slots; i++) { //for 'max_slots' times do => |i|
			item_slot = items[i];
			if (!item_slot) {
				items[i] = new {item, (int)Math.Min(qty, max_per_slot)};
				qty -= items[i][1];
				if (qty == 0) return true;
			} else if (item_slot[0] == item && item_slot[1] < max_per_slot) {
				new_amt = item_slot[1];
				new_amt = (int)Math.Min(new_amt + qty, max_per_slot);
				qty -= (new_amt - item_slot[1]);
				item_slot[1] = new_amt;
				if (qty == 0) return true;
			}
		}
		return false;
	}

	// Deletes an item (items array, max. size per slot, item, no. of items to delete).
	public void remove(items, item, qty) {
		if (qty < 0) raise $"Invalid value for qty: {qty}";
		if (qty == 0) return true;
		ret = false;
		items.each_with_index do |item_slot, i|
			if (!item_slot || item_slot[0] != item) continue;
			amount = (int)Math.Min(qty, item_slot[1]);
			item_slot[1] -= amount;
			qty -= amount;
			if (item_slot[1] == 0) items[i] = null;
			if (qty > 0) continue;
			ret = true;
			break;
		}
		items.compact!;
		return ret;
	}
}
