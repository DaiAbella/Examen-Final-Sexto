using UnityEngine;

public class WeaponColorApplier : MonoBehaviour
{
    private Renderer[] rends;

    public Material redMat;
    public Material blueMat;
    public Material greenMat;

    void Awake()
    {
        rends = GetComponentsInChildren<Renderer>();

        foreach (var r in rends)
            Debug.Log("[WeaponColorApplier] Renderer encontrado: " + r.gameObject.name);
    }

    public void ApplyColor(string color)
    {
        Debug.Log("[WeaponColorApplier] ApplyColor() → Color recibido: " + color);

        Material matToApply = null;

        switch (color)
        {
            case "red": matToApply = redMat; break;
            case "blue": matToApply = blueMat; break;
            case "green": matToApply = greenMat; break;
            default:
                Debug.LogWarning("[WeaponColorApplier] Color no reconocido.");
                return;
        }

        foreach (var r in rends)
        {
            r.material = matToApply;
            Debug.Log("[WeaponColorApplier] Material aplicado a: " + r.gameObject.name);
        }
    }
}
