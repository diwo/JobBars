using System;
using System.Runtime.InteropServices;

namespace JobBars.GameStructs {
    [StructLayout( LayoutKind.Explicit )]
    public unsafe struct AddonHotbarNumberArray {
        [FieldOffset( 0x000 )] public bool Locked;
        [FieldOffset( 0x00C )] public uint LoadedFlags;
        [FieldOffset( 0x03C )] public AddonHotbars Hotbars;

        public readonly bool IsLoaded( int pos ) {
            return ( LoadedFlags & ( 1 << pos ) ) != 0;
        }
    }

    [StructLayout( LayoutKind.Explicit )]
    public unsafe struct AddonHotbars {
        [FieldOffset( 0x44 * 16 * 0 )] public HotbarStruct HotBar_1;
        [FieldOffset( 0x44 * 16 * 1 )] public HotbarStruct HotBar_2;
        [FieldOffset( 0x44 * 16 * 2 )] public HotbarStruct HotBar_3;
        [FieldOffset( 0x44 * 16 * 3 )] public HotbarStruct HotBar_4;
        [FieldOffset( 0x44 * 16 * 4 )] public HotbarStruct HotBar_5;
        [FieldOffset( 0x44 * 16 * 5 )] public HotbarStruct HotBar_6;
        [FieldOffset( 0x44 * 16 * 6 )] public HotbarStruct HotBar_7;
        [FieldOffset( 0x44 * 16 * 7 )] public HotbarStruct HotBar_8;
        [FieldOffset( 0x44 * 16 * 8 )] public HotbarStruct HotBar_9;
        [FieldOffset( 0x44 * 16 * 9 )] public HotbarStruct HotBar_10;

        [FieldOffset( 0x44 * 16 * 10 )] public HotbarStruct CrossHotBar_1;
        [FieldOffset( 0x44 * 16 * 11 )] public HotbarStruct CrossHotBar_2;
        [FieldOffset( 0x44 * 16 * 12 )] public HotbarStruct CrossHotBar_3;
        [FieldOffset( 0x44 * 16 * 13 )] public HotbarStruct CrossHotBar_4;
        [FieldOffset( 0x44 * 16 * 14 )] public HotbarStruct CrossHotBar_5;
        [FieldOffset( 0x44 * 16 * 15 )] public HotbarStruct CrossHotBar_6;
        [FieldOffset( 0x44 * 16 * 16 )] public HotbarStruct CrossHotBar_7;
        [FieldOffset( 0x44 * 16 * 17 )] public HotbarStruct CrossHotBar_8;

        public readonly HotbarStruct this[int i] => i switch {
            0 => HotBar_1,
            1 => HotBar_2,
            2 => HotBar_3,
            3 => HotBar_4,
            4 => HotBar_5,
            5 => HotBar_6,
            6 => HotBar_7,
            7 => HotBar_8,
            8 => HotBar_9,
            9 => HotBar_10,
            10 => CrossHotBar_1,
            11 => CrossHotBar_2,
            12 => CrossHotBar_3,
            13 => CrossHotBar_4,
            14 => CrossHotBar_5,
            15 => CrossHotBar_6,
            16 => CrossHotBar_7,
            17 => CrossHotBar_8,
            _ => throw new IndexOutOfRangeException( "Index should be between 0 and 17" )
        };
    }

    [StructLayout( LayoutKind.Explicit, Size = 0x44 * 16 )]
    public unsafe struct HotbarStruct {
        [FieldOffset( 0x44 * 0 )] public HotbarSlotStruct Slot_1;
        [FieldOffset( 0x44 * 1 )] public HotbarSlotStruct Slot_2;
        [FieldOffset( 0x44 * 2 )] public HotbarSlotStruct Slot_3;
        [FieldOffset( 0x44 * 3 )] public HotbarSlotStruct Slot_4;
        [FieldOffset( 0x44 * 4 )] public HotbarSlotStruct Slot_5;
        [FieldOffset( 0x44 * 5 )] public HotbarSlotStruct Slot_6;
        [FieldOffset( 0x44 * 6 )] public HotbarSlotStruct Slot_7;
        [FieldOffset( 0x44 * 7 )] public HotbarSlotStruct Slot_8;
        [FieldOffset( 0x44 * 8 )] public HotbarSlotStruct Slot_9;
        [FieldOffset( 0x44 * 9 )] public HotbarSlotStruct Slot_10;
        [FieldOffset( 0x44 * 10 )] public HotbarSlotStruct Slot_11;
        [FieldOffset( 0x44 * 11 )] public HotbarSlotStruct Slot_12;
        [FieldOffset( 0x44 * 12 )] public HotbarSlotStruct Slot_13;
        [FieldOffset( 0x44 * 13 )] public HotbarSlotStruct Slot_14;
        [FieldOffset( 0x44 * 14 )] public HotbarSlotStruct Slot_15;
        [FieldOffset( 0x44 * 15 )] public HotbarSlotStruct Slot_16;

        public readonly HotbarSlotStruct this[int i] => i switch {
            0 => Slot_1,
            1 => Slot_2,
            2 => Slot_3,
            3 => Slot_4,
            4 => Slot_5,
            5 => Slot_6,
            6 => Slot_7,
            7 => Slot_8,
            8 => Slot_9,
            9 => Slot_10,
            10 => Slot_11,
            11 => Slot_12,
            12 => Slot_13,
            13 => Slot_14,
            14 => Slot_15,
            15 => Slot_16,
            _ => throw new IndexOutOfRangeException( "Index should be between 0 and 15" )
        };
    }

    [StructLayout( LayoutKind.Explicit, Size = 0x44 )]
    public unsafe struct HotbarSlotStruct {
        [FieldOffset( 0x00 )] public HotbarSlotStructType Type;
        [FieldOffset( 0x0C )] public uint ActionId;

        [FieldOffset( 4 )] public uint TextColor;
        [FieldOffset( 8 )] public uint TextStyle;
        [FieldOffset( 28 )] public bool UseRing;
        [FieldOffset( 32 )] public uint GcdSwingPercent;
        [FieldOffset( 36 )] public uint CdPercent;
        [FieldOffset( 40 )] public uint CdText;
        [FieldOffset( 64 )] public bool OutOfRange;
        [FieldOffset( 24 )] public bool Usable;

        [FieldOffset( 0x38 )] public bool YellowBorder;
    }

    public enum HotbarSlotStructType : byte {
        Empty = 0x00,
        Action = 0x2E
    }
}
