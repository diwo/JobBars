﻿using Dalamud.Plugin;
using JobBars.Data;
using JobBars.Helper;
using JobBars.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JobBars.UI.UIColor;

namespace JobBars.Gauges {
    public class GaugeTimer : Gauge {
        private UIIconManager Icon;
        private bool ReplaceIcon = false;
        private ActionIds[] ReplaceIconAction;

        private float Duration;
        private float MaxDuration;
        private float DefaultDuration;

        private bool ShowLowWarning = true;
        private float LastTimeLeft;
        private static float LowTimerWarning = 4.0f;

        public GaugeTimer(string name, float duration) : base(name) {
            MaxDuration = duration;
            DefaultDuration = MaxDuration;
            DefaultVisual = Visual = new GaugeVisual
            {
                Type = GaugeVisualType.Bar,
                Color = LightBlue
            };
        }

        public override void SetupVisual(bool resetValue = true) {
            UI?.SetColor(Visual.Color);
            if (resetValue) {
                if (UI is UIGauge gauge) {
                    gauge.SetText("0");
                    gauge.SetTextColor(NoColor);
                    gauge.SetPercent(0);
                }
            }
        }

        private void StartIcon() {
            if (!ReplaceIcon || !Configuration.Config.GaugeIconReplacement) return;
            foreach (var icon in ReplaceIconAction) {
                Icon.ActionIdToState[(uint)icon] = IconState.StartRunning;
            }
        }

        private void SetIcon(double current, float max) {
            if (!ReplaceIcon || !Configuration.Config.GaugeIconReplacement) return;
            foreach (var icon in ReplaceIconAction) {
                Icon.ActionIdToStatus[(uint)icon] = new IconProgress
                {
                    Current = current,
                    Max = max
                };
            }
        }

        private void ResetIcon() {
            if (!ReplaceIcon || !Configuration.Config.GaugeIconReplacement) return;
            foreach (var icon in ReplaceIconAction) {
                Icon.ActionIdToStatus[(uint)icon] = new IconProgress
                {
                    Current = 0,
                    Max = 1
                };
                Icon.ActionIdToState[(uint)icon] = IconState.DoneRunning;
            }
        }

        public override void Tick(DateTime time, Dictionary<Item, float> buffDict) {
            var timeLeft = TimeLeft(Duration, time, buffDict);
            if(timeLeft > 0 && State == GaugeState.Inactive) { // switching targets with DoTs on them, need to restart the icon, etc.
                State = GaugeState.Active;
                StartIcon();
            }

            if (State == GaugeState.Active) {
                if (timeLeft <= 0) {
                    timeLeft = 0; // prevent "-1" or something
                    State = GaugeState.Inactive;
                    ResetIcon();
                }

                if (UI is UIGauge gauge) {
                    if (LastTimeLeft >= LowTimerWarning && timeLeft < LowTimerWarning && timeLeft != 0 && ShowLowWarning) {
                        gauge.SetTextColor(Red);
                        if(Configuration.Config.SeNumber > 0) {
                            UiHelper._playSe(Configuration.Config.SeNumber + 36, 0, 0);
                        }
                    }
                    else if (LastTimeLeft < LowTimerWarning && timeLeft >= LowTimerWarning) {
                        gauge.SetTextColor(NoColor);
                    }

                    gauge.SetText(((int)timeLeft).ToString());
                    gauge.SetPercent((float)timeLeft / MaxDuration);
                    SetIcon(timeLeft, MaxDuration);
                }

                LastTimeLeft = timeLeft;
            }
        }

        public override void ProcessAction(Item action) {
            if (Triggers.Contains(action) && (!(State == GaugeState.Active) || AllowRefresh)) { // START
                SetActive(action);
                Duration = DefaultDuration;
                StartIcon();
            }
        }

        public override int GetHeight() {
            return UI == null ? 0 : UI.GetHeight(0);
        }

        public override int GetWidth() {
            return UI == null ? 0 : UI.GetWidth(0);
        }

        // ===== BUILDER FUNCS =====
        public GaugeTimer WithTriggers(Item[] triggers) {
            Triggers = triggers;
            return this;
        }

        public GaugeTimer WithStartHidden() {
            StartHidden = true;
            return this;
        }

        public GaugeTimer NoRefresh() {
            AllowRefresh = false;
            return this;
        }

        public GaugeTimer WithReplaceIcon(ActionIds[] action, UIIconManager icon) {
            Icon = icon;
            ReplaceIcon = true;
            ReplaceIconAction = action;
            return this;
        }

        public GaugeTimer WithDefaultDuration(float duration) {
            DefaultDuration = duration;
            return this;
        }

        public GaugeTimer WithNoLowWarning() {
            ShowLowWarning = false;
            return this;
        }

        public GaugeTimer WithVisual(GaugeVisual visual) {
            DefaultVisual = Visual = visual;
            if (Configuration.Config.GetColorOverride(Name, out var color)) {
                Visual.Color = color;
            }
            if (Configuration.Config.GaugeTypeOverride.TryGetValue(Name, out var type)) {
                Visual.Type = type;
            }
            return this;
        }
    }
}
