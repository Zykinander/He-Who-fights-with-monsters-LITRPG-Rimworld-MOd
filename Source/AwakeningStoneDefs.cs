using System.Collections.Generic;
using Verse;

namespace Essencebound
{
    // AspectDefs are DESIGN IMPRINTS, not RimWorld AbilityDefs.
    // A later milestone may map them to actual usable combat/noncombat abilities.
    public sealed class AwakeningAspectDef : Def
    {
        public string role;
    }

    public sealed class AwakeningStoneDef : Def
    {
        public string rarity;
        public List<EssenceDef> favoredEssences = new List<EssenceDef>();
        public List<AwakeningAspectDef> possibleAspects = new List<AwakeningAspectDef>();
        public string sourceStatus;
        public string sourceUrl;
    }
}
