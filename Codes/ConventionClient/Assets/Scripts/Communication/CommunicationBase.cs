using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace silab.conventions.communication {

    public class CommunicationBase : MonoBehaviour {

        [SerializeField] ParameterSender parameter_sender;
        [SerializeField] string function_name;

        public void Send<T>(T param) {
            parameter_sender.SendParameters(param, function_name);
        }

        public void GetData() {
            parameter_sender.GetParameter(function_name);
        }

        public void GetAudioData() {
            parameter_sender.getAudio(function_name);
        }

        public virtual void GetResultsFromServer(string parameter_json) {

        }

        public virtual void GetAudioRequestsFromServer(AudioClip audio_clip) {

        }
    }
}
