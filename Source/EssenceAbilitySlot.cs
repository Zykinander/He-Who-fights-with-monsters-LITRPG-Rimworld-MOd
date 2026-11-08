using Verse;

namespace Essencebound
{
    // Two to five active slots per essence, set globally in Mod Settings.
    // We preserve previously saved high-index slots if the cap is reduced,
    // so restoring a higher cap brings their imprints back.
    // Old v0.2 saves load with newer fields empty (not awakened).
    public sealed class EssenceAbilitySlot : IExposable
    {
        public string essenceDefName = "";
        public int index;
        public string abilityDefName = "";
        public string awakeningStoneDefName = "";
        public string awakeningAspectDefName = "";
        public int nextUseTick;
        public int abilityRankIndex; // Iron=0, Diamond=4.
        public float rankWork;       // Measured real contribution toward the next tier.
        // "innate" for absorption-granted powers; "stone" for awakening stones.
        public string sourceKind = "";

        public bool IsAwakened => !string.IsNullOrEmpty(awakeningAspectDefName)
                              || !string.IsNullOrEmpty(abilityDefName);

        public EssenceAbilitySlot() { }
        public EssenceAbilitySlot(string essenceName, int slotIndex)
        {
            essenceDefName = essenceName;
            index = slotIndex;
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref essenceDefName, "essenceDefName", "");
            Scribe_Values.Look(ref index, "index", 0);
            Scribe_Values.Look(ref abilityDefName, "abilityDefName", "");
            Scribe_Values.Look(ref awakeningStoneDefName, "awakeningStoneDefName", "");
            Scribe_Values.Look(ref awakeningAspectDefName, "awakeningAspectDefName", "");
            Scribe_Values.Look(ref nextUseTick, "nextUseTick", 0);
            Scribe_Values.Look(ref abilityRankIndex, "abilityRankIndex", 0);
            Scribe_Values.Look(ref rankWork, "rankWork", 0f);
            abilityRankIndex = System.Math.Max(0, System.Math.Min(4, abilityRankIndex));
            rankWork = System.Math.Max(0f, rankWork);
            Scribe_Values.Look(ref sourceKind, "sourceKind", "");
        }
    }
}
