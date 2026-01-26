using UnityEngine;
using UnityEngine.Events;

public class ServerGameManager : MonoBehaviour
{
    static private ServerGameManager m_Instance;
    static public ServerGameManager Instance { get { return m_Instance; } }

    [HideInInspector]
    public UnityEvent<string> DataReceiveEvent;

    [HideInInspector]
    public UnityEvent<string,int> DataUnreliableSendEvent;

    [HideInInspector]
    public UnityEvent<string,int> DataReliableSendEvent;

    //Can be used to setup a start timer (game will start in XX seconds)
    [HideInInspector]
    public UnityEvent ClientConnectionEvent;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TestReceive(string data)
    {
        Debug.Log($"ServerGameManager Receive : {data}");
        DataUnreliableSendEvent.Invoke("SendTestToClient", 0);
    }
}
