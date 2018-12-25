#!/usr/bin/env python2
#
# project to run classifier with hololens(camera capture srteam).
# 
# 2018/07/16
#
# Copyright 2018-2019 Beihang University
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# Contrib: yangzhi
# This example file shows to run a classifier on hololens capture stream. You need to
# run the classifier.py to generate classifier with your own dataset.
# To run this file from the openface home dir:
# ./demo/classifier_holo.py <path-to-your-classifier>



import time

start = time.time()

import argparse
import cv2
import os
import pickle
import sys

import numpy as np
np.set_printoptions(precision=2)
from sklearn.mixture import GMM
import openface

fileDir = os.path.dirname(os.path.realpath(__file__))
modelDir = os.path.join(fileDir, '..', 'models')
dlibModelDir = os.path.join(modelDir, 'dlib')
openfaceModelDir = os.path.join(modelDir, 'openface')

def getRep(bgrImg):
    start = time.time()
    if bgrImg is None:
        raise Exception("Unable to load image/frame")

    rgbImg = cv2.cvtColor(bgrImg, cv2.COLOR_BGR2RGB)

    if args.verbose:
        print("  + Original size: {}".format(rgbImg.shape))
    if args.verbose:
        print("Loading the image took {} seconds.".format(time.time() - start))

    start = time.time()

    # Get the largest face bounding box
    # bb = align.getLargestFaceBoundingBox(rgbImg) #Bounding box

    # Get all bounding boxes
    bb = align.getAllFaceBoundingBoxes(rgbImg)

    if bb is None:
        # raise Exception("Unable to find a face: {}".format(imgPath))
        return None
    if args.verbose:
        print("Face detection took {} seconds.".format(time.time() - start))

    start = time.time()

    alignedFaces = []
    for box in bb:
        alignedFaces.append(
            align.align(
                args.imgDim,
                rgbImg,
                box,
                landmarkIndices=openface.AlignDlib.OUTER_EYES_AND_NOSE))

    if alignedFaces is None:
        raise Exception("Unable to align the frame")
    if args.verbose:
        print("Alignment took {} seconds.".format(time.time() - start))

    start = time.time()

    reps = []
    for alignedFace in alignedFaces:
        reps.append(net.forward(alignedFace))

    if args.verbose:
        print("Neural network forward pass took {} seconds.".format(
            time.time() - start))

    # print (reps)
    return (reps,bb)


def infer(img, args):
    with open(args.classifierModel, 'r') as f:
        if sys.version_info[0] < 3:
                (le, clf) = pickle.load(f)  # le - label and clf - classifer
        else:
                (le, clf) = pickle.load(f, encoding='latin1')  # le - label and clf - classifer

    repsAndBBs = getRep(img)
    reps = repsAndBBs[0]
    bbs = repsAndBBs[1]
    persons = []
    confidences = []
    for rep in reps:
        try:
            rep = rep.reshape(1, -1)
        except:
            print ("No Face detected")
            return (None, None)
        start = time.time()
        predictions = clf.predict_proba(rep).ravel()
        # print (predictions)
        maxI = np.argmax(predictions)
        # max2 = np.argsort(predictions)[-3:][::-1][1]
        persons.append(le.inverse_transform(maxI))
        # print (str(le.inverse_transform(max2)) + ": "+str( predictions [max2]))
        # ^ prints the second prediction
        confidences.append(predictions[maxI])
        if args.verbose:
            print("Prediction took {} seconds.".format(time.time() - start))
            pass
        # print("Predict {} with {:.2f} confidence.".format(person.decode('utf-8'), confidence))
        if isinstance(clf, GMM):
            dist = np.linalg.norm(rep - clf.means_[maxI])
            print("  + Distance from the mean: {}".format(dist))
            pass
    return (persons, confidences ,bbs)


if __name__ == '__main__':

    parser = argparse.ArgumentParser()
    parser.add_argument(
        '--dlibFacePredictor',
        type=str,
        help="Path to dlib's face predictor.",
        default=os.path.join(
            dlibModelDir,
            "shape_predictor_68_face_landmarks.dat"))
    parser.add_argument(
        '--networkModel',
        type=str,
        help="Path to Torch network model.",
        default=os.path.join(
            openfaceModelDir,
            'nn4.small2.v1.t7'))
    parser.add_argument('--imgDim', type=int,
                        help="Default image dimension.", default=96)
    parser.add_argument('--threshold', type=float, default=0.83)
    parser.add_argument('--cuda', action='store_true')
    parser.add_argument('--verbose', action='store_true')
    parser.add_argument(
        'classifierModel',
        type=str,
        help='The Python pickle representing the classifier. This is NOT the Torch network model, which can be set with --networkModel.')

    args = parser.parse_args()

    align = openface.AlignDlib(args.dlibFacePredictor)
    net = openface.TorchNeuralNet(args.networkModel, imgDim=args.imgDim,
                                  cuda=args.cuda)
    import socket_holo as holo

    count = 0
    while True:
            confidenceList = []
            frame = holo.get_image_from_client() 
            matrix_buffer = holo.recieveMatrixString()
            if count == 1:
               count = 0
               continue
            count += 1
            persons, confidences, bbs = infer(frame, args)
            print ("P: " + str(persons) + " C: " + str(confidences))
            red = 125
            green = 233
            blue = 63
            num = 0
            names = 0
            try:
                # append with two floating point precision
                confidenceList.append('%.2f' % confidences[0])
            except:
                # If there is no face detected, confidences matrix will be empty.
                # We can simply ignore it.
                pass
          
            for i, c in enumerate(confidences):
                if persons[i] != "yangzhi" and persons[i] != "wenxiaoqing" and persons[i] != "liuhao" and persons[i] != "henan" :
                   continue
                num += 1
                if c <= args.threshold:  # 0.55 is kept as threshold for known face.
                    persons[i] = "_unknown"
                names += len(persons[i])
            print num , names 
            total_size = num*32 + names
            print total_size
            holo.sendDetectionsHeader(matrix_buffer,num,total_size)
            # Print the person name and conf value on the frame next to the person
            # Also print the bounding box
            for idx,person in enumerate(persons):
                length = len(person)
                print bbs[idx].left() , bbs[idx].top() , bbs[idx].right() , bbs[idx].bottom() 
                left = bbs[idx].left()
                top = bbs[idx].top()
                right = bbs[idx].right()
                bottom = bbs[idx].bottom() 
                if left < 0: 
                   left = 0
                if top < 0:
                   top = 0
                holo.sendDetectionBox(left, top, right, bottom, red, green, blue, length, person)
'''
        except:
            holo.connected_to_client = 0
            holo.client.close()
            print holo.connected_to_client
            continue
'''
'''
        for idx,person in enumerate(persons):
            cv2.rectangle(frame, (bbs[idx].left(), bbs[idx].top()), (bbs[idx].right(), bbs[idx].bottom()), (0, 255, 0), 2)
            cv2.putText(frame, "{} @{:.2f}".format(person, confidences[idx]),
                        (bbs[idx].left(), bbs[idx].bottom()+20), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 1)

        cv2.imshow('', frame)
        # quit the program on the press of key 'q'
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break
    # When everything is done, release the capture
'''
#    video_capture.release()
#    cv2.destroyAllWindows()
