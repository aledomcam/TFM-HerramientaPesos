using System;
using System.Xml.Linq;
using System.Collections.Generic;

/*
 * Clase que contiene los datos y cálculos necesarios para ejecutar el método de Sismos sobre una serie de criterios que se encuentren
 * en el mismo nivel de la jerarquía.
 */
public class Sismos {
	private Global G;

	/*
	 * Lista de criterios sobre los que trabajará esta instancia. Todos los criterios leídos de los datos Sismos del XML deben estar
	 * en esta lista.
	 */
	private ListaCriterios criterios;
	// Lista de todos los elementos (grupos de criterios y blancos) en la familia
	private List<ElementoSismos> elementos;
	// Número de elementos en cada nivel
	private int[] elementosPorNivel;

	// Criterio raíz de la familia de criterios con los que está trabajando la instancia
	private Criterio raízFamilia;
	
	/*
	 * Ratio de importancia entre el criterio más importante de la familia y el menos importante.
	 * Este es el valor "z" en el artículo que explica el método.
	 */
	private float ratioMenorMayor;
	/*
	 * numBlancosMasUno[n]: 1 + número de blancos entre el nivel n y el n+1. numBlancosMasUno[0] = 0.
	 * Este es el vector e_r en el artículo que explica el método.
	 */
	private List<int> numBlancosMasUno;
	/*
	 * Suma de todos los valores del array numBlancos a partir del primero, pero con cada uno incrementado una unidad.
	 * Este es el valor "e" en el artículo que explica el método.
	 */
	private int sumaBlancos;
	// Valor "u" en el artículo que explica el método
	private float valorU;

	/*
	 * criterios: Lista de criterios a los que se van a asignar pesos mediante el método de Sismos. Deben estar en el mismo nivel de
	 * la jerarquía.
	 */
	public Sismos(ListaCriterios criterios) {
		G = Global.getInstancia();
		this.criterios = criterios;
		elementos = new List<ElementoSismos>();
		numBlancosMasUno = new List<int>();

		// Validar prequisito de mismo nivel
		if (!criterios.mismoNivel()) {
			throw new ArgumentException("Todos los criterios pasados a una instancia de Sismos deben tener el mismo padre");
		}
	}

	/*
	 * Carga datos que contienen los valores del método asignados por el decisor a los diferentes criterios y los usa
	 * para ejecutar el algoritmo de Sismos y obtener los pesos de los criterios.
	 * nodoFamilia: Nodo XML que contiene los datos Sismos de la familia de criterios cargados en esta clase.
	 */
	public void ejecutar(XElement nodoFamilia) {
		construirListaElementos(nodoFamilia);
		comprobarFamiliaCompleta();
		calcularNumBlancos();
		
		// Calcular la "suma" de todos los blancos
		sumaBlancos = 0;
		for (int i = 1; i < numBlancosMasUno.Count; i++) {
			sumaBlancos += numBlancosMasUno[i];
		}
		valorU = (ratioMenorMayor - 1) / sumaBlancos;
		
		calcularPesos();
	}
	
	private void construirListaElementos(XElement nodoFamilia) {
		string raízStr = nodoFamilia.Element("raíz").Value;
		if (raízStr == "") {
			raízFamilia = null;
		} else {
			raízFamilia = G.listaCriterios.getCriterio(raízStr);
		}
		
		ratioMenorMayor = float.Parse(nodoFamilia.Element("ratioMenorMayor").Value);
		if (ratioMenorMayor < 1) {
			throw new IllegalOperationException("El ratio menor/mayor especificado para la familia " + raízStr + " no es válido. " +
				"El valor debe ser >= 1.");
		}
		
		/*
		 * Tenemos que recorrer todos los elementos dos veces: una para saber cuántos hay y otra para insertarlos en la lista de
		 * elementos del método de Sismos (se insertan en orden contrario ya que al aplicar el método se empieza por el elemento
		 * de menor importancia, mientras que en el XML se coloca primero el más importante).
		 */
		IEnumerable<XElement> elementosXml = nodoFamilia.Element("elementos").Elements();
		int nivel = 0;
		foreach (XElement nodo in elementosXml) {
			if (nodo.Name != "blanco") {
				nivel++;
			}
		}
		
		elementosPorNivel = new int[nivel + 1];
		
		foreach (XElement nodo in elementosXml) {
			if (nodo.Name == "criterio") {
				Criterio criterio = procesarNodoCriterio(nodo, raízFamilia);
				elementos.Insert(0, new GrupoCriterios(criterio, nivel));
				elementosPorNivel[nivel] = 1;
				nivel--;
			} else if (nodo.Name == "grupo") {
				List<Criterio> criterios = new List<Criterio>();
				int numElementos = 0;
				foreach (XElement nodoCriterio in nodo.Elements("criterio")) {
					criterios.Add(procesarNodoCriterio(nodoCriterio, raízFamilia));
					numElementos++;
				}
				elementos.Insert(0, new GrupoCriterios(criterios, nivel));
				elementosPorNivel[nivel] = numElementos;
				nivel--;
			} else if (nodo.Name == "blanco") {
				elementos.Insert(0, new Blanco(nivel));
			} else {
				throw new IllegalOperationException("El tipo de elemento Sismos \"" + nodo.Name + "\" no es válido");
			}
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
				throw new IllegalOperationException("El criterio \"" + idCriterio + "\" especificado en los datos Sismos no " +
					"pertenece a la familia en la que está colocado (indicada: " + raízFamilia.id + ", real: " +
					criterio.getNombreFamilia() + ")");
			}
		} else {
			throw new IllegalOperationException("El criterio \"" + idCriterio + "\" especificado en los datos Sismos no " +
				"está registrado");
		}
	}
	
	/*
	 * Se asegura de que todos los criterios de esta familia han sido listados en los datos Sismos
	 */
	private void comprobarFamiliaCompleta() {
		foreach (Criterio criterio in criterios) {
			if (!contieneCriterio(criterio.id)) {
				throw new IllegalStateException("El criterio " + criterio.id + " pertenece a la familia " + criterio.getNombreFamilia() +
					" pero no está incluido en los datos Sismos para dicha familia");
			}
		}
	}
	
	/*
	 * Rellena los arrays numBlancos y numBlancosMasUno
	 */
	private void calcularNumBlancos() {
		List<int> numBlancos = new List<int>();
		int blancosActuales = 0;
		foreach (ElementoSismos elemento in elementos) {
			if (elemento is Blanco) {
				blancosActuales++;
			} else if (elemento is GrupoCriterios) {
				numBlancos.Add(blancosActuales);
				blancosActuales = 0;
			}
		}
		
		// El primer elemento siempre es 0, incluso aunque se hayan leído blancos al principio (que no tendrían sentido)
		numBlancosMasUno.Add(0);
		for (int i = 1; i < numBlancos.Count; i++) {
			numBlancosMasUno.Add(numBlancos[i] + 1);
		}
	}
	
	/*
	 * Calcula los pesos de todos los criterios almacenados
	 */
	private void calcularPesos() {
		// Alamacena los pesos no normalizados de cada nivel. pesosBrutos[0] = 0.
		List<float> pesosBrutos = new List<float>();
		pesosBrutos.Add(0);
		
		// Valor actual de la suma de numBlancosMasUno[n] para n = 0...nivel-1
		int sumaBlancosActual = 0;
		float sumaPesosBrutos = 0;
		for (int nivel = 1; nivel <= numBlancosMasUno.Count; nivel++) {
			sumaBlancosActual += numBlancosMasUno[nivel - 1];
			
			float pesoBruto = 1 + valorU * sumaBlancosActual;
			pesosBrutos.Add(pesoBruto);
			sumaPesosBrutos += pesoBruto;
		}
		
		// Calcular el peso normalizado de cada grupo
		List<float> pesosNormalizados = new List<float>();
		foreach (float valor in pesosBrutos) {
			pesosNormalizados.Add(valor / sumaPesosBrutos);
		}
		
		// Asignar el peso final a cada criterio
		foreach (ElementoSismos elemento in elementos) {
			if (elemento is GrupoCriterios grupo) {
				foreach (Criterio criterio in grupo.criterios) {
					if (criterios.count() == 1) {
						criterio.pesosLocales.setPeso(Método.SISMOS, 1);
					} else {
						criterio.pesosLocales.setPeso(Método.SISMOS, pesosNormalizados[grupo.nivel] / elementosPorNivel[grupo.nivel]);
					}
				}
			}
		}
	}
	
	/*
	 * Return: True si el criterio identificado con el ID especificado está incluido en los datos Sismos leídos del XML
	 */
	private bool contieneCriterio(string idCriterio) {
		foreach (Criterio criterio in getCriteriosXML()) {
			if (criterio.id == idCriterio) {
				return true;
			}
		}
		return false;
	}
	
	/*
	 * Devuelve un enumerable con todos los criterios leídos del XML
	 */
	private IEnumerable<Criterio> getCriteriosXML() {
		List<Criterio> ret = new List<Criterio>();
		foreach (ElementoSismos elemento in elementos) {
			if (elemento is GrupoCriterios grupo) {
				foreach (Criterio criterio in grupo.criterios) {
					ret.Add(criterio);
				}
			}
		}
		return ret;
	}
}