using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using silab.conventions.parameters;

namespace silab.conventions.communication {

    public class ParameterSender : MonoBehaviour {

        [SerializeField] EnvironmentDescriptor env_descriptor;
        string base_url;
        [SerializeField] CommunicationBase communication_base;

        void Start() {
            base_url = "https://" + env_descriptor.ServerAddress + ":" + env_descriptor.Port + "/";

            // ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidationCallback;
        }

        // bool ServerCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
        //     TextAsset cert = Resources.Load<TextAsset>("server.crt");
        //     X509Certificate2 cert2 = new X509Certificate2(cert.bytes);

        //     Debug.Log("aaaaaaaa");

        //     if (certificate != null && certificate.GetCertHashString() == cert2.GetCertHashString()) {
        //         return true;
        //     } else {
        //         return false;
        //     }
        // }

        class BypassCertificate : CertificateHandler {

            protected override bool ValidateCertificate(byte[] certificateData) {
                return true;
            }
        }

	    public void SendParameters<T>(T parameter, string function) {
	    	string json_string = JsonUtility.ToJson(parameter);
	    	byte[] byte_data = Encoding.UTF8.GetBytes(json_string);
	    	T result = JsonUtility.FromJson<T>(json_string);
	    	Debug.Log(result);
            StartCoroutine(SendDataPost(byte_data, function));
	    }

        public void GetParameter(string function) {
            StartCoroutine(GetData(function));
        }

        public void getAudio(string function) {
            StartCoroutine(GetAudioData(function));
        }

	    IEnumerator SendDataPost(byte[] byte_data, string function) {
            string url = base_url + function;
	    	UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.certificateHandler = new BypassCertificate();
	    	request.uploadHandler = (UploadHandler)new UploadHandlerRaw(byte_data);
	    	request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
	    	request.SetRequestHeader("Content-Type", "application/json");
	    	yield return request.Send();
	    	if (request.isNetworkError) {
	    		Debug.Log(request.error);
	    	} else {
                communication_base.GetResultsFromServer(request.downloadHandler.text);
        		Debug.Log("Form upload complete!");
	    	}
	    }

        IEnumerator GetData(string function) {
            string url = base_url + function;
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.certificateHandler = new BypassCertificate();
            yield return request.SendWebRequest();
            if (request.isNetworkError) {
                Debug.Log(request.error);
            } else {
                communication_base.GetResultsFromServer(request.downloadHandler.text);
                Debug.Log("Get request complete!");
            }
        }

        IEnumerator GetAudioData(string function) {
            string url = base_url + function;
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
            request.certificateHandler = new BypassCertificate();
            yield return request.SendWebRequest();
            if (request.isNetworkError) {
                Debug.Log(request.error);
            } else {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                communication_base.GetAudioRequestsFromServer(clip);
            }
        }
    }
}
