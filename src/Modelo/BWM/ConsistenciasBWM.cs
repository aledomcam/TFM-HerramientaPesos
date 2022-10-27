using System;
using System.Collections.Generic;

/*
 * Almacena información acerca del grado de consistencia de unos datos BWM para los criterios de una cierta familia
 */
public class ConsistenciasBWM {
	/*
	 * Almacenan el nombre a mostrar en cada una de las columnas de datos al convertir la información de la instancia a una string
	 */
	private const string NOMBRE_COL_ID_CRITERIO = "ID criterio";
	private const string NOMBRE_COL_MEJOR_A_ESTE = "M>e";
	private const string NOMBRE_COL_ESTE_A_PEOR = "e>P";
	private const string NOMBRE_COL_CONSISTENCIA_ENTRADA = "C. Entrada";
	private const string NOMBRE_COL_CONSISTENCIA_ORDINAL = "C. Ordinal";
	/*
	 * Valores máximos aceptables para el valor de consistencia de entrada según el número de criterios (1-9)
	 * Fuente: Liang et al, 2020
	 */
	public readonly float[] MAX_CONSISTENCIA_ENTRADA = {0, 0, 0.1359f, 0.2681f, 0.3062f, 0.3337f, 0.3517f, 0.3620f, 0.3662f};

	private int numCriterios;
	List<ConsistenciaBWM> consistencias;

	/*
	 * numCriterios: Número de criterios en la familia actual (max 9)
	 */
	public ConsistenciasBWM(int numCriterios) {
		if (numCriterios > 9) {
			throw new ArgumentException("No se puede calcular el valor de consistencia para más de 9 criterios debido a que el " +
				"artículo que propone esta medida no proporciona datos para más de 9 criterios.");
		}
		this.numCriterios = numCriterios;
		consistencias = new List<ConsistenciaBWM>();
	}

	public void añadir(ConsistenciaBWM consistencia) {
		consistencias.Add(consistencia);
	}

	public float getConsistenciaDeEntrada() {
		float mayor = 0;
		foreach (ConsistenciaBWM consistencia in consistencias) {
			if (consistencia.consistenciaDeEntrada > mayor) {
				mayor = consistencia.consistenciaDeEntrada;
			}
		}
		return mayor;
	}

	public float getConsistenciaOrdinal() {
		float mayor = 0;
		foreach (ConsistenciaBWM consistencia in consistencias) {
			if (consistencia.consistenciaOrdinal > mayor) {
				mayor = consistencia.consistenciaOrdinal;
			}
		}
		return mayor;
	}

	/*
	 * Muestra los datos de consistencia de los criterios de esta familia en formato de columnas: Nombre del criterio,
	 * mejor a este, este a peor, consistencia de entrada y consistencia ordinal.
	 */
	public string toStringColumnas() {
		// Cabecera
		string ret = "Consistencia de entrada (cardinal): " + consistenciaEntradaToString(getConsistenciaDeEntrada()) +
			". Consistencia ordinal: " + getConsistenciaOrdinal() + "\n";
		string columna = "";
		columna = Utils.concatenarColumna(columna, NOMBRE_COL_ID_CRITERIO, Cst.POS_ID_CRITERIO, Cst.POS_MEJOR_A_ESTE - 1);
		columna = Utils.concatenarColumna(columna, NOMBRE_COL_MEJOR_A_ESTE, Cst.POS_MEJOR_A_ESTE, Cst.POS_ESTE_A_PEOR - 1);
		columna = Utils.concatenarColumna(columna, NOMBRE_COL_ESTE_A_PEOR, Cst.POS_ESTE_A_PEOR, Cst.POS_CONSISTENCIA_ENTRADA - 1);
		columna = Utils.concatenarColumna(columna, NOMBRE_COL_CONSISTENCIA_ENTRADA, Cst.POS_CONSISTENCIA_ENTRADA, Cst.POS_CONSISTENCIA_ORDINAL - 1);
		columna = Utils.concatenarColumna(columna, NOMBRE_COL_CONSISTENCIA_ORDINAL, Cst.POS_CONSISTENCIA_ORDINAL, int.MaxValue);
		ret += columna;

		// Añadir una línea por elemento contenido en la instancia
		foreach (ConsistenciaBWM consistencia in consistencias) {
			ret += "\n";
			ret += consistencia.toStringColumnas();
		}
		return ret;
	}

	/*
	 * Convierte el valor de consistencia de entrada especificado a una string, añadiendo la cadena " [!]" al final si excede
	 * el valor máximo según el número de criterios.
	 */
	private string consistenciaEntradaToString(float valor) {
		if (valor > MAX_CONSISTENCIA_ENTRADA[numCriterios]) {
			return valor.ToString() + " [!]";
		} else {
			return valor.ToString();
		}
	}
}