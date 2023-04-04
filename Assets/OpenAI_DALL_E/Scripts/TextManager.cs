using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using static OpenAI_DALL_E.Scripts.Main.DALL_E_ImageFetcher;


public class TextManager : MonoBehaviour
{
    public TMP_InputField text;
    public UnityEngine.UI.Text resultTxt;
    public Button getBtn;

    public string apiKey;

    private const string APILink = "https://api.openai.com/v1/completions";

    private void OnEnable()
    {
        getBtn.onClick.AddListener(() =>
        {
            GetTextPrompt(new textParams("text-davinci-003", text.text, 0.2F, true));
            getBtn.interactable = false;
        });
        
    }


    /// <summary>
    /// for text completion
    /// </summary>
    /// <param name="t"></param>
    public void GetTextPrompt(textParams t)
    {
       
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("Api Key is needed to access Open AI Api See Link :" +
                           "https://platform.openai.com/account/api-keys");
            return;
        }
        if (string.IsNullOrEmpty(t.prompt))
        {
            Debug.LogError("Input Prompt can not be empty");
            return;
        }
        StartCoroutine(SendRequest(t));
    }

    private IEnumerator SendRequest(textParams t)
    {

        string json = JsonUtility.ToJson(t);
        var request = new UnityWebRequest(APILink, UnityWebRequest.kHttpVerbPOST);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.uploadHandler.contentType = "application/json";
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Result res = JsonUtility.FromJson<Result>(request.downloadHandler.text);
            resultTxt.text = res.choices[0].text;
            getBtn.interactable = true;

        }
    } 

    [Serializable]
    public class ReceivedData
    {
        public string text;
    }

    [Serializable]
    public class Result
    {
        public List<ReceivedData> choices;
    }


}
[System.Serializable]
public class textParams
{
    public string model;
    public string prompt;
    public float temperature;
    public bool echo;

    public textParams(string model, string prompt,float temperature, bool echo)
    {
        this.model = model;
        this.prompt = prompt;
        this.temperature = temperature;
        this.echo = echo;
    }
}
