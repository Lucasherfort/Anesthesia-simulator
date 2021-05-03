#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SplineFollower), true)]
    public class SplineFollowerEditor : SplineUserEditor
    {
        private int trigger = -1;
        private bool triggerFoldout = false;
        private TransformModuleEditor motionEditor;
        private CustomRotationModuleEditor customRotationEditor;
        private CustomOffsetModuleEditor customOffsetEditor;

        void OnEnable()
        {
            SplineFollower user = (SplineFollower)target;
            motionEditor = new TransformModuleEditor(this, user.motion);
            customRotationEditor = new CustomRotationModuleEditor(this, user.customRotations);
            customOffsetEditor = new CustomOffsetModuleEditor(this, user.customOffsets);
        }



        public override void OnInspectorGUI()
        {
            BaseGUI();
            SplineFollower user = (SplineFollower)target;
            user.followMode = (SplineFollower.FollowMode)EditorGUILayout.EnumPopup("Follow mode", user.followMode);
            user.wrapMode = (SplineFollower.Wrap)EditorGUILayout.EnumPopup("Wrap mode", user.wrapMode);
            user.physicsMode = (SplineFollower.PhysicsMode)EditorGUILayout.EnumPopup("Physics mode", user.physicsMode);

            if(user.physicsMode == SplineFollower.PhysicsMode.Rigidbody)
            {
                Rigidbody rb = user.GetComponent<Rigidbody>();
                if (rb == null) EditorGUILayout.HelpBox("Assign a Rigidbody component.", MessageType.Error);
                else if(rb.interpolation == RigidbodyInterpolation.None && user.updateMethod != SplineUser.UpdateMethod.FixedUpdate) EditorGUILayout.HelpBox("Switch to FixedUpdate mode to ensure smooth update for non-interpolated rigidbodies", MessageType.Warning);
                
            } else if (user.physicsMode == SplineFollower.PhysicsMode.Rigidbody2D)
            {
                Rigidbody2D rb = user.GetComponent<Rigidbody2D>();
                if (rb == null) EditorGUILayout.HelpBox("Assign a Rigidbody2D component.", MessageType.Error);
                else if (rb.interpolation == RigidbodyInterpolation2D.None && user.updateMethod != SplineUser.UpdateMethod.FixedUpdate) EditorGUILayout.HelpBox("Switch to FixedUpdate mode to ensure smooth update for non-interpolated rigidbodies", MessageType.Warning);

            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Start position", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (!user.autoStartPosition) user.startPosition = EditorGUILayout.Slider((float)user.startPosition, (float)user.clipFrom, (float)user.clipTo);
            EditorGUIUtility.labelWidth = 55f;
            user.autoStartPosition = EditorGUILayout.Toggle("Auto", user.autoStartPosition, GUILayout.Width(80f));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            user.autoFollow = EditorGUILayout.Toggle("Auto follow", user.autoFollow);
            if (user.autoFollow)
            {
                if (user.followMode == SplineFollower.FollowMode.Uniform)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(20));
                    user.followSpeed = EditorGUILayout.FloatField("Follow speed", user.followSpeed);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(20));
                    user.followDuration = EditorGUILayout.FloatField("Follow duration", user.followDuration);
                    EditorGUILayout.EndHorizontal();
                }
            }
            user.direction = (Spline.Direction)EditorGUILayout.EnumPopup("Direction", user.direction);
            if(user.motion.applyRotation) user.applyDirectionRotation = EditorGUILayout.Toggle("Face Direciton", user.applyDirectionRotation);
            motionEditor.DrawInspector();
            customOffsetEditor.allowSelection = editIndex == -1;
            customOffsetEditor.DrawInspector();
            customRotationEditor.allowSelection = editIndex == -1;
            customRotationEditor.DrawInspector();
            triggerFoldout = EditorGUILayout.Foldout(triggerFoldout, "Triggers");
            if (triggerFoldout)
            {
                int lastTrigger = trigger;
                SplineEditorGUI.TriggerArray(ref user.triggers, ref trigger);
                if (lastTrigger != trigger) Repaint();
            }


            if (GUI.changed && !Application.isPlaying && user.computer != null)
            {
                if (!user.autoStartPosition)
                {
                    if (user.autoFollow)
                    {
                        ApplyFollow(user);
                    } else SceneView.RepaintAll();
                }
            }
        }

        void ApplyFollow(SplineFollower follower)
        {
            follower.motion.splineResult = follower.address.Evaluate(follower.startPosition);
            if (follower.applyDirectionRotation) follower.motion.direction = follower.direction;
            else follower.motion.direction = Spline.Direction.Forward;
            follower.motion.ApplyTransform(follower.transform);
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            SplineFollower user = (SplineFollower)target;

            if (triggerFoldout)
            {
                for (int i = 0; i < user.triggers.Length; i++)
                {
                    SplineEditorHandles.SplineSliderGizmo gizmo = SplineEditorHandles.SplineSliderGizmo.DualArrow;
                    switch (user.triggers[i].type)
                    {
                        case SplineTrigger.Type.Backward: gizmo = SplineEditorHandles.SplineSliderGizmo.BackwardTriangle; break;
                        case SplineTrigger.Type.Forward: gizmo = SplineEditorHandles.SplineSliderGizmo.ForwardTriangle; break;
                        case SplineTrigger.Type.Double: gizmo = SplineEditorHandles.SplineSliderGizmo.DualArrow; break;
                    }
                    double last = user.triggers[i].position;
                    if (SplineEditorHandles.Slider(user, ref user.triggers[i].position, user.triggers[i].color, user.triggers[i].name, gizmo) || last != user.triggers[i].position)
                    {
                        trigger = i;
                        Repaint();
                    }
                }
            }

            if (customOffsetEditor.isOpen)
            {
                if (customOffsetEditor.DrawScene(user)) ApplyFollow(user);
            }

            if (customRotationEditor.isOpen)
            {
                if (customRotationEditor.DrawScene(user)) ApplyFollow(user);
            }

            if (Application.isPlaying)
            {
                if (!user.autoFollow)
                {
                    Handles.color = SplineEditorGUI.selectionColor;
                    Handles.DrawLine(user.transform.position, user.followResult.position);
                    SplineEditorHandles.DrawSolidSphere(user.followResult.position, HandleUtility.GetHandleSize(user.followResult.position) * 0.2f);
                    Handles.color = Color.white;
                }
                return;
            }
            if (user.computer == null) return;
            if (user.autoStartPosition)
            {
                SplineResult result = user.address.Evaluate(user.address.Project(user.transform.position, 4, user.clipFrom, user.clipTo));
                Handles.DrawLine(user.transform.position, result.position);
                SplineEditorHandles.DrawSolidSphere(result.position, HandleUtility.GetHandleSize(result.position) * 0.2f);
            } else if(!user.autoFollow)
            {
                SplineResult result = user.address.Evaluate(user.startPosition);
                Handles.DrawLine(user.transform.position, result.position);
                SplineEditorHandles.DrawSolidSphere(result.position, HandleUtility.GetHandleSize(result.position) * 0.2f);
            }
        }
    }
}
#endif
