using UnityEngine;
using System.Collections;

public class StepTransform {
    public Transform _transform;
    public StepTransform(Transform _tran) {
        _transform = _tran;
    }
    //========================================= StepVector3 =====================================
    private StepVector3 _position;
    private StepVector3 _localScan;
    private StepVector3 _localPosition;
    public Vector3 forward {
        get {
            return _transform.forward;
        }
        set {
            _transform.forward = value;
        }
    }
    public Vector3 right {
        get {
            return _transform.right;
        }
        set {
            _transform.right = value;
        }
    }
    /// <summary>
    /// 世界位置
    /// </summary>
    public StepVector3 position {
        get {
            return _position;
        }
        set {
            _position = value;
            _transform.position = _position.Vector3;
        }
    }
    /// <summary>
    /// 自身位置
    /// </summary>
    public StepVector3 localPosition {
        get {
            return _localPosition;
        }
        set {
            _localPosition = value;
            _transform.localPosition = _localPosition.Vector3;
        }
    }
}
