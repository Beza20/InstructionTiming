using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.communication;
using silab.conventions.parameters;

namespace silab.conventions.tracking {

    public class HandTracking : CommunicationBase {

        [SerializeField] OVRSkeleton skeleton_left;
        [SerializeField] OVRSkeleton skeleton_right;
        [SerializeField] GameObject cube;
        [SerializeField] GameObject bone_sphere;
        List<GameObject> bone_spheres_left;
        List<GameObject> bone_spheres_right;
        MetaQuestHandsFrames hands_frames;
        [SerializeField] int frame_chunk;

        void Start() {
            bone_spheres_left = new List<GameObject>();
            bone_spheres_right = new List<GameObject>();
            hands_frames = new MetaQuestHandsFrames("instructor");
        }

        void Update() {
            IList<OVRBone> bones_left = skeleton_left.Bones;
            int bone_count_left = bones_left.Count;
            while (bone_spheres_left.Count < bone_count_left) {
                GameObject new_bone_sphere = Instantiate(bone_sphere);
                bone_spheres_left.Add(new_bone_sphere);
            }

            IList<OVRBone> bones_right = skeleton_right.Bones;
            int bone_count_right = bones_right.Count;
            while (bone_spheres_right.Count < bone_count_right) {
                GameObject new_bone_sphere = Instantiate(bone_sphere);
                bone_spheres_right.Add(new_bone_sphere);
            }

            MetaQuestHand hand_left = new MetaQuestHand();
            MetaQuestHand hand_right = new MetaQuestHand();

            for (int i=0; i<bone_count_left; i++) {
                OVRBone bone = bones_left[i];
                Transform bone_transform = bone.Transform;
                Vector3 bone_position = bone_transform.position;
                bone_spheres_left[i].transform.position = bone_position;
                hand_left.UpdateBone(bone_position, bone.Id);
            }

            for (int i=0; i<bone_count_right; i++) {
                OVRBone bone = bones_right[i];
                Transform bone_transform = bone.Transform;
                Vector3 bone_position = bone_transform.position;
                bone_spheres_right[i].transform.position = bone_position;
                hand_right.UpdateBone(bone_position, bone.Id);
            }

            MetaQuestHands hands = new MetaQuestHands(Time.time, Time.time, hand_right, hand_left);
            hands_frames.AddHands(hands);

            if (hands_frames.hands_list.Count >= frame_chunk) {
                Send(hands_frames);
                hands_frames.ResetHands();
            }
        }
    }
}
