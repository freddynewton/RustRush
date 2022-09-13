using UnityEngine;
using UnityEngine.AI;

namespace zone.nonon
{
    public class PatrolingStateNetwork : State
    {
        EnemyNetwork enemy;
        NavMeshAgent enemyAgent;
        float timerAtSamePosition;
        float maxWatingingTime = 5;
        public ChasingStateNetwork chasingState;

        private void Start()
        {
            enemy = mainTransform.GetComponent<EnemyNetwork>();
            enemyAgent = enemy.GetComponent<NavMeshAgent>();
        }

        public override State RunCurrentState()
        {
            EnemyNetwork.EnemyRayInfo rayInfo = enemy.SphereCast45();

            if (enemy.GetAggroObject() != null)
            {
                enemy.SetTarget(enemy.GetAggroObject());
                enemyAgent.SetDestination(enemy.GetAggroObject().position);
                enemy.SetPositionAtStartChase(enemy.transform.position);

                // set aggro on both objects
                GameEvents.Instance.AggroGained(enemy.transform, enemy.GetAggroObject().transform);
                GameEvents.Instance.AggroGained(enemy.GetAggroObject().transform, enemy.transform);

                return chasingState;
            }

            if (rayInfo != null)
            {
                if ((rayInfo.hitInfo.transform.tag.Equals(NononZoneConstants.HitBoxCollider.HITBOX_COLLIDED_TAG) &&
                    NononZoneObjectNetwork.isOneOfTypes(rayInfo.hitInfo.transform.parent, INononZoneObject.NononZoneObjType.PLAYER)) ||
                    NononZoneObjectNetwork.isOneOfTypes(rayInfo.hitInfo.transform, INononZoneObject.NononZoneObjType.PLAYER)
                    )
                {
                    Transform playerTransform = rayInfo.hitInfo.transform;
                    if (rayInfo.hitInfo.transform.tag.Equals(NononZoneConstants.HitBoxCollider.HITBOX_COLLIDED_TAG))
                    {
                        playerTransform = playerTransform.parent;
                    }
                    NononZoneObjectNetwork no = playerTransform.GetComponent<NononZoneObjectNetwork>();
                    if (no.GetCurrentHealth() > 0)
                    {
                        enemy.SetTarget(playerTransform);
                        enemyAgent.SetDestination(playerTransform.position);
                        enemy.SetPositionAtStartChase(enemy.transform.position);

                        // set aggro on both objects
                        GameEvents.Instance.AggroGained(enemy.transform, playerTransform);
                        GameEvents.Instance.AggroGained(playerTransform, enemy.transform);

                        return chasingState;
                    }

                }
            }

            float dist = enemyAgent.remainingDistance;
            if (dist != Mathf.Infinity && enemyAgent.pathStatus == NavMeshPathStatus.PathComplete && enemyAgent.remainingDistance == 0)
            {
                SetNewDestination();
            }
            else
            {
                if (enemyAgent.velocity.Equals(Vector3.zero))
                {
                    timerAtSamePosition += Time.deltaTime;
                }
                else
                {
                    timerAtSamePosition = 0;
                }
                if (timerAtSamePosition > maxWatingingTime)
                {
                    SetNewDestination();
                }

            }
            return this;
        }
        private void SetNewDestination()
        {
            Vector3 randompoint;
            if (RandomPoint(enemy.transform.position, enemy.maxPatrolDistance, out randompoint))
            {
                enemyAgent.SetDestination(randompoint);
            }
        }

        bool RandomPoint(Vector3 center, float range, out Vector3 result)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 randomPoint = center + Random.insideUnitSphere * range;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }
            result = Vector3.zero;
            return false;
        }

        public override void OnStateEntry()
        {
            enemyAgent.speed = enemy.patrollingSpeed;
            timerAtSamePosition = 0;
            enemy.SetEyeLook(false);

        }

        public override void OnStateExit()
        {
            // nothing to do
        }
    }
}