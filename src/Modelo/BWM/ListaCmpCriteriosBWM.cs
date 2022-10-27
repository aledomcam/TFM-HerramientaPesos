using System;
using System.Collections.Generic;

/*
 * Lista que contiene comparaciones entre criterios BWM
 */
public class ListaCmpCriteriosBWM {
	private List<CmpCriteriosBWM> mejorAResto;
	private List<CmpCriteriosBWM> restoAPeor;

	private Criterio mejor, peor;

	public ListaCmpCriteriosBWM(Criterio mejor, Criterio peor) {
		mejorAResto = new List<CmpCriteriosBWM>();
		restoAPeor = new List<CmpCriteriosBWM>();

		this.mejor = mejor;
		this.peor = peor;
	}

	public void añadirMejorAResto(CmpCriteriosBWM cmp) {
		eliminarExistentesMejorAResto(cmp.criterio1, cmp.criterio2);
		mejorAResto.Add(cmp);
	}

	public void añadirRestoAPeor(CmpCriteriosBWM cmp) {
		eliminarExistentesRestoAPeor(cmp.criterio1, cmp.criterio2);
		restoAPeor.Add(cmp);
	}

	public CmpCriteriosBWM getMejorAEste(Criterio criterio) {
		foreach (CmpCriteriosBWM cmp in mejorAResto) {
			if (cmp.criterio2 == criterio) {
				return cmp;
			}
		}
		return null;
	}

	public CmpCriteriosBWM getEsteAPeor(Criterio criterio) {
		foreach (CmpCriteriosBWM cmp in restoAPeor) {
			if (cmp.criterio1 == criterio) {
				return cmp;
			}
		}
		return null;
	}

	public CmpCriteriosBWM getMejorAPeor() {
		foreach (CmpCriteriosBWM cmp in mejorAResto) {
			if (cmp.criterio2 == peor) {
				return cmp;
			}
		}
		return null;
	}
	
	private void eliminarExistentesMejorAResto(Criterio criterio1, Criterio criterio2) {
		int i = 0;
		while (i < mejorAResto.Count) {
			CmpCriteriosBWM elemento = mejorAResto[i];
			if (elemento.criterio1 == criterio1 && elemento.criterio2 == criterio2) {
				mejorAResto.RemoveAt(i);
			} else {
				i++;
			}
		}
	}
	
	private void eliminarExistentesRestoAPeor(Criterio criterio1, Criterio criterio2) {
		int i = 0;
		while (i < restoAPeor.Count) {
			CmpCriteriosBWM elemento = restoAPeor[i];
			if (elemento.criterio1 == criterio1 && elemento.criterio2 == criterio2) {
				restoAPeor.RemoveAt(i);
			} else {
				i++;
			}
		}
	}
}