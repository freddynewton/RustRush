using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace zone.nonon
{
    public class Typer : NetworkBehaviour
    {

        public string textKey = NononZoneConstants.I18N_CustomerInfo.CUSTOMER_INFO_TEXT_KEY;

        string msg = "Hello Dear Zone Visitor\n" +
                     "This is your CharacterController Component.Here is what you can do....\n\n" +
                    "Keyboard and Mouse\n\n" +
                    "WASD: Move ; Q/E: Strive Move; Space: Jump(the longer, the higher)\n" +
                    "Right Mouse Button: Rotate, Left Mouse Button:  Look Around\n" +
                    "Both Mouse Buttons together: Move, Look and Rotate\n" +
                    "Mouse Wheel: Zoom in and Out, TAB: Enable Thrusters\n" +
                    "Left Shift: Switch between Walk and Run\n" +
                    "Left Control: Draw Weapon\n" +
                    "1: Aim and Shoot\n\n" +
                    "Gamepad\n\n" +
                    "Left Stick: Move, D-Pad: Strive Move; A: Jump(the longer, the higher)\n" +
                    "Right Stick: Look Around\n" +
                    "D-Pad Down/Up: Zoom in and Out\n" +
                    "Left Top: Switch between Walk and Run, Right Top: Enable Thrusters\n" +
                    "Y: Draw Weapon; Right Trigger: Aim and Shoot\n\n" +
                    "Have Fun and thanks for all the Fish :);";

        Text textComp;
        float startDelay = 2f;
        float typeDelay = 0.05f;

        // Start is called before the first frame update
        void Start()
        {
        }

        private void Awake()
        {
            textComp = GetComponent<Text>();
            StartCoroutine(TypeIn());
        }

        public IEnumerator TypeIn()
        {
            yield return new WaitForSeconds(startDelay);

            for (int i = 0; i < msg.Length; i++)
            {
                textComp.text = msg.Substring(0, i);
                yield return new WaitForSeconds(typeDelay);
            }
        }

        public IEnumerator TypeOff()
        {
            for (int i = msg.Length; i > 0; i--)
            {
                textComp.text = msg.Substring(0, i);
                yield return new WaitForSeconds(typeDelay);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}