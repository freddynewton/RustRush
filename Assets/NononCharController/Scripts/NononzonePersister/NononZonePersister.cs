using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace zone.nonon
{
    public class NononZonePersister : NononSingleton<NononZonePersister>
    {
        [Serializable]
        public class PersisterClassData
        {
            [SerializeField]
            public string persistantObjectsData;
            [SerializeField]
            public string className;
        }

        [Serializable]
        public class PersisterTransformData
        {
            [SerializeField]
            public List<PersisterClassData> persistantClasses = new List<PersisterClassData>();
            [SerializeField]
            public string transformName;

            public PersisterClassData FindPersisterClass(string className)
            {
                foreach (PersisterClassData classData in persistantClasses)
                {
                    if (classData.className.Equals(className))
                    {
                        return classData;
                    }
                }
                return null;
            }
        }

        public bool playerPersistEnabled = false;

        void Awake()
        {
        }

        private void Start()
        {

        }

        private PersisterTransformData ReadFileFromJSON(string playerID)
        {
            string saveFile = Application.persistentDataPath + "/" + playerID + "_gamedata.json";
            // Does the file exist?
            if (File.Exists(saveFile))
            {
                // Read the entire file and save its contents.
                string fileContents = File.ReadAllText(saveFile);

                // Deserialize the JSON data 
                //  into a pattern matching the GameData class.
                PersisterTransformData persistant = JsonUtility.FromJson<PersisterTransformData>(fileContents);
                return persistant;
            }
            else
            {
                return null;
            }
        }

        private List<INononZonePersister> GetPersistantObjects(Transform parent)
        {
            List<INononZonePersister> persistants = new List<INononZonePersister>();
            Component[] components = parent.GetComponents<Component>();
            foreach (Component component in components)
            {
                var myCastedObject = component as INononZonePersister;

                if (myCastedObject != null)
                {
                    persistants.Add(component as INononZonePersister);
                }
            }
            return persistants;
        }

        public void WriteTransform2Json(Transform obj2Write, string playerID)
        {
            if (playerPersistEnabled)
            {
                PersisterTransformData transformData = new PersisterTransformData();
                transformData.transformName = obj2Write.name;
                List<INononZonePersister> nononZonePersistantObjects = GetPersistantObjects(obj2Write);
                foreach (INononZonePersister persistantObject in nononZonePersistantObjects)
                {
                    PersisterClassData classData = new PersisterClassData();
                    classData.className = persistantObject.GetType().ToString();
                    classData.persistantObjectsData = persistantObject.Serialize2JSON();
                    transformData.persistantClasses.Add(classData);
                }
                WriteFile(transformData, playerID);
            }
        }

        public void RestoreValuesOnTransformFromJson(Transform obj2Restore, string playerID)
        {
            if (playerPersistEnabled)
            {
                PersisterTransformData persisterTransformData = ReadFileFromJSON(playerID);
                if (persisterTransformData != null)
                {
                    List<INononZonePersister> nononZonePersistantObjects = GetPersistantObjects(obj2Restore);
                    foreach (INononZonePersister persistantObject in nononZonePersistantObjects)
                    {
                        PersisterClassData classData = persisterTransformData.FindPersisterClass(persistantObject.GetType().ToString());
                        if (classData != null)
                        {
                            persistantObject.RestoreFromJSON(classData.persistantObjectsData);
                        }
                    }
                }
            }

        }

        public void WriteFile(PersisterTransformData persistant, string playerID)
        {
            if (playerPersistEnabled)
            {
                // Serialize the object into JSON and save string.
                string jsonString = JsonUtility.ToJson(persistant);
                string saveFile = Application.persistentDataPath + "/" + playerID + "_gamedata.json";

                // Write JSON to file.
                File.WriteAllText(saveFile, jsonString);
            }
        }

    }

}

