using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static zone.nonon.SpectatorControllerNetwork;
using UnityEngine.UI;

namespace zone.nonon
{
    public class SpectatorObjectListEntry : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public Image homeBtnImage;
        public Toggle favToggle;
        public Toggle autoViewToggle;
        public Toggle fpvDefaultToggle;
        public Toggle orbDefaultToggle;

        SpectatorObjectListUI parentUI;

        Color32 ColorHomeBtnEnabled = new Color32(255, 255, 255, 255);
        Color32 ColorHomeBtnDisabled = new Color32(255, 255, 255, 60);

        Image entryPanelBackground;
        Color32 ColorCurrentlySelected = new Color32(112, 111, 211, 255);
        Color32 ColorCurrentlyDeSelected = new Color32(34, 112, 147, 255);

        [HideInInspector]
        public SpectatorsObject spectatorsObj;

        public void SetSpectatorObj(SpectatorsObject so, SpectatorObjectListUI _parentUI)
        {
            spectatorsObj = so;
            nameText.text = spectatorsObj.objectName;

            if (so.isHome) homeBtnImage.color = ColorHomeBtnEnabled;
            else homeBtnImage.color = ColorHomeBtnDisabled;

            favToggle.isOn = so.favorite;
            autoViewToggle.isOn = so.autoSelect;
            if (so.isOrbitViewDefault) orbDefaultToggle.isOn = true;
            else fpvDefaultToggle.isOn = true;

            parentUI = _parentUI;
        }

        public void OrbDefaultToggleChanged()
        {
            if (orbDefaultToggle != null && parentUI != null) parentUI.SetOrbDefault(spectatorsObj, orbDefaultToggle.isOn);
        }

        public void FavToggleValueChanged()
        {
            if (favToggle != null && spectatorsObj != null && favToggle.isOn != spectatorsObj.favorite && parentUI != null) parentUI.AddRemoveAsFavSpectator(spectatorsObj, favToggle.isOn);
        }

        public void NonAutoSwitchToggleChanged()
        {
            if (autoViewToggle != null && spectatorsObj != null && autoViewToggle.isOn != spectatorsObj.autoSelect && parentUI != null) parentUI.AddRemoveAsNonAutoViewSpectator(spectatorsObj, !autoViewToggle.isOn);
        }

        public void HomeButtonPressed()
        {
            if (spectatorsObj.isHome)
            {
                parentUI.SetHomeNetworkID(0);
                homeBtnImage.color = ColorHomeBtnDisabled;
            }
            else
            {
                parentUI.SetHomeNetworkID(spectatorsObj.networkObjectId);
                homeBtnImage.color = ColorHomeBtnEnabled;
            }
        }

        public void SetFavToggle(bool value)
        {
            favToggle.isOn = value;
        }

        public void SetAutoViewToggle(bool value)
        {
            autoViewToggle.isOn = value;
        }

        public void SetFPVDefaultToggle()
        {
            fpvDefaultToggle.isOn = true;
        }

        public void SetORBDefaultToggle()
        {
            orbDefaultToggle.isOn = true;
        }

        public bool IsFavToglleOn()
        {
            return favToggle.isOn;
        }

        public bool IsAutoViewToggleOn()
        {
            return autoViewToggle.isOn;
        }

        public void ShowObjectPressed()
        {
            if (spectatorsObj != null) parentUI.ShowSpecificSpectatorObject(spectatorsObj);
        }

        // Start is called before the first frame update
        void Start()
        {
            entryPanelBackground = GetComponent<Image>();
        }

        // Update is called once per frame
        void Update()
        {
            if (spectatorsObj != null && parentUI.GetCurrentSelectedNetworkID() == spectatorsObj.networkObjectId)
            {
                entryPanelBackground.color = ColorCurrentlySelected;
            }
            else
            {
                entryPanelBackground.color = ColorCurrentlyDeSelected;
            }
        }
    }
}