using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _objectToFollow = null;

    [SerializeField] private Vector2 _maxOffset = Vector2.zero;
    [SerializeField] private float _followSpeed = 1.0f;
    [SerializeField] private AnimationCurve _speedFactorFromOffset = null;

    private void Update()
    {
        float xOffset = transform.position.x - _objectToFollow.transform.position.x;
        float offsetSign = Mathf.Sign(xOffset);

        float speed = _followSpeed * _speedFactorFromOffset.Evaluate(xOffset * offsetSign)* Time.deltaTime;

        float newOffset = xOffset + speed* offsetSign;
        newOffset = Mathf.Clamp(newOffset, -_maxOffset.x, _maxOffset.y);

        float newXPosition = _objectToFollow.transform.position.x + newOffset;


        Vector3 newPosition = transform.position;
        newPosition.x = newXPosition;
        transform.position = newPosition;

    }
}
