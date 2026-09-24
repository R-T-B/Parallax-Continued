using HarmonyLib;
using Kopernicus;
using UnityEngine;


namespace Parallax.Harmony_Patches
{
    // PQS StartSphere() is called for every planet at the start of every scene. The planet that actually builds is called with force = true (and can be multiple times!)
    // We need a patch here to add an event that fires when the planet we are about to build is the planet we are actually on
    // This condition is met when force is true, and force is true on the dominant body

    // This avoids loading everything for every planet at the main menu, which defeats the purpose of on demand
    [HarmonyPatch(typeof(PQS))]
    [HarmonyPatch("UpdateQuadsInit")]
    public class PQSStartPatch
    {
        public static string currentLoadedBody = "ParallaxFirstRunDoNotCallAPlanetThis";

        public delegate void PQSStart(string bodyName);
        public delegate void PQSUnload(string bodyName);
        public delegate void PQSRestart(string bodyName);

        /// <summary>
        /// Called when the planet currently loading is just about to start building terrain. Use this for functions you need to run before the quads are built.
        /// NOT called if a non-parallax body was loading/unloading. NOT called if the body did not change, but the scene did (quicksave, quickload for ex)
        /// </summary>
        public static event PQSStart onPQSStart;
        public static event PQSUnload onPQSUnload;
        public static event PQSRestart onPQSRestart;
        static bool Prefix(PQS __instance)
        {
            Debug.Log("Update Quads Init: " + __instance.name);
            if (ConfigLoader.parallaxScatterBodies.ContainsKey(__instance.name))
            {
                Debug.Log(" - Invoking events for: " + __instance.name);
                if (currentLoadedBody == __instance.name)
                {
                    onPQSRestart?.Invoke(__instance.name);
                    return true;
                }
                onPQSUnload?.Invoke(currentLoadedBody);
                onPQSStart?.Invoke(__instance.name);
                currentLoadedBody = __instance.name;

                //Do rescale PQS scaling if SigDim is present
                UrlDir.UrlConfig sigDimConfig = ConfigLoader.GetConfigByName("SigmaDimensions");

                if (sigDimConfig == null)
                {
                    return true;
                }

                CelestialBody cb = FlightGlobals.GetBodyByName(__instance.name);
                float resizeValue = (float)cb.Get<double>("resize");
                float pqsRaiseAmountFloat = 0;
                if (resizeValue > 1)
                {
                    pqsRaiseAmountFloat = Mathf.Log(resizeValue, 2);
                }
                else
                {
                    float resizeValueInverted = (1 / resizeValue);
                    pqsRaiseAmountFloat = Mathf.Log(resizeValueInverted, 2) * (-1);
                }
                int pqsRaiseAmountInteger = (int)Mathf.Round(pqsRaiseAmountFloat);
                if (PQSCache.PresetList != null)
                {
                    foreach (PQSCache.PQSPreset rawPreset in PQSCache.PresetList.presets)
                    {
                        foreach (PQSCache.PQSSpherePreset preset in rawPreset.spherePresets)
                        {
                            if (preset.name.Contains(__instance.name))
                            {
                                preset.maxSubdivision += pqsRaiseAmountInteger;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
