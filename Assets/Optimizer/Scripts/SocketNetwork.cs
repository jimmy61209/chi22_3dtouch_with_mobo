using UnityEngine;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;

namespace OptScourcing
{
    public class SocketNetwork
    {
        Socket serverSocket;
        IPAddress ip;
        IPEndPoint ipEnd;
        Thread connectThread;
        public static float coverage = 0;
        public static float tempCoverage = 0;
        public SocketNetwork()
        {
        }
        public void InitSocket()
        {
            ip = IPAddress.Parse("127.0.0.1");
            ipEnd = new IPEndPoint(ip, 50007);

            connectThread = new Thread(new ThreadStart(SocketReceive));
            connectThread.Start();
        }
        void SocketReceive()
        {
            SocketConnet();
            SendInitInfo();
            //keep receiveing data
            while (true)
            {
                byte[] recvData = new byte[1024];
                int recvLen = serverSocket.Receive(recvData);
                if (recvLen == 0)
                {
                    Debug.Log("connection closed by server");
                    SocketQuit();
                    break;
                }
                string recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);

                ParseMessage(recvStr);

            }
        }
        void ParseMessage(string recvStr)
        {
            string[] strArr;
            strArr = recvStr.Split(',');
            // Debug.Log(strArr[0]);
            // Debug.Log(strArr[1]);
            // Debug.Log(strArr[2]);
            // Debug.Log(strArr[3]);
            if (strArr.Length != 0)
            {
                // Debug.Log(strArr[0]);
                if (strArr[0] == "parameters")
                {
                    List<string> strList = strArr.ToList();
                    strList.RemoveAt(0);
                    // Debug.Log(strList[0]);
                    List<float> floatList = strList.Select(float.Parse).ToList();
                    // Debug.Log(floatList[0]);

                    foreach (var pa in Optimizer.parameters)
                    {
                        // Debug.Log(pa.Value.optSeqOrder);
                        // Debug.Log($"{pa.Key} {floatList[pa.Value.optSeqOrder]}");
                        pa.Value.Value = floatList[pa.Value.optSeqOrder];
                    }
                    Debug.Log(Optimizer.getParameterValue("D"));
                    Optimizer.canRead = true;
                }
                if (strArr[0] == "coverage")
                {
                    coverage = Convert.ToSingle(strArr[1]);
                    Debug.Log($"coverage {coverage}");
                }
                if (strArr[0] == "tempCoverage")
                {
                    tempCoverage = Convert.ToSingle(strArr[1]);
                    Debug.Log($"tempCoverage {tempCoverage}");
                }
            }
        }

        void SocketConnet()
        {
            if (serverSocket != null)
                serverSocket.Close();
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Debug.Log("ready to connect");

            serverSocket.Connect(ipEnd);
        }

        void SendInitInfo()
        {
            string a = "";
            int i = 0;
            foreach (var pa in Optimizer.parameters)
            {
                pa.Value.optSeqOrder = i;
                i++;
                a += pa.Value.GetInitInfoStr();
                // Debug.Log(pa.Value.GetInitInfoStr());
            }
            a += "_";
            i = 0;
            foreach (var ob in Optimizer.objectives)
            {
                ob.Value.optSeqOrder = i;
                i++;
                a += ob.Value.GetInitInfoStr();
                // Debug.Log(ob.Value);
            }
            Debug.Log(a);
            SocketSend(a);
        }
        public void SendObjectives(List<float> finalObjectives)
        {
            string sendStr = "";
            for (int i = 0; i < finalObjectives.Count; i++)
            {
                sendStr += finalObjectives[i].ToString() + ",";
            }
            if (sendStr != "")
            {
                sendStr = sendStr.Remove(sendStr.Length - 1);
            }
            Optimizer.canRead = false;
            SocketSend(sendStr);
        }

        // send the objective directly, user no need to write extra code
        public void SendObjectives()
        {
            List<float> finalObjectives = new List<float>();
            foreach (KeyValuePair<string, ObjectiveArgs> objective in Optimizer.objectives)
            {
                finalObjectives.Add(objective.Value.GetAvargeObjective());
                objective.Value.clearTrial();
            }

            SendObjectives(finalObjectives);
        }

        public void SocketSend(string sendStr)
        {
            // byte[] sendData = new byte[1024];
            byte[] sendData = Encoding.ASCII.GetBytes(sendStr);
            Debug.Log("sender " + sendStr);
            serverSocket.Send(sendData, sendData.Length, SocketFlags.None);
        }

        public void SocketQuit()
        {
            //close thread
            if (connectThread != null)
            {
                connectThread.Interrupt();
                connectThread.Abort();
            }
            //close socket
            if (serverSocket != null)
                serverSocket.Close();
            Debug.Log("diconnect");
        }
    }


}