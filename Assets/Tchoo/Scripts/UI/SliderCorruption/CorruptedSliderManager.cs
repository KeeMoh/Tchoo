using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace sliderCorruption
{
    public class CorruptedSliderManager : MonoBehaviour
    {
        [SerializeField] private SliderCorruptionElement headSlider;
        [SerializeField] private List<SliderCorruptionElement> extraSliders;

        private List<SliderCorruptionElement> unlockedSegments = new();

        private float totalMin;
        private float totalMax;
        public float currentAmount;

        private void Start()
        {
            SetupInitialSlider();
        }

        private void SetupInitialSlider()
        {
            headSlider.Min = - headSlider.SegmentValue;
            headSlider.Max = headSlider.SegmentValue;
            headSlider.SetUnlocked(true);
            unlockedSegments.Add(headSlider);
            UpdateGlobalRange();
        }

        private void UpdateGlobalRange()
        {
            totalMin = unlockedSegments.Min(s => s.Min);
            totalMax = unlockedSegments.Max(s => s.Max);
        }

        public float GetTotalRange() => totalMax - totalMin;

        [Button]
        public void UnlockNextSegment()
        {
            // Find next locked pair: one negative and one positive
            Debug.Log(extraSliders.Count);
            var positive = extraSliders
                .Where(s => !s.IsUnlocked && s.IsPositive)
                .OrderBy(s => s.Index).FirstOrDefault();

            var negative = extraSliders
                .Where(s => !s.IsUnlocked && !s.IsPositive)
                .OrderBy(s => s.Index).FirstOrDefault();

            if (positive != null && negative != null)
            {
                Debug.Log("Unlock two segments");
                UnlockSegment(negative, totalMin - negative.SegmentValue, totalMin);
                UnlockSegment(positive, totalMax, totalMax + positive.SegmentValue);
                UpdateGlobalRange();
            }
            else
            {
                Debug.Log("Unlock error");
            }
        }


        private void UnlockSegment(SliderCorruptionElement slider, float min, float max)
        {
            //if (slider.IsPositive)
            //{
                slider.Min = min;
                slider.Max = max;
            //}
            //else
            //{ //If slider is negative, inverse min and max
            //    slider.Min = min;
            //    slider.Max = min;
            //}
            slider.SetUnlocked(true);
            unlockedSegments.Add(slider);
        }

        [Button]
        public void SetGlobalValueAtPlus1()
        {
            SetGlobalValue(currentAmount + 0.37f, 0.3f);
        }

        [Button]
        public void SetGlobalValueAtMinus1()
        {
            SetGlobalValue(currentAmount - 0.25f, 0.6f);
        }

        public void SetGlobalValue(float value, float duration = 0.3f)
        {
            float clampedValue = Mathf.Clamp(value, totalMin, totalMax);
            currentAmount = clampedValue;

            foreach (var slider in unlockedSegments)
            {
                if (slider == headSlider)
                {
                    slider.SetHeadValue(clampedValue, duration);
                    slider.PlayGorgement();
                    continue;
                }

                if (slider.IsFullyCoveredBy(clampedValue))
                {
                    slider.SetSliderFullValue(duration);
                }
                else if (!slider.IsInRange(clampedValue))
                {
                    slider.ResetSlider(duration);
                    slider.PlayDrain();
                }
                else
                {
                    slider.SetSliderValue(clampedValue, duration);
                    slider.PlayGorgement();
                }
            }
        }

    }
}