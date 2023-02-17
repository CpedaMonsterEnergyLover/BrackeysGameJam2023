﻿using Gameplay.Genetics;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GeneDisplay : MonoBehaviour
    {
        [SerializeField] private Text[] geneTexts = new Text[3];
        
        public void UpdateGeneText(TrioGene trio, GeneType geneType) => geneTexts[(int) geneType].text = trio.GetGene(geneType).ToString();
        private void UpdateGeneText(TrioGene trio, int geneType) => geneTexts[geneType].text = trio.GetGene(geneType).ToString();

        public void UpdateTrioAsMedian(TrioGene trio)
        {
            geneTexts[0].text = $"~{trio.GetGene(0)}";
            geneTexts[1].text = $"~{trio.GetGene(1)}";
            geneTexts[2].text = $"~{trio.GetGene(2)}";
        }

        public void UpdateTrioText(TrioGene trio)
        {
            UpdateGeneText(trio, 0);
            UpdateGeneText(trio, 1);
            UpdateGeneText(trio, 2);
        }
    }
}