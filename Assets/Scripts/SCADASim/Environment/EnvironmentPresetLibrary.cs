using System;
using System.Collections.Generic;
using SCADASim.Core;
using UnityEngine;

namespace SCADASim.Environment
{
    [Serializable]
    public sealed class EnvironmentPreset
    {
        public EnvironmentType Type;
        public GameObject Prefab;
        public Vector3 SpawnPosition;
        public Vector3 SpawnEulerAngles;
    }

    [CreateAssetMenu(menuName = "SCADA Simulation/Environment/Preset Library")]
    public sealed class EnvironmentPresetLibrary : ScriptableObject
    {
        [SerializeField] private List<EnvironmentPreset> presets = new List<EnvironmentPreset>();

        public bool TryGetPreset(EnvironmentType type, out EnvironmentPreset preset)
        {
            for (int i = 0; i < presets.Count; i++)
            {
                if (presets[i].Type == type)
                {
                    preset = presets[i];
                    return true;
                }
            }

            preset = null;
            return false;
        }
    }
}
