using System.Data;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.TLS;
using Unity.Networking.Transport.Utilities;
using UnityEngine;

/// <summary>
/// Low-level Client implementation.
/// Connects to the Quest 3 Host using encryption and matching pipelines.
/// </summary>
public class SecureClient : MonoBehaviour
{
    private ClientGameManager m_ClientGameManager;

    private NetworkDriver m_Driver;
    private NetworkConnection m_Connection;


    [Header("Debug Settings")]
    [SerializeField][Tooltip("Enable Debug Log Message)")] private bool m_EnableDebug = true;

    [Header("Network Settings")]
    public string serverIP = "127.0.0.1";
    public ushort port = 12000;

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

    [Header("Security")]
    public bool useEncryption = true;
    [Tooltip("The public certificate of the server (.pem string)")]
    [TextArea(5, 10)] public string certificatePem;
    public string serverCommonName = "localhost"; // Must match OpenSSL CN

    [Header("Pipelines")]
    private NetworkPipeline m_ReliableFragmentedPipeline;
    private NetworkPipeline m_UnReliablePipeline;

    void Start()
    {
        m_ClientGameManager = ClientGameManager.Instance;

        m_ClientGameManager.DataUnreliableSendEvent.AddListener(DataSendUnReliable);

        m_ClientGameManager.DataReliableSendEvent.AddListener(DataSendReliable);

        var settings = new NetworkSettings();
        settings.WithNetworkConfigParameters(maxMessageSize: 1472);

        settings.WithSimulatorStageParameters(
            maxPacketCount: m_MaxPacketCount,
            mode: m_mode,
            packetDelayMs: m_MaxPacketDelay,
            packetJitterMs: m_MaxJitterMs,
            packetDropPercentage: m_MaxDropPercentage,
            packetDropInterval: m_MaxPacketInterval);


        if (useEncryption)
        {
            // Client only needs the public cert to verify the server identity
            settings.WithSecureClientParameters(certificatePem, serverCommonName);
        }

        m_Driver = NetworkDriver.Create(settings);

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

        var endpoint = NetworkEndpoint.Parse(serverIP, port);
        m_Connection = m_Driver.Connect(endpoint);

        Debug.Log("Client: Attempting to connect to " + serverIP);
    }

    void OnDestroy()
    {
        if (m_Driver.IsCreated) m_Driver.Dispose();
    }

    void Update()
    {
        if (!m_Driver.IsCreated) return;

        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated) return;

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Driver.PopEventForConnection(m_Connection, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Client: Successfully connected to Server!");

                if (m_Driver.BeginSend(m_ReliableFragmentedPipeline, m_Connection, out var writer) == 0)
                {
                    DataSendReliable("ConnectionTest");

                    m_Driver.EndSend(writer);
                }
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                DataReceive(stream);
                Debug.Log("Client: Received data from server.");
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client: Disconnected from server.");
                m_Connection = default;
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

        m_ClientGameManager.DataReceiveEvent.Invoke(data);

        if (m_EnableDebug)
            Debug.Log("Client: Receive String: " + data);

    }

    /// <summary>
    /// Method to send reliable large data string using the fragmentation pipeline.
    /// </summary>
    /// 
    public void DataSendReliable(string data)
    {
        if (m_Driver.BeginSend(m_ReliableFragmentedPipeline, m_Connection, out var writer) == 0)
        {
            // Convert String in UTF8
            var bytes = System.Text.Encoding.UTF8.GetBytes(data);

            // Write Lenght
            writer.WriteUInt((uint)bytes.Length);

            // Write Bytes
            writer.WriteBytes(bytes);

            m_Driver.EndSend(writer);

            if (m_EnableDebug)
                Debug.Log("Client : Send data : " + data);
        }
    }

    /// <summary>
    /// Method to send unreliable small data string
    /// </summary>
    public void DataSendUnReliable(string data)
    {
        if (m_Driver.BeginSend(m_UnReliablePipeline, m_Connection, out var writer) == 0)
        {
            // Convert String in UTF8
            var bytes = System.Text.Encoding.UTF8.GetBytes(data);

            // Write Lenght
            writer.WriteUInt((uint)bytes.Length);

            // Write Bytes
            writer.WriteBytes(bytes);

            m_Driver.EndSend(writer);

            if (m_EnableDebug)
                Debug.Log("Client : Send data : " + data);
        }
    }
}

