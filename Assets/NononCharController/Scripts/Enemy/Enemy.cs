using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace zone.nonon
{
    public class Enemy : MonoBehaviour
    {
        public class EnemyRayInfo
        {
            public RaycastHit hitInfo;

            public EnemyRayInfo(RaycastHit _hitInfo)
            {
                hitInfo = _hitInfo;
            }
        }

        public Transform rayCastOrigin;
        public Transform enemyModel;

        public float raycastDistance = 10;
        public float raycastWidth = 5;
        public float maxPatrolDistance = 20;
        public float raycastSideAngle = 45;
        public float chasingSpeed = 5f;
        public float patrollingSpeed = 3.5f;
        public float chasingAggroedSpeed = 7.0f;

        public float maxAggroTime = 5f;
        Transform aggroObject;
        Transform objectInHitbox;
        float timeObjectAggroed;

        bool isDead = false;
        bool enemyReached = false;

        DmgHealthTextController dmgHealthTextController;


        Transform target;
        Vector3 positionAtStartChase;

        void Start()
        {
            RegisterGameEvents();
            dmgHealthTextController = GetComponent<DmgHealthTextController>();
        }

        private void RegisterGameEvents()
        {
            GameEvents.Instance.onObjectHit += OnObjectHit;
            GameEvents.Instance.onDying += OnDying;
            GameEvents.Instance.onCollisionEntered += OnCollisionEntered;
            GameEvents.Instance.onCollisionExited += OnCollisionExited;
        }

        private void OnDestroy()
        {
            UnregisterGameEvents();
        }

        private void UnregisterGameEvents()
        {
            GameEvents.Instance.onObjectHit -= OnObjectHit;
            GameEvents.Instance.onDying -= OnDying;
            GameEvents.Instance.onCollisionEntered -= OnCollisionEntered;
            GameEvents.Instance.onCollisionExited -= OnCollisionExited;
        }

        public void PortEnemy(Vector3 newPosition, Quaternion newRotation)
        {
            PortEnemyLocal(newPosition, newRotation);
        }

        private void PortEnemyLocal(Vector3 newPosition, Quaternion newRotation)
        {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            agent.enabled = false;
            transform.position = newPosition;
            transform.rotation = newRotation;
            agent.enabled = true;
        }


        // Update is called once per frame
        void Update()
        {
            UpdateLocal();
        }

        private void UpdateLocal()
        {
            if (aggroObject != null)
            {
                if (Time.time - timeObjectAggroed > maxAggroTime)
                {
                    // Remove Aggro if the object is too far away
                    ResetAggro();
                }
            }
        }

        IEnumerator DoDmg2ObjectInHitbox()
        {
            while (objectInHitbox != null)
            {
                GetComponent<EnemySound>().Attack();
                GameEvents.Instance.ObjectHit(transform, objectInHitbox, null);
                yield return new WaitForSeconds(1);
            }
        }

        public Transform GetAggroObject()
        {
            return aggroObject;
        }

        public bool HasReachedEnemy()
        {
            return enemyReached;
        }

        public bool IsDead()
        {
            return isDead;
        }

        public void ResetAggro()
        {
            aggroObject = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        private void OnDying(Transform source)
        {
            if (!isDead)
            {
                if (source.Equals(transform))
                {
                    GetComponent<EnemySound>().Die();
                    isDead = true;

                    if (enemyModel.GetComponent<MeshRenderer>() != null)
                    {
                        // disable colliders
                        GetComponent<CapsuleCollider>().enabled = false;

                        DissolveEffect dissolveEffect = GetComponent<DissolveEffect>();
                        if (dissolveEffect != null)
                        {
                            dissolveEffect.StartDissolvingAndDestroy(enemyModel);
                        }

                        //enemyModel.GetComponent<MeshRenderer>().material.color = Color.red;
                    }

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="weapon"></param>
        private void OnObjectHit(Transform source, Transform target, Transform weapon)
        {
            if (!isDead)
            {
                if (target.Equals(transform))
                {
                    timeObjectAggroed = Time.time;
                    aggroObject = source;
                }
            }
        }

        public void OnCollisionEntered(Transform source, Transform origin)
        {
            if (source.transform.Equals(transform) && !isDead)
            {
                objectInHitbox = origin;
                enemyReached = true;
                StartCoroutine(DoDmg2ObjectInHitbox());
            }
        }

        public void OnCollisionExited(Transform source, Transform origin)
        {
            if (source.transform.Equals(transform))
            {
                objectInHitbox = null;
                enemyReached = false;
                StopCoroutine(DoDmg2ObjectInHitbox());
            }
        }

        public void SetEyeLook(bool _aggro)
        {
            Color _col = Color.white;
            if (_aggro) _col = Color.red;
            SetEyeColorOfMesh(_col);
        }

        private void SetEyeColorOfMesh(Color _color)
        {
            if (enemyModel.GetChild(0) != null && (enemyModel.GetChild(1) != null &&
                        enemyModel.GetChild(0).GetComponent<MeshRenderer>() != null && enemyModel.GetChild(1).GetComponent<MeshRenderer>() != null))
            {
                enemyModel.GetChild(0).GetComponent<MeshRenderer>().material.color = _color;
                enemyModel.GetChild(1).GetComponent<MeshRenderer>().material.color = _color;
            }
        }

        public void SetPositionAtStartChase(Vector3 pos)
        {
            positionAtStartChase = pos;
        }

        public Vector3 GetPositionAtStartChase()
        {
            return positionAtStartChase;
        }

        public void SetTarget(Transform _target)
        {
            target = _target;
        }

        public Transform GetTarget()
        {
            return target;
        }

        public EnemyRayInfo SphereCast45()
        {

            // raycast forward
            Vector3 origin = rayCastOrigin.position;
            Vector3 direction = rayCastOrigin.forward * raycastDistance;
            Ray ray = new Ray(origin, direction);
            RaycastHit hitInfo;
            Debug.DrawRay(origin, direction);
            if (Physics.SphereCast(ray, raycastWidth, out hitInfo, raycastDistance))
            {
                return new EnemyRayInfo(hitInfo);
            }
            // do 45 degrees right
            direction = Quaternion.Euler(0, raycastSideAngle, 0) * transform.forward * raycastDistance;
            ray = new Ray(origin, direction);
            Debug.DrawRay(origin, direction);
            if (Physics.SphereCast(ray, raycastWidth, out hitInfo, raycastDistance))
            {
                return new EnemyRayInfo(hitInfo);
            }
            // do 45 degrees left
            direction = Quaternion.Euler(0, -raycastSideAngle, 0) * transform.forward * raycastDistance;
            ray = new Ray(origin, direction);
            Debug.DrawRay(origin, direction);
            if (Physics.SphereCast(ray, raycastWidth, out hitInfo, raycastDistance))
            {
                return new EnemyRayInfo(hitInfo);
            }

            return null;
        }

    }
}