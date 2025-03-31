using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

namespace silab.conventions.ui {

    public class MetaQuestGrabbableBase : MonoBehaviour {
        
        InteractableUnityEventWrapper grabbable;

        public void Start() {
            grabbable = GetComponent<InteractableUnityEventWrapper>();
            grabbable.WhenSelect.AddListener(SelectGrabbableFunc);
            grabbable.WhenUnselect.AddListener(UnselectGrabbableFunc);
        }

        public virtual void SelectGrabbableFunc() {
            Debug.Log("grabbable hovered");
        }

        public virtual void UnselectGrabbableFunc() {
            Debug.Log("grabbable unselected");
        }
    }
}
