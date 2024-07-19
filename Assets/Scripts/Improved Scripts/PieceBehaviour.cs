using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceBehaviour : MonoBehaviour
{
    public GameManager gameManager;
    public Canvas pawnPromotionCanvas;
    public PawnPromotion pawnPromotion;
    public FenCalculator fenCalculator;

    [HideInInspector] public GameObject activePiece;
    [HideInInspector] public ChessPiece chessPiece;
    [HideInInspector] public List<GameObject> enPassantPawns;
    [HideInInspector] public GameObject enPassantVictim;
    [HideInInspector] public GameObject enPassantSquare;
    [HideInInspector] public Collider pieceCollider;

    //function which calculates a piece's possible moves based on the type of chesspiece.
    public void LineOfSight()
    {
        chessPiece = activePiece.GetComponent<ChessPiece>();
        pieceCollider = activePiece.GetComponent<Collider>();
        gameManager.possibleMoves.Clear();

        if (activePiece.tag == "Pawn")
        {
            Pawn();
        }
        if (activePiece.tag == "Rook")
        {
            Rook();
        }
        if (activePiece.tag == "Bishop")
        {
            Bishop();
        }
        if (activePiece.tag == "Queen")
        {
            Queen();
        }
        if (activePiece.tag == "King")
        {
            King();
        }
        if (activePiece.tag == "Knight")
        {
            Knight();
        }
    }

    //determines if the piece's new position is a valid move. 
    public bool ValidMove(Vector2 initialPos, Vector2 finalPos)
    {
        for (int i = 0; i < gameManager.possibleMoves.Count; i++)
        {
            if (finalPos - initialPos == gameManager.possibleMoves[i])
            {
                return true;
            }
        }

        return false;
    }

    void Pawn()
    {
        //check if the pawn has moved before. If has moved, then it can only move 1 space.
        int frontRayDistance = chessPiece.hasMoved ? 1 : 2;

        //determine what direction is "forward", and which direction is for possible attacks depending on piece colour.
        Vector3 forward = chessPiece.isWhite ? Vector3.up : Vector3.down;
        Vector3 leftDiagonal = chessPiece.isWhite ? new Vector3(-0.5f, 0.5f, 0) : new Vector3(-0.5f, -0.5f, 0);
        Vector3 rightDiagonal = chessPiece.isWhite ? new Vector3(0.5f, 0.5f, 0) : new Vector3(0.5f, -0.5f, 0);

        //disable collider to stop raycasts from colliding with the active piece. 
        pieceCollider.enabled = false;

        //create 3 raycasts - to check whats in front and if anything is available for capture on either side.
        Ray ray = new Ray(activePiece.transform.position, forward);
        Ray leftAttackRay = new Ray(activePiece.transform.position, leftDiagonal);
        Ray rightAttackRay = new Ray(activePiece.transform.position, rightDiagonal);

        RaycastHit hit;
        RaycastHit leftAttack;
        RaycastHit rightAttack;

        //determine how many spaces the pawn can move forward and add them to game manager move list.
        if (Physics.Raycast(ray, out hit, frontRayDistance))
        {
            float spacesAvailable = Mathf.Abs(hit.transform.localPosition.y - activePiece.transform.localPosition.y) - 1;
            for (int i = 1; i <= spacesAvailable; i++)
            {
                Vector2 move = new Vector2(0f, chessPiece.isWhite ? i : -i);
                gameManager.possibleMoves.Add(move);
            }
        }
        else
        {
            for (int i = 1; i <= frontRayDistance; i++)
            {
                Vector2 move = new Vector2(0f, chessPiece.isWhite ? i : -i);
                gameManager.possibleMoves.Add(move);
            }
        }

        //determine possible pawn attacks.
        if (Physics.Raycast(leftAttackRay, out leftAttack, 2))
        {
            if (leftAttack.collider.gameObject.tag == "Boundary")
            {
                return;
            }

            if (leftAttack.collider.gameObject.GetComponent<ChessPiece>().isWhite != chessPiece.isWhite)
            {
                Vector2 move = new Vector2(-1, chessPiece.isWhite ? 1 : -1);
                gameManager.possibleMoves.Add(move);
            }
        }

        if (Physics.Raycast(rightAttackRay, out rightAttack, 2))
        {
            if (rightAttack.collider.gameObject.tag == "Boundary")
            {
                return;
            }

            if (rightAttack.collider.gameObject.GetComponent<ChessPiece>().isWhite != chessPiece.isWhite)
            {
                Vector2 move = new Vector2(1, chessPiece.isWhite ? 1 : -1);
                gameManager.possibleMoves.Add(move);
            }
        }

        if (chessPiece.canEnPassant)
        {
            enPassantSquare = GameObject.Find(fenCalculator.enPassantSquareCode);
            Vector2 enPassantLocation = enPassantSquare.transform.localPosition;
            Vector2 currentLocation = activePiece.transform.localPosition;
            Vector2 move = enPassantLocation - currentLocation;
            gameManager.possibleMoves.Add(move);
        }

        pieceCollider.enabled = true;
    }

    public void PromotePawn()
    {
        pawnPromotion.promotingPawn = activePiece;
        pawnPromotionCanvas.enabled = true;
    }

    void AddMoves(Vector3 direction, Vector2 moveDirection, float rayDistance, bool isKing)
    {
        Ray ray = new Ray(activePiece.transform.position, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            float numberOfFreeSpaces = (direction == Vector3.up || direction == Vector3.down) ?
                Mathf.Abs(hit.collider.transform.localPosition.y - activePiece.transform.localPosition.y) :
                Mathf.Abs(hit.collider.transform.localPosition.x - activePiece.transform.localPosition.x);

            if (hit.collider.gameObject.layer == 7)
            {
                if (hit.collider.gameObject.GetComponent<ChessPiece>().isWhite != chessPiece.isWhite)
                {
                    numberOfFreeSpaces++;
                }
            }

            for (int i = 1; i < numberOfFreeSpaces; i++)
            {
                Vector2 move = moveDirection * i;
                gameManager.possibleMoves.Add(move);
            }
        }
        else
        {
            int maxMoves = isKing ? 1 : 7;
            for (int i = 1; i <= maxMoves; i++)
            {
                Vector2 move = moveDirection * i;
                gameManager.possibleMoves.Add(move);
            }
        }
    }

    void Rook()
    {
        pieceCollider.enabled = false;

        AddMoves(Vector3.up, Vector2.up, 20, false);
        AddMoves(Vector3.down, Vector2.down, 20, false);
        AddMoves(Vector3.left, Vector2.left, 20, false);
        AddMoves(Vector3.right, Vector2.right, 20, false);

        pieceCollider.enabled = true;
    }

    void Bishop()
    {
        pieceCollider.enabled = false;

        AddMoves(Vector3.up + Vector3.right, new Vector2(1f, 1f), 20, false);   // up-right
        AddMoves(Vector3.up + Vector3.left, new Vector2(-1f, 1f), 20, false);  // up-left
        AddMoves(Vector3.down + Vector3.right, new Vector2(1f, -1f), 20, false);    // down-right
        AddMoves(Vector3.down + Vector3.left, new Vector2(-1f, -1f), 20, false);    // down-left

        pieceCollider.enabled = true;
    }

    void Queen()
    {
        pieceCollider.enabled = false;

        AddMoves(Vector3.up + Vector3.right, new Vector2(1f, 1f), 20, false);   // up-right
        AddMoves(Vector3.up + Vector3.left, new Vector2(-1f, 1f), 20, false);  // up-left
        AddMoves(Vector3.down + Vector3.right, new Vector2(1f, -1f), 20, false);    // down-right
        AddMoves(Vector3.down + Vector3.left, new Vector2(-1f, -1f), 20, false);    // down-left
        AddMoves(Vector3.up, Vector2.up, 20, false);
        AddMoves(Vector3.down, Vector2.down, 20, false);
        AddMoves(Vector3.left, Vector2.left, 20, false);
        AddMoves(Vector3.right, Vector2.right, 20, false);

        pieceCollider.enabled = true;
    }

    void King()
    {
        pieceCollider.enabled = false;

        AddMoves(Vector3.up + Vector3.right, new Vector2(1f, 1f), 1.5f, true);   // up-right
        AddMoves(Vector3.up + Vector3.left, new Vector2(-1f, 1f), 1.5f, true);  // up-left
        AddMoves(Vector3.down + Vector3.right, new Vector2(1f, -1f), 1.5f, true);    // down-right
        AddMoves(Vector3.down + Vector3.left, new Vector2(-1f, -1f), 1.5f, true);    // down-left
        AddMoves(Vector3.up, Vector2.up, 1.5f, true);
        AddMoves(Vector3.down, Vector2.down, 1.5f, true);
        AddMoves(Vector3.left, Vector2.left, 1.5f, true);
        AddMoves(Vector3.right, Vector2.right, 1.5f, true);

        if (!chessPiece.hasMoved)
        {
            KingCheckForCastle(Vector3.right, Vector2.right);
            KingCheckForCastle(Vector3.left, Vector2.left);
        }

        pieceCollider.enabled = true;
    }

    void KingCheckForCastle(Vector3 direction, Vector2 moveDirection)
    {
        King king = activePiece.gameObject.GetComponent<King>();

        //if the king has ever moved, no castling is ever allowed.
        if (king.hasMoved)
        {
            return;
        }

        Ray ray = new Ray(activePiece.transform.position, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5))
        {
            if (hit.collider.gameObject.tag == "Rook")
            {
                //there is a clear path between the king and the rook.
                if (hit.collider.gameObject.GetComponent<ChessPiece>().hasMoved == false)
                {
                    //the rook has also not moved, therefore castling is allowed. King can move 2 sqaures to the side. 
                    Vector2 move = moveDirection * 2;
                    gameManager.possibleMoves.Add(move);
                }
            }
        }
    }

    void Knight()
    {
        pieceCollider.enabled = false;

        Vector2[] possibleMoves = new Vector2[]
        {
        new Vector2(2, 1),
        new Vector2(2, -1),
        new Vector2(-2, 1),
        new Vector2(-2, -1),
        new Vector2(1, 2),
        new Vector2(1, -2),
        new Vector2(-1, 2),
        new Vector2(-1, -2)
        };

        foreach (Vector2 move in possibleMoves)
        {
            Vector3 testPos = activePiece.transform.position + new Vector3(move.x, move.y, -10f);
            RaycastHit hit;

            if (Physics.Raycast(testPos, Vector3.forward, out hit, 30f))
            {
                if (hit.collider.gameObject.layer == 7)
                {
                    ChessPiece hitPiece = hit.collider.gameObject.GetComponent<ChessPiece>();

                    if (hitPiece.isWhite != chessPiece.isWhite)
                    {
                        gameManager.possibleMoves.Add(move);
                    }
                }

                if (hit.collider.gameObject.tag == "Square")
                {
                    gameManager.possibleMoves.Add(move);
                }
            }
        }

        pieceCollider.enabled = true;
    }
}
