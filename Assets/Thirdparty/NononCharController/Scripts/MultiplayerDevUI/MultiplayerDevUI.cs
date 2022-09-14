using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

namespace zone.nonon
{
    public class MultiplayerDevUI : NetworkBehaviour
    {
        public TMP_Dropdown modeDropDown;
        public TMP_Dropdown scene2LoadDropDown;
        public TMP_InputField playerNameInput;
        public TMP_InputField joinCodeInput;
        public Button startButton;

        void Start()
        {
            if (NononHelper.GetArg(NononZoneConstants.Starting.MODE_CMD_LINE_ARG) != null &&
                (NononHelper.GetArg(NononZoneConstants.Starting.MODE_CMD_LINE_ARG).Equals(NononZoneConstants.Starting.MODE_CLIENT) ||
                NononHelper.GetArg(NononZoneConstants.Starting.MODE_CMD_LINE_ARG).Equals(NononZoneConstants.Starting.MODE_SPECTATOR)))
            {
                transform.GetComponent<Canvas>().enabled = false;

            }
            else
            {

                transform.GetComponent<Canvas>().enabled = true;

                FillScenesInBuild();
                FillModes();
                playerNameInput.text = "NOCAZONE";

                if (ClientPlayerData.Instance != null && ClientPlayerData.Instance.mode != null && !ClientPlayerData.Instance.mode.Trim().Equals(""))
                {
                    modeDropDown.GetComponent<Image>().color = Color.gray;
                    modeDropDown.enabled = false;
                    if (ClientPlayerData.Instance.mode.Equals(NononZoneConstants.Starting.MODE_SERVER)) modeDropDown.value = 0;
                    if (ClientPlayerData.Instance.mode.Equals(NononZoneConstants.Starting.MODE_CLIENT)) modeDropDown.value = 1;
                    if (ClientPlayerData.Instance.mode.Equals(NononZoneConstants.Starting.MODE_HOST)) modeDropDown.value = 2;
                    if (ClientPlayerData.Instance.mode.Equals(NononZoneConstants.Starting.MODE_SPECTATOR)) modeDropDown.value = 3;

                    scene2LoadDropDown.GetComponent<Image>().color = Color.gray;
                    scene2LoadDropDown.enabled = false;
                    int i = 0;
                    foreach (TMP_Dropdown.OptionData option in scene2LoadDropDown.options)
                    {
                        if (option.text.Equals(ClientPlayerData.Instance.loadingScene))
                        {
                            scene2LoadDropDown.value = i;
                            break;
                        }
                        i++;
                    }

                    playerNameInput.GetComponent<Image>().color = Color.gray;
                    playerNameInput.enabled = false;
                    playerNameInput.text = ClientPlayerData.Instance.playerName;
                    joinCodeInput.GetComponent<Image>().color = Color.gray;
                    joinCodeInput.enabled = false;
                    joinCodeInput.text = ClientPlayerData.Instance.joinCode;
                    startButton.GetComponent<Image>().color = Color.gray;
                    startButton.enabled = false;

                }
            }
        }

        void FillModes()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(NononZoneConstants.Starting.MODE_SERVER);
            options.Add(data);
            data = new TMP_Dropdown.OptionData(NononZoneConstants.Starting.MODE_CLIENT);
            options.Add(data);
            data = new TMP_Dropdown.OptionData(NononZoneConstants.Starting.MODE_HOST);
            options.Add(data);
            data = new TMP_Dropdown.OptionData(NononZoneConstants.Starting.MODE_SPECTATOR);
            options.Add(data);
            modeDropDown.AddOptions(options);
        }

        void FillScenesInBuild()
        {

            // Get build scenes
            var sceneNumber = SceneManager.sceneCountInBuildSettings;
            string[] arrayOfNames;
            arrayOfNames = new string[sceneNumber];
            for (int i = 0; i < sceneNumber; i++)
            {
                arrayOfNames[i] = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            }

            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (string sceneName in arrayOfNames)
            {
                TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(sceneName);
                options.Add(data);
            }
            scene2LoadDropDown.AddOptions(options);
        }

        public void StartButtonPressed()
        {
            if (NetworkManager.Singleton != null)
            {
                Destroy(NetworkManager.Singleton.gameObject);
            }
            if (ClientPlayerData.Instance != null)
            {
                ClientPlayerData.Instance.playerName = playerNameInput.text;
                ClientPlayerData.Instance.mode = modeDropDown.options[modeDropDown.value].text;
                ClientPlayerData.Instance.loadingScene = scene2LoadDropDown.options[scene2LoadDropDown.value].text;
                ClientPlayerData.Instance.joinCode = joinCodeInput.text;
            }
            SceneManager.LoadScene(ClientPlayerData.Instance.loadingScene);

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}