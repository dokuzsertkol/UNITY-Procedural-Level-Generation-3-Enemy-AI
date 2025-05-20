using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class navMeshRuntime : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;
    void Start()
    {
        navMeshSurface.BuildNavMesh();
        foreach(var npc in GameObject.FindGameObjectsWithTag("npc"))
        {
            npc.GetComponent<npcbehaviour>().enabled = true;
        }
    }
}
