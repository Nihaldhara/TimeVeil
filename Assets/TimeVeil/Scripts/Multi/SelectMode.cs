using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectMode : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SceneManager.LoadScene(0);
            Debug.Log("[My Debug] ça marche");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            SceneManager.LoadScene(1);
            Debug.Log("[My Debug] ça marche");
        }
    }
}
