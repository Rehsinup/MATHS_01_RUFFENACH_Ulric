using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LerpExercice : MonoBehaviour
{
    [SerializeField] private float _oscillationAmplitude = 0.0f;
    [SerializeField] private float _oscillationFrequency = 0.0f;

    [SerializeField] private float _rotationSpeed = 0.0f;
    [SerializeField] private AnimationCurve _rotationCurve = null;

    private Vector3 _basePosition = Vector3.zero;
    private void Awake()
    {
        //On stock la position initiale de l'objet
        _basePosition = transform.position;
    }
    private void Update()
    {
        //On obtient l'oscillation en Y gr�ce � sinus. On multiplie le temps �coul� depuis le lancement du jeu par la fr�quence
        float osci = Mathf.Sin(Time.time * _oscillationFrequency) * _oscillationAmplitude;
        // On ram�ne l'ocsillation entre 0 et 1 (de base elle est entre -1 et 1)
        osci = (osci + 1.0f) / 2.0f;
        //On multiplie pas l'Amplitude
        osci *= _oscillationAmplitude;
        //On applique l'oscillation � la position
        transform.position = _basePosition + new Vector3(0.0f, osci, 0.0f);
    }
}