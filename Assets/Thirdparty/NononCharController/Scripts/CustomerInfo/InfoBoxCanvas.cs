using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace zone.nonon
{
    public class InfoBoxCanvas : NetworkBehaviour
    {
        public Text version;
        private void Awake()
        {
            Canvas canv = GetComponent<Canvas>();
            canv.worldCamera = Camera.main;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            version.text = "V" + Application.version;
        }
    }
}