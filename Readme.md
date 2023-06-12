# Neuracle LSL Sample user's manual

## IDE
 1.  VisualStudio 2022
  
 2. .net 4.8

## Compile
1. clone existing repository，open the solution with Visual Studio 2022，it will work after compilation.

## Configure the environment
1. Configure the hardware environment to start Neuracle.
2. Start Neuracle's computer software(Jellyfish)，and turn on the LabSTreamingLayer forwarding function for the target device，keep the software run in "Formal Acquisition" mode.Please contact Neuracle. Inc for details.
   Note：The serial number of the "target device" should match the parameters used in this sample project to create the NeuracleLSLReceiver instance，please refer to the comments in Sample Code for details. The reason for this convention is that the Neuracle multi-parameter system can support simultaneous acquisition by multiple devices and LabStreamingLayer forwarding.

## Additional notes for the Sample Project
1. The Neuracle multi-parameter system can acquire signals from multiple devices simultaneously,and LabStreamingLayer forwarding at the same time，and trigger will also be forwarded。
2. As the receiving side of LabStreamingLayer, multiple LabStreamingLayer inlet instances can be used to receive a variety of data forwarded by the Neuracle multi-parameter system at the same time.
3. It can also receive its forwarded Trigger data.
4. This sample project provides examples of:
   1. For how to receive Triggers forwarded by the Neuracle multiparameter system, refer to Class NeuracleLSLReceiver's ReceiveTrigger() method
   2. For how to receive EEG data forwarded by the Neuracle multi-parameter system, refer to Class NeuracleLSLReceiver's ReceiveBiosimData() method

## 5.Additional support
Please contact Neuracle official: http://www.neuracle.cn/