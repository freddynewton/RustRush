using UnityEngine;
using UnityEngine.UI;

namespace zone.nonon
{
    public class FloatingText : MonoBehaviour
    {
        public Animator animator;
        private Text damageText;

        // Start is called before the first frame update
        void Start()
        {
            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
            Destroy(gameObject, clipInfos[0].clip.length);

        }

        public void SetText(string text)
        {
            damageText = animator.transform.GetComponent<Text>();
            damageText.text = text;
        }

        public void SetTextColor(Color col)
        {
            damageText = animator.transform.GetComponent<Text>();
            damageText.color = col;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}