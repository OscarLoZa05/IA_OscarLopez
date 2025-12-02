using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    private NavMeshAgent _playerAgent;

    private InputAction _clickAction;
    private InputAction _mouseAction;
    private Vector2 _mousePosition;


    void Awake()
    {
        _playerAgent = GetComponent<NavMeshAgent>();
        _clickAction = InputSystem.actions["Attack"];
        _mouseAction = InputSystem.actions["Look"];
    }

    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        _mousePosition = _mouseAction.ReadValue<Vector2>();

        if(_clickAction.WasPressedThisFrame())
        {
            SetPlayerDestination();
        }
    }

    void SetPlayerDestination()
    {
        Ray ray = Camera.main.ScreenPointToRay(_mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            _playerAgent.SetDestination(hit.point);
        }

    }
}
