using System.Collections;
using UnityEngine;

public class TokenMover : MonoBehaviour
{
    public Transform[] boardPoints;
    public int currentIndex = 0;

    public float moveSpeed = 8f;
    public float hoverY = 0.8f;
    public float zOffset = 0f;

    private bool isMoving = false;

    void Start()
    {
        UpdatePosition();
    }

    public void MoveSteps(int steps)
    {
        if (isMoving) return;
        StartCoroutine(MoveRoutine(steps));
    }

    IEnumerator MoveRoutine(int steps)
    {
        isMoving = true;

        int targetIndex = currentIndex + steps;
        targetIndex = Mathf.Clamp(targetIndex, 0, boardPoints.Length - 1);

        while (currentIndex != targetIndex)
        {
            currentIndex += currentIndex < targetIndex ? 1 : -1;

            Vector3 targetPos = boardPoints[currentIndex].position;
            targetPos.y += hoverY;
            targetPos.z += zOffset;

            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            yield return new WaitForSeconds(0.08f);
        }

        isMoving = false;
    }

    public void SetPosition(int index)
    {
        currentIndex = Mathf.Clamp(index, 0, boardPoints.Length - 1);
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (boardPoints == null || boardPoints.Length == 0) return;

        Vector3 pos = boardPoints[currentIndex].position;
        pos.y += hoverY;
        pos.z += zOffset;

        transform.position = pos;
    }
}