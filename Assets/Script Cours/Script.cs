using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Script : MonoBehaviour
    
{
    private Vector3 Gauche;
    private Vector3 Droite;
    private float resultgauche;
    private float resultdroite;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private int Exercice01(Vector3 vectorOne, Vector3 vectorTwo)
    {
        Vector3 normalizedVectorOne = vectorOne.normalized; //Normaliser le vecteur 1 pour obtenir des résultars de dots entre -1 et 1
        Vector3 normalizedVectorTwo = vectorTwo.normalized; //Normaliser le Vecteur 2 pour obtenir des résultars de dots entre -1 et 1

        float dot = Vector3.Dot(normalizedVectorOne, normalizedVectorTwo); //Ensuite on les dot product entre les 2 vecteur normalisé
        //Si dot = 1, ils sont aligné
        //Si dot = -1, ils sont opposé
        if (dot == 1.0f)
            return 1;

        if (dot == -1.0f)
            return -1;

        //On effectue un cross avec un vecteur sur leur 3eme dimension pour obtenir les vecteurs de droite et de gauche par rapprort au vecteurOne.
        Vector3 rightVector = Vector3.Cross(normalizedVectorOne, new Vector3(0.0f, 0.0f, 1.0f)); //vecteur3 en face
        Vector3 leftVector = Vector3.Cross(normalizedVectorTwo, new Vector3(0.0f, 0.0f, -1.0f)); //vecteur3 en arrière

        //On effectue un dot entre ces deux vecteur et vecteurTwo pour savoir lequel il est le plus proche.
        float rightDot = Vector3.Dot(normalizedVectorTwo, rightVector);
        float leftDot = Vector3.Dot(normalizedVectorTwo, leftVector);

        //Si le dot de droite est plus petit que celui de gauche, alors le vectorTwo se trouve à droite
        if (rightDot <= leftDot)
            return 1;
        //Sinon, il est à gauche
        return -1;
    }
}







