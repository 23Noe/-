using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] [Range (0f,10f)]private float defaultDistance = 6f;
    [SerializeField] [Range(0f, 10f)] private float minimumDistance = 1f;
    [SerializeField] [Range(0f, 10f)] private float maximumDistance = 6f;

    [SerializeField][Range(0f, 10f)] private float smoothing = 4f;
    [SerializeField][Range(0f, 10f)] private float zoomSensitivity = 1f;

    private CinemachineFramingTransposer framingTransposer;
    private CinemachineInputProvider inputProvider;
    //摄像机框架和输入

    private float currentTargetDistance;
    private void Awake()
    {
        framingTransposer = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        inputProvider = GetComponent<CinemachineInputProvider>();
        currentTargetDistance = defaultDistance;
    }
    private void Update()
    {
        Zoom();
    }

    //当前距离推向目标距离
    private void Zoom()
    {
        float zoomValue = inputProvider.GetAxisValue(2) * zoomSensitivity;
        currentTargetDistance = Mathf.Clamp(currentTargetDistance + zoomValue,minimumDistance,maximumDistance);

        float currentDistance = framingTransposer.m_CameraDistance;
        //达到目标距离将停止移动
        if(currentDistance < currentTargetDistance) {
            return;
        }
        //确保不是整秒数，而是在每次更改时尽量一致（SA：0.5 is the mid point）
        float lerpedZoomValue = Mathf.Lerp(currentDistance, currentTargetDistance, smoothing * Time.deltaTime);
        framingTransposer.m_CameraDistance = lerpedZoomValue;
    }
}
