from Crypto.Cipher import PKCS1_v1_5 as Cipher_pkcs1_v1_5
import base64
from Crypto import Random
from Crypto.PublicKey import RSA
import socketserver
import client 
import time 
 
class Key:
    def __init__(self, public, private):
        self.public = public
        self.private = private
        self.public_all = ['0', ' ', ' ',' ']
        
    def creat_selfkey(self):
        random_generator = Random.new().read
        rsa = RSA.generate(3072, random_generator)

        #creat the key
        self.private= rsa.exportKey()
        self.public = rsa.publickey().exportKey()
        self.public_all[3] = self.public
   
    def distribute_publickey(self, host, port, mark， num_of_node):
        global public_key
        public_key = self.public
        
        message = ("received" + "-" + mark).encode("utf8")
        self.public_all[1] = client.client(host[1], port, message)
        #print(self.public_all[1])
        
        message = ("received" + "-" + mark).encode("utf8")
        self.public_all[2] = client.client(host[2], port, message)
        #print(self.public_all[2])
        
        global num
        num = 0
        class myTCPhandler(socketserver.BaseRequestHandler):
            def handle(self):
                while True:
                    #try:
                    #receive the gpsdata
                    self.data = self.request.recv(4096).decode('UTF8', 'ignore').strip()
                    if not self.data : break
                    print(self.data)
                  
                    
                    global num
                    num += 1
        
                    global public_key
                    self.request.sendall(public_key)
                    
                    if num == (num_of_node - 1):
                        self.server.shutdown()
                        self.request.close()
                        break
                        
                
      
                
        server = socketserver.ThreadingTCPServer((host[3],port),myTCPhandler,bind_and_activate = False)
        server.allow_reuse_address = True
        server.server_bind()
        server.server_activate()
        server.serve_forever()
        
        message = ('received'+ "-" + mark).encode("utf8")
        self.public_all[4]= client.client(host[4], port, message)


    def encrypt_text(self, num, text):
        rsakey = RSA.importKey(self.public_all[num])
        cipher = Cipher_pkcs1_v1_5.new(rsakey)
        encrypted_text = base64.b64encode(cipher.encrypt(text))
        return encrypted_text
        
    def decrypt_text(self,text):
        rsakey = RSA.importKey(self.private)
        cipher = Cipher_pkcs1_v1_5.new(rsakey)
        decrypted_text = cipher.decrypt(base64.b64decode(text), "error")
        return decrypted_text      

        
