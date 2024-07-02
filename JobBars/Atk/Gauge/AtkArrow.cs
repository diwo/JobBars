using FFXIVClientStructs.FFXIV.Component.GUI;
using JobBars.Data;
using JobBars.Helper;
using System.Collections.Generic;

namespace JobBars.Atk {
    public unsafe class AtkArrow : AtkGauge {
        private class UIArrowTick {
            public AtkResNode* MainTick;
            public AtkImageNode* Background;
            public AtkResNode* SelectedContainer;
            public AtkImageNode* Selected;

            private ElementColor Color = AtkColor.NoColor;

            public void Dispose() {
                if( Selected != null ) {
                    AtkHelper.UnloadTexture( Selected );
                    Selected->AtkResNode.Destroy( true );
                    Selected = null;
                }

                if( SelectedContainer != null ) {
                    SelectedContainer->Destroy( true );
                    SelectedContainer = null;
                }

                if( Background != null ) {
                    AtkHelper.UnloadTexture( Background );
                    Background->AtkResNode.Destroy( true );
                    Background = null;
                }

                if( MainTick != null ) {
                    MainTick->Destroy( true );
                    MainTick = null;
                }
            }

            public void SetColor( ElementColor color ) {
                Color = color;
                AtkColor.SetColor( Selected, color );
            }

            public void Tick( float percent ) {
                AtkColor.SetColorPulse( ( AtkResNode* )Selected, Color, percent );
            }
        }

        public static readonly int MAX = 12;
        private List<UIArrowTick> Ticks = [];

        public AtkArrow() : base() {
            RootRes = AtkBuilder.CreateResNode();
            RootRes->X = 0;
            RootRes->Y = 0;
            RootRes->Width = 160;
            RootRes->Height = 46;

            List<LayoutNode> tickNodes = [];

            for( var idx = 0; idx < MAX; idx++ ) {
                // ======= TICKS =========
                var tick = AtkBuilder.CreateResNode();
                tick->X = 18 * idx;
                tick->Y = 0;
                tick->Width = 32;
                tick->Height = 32;

                var bg = AtkBuilder.CreateImageNode();
                bg->AtkResNode.Width = 32;
                bg->AtkResNode.Height = 32;
                bg->AtkResNode.X = 0;
                bg->AtkResNode.Y = 0;
                AtkHelper.SetupTexture( bg, "ui/uld/JobHudSimple_StackB.tex" );
                AtkHelper.UpdatePart( bg, 0, 0, 32, 32 );
                bg->Flags = 0;
                bg->WrapMode = 1;

                // ======== SELECTED ========
                var selectedContainer = AtkBuilder.CreateResNode();
                selectedContainer->X = 0;
                selectedContainer->Y = 0;
                selectedContainer->Width = 32;
                selectedContainer->Height = 32;
                selectedContainer->OriginX = 16;
                selectedContainer->OriginY = 16;

                var selected = AtkBuilder.CreateImageNode();
                selected->AtkResNode.Width = 32;
                selected->AtkResNode.Height = 32;
                selected->AtkResNode.X = 0;
                selected->AtkResNode.Y = 0;
                selected->AtkResNode.OriginX = 16;
                selected->AtkResNode.OriginY = 16;
                AtkHelper.SetupTexture( selected, "ui/uld/JobHudSimple_StackB.tex" );
                AtkHelper.UpdatePart( selected, 32, 0, 32, 32 );
                selected->Flags = 0;
                selected->WrapMode = 1;

                Ticks.Add( new UIArrowTick {
                    MainTick = tick,
                    Background = bg,
                    Selected = selected,
                    SelectedContainer = selectedContainer
                } );

                tickNodes.Add( new LayoutNode( tick, new[] {
                    new LayoutNode(bg),
                    new LayoutNode(selectedContainer, new[] {
                        new LayoutNode(selected)
                    })
                } ) );
            }

            var layout = new LayoutNode( RootRes, tickNodes.ToArray() );
            layout.Setup();
            layout.Cleanup();
        }

        public override void Dispose() {
            for( var idx = 0; idx < MAX; idx++ ) {
                Ticks[idx].Dispose();
                Ticks[idx] = null;
            }
            Ticks = null;

            if( RootRes != null ) {
                RootRes->Destroy( true );
                RootRes = null;
            }
        }

        public void SetMaxValue( int value ) {
            for( var idx = 0; idx < MAX; idx++ ) {
                AtkHelper.SetVisibility( Ticks[idx].MainTick, idx < value );
            }
        }

        public void SetColor( int idx, ElementColor color ) {
            Ticks[idx].SetColor( color );
        }

        public void SetValue( int idx, bool value ) {
            var prevVisible = Ticks[idx].Selected->AtkResNode.IsVisible();
            AtkHelper.SetVisibility( Ticks[idx].Selected, value );

            if( value && !prevVisible ) {
                var item = ( AtkResNode* )Ticks[idx].Selected;
                Animation.AddAnim( ( float f ) => AtkHelper.SetScale( item, f, f ), 0.2f, 2.5f, 1.0f );
            }
        }

        public void Clear() {
            for( var idx = 0; idx < MAX; idx++ ) SetValue( idx, false );
        }

        public void Tick( float percent ) {
            Ticks.ForEach( t => t.Tick( percent ) );
        }
    }
}
