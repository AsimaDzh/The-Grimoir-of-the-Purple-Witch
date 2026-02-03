using System;
using UnityEngine;

public class ColliderTrigger : MonoBehaviour
{
    public event EventHandler OnPlayerEnterTrigger;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has a PlayerController component
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null) OnPlayerEnterTrigger?.Invoke(this, EventArgs.Empty);
    }
}
