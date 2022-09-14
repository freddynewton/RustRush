using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace zone.nonon
{
    public class NononZoneControllerNetwork : NetworkBehaviour
    {
        public Text messageText;
        public Button retryButton;
        public Button ExitButton;

        public Transform playerRespawnPoint;

        private void Awake()
        {

        }

        // Start is called before the first frame update
        void Start()
        {
            GameEvents.Instance.onDying += OnDying;
            HideButtons();

        }

        public override void OnDestroy()
        {
            GameEvents.Instance.onDying -= OnDying;
        }

        // Update is called once per frame
        void Update()
        {

        }

        void ShowButtons()
        {
            retryButton.gameObject.SetActive(true);
            ExitButton.gameObject.SetActive(true);
        }

        void HideButtons()
        {
            retryButton.gameObject.SetActive(false);
            ExitButton.gameObject.SetActive(false);
        }

        public void ShowMessageText(string msg)
        {
            StartCoroutine(ShowMessageTextAndHide(msg));
        }

        IEnumerator ShowMessageTextAndHide(string msg)
        {
            messageText.text = msg;
            yield return new WaitForSeconds(3);
            messageText.text = "";
        }

        public void OnRetryBtnClicked()
        {
            HideButtons();
            messageText.text = "";
            if (playerRespawnPoint != null)
            {
                ResetPlayerPos();
            }

        }

        public void ResetPlayerPos()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag(NononZoneConstants.Player.PLAYER_TAG);
            foreach (GameObject player in players)
            {
                if (player.GetComponent<NetworkObject>().IsOwner)
                {
                    // if we find owner, the player can be resetted
                    player.GetComponent<PlayerControllerNetwork>().ResetPlayer(GetSpawnPosition(), GetSpawnRotation());
                    break;
                }
            }

        }

        Vector3 GetSpawnPosition()
        {
            Vector3 position = Vector3.zero;
            if (playerRespawnPoint != null)
            {
                position = playerRespawnPoint.position;
            }
            return position;
        }

        Quaternion GetSpawnRotation()
        {
            Quaternion rotation = Quaternion.identity;
            if (playerRespawnPoint != null)
            {
                rotation = playerRespawnPoint.rotation;
            }
            return rotation;
        }

        public void OnExitBtnClicked()
        {
            Application.Quit();
        }

        private void OnDying(Transform source)
        {
            if (NononZoneObjectNetwork.isOneOfTypes(source, INononZoneObject.NononZoneObjType.PLAYER))
            {
                    if (source.GetComponent<NetworkObject>().IsOwner)
                    {
                        messageText.text = "GAME OVER";
                        ShowButtons();
                    }
            }
        }
    }
}