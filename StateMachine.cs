using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine{
    protected IState currentState;

    public void ChangeState(IState newstate)
    {
        currentState?.Exit();

        currentState = newstate;
        currentState.Enter();
    }
    
    public void HandleInput() {
        currentState?.HandleInput();
    }
    public void Update()
    {
        currentState?.Update();
    }
    public void PhysicsUpdate()
    {
        currentState?.PhysicsUpdate();
    }
}
