using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace zone.nonon
{
    public class SliderMenuAnim : MonoBehaviour
    {
        public GameObject panelMenu;
        public TMP_Text showHideBtnText;
        bool isOpen = false;
        Animator animator;

        private void Start()
        {
            animator = panelMenu.GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (animator != null)
            {
                animator.SetBool("show", isOpen);
            }
        }

        public void ShowHideMenu()
        {
            isOpen = !isOpen;
            animator.SetBool("show", isOpen);
            if (!isOpen) showHideBtnText.text = "SHOW";
            else showHideBtnText.text = "HIDE";
        }
    }
}