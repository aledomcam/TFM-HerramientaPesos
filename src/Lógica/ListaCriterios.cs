using System;
using System.Xml.Linq;
using System.Collections.Generic;

/*
 * Almacena una serie de criterios y mantiene la consistencia de la lista
 */
public class ListaCriterios {
	private Global G;

	public List<Criterio> criterios;

	public ListaCriterios(Global G) {
		// No se obtiene con getInstancia() porque al cargarse, G instancia esta clase
		this.G = G;
		criterios = new List<Criterio>();
	}
	public ListaCriterios(Global G, List<Criterio> criterios) {
		this.G = G;
		this.criterios = criterios;
	}

	/*
	 * Devuelve el criterio de la lista de criterios que tenga el id especificado, o null si no existe en la lista.
	 */
	public Criterio getCriterio(string id) {
		foreach (Criterio criterio in criterios) {
			if (criterio.id == id) {
				return criterio;
			}
		}
		return null;
	}

	/*
	 * Devuelve una lista con los criterios que no tienen padre
	 */
	public List<Criterio> getCriteriosRaíz() {
		List<Criterio> ret = new List<Criterio>();

		foreach (Criterio criterio in criterios) {
			if (criterio.padre == null) {
				ret.Add(criterio);
			}
		}

		return ret;
	}

	public void añadirCriterio(Criterio criterio) {
		if (getCriterio(criterio.id) == null) {
			criterios.Add(criterio);
		} else {
			throw new ArgumentException("No se puede insertar un criterio con un id duplicado en la lista de criterios");
		}
	}

	public void borrarCriterio(string idCriterio) {
		for (int i = 0; i < criterios.Count; i++) {
			if (criterios[i].id == idCriterio) {
				criterios.RemoveAt(i);
			}
		}
	}

	/*
	 * Carga los datos de criterios de un fichero XML
	 * rutaFichero: Ruta al fichero XML con los criterios
	 */
	public void cargarCriterios(string rutaFichero) {
		XElement raíz = XElement.Load(rutaFichero);
		foreach (XElement nodoCriterio in raíz.Elements("criterio")) {
			añadirCriterio(cargarCriterio(nodoCriterio, null));
		}
	}

	/*
	 * Fija el peso normalizado de todos los criterios de la lista. Los pesos normalizados de cada nivel de la jerarquía sumarán 1.
	 */
	public void calcularPesosGlobales(Método método) {
		foreach (Criterio criterio in getCriteriosRaíz()) {
			calcularPesoGlobal(criterio, 1, método);
		}
	}

	/*
	 * Devuelve true si todos los criterios almacenados están en el mismo nivel de la jerarquía de criterios (es decir, si
	 * tienen el mismo padre)
	 */
	public bool mismoNivel() {
		Criterio padrePrimerNodo = criterios[0].padre;
		for (int i = 1; i < criterios.Count; i++) {
			if (padrePrimerNodo != criterios[i].padre) {
				return false;
			}
		}
		return true;
	}

	/*
	 * Devuelve una lista que contiene los IDs de todos los criterios almacenados en esta lista de criterios
	 */
	public List<string> listaIDs() {
		List<string> ret = new List<string>();
		foreach (Criterio criterio in criterios) {
			ret.Add(criterio.id);
		}
		return ret;
	}	

	public Criterio this[int i] {
		get {
			return criterios[i];
		}
		set {
			criterios[i] = value;
		}
	}

	public List<Criterio>.Enumerator GetEnumerator() {
		return criterios.GetEnumerator();
	}

	public int count() {
		return criterios.Count;
	}

	private Criterio cargarCriterio(XElement nodoCriterio, Criterio padre) {
		Criterio actual = new Criterio(nodoCriterio.Element("id").Value, nodoCriterio.Element("nombre").Value, padre);
		XElement nodoSubCriterios = nodoCriterio.Element("subcriterios");
		if (nodoSubCriterios != null) {
			foreach (XElement nodoSubCriterio in nodoSubCriterios.Elements("criterio")) {
				Criterio subcriterio = cargarCriterio(nodoSubCriterio, actual);
				// Instertarlo tanto en la lista general como en la lista de subcriterios del actual
				añadirCriterio(subcriterio);
				actual.añadirSubcriterio(subcriterio);
			}
		}
		return actual;
	}

	/*
	 * Fija el peso normalizado del criterio indicado en base al peso de su padre. El valor resultante se usará para fijar el peso
	 * normalizado de todos los subcriterios del actual.
	 */
	private void calcularPesoGlobal(Criterio criterio, float pesoPadre, Método método) {
		float pesoLocal = criterio.pesosLocales.getPeso(método);
		if (pesoLocal == -1) {
			throw new IllegalOperationException("No se pueden calcular el peso global de un criterio cuyo peso local aún no se ha calculado");
		} else if (pesoPadre == -1) {
			throw new IllegalOperationException("No se pueden calcular el pesos global de un criterio si el peso normal de su padre aún no se ha calculado");
		} else {
			criterio.pesosGlobales.setPeso(método, pesoLocal * pesoPadre);
			
			foreach (Criterio subcriterio in criterio.subcriterios) {
				calcularPesoGlobal(subcriterio, criterio.pesosGlobales.getPeso(método), método);
			}
		}
	}
}