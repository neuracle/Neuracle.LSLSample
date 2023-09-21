#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
@Time ： 2023/9/20 16:56
@Author ： FANG Junying
@Email ： fangjunying@neuracle.cn,fjy11193102@foxmail.com
@File ：LSLReceiverSample.py
@ Versions:
 	v1.0: 2023-09-20，read lsl streamer
@ Copyright (c) 2016 Neuracle, Inc. All Rights Reserved. http://neuracle.cn/
"""

from pylsl import StreamInlet, resolve_stream
import matplotlib.pyplot as plt
import numpy as np
def readMarkersStreamer():
    # first resolve a marker stream on the lab network
    print("looking for a marker stream...")
    streams = resolve_stream('type', 'Markers')

    # create a new inlet to read from the stream
    inlet = StreamInlet(streams[0])
    trg = []
    while True:
        # get a new sample (you can also omit the timestamp part if you're not
        # interested in it)
        sample, timestamp = inlet.pull_sample()
        print("got %s at time %s" % (sample[0], timestamp))
        thisTrg = int(sample[0])
        trg.append(thisTrg)
        #### a condition used to break loop, you can modify it
        if thisTrg == 255:
            break
    print('*** Number of trigger = {0}\n'.format(len(trg)))

def readEEGStreamer():
    # first resolve an EEG stream on the lab network
    streams = resolve_stream()
    streamsType = []
    for info in streams:
        print('Name:{0}, Type: {1}, Channel counter:{2}, Srate:{3}'.format(info.name(), info.type(), info.channel_count(), info.nominal_srate()))
        streamsType.append(info.type())
    eeg = []
    srate = 0
    nchan = 0
    print("looking for an EEG stream...")
    if 'EEG' in streamsType:
        streams = resolve_stream('type', 'EEG')
        srate = streams[0].nominal_srate()
        nchan = streams[0].channel_count()
        # create a new inlet to read from the stream
        inlet = StreamInlet(streams[0])
        i = 0
        NB = 1E4 ### a condition used to stop loop, you can modify it
        while i < NB:
            # get a new sample (you can also omit the timestamp part if you're not
            # interested in it)
            sample, timestamp = inlet.pull_sample()
            # print(timestamp, sample)
            eeg.append(sample)
            # chunk, timestamp =inlet.pull_chunk()
            i = i + 1
            if i % 1000 == 0:
                print('i={0}'.format(i))
        return eeg, srate, nchan

    else:
        return eeg, srate, nchan

def readOtherStreamer():
    # first resolve other non-EEG streams on the lab network, for example, SpO2, ECG, GSR,MEMsAll
    print("looking for all streams...")
    streams = resolve_stream()
    streamsType = []
    for info in streams:
        print('Name:{0}, Type: {1}, Channel counter:{2}, Srate:{3}'.format(info.name(), info.type(), info.channel_count(), info.nominal_srate()))
        streamsType.append(info.type())
    raw = []
    srate = 0
    nchan = 0
    ## here is an example for reading SpO2 data stream
    print("looking for an SpO2 stream...")
    if 'SpO2' in streamsType:
        streams = resolve_stream('type', 'SpO2')
        srate = streams[0].nominal_srate()
        nchan = streams[0].channel_count()
        # create a new inlet to read from the streamer
        inlet = StreamInlet(streams[0])
        i = 0
        NB = 1E3  ### a condition used to stop loop, you can modify it
        while i < NB:
            # get a new sample (you can also omit the timestamp part if you're not
            # interested in it)
            sample, timestamp = inlet.pull_sample()
            # print(timestamp, sample)
            raw.append(sample)
            i = i + 1
            if i % 100 == 0:
                print('i={0}'.format(i))
        return raw, srate, nchan

    else:
        return raw, srate, nchan




if __name__ == '__main__':
    ### pay attention to : EEG montage can be found from jellyfish software

    # Example1: read EEG streamer
    eeg, srate_eeg, nchan_eeg = readEEGStreamer()
    if len(eeg) > 0:
        eeg = np.asarray(eeg)
        t_eeg = np.arange(eeg.shape[0])/srate_eeg
        plt.figure()
        plt.plot(t_eeg, eeg[:,0])# plot first channel
        plt.show()

    # Example2: read markers streamer,
    # here you have to send trigger, to continue program
    readMarkersStreamer()

    # Example3: read SpO2 streamer
    raw,srate_raw, nchan_raw = readOtherStreamer()
    if len(raw) > 0:
        raw = np.asarray(raw)
        t_raw = np.arange(raw.shape[0]) / srate_raw
        plt.figure()
        plt.plot(t_raw, raw[:, 1])  # plot second channel
        plt.show()
