﻿using System.Collections;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;


public class ServerThread : ThreadedJob
{
    private bool isConnected = false;
    public ServerThread()
    {
    }

    protected override void ThreadFunction()
    {
        //StartListening();
        ServerSocket();
        //ListeningServer();
        TCPListener();
    }

    void TCPListener()
    {
        TcpListener server = null;
        try
        {
            // Set the TcpListener on port 13000.
            Int32 port = 9999;
            IPAddress localAddr = IPAddress.Parse("192.168.0.102");

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;

            // Enter the listening loop.
            while (true)
            {
                Debug.Log("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                TcpClient client = server.AcceptTcpClient();
                Debug.Log("Connected!");

                data = null;

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Debug.Log("Received: " + data);

                    // Process the data sent by the client.
                    data = data.ToUpper();

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    Debug.Log("Sent: " + data);
                }

                // Shutdown and end connection
                client.Close();
            }
        }
        catch (SocketException e)
        {
           Debug.Log("SocketException: " + e);
        }
        finally
        {
            // Stop listening for new clients.
            server.Stop();
        }
    }

    protected override void OnFinished()
    {
    }

        // Incoming data from the client.  
        public static string data = null;

        public static void StartListening()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the   
            // host running the application.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress,9999);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.  
                while (true)
                {
                    Debug.Log("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();
                    data = null;

                    // An incoming connection needs to be processed.  
                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                   
                    }

                    // Show the data on the console.  
                    Debug.Log("Text received : " + data);

                    // Echo the data back to the client.  
                    byte[] msg = Encoding.ASCII.GetBytes(data);

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }


        }

        void ListeningServer()
    {
        Debug.Log("starting listening server");
        Socket listenSocket = new Socket(AddressFamily.InterNetwork,
                                       SocketType.Stream,
                                       ProtocolType.Tcp);

        // bind the listening socket to the port
        int port = 9999;
        int backlog = 10;
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
        listenSocket.Bind(ep);
        Debug.Log("binded");

        // start listening
        Debug.Log("listening");
        listenSocket.Listen(backlog);

        Debug.Log("about to accept");
        Socket s  = listenSocket.Accept();
        Debug.Log("accepted");
        SocketAsyncEventArgs e = new SocketAsyncEventArgs();

        byte[] bytes = new byte[256];
        Debug.Log("about to receive");
        int byteCount  = s.Receive(bytes,0,s.Available, SocketFlags.None);

        Debug.Log("byte count " + byteCount.ToString());
        if (byteCount > 0)
        {
            Debug.Log(Encoding.UTF8.GetString(bytes));
        }

    }

    void ServerSocket()
    {
        var Server = new UdpClient(9999);
        var ResponseData = Encoding.ASCII.GetBytes("Connected");

        while (!isConnected)
        {
            Debug.Log("waiting for client to be found");
            var ClientEp = new IPEndPoint(IPAddress.Any, 0);
            var ClientRequestData = Server.Receive(ref ClientEp);
            var ClientRequest = Encoding.ASCII.GetString(ClientRequestData);

            UnityEngine.Debug.Log("Received " + ClientRequest + " from " + ClientEp.Address.ToString() + " sending response");
            Server.Send(ResponseData, ResponseData.Length, ClientEp);
            if (ClientRequest == "!")
            {
                isConnected = true;
                Debug.Log("established connection");
            }
        }
        Debug.Log("closing server");
        Server.Close();

    }

    

    public virtual void close()
    {
        isConnected = false;
    }


}

public class ClientThread : ThreadedJob
{

    private bool isConnected = false;
    public ClientThread()
    {

    }

    protected override void ThreadFunction()
    {

        IPAddress serverAddress = ClientSocket();

        if(serverAddress != IPAddress.Parse("0.0.0.0"))
        {
            ConnectToServer(serverAddress);
        }

    }

    void ConnectToServer(IPAddress servAddr)
    {
        Debug.Log("establishing connection via socket");
        Socket s = new Socket(AddressFamily.InterNetwork,
         SocketType.Stream,
         ProtocolType.Tcp);
        s.Connect(servAddr,9999);
        Debug.Log("connected");
        isConnected = true;

        while(isConnected)
        {
            string msg = "ok";
            byte[] bytes = new byte[256];
            bytes = Encoding.UTF8.GetBytes(msg.ToCharArray());
            s.Send(bytes);
            Thread.Sleep(2000);
        }

        s.Close();




    }

    protected override void OnFinished()
    {
        isConnected = false;
    }

    IPAddress ClientSocket()
    {
        var Client = new UdpClient();
        var RequestData = Encoding.ASCII.GetBytes("!");
        var ServerEp = new IPEndPoint(IPAddress.Any, 0);

        Client.EnableBroadcast = true;
        Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 9999));
        Debug.Log("waiting to receive as client");
        //var ServerResponseData = Client.Receive(ref ServerEp);
        //var ServerResponse = Encoding.ASCII.GetString(ServerResponseData);

        var ServerResponseData = Client.ReceiveAsync();
        var ServerResponse = ServerResponseData.Result.Buffer.ToString();

        IPAddress targetAddr = ServerResponseData.Result.RemoteEndPoint.Address;

        UnityEngine.Debug.Log("Received " + ServerResponse + " from " + ServerResponseData.Result.RemoteEndPoint.Address.ToString());
        Client.Close();

        return targetAddr;

    }

    public virtual void close()
    {
        isConnected = false;
    }
}

public class SocketControl : MonoBehaviour
{
    ServerThread _server;
    ClientThread _client;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine("RunServer");
        }
        if(Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine("RunClient");
        }
    }

    IEnumerator RunClient()
    {
        UnityEngine.Debug.Log("starting client thread");
        _client = new ClientThread();
        _client.Start();
        yield return null;
    }

    IEnumerator RunServer()
    {
        UnityEngine.Debug.Log("starting server thread");
        _server = new ServerThread();
        _server.Start();
        yield return null;
    }
}
