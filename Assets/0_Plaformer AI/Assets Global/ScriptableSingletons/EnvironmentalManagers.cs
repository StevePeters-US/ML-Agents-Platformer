using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG.Environment {
    [CreateAssetMenu(fileName = "Environmental Managers", menuName = "Scriptable Objects/Environmental Managers")]
    public class EnvironmentalManagers : SingletonScriptableObject<EnvironmentalManagers> {
        public List<EnvironmentManager> Items = new List<EnvironmentManager>();

        public void Add(EnvironmentManager manager) {
            if (!Items.Contains(manager))
                Items.Add(manager);
        }

        public void Remove(EnvironmentManager manager) {
            if (Items.Contains(manager))
                Items.Remove(manager);
        }
    }
}
