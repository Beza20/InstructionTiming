using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace silab.conventions.parameters {

    [System.Serializable]
    public class Finger {

        public Vector3 mp;
        public Vector3 pip;
        public Vector3 dip;
        public Vector3 tip;

        public Finger() {
            this.mp = new Vector3(0, 0, 0);
            this.pip = new Vector3(0, 0, 0);
            this.dip = new Vector3(0, 0, 0);
            this.tip = new Vector3(0, 0, 0);
        }
    }

    [System.Serializable]
    public class Hand {

        public Finger thumb;
        public Finger index;
        public Finger middle;
        public Finger ring;
        public Finger pinky;

        public Hand() {
            this.thumb = new Finger();
            this.index = new Finger();
            this.middle = new Finger();
            this.ring = new Finger();
            this.pinky = new Finger();
        }
    }

    [System.Serializable]
    public class MetaQuestHand {

        public Vector3 forearm_stub;
        public Vector3 wrist_root;
        public Vector3 thumb_0, thumb_1, thumb_2, thumb_3, thumb_tip;
        public Vector3 index_1, index_2, index_3, index_tip;
        public Vector3 middle_1, middle_2, middle_3, middle_tip;
        public Vector3 ring_1, ring_2, ring_3, ring_tip;
        public Vector3 pinky_0, pinky_1, pinky_2, pinky_3, pinky_tip;

        public MetaQuestHand() {
            this.forearm_stub = new Vector3(0, 0, 0);
            this.wrist_root = new Vector3(0, 0, 0);
            this.thumb_0 = new Vector3(0, 0, 0);
            this.thumb_1 = new Vector3(0, 0, 0);
            this.thumb_2 = new Vector3(0, 0, 0);
            this.thumb_3 = new Vector3(0, 0, 0);
            this.thumb_tip = new Vector3(0, 0, 0);
            this.index_1 = new Vector3(0, 0, 0);
            this.index_2 = new Vector3(0, 0, 0);
            this.index_3 = new Vector3(0, 0, 0);
            this.index_tip = new Vector3(0, 0, 0);
            this.middle_1 = new Vector3(0, 0, 0);
            this.middle_2 = new Vector3(0, 0, 0);
            this.middle_3 = new Vector3(0, 0, 0);
            this.middle_tip = new Vector3(0, 0, 0);
            this.ring_1 = new Vector3(0, 0, 0);
            this.ring_2 = new Vector3(0, 0, 0);
            this.ring_3 = new Vector3(0, 0, 0);
            this.ring_tip = new Vector3(0, 0, 0);
            this.pinky_0 = new Vector3(0, 0, 0);
            this.pinky_1 = new Vector3(0, 0, 0);
            this.pinky_2 = new Vector3(0, 0, 0);
            this.pinky_3 = new Vector3(0, 0, 0);
            this.pinky_tip = new Vector3(0, 0, 0);
        }

        public void UpdateBone(Vector3 new_pos, OVRSkeleton.BoneId bone_id) {
            if (bone_id == OVRSkeleton.BoneId.Hand_ThumbTip) {
                this.thumb_tip = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Thumb3) {
                this.thumb_3 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Thumb2) {
                this.thumb_2 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Thumb1) {
                this.thumb_1 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Thumb0) {
                this.thumb_0 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_IndexTip) {
                this.index_tip = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Index3) {
                this.index_3 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Index2) {
                this.index_2 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Index1) {
                this.index_1 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_MiddleTip) {
                this.middle_tip = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Middle3) {
                this.middle_3 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Middle2) {
                this.middle_2 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Middle1) {
                this.middle_1 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_RingTip) {
                this.ring_tip = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Ring3) {
                this.ring_3 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Ring2) {
                this.ring_2 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Ring1) {
                this.ring_1 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_PinkyTip) {
                this.pinky_tip = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Pinky3) {
                this.pinky_3 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Pinky2) {
                this.pinky_2 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Pinky1) {
                this.pinky_1 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_Pinky0) {
                this.pinky_0 = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_WristRoot) {
                this.wrist_root = new_pos;
            } else if (bone_id == OVRSkeleton.BoneId.Hand_ForearmStub) {
                this.forearm_stub = new_pos;
            }
        }

        public List<List<Vector3>> GetConnections() {
            List<List<Vector3>> connections = new List<List<Vector3>>();
            connections.Add(new List<Vector3> {this.forearm_stub, this.wrist_root});
            connections.Add(new List<Vector3> {this.wrist_root, this.thumb_0, this.index_1, this.middle_1, this.ring_1, this.pinky_0});
            connections.Add(new List<Vector3> {this.thumb_0, this.thumb_1});
            connections.Add(new List<Vector3> {this.thumb_1, this.thumb_2});
            connections.Add(new List<Vector3> {this.thumb_2, this.thumb_3});
            connections.Add(new List<Vector3> {this.thumb_3, this.thumb_tip});
            connections.Add(new List<Vector3> {this.thumb_tip});
            connections.Add(new List<Vector3> {this.index_1, this.index_2});
            connections.Add(new List<Vector3> {this.index_2, this.index_3});
            connections.Add(new List<Vector3> {this.index_3, this.index_tip});
            connections.Add(new List<Vector3> {this.index_tip});
            connections.Add(new List<Vector3> {this.middle_1, this.middle_2});
            connections.Add(new List<Vector3> {this.middle_2, this.middle_3});
            connections.Add(new List<Vector3> {this.middle_3, this.middle_tip});
            connections.Add(new List<Vector3> {this.middle_tip});
            connections.Add(new List<Vector3> {this.ring_1, this.ring_2});
            connections.Add(new List<Vector3> {this.ring_2, this.ring_3});
            connections.Add(new List<Vector3> {this.ring_3, this.ring_tip});
            connections.Add(new List<Vector3> {this.ring_tip});
            connections.Add(new List<Vector3> {this.pinky_0, this.pinky_1});
            connections.Add(new List<Vector3> {this.pinky_1, this.pinky_2});
            connections.Add(new List<Vector3> {this.pinky_2, this.pinky_3});
            connections.Add(new List<Vector3> {this.pinky_3, this.pinky_tip});
            connections.Add(new List<Vector3> {this.pinky_tip});
            return connections;
        }
    }

    [System.Serializable]
    public class Hands {

        public float start;
        public float end;
        public Hand right;
        public Hand left;

        public Hands(float start, float end, Hand right, Hand left) {
            this.start = start;
            this.end = end;
            this.right = right;
            this.left = left;
        }

        public List<Vector3> GetFingerTIPs() {
            List<Vector3> finger_tips = new List<Vector3>();
            finger_tips.Add(this.left.thumb.tip);
            finger_tips.Add(this.left.index.tip);
            finger_tips.Add(this.left.middle.tip);
            finger_tips.Add(this.left.ring.tip);
            finger_tips.Add(this.left.pinky.tip);
            return finger_tips;
        }

        public List<Vector3> GetFingerDIPs() {
            List<Vector3> finger_dips = new List<Vector3>();
            finger_dips.Add(this.left.thumb.dip);
            finger_dips.Add(this.left.index.dip);
            finger_dips.Add(this.left.middle.dip);
            finger_dips.Add(this.left.ring.dip);
            finger_dips.Add(this.left.pinky.dip);
            return finger_dips;
        }

        public List<Vector3> GetFingerPIPs() {
            List<Vector3> finger_pips = new List<Vector3>();
            finger_pips.Add(this.left.thumb.pip);
            finger_pips.Add(this.left.index.pip);
            finger_pips.Add(this.left.middle.pip);
            finger_pips.Add(this.left.ring.pip);
            finger_pips.Add(this.left.pinky.pip);
            return finger_pips;
        }

        public List<Vector3> GetFingerMPs() {
            List<Vector3> finger_mps = new List<Vector3>();
            finger_mps.Add(this.left.thumb.mp);
            finger_mps.Add(this.left.index.mp);
            finger_mps.Add(this.left.middle.mp);
            finger_mps.Add(this.left.ring.mp);
            finger_mps.Add(this.left.pinky.mp);
            return finger_mps;
        }
    }

    [System.Serializable]
    public class MetaQuestHands {

        public float start;
        public float end;
        public MetaQuestHand right;
        public MetaQuestHand left;

        public MetaQuestHands(float start, float end, MetaQuestHand right, MetaQuestHand left) {
            this.start = start;
            this.end = end;
            this.right = right;
            this.left = left;
        }
    }

    [System.Serializable]
    public class HandsFrames {
            
        public List<Hands> hands_list;

        public HandsFrames() {
            this.hands_list = new List<Hands>();
        }

        public void AddHands(Hands hands) {
            this.hands_list.Add(hands);
        }

        public void ResetHands() {
            this.hands_list.Clear();
        }
    }

    [System.Serializable]
    public class MetaQuestHandsFrames : ISerializationCallbackReceiver {
            
        public List<MetaQuestHands> hands_list;
        public string client_type;

        public void OnAfterDeserialize() {
            if (string.IsNullOrEmpty(client_type)) {
                client_type = "instructor";
            }
        }

        public void OnBeforeSerialize() {}

        public MetaQuestHandsFrames(string client_type) {
            this.hands_list = new List<MetaQuestHands>();
            this.client_type = client_type;
        }

        public void AddHands(MetaQuestHands hands) {
            this.hands_list.Add(hands);
        }

        public void ResetHands() {
            this.hands_list.Clear();
        }
    }
}
