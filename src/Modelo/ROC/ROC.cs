using System;
using System.Xml.Linq;
using System.Collections.Generic;

/*
 * Clase que contiene los datos y cálculos necesarios para ejecutar el método ROC sobre una serie de criterios que se encuentren
 * en el mismo nivel de la jerarquía.
 */
public class ROC {
	private Global G;

	/*
	 * Lista de criterios sobre los que trabajará esta instancia. Todos los criterios leídos de los datos ROC del XML deben estar
	 * en esta lista.
	 */
	private ListaCriterios criterios;
	// Lista de todos los criterios leídos del XML, ordenados de más a menos importante
	private List<Criterio> criteriosXml;

	// Criterio raíz de la familia de criterios con los que está trabajando la instancia
	private Criterio raízFamilia;

	/*
	 * criterios: Lista de criterios a los que se van a asignar pesos mediante el método ROC. Deben estar en el mismo nivel de
	 * la jerarquía.
	 */
	public ROC(ListaCriterios criterios) {
		G = Global.getInstancia();
		this.criterios = criterios;
		criteriosXml = new List<Criterio>();

		// Validar prequisito de mismo nivel
		if (!criterios.mismoNivel()) {
			throw new ArgumentException("Todos los criterios pasados a una instancia de ROC deben tener el mismo padre");
		}
	}

	/*
	 * Carga datos que contienen los valores del método asignados por el decisor a los diferentes criterios y los usa
	 * para ejecutar el algoritmo ROC y obtener los pesos de los criterios.
	 * nodoFamilia: Nodo XML que contiene los datos ROC de la familia de criterios cargados en esta clase.
	 */
	public void ejecutar(XElement nodoFamilia) {
		construirListaCriteriosXml(nodoFamilia);
		comprobarFamiliaCompleta();
		calcularPesos();
	}
	
	private void construirListaCriteriosXml(XElement nodoFamilia) {
		string raízStr = nodoFamilia.Element("raíz").Value;
		if (raízStr == "") {
			raízFamilia = null;
		} else {
			raízFamilia = G.listaCriterios.getCriterio(raízStr);
		}
		
		IEnumerable<XElement> elementosXml = nodoFamilia.Element("criterios").Elements();
		foreach (XElement nodo in elementosXml) {
			Criterio criterio = procesarNodoCriterio(nodo, raízFamilia);
			criteriosXml.Add(criterio);
		}
	}
	
	/*
	 * Procesa el nodo especificado, asumiendo que contiene datos de un criterio
	 * nodoCriterio: El nodo XML que contiene los datos del criterio a procesar
	 * raízFamilia: Criterio raíz de la familia actual. El criterio a procesar debe tener este como padre.
	 */
	private Criterio procesarNodoCriterio(XElement nodoCriterio, Criterio raízFamilia) {
		String idCriterio = nodoCriterio.Attribute("id").Value;
		Criterio criterio = criterios.getCriterio(idCriterio);
		if (criterio != null) {
			if (criterio.padre == raízFamilia) {
				return criterio;
			} else {
				throw new IllegalOperationException("El criterio \"" + idCriterio + "\" especificado en los datos ROC no " +
					"pertenece a la familia en la que está colocado (indicada: " + raízFamilia.id + ", real: " +
					criterio.getNombreFamilia() + ")");
			}
		} else {
			throw new IllegalOperationException("El criterio \"" + idCriterio + "\" especificado en los datos ROC no " +
				"está registrado");
		}
	}
	
	/*
	 * Se asegura de que todos los criterios de esta familia han sido listados en los datos ROC
	 */
	private void comprobarFamiliaCompleta() {
		foreach (Criterio criterio in criterios) {
			if (!contieneCriterio(criterio.id)) {
				throw new IllegalStateException("El criterio " + criterio.id + " pertenece a la familia " + criterio.getNombreFamilia() +
					" pero no está incluido en los datos ROC para dicha familia");
			}
		}
	}
	
	/*
	 * Calcula los pesos de todos los criterios almacenados
	 */
	private void calcularPesos() {
		for (int i = 0; i < criteriosXml.Count; i++) {
			Criterio criterio = criteriosXml[i];
			float peso = 0;
			for (int j = i + 1; j <= criteriosXml.Count; j++) {
				peso += 1f/j;
			}
			peso /= criteriosXml.Count;
			criterio.pesosLocales.setPeso(Método.ROC, peso);
		}
	}
	
	/*
	 * Return: True si el criterio identificado con el ID especificado está incluido en los datos ROC leídos del XML
	 */
	private bool contieneCriterio(string idCriterio) {
		foreach (Criterio criterio in criteriosXml) {
			if (criterio.id == idCriterio) {
				return true;
			}
		}
		return false;
	}
}