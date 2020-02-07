using UnityEngine;
using System.Collections;

/// <summary>
///This class is used when passing additional information to the TimedGameObjects.
/// </summary>
public class BaseTimedClass {
    /// <summary>
    ///The TimeController group ID.
    /// </summary>
    public int GroupID;

    /// <summary>
    ///The New speed as a percentage.
    /// </summary>
    public float NewSpeed;

    /// <summary>
    ///Constructor which sets the Group ID and new speed as a percentage.
    /// </summary>
    public BaseTimedClass(int _GroupID, float _NewSpeed) {
        GroupID = _GroupID;
        NewSpeed = _NewSpeed;
    }
}