using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INononZoneObject
{
    public enum NononZoneObjType { PLAYER, WATER, ENEMY, LOOT, PLATTFORM, WEAPON, PROJECTILE, MOUNT, DESTROYABLE, WALKABLE };

    public bool IsOneOfTypes(NononZoneObjType type);

}
