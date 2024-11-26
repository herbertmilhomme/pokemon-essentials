//===============================================================================
// Defines an event that procedures can subscribe to.
//===============================================================================
public partial class Event {
	public void initialize() {
		@callbacks = new List<string>();
	}

	// Sets an event handler for this event and removes all other event handlers.
	public void set(method) {
		@callbacks.clear;
		@callbacks.Add(method);
	}

	// Removes an event handler procedure from the event.
	public void -(other() {
		@callbacks.delete(other);
		return self;
	}

	// Adds an event handler procedure from the event.
	public void +(other() {
		if (@callbacks.Contains(other)) return self;
		@callbacks.Add(other);
		return self;
	}

	// Clears the event of event handlers.
	public void clear() {
		@callbacks.clear;
	}

	// Triggers the event and calls all its event handlers.  Normally called only
	// by the code where the event occurred.
	// The first argument is the sender of the event, the second argument contains
	// the event's parameters. If three or more arguments are given, this method
	// supports the following callbacks:
	// block: (sender,params) => { } where params is an array of the other parameters, and
	// block: (sender,arg0,arg1,...) => { }
	public void trigger(*arg) {
		arglist = arg[1, arg.length];
		@callbacks.each do |callback|
			if (callback.arity > 2 && arg.length == callback.arity) {
				// Retrofitted for callbacks that take three or more arguments
				callback.call(*arg);
			} else {
				callback.call(arg[0], arglist);
			}
		}
	}

	// Triggers the event and calls all its event handlers. Normally called only
	// by the code where the event occurred. The first argument is the sender of
	// the event, the other arguments are the event's parameters.
	public void trigger2(*arg) {
		@callbacks.each do |callback|
			callback.call(*arg);
		}
	}
}

//===============================================================================
// Same as class Event, but each registered proc has a name (a symbol) so it can
// be referenced individually.
//===============================================================================
public partial class NamedEvent {
	public void initialize() {
		@callbacks = new List<string>();
	}

	// Adds an event handler procedure from the event.
	public void add(key, proc) {
		@callbacks[key] = proc;
	}

	// Removes an event handler procedure from the event.
	public void remove(key) {
		@callbacks.delete(key);
	}

	// Clears the event of event handlers.
	public void clear() {
		@callbacks.clear;
	}

	// Triggers the event and calls all its event handlers. Normally called only
	// by the code where the event occurred.
	public void trigger(*args) {
		@callbacks.each_value(callback => callback.call(*args));
	}
}

//===============================================================================
// A class that stores code that can be triggered. Each piece of code has an
// associated ID, which can be anything that can be used as a key in a hash.
//===============================================================================
public partial class HandlerHash {
	public void initialize() {
		@hash = new List<string>();
	}

	public int this[int id] { get {
		if (id && @hash[id]) return @hash[id];
		return null;
		}
	}

	public void add(id, handler = null, &handlerBlock) {
		if (!new []{Proc, Hash}.Contains(handler.class) && !block_given()) {
			Debug.LogError($"{self.class.name} for {id.inspect} has no valid handler ({handler.inspect} was given)");
			//throw new Exception($"{self.class.name} for {id.inspect} has no valid handler ({handler.inspect} was given)");
		}
		if (id && !id.empty()) @hash[id] = handler || handlerBlock;
	}

	public void copy(src, *dests) {
		handler = self[src];
		if (!handler) return;
		dests.each(dest => add(dest, handler));
	}

	public void remove(key) {
		@hash.delete(key);
	}

	public void clear() {
		@hash.clear;
	}

	public void each() {
		@hash.each_pair((key, value) => yield key, value);
	}

	public void keys() {
		return @hash.keys.clone;
	}

	// NOTE: The call does not pass id as a parameter to the proc/block.
	public void trigger(id, *args) {
		handler = self[id];
		return handler&.call(*args);
	}
}

//===============================================================================
// A stripped-down version of class HandlerHash which only deals with IDs that
// are symbols. Also contains an add_ifs hash for code that applies to multiple
// IDs (determined by its condition proc).
//===============================================================================
public partial class HandlerHashSymbol {
	public void initialize() {
		@hash    = new List<string>();
		@add_ifs = new List<string>();
	}

	public int this[int sym] { get {
		if (!sym.is_a(Symbol) && sym.respond_to("id")) sym = sym.id;
		if (sym && @hash[sym]) return @hash[sym];
		@add_ifs.each_value do |add_if|
			if (add_if[0].call(sym)) return add_if[1];
		}
		return null;
		}
	}

	public void add(sym, handler = null, &handlerBlock) {
		if (!new []{Proc, Hash}.Contains(handler.class) && !block_given()) {
			Debug.LogError($"{self.class.name} for {sym.inspect} has no valid handler ({handler.inspect} was given)");
			//throw new Exception($"{self.class.name} for {sym.inspect} has no valid handler ({handler.inspect} was given)");
		}
		if (sym) @hash[sym] = handler || handlerBlock;
	}

	public void addIf(sym, conditionProc, handler = null, &handlerBlock) {
		if (!new []{Proc, Hash}.Contains(handler.class) && !block_given()) {
			Debug.LogError($"addIf call for {sym} in {self.class.name} has no valid handler ({handler.inspect} was given)");
			//throw new Exception($"addIf call for {sym} in {self.class.name} has no valid handler ({handler.inspect} was given)");
		}
		@add_ifs[sym] = new {conditionProc, handler || handlerBlock};
	}

	public void copy(src, *dests) {
		handler = self[src];
		if (!handler) return;
		dests.each(dest => add(dest, handler));
	}

	public void remove(key) {
		@hash.delete(key);
	}

	public void clear() {
		@hash.clear;
		@add_ifs.clear;
	}

	public void trigger(sym, *args) {
		if (!sym.is_a(Symbol) && sym.respond_to("id")) sym = sym.id;
		handler = self[sym];
		return handler&.call(sym, *args);
	}
}

//===============================================================================
// A specialised version of class HandlerHash which only deals with IDs that are
// constants in a particular class or module. That class or module must be
// defined when creating an instance of this class.
// Unused.
//===============================================================================
public partial class HandlerHashEnum {
	public void initialize(mod) {
		@mod         = mod;
		@hash        = new List<string>();
		@addIfs      = new List<string>();
		@symbolCache = new List<string>();
	}

	// 'sym' can be an ID or symbol.
	public int this[int sym] { get {
		id = fromSymbol(sym);
		ret = null;
		if (id && @hash[id]) ret = @hash[id];   // Real ID from the item
		symbol = toSymbol(sym);
		if (symbol && @hash[symbol]) ret = @hash[symbol];   // Symbol or string
		unless (ret) {
			@addIfs.each do |addif|
				if (addif[0].call(id)) return addif[1];
			}
		}
		return ret;
		}
	}

	public void fromSymbol(sym) {
		unless (sym.is_a(Symbol) || sym.is_a(String)) return sym;
		mod = Object.const_get(@mod) rescue null;
		if (!mod) return null;
		return mod.const_get(sym.to_sym) rescue null;
	}

	public void toSymbol(sym) {
		if (sym.is_a(Symbol) || sym.is_a(String)) return sym.to_sym;
		ret = @symbolCache[sym];
		if (ret) return ret;
		mod = Object.const_get(@mod) rescue null;
		if (!mod) return null;
		foreach (var key in mod.constants) { //'mod.constants.each' do => |key|
			if (mod.const_get(key) != sym) continue;
			ret = key.to_sym;
			@symbolCache[sym] = ret;
			break;
		}
		return ret;
	}

	// 'sym' can be an ID or symbol.
	public void add(sym, handler = null, &handlerBlock) {
		if (!new []{Proc, Hash}.Contains(handler.class) && !block_given()) {
			Debug.LogError($"{self.class.name} for {sym.inspect} has no valid handler ({handler.inspect} was given)");
			//throw new Exception($"{self.class.name} for {sym.inspect} has no valid handler ({handler.inspect} was given)");
		}
		id = fromSymbol(sym);
		if (id) @hash[id] = handler || handlerBlock;
		symbol = toSymbol(sym);
		if (symbol) @hash[symbol] = handler || handlerBlock;
	}

	public void addIf(conditionProc, handler = null, &handlerBlock) {
		if (!new []{Proc, Hash}.Contains(handler.class) && !block_given()) {
			Debug.LogError($"addIf call for {self.class.name} has no valid handler ({handler.inspect} was given)");
			//throw new Exception($"addIf call for {self.class.name} has no valid handler ({handler.inspect} was given)");
		}
		@addIfs.Add(new {conditionProc, handler || handlerBlock});
	}

	public void copy(src, *dests) {
		handler = self[src];
		if (!handler) return;
		dests.each(dest => self.add(dest, handler));
	}

	public void clear() {
		@hash.clear;
		@addIfs.clear;
	}

	public void trigger(sym, *args) {
		handler = self[sym];
		return (handler) ? handler.call(fromSymbol(sym), *args) : null;
	}
}

//===============================================================================
//
//===============================================================================
public partial class SpeciesHandlerHash : HandlerHashSymbol {
}

public partial class AbilityHandlerHash : HandlerHashSymbol {
}

public partial class ItemHandlerHash : HandlerHashSymbol {
}

public partial class MoveHandlerHash : HandlerHashSymbol {
}
