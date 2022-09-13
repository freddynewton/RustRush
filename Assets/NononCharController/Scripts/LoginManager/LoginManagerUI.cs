using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace zone.nonon
{

    public class LoginManagerUI : MonoBehaviour
    {
        public TMP_InputField playerNameInput; // 0
        public TMP_InputField passwordInput; // 0
        public string startSceneClient;

        PlayerInput playerInput;

        int selectedField = 0;

        // Start is called before the first frame update
        void Start()
        {
            playerInput = new PlayerInput();
            SelectField();
            FillClientPlayerDataFromArguments();
            playerInput.CharacterControls.TabPressed.started += OnTabPressed;
            playerInput.CharacterControls.OKPressed.started += OnOKPressed;
            playerInput.Enable();

            // if the arguments come by command line, immediately start scene
            if (ClientPlayerData.Instance != null &&
                (ClientPlayerData.Instance.mode.Equals(NononZoneConstants.Starting.MODE_SERVER) ||
                ClientPlayerData.Instance.mode.Equals(NononZoneConstants.Starting.MODE_HOST) ||
                ClientPlayerData.Instance.mode.Equals(NononZoneConstants.Starting.MODE_SPECTATOR)))
            {
                ClientPlayerData.Instance.playerName = "ServerInstance";
                SceneManager.LoadScene(ClientPlayerData.Instance.loadingScene);
            }
        }

        private void OnDestroy()
        {
            playerInput.CharacterControls.TabPressed.started -= OnTabPressed;
            playerInput.CharacterControls.OKPressed.started -= OnOKPressed;
        }

        void FillClientPlayerDataFromArguments()
        {
            string mode = NononHelper.GetArg(NononZoneConstants.Starting.MODE_CMD_LINE_ARG);
            string loadingScene = NononHelper.GetArg(NononZoneConstants.Starting.SCENE_CMD_LINE_ARG);
            if (mode != null && ClientPlayerData.Instance != null)
            {
                ClientPlayerData.Instance.mode = mode;
            }
            if (loadingScene != null && ClientPlayerData.Instance != null)
            {
                ClientPlayerData.Instance.loadingScene = loadingScene;
            }
        }

        void OnTabPressed(InputAction.CallbackContext context)
        {
            selectedField++;
            if (selectedField > 1)
            {
                selectedField = 0;
            }
            SelectField();
        }

        void OnOKPressed(InputAction.CallbackContext context)
        {
            LoginButtonPressed();
        }

        public void LoginButtonPressed()
        {
            if (ClientPlayerData.Instance != null)
            {
                ClientPlayerData.Instance.playerName = playerNameInput.text;
                if (ClientPlayerData.Instance.loadingScene.Trim().Equals(""))
                {
                    ClientPlayerData.Instance.loadingScene = startSceneClient;
                }
                SceneManager.LoadScene(ClientPlayerData.Instance.loadingScene);

            }
        }

        void SelectField()
        {
            switch (selectedField)
            {
                case 0:
                    playerNameInput.Select();
                    break;
                case 1:
                    passwordInput.Select();
                    break;
            }
        }

        void OnLoginPressed()
        {

        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}