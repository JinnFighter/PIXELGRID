using Leopotam.Ecs;
using System.Collections.Generic;

namespace Pixelgrid 
{
    public sealed class PauseSystem : IEcsRunSystem 
    {
        private readonly EcsFilter<PauseEvent> _filter = null;

        private readonly EcsSystems _systems;
        private readonly IEnumerable<string> _pausableSystemsNames;

        public PauseSystem(EcsSystems systems, IEnumerable<string> pausableSystemsNames)
        {
            _systems = systems;
            _pausableSystemsNames = pausableSystemsNames;
        }
        
        void IEcsRunSystem.Run() 
        {
            if(!_filter.IsEmpty())
            {
                foreach (var systemName in _pausableSystemsNames)
                    _systems.SetRunSystemState(_systems.GetNamedRunSystem(systemName), false);

                var entity = _filter.GetEntity(0);
                entity.Get<Paused>();
            }
        }
    }
}