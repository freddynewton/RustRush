using System;
using UnityEngine;

namespace zone.nonon
{
    public class GameEvents : NononSingleton<GameEvents>
    {
        // Start is called before the first frame update
        public event Action<Transform, Transform, Transform> onObjectHit;
        public event Action<Transform, Transform> onCollisionEntered;
        public event Action<Transform, Transform> onCollisionExited;
        public event Action<Transform, int, int> onTakeDmg;
        public event Action<Transform, int, int> onGainHealth;
        public event Action<Transform, Transform> onGotAggro;
        public event Action<Transform, Transform> onLostAggro;
        public event Action<Transform> onDying;
        public event Action<Transform> onMounted;
        public event Action<Transform> onUnMounted;
        public event Action<Transform, Transform> onGotLoot;

        public void Die(Transform source)
        {
            if (onDying != null)
            {
                onDying(source);
            }
        }

        public void Mounted(Transform source)
        {
            if (onMounted != null)
            {
                onMounted(source);
            }
        }

        public void UnMounted(Transform source)
        {
            if (onUnMounted != null)
            {
                onUnMounted(source);
            }
        }

        public void GotLoot(Transform source, Transform loot)
        {
            if (onGotLoot != null)
            {
                onGotLoot(source, loot);
            }
        }

        public void ObjectHit(Transform source, Transform target, Transform weapon)
        {
            if (onObjectHit != null)
            {
                onObjectHit(source, target, weapon);
            }
        }


        public void CollisionEntered(Transform source, Transform origin)
        {
            if (onCollisionEntered != null)
            {
                onCollisionEntered(source, origin);
            }
        }



        public void CollisionExited(Transform source, Transform origin)
        {
            if (onCollisionExited != null)
            {
                onCollisionExited(source, origin);
            }
        }

        public void TakeDmg(Transform target, int dmg, int maxHealth)
        {
            if (onTakeDmg != null)
            {
                onTakeDmg(target, dmg, maxHealth);
            }
        }

        public void GainHealth(Transform target, int health, int maxHealth)
        {
            if (onGainHealth != null)
            {
                onGainHealth(target, health, maxHealth);
            }
        }

        public void AggroGained(Transform target, Transform from)
        {
            if (onGotAggro != null)
            {
                onGotAggro(target, from);
            }
        }

        public void AggroLost(Transform target, Transform from)
        {
            if (onLostAggro != null)
            {
                onLostAggro(target, from);
            }
        }

    }
}