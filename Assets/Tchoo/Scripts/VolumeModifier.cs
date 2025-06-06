using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeModifier : MonoBehaviour
{
    [SerializeField] private AnimationCurve vignetteIntensityCurve = new AnimationCurve();
    [SerializeField] private AnimationCurve lensFalreIntensityCurve = new AnimationCurve();
    [SerializeField] private VolumeProfile volumeProfile;

    //[SerializeField] private TextureCurve textureCurve1;
    //[SerializeField] private TextureCurve textureCurve2;
    //[SerializeField] private TextureCurve textureCurve3;
    PlayerController controller;

    private ColorCurves colorCurves;
    private Vignette vignette;
    private ScreenSpaceLensFlare screenSpaceLensFlare;
    //chromaticaberation

    //ColorCurves colorCurvesSaturated;
    //[SerializeField] private TextureCurve tex;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!volumeProfile) throw new NullReferenceException(nameof(VolumeProfile));

        // You can leave this variable out of your function, so you can reuse it throughout your class.
        //Vignette vignette;

        if (!volumeProfile.TryGet(out vignette)) throw new NullReferenceException(nameof(vignette));
        if (!volumeProfile.TryGet(out colorCurves)) throw new NullReferenceException(nameof(colorCurves));
        if (!volumeProfile.TryGet(out screenSpaceLensFlare)) throw new NullReferenceException(nameof(screenSpaceLensFlare));

        

        //vignette = GetComponent<Vignette>();
        //colorCurves = GetComponent<ColorCurves>();
        //controller = FindAnyObjectByType<PlayerController>();
        controller = FindAnyObjectByType<PlayerController>();
        Debug.Log("Get controller from volume : controller corruption => " + controller.CurrentCorruption);
        controller.OnCorruptionValueChange += UpdateVolumeSettings;
    }

    private void UpdateVolumeSettings(float corruptionPercent, bool switchState)
    {
        vignette.intensity.Override(vignetteIntensityCurve.Evaluate(corruptionPercent));
        screenSpaceLensFlare.intensity.Override(lensFalreIntensityCurve.Evaluate(corruptionPercent));
        switchColorCurve(corruptionPercent);
    }

    private void switchColorCurve(float percent)
    {
        Debug.Log("SWITCH CURVE COLOR : " + percent);
        if (percent > 0.8f)
        {
            OverrideColorCurve(new Keyframe[] { new(0.3f, 0f), new(0.6f, 0f), new(1f, 1f) });
            return;
        }
        if (percent > 0.65f)
        {
            OverrideColorCurve(new Keyframe[] { new(0.3f, 0.3f), new(0.6f, 0.3f), new(1f, 0.7f) });
            return;
        }
        if (percent > 0.5f)
        {
            OverrideColorCurve(new Keyframe[] { new(0.3f, 0.4f), new(0.6f, 0.4f), new(1f, 0.6f) });
            return;
        }
        ResetColorCurve();
    }

    private void ResetColorCurve()
    {
        Debug.Log("Reset curve");
        // RESET HUE VS SAT to a neutral flat curve
        OverrideColorCurve(new Keyframe[] { new(0f, 0.5f), new(1f, 0.5f) });
    }

    private void OverrideColorCurve(Keyframe[] keys)
    {
        Vector2 bounds = new Vector2(0f, 1f); // Assuming 0–1 range is standard for hue
        var newCurve = new TextureCurve(keys, 0f, false, bounds);

        colorCurves.hueVsSat.Override(newCurve);
    }

    private void OnDisable()
    {
        ResetColorCurve();
        vignette.intensity.Override(vignetteIntensityCurve.Evaluate(0f));
        screenSpaceLensFlare.intensity.Override(lensFalreIntensityCurve.Evaluate(0f));

    }

    //void SetBlueChannel()
    //{
    //    ColorCorrectionCurves myCCC = myCam.GetComponent<ColorCorrectionCurves>();

    //    //Set left and right key values
    //     .blueChannel.MoveKey(0, new Keyframe(0, myNewLeftSideValue));
    //    myCCC.blueChannel.MoveKey(1, new Keyframe(1, myNewRightSideValue));

    //    //Enforce linear tangents off each key. (Optional. Seems to default to curved.)
    //    //These can also be used to explicitly curve the tangents by a specified weight.
    //    myCCC.blueChannel.SmoothTangents(0, 0);
    //    myCCC.blueChannel.SmoothTangents(1, 0);

    //    //Update the component. Basically, redraw.
    //    myCCC.UpdateParameters();

    //}
}
