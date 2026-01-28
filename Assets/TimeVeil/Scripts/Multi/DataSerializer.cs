using System;
using System.Globalization;
using UnityEngine;

public class DataSerializer : MonoBehaviour
{
    static private DataSerializer m_Instance;
    static public DataSerializer Instance => m_Instance;

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

    public string SerializeFloat(float newData)
    {
        return Math.Round(newData, 5).ToString(CultureInfo.InvariantCulture);
    }
}
