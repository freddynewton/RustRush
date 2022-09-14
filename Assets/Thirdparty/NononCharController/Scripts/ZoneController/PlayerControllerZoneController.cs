using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace zone.nonon
{

    public class PlayerControllerZoneController : MonoBehaviour
    {
        public Text bestTimeValue;
        public Text youTimeValue;
        public Text pointsValue;
        public Text quest1Text;
        public Text quest2Text;
        public Text quest3Text;
        public Text quest4Text;

        public Transform meImage;
        bool weaponQDone = false;

        public Transform portalStart;
        public Transform portalFirstPlattform;

        public Transform enemyPrefab;
        public Transform enemySpawnPoints;

        GameObject[] spawnedEnemies;

        public NononZoneController zoneController;

        public Transform coinBurstEffectPrefab;
        ParticleSystem coinBurstEffect;

        float currentTime = 0f;
        float bestTime = 0f;
        bool timerStarted = false;
        int points = 0;
        float enemyChasingSpeed = 0;
        float enemyChasingAggroSpeed = 0;
        float enemyRaycastDistance = 0;
        bool successHasBeenShown = false;

        string NONONZONECONTROLLER_STRING_TABLE = NononZoneConstants.I18N_NononZoneController.TABLE_NONONZONE_CONTROLLER;

        private void Awake()
        {
            coinBurstEffect = Instantiate(coinBurstEffectPrefab).gameObject.GetComponent<ParticleSystem>();
        }


        // Start is called before the first frame update
        void Start()
        {
            spawnedEnemies = new GameObject[enemySpawnPoints.childCount];
            Enemy enemy = enemyPrefab.GetComponent<Enemy>();
            enemyChasingAggroSpeed = enemy.chasingAggroedSpeed;
            enemyChasingSpeed = enemy.chasingSpeed;
            enemyRaycastDistance = enemy.raycastDistance;

            GameEvents.Instance.onDying += OnDying;
            GameEvents.Instance.onMounted += PlayerMounted;
            GameEvents.Instance.onGotLoot += OnGotLoot;
            GameEvents.Instance.onCollisionEntered += OnCollisionEntered;

        }

        private void OnDestroy()
        {
            GameEvents.Instance.onDying -= OnDying;
            GameEvents.Instance.onMounted -= PlayerMounted;
            GameEvents.Instance.onGotLoot -= OnGotLoot;

            GameEvents.Instance.onCollisionEntered -= OnCollisionEntered;
        }

        private void OnCollisionEntered(Transform source, Transform origin)
        {
            if (NononZoneObject.isOneOfTypes(source, INononZoneObject.NononZoneObjType.PLAYER) && NononZoneObject.isOneOfTypes(origin, INononZoneObject.NononZoneObjType.PLATTFORM))
            {
                PlattformMover mover = origin.GetComponent<PlattformMover>();

                if (mover.startingPlattform)
                {
                    StartTimer();
                }
                if (mover.finishPlattform)
                {
                    StopTimer();
                }
            }
        }

        public void PlayerMounted(Transform source)
        {
            if (NononZoneObject.isOneOfTypes(source, INononZoneObject.NononZoneObjType.PLAYER))
            {
                currentTime = 0f;
                timerStarted = false;
            }
        }

        void SpawnEnemies()
        {
            int multiplier = points / 5;
            for (int i = 0; i < enemySpawnPoints.childCount; i++)
            {
                if (spawnedEnemies[i] == null)
                {
                    for (int j = 0; j <= multiplier; j++)
                    {
                        spawnedEnemies[i] = Instantiate(enemyPrefab, enemySpawnPoints.GetChild(i).position, enemySpawnPoints.GetChild(i).rotation).gameObject;
                        Enemy enemy = spawnedEnemies[i].GetComponent<Enemy>();
                        enemy.chasingAggroedSpeed = enemyChasingAggroSpeed;
                        enemy.chasingSpeed = enemyChasingSpeed;
                        enemy.raycastDistance = enemyRaycastDistance;
                    }
                }
            }
        }

        public void OnGotLoot(Transform source, Transform loot)
        {

            if (NononZoneObject.isOneOfTypes(source, INononZoneObject.NononZoneObjType.PLAYER))
            {
                if (NononZoneObject.isOneOfTypes(loot, INononZoneObject.NononZoneObjType.WEAPON))
                {
                    if (!weaponQDone)
                    {
                        weaponQDone = true;
                        zoneController.ShowMessageText("You got a weapon.Equip and shoot em all!");
                        quest2Text.color = Color.green;
                        CheckQuests();
                    }
                }
                if (NononZoneObject.isOneOfTypes(loot, INononZoneObject.NononZoneObjType.MOUNT))
                {
                    zoneController.ShowMessageText("You got a Thruster. You can Fly now!");
                    quest4Text.color = Color.green;
                    CheckQuests();
                }
            }
        }

        private void OnDying(Transform source)
        {
            NononZoneObject nononObject = source.GetComponent<NononZoneObject>();
            if (nononObject != null)
            {
                coinBurstEffect.transform.position = source.position;
                ParticleSystem.EmissionModule em = coinBurstEffect.emission;
                ParticleSystem.Burst burst = new ParticleSystem.Burst(0f, nononObject.GetBounty());
                em.SetBurst(0, burst);
                coinBurstEffect.Play();
                Debug.Log("Bounty: " + nononObject.GetBounty());
            }


            if (NononZoneObject.isOneOfTypes(source, INononZoneObject.NononZoneObjType.ENEMY))
            {
                points++;
                if (points >= 50)
                {
                    quest3Text.color = Color.green;
                    CheckQuests();
                }
                if (points % 5 == 0 && points != 50)
                {
                    zoneController.ShowMessageText("You get more and faster enemies!");
                    enemyChasingAggroSpeed++;
                    enemyChasingSpeed++;
                    enemyRaycastDistance++;
                }
            }

        }

        void OnGUI()
        {
            bestTimeValue.text = NiceTime(bestTime);
            youTimeValue.text = NiceTime(currentTime);
            pointsValue.text = points.ToString();
        }

        // Update is called once per frame
        void Update()
        {
            SpawnEnemies();

            if (timerStarted)
            {
                currentTime += Time.deltaTime;
            }

            if (currentTime > 120)
            {
                timerStarted = false;
            }

        }

        private IEnumerator ShowAndHideImage()
        {
            meImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(5);
            meImage.gameObject.SetActive(false);
        }

        void CheckQuests()
        {
            if (quest1Text.color.Equals(Color.green) && quest2Text.color.Equals(Color.green) && quest3Text.color.Equals(Color.green) && quest4Text.color.Equals(Color.green) && !successHasBeenShown)
            {
                StartCoroutine(ShowAndHideImage());
                zoneController.ShowMessageText("You're a Monster :) Well Done!");
            }
        }


        public void StartTimer()
        {
            currentTime = 0f;
            timerStarted = true;
        }

        public void StopTimer()
        {
            if (timerStarted)
            {
                timerStarted = false;
                if (bestTime == 0 || currentTime < bestTime)
                {
                    bestTime = currentTime;
                    if (bestTime < 14)
                    {
                        quest1Text.color = Color.green;
                        CheckQuests();
                        portalFirstPlattform.gameObject.SetActive(true);
                        portalStart.gameObject.SetActive(true);
                    }
                }
            }
        }


        float GetMinutes(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60.0f);
            return minutes;
        }

        float GetSeconds(float time)
        {
            int seconds = Mathf.FloorToInt(time - GetMinutes(time) * 60);
            return seconds;
        }

        string NiceTime(float time)
        {
            string niceTime = string.Format("{0:00}:{1:00}", GetMinutes(time), GetSeconds(time));
            return niceTime;
        }

    }
}