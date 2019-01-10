

public class EnemyStats {

    public float lineOfSightDistance = 8.0f; // How far the enemy can see
    public float enemyMaxSpeed = 7.0f;

    public float runSpeed = 12f;

    public int fixedUpdateCount = 0;

    /* May move all jumping modifiers to utils if we want some unified jump feel for player and npcs */
    public float jumpSpeed = 20f;
	public float fallMultiplier = 4f;
	public float lowJumpMultiplier = 3f;
	public float playerBonusGravity = 9.8f; // add additional gravity to get faster jumps

    public bool jumping = false;

    public bool currentlyAvoidingBullet = false;

    // Don't move further than this to the left or right while dodging a bullet 
    public float maxDodgeDistance = 5f;

}