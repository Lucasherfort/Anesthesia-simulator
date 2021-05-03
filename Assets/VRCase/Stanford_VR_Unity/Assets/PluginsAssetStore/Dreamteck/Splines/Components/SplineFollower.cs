using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEngine.Serialization;
#endif
namespace Dreamteck.Splines
{
    public delegate void SplineReachHandler();
    [AddComponentMenu("Dreamteck/Splines/Spline Follower")]
    public class SplineFollower : SplineUser
    {
        public enum FollowMode { Uniform, Time }
        public enum Wrap { Default, Loop, PingPong }
        public enum PhysicsMode { Transform, Rigidbody, Rigidbody2D }
        [HideInInspector]
        public Wrap wrapMode = Wrap.Default;
        [HideInInspector]
        public FollowMode followMode = FollowMode.Uniform;
        public PhysicsMode physicsMode
        {
            get { return _physicsMode; }
            set
            {
                switch (value)
                {
                    case PhysicsMode.Rigidbody:
                        rb = GetComponent<Rigidbody>();
                        break;
                    case PhysicsMode.Rigidbody2D:
                        rb2d = GetComponent<Rigidbody2D>();
                        break;
                }
                _physicsMode = value;
            }
        }

        [HideInInspector]
#if UNITY_EDITOR
        [FormerlySerializedAs("startPercent")]
#endif
        public double startPosition = 0.0;
        [HideInInspector]
#if UNITY_EDITOR
        [FormerlySerializedAs("findStartPoint")]
#endif
        public bool autoStartPosition = false;
        [HideInInspector]
        public SplineTrigger[] triggers = new SplineTrigger[0];
        public TransformModule motion
        {
            get {
                if (_motion == null) _motion = new TransformModule();
                return _motion;
            }
        }

        public CustomRotationModule customRotations
        {
            get
            {
                if (_customRotations == null) _customRotations = new CustomRotationModule();
                return _customRotations;
            }
        }

        public CustomOffsetModule customOffsets
        {
            get
            {
                if (_customOffsets == null) _customOffsets = new CustomOffsetModule();
                return _customOffsets;
            }
        }
        [HideInInspector]
        public bool autoFollow = true;
        /// <summary>
        /// Used when follow mode is set to Uniform. Defines the speed of the follower
        /// </summary>
        public float followSpeed
        {
            get { return _followSpeed; }
            set
            {
                if (_followSpeed != value)
                {
                    if (value < 0f) value = 0f;
                    _followSpeed = value;
                }
            }
        }

        /// <summary>
        /// Used when follow mode is set to Time. Defines how much time it takes for the follower to travel through the path
        /// </summary>
        public float followDuration
        {
            get { return _followDuration; }
            set
            {
                if (_followDuration != value)
                {
                    if (value < 0f) value = 0f;
                    _followDuration = value;
                }
            }
        }

        private SplineResult _followResult = new SplineResult();
        [HideInInspector]
        public Spline.Direction direction = Spline.Direction.Forward;

        /// <summary>
        /// Returns the evaluation result from the current follow position
        /// </summary>
        public SplineResult followResult
        {
            get { return _followResult; }
        }

        /// <summary>
        /// Returns the offsetted evaluation result from the current follow position
        /// </summary>
        public SplineResult offsettedFollowResult
        {
            get {
                SplineResult offsetted = new SplineResult(_followResult);
                offsetted.position += offsetted.right * motion.offset.x + offsetted.normal * motion.offset.y;
                //offsetted.direction = _customDireciton.Evaluate(offsetted.direction, offsetted.percent);
                offsetted.direction =  Quaternion.Euler(motion.rotationOffset) * offsetted.direction;
                offsetted.normal = Quaternion.Euler(motion.rotationOffset) * offsetted.normal;
                return offsetted;
            }
        }
        public event SplineReachHandler onEndReached;
        public event SplineReachHandler onBeginningReached;

        private bool percentSet = false;
        private Rigidbody rb;
        private Rigidbody2D rb2d;
        [SerializeField]
        [HideInInspector]
        private float _followSpeed = 1f;
        [SerializeField]
        [HideInInspector]
        private float _followDuration = 1f;
        [SerializeField]
        [HideInInspector]
#if UNITY_EDITOR
        [FormerlySerializedAs("_customDireciton")]
#endif
        private CustomRotationModule _customRotations = new CustomRotationModule();
        [SerializeField]
        [HideInInspector]
        private CustomOffsetModule _customOffsets = new CustomOffsetModule();
        [SerializeField]
        [HideInInspector]
        private PhysicsMode _physicsMode = PhysicsMode.Transform;
        [SerializeField]
        [HideInInspector]
        private TransformModule _motion = new TransformModule();

        //Obsolete properties
        [System.Obsolete("Deprecated in 1.0.7. Use startPosition instead")]
        public double startPercent
        {
            get { return startPosition; }
            set { startPosition = value; }
        }
        [System.Obsolete("Deprecated in 1.0.7. Use autoStartPosition instead")]
        public bool findStartPoint
        {
            get { return autoStartPosition; }
            set { autoStartPosition = value; }
        }

        [System.Obsolete("Deprecated in version 1.0.7. Use motion.applyPosition instead")]
        public bool applyPosition
        {
            get { return motion.applyPosition; }
            set { motion.applyPosition = value; }
        }

        [System.Obsolete("Deprecated in version 1.0.7. Use motion.applyRotation instead")]
        public bool applyRotation
        {
            get { return motion.applyRotation; }
            set { motion.applyRotation = value; }
        }
        [HideInInspector]
        public bool applyDirectionRotation = true;

        [System.Obsolete("Deprecated in version 1.0.7. User motion.applyScale instead")]
        public bool applyScale
        {
            get { return motion.applyScale; }
            set { motion.applyScale = value; }
        }

        [System.Obsolete("Deprecated in version 1.0.7. User motion.offset instead")]
        public Vector2 offset
        {
            get { return motion.offset; }
            set { motion.offset = value; }
        }

        [System.Obsolete("Deprecated in version 1.0.7. User motion.rotationOffset instead")]
        public Vector3 rotationOffset
        {
            get { return motion.rotationOffset; }
            set { motion.rotationOffset = value; }
        }


        // Use this for initialization
        void Start()
        {
            if (autoFollow)
            {
                if (percentSet) return;
                Restart();
            }
        }

        protected override void LateRun()
        {
            base.LateRun();
            if (autoFollow) AutoFollow();
        }

        protected override void PostBuild()
        {
            base.PostBuild();
            if (samples.Length == 0) return;
#if UNITY_EDITOR
            if (!Application.isPlaying || _followResult == null)  Evaluate(_followResult, startPosition);
            else Evaluate(_followResult, _followResult.percent);
#else
            _followResult = Evaluate(_followResult.percent);
#endif
            if (autoFollow && !autoStartPosition) ApplyMotion();
        }

        void AutoFollow()
        {
            switch (followMode)
            {
                case FollowMode.Uniform: Move(Time.deltaTime * _followSpeed); break;
                case FollowMode.Time: 
                    if(_followDuration == 0.0) Move(0.0);
                    else Move((double)(Time.deltaTime / _followDuration)); break;
            }
            
        }

        private void ApplyMotion()
        {
            if (_followResult == null) return;
            motion.splineResult = _followResult;
            motion.customRotation = _customRotations;
            motion.customOffset = _customOffsets;
            if (applyDirectionRotation) motion.direction = direction;
            else motion.direction = Spline.Direction.Forward;
            switch (_physicsMode)
            {
                case PhysicsMode.Transform: motion.ApplyTransform(transform); break;
                case PhysicsMode.Rigidbody:
                    if (rb == null)
                    {
                        rb = GetComponent<Rigidbody>();
                        if (rb == null) throw new MissingComponentException("There is no Rigidbody attached to " + name + " but the Physics mode is set to use one.");
                    }
                    motion.ApplyRigidbody(rb);
                    break;
                case PhysicsMode.Rigidbody2D:
                    if (rb2d == null)
                    {
                        rb2d = GetComponent<Rigidbody2D>();
                        if (rb2d == null) throw new MissingComponentException("There is no Rigidbody2D attached to " + name + " but the Physics mode is set to use one.");

                    }
                    motion.ApplyRigidbody2D(rb2d);
                    break;
            }
        }

        /// <summary>
        /// Get the available node in front or behind the follower
        /// </summary>
        /// <returns></returns>
        public Node GetNextNode()
        {
            SplineComputer comp;
            double evaluatePercent = 0.0;
            Spline.Direction dir = Spline.Direction.Forward;
            _address.GetEvaluationValues(_followResult.percent, out comp, out evaluatePercent, out dir);
            if (direction == Spline.Direction.Backward)
            {
                if (dir == Spline.Direction.Forward) dir = Spline.Direction.Backward;
                else dir = Spline.Direction.Forward;
            }
            int[] links = comp.GetAvailableNodeLinksAtPosition(evaluatePercent, dir);
            if (links.Length == 0) return null;
            //Find the closest one
            if (dir == Spline.Direction.Forward)
            {
                int min = comp.pointCount-1;
                int index = 0;
                for (int i = 0; i < links.Length; i++)
                {
                    if (comp.nodeLinks[links[i]].pointIndex < min)
                    {
                        min = comp.nodeLinks[links[i]].pointIndex;
                        index = i;
                    }
                }
                return comp.nodeLinks[links[index]].node;
            }
            else
            {
                int max = 0;
                int index = 0;
                for (int i = 0; i < links.Length; i++)
                {
                    if (comp.nodeLinks[links[i]].pointIndex > max)
                    {
                        max = comp.nodeLinks[links[i]].pointIndex;
                        index = i;
                    }
                }
                return comp.nodeLinks[links[index]].node;
            }
        }

        /// <summary>
        /// Get the current computer the follower is on at the moment
        /// </summary>
        /// <returns></returns>
        public void GetCurrentComputer(out SplineComputer comp, out double percent, out Spline.Direction dir)
        {
            _address.GetEvaluationValues(_followResult.percent, out comp, out percent, out dir);
        }

        public override void EnterAddress(Node node, int pointIndex, Spline.Direction direction = Spline.Direction.Forward)
        {
            int element = _address.GetElementIndex(_followResult.percent);
            double localPercent = _address.PathToLocalPercent(_followResult.percent, element);
            base.EnterAddress(node, pointIndex, direction);
            double newPercent = _address.LocalToPathPercent(localPercent, element);
            SetPercent(newPercent);
            percentSet = false;
        }

        public override void ExitAddress(int depth)
        {
            int element = _address.GetElementIndex(_followResult.percent);
            double localPercent = _address.PathToLocalPercent(_followResult.percent, element);
            base.ExitAddress(depth);
            double newPercent = _address.LocalToPathPercent(localPercent, element);
            SetPercent(newPercent);
            percentSet = false;
        }

        public void Restart()
        {
            if (autoStartPosition) SetPercent(Project(this.transform.position, clipFrom, clipTo).percent);
            else SetPercent(startPosition);
            percentSet = false;
        }

        public void SetPercent(double percent)
        {
            percentSet = true;
            percent = DMath.Clamp01(percent);
            Evaluate(_followResult, percent);
            ApplyMotion();
        }

        public void SetDistance(float distance)
        {
            Evaluate(_followResult, 0.0);
            Spline.Direction dir = direction;
            direction = Spline.Direction.Forward;
            Move(distance);
            direction = dir;
        }

        private void CheckTriggers(double prevPercent, double curPercent)
        {
            for(int i = 0; i < triggers.Length; i++)
            {
                if (triggers[i] == null) continue;
                if(clipFrom <= triggers[i].position && clipTo >= triggers[i].position) triggers[i].Check(prevPercent, curPercent);
            }
        }

        public void Move(double percent)
        {
			if(percent == 0.0) return;
            if (samples.Length <= 1)
            {
                if (samples.Length == 1)
                {
                    _followResult.CopyFrom(samples[0]);
                    ApplyMotion();
                }
                return;
            }
            Evaluate(_followResult, followResult.percent);
            double startPercent = followResult.percent;
            double p = startPercent + (direction == Spline.Direction.Forward ? percent : -percent);
            double trace1 = DMath.Clamp01(p);
            bool callOnEndReached = false, callOnBeginningReached = false;
            if(p > 1.0)
            {
                if (onEndReached != null && !Mathf.Approximately((float)_followResult.percent, (float)trace1)) callOnEndReached = true;
                switch (wrapMode)
                {
                    case Wrap.Default:
                        p = trace1; 
                        CheckTriggers(startPercent, p); 
                        break;
                    case Wrap.Loop:
                        while (p > 1.0) p -= 1.0;
                        CheckTriggers(0.0, p);
                        break;
                    case Wrap.PingPong:
                        p = DMath.Clamp01(1.0-(p-1.0));
                        direction = Spline.Direction.Backward;
                        CheckTriggers(1.0, p);
                        break;
                }
            } else if(p < 0.0)
            {
                if (onBeginningReached != null && !Mathf.Approximately((float)_followResult.percent, (float)trace1)) callOnBeginningReached = true;
                switch (wrapMode)
                {
                    case Wrap.Default:
                        p = trace1; 
                        CheckTriggers(startPercent, p); 
                        break;
                    case Wrap.Loop:
                        while (p < 0.0) p += 1.0;
                        CheckTriggers(1.0, p);
                        break;
                    case Wrap.PingPong:
                        p = DMath.Clamp01(-p);
                        direction = Spline.Direction.Forward;
                        CheckTriggers(0.0, p);
                        break;
                }
            } else CheckTriggers(startPercent, p);
            Evaluate(_followResult, p);
            ApplyMotion();
            if (callOnEndReached) onEndReached();
            else if (callOnBeginningReached) onBeginningReached();
        }

        public void Move(float distance)
        {
            if (distance < 0f) distance = 0f;
			if(distance == 0f) return;
            if (samples.Length <= 1)
            {
                if (samples.Length == 1)
                {
                    _followResult.CopyFrom(samples[0]);
                    ApplyMotion();
                }
                return;
            }
            bool callOnEndReached = false, callOnBeginningReached = false;
            Evaluate(_followResult, followResult.percent);
            SplineResult lastResult = new SplineResult(_followResult);
            double tracePercent = _followResult.percent;
            SplineResult traceResult = new SplineResult(_followResult);
            double traceTriggersFrom = traceResult.percent;
            float moved = 0f;
            while (moved < distance)
            {
                int resultIndex = GetSampleIndex(traceResult.percent);
                if (direction == Spline.Direction.Forward)
                {
                    if (tracePercent == clipTo)
                    {
                        if (onEndReached != null && !Mathf.Approximately((float)_followResult.percent, (float)clipTo)) callOnEndReached = true;
                        CheckTriggers((float)traceTriggersFrom, traceResult.percent);
                        if (wrapMode == Wrap.Default)
                        {
                            _followResult .CopyFrom(traceResult);
                            break;
                        } else if (wrapMode == Wrap.Loop)
                        {
                            Evaluate(traceResult, clipFrom);
                            traceTriggersFrom = traceResult.percent;
                            resultIndex = GetSampleIndex(clipFrom);
                        } else if (wrapMode == Wrap.PingPong)
                        {
                            direction = Spline.Direction.Backward;
                            lastResult.CopyFrom(traceResult);
                            traceTriggersFrom = traceResult.percent;
                            continue;
                        }
                    }
                    double nextPercent = (double)(resultIndex + 1) / (samples.Length - 1);
                    if (nextPercent <= tracePercent) nextPercent = (double)(resultIndex + 2) / (samples.Length - 1);
                    if (nextPercent > clipTo) nextPercent = clipTo;
                    tracePercent = nextPercent;
                }
                else
                {
                    if (tracePercent == clipFrom)
                    {
                        CheckTriggers((float)traceTriggersFrom, traceResult.percent);
                        if (onBeginningReached != null && !Mathf.Approximately((float)_followResult.percent, (float)clipFrom)) callOnBeginningReached = true;
                        if (wrapMode == Wrap.Default)
                        {
                            _followResult.CopyFrom(traceResult);
                            break;
                        }
                        if (wrapMode == Wrap.Loop)
                        {
                            Evaluate(traceResult, clipTo);
                            traceTriggersFrom = traceResult.percent;
                            resultIndex = GetSampleIndex(clipTo);
                        }
                        if (wrapMode == Wrap.PingPong)
                        {
                            direction = Spline.Direction.Forward;
                            lastResult.CopyFrom(traceResult);
                            traceTriggersFrom = traceResult.percent;
                            continue;
                        }
                    }
                    double nextPercent = (double)resultIndex / (samples.Length - 1);
                    if (nextPercent >= tracePercent) nextPercent = (double)(resultIndex - 1) / (samples.Length - 1);
                    if (nextPercent < clipFrom) nextPercent = clipFrom;
                    tracePercent = nextPercent;
                }
                lastResult.CopyFrom(traceResult);
                Evaluate(traceResult, tracePercent);
                float traveled = (traceResult.position - lastResult.position).magnitude;
                moved += traveled;
                if (moved >= distance)
                {
                    float excess = moved - distance;
                    double lerpPercent = 1.0 - excess / traveled;
                    if (direction == Spline.Direction.Backward && !averageResultVectors)
                    {
                        traceResult.direction = samples[Mathf.Max(resultIndex - 1, 0)].direction;
                        float directionLerp = (float)lastResult.percent * (samples.Length - 1)-resultIndex;
                        lastResult.direction = Vector3.Slerp(samples[resultIndex].direction, traceResult.direction, 1f - directionLerp);
                    }
                    SplineResult.Lerp(lastResult, traceResult, lerpPercent, _followResult);
                    CheckTriggers((float)traceTriggersFrom, _followResult.percent);
                    break;
                }
            }
            ApplyMotion();
            if (callOnEndReached) onEndReached();
            else if (callOnBeginningReached) onBeginningReached();
        }

        private void AddTrigger(SplineTrigger trigger)
        {
            SplineTrigger[] newTriggers = new SplineTrigger[triggers.Length + 1];
            triggers.CopyTo(newTriggers, 0);
            newTriggers[newTriggers.Length - 1] = trigger;
            triggers = newTriggers;
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction call, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<int> call, int value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<float> call, float value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<double> call, double value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<string> call, string value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<bool> call, bool value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<GameObject> call, GameObject value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<Transform> call, Transform value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

    }
}
