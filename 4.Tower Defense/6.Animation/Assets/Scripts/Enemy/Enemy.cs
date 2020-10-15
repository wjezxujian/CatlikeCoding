using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemy : GameBehaviour
{
    [SerializeField]
    Transform model = default;

    [SerializeField]
    EnemyAnimationConfig animationConfig = default;

    EnemyFactory originFactory;

    GameTile tileFrom, tileTo;

    Vector3 positionFrom, positionTo;

    float progress, progressFactor;

    float pathOffset;

    float speed;

    Direction direction;

    DirectionChange directionChange;

    float directionAngleFrom, directionAngleTo;

    EnemyAnimator animator;

    Collider targetPointCollider;

    public float Scale { get; private set; }

    float Health { get; set; }

    public bool IsValidTarget => animator.CurrentClip == EnemyAnimator.Clip.Move;

    public EnemyFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    public Collider TargetPointCollider
    {
        set
        {
            Debug.Assert(targetPointCollider == null, "Redefined collider!");
            targetPointCollider = value;
        }
    }

    private void Awake()
    {
        animator.Configure(model.GetChild(0).gameObject.AddComponent<Animator>(), animationConfig);
    }

    private void OnDestroy()
    {
        animator.Destroy();
    }

    public void Initialize(float scale, float pathOffset, float speed, float health)
    {
        model.localScale = new Vector3(scale, scale, scale);

        this.pathOffset = pathOffset;
        this.speed = speed;

        Scale = scale;
        Health = health;

        //animator.Play(speed / scale);
        animator.PlayIntro();
        targetPointCollider.enabled = false;

    }

    public void SpawnOn(GameTile tile)
    {
        //transform.localPosition = tile.transform.localPosition;
        Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
        tileFrom = tile;
        tileTo = tile.NextTileOnPath;
        //positionFrom = tileFrom.transform.localPosition;
        ////positionTo = tileTo.transform.localPosition;
        //positionTo = tileFrom.ExitPoint;
        //transform.localRotation = tileFrom.PathDirection.GetRotation();
        progress = 0f;

        PrepareIntro();

        
    }

    public override bool GameUpdate()
    {
        animator.GameUpdate();
        if(animator.CurrentClip == EnemyAnimator.Clip.Intro)
        {
            if (!animator.IsDone)
            {
                return true;
            }
            animator.PlayMove(speed / Scale);
            targetPointCollider.enabled = true;
        }
        else if(animator.CurrentClip >= EnemyAnimator.Clip.Outro)
        {
            if (animator.IsDone)
            {
                Recycle();
                return false;
            }
            return true;
        }

        if(Health <= 0f)
        {
            //OriginFactory.Reclaim(this);
            //Recycle();            
            //return false;
            animator.PlayDying();
            targetPointCollider.enabled = false;
            return true;
        }

        //transform.localPosition += Vector3.forward * Time.deltaTime;
        progress += Time.deltaTime * progressFactor;
        while(progress >= 1f)
        {
            //tileFrom = tileTo;
            //tileTo = tileTo.NextTileOnPath;
            if (tileTo == null)
            {
                //OriginFactory.Reclaim(this);
                Game.EnemyReachedDestination();
                //Recycle();
                animator.PlayOutro();
                targetPointCollider.enabled = false;
                return true;
            }
            //positionFrom = positionTo;
            ////positionTo = tileTo.transform.localPosition;
            //positionTo = tileFrom.ExitPoint;
            //transform.localRotation = tileFrom.PathDirection.GetRotation();
            //progress -= 1f;
            progress = (progress - 1f) / progressFactor;
            PrepareNextState();
            progress *= progressFactor;
        }

        if(directionChange == DirectionChange.None)
        {
            transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        }
        else
        {
            float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }

        return true;
    }

    public override void Recycle()
    {
        animator.Stop();
        OriginFactory.Reclaim(this);
    }

    public void ApplyDamage(float damage)
    {
        Debug.Assert(damage >= 0f, "Negavite damage applied.");
        Health -= damage;
    }

    private void PrepareNextState()
    {
        tileFrom = tileTo;
        tileTo = tileTo.NextTileOnPath;
        positionFrom = positionTo;

        if (tileTo == null)
        {
            PrepareOutro();
            return;
        }

        positionTo = tileFrom.ExitPoint;
        directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
        direction = tileFrom.PathDirection;
        directionAngleFrom = directionAngleTo;

        switch (directionChange)
        {
            case DirectionChange.None:
                PrepareForward();
                break;
            case DirectionChange.TurnLeft:
                PrepareTurnLeft();
                break;
            case DirectionChange.TurnRight:
                PrepareTurnRight();
                break;
            default:
                PrepareTurnAround();
                break;
        }
    }

    private void PrepareIntro()
    {
        positionFrom = tileFrom.transform.localPosition;
        transform.localPosition = positionFrom;
        positionTo = tileFrom.ExitPoint;
        direction = tileFrom.PathDirection;
        directionChange = DirectionChange.None;
        directionAngleFrom = directionAngleTo = direction.GetAngle();
        transform.localRotation = direction.GetRotation();
        progressFactor = 2f * speed;
    }

    private void PrepareForward()
    {
        transform.localRotation = direction.GetRotation();
        directionAngleTo = direction.GetAngle();
        //model.localPosition = Vector3.zero ;
        model.localPosition = new Vector3(pathOffset, 0f);
        progressFactor = speed;
    }

    private void PrepareTurnRight()
    {
        directionAngleTo = directionAngleFrom + 90f;
        //model.localPosition = new Vector3(-0.5f, 0f);
        model.localPosition = new Vector3(pathOffset - 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
        //progressFactor = 1f / (Mathf.PI * 0.25f);
        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));
    }

    private void PrepareTurnLeft()
    {
        directionAngleTo = directionAngleFrom - 90f;
        //model.localPosition = new Vector3(0.5f, 0f);
        model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
        //progressFactor = 1f / (Mathf.PI * 0.25f);
        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f + pathOffset));
    }

    private void PrepareTurnAround()
    {
        directionAngleTo = directionAngleFrom + (pathOffset < 0f ? 180f : -180f);
        //model.localPosition = Vector3.zero;
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localPosition = positionFrom;
        progressFactor = speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));
    }

    private void PrepareOutro()
    {
        positionTo = tileFrom.transform.localPosition;
        directionChange = DirectionChange.None;
        directionAngleTo = direction.GetAngle();
        //model.localPosition = Vector3.zero;
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localRotation = direction.GetRotation();
        progressFactor = 2f * speed;
    }
}
