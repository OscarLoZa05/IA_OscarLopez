using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent _enemyAgent;
    

    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Searhing,
        Attacking,
        Waiting,

    }

    //Cosas Patrullar
    public EnemyState currentState;
    Transform _player;
    Vector3 _playerLastPositionKnown;
    [SerializeField] private Transform[] _patrolPoints;
    public int indexPatrolling = 0;

    //Cosas de Detección
    [SerializeField] private float _detectionRange = 7;
    [SerializeField] private float _detectionAngle = 90;

    //Cosas de Busqueda
    public float _searchTimer;
    [SerializeField] private float _searchWaitTimer = 15;
    [SerializeField] private float _searchRadius = 10;

    //Cosas de atacar
    public float _attackWaitTimer = 5;
    public float _attackTimer = 0;

    //Cosas Esperar
    [SerializeField] public float _waitTimer;
    [SerializeField] private float _waitingWaitTimer = 5;



    public void Awake()
    {
        _enemyAgent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindWithTag("Player").transform;
    }

    void Start()
    {
        currentState = EnemyState.Patrolling;
        indexPatrolling = 0;
        PatrolPoint();
        //SetRandomPatrolPoint();
    }

    void Update()
    {
        switch(currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
            break;
            case EnemyState.Chasing:
                Chase();
            break;
            case EnemyState.Searhing:
                Search();
            break;
            case EnemyState.Attacking:
                Attack();
            break;
            case EnemyState.Waiting:
                Wait();
            break;
            default:
                Patrol();
            break;
        }  
    }

    void Patrol()
    {
        if(OnRange())
        {
            currentState = EnemyState.Chasing;
        }
        if(_enemyAgent.remainingDistance < 0.5f)
        {
            currentState = EnemyState.Waiting;
        }

    }
    void Chase()
    {
        if(!OnRange())
        {
            currentState = EnemyState.Searhing;
        }
        if(OnRange())
        {
            _enemyAgent.SetDestination(_player.position);

            _playerLastPositionKnown = _player.position;

            if(_enemyAgent.remainingDistance < 1f)
            {
                //_attackTimer = 0;
                currentState = EnemyState.Attacking;
            }
        }        
    }
    
    void Search()
    {
        if(OnRange())
        {
            currentState = EnemyState.Chasing;
        }

        _searchTimer += Time.deltaTime;

        if(_searchTimer < _searchWaitTimer)
        {
            if(_enemyAgent.remainingDistance < 0.5f)
            {
                Vector3 randomPoint;
                if(RandomSearchPoint(_playerLastPositionKnown, _searchRadius, out randomPoint))
                {
                    _enemyAgent.SetDestination(randomPoint);
                }
            }
        }
        else
        {
            currentState = EnemyState.Patrolling;
            PatrolPoint();
            _searchTimer = 0;
        }
    }

    void Attack()
    {
        if(OnRange() && Vector3.Distance(transform.position, _player.position) < 1.5f)
        {
            _attackTimer += Time.deltaTime;
            
            if(_attackTimer > _attackWaitTimer)
            {
                Debug.Log("Atacado");
                _attackTimer = 0;
                currentState = EnemyState.Chasing;
            }
        }
        if(!OnRange())
        {
            _attackTimer = 0;
            currentState = EnemyState.Searhing;
        }
    }
    

    void Wait()
    {
        if(OnRange())
        {
            currentState = EnemyState.Chasing;
        }
        if(!OnRange())
        {
            _waitTimer += Time.deltaTime;

            if(_waitTimer >= _waitingWaitTimer)
            {
                indexPatrolling ++;
                if(indexPatrolling >= _patrolPoints.Length)
                {
                    indexPatrolling = 0;
                }
                currentState = EnemyState.Patrolling;
                PatrolPoint();
            }
        }   
    }

    void PatrolPoint()
    {
        _waitTimer = 0;
        if(indexPatrolling < _patrolPoints.Length)
        {
            _enemyAgent.SetDestination(_patrolPoints[indexPatrolling].position);
        }
        
    }

    bool RandomSearchPoint(Vector3 center, float radius, out Vector3 point)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * radius;
        
        NavMeshHit hit;
        if(NavMesh.SamplePosition(randomPoint, out hit, 4, NavMesh.AllAreas))
        {
            point = hit.position;
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    /*void SetRandomPatrolPoint()
    {
        _enemyAgent.SetDestination(_patrolPoints[Random.Range(0, _patrolPoints.Length)].position);
    }*/

    bool OnRange()
    {
        /*if(Vector3.Distance(transform.position, _player.position) < _detectionRange)
        {
            return true;
        }
        else
        {
            return false;
        }*/

        Vector3 directionToPlayer = _player.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        /*if(_player.position == _playerLastPositionKnown)
        {
            return true;
        }*/

        if(distanceToPlayer > _detectionRange)
        {
            return false;
        }
        
        if(angleToPlayer > _detectionAngle * 0.5f)
        {
            return false; 
        }

        RaycastHit hit;
        if(Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer))
        {
            if(hit.collider.CompareTag("Player"))
            {
                _playerLastPositionKnown = _player.position;

                return true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        foreach (Transform point in _patrolPoints)
        {
            Gizmos.DrawWireSphere(point.position, 0.5f);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        Gizmos.color = Color.yellow;
        Vector3 fovLine1 = Quaternion.AngleAxis(_detectionAngle * 0.5f, transform.up) * transform.forward * _detectionRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-_detectionAngle * 0.5f, transform.up) * transform.forward * _detectionRange;

        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);

    }    
}
