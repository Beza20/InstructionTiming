using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.clients;

namespace silab.conventions.ui {

    public class GrabbableSeekBarCursor : MetaQuestGrabbableBase {

        [SerializeField] RecordPlayer record_player;
        bool is_grabbed = false;

        public bool IsGrabbed => is_grabbed;
        
        new void Start() {
            base.Start();
        }

        public override void SelectGrabbableFunc() {
            is_grabbed = true;
            record_player.PauseRecord();
        }

        public override void UnselectGrabbableFunc() {
            is_grabbed = false;
            record_player.PlayRecord();
        }
    }
}
