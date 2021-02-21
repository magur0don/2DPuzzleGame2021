using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class BallsManager : MonoBehaviour
{
    public List<BallController> ballControllers = new List<BallController>();
    public BallGenerator ballGenerator = null;

    public enum GameState
    {
        Invalide,
        Init,
        Start,
        BallControll,
        Match,
        Result
    }

    public GameState gameState = GameState.Init;

    public BallController ControllBall = null;

    float BottomRange = 0f;

    float waitTime = 2f;

    bool stateChange = true;

    private void Update()
    {
        switch (gameState)
        {
            case GameState.Invalide:

                break;

            case GameState.Init:
                ballGenerator.BallsGenerate();

                ballGenerator.GetComponent<GridLayoutGroup>().enabled = true;
                ballControllers.Clear();
                foreach (var ballCtrl in ballGenerator.Balls)
                {
                    ballControllers.Add(ballCtrl.GetComponent<BallController>());
                }

                gameState = GameState.Start;
                break;
            case GameState.Start:
                ControllBall = null;

                // BottomRangeがとれないのでここで取得
                if (BottomRange == 0f)
                {
                    BottomRange = (ballControllers[0].CurrentPos.y - ballControllers[6].CurrentPos.y);
                }

                foreach (var ballCtrl in ballControllers)
                {
                    if (ControllBall == null && ballCtrl.IsTouch)
                    {
                        ControllBall = ballCtrl;
                        gameState = GameState.BallControll;
                    }
                }
                break;
            case GameState.BallControll:
                ballGenerator.GetComponent<GridLayoutGroup>().enabled = false;
                if (!ControllBall.IsTouch)
                {
                    gameState = GameState.Match;
                }
                break;
            case GameState.Match:
                CheckMatch();
                foreach (var ball in ballControllers)
                {
                    if (ball.DestroyFlag)
                    {
                        ball.gameObject.SetActive(false);
                    }
                }
                stateChange = true;
                waitTime -= Time.deltaTime;
                if (waitTime < 0)
                {
                    waitTime = 2f;
                    gameState = GameState.Result;
                }
                break;
            case GameState.Result:
                if (stateChange)
                {
                    if (DropBalls())
                    {
                        StartCoroutine(wait());


                    }
                    else
                    {
                        gameState = GameState.Start;
                    }
                    stateChange = false;
                }

                break;
        }
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(1f);
        ReGenerate();
        yield return new WaitForSeconds(0.5f);
        gameState = GameState.Match;
    }

    public void ReGenerate()
    {
        // 移動し終わった後、消えたボールを補充
        foreach (var destroyBall in ballControllers)
        {
            if (!destroyBall.gameObject.activeSelf)
            {
                destroyBall.SetRandomType();
                destroyBall.gameObject.SetActive(true);
                destroyBall.DestroyFlag = false;
            }
        }
    }

    public bool CheckMatch()
    {
        var rowCount = -1;
        var columCount = 0;

        List<BallController> controlledList = new List<BallController>();
        // まずy軸のグループを作り、6個入り5列作成する
        var chunks = ballControllers.OrderByDescending(y => Mathf.Round(y.CurrentPos.y))
            .GroupBy(g => Mathf.Round(g.CurrentPos.y));

        foreach (var group in chunks)
        {
            // X軸を6個の中身で昇順で並び替える
            var xgroup = group.OrderBy(x => Mathf.Round(x.CurrentPos.x));
            columCount = -1;
            rowCount++;
            foreach (var item in xgroup)
            {
                columCount++;
                item.BoardPos = new Vector2(columCount, rowCount);
                controlledList.Add(item);
            }
        }
        ballControllers = controlledList;
        // 判定をしていく
        var pos = 0;

        foreach (var piece in ballControllers)
        {
            if (IsMatchPiece(piece))
            {
                pos++;
            }
        }
        return 0 < pos;
    }

    private bool IsMatchPiece(BallController piece)
    {
        // ピースの情報を取得
        var pos = piece.BoardPos;
        var kind = piece.ThisBallType;

        // 縦方向にマッチするかの判定 MEMO: 自分自身をカウントするため +1 する
        var verticalMatchCount = GetSameKindPieceNum(kind, pos, Vector2.up) + GetSameKindPieceNum(kind, pos, Vector2.down) + 1;

        // 横方向にマッチするかの判定 MEMO: 自分自身をカウントするため +1 する
        var horizontalMatchCount = GetSameKindPieceNum(kind, pos, Vector2.right) + GetSameKindPieceNum(kind, pos, Vector2.left) + 1;
        if (2 < horizontalMatchCount || 2 < verticalMatchCount)
        {
            piece.DestroyFlag = true;
        }
        return 2 < horizontalMatchCount || 2 < verticalMatchCount;
    }

    // 対象の方向に引数で指定した種類のピースがいくつあるかを返す
    private int GetSameKindPieceNum(BallController.BallType kind, Vector2 piecePos, Vector2 searchDir)
    {
        var count = 0;
        while (true)
        {
            piecePos += searchDir;
            if (IsInBoard(piecePos) && ballControllers.Find(x => x.BoardPos.x == piecePos.x && x.BoardPos.y == piecePos.y).ThisBallType == kind)
            {
                count++;
            }
            else
            {
                break;
            }
        }
        return count;
    }

    // 対象の座標がボードに存在するか(ボードからはみ出していないか)を判定する
    private bool IsInBoard(Vector2 pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < 6 && pos.y < 5;
    }


    private bool DropBalls()
    {
        var destroyBollCount = 0;
        foreach (var destroyBall in ballControllers)
        {
            if (destroyBall.DestroyFlag)
            {
                // 上にあるボールたちを抽出する
                var destoyBallUpList = ballControllers.Where(b => b.BoardPos.x == destroyBall.BoardPos.x &&
                !b.DestroyFlag && destroyBall.BoardPos.y > b.BoardPos.y);
                foreach (var downBall in destoyBallUpList)
                {
                    var downPos = new Vector2(downBall.CurrentPos.x, downBall.CurrentPos.y -= BottomRange);
                    downBall.SetPos(downPos);
                    // 消えたボールを上に持ってくる
                    var upPos = new Vector2(destroyBall.CurrentPos.x, destroyBall.CurrentPos.y += BottomRange);
                    destroyBall.SetPos(upPos);
                }
                destroyBollCount++;
            }
        }
        return destroyBollCount > 0;
    }
}