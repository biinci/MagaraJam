using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCManager : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _interractDistance;
    [SerializeField] private LayerMask _interractLayer;

    public bool LeaveCooldown;

    Rigidbody2D _rb;
    Direction _currentDirection;
    Coroutine _directionDecider;
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _currentDirection = (Direction)Random.Range(-1, 2);

        _directionDecider = StartCoroutine(DirectionDeciderCoroutine());
    }
    private void Update()
    {
        _rb.velocity = new Vector2((int)_currentDirection * _speed, _rb.velocity.y);

        if (_currentDirection != Direction.none && LeaveCooldown == false)
        {
            Collider2D[] npcCols = Physics2D.OverlapCircleAll(transform.position, _interractDistance, _interractLayer);
            foreach (var col in npcCols)
            {
                if (col.transform == this.transform) continue;
                if (col.GetComponent<NPCManager>().LeaveCooldown == true) continue;

                NPCConversationManager.Instance.MakeInterractionWith(this, col.GetComponent<NPCManager>());
                break;
            }
        }
    }
    IEnumerator DirectionDeciderCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(4, 10));
            _currentDirection = Direction.none;
            yield return new WaitForSeconds(1.5f);
            _currentDirection = Random.Range(0, 2) == 0 ? Direction.left : Direction.right;

        }
    }
    public void OnStartConversation()
    {
        StopCoroutine(_directionDecider);
        _currentDirection = Direction.none;

    }
    public void OnEndConversation(Direction endDirection)
    {
        _currentDirection = endDirection;
        _directionDecider = StartCoroutine(DirectionDeciderCoroutine());

        StartCoroutine(CooldownCoroutine());
    }
    IEnumerator CooldownCoroutine()
    {
        LeaveCooldown = true;
        yield return new WaitForSeconds(3);
        LeaveCooldown = false;
    }
    private Vector2 CurrentDirectionToVector => _currentDirection switch
    {
        Direction.left => -Vector2.right,
        Direction.right => Vector2.right,
        _ => Vector2.zero
    };
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _interractDistance);
    }
}
public enum Direction
{
    left = -1, none = 0, right = 1
}
