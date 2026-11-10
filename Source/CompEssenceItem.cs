using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Essencebound
{
    public sealed class CompProperties_EssenceItem : CompProperties
    {
        public string essenceDefName;
        public CompProperties_EssenceItem() { compClass = typeof(CompEssenceItem); }
    }

    // Minimal interaction: with a colonist selected, right-click a crystal on the map.
    // The pawn must be adjacent. Pathfinding/jobs are deferred to a later prototype.
    public sealed class CompEssenceItem : ThingComp
    {
        private EssenceDef Essence => DefDatabase<EssenceDef>.GetNamedSilentFail(
            ((CompProperties_EssenceItem)props).essenceDefName);
        // The codex observes crystal discoveries without consuming items.
        public EssenceDef DiscoveryEssence => Essence;

        public override string CompInspectStringExtra()
        {
            return "Essencebound: right-click with an adjacent colonist to absorb.";
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn == null || selPawn.Faction != Faction.OfPlayer ||
                !selPawn.RaceProps.Humanlike || !selPawn.Spawned || !parent.Spawned ||
                selPawn.Map != parent.Map) yield break;

            EssenceDef essence = Essence;
            string label = "Absorb " + (essence?.label ?? "essence");
            if (selPawn.Position.DistanceTo(parent.Position) > 1.5f)
            {
                yield return new FloatMenuOption(label + " (move next to crystal)", null);
                yield break;
            }
            var state = Hediff_EssenceUser.ForPawn(selPawn);
            if (essence == null || (state != null && !state.CanAbsorb(essence, out _)))
            {
                yield return new FloatMenuOption(label + " (not available)", null);
                yield break;
            }

            // On the third unique essence, preview the exact confluence
            // before consuming the crystal. Preview and absorption share the
            // same order-independent resolver, including generated outcomes.
            if (state != null && state.NormalEssences.Count == 2)
            {
                var inputs = state.NormalEssences
                    .Select(DefDatabase<EssenceDef>.GetNamedSilentFail)
                    .Concat(new[] { essence }).ToList();
                var outcome = ConfluenceResolver.Resolve(inputs);
                if (outcome.essence != null)
                    label += " (confluence: " + outcome.essence.label +
                        (outcome.state == ConfluenceState.Generated ? ", generated" :
                         outcome.state == ConfluenceState.ConflictResolved ? ", disputed recipe" : "") + ")";
            }

            yield return new FloatMenuOption(label, () =>
            {
                if (parent.Destroyed || selPawn.Destroyed || !parent.Spawned || !selPawn.Spawned ||
                    selPawn.Map != parent.Map || selPawn.Position.DistanceTo(parent.Position) > 1.5f)
                    return;
                if (Hediff_EssenceUser.TryAbsorb(selPawn, essence, out string reason))
                {
                    if (parent.stackCount > 1) parent.stackCount--;
                    else parent.Destroy(DestroyMode.Vanish);
                    Messages.Message(selPawn.LabelShort + " absorbed the " + essence.label + " essence." + reason,
                        MessageTypeDefOf.PositiveEvent, false);
                    if (Hediff_EssenceUser.ForPawn(selPawn)?.NormalEssences.Count == 3)
                        Messages.Message(Hediff_EssenceUser.ForPawn(selPawn).ProfileText(), MessageTypeDefOf.NeutralEvent, false);
                }
                else
                    Messages.Message(reason, MessageTypeDefOf.RejectInput, false);
            });
        }
    }
}
