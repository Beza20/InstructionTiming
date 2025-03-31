using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.clients;

namespace silab.conventions.ui {

    public class RecordEnableButton : MetaQuestButtonBase {

        bool is_enable = true;
        [SerializeField] RecordPlayer record_player;

        public bool IsEnable => is_enable;
        
        new void Start() {
            base.Start();
        }

        public override void UnselectButtonFunc() {
            if (is_enable) {
                is_enable = false;
                record_player.EnableVisualization(false);
            } else {
                is_enable = true;
                record_player.EnableVisualization(true);
            }
        }
    }
}
