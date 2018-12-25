#接受北斗串口数据并按一定格式socket传到接收端
import serial
import time
import re
import socket
import time
import sys

def beidou_chuankou():
    ser = serial.Serial('/dev/ttyUSB0', 9600)
    if ser.isOpen == False:
        ser.open()
    try:
        while True:
            count = ser.inWaiting()  #获得总长度
            try:
                if count != 0:
                    recv = ser.read(count)  #通过ser的read()方法只能获取一个字符，但read()方法有一个接收字符长度的重载
                    #recv_s = bytes.decode(recv)    #bytes to str
                    recv_s = recv.decode('UTF-8', 'ignore')  #防止Unicodeerror报错
                    matchObj1 = re.search('(?<=\$GNRMC).*(?=V)', recv_s)
                    if matchObj1:
                        #matchObj2 = matchObj2.raplace(" "," ")
                        matchObj2 = re.search( '(?<=' + matchObj1.group() + 'V,' + ').*?(?=E)' ,recv_s)
                    if matchObj2:
                        #print ("%sE" % (matchObj2.group()))
                        return matchObj2.group().encode()
                        time.sleep(0.1)
            except (ConnectionResetError, TimeoutError, FileNotFoundError,OSError, UnboundLocalError):
                continue

            ser.flushInput()
            time.sleep(0.1)

    except KeyboardInterrupt:
        if ser != None:
            ser.close()

""" def get_beidou_data():
    try:
        return beidou_chuankou()

    #ConnectionResetError:peer disconnect
    #TimeoutError:long time no connect

    except (ConnectionResetError, TimeoutError, FileNotFoundError, OSError, UnboundLocalError):
        return beidou_chuankou()


    #FileNotFoundError:raspberry not connect usb or connect the wrong interface
    #OSError:not the same wifi """
'''
while True:
    a = beidou_chuankou()
    print (a)
'''