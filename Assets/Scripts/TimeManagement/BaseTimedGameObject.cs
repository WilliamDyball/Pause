using UnityEngine;
using System.Collections;

public class BaseTimedGameObject : MonoBehaviour {
    [SerializeField] public TimeGroupController TimeGroupController;
    [SerializeField] public int ControllerGroupID = 1;
    [SerializeField] public bool SearchObjects = true;
    [SerializeField] public Object[] AssignedObjects;
    internal float fCurrentSpeedPercent = 100f;
    internal float fCurrentSpeedZeroToOne = 1f;
    protected float fPreviousSpeedPercentage = 100f;

    void Start() {
        if (TimeGroupController != null) {
            SetSpeed(TimeGroupController.ControllerSpeedPercent);
        }
    }

    protected float GetNewSpeedFromPercentage(float _fInSpeed) {
        float thisOriginalSpeed;

        if (fPreviousSpeedPercentage != 0f) {
            thisOriginalSpeed = (100f / fPreviousSpeedPercentage) * _fInSpeed;
        } else {
            thisOriginalSpeed = _fInSpeed;
        }

        return (thisOriginalSpeed / 100f) * fCurrentSpeedPercent;
    }

    protected float GetNewSpeedFromPercentage(float _fOriginalSpeed, float _fInSpeed, bool _bReverse) {
        if (_bReverse && fCurrentSpeedPercent != 0f) {
            return _fOriginalSpeed * (100f / fCurrentSpeedPercent);
        } else if (_bReverse) {
            return _fOriginalSpeed * (100f / 0.001f);
        } else {
            return (_fOriginalSpeed / 100f) * fCurrentSpeedPercent;
        }
    }

    protected virtual void ClearAssignedObjects() {
    }

    protected virtual void SetSpeedLooping(float _fNewSpeed, float _fCurrentSpeedPercent, float _fCurrentSpeedZeroToOne) {
    }

    protected virtual void SetSpeedAssigned(float _fNewSpeed, float _fCurrentSpeedPercent, float _fCurrentSpeedZeroToOne) {
    }

    void SetSpeed(float _fNewSpeed) {
        fCurrentSpeedPercent = Mathf.Clamp(_fNewSpeed, 0f, 1000f);
        fCurrentSpeedZeroToOne = (fCurrentSpeedPercent == 0f ? 0f : fCurrentSpeedPercent / 100f);
        if (SearchObjects) {
            SetSpeedAssigned(_fNewSpeed, fCurrentSpeedPercent, fCurrentSpeedZeroToOne);
        } else {
            SetSpeedLooping(_fNewSpeed, fCurrentSpeedPercent, fCurrentSpeedZeroToOne);
        }
        if (fCurrentSpeedPercent != 0) {
            fPreviousSpeedPercentage = fCurrentSpeedPercent;
        }
    }

    public float TimedDeltaTime() {
        if (fCurrentSpeedPercent != 0) {
            return Time.deltaTime / (100f / fCurrentSpeedPercent);
        } else {
            return 0f;
        }
    }

    void OnGroupPauseBroadcast(BaseTimedClass _swpTimedClass) {
        if (_swpTimedClass.GroupID != ControllerGroupID) {
            return;
        }
        SetSpeed(_swpTimedClass.NewSpeed);
    }

    void OnGroupSlowDownBroadcast(BaseTimedClass _swpTimedClass) {
        if (_swpTimedClass.GroupID != ControllerGroupID) {
            return;
        }

        SetSpeed(_swpTimedClass.NewSpeed);
    }

    void OnGroupSpeedUpBroadcast(BaseTimedClass _swpTimedClass) {
        if (_swpTimedClass.GroupID != ControllerGroupID) {
            return;
        }

        SetSpeed(_swpTimedClass.NewSpeed);
    }

    void OnGroupResumeBroadcast(BaseTimedClass _swpTimedClass) {
        if (_swpTimedClass.GroupID != ControllerGroupID) {
            return;
        }

        SetSpeed(_swpTimedClass.NewSpeed);
    }

    void OnGlobalPauseBroadcast() {
    }

    void OnGlobalResumeBroadcast() {
    }
}