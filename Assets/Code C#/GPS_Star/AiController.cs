﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AiController : MonoBehaviour
{
    public float moveSpeed;             // Tốc độ di chuyển của AI
    public float nextWPDistance;        // Khoảng cách nhỏ nhất để di chuyển đến waypoint tiếp theo
    public Seeker seeker;               // Component Seeker để tính toán đường đi
    private Transform target;           // Mục tiêu di chuyển của AI
    private Path path;                  // Đường đi tính toán được
    private Coroutine moveCoroutine;    // Coroutine để di chuyển
    private bool isCalculatingPath = false; // Biến để kiểm tra xem có đang tính toán đường đi hay không

    private ColliderExpander colliderExpander; // Tham chiếu đến script ColliderExpander
    private Vector3 firstBookPosition;   // Vị trí của quyển sách đầu tiên được chạm vào

    void Start()
    {
        colliderExpander = GetComponent<ColliderExpander>(); // Lấy component ColliderExpander từ AI
        target = null; // Bắt đầu không có mục tiêu để AI không tính toán đường đi
    }

    
    // Phương thức để cập nhật mục tiêu mới và tính toán đường đi
    public void SetTarget(Transform newTarget)
    {
        if (!isCalculatingPath && newTarget != target)
        {
            target = newTarget;         // Cập nhật mục tiêu mới
            Debug.Log("Mục tiêu mới: " + target.position); // In ra vị trí của mục tiêu mới
            isCalculatingPath = true;   // Bắt đầu tính toán đường đi
            CalculatePath();            // Tính toán đường đi tới mục tiêu mới
        }
    }


    // Phương thức để tính toán đường đi từ vị trí hiện tại đến mục tiêu
    void CalculatePath()
    {
        if (seeker.IsDone() && target != null)
        {
            seeker.StartPath(transform.position, target.position, OnPathCallback);
        }
    }

    // Callback khi tính toán đường đi hoàn thành
    void OnPathCallback(Path p)
    {
        isCalculatingPath = false;      // Kết thúc tính toán đường đi
        if (!p.error)
        {
            path = p;                   // Lưu đường đi tính toán được
            Debug.Log("Đường đi tính toán được: " + path.vectorPath.Count + " waypoints"); // In ra số lượng waypoint
            MoveToTarget();             // Bắt đầu di chuyển theo đường đi
        }
        else
        {
            Debug.LogError("Có lỗi xảy ra khi tính toán đường đi: " + p.errorLog);
        }
    }

    // Phương thức để bắt đầu di chuyển theo đường đi tính toán được
    void MoveToTarget()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);   // Dừng coroutine di chuyển nếu có
        }
        moveCoroutine = StartCoroutine(MoveToTargetCoroutine()); // Bắt đầu coroutine mới
    }

    IEnumerator MoveToTargetCoroutine()
    {
        // Di chuyển theo đường đi đã tính toán
        int currentWP = 0;
        while (currentWP < path.vectorPath.Count)
        {
            Vector2 direction = ((Vector2)path.vectorPath[currentWP] - (Vector2)transform.position).normalized;
            Vector2 force = direction * moveSpeed * Time.deltaTime;
            transform.position = (Vector2)transform.position + force;

            float distance = Vector2.Distance(transform.position, path.vectorPath[currentWP]);
            if (distance < nextWPDistance)
            {
                currentWP++;
            }

            if (currentWP >= path.vectorPath.Count)
            {
                break;
            }

            yield return null;
        }

        // Sau khi đến đích, di chuyển đến vị trí của quyển sách đầu tiên được chạm vào
        Vector3 firstBookPos = colliderExpander.GetFirstBookPosition();
        if (firstBookPos != Vector3.zero)
        {
            // Tạo một điểm mục tiêu ảo từ vị trí mới
            GameObject targetObject = new GameObject();
            targetObject.transform.position = firstBookPos;

            // Truyền transform của đối tượng mục tiêu ảo
            SetTarget(targetObject.transform);

            // Sau khi sử dụng, hủy đối tượng mục tiêu ảo để tránh rò rỉ bộ nhớ
            Destroy(targetObject);
        }
    }

}