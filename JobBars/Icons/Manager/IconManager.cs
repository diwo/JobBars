using FFXIVClientStructs.FFXIV.Client.UI;
using JobBars.Data;
using JobBars.GameStructs;
using JobBars.Helper;
using System.Linq;

namespace JobBars.Icons.Manager {
    public unsafe partial class IconManager : PerJobManager<IconReplacer[]> {
        public JobIds CurrentJob = JobIds.OTHER;
        private IconReplacer[] CurrentIcons => JobToValue.TryGetValue( CurrentJob, out var gauges ) ? gauges : JobToValue[JobIds.OTHER];

        public IconManager() : base( "##JobBars_Icons" ) { }

        public void SetJob( JobIds job ) {
            CurrentJob = job;
        }

        public void Reset() => SetJob( CurrentJob );

        public void ResetJob( JobIds job ) {
            if( job == CurrentJob ) Reset();
        }

        public void PerformAction( Item action ) {
            if( !JobBars.Configuration.IconsEnabled ) return;
            foreach( var icon in CurrentIcons.Where( i => i.Enabled ) ) icon.ProcessAction( action );
        }

        public void Tick() {
            if( !JobBars.Configuration.IconsEnabled ) return;
            foreach( var icon in CurrentIcons.Where( i => i.Enabled ) ) icon.Tick();
        }

        public void UpdateIcon( HotbarSlotStruct* data, ActionBarSlot slot ) {
            if( !JobBars.Configuration.IconsEnabled ) return;
            var action = UiHelper.GetAdjustedAction( data->ActionId );
            CurrentIcons.FirstOrDefault( i => i.AppliesTo( action ) )?.UpdateIcon( data, slot );
        }
    }
}
