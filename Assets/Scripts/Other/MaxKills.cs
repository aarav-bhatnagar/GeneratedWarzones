using UnityEngine;
using TMPro;

public class MaxKills : MonoBehaviour
{
    [SerializeField] private TMP_Text MaxText;

    private void Update()
    {
        if (GameLoader.Instance != null)
        {
            MaxText.text = GameLoader.Instance.maxKills.Value.ToString();
        }
    }
}
