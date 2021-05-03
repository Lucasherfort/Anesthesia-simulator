using UnityEngine;
using System.Collections;
using UnityEditor;
using Dreamteck.Splines;
namespace Dreamteck.Splines
{
    public class RoundedRectangle : SplinePrimitive, ISplinePrimitive
    {
        private Vector2 size = Vector2.one*2f;
        private float radius = 0.5f;
        private float rotation = 0f;
        private int axis = 1;
        private string[] axisText = new string[] {"X", "Y", "Z"};

        public string GetName()
        {
            return "Rounded Rect.";
        }

        public void SetOrigin(Vector3 o)
        {
            origin = o;
        }

        public override void Init(SplineComputer comp)
        {
            base.Init(comp);
        }

        public void Draw()
        {
            axis = EditorGUILayout.Popup("Axis", axis, axisText);
            size = EditorGUILayout.Vector2Field("Size", size);
            radius = EditorGUILayout.FloatField("Radius", radius);
            rotation = EditorGUILayout.Slider("Rotation", rotation, -180f, 180f);
            SplinePoint[] generated = GetPoints(axis, radius, size, rotation);
            OffsetPoints(generated, origin);
            computer.type = Spline.Type.Bezier;
            computer.SetPoints(generated, SplineComputer.Space.Local);
            computer.Close();
            if (GUI.changed)
            {
                UpdateUsers();
                SceneView.RepaintAll();
            }
        }

        public static SplinePoint[] GetPoints(int axis, float radius, Vector2 size, float rotation) 
        {
            Vector3 look = Vector3.right;
            Vector2 edgeSize = size - Vector2.one * radius * 2f;

            if (axis == 1) look = Vector3.up;
            if (axis == 2) look = Vector3.forward;
            SplinePoint[] points = CreatePoints(9, 1f, look, Color.white);

            points[0].SetPosition(Vector3.up / 2f * edgeSize.y + Vector3.left / 2f * size.x);
            points[1].SetPosition(Vector3.up / 2f * size.y + Vector3.left / 2f * edgeSize.x);
            points[2].SetPosition(Vector3.up / 2f * size.y + Vector3.right / 2f * edgeSize.x);
            points[3].SetPosition(Vector3.up / 2f * edgeSize.y + Vector3.right / 2f * size.x);
            points[4].SetPosition(Vector3.down / 2f * edgeSize.y + Vector3.right / 2f * size.x);
            points[5].SetPosition(Vector3.down / 2f * size.y + Vector3.right / 2f * edgeSize.x);
            points[6].SetPosition(Vector3.down / 2f * size.y + Vector3.left / 2f * edgeSize.x);
            points[7].SetPosition(Vector3.down / 2f * edgeSize.y + Vector3.left / 2f * size.x);


            float rad = 2f * (Mathf.Sqrt(2f) - 1f) / 3f * radius * 2f;
            points[0].SetTangent2Position(points[0].position + Vector3.up * rad);
            points[1].SetTangentPosition(points[1].position + Vector3.left * rad);
            points[2].SetTangent2Position(points[2].position + Vector3.right * rad);
            points[3].SetTangentPosition(points[3].position + Vector3.up * rad);

            points[4].SetTangent2Position(points[4].position + Vector3.down * rad);
            points[5].SetTangentPosition(points[5].position + Vector3.right * rad);
            points[6].SetTangent2Position(points[6].position + Vector3.left * rad);
            points[7].SetTangentPosition(points[7].position + Vector3.down * rad);


            points[8] = points[0];

            if (look != Vector3.forward)
            {
                Quaternion lookRot = Quaternion.LookRotation(look);
                RotatePoints(points, lookRot * Quaternion.AngleAxis(rotation, look));
            }
            return points;
        }

        public void Cancel()
        {
            Revert();
        }

    }
}
