//===============================================================================
// The Kernel module is extended to include the validate method.
//===============================================================================
public static partial class Kernel {
	private;

	// Used to check whether method arguments are of a given class or respond to a method.
	// @param value_pairs [Hash{Object => Class, Array<Class>, Symbol}] value pairs to validate
	// @example Validate a class or method
	//   validate foo => Integer, baz => :to_s // raises an error if foo is not an Integer or if baz doesn't implement #to_s
	// @example Validate a class from an array
	//   validate foo => [Sprite, Bitmap, Viewport] // raises an error if foo isn't a Sprite, Bitmap or Viewport
	// @raise [ArgumentError] if validation fails
	public void validate(value_pairs) {
		unless (value_pairs.is_a(Hash)) {
			Debug.LogError($"Non-hash argument {value_pairs.inspect} passed into validate.");
			//throw new Exception($"Non-hash argument {value_pairs.inspect} passed into validate.");
		}
		errors = value_pairs.map do |value, condition|
			if (condition.Length > 0) {
				unless (condition.any(klass => value.is_a(klass))) {
					next $"Expected {value.inspect} to be one of {condition.inspect}, but got {value.class.name}.";
				}
			} else if (condition.is_a(Symbol)) {
				unless (value.respond_to(condition)) next $"Expected {value.inspect} to respond to {condition}.";
			} else if (!value.is_a(condition)) {
				next $"Expected {value.inspect} to be a {condition.name}, but got {value.class.name}.";
			}
		}
		errors.compact!;
		if (errors.empty()) return;
		Debug.LogError("Invalid argument passed to method.\r\n" + errors.join("\r\n"));
		//throw new Exception("Invalid argument passed to method.\r\n" + errors.join("\r\n"));
	}
}
