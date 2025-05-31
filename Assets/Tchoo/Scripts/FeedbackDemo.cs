using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FeedbackDemo : MonoBehaviour
{
    [SerializeField] private string data1;
    [SerializeField] private string data2;

    private string formURL = "https://docs.google.com/forms/d/e/[YOUR-FORM-ID]/formResponse";

    [Button]
    public void SubmitFeedback()
    {
        StartCoroutine(Post(data1, data2));
    }

    private IEnumerator Post(string data1, string data2)
    {
        WWWForm form = new WWWForm();
        form.AddField("entry.XXXXX", data1);
        form.AddField("entry.YYYYY", data2);

        using (UnityWebRequest www = UnityWebRequest.Post(formURL, form)) { 
        yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Feedback submitted successfully.");
            }
            else
            {
                Debug.LogError("Error in feedback sumission : " + www.error);
            }
        }
    }
}
