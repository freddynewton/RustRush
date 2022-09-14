namespace zone.nonon
{
    public static class NononZoneConstants
    {
        public static class CommonEditor
        {
            public const string NONON_ZONE_MENU = "Tools/Nonon Zone";
            public const string PLAYER_MENU = NONON_ZONE_MENU + "/Player";
            public const string JOBS_MENU = NONON_ZONE_MENU + "/Jobs";

            // Player            
            public const string IMPORT_PLAYER_MENU = PLAYER_MENU + "/Rig/Import Player Prefab or FBX (incl. Weapon Rigging)";
            public const string ADD_PLAYER_WEAPON_RIGS_MENU = PLAYER_MENU + "/Rig/Add Weapon Rigs";
            public const string STORE_BONES_MENU = PLAYER_MENU + "/Rig/Store Bones";

            // Leak Detection
            public const string LEAK_DETECTION_ENABLED = JOBS_MENU + "/Leak Detection Enabled";
            public const string LEAK_DETECTION_STACKTRACE_ENABLED = JOBS_MENU + "/Leak Detection Stacktrace Enabled";
            public const string LEAK_DETECTION_DISABLED = JOBS_MENU + "/Leak Detection Disabled";

            // Directorties
            public const string NON_ZONE_CONTROLLER_PATH_PURE = "NononCharController/";
            public const string NON_ZONE_CONTROLLER_PATH = "Assets/" + NON_ZONE_CONTROLLER_PATH_PURE;
            public const string NON_ZONE_ASSET_PATH = "Assets/";
            public const string NON_ZONE_PLAYER_PREFAB_PATH = "prefabs/Player/";
            public const string NON_ZONE_PLAYER_PREFAB_PATH_COMPLETE = NON_ZONE_CONTROLLER_PATH + NON_ZONE_PLAYER_PREFAB_PATH;

        }

        public static class EditorSinglePlayer
        {
            public const string SINGLE_PLAYER_MENU = CommonEditor.NONON_ZONE_MENU + "/Singleplayer";
            public const string SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU = SINGLE_PLAYER_MENU + "/Simple Elements";
            public const string SINGLE_PLAYER_ZONE_MENU = SINGLE_PLAYER_MENU + "/Zone";

            // Simple Elements
            public const string ADD_HITBOX_COLLIDER_MENU = SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add HitBoxCollider";
            public const string ADD_HEALTH_BAR_MENU = SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add HealthBar";
            public const string ADD_DMG_HEALTH_TEXT_MENU = SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add DMG Health Text";
            public const string ADD_WATER_MENU = SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add Water";
            public const string ADD_PORTAL_PAIR_MENU = SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add Portal Pair";
            public const string ADD_LOOT_BUBBLE_MENU = SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add Loot Bubble";
            public const string ADD_SPAWN_POINT_MENU = SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add Spawn Point";
            public const string ADD_CUSTOMER_INFO_MENU = SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add Customer Info";
            public const string ADD_ENEMY_MENU = SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add Enemy";
            public const string ADD_LANDING_PLATTFORM_MENU = SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU + "/Plattform/Add Landing Plattform";
            public const string ADD_SMALL_PLATTFORM_MENU = SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU + "/Plattform/Add Small Plattform";
            public const string ADD_BIG_PLATTFORM_MENU = SINGLE_PLAYER_SIMPLE_ELEMENTS_MENU + "/Plattform/Add Big Plattform";

            // Zone
            public const string ADD_PLAYER_MENU = SINGLE_PLAYER_ZONE_MENU + "/Add Player";
            public const string ADD_PREFAB_DIR_MENU = SINGLE_PLAYER_ZONE_MENU + "/Add PrefabDirectory";
            public const string ADD_ZONE_PORTAL_MENU = SINGLE_PLAYER_ZONE_MENU + "/Add Zone Portal";
            public const string SETUP_ZONE_MENU = SINGLE_PLAYER_ZONE_MENU + "/Setup New Zone";
            public const string ADD_ZONE_CONTROLLER_UI_MENU = SINGLE_PLAYER_ZONE_MENU + "/Add Zone Controller UI";


            // Prefabs
            public const string PLAYER_PREFAB_NAME = "PlayerPrefab.prefab";
            public const string GAME_CONTROLLER_PREFAB_NAME = "NononZoneGameController.prefab";
            public const string HITBOX_PREFAB_NAME = "NononZoneBoxColliderPrefab.prefab";
            public const string HEALTH_BAR_PREFAB_NAME = "HealthBarCanvas.prefab";
            public const string FLOATING_TEXT_CONTAINER_PREFAB_NAME = "FloatingTextContainer.prefab";
            public const string WATER_BASIC_DAYTIME_PREFAB_NAME = "WaterBasicDaytime.prefab";
            public const string PORTAL_PAIR_PREFAB_NAME = "PortalPair.prefab";
            public const string ZONE_PORTAL_PREFAB_NAME = "ZonePortal.prefab";
            public const string TERRAIN_PREFAB_NAME = "TerrainPrefab.prefab";
            public const string LOOT_BUBBLE_PREFAB_NAME = "LootBubble.prefab";
            public const string ENEMY_PREFAB_NAME = "Enemy.prefab";
            public const string CUSTOMER_INFO_PREFAB_NAME = "CustomerInfo.prefab";
            public const string LANDING_PLATTFORM_PREFAB_NAME = "LandingPlattform.prefab";
            public const string BIG_PLATTFORM_PREFAB_NAME = "BigPlattform.prefab";
            public const string SMALL_PLATTFORM_PREFAB_NAME = "SmallPlattform.prefab";
            public const string SPAWN_POINT_PREFAB_NAME = "NononSpawnPoint.prefab";
            public const string PREFAB_DIR_PREFAB_NAME = "PrefabDirectory.prefab";
            public const string ZONE_CONTROLLE_UI_PREFAB_NAME = "NononZONEControllerUI.prefab";



            // Spawned Prefab Names
            public const string DMG_HEALTH_TEXT_SPAWN_POS = "DMG_HEALTH_TEXT_SPAWN_POS";
            public const string GAME_CONTROLLER_NAME = "GAME_CONTROLLER";            
            public const string PLAYER_SPAWN_POINT_NAME = "PLAYER_SPAWN_POINT";
            public const string PLAYER_REBIRTH_POINT_NAME = "PLAYER_REBIRTH_POINT";

            // Distance
            public const int OBJECT_PLACING_DISTANCE = 15;
        }

        public static class EditorMultiplayer
        {
            public const string MULTI_PLAYER_MENU = CommonEditor.NONON_ZONE_MENU + "/Multiplayer";
            public const string MULTI_PLAYER_SIMPLE_ELEMENTS_MENU = MULTI_PLAYER_MENU + "/Simple Elements";
            public const string MULTI_PLAYER_ZONE_MENU = MULTI_PLAYER_MENU + "/Zone";

            // Simple Elements
            public const string ADD_HITBOX_COLLIDER_MENU = MULTI_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add HitBoxCollider";
            public const string ADD_HEALTH_BAR_MENU = MULTI_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add HealthBar";
            public const string ADD_DMG_HEALTH_TEXT_MENU = MULTI_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add DMG Health Text";
            public const string ADD_WATER_MENU = MULTI_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add Water";
            public const string ADD_PORTAL_PAIR_MENU = MULTI_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add Portal Pair";
            public const string ADD_LOOT_BUBBLE_MENU = MULTI_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add Loot Bubble";
            public const string ADD_SPAWN_POINT_MENU = MULTI_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add Spawn Point";
            public const string ADD_CUSTOMER_INFO_MENU = MULTI_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add Customer Info";
            public const string ADD_ENEMY_MENU = MULTI_PLAYER_SIMPLE_ELEMENTS_MENU + "/Add Enemy";
            public const string ADD_LANDING_PLATTFORM_MENU = MULTI_PLAYER_SIMPLE_ELEMENTS_MENU + "/Plattform/Add Landing Plattform";
            public const string ADD_SMALL_PLATTFORM_MENU = MULTI_PLAYER_SIMPLE_ELEMENTS_MENU + "/Plattform/Add Small Plattform";
            public const string ADD_BIG_PLATTFORM_MENU = MULTI_PLAYER_SIMPLE_ELEMENTS_MENU + "/Plattform/Add Big Plattform";

            // Prefabs
            public const string PLAYER_PREFAB_NAME = "PlayerPrefabNetwork.prefab";
            public const string GAME_CONTROLLER_PREFAB_NAME = "NononZoneGameControllerNetwork.prefab";
            public const string HITBOX_PREFAB_NAME = "NononZoneBoxColliderPrefab.prefab";
            public const string HEALTH_BAR_PREFAB_NAME = "HealthBarCanvasNetwork.prefab";
            public const string FLOATING_TEXT_CONTAINER_PREFAB_NAME = "FloatingTextContainer.prefab";
            public const string WATER_BASIC_DAYTIME_PREFAB_NAME = "WaterBasicDaytime.prefab";
            public const string PORTAL_PAIR_PREFAB_NAME = "PortalPair.prefab";
            public const string ZONE_PORTAL_PREFAB_NAME = "ZonePortalNetwork.prefab";
            public const string TERRAIN_PREFAB_NAME = "TerrainPrefab.prefab";
            public const string LOOT_BUBBLE_PREFAB_NAME = "LootBubbleNetwork.prefab";
            public const string ENEMY_PREFAB_NAME = "EnemyNetwork.prefab";
            public const string CUSTOMER_INFO_PREFAB_NAME = "CustomerInfo.prefab";
            public const string LANDING_PLATTFORM_PREFAB_NAME = "LandingPlattformNetwork.prefab";
            public const string BIG_PLATTFORM_PREFAB_NAME = "BigPlattformNetwork.prefab";
            public const string SMALL_PLATTFORM_PREFAB_NAME = "SmallPlattformNetwork.prefab";
            public const string LOGIN_MANAGER_PREFAB_NAME = "LoginManager.prefab";
            public const string MULTIPLAYER_DEV_UI_PREFAB_NAME = "MultiplayerDevUI.prefab";
            public const string NETWORK_MANAGER_PREFAB_NAME = "NononNetworkManager.prefab";
            public const string CLIENT_PLAYER_DATA_PREFAB_NAME = "ClientPlayerData.prefab";
            public const string SPAWN_POINT_PREFAB_NAME = "NononSpawnPoint.prefab";
            public const string PREFAB_DIR_PREFAB_NAME = "PrefabDirectory.prefab";
            public const string NETWORK_SPAWNER_PREFAB_NAME = "NononNetworkspawner.prefab";
            public const string ZONE_CONTROLLE_UI_PREFAB_NAME = "NononZONEControllerUINetwork.prefab";


            // Zone
            public const string ADD_PLAYER_MENU = MULTI_PLAYER_ZONE_MENU + "/Add Player";
            public const string SETUP_ZONE_MENU = MULTI_PLAYER_ZONE_MENU + "/Setup New Zone";
            public const string ADD_ZONE_PORTAL_MENU = MULTI_PLAYER_ZONE_MENU + "/Add Zone Portal";
            public const string ADD_MULTIPLAYER_DEV_UI_MENU = MULTI_PLAYER_ZONE_MENU + "/Add MultiplayerDevUI";
            public const string ADD_NW_MANAGER_MENU = MULTI_PLAYER_ZONE_MENU + "/Add NetworkManager";
            public const string ADD_LOGIN_MANAGER_MENU = MULTI_PLAYER_ZONE_MENU + "/Add LoginManager";
            public const string ADD_CLIENT_PLAYER_DATA_MENU = MULTI_PLAYER_ZONE_MENU + "/Add ClientPlayerData";
            public const string ADD_PREFAB_DIR_MENU = MULTI_PLAYER_ZONE_MENU + "/Add PrefabDirectory";
            public const string ADD_NW_SPAWNER_MENU = MULTI_PLAYER_ZONE_MENU + "/Add NononNetworkspawner";
            public const string ADD_ZONE_CONTROLLER_UI_MENU = MULTI_PLAYER_ZONE_MENU + "/Add Zone Controller UI";

            // Spawned Prefab Names
            public const string DMG_HEALTH_TEXT_SPAWN_POS = "DMG_HEALTH_TEXT_SPAWN_POS";
            public const string LOOT_SPAWN_POINT_GO_NAME = "LOOT_SPAWN_POINT_";
            public const string PLAYER_SPAWN_POINT_NAME = "PLAYER_SPAWN_POINT";            
            public const string PLAYER_REBIRTH_POINT_NAME = "PLAYER_REBIRTH_POINT";

            // Directorties
            public const string NON_NW_ZONE_CONTROLLER_PATH_PURE = "NononCharController/";
            public const string NON_NW_ZONE_CONTROLLER_PATH = "Assets/" + NON_NW_ZONE_CONTROLLER_PATH_PURE;
        }

        public static class I18N_CustomerInfo
        {
            public readonly static string TABLE_CUSTOMER_INFO = "CustomerInfo";
            public readonly static string CUSTOMER_INFO_TEXT_KEY = "customerInfoText";
        }

        public static class I18N_NononZoneController
        {
            public readonly static string TABLE_NONONZONE_CONTROLLER = "NononZoneController";
            public readonly static string GOT_WEAPON_MESSAGE_KEY = "GotWeapon";
            public readonly static string GOT_THRUSTER_MESSAGE_KEY = "GotThruster";
            public readonly static string QUEST1_TEXT_KEY = "Quest1Text";
            public readonly static string QUEST2_TEXT_KEY = "Quest2Text";
            public readonly static string QUEST3_TEXT_KEY = "Quest3Text";
            public readonly static string QUEST4_TEXT_KEY = "Quest4Text";
            public readonly static string ENEMIES_FASTER_MESSAGE_KEY = "EnemiesFaster";
            public readonly static string GAME_OVER_MESSAGE_KEY = "GameOver";
            public readonly static string MONSTER_TEXT_KEY = "Monster";
        }

        public static class Player
        {
            public readonly static string PLAYER_TAG = "Player";
            public readonly static string IS_WALKING_ANIM_PARAM = "isWalking";
            public readonly static string IS_RUNNING_ANIM_PARAM = "isRunning";
            public readonly static string IS_SPEEDPERCENT_ANIM_PARAM = "speedPercent";
            public readonly static string IS_JUMPING_ANIM_PARAM = "isJumping";
            public readonly static string IS_FALLING_ANIM_PARAM = "isFalling";
            public readonly static string IS_SWIMMING_ANIM_PARAM = "isSwimming";
            public readonly static string IS_FLYING_ANIM_PARAM = "isFlying";
            public readonly static string IS_DEAD_ANIM_PARAM = "isDead";
            public readonly static string DRAW_2HR_WEAPON_ANIM_TRIGGER = "draw2hRWeapon";
            public readonly static string DRAW_2HR_WEAPON_ANIM_NAME = "Draw2HRWeapon";
            public readonly static string IGNORE_ISGROUNDED_LAYER_NAME = "IgnoreIsGrounded";
        }

        public static class ZoneController
        {
            public readonly static string ZONE_CONTROLLER_TAG = "ZoneController";
        }

        public static class Starting
        {
            public readonly static string MODE_CMD_LINE_ARG = "-Mode";
            public readonly static string SCENE_CMD_LINE_ARG = "-Scene";
            public readonly static string MODE_SERVER = "SERVER";
            public readonly static string MODE_CLIENT = "CLIENT";
            public readonly static string MODE_HOST = "HOST";
            public readonly static string MODE_SPECTATOR = "SPECTATOR";
        }

        public static class Spectator
        {
            public readonly static string TERRAIN_LAYER_NAME = "Terrain";
        }

        public static class InputActions
        {
            public readonly static string REGULAR_INPUT_ACTION = "Regular_Input_Action";
            public readonly static string DIALOG_INPUT_ACTION = "Dialog_Input_Action";
        }

        public static class HitBoxCollider
        {
            public readonly static string HITBOX_COLLIDED_TAG = "HitBoxCollider";
        }

        public static class Camera
        {
            public readonly static string MAIN_CAMERA_TAG = "MainCamera";
        }

        public static class Weapon
        {
            public readonly static int WEAPON_STANDARD_DISTANCE = 50;
        }

        public static class Loot
        {

        }

        public static class Effect
        {
            public const string COIN_EFFECT_PREFAB_NAME = "LandingPlattform";
        }

        public static class OverlayGUI
        {
            public readonly static string OVERLAY_GUI_TAG = "OverlayGUI";
        }


        public static class Messages
        {
            public readonly static string HITBOX_COLLIDED_ENTERED_MESSAGE = "HitBoxCollisionEntered";
            public readonly static string HITBOX_COLLIDED_EXITED_MESSAGE = "HitBoxCollisionExited";
        }

        public static class WeaponPlaceHolder
        {
            public readonly static string RIGHT_ARM_IK_LABEL = "RIGHT_ARM_IK";
            public readonly static string LEFT_ARM_IK_LABEL = "LEFT_ARM_IK";

            public readonly static string RH_IK_TARGET_LABEL = "RH_IK_TARGET";
            public readonly static string LH_IK_TARGET_LABEL = "LH_IK_TARGET";
            public readonly static string RH_IK_HINT_LABEL = "RH_IK_HINT";
            public readonly static string LH_IK_HINT_LABEL = "LH_IK_HINT";

            public readonly static string WPN_HOLER_2HRIGHT_LABEL = "WPN_HOLDER_2HRIGHT";
            public readonly static string WPN_HOLER_2HLEFT_LABEL = "WPN_HOLDER_2HLEFT";
            public readonly static string WPN_HOLER_1HRIGHT_LABEL = "WPN_HOLDER_1HRIGHT";
            public readonly static string WPN_HOLER_1HLEFT_LABEL = "WPN_HOLDER_1HLEFT";

            public readonly static string WPN_DRAW_HOLDER_LABEL = "WPN_DRAW_HOLDER";

            public readonly static string RH_POS_LABEL = "RH_POS";
            public readonly static string LH_POS_LABEL = "LH_POS";

            public readonly static string WEAPON_POSE_RIG_LABEL = "WEAPON_POSE_RIG";
            public readonly static string WEAPON_POSE_LABEL = "WEAPON_POSE";
            public readonly static string PLAYER_CHAR_RIG_LABEL = "PLAYER_CHAR_RIG";
            public readonly static string WEAPON_AIM_RIG_LABEL = "WEAPON_AIM_RIG";
            public readonly static string WEAPON_AIM_POSE_LABEL = "WEAPON_AIM_POSE";

            public readonly static string AIM_TARGET_HOLDER_LABEL = "AIM_TARGET_HOLDER";
            public readonly static string AIM_TARGET_LABEL = "AIM_TARGET";

            public readonly static string BODY_POSE_AIM_LABEL = "BODY_POSE_AIM";
            public readonly static string BODY_POSE_AIM_SPINE_LABEL = "BODY_POSE_AIM_SPINE";
            public readonly static string BODY_POSE_AIM_HEAD_LABEL = "BODY_POSE_AIM_HEAD";

        }

    }
}