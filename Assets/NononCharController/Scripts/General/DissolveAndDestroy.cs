using UnityEngine;

namespace zone.nonon
{
    public class DissolveAndDestroy : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            GameEvents.Instance.onDying += OnDying;
        }

        private void OnDestroy()
        {
            GameEvents.Instance.onDying -= OnDying;
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDying(Transform source)
        {
            if (source.transform.Equals(transform))
            {
                DissolveEffect dissolveEffect = GetComponent<DissolveEffect>();
                if (dissolveEffect != null)
                {
                    dissolveEffect.StartDissolvingAndDestroy(source);
                }
            }
            
        }
    }
}