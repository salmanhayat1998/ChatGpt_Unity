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

public class chatManager : MonoBehaviour
{
    public TMP_InputField text;
    public UnityEngine.UI.Text resultTxt;
    public Button getBtn;

    public string apiKey;

    private const string APILink = "https://api.openai.com/v1/chat/completions";

    private void OnEnable()
    {

    

        getBtn.onClick.AddListener(() =>
        {
            message msg = new message("user", text.text);
            message[] messages = new message[1];
            messages[0] = msg;  
            GetChatPrompt(new chatParams("gpt-3.5-turbo", messages));
            getBtn.interactable = false;
        });
    }


    public void GetChatPrompt(chatParams c)
    {

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("Api Key is needed to access Open AI Api See Link :" +
                           "https://platform.openai.com/account/api-keys");
            return;
        }
        //if (string.IsNullOrEmpty(c.prompt))
        //{
        //    Debug.LogError("Input Prompt can not be empty");
        //    return;
        //}
        StartCoroutine(SendRequest(c));
    }
    private IEnumerator SendRequest(chatParams c)
    {

        string json = JsonUtility.ToJson(c);
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
            resultTxt.text = res.choices[0].message.content;
            getBtn.interactable = true;

        }
    }
    [Serializable]
    public class ReceivedData
    {
        public message message;
    }

    [Serializable]
    public class Result
    {
        public List<ReceivedData> choices;
    }

    [System.Serializable]
    public class chatParams
    {
        public string model;
        public message[] messages;
        public chatParams(string model, message[] messages)
        {
            this.model = model;
            this.messages = messages;

        }
    }
    [System.Serializable]
    public class message
    {
        public string role;
        public string content;
        public message(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }
}
