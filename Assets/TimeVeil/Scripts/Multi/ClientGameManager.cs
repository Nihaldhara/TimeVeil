using UnityEngine;
using UnityEngine.Events;

public class ClientGameManager : MonoBehaviour
{
    static private ClientGameManager m_Instance;
    static public ClientGameManager Instance { get { return m_Instance; } }

    [HideInInspector]
    public UnityEvent<string> DataReceiveEvent;

    [HideInInspector]
    public UnityEvent<string> DataUnreliableSendEvent;

    [HideInInspector]
    public UnityEvent<string> DataReliableSendEvent;



    [HideInInspector]
    public UnityEvent FailedConnectEvent;
    [HideInInspector]
    public UnityEvent ConnectedEvent;
    [HideInInspector]
    public UnityEvent DisconnectEvent;

    private void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //DataReceiveEvent.AddListener(TestReceive);
        //ConnectedEvent.AddListener(Connection);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Connection()
    {
        Debug.Log($"ClientGameManager Send : Connected Test");

    }

    void TestReceive(string data)
    {
        Debug.Log($"ClientGameManager Receive : {data}");
    }
}
