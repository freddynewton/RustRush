using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace zone.nonon
{
    public class DmgHealthTextControllerNetwork : NetworkBehaviour
    {
        public FloatingText floatingTextPrefab;
        public Transform textSpawnPosition;

        // Start is called before the first frame update
        void Start()
        {
            RegisterGameEvents();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public override void OnDestroy()
        {
            UnRegisterGameEvents();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        public void RegisterGameEvents()
        {
            GameEvents.Instance.onTakeDmg += OnTakeDmg;
            GameEvents.Instance.onGainHealth += OnHealthGain;
        }

        public void UnRegisterGameEvents()
        {
            GameEvents.Instance.onTakeDmg -= OnTakeDmg;
            GameEvents.Instance.onGainHealth -= OnHealthGain;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RegisterGameEvents();
        }

        private void OnSceneUnloaded(Scene current)
        {
            UnRegisterGameEvents();
        }


        // Update is called once per frame 
        void Update()
        {

        }



        public void OnTakeDmg(Transform target, int dmg, int maxHealth)
        {
            if (target.transform.Equals(transform))
            {
                CreateInstance(target, dmg, Color.red);
            }
        }

        public void OnHealthGain(Transform target, int dmg, int maxHealth)
        {
            if (target.transform.Equals(transform))
            {
                CreateInstance(target, dmg, Color.green);
            }
        }

        private void CreateInstance(Transform target, int dmg, Color col)
        {
            if (IsClient)
            {
                CreateInstanceForClient(target, dmg, col);
            }
        }

        private void CreateInstanceForClient(Transform target, int dmg, Color col)
        {
            float distance = Vector3.Distance(target.position, Camera.main.transform.position);
            FloatingText instance = Instantiate(floatingTextPrefab);
            instance.transform.localScale *= 0.2f * distance;
            instance.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            instance.SetTextColor(col);
            instance.transform.SetParent(transform, false);
            float randomX = Random.Range(-0.2f, 0.2f);
            float randomY = Random.Range(-0.2f, 0.2f);
            instance.transform.localPosition = new Vector3(textSpawnPosition.localPosition.x + randomX, textSpawnPosition.localPosition.y + randomY, textSpawnPosition.localPosition.z);

            instance.SetText(dmg.ToString());
        }
    }
}