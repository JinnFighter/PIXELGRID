using Leopotam.Ecs;
using Pixelgrid.Components.Time.Timer;

namespace Pixelgrid.Systems.Timers
{
    public sealed class GenerateCountdownTimersSystem : IEcsInitSystem 
    {
        private readonly EcsWorld _world = null;

        public void Init()
        {
            var timerEntity = _world.NewEntity();
            timerEntity.Get<Timer>();
            timerEntity.Get<CountdownTimer>();
        }
    }
}