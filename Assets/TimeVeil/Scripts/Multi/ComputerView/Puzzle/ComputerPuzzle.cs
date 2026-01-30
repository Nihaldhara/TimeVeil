using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerPuzzle : MonoBehaviour
{
    private string m_Clue;

    public string Clue
    {
        get => m_Clue;
        set => m_Clue = value;
    }

    [SerializeField] private Material m_SelectedMaterial;
    [SerializeField] private Material m_UnSelectedMaterial;
    
    [SerializeField] private TMP_Text m_ClueText;

    public TMP_Text ClueText
    {
        get => m_ClueText;
        set => m_ClueText = value;
    }

    private bool m_Selected = false;
    
    Ray ray;
    RaycastHit hit;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Click!");
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Clicked on cube!");
                m_ClueText.text = m_Clue;
                GetComponent<MeshRenderer>().material = m_SelectedMaterial;
            }
            else
            {
                Debug.Log("Did not click on cube");
                GetComponent<MeshRenderer>().material = m_UnSelectedMaterial;
            }
        }
    }
}
