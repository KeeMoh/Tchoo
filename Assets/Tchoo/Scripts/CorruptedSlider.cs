using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CorruptedSlider : MonoBehaviour
{    
    [SerializeField] private PlayerControllerOLD controller;
    //[SerializeField] private Slider _sliderCorruption;
    //[SerializeField] private Slider _sliderSanity;
    //[SerializeField] private Slider[] _sliderSanity;    
    //[SerializeField] private Slider[] _sliderCorruption;
    [SerializeField] private Slider _slider;
    [SerializeField] private Image fillImg;
    //[SerializeField] private Sprite spriteSanity;
    //[SerializeField] private Sprite spriteCorruption;    
    [SerializeField] private Gradient gradientSanity;
    [SerializeField] private Gradient gradientCorruption;
    [SerializeField] private TextMeshProUGUI debugCorruptionValue;
    //private Image imgCorruption;
    //private Image imgSanity;
    //[SerializeField] private ParticleSystem Fx_Activation;

    //[SerializeField] private Material _mat;
    //[SerializeField, Range(0, 3)] private float minIntensity;
    //[SerializeField, Range(0, 3)] private float maxIntensity;

    //private float highIntensity;
    //private float lowIntensity;
    ////private Color highColor;
    //private float timePast = 0;
    private Gradient currentGradient;
    //private Color glowColor = new(191,191,191,0);
    //float intensity;

    //private void Awake()
    //{
    //    //baseColor = _mat.GetColor("_GlowColor");
    //    lowIntensity = minIntensity;// Mathf.Pow(2, minIntensity);
    //    //lowIntensity = new Color(baseColor.r * factor, baseColor.g * factor, baseColor.b * factor, 0);
    //    highIntensity = maxIntensity; // Mathf.Pow(2, maxIntensity);
    //    Debug.Log("highIntensity : " + lowIntensity + " " + highIntensity);
    //    _mat.SetColor("_GlowColor", glowColor * 0);
    //    //highColor = new Color(baseColor.r * factor, baseColor.g * factor, baseColor.b * factor, 0);
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    timePast += Time.deltaTime;
    //    float delta = timePast * 0.2f % 1;
        
    //    //Debug.Log(timePast * 0.1f % 1);
    //    if (delta < 0.5f)
    //    {
    //        intensity = Mathf.Lerp(lowIntensity, highIntensity, delta);
    //    }
    //    else
    //    {
    //        intensity = Mathf.Lerp(highIntensity, lowIntensity, delta);
    //    }
        
    //    //fillImg.color = currentGradient.Evaluate(delta);
    //    _mat.SetColor("_GlowColor", new Color(glowColor.r * intensity, glowColor.g * intensity, glowColor.b * intensity, 0));

    //    //_mat.color = gradient.Evaluate(timePast * 0.1f % 1);
    //}

    private void Start()
    {
        controller.OnCorruptionValueChange += UpdateSlider;
        currentGradient = gradientSanity;
    }

    private void UpdateSlider(float percent, bool transition)
    {
        debugCorruptionValue.text = controller.CurrentCorruption.ToString();
        float sliderValue = Mathf.Abs(percent - 0.5f) * 2;
        if (transition)
        {
            Debug.Log("SLIDER TRANSITION");
            ChangeSprite();
            _slider.transform.DOShakePosition(1f, 50, 100, 90).OnComplete(() =>
            {
                _slider.DOValue(sliderValue, 0.5f);
                fillImg.color = currentGradient.Evaluate(sliderValue);
                _slider.transform.DOShakePosition(0.5f, 5, 15, 60);
            });
            return;
        }
        _slider.DOValue(sliderValue, 0.5f);
        fillImg.color = currentGradient.Evaluate(sliderValue);
        _slider.transform.DOShakePosition(0.5f, 5, 15, 60);
        //slider.value = value;
        //_slider.fillRect.anchorMin = new Vector2(_slider.handleRect.anchorMin.x, 0.5f);
        //_slider.fillRect.anchorMax = new Vector2(0.5f, 1);
        //if (value > 0f)
        //{
        //    DesactiveSlider(_sliderSanity);
        //    ActiveSlider(_sliderCorruption);
        //    _sliderCorruption.DOValue(value, 1.5f);
        //    _sliderCorruption.transform.DOShakePosition(1.5f, 50, 100, 90);
        //}
        //else
        //{
        //    value *= -1f;
        //    DesactiveSlider(_sliderCorruption);
        //    ActiveSlider(_sliderSanity);

        //    _sliderSanity.DOValue(value, 1.5f);
        //    _sliderSanity.transform.DOShakePosition(1.5f, 30, 60, 60);
        //}

        //_slider.value = value;
        //_slider.DOValue(value, 1.5f);
        //StartCoroutine(adjustValue(value, 0.5f));

        //imgCorruption.color = color;
        //imgSanity.color = color;
    }

    private void ChangeSprite()
    {
        if(currentGradient == gradientSanity)
        {
            currentGradient = gradientCorruption;
        }
        else
        {
            currentGradient = gradientSanity;
        }
    }

    private void OnDisable()
    {
        Debug.Log("ON DISABLE SLIDERCORRUPTION");
        controller.OnCorruptionValueChange -= UpdateSlider;
    }
}
//using UnityEngine;
//using UnityEngine.UI;


//public class SliderSwitcher : MonoBehaviour
//{
//    private Slider _slider;

//    void Awake()
//    {
//        _slider = GetComponent<Slider>();
//    }

//    void Update()
//    {
//        UpdateSliderSense();
//    }

//    public void UpdateSliderSense()
//    {
//        if (_slider.value > 0)
//        {
//            _slider.fillRect.anchorMin = new Vector2(0.5f, 0);
//            _slider.fillRect.anchorMax = new Vector2(_slider.handleRect.anchorMin.x, 1);
//        }
//        else
//        {
//            _slider.fillRect.anchorMin = new Vector2(_slider.handleRect.anchorMin.x, 0);
//            _slider.fillRect.anchorMax = new Vector2(0.5f, 1);
//        }
//    }
//}