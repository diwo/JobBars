using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using JobBars.Data;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Status = FFXIVClientStructs.FFXIV.Client.Game.Status;

namespace JobBars.Helper {
    public struct ItemData {
        public string Name;
        public Item Data;
        public uint Icon;

        public override readonly string ToString() {
            return Name;
        }

        public override readonly bool Equals( object obj ) {
            return obj is ItemData overrides && Equals( overrides );
        }

        public readonly bool Equals( ItemData other ) {
            return Data.Id == other.Data.Id;
        }

        public override readonly int GetHashCode() {
            return HashCode.Combine( Name, Data );
        }

        public static bool operator ==( ItemData left, ItemData right ) {
            return left.Equals( right );
        }

        public static bool operator !=( ItemData left, ItemData right ) {
            return !( left == right );
        }
    }

    public struct ClassJobActionInfo {
        public uint ActionId;
        public byte Level;
        public bool IsRoleAction;
    }

    public struct ClassJobActionEvolution {
        public uint ActionId;
        /// <summary>
        /// Action ID of the greatest ancestor.
        /// </summary>
        public uint BaseActionId;
    }

    public class ClassJobActionLevelRange {
        public byte MinLevel;
        public byte ReplacedLevel;
    }

    public unsafe partial class UiHelper {
        public static bool OutOfCombat => !Dalamud.Condition[ConditionFlag.InCombat];
        public static bool WeaponSheathed => Dalamud.ClientState.LocalPlayer != null && !Dalamud.ClientState.LocalPlayer.StatusFlags.HasFlag( StatusFlags.WeaponOut );
        public static bool WatchingCutscene =>
            Dalamud.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Dalamud.Condition[ConditionFlag.WatchingCutscene78] ||
            Dalamud.Condition[ConditionFlag.BetweenAreas] || Dalamud.Condition[ConditionFlag.BetweenAreas51] ||
            Dalamud.Condition[ConditionFlag.OccupiedInEvent] || Dalamud.Condition[ConditionFlag.OccupiedInQuestEvent];

        public static bool CalcDoHide( bool enabled, bool hideOutOfCombat, bool hideWeaponSheathed ) {
            if( !enabled ) return true;
            if( OutOfCombat && hideOutOfCombat ) return true;
            if( WeaponSheathed && hideWeaponSheathed ) return true;
            if( WatchingCutscene ) return true;
            if( Dalamud.ClientState.IsPvP ) return true;
            return false;
        }

        public static bool CalcDoHide(bool enabled, bool hideOutOfCombat, bool hideWeaponSheathed, bool showWeaponUnsheathed) {
            if( !enabled ) return true;
            if( WatchingCutscene ) return true;
            if( Dalamud.ClientState.IsPvP ) return true;
            if( !WeaponSheathed && showWeaponUnsheathed ) return false;
            if( OutOfCombat && hideOutOfCombat ) return true;
            if( WeaponSheathed && hideWeaponSheathed ) return true;
            return false;
        }

        private static readonly HashSet<uint> GCDs = [];
        private static readonly Dictionary<uint, uint> ActionToIcon = [];

        public static List<ItemData> StatusList { get; private set; } = [];
        public static List<ItemData> ActionList { get; private set; } = [];

        public static bool IsGcd( ActionIds action ) => IsGcd( ( uint )action );
        public static bool IsGcd( uint action ) => GCDs.Contains( action );

        public static uint GetIcon( ActionIds action ) => GetIcon( ( uint )action );
        public static uint GetIcon( uint action ) => ActionToIcon[action];

        public static bool IsActionAvailableAtLevel( ActionIds action, byte level ) => IsActionAvailableAtLevel( (uint)action, level );
        public static bool IsActionAvailableAtLevel( uint action, byte level ) {
            if ( level == 0 ) return true;
            var actionInfo =  ClassJobActionIdToInfo.GetValueOrDefault( action );
            if ( actionInfo.IsRoleAction ) return true;
            if ( ClassJobActionToLevelRange.TryGetValue( action, out var levelRange ) ) {
                return level >= levelRange.MinLevel && ( levelRange.ReplacedLevel == 0 || level < levelRange.ReplacedLevel );
            }
            return level >= actionInfo.Level;
        }

        public static JobIds IdToJob( uint job ) => job < 19 ? JobIds.OTHER : ( JobIds )job;

        private static IEnumerable<ClassJob> JobSheet;
        private static IEnumerable<Lumina.Excel.Sheets.Action> ActionSheet;
        private static IEnumerable<Lumina.Excel.Sheets.Status> StatusSheet;

        // Cache converted strings
        private static Dictionary<JobIds, string> JobToString;
        private static Dictionary<Item, string> ItemToString;
        private static Dictionary<uint, ClassJobActionInfo> ClassJobActionIdToInfo = new();
        private static Dictionary<uint, ClassJobActionLevelRange> ClassJobActionToLevelRange = new();

        public static string Localize( JobIds job ) {
            if( JobToString.TryGetValue( job, out var jobString ) ) return jobString;
            else {
                var convertedJob = ConvertJobToString( job );
                JobToString[job] = convertedJob;
                return convertedJob;
            }
        }

        public static string ProcText => Dalamud.ClientState.ClientLanguage switch {
            ClientLanguage.Japanese => "Procs",
            ClientLanguage.English => "Procs",
            ClientLanguage.German => "Procs",
            ClientLanguage.French => "Procs",
            _ => "触发"
        };

        public static string Localize( ActionIds action ) => Localize( new Item( action ) );
        public static string Localize( BuffIds buff ) => Localize( new Item( buff ) );
        public static string Localize( Item item ) {
            if( ItemToString.TryGetValue( item, out var itemString ) ) return itemString;
            else {
                var convertedItem = ConvertItemToString( item );
                ItemToString[item] = convertedItem;
                return convertedItem;
            }
        }

        private static string ConvertJobToString( JobIds job ) {
            foreach( var classJob in JobSheet ) {
                if( classJob.RowId == ( uint )job ) return ToTitleCase( classJob.Name.ExtractText() );
            }
            return "ERROR";
        }

        private static string ConvertItemToString( Item item ) {
            if( item.Type == ItemType.Buff ) {
                var buff = StatusSheet.Where( x => x.RowId == item.Id );
                return !buff.Any() ? "Unknown" : ToTitleCase( buff.First().Name.ExtractText() );
            }
            else {
                var action = ActionSheet.Where( x => x.RowId == item.Id );
                return !action.Any() ? "Unknown" : ToTitleCase( action.First().Name.ExtractText() );
            }
        }

        private static string ToTitleCase( this string title ) {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase( title );
        }

        private static void SetupSheets() {
            JobToString = [];
            ItemToString = [];
            ActionList.Clear();
            StatusList.Clear();
            ClassJobActionIdToInfo.Clear();

            ActionSheet = Dalamud.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>().Where(
                x => !string.IsNullOrEmpty( x.Name.ExtractText() ) && ( x.IsPlayerAction || x.ClassJob.ValueNullable != null ) && !x.IsPvP // weird conditions to catch things like enchanted RDM spells
            );
            foreach( var item in ActionSheet ) {
                var name = item.Name.ToString();
                var attackType = item.ActionCategory.Value.Name.ToString();
                var categoryId = item.ActionCategory.Value.RowId;
                if( item.Icon != 405 && item.Icon != 0 ) ActionToIcon[item.RowId] = item.Icon;

                if( categoryId == 2 || categoryId == 3 ) { // spell or weaponskill
                    if( item.CooldownGroup == 58 || item.AdditionalCooldownGroup == 58 ) GCDs.Add( item.RowId ); // not actually a gcd
                }

                ActionList.Add( new ItemData {
                    Name = item.Name.ExtractText(),
                    Icon = item.Icon,
                    Data = new Item {
                        Id = item.RowId,
                        Type = ItemType.Action
                    }
                } );

                ClassJobActionIdToInfo.Add( item.RowId, new ClassJobActionInfo {
                    ActionId = item.RowId,
                    Level = item.ClassJobLevel,
                    IsRoleAction = item.IsRoleAction
                } );
            }

            StatusSheet = Dalamud.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Status>().Where( x => !string.IsNullOrEmpty( x.Name.ExtractText() ) );
            foreach( var item in StatusSheet ) {
                StatusList.Add( new ItemData {
                    Name = item.Name.ExtractText(),
                    Icon = item.Icon,
                    Data = new Item {
                        Id = item.RowId,
                        Type = ItemType.Buff
                    }
                } );
            }

            JobSheet = Dalamud.DataManager.GetExcelSheet<ClassJob>().Where( x => x.Name.ExtractText() != null );

            SetupClassJobActionLevelRanges( ClassJobActionIdToInfo );
        }

        private static void SetupClassJobActionLevelRanges( Dictionary<uint, ClassJobActionInfo> classJobActionIdToInfo ) {
            ClassJobActionToLevelRange.Clear();

            var replaceActionSheet = Dalamud.DataManager.GetSubrowExcelSheet<ReplaceAction>();
            Dictionary<uint, bool> isUpgradeAction = new();
            List<List<uint>> actionGroups = new();
            foreach ( var row in replaceActionSheet ) {
                foreach ( var subrow in row ) {
                    var action = subrow.Action.Value.RowId;
                    if( classJobActionIdToInfo.ContainsKey( action ) ) {
                        var actionGroup = new List<uint>();
                        actionGroup.Add( action );
                        isUpgradeAction[action] = true;

                        var replaceActions = subrow.ReplaceActions.Select( a => a.Value.RowId ).ToList();
                        sbyte[] types = [ subrow.Type1, subrow.Type2, subrow.Type3 ];

                        for ( var i = 0; i < Math.Min( replaceActions.Count, 3 ); i++ ) {
                            if ( classJobActionIdToInfo.ContainsKey( replaceActions[i] ) ) {
                                actionGroup.Add( replaceActions[i] );
                                isUpgradeAction[replaceActions[i]] = types[i] == 3;
                            }
                        }

                        actionGroups.Add( actionGroup );
                    }
                }
            }

            foreach ( var actionGroup in actionGroups ) {
                actionGroup.Sort( ( a1, a2 ) => {
                    var a1Level = classJobActionIdToInfo.GetValueOrDefault( a1 ).Level;
                    var a2Level = classJobActionIdToInfo.GetValueOrDefault( a2 ).Level;
                    return a1Level - a2Level;
                } );

                for ( int i = 0; i < actionGroup.Count; i++ ) {
                    byte minLevel = classJobActionIdToInfo.GetValueOrDefault( actionGroup[i] ).Level;
                    byte replacedLevel = 0;
                    for ( int j = i + 1; j < actionGroup.Count; j++ ) {
                        if ( isUpgradeAction[actionGroup[i]] && isUpgradeAction[actionGroup[j]] ) {
                            replacedLevel = classJobActionIdToInfo.GetValueOrDefault( actionGroup[j] ).Level;
                        }
                    }
                    ClassJobActionToLevelRange[actionGroup[i]] = new ClassJobActionLevelRange {
                        MinLevel = minLevel,
                        ReplacedLevel = replacedLevel
                    };
                }
            }
        }

        public static float TimeLeft( float defaultDuration, Dictionary<Item, Status> buffDict, Item lastActiveTrigger, DateTime lastActiveTime ) {
            if( lastActiveTrigger.Type == ItemType.Buff ) {
                if( buffDict.TryGetValue( lastActiveTrigger, out var elem ) ) { // duration exists, use that
                    return elem.RemainingTime;
                }
                else { // time isn't there, are we just waiting on it?
                    var timeSinceActive = ( DateTime.Now - lastActiveTime ).TotalSeconds;
                    if( timeSinceActive <= 2 ) { // hasn't been enough time for it to show up in the buff list
                        return defaultDuration;
                    }
                    return -1; // yeah lmao it's gone
                }
            }
            else {
                return ( float )( defaultDuration - ( DateTime.Now - lastActiveTime ).TotalSeconds ); // triggered by an action, just calculate the time
            }
        }
    }
}
