using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR;

namespace OpenAI_DALL_E.Scripts.Main
{
    public class DALL_E_ImageFetcher : MonoBehaviour
    {
        /// <summary>
        /// Api Key for Open AI API can be generated <see href="https://platform.openai.com/account/api-keys">HERE</see>
        /// </summary>
        public string apiKey;
        /// <summary>
        /// Invoked when Image has been generated by GetImageFromPrompt call
        /// (Sets texture to null in case of error)
        /// </summary>
        public Action<Texture2D> OnImageGenerated;
        private const string APILink = "https://api.openai.com/v1/images/generations";

        private const string APILinkVariation = "https://api.openai.com/v1/images/variations";

        /// <summary>
        /// Get image using input data
        /// </summary>
        /// <param name="promptData"></param>
        public void GetImageFromPrompt(InputData promptData)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("Api Key is needed to access Open AI Api See Link :" +
                               "https://platform.openai.com/account/api-keys");
                return;
            }
            if (string.IsNullOrEmpty(promptData.prompt))
            {
                Debug.LogError("Input Prompt can not be empty");
                return;
            }
            StartCoroutine(SendRequest(promptData));
        }   
        public void GetImageFromPrompt2(InputData promptData)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("Api Key is needed to access Open AI Api See Link :" +
                               "https://platform.openai.com/account/api-keys");
                return;
            }
            StartCoroutine(SendRequest2(promptData));
        }

        #region Main

        /* Example Request 
           curl https://api.openai.com/v1/images/generations \
            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $OPENAI_API_KEY" \
            -d '{
             "prompt": "A cute baby sea otter",
             "n": 2,
             "size": "1024x1024"
           }'

          */
        Texture2D tex;
        private IEnumerator SendRequest(InputData promptData)
        {
            // convert our  custom class into json format as it is required. See https://platform.openai.com/docs/api-reference/images/create //
            // the format is prompt, number of images, size e.g 512x512 // 
            // response_format: The format in which the generated images are returned. Must be one of url or b64_json 
            
            string json = JsonUtility.ToJson(promptData);

            var request = new UnityWebRequest();
            request = new UnityWebRequest(APILink);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                OnImageGenerated?.Invoke(null);
                Debug.Log(request.error);
            }
            else
            {
                Result res = JsonUtility.FromJson<Result>(request.downloadHandler.text);
                var textureRequest = UnityWebRequestTexture.GetTexture(res.data[0].url);
                textureRequest.SendWebRequest();
                yield return new WaitUntil(() => textureRequest.isDone);

                if (textureRequest.result == UnityWebRequest.Result.Success)
                {
                    tex = DownloadHandlerTexture.GetContent(textureRequest);                   
                    OnImageGenerated?.Invoke(tex);
                    saveImg();
                }
                else
                {
                    OnImageGenerated?.Invoke(null);
                    Debug.Log(textureRequest.error);
                }

            }
        }
              
        private IEnumerator SendRequest2(InputData promptData)
        {
            // convert our  custom class into json format as it is required. See https://platform.openai.com/docs/api-reference/images/create //
            // the format is prompt, number of images, size e.g 512x512 // 
            // response_format: The format in which the generated images are returned. Must be one of url or b64_json 
            //Destroy(tex);
            
            string json = JsonUtility.ToJson(promptData);

            var request = new UnityWebRequest();
            request = new UnityWebRequest(APILinkVariation);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            // request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("came");
                OnImageGenerated?.Invoke(null);
                Debug.Log(request.error);
            }
            else
            {
                Result res = JsonUtility.FromJson<Result>(request.downloadHandler.text);
                var textureRequest = UnityWebRequestTexture.GetTexture(res.data[0].url);
                textureRequest.SendWebRequest();
                yield return new WaitUntil(() => textureRequest.isDone);

                if (textureRequest.result == UnityWebRequest.Result.Success)
                {
                    tex = DownloadHandlerTexture.GetContent(textureRequest);                   
                    OnImageGenerated?.Invoke(tex);

                }
                else
                {
                    OnImageGenerated?.Invoke(null);
                    Debug.Log(textureRequest.error);
                }

            }
        }




        void saveImg()
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Resources/img.png", bytes);
            AssetDatabase.Refresh();
        }

        public string texttoPng()
        {
            //byte[] bytes = tex.EncodeToPNG();
            //string enc = Convert.ToBase64String(bytes);
            //return enc;
          //  string path = Application.dataPath + "/Resources/img.png";
            string path =  "@img.png";

            Debug.Log(path);
            return path;
            //if (File.Exists(Application.dataPath + "/img.png"))
            //{
            //    return Application.dataPath + "/img.png";
            //}
            //else
            //    return "";
        }
        #endregion

        #region Models

        [Serializable]
        public class InputData
        {

            public string prompt;
            public int n;
            public string size;
            /// <summary>
            /// Input data to generate Image from
            /// </summary>
            /// <param name="prompt">Input prompt as text</param>
            /// <param name="numberOfImages">Number of images to generate from prompt</param>
            /// <param name="size">Input to generate (size x size) image</param>
            public InputData(string prompt, int numberOfImages, int size)
            {
                this.prompt = prompt;
                n = numberOfImages;
                this.size = size + "x" + size;
            }
        }

        [Serializable]
        public class ReceivedData
        {
            public string url;
        }

        [Serializable]
        public class Result
        {
            public int created;
            public List<ReceivedData> data;
        }

        public class ImageEvent : UnityEvent<List<Sprite>> { }

        #endregion
    }
}
