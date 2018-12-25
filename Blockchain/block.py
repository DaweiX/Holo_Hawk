import socketserver
import time
import hashlib
import datetime
import pickle
import client

class Node:
    def __init__(self, location, distance, public_key, conversation_key):
        self.location = location
        self.distance = distance
        self.public_key = public_key
        self.conversation_key = conversation_key
        self.signature = signature


class Block:
    
    def __init__(self, list_miner, blacklist_miner, index, timestamp, node, previous_hash):
        self.miner = 1
        self.list_miner = list_miner
        self.blacklist_miner = blacklist_miner
        self.index = index
        self.timestamp = timestamp
        self.node = node
        self.previous_hash = previous_hash
        self.hash = self.hash_block()
        self.miner_signature = miner_signature
        

    def hash_block(self):
        string=(str(self.index) + str(self.timestamp) + str(self.data) + str(self.previous_hash))
        sha = hashlib.sha256()
        sha.update(string.encode('utf-8'))
        return sha.hexdigest()
    
    def change_miner(self, num):
        self.next_miner = num
        


#with open('gps_data.txt') as f:
#    gpsdata = f.read()

def create_genesis_block():
    # Manually construct a block with
    # index zero and arbitrary previous hash
    return Block([0],0, datetime.datetime.now(), "Genesis Block", "0")

def next_block(last_block, gps_data):
    this_blacklist_miner = last_block.blacklist_miner
    this_index = last_block.index + 1
    this_timestamp = datetime.datetime.now()
    this_data = gps_data 
    this_hash = last_block.hash
    return Block(this_blacklist_miner, this_index, this_timestamp, this_data, this_hash)

def broadcast_genesis_block(genesis_block, host, port, num_of_node):
    
    global num
    num = 0
    global block_to_send
    block_to_send = genesis_block
    
    class myTCPhandler(socketserver.BaseRequestHandler):
        def handle(self):
            while True:
                #try:
                #receive from the client
                self.data = self.request.recv(4096).decode('UTF8', 'ignore').strip()
                if not self.data : break
                print(self.data)

                global num
                num += 1
                    
                global block_to_send
                packed_genesis_block = pickle.dumps(block_to_send)
                self.request.sendall(packed_genesis_block)
                    
                if num == (num_of_node - 1):
                    self.server.shutdown()
                    self.request.close()
                    break

                
    server = socketserver.ThreadingTCPServer((host,port),myTCPhandler,bind_and_activate = False)
    server.allow_reuse_address = True
    server.server_bind()
    server.server_activate()
    server.serve_forever()

def receive_genesis_block(host, port, mark):
    message = ('received'+ "-" + mark).encode("utf8")
    genesis_block = client.client(host, port, message)
    return genesis_block





