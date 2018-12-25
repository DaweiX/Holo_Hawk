import socket
import socketserver
import time
import hashlib
import datetime
import pickle
import copy
import block
import client
import RSAkey
import beidou_data
#import server    
    
#the mark of this raspberrypi
global mark_str
mark_str = '4'
global mark_int
mark_int  = 4

num_of_node = 4

host_list = ['192.168.1.111', '192.168.1.101', '192.168.1.100', '192.168.1.102','192.168.1.106 ']
port = 9000
#the address of this raspberry
host_server = host_list[mark_int]
port_server = 9000

#the break of creat a new block
global break_of_block
break_of_block = 3
# the breakof time
global break_of_time
break_of_time = 20
      
#public key distribute
global rsakey
rsakey = RSAkey.Key('0', '0')
rsakey.creat_selfkey()
rsakey.distribute_publickey(host_list, port, mark_str, num_of_node)
print('public key distribute finished')
time.sleep(break_of_time)

#receive the genesis block
global blockchain
global previous_block
block.broadcast_genesis_block(genesis_block, host_list[1], port, num_of_node)
previous_block = pickle.loads(data)
print(previous_block.index)
print(previous_block)
blockchain = [previous_block]
#the record of the block index
block_index = 0
#the record of the block hash
hash_check = previous_block.hash
time.sleep(break_of_time)

#the first server is raspberry 1
host = host_list[1]
key_num = 1

while True:

    while True:
        #run the client
        beidou_location = beidou_data.beidou_chuankou()
        beidou_location_str = str(beidou_location, encoding = "utf8")
        message_to_send = (beidou_location_str + "-" + mark_str).encode("utf8")
        
        
        encrypt_message_to_send = rsakey.encrypt_text(key_num, message_to_send)
        data = client.client(host, port, encrypt_message_to_send)
        
        previous_block = pickle.loads(data)
        previous_block.data = rsakey.decrypt_text(previous_block.data)
        previous_block.data = bytes.decode(previous_block.data)
        
        #decide whne to add the new block
        if previous_block.index != block_index:
            if previous_block.previous_hash == hash_check:
                #print(blockchain[block_index-1].previous_hash)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   print(blockchain[block_index-1].hash
                #add the last block to the blockchain
                blockchain.append(previous_block)
                #print the received block
                print(previous_block.index)
                print(previous_block)
                
            else:
                previous.blacklist_miner.append(miner_check)
                
        block_index = previous_block.index
        hash_check = previous_block.hash
        miner_check = previous_block.miner
        
        #ignore the server in the blacklist
        for blackminer in previous_block.blacklist_miner:
            if previous_block.next_miner == blackminer:
                if previous_block.next_miner == num_of_node:
                    previous_block.next_miner = 1
                else:
                    previous_block.next_miner += 1
            
        #change the server
        if previous_block.next_miner == 3:
            print("OK")
            break
        else:
            for i in range(1, num_of_node + 1):
                if previous_block.next_miner == i:
                    host = host_list[i]
                    key_num = i
                    time.sleep(break_of_time)
                    continue
     
     
    global num_of_gpsdata
    num_of_gpsdata = 0
    #run the server   
    class myTCPhandler(socketserver.BaseRequestHandler):
        def handle(self):
            while True:
                #try:
                    #receive the gpsdata
                    self.data = self.request.recv(4096)
                    if not self.data : break
                    data = rsakey.decrypt_text(self.data)
                    data = bytes.decode(data)
                    
                    managed_data = data.split("-")
                    gps_data = managed_data[0]
                    number_data = managed_data[1]
                    data_from = int(number_data)
                    print(data)
                    
                    if data_from != 0:
                        # to record how many data has been transfered
                        global num_of_gpsdata
                        num_of_gpsdata +=1
                            
                        # write the data to the file 'gps_data.txt'
                        with open('gps_data.txt','a') as f:
                            f.write(data + '\n')
                            f.close()
                        
                    global mark_str
                    global num_of_node
                    if num_of_gpsdata % (num_of_node - 1) == 0:
                        beidou_location = beidou_data.beidou_chuankou()
                        beidou_location_str = str(beidou_location, encoding = "utf8")
                        with open('gps_data.txt', 'a') as f:
                            f.write(beidou_location_str +'-' + mark_str + '\n')
                            f.close()
                        print(beidou_location_str + '-' + mark_str )
                        
                    # decide when to add a new block    
                    global break_of_block   
                    if num_of_gpsdata == break_of_block:
                        with open('gps_data.txt') as f:
                            contents = f.read()
                            f.close()
                        with open('gps_data.txt', 'w') as f:
                            f.truncate()
                            f.close()
                            
                        # Add blocks to the chain
                        global previous_block
                        global blockchain
                        block_to_add = block.next_block(previous_block,contents)
                        blockchain.append(block_to_add)
                        previous_block = block_to_add
                        # Tell everyone about it!
                        #print(previous_block.data)
                        print ("Block #{} has been added to the blockchain!".format(block_to_add.index))
                        print ("Hash: {}\n".format(block_to_add.hash))
                        
                        #send the block to the computer
                        data = client.client(host_list[0], 9000, pickle.dumps(previous_block))
                  
                        
                    #decied when to change the server    
                    if num_of_gpsdata == break_of_block or num_of_gpsdata == break_of_block + 1:
                        
                        #change the server to raspberry 1
                        previous_block.change_miner(1)
                        
                        #copy the previous_block
                        block_to_send = copy.deepcopy(previous_block)
                        
                        for i in range(1, num_of_node + 1):
                            if data_from == i:
                                block_to_send.data = str.encode(block_to_send.data)
                                block_to_send.data = rsakey.encrypt_text(i, block_to_send.data)
                        
                        
                        #send block to the client
                        packaged_block_to_send = pickle.dumps(block_to_send)
                        self.request.sendall(packaged_block_to_send)
                        
                        if num_of_gpsdata == break_of_block + 1:
                            with open('gps_data.txt', 'w') as f:
                                f.truncate()
                                f.close()
                            print('exit')
                            self.server.shutdown()
                            self.request.close()
                            break
                    
                    block_to_send = copy.deepcopy(previous_block)
                    for i in range(1, num_of_node + 1):
                        if data_from == i:
                            block_to_send.data = str.encode(block_to_send.data)
                            block_to_send.data = rsakey.encrypt_text(i, block_to_send.data)
                        
                    #send block to the client
                    packaged_block_to_send = pickle.dumps(block_to_send)
                    self.request.sendall(packaged_block_to_send)
                            
                #except Exception as e:
                    #self.server.shutdown()
                    #self.request.close()
                    #break
                    #continue
                
    #socketserver de duo jin cheng fei zu se qi dong
    # Activate the server; this will keep running until you interrupt the program with ctrl-c            
    server = socketserver.ThreadingTCPServer((host_server,port_server),myTCPhandler,bind_and_activate = False)
    server.allow_reuse_address = True
    server.server_bind()
    server.server_activate()
    server.serve_forever()
    
    
    #chang the address of setver
    host = host_list[1]
    key_num = 1
    #to record the last block's index hash and miner
    hash_check = previous_block.hash
    miner_check = previous_block.miner
    block_index = previous_block.index
    time.sleep(break_of_time + 10)
    


