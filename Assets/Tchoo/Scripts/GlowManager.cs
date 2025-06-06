using System;
using UnityEngine;

public class GlowManager : MonoBehaviour
{
    [SerializeField] private GlowElement[] glowElements;

    void Update()
    {
        float timePast = Time.unscaledTime;
        for (int i = 0; i < glowElements.Length; i++)
        {
            float delta = timePast * 0.1f * glowElements[i].Speed % 1;
            float intensity = Mathf.Lerp(glowElements[i].MinIntensity, 
                glowElements[i].MaxIntensity, 
                glowElements[i].IntensityOverTime.Evaluate(delta));
            Color color = glowElements[i].BaseColor;
            float factor = Mathf.Pow(2, intensity);

            glowElements[i].Mat.SetColor("_GlowColor", 
                new Color(color.r * factor, color.g * factor, color.b * factor, 0));
        }
    }
}

[Serializable]
public struct GlowElement
{
    [SerializeField] private Material _mat ;
    [SerializeField] private AnimationCurve _intensityOverTime;
    [SerializeField, Range(0, 10)] private float _minIntensity;
    [SerializeField, Range(0, 10)] private float _maxIntensity;
    [SerializeField, Range(1, 10)] private float _speed;
    [SerializeField] private Color _baseColor;

    public readonly Material Mat => _mat;
    public readonly AnimationCurve IntensityOverTime => _intensityOverTime;
    public readonly Color BaseColor => _baseColor;
    public readonly float MinIntensity => _minIntensity;
    public readonly float MaxIntensity => _maxIntensity;
    public readonly float Speed => _speed;
}
