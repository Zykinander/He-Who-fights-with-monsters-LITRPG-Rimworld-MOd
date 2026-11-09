using System.Collections.Generic;
using Verse;

namespace Essencebound
{
    // Reusable effect template. Multiple essence/stone combinations can select the same power.
    public sealed class EssenceStarterPowerDef : Def
    {
        public EssenceDef essence;
        public EssencePowerDef power;
    }

    public sealed class EssencePowerDef : Def
    {
        public string effectKind = "";
        public string targetKind = "self";
        public float range;
        public float magnitude;
        public int cooldownTicks;
        public int durationTicks;
        // Radius around a target or caster for chain, splash and aura powers.
        public float areaRadius;
        // Number of extra hostile chain targets beyond the primary pawn.
        public int maxExtraTargets = 2;
        public string buffHediffDefName = "";
        public string damageDefName = "";
        public string iconPath = "";
        public List<AwakeningAspectDef> compatibleAspects = new List<AwakeningAspectDef>();
        public List<EssenceDef> favoredEssences = new List<EssenceDef>();
        // Informational taxonomy; casting and awakening continue to use the
        // existing essence affinities and compatibleAspects.
        public EssenceDef primaryEssence;
        public string primaryCategory = "";
        public List<string> abilityTags = new List<string>();
    }
}
