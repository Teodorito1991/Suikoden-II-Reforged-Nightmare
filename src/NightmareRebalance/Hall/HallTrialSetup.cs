namespace NightmareRebalance;

// Shared by live requests and source-backed native preparation checks.
internal static class HallTrialSetup
{
    internal static MONSTER_PARTY_DATA Copy(MONSTER_PARTY_DATA source, FormationAudit audit, HallTrialDefinition definition)
    {
        if(definition.RetainCombatMain &&
            (audit.Hooks[5]==0 || !definition.RetainPhaseCallbacks && (audit.Hooks[1]!=0 || audit.Hooks[2]!=0) || !definition.RetainCommand && audit.Hooks[4]!=0))
            throw new InvalidOperationException("Trial combat-main role is not a standalone audited controller.");
        var party = CombatEvolutionRuntime.CopyParty(source, audit.OldCodes);
        party.id = "echo_trial_" + definition.Id + "_00";
        party.party_flg = 0;
        party.item_no = 0;
        // The original opening creates the boss's battle-local effect work
        // and turns off the captured field background. Retain its audited
        // binding. Only the per-definition battle-local phase/command roles survive.
        party.data.phase_start_func_index = definition.RetainPhaseCallbacks ? audit.Hooks[1] : 0;
        party.data.phase_end_func_index = definition.RetainPhaseCallbacks ? audit.Hooks[2] : 0;
        party.data.battle_end_func_index = 0;
        party.data.command_func_index = definition.RetainCommand ? audit.Hooks[4] : 0;
        // Only explicitly audited battle-local visual controllers are retained.
        // Story ending callbacks remain detached for every archetype.
        party.data.battle_main_func_index = definition.RetainCombatMain ? audit.Hooks[5] : 0;
        return party;
    }
}
