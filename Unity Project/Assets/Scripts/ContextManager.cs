﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextManager : MonoBehaviour {

    public Microscope Microscope;
    public GenomeEditor GenomeEditor;

    // Use this for initialization
    void Start () {
        // Tenta encontrar se não foi atribuído no Inspector
        // Nota: FindObjectsByType com includeInactive encontra objetos desativados
        if (Microscope == null)
        {
            Microscope = FindAnyObjectByType<Microscope>(FindObjectsInactive.Include);
        }
        
        if (GenomeEditor == null)
        {
            GenomeEditor = FindAnyObjectByType<GenomeEditor>(FindObjectsInactive.Include);
        }
        
        if (Microscope == null)
            Debug.LogWarning("[ContextManager] Microscope não encontrado! Atribua no Inspector.");
        if (GenomeEditor == null)
            Debug.LogWarning("[ContextManager] GenomeEditor não encontrado! Atribua no Inspector.");
    }
	
	// Update is called once per frame
	void Update () {
        if (Microscope == null || GenomeEditor == null) return;
        
		if (Input.GetKeyDown(KeyCode.G) && !GenomeEditor.Active)
        {
            Microscope.Active = false;
            GenomeEditor.Active = true;
        }
        else if (Input.GetKeyDown(KeyCode.M) && !Microscope.Active)
        {
            Microscope.Active = true;
            GenomeEditor.Active = false;
            Microscope.CurrentGenome = GenomeEditor.CurrentGenome.Clone();
        }
	}
}
