using UnityEngine;

public class GetHmdDevicePose : MonoBehaviour
{
    public enum PoseSource
    {
        None,
        StereoViewMatrix,
        UnityHeadTransform
    }

    [Header("Apply Target")]
    [SerializeField] private Transform poseTarget;
    [SerializeField] private Transform xrHeadTransform;

    [Header("动态景深准星 (你的小白点 Quad)")]
    [SerializeField] private Transform reticleTransform;
    [SerializeField] private float maxReticleDistance = 2.0f;
    [SerializeField] private LayerMask hitMask = ~0; // 默认检测所有层

    [Header("【新增】视频巨幕设置")]
    public Transform videoScreen;       // 新增：把你的内凹巨幕拖给这个变量
    public float screenDistance = 3.5f; // 新增：巨幕距离眼睛多远
    public bool lockPitch = true;       // 新增：是否锁定上下低头（看视频建议锁定为 true）
    public float smoothSpeed = 8f;      // 新增：跟随平滑度，越大越跟手

    [Header("Runtime Output")]
    [SerializeField] private Vector3 currentPosition;
    [SerializeField] private Vector3 currentEulerAngles;
    [SerializeField] private Quaternion currentRotation = Quaternion.identity;
    [SerializeField] private PoseSource currentSource = PoseSource.None;

    [Header("Debug")]
    [SerializeField] private bool logWhenSourceChanges = true;
    private PoseSource lastLoggedSource = PoseSource.None;

    public Vector3 CurrentPosition => currentPosition;
    public Quaternion CurrentRotation => currentRotation;
    public Vector3 CurrentEulerAngles => currentEulerAngles;
    public PoseSource CurrentSource => currentSource;

    // 用于记录小白点的初始大小，以此为基准进行缩放补偿
    private Vector3 initialReticleScale;

    private void Awake()
    {
        if (poseTarget == null)
        {
            poseTarget = transform;
        }

        // 记录小白点的初始缩放值（假设这个大小是在最大距离 maxReticleDistance 下看着最舒服的大小）
        if (reticleTransform != null)
        {
            initialReticleScale = reticleTransform.localScale;
        }
    }

    private void LateUpdate()
    {
        //获取底层真实视点数据
        var resolvedPose = ResolveCurrentPose();
        StorePose(resolvedPose.pose, resolvedPose.source);
        //将控制权交给发射射线的空物体
        ApplyPose(resolvedPose.pose);
        //接管小白点的纯视觉表现（深度适应 + 恒定大小）
        UpdateDynamicReticle(resolvedPose.pose);
        UpdateVideoScreen(resolvedPose.pose);
    }

    private void UpdateVideoScreen(Pose headPose)
    {
        if (videoScreen == null) return;

        // 获取当前头部的旋转欧拉角
        Vector3 targetEuler = headPose.rotation.eulerAngles;

        // 如果开启了锁定，抹除掉 X (抬头低头) 和 Z (侧头) 的影响
        if (lockPitch)
        {
            targetEuler.x = 0;
            targetEuler.z = 0;
        }

        // 计算目标旋转和目标位置（在头部正前方 screenDistance 处）
        Quaternion targetRotation = Quaternion.Euler(targetEuler);
        Vector3 targetPosition = headPose.position + (targetRotation * Vector3.forward * screenDistance);

        // 使用 Lerp 和 Slerp 进行平滑跟随，防止晃眼
        videoScreen.position = Vector3.Lerp(videoScreen.position, targetPosition, Time.deltaTime * smoothSpeed);
        videoScreen.rotation = Quaternion.Slerp(videoScreen.rotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    private void UpdateDynamicReticle(Pose headPose)
    {
        if (reticleTransform == null) return;

        // 射线起点：真实双眼正中心
        Vector3 origin = headPose.position;
        // 射线方向：真实视野正前方
        Vector3 direction = headPose.rotation * Vector3.forward;

        Vector3 targetPosition;
        float currentDistance;

        // 发射物理射线寻找遮挡物
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxReticleDistance, hitMask))
        {
            currentDistance = hit.distance;
            // 往后退 0.02 米，防止小白点陷入墙壁导致闪烁 (Z-Fighting)
            targetPosition = hit.point - direction * 0.02f;
        }
        else
        {
            currentDistance = maxReticleDistance;
            targetPosition = origin + direction * maxReticleDistance;
        }

        // 更新小白点的位置和旋转
        reticleTransform.position = targetPosition;
        reticleTransform.rotation = headPose.rotation;

        // ==========================================
        // 核心魔法：保持屏幕视觉大小绝对恒定！
        // 根据“相似三角形”原理，距离缩短一半，模型缩小一半，它在视网膜上的大小就保持不变。
        // ==========================================
        float scaleFactor = currentDistance / maxReticleDistance;
        // 防止距离过近时缩放到 0 甚至反转
        scaleFactor = Mathf.Max(scaleFactor, 0.1f);

        reticleTransform.localScale = initialReticleScale * scaleFactor;
    }

    private (Pose pose, PoseSource source) ResolveCurrentPose()
    {
        if (TryReadStereoViewPose(out var stereoPose))
        {
            return (stereoPose, PoseSource.StereoViewMatrix);
        }

        if (TryReadTrackedHeadTransformPose(out var trackedHeadPose))
        {
            return (trackedHeadPose, PoseSource.UnityHeadTransform);
        }

        return (new Pose(GetReferenceLocalPosition(), Quaternion.identity), PoseSource.None);
    }

    private bool TryReadStereoViewPose(out Pose pose)
    {
        var trackedHead = GetTrackedHeadTransform();
        if (trackedHead == null)
        {
            pose = default;
            return false;
        }

        var camera = trackedHead.GetComponent<Camera>();
        if (camera == null || !camera.stereoEnabled)
        {
            pose = default;
            return false;
        }

        var leftEyeWorld = camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
        var rightEyeWorld = camera.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
        var leftEyePose = MatrixToPose(leftEyeWorld);
        var rightEyePose = MatrixToPose(rightEyeWorld);

        pose = new Pose(
            (leftEyePose.position + rightEyePose.position) * 0.5f,
            Quaternion.Slerp(leftEyePose.rotation, rightEyePose.rotation, 0.5f));
        return true;
    }

    private bool TryReadTrackedHeadTransformPose(out Pose pose)
    {
        var trackedHead = GetTrackedHeadTransform();
        if (trackedHead != null)
        {
            pose = new Pose(trackedHead.localPosition, trackedHead.localRotation);
            return true;
        }

        pose = default;
        return false;
    }

    private Transform GetTrackedHeadTransform()
    {
        if (xrHeadTransform != null)
        {
            return xrHeadTransform;
        }

        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

        return null;
    }

    private Vector3 GetReferenceLocalPosition()
    {
        var trackedHead = GetTrackedHeadTransform();
        if (trackedHead != null)
        {
            return trackedHead.localPosition;
        }

        if (poseTarget != null)
        {
            return poseTarget.localPosition;
        }

        return Vector3.zero;
    }

    private void StorePose(Pose pose, PoseSource source)
    {
        currentPosition = pose.position;
        currentRotation = pose.rotation;
        currentEulerAngles = currentRotation.eulerAngles;
        currentSource = source;

        if (logWhenSourceChanges && lastLoggedSource != currentSource)
        {
            lastLoggedSource = currentSource;
            Debug.Log($"GetHmdDevicePose: source={currentSource}, position={currentPosition}, euler={currentEulerAngles}");
        }
    }

    private void ApplyPose(Pose pose)
    {
        if (poseTarget != null)
        {
            poseTarget.position = pose.position;
            poseTarget.rotation = pose.rotation;
        }
    }

    private static Pose MatrixToPose(Matrix4x4 matrix)
    {
        var position = matrix.GetColumn(3);
        var rotation = Quaternion.LookRotation(-matrix.GetColumn(2), matrix.GetColumn(1));
        return new Pose(position, rotation);
    }
}
