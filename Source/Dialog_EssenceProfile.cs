using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace Essencebound
{
    // Read-only four-essence overview reached from the pawn's Essence profile gizmo.
    // Uses vanilla RimWorld Window/Widgets rather than a separate UI framework.
    public sealed class Dialog_EssenceProfile : Window
    {
        private static readonly string[] RankNames =
            { "Iron", "Bronze", "Silver", "Gold", "Diamond" };
        private static readonly Color PanelBackground = new Color(0.13f, 0.15f, 0.18f, 1f);
        private static readonly Color ConfluenceBackground = new Color(0.16f, 0.13f, 0.20f, 1f);
        private static readonly Color RowBackground = new Color(0.19f, 0.21f, 0.24f, 1f);
        private static readonly Color Highlight = new Color(0.29f, 0.45f, 0.47f, 1f);
        private static readonly Color Muted = new Color(0.60f, 0.65f, 0.69f, 1f);

        private readonly Hediff_EssenceUser state;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(980f, 690f);

        public Dialog_EssenceProfile(Hediff_EssenceUser essenceUser)
        {
            state = essenceUser;
            doCloseX = true;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;
            draggable = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (state == null || state.pawn == null)
            {
                Widgets.Label(inRect, "The Essencebound pawn is no longer available.");
                return;
            }

            state.RefreshAbilitySlots();
            int cap = state.SlotsPerEssence;
            int active = state.OwnedEssenceNames
                .Sum(name => state.GetActiveSlotsFor(name).Count(slot => slot.IsAwakened));
            int possible = cap * 4;

            DrawHeader(new Rect(inRect.x, inRect.y, inRect.width, 122f), cap, active, possible);

            Rect outer = new Rect(inRect.x, inRect.y + 130f, inRect.width, inRect.height - 130f);
            float panelHeight = 85f + cap * 108f;
            float contentHeight = panelHeight * 2f + 16f;
            float contentWidth = outer.width - 20f;
            Rect content = new Rect(0f, 0f, contentWidth, contentHeight);
            Widgets.BeginScrollView(outer, ref scrollPosition, content);
            try
            {
                float gap = 12f;
                float panelWidth = (contentWidth - gap) / 2f;

                DrawEssencePanel(new Rect(0f, 0f, panelWidth, panelHeight),
                    "NORMAL ESSENCE I", GetNormalName(0), false, cap);
                DrawEssencePanel(new Rect(panelWidth + gap, 0f, panelWidth, panelHeight),
                    "NORMAL ESSENCE II", GetNormalName(1), false, cap);
                DrawEssencePanel(new Rect(0f, panelHeight + 16f, panelWidth, panelHeight),
                    "NORMAL ESSENCE III", GetNormalName(2), false, cap);
                DrawEssencePanel(new Rect(panelWidth + gap, panelHeight + 16f, panelWidth, panelHeight),
                    "CONFLUENCE ESSENCE", state.ConfluenceName, true, cap);
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }

        private string GetNormalName(int position)
        {
            return position < state.NormalEssences.Count ? state.NormalEssences[position] : "";
        }

        private void DrawHeader(Rect rect, int cap, int active, int possible)
        {
            string pawnName = state.pawn.LabelShort.CapitalizeFirst();
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, rect.y + 3f, rect.width, 33f),
                pawnName + "   /   ESSENCEBOUND");
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(rect.x, rect.y + 36f, rect.width, 23f),
                "Rank: " + state.Rank + "    •    Awakened abilities: " + active +
                "/" + possible + "    •    " + cap + " slots per essence");

            float step = (rect.width - 8f) / RankNames.Length;
            int activeRank = Array.FindIndex(RankNames, rank => state.Rank.StartsWith(rank));
            for (int i = 0; i < RankNames.Length; i++)
            {
                Rect tile = new Rect(rect.x + i * step, rect.y + 66f, step - 6f, 26f);
                Widgets.DrawBoxSolid(tile, i == activeRank ? Highlight : PanelBackground);
                TextAnchor previousAnchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(tile, RankNames[i]);
                Text.Anchor = previousAnchor;
            }

            string note = state.PausedPowersCount > 0
                ? state.PausedPowersCount + " stored powers paused by the current slot limit."
                : "Attribute bonuses: Power " + (100f * state.GetAffinityBonus("Power")).ToString("0.#") +
                  "%  •  Speed " + (100f * state.GetAffinityBonus("Speed")).ToString("0.#") +
                  "%  •  Spirit " + (100f * state.GetAffinityBonus("Spirit")).ToString("0.#") +
                  "%  •  Recovery " + (100f * state.GetAffinityBonus("Recovery")).ToString("0.#") + "%";
            Color previous = GUI.color;
            GUI.color = Muted;
            Widgets.Label(new Rect(rect.x, rect.y + 99f, rect.width, 22f), note);
            GUI.color = previous;
        }

        private void DrawEssencePanel(Rect panel, string heading, string essenceName,
            bool confluence, int cap)
        {
            Widgets.DrawBoxSolid(panel, confluence ? ConfluenceBackground : PanelBackground);
            Widgets.DrawBox(panel);

            bool unlocked = !string.IsNullOrEmpty(essenceName);
            var def = unlocked ? DefDatabase<EssenceDef>.GetNamedSilentFail(essenceName) : null;
            string displayName = unlocked
                ? (def == null ? essenceName : def.label.CapitalizeFirst())
                : (confluence ? "Confluence locked" : "No essence absorbed");
            var slots = unlocked ? state.GetActiveSlotsFor(essenceName) : null;
            int awakened = slots == null ? 0 : slots.Count(s => s.IsAwakened);

            Widgets.Label(new Rect(panel.x + 12f, panel.y + 8f, panel.width - 24f, 20f), heading);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(panel.x + 12f, panel.y + 27f, panel.width - 24f, 30f), displayName);
            Text.Font = GameFont.Small;

            string countText = unlocked
                ? awakened + "/" + cap + " awakened   •   " +
                  state.GetEssenceRankDisplay(essenceName) +
                  "   •   " + state.GetAffinity(essenceName) +
                  "   •   needs " + state.GetRequiredSlotCount(essenceName) + " slots"
                : (confluence ? "Absorb three different normal essences" : "Absorb an essence to unlock");
            Widgets.Label(new Rect(panel.x + 12f, panel.y + 56f, panel.width - 24f, 17f), countText);

            for (int index = 0; index < cap; index++)
            {
                EssenceAbilitySlot slot = slots == null ? null :
                    slots.FirstOrDefault(s => s.index == index);
                Rect row = new Rect(panel.x + 9f, panel.y + 80f + index * 108f,
                    panel.width - 18f, 102f);
                DrawAbilityRow(row, index, slot, unlocked);
            }
        }

        private void DrawAbilityRow(Rect rect, int index, EssenceAbilitySlot slot, bool essenceUnlocked)
        {
            Widgets.DrawBoxSolid(rect, RowBackground);

            if (!essenceUnlocked)
            {
                Widgets.Label(new Rect(rect.x + 10f, rect.y + 26f, rect.width - 20f, 26f),
                    "Slot " + (index + 1) + "   —   locked");
                return;
            }

            if (slot == null || !slot.IsAwakened)
            {
                Widgets.Label(new Rect(rect.x + 10f, rect.y + 13f, rect.width - 20f, 22f),
                    "Slot " + (index + 1) + "   —   Empty");
                Color prev = GUI.color;
                GUI.color = Muted;
                Widgets.Label(new Rect(rect.x + 10f, rect.y + 39f, rect.width - 20f, 27f),
                    "Requires an awakening stone");
                GUI.color = prev;
                return;
            }

            EssencePowerDef power = DefDatabase<EssencePowerDef>.GetNamedSilentFail(slot.abilityDefName);
            float left = rect.x + 12f;
            if (power != null && !string.IsNullOrEmpty(power.iconPath))
            {
                Texture2D texture = ContentFinder<Texture2D>.Get(power.iconPath, false);
                if (texture != null)
                {
                    GUI.DrawTexture(new Rect(left, rect.y + 16f, 40f, 40f),
                        texture, ScaleMode.ScaleToFit);
                    left += 49f;
                }
            }

            float contentWidth = rect.xMax - left - 9f;
            string title = power == null ? "Unresolved power" : power.label.CapitalizeFirst();
            string source = slot.sourceKind == "innate" ? "INNATE" : "AWAKENED";
            Widgets.Label(new Rect(left, rect.y + 4f, contentWidth, 22f),
                (index + 1) + ". " + title + "   [" + source + "]");

            int remaining = slot.nextUseTick - Find.TickManager.TicksGame;
            string cooldown = remaining > 0
                ? "Cooldown: " + ((remaining + 59) / 60) + " s"
                : "Ready";
            string description = power == null ? "Power definition could not be found."
                : power.description;
            Widgets.Label(new Rect(left, rect.y + 24f, contentWidth, 33f), description);
            Color previousColor = GUI.color;
            GUI.color = Muted;
            float needed = EssenceProgression.NextSublevelWork(slot.abilityRankIndex);
            bool stalled = state.IsTierTrainingStalled(slot.abilityRankIndex);
            string training = needed > 0f
                ? EssenceProgression.RankWithLevel(slot.abilityRankIndex, slot.abilitySublevel) +
                  (stalled ? " [WAITING FOR OTHER ESSENCES]" : "") + "  •  " +
                  slot.rankWork.ToString("0.#") + "/" + needed.ToString("0.#") + " " +
                  EssenceProgression.WorkLabel(power?.effectKind ?? "")
                : "Diamond rank  •  mastered";
            Widgets.Label(new Rect(left, rect.y + 58f, contentWidth, 17f), training);
            if (needed > 0f)
            {
                Rect meter = new Rect(left, rect.y + 76f, contentWidth - 5f, 4f);
                Widgets.DrawBoxSolid(meter, PanelBackground);
                float fraction = Mathf.Clamp01(slot.rankWork / needed);
                Widgets.DrawBoxSolid(new Rect(meter.x, meter.y, meter.width * fraction, meter.height),
                    Highlight);
            }
            Widgets.Label(new Rect(left, rect.y + 82f, contentWidth, 17f), cooldown);
            GUI.color = previousColor;
        }
    }
}
