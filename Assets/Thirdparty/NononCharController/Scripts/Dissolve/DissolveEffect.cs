using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zone.nonon
{
    public class DissolveEffect : MonoBehaviour
    {

        public Material dissolveMat;
        class RendererAndShader
        {
            public Renderer renderer;
            //public Shader[] shaders;
            public Material[] materials;
        }

        Shader dissolveShader;
        List<RendererAndShader> modifiedRenderers = new List<RendererAndShader>();

        private void Awake()
        {
            dissolveShader = Shader.Find("Shader Graphs/Dissolve");
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void StartDissolvingAndDestroy(Transform model)
        {
            modifiedRenderers.Clear();
            BackupMaterialsOnShader(model);
            StartCoroutine(DissolveAndDestroy(model));
        }

        public void StartAppear(Transform model)
        {
            modifiedRenderers.Clear();
            BackupMaterialsOnShader(model);
            StartCoroutine(Appear(model));
        }

        public void StopAppear(Transform model)
        {
            StopCoroutine(Appear(model));
            ResetShaders();
        }

        IEnumerator DissolveAndDestroy(Transform modelRoot)
        {
            float dissolveValue = -1.0f;
            SetNewShaders(modelRoot);
            while (dissolveValue < 1)
            {

                UpdateDissolveValue(dissolveValue);

                dissolveValue += Time.deltaTime;
                yield return null;
            }
            // as we destroy the object, we do not reset the shaders
            DestroyImmediate(gameObject);
        }

        IEnumerator Appear(Transform modelRoot)
        {
            float dissolveValue = 1.0f;
            SetNewShaders(modelRoot);
            while (dissolveValue > -1)
            {
                UpdateDissolveValue(dissolveValue);

                dissolveValue -= Time.deltaTime;
                yield return null;
            }
            // reset all shaders
            ResetShaders();
        }

        void BackupMaterialsOnShader(Transform root)
        {
            // set it on the root
            Renderer renderer = root.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material[] oldMats = new Material[renderer.materials.Length];

                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    oldMats[i] = renderer.materials[i];
                }

                RendererAndShader ras = new RendererAndShader();
                ras.renderer = renderer;
                ras.materials = oldMats;
                modifiedRenderers.Add(ras);
            }
            // do it for each child
            for (int j = 0; j < root.childCount; j++)
            {
                BackupMaterialsOnShader(root.GetChild(j));
            }
        }

        void SetNewShaders(Transform root)
        {


            foreach (RendererAndShader ras in modifiedRenderers)
            {
                Material[] newMats = new Material[ras.renderer.materials.Length];

                for (int i = 0; i < ras.renderer.materials.Length; i++)
                {
                    newMats[i] = dissolveMat;
                }
                ras.renderer.materials = newMats;

            }
        }

        void UpdateDissolveValue(float dissolveValue)
        {
            foreach (RendererAndShader ras in modifiedRenderers)
            {
                if (ras.renderer != null)
                {
                    for (int i = 0; i < ras.renderer.materials.Length; i++)
                    {
                        ras.renderer.materials[i].SetFloat("_Effect_Value", dissolveValue);
                    }
                }
            }
        }

        void ResetShaders()
        {
            foreach (RendererAndShader ras in modifiedRenderers)
            {
                if (ras.renderer != null) ras.renderer.materials = ras.materials;
            }
        }
    }
}