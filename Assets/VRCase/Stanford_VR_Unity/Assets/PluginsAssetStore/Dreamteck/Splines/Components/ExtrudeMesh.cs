using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Extrude Mesh")]
    public class ExtrudeMesh : MeshGenerator
    {
        public enum Axis { X, Y, Z }
        public enum Iteration { Ordered, Random }
        public enum MirrorMethod { None, X, Y, Z }

        public Axis axis
        {
            get { return _axis; }
            set
            {
                if (value != _axis)
                {
                    _axis = value;
                    UpdateExtrudableMeshes();
                    Rebuild(false);
                }
            }
        }

        public Iteration iteration
        {
            get { return _iteration; }
            set
            {
                if (value != _iteration)
                {
                    _iteration = value;
                    UpdateExtrudableMeshes();
                    Rebuild(false);
                }
            }
        }

        public int randomSeed
        {
            get { return _randomSeed; }
            set
            {
                if (value != _randomSeed)
                {
                    _randomSeed = value;
                    if (_iteration == Iteration.Random)
                    {
                        UpdateExtrudableMeshes();
                        Rebuild(false);
                    }
                }
            }
        }

        [System.Obsolete("Deprecated. Use the GetMesh, SetMesh, AddMesh and RemoveMesh methods instead")]
        public Mesh sourceMesh
        {
            get { return _sourceMesh; }
            set
            {
                if (value != _sourceMesh)
                {
                    Debug.LogWarning("Setting deprecated property sourceMesh! Please use SetMesh instead.");
                }
            }
        }

        public int repeat
        {
            get { return _repeat; }
            set
            {
                if (value != _repeat)
                {
                    _repeat = value;
                    if (_repeat < 1) _repeat = 1;
                    UpdateEndExtrudeMesh();
                    Rebuild(false);
                }
            }
        }

        public double spacing
        {
            get { return _spacing; }
            set
            {
                if (value != _spacing)
                {
                    if ((_spacing == 0f && value > 0f) || (value == 0f && _spacing > 0f)) UpdateExtrudableMeshes();
                    _spacing = value;
                    Rebuild(false);
                }
            }
        }

        public Vector2 scale
        {
            get { return _scale; }
            set
            {
                if (value != _scale)
                {
                    _scale = value;
                    Rebuild(false);
                }
            }
        }

        public ExtrudeUVMode extrudeUVMode
        {
            get { return _extrudeUvMode;  }
            set
            {
                if(value != _extrudeUvMode)
                {
                    _extrudeUvMode = value;
                    Rebuild(false);
                }
            }
        }

        //Mesh data
        [SerializeField]
        [HideInInspector]
        private Mesh _sourceMesh = null;

        [SerializeField]
        [HideInInspector]
        private Mesh _startMesh = null;
        [SerializeField]
        [HideInInspector]
        private Mesh _endMesh = null;
        [SerializeField]
        [HideInInspector]
        private Mesh[] _middleMeshes = new Mesh[0];
        [SerializeField]
        [HideInInspector]
        private List<ExtrudableMesh> extrudableMeshes = new List<ExtrudableMesh>();
        [SerializeField]
        [HideInInspector]
        private Axis _axis = Axis.Z;
        [SerializeField]
        [HideInInspector]
        private Iteration _iteration = Iteration.Ordered;
        [SerializeField]
        [HideInInspector]
        private int _randomSeed = 0;
        [SerializeField]
        [HideInInspector]
        private int _repeat = 1;
        [SerializeField]
        [HideInInspector]
        private double _spacing = 0.0;
        [SerializeField]
        [HideInInspector]
        private Vector2 _scale = Vector2.one;
        private SplineResult lastResult = new SplineResult();
        private bool useLastResult = false;
        private List<TS_Mesh> combineMeshes = new List<TS_Mesh>();
        private System.Random random;
        private int iterations = 0;
        private bool _hasAnyMesh = false;
        private bool _hasStartMesh = false;
        private bool _hasEndMesh = false;

        public bool hasAnyMesh
        {
            get { return _hasAnyMesh; }
        }

        public enum ExtrudeUVMode { Default, UniformU, UniformV }
        [SerializeField]
        [HideInInspector]
        private ExtrudeUVMode _extrudeUvMode = ExtrudeUVMode.Default;

#if UNITY_EDITOR
        public override void EditorAwake()
        {
            UpdateExtrudableMeshes();
            CheckMeshes();
            base.EditorAwake();
        }
#endif

        protected override void Awake()
        {
            if(_sourceMesh != null) UpdateExtrudableMeshes(); //This will be removed in version 1.0.9 but is currently kept so that it migrates the sourceMesh property to the new mesh array
            base.Awake();
            CheckMeshes();
            mesh.name = "Stretch Mesh";
        }

        public Mesh GetStartMesh()
        {
            return _startMesh;
        }

        public Mesh GetEndMesh()
        {
            return _endMesh;
        }

        public MirrorMethod GetStartMeshMirror()
        {
            if (extrudableMeshes.Count == 0) return MirrorMethod.None;
            if (extrudableMeshes[0] == null) return MirrorMethod.None;
            return extrudableMeshes[0].mirror;
        }

        public MirrorMethod GetEndMeshMirror()
        {
            if (extrudableMeshes.Count < 2) return MirrorMethod.None;
            if (extrudableMeshes[1] == null) return MirrorMethod.None;
            return extrudableMeshes[1].mirror;
        }

        public void SetStartMeshMirror(MirrorMethod mirror)
        {
            if (extrudableMeshes.Count == 0) return;
            if (extrudableMeshes[0] == null) return;
            extrudableMeshes[0].mirror = mirror;
            CheckMeshes();
            Rebuild(false);
        }

        public void SetEndMeshMirror(MirrorMethod mirror)
        {
            if (extrudableMeshes.Count < 2) return;
            if (extrudableMeshes[1] == null) return; 
            extrudableMeshes[1].mirror = mirror;
            CheckMeshes();
            Rebuild(false);
        }

        public void SetMeshMirror(int index, MirrorMethod mirror)
        {
            if (extrudableMeshes.Count < 2 + index) return;
            if (extrudableMeshes[2 + index] == null) return;
            extrudableMeshes[2 + index].mirror = mirror;
            CheckMeshes();
            Rebuild(false);
        }

        public void SetStartMesh(Mesh inputMesh, MirrorMethod mirror = MirrorMethod.None)
        {
            _startMesh = inputMesh;
            if (extrudableMeshes.Count == 0) extrudableMeshes.Add(null);
            if(_startMesh == null)
            {
                extrudableMeshes[0] = null;
                Rebuild(false);
                return;
            }
            extrudableMeshes[0] = new ExtrudableMesh(_startMesh, _axis);
            extrudableMeshes[0].mirror = mirror;
            CheckMeshes();
            Rebuild(false);
        }

        public void SetEndMesh(Mesh inputMesh, MirrorMethod mirror = MirrorMethod.None)
        {
            _endMesh = inputMesh;
            if (extrudableMeshes.Count < 2) extrudableMeshes.AddRange(new ExtrudableMesh[2-extrudableMeshes.Count]);
            if (_endMesh == null)
            {
                extrudableMeshes[1] = null;
                Rebuild(false);
                return;
            }
            extrudableMeshes[1] = new ExtrudableMesh(_endMesh, _axis);
            extrudableMeshes[1].mirror = mirror;
            CheckMeshes();
            Rebuild(false);
        }

        public Mesh GetMesh(int index)
        {
            return _middleMeshes[index];
        }

        public MirrorMethod GetMeshMirror(int index)
        {
            if (extrudableMeshes[index+2] == null) return MirrorMethod.None;
            return extrudableMeshes[index+2].mirror;
        }

        public void SetMesh(int index, Mesh inputMesh, MirrorMethod mirror = MirrorMethod.None)
        {
            if (inputMesh == null)
            {
                RemoveMesh(index);
                return;
            }
            _middleMeshes[index] = inputMesh;
            UpdateExtrudableMeshes();
            extrudableMeshes[2 + index].mirror = mirror;
            CheckMeshes();
            Rebuild(false);
        }

        public void RemoveMesh(int index)
        {
            Mesh[] newMeshes = new Mesh[_middleMeshes.Length - 1];
            for (int i = 0; i < _middleMeshes.Length; i++)
            {
                if (i < index) newMeshes[i] = _middleMeshes[i];
                else if (i > index) newMeshes[i - 1] = _middleMeshes[i];
            }
            _middleMeshes = newMeshes;
            extrudableMeshes.RemoveAt(index+2);
            UpdateExtrudableMeshes();
            CheckMeshes();
            Rebuild(false);
        }

        public void AddMesh(Mesh inputMesh)
        {
            if (inputMesh == null) return;
            Mesh[] newMeshes = new Mesh[_middleMeshes.Length + 1];
            _middleMeshes.CopyTo(newMeshes, 0);
            newMeshes[newMeshes.Length - 1] = inputMesh;
            _middleMeshes = newMeshes;
            UpdateExtrudableMeshes();
            CheckMeshes();
            Rebuild(false);
        }

        void CheckMeshes()
        {
            _hasAnyMesh = false;
            _hasStartMesh = false;
            _hasEndMesh = false;
            if (_startMesh != null)
            {
                _hasAnyMesh = true;
                _hasStartMesh = true;
            }
            for(int i = 0; i < _middleMeshes.Length; i++)
            {
                if (_middleMeshes[i] != null)
                {
                    _hasAnyMesh = true;
                    break;
                }
            }
            if (_endMesh != null)
            {
                _hasAnyMesh = true;
                _hasEndMesh = true;
            }
        }

        public int GetMeshCount()
        {
            return _middleMeshes.Length;
        }

        protected override void BuildMesh()
        {
            if (clippedSamples.Length == 0) return;
            base.BuildMesh();
            if (!_hasAnyMesh && !multithreaded)
            {
                UpdateExtrudableMeshes();
                if(!_hasAnyMesh) return;
            }
            Generate();
        }

        void Generate()
        {
            random = new System.Random(_randomSeed);
            useLastResult = false;
            iterations = 0;
            if (_hasStartMesh) iterations++;
            if (_hasEndMesh) iterations++;
            iterations += (extrudableMeshes.Count-2) * _repeat;
            double step = span / iterations;
            double space = step * _spacing * 0.5;
            if (combineMeshes.Count < iterations) combineMeshes.AddRange(new TS_Mesh[iterations - combineMeshes.Count]);
            else if (combineMeshes.Count > iterations) combineMeshes.RemoveRange((combineMeshes.Count - 1) - (combineMeshes.Count - iterations), combineMeshes.Count - iterations);

            for (int i = 0; i < iterations; i++)
            {
                double from = clipFrom + i * step + space;
                double to = clipFrom + i * step + step - space;
                ExtrudableMesh current = extrudableMeshes[GetMeshIndex(i)];
                if (combineMeshes[i] == null) combineMeshes[i] = new TS_Mesh();
                if (combineMeshes[i].vertices.Length != current.vertices.Length) combineMeshes[i].vertices = new Vector3[current.vertices.Length];
                if (combineMeshes[i].normals.Length != current.normals.Length) combineMeshes[i].normals = new Vector3[current.normals.Length];
                if (combineMeshes[i].tangents.Length != current.tangents.Length) combineMeshes[i].tangents = new Vector4[current.tangents.Length];
                if (combineMeshes[i].colors.Length != current.colors.Length) combineMeshes[i].colors = new Color[current.colors.Length];
                if (combineMeshes[i].uv.Length != current.uv.Length) combineMeshes[i].uv = new Vector2[current.uv.Length];
                current.uv.CopyTo(combineMeshes[i].uv, 0);
                current.colors.CopyTo(combineMeshes[i].colors, 0);
                combineMeshes[i].subMeshes.Clear();
                for (int n = 0; n < current.subMeshes.Count; n++) combineMeshes[i].subMeshes.Add(current.subMeshes[n].triangles);
                uvs = Vector2.zero;
                Stretch(current, combineMeshes[i], from, to);
                if (_spacing == 0f) useLastResult = true;
            }
            if (tsMesh == null) tsMesh = new TS_Mesh();
            else tsMesh.Clear();
            tsMesh.Combine(combineMeshes);
        }

        /// <summary>
        /// Used to get the index of the extrudableMesh at the given repeat iteration
        /// </summary>
        /// <param name="repeatIndex"></param>
        /// <returns></returns>
        private int GetMeshIndex(int repeatIndex)
        {
            if (repeatIndex == 0 && _hasStartMesh) return 0;
            if (repeatIndex == iterations - 1 && _hasEndMesh) return 1;
            if (_middleMeshes.Length == 0)
            {
                if (_hasStartMesh) return 0;
                else if (_hasEndMesh) return 1;
            }
            if (_middleMeshes.Length == 1) return 2;
            if (_iteration == Iteration.Random) return 2 + random.Next(_middleMeshes.Length);
            return 2 + (repeatIndex - (_hasStartMesh ? 0 : 1)) % _middleMeshes.Length;
        }

        private void Stretch(ExtrudableMesh source, TS_Mesh target, double from, double to)
        {
            SplineResult result = new SplineResult();
            if (_axis == Axis.X)
            {
                for (int i = 0; i < source.vertexGroups.Count; i++)
                {
                    //Get the group's percent in the bounding box
                    double xPercent = DMath.Clamp01(Mathf.InverseLerp(source.bounds.min.x, source.bounds.max.x, source.vertexGroups[i].value));

                    if (useLastResult && i == source.vertexGroups.Count) result = lastResult;
                    else Evaluate(DMath.Lerp(from, to, xPercent), ref result);

                    if (extrudeUVMode != ExtrudeUVMode.Default) uvs.y = CalculateLength(clipFrom, result.percent);
                    if (i == 0) lastResult.CopyFrom(result);

                    for (int n = 0; n < source.vertexGroups[i].ids.Length; n++)
                    {
                        int index = source.vertexGroups[i].ids[n];
                        float yPercent = Mathf.Clamp01(Mathf.InverseLerp(source.bounds.min.y, source.bounds.max.y, source.vertices[index].y));
                        float zPercent = Mathf.Clamp01(Mathf.InverseLerp(source.bounds.min.z, source.bounds.max.z, source.vertices[index].z));
                        Quaternion rot = Quaternion.AngleAxis(rotation, result.direction);
                        Vector3 right = Vector3.Cross(result.direction, result.normal);
                        target.vertices[index] = result.position + rot * right * Mathf.Lerp(source.bounds.min.z, source.bounds.max.z, zPercent) * result.size * _scale.x - right * offset.x;
                        target.vertices[index] += rot * result.normal * Mathf.Lerp(source.bounds.min.y, source.bounds.max.y, yPercent) * result.size * _scale.y + result.normal * offset.y;
                        target.vertices[index] += result.direction * offset.z;
                        //Apply all rotations to the normal
                        target.normals[index] = rot * result.rotation * Quaternion.AngleAxis(-90f, Vector3.up) * Quaternion.FromToRotation(Vector3.up, result.normal) * source.normals[index];
                        if (extrudeUVMode == ExtrudeUVMode.UniformV) target.uv[index] = uvOffset + new Vector2(source.uv[index].x * uvScale.x, uvs.y * uvScale.y);
                        else if (extrudeUVMode == ExtrudeUVMode.UniformU) target.uv[index] = uvOffset + new Vector2(uvs.y * uvScale.x, source.uv[index].y * uvScale.y);
                    }
                }
            }

            if (_axis == Axis.Y)
            {
                for (int i = 0; i < source.vertexGroups.Count; i++)
                {
                    double yPercent = DMath.Clamp01(Mathf.InverseLerp(source.bounds.min.y, source.bounds.max.y, source.vertexGroups[i].value));
                    if (useLastResult && i == source.vertexGroups.Count) result = lastResult;
                    else Evaluate(DMath.Lerp(from, to, yPercent), ref result);

                    if (extrudeUVMode != ExtrudeUVMode.Default) uvs.y = CalculateLength(clipFrom, result.percent);
                    if (i == 0) lastResult.CopyFrom(result);

                    for (int n = 0; n < source.vertexGroups[i].ids.Length; n++)
                    {
                        int index = source.vertexGroups[i].ids[n];
                        float xPercent = Mathf.Clamp01(Mathf.InverseLerp(source.bounds.min.x, source.bounds.max.x, source.vertices[index].x));
                        float zPercent = Mathf.Clamp01(Mathf.InverseLerp(source.bounds.min.z, source.bounds.max.z, source.vertices[index].z));
                        Quaternion rot = Quaternion.AngleAxis(rotation, result.direction);
                        Vector3 right = Vector3.Cross(result.direction, result.normal);
                        target.vertices[index] = result.position - rot * right * Mathf.Lerp(source.bounds.min.x, source.bounds.max.x, xPercent) * result.size * _scale.x - right * offset.x;
                        target.vertices[index] -= rot * result.normal * Mathf.Lerp(source.bounds.min.z, source.bounds.max.z, zPercent) * result.size * _scale.y - result.normal * offset.y;
                        target.vertices[index] += result.direction * offset.z;
                        target.normals[index] = rot * result.rotation * Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.FromToRotation(Vector3.up, result.normal) * source.normals[index];
                        if (extrudeUVMode == ExtrudeUVMode.UniformV) target.uv[index] = uvOffset + new Vector2(source.uv[index].x * uvScale.x, uvs.y * uvScale.y);
                        else if (extrudeUVMode == ExtrudeUVMode.UniformU) target.uv[index] = uvOffset + new Vector2(uvs.y * uvScale.x, source.uv[index].y * uvScale.y);
                    }

                }
            }

            if (_axis == Axis.Z)
            {
                for (int i = 0; i < source.vertexGroups.Count; i++)
                {
                    double zPercent = DMath.Clamp01(Mathf.InverseLerp(source.bounds.min.z, source.bounds.max.z, source.vertexGroups[i].value));
                    if (useLastResult && i == source.vertexGroups.Count) result = lastResult;
                    else Evaluate(DMath.Lerp(from, to, zPercent), ref result);

                    if(extrudeUVMode != ExtrudeUVMode.Default) uvs.y = CalculateLength(clipFrom, result.percent);
                    if (i == 0) lastResult.CopyFrom(result);
                    for (int n = 0; n < source.vertexGroups[i].ids.Length; n++)
                    {
                        int index = source.vertexGroups[i].ids[n];
                        float xPercent = Mathf.Clamp01(Mathf.InverseLerp(source.bounds.min.x, source.bounds.max.x, source.vertices[index].x));
                        float yPercent = Mathf.Clamp01(Mathf.InverseLerp(source.bounds.min.y, source.bounds.max.y, source.vertices[index].y));
                        Quaternion rot = Quaternion.AngleAxis(rotation, result.direction);
                        Vector3 right = Vector3.Cross(result.direction, result.normal);
                        target.vertices[index] = result.position - rot * right * Mathf.Lerp(source.bounds.min.x, source.bounds.max.x, xPercent) * result.size * _scale.x - right * offset.x;
                        target.vertices[index] += rot * result.normal * Mathf.Lerp(source.bounds.min.y, source.bounds.max.y, yPercent) * result.size * _scale.y + result.normal * offset.y;
                        target.vertices[index] += result.direction * offset.z;
                        target.normals[index] = rot * result.rotation * source.normals[index];
                        if (extrudeUVMode == ExtrudeUVMode.UniformV) target.uv[index] = uvOffset + new Vector2(source.uv[index].x * uvScale.x, uvs.y * uvScale.y);
                        else if (extrudeUVMode == ExtrudeUVMode.UniformU) target.uv[index] = uvOffset + new Vector2(uvs.y * uvScale.x, source.uv[index].y * uvScale.y);
                    }
                }
            }
        }

        /// <summary>
        /// Creates the mesh array that will be referenced when extruding the meshes. Elements 0 and 1 are the start and end cap meshes
        /// </summary>
        void UpdateExtrudableMeshes()
        {
            iterations = 0;
            if (_startMesh != null) iterations++;
            if (_endMesh != null) iterations++;
            iterations += (extrudableMeshes.Count - 2) * _repeat;
            int targetCount = 2 + _middleMeshes.Length;
            if (extrudableMeshes.Count < targetCount) extrudableMeshes.AddRange(new ExtrudableMesh[targetCount - extrudableMeshes.Count]);
            //If the deprecated sourceMesh is set, then migrate it to the new mesh array
            if (_sourceMesh != null)
            {
                Mesh[] newMeshes = new Mesh[_middleMeshes.Length + 1];
                _middleMeshes.CopyTo(newMeshes, 0);
                newMeshes[newMeshes.Length - 1] = _sourceMesh;
                _middleMeshes = newMeshes;
                extrudableMeshes.Add(new ExtrudableMesh(_sourceMesh, _axis));
                _sourceMesh = null;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
#if UNITY_5_4_OR_NEWER
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#else
                    UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
                }
#endif
            }

            for (int i = 0; i < _middleMeshes.Length; i++)
            {
                if (_middleMeshes[i] == null)
                {
                    RemoveMesh(i);
                    i--;
                    continue;
                }
                MirrorMethod lastMirror = MirrorMethod.None;
                if (extrudableMeshes[i + 2] != null)
                {
                    lastMirror = extrudableMeshes[i + 2].mirror;
                    extrudableMeshes[i + 2].Update(_middleMeshes[i], _axis);
                } else extrudableMeshes[i + 2] = new ExtrudableMesh(_middleMeshes[i], _axis);
                extrudableMeshes[i+2].mirror = lastMirror;
            }
            UpdateStartExtrudeMesh();
            UpdateEndExtrudeMesh();
        }

        void UpdateStartExtrudeMesh()
        {
            MirrorMethod lastMirror = MirrorMethod.None;
            if (extrudableMeshes[0] != null) lastMirror = extrudableMeshes[0].mirror;
            if (_startMesh != null) extrudableMeshes[0] = new ExtrudableMesh(_startMesh, _axis);
            else if (_middleMeshes.Length > 0)
            {
                if (_iteration == Iteration.Ordered) extrudableMeshes[0] = new ExtrudableMesh(_middleMeshes[0], _axis);
                else
                {
                    random = new System.Random(_randomSeed);
                    extrudableMeshes[0] = new ExtrudableMesh(_middleMeshes[random.Next(_middleMeshes.Length - 1)], _axis);
                }
            }
            if (extrudableMeshes[0] != null) extrudableMeshes[0].mirror = lastMirror;
        }

        void UpdateEndExtrudeMesh()
        {
            MirrorMethod lastMirror = MirrorMethod.None;
            lastMirror = MirrorMethod.None;
            if (extrudableMeshes[1] != null) lastMirror = extrudableMeshes[1].mirror;
            if (_endMesh != null) extrudableMeshes[1] = new ExtrudableMesh(_endMesh, _axis);
            else if (_middleMeshes.Length > 0)
            {
                if (_iteration == Iteration.Ordered) extrudableMeshes[1] = new ExtrudableMesh(_middleMeshes[_startMesh != null ? (iterations - 2) % _middleMeshes.Length : (iterations - 1) % _middleMeshes.Length], _axis);
                else
                {
                    random = new System.Random(_randomSeed);
                    for (int i = 0; i < iterations - 1; i++) random.Next(_middleMeshes.Length - 1);
                    extrudableMeshes[1] = new ExtrudableMesh(_middleMeshes[random.Next(_middleMeshes.Length - 1)], _axis);
                }
            }
            if (extrudableMeshes[1] != null) extrudableMeshes[1].mirror = lastMirror;
        }

        //Internal classes
        [System.Serializable]
        internal class ExtrudableMesh
        {
            [System.Serializable]
            public class VertexGroup
            {
                public float value;
                public int[] ids;

                public VertexGroup(float val, int[] vertIds)
                {
                    value = val;
                    ids = vertIds;
                }

                public void AddId(int id)
                {
                    int[] newIds = new int[ids.Length + 1];
                    ids.CopyTo(newIds, 0);
                    newIds[newIds.Length - 1] = id;
                    ids = newIds;
                }
            }
            [System.Serializable]
            public class Submesh
            {
                public int[] triangles = new int[0];

                public Submesh()
                {

                }

                public Submesh(int[] input)
                {
                    triangles = new int[input.Length];
                    input.CopyTo(triangles, 0);
                }
            }

            public Vector3[] vertices = new Vector3[0];
            public Vector3[] normals = new Vector3[0];
            public Vector4[] tangents = new Vector4[0];
            public Color[] colors = new Color[0];
            public Vector2[] uv = new Vector2[0];
            public List<Submesh> subMeshes = new List<Submesh>();
            public TS_Bounds bounds = new TS_Bounds(Vector3.zero, Vector3.zero);
            public List<VertexGroup> vertexGroups = new List<VertexGroup>();

            public MirrorMethod mirror
            {
                get { return _mirror;  }
                set
                {
                    if(_mirror != value)
                    {
                        Mirror(_mirror);
                        _mirror = value;
                        Mirror(_mirror);
                    }
                }
            }

            [SerializeField]
            private MirrorMethod _mirror = MirrorMethod.None;
            [SerializeField]
            private Axis _axis = Axis.Z;

            public ExtrudableMesh()
            {
                vertices = new Vector3[0];
                normals = new Vector3[0];
                tangents = new Vector4[0];
                colors = new Color[0];
                uv = new Vector2[0];
                subMeshes = new List<Submesh>();
                bounds = new TS_Bounds(Vector3.zero, Vector3.zero);
                vertexGroups = new List<VertexGroup>();
            }

            public ExtrudableMesh(Mesh inputMesh, Axis axis)
            {
                Update(inputMesh, axis);
            }

            public void Update(Mesh inputMesh, Axis axis)
            {
                vertices = inputMesh.vertices;
                normals = inputMesh.normals;
                tangents = inputMesh.tangents;
                colors = inputMesh.colors;
                uv = inputMesh.uv;
                bounds = new TS_Bounds(inputMesh.bounds);
                subMeshes.Clear();
                for (int i = 0; i < inputMesh.subMeshCount; i++) subMeshes.Add(new Submesh(inputMesh.GetTriangles(i)));
                _axis = axis;
                GroupVertices(axis);
            }

            private void Mirror(MirrorMethod method)
            {
                if (method == MirrorMethod.None) return;
                switch (method)
                {
                    case MirrorMethod.X:
                        for(int i = 0; i < vertices.Length; i++)
                        {
                            float percent = Mathf.InverseLerp(bounds.min.x, bounds.max.x, vertices[i].x);
                            vertices[i].x = Mathf.Lerp(bounds.min.x, bounds.max.x, 1f - percent);
                            normals[i].x = -normals[i].x;
                        }
                        if (_axis == Axis.X)
                        {
                            for (int i = 0; i < vertexGroups.Count; i++)
                            {
                                float percent = Mathf.InverseLerp(bounds.min.x, bounds.max.x, vertexGroups[i].value);
                                vertexGroups[i].value = Mathf.Lerp(bounds.min.x, bounds.max.x, 1f - percent);
                            }
                        }
                        break;
                    case MirrorMethod.Y:
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            float percent = Mathf.InverseLerp(bounds.min.y, bounds.max.y, vertices[i].y);
                            vertices[i].y = Mathf.Lerp(bounds.min.y, bounds.max.y, 1f - percent);
                            normals[i].y = -normals[i].y;
                        }
                        if (_axis == Axis.Y)
                        {
                            for (int i = 0; i < vertexGroups.Count; i++)
                            {
                                float percent = Mathf.InverseLerp(bounds.min.y, bounds.max.y, vertexGroups[i].value);
                                vertexGroups[i].value = Mathf.Lerp(bounds.min.y, bounds.max.y, 1f - percent);
                            }
                        }
                        break;
                    case MirrorMethod.Z:
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            float percent = Mathf.InverseLerp(bounds.min.z, bounds.max.z, vertices[i].z);
                            vertices[i].z = Mathf.Lerp(bounds.min.z, bounds.max.z, 1f - percent);
                            normals[i].z = -normals[i].z;
                        }
                        if (_axis == Axis.Z)
                        {
                            for (int i = 0; i < vertexGroups.Count; i++)
                            {
                                float percent = Mathf.InverseLerp(bounds.min.z, bounds.max.z, vertexGroups[i].value);
                                vertexGroups[i].value = Mathf.Lerp(bounds.min.z, bounds.max.z, 1f - percent);
                            }
                        }
                        break;
                }
                for (int i = 0; i < subMeshes.Count; i++)
                {
                    for (int n = 0; n < subMeshes[i].triangles.Length; n += 3)
                    {
                        int temp = subMeshes[i].triangles[n];
                        subMeshes[i].triangles[n] = subMeshes[i].triangles[n + 2];
                        subMeshes[i].triangles[n + 2] = temp;
                    }
                }
                CalculateTangents();
            }

            void GroupVertices(Axis axis)
            {
                vertexGroups = new List<VertexGroup>();
                int ax = (int)axis;
                if (ax > 2) ax -= 2;
                for (int i = 0; i < vertices.Length; i++)
                {
                    float value = 0f;
                    switch (ax)
                    {
                        case 0: value = vertices[i].x; break;
                        case 1: value = vertices[i].y; break;
                        case 2: value = vertices[i].z; break;
                    }
                    int index = FindInsertIndex(vertices[i], value);
                    if (index >= vertexGroups.Count) vertexGroups.Add(new VertexGroup(value, new int[] { i }));
                    else
                    {
                        if (Mathf.Approximately(vertexGroups[index].value, value)) vertexGroups[index].AddId(i);
                        else if (vertexGroups[index].value < value) vertexGroups.Insert(index, new VertexGroup(value, new int[] { i }));
                        else
                        {
                            if (index < vertexGroups.Count - 1) vertexGroups.Insert(index + 1, new VertexGroup(value, new int[] { i }));
                            else vertexGroups.Add(new VertexGroup(value, new int[] { i }));
                        }
                    }
                }
            }

            int FindInsertIndex(Vector3 pos, float value)
            {
                int lower = 0;
                int upper = vertexGroups.Count - 1;

                while (lower <= upper)
                {
                    int middle = lower + (upper - lower) / 2;
                    if (vertexGroups[middle].value == value) return middle;
                    else if (vertexGroups[middle].value < value) upper = middle - 1;
                    else lower = middle + 1;
                }
                return lower;
            }

            void CalculateTangents()
            {
                if (vertices.Length == 0)
                {
                    tangents = new Vector4[0];
                    return;
                }
                tangents = new Vector4[vertices.Length];
                Vector3[] tan1 = new Vector3[vertices.Length];
                Vector3[] tan2 = new Vector3[vertices.Length];
                for (int i = 0; i < subMeshes.Count; i++)
                {
                    for (int n = 0; n < subMeshes[i].triangles.Length; n += 3)
                    {
                        int i1 = subMeshes[i].triangles[n];
                        int i2 = subMeshes[i].triangles[n + 1];
                        int i3 = subMeshes[i].triangles[n + 2];
                        float x1 = vertices[i2].x - vertices[i1].x;
                        float x2 = vertices[i3].x - vertices[i1].x;
                        float y1 = vertices[i2].y - vertices[i1].y;
                        float y2 = vertices[i3].y - vertices[i1].y;
                        float z1 = vertices[i2].z - vertices[i1].z;
                        float z2 = vertices[i3].z - vertices[i1].z;
                        float s1 = uv[i2].x - uv[i1].x;
                        float s2 = uv[i3].x - uv[i1].x;
                        float t1 = uv[i2].y - uv[i1].y;
                        float t2 = uv[i3].y - uv[i1].y;
                        float div = s1 * t2 - s2 * t1;
                        float r = div == 0f ? 0f : 1f / div;
                        Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                        Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
                        tan1[i1] += sdir;
                        tan1[i2] += sdir;
                        tan1[i3] += sdir;
                        tan2[i1] += tdir;
                        tan2[i2] += tdir;
                        tan2[i3] += tdir;
                    }
                }
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 n = normals[i];
                    Vector3 t = tan1[i];
                    Vector3.OrthoNormalize(ref n, ref t);
                    tangents[i].x = t.x;
                    tangents[i].y = t.y;
                    tangents[i].z = t.z;
                    tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
                }
            }
        }

    }
}
