using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace zone.nonon
{
    public class HealthBarScript : MonoBehaviour
    {

        public Image foregroundRed;
        public Image foregroundGreen;
        private float updateSpeedSeconds = 0.5f;
        float currentFillAmount = 1.0f;
        NononZoneObject parentObject;
        int lastHealth = 0;

        void Awake()
        {
        }

        void Start()
        {
            parentObject = GetComponentInParent<NononZoneObject>();
            lastHealth = parentObject.GetCurrentHealth();
            float newValue = (1.0f / parentObject.max_health) * parentObject.GetCurrentHealth();
            foregroundGreen.fillAmount = newValue;
            foregroundRed.fillAmount = newValue;
            currentFillAmount = newValue;

        }

        public void Reset()
        {
            currentFillAmount = 1.0f;
            foregroundGreen.fillAmount = 1.0f;
            foregroundRed.fillAmount = 1.0f;
            lastHealth = parentObject.max_health;
            GetComponent<Canvas>().enabled = true;
        }

        public void TakeDmg(int dmg, int maxHealth)
        {
            float pctChange = (1.0f / maxHealth) * dmg;
            StartCoroutine(ChangeToPct(pctChange));
        }

        public void GainHealth(int health, int maxHealth)
        {
            float pctChange = (1.0f / maxHealth) * -health;
            StartCoroutine(ChangeToPct(pctChange));
        }

        IEnumerator ChangeToPct(float pctChange)
        {
            float preChangePct = currentFillAmount;
            float newValue = preChangePct - pctChange;
            float elapsed = 0f;
            foregroundGreen.fillAmount = newValue;
            currentFillAmount = newValue;

            // cap values
            if (newValue > 1) newValue = 1;
            if (newValue < 0) newValue = 0;

            while (elapsed < updateSpeedSeconds)
            {
                elapsed += Time.deltaTime;
                foregroundRed.fillAmount = Mathf.Lerp(preChangePct, newValue, elapsed / updateSpeedSeconds);
                yield return null;
            }

            foregroundRed.fillAmount = newValue;
        }


        // Update is called once per frame
        void Update()
        {
            if (currentFillAmount <= 0)
            {
                GetComponent<Canvas>().enabled = false;
            }
            if (lastHealth < parentObject.GetCurrentHealth())
            {
                GainHealth(parentObject.GetCurrentHealth() - lastHealth, parentObject.max_health);
                lastHealth = parentObject.GetCurrentHealth();
            }
            else if (lastHealth > parentObject.GetCurrentHealth())
            {
                TakeDmg(lastHealth - parentObject.GetCurrentHealth(), parentObject.max_health);
                lastHealth = parentObject.GetCurrentHealth();
            }
        }
    }
}