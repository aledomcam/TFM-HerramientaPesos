
/*
 * Clase abstracta que representa un elemento en una lista ordenada de elementos usados en el método de Sismos. Puede ser un
 * elemento blanco o un conjunto de uno o más criterios de igual importancia.
 */
public abstract class ElementoSismos {
	// Nivel que indica la importancia del elemento, empezando en 1 (menos importante)
	public int nivel;
	
	public ElementoSismos(int nivel) {
		this.nivel = nivel;
	}
}