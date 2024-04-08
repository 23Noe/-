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

    //给镜头加入一个缓冲使其更平滑
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

        //跳跃时相机转动角色视角不移动，所以空转和跳跃不被视为“移动”，so将速度调节器设置为0
        float targetRotationYAngle = Rotate(movementDirection);

        Vector3 targetRotationDirection = GetTargetRotationDirection(targetRotationYAngle);

        float movementSpeed = GetMovementSpeed(); 

        Vector3 currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();

        stateMachine.Player.Rigidbody.
            AddForce(targetRotationDirection * movementSpeed - currentPlayerHorizontalVelocity,ForceMode.VelocityChange);
    }

    

    //设置运动输入角度,确保角色移动时是按照相机视角的方向移动，而不是游戏内世界的方向
    private float Rotate(Vector3 direction)
    {
        //定义directionangle
        float directionAngle = UpdateTargetRotation(direction);

        RotateTowardsTargetRotation();

        return directionAngle;
    }

    
    private static float GetDirectionAngle(Vector3 direction)
    {
        //方法返回弧度——> 方法返回度数
        float directionAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        //Atan2的范围是-180~180，通过加度数将其保持在正数的角度
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
        //获取当前角度
        float currentYangle = stateMachine.Player.Rigidbody.rotation.eulerAngles.y;
        
        if(currentYangle == currentTargetRotation.y) 
        {
            return;
        }

        //对于达到目标旋转所需的时间，输入timeToReach参数，用以平滑角度
        float smoothedYangle = Mathf.SmoothDampAngle(currentYangle, currentTargetRotation.y, ref dampedTargetRotationCurrentVelocity.y, timeToReachTargetRotation.y - dampedTargetRotationPassedTime.y);
        //单独传递timeToReachTargetRotation每次平滑方法调用总是需要0.14秒
        
        //为传递的时间变量增加值(平滑值，用来旋转玩家)
        dampedTargetRotationPassedTime.y += Time.deltaTime;

        Quaternion targetRotation = Quaternion.Euler(0f,smoothedYangle,0f);

        //用来移动刚体到指定的位置
        stateMachine.Player.Rigidbody.MoveRotation(targetRotation);
    }
    protected float UpdateTargetRotation(Vector3 direction, bool shouldConsiderCameraTotation=true)
    {
        float directionAngle = GetDirectionAngle(direction);
        if (shouldConsiderCameraTotation)
        {
            //实现dashing state
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
        //相机旋转角度时，四元数与向量相乘
        return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        //传递Vector3.forward是为了从z轴开始旋转（z轴为玩家向前的轴）
    }
    #endregion
}
