using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.clients;

namespace silab.conventions.ui {

    public class ResetButton : MetaQuestButtonBase {

        [SerializeField] RecordPlayer record_player;

        new void Start() {
            base.Start();
        }

        public override void UnselectButtonFunc() {
            record_player.ResetRecord();
        }
    }
}
