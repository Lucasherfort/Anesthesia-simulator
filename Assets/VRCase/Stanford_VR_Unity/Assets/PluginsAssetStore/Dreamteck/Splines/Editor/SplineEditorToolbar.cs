using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Dreamteck.Splines
{
    public partial class SplineEditor
    {
        private bool mouseHoversToolbar = false;
        private int minWidth = 600;
        private float scale = 1f;
        private PresetsWindow presetWindow = null;


        private static GUIContent presetButtonContent = new GUIContent("P", "Open Primitives and Presets");
        private static GUIContent moveButtonContent = new GUIContent("M", "Move points");
        private static GUIContent rotateButtonContent = new GUIContent("R", "Rotate points");
        private static GUIContent scaleButtonContent = new GUIContent("S", "Scale points");
        private static GUIContent normalsButtonContent = new GUIContent("N", "Edit point normals");
        private static GUIContent mirrorButtonContent = new GUIContent("||", "Symmetry editor");
        private static GUIContent mergeButtonContent = new GUIContent("><", "Merge Spline Computers");
        private static GUIContent addButtonContent = new GUIContent("+", "Enter point creation mode");
        private static GUIContent removeButtonContent = new GUIContent("-", "Enter point removal mode");

        public void DisableToolbar()
        {
            if (presetWindow != null) presetWindow.Close();
        }

        public void EnableToolbar()
        {
            Texture2D tex = ImageDB.GetImage("presets.png", "Splines/Editor/Icons");
            if (tex != null) { presetButtonContent.image = tex; presetButtonContent.text = ""; }
            tex = ImageDB.GetImage("move.png", "Splines/Editor/Icons");
            if (tex != null) { moveButtonContent.image = tex; moveButtonContent.text = ""; }
            tex = ImageDB.GetImage("rotate.png", "Splines/Editor/Icons");
            if (tex != null) { rotateButtonContent.image = tex; rotateButtonContent.text = ""; }
            tex = ImageDB.GetImage("scale.png", "Splines/Editor/Icons");
            if (tex != null) { scaleButtonContent.image = tex; scaleButtonContent.text = ""; }
            tex = ImageDB.GetImage("normals.png", "Splines/Editor/Icons");
            if (tex != null) { normalsButtonContent.image = tex; normalsButtonContent.text = ""; }
            tex = ImageDB.GetImage("mirror.png", "Splines/Editor/Icons");
            if (tex != null) { mirrorButtonContent.image = tex; mirrorButtonContent.text = ""; }
            tex = ImageDB.GetImage("merge.png", "Splines/Editor/Icons");
            if (tex != null) { mergeButtonContent.image = tex; mergeButtonContent.text = ""; }
        }

        public void DrawToolbar()
        {
            if (Screen.width < minWidth) scale = (float)Screen.width/minWidth;
            else scale = 1f;
            SplineEditorGUI.SetScale(scale);
            SplineEditorGUI.scale = scale;
            mouseHoversToolbar = false;
            minWidth = 610;
            if (InMirrorMode()) Mirror(Mathf.RoundToInt(44 * scale));
            else if(InMergeMode()) Merge(Mathf.RoundToInt(44 * scale));
            else
            {
                if (tool == SplineEditor.PointTool.Create) Create(Mathf.RoundToInt(44 * scale));
                if (tool == SplineEditor.PointTool.NormalEdit) Normals(Mathf.RoundToInt(44 * scale));
                if (tool == SplineEditor.PointTool.Scale) Scale(Mathf.RoundToInt(44 * scale));
                if (tool == SplineEditor.PointTool.Rotate) Rotate(Mathf.RoundToInt(44 * scale));
            }
            
            Main();
        }
         
        private void Main() {
            Rect barRect = new Rect(0f, 0f, Screen.width, 45*scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;

            if (computer.hasMorph && !MorphWindow.editShapeMode)
            {
                SplineEditorGUI.Label(new Rect(Screen.width / 2f - 200f, 10, 400, 30), "Editing unavailable outside of morph states.");
                return;
            }
            if (computer.hasMorph)
            {
                if (tool == SplineEditor.PointTool.Create || tool == SplineEditor.PointTool.Delete) tool = SplineEditor.PointTool.None;
            }
            else
            {
                if (SplineEditorGUI.BigButton(new Rect(5 * scale, 5 * scale, 35 * scale, 35 * scale), addButtonContent, true, tool == SplineEditor.PointTool.Create)) ToggleCreateTool();
                if (SplineEditorGUI.BigButton(new Rect(45 * scale, 5 * scale, 35 * scale, 35 * scale), removeButtonContent, computer.pointCount > 0, tool == SplineEditor.PointTool.Delete))
                {
                    if (tool != SplineEditor.PointTool.Delete) tool = SplineEditor.PointTool.Delete;
                    else tool = SplineEditor.PointTool.None;
                }
                if (SplineEditorGUI.BigButton(new Rect(85 * scale, 5 * scale, 35 * scale, 35 * scale), presetButtonContent, true, presetWindow != null))
                {
                    if (presetWindow == null)
                    {
                        presetWindow = EditorWindow.GetWindow<PresetsWindow>();
                        presetWindow.init(this, "Primitives & Presets", new Vector3(200, 200));
                    }
                }
            }

            if (SplineEditorGUI.BigButton(new Rect(150 * scale, 5 * scale, 35 * scale, 35 * scale), moveButtonContent, true, tool == SplineEditor.PointTool.Move)) {
                if (tool != SplineEditor.PointTool.Move) ToggleMoveTool();
                else tool = SplineEditor.PointTool.None;
            }
            if (SplineEditorGUI.BigButton(new Rect(190 * scale, 5 * scale, 35 * scale, 35 * scale), rotateButtonContent, true, tool == SplineEditor.PointTool.Rotate))
            {
                if (tool != SplineEditor.PointTool.Rotate) ToggleRotateTool();
                else tool = SplineEditor.PointTool.None;
            }
            if (SplineEditorGUI.BigButton(new Rect(230 * scale, 5 * scale, 35 * scale, 35 * scale), scaleButtonContent, true, tool == SplineEditor.PointTool.Scale))
            {
                if (tool != SplineEditor.PointTool.Scale) ToggleScaleTool();
                else tool = SplineEditor.PointTool.None;
            }
            if (SplineEditorGUI.BigButton(new Rect(270 * scale, 5 * scale, 35 * scale, 35 * scale), normalsButtonContent, true, tool == SplineEditor.PointTool.NormalEdit))
            {
                if (tool != SplineEditor.PointTool.NormalEdit) tool = SplineEditor.PointTool.NormalEdit;
                else tool = SplineEditor.PointTool.None;
            }
            if (SplineEditorGUI.BigButton(new Rect(330 * scale, 5 * scale, 35 * scale, 35 * scale), mirrorButtonContent, computer.pointCount > 0 || InMirrorMode(), InMirrorMode()))
            {
                if (InMirrorMode()) ExitMirrorMode();
                else EnterMirrorMode();
            }

            if (SplineEditorGUI.BigButton(new Rect(370 * scale, 5 * scale, 35 * scale, 35 * scale), mergeButtonContent, computer.pointCount > 0 && !computer.isClosed, InMergeMode()))
            {
                if (InMergeMode()) ExitMergeMode();
                else EnterMergeMode();
            }

            int operation = 0;
            List<string> options = new List<string>();
            options.Add("Operations");
            if (selectedPointsCount > 0) {
                options.Add("Center to Transform");
                options.Add("Move Transform to");
            }
            if (selectedPointsCount >= 2)
            {
                options.Add("Flat X");
                options.Add("Flat Y");
                options.Add("Flat Z");
                options.Add("Mirror X");
                options.Add("Mirror Y");
                options.Add("Mirror Z");
            }
            if (selectedPointsCount >= 3) options.Add("Distribute evenly");
            float operationsPosition = Screen.width - (190 * scale + 100);
#if UNITY_EDITOR_OSX
            operationsPosition = 430 * scale;
#endif

            bool hover = SplineEditorGUI.DropDown(new Rect(operationsPosition, 10 * scale, 150 * scale, 25 * scale), SplineEditorGUI.defaultButton, options.ToArray(), HasSelection(), ref operation);
            if (hover) mouseHoversToolbar = true;
            if (operation > 0)
            {
                if (operation == 1 && selectedPointsCount > 0) CenterSelection();
                else if (operation == 2 && selectedPointsCount > 0) MoveTransformToSelection();
                if (selectedPointsCount >= 2)
                {
                    if (operation <= 5) FlatSelection(operation - 3);
                    else if (operation <= 8) MirrorSelection(operation - 6);
                    else if (operation == 9) DistributeEvenly();
                }
            }
            GUI.color = SplineEditorGUI.activeColor;
            ((SplineComputer)target).editorPathColor = EditorGUI.ColorField(new Rect(operationsPosition + 160 * scale, 13 * scale, 40 * scale, 20 * scale), ((SplineComputer)target).editorPathColor);
        }

        private void Create(int verticalOffset)
        {
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            SplineEditorGUI.Label(new Rect(5 * scale, verticalOffset+5 * scale, 105 * scale, 25 * scale), "Place method:", true);
            bool hover = SplineEditorGUI.DropDown(new Rect(115 * scale, verticalOffset+5 * scale, 140 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] { "Camera Plane", "Insert", "Surface", "Plane-X", "Plane-Y", "Plane-Z" }, true, ref createPointMode);
            if (hover) mouseHoversToolbar = true;
            SplineEditorGUI.Label(new Rect(280 * scale, verticalOffset+5 * scale, 135 * scale, 25 * scale), "Normal orientation:", true);
            hover = SplineEditorGUI.DropDown(new Rect(420 * scale, verticalOffset+5 * scale, 160 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] { "Auto", "Look at Camera", "Align with Camera", "Calculate", "Left", "Right", "Up", "Down", "Forward", "Back" }, true, ref createNormalMode);
            if (hover) mouseHoversToolbar = true;
            SplineEditorGUI.Label(new Rect(575 * scale, verticalOffset + 5 * scale, 90 * scale, 25 * scale), "Add Node", true);
            createNodeOnCreatePoint = GUI.Toggle(new Rect(670 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), createNodeOnCreatePoint, "");

            bool showNormalField = false;
            if (createPointMode == 0)
            {
                SplineEditorGUI.Label(new Rect(700 * scale, verticalOffset+5 * scale, 80 * scale, 30 * scale), "Far plane:", true);
                showNormalField = true;
            }
            

            if (createPointMode == 2)
            {
                SplineEditorGUI.Label(new Rect(700 * scale, verticalOffset+5 * scale, 100 * scale, 30 * scale), "Normal offset:", true);
                showNormalField = true;
            }

            if (createPointMode >= 3 && createPointMode <= 5)
            {
                SplineEditorGUI.Label(new Rect(700 * scale, verticalOffset+5 * scale, 80 * scale, 30 * scale), "Grid offset:", true);
                showNormalField = true;
            }
            minWidth = 790;
            if (createPointMode != 1)
            {
                SplineEditorGUI.Label(new Rect(850 * scale, verticalOffset + 5 * scale, 80 * scale, 30 * scale), "Append:", true);
                if (SplineEditorGUI.DropDown(new Rect(940 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] { "End", "Beginning"}, true, ref appendMode)) mouseHoversToolbar = true;
                minWidth = 1100;
            }
            

            if (showNormalField) {
                createPointOffset = SplineEditorGUI.FloatField(new Rect(790 * scale, verticalOffset+5 * scale, 70 * scale, 25 * scale), createPointOffset);
                createPointOffset = SplineEditorGUI.FloatDrag(new Rect(700 * scale, verticalOffset+5 * scale, 80 * scale, 25 * scale), createPointOffset);
                if (createPointOffset < 0f && createPointMode < 3) createPointOffset = 0f;
            }

        }

        private void Normals(int verticalOffset)
        {
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            if(SplineEditorGUI.Button(new Rect(5 * scale, verticalOffset+5 * scale, 130 * scale, 25 * scale), "Set Normals:")) SetSelectedNormals();
            bool hover = SplineEditorGUI.DropDown(new Rect(160 * scale, verticalOffset+5 * scale, 150 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] {"At Camera", "Align with Camera", "Calculate", "Left", "Right", "Up", "Down", "Forward", "Back", "Inverse", "At Avg. Center", "By Direction"}, true, ref normalEditor.setNormalMode);
            if (hover) mouseHoversToolbar = true;
        }


        private void Scale(int verticalOffset)
        {
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            SplineEditorGUI.Label(new Rect(5 * scale, verticalOffset + 5 * scale, 90 * scale, 25 * scale), "Scale sizes", true);
            scaleEditor.scaleSize = GUI.Toggle(new Rect(100 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), scaleEditor.scaleSize, "");

            SplineEditorGUI.Label(new Rect(110 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Scale tangents", true);
            scaleEditor.scaleTangents = GUI.Toggle(new Rect(235 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), scaleEditor.scaleTangents, "");
        }

        private void Rotate(int verticalOffset)
        {
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            SplineEditorGUI.Label(new Rect(5 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Rotate normals", true);
            rotationEditor.rotateNormals = GUI.Toggle(new Rect(130 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), rotationEditor.rotateNormals, "");

            SplineEditorGUI.Label(new Rect(140 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Rotate tangents", true);
            rotationEditor.rotateTangents = GUI.Toggle(new Rect(265 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), rotationEditor.rotateTangents, "");
        }

        private void Mirror(int verticalOffset)
        {
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            if (SplineEditorGUI.Button(new Rect(5 * scale, verticalOffset + 5 * scale, 100 * scale, 25 * scale), "Cancel")) ExitMirrorMode();
            if (SplineEditorGUI.Button(new Rect(115 * scale, verticalOffset + 5 * scale, 100 * scale, 25 * scale), "Save")) SaveMirror();

            SplineEditorGUI.Label(new Rect(215 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), "Axis", true);
            int axis = (int)mirrorEditor.axis;
            bool hover = SplineEditorGUI.DropDown(new Rect(270 * scale, verticalOffset + 5 * scale, 60 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] { "X", "Y", "Z"}, true, ref axis);
            mirrorEditor.axis = (SplinePointMirrorEditor.Axis)axis;
            if (hover) mouseHoversToolbar = true;

            SplineEditorGUI.Label(new Rect(315 * scale, verticalOffset + 5 * scale, 60 * scale, 25 * scale), "Flip", true);
            mirrorEditor.flip = GUI.Toggle(new Rect(380 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), mirrorEditor.flip, "");

            SplineEditorGUI.Label(new Rect(390 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Weld Distance", true);

            mirrorEditor.weldDistance = SplineEditorGUI.FloatField(new Rect(525 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), mirrorEditor.weldDistance);
            mirrorEditor.weldDistance = SplineEditorGUI.FloatDrag(new Rect(390 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), mirrorEditor.weldDistance);
            if (mirrorEditor.weldDistance < 0f) mirrorEditor.weldDistance = 0f;

            SplineEditorGUI.Label(new Rect(570 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Center  X:", true);
            mirrorEditor.center.x = SplineEditorGUI.FloatField(new Rect(700 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), mirrorEditor.center.x);

            SplineEditorGUI.Label(new Rect(720 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), "Y:", true);
            mirrorEditor.center.y = SplineEditorGUI.FloatField(new Rect(770 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), mirrorEditor.center.y);
            SplineEditorGUI.Label(new Rect(790 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), "Z:", true);
            mirrorEditor.center.z = SplineEditorGUI.FloatField(new Rect(840 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), mirrorEditor.center.z);
        }

        private void Merge(int verticalOffset)
        {
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            SplineEditorGUI.Label(new Rect(5 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Merge endpoints", true);
            mergeEditor.mergeEndpoints = GUI.Toggle(new Rect(130 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), mergeEditor.mergeEndpoints, "");
            int mergeSide = (int)mergeEditor.mergeSide;
            SplineEditorGUI.Label(new Rect(120 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Merge side", true);
            bool hover = SplineEditorGUI.DropDown(new Rect(250, verticalOffset + 5 * scale, 100 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] { "Start", "End" }, true, ref mergeSide);
            mergeEditor.mergeSide = (SplineComputerMergeEditor.MergeSide)mergeSide;
            if (hover) mouseHoversToolbar = true;
        }
    }
}
