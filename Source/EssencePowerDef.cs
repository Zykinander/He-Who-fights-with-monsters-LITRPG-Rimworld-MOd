using System.Collections.Generic;
using Verse;

namespace Essencebound
{
    // Reusable effect template. Multiple essence/stone combinations can select the same power.
    public sealed class EssencePowerDef : Def
    {
        public string effectKind = "";
        public string targetKind = "self";
        public float range;
        public float magnitude;
        public int cooldownTicks;
        public int durationTicks;
        public string buffHediffDefName = "";
        public string damageDefName = "";
        public string iconPath = "";
        public List<AwakeningAspectDef> compatibleAspects = new List<AwakeningAspectDef>();
        public List<EssenceDef> favoredEssences = new List<EssenceDef>();
    }
}
