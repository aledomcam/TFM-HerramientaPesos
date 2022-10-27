using System;
using System.Xml.Linq;
using System.Collections.Generic;

/*
 * Representa un criterio de decisión que tiene asociado un cierto peso.
 */
public class Criterio {
	// Identificador del criterio. Debe ser único.
	public string id;
	public string nombre;
	// Peso del criterio con respecto a sus hermanos (dividido según el método usado para calcularlo). -1 si no se ha inicializado.
	public Pesos pesosLocales;
	/*
	 * Peso global del criterio teniendo en cuenta todos los demás pesos en la jerarquía (dividido según el método usado para calcularlo).
	 * -1 si no se ha inicializado.
	 */
	public Pesos pesosGlobales;
	// Si el criterio tiene un padre en la jerarquía, se almacena aquí
	public Criterio padre;
	// Subcriterios de este criterio
	public List<Criterio> subcriterios;

	public Criterio(string id, string nombre, Criterio padre = null) {
		this.id = id;
		this.nombre = nombre;
		pesosLocales = new Pesos();
		pesosGlobales = new Pesos();
		this.padre = padre;
		subcriterios = new List<Criterio>();
	}

	public void añadirSubcriterio(Criterio criterio) {
		subcriterios.Add(criterio);
	}

	/*
	 * Devuelve una representación textual de este criterio y sus pesos
	 * globales: True para mostrar los pesos globales, false para los loales
	 * incluirSubcriterios: Si se especifica, se incluye también la salida de este método para los subcriterios de este criterio,
	 * indentada un nivel más.
	 * indentación: Número de tabuladores a insertar al inicio de la string
	 */
	public string toStringPesos(bool globales, bool incluirSubcriterios, int indentación = 0) {
		string ret = "";
		for (int i = 0; i < indentación; i++) {
			ret += "\t";
		}
		
		string pesosStr = globales ? pesosGlobales.toString() : pesosLocales.toString();
		ret += id + " - " + nombre + " [" + pesosStr + "]";
		if (incluirSubcriterios) {
			foreach (Criterio criterio in subcriterios) {
				ret += "\n" + criterio.toStringPesos(globales, true, indentación + 1);
			}
		}
		return ret;
	}

	/*
	 * Convierte el criterio a una representación XML en la que se incluyen sus pesos
	 */
	public XElement toNodoXML() {
		XElement nodo = new XElement("criterio");

		XElement nodoId = new XElement("id");
		nodoId.Value = id;
		nodo.Add(nodoId);

		XElement nodoPesoLocal = pesosLocales.toNodoXML("pesoLocal");
		XElement nodoPesoGlobal = pesosGlobales.toNodoXML("pesoGlobal");
		nodo.Add(nodoPesoLocal);
		nodo.Add(nodoPesoGlobal);
		return nodo;
	}

	/*
	 * Devuelve el nombre de la familia a la que pertenece el criterio, que está identificada por el nombre de su criterio padre
	 */
	public string getNombreFamilia() {
		if (padre == null) {
			return Cst.NOMBRE_FAMILIA_RAÍZ;
		} else {
			return padre.id;
		}
	}

	public override bool Equals(object obj) {
		if (obj == null || GetType() != obj.GetType()) {
			return false;
		}

		Criterio otro = (Criterio) obj;
		
		return this.id == otro.id;
	}
	
	public override int GetHashCode() {
		return id.GetHashCode();
	}
}