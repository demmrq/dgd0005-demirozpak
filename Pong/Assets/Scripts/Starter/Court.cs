using UnityEngine;

public class Court : MonoBehaviour
{
    public const float HalfWidth = 8f;
    public const float HalfHeight = 5f;
    public const float PaddleX = 7f;
    public const float PaddleHalfLength = 1f;
    public const float BallRadius = 0.25f;
    public const float BallY = 0.25f;

    [SerializeField] private bool localBall = true;

    private static readonly string[] ShaderNames =
    {
        "Universal Render Pipeline/Unlit",
        "Universal Render Pipeline/Lit",
        "Sprites/Default",
        "Standard",
        "Unlit/Color"
    };

    private static Material _mat;
    private static bool _shaderSearched;

    private void Awake()
    {
        Make("Floor", PrimitiveType.Cube, new Vector3(0, -0.1f, 0), new Vector3(HalfWidth * 2, 0.2f, HalfHeight * 2), new Color(0.10f, 0.12f, 0.16f));
        Make("Wall Top", PrimitiveType.Cube, new Vector3(0, 0.25f, HalfHeight + 0.25f), new Vector3(HalfWidth * 2 + 1, 0.5f, 0.5f), new Color(0.75f, 0.78f, 0.82f));
        Make("Wall Bottom", PrimitiveType.Cube, new Vector3(0, 0.25f, -HalfHeight - 0.25f), new Vector3(HalfWidth * 2 + 1, 0.5f, 0.5f), new Color(0.75f, 0.78f, 0.82f));
        for (int i = -4; i <= 4; i++)
            Make($"Centre {i}", PrimitiveType.Cube, new Vector3(0, 0.01f, i * 1.1f), new Vector3(0.15f, 0.02f, 0.6f), new Color(0.35f, 0.38f, 0.44f));

        if (localBall)
        {
            GameObject ball = Make("Local Ball", PrimitiveType.Sphere, new Vector3(0, BallY, 0), Vector3.one * (BallRadius * 2), Color.white);
            ball.AddComponent<LocalBall>();
        }
    }

    private static GameObject Make(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        Destroy(go.GetComponent<Collider>());
        Paint(go, color);
        return go;
    }

    public static void ClampPaddle(Transform t)
    {
        Vector3 p = t.position;
        float limit = HalfHeight - PaddleHalfLength;
        p.z = Mathf.Clamp(p.z, -limit, limit);
        t.position = p;
    }

    public static void Paint(GameObject go, Color color)
    {
        if (go == null) return;

        Renderer r = go.GetComponent<Renderer>();
        if (r == null) return;

        if (_mat == null && !_shaderSearched)
        {
            _shaderSearched = true;
            for (int i = 0; i < ShaderNames.Length; i++)
            {
                Shader shader = Shader.Find(ShaderNames[i]);
                if (shader != null)
                {
                    _mat = new Material(shader);
                    break;
                }
            }

            if (_mat == null)
                Debug.LogWarning("[Court] No suitable shader found, using the renderer's default material.");
        }

        if (_mat != null)
        {
            r.material = new Material(_mat) { color = color };
            return;
        }

        if (r.sharedMaterial != null)
            r.material.color = color;
    }
}