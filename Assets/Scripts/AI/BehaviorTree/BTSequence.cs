using System.Collections.Generic;

/// <summary>
/// Sequence node: executes children until one fails.
/// Returns Success if all children succeed, Failure if any fails.
/// </summary>
public class BTSequence : BTNode
{
    private List<BTNode> children;

    public BTSequence(List<BTNode> children)
    {
        this.children = children;
    }

    public override NodeState Evaluate()
    {
        foreach (var child in children)
        {
            NodeState result = child.Evaluate();

            if (result == NodeState.Failure)
            {
                return NodeState.Failure;
            }
            else if (result == NodeState.Running)
            {
                return NodeState.Running;
            }
        }

        return NodeState.Success;
    }
}
