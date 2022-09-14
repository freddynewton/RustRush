using UnityEngine;
using UnityEngine.AI;

namespace zone.nonon
{

    public class ChasingStateNetwork : State
    {
        EnemyNetwork enemy;
        NavMeshAgent enemyAgent;
        public PatrolingStateNetwork patrolingState;
        public DeadState deadState;

        private void Start()
        {
            enemy = mainTransform.GetComponent<EnemyNetwork>();
            enemyAgent = enemy.GetComponent<NavMeshAgent>();
        }

        public override State RunCurrentState()
        {
            if (enemy != null)
            {


                if (enemy.GetAggroObject() == null && enemy.GetTarget() != null && Vector3.Distance(enemy.GetTarget().position, enemy.transform.position) > enemy.raycastDistance + 5.0f)
                {
                    GameEvents.Instance.AggroLost(enemy.transform, enemy.GetTarget().transform);
                    GameEvents.Instance.AggroLost(enemy.GetTarget().transform, enemy.transform);

                    // not in range? just patrol at where you were 
                    enemyAgent.SetDestination(enemy.GetPositionAtStartChase());
                    return patrolingState;

                }
                else if (enemy.IsDead())
                {
                    GameEvents.Instance.AggroLost(enemy.transform, enemy.GetTarget().transform);
                    GameEvents.Instance.AggroLost(enemy.GetTarget().transform, enemy.transform);
                    if (enemy.GetAggroObject() != null)
                    {
                        GameEvents.Instance.AggroLost(enemy.transform, enemy.GetAggroObject().transform);
                        GameEvents.Instance.AggroLost(enemy.GetAggroObject().transform, enemy.transform);
                    }

                    enemyAgent.ResetPath();
                    enemyAgent.SetDestination(enemy.transform.position);
                    return deadState;
                }
                else
                {
                    if (enemy.GetAggroObject() != null)
                    {
                        Transform enemyTarget = enemy.GetAggroObject();
                        NononZoneObjectNetwork enemyNoNoNZoneObject;
                        if (enemyTarget.TryGetComponent<NononZoneObjectNetwork>(out enemyNoNoNZoneObject))
                        {
                            if (enemyNoNoNZoneObject.GetCurrentHealth() <= 0)
                            {
                                GameEvents.Instance.AggroLost(enemy.transform, enemy.GetTarget().transform);
                                GameEvents.Instance.AggroLost(enemy.transform, enemy.GetAggroObject().transform);
                                GameEvents.Instance.AggroLost(enemy.GetTarget().transform, enemy.transform);
                                GameEvents.Instance.AggroLost(enemy.GetAggroObject().transform, enemy.transform);

                                enemyAgent.ResetPath();
                                enemyAgent.SetDestination(enemy.transform.position);
                                return patrolingState;
                            }
                            else
                            {
                                // if u injured it, it will follow you
                                GameEvents.Instance.AggroGained(enemy.transform, enemyTarget.transform);
                                GameEvents.Instance.AggroGained(enemyTarget.transform, enemy.transform);
                                if (!enemy.HasReachedEnemy())
                                {
                                    enemyAgent.SetDestination(enemyTarget.position);
                                    enemyAgent.speed = enemy.chasingAggroedSpeed;
                                }
                                else
                                {
                                    enemyAgent.isStopped = true;
                                    enemyAgent.ResetPath();
                                    enemy.transform.LookAt(enemyTarget);
                                }

                            }
                        }
                        else
                        {

                            // if u injured it, it will follow you
                            GameEvents.Instance.AggroGained(enemy.transform, enemyTarget.transform);
                            GameEvents.Instance.AggroGained(enemyTarget.transform, enemy.transform);
                            Debug.Log(enemy.HasReachedEnemy());
                            if (!enemy.HasReachedEnemy())
                            {
                                enemyAgent.SetDestination(enemyTarget.position);
                                enemyAgent.speed = enemy.chasingAggroedSpeed;
                            }
                            else
                            {
                                enemyAgent.isStopped = true;
                                enemyAgent.ResetPath();
                                enemy.transform.LookAt(enemyTarget);
                            }
                        }


                    }
                    else
                    {
                        Transform enemyTarget = enemy.GetTarget();
                        NononZoneObjectNetwork no;

                        if (enemyTarget != null && enemyTarget.TryGetComponent<NononZoneObjectNetwork>(out no))
                        {
                            if (no.GetCurrentHealth() <= 0)
                            {
                                GameEvents.Instance.AggroLost(enemy.transform, enemy.GetTarget().transform);
                                GameEvents.Instance.AggroLost(enemy.GetTarget().transform, enemy.transform);

                                enemyAgent.ResetPath();
                                enemyAgent.SetDestination(enemy.transform.position);
                                return patrolingState;
                            }
                            else
                            {
                                enemyAgent.speed = enemy.chasingSpeed;
                                // still in distance, follow again
                                GameEvents.Instance.AggroGained(enemy.transform, enemyTarget.transform);
                                GameEvents.Instance.AggroGained(enemyTarget.transform, enemy.transform);
                                if (!enemy.HasReachedEnemy())
                                {
                                    enemyAgent.SetDestination(enemyTarget.position);
                                    enemyAgent.speed = enemy.chasingSpeed;
                                }
                                else
                                {
                                    enemyAgent.isStopped = true;
                                    enemyAgent.ResetPath();
                                    enemy.transform.LookAt(enemyTarget);
                                }
                            }
                        }
                        else if (enemyTarget != null)
                        {
                            enemyAgent.speed = enemy.chasingSpeed;
                            // still in distance, follow again
                            GameEvents.Instance.AggroGained(enemy.transform, enemyTarget.transform);
                            GameEvents.Instance.AggroGained(enemyTarget.transform, enemy.transform);
                            if (!enemy.HasReachedEnemy())
                            {
                                enemyAgent.SetDestination(enemyTarget.position);
                                enemyAgent.speed = enemy.chasingSpeed;
                            }
                            else
                            {
                                enemyAgent.isStopped = true;
                                enemyAgent.ResetPath();
                                enemy.transform.LookAt(enemyTarget);
                            }

                        }

                    }

                    // ROll the eyes :)
                    float maxAngle = 85;
                    float randomX = Random.Range(-maxAngle, maxAngle);
                    float randomY = Random.Range(-maxAngle, maxAngle);
                    float randomZ = Random.Range(-maxAngle, maxAngle);
                    float randomX2 = Random.Range(-maxAngle, maxAngle);
                    float randomY2 = Random.Range(-maxAngle, maxAngle);
                    float randomZ2 = Random.Range(-maxAngle, maxAngle);

                    enemy.enemyModel.GetChild(0).localRotation = Quaternion.Slerp(enemy.enemyModel.GetChild(0).localRotation, Quaternion.Euler(randomX, randomY, randomZ), 0.05f);
                    enemy.enemyModel.GetChild(1).localRotation = Quaternion.Slerp(enemy.enemyModel.GetChild(0).localRotation, Quaternion.Euler(randomX, randomY, randomZ), 0.05f);
                }
            }

            return this;
        }

        public override void OnStateEntry()
        {
            enemyAgent.speed = enemy.chasingSpeed;
            enemy.SetEyeLook(true);
            enemy.GetComponent<EnemySound>().Aggro();
        }

        public override void OnStateExit()
        {
            // nothing to do
        }
    }
}