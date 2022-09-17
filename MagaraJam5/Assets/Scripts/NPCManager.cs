using System;
using System.Collections;
using binc.PixelAnimator;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCManager : MonoBehaviour
{
    public float _speed;

    [SerializeField] private float _interractDistance;
    public LayerMask _interractLayer;

    [SerializeField] private float _wallCheckDistance;
    [SerializeField] private LayerMask _wallCheckLayer;

    public GameObject enterNPCIcon;

    [SerializeField] private bool leaveCooldown;
    public bool LeaveCooldown => leaveCooldown;

    private Rigidbody2D _rb;
    public AnimationManager anim;

    public PixelAnimation walk, idle, to, punchOne;
    public bool canMove = true;
    private int facingDirection;
    private Direction currentDirection;

    private Direction CurrentDirection
    {
        get => currentDirection;
        set => currentDirection = value;
    }

    private Coroutine _directionDecider;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        CurrentDirection = (Direction)Random.Range(-1, 2);

        _directionDecider = StartCoroutine(DirectionDeciderCoroutine());

        SetAnimation();
        anim.AddListener("To", SetAnimation);
        anim.SetProperty("CanMove", o => { canMove = (bool)o; });


        anim.AddListener("SetAnimation", SetAnimation);

        anim.AddListener("To", SetAnimation);

        anim.SetProperty("Velocity", o => { _rb.velocity += (Vector2)o * facingDirection; });
    }

    private void Update()
    {
        if (LeaveCooldown == false)
        {
            var closestNPC = GetClosestNpc();
            if (closestNPC != null)
                NPCConversationSystem.Instance.MakeConversationWith(this, closestNPC);
        }

        CheckIcon();
        CheckAnimations();
        CheckRotations();
        CheckWall();

    }

    private NPCManager GetClosestNpc()
    {
        var npcCols = Physics2D.OverlapCircleAll(transform.position, _interractDistance, _interractLayer);
        foreach (var col in npcCols)
        {
            if (col.GetComponent<NPCManager>() == null) continue;
            if (col.transform == this.transform) continue;
            if (col.GetComponent<NPCManager>().LeaveCooldown) continue;

            return col.GetComponent<NPCManager>();

        }

        return null;
    }

    public void CheckAnimations()
    {
        if (!canMove) return;

        if (Mathf.Abs(_rb.velocity.x) > 0 && anim.CurrentAnimation == idle)
        {
            anim.ChangeAnimation(to);
        }
        else if (_rb.velocity.x == 0 && anim.CurrentAnimation == walk)
        {
            anim.ChangeAnimation(to);
        }
    }

    public void CheckRotations()
    {
        if (currentDirection == Direction.right)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (currentDirection == Direction.left)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    public void CheckWall()
    {
        if (Physics2D.Raycast(transform.position, transform.right, _wallCheckDistance, _wallCheckLayer))
        {
            CurrentDirection = transform.right == Vector3.right ? Direction.left : Direction.right;
        }
    }

    public void CheckIcon()
    {
        enterNPCIcon.SetActive(GhostManager.Instance.AvailableNPC == this);
    }

    private void FixedUpdate()
    {
        if (canMove)
            _rb.velocity = new Vector2((int)CurrentDirection * _speed, _rb.velocity.y);
    }

    private void SetAnimation()
    {
        switch (_rb.velocity.x)
        {
            case > 0 or < 0:
                anim.ChangeAnimation(walk);
                break;
            case 0:
                anim.ChangeAnimation(idle);
                break;
        }
    }

    private IEnumerator DirectionDeciderCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(4, 10));
            CurrentDirection = Direction.none;
            yield return new WaitForSeconds(1.5f);
            CurrentDirection = Random.Range(0, 2) == 0 ? Direction.left : Direction.right;

        }
    }

    public void OnStartConversation()
    {
        StopCoroutine(_directionDecider);
        CurrentDirection = Direction.none;

    }

    public void OnEndConversation(Direction endDirection)
    {
        CurrentDirection = endDirection;
        _directionDecider = StartCoroutine(DirectionDeciderCoroutine());

        StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        leaveCooldown = true;
        yield return new WaitForSeconds(3);
        yield return new WaitUntil(() => GetClosestNpc() == null);
        leaveCooldown = false;
    }


    public void SetFacingDirection(int i)
    {
        facingDirection = i;
        switch (i)
        {
            case > 0:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case < 0:
                transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _interractDistance);
    }
    

}

public enum Direction
{
    left = -1, none = 0, right = 1
}
