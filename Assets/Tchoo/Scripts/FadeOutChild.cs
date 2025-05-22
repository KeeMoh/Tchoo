//using NaughtyAttributes;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class FadeOutChild : MonoBehaviour
//{
//    [SerializeField] private float fadeSpeed;

//    [Button]
//    public void fadeChildrens()
//    {
//        StartCoroutine(FadeOut());
//    }

//    private void FadeOutItems()
//    {
//        Tilemap[] tileMaps = GetComponentsInChildren<Tilemap>();
//        for (int i = 0; i < tileMaps.Length - 1; i++)
//        {
//            var d =(SpriteRenderer)tileMaps[i];
//        }


//    IEnumerator FadeOut()
//    {
//        Tilemap[] tileMaps = GetComponentsInChildren<Tilemap>();

//            for (int i = 0; i < tileMaps.Length - 1; i++)
//            {
//        while (tileMaps[0].color.a > 0)
//        {
//                Color alphacolor = tileMaps[i].color;

//                float fadeAmount = alphacolor.a - (fadeSpeed * Time.deltaTime);

//                alphacolor = new Color(alphacolor.r, alphacolor.g, alphacolor.b, fadeAmount);

//                tileMaps[i].color = alphacolor;

//                yield return null;
//            }
//        }

//    }
//}
