using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.ui;

namespace silab.conventions.clients {

    public class RecordSeekBar : MonoBehaviour {
        
        [SerializeField] GameObject seek_bar_cursor;
        [SerializeField] GameObject seek_bar;
        [SerializeField] GameObject progress_bar;
        float seek_bar_width;
        [SerializeField] RecordPlayer record_player;
        [SerializeField] GrabbableSeekBarCursor grabbable_seek_bar_cursor;

        void Start() {
            seek_bar_width = seek_bar.transform.localScale.y;
            Debug.Log(seek_bar_width);
        }

        void Update() {
            if (grabbable_seek_bar_cursor.IsGrabbed) {
                float relative_time = (grabbable_seek_bar_cursor.transform.position.x - seek_bar.transform.position.x + seek_bar_width) / (2 * seek_bar_width);
                relative_time = Mathf.Clamp(relative_time, 0, 1);
                record_player.SetRelativeTime(relative_time);
                relative_time = record_player.GetRelativeTime();
                UpdateCursorPosition(relative_time);
            } else {
                float relative_time = record_player.GetRelativeTime();
                UpdateCursorPosition(relative_time);
                grabbable_seek_bar_cursor.gameObject.transform.position = seek_bar_cursor.transform.position;
            }
        }

        void UpdateCursorPosition(float relative_time) {
            seek_bar_cursor.transform.position = Vector3.Lerp(
                seek_bar.transform.position - new Vector3(seek_bar_width, 0, 0),
                seek_bar.transform.position + new Vector3(seek_bar_width, 0, 0),
                relative_time
            );
            progress_bar.transform.localScale = new Vector3(0.0021f, relative_time * 0.1f, 0.0021f);
            progress_bar.transform.position = new Vector3(
                (seek_bar_cursor.transform.position.x - seek_bar_width) / 2,
                seek_bar_cursor.transform.position.y,
                seek_bar_cursor.transform.position.z
            );
        }
    }
}
