using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class PlayerMovementState : IState
{
    protected PlayerMovementStateMachine stateMachine;

    protected Vector2 movementInput;

    protected float baseSpeed = 5f;
    protected float speedModifier = 1f;

    protected Vector3 currentTargetRotation;
    protected Vector3 timeToReachTargetRotation;
    protected Vector3 dampedTargetRotationCurrentVelocity;
    protected Vector3 dampedTargetRotationPassedTime;

    public PlayerMovementState(PlayerMovementStateMachine playerMovementStateMachine)
    {
        stateMachine = playerMovementStateMachine;
        InitializeData();
    }

    //����ͷ����һ������ʹ���ƽ��
    private void InitializeData()
    {
        timeToReachTargetRotation.y = 0.14f;
    }
    #region IState Methods

    public virtual void Enter()
    {
        Debug.Log("State:"+GetType().Name);
    }

    public virtual void Exit()
    { 
    }

    public virtual void HandleInput()
    {
        ReadMovementInput();
    }


    public virtual void Update()
    { 
    } 
    public virtual void PhysicsUpdate()
    {
        Move();
    }
    #endregion

    #region Main Methods

    private void ReadMovementInput()
    {
        movementInput= stateMachine.Player.Input.PlayerActions.Movement.ReadValue<Vector2>();
    }

    private void Move()
    {
     if(movementInput == Vector2.zero || speedModifier == 0f)
        {
            return;
        }
        Vector3 movementDirection = GetMovementInputDirection();

        //��Ծʱ���ת����ɫ�ӽǲ��ƶ������Կ�ת����Ծ������Ϊ���ƶ�����so���ٶȵ���������Ϊ0
        float targetRotationYAngle = Rotate(movementDirection);

        Vector3 targetRotationDirection = GetTargetRotationDirection(targetRotationYAngle);

        float movementSpeed = GetMovementSpeed(); 

        Vector3 currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();

        stateMachine.Player.Rigidbody.
            AddForce(targetRotationDirection * movementSpeed - currentPlayerHorizontalVelocity,ForceMode.VelocityChange);
    }

    

    //�����˶�����Ƕ�,ȷ����ɫ�ƶ�ʱ�ǰ�������ӽǵķ����ƶ�����������Ϸ������ķ���
    private float Rotate(Vector3 direction)
    {
        //����directionangle
        float directionAngle = UpdateTargetRotation(direction);

        RotateTowardsTargetRotation();

        return directionAngle;
    }

    
    private static float GetDirectionAngle(Vector3 direction)
    {
        //�������ػ��ȡ���> �������ض���
        float directionAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        //Atan2�ķ�Χ��-180~180��ͨ���Ӷ������䱣���������ĽǶ�
        if (directionAngle < 0f)
        {
            directionAngle += 360f;
        }

        return directionAngle;
    }
    private float AddCameraRotationToAngle(float angle)
    {
        angle += stateMachine.Player.MainCameraTransform.eulerAngles.y;
        if (angle > 360f)
        {
            angle -= 360f;
        }

        return angle;
    }
    private void UpdateTargetRotationData(float targetAngle)
    {
        currentTargetRotation.y = targetAngle;
        dampedTargetRotationPassedTime.y = 0f;
    }

    #endregion
    #region Reusable Methods

    protected Vector3 GetMovementInputDirection()
    {
        return new Vector3(movementInput.x,0f,movementInput.y);
    }

    protected float GetMovementSpeed()
    {
        return baseSpeed* speedModifier;
    }

    protected Vector3 GetPlayerHorizontalVelocity()
    {
        Vector3 playerHorizontalVelocity = stateMachine.Player.Rigidbody.velocity;

        playerHorizontalVelocity.y = 0f;

        return playerHorizontalVelocity;
    }

    protected void RotateTowardsTargetRotation()
    {
        //��ȡ��ǰ�Ƕ�
        float currentYangle = stateMachine.Player.Rigidbody.rotation.eulerAngles.y;
        
        if(currentYangle == currentTargetRotation.y) 
        {
            return;
        }

        //���ڴﵽĿ����ת�����ʱ�䣬����timeToReach����������ƽ���Ƕ�
        float smoothedYangle = Mathf.SmoothDampAngle(currentYangle, currentTargetRotation.y, ref dampedTargetRotationCurrentVelocity.y, timeToReachTargetRotation.y - dampedTargetRotationPassedTime.y);
        //��������timeToReachTargetRotationÿ��ƽ����������������Ҫ0.14��
        
        //Ϊ���ݵ�ʱ���������ֵ(ƽ��ֵ��������ת���)
        dampedTargetRotationPassedTime.y += Time.deltaTime;

        Quaternion targetRotation = Quaternion.Euler(0f,smoothedYangle,0f);

        //�����ƶ����嵽ָ����λ��
        stateMachine.Player.Rigidbody.MoveRotation(targetRotation);
    }
    protected float UpdateTargetRotation(Vector3 direction, bool shouldConsiderCameraTotation=true)
    {
        float directionAngle = GetDirectionAngle(direction);
        if (shouldConsiderCameraTotation)
        {
            //ʵ��dashing state
            directionAngle = AddCameraRotationToAngle(directionAngle);
        }

        if (directionAngle != currentTargetRotation.y)
        {
            UpdateTargetRotationData(directionAngle);
        }

        return directionAngle;
    }
    protected Vector3 GetTargetRotationDirection(float targetAngle)
    {
        //�����ת�Ƕ�ʱ����Ԫ�����������
        return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        //����Vector3.forward��Ϊ�˴�z�Ὺʼ��ת��z��Ϊ�����ǰ���ᣩ
    }
    #endregion
}
