using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.clients;

namespace silab.conventions.ui {

    public class RecordPlayButton : MetaQuestButtonBase
    {
        [SerializeField] bool is_play_button;
        [SerializeField] RecordPlayer record_player;
        [SerializeField] GameObject playing_indicator;

        new void Start() {
            base.Start();
        }

        public override void UnselectButtonFunc() {
            if (is_play_button) {
                record_player.PlayRecord();
                playing_indicator.SetActive(is_play_button);
            } else {
                record_player.PauseRecord();
                playing_indicator.SetActive(is_play_button);
            }
        }
    }
}
