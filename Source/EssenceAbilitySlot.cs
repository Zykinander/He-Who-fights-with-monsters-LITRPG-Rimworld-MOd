using Verse;

namespace Essencebound
{
    // Three permanent slots per essence. An awakened slot currently stores a
    // stone/aspect IMPRINT only; abilityDefName is reserved for working powers.
    // Old v0.2 saves load with these new fields empty (not awakened).
    public sealed class EssenceAbilitySlot : IExposable
    {
        public string essenceDefName = "";
        public int index;
        public string abilityDefName = "";
        public string awakeningStoneDefName = "";
        public string awakeningAspectDefName = "";
        public int nextUseTick;

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
        }
    }
}
