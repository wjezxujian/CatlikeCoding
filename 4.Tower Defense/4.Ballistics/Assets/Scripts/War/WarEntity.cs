using UnityEngine;

public class WarEntity : GameBehaviour
{
    WarFactory originFactory;

    public WarFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    public void Recyle()
    {
        originFactory.Reclaim(this);
    }

    public override bool GameUpdate()
    {
        return true;
    }
}
