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
        // 列(縦の値y)
        var columCount = -1;
        // 行(横の値x)
        var rowCount = 0;

        // 操作した後のListを作成する
        List<BallController> controlledList = new List<BallController>();
        controlledList.Clear();

        // まず行のグループを作り、6個入り5行のまとまりを作成する
        var ballChunks = ballControllers.OrderByDescending(y => Mathf.Round(y.CurrentPos.y))
            .GroupBy(g => Mathf.Round(g.CurrentPos.y));

        foreach (var group in ballChunks)
        {
            // 列を6個の中身で昇順で並び替える
            var rowGroup = group.OrderBy(x => Mathf.Round(x.CurrentPos.x));
            rowCount = -1;
            columCount++;
            foreach (var ball in rowGroup)
            {
                rowCount++;
                ball.BoardPos = new Vector2(rowCount, columCount);
                // 操作後のListにBoardPosを定義したball追加していく
                controlledList.Add(ball);
            }
        }

        ballControllers = controlledList;
        // 判定をしていく
        var pos = 0;
        foreach (var piece in ballControllers)
        {
            if (IsMatchBall(piece))
            {
                pos++;
            }
        }
        return 0 < pos;
    }

    private bool IsMatchBall(BallController ball)
    {
        // ballの情報を取得
        var pos = ball.BoardPos;
        var type = ball.ThisBallType;

        // 縦方向にマッチするかの判定 MEMO: 自分自身をカウントするため +1 する
        var verticalMatchCount = GetSameTypePieceNum(type, pos, Vector2.up) + GetSameTypePieceNum(type, pos, Vector2.down) + 1;

        // 横方向にマッチするかの判定 MEMO: 自分自身をカウントするため +1 する
        var horizontalMatchCount = GetSameTypePieceNum(type, pos, Vector2.right) + GetSameTypePieceNum(type, pos, Vector2.left) + 1;
        if (2 < horizontalMatchCount || 2 < verticalMatchCount)
        {
            ball.DestroyFlag = true;
        }
        return 2 < horizontalMatchCount || 2 < verticalMatchCount;
    }

    // 対象の方向に引数で指定した種類がいくつあるかを返す
    private int GetSameTypePieceNum(BallController.BallType ballType, Vector2 pos, Vector2 searchDir)
    {
        var count = 0;
        while (true)
        {
            pos += searchDir;
            if (IsInBoard(pos) && ballControllers.Find(x => x.BoardPos.x == pos.x && x.BoardPos.y == pos.y).ThisBallType == ballType)
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
    private bool IsInBoard(Vector2 boardPos)
    {
        return boardPos.x >= 0 && boardPos.y >= 0 && boardPos.x < 6 && boardPos.y < 5;
    }


    private bool DropBalls()
    {
        var destroyBollCount = 0;
        foreach (var destroyBall in ballControllers)
        {
            // まず削除対象のBallを取得する
            if (destroyBall.DestroyFlag)
            {
                // DestroyBallの上にあるBallのリストを作成していく
                var destoyBallUpList = new List<BallController>();
                destoyBallUpList.Clear();
                foreach (var upBall in ballControllers)
                {
                    // x軸が同じで、yが自分より小さい(4が一番下、0が一番上なので)、DestroyFlagを立てていないBallをまとめる
                    if (upBall.BoardPos.x == destroyBall.BoardPos.x && !upBall.DestroyFlag
                        && destroyBall.BoardPos.y > upBall.BoardPos.y)
                    {
                        destoyBallUpList.Add(upBall);
                    }
                }

                // 作成したリストの要素のpositionを下に移動させる
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