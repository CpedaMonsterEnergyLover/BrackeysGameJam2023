﻿using System.Collections.Generic;
using Definitions;
using GameCycle;
using Gameplay;
using Gameplay.Abilities;
using Genes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private bool showOnStartup;
        [SerializeField] private GameObject rootGO;
        [SerializeField] private GameObject gameUICanvas;
        [SerializeField] private GameObject gameOverGO;
        [SerializeField] private TutorialMenu tutorialMenu;
        [SerializeField] private PauseMenu pauseMenu;
        [FormerlySerializedAs("statsText")] 
        [SerializeField] private Text gameOverStatsText;
        
        public delegate void MainMenuEvent();
        public static event MainMenuEvent OnResetRequested;
        
        private void Start()
        {
            Application.targetFrameRate = 60;
        
            if(showOnStartup) ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            pauseMenu.gameObject.SetActive(false);
            Time.timeScale = 0;
            rootGO.SetActive(true);
            gameUICanvas.SetActive(false);
        }

        public void ShowTutorial()
        {
            rootGO.SetActive(false);
            gameUICanvas.SetActive(true);
            tutorialMenu.Show();
        }

        public void CloseTutorial()
        {
            tutorialMenu.Close();
            gameUICanvas.SetActive(false);
            rootGO.SetActive(true);
        }

        public void ShowGameOver()
        {
            Time.timeScale = 0;
            gameOverStatsText.text = StatRecorder.Print();
            gameOverGO.SetActive(true);
            gameUICanvas.SetActive(false);
        }

        public void CloseGameOver()
        {
            rootGO.SetActive(true);
            gameOverGO.SetActive(false);
        }
        
        public void Play()
        {
            ResetGame();
            rootGO.SetActive(false);
            gameUICanvas.SetActive(true);
            Time.timeScale = 1;
        }

        public void Pause() => pauseMenu.gameObject.SetActive(true);

        public void Restart()
        {
            ResetGame();
            pauseMenu.gameObject.SetActive(false);
        }
        
        private void ResetGame()
        {
            OnResetRequested?.Invoke();
            CreateFirstEggBed();
        }

        private void CreateFirstEggBed()
        {
            var bed = Instantiate(GlobalDefinitions.EggBedPrefab, GlobalDefinitions.GameObjectsTransform);
            int amount = Random.Range(1, 7);
            var eggs = new List<Egg>();
            while (amount > 0)
            {
                Egg egg = new Egg(TrioGene.Zero, new MutationData());
                eggs.Add(egg);
                amount--;
            }

            StatRecorder.eggsLayed += amount;
            bed.SetEggs(eggs);
            bed.transform.position = new Vector3(15, 15, 0);
        }

        public void Exit() => Application.Quit();
    }
}