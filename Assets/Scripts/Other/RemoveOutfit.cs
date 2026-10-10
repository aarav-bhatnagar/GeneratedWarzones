using UnityEngine;

public class RemoveOutfit : MonoBehaviour
{
    private void Update()
    {
        if (transform.parent == null)
        {
            Destroy(this.gameObject);
        }
    }
}
