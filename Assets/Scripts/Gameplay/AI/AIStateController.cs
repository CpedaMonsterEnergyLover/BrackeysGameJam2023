﻿using System;
using System.Collections;
using Definitions;
using Gameplay.AI.Locators;
using Gameplay.Enemies;
using Gameplay.Food;
using Pathfinding;
using UnityEngine;

namespace Gameplay.AI
{
    [RequireComponent(typeof(AIPath)),
     RequireComponent(typeof(AIDestinationSetter)),
     RequireComponent(typeof(Enemy))]
    public class AIStateController : MonoBehaviour
    {
        [SerializeField] private Locator locator;

        public AIState debug_AIState;
        
        private Enemy enemy;
        private CallbackableAIPath aiPath;
        private AIDestinationSetter destinationSetter;

        private AIState CurrentState { get; set; }
        private Transform currentFollowTarget;
        

        
        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            aiPath = GetComponent<CallbackableAIPath>();
            destinationSetter = GetComponent<AIDestinationSetter>();

            locator.OnTargetLocated += OnLocatorTriggered;
            aiPath.enabled = false;
            destinationSetter.enabled = false;
        }

        private IEnumerator Start()
        {            
            yield return new WaitUntil(() => EnemySpawnLocation.InitializedLocationsAmount == EnemySpawner.SpawnLocationsCount);
            SetState(debug_AIState);
            SetMovementSpeed(enemy.Scriptable.MovementSpeed);
        }

        public void SetState(
            AIState newState, 
            GameObject followTarget = null, 
            Action<GameObject> onTargetReach = null)
        {
            if(newState == CurrentState) return;
            
            CancelPath();
            
            switch (newState)
            {
                case AIState.Enter:
                    SetEnter();
                    break;
                case AIState.Exit:
                    SetExit();
                    break;
                case AIState.Wander:
                    SetWander();
                    break;
                case AIState.Follow:
                    SetFollow(followTarget, onTargetReach);
                    break;
                case AIState.None:
                    SetNone();
                    break;
                case AIState.Flee:
                    SetFlee();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
            
            CurrentState = newState;
        }

        private void SetEnter()
        {
            DisableLocator();
            DoNotRepath();
            destinationSetter.enabled = false;
            aiPath.enabled = true;
            aiPath.SetPath(enemy.SpawnLocation.EnteringPath);
            aiPath.Callback = () =>
            {
                aiPath.Callback = null;
                enemy.OnMapEntered();
            };
        }

        private void SetExit()
        {
            DisableLocator();
            DoNotRepath();
            destinationSetter.enabled = false;
            aiPath.enabled = true;
            aiPath.destination = enemy.SpawnLocation.SpawnPosition;
            aiPath.SearchPath();
            aiPath.Callback = () =>
            {
                Destroy(gameObject);
                aiPath.Callback = null;
            };
        }

        private void SetFollow(GameObject targetGO, Action<GameObject> onTargetReach)
        {
            Transform target = targetGO is null ? Player.Movement.Transform : targetGO.transform;
            DisableLocator();
            AutoRepath();
            destinationSetter.enabled = true;
            aiPath.enabled = true;
            destinationSetter.target = target;
            if (onTargetReach is not null)
                aiPath.Callback = () =>
                {
                    onTargetReach(targetGO);
                    aiPath.Callback = null;
                };
        }

        private void SetNone()
        {
            DisableLocator();
            DoNotRepath();
            destinationSetter.enabled = false;
            aiPath.enabled = false;
        }
        
        private void SetWander()
        {
            EnableLocator();
            AutoRepath();
            destinationSetter.enabled = false;
            aiPath.enabled = true;
            SetMovementSpeed(enemy.Scriptable.MovementSpeed * GlobalDefinitions.WanderingSpeedMultiplier);
            PickRandomDestination();
            aiPath.Callback = PickRandomDestination;
        }

        private void SetFlee()
        {
            SetMovementSpeed(enemy.Scriptable.MovementSpeed * GlobalDefinitions.FleeingSpeedMultiplier);
            SetExit();
        }


        

        // Utility
        private void PickRandomDestination()
        {
            var startNode = AstarPath.active.GetNearest(enemy.Position).node;
            var nodes = PathUtilities.BFS(startNode, enemy.Scriptable.WanderingRadius);
            if(nodes.Count == 0)
                return;
            var randomPoint = PathUtilities.GetPointsOnNodes(nodes, 1)[0];
            aiPath.destination = randomPoint;
            aiPath.SearchPath();
        }
        
        private void DoNotRepath() => aiPath.autoRepath.mode = AutoRepathPolicy.Mode.Never;
        private void AutoRepath() => aiPath.autoRepath.mode = AutoRepathPolicy.Mode.Dynamic;
        private void SetMovementSpeed(float speed) => aiPath.maxSpeed = speed;
        
        private void CancelPath()
        {
            aiPath.Callback = null;
            aiPath.SetPath(null);
        }

        private void OnLocatorTriggered(ILocatorTarget target)
        {
            switch (target)
            {
                case EggBed eggBed:
                    enemy.OnEggsLocated(eggBed);
                    break;
                case FoodBed foodBed:
                    enemy.OnFoodLocated(foodBed);
                    break;
                default:
                    enemy.OnPlayerLocated();
                    break;
            }
        }
        
        private void DisableLocator() => locator.gameObject.SetActive(false);

        private void EnableLocator() => locator.gameObject.SetActive(true);

        private void OnDestroy() => locator.OnTargetLocated -= OnLocatorTriggered;
    }
}