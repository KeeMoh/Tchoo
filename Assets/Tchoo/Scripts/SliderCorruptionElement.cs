using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SliderCorruptionElement : MonoBehaviour
{
    [SerializeField] private bool isUnlock;
    [SerializeField] private bool isPositive;
    [SerializeField, Range(0, 1)] private float value;
    [SerializeField] private int index;

    public bool IsUnlock => isUnlock;
    public bool IsPositive => isPositive;
    public float Value => value;
    public int Index => index;
    private Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    public Tween SetSliderValueWithTween(float value, float duration)
    {
        return slider.DOValue(value, duration);
    }
    public void SetUnlocked(bool value)
    {
        isUnlock = value;
        gameObject.SetActive(value); 
    }
}