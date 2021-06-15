using Leopotam.Ecs;
using Leopotam.Ecs.Ui.Components;

namespace Pixelgrid 
{
    public sealed class CheckColorPickerClickSystem : IEcsRunSystem 
    {
        private EcsFilter<EcsUiClickEvent> _filter;
        private EcsFilter<GameplayEventReceiver> _eventReceiverFilter;
        private GameState _gameState;
        private ImageHolderContainer _imageHolderContainer;

        void IEcsRunSystem.Run()
        {
            if (!_gameState.IsPaused)
            {
                foreach (var index in _filter)
                {
                    ref EcsUiClickEvent data = ref _filter.Get1(index);
                    var sender = data.Sender;
                    if(sender.CompareTag("ColorCheckButton"))
                    {
                        var color = _imageHolderContainer.AnswerHolder.color;
                        foreach(var eventIndex in _eventReceiverFilter)
                        {
                            var entity = _eventReceiverFilter.GetEntity(eventIndex);
                            ref var chosenColor = ref entity.Get<ColorChosenEvent>();
                            chosenColor.R = byte.Parse(color.r.ToString());
                            chosenColor.G = byte.Parse(color.g.ToString());
                            chosenColor.B = byte.Parse(color.b.ToString());
                        }
                    }
                }
            }
        }
    }
}