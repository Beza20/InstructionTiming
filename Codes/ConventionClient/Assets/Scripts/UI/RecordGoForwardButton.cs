using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using silab.conventions.clients;

namespace silab.conventions.ui {

    public class RecordGoForwardButton : MetaQuestButtonBase {

        [SerializeField] float forward_time;
        [SerializeField] RecordPlayer record_player;
        [SerializeField] TextMeshProUGUI text;
        
        new void Start() {
            base.Start();
            int absolute_forward_time = (int)Mathf.Abs(forward_time);
            text.text = absolute_forward_time.ToString();
        }

        public override void UnselectButtonFunc() {
            record_player.GoForwardRecord(forward_time);
            record_player.PauseRecord();
        }
    }
}
