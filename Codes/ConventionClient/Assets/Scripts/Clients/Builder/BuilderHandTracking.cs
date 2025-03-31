using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.communication;
using silab.conventions.parameters;

namespace silab.conventions.tracking {

    public class BuilderHandTracking : CommunicationBase {
        
        [SerializeField] OVRSkeleton skeleton_right;
        MetaQuestHandsFrames hands_frames;
        [SerializeField] float interval;
        float t;
    
        void Start() {
            hands_frames = new MetaQuestHandsFrames("builder");
        }

        void Update() {
            IList<OVRBone> bones_right = skeleton_right.Bones;
            int bone_count_right = bones_right.Count;
            MetaQuestHand hand_right = new MetaQuestHand();

            for (int i=0; i<bone_count_right; i++) {
                OVRBone bone = bones_right[i];
                Transform bone_transform = bone.Transform;
                Vector3 bone_position = bone_transform.position;
                hand_right.UpdateBone(bone_position, bone.Id);
            }

            MetaQuestHands hands = new MetaQuestHands(Time.time, Time.time, hand_right, null);
            if (hands_frames.hands_list.Count == 0) {
                hands_frames.AddHands(hands);
            } else {
                hands_frames.hands_list[0] = hands;
            }

            t += Time.deltaTime;
            if (t > interval) {
                t = 0;
                Send(hands_frames);
                hands_frames.ResetHands();
            }
        }
    }
}
