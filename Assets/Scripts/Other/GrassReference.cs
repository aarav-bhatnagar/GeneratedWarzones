using UnityEngine;

public class GrassReference : MonoBehaviour
{
    private Shader grassShader;

    private void Start()
    {
        grassShader = Shader.Find("Custom/GeometryGrass");
        if (grassShader == null)
        {
            Debug.LogError("Could not find GrassGeometryShader!");
        }
        else
        {
            print("Found grass shader");
            Material mat = new Material(grassShader);
        }
    }
}
