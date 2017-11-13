using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability/New Ability")]
public class Ability : ScriptableObject {

    public string AbilityName = "New Ability";
    public GameObject AbilitySwingMeterPrefab;
    public bool isSelfCast = false;
	
}
