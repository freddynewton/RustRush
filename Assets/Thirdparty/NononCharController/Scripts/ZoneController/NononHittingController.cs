using UnityEngine;

namespace zone.nonon
{

    public class NononHittingController : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            GameEvents.Instance.onObjectHit += OnObjectHit;
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            GameEvents.Instance.onObjectHit -= OnObjectHit;
        }

        // This will be called if an object hits another object with an attack
        private void OnObjectHit(Transform source, Transform target, Transform weapon)
        {
            ApplyDmgToTarget(source, target, weapon);
        }

        void ApplyDmgToTarget(Transform source, Transform target, Transform weapon)
        {
            if (!target.name.Equals("Terrain"))
            {
                NononZoneObject nononObjectSource = null;
                NononZoneObject nononObjectTarget = null;
                NononZoneObject nononObjectWeapon = null;
                source.TryGetComponent<NononZoneObject>(out nononObjectSource);
                target.TryGetComponent<NononZoneObject>(out nononObjectTarget);
                if (weapon != null)
                {
                    weapon.TryGetComponent<NononZoneObject>(out nononObjectWeapon);
                }


                if (nononObjectSource == null)
                {
                    nononObjectSource = source.GetComponentInParent<NononZoneObject>();
                }
                if (nononObjectTarget == null)
                {
                    nononObjectTarget = target.GetComponentInParent<NononZoneObject>();
                }

                if (nononObjectSource != null && nononObjectTarget != null)
                {
                    if (NononZoneObject.isOneOfTypes(target, INononZoneObject.NononZoneObjType.DESTROYABLE))
                    {
                        // no need to waste time when target is dead
                        if (nononObjectTarget.GetCurrentHealth() > 0)
                        {
                            int dmg = CalcDmg(nononObjectSource, nononObjectTarget, nononObjectWeapon);
                            nononObjectTarget.ReduceHealth(dmg);
                        }
                    }
                }

            }
        }

        private int CalcDmg(NononZoneObject nononObjectSource, NononZoneObject nononObjectTarget, NononZoneObject nononObjectWeapon)
        {

            // DMG = A * (A + 100) / 100 * 8 / (D + 8) = A * (A + 100) * 0.08 / (D + 8)
            //int dmg = (int)(nononObjectSource.attackPower * (nononObjectSource.attackPower + 100) * 0.08 / (nononObjectTarget.defence + 8));
            int dmg = (int)(2 * nononObjectSource.attackPower ^ 2 / (nononObjectSource.attackPower + nononObjectTarget.defence));
            if (nononObjectWeapon != null)
            {
                // Add the dmg of the weapon on top
                //int dmgWpn = (int)(nononObjectWeapon.attackPower * (nononObjectWeapon.attackPower + 100) * 0.08 / (nononObjectTarget.defence + 8));
                int dmgWpn = (int)(2 * nononObjectWeapon.attackPower ^ 2 / (nononObjectWeapon.attackPower + nononObjectTarget.defence));
                dmg += dmgWpn;
            }
            return dmg;
        }
    }
}