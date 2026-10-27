using UnityEngine;

public class CurvedWorld : MonoBehaviour
{
    [Header("Curved World Global Settings")]
    public float globalCurveX = -0.0018f;
    public float globalCurveY = 0.0005f;
    public float globalFlatZoneZ = 22.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnValidate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        UpdateCurvedWorldGlobals();
    }

    private void UpdateCurvedWorldGlobals()
    {
        Shader.SetGlobalFloat("_GlobalCurveX", globalCurveX);
        Shader.SetGlobalFloat("_GlobalCurveY", globalCurveY);
        Shader.SetGlobalFloat("_GlobalFlatZoneZ", globalFlatZoneZ);
    }
}
