// HEADER

// INCLUDES
using UnityEngine;

/// <summary>
/// Namespace for classes with general functionalities
/// </summary>
namespace Utils.Haptics
{
    /// <summary>
    /// Class which provides the forces of the obstacles
    /// </summary>
    public class Obstacles : MonoBehaviour
    {
        /// <summary>
        /// Calculates the force feedback of the object - re-defined depending on each object
        /// </summary>
        /// <param name="tipPosition">position of the needle</param>
        /// <param name="tipVelocity">speed of the needle</param>
        /// <returns>a 3D vector with the forces to exert to the haptic arm in response of object contact</returns>
        public virtual Vector3 CalculateForce(Vector3 tipPosition, Vector3 tipVelocity)
        {
            return Vector3.zero;
        }
    }
}