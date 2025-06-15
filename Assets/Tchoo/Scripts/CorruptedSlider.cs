using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CorruptedSlider : MonoBehaviour
{    
    [SerializeField] private PlayerControllerOLD controller;
    [SerializeField] private Slider _sliderHead;
    [SerializeField] private SliderCorruptionElement[] _sliderArray;
    [SerializeField] private TextMeshProUGUI debugCorruptionValue;

    private List<SliderCorruptionElement> sliderPositiveList = new();
    private List<SliderCorruptionElement> sliderNegativeList = new();


    private void Start()
    {
        controller.OnCorruptionValueChange += UpdateSlider;
        AssignSliderLists();
        ReorderSliderLists();
    }

    private void AssignSliderLists()
    {
        sliderPositiveList.Clear();
        sliderNegativeList.Clear();
        foreach (var slider in _sliderArray)
        {
            if(slider.IsPositive)
            {
                sliderPositiveList.Add(slider);
                Debug.Log("Slider (+) : index > " + slider.Index);

            }
            else
            {
                sliderNegativeList.Add(slider);
                Debug.Log("Slider (-) : index > " + slider.Index);

            }
        }
    }

    public float UnlockSliderElements(int countToUnlock)
    {
        int unlocked = 0;
        float totalUnlockedValue = 0f;

        List<SliderCorruptionElement> lockedElements = _sliderArray
            .OrderBy(slider => slider.Index)
            .ToList();

        foreach (SliderCorruptionElement element in lockedElements)
        {
            if (unlocked >= countToUnlock) break;

            element.SetUnlocked(true);
            totalUnlockedValue += element.Value;
            unlocked++;
        }

        return totalUnlockedValue;
    }

    private void ReorderSliderLists()
    {
        sliderNegativeList.OrderBy(slider => slider.Index).ToList();

        foreach(var slider in sliderNegativeList)
        {
                Debug.Log("Slider negativ : index > " + slider.Index);
        }

        sliderPositiveList.OrderBy(slider => slider.Index).ToList();

        foreach (var slider in sliderPositiveList)
        {
                Debug.Log("Slider positiv : index > " + slider.Index);
        }

    }

    public void UpdateSlider(float targetValue, bool transition)
    {
        float totalDuration = 1f;
        if ( targetValue > 0)
        {
            ClearOppositeList(sliderNegativeList);
            AnimateSliders(sliderPositiveList, targetValue, totalDuration);
        }
        else
        {
            ClearOppositeList(sliderPositiveList);
            AnimateSliders(sliderNegativeList, targetValue, totalDuration);
        }
    }

    private void ClearOppositeList(List<SliderCorruptionElement> oppositeList)
    {
        // STEP 1: Clear the opposite list first (if needed)
        foreach (var slider in oppositeList)
        {
            if (slider.SetSliderValueWithTween(0f, 0.3f) != null) // Optional fixed unfill time
            {
                slider.SetSliderValueWithTween(0f, 0.3f);
            }
        }
    }

    private void AnimateSliders(List<SliderCorruptionElement> activeList, float targetValue, float totalDuration)
    {
        float absTarget = Mathf.Abs(targetValue);
        float weightedTotal = CalculateTotalFillAmount(activeList, absTarget);
        // STEP 3: Animate each slider with its proportional share of the total duration
        float remaining = absTarget;
        float elapsedDelay = 0f;
        foreach (var slider in activeList)
        {
            float share = slider.Value * 3f;
            float fill = Mathf.Clamp01(Mathf.Min(share, remaining) / share);

            float segmentValue = Mathf.Min(share, remaining);
            float duration = (segmentValue / weightedTotal) * totalDuration;

            slider.SetSliderValueWithTween(fill, duration)
                  .SetDelay(elapsedDelay)
                  .SetEase(Ease.OutQuad);

            elapsedDelay += duration;
            remaining -= share;
            if (remaining <= 0f) break;
        }
    }

    private float CalculateTotalFillAmount(List<SliderCorruptionElement> activeList, float absTarget)
    {
        // STEP 2: Calculate total weighted fill needed
        float weightedTotal = 0f;
        foreach (var slider in activeList)
        {
            float slice = Mathf.Min(slider.Value * 3f, absTarget);
            weightedTotal += slice;
            absTarget -= slice;
            if (absTarget <= 0f) break;
        }
        return weightedTotal;
    }

    private void OnDisable()
    {
        Debug.Log("ON DISABLE SLIDERCORRUPTION");
        controller.OnCorruptionValueChange -= UpdateSlider;
    }
}