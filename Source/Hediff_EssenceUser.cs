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

        public IReadOnlyList<string> NormalEssences => normalEssenceNames;
        public string ConfluenceName => confluenceName;
        public string Rank => rank;
        public IReadOnlyList<EssenceAbilitySlot> AbilitySlots => abilitySlots;
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
            }
        }

        private void EnsureAbilitySlots()
        {
            abilitySlots ??= new List<EssenceAbilitySlot>();
            foreach (string name in normalEssenceNames.Concat(string.IsNullOrEmpty(confluenceName)
                         ? Array.Empty<string>() : new[] { confluenceName }))
            {
                for (int i = 0; i < 3; i++)
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
            var free = ownSlots.Where(s => !s.IsAwakened).OrderBy(s => s.index).FirstOrDefault();
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
            var slots = abilitySlots.Where(s => s != null && s.essenceDefName == essenceName).ToList();
            if (slots.Count != 3 || slots.All(s => s.IsAwakened))
            { reason = "This essence already has its three awakened slots."; return false; }
            if (EssencePowerCatalog.GetOptions(stone, essenceName, slots).Count == 0)
            { reason = "No new compatible imprint or power remains for that stone."; return false; }
            reason = "";
            return true;
        }

        internal bool TryAwaken(AwakeningStoneDef stone, string essenceName, out string message)
        {
            if (!CanAwaken(stone, essenceName, out message)) return false;
            var slots = abilitySlots.Where(s => s != null && s.essenceDefName == essenceName).ToList();
            var chosen = EssencePowerCatalog.PickOption(stone, essenceName, slots);
            if (chosen == null) { message = "No compatible power remains."; return false; }
            var slot = slots.Where(s => !s.IsAwakened).OrderBy(s => s.index).First();
            slot.awakeningStoneDefName = stone.defName;
            slot.awakeningAspectDefName = chosen.aspect.defName;
            slot.abilityDefName = chosen.power.defName;
            slot.sourceKind = "stone";
            slot.nextUseTick = 0;
            var essence = DefDatabase<EssenceDef>.GetNamedSilentFail(essenceName);
            message = "awakened " + chosen.power.label + " (" + chosen.aspect.label + ") in " +
                (essence?.label ?? essenceName) + " (slot " + (slot.index + 1) + "/3).";
            return true;
        }

        public bool TryCastPower(EssenceAbilitySlot slot, Pawn target, out string message)
        {
            if (pawn == null || pawn.Faction != Faction.OfPlayer || slot == null ||
                !abilitySlots.Contains(slot) || !slot.IsAwakened)
            { message = "That pawn cannot cast this power."; return false; }
            var power = DefDatabase<EssencePowerDef>.GetNamedSilentFail(slot.abilityDefName);
            if (power == null) { message = "This imprint has no playable power definition."; return false; }
            int now = Find.TickManager.TicksGame;
            if (slot.nextUseTick > now)
            { message = "Cooldown: " + ((slot.nextUseTick - now + 59) / 60) + " seconds left."; return false; }
            if (!EssencePowerExecutor.Apply(pawn, power, target, out message)) return false;
            slot.nextUseTick = now + power.cooldownTicks;
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

        public string ProfileText()
        {
            var b = new StringBuilder();
            b.AppendLine("ESSENCEBOUND PROFILE");
            b.AppendLine("Rank: " + rank);
            b.AppendLine("Normal essences (" + normalEssenceNames.Count + "/3):");
            foreach (var name in normalEssenceNames)
                b.AppendLine("  • " + (DefDatabase<EssenceDef>.GetNamedSilentFail(name)?.label ?? name));
            b.AppendLine("Confluence: " + (string.IsNullOrEmpty(confluenceName) ? "None" :
                DefDatabase<EssenceDef>.GetNamedSilentFail(confluenceName)?.label ?? confluenceName));
            b.AppendLine("Confluence state: " + confluenceStatus);
            if (confluenceRestricted) b.AppendLine("Warning: the community list marks this combination as restricted.");
            b.AppendLine("Awakened imprints: " + abilitySlots.Count(s => s.IsAwakened) + "/" + abilitySlots.Count + " (max 12)");
            foreach (string name in OwnedEssenceNames)
            {
                var essence = DefDatabase<EssenceDef>.GetNamedSilentFail(name);
                b.AppendLine("  " + (essence?.label ?? name) + ":");
                foreach (var slot in abilitySlots.Where(s => s.essenceDefName == name).OrderBy(s => s.index))
                {
                    var aspect = DefDatabase<AwakeningAspectDef>.GetNamedSilentFail(slot.awakeningAspectDefName);
                    var stone = DefDatabase<AwakeningStoneDef>.GetNamedSilentFail(slot.awakeningStoneDefName);
                    var power = DefDatabase<EssencePowerDef>.GetNamedSilentFail(slot.abilityDefName);
                    b.AppendLine("    " + (slot.index + 1) + ". " + (slot.IsAwakened
                        ? (power?.label ?? "unresolved imprint") + " [" + (slot.sourceKind == "innate" ? "starter" : (aspect?.label ?? "unknown aspect")) +
                          "; " + (slot.sourceKind == "innate" ? "innate" : (stone?.label ?? "legacy")) + "]" +
                          (slot.nextUseTick > Find.TickManager.TicksGame ? " (cooldown)" : "")
                        : "empty"));
                }
            }
            b.AppendLine("Awakened powers can be cast via pawn gizmos. Effects are experimental.");
            return b.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos()) yield return gizmo;
            if (pawn == null || pawn.Faction != Faction.OfPlayer) yield break;
            yield return new Command_Action
            {
                defaultLabel = "Essence profile",
                defaultDesc = "Inspect this pawn's three normal essences, confluence, rank, and awakening slots.",
                action = () => Find.WindowStack.Add(new Dialog_MessageBox(ProfileText()))
            };
            foreach (var slot in abilitySlots.Where(s => s != null && !string.IsNullOrEmpty(s.abilityDefName)))
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
                        "\nCooldown: " + (power.cooldownTicks / 60f).ToString("0.#") + " s",
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
