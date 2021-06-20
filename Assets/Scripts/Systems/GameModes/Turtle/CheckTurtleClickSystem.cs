using Leopotam.Ecs;
using Leopotam.Ecs.Ui.Components;

namespace Pixelgrid 
{
    public sealed class CheckTurtleClickSystem : IEcsRunSystem 
    {
        private EcsFilter<EcsUiClickEvent> _filter;
        private EcsFilter<GameplayEventReceiver> _eventReceiverFilter;
        private GameState _gameState;

        void IEcsRunSystem.Run()
        {
            if (!_gameState.IsPaused)
            {
                foreach (var index in _filter)
                {
                    ref EcsUiClickEvent data = ref _filter.Get1(index);
                    var sender = data.Sender;
                    var turtleControl = sender.GetComponent<TurtleControl>();
                    if (turtleControl)
                    {
                        foreach(var eventReceiverIndex in _eventReceiverFilter)
                        {
                            var entity = _eventReceiverFilter.GetEntity(eventReceiverIndex);
                            ref var turtleCommand = ref entity.Get<TurtleCommand>();
                            turtleCommand.CommandSymbol = turtleControl.CommandSymbol;
                        }
                    }
                }
            }
        }
    }
}