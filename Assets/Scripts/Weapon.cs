using UnityEngine;

public class Weapon : CellObject
{
    public int StrengthBonus = 1;

    public override void PlayerEntered()
    {
        GameManager.Instance.PlayerController.AddStrength(StrengthBonus);
        Destroy(gameObject);
    }
}