using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Move
{
    public Box from, to;

    public Pieces piece;

    public Pieces attackTo;

    public bool isCastling;

    public static Stack<Move> gameMoves = new Stack<Move>();

    public Move(Box from, Box to, Pieces piece, Pieces attackTo, bool isCastling = false)
    {
        gameMoves.Push(this);

        this.from = from;
        this.to = to;
        this.piece = piece;
        this.attackTo = attackTo;
        this.isCastling = isCastling;

        if (isCastling)
        {
            piece.MoveTo(to);
            if(from.x < to.x)
            {
                Game.boardMatrix[7, from.y].MoveTo(new Box(to.x - 1, to.y));
            }
            else
            {
                Game.boardMatrix[0, from.y].MoveTo(new Box(to.x + 1, to.y));
            }

            int t = piece.player == Game.whitePlayer ? 0 : 1;

            Game.shortCastlingPossible[t] = false;
            Game.longCastlingPossible[t] = false;
        }
        else
        {
            if (attackTo != null)
            {
                attackTo.gameObject.SetActive(false);
                Game.boardMatrix[attackTo.box.x, attackTo.box.y] = null;

                if (attackTo.player == Game.whitePlayer)
                {
                    Game.whitePieces.Remove(attackTo);
                }
                else
                {
                    Game.blackPieces.Remove(attackTo);
                }

                
            }
            
            piece.MoveTo(to);

            int t = piece.player == Game.whitePlayer ? 0 : 1;

            if (piece.tag == Game.tags[0])
            {
                Game.shortCastlingPossible[t] = false;
                Game.longCastlingPossible[t] = false;
            }
            else if (piece.tag == Game.tags[2] || attackTo != null && attackTo.tag == Game.tags[2])
            {
                if (piece.box.x == 0)
                {
                    Game.longCastlingPossible[t] = false;
                }
                else if (piece.box.x == 7)
                {
                    Game.shortCastlingPossible[t] = false;
                }
            }
            else if (piece.tag == Game.tags[5])
            {
                if(piece.player == Game.whitePlayer && piece.box.y == 7 || piece.player == Game.blackPlayer && piece.box.y == 0)
                {
                    piece.SetPiece(piece.box, piece.player, Game.tags[1]);
                }
            }
        }

        Game.Instance.candidateBoxes.Clear();
        Game.Instance.enPassantCandidate = null;

        // if king or rook is moved then the castling is not possible



        Game.turn = (Game.turn == Game.whitePlayer) ? Game.blackPlayer : Game.whitePlayer;


        // logic for mate
        ArrayList ar = Game.turn == Game.whitePlayer ? Game.whitePieces : Game.blackPieces;
        Pieces k = Game.turn == Game.whitePlayer ? Game.whiteKing : Game.blackKing;
        Game.gameOver = true;

        if (k.box.PiecesAttackOnBox().Count != 0)
        {
            for(int i = 0; i < ar.Count; i++)
            {
                Pieces p = (Pieces)ar[i];

                int temp = Game.Instance.FindCandidateBox(p) + (Game.Instance.enPassantCandidate == null ? 0 : 1);

                Game.Instance.candidateBoxes.Clear();
                Debug.Log(p.tag + " " + temp);
                
                if(temp > 0)
                {
                    Game.gameOver = false;
                    break;
                }
            }

            if (Game.gameOver) Debug.Log("Mate");
        }
        else
        {
            // this logic to chake still mate
            for (int i = 0; i < ar.Count; i++)
            {
                Pieces p = (Pieces)ar[i];
                if (p.CanMove())
                {
                    Game.gameOver = false;
                    break;
                }
            }
            if (Game.gameOver) Debug.Log("still mate");
        }
        
    }

    public static bool IsMoveSafe(Pieces p, int x, int y)
    {
        Pieces k = p.player == Game.whitePlayer ? Game.whiteKing : Game.blackKing;

        Pieces temp = Game.boardMatrix[x, y];
        Game.boardMatrix[x, y] = p;
        Box b = p.box;
        p.box = new Box(x, y);

        Game.boardMatrix[b.x, b.y] = null;

        int totalCount = k.box.PiecesAttackOnBox().Count;

        Game.boardMatrix[b.x, b.y] = p;

        Game.boardMatrix[x, y] = temp;
        p.box = b;

        return totalCount == 0;
    }
}

public class Box
{
    public int x;
    public int y;

    public Box()
    {
        this.x = 0; this.y = 0;
    }

    public Box(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Box(Box other)
    {
        this.x = other.x;
        this.y = other.y;
    }

    public ArrayList PiecesAttackOnBox()
    {
        ArrayList last = new ArrayList();

        // for rook and queen
        Pieces[] pi = new Pieces[4];

        pi[0] = FindPiecesOnLine(1, 0);
        pi[1] = FindPiecesOnLine(-1, 0);
        pi[2] = FindPiecesOnLine(0, 1);
        pi[3] = FindPiecesOnLine(0, -1);

        for(int i = 0; i < 4; i++)
        {
            if (pi[i] != null && pi[i].player != Game.turn && (pi[i].tag == Game.tags[1] || pi[i].tag == Game.tags[2]))
            {
                last.Add(pi[i]);
            }
        }

        // for bishop and queen
        pi[0] = FindPiecesOnLine(1, 1);
        pi[1] = FindPiecesOnLine(-1, -1);
        pi[2] = FindPiecesOnLine(-1, 1);
        pi[3] = FindPiecesOnLine(1, -1);

        for (int i = 0; i < 4; i++)
        {
            if (pi[i] != null && pi[i].player != Game.turn && (pi[i].tag == Game.tags[1] || pi[i].tag == Game.tags[3]))
            {
                last.Add(pi[i]);
            }
        }

        // for king
        for (int i = (x - 1) >= 0 ? (x - 1) : x; i <= ((x + 1) < 8 ? (x + 1) : x); i++)
        {
            for(int j = (y - 1) >= 0 ? (y - 1) : y; j <= ((y + 1) < 8 ? (y + 1) : y); j++)
            {
                if (Game.boardMatrix[i, j] != null && i != x && j != y)
                {
                    Pieces p = Game.boardMatrix[i, j];

                    if (p.player != Game.turn && p.tag == Game.tags[0])
                    {
                        last.Add(p);
                    }
                }
            }
        }

        // for knight
        int[] a = { x + 2, x + 2, x - 2, x - 2, x + 1, x + 1, x - 1, x - 1 };
        int[] b = { y + 1, y - 1, y + 1, y - 1, y + 2, y - 2, y + 2, y - 2 };

        for(int i = 0; i < 8; i++)
        {
            if (a[i] >= 0 && a[i] < 8 && b[i] >= 0 && b[i] < 8 && Game.boardMatrix[a[i], b[i]] != null)
            {
                Pieces p = Game.boardMatrix[a[i], b[i]];

                if(p.tag == Game.tags[4] && p.player != Game.turn)
                {
                    last.Add(p);
                }
            }
        }


        // for pawn
        if (Game.turn == Game.whitePlayer && y + 1 < 8 || Game.turn == Game.blackPlayer && y - 1 >= 0)
        {
            int t = Game.turn == Game.whitePlayer ? 1 : -1;

            if (x - 1 >= 0 && Game.boardMatrix[x - 1, y + t] != null)
            {
                Pieces p = Game.boardMatrix[x - 1, y + t];

                if (p.tag == Game.tags[5] && p.player != Game.turn)
                {
                    last.Add(p);
                }
            }

            if (x + 1 < 8 && Game.boardMatrix[x + 1, y + t] != null)
            {
                Pieces p = Game.boardMatrix[x + 1, y + t];

                if (p.tag == Game.tags[5] && p.player != Game.turn)
                {
                    last.Add(p);
                }
            }
        }

        return last;
    }

    public Pieces FindPiecesOnLine(int xi, int yi)
    {
        int tx = x + xi, ty = y + yi;

        while(tx >= 0 && tx < 8 && ty >= 0 && ty < 8)
        {
            if (Game.boardMatrix[tx, ty] != null)
            {
                return Game.boardMatrix[tx, ty];
            }

            tx += xi;
            ty += yi;
        }

        return null;
    }
}

