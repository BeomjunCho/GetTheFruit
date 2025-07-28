using UnityEngine;

/// <summary>
/// Draws two separate ropes from the anchor to each platform
/// using independent LineRenderers.
/// </summary>
[ExecuteAlways]
public class PulleyRopeRenderer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform anchor;
    [SerializeField] private Transform platformA;
    [SerializeField] private Transform platformB;

    [Header("Rope Visual")]
    [SerializeField, Min(4)] private int segmentCount = 20;
    [SerializeField] private float sagAmount = 0.4f;

    private LineRenderer _left;   // Rope from anchor to platform A
    private LineRenderer _right;  // Rope from anchor to platform B

    /* ================================================================== */
    /*  Unity lifecycle                                                   */
    private void Awake()
    {
        // Automatically create or get child LineRenderer objects
        _left = CreateOrGetLR("Rope_L");
        _right = CreateOrGetLR("Rope_R");

        InitLineRenderer(_left);
        InitLineRenderer(_right);
    }

    private void LateUpdate()
    {
        if (anchor == null || platformA == null || platformB == null) return;

        DrawRope(_left, anchor.position, platformA.position);
        DrawRope(_right, anchor.position, platformB.position);
    }

    /* ================================================================== */
    /*  Helpers                                                           */
    private LineRenderer CreateOrGetLR(string childName)
    {
        Transform child = transform.Find(childName);
        if (child == null)
        {
            child = new GameObject(childName).transform;
            child.parent = transform;
            child.localPosition = Vector3.zero;
        }

        LineRenderer lr = child.GetComponent<LineRenderer>();
        if (lr == null) lr = child.gameObject.AddComponent<LineRenderer>();
        return lr;
    }

    private void InitLineRenderer(LineRenderer lr)
    {
        lr.useWorldSpace = true;
        lr.positionCount = segmentCount;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        // Material and width should be set via Inspector
    }

    private void DrawRope(LineRenderer lr, Vector3 start, Vector3 end)
    {
        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);

            // Linear interpolation between start and end
            Vector3 pos = Vector3.Lerp(start, end, t);

            // Apply sag based on vertical direction
            float sag = Mathf.Sin(Mathf.PI * t) * sagAmount;
            pos.y -= sag;

            lr.SetPosition(i, pos);
        }
    }
}
