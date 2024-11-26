//===============================================================================
// The Deprecation module is used to warn game & plugin creators of deprecated
// methods.
//===============================================================================
public static partial class Deprecation {
	#region Class Functions
	#endregion

	// Sends a warning of a deprecated method into the debug console.
	/// <param name="method_name">name of the deprecated method</param>
	/// <param name="removal_version">version the method is removed in</param>
	/// <param name="alternative">preferred alternative method</param>
	public void warn_method(String method_name, String removal_version = null, String alternative = null) {
		text = _INTL('Usage of deprecated method "{1}" or its alias.', method_name);
		unless (removal_version.null()) {
			text += "\n" + _INTL("The method is slated to be removed in Essentials {1}.", removal_version);
		}
		unless (alternative.null()) {
			text += "\n" + _INTL("Use \"{1}\" instead.", alternative);
		}
		Console.echo_warn text;
	}
}

//===============================================================================
// The Module class is extended to allow easy deprecation of instance and class
// methods.
//===============================================================================
public partial class Module {
	private;

	// Creates a deprecated alias for a method.
	// Using it sends a warning to the debug console.
	/// <param name="name">name of the new alias</param>
	/// <param name="aliased_method">name of the aliased method</param>
	/// <param name="removal_in">version the alias is removed in</param>
	/// <param name="class_method">whether the method is a class method</param>
	public void deprecated_method_alias(Symbol name, Symbol aliased_method, String removal_in: null, Boolean class_method: false) {
		validate name => Symbol, aliased_method => Symbol, removal_in => [NilClass, String],
						class_method => [TrueClass, FalseClass]

		target = class_method ? self.class : self;
		class_name = self.name

		unless (target.method_defined(aliased_method)) {
			Debug.LogError($"{class_name} does not have method {aliased_method} defined");
			//throw new Exception($"{class_name} does not have method {aliased_method} defined");
		}

		delimiter = class_method ? "." : "#";

		target.define_method(name) do |*args, **kvargs|
			alias_name = string.Format("{0}{0}{0}", class_name, delimiter, name);
			aliased_method_name = string.Format("{0}{0}{0}", class_name, delimiter, aliased_method);
			Deprecation.warn_method(alias_name, removal_in, aliased_method_name);
			method(aliased_method).call(*args, **kvargs);
		}
	}
}
