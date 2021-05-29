using Leopotam.Ecs;

namespace Pixelgrid
{
    public sealed class LaunchCountdownTimerSystem : IEcsRunSystem
    {
        private EcsFilter<RestartGameEvent> _filter;
        private EcsFilter<Timer, CountdownTimer> _countdownTimerFilter;

        void IEcsRunSystem.Run()
        {
            if(!_filter.IsEmpty())
            {
                 foreach(var index in _countdownTimerFilter)
                {
                    var entity = _countdownTimerFilter.GetEntity(index);
                    ref var timer = ref _countdownTimerFilter.Get1(index);
                    timer.startTime = 4f;
                    timer.currentTime = timer.startTime;
                    entity.Get<Counting>();
                }
            }
        }
    }
}