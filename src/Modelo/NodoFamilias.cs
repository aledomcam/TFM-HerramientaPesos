using System;
using System.Xml.Linq;
using System.Collections.Generic;

/*
 * Contiene información acerca de un nodo XML que tiene datos acerca de diferentes familias de criterios
 */
public class NodoFamilias {
	private XElement nodo;

	/*
	 * Nodo: Nodo XML que contiene un nodo hijo por cada familia
	 */
	public NodoFamilias(XElement nodo) {
		this.nodo = nodo;
	}

	/*
	 * Devuelve el nodo XML que contiene los datos de la familia indicada, o null si no existe
	 */
	public XElement getNodoFamilia(string id) {
		foreach (XElement nodoFamilia in nodo.Elements("familia")) {
			if (nodoFamilia.Element("raíz").Value == id) {
				return nodoFamilia;
			}
		}
		return null;
	}
}