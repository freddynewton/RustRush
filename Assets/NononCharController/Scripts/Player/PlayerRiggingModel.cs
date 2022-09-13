using System;
using System.Collections.Generic;
using UnityEngine;

namespace zone.nonon
{
    [Serializable]
    public class PlayerRiggingModel : MonoBehaviour
    {
        [Serializable]
        public class RigModelEntry
        {

            [SerializeField]
            public PlayerRiggingModel.RigModelPart rigModelPart;

            [SerializeField]
            public string rigModelName;

            [SerializeField]
            public Transform rigModelTransform;

        }

        public enum RigModelPart { Armature, Spine, Head, UpperArmRight, HandRight, HandLeft, ElbowRight, ElbowLeft, ShoulderRight, ShoulderLeft }

        public List<RigModelEntry> rigModelEntries = new List<RigModelEntry>();

        public string GetNameFromEntries(RigModelPart partType)
        {
            foreach (RigModelEntry entry in rigModelEntries)
            {
                if (entry.rigModelPart.Equals(partType))
                {
                    return entry.rigModelName;
                }
            }
            Debug.LogError("You try to access " + partType + " but there is no part defined in the riggingModel on the Player");
            return null;
        }

        public bool PartNameExists(RigModelPart partType)
        {
            foreach (RigModelEntry entry in rigModelEntries)
            {
                if (entry.rigModelPart.Equals(partType))
                {
                    return true;
                }
            }
            return false;
        }


        /*public string armatureName = "Armature";
        public string spineName = "Spine";
        public string headName = "Head";
        public string upperArmRightName = "Upper Arm.R";
        public string handRightName = "Hand.R";
        public string handLeftName = "Hand.L";
        public string elbowRightName = "Lower Arm.R";
        public string elbowLeftName = "Lower Arm.L";
        public string shoulderRightName = "Upper Arm.R";
        public string shoulderLeftName = "Upper Arm.L";*/


    }
}