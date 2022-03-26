using Leopotam.Ecs;
using System.Collections.Generic;
using Configurations.Script;
using Pixelgrid.ScriptableObjects.Sprites;
using UnityEngine;

namespace Pixelgrid 
{
    public sealed class ResetTurtlePositionSystem : IEcsRunSystem 
    {
        private readonly EcsFilter<RestartGameEvent> _restartEventFilter = null;
        private readonly EcsFilter<TurtleComponent, PixelPosition> _filter = null;

        private readonly DifficultyConfiguration _difficultyConfiguration = null;
        private readonly TurtleSpritesContent _turtleSpritesContent = null;
        private readonly TurtleConfigs _turtleConfigs = null;

        void IEcsRunSystem.Run() 
        {
            if(!_restartEventFilter.IsEmpty())
            {
                foreach (var index in _filter)
                {
                    ref var positionComponent = ref _filter.Get2(index);

                    var config = _turtleConfigs.Configs[_difficultyConfiguration.Difficulty];
                    positionComponent.position = config.TurtleStartPosition;

                    var entity = _filter.GetEntity(index);
                    entity.Get<ClearGridEvent>();
                    ref var drawData = ref entity.Get<LineDrawData>();
                    drawData.drawData = new List<(Vector2Int, Sprite)>
                {
                    (positionComponent.position, _turtleSpritesContent.TurtleRight)
                };

                    ref var turtle = ref _filter.Get1(index);
                    turtle.CurrentSprite = _turtleSpritesContent.TurtleRight;
                    turtle.DirectionState = new RightDirectionState();
                }
            }
        }
    }
}