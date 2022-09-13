using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace zone.nonon
{
    public class NononSingletonNetwork<T> : NetworkBehaviour
        where T : Component
    {

        private static string destroyedAtScene = "";
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    T[] objs = FindObjectsOfType(typeof(T)) as T[];
                    if (objs.Length > 0)
                    {
                        _instance = objs[0];
                    }
                    if (objs.Length > 1)
                    {
                        Debug.Log("There is more than one " + typeof(T).Name + " in the scene.");
                        foreach (T duplObj in objs)
                        {
                            Debug.Log(typeof(T).Name + " exists in " + duplObj.transform.name);
                        }
                    }
                    if (_instance == null)
                    {
                        if (!SceneManager.GetActiveScene().name.Equals(destroyedAtScene))
                        {
                            GameObject obj = new GameObject();
                            obj.name = string.Format("_{0}", typeof(T).Name);
                            _instance = obj.AddComponent<T>();
                        }
                    }
                }
                return _instance;
            }
        }

        public override void OnDestroy()
        {
            destroyedAtScene = SceneManager.GetActiveScene().name;
            base.OnDestroy();
        }

    }
}