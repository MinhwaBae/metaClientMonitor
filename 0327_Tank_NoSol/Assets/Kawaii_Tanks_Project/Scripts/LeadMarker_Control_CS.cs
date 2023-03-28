using UnityEngine;
using UnityEngine.UI;


namespace ChobiAssets.KTP
{

    [DefaultExecutionOrder(+2)] // (Note.) This script is executed after the main camera is moved, in order to move the marker smoothly.
    public class LeadMarker_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to "Lead_Marker" in the scene.
		 * The appearance and position of the marker are controlled by this script.
		 * This script works in combination with the "Aiming_Control_CS" and "Fire_Spawn_CS" in the tank.
		*/

        // User options >>
        public Sprite rightSprite;
        public Sprite wrongSprite;
        public float calculationTime = 2.0f;
        // << User options


        Transform thisTransform;
        Image thisImage;
        Aiming_Control_CS aimingScript;
        Transform firePointTransform;
        float bulletSpeed = 250.0f;


        [HideInInspector] public static LeadMarker_Control_CS instance;


        void Awake()
        {
            instance = this;
        }


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisTransform = transform;
            thisImage = GetComponent<Image>();
        }


        public void Get_Aiming_Control_Script(Aiming_Control_CS aimingScript)
        { // Called from "Aiming_Control_CS" in the player's tank.
            this.aimingScript = aimingScript;
        }


        public void Get_Fire_Spwan_Script(Fire_Spawn_CS fireSpawnScript)
        { // Called from "Fire_Spawn_CS" in the player's tank.
            firePointTransform = fireSpawnScript.transform;
            bulletSpeed = fireSpawnScript.bulletVelocity;
        }


        void LateUpdate()
        {
            Marker_Control();
        }


        void Marker_Control()
        {
            if (aimingScript == null || firePointTransform == null)
            { // The tank has been destroyed.
                thisImage.enabled = false;
                return;
            }

            // Check the aiming mode.
            switch (aimingScript.mode)
            {
                case 0: // Keep the initial positon.
                    thisImage.enabled = false;
                    return;
            }

            // Check the target is locked on now.
            if (aimingScript.targetTransform == null)
            { // The target is not locked on.
                thisImage.enabled = false;
                return;
            }

            // Calculate the ballistic.
            var muzzlePos = firePointTransform.position;
            var targetDir = aimingScript.targetPosition - muzzlePos;
            var targetBase = Vector2.Distance(Vector2.zero, new Vector2(targetDir.x, targetDir.z));
            var bulletVelocity = firePointTransform.forward * bulletSpeed;
            if (aimingScript.targetRigidbody)
            { // The target has a rigidbody.
                // Reduce the target's velocity to help the lead-shooting.
                bulletVelocity -= aimingScript.targetRigidbody.velocity;
            }
            var isHit = false;
            var isTank = false;
            var previousPos = muzzlePos;
            var currentPos = previousPos;
            var count = 0.0f;
            while (count < calculationTime)
            {
                // Get the current position.
                var virtualPos = bulletVelocity * count;
                virtualPos.y -= 0.5f * -Physics.gravity.y * Mathf.Pow(count, 2.0f);
                currentPos = virtualPos + muzzlePos;

                // Get the hit point by casting a ray.
                if (Physics.Linecast(previousPos, currentPos, out RaycastHit raycastHit, Layer_Settings_CS.Aiming_Layer_Mask))
                {
                    currentPos = raycastHit.point;
                    isHit = true;
                    if (raycastHit.rigidbody && raycastHit.transform.root.tag != "Finish")
                    { // The target has a rigidbody, and it is living.
                        isTank = true;
                    }
                    break;
                }

                // Check the ray has exceeded the target.
                var currenBase = Vector2.Distance(Vector2.zero, new Vector2(virtualPos.x, virtualPos.z));
                if (currenBase > targetBase)
                {
                    break;
                }

                previousPos = currentPos;
                count += Time.fixedDeltaTime;
            }

            // Convert the hit point to the screen point.
            var screenPos = Camera.main.WorldToScreenPoint(currentPos);
            if (screenPos.z < 0.0f)
            { // The hit point is behind the camera.
                thisImage.enabled = false;
                return;
            }

            // Set the position.
            thisImage.enabled = true;
            screenPos.z = 128.0f;
            thisTransform.position = screenPos;

            // Set the appearance.
            if (isHit)
            { // The bullet will hit something.
                if (isTank)
                { // The hit object has a rigidbody.
                    thisImage.sprite = rightSprite;
                }
                else
                { // The hit object has no rigidbody.
                    thisImage.sprite = wrongSprite;
                }
            }
            else
            { // The bullet will not hit anything.
                thisImage.sprite = wrongSprite;
            }
        }
        
    }

}
