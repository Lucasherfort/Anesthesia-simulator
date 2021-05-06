using UnityEngine;
using System.Collections;

namespace Dreamteck.Splines
{
    [AddComponentMenu("Dreamteck/Splines/Spline Positioner")]
    public class SplinePositioner : SplineUser
    {
        public enum Mode { Percent, Distance }
        
        public Transform applyTransform
        {
            get
            {
                if (_applyTransform == null) return this.transform;
                return _applyTransform;
            }

            set
            {
                if(value != _applyTransform)
                {
                    _applyTransform = value;
                    if (value != null) Rebuild(false);
                }
            }
        }
        public double position
        {
            get
            {
                return _position;
            }
            set
            {
                if (value != _position)
                {
                    animPosition = (float)value;
                    _position = value;
                    Rebuild(false);
                }
            }
        }

        public Mode mode
        {
            get { return _mode;  }
            set
            {
                if (value != _mode)
                {
                    _mode = value;
                    Rebuild(false);
                }
            }
        }

        public Spline.Direction direction
        {
            get { return _direction; }
            set
            {
                if (value != _direction)
                {
                    _direction = value;
                    Rebuild(false);
                }
            }
        }

        /// <summary>
        /// Returns the evaluation result at the current position
        /// </summary>
        public SplineResult positionResult
        {
            get { return _positionResult; }
        }

        /// <summary>
        /// Returns the offsetted evaluation result at the current position. 
        /// </summary>
        public SplineResult offsettedPositionResult
        {
            get
            {
                SplineResult offsetted = new SplineResult(_positionResult);
                offsetted.position += offsetted.right * motion.offset.x + offsetted.normal * motion.offset.y;
                //offsetted.direction = _customDireciton.Evaluate(offsetted.direction, offsetted.percent);
                offsetted.direction = Quaternion.Euler(motion.rotationOffset) * offsetted.direction;
                offsetted.normal = Quaternion.Euler(motion.rotationOffset) * offsetted.normal;
                offsetted.normal = Quaternion.Euler(motion.rotationOffset) * offsetted.normal;
                return offsetted;
            }
        }

        [SerializeField]
        [HideInInspector]
        private Transform _applyTransform;
        [SerializeField]
        [HideInInspector]
        private double _position = 0.0;
        [SerializeField]
        [HideInInspector]
        private float animPosition = 0f;
        [SerializeField]
        [HideInInspector]
        private Mode _mode = Mode.Percent;
        [SerializeField]
        [HideInInspector]
        private Spline.Direction _direction = Spline.Direction.Forward;
        [SerializeField]
        [HideInInspector]
        private SplineResult _positionResult = null;


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

        [System.Obsolete("Deprecated in version 1.0.7. Use motion.applyScale instead")]
        public bool applyScale
        {
            get { return motion.applyScale; }
            set { motion.applyScale = value; }
        }


        public TransformModule motion
        {
            get
            {
                if (_motion == null) _motion = new TransformModule();
                return _motion;
            }
        }
        [SerializeField]
        [HideInInspector]
        private TransformModule _motion = new TransformModule();

        public CustomRotationModule customRotations
        {
            get
            {
                if (_customRotations == null) _customRotations = new CustomRotationModule();
                return _customRotations;
            }
        }
        [SerializeField]
        [HideInInspector]
        private CustomRotationModule _customRotations = new CustomRotationModule();

        public CustomOffsetModule customOffsets
        {
            get
            {
                if (_customOffsets == null) _customOffsets = new CustomOffsetModule();
                return _customOffsets;
            }
        }
        [SerializeField]
        [HideInInspector]
        private CustomOffsetModule _customOffsets = new CustomOffsetModule();

        void Start()
        {
            //Write initialization code here
        }

        protected override void LateRun()
        {
            base.LateRun();
            //Code to run every Update/FixedUpdate/LateUpdate
        }

        protected override void OnDidApplyAnimationProperties()
        {
            if (animPosition != _position) position = animPosition;
            base.OnDidApplyAnimationProperties();
        }

        protected override void Build()
        {
            base.Build();
            double percent = _position;
            if (mode == Mode.Distance)
            {
                percent = 1.0;
                double p = clipFrom;
                double prevP = p;
                float distance = 0f;
                while (true)
                {
                    Vector3 prev = EvaluatePosition(p);
                    p = DMath.Move(p, clipTo, _address.root.moveStep);
                    Vector3 current = EvaluatePosition(p);
                    float distAdd = Vector3.Distance(current, prev);
                    distance += distAdd;
                    if (distance >= _position)
                    {
                        percent = DMath.Lerp(prevP, p, Mathf.InverseLerp(distance - distAdd, distance, (float)_position));
                        break;
                    }
                    prevP = p;
                    if (p == clipTo) break;
                }
            } else percent = DMath.Lerp(clipFrom, clipTo, _position);
            Evaluate(_positionResult, percent);
        }

        private void ApplyMotion()
        {
            motion.splineResult = _positionResult;
            motion.customRotation = _customRotations;
            motion.customOffset = _customOffsets;
            motion.direction = direction;
            motion.ApplyTransform(transform);
        }

        protected override void PostBuild()
        {
            base.PostBuild();
            if (_positionResult == null) return;
            ApplyMotion();
        }

    }
}
