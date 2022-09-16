using System;
using System.Collections;
using binc.PixelAnimator;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCManager : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _interractDistance;
    [SerializeField] private LayerMask _interractLayer;

    [SerializeField] private bool leaveCooldown;
    public bool LeaveCooldown => leaveCooldown;

    private Rigidbody2D _rb;
    [SerializeField] private AnimationManager anim;

    [SerializeField] private PixelAnimation walk, idle, to, punchOne;

    private Direction currentDirection;
    private Direction CurrentDirection{
        get => currentDirection;
        set{
            anim.ChangeAnimation(to);
            currentDirection = value;
        }
    }

    private Coroutine _directionDecider;
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        CurrentDirection = (Direction)Random.Range(-1, 2);

        _directionDecider = StartCoroutine(DirectionDeciderCoroutine());
        SetAnimation();
        anim.AddListener("To", SetAnimation);
    }
    private void Update()
    {
        if (CurrentDirection != Direction.none && LeaveCooldown == false)
        {
            var npcCols = Physics2D.OverlapCircleAll(transform.position, _interractDistance, _interractLayer);
            foreach (var col in npcCols) {
                //Animatorun collideri olup olmadigini kontrol editorum.
                if (col.GetComponent<NPCManager>() == null) continue;
                if (col.transform == this.transform) continue;
                if (col.GetComponent<NPCManager>().LeaveCooldown) continue;

                NPCConversationManager.Instance.MakeInteractionWith(this, col.GetComponent<NPCManager>());
                break;
            }
        }
        
        if(Input.GetMouseButtonDown(0))
        {
            anim.ChangeAnimation(punchOne);
        }
        
        
    }



    private void FixedUpdate(){
        _rb.velocity = new Vector2((int)CurrentDirection * _speed, _rb.velocity.y);
        
        if (_rb.velocity.x > 0) {
            transform.rotation = Quaternion.Euler(0,0,0);
        }else if (_rb.velocity.x < 0) {
            transform.rotation = Quaternion.Euler(0,180,0);
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
        leaveCooldown = false;
    }
    private Vector2 CurrentDirectionToVector => CurrentDirection switch
    {
        Direction.left => -Vector2.right,
        Direction.right => Vector2.right,
        _ => Vector2.zero
    };
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _interractDistance);
    }

    private void SetAnimation(){
        switch (CurrentDirection) {
            case Direction.left or Direction.right:
                anim.ChangeAnimation(walk);
                break;
            case Direction.none:
                anim.ChangeAnimation(idle);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    
}
public enum Direction
{
    left = -1, none = 0, right = 1
}
