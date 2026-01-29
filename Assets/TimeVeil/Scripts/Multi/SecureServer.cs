using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.TLS;
using Unity.Networking.Transport.Utilities;
using UnityEngine;

/// <summary>
/// Low-level Server implementation for Meta Quest 3.
/// Handles DTLS encryption, packet fragmentation, and reliable/unreliable pipelines.
/// </summary>
public class SecureServer : MonoBehaviour
{
    private ServerGameManager m_ServerGameManager;

    private NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

    [Header("Debug Settings")]
    [SerializeField][Tooltip("Enable Debug Log Message)")] private bool m_EnableDebug = true;

    [Header("Network Settings")]
    [SerializeField][Tooltip("Display current IP Adresse of the server")] private string m_IP = "0.0.0.0";
    [SerializeField][Tooltip("Port use for bind the server")] private ushort m_Port = 12000;
    [SerializeField][Tooltip("Maximum message size per packet")] private int m_MaxMessageSize = 1472;

    [Header("NetworkSimulator Settings")]
    [SerializeField][Tooltip("Enable NetworkSimulator")] private bool m_UseNetworkSimulator = false;
    [SerializeField][Tooltip("The number of packets that can be delayed at any given time. Past that, packets will go through without any delay added")] private int m_MaxPacketCount = 0;
    [SerializeField][Tooltip("In which direction the simulator should apply the network conditions (send, receive, or both)")] private ApplyMode m_mode = ApplyMode.Off;
    [SerializeField][Tooltip("Delay in milliseconds to be applied to packets. Good values to use would go from 20 for a good broadband connection, up to 200 for bad mobile connections")] private int m_MaxPacketDelay = 0;
    [SerializeField][Tooltip("Deviation around the delay. Typically half of the delay or slightly less")] private int m_MaxJitterMs = 0;
    [SerializeField][Tooltip("Percentage of packets to drop. This should rarely be above 3 even for bad mobile connections")] private int m_MaxDropPercentage = 0;
    [SerializeField]
    [Tooltip("Fixed interval to drop packets on. This is most suitable for tests where predictable behaviour is desired," +
    " as every X-th packet will be dropped. For example, if the value is 5 every fifth packet is dropped")]
    private int m_MaxPacketInterval = 0;

    [Header("Security Settings")]
    [SerializeField] private bool m_UseEncryption = true;
    [SerializeField][Tooltip("The public certificate (.pem string)")][TextArea(5, 10)] private string m_CertificatePem;
    [SerializeField][Tooltip("The private key (.key string)")][TextArea(5, 10)] private string m_PrivateKeyPem;

    [Header("Pipelines")]
    private NetworkPipeline m_ReliableFragmentedPipeline;
    private NetworkPipeline m_UnReliablePipeline;

    void Start()
    {
        // --- 0. LOGIC CONNECTION BTW SERVER AND MANAGER ---

        m_ServerGameManager = ServerGameManager.Instance;

        m_ServerGameManager.DataUnreliableSendEvent.AddListener(DataSendUnReliable);

        m_ServerGameManager.DataReliableSendEvent.AddListener(DataSendReliable);
        
        // --- 1. NETWORK SETTINGS ---
        NetworkSettings settings = new NetworkSettings();

        // MaxPayloadSize must be large enough to allow Fragmentation to work
        settings.WithNetworkConfigParameters(maxMessageSize: m_MaxMessageSize);

        settings.WithSimulatorStageParameters(
            maxPacketCount: m_MaxPacketCount,
            mode: m_mode,
            packetDelayMs: m_MaxPacketDelay,
            packetJitterMs: m_MaxJitterMs,
            packetDropPercentage: m_MaxDropPercentage,
            packetDropInterval: m_MaxPacketInterval);

        // Configure DTLS Encryption
        if (m_UseEncryption)
        {
            if (string.IsNullOrEmpty(m_CertificatePem) || string.IsNullOrEmpty(m_PrivateKeyPem))
            {
                if (m_EnableDebug)
                    Debug.LogError("Encryption enabled but certificates are missing!");
                return;
            }
            settings.WithSecureServerParameters(m_CertificatePem, m_PrivateKeyPem);
        }

        // --- 2. DRIVER INITIALIZATION ---
        m_Driver = NetworkDriver.Create(settings);
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        // --- 3. PIPELINE DEFINITION ---

        // Reliable + Fragmented: For large and ensure data.

        var ReliableFragmentedStages = new System.Collections.Generic.List<System.Type>
        {
            typeof(FragmentationPipelineStage),
            typeof(ReliableSequencedPipelineStage)
        };

        if (m_UseNetworkSimulator)
            ReliableFragmentedStages.Add(typeof(SimulatorPipelineStage));

        m_ReliableFragmentedPipeline = m_Driver.CreatePipeline(ReliableFragmentedStages.ToArray());


        // Unreliable: For fast data, No fragmentation.

        var UnReliablePipelineStages = new System.Collections.Generic.List<System.Type>
        {
            typeof(UnreliableSequencedPipelineStage),
            typeof(SimulatorPipelineStage)
        };

        if (m_UseNetworkSimulator)
            UnReliablePipelineStages.Add(typeof(SimulatorPipelineStage));

        m_UnReliablePipeline = m_Driver.CreatePipeline(UnReliablePipelineStages.ToArray());

        // --- 4. BINDING ---
        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(m_Port);
        if (m_Driver.Bind(endpoint) != 0)
        {
            if (m_EnableDebug)
                Debug.LogError($"Server: Failed to bind to port {m_Port}.");
        }
        else
        {
            m_Driver.Listen();
        }

        // --- 4. GET IP ADDRESS ---
        IPHostEntry host;
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                m_IP = ip.ToString();
                break;
            }
        }

        if (m_EnableDebug)
            Debug.Log($"Server: Started on IP {m_IP} and port {m_Port} (Secure : {m_UseEncryption})");
    }

    void OnDestroy()
    {
        if (m_Driver.IsCreated)
        {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }
    }

    void Update()
    {
        if (!m_Driver.IsCreated) return;

        m_Driver.ScheduleUpdate().Complete();

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // Accept new clients
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default)
        {
            m_Connections.Add(c);
            m_ServerGameManager.ClientConnectionEvent.Invoke();

            Debug.Log("Server: New client connected.");
        }

        // Process Events
        for (int i = 0; i < m_Connections.Length; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    DataReceive(stream);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    if (m_EnableDebug)
                        Debug.Log("Server: Client disconnected.");

                    m_Connections[i] = default;
                }
            }
        }
    }


    /// <summary>
    /// Method to receive data string
    /// </summary>
    public void DataReceive(DataStreamReader stream)
    {
        try
        {
            // Remaining unread bytes in this frame
            int unread = stream.Length - stream.GetBytesRead();
            // Need at least 4 bytes for the length header
            if (unread < sizeof(uint)) return;
        }
        catch (System.Exception)
        {
            return;
        }

        // Payload length (uint32)
        uint byteCount = stream.ReadUInt();

        // Read payload bytes
        var bytes = new NativeArray<byte>((int)byteCount, Allocator.Temp);
        stream.ReadBytes(bytes);

        // Decode UTF-8 to string
        string data = Encoding.UTF8.GetString(bytes.ToArray());

        m_ServerGameManager.DataReceiveEvent.Invoke(data);

        if (m_EnableDebug)
            Debug.Log("Server: Receive String: " + data);
    }

    /// <summary>
    /// Method to send reliable large data string using the fragmentation pipeline.
    /// </summary>
    public void DataSendReliable(string data, int client)
    {
        if (client >= m_Connections.Length || !m_Connections[client].IsCreated)
        {
            if (m_EnableDebug)
                Debug.LogWarning($"Server: Cannot send to client {client} - not connected. Connections count: {m_Connections.Length}");
            return;
        }
        
        if (m_Driver.BeginSend(m_ReliableFragmentedPipeline, m_Connections[client], out var writer) == 0)
        {
            // Convert String in UTF8
            var bytes = System.Text.Encoding.UTF8.GetBytes(data);

            // Write Lenght
            writer.WriteUInt((uint)bytes.Length);

            // Write Bytes
            writer.WriteBytes(bytes);

            m_Driver.EndSend(writer);

            if (m_EnableDebug)
                Debug.Log("Server : Send data : " + data);
        }
    }

    /// <summary>
    /// Method to send unreliable small data string
    /// </summary>
    public void DataSendUnReliable(string data, int client)
    {
        if (m_Driver.BeginSend(m_UnReliablePipeline, m_Connections[client], out var writer) == 0)
        {
            // Convert String in UTF8
            var bytes = System.Text.Encoding.UTF8.GetBytes(data);

            // Write Lenght
            writer.WriteUInt((uint)bytes.Length);

            // Write Bytes
            writer.WriteBytes(bytes);

            m_Driver.EndSend(writer);

            if (m_EnableDebug)
                Debug.Log("Server : Send data : " + data);
        }
    }
}
