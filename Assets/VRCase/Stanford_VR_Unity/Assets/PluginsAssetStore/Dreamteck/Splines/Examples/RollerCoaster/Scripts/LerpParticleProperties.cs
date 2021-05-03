using UnityEngine;
using System.Collections;

namespace Dreamteck.Splines.Examples
{
    public class LerpParticleProperties : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float percent = 0f;
        public float startEmission = 0f;
        public float endEmission = 0f;
        public float minSize = 0f;
        public float maxSize = 0f;
        ParticleSystem particleSys;
        public float idealPercent = 0f;
        public float percentSpeed = 0f;

        // Use this for initialization
        void Start()
        {
            particleSys = GetComponent<ParticleSystem>();
        }

        public void SetIdealValue(float value)
        {
            idealPercent = value;
        }

        // Update is called once per frame
        void Update()
        {
            if (percentSpeed > 0f) percent = Mathf.MoveTowards(percent, idealPercent, percentSpeed * Time.deltaTime);
#if UNITY_5_5_OR_NEWER
            ParticleSystem.EmissionModule emission = particleSys.emission;
            ParticleSystem.MinMaxCurve newRate = new ParticleSystem.MinMaxCurve();
            newRate.constantMax = Mathf.Lerp(startEmission, endEmission, percent);
            emission.rateOverTime = newRate;
            ParticleSystem.MinMaxCurve newSize = particleSys.main.startSize;
            newSize.constantMax = Mathf.Lerp(minSize, maxSize, percent);
#elif UNITY_5_3 || UNITY_5_3_OR_NEWER
            ParticleSystem.EmissionModule emission = particleSys.emission;
            ParticleSystem.MinMaxCurve newRate = new ParticleSystem.MinMaxCurve();
            newRate.constantMax = Mathf.Lerp(startEmission, endEmission, percent);
            emission.rate = newRate;
            particleSys.startSize = Mathf.Lerp(minSize, maxSize, percent);
#else
            particleSys.emissionRate = Mathf.Lerp(startEmission, endEmission, percent);
            particleSys.startSize = Mathf.Lerp(minSize, maxSize, percent);
#endif

        }
    }
}
