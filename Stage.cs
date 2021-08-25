using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Stage : MonoBehaviour
{
    [Header("Source")] // 변수들을 항목으로 묶어 관리
    public GameObject tilePrefab;
    public Transform backgroundNode;
    public Transform boardNode;
    public Transform tetrominoNode;
    public Transform previewNode;

    public GameObject gameoverPanel;
    public GameObject scoreO;
    public GameObject levelO;
    public GameObject lineO;
    public Text score;
    public Text level;
    public Text line;//레벨업까지 남은 줄 개수
    

    private int scoreVal = 0;
    private int levelVal = 1;
    private int lineVal;
    private int indexVal = -1;


    [Header("Setting")]
    [Range(4, 40)] // 사용자에게 입력받을 값의 범위를 정해주어 오류를 방지
    public int boardWidth = 10;
    [Range(5, 20)]
    public int boardHeight = 20;
    public float fallCycle = 1.0f;
    public float offset_x = 0f;
    public float offset_y = 0f;

    // 타일 생성 메서드
    Tile CreateTile(Transform parent, Vector2 position, Color color, int order = 1)
    {
        var go = Instantiate(tilePrefab);//tileprefab을 복제한 오브젝트 생성
        go.transform.parent = parent;
        go.transform.localPosition = position;
        var tile = go.GetComponent<Tile>();//tileprefab의 tile컴포넌트 불러오기
        tile.Color = color;
        tile.sortingOrder = order;
        return tile;
    }
    //tileprefab을 복제하고 부모와 위치를 정해준다.
    //복제된 prefab의 tile컴포넌트를 불러와 색깔과 정렬순서를 정해준다.
    //tile객체를 반환한다.

    private int halfWidth;
    private int halfHeight;
    private float nextFallTime;//다음에 tetromino가 떨어질 시간을 저장
    private void Start()
    {
        lineVal = levelVal * 2;
        score.text = "Score : " + scoreVal;
        level.text = "Level : " + levelVal;
        line.text = lineVal.ToString() + " lines need to be crashed for Level up";

        halfWidth = Mathf.RoundToInt(boardWidth * 0.5f);//주어진 float을 정수형으로 반올림하여 반환
        halfHeight = Mathf.RoundToInt(boardHeight * 0.5f);
        nextFallTime = Time.time + fallCycle;//다음에 테트로미노가 떨어질 시간 설정

        gameoverPanel.SetActive(false);
        levelO.SetActive(true);
        scoreO.SetActive(true);
        lineO.SetActive(true);
        CreateBackground();
        for (int i = 0; i < boardHeight; ++i)//높이만큼 행 노드 만들기
        {
            var col = new GameObject((boardHeight - i - 1).ToString());//0~19
            col.transform.position = new Vector3(0, halfHeight - i, 0);
            col.transform.parent = boardNode;
        }
        CreateTetromino();
    }
    private void Update()
    {
        if (gameoverPanel.activeSelf)
        {
            if (Input.GetKeyDown("r"))
            {
                SceneManager.LoadScene(0);
            }
        }
        else
        {
            Vector3 moveDir = Vector3.zero;//이동 여부
            bool isRotate = false;// 회전 여부
            if (Input.GetKeyDown(KeyCode.LeftArrow))// 왼, LEFTARROW
            {
                moveDir.x = -1;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))// 오, RIGHTARROW
            {
                moveDir.x = 1;
            }
            if (Input.GetKeyDown(KeyCode.LeftControl))// 90회전, CTRL
            {
                isRotate = true;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))// 아래, DOWNARROW
            {
                moveDir.y = -1;
            }
            if (Input.GetKeyDown(KeyCode.LeftAlt))// 아래로 쭉 내리기, Left_ALT
            {
                while (MoveTetromino(Vector3.down, false))
                {
                }
            }
            if (Input.GetKeyDown("r"))// 재시작 버튼, r
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            if (Time.time > nextFallTime)//선언 후 흐른 시간(Time.time)이 다음에 tetromino가 떨어질 시간보다 크다면
            {
                nextFallTime = Time.time + fallCycle;//다음에 떨어질 시간을 재설정 해주고
                moveDir.y = -1;//tetrominoNode를 아래로 한 칸 내린다.
                isRotate = false;//시간에 의해 아래로 이동하는 경우는 회전시키지 않는다.
            }// 키 입력 없이 자동으로 떨어지게 하기
            if (moveDir != Vector3.zero || isRotate)
            {
                MoveTetromino(moveDir, isRotate);
            }// 제시된 키 값을 반영
             // 방향키나 ALT를 누르지 않은 경우 아무것도 실행되지 않음
        }


    }
    bool MoveTetromino(Vector3 moveDir, bool isRotate)
    {
        // 회전 & 이동 불가시 다시 돌아가기 위한 값을 저장
        Vector3 oldPos = tetrominoNode.transform.position;
        Quaternion oldRot = tetrominoNode.transform.rotation;
        tetrominoNode.transform.position += moveDir;
        if (isRotate)
        {
            tetrominoNode.transform.rotation *= Quaternion.Euler(0, 0, 90);
        }
        if (!CanMoveTo(tetrominoNode))
        {
            tetrominoNode.transform.position = oldPos;
            tetrominoNode.transform.rotation = oldRot;
            if ((int)moveDir.y == -1 && (int)moveDir.x == 0 && isRotate == false)//이동이 불가하고 현재 아래로 떨어지고 있는 상황(바닥에 닿은 경우)
            {
                AddToBoard(tetrominoNode);
                CheckBoardColumn();
                CreateTetromino();
                if (!CanMoveTo(tetrominoNode))
                {
                    gameoverPanel.SetActive(true);
                }
            }
            return false;
        }
        return true;
    }// 이동을 실질적으로 진행시키고 이동이 가능한지 체크 후 bool값을 리턴, 이동이 불가능하면 원래의 위치와 회전값으로 되돌림
    bool CanMoveTo(Transform root) // root = tetrominoNode
    {
        for (int i = 0; i < root.childCount; ++i)//노드의 자식 타일을 모두 검사
        {
            var node = root.GetChild(i);
            //유니티 좌표계에서 테트리스 좌표계로 변환
            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);
            if (x < 0 || x > boardWidth - 1)
            {
                return false;
            }
            if (y < 0)
            {
                return false;
            }
            var column = boardNode.Find(y.ToString());
            if (column != null && column.Find(x.ToString()) != null)// 조건 추가. 다음 행 노드에(해당 테트로미노가 떨어진 위치에) BoardNode에 추가된 타일이 있는지 검사
            {
                return false;
            }
        }
        return true;
    }// 이동이 가능한지 체크 후 true or false를 반환하는 메서드
    void CreateBackground()//배경 생성 메서드, Start에서 호출
    {
        //기본 배경
        Color color = Color.gray;
        color.a = 0.5f;//회색이면서 투명도 0.5인 color 필드 선언
        for (int x = -halfWidth; x < halfWidth; ++x)//(-5~4)
        {
            for (int y = halfHeight; y > -halfHeight; --y)//(10~ -9)
            {
                CreateTile(backgroundNode, new Vector2(x, y), color, 0);
            }
        }
        //좌우 테두리
        color.a = 1.0f;
        for (int y = halfHeight; y > -halfHeight; --y)
        {
            CreateTile(backgroundNode, new Vector2(-halfWidth - 1, y), color, 0);
            CreateTile(backgroundNode, new Vector2(halfWidth, y), color, 0);
        }
        //아래 테두리
        for (int x = -halfWidth - 1; x <= halfWidth; ++x)
        {
            CreateTile(backgroundNode, new Vector2(x, -halfHeight), color, 0);
        }

    }
    void CreateTetromino()
    {
        int index;
        if (indexVal == -1)
        {
            index = Random.Range(0, 7);
        }
        else index = indexVal;

        Color32 color = Color.white;
        tetrominoNode.rotation = Quaternion.identity;
        tetrominoNode.position = new Vector2(offset_x, halfHeight + offset_y);
        switch (index)
        {
            case 0:// I, 하늘색
                color = new Color32(115, 251, 253, 255);
                CreateTile(tetrominoNode, new Vector2(-2f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                break;
            case 1:// J, 파란색
                color = new Color32(0, 33, 245, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color);
                break;
            case 2:// L, 주황색
                color = new Color32(243, 168, 59, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 1.0f), color);
                break;
            case 3:// O, 노란색
                color = new Color32(255, 253, 84, 255);
                CreateTile(tetrominoNode, new Vector2(0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 1.0f), color);
                break;
            case 4:// S, 녹색
                color = new Color32(117, 250, 76, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, -1f), color);
                CreateTile(tetrominoNode, new Vector2(0f, -1f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0f), color);
                break;
            case 5:// T, 자주색
                color = new Color32(155, 47, 246, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 1.0f), color);
                break;
            case 6:// Z, 빨간색
                color = new Color32(235, 51, 35, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 1f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 1f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0f), color);
                break;
        }// classify tetrominos by grid of tetrominoNode
        CreatePreview();// 테트로미노가 생성됨과 동시에 preview도 생성

    }
    void AddToBoard(Transform root)//테트로미노를 보드에 추가 root = tetrominoNode
    {
        //1. 테트로미노의 자녀 오브젝트들의 부모 노드를 Board 노드로 바군다.
        //2. 자녀 오브젝트들의 이름을 테트리스 좌표계로 변환한다.
        while (root.childCount > 0)
        {
            var node = root.GetChild(0);//node = 자식 타일
            //테트리스 좌표계로 변환
            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);
            node.parent = boardNode.Find(y.ToString());//자식 타일의 부모를 BoardNode의 행 노드로 설정(테트리스 좌표계를 기준으로)
            node.name = x.ToString();//각 행에서 테트리스 X좌표를 의미하도록 이름 설정
        }
    }
    void CheckBoardColumn()//보드에 완성된 행이 있으면 삭제
    {
        bool isCleared = false;
        int linecount = 0;//한번에 사라진 행 개수
        foreach (Transform column in boardNode)
        {
            if (column.childCount == boardWidth)
            {
                foreach (Transform tile in column)
                {
                    Destroy(tile.gameObject);
                }
                column.DetachChildren();//행의 모든 자식들과 연결 끊기
                isCleared = true;
                linecount++;
            }
        }
        if (linecount != 0)
        {
            scoreVal += linecount * linecount * 100;
            score.text = "Score : " + scoreVal;
        }
        if (linecount != 0)
        {
            lineVal -= linecount;
            if (lineVal <= 0 && levelVal <= 10)
            {
                levelVal += 1;
                lineVal = levelVal * 2 + lineVal;
                fallCycle = 0.1f * (11 - levelVal);
            }
            level.text = "Level : " + levelVal;
            line.text = lineVal.ToString() + " lines need to be crashed for Level up";
        }
        if (isCleared)// 지웠다면.
        {
            for (int i = 1; i < boardNode.childCount; ++i) //1행부터 조사. 아래에서 위로 검사해야 한다.
            {
                var column = boardNode.Find(i.ToString());
                if (column.childCount == 0)//이미 비어 있는 행은 무시
                {
                    continue;
                }
                // 현재 행 아래쪽에 빈 행이 존재하는지 확인, 빈 행만큼 emptyCol 증가
                int emptyCol = 0;
                int j = i - 1;
                while (j >= 0)
                {
                    if (boardNode.Find(j.ToString()).childCount == 0)
                    {
                        emptyCol++;
                    }
                    j--;
                }
                if (emptyCol > 0)
                {
                    var targetColumn = boardNode.Find((i - emptyCol).ToString());
                    while (column.childCount > 0)
                    {
                        Transform tile = column.GetChild(0);
                        tile.parent = targetColumn;
                        tile.transform.position += new Vector3(0, -emptyCol, 0);
                    }// targetColumn으로 조사된 행의 타일들 옮기기. 비어진 행의 개수만큼 위치 이동.
                    column.DetachChildren();//조사된 행의 자식들 전부 제거
                }
            }
        }
    }
    void CreatePreview()
    {
        foreach (Transform tile in previewNode)
        {
            Destroy(tile.gameObject);
        }
        previewNode.DetachChildren();
        indexVal = Random.Range(0, 7);
        Color32 color = Color.white;
        previewNode.position = new Vector2(halfWidth + 5, halfHeight - 3);
        switch (indexVal)
        {
            case 0: // I
                color = new Color32(115, 251, 253, 255);    // 하늘색
                CreateTile(previewNode, new Vector2(-2f, 0.0f), color);
                CreateTile(previewNode, new Vector2(-1f, 0.0f), color);
                CreateTile(previewNode, new Vector2(0f, 0.0f), color);
                CreateTile(previewNode, new Vector2(1f, 0.0f), color);
                break;

            case 1: // J
                color = new Color32(0, 33, 245, 255);    // 파란색
                CreateTile(previewNode, new Vector2(-1f, 0.0f), color);
                CreateTile(previewNode, new Vector2(0f, 0.0f), color);
                CreateTile(previewNode, new Vector2(1f, 0.0f), color);
                CreateTile(previewNode, new Vector2(-1f, 1.0f), color);
                break;

            case 2: // L
                color = new Color32(243, 168, 59, 255);    // 주황색
                CreateTile(previewNode, new Vector2(-1f, 0.0f), color);
                CreateTile(previewNode, new Vector2(0f, 0.0f), color);
                CreateTile(previewNode, new Vector2(1f, 0.0f), color);
                CreateTile(previewNode, new Vector2(1f, 1.0f), color);
                break;

            case 3: // O 
                color = new Color32(255, 253, 84, 255);    // 노란색
                CreateTile(previewNode, new Vector2(0f, 0f), color);
                CreateTile(previewNode, new Vector2(1f, 0f), color);
                CreateTile(previewNode, new Vector2(0f, 1f), color);
                CreateTile(previewNode, new Vector2(1f, 1f), color);
                break;

            case 4: //  S
                color = new Color32(117, 250, 76, 255);    // 녹색
                CreateTile(previewNode, new Vector2(-1f, -1f), color);
                CreateTile(previewNode, new Vector2(0f, -1f), color);
                CreateTile(previewNode, new Vector2(0f, 0f), color);
                CreateTile(previewNode, new Vector2(1f, 0f), color);
                break;

            case 5: //  T
                color = new Color32(155, 47, 246, 255);    // 자주색
                CreateTile(previewNode, new Vector2(-1f, 0f), color);
                CreateTile(previewNode, new Vector2(0f, 0f), color);
                CreateTile(previewNode, new Vector2(1f, 0f), color);
                CreateTile(previewNode, new Vector2(0f, 1f), color);
                break;

            case 6: // Z
                color = new Color32(235, 51, 35, 255);    // 빨간색
                CreateTile(previewNode, new Vector2(-1f, 1f), color);
                CreateTile(previewNode, new Vector2(0f, 1f), color);
                CreateTile(previewNode, new Vector2(0f, 0f), color);
                CreateTile(previewNode, new Vector2(1f, 0f), color);
                break;
        }

    }
}
