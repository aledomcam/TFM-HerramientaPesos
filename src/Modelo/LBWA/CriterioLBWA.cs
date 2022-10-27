using System;
using System.Collections.Generic;

/*
 * Almacena un criterio junto con los datos adicionales necesarios para el cálculo de su peso mediante el método LBWA
 */
public class CriterioLBWA {
	public Criterio criterio;

	/*
	 * Nivel en el que se ubica el criterio. Depende de la importancia relativa del criterio con respecto al principal
	 * (el más importante)
	 * 1: El criterio es entre igual de importante y 2 veces menos importante que el criterio principal
	 * 2: El criterio es entre 2 y 3 veces menos importante que el principal
	 * etc
	 */
	public int nivel;
	/*
	 * Nivel de importancia de este criterio con respecto a los que se encuentran en su mismo nivel.
	 * Debe estar entre 0 y el máximo de escala.
	 * Representa el valor de importancia aproximada con respecto al criterio principal dentro del nivel actual.
	 * (Por ejemplo, si estamos en el nivel 2 y el máximo de escala es 5, este criterio se considera 2 + importanciaLocal / 5
	 * veces menos importante que el principal).
	 */
	private float _importanciaLocal;
	public float importanciaLocal {
		get {return _importanciaLocal;}
	}
	// Versión entre 0 y 1 del valor anterior
	public float importanciaLocalNorm;

	public CriterioLBWA(Criterio criterio, int nivel, float importanciaLocalNorm) {
		if (nivel > Cst.MAX_NIVELES_LBWA) {
			throw new ArgumentException("El nivel máximo para LBWA es " + Cst.MAX_NIVELES_LBWA);
		}

		this.criterio = criterio;
		this.nivel = nivel;
		this.importanciaLocalNorm = importanciaLocalNorm;
		_importanciaLocal = -1;
	}

	/*
	 * Fija el valor de importancia local en base al valor normalizado y al valor máximo especificado
	 * importanciaLocalMax: Valor máximo posible de la importancia local para esta familia
	 */
	public void setImportanciaLocal(float importanciaLocalMax) {
		_importanciaLocal = importanciaLocalNorm * importanciaLocalMax;
	}

	/*
	 * Devuelve el valor de la función de influencia del criterio.
	 * Esta función está definida en el método LBWA.
	 */
	public float getInfluencia(float coeficienteElasticidad) {
		return coeficienteElasticidad / (nivel * coeficienteElasticidad + _importanciaLocal);
	}

	/*
	 * Fija el peso local asociado a este criterio por el método LBWA asumiendo que es el criterio principal de la familia.
	 * sumaRestoInfluencias: Resultado de sumar el valor de la función de influencia para todos los demás criterios de la familia
	 */
	public void setPesoLocalCriterioPrincipal(float sumaRestoInfluencias) {
		criterio.pesosLocales.setPeso(Método.LBWA, 1 / (1 + sumaRestoInfluencias));
	}
	/*
	 * Fija el peso local asociado a este criterio por el método LBWA asumiendo que no es el criterio principal de la familia.
	 * pesoLocalPrincipal: Peso local del nodo principal de la familia
	 */
	public void setPesoLocalOtroCriterio(float coeficienteElasticidad, float pesoLocalPrincipal) {
		criterio.pesosLocales.setPeso(Método.LBWA, getInfluencia(coeficienteElasticidad) * pesoLocalPrincipal);
	}
}