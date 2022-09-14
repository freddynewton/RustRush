using UnityEngine;

namespace zone.nonon
{
    public class PlattformMover : MonoBehaviour
    {
        Vector3 origPositionBeforeMove;
        bool isColliding = false;

        public float maxMovingDistance = 3f;
        public float movingSpeed = 4.0f;
        public bool moveHorizontal = false;
        public bool moveVerticval = false;
        public bool inverseHorizontal = false;
        public bool inverseVertical = false;
        public bool startingPlattform = false;
        public bool finishPlattform = false;

        bool movingForward = false;
        float directionChangedTime;

        Vector3 daVelocity = Vector3.zero;
        Rigidbody rigidBody;

        // Start is called before the first frame update
        void Start()
        {
            origPositionBeforeMove = transform.position;
            GameEvents.Instance.onCollisionEntered += OnCollisionEntered;
            GameEvents.Instance.onCollisionExited += OnCollisionExited;
            directionChangedTime = Time.time;
            rigidBody = GetComponent<Rigidbody>();
        }

        private void OnDestroy()
        {
            GameEvents.Instance.onCollisionEntered -= OnCollisionEntered;
            GameEvents.Instance.onCollisionExited -= OnCollisionExited;
        }

        private void OnCollisionEntered(Transform source, Transform origin)
        {
            if (origin.Equals(transform))
            {
                isColliding = true;
            }
        }

        private void OnCollisionExited(Transform source, Transform origin)
        {
            if (origin.Equals(transform))
            {
                isColliding = false;
            }
        }

        void HandleVerticalMovement()
        {


            float distance = origPositionBeforeMove.y - transform.position.y;
            if (inverseVertical)
            {
                distance = transform.position.y - origPositionBeforeMove.y;
            }
            if (isColliding)
            {
                if (distance < maxMovingDistance)
                {
                    Vector3 newPos = new Vector3(transform.position.x, transform.position.y - maxMovingDistance, transform.position.z);
                    if (inverseVertical)
                    {
                        newPos = new Vector3(transform.position.x, transform.position.y + maxMovingDistance, transform.position.z);
                    }
                    daVelocity = (newPos - transform.position).normalized * movingSpeed;

                }
                else
                {
                    daVelocity = Vector3.zero;
                }

            }
            else
            {

                if (distance > 0)
                {
                    Vector3 newPos = new Vector3(transform.position.x, transform.position.y + maxMovingDistance, transform.position.z);
                    if (inverseVertical)
                    {
                        newPos = new Vector3(transform.position.x, transform.position.y - maxMovingDistance, transform.position.z);
                    }
                    daVelocity = (newPos - transform.position).normalized * movingSpeed;
                }
                else
                {
                    daVelocity = Vector3.zero;
                }
            }
        }

        void HandleHorizontalMovement(bool additive)
        {
            float distance = Vector3.Distance(transform.position, origPositionBeforeMove);

            // check if the distance is greater and if the direction was not changed recently
            if (distance > maxMovingDistance && !movingForward && Time.time - directionChangedTime > 0.5f)
            {
                movingForward = true;
                directionChangedTime = Time.time;
            }
            // check if the distance is greater and if the direction was not changed recently
            if (distance > maxMovingDistance && movingForward && Time.time - directionChangedTime > 0.5f)
            {
                movingForward = false;
                directionChangedTime = Time.time;
            }

            if (!movingForward)
            {
                float step = movingSpeed * Time.deltaTime;
                //Vector3 newPos = new Vector3(transform.position.x + maxMovingDistance, transform.position.y, transform.position.z);
                Vector3 newPos = transform.position + (transform.forward.normalized * maxMovingDistance);
                if (inverseHorizontal)
                {
                    newPos = transform.position + (transform.forward.normalized * -maxMovingDistance);
                    //newPos = new Vector3(transform.position.x - maxMovingDistance, transform.position.y, transform.position.z);
                }
                Debug.DrawLine(transform.position, newPos, Color.red);
                if (additive)
                {
                    daVelocity += (newPos - transform.position).normalized * movingSpeed;
                }
                else
                {
                    daVelocity = (newPos - transform.position).normalized * movingSpeed;
                }

            }


            if (movingForward)
            {
                float step = movingSpeed * Time.deltaTime;
                //Vector3 newPos = new Vector3(transform.position.x - maxMovingDistance, transform.position.y, transform.position.z);
                Vector3 newPos = transform.position + (transform.forward.normalized * -maxMovingDistance);
                if (inverseHorizontal)
                {
                    newPos = transform.position + (transform.forward.normalized * maxMovingDistance);
                    //newPos = new Vector3(transform.position.x + maxMovingDistance, transform.position.y, transform.position.z);
                }
                Debug.DrawLine(transform.position, newPos, Color.blue);
                if (additive)
                {
                    daVelocity += (newPos - transform.position).normalized * movingSpeed;
                }
                else
                {
                    daVelocity = (newPos - transform.position).normalized * movingSpeed;
                }
            }


        }

        private void FixedUpdate()
        {
            rigidBody.MovePosition(transform.position + daVelocity * Time.fixedDeltaTime);
        }

        // Update is called once per frame
        void Update()
        {
            if (moveVerticval && !moveHorizontal)
            {
                HandleVerticalMovement();
            }
            if (moveHorizontal && !moveVerticval)
            {
                HandleHorizontalMovement(false);
            }
            if (moveHorizontal && moveVerticval)
            {
                HandleVerticalMovement();
                HandleHorizontalMovement(true);
            }
        }

    }
}