using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class StepColliderData {
    public uint id;        //step类的Id
    public uint node = 0;  //物体节点位置
    public uint type = 0;  //包围盒类型
    public uint r = 1;     //半径(一个单位)
}
public class TreeNode {
    public uint nodeId;
    public int deep = -1;
    public List<ushort> objects;
    public int objindex;
    public Vector4 rect;
    public TreeNode[] childNodes;
    public TreeNode(Vector4 rect, int deep){
        this.rect = rect;
        this.deep = deep;
        //直接生成满四叉树
        //大于最大深度结束
        if(deep >= 5) {
            return;
        }
        float width = rect.z / 2;
        float height = rect.w / 2;
        Push(
               new TreeNode(new Vector4(rect.x - width,rect.y - height,width,height),deep + 1),
               new TreeNode(new Vector4(rect.x + width,rect.y - height,width,height),deep + 1),
               new TreeNode(new Vector4(rect.x - width,rect.y + height,width,height),deep + 1),
               new TreeNode(new Vector4(rect.x + width,rect.y + height,width,height),deep + 1)
           );
    }
    //插入节点
    public void InsertObj(Vector4 objrect,ushort objNum,TreeNode father) {
        int checkedId = QuadTree.Check(ref objrect,ref rect);
        //递归结束
        if(checkedId == -1 || deep >= 5) {
            if(father.objects == null)
                father.objects = new List<ushort>();
            if(!father.objects.Contains(objNum))
                father.objects.Add(objNum);
            if(StepQTreeManager.TreeNodesByObjId.ContainsKey(objNum)) {
                StepQTreeManager.TreeNodesByObjId[objNum] = father;
            }
            else {
                StepQTreeManager.TreeNodesByObjId.Add(objNum,father);
            }
            return;
        }
        Vector4 newRect = Vector4.zero;
        if(checkedId == 1) {
            newRect = new Vector4(rect.x,rect.y, rect.z / 2,rect.w / 2);
        }
        if(checkedId == 2) {
            newRect = new Vector4(rect.z / 2,rect.y,rect.z / 2,rect.w / 2);
        }
        if(checkedId == 3) {
            newRect = new Vector4(rect.x,rect.w / 2,rect.z / 2,rect.w / 2);
        }
        if(checkedId == 4) {
            newRect = new Vector4(rect.z / 2,rect.w / 2,rect.z / 2,rect.w / 2);
        }
        //递归
        childNodes[checkedId - 1].InsertObj(objrect,objNum, this);
    }
    public void Push(TreeNode one, TreeNode two, TreeNode three, TreeNode four) {
        childNodes = new TreeNode[4];
        childNodes[0] = one;
        childNodes[1] = two;
        childNodes[2] = three;
        childNodes[3] = four;
    }
}
//四叉树
public class QuadTree {
    public const int MaxObjectLen = 10; //每个节点能放多少个object
    public const int MaxDeep = 5;       //数的最大深度
    public TreeNode Foot;               //根节点
    private int deep = -1;              //当前节点的深度
    public static bool OnTop;
    public static bool OnBottom;
    public static bool OnLeft;
    public static bool OnRight;
    public Transform[] AllTrans;
    //平方
    public static float Pow(float num) {
        return num * num;
    }
    //只生成根节点
    public QuadTree() {
        Foot = new TreeNode(new Vector4(0,0,Screen.width / 2,Screen.height / 2),0);
        Foot.deep = 0;
    }
    //obj根据位置加入到四叉树中
    public void InsertByNum(ushort objNum) {
        Vector4 rect = new Vector4(AllTrans[objNum].localPosition.x,AllTrans[objNum].localPosition.y,0.4f,0.4f);
        Foot.InsertObj(rect, objNum, Foot);
    }
    //检测该碰撞物体在哪个象限 -1 表示属于二个或两个以上 1 下左 2 下右 3 上左 4 上右
    public static int Check(ref Vector4 objrect, ref Vector4 rect) {
        //确定物体圆心是否在分割线上,在的话肯定属于两个或两个以上
        if(objrect.x == rect.x || objrect.y == rect.y) {
            //Debug.Log("中心心在分割线上");
            return -1;
        }

        //如果包围盒半径大于象限半径
        if(objrect.z >= rect.z || objrect.w >= rect.w) {
            //Debug.Log("物体半径大于象限半径");
            return -1;
        }
        //确定物体中心在哪个象限
        int index = 0;
        if(objrect.x < rect.x) {
            //左
            index += 2;
        }
        else {
            //右
            index -= 2;
        }
        if(objrect.y > rect.y) {
            //上
            index += 1;
        }
        else {
            //下
            index -= 1;
        }
        //平方比较距离(性能)
        float PowWidth = Pow(rect.z);
        float PowHeight = Pow(rect.w); 
        
        //下左
        if(index == 1) {
            if(Pow(rect.x - objrect.x) >= PowWidth) {
                return -1;
            }
            if(Pow(rect.y - objrect.y) >= PowHeight) {
                return -1;
            }
            return 1;
        }
        //Debug.Log("下右");
        if(index == -3) {
            if(Pow(objrect.x - rect.x) >= PowWidth) {  
                return -1;
            }
            if(Pow(rect.y - objrect.y) >= PowHeight) {
                return -1;
            }
            return 2;
        }
        //上左
        if(index == 3) {
            if(Pow(rect.x - objrect.x) >= PowWidth) {
                return -1;
            }
            if(Pow(objrect.y - rect.y) >= PowHeight) {
                return -1;
            }
            return 3;
        }
        //上右
        if(index == -1) {
            if(Pow(objrect.x - rect.x) >= PowWidth) {
                return -1;
            }
            if(Pow(objrect.y - rect.y) >= PowHeight) {
                return -1;
            }
            return 4;
        }
        return -1;
    }
}
public class StepQTreeManager : MonoBehaviour{
    public List<StepColliderData> AllColliders = new List<StepColliderData>();
    public QuadTree Tree;  //四叉树
    public static Dictionary<ushort,TreeNode> TreeNodesByObjId = new Dictionary<ushort,TreeNode>();
    void Start() {
        Init();
    }

    public void Init() {
       //初始化四叉树
       Tree = new QuadTree();
       Tree.AllTrans = new Transform[transform.childCount];
       for(int i = 0;i < transform.childCount;i++) {
           Tree.AllTrans[i] = transform.GetChild(i);
           Tree.AllTrans[i].transform.localPosition = Tree.AllTrans[i].localPosition + 2 * (new Vector3(i % 8,-Mathf.CeilToInt(i / 8),0));
       }
       AddQuadTreeByRect();
    }

    //包围盒根据位置跟半径加入四叉树
    public void AddQuadTreeByRect() {
        for(ushort i = 0;i < Tree.AllTrans.Length;i++) {
             Tree.InsertByNum(i);
        }
    }

    public void AddAndSetColliers(List<StepColliderData> allColliders) {
        AllColliders = allColliders;
        for(int i = 0; i < AllColliders.Count; i++) {
            
        }
    }

    public List<TriggerData> OnTrigger = new List<TriggerData>();
    public List<TriggerData> CacheTrigger = new List<TriggerData>();
    public List<uint> CacheChecked = new List<uint>();
    public struct TriggerData {
        public ushort trigger;
        public ushort other;
        public TriggerData(ushort trigger,ushort other) {
            this.trigger = trigger;
            this.other = other;
        }
    }
    //检测某碰撞提同一deep是否发生碰撞
    public void CheckTrigger(ushort objnum) {
        CacheChecked.Add(objnum);
        TriggerCheck(TreeNodesByObjId[objnum],objnum);
    }
    private void TriggerCheck(TreeNode treeNode, ushort objnum){
        if(treeNode.objects != null) {
            for(int i = 0;i < treeNode.objects.Count;i++) {

                if(treeNode.objects[i] == objnum) {
                    //自己不能碰撞自己
                    continue;
                }
                if(CacheChecked.Contains(treeNode.objects[i])) {
                    //已经做过碰撞
                    continue;
                }
                //if(Vector2.Distance(Tree.AllTrans[treeNode.objects[i]].localPosition,Tree.AllTrans[objnum].transform.localPosition) < 0.4f) {
                //    CacheTrigger.Add(new TriggerData(objnum,treeNode.objects[i]));
                //}
            }
        }
        
        if(treeNode.childNodes == null) {
            return;
        }
        for(int i = 0;i < 4;i++) {
            if(treeNode.childNodes[i] == null) {
                continue;
            }
            TriggerCheck(treeNode.childNodes[i], objnum);
        }
        
    }
    private List<ushort> CacheRemove = new List<ushort>();
    private void TriggerEvent() {
        CacheRemove.Clear();
        for(ushort i = 0; i < OnTrigger.Count;i++) {
            if(!CacheTrigger.Contains(OnTrigger[i])) {
                CacheRemove.Add(i);
                //Debug.Log("离开碰撞 trigger:" + OnTrigger[i].trigger + "other" + OnTrigger[i].other);
            }
        }
        for(ushort i = 0;i < CacheRemove.Count;i++) {
            OnTrigger.RemoveAt(CacheRemove[i]);
        }
        for(int i = 0;i < CacheTrigger.Count;i++) {
            if(OnTrigger.Contains(CacheTrigger[i])) {
                //Debug.Log("碰撞中 :" + Tree.AllTrans[CacheTrigger[i]]);
            }
            else {
                OnTrigger.Add(CacheTrigger[i]);
                //Debug.Log("开始碰撞 trigger:" + CacheTrigger[i].trigger  + "other"+ CacheTrigger[i].other);
            }
        }
    }
    void FixedUpdate() {
        //Tree.AllTrans[0].Translate(0.1f, 0, 0);
        Tree.AllTrans[6].Translate(-0.1f,0,0);
        //SomeOneMove(0);
        for(int i = 0;i < 200;i++) {
            SomeOneMove(6);
        }
        CheckTriggers();
    }
    //某个物体移动
    public void SomeOneMove(ushort objnum) {
        TreeNodesByObjId[objnum].objects.Remove(objnum);
        Tree.InsertByNum(objnum);
    }
    public void CheckTriggers() {
        CacheTrigger.Clear();
        CacheChecked.Clear();
        foreach(var key in TreeNodesByObjId.Keys) {
            CheckTrigger(key);
        }
        TriggerEvent();
    }
    public void CheckTrigger(TreeNode node) {
        if(node == null)
            return;

        Queue<TreeNode> queue = new Queue<TreeNode>();
        queue.Enqueue(node);
        int index = 0;
        while(queue.Count > 0) {
            var item = queue.Dequeue();

            if(item.childNodes == null)
                continue;
            for(int i = 0; i < 4; i++) {
                queue.Enqueue(item.childNodes[i]);
            }
        }
    }
}
