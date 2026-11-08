using System;
using UnityEngine;
using Verse;

namespace Essencebound
{
    // A single global per-essence slot cap. Stored in RimWorld's mod settings,
    // not in individual pawn saves, so it can be changed during a campaign.
    public sealed class EssenceboundSettings : ModSettings
    {
        public int abilitySlotsPerEssence = 3;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref abilitySlotsPerEssence, "abilitySlotsPerEssence", 3);
            abilitySlotsPerEssence = Math.Max(2, Math.Min(5, abilitySlotsPerEssence));
        }
    }

    public sealed class EssenceboundMod : Mod
    {
        private static EssenceboundSettings settings;

        public static int SlotsPerEssence
        {
            get
            {
                int amount = settings == null ? 3 : settings.abilitySlotsPerEssence;
                return Math.Max(2, Math.Min(5, amount));
            }
        }

        public EssenceboundMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<EssenceboundSettings>();
        }

        public override string SettingsCategory() => "Essencebound";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var list = new Listing_Standard();
            list.Begin(inRect);
            list.Label("Ability slots per essence");
            list.Label("Affects all three normal essences and the confluence (2-5 each).");
            list.Gap(10f);
            if (list.ButtonText("Slots per essence: " + SlotsPerEssence + "   (click to change)"))
            {
                settings.abilitySlotsPerEssence = SlotsPerEssence == 5 ? 2 : SlotsPerEssence + 1;
                WriteSettings();
            }
            list.Gap(10f);
            list.Label("Maximum abilities for a completed set: " + (4 * SlotsPerEssence) + ".");
            list.Gap(6f);
            list.Label("Reducing the limit pauses abilities in slots above the limit.");
            list.Label("Those abilities are not deleted; raise the limit to restore them.");
            list.Label("Each absorbed essence still grants its innate starter ability.");
            list.End();
        }
    }
}
