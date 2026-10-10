using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameHandler : NetworkBehaviour {

    [SerializeField] private CharacterWindow characterWindow;
    [SerializeField] private PlayerLoader playerLoader;
    
    private Character character;

    private void Awake() {
        character = new Character();
        character.SetEquippedBodyArmor(Character.BodyArmor.None);
        characterWindow.SetCharacter(character);
        
    }


    private void Update()
    {
        if (character.GetHealth() <= 0)
        {
            print(gameObject.name + " is DEAD"); //<= RUN YOUR FUNCTION WHEN PLAYER IS DEAD HERE (i.e. health is 0)
            //Destroy(gameObject.GetComponentInParent<PlayerMovement>().gameObject);

            playerLoader.RespawnCharacter();
            character.HealHealth(100000000);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void TakeDamageServerRpc(int damageLevel)
    {
        TakeDamageClientRpc(damageLevel);
    }

    [ClientRpc]
    public void TakeDamageClientRpc (int damageLevel)
    {
        character.Damage(damageLevel);
    }

    public bool TakeDamage(int damageLevel)
    {
        int currentHealth = character.GetHealth();
        int newHealth = currentHealth - damageLevel;
        TakeDamageServerRpc(damageLevel);
        return (newHealth <= 0);


    }
}
