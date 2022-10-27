using System;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

/*
 * Clase usada para ejecutar los diferentes algoritmos y opciones del menú principal
 */
public class Ejecutar {
	private Global G;

	private NodoFamilias nodoFamiliasLBWA;
	private NodoFamilias nodoFamiliasBWM;
	private NodoFamilias nodoFamiliasSismos;
	private NodoFamilias nodoFamiliasRoc;

	public Ejecutar(Global G) {
		// No se obtiene con getInstancia() porque al cargarse, G instancia esta clase
		this.G = G;
	}

	/*
	 * Ejecuta el algoritmo LBWA partiendo de los criterios que se encuentren ahora mismo en G.listaCriterios.
	 * LBWA se ejecutará para cada rama del árbol de criteros usando las preferencias del decisor especificadas en el fichero
	 * XML con ruta <Global.RUTA_LBWA>, lo que permitirá obtener los pesos finales de cada criterio.
	 */
	public void ejecutarLBWA() {
		XElement raíz = XElement.Load(Cst.RUTA_LBWA);
		nodoFamiliasLBWA = new NodoFamilias(raíz);

		// Empezar por la familia raíz, es decir, los criterios de primer nivel
		procesarFamiliaLBWA("");
		// Luego el resto de familias, una por cada nodo que tenga hijos
		foreach (Criterio criterio in G.listaCriterios) {
			if (criterio.subcriterios.Count > 0) {
				procesarFamiliaLBWA(criterio.id);
			}
		}

		// Llegados a este punto ya tenemos los pesos locales de cada nodo. Ahora solo falta convertirlos a globales.
		G.listaCriterios.calcularPesosGlobales(Método.LBWA);
	}

	/*
	 * Analiza los valores BWM proporcionados por el experto para comprobar su grado de consistencia (ordinal y cardinal).
	 * Los resultados se muestran por pantalla.
	 */
	public void consistenciaBWM() {
		XElement raíz = XElement.Load(Cst.RUTA_BWM);
		nodoFamiliasBWM = new NodoFamilias(raíz);

		procesarFamiliaBWM("");
		foreach (Criterio criterio in G.listaCriterios) {
			if (criterio.subcriterios.Count > 0) {
				procesarFamiliaBWM(criterio.id);
			}
		}
	}
	
	/*
	 * Ejecuta el algoritmo de Sismos revisado para obtener los pesos de los diferentes criterios a partir de los datos
	 * especificados en el fichero XML de entrada y de los criterios que se encuentren ahora mismo en G.listaCriterios.
	 * El método se ejecuta para cada rama del árbol de criterios.
	 */
	public void ejecutarSismos() {
		XElement raíz = XElement.Load(Cst.RUTA_SISMOS);
		nodoFamiliasSismos = new NodoFamilias(raíz);

		procesarFamiliaSismos("");
		foreach (Criterio criterio in G.listaCriterios) {
			if (criterio.subcriterios.Count > 0) {
				procesarFamiliaSismos(criterio.id);
			}
		}

		G.listaCriterios.calcularPesosGlobales(Método.SISMOS);
	}
	
	/*
	 * Ejecuta el algoritmo ROC para obtener los pesos de los diferentes criterios a partir de los datos
	 * especificados en el fichero XML de entrada y de los criterios que se encuentren ahora mismo en G.listaCriterios.
	 * El método se ejecuta para cada rama del árbol de criterios.
	 */
	public void ejecutarRoc() {
		XElement raíz = XElement.Load(Cst.RUTA_ROC);
		nodoFamiliasRoc = new NodoFamilias(raíz);

		procesarFamiliaRoc("");
		foreach (Criterio criterio in G.listaCriterios) {
			if (criterio.subcriterios.Count > 0) {
				procesarFamiliaRoc(criterio.id);
			}
		}

		G.listaCriterios.calcularPesosGlobales(Método.ROC);
	}

	/*
	 * Muestra por pantalla los pesos globales calculados para cada criterio
	 */
	public void printPesosGlobales() {
		string resultado = "";
		foreach (Criterio criterio in G.listaCriterios.getCriteriosRaíz()) {
			resultado += criterio.toStringPesos(true, true) + "\n";
		}
		Console.Out.Write("Criterios y pesos globales:\n" + resultado);
	}

	/*
	 * Muestra por pantalla los pesos locales calculados para cada criterio
	 */
	public void printPesosLocales() {
		string resultado = "";
		foreach (Criterio criterio in G.listaCriterios.getCriteriosRaíz()) {
			resultado += criterio.toStringPesos(false, true) + "\n";
		}
		Console.Out.Write("Criterios y pesos locales:\n" + resultado);
	}

	/*
	 * Exporta los pesos de todos los criterios a un fichero XML
	 * rutaFicheroSalida: Ruta relativa al fichero de salida. Si no termina en ".xml", se añadirá la extensión al final.
	 */
	public void exportarPesos(string rutaFicheroSalida) {
		if (!rutaFicheroSalida.EndsWith(".xml")) {
			rutaFicheroSalida += ".xml";
		}

		XElement nodo = new XElement("criterios");
		foreach (Criterio criterio in G.listaCriterios) {
			nodo.Add(criterio.toNodoXML());
		}

		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		settings.IndentChars = "\t";
		using (XmlWriter writer = XmlWriter.Create(rutaFicheroSalida, settings)) {
			nodo.Save(writer);
		}
	}

	/*
	 * Ejecuta el algoritmo LBWA sobre una familia de criterios identificada por el id de su padre.
	 * id: ID del criterio padre que identifica la familia a procesar.
	 */
	private void procesarFamiliaLBWA(string id) {
		LBWA lbwa;
		if (id == "") {
			// Procesar la familia raíz, es decir, los criterios de primer nivel
			lbwa = new LBWA(new ListaCriterios(G, G.listaCriterios.getCriteriosRaíz()));
		} else {
			// Obtener el criterio que tiene el id indicado. Este será el padre de la familia.
			Criterio padre = G.listaCriterios.getCriterio(id);
			lbwa = new LBWA(new ListaCriterios(G, padre.subcriterios));
		}

		// Obtener los datos LBWA asociados a esta familia del XML
		XElement nodoFamilia = nodoFamiliasLBWA.getNodoFamilia(id);
		if (nodoFamilia != null) {
			lbwa.ejecutar(nodoFamilia);
		} else {
			throw new IllegalOperationException("No se puede ejecutar el algoritmo LBWA porque faltan los datos LBWA para la familia \"" +
				(id == "" ? Cst.NOMBRE_FAMILIA_RAÍZ : id) + "\"");
		}
	}

	private void procesarFamiliaBWM(string id) {
		ListaCriterios criterios;

		if (id == "") {
			// Familia raíz
			criterios = new ListaCriterios(G, G.listaCriterios.getCriteriosRaíz());
		} else {
			Criterio padre = G.listaCriterios.getCriterio(id);
			criterios = new ListaCriterios(G, padre.subcriterios);
		}
		XElement nodoFamilia = nodoFamiliasBWM.getNodoFamilia(id);
		if (nodoFamilia != null) {
			BWM bwm = new BWM(criterios, nodoFamilia);
			ConsistenciasBWM consistencias = bwm.calcularConsistencia();

			// Imprimir el resultado de la consistencia
			Console.Out.WriteLine("\n--- Familia: " + (id == "" ? Cst.NOMBRE_FAMILIA_RAÍZ : id) + " ---");
			Console.Out.WriteLine(consistencias.toStringColumnas());
		} else {
			throw new IllegalOperationException("No se puede ejecutar el cálculo de consistencia BWM porque faltan los datos BWM para " +
				"la familia \"" + (id == "" ? Cst.NOMBRE_FAMILIA_RAÍZ : id) + "\"");
		}
	}
	
	/*
	 * Ejecuta el algoritmo de Sismos sobre una familia de criterios identificada por el id de su padre.
	 * id: ID del criterio padre que identifica la familia a procesar.
	 */
	private void procesarFamiliaSismos(string id) {
		Sismos sismos;
		if (id == "") {
			sismos = new Sismos(new ListaCriterios(G, G.listaCriterios.getCriteriosRaíz()));
		} else {
			// Obtener el criterio que tiene el id indicado. Este será el padre de la familia.
			Criterio padre = G.listaCriterios.getCriterio(id);
			sismos = new Sismos(new ListaCriterios(G, padre.subcriterios));
		}

		// Obtener los datos Sismos asociados a esta familia del XML
		XElement nodoFamilia = nodoFamiliasSismos.getNodoFamilia(id);
		if (nodoFamilia != null) {
			sismos.ejecutar(nodoFamilia);
		} else {
			throw new IllegalOperationException("No se puede ejecutar el algoritmo Sismos porque faltan los datos Sismos para la familia \"" +
				(id == "" ? Cst.NOMBRE_FAMILIA_RAÍZ : id) + "\"");
		}
	}
	
	/*
	 * Ejecuta el algoritmo ROC sobre una familia de criterios identificada por el id de su padre.
	 * id: ID del criterio padre que identifica la familia a procesar.
	 */
	private void procesarFamiliaRoc(string id) {
		ROC roc;
		if (id == "") {
			roc = new ROC(new ListaCriterios(G, G.listaCriterios.getCriteriosRaíz()));
		} else {
			// Obtener el criterio que tiene el id indicado. Este será el padre de la familia.
			Criterio padre = G.listaCriterios.getCriterio(id);
			roc = new ROC(new ListaCriterios(G, padre.subcriterios));
		}

		// Obtener los datos ROC asociados a esta familia del XML
		XElement nodoFamilia = nodoFamiliasRoc.getNodoFamilia(id);
		if (nodoFamilia != null) {
			roc.ejecutar(nodoFamilia);
		} else {
			throw new IllegalOperationException("No se puede ejecutar el algoritmo ROC porque faltan los datos ROC para la familia \"" +
				(id == "" ? Cst.NOMBRE_FAMILIA_RAÍZ : id) + "\"");
		}
	}
}