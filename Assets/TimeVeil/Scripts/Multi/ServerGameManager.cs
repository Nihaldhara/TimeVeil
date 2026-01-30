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
}
