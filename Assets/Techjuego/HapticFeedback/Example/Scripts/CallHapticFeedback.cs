using UnityEngine;

namespace TechJuego.HapticFeedback
{
    public class CallHapticFeedback : MonoBehaviour
    {
        public static CallHapticFeedback Instance { get; private set; }

        [Header("Enable or disable vibration globally")]
        public bool VibrationEnabled = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Синглтон живет между сценами
        }

        public void HeavyHaptic()
        {
            if (VibrationEnabled)
                HapticCall.HeavyHaptic();
        }

        public void MediumHaptic()
        {
            if (VibrationEnabled)
                HapticCall.MediumHaptic();
        }

        public void LightHaptic()
        {
            if (VibrationEnabled)
                HapticCall.LightHaptic();
        }

        public void RigidHaptic()
        {
            if (VibrationEnabled)
                HapticCall.RigidHaptic();
        }

        public void SoftHaptic()
        {
            if (VibrationEnabled)
                HapticCall.SoftHaptic();
        }

        public void PerformSuccessFeedback()
        {
            if (VibrationEnabled)
                HapticCall.PerformSuccessFeedback();
        }

        public void PerformErrorFeedback()
        {
            if (VibrationEnabled)
                HapticCall.PerformErrorFeedback();
        }

        public void PerformWarningFeedback()
        {
            if (VibrationEnabled)
                HapticCall.PerformWarningFeedback();
        }
    }
}