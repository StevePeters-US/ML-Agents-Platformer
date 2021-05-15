using UnityEngine;

namespace APG
{
    // https://www.youtube.com/watch?v=6kWUGEQiMUI
    public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    T[] assets = Resources.LoadAll<T>(""); // Empty path loads from everywhere in project

                    if (assets == null || assets.Length < 1)
                        throw new System.Exception("Could not find singleton scriptable object instance in resources");
                    else if (assets.Length > 1)
                        Debug.LogWarning("Multiple instances of singleton scriptable object found in resources");

                    instance = assets[0];
                }
                return instance;
            }
        }
    }
}
