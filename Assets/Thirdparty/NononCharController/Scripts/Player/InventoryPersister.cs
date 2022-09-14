using Unity.Netcode;

namespace zone.nonon
{
    public class InventoryPersister : NetworkBehaviour, INononZonePersister
    {
        public void RestoreFromJSON(string serializedJSON)
        {
            throw new System.NotImplementedException();
        }

        public string Serialize2JSON()
        {
            throw new System.NotImplementedException();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}