//===============================================================================
//
//===============================================================================
public partial class Battle.AI {
	// Decide whether the opponent should Mega Evolve.
	public bool EnemyShouldMegaEvolve() {
		if (@battle.CanMegaEvolve(@user.index)) {   // Simple "always should if possible"
			Debug.log_ai($"{@user.name} will Mega Evolve");
			return true;
		}
		return false;
	}
}
