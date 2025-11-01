using System;

/// <summary>
/// Action node: executes a specific action.
/// </summary>
public class BTAction : BTNode
{
    private Func<NodeState> action;

    public BTAction(Func<NodeState> action)
    {
        this.action = action;
    }

    public override NodeState Evaluate()
    {
        return action();
    }
}
