using Verse;

namespace Essencebound
{
    // Each essence (normal or confluence) is a vanilla-style Def.
    public sealed class EssenceDef : Def
    {
        public bool isConfluence;
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
