// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and despawns the chicken and their fx which are in pools. Managed by gameController.
/// </summary>
public class ChickenManager : MonoBehaviour
{
    public ChickenAgent chickenPrefab;

    public float chickenSpawnMinRadius = 5f;
    public float chickenSpawnMaxRadius = 15f;

    public ParticleSystem explosionPrefab;

    public event System.Action onChickenWhacked = delegate { };

    private GenericPool<ChickenAgent> chickenPool;

    private GenericPool<ParticleSystem> chickenFxPool;

    private float explosionDuration;

    private void Awake()
    {
        chickenFxPool = new GenericPool<ParticleSystem>(explosionPrefab, 5);

        explosionDuration = explosionPrefab.main.duration;

        chickenPool = new GenericPool<ChickenAgent>(chickenPrefab, 0);
    }

    public void SpawnChickens(int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            var chicken = chickenPool.GetObject();
            bool spawned = false;
            do
            {
                spawned = chicken.Spawn(this, Quaternion.AngleAxis(360f * Random.value, Vector3.up) * Vector3.forward * Random.Range(chickenSpawnMinRadius, chickenSpawnMaxRadius));
            }
            while (!spawned);
        }
    }

    public void Clear()
    {
        chickenPool.ClearObjects(x => x.Despawn());
        chickenFxPool.ClearObjects(x => x.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear));
    }

    public void ChickenWhacked()
    {
        onChickenWhacked();
    }

	public void PlayWhackEffect(Vector3 position)
	{
		var explosion = chickenFxPool.GetObject(explosionDuration);
		explosion.transform.position = position;
		explosion.Play();
	}
}
