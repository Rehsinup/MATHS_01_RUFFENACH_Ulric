using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCharacter1 : MonoBehaviour
{
    public enum PhysicState
    {
        Ground,
        Air
    }

    [Serializable]
    private struct MovementValues
    {
        public float MaxSpeed;
        public float Acceleration;
        public float MaxAcceleration;
        [Tooltip("Range [-1, 1]")] public AnimationCurve AccelerationRemapFromVelocityDot;
    }

    [Serializable]
    private struct GravityValues
    {
        public float MaxForce;
        public float Acceleration;
        public float MaxAcceleration;
        public float CoyoteTime;
        [Tooltip("Range [0, 1]")] public AnimationCurve GravityRemapFromCoyoteTime;
    }

    [Serializable]
    private struct JumpValues
    {
        public float ImpulseForce;
        public float Deceleration;
        public float MaxDeceleration;
        [Tooltip("Range [0, 1]")] public AnimationCurve DecelerationFromAirTime;
        public float Height;
    }

    [Header("Gameplay")]
    [SerializeField] private MovementValues _groundPhysic = new MovementValues();
    [SerializeField] private MovementValues _airPhysic = new MovementValues();
    [SerializeField] private GravityValues _gravityParameters = new GravityValues();
    [SerializeField] private JumpValues _jumpParameters = new JumpValues();
    [SerializeField] private ContactFilter2D _groundContactFilter = new ContactFilter2D();

    [Header("Setup")]
    [SerializeField] private Transform _mesh = null;
    [SerializeField] private float _meshRotationSpeed = 10.0f;

    //Components
    private Rigidbody2D _rigidbody = null;

    //Force
    private Vector2 _forceToAdd = Vector2.zero;

    //Horizontal movement
    private float _currentHorizontalVelocity = 0.0f;
    private float _movementInput = 0.0f;
    private MovementValues _horizontalPhysic = new MovementValues();

    //Gravity
    private float _currentGravity = 0.0f;

    //Ground
    private bool _isGrounded = true;

    //Air
    private float _airTime = 0.0f;

    //Jump
    private float _currentJumpForce = 0.0f;
    private bool _isJumping = false;
    private float _jumpTime = 0.0f;

    //Event appelé quand on touche ou quitte le sol
    public event Action<PhysicState> OnPhysicStateChanged;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _horizontalPhysic = _groundPhysic;
        CalculateJumpTime();

        //On enregistre le changement de physic à l'event qui detecte le changement d'état du sol
        OnPhysicStateChanged += ChangePhysic;
        OnPhysicStateChanged += ResetGravity;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CalculateJumpTime();
    }
#endif

    private void CalculateJumpTime()
    {
        _jumpTime = _jumpParameters.Height / _jumpParameters.ImpulseForce;
    }

    private void Update()
    {
        RotateMesh();
    }

    private void FixedUpdate()
    {
        //On reset la force à ajouter cette boucle de fixed update
        _forceToAdd = Vector2.zero;

        //Fonction qui détecte si on touche le sol ou non
        //Et appelle les events associés
        GroundDetection();
        ManageAirTime();

        //On effectue tous les calculs physiques
        Movement();
        Gravity();
        JumpForce();

        //On ajoute la force au rigidbody
        _rigidbody.velocity += _forceToAdd;
    }

    private void GroundDetection()
    {
        //On utilise le filtre qui contient l'inclinaison du sol pour savoir si le rigidbody touche le sol ou non
        bool isTouchingGround = _rigidbody.IsTouching(_groundContactFilter);

        //Si le rigidbody touche le sol mais on a en mémoire qu'il ne le touche pas, on est sur la frame où il touche le sol
        if (isTouchingGround && !_isGrounded)
        {
            _isGrounded = true;
            //On invoque l'event en passant true pour signifier que le joueur arrive au sol
            OnPhysicStateChanged.Invoke(PhysicState.Ground);
        }
        //Si le rigidbody ne touche pas le sol mais on a en mémoire qu'il le touche, on est sur la frame où il quitte le sol
        else if (!isTouchingGround && _isGrounded)
        {
            _isGrounded = false;
            //On invoque l'event en passant false pour signifier que le joueur quitte au sol
            OnPhysicStateChanged.Invoke(PhysicState.Air);
        }
    }

    private void ManageAirTime()
    {
        if (!_isGrounded)
            _airTime += Time.fixedDeltaTime;
    }

    private void ChangePhysic(PhysicState groundState)
    {
        //On change la physique en fonction de si le joueur est au sol ou non
        if (groundState == PhysicState.Ground)
            _horizontalPhysic = _groundPhysic;
        else if (groundState == PhysicState.Air)
            _horizontalPhysic = _airPhysic;
    }

    private void Movement()
    {
        float maxSpeed = _horizontalPhysic.MaxSpeed * _movementInput;
        float velocityDot = Mathf.Clamp(_rigidbody.velocity.x * maxSpeed, -1.0f, 1.0f);
        velocityDot = _horizontalPhysic.AccelerationRemapFromVelocityDot.Evaluate(velocityDot);
        float acceleration = _horizontalPhysic.Acceleration * velocityDot * Time.fixedDeltaTime;

        //On fait avancer notre vitesse actuelle vers la max speed en fonction de l'acceleration
        _currentHorizontalVelocity = Mathf.MoveTowards(_currentHorizontalVelocity, maxSpeed, acceleration);

        //On calcul l'écart entre la velocité actuelle du rigidbody et la vélocité cible
        float velocityDelta = _currentHorizontalVelocity - _rigidbody.velocity.x;

        //On clamp le delta de vélocité avec l'acceleration maximum en négatif et positif pour éviter des bugs dans la physic
        velocityDelta = Mathf.Clamp(velocityDelta, -_horizontalPhysic.MaxAcceleration, _horizontalPhysic.MaxAcceleration);

        //On a ajoute le delta de vélocité à la force à donné ce tour de boucle au rigidbody
        _forceToAdd.x += velocityDelta;
    }

    private void RotateMesh()
    {
        if (_currentHorizontalVelocity == 0.0f)
            return;

        //On récupère la rotation acutelle du mesh
        float currentRotation = _mesh.eulerAngles.y;

        //On définit la rotation cible en fonction de la vélocité du personnage
        //90 à droite / 270 à gauche
        float targetRotation = _currentHorizontalVelocity > 0.0f ? 90.0f : 270f;
        //float targetRotation2 = 270.0f;
        //if (_currentHorizontalVelocity > 0.0f)
        //    targetRotation2 = 90.0f;

        //On interpole les rotations
        float newRotation = Mathf.MoveTowards(currentRotation, targetRotation, _meshRotationSpeed * Time.deltaTime);

        //On applique la nouvelle rotation au mesh
        _mesh.rotation = Quaternion.Euler(0.0f, newRotation, 0.0f);
    }

    private void Gravity()
    {
        if (_isGrounded || _isJumping)
            return;

        float coyoteTimeRatio = Mathf.Clamp01(_airTime / _gravityParameters.CoyoteTime);
        float acceleration = _gravityParameters.Acceleration * _gravityParameters.GravityRemapFromCoyoteTime.Evaluate(coyoteTimeRatio) * Time.fixedDeltaTime;

        _currentGravity = Mathf.MoveTowards(_currentGravity, _gravityParameters.MaxForce, acceleration);

        float velocityDelta = _currentGravity - _rigidbody.velocity.y;
        velocityDelta = Mathf.Clamp(velocityDelta, -_gravityParameters.MaxAcceleration, 0.0f);

        _forceToAdd.y += velocityDelta;
    }

    private void ResetGravity(PhysicState physicState)
    {
        if (physicState != PhysicState.Air)
        {
            _currentGravity = 0.0f;
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0.0f);
            _airTime = 0.0f;
        }
    }

    public void GetMovementInput(float input)
    {
        _movementInput = input;
    }

    public void StartJump()
    {
        if (!_isGrounded || _isJumping)
            return;

        _currentJumpForce = _jumpParameters.ImpulseForce;
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _currentJumpForce);
        _isJumping = true;
    }

    private void JumpForce()
    {
        if (!_isJumping)
            return;

        float jumpTimeRatio = Mathf.Clamp01(_airTime / _jumpTime);
        float deceleration = _jumpParameters.Deceleration *_jumpParameters.DecelerationFromAirTime.Evaluate(jumpTimeRatio) * Time.fixedDeltaTime;

        _currentJumpForce = Mathf.MoveTowards(_currentJumpForce, 0.0f, deceleration);

        float velocityDelta = _currentJumpForce - _rigidbody.velocity.y;
        velocityDelta = Mathf.Clamp(velocityDelta, -_jumpParameters.MaxDeceleration, 0.0f);

        _forceToAdd.y += velocityDelta;

        if (_airTime > _jumpTime)
        {
            _isJumping = false;
            _currentJumpForce = 0.0f;
        }
    }

    public void ActionOne()
    {

    }

    public void ActionTwo()
    {

    }
}