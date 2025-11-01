/// <summary>
/// Base class for all behavior tree nodes.
/// </summary>
public abstract class BTNode
{
    public enum NodeState
    {
        Success,
        Failure,
        Running
    }

    public abstract NodeState Evaluate();
}
