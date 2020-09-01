#!/usr/bin/env python
import tf
import gym
import rospy
import threading

from sensor_msgs.msg import JointState

class GymPepper():
    def __init__(self):
        rospy.init_node('gym_pepper')
        rospy.Subscriber('/joint_states', JointState, self._callback)
        self.tf_sub = tf.TransformListener()
        self._obs = None
        self.pepper_joints = ['HeadYaw', 'HeadPitch', 'LShoulderPitch', 'LShoulderRoll', 
                              'LElbowYaw', 'LElbowRoll', 'LWristYaw', 'LHand', 'RShoulderPitch', 
                              'RShoulderRoll', 'RElbowYaw', 'RElbowRoll', 'RWristYaw', 'RHand']
        self.picked_idx = []
        self.last_recv = {}
        self.lock = threading.Lock()

        #ms
        self.idx_curr = -1
    
    def populate(self, recv_names):
        if len(self.picked_idx) == len(self.pepper_joints):
            return
        for j in self.pepper_joints:
            self.picked_idx.append(recv_names.index(j))

    def _callback(self, data):
        '''
        header: 
        seq: 4829
        stamp: 
            secs: 1598482491
            nsecs: 618535041
        frame_id: ''
        name: [HeadYaw, HeadPitch, HipRoll, HipPitch, KneePitch, LShoulderPitch, LShoulderRoll,
               LElbowYaw, LElbowRoll, LWristYaw, LHand, RShoulderPitch, RShoulderRoll, RElbowYaw,
               RElbowRoll, RWristYaw, RHand, RFinger41, LFinger42, RFinger12, LFinger33, RFinger31,
               LFinger21, RFinger32, LFinger13, LFinger32, LFinger11, RFinger22, RFinger13, LFinger22,
               RFinger21, LFinger41, LFinger12, RFinger23, RFinger11, LFinger23, LFinger43, RFinger43,
               RFinger42, LFinger31, RFinger33, LThumb1, RThumb2, RThumb1, LThumb2, WheelFL, WheelB,
               WheelFR]
        position: [0.0, -9.941229999987922e-05, 0.0, 0.0, 0.0, 0.0, 0.7853983250000001, 0.0, -0.7853983250000001, 0.0, 0.5, 0.0, -0.7853983250000001, 0.0, 0.7853983250000001, 0.0, 0.5, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.4363325, 0.0, 0.0, 0.0]
        velocity: []
        effort: []
        '''
        recv_names = data.name
        recv_pos = data.position
        recv_vel = data.velocity
        recv_eff = data.effort
        with self.lock:
            self.last_recv['names'] = recv_names
            self.last_recv['pos'] = recv_pos
            self.last_recv['vel'] = recv_vel
            self.last_recv['eff'] = recv_eff

    def reset(self):
        pass

    def calc_reward(self, obs, target):
        # added by ms

        # Only compare the rotation between currentconfig and targetconfig??
        curr_configs = obs['vel']
        target_configs = target[self.idx_curr]['vel']

        assert len(curr_configs) == len(target_configs)
        err_config = np.sum(np.abs(curr_configs - target_configs))
        reward_config = math.exp(-err_config)

        #采样率120pfs，每秒为一个step
        self.idx_curr += 120
        self.idx_curr = self.idx_curr % self.mocap_data_len

        return reward_config

        # TODO
        return 0.0

    def get_target(self):
        # ms
        '''
        header
            stamp             time stamp, rospy.Time
            frame_id          string, parent frame
        child_frame_id        string, child frame
        transform
            translation
                x             float
                y             float
                z             float
            rotation
                x             float
                y             float
                z             float
                w             float
        '''
        pos = []
        vel = []
        for i in range(len(self.tf_sub)):
            pos.append(self.tf_sub[i].translation)
            vel.append(self.tf_sub[i].rotation)
        return {'pos': pos, 'vel': vel}
        
        # TODO

        return None

    def is_done(self):
        # TODO
        return False

    def step(self, action):
        # execute action 
        # TODO
        obs = self._get_obs()
        target = self.get_target()
        reward = self.calc_reward(obs, target)
        done = self.is_done
        info = {}
        return obs, reward, done, info

    def _get_obs(self):
        pos = []
        vel = []
        with self.lock:
            for i in self.picked_idx:
                pos.append(self.last_recv['pos'][i])
                vel.append(self.last_recv['vel'][i])
        return {'pos': pos, 'vel': vel}