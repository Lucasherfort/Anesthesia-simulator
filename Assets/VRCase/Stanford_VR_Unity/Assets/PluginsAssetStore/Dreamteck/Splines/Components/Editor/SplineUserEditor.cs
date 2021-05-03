using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SplineUser), true)]
    public class SplineUserEditor : Editor
    {
        protected bool showResolution = true;
        protected bool showClip = true;
        protected bool showAveraging = true;
        protected bool showUpdateMethod = true;
        protected bool showMultithreading = true;
        private PathWindow pathWindow = null;

        public int editIndex
        {
            get { return _editIndex; }
            set
            {
                if(value == 0)
                {
                    Debug.LogError("Cannot set edit index to 0. 0 is reserved.");
                    return;
                }
                if (value < -1) value = -1;
                _editIndex = value;
            }
        }
        private int _editIndex = -1; //0 is reserved for editing clip values

        protected GUIContent editButtonContent = new GUIContent("Edit", "Enable edit mode in scene view");

        enum SampleTarget { Computer, User }
        private SampleTarget sampleTarget = SampleTarget.Computer;

        public virtual void BaseGUI() {
            base.OnInspectorGUI();
            SplineUser user = (SplineUser)target;
            bool isTargetComputer = (user.user == null || sampleTarget == SampleTarget.Computer);
            if (user.computer != null && !user.computer.IsSubscribed(user)) user.computer.Subscribe(user);
            Undo.RecordObject(user, "Inspector Change");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Spline User", EditorStyles.boldLabel, GUILayout.Width(85));
            GUI.color = new Color(1f, 1f, 1f, 0.75f);
            sampleTarget = (SampleTarget)EditorGUILayout.EnumPopup(sampleTarget, GUILayout.Width(75));
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            if (sampleTarget == SampleTarget.Computer) user.computer = (SplineComputer)EditorGUILayout.ObjectField("Computer", user.computer, typeof(SplineComputer), true);
            else
            {
                SplineUser lastUser = user.user;
                user.user = (SplineUser)EditorGUILayout.ObjectField("User", user.user, typeof(SplineUser), true);
                if(lastUser != user.user && user.rootUser == user)
                {
                    user.user = null;
                    EditorUtility.DisplayDialog("Cannot assign user.", "A SplineUser component cannot sample itself, please select another user to sample.", "OK");
                }
            }
            if (showUpdateMethod && isTargetComputer) user.updateMethod = (SplineUser.UpdateMethod)EditorGUILayout.EnumPopup("Update Method", user.updateMethod);
            if (user.computer == null && isTargetComputer) EditorGUILayout.HelpBox("No SplineComputer or SplineUser is referenced. Reference a SplineComputer or another SplineUser component to make this SplineUser work.", MessageType.Error);

            if (showResolution && isTargetComputer) user.resolution = (double)EditorGUILayout.Slider("Resolution", (float)user.resolution, 0f, 1f);
            if (showClip)
            {
                EditorGUILayout.BeginHorizontal();
                float clipFrom = (float)user.clipFrom;
                float clipTo = (float)user.clipTo;
                EditorGUILayout.BeginHorizontal();
                if (EditButton(_editIndex == 0))
                {
                    if (_editIndex == 0) _editIndex = -1;
                    else _editIndex = 0;
                }
                SplineEditor.hold = _editIndex >= 0;
                EditorGUILayout.MinMaxSlider(new GUIContent("Clip Range:"), ref clipFrom, ref clipTo, 0f, 1f);
                EditorGUILayout.EndHorizontal();
                user.clipFrom = clipFrom;
                user.clipTo = clipTo;
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(30));
                user.clipFrom = EditorGUILayout.FloatField((float)user.clipFrom);
                user.clipTo = EditorGUILayout.FloatField((float)user.clipTo);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
            }
            if (showAveraging && (user.user == null || sampleTarget == SampleTarget.Computer)) user.averageResultVectors = EditorGUILayout.Toggle("Average Result Vectors", user.averageResultVectors);
            if (showMultithreading) user.multithreaded = EditorGUILayout.Toggle("Multithreading", user.multithreaded);
            
            user.buildOnAwake = EditorGUILayout.Toggle("Build on Awake", user.buildOnAwake);
            if (user.computer != null && user.computer.nodeLinks.Length > 0 && isTargetComputer)
            {
                if(GUILayout.Button("Edit junction path"))
                {
                    pathWindow = EditorWindow.GetWindow<PathWindow>();
                    pathWindow.init(this, "Junction Path", new Vector2(300, 150));
                }
            }
        }

        protected virtual void OnSceneGUI()
        {
            SplineUser user = (SplineUser)target;
            if (user.computer == null)
            {
                SplineUser root = user.rootUser;
                if (root == null) return;
                if (root.computer == null) return;
                List<SplineComputer> allComputers = root.computer.GetConnectedComputers();
                for (int i = 0; i < allComputers.Count; i++)
                {
                    if (allComputers[i] == root.computer) continue;
                    SplineDrawer.DrawSplineComputer(allComputers[i], 0.0, 1.0, 0.4f);
                }
                for (int i = 0; i < root.address.depth; i++) SplineDrawer.DrawSplineComputer(root.address.elements[i].computer, root.address.elements[i].startPercent, root.address.elements[i].endPercent, 1f);
            }
            else
            {
                SplineComputer rootComputer = user.GetComponent<SplineComputer>();
                List<SplineComputer> allComputers = user.computer.GetConnectedComputers();
                for (int i = 0; i < allComputers.Count; i++)
                {
                    if (allComputers[i] == rootComputer && _editIndex == -1) continue;
                    SplineDrawer.DrawSplineComputer(allComputers[i], 0.0, 1.0, 0.4f);
                }
                for (int i = 0; i < user.address.depth; i++)
                {
                    if (user.address.elements[i].computer == rootComputer) continue;
                    SplineDrawer.DrawSplineComputer(user.address.elements[i].computer, user.address.elements[i].startPercent, user.address.elements[i].endPercent, 1f);
                }
            }
            if (_editIndex == 0) SceneClipEdit();
        }

        void SceneClipEdit()
        {
            SplineUser user = (SplineUser)target;
            Color col = Color.white;
            if (user.computer != null) col = user.computer.editorPathColor;
            double val = user.clipFrom;
            SplineEditorHandles.Slider(user, ref val, col, "Clip From", SplineEditorHandles.SplineSliderGizmo.ForwardTriangle);
            if (val != user.clipFrom)
            {
                //Debug.Log("Setting clip from to " + val);
                user.clipFrom = val;
            }
            val = user.clipTo;
            SplineEditorHandles.Slider(user, ref val, col, "Clip To", SplineEditorHandles.SplineSliderGizmo.BackwardTriangle);
            if (val != user.clipTo)
            {
                //Debug.Log("Setting clip to to " + val);
                user.clipTo = val;
            }
        }

        public override void OnInspectorGUI()
        {
            BaseGUI();
        }

        protected virtual void OnDestroy()
        {
            if (pathWindow != null) pathWindow.Close();
            SplineUser user = (SplineUser)target;
            if (Application.isEditor && !Application.isPlaying)
            {
                if (user == null) OnDelete(); //The object or the component is being deleted
                else if (user.computer != null) user.Rebuild(true);
            }
            SplineEditor.hold = false;
        }

        protected virtual void OnDelete()
        {

        }

        protected virtual void Awake()
        {
            SplineUser user = (SplineUser)target;
            if (user.user != null) sampleTarget = SampleTarget.User;
            else sampleTarget = SampleTarget.Computer;
            user.EditorAwake();
        }

        public bool EditButton(bool selected)
        {
            float width = 40f;
            editButtonContent.image = ImageDB.GetImage("edit_cursor.png", "Splines/Editor/Icons");
            if (editButtonContent.image != null)
            {
                editButtonContent.text = "";
                width = 25f;
            }
            if (selected)
            {
                GUI.backgroundColor = SplineEditorGUI.selectionColor;
                GUI.contentColor = SplineEditorGUI.selectedButtonContentColor;
            }
            else GUI.contentColor = SplineEditorGUI.buttonContentColor;

            if (GUILayout.Button(editButtonContent, GUILayout.Width(width)))
            {
                GUI.backgroundColor = GUI.contentColor = Color.white;
                GUI.color = Color.white;
                SceneView.RepaintAll();
                return true;
            }
            GUI.backgroundColor = GUI.contentColor = Color.white;
            GUI.color = Color.white;
            return false;
        }
    }
}
