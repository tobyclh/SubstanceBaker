using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace SubstanceBaker
{
    public class BakerWindow : EditorWindow
    { 
        public BakerProfile _profile;
        [MenuItem("Window/SubstanceBaker/Profiles")]
        public static void Init()
        {
            EditorWindow.GetWindow(typeof(BakerWindow)).Show();
        }
        void OnGUI()
        {
            GUILayout.Label("Profile", EditorStyles.boldLabel);
            EditorGUILayout.PrefixLabel("Profile to use:");
            _profile = EditorGUILayout.ObjectField(_profile, typeof(BakerProfile), false) as BakerProfile;
            if (_profile != null)
            {
                Editor editor = Editor.CreateEditor(_profile);
                editor.DrawDefaultInspector();
            }
            if (GUILayout.Button("Create new profile"))
            {
                var profile = ScriptableObject.CreateInstance("BakerProfile") as BakerProfile;
                string pathToProfile;
                try
                {
                    pathToProfile = "Assets/SubstanceBaker/Profiles/New BakerProfile.asset";
                    AssetDatabase.CreateAsset(profile, pathToProfile);
                }
                catch
                {
                    pathToProfile = "Assets/New BakerProfile.asset";
                    AssetDatabase.CreateAsset(profile, pathToProfile);
                }

                if (_profile == null)
                {
                    _profile = profile;
                }

            }

            if (GUILayout.Button("Apply settings to selected materials"))
            {
                
            }

            if (GUILayout.Button("Start Baking selected materials"))
            {

            }
        }

    }
}