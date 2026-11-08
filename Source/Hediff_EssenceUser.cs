using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Essencebound
{
    // A health Hediff is used for per-pawn persistence; no modification to vanilla Pawn defs.
    public sealed class Hediff_EssenceUser : HediffWithComps
    {
        private List<string> normalEssenceNames = new List<string>();
        private string confluenceName = "";
        private string confluenceStatus = "incomplete";
        private bool confluenceRestricted;
        private string rank = "Iron";
        private List<EssenceAbilitySlot> abilitySlots = new List<EssenceAbilitySlot>();
        private List<EssenceAffinityRecord> affinities = new List<EssenceAffinityRecord>();
        private List<EssenceRankRecord> essenceRanks = new List<EssenceRankRecord>();

        public IReadOnlyList<string> NormalEssences => normalEssenceNames;
        public string ConfluenceName => confluenceName;
        public string Rank => rank;
        public string GetAffinity(string essenceName)
        {
            EnsureProgressionRecords();
            return affinities.FirstOrDefault(a => a.essenceDefName == essenceName)?.attribute ?? "";
        }

        public int GetEssenceRankIndex(string essenceName)
        {
            EnsureProgressionRecords();
            return essenceRanks.FirstOrDefault(r => r.essenceDefName == essenceName)?.rankIndex ?? 0;
        }

        public int GetRequiredSlotCount(string essenceName)
        {
            EnsureProgressionRecords();
            return essenceRanks.FirstOrDefault(r => r.essenceDefName == essenceName)?.requiredSlotCount ?? SlotsPerEssence;
        }

        public float GetAffinityBonus(string attribute)
        {
            EnsureProgressionRecords();
            float tierCount = 0f;
            foreach (var a in affinities.Where(a => a.attribute == attribute))
            {
                int tier = essenceRanks.FirstOrDefault(r => r.essenceDefName == a.essenceDefName)?.rankIndex ?? 0;
                tierCount += tier + 1; // Iron contributes 1 tier; Diamond contributes 5.
            }
            return Math.Min(0.8f, tierCount * EssenceProgression.BonusPerTier(attribute));
        }

        public float NextWorkFor(EssenceAbilitySlot slot)
        {
            return EssenceProgression.NextRankWork(slot?.abilityRankIndex ?? 4);
        }
        public IReadOnlyList<EssenceAbilitySlot> AbilitySlots => abilitySlots;
        public int SlotsPerEssence => EssenceboundMod.SlotsPerEssence;
        public string ConfluenceStatus => confluenceStatus;
        public bool ConfluenceRestricted => confluenceRestricted;

        // Existing high-index slots are retained for save compatibility when the cap shrinks.
        public void RefreshAbilitySlots() => EnsureAbilitySlots();

        public IReadOnlyList<EssenceAbilitySlot> GetActiveSlotsFor(string essenceName)
        {
            EnsureAbilitySlots();
            return abilitySlots.Where(s => s != null && s.essenceDefName == essenceName &&
                                      s.index >= 0 && s.index < SlotsPerEssence)
                               .OrderBy(s => s.index).ToList();
        }

        public int PausedPowersCount => abilitySlots.Count(s => s != null && s.IsAwakened &&
                                                           s.index >= SlotsPerEssence);
        public IReadOnlyList<string> OwnedEssenceNames => normalEssenceNames
            .Concat(string.IsNullOrEmpty(confluenceName) ? Array.Empty<string>() : new[] { confluenceName })
            .ToList();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref normalEssenceNames, "normalEssences", LookMode.Value);
            Scribe_Values.Look(ref confluenceName, "confluenceEssence", "");
            Scribe_Values.Look(ref confluenceStatus, "confluenceStatus", "incomplete");
            Scribe_Values.Look(ref confluenceRestricted, "confluenceRestricted", false);
            Scribe_Values.Look(ref rank, "essenceRank", "Iron");
            Scribe_Collections.Look(ref abilitySlots, "abilitySlots", LookMode.Deep);
            Scribe_Collections.Look(ref affinities, "essenceAffinities", LookMode.Deep);
            Scribe_Collections.Look(ref essenceRanks, "essenceRanks", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                normalEssenceNames ??= new List<string>();
                abilitySlots ??= new List<EssenceAbilitySlot>();
                confluenceName ??= "";
                confluenceStatus ??= "incomplete";
                rank ??= "Iron";
                EnsureAbilitySlots();
                EssencePowerCatalog.BackfillLegacyPowers(abilitySlots);
                // Grant missing starter powers without replacing existing awakenings.
                GrantMissingInnatePowers();
                EnsureProgressionRecords();
                RefreshOverallRank();
            }
        }


        private void EnsureProgressionRecords()
        {
            affinities ??= new List<EssenceAffinityRecord>();
            essenceRanks ??= new List<EssenceRankRecord>();
            foreach (string name in OwnedEssenceNames)
            {
                if (!affinities.Any(a => a != null && a.essenceDefName == name))
                {
                    var essence = DefDatabase<EssenceDef>.GetNamedSilentFail(name);
                    affinities.Add(new EssenceAffinityRecord
                    {
                        essenceDefName = name,
                        attribute = EssenceProgression.RollAttribute(essence)
                    });
                }
                var record = essenceRanks.FirstOrDefault(r => r != null && r.essenceDefName == name);
                if (record == null)
                {
                    essenceRanks.Add(new EssenceRankRecord
                    {
                        essenceDefName = name, rankIndex = 0, requiredSlotCount = SlotsPerEssence
                    });
                }
                else
                {
                    record.requiredSlotCount = Math.Max(record.requiredSlotCount, SlotsPerEssence);
                }
            }
        }

        private void RefreshOverallRank()
        {
            int lowestRank = essenceRanks == null || essenceRanks.Count == 0 ? 0 :
                OwnedEssenceNames.Select(n => essenceRanks.FirstOrDefault(r =>
                    r != null && r.essenceDefName == n)?.rankIndex ?? 0).DefaultIfEmpty(0).Min();
            rank = EssenceProgression.RankLabel(lowestRank);
        }

        private void TryAdvanceEssenceRank(string essenceName)
        {
            EnsureProgressionRecords();
            var record = essenceRanks.FirstOrDefault(r => r.essenceDefName == essenceName);
            if (record == null) return;
            while (record.rankIndex < 4)
            {
                // The maximum-ever slot count is a permanent progression requirement.
                // Reducing the configured cap does not erase or bypass this requirement.
                var required = abilitySlots.Where(s => s != null && s.essenceDefName == essenceName &&
                    s.index >= 0 && s.index < record.requiredSlotCount).ToList();
                if (required.Count != record.requiredSlotCount ||
                    required.Any(s => !s.IsAwakened || s.abilityRankIndex <= record.rankIndex))
                    break;
                record.rankIndex++;
                string label = DefDatabase<EssenceDef>.GetNamedSilentFail(essenceName)?.label ?? essenceName;
                if (pawn?.Faction == Faction.OfPlayer)
                    Messages.Message(pawn.LabelShort + "'s " + label + " essence advanced to " +
                        EssenceProgression.RankLabel(record.rankIndex) + "!",
                        MessageTypeDefOf.PositiveEvent, false);
            }
            RefreshOverallRank();
        }

        public void AddAbilityWork(string essenceName, int slotIndex, float amount)
        {
            if (amount <= 0f || string.IsNullOrEmpty(essenceName)) return;
            EnsureAbilitySlots();
            EnsureProgressionRecords();
            var slot = abilitySlots.FirstOrDefault(s => s != null && s.essenceDefName == essenceName &&
                s.index == slotIndex && s.index >= 0 && s.index < SlotsPerEssence && s.IsAwakened);
            if (slot == null || slot.abilityRankIndex >= 4) return;
            slot.rankWork += amount;
            while (slot.abilityRankIndex < 4 &&
                   slot.rankWork >= EssenceProgression.NextRankWork(slot.abilityRankIndex))
            {
                slot.rankWork -= EssenceProgression.NextRankWork(slot.abilityRankIndex);
                slot.abilityRankIndex++;
            }
            if (slot.abilityRankIndex == 4) slot.rankWork = 0f;
            TryAdvanceEssenceRank(essenceName);
        }

        private void EnsureAbilitySlots()
        {
            abilitySlots ??= new List<EssenceAbilitySlot>();
            foreach (string name in normalEssenceNames.Concat(string.IsNullOrEmpty(confluenceName)
                         ? Array.Empty<string>() : new[] { confluenceName }))
            {
                for (int i = 0; i < SlotsPerEssence; i++)
                {
                    if (!abilitySlots.Any(slot => slot != null && slot.essenceDefName == name && slot.index == i))
                        abilitySlots.Add(new EssenceAbilitySlot(name, i));
                }
            }
        }

        // One automatic starter per essence, consuming an ordinary ability slot.
        private string GrantInnatePower(string essenceName)
        {
            var starter = DefDatabase<EssenceStarterPowerDef>.AllDefsListForReading
                .FirstOrDefault(s => s.essence != null && s.essence.defName == essenceName);
            if (starter?.power == null) return "";
            var ownSlots = abilitySlots.Where(s => s != null && s.essenceDefName == essenceName).ToList();
            if (ownSlots.Any(s => s.abilityDefName == starter.power.defName)) return "";
            var free = ownSlots.Where(s => s.index >= 0 && s.index < SlotsPerEssence && !s.IsAwakened)
                               .OrderBy(s => s.index).FirstOrDefault();
            if (free == null) return "";
            free.abilityDefName = starter.power.defName;
            free.sourceKind = "innate";
            free.nextUseTick = 0;
            return starter.power.label;
        }

        private void GrantMissingInnatePowers()
        {
            foreach (string essenceName in OwnedEssenceNames)
                GrantInnatePower(essenceName);
        }

        internal bool CanAbsorb(EssenceDef essence, out string reason)
        {
            if (essence == null || essence.isConfluence)
            {
                reason = "Only normal essences may be absorbed directly.";
                return false;
            }
            if (normalEssenceNames.Contains(essence.defName))
            {
                reason = "This pawn already has that essence.";
                return false;
            }
            if (normalEssenceNames.Count >= 3)
            {
                reason = "All three normal essence slots are filled.";
                return false;
            }
            reason = "";
            return true;
        }

        internal bool Absorb(EssenceDef essence, out string reason)
        {
            if (!CanAbsorb(essence, out reason)) return false;
            normalEssenceNames.Add(essence.defName);
            if (normalEssenceNames.Count == 3)
            {
                var inputs = normalEssenceNames.Select(DefDatabase<EssenceDef>.GetNamedSilentFail).ToList();
                var outcome = ConfluenceResolver.Resolve(inputs);
                confluenceStatus = outcome.state.ToString().ToLowerInvariant();
                confluenceName = outcome.essence?.defName ?? "";
                confluenceRestricted = outcome.restricted;
            }
            EnsureAbilitySlots();
            EnsureProgressionRecords();
            string innate = GrantInnatePower(essence.defName);
            string confluenceInnate = string.IsNullOrEmpty(confluenceName) ? "" : GrantInnatePower(confluenceName);
            reason = (string.IsNullOrEmpty(innate) ? "" : " Innate ability: " + innate + ".") +
                     (string.IsNullOrEmpty(confluenceInnate) ? "" : " Confluence ability: " + confluenceInnate + ".");
            return true;
        }

        internal bool CanAwaken(AwakeningStoneDef stone, string essenceName, out string reason)
        {
            if (stone == null || stone.possibleAspects == null || stone.possibleAspects.Count == 0)
            { reason = "Stone has no awakening aspects."; return false; }
            if (!OwnedEssenceNames.Contains(essenceName))
            { reason = "Pawn does not possess that essence."; return false; }
            var slots = GetActiveSlotsFor(essenceName).ToList();
            if (slots.Count != SlotsPerEssence || slots.All(s => s.IsAwakened))
            { reason = "This essence has no free slots within the configured limit (" + SlotsPerEssence + ")."; return false; }
            if (EssencePowerCatalog.GetOptions(stone, essenceName, slots).Count == 0)
            { reason = "No new compatible imprint or power remains for that stone."; return false; }
            reason = "";
            return true;
        }

        internal bool TryAwaken(AwakeningStoneDef stone, string essenceName, out string message)
        {
            if (!CanAwaken(stone, essenceName, out message)) return false;
            var slots = GetActiveSlotsFor(essenceName).ToList();
            var chosen = EssencePowerCatalog.PickOption(stone, essenceName, slots);
            if (chosen == null) { message = "No compatible power remains."; return false; }
            var slot = slots.Where(s => !s.IsAwakened).OrderBy(s => s.index).First();
            slot.awakeningStoneDefName = stone.defName;
            slot.awakeningAspectDefName = chosen.aspect.defName;
            slot.abilityDefName = chosen.power.defName;
            slot.sourceKind = "stone";
            slot.nextUseTick = 0;
            slot.abilityRankIndex = 0;
            slot.rankWork = 0f;
            TryAdvanceEssenceRank(essenceName);
            var essence = DefDatabase<EssenceDef>.GetNamedSilentFail(essenceName);
            message = "awakened " + chosen.power.label + " (" + chosen.aspect.label + ") in " +
                (essence?.label ?? essenceName) + " (slot " + (slot.index + 1) + "/" + SlotsPerEssence + ").";
            return true;
        }

        public bool TryCastPower(EssenceAbilitySlot slot, Pawn target, out string message)
        {
            if (pawn == null || pawn.Faction != Faction.OfPlayer || slot == null ||
                !abilitySlots.Contains(slot) || !slot.IsAwakened)
            { message = "That pawn cannot cast this power."; return false; }
            if (slot.index < 0 || slot.index >= SlotsPerEssence)
            { message = "This ability is paused by the configured slot limit."; return false; }
            var power = DefDatabase<EssencePowerDef>.GetNamedSilentFail(slot.abilityDefName);
            if (power == null) { message = "This imprint has no playable power definition."; return false; }
            int now = Find.TickManager.TicksGame;
            if (slot.nextUseTick > now)
            { message = "Cooldown: " + ((slot.nextUseTick - now + 59) / 60) + " seconds left."; return false; }
            if (!EssencePowerExecutor.Apply(pawn, power, target, slot, out message, out float realWork)) return false;
            if (realWork > 0f) AddAbilityWork(slot.essenceDefName, slot.index, realWork);
            float cooldownReduction = GetAffinityBonus("Speed");
            slot.nextUseTick = now + Math.Max(30, (int)(power.cooldownTicks * (1f - cooldownReduction)));
            message = power.label.CapitalizeFirst() + ": " + message;
            return true;
        }

        private void ActivatePower(EssenceAbilitySlot slot, EssencePowerDef power)
        {
            if (power.targetKind == "self")
            {
                NotifyUse(slot, pawn);
                return;
            }
            var targets = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetSelf = true,
                canTargetBuildings = false,
                canTargetItems = false,
                canTargetLocations = false
            };
            Find.Targeter.BeginTargeting(targets, (LocalTargetInfo local) =>
            {
                NotifyUse(slot, local.Thing as Pawn);
            });
        }

        private void NotifyUse(EssenceAbilitySlot slot, Pawn target)
        {
            bool okay = TryCastPower(slot, target, out string message);
            Messages.Message((pawn?.LabelShort ?? "Pawn") + ": " + message,
                okay ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput, false);
        }

        // Plain-text fallback for diagnostics; normal players use Dialog_EssenceProfile.
        public string ProfileText()
        {
            EnsureAbilitySlots();
            var b = new StringBuilder();
            b.AppendLine("ESSENCEBOUND PROFILE");
            b.AppendLine("Overall rank: " + rank + " (lowest owned essence rank)");
            b.AppendLine("Normal essences (" + normalEssenceNames.Count + "/3)");
            b.AppendLine("Confluence: " + (string.IsNullOrEmpty(confluenceName) ? "None" :
                DefDatabase<EssenceDef>.GetNamedSilentFail(confluenceName)?.label ?? confluenceName));
            b.AppendLine("Confluence state: " + confluenceStatus);
            b.AppendLine("Slots per essence: " + SlotsPerEssence + " (configurable from 2 to 5)");
            b.AppendLine("Total potential slots: " + (4 * SlotsPerEssence));
            foreach (string name in OwnedEssenceNames)
            {
                var essence = DefDatabase<EssenceDef>.GetNamedSilentFail(name);
                b.AppendLine("  " + (essence?.label ?? name) + ": " +
                    EssenceProgression.RankLabel(GetEssenceRankIndex(name)) +
                    " / " + GetAffinity(name) + " (progression requires " +
                    GetRequiredSlotCount(name) + " abilities)");
                foreach (var slot in GetActiveSlotsFor(name))
                {
                    var power = DefDatabase<EssencePowerDef>.GetNamedSilentFail(slot.abilityDefName);
                    b.AppendLine("    " + (slot.index + 1) + ". " +
                        (slot.IsAwakened ? (power?.label ?? "unresolved imprint") +
                        " [" + EssenceProgression.RankLabel(slot.abilityRankIndex) + ", " +
                        slot.rankWork.ToString("0.#") + " work]" : "empty"));
                }
            }
            if (PausedPowersCount > 0)
                b.AppendLine("Stored powers paused by the current limit: " + PausedPowersCount);
            return b.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            EnsureAbilitySlots();
            foreach (Gizmo gizmo in base.GetGizmos()) yield return gizmo;
            if (pawn == null || pawn.Faction != Faction.OfPlayer) yield break;
            yield return new Command_Action
            {
                defaultLabel = "Essence profile",
                defaultDesc = "Open the Essencebound character interface: four essences, rank and configured ability slots.",
                action = () => Find.WindowStack.Add(new Dialog_EssenceProfile(this))
            };
            foreach (var slot in abilitySlots.Where(s => s != null && s.index >= 0 &&
                                                        s.index < SlotsPerEssence &&
                                                        !string.IsNullOrEmpty(s.abilityDefName)))
            {
                var captured = slot;
                var power = DefDatabase<EssencePowerDef>.GetNamedSilentFail(slot.abilityDefName);
                if (power == null) continue;
                var essence = DefDatabase<EssenceDef>.GetNamedSilentFail(slot.essenceDefName);
                int remaining = slot.nextUseTick - Find.TickManager.TicksGame;
                var cmd = new Command_Action
                {
                    defaultLabel = power.label.CapitalizeFirst(),
                    defaultDesc = power.description + "\nEssence: " + (essence?.label ?? slot.essenceDefName) +
                        "\nTarget: " + (power.targetKind == "self" ? "Self" : "Any living pawn (ally or enemy)") +
                        "\nRank: " + EssenceProgression.RankLabel(slot.abilityRankIndex) +
                        "\nTraining: " + EssenceProgression.WorkLabel(power.effectKind) +
                        "\nCooldown: " + (power.cooldownTicks / 60f).ToString("0.#") + " s (before Speed bonus)",
                    icon = ContentFinder<UnityEngine.Texture2D>.Get(power.iconPath, false),
                    action = () => ActivatePower(captured, power)
                };
                if (remaining > 0) cmd.Disable("Cooldown: " + ((remaining + 59) / 60) + " s");
                yield return cmd;
            }
        }

        public static Hediff_EssenceUser ForPawn(Pawn pawn)
        {
            return pawn?.health?.hediffSet?.hediffs?.OfType<Hediff_EssenceUser>().FirstOrDefault();
        }

        public static bool TryAbsorb(Pawn pawn, EssenceDef essence, out string reason)
        {
            if (pawn == null || pawn.health == null || !pawn.RaceProps.Humanlike)
            {
                reason = "Only living humanlike pawns can absorb essences in this prototype.";
                return false;
            }
            var state = ForPawn(pawn);
            if (state != null && !state.CanAbsorb(essence, out reason)) return false;
            if (essence == null || essence.isConfluence)
            {
                reason = "This is not a normal essence.";
                return false;
            }
            if (state == null)
            {
                var def = DefDatabase<HediffDef>.GetNamedSilentFail("EB_EssenceUser");
                if (def == null)
                {
                    reason = "Essencebound hediff definition was not loaded.";
                    return false;
                }
                state = HediffMaker.MakeHediff(def, pawn) as Hediff_EssenceUser;
                if (state == null)
                {
                    reason = "Could not create Essencebound pawn state.";
                    return false;
                }
                pawn.health.AddHediff(state);
            }
            return state.Absorb(essence, out reason);
        }
    }
}
