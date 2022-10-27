using System;
using System.Xml.Linq;
using System.Collections.Generic;

/*
 * Clase que contiene los datos y cálculos necesarios para ejecutar el método LBWA sobre una serie de criterios que se encuentren
 * en el mismo nivel de la jerarquía.
 */
public class LBWA {
	private Global G;

	/*
	 * Lista de criterios sobre los que trabajará esta instancia. Todos los criterios leídos de los datos LBWA del XML deben estar
	 * en esta lista.
	 */
	private ListaCriterios criterios;
	// Lista de todos los criterios junto con sus datos usados para este método
	private List<CriterioLBWA> criteriosLBWA;
	// Lleva la cuenta de cuántos criterios hay en cada nivel
	public int[] elementosPorNivel;

	// Criterio raíz de la familia de criterios con los que está trabajando la instancia
	private Criterio raízFamilia;
	// ID del criterio de mayor importancia en esta familia
	private string idPrincipal;

	/*
	 * criterios: Lista de criterios a los que se van a asignar pesos mediante el método LBWA. Deben estar en el mismo nivel de
	 * la jerarquía.
	 */
	public LBWA(ListaCriterios criterios) {
		G = Global.getInstancia();
		this.criterios = criterios;
		criteriosLBWA = new List<CriterioLBWA>();
		elementosPorNivel = new int[Cst.MAX_NIVELES_LBWA];
		for (int i = 0; i < Cst.MAX_NIVELES_LBWA; i++) {
			elementosPorNivel[i] = 0;
		}

		// Validar prequisito de mismo nivel
		if (!criterios.mismoNivel()) {
			throw new ArgumentException("Todos los criterios pasados a una instancia de LBWA deben tener el mismo padre");
		}
	}

	/*
	 * Carga datos que contienen los valores del método asignados por el decisor a los diferentes criterios y los usa
	 * para ejecutar el algoritmo LBWA y obtener los pesos de los criterios.
	 * nodoFamilia: Nodo XML que contiene los datos LBWA de la familia de criterios cargados en esta clase.
	 */
	public void ejecutar(XElement nodoFamilia) {
		// Primero obtener los datos LBWA del XML e insertar los criterios en la lista

		string raízStr = nodoFamilia.Element("raíz").Value;
		if (raízStr == "") {
			raízFamilia = null;
		} else {
			raízFamilia = G.listaCriterios.getCriterio(raízStr);
		}

		idPrincipal = nodoFamilia.Element("criterioPrincipal").Value;
		if (criterios.getCriterio(idPrincipal) == null) {
			throw new ArgumentException("El criterio principal especificado no existe");
		}

		foreach (XElement nodoNivel in nodoFamilia.Element("niveles").Elements("nivel")) {
			int nivel = int.Parse(nodoNivel.Element("valor").Value);

			foreach (XElement nodoCriterio in nodoNivel.Element("criterios").Elements("criterio")) {
				string idCriterio = nodoCriterio.Element("id").Value;
				Criterio criterio = criterios.getCriterio(idCriterio);
				if (criterio != null) {
					if (criterio.padre == raízFamilia) {
						float importanciaLocalNorm = float.Parse(nodoCriterio.Element("importanciaLocalNorm").Value);
						if (importanciaLocalNorm >= 0 && importanciaLocalNorm <= 1) {
							if (criterio.id != idPrincipal || nivel == 1 && importanciaLocalNorm == 0) {
								CriterioLBWA criterioLBWA = new CriterioLBWA(criterio, nivel, importanciaLocalNorm);
								añadirCriterio(criterioLBWA);
							} else {
								throw new IllegalOperationException("El criterio principal debe estar en el nivel 1 con una importancia " +
									"local normalizada de 0");
							}
						} else {
							throw new IllegalOperationException("La importancia local normalizada debe ser un valor entre 0 y 1. " + 
								"(ID criterio: " + criterio.id + ")");
						}
					} else {
						throw new IllegalOperationException("El criterio \"" + idCriterio + "\" especificado en los datos LBWA no " +
							"pertenece a la familia en la que está colocado (indicada: " + raízStr + ", real: " +
							criterio.getNombreFamilia() + ")");
					}
				} else {
					throw new IllegalOperationException("El criterio \"" + idCriterio + "\" especificado en los datos LBWA no " +
						"está registrado");
				}
			}
		}

		/*
		 * Comprobar que no falta información sobre ningún criterio de esta familia. Para ello iteramos todos los criterios de
		 * la familia (que se supone que son los que se pasaron a esta instancia al crearla) y nos aseguramos de que están en la
		 * lista de criterios LBWA.
		 * Al mismo tiempo vamos fijando los valores de las importancias locales denormalizadas para cada criterio.
		 */
		float importanciaLocalMax = getImportanciaLocalMax();
		foreach (Criterio criterio in criterios) {
			CriterioLBWA criterioLBWA = getCriterio(criterio.id);
			if (criterioLBWA == null) {
				throw new IllegalStateException("El criterio " + criterio.id + " pertenece a la familia " + criterio.getNombreFamilia() +
					" pero no tiene datos LBWA asociados a dicha familia");
			} else {
				criterioLBWA.setImportanciaLocal(importanciaLocalMax);
			}
		}

		// Finalmente, calcular los pesos locales de cada criterio usando el algoritmo LBWA
		float sumaInfluencias = getSumaInfluenciasOtrosCriterios();
		CriterioLBWA principal = getCriterio(idPrincipal);
		principal.setPesoLocalCriterioPrincipal(sumaInfluencias);

		float coeficienteElasticidad = getCoeficienteElasticidad();
		foreach (CriterioLBWA criterioLBWA in criteriosLBWA) {
			if (criterioLBWA.criterio.id != idPrincipal) {
				criterioLBWA.setPesoLocalOtroCriterio(coeficienteElasticidad, principal.criterio.pesosLocales.getPeso(Método.LBWA));
			}
		}
	}

	public void añadirCriterio(CriterioLBWA criterio) {
		criteriosLBWA.Add(criterio);
		elementosPorNivel[criterio.nivel]++;
	}

	public void borrarCriterio(string idCriterio) {
		for (int i = 0; i < criteriosLBWA.Count; i++) {
			CriterioLBWA actual = criteriosLBWA[i];
			if (actual.criterio.id == idCriterio) {
				criteriosLBWA.RemoveAt(i);
				elementosPorNivel[actual.nivel]--;
			}
		}
	}

	public CriterioLBWA getCriterio(string idCriterio) {
		foreach (CriterioLBWA criterio in criteriosLBWA) {
			if (criterio.criterio.id == idCriterio) {
				return criterio;
			}
		}
		return null;
	}

	/*
	 * Devuelve el valor máximo posible para la importancia local.
	 * Si no se ha insertado ningún criterio en la lista aún, devuelve 0.
	 */
	public int getImportanciaLocalMax() {
		int cantidadMax = 0;
		for (int i = 0; i < elementosPorNivel.Length; i++) {
			if (elementosPorNivel[i] > cantidadMax) {
				cantidadMax = elementosPorNivel[i];
			}
		}
		return cantidadMax;
	}

	/*
	 * Devuelve el valor del coeficiente de elasticidad
	 */
	public int getCoeficienteElasticidad() {
		return getImportanciaLocalMax() + G.config.variaciónElasticidad;
	}

	/*
	 * Obtiene la suma de todas las funciones de influencia de todos los criterios de la familia que no sean el principal
	 */
	private float getSumaInfluenciasOtrosCriterios() {
		float coeficienteElasticidad = getCoeficienteElasticidad();
		float suma = 0;
		foreach (CriterioLBWA criterio in criteriosLBWA) {
			if (criterio.criterio.id != idPrincipal) {
				suma += criterio.getInfluencia(coeficienteElasticidad);
			}
		}
		return suma;
	}
}