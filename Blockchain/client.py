import socket
import time

def client(host, port, message):
    while True:
        try:
            sk = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sk.connect((host,port))
               
            sk.sendall(message)
            data = sk.recv(1024)
            #print(data)
            time.sleep(20)
            sk.close
            return(data)
        except Exception as e:
            continue
