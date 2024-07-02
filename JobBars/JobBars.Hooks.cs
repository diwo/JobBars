using Dalamud.Game.ClientState.Objects.Types;
using JobBars.Data;
using JobBars.GameStructs;
using JobBars.Helper;
using System;
using System.Collections.Generic;

namespace JobBars {
    public unsafe partial class JobBars {
        private void ReceiveActionEffect( int sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail ) {
            if( !IsLoaded || !PlayerExists ) {
                ReceiveActionEffectHook.Original( sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail );
                return;
            }

            var id = *( ( uint* )effectHeader.ToPointer() + 0x2 );
            var type = *( ( byte* )effectHeader.ToPointer() + 0x1F ); // 1 = action

            var selfId = ( int )Dalamud.ClientState.LocalPlayer.GameObjectId;
            var isSelf = sourceId == selfId;
            var isPet = !isSelf && ( GaugeManager?.CurrentJob == JobIds.SMN || GaugeManager?.CurrentJob == JobIds.SCH ) && IsPet( ( ulong )sourceId, selfId );
            var isParty = !isSelf && !isPet && IsInParty( ( uint )sourceId );

            if( type != 1 || !( isSelf || isPet || isParty ) ) {
                ReceiveActionEffectHook.Original( sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail );
                return;
            }

            var actionItem = new Item {
                Id = id,
                Type = ( AtkHelper.IsGCD( id ) ? ItemType.GCD : ItemType.OGCD )
            };

            if( !isParty ) { // don't let party members affect our gauge
                GaugeManager?.PerformAction( actionItem );
                IconManager?.PerformAction( actionItem );
            }
            if( !isPet ) {
                BuffManager?.PerformAction( actionItem, ( uint )sourceId );
                CooldownManager?.PerformAction( actionItem, ( uint )sourceId );
            }

            var targetCount = *( byte* )( effectHeader + 0x21 );

            var effectsEntries = 0;
            var targetEntries = 1;
            if( targetCount == 0 ) {
                effectsEntries = 0;
                targetEntries = 1;
            }
            else if( targetCount == 1 ) {
                effectsEntries = 8;
                targetEntries = 1;
            }
            else if( targetCount <= 8 ) {
                effectsEntries = 64;
                targetEntries = 8;
            }
            else if( targetCount <= 16 ) {
                effectsEntries = 128;
                targetEntries = 16;
            }
            else if( targetCount <= 24 ) {
                effectsEntries = 192;
                targetEntries = 24;
            }
            else if( targetCount <= 32 ) {
                effectsEntries = 256;
                targetEntries = 32;
            }

            List<EffectEntry> entries = new( effectsEntries );
            for( var i = 0; i < effectsEntries; i++ ) {
                entries.Add( *( EffectEntry* )( effectArray + i * 8 ) );
            }

            var targets = new ulong[targetEntries];
            for( var i = 0; i < targetCount; i++ ) {
                targets[i] = *( ulong* )( effectTrail + i * 8 );
            }

            for( var i = 0; i < entries.Count; i++ ) {
                var entryTarget = targets[i / 8];

                if( entries[i].type == ActionEffectType.ApplyStatusTarget || entries[i].type == ActionEffectType.ApplyStatusSource ) {
                    var buffItem = new Item {
                        Id = entries[i].value,
                        Type = ItemType.Buff
                    };

                    if( !isParty ) { // more accurate than using the status list
                        GaugeManager?.PerformAction( buffItem );
                        IconManager?.PerformAction( buffItem );
                    }
                }
            }

            ReceiveActionEffectHook.Original( sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail );
        }

        private void ActorControlSelf( uint entityId, uint id, uint arg0, uint arg1, uint arg2, uint arg3, uint arg4, uint arg5, ulong targetId, byte a10 ) {
            ActorControlSelfHook.Original( entityId, id, arg0, arg1, arg2, arg3, arg4, arg5, targetId, a10 );
            if( !IsLoaded ) return;

            if( entityId > 0 && id == Constants.ActorControlSelfId && entityId == Dalamud.ClientState.LocalPlayer?.GameObjectId ) {
                AtkHelper.UpdateActorTick();
            }
            else if( entityId > 0 && id == Constants.ActorControlOtherId ) {
                AtkHelper.UpdateDoTTick( entityId );
            }

            if( arg1 == Constants.WipeArg1 ) {
                GaugeManager?.Reset();
                IconManager?.Reset();
                BuffManager?.ResetTrackers();
                CooldownManager?.ResetTrackers();
                AtkHelper.ResetTicks();
            }
        }

        private IntPtr IconDimmedDetour( IntPtr iconUnk, byte dimmed ) {
            var icon = IconDimmedHook.Original( iconUnk, dimmed );
            if( !IsLoaded ) return icon;

            IconBuilder?.ProcessIconOverride( icon );
            return icon;
        }

        private void ZoneChanged( ushort data ) {
            if( !IsLoaded ) return;

            GaugeManager?.Reset();
            IconManager?.Reset();
            BuffManager?.ResetTrackers();
            // don't reset CDs on zone change
            AtkHelper.ResetTicks();
            AtkHelper.ZoneChanged( data );
        }

        private static bool IsPet( ulong objectId, int ownerId ) {
            if( objectId == 0 ) return false;
            foreach( var actor in Dalamud.Objects ) {
                if( actor == null ) continue;
                if( actor.GameObjectId == objectId ) {
                    if( actor is IBattleNpc npc ) {
                        if( npc.Address == IntPtr.Zero ) return false;
                        return npc.OwnerId == ownerId;
                    }
                    return false;
                }
            }
            return false;
        }
    }
}
