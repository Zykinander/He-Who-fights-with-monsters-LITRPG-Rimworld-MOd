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
        public bool manifestationsEnabled = true;
        public bool manifestationMonstersEnabled = true;
        public int manifestationFrequency = 2;
        public int densityPreset = 1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref abilitySlotsPerEssence, "abilitySlotsPerEssence", 3);
            abilitySlotsPerEssence = Math.Max(2, Math.Min(5, abilitySlotsPerEssence));
            Scribe_Values.Look(ref manifestationsEnabled, "manifestationsEnabled", true);
            Scribe_Values.Look(ref manifestationMonstersEnabled, "manifestationMonstersEnabled", true);
            Scribe_Values.Look(ref manifestationFrequency, "manifestationFrequency", 2);
            Scribe_Values.Look(ref densityPreset, "densityPreset", 1);
            manifestationFrequency = Math.Max(0, Math.Min(3, manifestationFrequency));
            densityPreset = Math.Max(0, Math.Min(2, densityPreset));
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

        public static bool ManifestationsEnabled => settings == null || settings.manifestationsEnabled;
        public static bool ManifestationMonstersEnabled =>
            settings == null || settings.manifestationMonstersEnabled;
        public static int ManifestationFrequency =>
            settings == null ? 2 : Math.Max(0, Math.Min(3, settings.manifestationFrequency));
        public static float MagicDensityMultiplier
        {
            get
            {
                int preset = settings == null ? 1 : settings.densityPreset;
                return preset == 0 ? 0.65f : preset == 2 ? 1.45f : 1f;
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
            list.GapLine();
            list.Label("Astral manifestations and magic density");
            if (list.ButtonText("Astral tears: " +
                (settings.manifestationsEnabled ? "Enabled" : "Disabled")))
            {
                settings.manifestationsEnabled = !settings.manifestationsEnabled;
                WriteSettings();
            }
            if (list.ButtonText("Monsters from tears: " +
                (settings.manifestationMonstersEnabled ? "Enabled" : "Essence only")))
            {
                settings.manifestationMonstersEnabled = !settings.manifestationMonstersEnabled;
                WriteSettings();
            }
            string[] frequencies = { "Off", "Rare", "Normal", "Frequent" };
            if (list.ButtonText("Manifestation frequency: " +
                frequencies[ManifestationFrequency]))
            {
                settings.manifestationFrequency = (ManifestationFrequency + 1) % 4;
                WriteSettings();
            }
            string[] densities = { "Low", "Normal", "High" };
            if (list.ButtonText("Map magic density: " +
                densities[Math.Max(0, Math.Min(2, settings.densityPreset))]))
            {
                settings.densityPreset = (settings.densityPreset + 1) % 3;
                WriteSettings();
            }
            list.Label("Density now tracks wealth, population, combat skills and essence ranks.");
            list.Label("The Low/Normal/High setting multiplies the colony-derived density.");
            var currentMap = Find.CurrentMap;
            if (currentMap != null)
            {
                var report = currentMap.GetComponent<MapComponent_EssenceWorld>()?.DensityReport;
                if (report.HasValue)
                {
                    list.Gap(6f);
                    list.Label("Current colony magic density: " +
                        (report.Value.FinalDensity * 100f).ToString("0.#") + "%");
                    list.Label("Contributions (0-100): wealth " +
                        (report.Value.WealthFactor * 100f).ToString("0") +
                        ", population " + (report.Value.PopulationFactor * 100f).ToString("0") +
                        ", combat " + (report.Value.CombatFactor * 100f).ToString("0") +
                        ", essence " + (report.Value.EssenceFactor * 100f).ToString("0"));
                }
            }
            list.Label("Stronger colonies attract more and deadlier manifestations.");
            list.End();
        }
    }
}
