using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreeDPool.Managers;
using ThreeDPool.EventHandlers;

public class SpinEffectController : MonoBehaviour
{
    public enum SpinType
    {
        TopSpin,
        BackSpin,
        LeftSpin,
        RightSpin,
    }

    [SerializeField]
    private float _spinForce;

    private float _spinRatio;

    private SpinType _spinType;

    private Vector3 _centerPosition;

    private Vector3 _prePosition;

    private float _max = 0.39f;

    public float GetSpinForceBySpinRatioAndForceGathered(float forceGatheredToHit)
    {
        return _spinForce * _spinRatio * forceGatheredToHit;
    }

    public SpinType GetSpinType()
    {
        return _spinType;
    }

    void Start()
    {
        EventManager.Subscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
        _centerPosition = transform.position;
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, _centerPosition);

        if (distance < _max)
        {
            _prePosition = transform.position;
        }
    }

    private void OnGameInputEvent(object sender, IGameEvent gameEvent)
    {
        GameInputEvent gameInputEvent = (GameInputEvent)gameEvent;
        Vector3 direction;
        float offset = gameInputEvent.axisOffset / 15;
        float distance = Vector3.Distance(transform.position, _centerPosition);

        switch (gameInputEvent.State)
        {
            case GameInputEvent.States.HorizontalPointMovement:
                if (distance < _max)
                {
                    direction = transform.right;
                    transform.position = transform.position + direction * offset;
                }
                else
                {
                    transform.position = _prePosition;
                }
                break;

            case GameInputEvent.States.VerticalPointMovement:
                if (distance < _max)
                {
                    direction = transform.forward;
                    transform.position = transform.position + direction * offset;
                }
                else
                {
                    transform.position = _prePosition;
                }
                break;
        }

        _spinRatio = distance / _max;

        if(transform.position.y < _centerPosition.y)
        {
            _spinType = SpinType.BackSpin;
        }

        if (transform.position.y > _centerPosition.y)
        {
            _spinType = SpinType.TopSpin;
        }
    }
}
