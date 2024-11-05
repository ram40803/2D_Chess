using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pieces : MonoBehaviour
{
    [HideInInspector]
    public Box box;

    [HideInInspector]
    public string player;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetPiece(Box box, string player, string tag)
    {
        this.box = box;
        this.player = player;
        this.tag = tag;

        transform.localScale = Vector3.one;

        sr.sprite = Resources.Load<Sprite>("chess_green/" + player.ToLower() + "_" + tag.ToLower());
    }

    public void Move(Box b)
    {
        Game.boardMatrix[box.x, box.y] = null;
        box = b;
        transform.position = Game.Instance.GetBoxPos(box);
        Game.boardMatrix[box.x, box.y] = this;
    }

    public bool CanMove()
    {
        switch (tag)
        {
            case "King":
                return IsBoxsAvailable(new int[] { box.x + 1, box.x - 1, box.x, box.x, box.x + 1, box.x - 1, box.x - 1, box.x + 1 },
                    new int[] { box.y, box.y, box.y + 1, box.y - 1, box.y + 1, box.y - 1, box.y + 1, box.y - 1 });

            case "Queen":
                return IsBoxsAvailable(new int[] { box.x + 1, box.x - 1, box.x, box.x, box.x + 1, box.x - 1, box.x - 1, box.x + 1 },
                    new int[] { box.y, box.y, box.y + 1, box.y - 1, box.y + 1, box.y - 1, box.y + 1, box.y - 1 });

            case "Rook":
                return IsBoxsAvailable(new int[] { box.x + 1, box.x - 1, box.x, box.x }, new int[] { box.y, box.y, box.y + 1, box.y - 1 });

            case "Bishop":
                return IsBoxsAvailable(new int[] { box.x + 1, box.x - 1, box.x - 1, box.x + 1 }, new int[] { box.y + 1, box.y - 1, box.y + 1, box.y - 1 });

            case "Knight":
                return IsBoxsAvailable(new int[] {box.x + 2, box.x + 2, box.x + 1, box.x + 1, box.x - 1, box.x - 1, box.y - 2, box.y - 2},
                    new int[] { box.y - 1, box.y + 1, box.y - 2, box.y + 2, box.y - 2, box.y + 2, box.y - 1, box.y + 1});

            case "Pawn":
                int t = player == Game.whitePlayer ? 1 : -1;

                if (Game.boardMatrix[box.x, box.y + t] == null ||
                        box.x - 1 >= 0 && Game.boardMatrix[box.x - 1, box.y + t] != null && Game.boardMatrix[box.x - 1, box.y + t].player != player ||
                        box.x + 1 < 8 && Game.boardMatrix[box.x + 1, box.y + t] != null && Game.boardMatrix[box.x + 1, box.y + t].player != player)
                {
                    return true;
                }
                break;
        }

        return false;
    }

    public bool IsBoxsAvailable(int[] a, int[] b)
    {
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] >= 0 && a[i] < 8 && b[i] >= 0 && b[i] < 8 && (Game.boardMatrix[a[i], b[i]] == null || Game.boardMatrix[a[i], b[i]].player != player))
            {
                if(tag != Game.tags[0] || (new Box(a[i], b[i])).PiecesAttackOnBox().Count == 0)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
