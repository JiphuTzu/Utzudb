using UnityEngine;
using UnityEditor;
//============================================================
//@author	JiphuTzu
//@create	2/9/2017
//@company	STHX
//
//@description:
//============================================================
namespace Hexice.Components
{
    [CustomEditor(typeof(Prism))]
    public class PrismEditor : Editor
    {
        private Prism _prism;
        private void OnEnable()
        {
            _prism = target as Prism;
            _prism.Draw();
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUI.changed)
            {
                _prism.Draw();
            }

        }
        [MenuItem("GameObject/3D Object/Prism")]
        private static void Create()
        {
            Selection.activeObject = Prism.Create().gameObject;
        }
    }
}