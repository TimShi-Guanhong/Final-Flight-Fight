using UnityEngine;

public class BoardPath : MonoBehaviour
{
    public Transform pathRoot;
    public Transform[] points;

    private void Awake()
    {
        points = new Transform[pathRoot.childCount];

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = pathRoot.GetChild(i);
        }
    }
}