using TMPro;
using UnityEngine;

namespace zone.nonon
{

    public class NamebarNetwork : MonoBehaviour
    {
        public TextMeshProUGUI nameBarText;
        public NononZoneObjectNetwork textSource;

        private void Update()
        {
            if (nameBarText != null && textSource != null)
            {
                if (!textSource.GetNononZoneObjectName().Equals(nameBarText.text))
                {
                    nameBarText.text = textSource.GetNononZoneObjectName();
                }
            }
            else
            {
                Debug.Log("If you want a namebar drag a NononZoneObject to the Text Source field of the namebar. The text will be taken from there.");
            }

        }
    }
}