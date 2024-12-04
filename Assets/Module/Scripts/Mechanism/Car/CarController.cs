using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    private Rigidbody m_carRb;
    [SerializeField] private Transform[] _rayPoints;
    [SerializeField] private LayerMask _drivableLayer;

    [Header("Suspension Settings")]
    [SerializeField] private float _springStiffness;
    [SerializeField] private float _damperStiffness;
    [SerializeField] private float _restLength;
    [SerializeField] private float _springTravel;
    [SerializeField] private float _wheelRadius;

    private int[] m_wheelsGrounded = new int[4];
    private bool isGrounded = false;

    private float m_moveInput = 0;
    private float m_steerInput = 0;

    [Header("Car Settings")]
    [SerializeField] private float _acceleration = 25f;
    [SerializeField] private float _maxSpeed = 100f;
    [SerializeField] private float _deceleration = 10f;

    private Vector3 m_currentCarLocalVelocity = Vector3.zero;
    private float m_carVelocityRatio = 0;

    #region UNITY FUNCTIONS

    private void Start()
    {
        m_carRb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        GetPlayerInput();
    }

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
    }

    #endregion

    #region INPUT HANDELING

    private void GetPlayerInput()
    {
        m_moveInput = Input.GetAxis("Vertical");
        m_steerInput = Input.GetAxis("Horizontal");
    }

    #endregion

    #region CAR STATUS CHECK
    private void GroundCheck()
    {
        int tempGroundCheckCounter = 0;

        for(int i = 0; i < m_wheelsGrounded.Length; i++)
        {
            tempGroundCheckCounter += m_wheelsGrounded[i];
        }

        if(tempGroundCheckCounter > 1)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void CalculateCarVelocity()
    {
        m_currentCarLocalVelocity = transform.InverseTransformDirection(m_carRb.linearVelocity);
        m_carVelocityRatio = m_currentCarLocalVelocity.z / _maxSpeed;
    }

    #endregion

    #region CAR SETTINGS
    private void Suspension()
    {
        for (int i = 0; i < _rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxLength = _restLength + _springTravel;

            if (Physics.Raycast(_rayPoints[i].position, -_rayPoints[i].up, out hit, maxLength + _wheelRadius, _drivableLayer))
            {
                m_wheelsGrounded[i] = 1;

                float currentSpringLength = hit.distance - _wheelRadius;
                float springCompression = (_restLength - currentSpringLength) / _springTravel;

                float springVelocity = Vector3.Dot(m_carRb.GetPointVelocity(_rayPoints[i].position), _rayPoints[i].up);
                float dampForce = _damperStiffness * springVelocity;

                float springForce = _springStiffness * springCompression;

                float netForce = springForce - dampForce;

                m_carRb.AddForceAtPosition(netForce * _rayPoints[i].up, _rayPoints[i].position);

                Debug.DrawLine(_rayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                m_wheelsGrounded[i] = 0;

                Debug.DrawLine(_rayPoints[i].position, _rayPoints[i].position + (_wheelRadius * maxLength) * -_rayPoints[i].up, Color.green);
            }
        }
    }

    #endregion

}











/*using UnityEngine;
using System;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    public enum ControlMode
    {
        Keyboard,
        Buttons
    };

    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        //public GameObject wheelEffectObj;
        //public ParticleSystem smokeParticle;
        public Axel axel;
    }

    public ControlMode control;

    public float moveSpeed;
    public float brakeSpeed;
    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    private Rigidbody carRb;

    //private CarLights carLights;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;

        //carLights = GetComponent<CarLights>();
    }

    void Update()
    {
        GetInputs();
        AnimateWheels();
        WheelEffects();
    }

    void FixedUpdate()
    {
        Move();
        Steer();
        Deaccelerate();
    }

    public void MoveInput(float input)
    {
        moveInput = input;
    }

    public void SteerInput(float input)
    {
        steerInput = input;
    }

    void GetInputs()
    {
        if (control == ControlMode.Keyboard)
        {
            moveInput = Input.GetAxis("Vertical");
            steerInput = Input.GetAxis("Horizontal");
        }
    }

    void Move()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * moveSpeed * maxAcceleration * Time.fixedDeltaTime;
        }
    }

    void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.2f);
            }
        }
    }

    void Deaccelerate()
    {
        if (moveInput == 0)
        {
            foreach (var wheel in wheels)
            {
                if(wheel.wheelCollider.motorTorque > 0)
                    wheel.wheelCollider.motorTorque -= brakeSpeed * brakeAcceleration * Time.fixedDeltaTime;
            }

            //carLights.isBackLightOn = true;
            //carLights.OperateBackLights();
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }

            //carLights.isBackLightOn = false;
            //carLights.OperateBackLights();
        }
    }

    void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    void WheelEffects()
    {
        foreach (var wheel in wheels)
        {
            //var dirtParticleMainSettings = wheel.smokeParticle.main;

            if (Input.GetKey(KeyCode.Space) && wheel.axel == Axel.Rear && wheel.wheelCollider.isGrounded == true && carRb.linearVelocity.magnitude >= 10.0f)
            {
                //wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
                //wheel.smokeParticle.Emit(1);
            }
            else
            {
                //wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
            }
        }
    }
}*/

/*using UnityEngine;

public class CarController : MonoBehaviour
{
    public float acceleration = 1000f;
    public float turnSpeed = 5f;
    public float jumpForce = 500f;
    public float boostForce = 1500f;
    public float maxSpeed = 20f;

    private Rigidbody rb;
    private bool isGrounded;
    private float inputHorizontal;
    private float inputVertical;

    public Transform[] groundCheckPoints; // Assign ground check points around the car
    public LayerMask groundLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Input
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // Boost
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Boost();
        }
    }

    void FixedUpdate()
    {
        // Movement
        Drive();
        Steer();

        // Check if car is grounded
        CheckGrounded();
    }

    private void Drive()
    {
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            Vector3 force = transform.forward * inputVertical * acceleration * Time.fixedDeltaTime;
            rb.AddForce(force, ForceMode.Acceleration);
        }
    }

    private void Steer()
    {
        float turn = inputHorizontal * turnSpeed * Time.fixedDeltaTime;
        transform.Rotate(0, turn, 0);
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void Boost()
    {
        if (rb.linearVelocity.magnitude < maxSpeed * 1.5f) // Boost speed cap
        {
            rb.AddForce(transform.forward * boostForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    private void CheckGrounded()
    {
        isGrounded = false;
        foreach (Transform point in groundCheckPoints)
        {
            if (Physics.Raycast(point.position, Vector3.down, 0.5f, groundLayer))
            {
                isGrounded = true;
                break;
            }
        }
    }
}*/
