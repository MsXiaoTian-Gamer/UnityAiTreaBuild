using UnityEngine;
using System.Collections.Generic;
public abstract class BTNode
{
    public enum State
    {
        Success,
        Failure,
        Running
    }
    public abstract State Evaluate();
}
