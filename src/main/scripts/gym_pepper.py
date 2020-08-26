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

    def calc_reward(self):
        pass

    def step(self):
        pass

    def _get_obs(self):
        with self.lock:
            self.last_recv['names']
            self.last_recv['pos']
            self.last_recv['vel']
            self.last_recv['eff']