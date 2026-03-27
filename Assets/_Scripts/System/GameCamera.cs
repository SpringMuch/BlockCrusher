using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GameCamera : MonoBehaviour
{
    [Header("Cấu hình Board")]
    [SerializeField] private Vector2 boardSize = new Vector2(8f, 12f);
    [SerializeField] private float buffer = 1.1f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        AdjustCamera();
    }

    private void Update()
    {
#if UNITY_EDITOR
        AdjustCamera();
#endif
    }

    public void AdjustCamera()
    {
        if (cam == null) cam = GetComponent<Camera>();

        cam.orthographic = true;

        float screenAspect = (float)Screen.width / (float)Screen.height;
        float targetAspect = boardSize.x / boardSize.y;

        if (screenAspect >= targetAspect)
        {
            cam.orthographicSize = (boardSize.y / 2) * buffer;
        }
        else
        {
            float differenceInSize = targetAspect / screenAspect;
            cam.orthographicSize = (boardSize.y / 2) * differenceInSize * buffer;
        }

        transform.position = new Vector3(boardSize.x / 2f, boardSize.y / 2f - 2f, -10f);
    }
}