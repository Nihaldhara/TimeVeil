using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /*private List<GameObject> m_CollectedKeys = new List<GameObject>();
    
    public void CollectKey(GameObject key)
    {
        m_CollectedKeys.Add(key);
        Debug.Log($"Collected a new key {key}");
    }

    public void ConsumeKey(GameObject key)
    {
        if (m_CollectedKeys.Contains(key))
        {
            m_CollectedKeys.Remove(key);
        }
    }

    public List<GameObject> GetCollectedKeys()
    {
        return m_CollectedKeys;
    }*/

    public void Die()
    {
        Debug.Log("You died...");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
