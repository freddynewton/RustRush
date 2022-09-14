using TMPro;
using UnityEngine;

namespace zone.nonon
{

    public class NononLogger : NononSingleton<NononLogger>
    {
        [SerializeField]
        public TextMeshProUGUI loggingTextUI;

        public void LogDebug(string message)
        {
            if (loggingTextUI != null)
            {
                loggingTextUI.text += message + "\n";
            }
            Debug.Log(message);
        }

    }
}