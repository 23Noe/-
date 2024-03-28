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
    //�������ܺ�����

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

    //��ǰ��������Ŀ�����
    private void Zoom()
    {
        float zoomValue = inputProvider.GetAxisValue(2) * zoomSensitivity;
        currentTargetDistance = Mathf.Clamp(currentTargetDistance + zoomValue,minimumDistance,maximumDistance);

        float currentDistance = framingTransposer.m_CameraDistance;
        //�ﵽĿ����뽫ֹͣ�ƶ�
        if(currentDistance < currentTargetDistance) {
            return;
        }
        //ȷ��������������������ÿ�θ���ʱ����һ�£�SA��0.5 is the mid point��
        float lerpedZoomValue = Mathf.Lerp(currentDistance, currentTargetDistance, smoothing * Time.deltaTime);
        framingTransposer.m_CameraDistance = lerpedZoomValue;
    }
}
