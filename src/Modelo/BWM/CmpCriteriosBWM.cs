using System;
using System.Collections.Generic;

/*
 * Almacena datos acerca de la comparación de un criterio con respecto a otro en el método BWM
 */
public class CmpCriteriosBWM {
	public Criterio criterio1;
	public Criterio criterio2;
	/*
	 * Valor entre 1 y 9 que representa cuánto de importante de más es el criterio 1 con respecto al 2
	 * (1 = Ambos son igual de importante, 9 = El criterio 1 es muchísimo más importante que el 2)
	 */
	public int valor;

	public CmpCriteriosBWM(Criterio criterio1, Criterio criterio2, int valor) {
		this.criterio1 = criterio1;
		this.criterio2 = criterio2;
		this.valor = valor;

		if (valor < 1 || valor > 9) {
			throw new ArgumentException("El valor de comparación entre los criterios \"" + criterio1.id + "\" y \"" + criterio2.id +
				"\" especificado (" + valor + ") es inválido. Debe estar entre 1 y 9.");
		}
	}
}