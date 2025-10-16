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
using Unity.VisualScripting;
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

    // Eventos
    public delegate void UnityTcpServerEvent(params object[] args);
    public event UnityTcpServerEvent OnClientConnected;
    public event UnityTcpServerEvent OnClientDisconnected;
    public event UnityTcpServerEvent OnServerStarted;
    public event UnityTcpServerEvent OnServerDisconnected;
    public event UnityTcpServerEvent OnMessageArrived;

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
        try { OnServerStarted?.Invoke(port); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - UnityTcpServer > StartServer() > OnServerStarted error: " + e); }
    }

    public void StopServer()
    {
        running = false;

        try
        {
            // Fecha o listener
            server?.Stop();
        }
        catch { }

        // Fecha todos os clientes conectados
        lock (clients)
        {
            foreach (var client in clients)
            {
                try { client.Close(); } catch { }
            }
            clients.Clear();
        }

        // Espera a thread encerrar graciosamente
        if (serverThread != null && serverThread.IsAlive)
        {
            serverThread.Join(100); // espera até 100ms
        }

        Debug.Log("UnityTcpServer > Servidor TCP parado.");
        try { OnServerDisconnected?.Invoke(); }
        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - UnityTcpServer > StopServer() > OnServerDisconnected error: " + e); }
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
                    Debug.Log("UnityTcpServer > ServerLoop() > Novo cliente conectado: " + client.Client.RemoteEndPoint + ".");
                    try { OnClientConnected?.Invoke(client.Client.RemoteEndPoint); }
                    catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - UnityTcpServer > ServerLoop() > OnClientConnected error: " + e); }
                }

                List<TcpClient> disconnected = new List<TcpClient>();

                lock (clients)
                {
                    foreach (var client in clients)
                    {
                        Socket socket = client.Client;
                        if (socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0)
                        {
                            disconnected.Add(client);
                            continue;
                        }

                        NetworkStream stream = client.GetStream();
                        if (stream.DataAvailable)
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                            // Evita log se o Unity já estiver fechando
                            if (running)
                            {
                                Debug.Log("UnityTcpServer > ServerLoop() > Recebido do cliente: " + msg);
                                try { OnMessageArrived?.Invoke(msg); }
                                catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - UnityTcpServer > ServerLoop() > OnMessageArrived error: " + e); }
                            }
                        }
                    }

                    foreach (var dc in disconnected)
                    {
                        clients.Remove(dc);
                        Debug.Log("UnityTcpServer > ServerLoop() > Cliente desconectado: " + dc.Client.RemoteEndPoint);
                        dc.Close();
                        try { OnClientDisconnected?.Invoke(dc.Client.RemoteEndPoint); }
                        catch (Exception e) { Debug.LogError(Time.time.ToString("F3") + " - UnityTcpServer > ServerLoop() > OnClientDisconnected error: " + e); }
                    }
                }

                Thread.Sleep(10);
            }
        }
        catch (SocketException se)
        {
            if (running) // ignora erros de parada normal
                Debug.LogError("UnityTcpServer > Erro no socket: " + se.Message);
        }
        catch (Exception e)
        {
            if (running)
                Debug.LogError("UnityTcpServer > Erro no servidor: " + e.Message);
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

    //void OnDisable()
    //{
    //    StopServer();
    //}

}