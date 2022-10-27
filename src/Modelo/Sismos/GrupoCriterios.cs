using System;
using System.Collections.Generic;

/*
 * Representa un grupo de criterios que tienen la misma importancia en el método de Sismos
 */
public class GrupoCriterios : ElementoSismos {
	// Lista de criterios incluidos en este grupo
	public List<Criterio> criterios;
	
	public GrupoCriterios(List<Criterio> criterios, int nivel) : base(nivel) {
		this.criterios = criterios;
	}
	
	public GrupoCriterios(Criterio criterio, int nivel) : base(nivel) {
		this.criterios = new List<Criterio>();
		criterios.Add(criterio);
	}
}