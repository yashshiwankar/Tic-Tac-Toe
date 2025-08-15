using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    [SerializeField] RawImage img;
    [SerializeField] float x = 1, y = 1;

    private float height;
    void Start()
    {
        img = GetComponent<RawImage>();
    }

    void LateUpdate()
    {
        img.uvRect = new Rect(img.uvRect.position + new Vector2(x, y) * Time.deltaTime, Vector2.one);
    }
}
