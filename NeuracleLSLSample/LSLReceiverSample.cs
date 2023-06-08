using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Neuracle.LSLSample
{
    class LSLReceiverSample
    {
        static void Main(string[] args)
        {
            //Construct a receiver
            //"7FFFFFFF" is the SerialNumber of an Neruacle device
            //Please contact Neruacle company for details
            NeuracleLSLReceiver neuracleLSLReceiver = new NeuracleLSLReceiver("7FFFFFFF_EEG");

            Console.WriteLine($"Start receive triggers...");
            Task.Run(() => neuracleLSLReceiver.ReceiveTrigger());

            Console.WriteLine($"Start receive biosim datas...");
            Task.Run(() => neuracleLSLReceiver.ReceiveBiosimData());

            Console.WriteLine($"Press any key to exit program");

            while (Console.KeyAvailable == false)
            {
                Thread.Sleep(10);
            }

            neuracleLSLReceiver.StopReceiveTrigger();
            neuracleLSLReceiver.StopReceiveBiosimData();
        }
    }

    public class NeuracleLSLReceiver
    {
        private string _triggerStreamName = "Neuracle";
        private string _triggerStreamType = "Markers";

        private string _biosimDataStreamName = "";
        private string _biosimDataStreamType = "EEG";

        private ManualResetEvent _stopReceiveTriggerEvent = new ManualResetEvent(false);
        private ManualResetEvent _stopReceiveDataEvent = new ManualResetEvent(false);
        public NeuracleLSLReceiver(string biosimDataStreamName,
                                   string biosimDataStreamType = "EEG",
                                   string triggerStreamName = "Neuracle",
                                   string triggerStreamType = "Markers")
        {
            Debug.Assert(biosimDataStreamName != "");
            Debug.Assert(biosimDataStreamType != "");
            Debug.Assert(triggerStreamName != "");
            Debug.Assert(triggerStreamType != "");

            _biosimDataStreamName = biosimDataStreamName;
            _biosimDataStreamType = biosimDataStreamType;
            _triggerStreamName = triggerStreamName;
            _triggerStreamType = triggerStreamType;
        }

        /// <summary>
        /// Interface to stop receive Trigger
        /// </summary>
        public void StopReceiveTrigger()
        {
            _stopReceiveTriggerEvent.Set();
        }

        /// <summary>
        /// Interface to stop receive biosim data
        /// </summary>
        public void StopReceiveBiosimData()
        {
            _stopReceiveDataEvent.Set();
        }

        /// <summary>
        /// Demostrate how to Receive Trigger from Neuracle device
        /// </summary>
        public void ReceiveTrigger()
        {
            Console.Write($"Enter ReceiveTrigger()...");

            _stopReceiveTriggerEvent.Reset();

            StreamInfo[] streamInfos = LSL.Resolve_streams();
            StreamInlet triggerStreamInlet = null;

            for (int i = 0; i < streamInfos.Length; i++)
            {
                string name = streamInfos[i].Name();
                string type = streamInfos[i].Type();

                if (name == _triggerStreamName || type == _triggerStreamType)
                {
                    triggerStreamInlet = new StreamInlet(streamInfos[i]);
                    break;
                }
            }
            if (triggerStreamInlet == null)
            {
                Console.WriteLine($"Cannot find trigger outlet, exit ReceiveTrigger()");
                return;
            }

            streamInfos.DisposeArray();
            Console.WriteLine(triggerStreamInlet.Info().As_xml());

            string[,] samples = new string[1, 1];
            double[] timestamps = new double[1];

            while (false == _stopReceiveTriggerEvent.WaitOne(1))
            {
                triggerStreamInlet.Pull_chunk(samples, timestamps, LSL.FOREVER);

                //output the annotation and timestamp of the received trigger
                Console.WriteLine(samples[0, 0] + " : " + timestamps[0].ToString());
            }

            Console.WriteLine($"Exit ReceiveTrigger() ...");
        }

        /// <summary>
        /// Demostrate how to Receive Biosim data from Neuracle device
        /// according to the Data type
        /// </summary>
        public void ReceiveBiosimData()
        {
            // wait until an EEG stream shows up
            StreamInfo[] results = LSL.Resolve_stream("type", _biosimDataStreamType);

            Console.Write($"Enter ReceiveBiosimData()...");

            _stopReceiveDataEvent.Reset();

            bool foundTargetStream = false;

            int channelCount = 0;
            int dataCount = 5;
            double sampleRate = 0.0;

            StreamInlet biosimDataInlet = null;

            try
            {
                for (int i = 0; i < results.Length; i++)
                {
                    string name = results[i].Name();
                    string type = results[i].Type();

                    if (name != _biosimDataStreamName)
                    {
                        continue;
                    }

                    channelCount = results[i].Channel_count();
                    sampleRate = results[i].Nominal_srate();

                    // open an inlet, with post-processing enabled, and print meta-data
                    // Note: The use of post-processing makes it impossible to recover
                    // the original timestamps and is not recommended for applications
                    // that store data to disk.
                    biosimDataInlet = new StreamInlet(results[i], postproc_flags: Processing_options_t.proc_ALL);
                    Console.WriteLine(biosimDataInlet.Info().As_xml());

                    //You can tranvers the description xmlElement for the meta information
                    //For the structure of the xml tree, please refer to the above As_xml() result
                    //XMLElement descXML = biosimDataInlet.Info().Desc();

                    results.DisposeArray();
                    foundTargetStream = true;
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                results.DisposeArray();
                return;
            }

            if (foundTargetStream == false)
            {
                Console.WriteLine($"Cannot find the target LSL source {_biosimDataStreamName}");
                results.DisposeArray();
                return;
            }

            results.DisposeArray();
#if DEBUG
            //CSV file to save the biosim data for debugging
            StreamWriter _sw = new StreamWriter($".\\{_biosimDataStreamName}_LSLSubscriber.csv", false);
#endif

            // read samples
            float[,] buffer = new float[dataCount, channelCount];
            double[] timestamps = new double[dataCount];
            while (false == _stopReceiveDataEvent.WaitOne(1))
            {
                int num = biosimDataInlet.Pull_chunk(buffer, timestamps);

                //Console.WriteLine($"Received {timestamps.Length}ms datas");

#if DEBUG
                //Dump the biosim data to csv file
                string dumpString = "";
                for (int s = 0; s < num; s++)
                {
                    dumpString = ((int)(timestamps[s]*1000)).ToString() + ",";
                    for (int c = 0; c < channelCount; c++)
                    {
                        //Console.Write("\t{0}", buffer[s, c]);
                        dumpString += buffer[s, c].ToString();
                        if (c < channelCount - 1)
                        {
                            dumpString += ",";
                        }
                    }
                    _sw.WriteLine(dumpString);
                    _sw.Flush();
#endif
                }
            }
        }
    }
}
