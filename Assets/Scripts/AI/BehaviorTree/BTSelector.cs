using System.Collections.Generic;

/// <summary>
/// Selector node: executes children until one succeeds.
/// Returns Success if any child succeeds, Failure if all fail.
/// </summary>
public class BTSelector : BTNode
{
    private List<BTNode> children;

    public BTSelector(List<BTNode> children)
    {
        this.children = children;
    }

    public override NodeState Evaluate()
    {
        foreach (var child in children)
        {
            NodeState result = child.Evaluate();

            if (result == NodeState.Success)
            {
                return NodeState.Success;
            }
            else if (result == NodeState.Running)
            {
                return NodeState.Running;
            }
        }

        return NodeState.Failure;
    }
}
