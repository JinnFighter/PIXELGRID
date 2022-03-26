using UnityEngine;

namespace Pixelgrid.Configurations.Script
{
    [CreateAssetMenu(fileName = "GameFieldConfigs", menuName = "Configs/GameFieldConfigs")]
    public class GameFieldConfigs : ScriptableObject
    {
        [field: SerializeField] public GameFieldConfig[] Configs { get; private set; }
    }
}
