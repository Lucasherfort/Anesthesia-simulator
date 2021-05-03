using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines
{

    [System.Serializable]
    public class TransformModule
    {
        public Vector2 offset;
        public Vector3 rotationOffset = Vector3.zero;
        public Vector3 baseScale = Vector3.one;
        public SplineResult splineResult
        {
            get
            {
                if (_splineResult == null) _splineResult = new SplineResult();
                return _splineResult;
            }
            set
            {
                if (_splineResult == null) _splineResult = new SplineResult(value);
                else _splineResult.CopyFrom(value);
            }
        }
        private SplineResult _splineResult;
        public CustomRotationModule customRotation = null;
        public CustomOffsetModule customOffset = null;

        public bool applyPositionX = true;
        public bool applyPositionY = true;
        public bool applyPositionZ = true;
        public Spline.Direction direction = Spline.Direction.Forward;
        public bool applyPosition
        {
            get
            {
                return applyPositionX || applyPositionY || applyPositionZ;
            }
            set
            {
                applyPositionX = applyPositionY = applyPositionZ = value;
            }
        }

        public bool applyRotationX = true;
        public bool applyRotationY = true;
        public bool applyRotationZ = true;
        public bool applyRotation
        {
            get
            {
                return applyRotationX || applyRotationY || applyRotationZ;
            }
            set
            {
                applyRotationX = applyRotationY = applyRotationZ = value;
            }
        }

        public bool applyScaleX = false;
        public bool applyScaleY = false;
        public bool applyScaleZ = false;
        public bool applyScale
        {
            get
            {
                return applyScaleX || applyScaleY || applyScaleZ;
            }
            set
            {
                applyScaleX = applyScaleY = applyScaleZ = value;
            }
        }
        //These are used to save allocations
        private static Vector3 position = Vector3.zero;
        private static Quaternion rotation = Quaternion.identity;

        public void ApplyTransform(Transform input)
        {
            input.position = GetPosition(input.position);
            input.rotation = GetRotation(input.rotation);
            input.localScale = GetScale(input.localScale);
        }

        public void ApplyRigidbody(Rigidbody input)
        {
            input.transform.localScale = GetScale(input.transform.localScale);
            input.MovePosition(GetPosition(input.position));
            Vector3 velocity = input.velocity;
            if (applyPositionX) velocity.x = 0f;
            if (applyPositionY) velocity.y = 0f;
            if (applyPositionZ) velocity.z = 0f;
            input.velocity = velocity;
            input.MoveRotation(GetRotation(input.rotation));
            velocity = input.angularVelocity;
            if (applyRotationX) velocity.x = 0f;
            if (applyRotationY) velocity.y = 0f;
            if (applyRotationZ) velocity.z = 0f;
            input.angularVelocity = velocity;
        }

        public void ApplyRigidbody2D(Rigidbody2D input)
        {
            input.transform.localScale = GetScale(input.transform.localScale);
            input.position = GetPosition(input.position);
            Vector2 velocity = input.velocity;
            if (applyPositionX) velocity.x = 0f;
            if (applyPositionY) velocity.y = 0f;
            input.velocity = velocity;
            input.rotation = -GetRotation(Quaternion.Euler(0f, 0f, input.rotation)).eulerAngles.z;
            if (applyRotationX) input.angularVelocity = 0f;
        }

        private Vector3 GetPosition(Vector3 inputPosition)
        {
            position = _splineResult.position;
            Vector2 finalOffset = offset;
            if (customOffset != null) finalOffset += customOffset.Evaluate(_splineResult.percent);
            if (finalOffset != Vector2.zero) position += _splineResult.right * finalOffset.x * _splineResult.size + _splineResult.normal * finalOffset.y * _splineResult.size;
            if (applyPositionX) inputPosition.x = position.x;
            if (applyPositionY) inputPosition.y = position.y;
            if (applyPositionZ) inputPosition.z = position.z;
            return inputPosition;
        }

        private Quaternion GetRotation(Quaternion inputRotation)
        {
            rotation = Quaternion.LookRotation(_splineResult.direction * (direction == Spline.Direction.Forward ? 1f : -1f), _splineResult.normal);
            if (rotationOffset != Vector3.zero) rotation = rotation * Quaternion.Euler(rotationOffset);
            if (customRotation != null) rotation = customRotation.Evaluate(rotation, _splineResult.percent);
            if (!applyRotationX || !applyRotationY)
            {
                Vector3 euler = rotation.eulerAngles;
                if (!applyRotationX) euler.x = inputRotation.eulerAngles.x;
                if (!applyRotationY) euler.y = inputRotation.eulerAngles.y;
                if (!applyRotationZ) euler.z = inputRotation.eulerAngles.z;
                inputRotation.eulerAngles = euler;
            }
            else inputRotation = rotation;
            return inputRotation;
        }

        private Vector3 GetScale(Vector3 inputScale)
        {
            if (applyScaleX) inputScale.x = baseScale.x * _splineResult.size;
            if (applyScaleY) inputScale.y = baseScale.y * _splineResult.size;
            if (applyScaleZ) inputScale.z = baseScale.z * _splineResult.size;
            return inputScale;
        }
    }
}
