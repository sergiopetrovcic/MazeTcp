/*

Baixe o PuTTY (não precisa instalar, pode usar a versão .exe portátil): https://www.putty.org/
Abra o PuTTY → escolha Connection type = Raw → coloque:
Host Name: 127.0.0.1
Port: 5005
Clique em Open → vai abrir um terminal interativo.
Tudo que você digitar será enviado ao servidor Unity.
*/

/*
Exemplo de comando json {"name":"robo1","command":"FORWARD","value":1.0}
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UnityTcpServer : MonoBehaviour
{
    //public List<RobotController> robots;

    //public RobotCommandStructure commandMessage;

    public int port = 5005;

    private TcpListener server;
    private Thread serverThread;
    private List<TcpClient> clients = new List<TcpClient>();
    private bool running = false;

    void Start()
    {
        Application.runInBackground = true; // Força o Unity a executar Update mesmo sem foco na aba Scene

        // Testa envio de comando via json
        //commandMessage = new RobotCommandStructure();
        //Debug.Log("UnityTcpServer > Command: " + commandMessage.command + " & value: " + commandMessage.value);
        //commandMessage = JsonUtility.FromJson<RobotCommandStructure>("{\"command\":\"JUMP\",\"value\":1.0}"); 
        //Debug.Log("UnityTcpServer > Command: " + commandMessage.command + " & value: " + commandMessage.value);

        StartServer();

        //foreach (RobotController robot in robots)
        //{
        //    robot.Init(this);
        //}

        //InvokeRepeating(nameof(BroadcastPlayerStates), 0f, 1f);
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    public void StartServer()
    {
        running = true;
        serverThread = new Thread(ServerLoop);
        serverThread.Start();
        Debug.Log("UnityTcpServer > Servidor TCP iniciado na porta " + port);
    }

    public void StopServer()
    {
        running = false;
        foreach (var client in clients)
            client.Close();

        server?.Stop();
        serverThread?.Abort();
        Debug.Log("UnityTcpServer > Servidor TCP parado.");
    }

    private void ServerLoop()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            while (running)
            {
                if (server.Pending())
                {
                    TcpClient client = server.AcceptTcpClient();
                    lock (clients) clients.Add(client);
                    Debug.Log("UnityTcpServer > Novo cliente conectado: " + client.Client.RemoteEndPoint + ".");
                }

                List<TcpClient> disconnected = new List<TcpClient>();

                lock (clients)
                {
                    foreach (var client in clients)
                    {
                        if (!client.Connected)
                        {
                            disconnected.Add(client);
                            continue;
                        }

                        NetworkStream stream = client.GetStream();
                        // Trata a mensagem recebida
                        if (stream.DataAvailable)
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            Debug.Log("UnityTcpServer > Recebido do cliente: " + msg);
                            //foreach (RobotController robot in robots)
                            //{
                            //    robot.EnqueueCommandFromNetwork(msg);
                            //}
                        }
                    }
                    foreach (var dc in disconnected)
                        clients.Remove(dc);
                }
                Thread.Sleep(10);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("UnityTcpServer > " + "Erro no servidor: " + e.Message);
        }
    }

    public void BroadcastPlayerStates()
    {
        //foreach (RobotController robot in robots)
        //{
        //    string stateJson = robot.GetStatusJson();
        //    Broadcast(stateJson);
        //}
    }

    // Enviar broadcast para todos os clientes
    public void Broadcast(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        lock (clients)
        {
            foreach (var client in clients)
            {
                if (client.Connected)
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
            }
        }
    }

    // Exemplo: enviar estado do player a cada frame
    void Update()
    {
        //Vector3 pos = transform.position; // por exemplo, posição do GameObject
        //Broadcast($"STATE,{pos.x},{pos.y},{pos.z}");
    }
}