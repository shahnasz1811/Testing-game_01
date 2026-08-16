using UnityEngine;
using System.Collections;

public class GrassExternalVelocityTrigger : MonoBehaviour
{
    private GrassVelocityController _grassVelocityController;
    private GameObject _player;
    private Rigidbody2D _playerRB;
    private Material _material;

    private Coroutine _easeInCoroutine;
    private Coroutine _easeOutCoroutine;

    private int _externalInfluence = Shader.PropertyToID("_ExternalInfluence");

    private float _startingXVelocity;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");

        if (_player == null)
        {
            Debug.LogError("GrassExternalVelocityTrigger: Player not found!");
            return;
        }

        _playerRB = _player.GetComponent<Rigidbody2D>();

        if (_playerRB == null)
        {
            Debug.LogError("GrassExternalVelocityTrigger: Player has no Rigidbody2D!");
            return;
        }

        _grassVelocityController = GetComponent<GrassVelocityController>();

        _material = GetComponent<Renderer>().material;

        _startingXVelocity = _material.GetFloat(_externalInfluence);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        float playerVelocity = _playerRB.linearVelocity.x;

        Debug.Log("Player entered grass. X Velocity: " + playerVelocity);

        if (Mathf.Abs(playerVelocity) > _grassVelocityController.VelocityThreshold)
        {
            StartEaseIn(playerVelocity);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        float playerVelocity = _playerRB.linearVelocity.x;

        if (Mathf.Abs(playerVelocity) > _grassVelocityController.VelocityThreshold)
        {
            // Stop ease-out if player starts moving again
            if (_easeOutCoroutine != null)
            {
                StopCoroutine(_easeOutCoroutine);
                _easeOutCoroutine = null;
            }

            // Update grass while player is moving
            if (_easeInCoroutine == null)
            {
                StartEaseIn(playerVelocity);
            }
            else
            {
                _grassVelocityController.InfluenceGrass(
                    _material,
                    playerVelocity * _grassVelocityController.ExternalInfluenceStrength
                );
            }
        }
        else
        {
            // Player is inside grass but stopped
            if (_easeInCoroutine == null && _easeOutCoroutine == null)
            {
                StartEaseOut();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        StartEaseOut();
    }

    private void StartEaseIn(float playerVelocity)
    {
        // Stop EaseOut
        if (_easeOutCoroutine != null)
        {
            StopCoroutine(_easeOutCoroutine);
            _easeOutCoroutine = null;
        }

        // Stop existing EaseIn
        if (_easeInCoroutine != null)
        {
            StopCoroutine(_easeInCoroutine);
        }

        float targetVelocity =
            playerVelocity *
            _grassVelocityController.ExternalInfluenceStrength;

        _easeInCoroutine = StartCoroutine(EaseIn(targetVelocity));
    }

    private void StartEaseOut()
    {
        // Stop EaseIn
        if (_easeInCoroutine != null)
        {
            StopCoroutine(_easeInCoroutine);
            _easeInCoroutine = null;
        }

        // Don't create multiple EaseOut coroutines
        if (_easeOutCoroutine == null)
        {
            _easeOutCoroutine = StartCoroutine(EaseOut());
        }
    }

    private IEnumerator EaseIn(float targetVelocity)
    {
        float elapsedTime = 0f;

        float startValue =
            _material.GetFloat(_externalInfluence);

        while (elapsedTime < _grassVelocityController.EaseInTime)
        {
            elapsedTime += Time.deltaTime;

            float t =
                elapsedTime /
                _grassVelocityController.EaseInTime;

            float value =
                Mathf.Lerp(
                    startValue,
                    targetVelocity,
                    t
                );

            _grassVelocityController.InfluenceGrass(
                _material,
                value
            );

            yield return null;
        }

        _grassVelocityController.InfluenceGrass(
            _material,
            targetVelocity
        );

        _easeInCoroutine = null;
    }

    private IEnumerator EaseOut()
    {
        float elapsedTime = 0f;

        float startValue =
            _material.GetFloat(_externalInfluence);

        while (elapsedTime < _grassVelocityController.EaseOutTime)
        {
            elapsedTime += Time.deltaTime;

            float t =
                elapsedTime /
                _grassVelocityController.EaseOutTime;

            float value =
                Mathf.Lerp(
                    startValue,
                    _startingXVelocity,
                    t
                );

            _grassVelocityController.InfluenceGrass(
                _material,
                value
            );

            yield return null;
        }

        _grassVelocityController.InfluenceGrass(
            _material,
            _startingXVelocity
        );

        _easeOutCoroutine = null;
    }
}