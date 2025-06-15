using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace sliderCorruption
{
    public class SliderCorruptionElement : MonoBehaviour
    {
        [SerializeField] private int index;
        [SerializeField] private Image fillImage;
        [SerializeField] private float segmentValue = 1f;
        [SerializeField] private bool isUnlocked = false;
        [SerializeField] private bool isPositive = true;
        //[SerializeField] private Color baseColor = Color.white;
        //[SerializeField] private Color fillColor = Color.red;        
        [SerializeField] private Sprite CorruptedHead;
        [SerializeField] private Sprite SanityHead;        
        private Slider slider;


        public float SegmentValue => segmentValue;
        public int Index => index;
        public bool IsUnlocked => isUnlocked;
        public bool IsPositive => isPositive;
        public float Min { get; set; } // Assigned by manager
        public float Max { get; set; } // Assigned by manager

        private void Start()
        {
            slider = GetComponent<Slider>();
            slider.value = 0;
        }

        private void OnEnable()
        {
            //fillImage.color = baseColor;
        }

        public void SetHeadValue(float globalValue, float duration)
        {
            fillImage.sprite = globalValue < 0f ? CorruptedHead : SanityHead;
            float absValue = Mathf.Clamp(Mathf.Abs(globalValue), 0f, Max); // Toujours positif
            float fillAmount = absValue / Max; // Normalisation sur la plage Max (1, ou 3 si étendu)

            slider.DOValue(fillAmount, duration).SetEase(Ease.OutQuad);
        }

        public void PlayGorgement()
        {
            if (fillImage == null) return;

            //fillImage.color = baseColor;
            //fillImage.DOColor(fillColor, 0.15f);
            //transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, vibrato: 1, elasticity: 0.7f);
        }

        public void PlayDrain()
        {
            if (fillImage == null) return;

            //fillImage.color = baseColor;
            //transform.DOShakeScale(0.3f, strength: 0.2f, vibrato: 3);
        }

        public void SetUnlocked(bool value)
        {
                Debug.Log("Unlock slider " + gameObject.name);

            isUnlocked = value;
            gameObject.SetActive(value);
        }

        public void SetSliderValue(float value, float duration)
        {
            //Debug.Log("value> " + value + " min> " + Min + " max>" + Max);
            float normalized;
            normalized = isPositive ? Mathf.Clamp01(Mathf.Abs(value - Min) / Mathf.Abs(Max - Min))
                : Mathf.Clamp01(Mathf.Abs(value - Max) / Mathf.Abs(Max - Min));
            slider.DOValue(normalized, duration).SetEase(Ease.OutQuad);
        }

        public void SetSliderFullValue(float duration)
        {
            if(isPositive)
            {
                SetSliderValue(Max, duration);
            }
            else
            {
                SetSliderValue(Min, duration);
            }
        }

        public void ResetSlider(float duration)
        {
            slider.DOValue(0f, duration).SetEase(Ease.OutQuad);
        }

        public bool IsFullyCoveredBy(float value)
        {
            if (Max < 0f) // Slider négatif
                return value <= Min;
            else          // Slider positif
                return value >= Max;
        }

        public bool IsInRange(float value)
        {
            return value >= Min && value <= Max;
        }
    }
}