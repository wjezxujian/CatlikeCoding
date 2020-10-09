using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    Vector2Int boardSize = new Vector2Int(11, 11);

    [SerializeField]
    GameBoard board = default;

    private void Awake()
    {
        board.Initialize(boardSize);
    }

    private void OnValidate()
    {
        if(boardSize.x < 2)
        {
            boardSize.x = 2;
        }

        if(boardSize.y < 2)
        {
            boardSize.y = 2;
        }
    }

}
