using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class WorldReset : MonoBehaviour {
    void Update() {
        if (Keyboard.current.rKey.wasPressedThisFrame) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}