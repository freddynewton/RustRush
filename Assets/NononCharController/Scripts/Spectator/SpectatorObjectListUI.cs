using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using static zone.nonon.SpectatorControllerNetwork;
using System;

namespace zone.nonon
{
    public class SpectatorObjectListUI : MonoBehaviour
    {
        [Header("References")]
        public SpectatorControllerNetwork controller;
        public TMP_InputField searchInputField;
        public Button clearButton;
        public Transform objectListEntryPrefab;
        public Transform objectListFillerPrefab;
        public Transform objectListContainer;
        public Toggle fixedHeightToggle;
        public Toggle ghostModeToggle;

        [Header("Search")]
        [Tooltip("Treshhold used to narrow search. The more to 100 the less tolerance.")]
        public double searchTreshhold = 49.0;

        // Start is called before the first frame update
        void Start()
        {
            AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
            AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });
            fixedHeightToggle.isOn = controller.IsFixedHeight();
            if (controller.IsFixedHeight())
            {
                ghostModeToggle.isOn = false;
                ghostModeToggle.interactable = false;
            }
            else
            {
                ghostModeToggle.isOn = controller.IsGhostMode();
            }

        }

        // Update is called once per frame
        void Update()
        {
            if (controller.spectatorListIsDirty)
            {
                ClearObjectList();
                List<SpectatorsObject> spectatorsObjects = controller.GetSpectatorObjectList();
                foreach (SpectatorsObject so in spectatorsObjects)
                {
                    Transform instantiatedObjectListEntry = Instantiate(objectListEntryPrefab, objectListContainer);
                    SpectatorObjectListEntry spectatorObjectListEntry = instantiatedObjectListEntry.GetComponent<SpectatorObjectListEntry>();
                    spectatorObjectListEntry.SetSpectatorObj(so, this);
                }
                Instantiate(objectListFillerPrefab, objectListContainer);
                HideShowSpectatorsBySearch();
                controller.spectatorListIsDirty = false;
            }
            if (controller.IsGhostMode() != ghostModeToggle.isOn) ghostModeToggle.isOn = controller.IsGhostMode();
            if (controller.IsFixedHeight() != fixedHeightToggle.isOn) fixedHeightToggle.isOn = controller.IsFixedHeight();
        }

        public void OnGhostModeToggleChanged()
        {
            controller.SetGhostMode(ghostModeToggle.isOn);
        }

        public void OnFixedHeightToggleChanged()
        {
            controller.SetFixedHeight(fixedHeightToggle.isOn);
            if (fixedHeightToggle.isOn)
            {
                controller.SetGhostMode(true);
                ghostModeToggle.isOn = true;
                ghostModeToggle.interactable = false;
            }
            else if (!ghostModeToggle.IsInteractable())
            {
                controller.SetGhostMode(false);
                ghostModeToggle.isOn = false;
                ghostModeToggle.interactable = true;
            }
        }


        public ulong GetCurrentSelectedNetworkID()
        {
            return controller.GetCurrentSelectedNetworkID();
        }

        public void AddRemoveAsFavSpectator(SpectatorsObject obj, bool add)
        {
            controller.AddRemoveAsFavSpectator(obj, add);
        }

        public void SetOrbDefault(SpectatorsObject obj, bool orbDefault)
        {
            controller.SetOrbDefault(obj, orbDefault);
        }

        public void AddRemoveAsNonAutoViewSpectator(SpectatorsObject obj, bool add)
        {
            controller.AddRemoveAsNonAutoViewSpectator(obj, add);
        }

        public void SetHomeNetworkID(ulong homeNetworkID)
        {
            controller.SetHomeNetworkID(homeNetworkID);
        }

        public void ShowSpecificSpectatorObject(SpectatorsObject so)
        {
            controller.ShowSpecificSpectatorObject(so);
        }

        public void AllFPVClicked()
        {
            for (int i = 0; i < objectListContainer.childCount; i++)
            {
                SpectatorObjectListEntry so = objectListContainer.GetChild(i).GetComponent<SpectatorObjectListEntry>();
                if (so != null) so.SetFPVDefaultToggle();
            }
        }

        public void AllORBClicked()
        {
            for (int i = 0; i < objectListContainer.childCount; i++)
            {
                SpectatorObjectListEntry so = objectListContainer.GetChild(i).GetComponent<SpectatorObjectListEntry>();
                if (so != null) so.SetORBDefaultToggle();
            }
        }

        public void AllFavoritesClicked()
        {
            SpectatorObjectListEntry firstEntry = objectListContainer.GetChild(0).GetComponent<SpectatorObjectListEntry>();
            bool firstEntrySelected = firstEntry.IsFavToglleOn();
            for (int i = 0; i < objectListContainer.childCount; i++)
            {
                SpectatorObjectListEntry so = objectListContainer.GetChild(i).GetComponent<SpectatorObjectListEntry>();
                if (so != null) so.SetFavToggle(!firstEntrySelected);
            }
        }

        void HideShowSpectatorsBySearch()
        {
            Transform[] allChildren = objectListContainer.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < allChildren.Length; i++)
            {
                Transform child = allChildren[i];
                if (searchInputField.text.Trim().Equals(""))
                {
                    child.gameObject.SetActive(true);
                }
                else
                {
                    SpectatorObjectListEntry so = child.GetComponent<SpectatorObjectListEntry>();
                    if (so != null && so.spectatorsObj != null)
                    {
                        //long score = String.CompareOrdinal(searchInputField.text.Trim(), so.spectatorsObj.objectName);
                        double score = CompareStrings(searchInputField.text.Trim(), so.spectatorsObj.objectName);
                        if (score > searchTreshhold)
                        {
                            child.gameObject.SetActive(true);
                        }
                        else
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        // Compares the two strings based on letter pair matches
        public double CompareStrings(string str1, string str2)
        {
            List<string> pairs1 = WordLetterPairs(str1.ToUpper());
            List<string> pairs2 = WordLetterPairs(str2.ToUpper());

            int intersection = 0;
            int union = pairs1.Count + pairs2.Count;

            for (int i = 0; i < pairs1.Count; i++)
            {
                for (int j = 0; j < pairs2.Count; j++)
                {
                    if (pairs1[i] == pairs2[j])
                    {
                        intersection++;
                        pairs2.RemoveAt(j);//Must remove the match to prevent "AAAA" from appearing to match "AA" with 100% success
                        break;
                    }
                }
            }

            return (2.0 * intersection * 100) / union; //returns in percentage
                                                       //return (2.0 * intersection) / union; //returns in score from 0 to 1
        }

        // Gets all letter pairs for each
        private List<string> WordLetterPairs(string str)
        {
            List<string> AllPairs = new List<string>();

            // Tokenize the string and put the tokens/words into an array
            string[] Words = System.Text.RegularExpressions.Regex.Split(str, @"\s");

            // For each word
            for (int w = 0; w < Words.Length; w++)
            {
                if (!string.IsNullOrEmpty(Words[w]))
                {
                    // Find the pairs of characters
                    String[] PairsInWord = LetterPairs(Words[w]);

                    for (int p = 0; p < PairsInWord.Length; p++)
                    {
                        AllPairs.Add(PairsInWord[p]);
                    }
                }
            }
            return AllPairs;
        }

        // Generates an array containing every two consecutive letters in the input string
        private string[] LetterPairs(string str)
        {
            int numPairs = str.Length - 1;
            string[] pairs = new string[numPairs];

            for (int i = 0; i < numPairs; i++)
            {
                pairs[i] = str.Substring(i, 2);
            }
            return pairs;
        }

        public void AllAutoViewClicked()
        {
            SpectatorObjectListEntry firstEntry = objectListContainer.GetChild(0).GetComponent<SpectatorObjectListEntry>();
            SpectatorObjectListEntry lastEntry = objectListContainer.GetChild(objectListContainer.childCount - 2).GetComponent<SpectatorObjectListEntry>();
            bool lastEntrySelected = lastEntry.IsAutoViewToggleOn();
            int startPoint = 0;
            // if all need to be deselected, select the first one
            if (lastEntrySelected)
            {
                firstEntry.SetAutoViewToggle(true);
                startPoint = 1;
            }
            for (int i = startPoint; i < objectListContainer.childCount; i++)
            {
                SpectatorObjectListEntry so = objectListContainer.GetChild(i).GetComponent<SpectatorObjectListEntry>();
                if (so != null) so.SetAutoViewToggle(!lastEntrySelected);
            }
        }

        void ClearObjectList()
        {
            List<Transform> childs2Destroy = new List<Transform>();
            for (int i = 0; i < objectListContainer.childCount; i++)
            {
                childs2Destroy.Add(objectListContainer.GetChild(i));
            }
            foreach (Transform item in childs2Destroy)
            {
                Destroy(item.gameObject);
            }

        }

        protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
        {
            EventTrigger trigger = obj.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = obj.AddComponent<EventTrigger>();
            }
            EventTrigger.Entry eventTrigger = new EventTrigger.Entry();
            eventTrigger.eventID = type;
            eventTrigger.callback.AddListener(action);
            trigger.triggers.Add(eventTrigger);
        }

        public void OnEnterInterface(GameObject obj)
        {
            controller.EnableDisableMovementInput(false);
        }

        public void OnExitInterface(GameObject obj)
        {
            controller.EnableDisableMovementInput(true);
        }

        public void OnSearchValueChanged()
        {
            SearchInputValueChanged();
        }

        public void OnSearchInputEndEdit()
        {
            clearButton.Select();
        }

        public void OnSearchInputSelected()
        {
            controller.EnableDisableHotKeys(false);
        }

        public void OnSearchInputDeselected()
        {
            controller.EnableDisableHotKeys(true);
        }

        public void OnClearInputPressed()
        {
            searchInputField.text = "";
            SearchInputValueChanged();
        }

        void SearchInputValueChanged()
        {
            HideShowSpectatorsBySearch();
        }
    }
}