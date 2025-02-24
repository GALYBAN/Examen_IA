using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Attacking
    }

    public EnemyState currentState;

    private NavMeshAgent _AIAgent;
    private Transform _playerTransform;

    [SerializeField] Transform[] _patrolPoints;
    private int _currentPatrolIndex = 0;

    [SerializeField] float _visionRange = 20;
    [SerializeField] float _visionAngle = 90;

    private Vector3 _playerLastPosition;

    [SerializeField] float _waitingTime = 5f;  

    private float _waitingTimer;

    [SerializeField] float _attackRange = 2f;  

    void Awake()
    {
        _AIAgent = GetComponent<NavMeshAgent>();
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    void Start()
    {
        currentState = EnemyState.Patrolling;
        SetNextPatrolPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;

            case EnemyState.Chasing:
                Chase();
                break;

            case EnemyState.Attacking:
                Attack();
                break;
        }
    }

    void Patrol()
    {
        if (OnRange())
        {
            currentState = EnemyState.Chasing;
            Debug.Log("Jugador detectado, cambiando a estado Chasing");
        }
    }

    void Chase()
    {
        _AIAgent.destination = _playerTransform.position;

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        if (distanceToPlayer <= _attackRange)
        {
            currentState = EnemyState.Attacking;
        }
    }

    void Wait()
    {
        _waitingTimer += Time.deltaTime;

        if (_waitingTimer >= _waitingTime)
        {
            currentState = EnemyState.Patrolling;
            SetNextPatrolPoint();
            _waitingTimer = 0;
        }
    }

    void Attack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        if (distanceToPlayer <= _attackRange)
        {
            Debug.Log("Enemigo atacando");
        }
        else
        {
            currentState = EnemyState.Chasing;
        }
    }

    bool OnRange()
    {
        Vector3 directionToPlayer = _playerTransform.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        if (_playerTransform.position == _playerLastPosition)
        {
            return true;
        }

        if (distanceToPlayer > _visionRange)
        {
            return false;
        }

        if (angleToPlayer > _visionAngle * 0.5f)
        {
            return false;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer))
        {
            if (hit.collider.CompareTag("Player"))
            {
                _playerLastPosition = _playerTransform.position;
                return true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    void SetNextPatrolPoint()
    {
        _AIAgent.destination = _patrolPoints[_currentPatrolIndex].position;

        _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _visionRange);

        Gizmos.color = Color.yellow;
        Vector3 fovLine1 = Quaternion.AngleAxis(_visionAngle * 0.5f, transform.up) * transform.forward * _visionRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-_visionAngle * 0.5f, transform.up) * transform.forward * _visionRange;
        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);
    }
}
