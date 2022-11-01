#pragma warning disable 0649

using UnityEngine;
using Watermelon;

sealed public class Initialiser : MonoBehaviour
{
    [SerializeField, Tooltip("Objects with IInitialized interface")]
    private ScriptableObject[] m_InitObjects;

    void Awake()
    {
        for (int i = 0; i < m_InitObjects.Length; i++)
        {
            if (m_InitObjects[i] is IInitialized)
                (m_InitObjects[i] as IInitialized).Init();
        }

        //Do init

        Destroy(this);
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(Initialiser))]
    public class InitialiserEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Clear Prefs"))
            {
                PlayerPrefs.DeleteAll();
            }
        }
    }
#endif
}
