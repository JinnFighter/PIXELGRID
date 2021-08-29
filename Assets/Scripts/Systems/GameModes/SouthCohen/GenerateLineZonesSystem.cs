using Leopotam.Ecs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixelgrid
{
    public sealed class GenerateLineZonesSystem : IEcsRunSystem 
    {
        private readonly EcsFilter<RestartGameEvent> _restartEventFilter = null;
        private readonly EcsFilter<LineData, SouthCohenData, BorderComponent> _gameModeDataFilter = null;

        private CodeReceiver _codeReceiver;

        void IEcsRunSystem.Run() 
        {
            if(!_restartEventFilter.IsEmpty())
            {
                foreach(var index in _gameModeDataFilter)
                {
                    var border = _gameModeDataFilter.Get3(index);
                    var left = border.LeftCorner;
                    var right = border.RightCorner;
                    var lineData = _gameModeDataFilter.Get1(index);
                    ref var zonesData = ref _gameModeDataFilter.Get2(index);
                    var zones = new List<List<int>>();

                    for(var i = 0; i < lineData.LinePoints.Count; i++)
                        zones.Add(new List<int>());

                    for(var i = 0; i< lineData.LinePoints.Count; i++)
                    {
                        var start = lineData.LinePoints[i].First();
                        var end = lineData.LinePoints[i].Last();
                        var ax = start.x;
                        var ay = start.y;
                        var bx = end.x;
                        var by = end.y;
                        var code1 = _codeReceiver.GetCode(start, left, right);
                        var code2 = _codeReceiver.GetCode(end, left, right);
                        var inside = (code1 | code2) == 0;
                        var outside = (code1 & code2) != 0;
                        while (!inside && !outside)
                        {
                            if (code1 == 0)
                            {
                                Swap(ref ax, ref bx);
                                Swap(ref ay, ref by);
                                Swap(ref code1,  ref code2);
                            }

                            if (Convert.ToBoolean(code1 & 0x01))
                            {
                                ay += (left.x - ax) * (by - ay) / (bx - ax);
                                ax = left.x;
                            }

                            if (Convert.ToBoolean(code1 & 0x02))
                            {
                                ax += (left.y - ay) * (bx - ax) / (by - ay);
                                ay = left.y;
                            }

                            if (Convert.ToBoolean(code1 & 0x04))
                            {
                                ay += (right.x - ax) * (by - ay) / (bx - ax);
                                ax = right.x;
                            }

                            if (Convert.ToBoolean(code1 & 0x08))
                            {
                                ax += (right.y - ay) * (bx - ax) / (by - ay);
                                ay = right.y;
                            }

                            code1 = _codeReceiver.GetCode(new UnityEngine.Vector2Int(ax, ay), left, right);
                            inside = (code1 | code2) == 0;
                            outside = (code1 & code2) != 0;
                        }
                        if(!outside && ! zones[i].Contains(code1))
                            zones[i].Add(code1);
                    }

                    zonesData.Zones = zones;
                    var entity = _gameModeDataFilter.GetEntity(index);
                    ref var dataGeneratedEvent = ref entity.Get<GameModeDataGeneratedEvent>();
                    dataGeneratedEvent.DataCount = zonesData.Zones.Sum(zonePart => zonePart.Count);
                }
            }
        }

        private void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);
    }
}