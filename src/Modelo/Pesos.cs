using System;
using System.Xml.Linq;
using System.Collections.Generic;

/*
 * Clase usada para almacenar varios pesos asociados a un criterio según el método empleado para calcularlos
 */
public class Pesos {
	private Dictionary<Método, float> pesos;

	public Pesos() {
		pesos = new Dictionary<Método, float>();
	}

	/*
	 * Devuelve el peso asociado al método indicado, o -1 si no existe.
	 */
	public float getPeso(Método método) {
		float salida;
		if (pesos.TryGetValue(método, out salida)) {
			return salida;
		} else {
			return -1;
		}
	}

	public void setPeso(Método método, float valor) {
		pesos.Add(método, valor);
	}

	/*
	 * Obtiene la media aritmética de todos los pesos almacenados en la instancia
	 */
	public float getPesoMedio() {
		float suma = 0;
		foreach (float peso in pesos.Values) {
			suma += peso;
		}
		return suma / pesos.Count;
	}

	public string toString() {
		string res = "";
		bool primero = true;

		foreach (KeyValuePair<Método, float> entrada in pesos) {
			if (primero) {
				primero = false;
			} else {
				res += ", ";
			}

			res += Enum.GetName(typeof(Método), entrada.Key) + ": " + entrada.Value;
		}

		if (pesos.Keys.Count > 1) {
			// Añadir el valor medio
			res += ", media: " + getPesoMedio();
		} else {
			// No hay datos o solo los hay de un método, no hacer nada más.
		}

		return res;
	}

	/*
	 * Convierte este peso a una representación en XML
	 * nombreNodo: Nombre del nodo resultante
	 */
	public XElement toNodoXML(string nombreNodo) {
		XElement nodoPeso = new XElement(nombreNodo);

		XElement nodoMétodos = new XElement("métodos");

		foreach (KeyValuePair<Método, float> entrada in pesos) {
			nodoMétodos.Add(getNodoMétodo(entrada.Key.ToString().ToLower(), entrada.Value));
		}
		if (pesos.Keys.Count > 1) {
			nodoMétodos.Add(getNodoMétodo("media", getPesoMedio()));
		}

		nodoPeso.Add(nodoMétodos);
		return nodoPeso;
	}

	private XElement getNodoMétodo(string idMétodo, float peso) {
		XElement nodoMétodo = new XElement("método");
			
		XElement nodoIdMétodo = new XElement("idMétodo");
		nodoIdMétodo.Value = idMétodo;
		XElement nodoValorMétodo = new XElement("valor");
		nodoValorMétodo.Value = peso.ToString();

		nodoMétodo.Add(nodoIdMétodo);
		nodoMétodo.Add(nodoValorMétodo);
		return nodoMétodo;
	}
}