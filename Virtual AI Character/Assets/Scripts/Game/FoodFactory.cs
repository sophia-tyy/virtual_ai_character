using UnityEngine;

public class FoodFactory : MonoBehaviour {
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Transform foodParent;

    public GameObject InstantiateFood(float x, float y) {
        var instance = Instantiate(foodPrefab, new Vector2(x, y), Quaternion.identity, foodParent);
        return instance;
    }
}