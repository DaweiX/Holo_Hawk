import socket
import socketserver
'''
#client

host = '192.168.1.101'
port = 9000
sk = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sk.connect((host,port))
    
sk.sendall(("exit").encode("utf8"))
data = sk.recv(1024)
print(data)
time.sleep(2)
sk.close
'''
#server
class myTCPhandler(socketserver.BaseRequestHandler):
    def __init__(self, request, client_address, server):
        self.request = request
        self.client_address = client_address
        self.server = server
        self.setup()
        try:
            self.handle()
        finally:
            self.finish()
            
    def handle(self):
        while True:
            #try:
                self.data = self.request.recv(1024).decode('UTF8', 'ignore').strip()
                if not self.data : break
                print(self.data)

                    
                self.feedback_data =('hhh').encode("utf-8")
                self.request.sendall(self.feedback_data)
                if self.data == "exit":
                    print('exit')
                    self.server.shutdown()
                    self.request.close()
                    break
            #except Exception as e:
                #self.server.shutdown()
                #self.request.close()
                #break
                
 
host = '192.168.1.101'
port = 9000

#socketserver de duo jin cheng fei zu se qi dong
# Activate the server; this will keep running until you interrupt the program with ctrl-c
server = socketserver.ThreadingTCPServer((host,port),myTCPhandler,bind_and_activate = False)
server.allow_reuse_address = True
server.server_bind()
server.server_activate()
server.serve_forever()

