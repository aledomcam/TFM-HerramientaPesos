using System;
using System.Xml.Linq;
using System.Collections.Generic;

/*
 * Clase que contiene los datos y cálculos necesarios para ejecutar las comprobaciones de consistencia del método BWM sobre una serie
 * de criterios que se encuentren en el mismo nivel de la jerarquía.
 */
public class BWM {
	private Global G;

	// Criterios más y menos importantes en esta familia
	public Criterio mejor, peor;
	/*
	 * Lista de criterios sobre los que trabajará esta instancia. Todos los criterios leídos de los datos BWM del XML deben estar
	 * en esta lista.
	 */
	private ListaCriterios criterios;
	// Nodo XML con los datos BWM de la familia con la que va a trabajar esta instancia
	private XElement nodoFamilia;

	/*
	 * Contiene los arrays de comparación entre criterios
	 */
	public ListaCmpCriteriosBWM comparaciones;

	// Criterio raíz de la familia de criterios con los que está trabajando la instancia
	private Criterio raízFamilia;

	/*
	 * criterios: Lista de criterios para los que se va a calcular la consistencia mediante el método BW. Deben estar en el mismo nivel
	 * de la jerarquía.
	 * nodoFamilia: Nodo XML que contiene los datos BWM de la familia de criterios sobre la que se va a trabajar.
	 */
	public BWM(ListaCriterios criterios, XElement nodoFamilia) {
		G = Global.getInstancia();
		mejor = null;
		peor = null;
		comparaciones = null;
		this.criterios = criterios;
		this.nodoFamilia = nodoFamilia;

		// Validar prequisito de mismo nivel
		if (!criterios.mismoNivel()) {
			throw new ArgumentException("Todos los criterios pasados a una instancia de BWM deben tener el mismo padre");
		}

		cargar();
	}

	/*
	 * Calcula la consistencia de los valores especificados en los datos BWM
	 */
	public ConsistenciasBWM calcularConsistencia() {
		ConsistenciasBWM ret = new ConsistenciasBWM(criterios.count());

		foreach (Criterio criterio in criterios) {
			ret.añadir(new ConsistenciaBWM(criterio, criterios, comparaciones));
		}
		return ret;
	}

	/*
	 * Carga los datos del nodo XML indicado, rellenando las listas de comparaciones entre criterios.
	 */
	private void cargar() {
		string raízStr = nodoFamilia.Element("raíz").Value;
		if (raízStr == "") {
			raízFamilia = null;
		} else {
			raízFamilia = G.listaCriterios.getCriterio(raízStr);
		}

		string mejorStr = nodoFamilia.Element("mejorCriterio").Value;
		string peorStr = nodoFamilia.Element("peorCriterio").Value;
		if (criterios.getCriterio(mejorStr) == null) {
			throw new ArgumentException("El criterio indicado como mejor criterio no existe");
		} else if (criterios.getCriterio(peorStr) == null) {
			throw new ArgumentException("El criterio indicado como mejor criterio no existe");
		}
		
		mejor = G.listaCriterios.getCriterio(mejorStr);
		peor = G.listaCriterios.getCriterio(peorStr);
		comparaciones = new ListaCmpCriteriosBWM(mejor, peor);

		// Recorrer comparaciones entre el mejor criterio y el resto

		// Lista usada para confirmar que están todos los criterios, sin que falte o sobre ninguno.
		List<string> IDs = criterios.listaIDs();

		XElement nodoCmpMejorResto = nodoFamilia.Element("comparacionesMejorAResto");
		foreach (XElement nodoCmp in nodoCmpMejorResto.Elements("mejorAResto")) {
			string id = nodoCmp.Element("id").Value;
			int valor = int.Parse(nodoCmp.Element("valor").Value);

			if (validarCriterio(id, valor, IDs)) {
				Criterio criterio = criterios.getCriterio(id);
				IDs.Remove(id);
				comparaciones.añadirMejorAResto(new CmpCriteriosBWM(mejor, criterio, valor));
			}
		}

		// Añadir una entrada para el mejor criterio
		if (!IDs.Contains(mejor.id)) {
			// El mejor criterio también tenía un valor de comparación en el XML. Esto es innecesario.
			Console.Out.WriteLine("Aviso: No es necesario especificar el resultado de la comparación entre el mejor criterio y sí mismo " +
				"en el método BWM. El valor se ignorará y se usará un valor de 1 en su lugar.");
		}
		comparaciones.añadirMejorAResto(new CmpCriteriosBWM(mejor, mejor, 1));
		IDs.Remove(mejor.id);

		// Comprobar que no falta ninguno
		if (IDs.Count > 0) {
			throw new IllegalOperationException("BWM: No se han especificado los resultados de todas las comparaciones entre el mejor " +
				"criterio y el resto en la familia " + raízFamilia.id + "(faltan " + IDs.Count + ")");
		}

		// Recorrer comparaciones entre el peor criterio y el resto
		
		IDs = criterios.listaIDs();

		XElement nodoCmpRestoAPeor = nodoFamilia.Element("comparacionesRestoAPeor");
		foreach (XElement nodoCmp in nodoCmpRestoAPeor.Elements("restoAPeor")) {
			string id = nodoCmp.Element("id").Value;
			int valor = int.Parse(nodoCmp.Element("valor").Value);

			if (validarCriterio(id, valor, IDs)) {
				Criterio criterio = criterios.getCriterio(id);
				IDs.Remove(id);
				comparaciones.añadirRestoAPeor(new CmpCriteriosBWM(criterio, peor, valor));
			}
		}

		// Añadir una entrada para el peor criterio
		if (!IDs.Contains(peor.id)) {
			// El peor criterio también tenía un valor de comparación en el XML. Esto es innecesario.
			Console.Out.WriteLine("Aviso: No es necesario especificar el resultado de la comparación entre el peor criterio y sí mismo " +
				"en el método BWM. El valor se ignorará y se usará un valor de 1 en su lugar.");
		}
		comparaciones.añadirRestoAPeor(new CmpCriteriosBWM(peor, peor, 1));
		IDs.Remove(peor.id);
		
		/*
		 * La comparación del mejor al peor criterio ya fue añadida en las comparaciones del mejor al resto, así que la insertamos
		 * en las comparaciones de resto a peor con el mismo valor. No hace falta especificarla dos veces.
		 */
		if (!IDs.Contains(mejor.id) && mejor.id != peor.id) {
			Console.Out.WriteLine("Aviso: No es necesario especificar el resultado de la comparación entre el mejor y el peor criterio " +
				"otra vez en las comparaciones del resto al peor en el método BWM porque ya fue especificado en las comparaciones " +
				"del mejor al resto. El valor se ignorará y se usará el especificado la primera vez.");
		}
		comparaciones.añadirRestoAPeor(new CmpCriteriosBWM(mejor, peor, comparaciones.getMejorAEste(peor).valor));
		IDs.Remove(mejor.id);

		// Comprobar que no falta ninguno
		if (IDs.Count > 0) {
			throw new IllegalOperationException("BWM: No se han especificado los resultados de todas las comparaciones entre otros " +
				"criterios de la familia " + raízFamilia.id + " y el peor (faltan " + IDs.Count + ")");
		}
	}

	/*
	 * Comprueba que los datos de un criterio leídos del XML son válidos y lanza una excepción si no es el caso.
	 * IDs: Lista de IDs de todos los criterios del nivel actual que aún no han sido procesados.
	 * return: True si los datos son válidos
	 */
	private bool validarCriterio(string id, int valor, List<string> IDs) {
		if (IDs.Contains(id)) {
			Criterio criterio = criterios.getCriterio(id);
			if (criterio != null) {
				if (criterio.padre == raízFamilia) {
					return true;
				} else {
					throw new IllegalOperationException("El criterio \"" + id + "\" especificado en los datos BWM no " +
						"pertenece a la familia en la que está colocado (indicada: " + raízFamilia.id + ", real: " +
						criterio.getNombreFamilia() + ")");
				}
			} else {
				throw new IllegalOperationException("El criterio \"" + id + "\" especificado en los datos BWM no " +
					"está registrado");
			}
		} else {
			throw new IllegalOperationException("El id \"" + id + "\", especificado en los datos BWM, no existe en la lista de nodos " +
				"de la familia actual");
		}
	}
}