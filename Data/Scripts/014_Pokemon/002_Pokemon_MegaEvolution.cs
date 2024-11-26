//===============================================================================
//
//===============================================================================
public partial class Pokemon {
	//-----------------------------------------------------------------------------
	// Mega Evolution.
	// NOTE: These are treated as form changes in Essentials.
	//-----------------------------------------------------------------------------
	public void getMegaForm() {
		ret = 0;
		foreach (var data in GameData.Species) { //'GameData.Species.each' do => |data|
			if (data.species != @species || data.unmega_form != form_simple) continue;
			if (data.mega_stone && hasItem(data.mega_stone)) {
				ret = data.form;
				break;
			} else if (data.mega_move && hasMove(data.mega_move)) {
				ret = data.form;
				break;
			}
		}
		return ret;   // form number, or 0 if no accessible Mega form
	}

	public void getUnmegaForm() {
		return (mega()) ? species_data.unmega_form : -1;
	}

	public bool hasMegaForm() {
		megaForm = self.getMegaForm;
		return megaForm > 0 && megaForm != form_simple;
	}

	public bool mega() {
		return (species_data.mega_stone || species_data.mega_move) ? true : false;
	}

	public void makeMega() {
		megaForm = self.getMegaForm;
		if (megaForm > 0) self.form = megaForm;
	}

	public void makeUnmega() {
		unmegaForm = self.getUnmegaForm;
		if (unmegaForm >= 0) self.form = unmegaForm;
	}

	public void megaName() {
		formName = species_data.form_name
		return (formName && !formName.empty()) ? formName : _INTL("Mega {1}", species_data.name);
	}

	// 0=default message, 1=Rayquaza message.
	public void megaMessage() {
		megaForm = self.getMegaForm;
		message_number = GameData.Species.get_species_form(@species, megaForm)&.mega_message;
		return message_number || 0;
	}

	//-----------------------------------------------------------------------------
	// Primal Reversion.
	// NOTE: These are treated as form changes in Essentials.
	//-----------------------------------------------------------------------------
	public bool hasPrimalForm() {
		v = MultipleForms.call("getPrimalForm", self);
		return !v.null();
	}

	public bool primal() {
		v = MultipleForms.call("getPrimalForm", self);
		return !v.null() && v == @form;
	}

	public void makePrimal() {
		v = MultipleForms.call("getPrimalForm", self);
		if (!v.null()) self.form = v;
	}

	public void makeUnprimal() {
		v = MultipleForms.call("getUnprimalForm", self);
		if (!v.null()) {
			self.form = v;
		} else if (primal()) {
			self.form = 0;
		}
	}
}
