using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents any object which has weight and can be grapped, moved,
/// affected by other objects
/// </summary>
public interface Weighted
{
    /// <summary>
    /// Provides access to this objects accumulative weight
    /// ie. base weight + any carried objects weight
    /// </summary>
    float GetWeight {
        get;
    }

    bool IsGrabbed {
        get;
    }

    /// <summary>
    /// Method for grabbing this object
    /// </summary>
    /// <param name="other">The other weighted object which is grabbing this 
    /// one</param>
    /// <returns>the Vector3 position of the grabbed object</returns>
    Vector3 Grab(Weighted other);

    /// <summary>
    /// Method for dropping a grabbed object
    /// </summary>
    void Drop();

    /// <summary>
    /// Provides a method to move this object from one
    /// position to another if being grabbed and carried
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="other">The other weight object which is moving this one</param>
    void MoveObject(Weighted other, Vector3 destination, Vector3 offset);
}
