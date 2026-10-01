using System.Collections.Generic;
using UnityEngine;

namespace Parallax.LateCompatibility
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class SigmaDimensionsDataHolder : MonoBehaviour
    {
        public static Dictionary<string, int> defaultPresetMaxLevelDictionary = null;
        private void Awake()
        {
            //Make sure this is never destroyed, the data it holds must remain available across all scene switches.
            DontDestroyOnLoad(this);
        }
    }
}
