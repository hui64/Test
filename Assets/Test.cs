using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Test : YieldTest {
	// Use this for initialization
	void Start () {
        //StartCoroutine
       StartContinue("move");
	}
    void FixedUpdate() {
        ContinueLoop();
    }
    IEnumerator move() {
        float time = Time.time;
        yield return new WaitForSeconds(1.0f);
        print(Time.time - time);
    }
}
public class YieldTest : MonoBehaviour{
    private List<IEnumerator> Coroutines = new List<IEnumerator>();
    private List<int> removeCache = new List<int>();
    private Dictionary<string,IEnumerator> NameCoroutines = new Dictionary<string,IEnumerator>();
    private List<string> NameRemoveCache = new List<string>();
    public void StartContinue(IEnumerator ie) {
        Debug.Log("StartContinue");
        Coroutines.Add(ie);
    }
    public void StartContinue(string name) {
        System.Reflection.MethodInfo fun = this.GetType().GetMethod(name,System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        NameCoroutines.Add(name, fun.Invoke(this, null) as IEnumerator);
    }
    public void StopAllContinue() {
        Coroutines.Clear();
        NameCoroutines.Clear();
        removeCache.Clear();
        NameRemoveCache.Clear();
    }
    public void StopContinue(string name) {
        if(NameCoroutines.ContainsKey(name)) {
            NameCoroutines.Remove(name);
            if(NameRemoveCache.Contains(name))
                NameRemoveCache.Remove(name);
        }
    }
    public void ContinueLoop() {
        //==name
        var keys = NameCoroutines.Keys;
        foreach(string key in keys) {
            if(NameCoroutines[key].Current == null) {
                NameCoroutines[key].MoveNext();
            }
            if(NameCoroutines[key].Current is YieldInstruction)
                if(!(NameCoroutines[key].Current as YieldInstruction).IsDone())
                    continue;
            if(!NameCoroutines[key].MoveNext()) {
                NameRemoveCache.Add(key);
            }
        }
        for(int i = 0;i < NameRemoveCache.Count;i++) {
            NameCoroutines.Remove(NameRemoveCache[i]);
        }
        NameRemoveCache.Clear();
        //==fun
        for(int i = 0;i < Coroutines.Count;i++) {
            if(Coroutines[i].Current == null) {
                Coroutines[i].MoveNext();
            }
            if(Coroutines[i].Current is YieldInstruction)
                if(!(Coroutines[i].Current as YieldInstruction).IsDone())
                    continue;
            if(!Coroutines[i].MoveNext()) {
                removeCache.Add(i);
            }
        }
        for(int i = 0;i < removeCache.Count;i++) {
            Coroutines.RemoveAt(removeCache[i]);
        }
        removeCache.Clear();
    }
}
public class YieldInstruction{
    public virtual bool IsDone(){
        return true;
    }
}
public class WaitForSeconds : YieldInstruction {
    public float stepdelTime = 0.02f;
    public float _wait;
    public float _start = 0;
    public WaitForSeconds(float time) {
        _wait = time;
    }
    public override bool IsDone() {
        _start += stepdelTime;
        return _start >= _wait;
    }
}
