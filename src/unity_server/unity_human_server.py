import os
import tf
import ast
import gym
import math
import rospy
import socket
import string
import numpy as np
from gym import spaces, utils
from collections import deque
from bvh_broadcaster import BVHBroadcaster, BVHReader

def encode_data(frame_name, trans, rot):
    # (bone name, position 3 DoF, rotation 4 DoF)
    # each bone has 7 arguments
    encode_str = '%s#(,'%(frame_name)
    for i in range(3):
        encode_str += '%.4f,'%(trans[i])
    encode_str += ')#(,'
    for i in range(4):
        encode_str += '%.4f,'%(rot[i])
    encode_str += ')'
    return encode_str

class UnityBVHSender(BVHBroadcaster):
    def __init__(self, filename, root_frame, tcp_conn):
        #addbyms
        BVHBroadcaster.__init__(self, filename, root_frame)

        #BVHReader.__init__(self, filename)
        print('mark 1')
        self.tcp_conn = tcp_conn

    def sendTf(self, _trans, _rot, _time, _tf, _parent_tf):
        self.br.sendTransform(_trans, _rot, _time, _tf, _parent_tf)
        a_str = encode_data(_tf, _trans, _rot)
        self.tcp_conn.sendall(str.encode(a_str))

class UnityHumanServer(gym.Env):
    def __init__(self, bvh_file='walk.bvh', root_frame='human'):
        HOST = "localhost"
        PORT = 65432
        self.frame_queue = deque(maxlen=100) # frame buffer
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.bind((HOST, PORT))
        print('Waiting for connection...')
        s.listen(100)
        conn, _ = s.accept()
        print('Connected!')
        self.bvh_sender = UnityBVHSender(bvh_file, root_frame, conn)
        
    def send(self, loop=True):
        self.bvh_sender.broadcast(loop=loop)
    
if __name__ == "__main__":
    env = UnityHumanServer()
    env.send()
        
