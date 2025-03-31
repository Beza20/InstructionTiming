using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

namespace silab.conventions.ui {

    public class MetaQuestButtonBase : MonoBehaviour {

        InteractableUnityEventWrapper button;

        // Start is called before the first frame update
        public void Start() {
            button = GetComponent<InteractableUnityEventWrapper>();
            button.WhenUnselect.AddListener(UnselectButtonFunc);
        }

        public virtual void UnselectButtonFunc() {
            Debug.Log("button selected");
        }
    }
}