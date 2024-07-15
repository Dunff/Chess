using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChessPiece : MonoBehaviour
{
    public bool isWhite;
    public bool hasMoved;
    public PieceBehaviour pieceBehaviour;
    Collider myCollider;
    public bool canEnPassant = false;

    public King myKing;

    private void Start()
    {
        myCollider = GetComponent<Collider>();
    }

    public void LineOfSight()
    {
        pieceBehaviour.activePiece = gameObject;
        pieceBehaviour.pieceCollider = myCollider;
        pieceBehaviour.chessPiece = this;

        pieceBehaviour.LineOfSight();
    }
}
