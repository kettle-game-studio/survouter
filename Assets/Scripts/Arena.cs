using UnityEngine;
using UnityEngine.AI;

public class Arena : MonoBehaviour
{
    private PlayerController _player;

    public PlayerController GetPlayer()
    {
        if (_player == null)
            _player = FindObjectsByType<PlayerController>(FindObjectsSortMode.None)[0];

        return _player;
    }

    public void HowToGetTo()
    {
        
    }
}
