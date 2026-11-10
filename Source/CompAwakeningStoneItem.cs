using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Essencebound
{
    public sealed class CompProperties_AwakeningStoneItem : CompProperties
    {
        public string stoneDefName;
        public CompProperties_AwakeningStoneItem() { compClass = typeof(CompAwakeningStoneItem); }
    }

    // Developer prototype: adjacency-based right-click interaction only.
    // No pathfinding, hauling job, ritual sequence or actual powers at this stage.
    public sealed class CompAwakeningStoneItem : ThingComp
    {
        private AwakeningStoneDef Stone => DefDatabase<AwakeningStoneDef>.GetNamedSilentFail(
            ((CompProperties_AwakeningStoneItem)props).stoneDefName);
        public AwakeningStoneDef DiscoveryStone => Stone;

        public override string CompInspectStringExtra()
        {
            var stone = Stone;
            return stone == null
                ? "Essencebound: missing awakening stone definition."
                : "Essencebound: " + stone.rarity + " awakening stone; right-click with an adjacent essence user.";
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn == null || selPawn.Faction != Faction.OfPlayer ||
                !selPawn.RaceProps.Humanlike || !selPawn.Spawned || !parent.Spawned ||
                selPawn.Map != parent.Map) yield break;

            var stone = Stone;
            if (stone == null) yield break;
            string prefix = "Awaken with " + stone.label;
            if (selPawn.Position.DistanceTo(parent.Position) > 1.5f)
            {
                yield return new FloatMenuOption(prefix + " (move next to stone)", null);
                yield break;
            }

            var state = Hediff_EssenceUser.ForPawn(selPawn);
            if (state == null)
            {
                yield return new FloatMenuOption(prefix + " (absorb an essence first)", null);
                yield break;
            }

            bool any = false;
            // The player picks WHICH essence to awaken; the stone selects an imprint.
            // This avoids placing the last slot on an unwanted essence by accident.
            foreach (string essenceName in state.OwnedEssenceNames)
            {
                string capturedName = essenceName;
                if (!state.CanAwaken(stone, capturedName, out _)) continue;
                any = true;
                var essence = DefDatabase<EssenceDef>.GetNamedSilentFail(capturedName);
                string essenceLabel = essence?.label ?? capturedName;
                yield return new FloatMenuOption(prefix + " → " + essenceLabel, () =>
                {
                    if (parent.Destroyed || selPawn.Destroyed || !parent.Spawned || !selPawn.Spawned ||
                        selPawn.Map != parent.Map || selPawn.Position.DistanceTo(parent.Position) > 1.5f)
                        return;
                    var current = Hediff_EssenceUser.ForPawn(selPawn);
                    string message = "";
                    if (current != null && current.TryAwaken(stone, capturedName, out message))
                    {
                        if (parent.stackCount > 1) parent.stackCount--;
                        else parent.Destroy(DestroyMode.Vanish);
                        Messages.Message(selPawn.LabelShort + ": " + message,
                            MessageTypeDefOf.PositiveEvent, false);
                    }
                    else
                        Messages.Message(current == null ? "Pawn has no essences." : message,
                            MessageTypeDefOf.RejectInput, false);
                });
            }
            if (!any)
                yield return new FloatMenuOption(prefix + " (no compatible empty slots)", null);
        }
    }
}
