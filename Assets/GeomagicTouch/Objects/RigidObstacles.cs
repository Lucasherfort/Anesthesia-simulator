// HEADER

// INCLUDES
using UnityEngine;

/// <summary>
/// Namespace for classes with general functionalities
/// </summary>
namespace Utils.Haptics
{
    /// <summary>
    /// Beahviours for rigid objects such as a table
    /// </summary>
    public class RigidObstacles : Obstacles
    {
        //---------------------------------------------------------------------------
        // CONSTANTS
        //---------------------------------------------------------------------------

        /// <summary>
        /// Scale for Unity
        /// </summary>
        private const float Scale = 100f;

        /// <summary>
        /// Position of the center of the object [mm]
        /// </summary>
        private Vector3 Position = new Vector3(0, -25f, 0);

        /// <summary>
        /// Boundary for the collision with the object
        /// </summary>
        private const float Boundary = 0.01f;

        /// <summary>
        /// The spring constant of the force applied when pushed from the rigid object surface [N/mm]
        /// </summary>
        private const float Stiffness = 1.0f;

        /// <summary>
        /// Damping factor of when you are in the object [Ns/mm]
        /// </summary>
        private const float Dumping = 0.0f;

        /// <summary>
        /// The maximum value of the generated force [N]
        /// </summary>
        private const float ForceLimit = 3.0f;

        //---------------------------------------------------------------------------
        // OBJECT ATTRIBUTES
        //---------------------------------------------------------------------------

        /// <summary>
        /// Stores the predefined object's material
        /// </summary>
        private Material OriginalMaterial = null;

        /// <summary>
        /// Stores the material to apply to the object each cycle
        /// </summary>
        private Material CurrentMaterial = null;

        /// <summary>
        /// User defined material force visual feedback of collision
        /// </summary>
        public Material OnCollisionMaterial = null;

        //---------------------------------------------------------------------------
        // FUNCTIONS
        //---------------------------------------------------------------------------

        /// <summary>
        /// Process when the script becomes enabled and active
        /// </summary>
        private void OnEnable()
        {

        }

        /// <summary>
        /// Process when starting script
        /// </summary>
        private void Start()
        {
            // Convert the position to device's world
            //Position = transform.position * Scale;

            // Store the initial material configuration
            OriginalMaterial = transform.GetComponent<Renderer>().material;

            // Initialise the material to apply in each cycle
            CurrentMaterial = transform.GetComponent<Renderer>().material;

            // Set a red coloured material in case none was defined
            if (OnCollisionMaterial == null)
            {
                var newMaterial = new Material(OriginalMaterial);
                newMaterial.color = Color.red;
                OnCollisionMaterial = newMaterial;
            }
        }

        /// <summary>
        /// Process each frame
        /// </summary>
        private void Update()
        {
            // Update material
            transform.GetComponent<Renderer>().material = CurrentMaterial;
        }

        /// <summary>
        /// Process when the script becomes disabled or inactive
        /// </summary>
        private void OnDisable()
        {

        }

        /// <summary>
        /// Function to re-set the material of the rigid obstacle
        /// </summary>
        public void reInit()
        {
            CurrentMaterial = OriginalMaterial;
        }

        /// <summary>
        /// Seek the forces generated when the operating point is in contact with the rigid object
        /// </summary>
        /// <param name="tipPosition">The position of the tip [mm]</param>
        /// <param name="tipVelocity">The speed of the tip [mm/s]</param>
        /// <returns>The force generated due to contact with the rigid object</returns>
        public override Vector3 CalculateForce(Vector3 tipPosition, Vector3 tipVelocity)
        {
            // Calculate the difference from the tip to the object center
            Vector3 difPosition = tipPosition - Position;

            // The distance to planar object is assumed in the Y axis
            float distance = difPosition.y;

            // No force in the outside of the boundary with object and no visual feedback
            if (distance >= Boundary)
            {
                CurrentMaterial = OriginalMaterial;
                return Vector3.zero;
            }

            // Set collision visual feedback
            CurrentMaterial = OnCollisionMaterial;

            // No lateral forces with planar object
            difPosition.x = difPosition.z = 0;

            // Normalisation
            difPosition /= distance;

            // Stiffness force calculation
            float force = Stiffness * (Boundary - distance);

            // Restrict to max force
            if (force > ForceLimit) force = ForceLimit;

            return (force * difPosition) - Dumping * tipVelocity;
        }
    }
}