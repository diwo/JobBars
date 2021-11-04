﻿using JobBars.Buffs;
using JobBars.Cooldowns;
using JobBars.Cursors;
using JobBars.Data;

using JobBars.Gauges;
using JobBars.Gauges.Procs;
using JobBars.Gauges.Timer;
using JobBars.Helper;
using JobBars.Icons;
using JobBars.UI;
using System;

namespace JobBars.Jobs {
    public static class SCH {
        public static GaugeConfig[] Gauges => new GaugeConfig[] {
            new GaugeProcsConfig($"{UIHelper.Localize(JobIds.SCH)} {UIHelper.ProcText}", GaugeVisualType.Diamond, new GaugeProcProps{
                Procs = new []{
                    new ProcConfig(UIHelper.Localize(BuffIds.Excog), BuffIds.Excog, UIColor.BrightGreen)
                },
                NoSoundOnProc = true
            }),
            new GaugeTimerConfig(UIHelper.Localize(BuffIds.Biolysis), GaugeVisualType.Bar, new GaugeSubTimerProps {
                MaxDuration = 30,
                Color = UIColor.BlueGreen,
                Triggers = new []{
                    new Item(BuffIds.ArcBio),
                    new Item(BuffIds.ArcBio2),
                    new Item(BuffIds.Biolysis)
                }
            })
        };

        public static BuffConfig[] Buffs => new[] {
            new BuffConfig(UIHelper.Localize(ActionIds.ChainStratagem), new BuffProps {
                CD = 120,
                Duration = 15,
                Icon = ActionIds.ChainStratagem,
                Color = UIColor.White,
                Triggers = new []{ new Item(ActionIds.ChainStratagem) }
            })
        };

        public static Cursor Cursors => new(JobIds.SCH, CursorType.None, CursorType.CastTime);

        public static CooldownConfig[] Cooldowns => new[] {
            new CooldownConfig(UIHelper.Localize(ActionIds.SummonSeraph), new CooldownProps {
                Icon = ActionIds.SummonSeraph,
                Duration = 22,
                CD = 120,
                Triggers = new []{ new Item(ActionIds.SummonSeraph) }
            }),
            new CooldownConfig(UIHelper.Localize(ActionIds.DeploymentTactics), new CooldownProps {
                Icon = ActionIds.DeploymentTactics,
                CD = 120,
                Triggers = new []{ new Item(ActionIds.DeploymentTactics) }
            }),
            new CooldownConfig(UIHelper.Localize(ActionIds.Recitation), new CooldownProps {
                Icon = ActionIds.Recitation,
                CD = 90,
                Triggers = new []{ new Item(ActionIds.Recitation) }
            }),
            new CooldownConfig($"{UIHelper.Localize(ActionIds.Swiftcast)} ({UIHelper.Localize(JobIds.SCH)})", new CooldownProps {
                Icon = ActionIds.Swiftcast,
                CD = 60,
                Triggers = new []{ new Item(ActionIds.Swiftcast) }
            })
        };

        public static IconReplacer[] Icons => new[] {
            new IconReplacer(UIHelper.Localize(ActionIds.ChainStratagem), new IconProps {
                Icons = new [] { ActionIds.ChainStratagem },
                Triggers = new[] {
                    new IconTriggerStruct { Trigger = new Item(BuffIds.ChainStratagem), Duration = 15 }
                }
            }),
            new IconReplacer(UIHelper.Localize(BuffIds.Biolysis), new IconProps {
                IsTimer = true,
                Icons = new [] {
                    ActionIds.SchBio,
                    ActionIds.SchBio2,
                    ActionIds.Biolysis
                },
                Triggers = new[] {
                    new IconTriggerStruct { Trigger = new Item(BuffIds.ArcBio), Duration = 30 },
                    new IconTriggerStruct { Trigger = new Item(BuffIds.ArcBio2), Duration = 30 },
                    new IconTriggerStruct { Trigger = new Item(BuffIds.Biolysis), Duration = 30 }
                }
            })
        };
    }
}
