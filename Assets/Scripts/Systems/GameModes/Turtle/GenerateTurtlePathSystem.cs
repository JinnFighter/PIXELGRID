using System.Collections.Generic;
using System.Linq;
using Configurations.Script;
using Leopotam.Ecs;
using Pixelgrid.DataModels;
using UnityEngine;

namespace Pixelgrid.Systems.GameModes.Turtle 
{
    public sealed class GenerateTurtlePathSystem : IEcsRunSystem 
    {
        private readonly EcsFilter<RestartGameEvent> _restartEventFilter = null;
        private readonly EcsWorld _world = null;
        
        private readonly DifficultyConfiguration _difficultyConfiguration = null;
        private readonly TurtleConfigs _turtleConfigs = null;

        private IDirectionState _direction;

        private readonly List<char> _commands = new List<char>{ TurtleModeConfig.ForwardSymbol, TurtleModeConfig.TurnLeftSymbol, TurtleModeConfig.TurnRightSymbol };

        private readonly TurtlePathModel _turtlePathModel = null;
        private readonly AnswersModel _answersModel = null;

        void IEcsRunSystem.Run() 
        {
            if(!_restartEventFilter.IsEmpty())
            {
                var paths = _turtlePathModel.Path;
                paths.Clear();

                _direction = new RightDirectionState();

                for (var i = 0; i < _turtleConfigs[_difficultyConfiguration.Difficulty].PathCount; i++)
                    paths.Add(GeneratePath());

                var entity = _world.NewEntity();

                _turtlePathModel.CurrentSymbol = 0;
                _turtlePathModel.CurrentPath = 0;

                ref var dataGeneratedEvent = ref entity.Get<GameModeDataGeneratedEvent>();
                dataGeneratedEvent.DataCount = paths.Sum(path => path.Count);
                _answersModel.MaxAnswerCount = dataGeneratedEvent.DataCount;
                _answersModel.CurrentAnswerCount = 0;
            }
        }

        List<char> GeneratePath()
        {
            var route = new List<char>();
            for (var j = 0; j < _turtleConfigs[_difficultyConfiguration.Difficulty].PathLength; j++)
            {
                var c = _commands[Random.Range(0, 3)];
                while (j == 0 && c != _commands[0])
                    c = _commands[Random.Range(0, 3)];

                route.Add(c);
            }

            return route;
        }

        bool CanMove(Vector2Int position, List<char> route, out Vector2Int currentPosition, int fieldSize)
        {
            currentPosition = new Vector2Int(position.x,  position.y);
            foreach (var symbol in route)
            {
                switch(symbol)
                {
                    case TurtleModeConfig.ForwardSymbol:
                        var tempPosition = _direction.Move(currentPosition);
                        if (tempPosition.x < 0 || tempPosition.x >= fieldSize ||
                            tempPosition.y < 0 || tempPosition.y >= fieldSize)
                        {
                            return false;
                        }
                        currentPosition = tempPosition;
                        break;
                    case TurtleModeConfig.TurnLeftSymbol:
                        _direction = _direction.RotateLeft(out _);
                        break;
                    case TurtleModeConfig.TurnRightSymbol:
                        _direction = _direction.RotateRight(out _);
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }
    }
}