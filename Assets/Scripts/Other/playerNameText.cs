using UnityEngine;
using TMPro;

public class playerNameText : MonoBehaviour
{
    public void SetName(ulong playerName)
    {
        GetComponent<TMP_Text>().text = $"Player {playerName}";
    }
}
