using System.Collections.Generic;
using UnityEngine;

namespace Core.Settings
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Settings/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] 
        private int _gridWidth;
        [SerializeField] 
        private int _gridHeight;
        [SerializeField] 
        private List<Color> _tileColors;

        public int GridWidth => _gridWidth;
        public int GridHeight => _gridHeight;
        public List<Color> TileColors => _tileColors;
    }
}
