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

    private bool m_Selected = false;
    
    Ray ray;
    RaycastHit hit;

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit))
        {
            if (Input.GetMouseButtonDown(0))
            {
                m_ClueText.text = m_Clue;
                GetComponent<MeshRenderer>().material = m_SelectedMaterial;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                GetComponent<MeshRenderer>().material = m_UnSelectedMaterial;
            }
        }
    }
}
