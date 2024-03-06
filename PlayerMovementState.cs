using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class PlayerMovementState : IState
{
    protected PlayerMovementStateMachine stateMachine;

    protected Vector2 movemnetInput;

    protected float baseSpeed = 5f;

    protected float speedModifier = 1f;

    public PlayerMovementState(PlayerMovementStateMachine playerMovementStateMachine)
    {
        stateMachine = playerMovementStateMachine;
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
        movemnetInput= stateMachine.Player.Input.PlayerActions.Movement.ReadValue<Vector2>();
    }
    private void Move()
    {
     if(movemnetInput == Vector2.zero || speedModifier == 0f)
        {
            return;
        }
        Vector3 movementDirection = GetMovementInputDirection();

        float movementSpeed = GetMovementSpeed();

        Vector3 currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();

        stateMachine.Player.Rigidbody.
            AddForce(movementDirection * movementSpeed -
            currentPlayerHorizontalVelocity,ForceMode.VelocityChange);
    }

    

    #endregion

    #region Reusable Methods
    protected float GetMovementSpeed()
    {
        return baseSpeed* speedModifier;
    }
    private Vector3 GetMovementInputDirection()
    {
        return new Vector3(movemnetInput.x,0f,movemnetInput.y);
    }

    protected Vector3 GetPlayerHorizontalVelocity()
    {
        Vector3 playerHorizontalVelocity = stateMachine.Player.Rigidbody.velocity;
        playerHorizontalVelocity.y = 0f;
        return playerHorizontalVelocity;
    }
    #endregion
}
