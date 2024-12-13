using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Game : MonoBehaviour
{
    public static Game Instance;

    public static string whitePlayer = "White";
    public static string blackPlayer = "Black";

    public static string[] tags = { "King", "Queen", "Rook", "Bishop", "Knight", "Pawn" };

    // this matrix is same as chess board they contain chess pieces same as the board
    public static Pieces[,] boardMatrix = new Pieces[8, 8];

    public static ArrayList whitePieces = new ArrayList();
    public static ArrayList blackPieces = new ArrayList();

    public static Pieces whiteKing, blackKing;

    public static string turn;

    public static bool gameOver;

    public GameObject whitePiecesHolder, blackPiecesHolder;

    public GameObject chessPiece;

    // active piece is the piece of the player with have a turn and the piece is clicked
    [HideInInspector]
    public Pieces activePiece;

    // active plate is the square that show clicked box on the board
    public GameObject activePlate;

    // candidate Boxes are those boxes that are target by the active piece for move
    public Stack<Box> candidateBoxes = new Stack<Box>();

    [HideInInspector]
    public Box enPassantCandidate;

    [HideInInspector]
    public Box[] castlingCandidate = new Box[2];

    [HideInInspector]
    public static bool[] shortCastlingPossible = {true, true}, longCastlingPossible = {true, true};

    public float lengthOfBox = 0.22f;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InstantiateGame();
        gameOver = false;
        turn = whitePlayer;
        activePlate.SetActive(false);
    }

    private void Update()
    {
        if (!gameOver)
        {
            ChakeInput();
        }
    }

    private void ChakeInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Box box = GetBox(mousePos);

            activePlate.SetActive(false);

            if (box != null)
            {
                activePlate.transform.position = GetBoxPos(box);
                activePlate.SetActive(true);

                Pieces p = boardMatrix[box.x, box.y];

                // candition to chake clicked piece is of same player
                if (p != null && p.player == turn)
                {
                    // if the clicked piece is same player then we make that piece active
                    candidateBoxes.Clear();
                    activePiece = p;
                    FindCandidateBox(activePiece);
                }
                // candition to move the active piece
                else if (activePiece != null)
                {
                    while (candidateBoxes.Count > 0)
                    {
                        Box b = candidateBoxes.Pop();

                        if (b.x == box.x && b.y == box.y)
                        {
                            new Move(
                                    new Box(activePiece.box),
                                    new Box(box),
                                    activePiece,
                                    p
                                );

                            break;
                        }
                    }

                    // for en passant move by pawn
                    if (enPassantCandidate != null && enPassantCandidate.x == box.x && enPassantCandidate.y == box.y)
                    {
                        new Move(
                                new Box(activePiece.box),
                                new Box(box),
                                activePiece,
                                Move.gameMoves.Peek().piece
                            );
                    }

                    if (castlingCandidate[0] != null && castlingCandidate[0].x == box.x && castlingCandidate[0].y == box.y ||
                        castlingCandidate[1] != null && castlingCandidate[1].x == box.x && castlingCandidate[1].y == box.y)
                    {
                        new Move(
                                new Box(activePiece.box),
                                new Box(box),
                                activePiece,
                                null,
                                true
                            );
                    }

                    castlingCandidate[0] = null;
                    castlingCandidate[1] = null;

                    
                }
                else
                {
                    candidateBoxes.Clear();
                }
            }
        }
    }


    private void InstantiateGame()
    {
        // Instantiate white pieces

        InstantiateChessPiece(whitePlayer, tags[0], new Box(4, 0), whitePiecesHolder);
        whiteKing = Game.boardMatrix[4, 0];

        InstantiateChessPiece(whitePlayer, tags[1], new Box(3, 0), whitePiecesHolder);

        InstantiateChessPiece(whitePlayer, tags[2], new Box(0, 0), whitePiecesHolder);
        InstantiateChessPiece(whitePlayer, tags[2], new Box(7, 0), whitePiecesHolder);

        InstantiateChessPiece(whitePlayer, tags[3], new Box(2, 0), whitePiecesHolder);
        InstantiateChessPiece(whitePlayer, tags[3], new Box(5, 0), whitePiecesHolder);

        InstantiateChessPiece(whitePlayer, tags[4], new Box(1, 0), whitePiecesHolder);
        InstantiateChessPiece(whitePlayer, tags[4], new Box(6, 0), whitePiecesHolder);

        for (int i = 0; i < 8; i++)
        {
            InstantiateChessPiece(whitePlayer, tags[5], new Box(i, 1), whitePiecesHolder);
        }

        // Instantiate black pieces

        InstantiateChessPiece(blackPlayer, tags[0], new Box(4, 7), blackPiecesHolder);
        blackKing = Game.boardMatrix[4, 7];

        InstantiateChessPiece(blackPlayer, tags[1], new Box(3, 7), blackPiecesHolder);

        InstantiateChessPiece(blackPlayer, tags[2], new Box(0, 7), blackPiecesHolder);
        InstantiateChessPiece(blackPlayer, tags[2], new Box(7, 7), blackPiecesHolder);

        InstantiateChessPiece(blackPlayer, tags[3], new Box(2, 7), blackPiecesHolder);
        InstantiateChessPiece(blackPlayer, tags[3], new Box(5, 7), blackPiecesHolder);

        InstantiateChessPiece(blackPlayer, tags[4], new Box(1, 7), blackPiecesHolder);
        InstantiateChessPiece(blackPlayer, tags[4], new Box(6, 7), blackPiecesHolder);

        for (int i = 0; i < 8; i++)
        {
            InstantiateChessPiece(blackPlayer, tags[5], new Box(i, 6), blackPiecesHolder);
        }
    }

    public void InstantiateChessPiece(string player, string tag, Box b, GameObject holder)
    {
        boardMatrix[b.x, b.y] = Instantiate(chessPiece, GetBoxPos(b), Quaternion.identity).GetComponent<Pieces>();
        boardMatrix[b.x, b.y].transform.SetParent(holder.transform);
        boardMatrix[b.x, b.y].SetPiece(b, player, tag);

        if(player == whitePlayer)
        {
            whitePieces.Add(boardMatrix[b.x, b.y]);
        }
        else
        {
            blackPieces.Add(boardMatrix[b.x, b.y]);
        }
    }


    

    // this method find all cnadidate box on chess board for active piece
    public int FindCandidateBox(Pieces p)
    {
        int x = p.box.x, y = p.box.y;

        switch (p.tag)
        {
            case "King":
                AddToCandidate(p, x + 1, y + 1);
                AddToCandidate(p, x, y + 1);
                AddToCandidate(p, x - 1, y + 1);
                AddToCandidate(p, x + 1, y);
                AddToCandidate(p, x - 1, y);
                AddToCandidate(p, x + 1, y - 1);
                AddToCandidate(p, x, y - 1);
                AddToCandidate(p, x - 1, y - 1);

                int t = p.player == Game.whitePlayer ? 0 : 1;

                if(p.box.PiecesAttackOnBox().Count == 0)
                {
                    if (longCastlingPossible[t] && boardMatrix[p.box.x - 1, p.box.y] == null && boardMatrix[p.box.x - 2, p.box.y] == null && boardMatrix[p.box.x - 3, p.box.y] == null)
                    {
                        castlingCandidate[0] = new Box(p.box.x - 2, p.box.y);
                    }

                    if (shortCastlingPossible[t] && boardMatrix[p.box.x + 1, p.box.y] == null && boardMatrix[p.box.x + 2, p.box.y] == null)
                    {
                        castlingCandidate[1] = new Box(p.box.x + 2, p.box.y);
                    }
                }
               
                break;

            case "Queen":
                FindInLine(p, 1, 1, p.box);
                FindInLine(p, -1, -1, p.box);
                FindInLine(p, 1, -1, p.box);
                FindInLine(p, -1, 1, p.box);
                FindInLine(p, 0, 1, p.box);
                FindInLine(p, 0, -1, p.box);
                FindInLine(p, 1, 0, p.box);
                FindInLine(p, -1, 0, p.box);
                break;

            case "Rook":
                FindInLine(p, 0, 1, p.box);
                FindInLine(p, 0, -1, p.box);
                FindInLine(p, 1, 0, p.box);
                FindInLine(p, -1, 0, p.box);
                break;

            case "Bishop":
                FindInLine(p, 1, 1, p.box);
                FindInLine(p, -1, -1, p.box);
                FindInLine(p, 1, -1, p.box);
                FindInLine(p, -1, 1, p.box);
                break;

            case "Knight":
                AddToCandidate(p, x + 1, y + 2);
                AddToCandidate(p, x - 1, y + 2);
                AddToCandidate(p, x + 2, y + 1);
                AddToCandidate(p, x - 2, y + 1);
                AddToCandidate(p, x + 2, y - 1);
                AddToCandidate(p, x - 2, y - 1);
                AddToCandidate(p, x + 1, y - 2);
                AddToCandidate(p, x - 1, y - 2);
                break;

            case "Pawn":
                int yi = p.player == whitePlayer ? 1 : -1;
                CandidateForPawn(p, x, y, yi);
                break;

        }

        return candidateBoxes.Count;
    }

    private void CandidateForPawn(Pieces p, int x, int y, int yi)
    {
        if (boardMatrix[x, y + yi] == null)
        {
            AddToCandidate(p, x, y + yi);

            if (yi == 1 && y == 1 && boardMatrix[x, 3] == null)
            {
                AddToCandidate(p, x, 3);
            }
            else if (yi == -1 && y == 6 && boardMatrix[x, 4] == null)
            {
                AddToCandidate(p, x, 4);
            }
        }

        if (x - 1 >= 0 && boardMatrix[x - 1, y + yi] != null && boardMatrix[x - 1, y + yi].player != turn)
        {
            AddToCandidate(p, x - 1, y + yi);
        }
        if (x + 1 < 8 && boardMatrix[x + 1, y + yi] != null && boardMatrix[x + 1, y + yi].player != turn)
        {
            AddToCandidate(p, x + 1, y + yi);
        }

        // En Passant
        if (y == 4 && yi == 1)
        {
            Move lastMove = Move.gameMoves.Peek();
            Box f = lastMove.from, t = lastMove.to;

            if(x - 1 >= 0 && lastMove.piece.tag == tags[5] && f.x == x - 1 && f.y == 6 && t.y == 4)
            {
                enPassantCandidate = new Box(x - 1, y + 1);
            }
            else if (x + 1 < 8 && lastMove.piece.tag == tags[5] && f.x == x + 1 && f.y == 6 && t.y == 4)
            {
                enPassantCandidate = new Box(x + 1, y + 1);
            }
        }
        else if (y == 3 && yi == -1)
        {
            Move lastMove = Move.gameMoves.Peek();
            Box f = lastMove.from, t = lastMove.to;

            if (x - 1 >= 0 && lastMove.piece.tag == tags[5] && f.x == x - 1 && f.y == 1 && t.y == 3)
            {
                enPassantCandidate = new Box(x - 1, y - 1);
            }
            else if (x + 1 < 8 && lastMove.piece.tag == tags[5] && f.x == x + 1 && f.y == 1 && t.y == 3)
            {
                enPassantCandidate = new Box(x + 1, y - 1);
            }
        }
    }

    private void AddToCandidate(Pieces p,int x, int y)
    {
        if (x < 0 || x >= 8 || y < 0 || y >= 8) return;

        if ((boardMatrix[x, y] == null || boardMatrix[x, y].player != turn) && Move.IsMoveSafe(p, x, y))
        {
            candidateBoxes.Push(new Box(x, y));
        }
    }

    

    private void FindInLine(Pieces p, int xi, int yi, Box b)
    {
        int x = b.x + xi, y = b.y + yi;

        while (x >= 0 && x < 8 && y >= 0 && y < 8)
        {
            AddToCandidate(p, x, y);
            
            if (boardMatrix[x, y] != null)
            {
                break;
            }

            x += xi;
            y += yi;
        } 
    }

    private Box GetBox(Vector2 pos)
    {
        Box temp = new Box();
        pos.x = (pos.x - transform.position.x);
        pos.y = (pos.y - transform.position.y);

        if (pos.x < -lengthOfBox * transform.localScale.x * 4 || pos.x > lengthOfBox * transform.localScale.x * 4 ||
            pos.y < -lengthOfBox * transform.localScale.y * 4 || pos.y > lengthOfBox * transform.localScale.y * 4)
        {
            return null;
        }

        temp.x = (int)(pos.x / (lengthOfBox * transform.localScale.x) + 4);
        temp.y = (int)(pos.y / (lengthOfBox * transform.localScale.y) + 4);

        return temp;
    }

    public Vector2 GetBoxPos(Box box)
    {
        Vector2 pos = new Vector2((-7 * lengthOfBox * transform.localScale.x) / 2, (-7 * lengthOfBox * transform.localScale.y) / 2);
        pos.x += box.x * lengthOfBox * transform.localScale.x;
        pos.y += box.y * lengthOfBox * transform.localScale.y;

        return pos;
    }
}