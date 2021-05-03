#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SplinePositioner), true)]
    public class SplinePositionerEditor : SplineUserEditor
    {
        private TransformModuleEditor motionEditor;
        private CustomRotationModuleEditor customRotationEditor;
        private CustomOffsetModuleEditor customOffsetEditor;

        void OnEnable()
        {
            SplinePositioner user = (SplinePositioner)target;
            motionEditor = new TransformModuleEditor(this, user.motion);
            customRotationEditor = new CustomRotationModuleEditor(this, user.customRotations);
            customOffsetEditor = new CustomOffsetModuleEditor(this, user.customOffsets);
        } 

        public override void BaseGUI()
        {
            base.BaseGUI();
            SplinePositioner positioner = (SplinePositioner)target;
            positioner.mode = (SplinePositioner.Mode)EditorGUILayout.EnumPopup("Mode", positioner.mode);
            if(positioner.mode == SplinePositioner.Mode.Distance) positioner.position = EditorGUILayout.FloatField("Distance", (float)positioner.position);
            else positioner.position = EditorGUILayout.Slider("Percent", (float)positioner.position, 0f, 1f);
            positioner.applyTransform = (Transform)EditorGUILayout.ObjectField("Apply transform", positioner.applyTransform, typeof(Transform), true);
            if (positioner.motion.applyRotation) positioner.direction = (Spline.Direction)EditorGUILayout.EnumPopup("Face direction", positioner.direction);
            EditorGUI.BeginChangeCheck();
            motionEditor.DrawInspector();
            customOffsetEditor.allowSelection = editIndex == -1;
            customOffsetEditor.DrawInspector();
            customRotationEditor.allowSelection = editIndex == -1;
            customRotationEditor.DrawInspector();
            if (EditorGUI.EndChangeCheck()) positioner.Rebuild(false);
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            SplinePositioner user = (SplinePositioner)target;
            if (customOffsetEditor.isOpen)
            {
                if (customOffsetEditor.DrawScene(user)) user.Rebuild(false);
            }
            if (customRotationEditor.isOpen)
            {
                if (customRotationEditor.DrawScene(user)) user.Rebuild(false);
            }
        }

        public override void OnInspectorGUI()
        {
            BaseGUI();
        }
    }
}
#endif
