using Verse;

namespace Essencebound
{
    // Each essence (normal or confluence) is a vanilla-style Def.
    public sealed class EssenceDef : Def
    {
        public bool isConfluence;
        // Weighted random attribute rolled when each pawn acquires this essence.
        // All weights stay positive; every attribute remains possible.
        public float powerWeight = 1f;
        public float speedWeight = 1f;
        public float spiritWeight = 1f;
        public float recoveryWeight = 1f;
    }

    // Each ordered-free triple has exactly one proposed output, but
    // duplicate triples in community data are deliberately retained.
    public sealed class ConfluenceRecipeDef : Def
    {
        public EssenceDef first;
        public EssenceDef second;
        public EssenceDef third;
        public EssenceDef result;
        public string sourceStatus;
        public string restrictionStatus;
        public string sourceUrl;
        public bool conflict;
    }
}
