using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCManager : MonoBehaviour
{
    [Header("NPC Prefab")]
    [SerializeField] private GameObject npcPrefab;

    [Header("Position Assignments")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform waitingPoint;
    [SerializeField] private Transform[] randomDestinations;

    [Header("Player Reference")]
    [SerializeField] private Transform player;

    private GameObject spawnedNPC;

    void Update()
    {
        // Dev tool - Press T to spawn NPC (remove later)
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            SpawnNPC();
        }

        // Check if spawned NPC was destroyed/deactivated
        if (spawnedNPC != null && !spawnedNPC.activeInHierarchy)
        {
            spawnedNPC = null;
        }
    }

    public void SpawnNPC()
    {
        if (npcPrefab == null)
        {
            Debug.LogError("NPCManager: NPC Prefab is not assigned!");
            return;
        }

        if (spawnPoint == null || waitingPoint == null || randomDestinations.Length == 0)
        {
            Debug.LogError("NPCManager: Please assign spawn point, waiting point, and random destinations!");
            return;
        }

        // Don't spawn if one already exists and is active
        if (spawnedNPC != null && spawnedNPC.activeInHierarchy)
        {
            Debug.LogWarning("NPCManager: An NPC is already active!");
            return;
        }

        // Clear reference if NPC was deactivated
        if (spawnedNPC != null && !spawnedNPC.activeInHierarchy)
        {
            spawnedNPC = null;
        }

        // Instantiate the NPC
        spawnedNPC = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);

        // Get the NPC component and assign references
        NPC npcScript = spawnedNPC.GetComponent<NPC>();
        if (npcScript != null)
        {
            npcScript.Initialize(spawnPoint, waitingPoint, randomDestinations, player);
        }
        else
        {
            Debug.LogError("NPCManager: NPC prefab doesn't have NPC component!");
            Destroy(spawnedNPC);
        }
    }

    public void DespawnNPC()
    {
        if (spawnedNPC != null)
        {
            Destroy(spawnedNPC);
            spawnedNPC = null;
        }
    }
}

