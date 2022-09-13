using UnityEngine;
using Unity.Netcode;

namespace zone.nonon
{
    public class NononHittingControllerNetwork : NetworkBehaviour
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

        public override void OnDestroy()
        {
            GameEvents.Instance.onObjectHit -= OnObjectHit;
            base.OnDestroy();
        }

        // This will be called if an object hits another object with an attack
        private void OnObjectHit(Transform source, Transform target, Transform weapon)
        {
            // only the server can apply dmg
            if (IsServer)
            {
                ApplyDmgToTarget(source, target, weapon);
            }
        }

        void ApplyDmgToTarget(Transform source, Transform target, Transform weapon)
        {
            if (!target.name.Equals("Terrain"))
            {
                NononZoneObjectNetwork nononObjectSource = null;
                NononZoneObjectNetwork nononObjectTarget = null;
                NononZoneObjectNetwork nononObjectWeapon = null;
                source.TryGetComponent<NononZoneObjectNetwork>(out nononObjectSource);
                target.TryGetComponent<NononZoneObjectNetwork>(out nononObjectTarget);
                if (weapon != null)
                {
                    weapon.TryGetComponent<NononZoneObjectNetwork>(out nononObjectWeapon);
                }


                if (nononObjectSource == null)
                {
                    nononObjectSource = source.GetComponentInParent<NononZoneObjectNetwork>();
                }
                if (nononObjectTarget == null)
                {
                    nononObjectTarget = target.GetComponentInParent<NononZoneObjectNetwork>();
                }

                if (nononObjectSource != null && nononObjectTarget != null)
                {
                    if (NononZoneObjectNetwork.isOneOfTypes(target, INononZoneObject.NononZoneObjType.DESTROYABLE))
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

        private int CalcDmg(NononZoneObjectNetwork nononObjectSource, NononZoneObjectNetwork nononObjectTarget, NononZoneObjectNetwork nononObjectWeapon)
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
