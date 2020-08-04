import os
import ast
import gym
import socket
import numpy as np
from gym import spaces, utils

def decode_recv(_recv):
    state_strs = _recv.split('#')[1:-1] # start and end with #
    state_list = []
    for each in state_strs:
        state_list.append(np.fromstring(each[1:-1], dtype=np.float, sep=','))

    return np.array(state_list)

def encode_send(_to_send, _num_frames, prefix):
    assert len(_to_send)%3 == 0
    assert type(prefix) == str
    assert prefix.upper() in ["ACTION", "STATE"]
    encode_str = '%s#'%(prefix.upper())
    for i in range(len(_to_send)//3):
        encode_str += "(%.3f,%.3f,%.3f)#"%(_to_send[3*i], _to_send[3*i+1], _to_send[3*i+2])
    if prefix.upper() == "ACTION":
        encode_str += "%4d#"%(_num_frames) # add frame number to indicate action
    else:
        encode_str += "0000#" # add 0000 to indicate state
    return encode_str

class UnityClothBallEnv(gym.Env):
    metadata = {"render.modes": ["human", "rgb_array"]}
    def __init__(self):
        HOST = "localhost"
        PORT = 65431

        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.bind((HOST, PORT))
            s.listen()
            self.conn, _ = s.accept()
        
    def step(self, a, num_frames):
        a_str = encode_send(a, num_frames, "action")
        self.conn.sendall(str.encode(a_str))
        ob = self._get_obs(num_frames)

        reward = 0.0
        done = False
        meta_data = {}

        return ob, reward, done, meta_data

    def _get_obs(self, num_frames):
        obs = []
        for _ in range(num_frames):
            ob_str = (self.conn.recv(204800)).decode()
            ob_array = decode_recv(ob_str)
            obs.append(ob_array)
        return np.array(obs)

    def predict(self, _states, num_frames):
        assert num_frames == 1
        for i in range(num_frames):
            state = _states[i].flatten()
            state_str = encode_send(state, num_frames, "state")
            self.conn.sendall(str.encode(state_str))

    def reset(self):
        if not self.first_init:
            self.conn.sendall(a_bytes)

        self.first_init = False
        return self._get_obs()
    
    def test_a(self):
        ob_str1 = (self.conn.recv(204800)).decode()
        ob_str2 = (self.conn.recv(204800)).decode()

        ob_array = decode_recv(ob_str1+ob_str2)
        return ob_array

    def test_b(self, _ac):
        ac_str = encode_send(_ac, 100, "action")
        self.conn.sendall(str.encode(ac_str))

    def test_c(self, _state):
        state = _state.flatten()
        state_str = encode_send(state, 1, "state")
        self.conn.sendall(str.encode(state_str))

if __name__ == "__main__":
    env = UnityClothBallEnv()
    print("Env test")
    # env.reset()
    # action = np.random.rand(6)
    # env.test_b(np.array([1.0, 2.0, 3.0, 4.0, 5.0, 6.0]))
    import time

    while True:
        # ob, reward, done, _ = env.step([10.0, 8.0, 25.0, -25.0, 25.0, 25.0, -25.0, -25.0, -25.0, -25.0])
        # print(ob[0], ob[1])
        # print(done)
        # print(reward)
        # action = np.random.rand(6)
        # action[3:] = action[3:] - 0.5
        # ob = env.step(action, 500)[0]
        # print(ob.shape)
        # for each in ob:
        #     env.predict(each[None], 1)
        #     time.sleep(0.02)

        # time.sleep(3)
        ob = env.test_a()
        print(ob.shape)
        env.test_c(ob)
        time.sleep(0.05)
        