using System;
using System.Collections.Generic;

/*
 * Almacena información acerca del grado de consistencia de un criterio BWM
 */
public class ConsistenciaBWM {
	private Criterio criterio;
	private ListaCriterios criterios;
	private ListaCmpCriteriosBWM comparaciones;

	private int mejorAEste, esteAPeor, mejorAPeor;

	public float consistenciaDeEntrada;
	public float consistenciaOrdinal;

	public ConsistenciaBWM(Criterio criterio, ListaCriterios criterios, ListaCmpCriteriosBWM comparaciones) {
		this.criterio = criterio;
		this.criterios = criterios;
		this.comparaciones = comparaciones;
		mejorAEste = comparaciones.getMejorAEste(criterio).valor;
		esteAPeor = comparaciones.getEsteAPeor(criterio).valor;
		mejorAPeor = comparaciones.getMejorAPeor().valor;

		consistenciaDeEntrada = calcularConsistenciaDeEntrada();
		consistenciaOrdinal = calcularConsistenciaOrdinal();
	}

	private float calcularConsistenciaDeEntrada() {
		if (mejorAPeor == 1) {
			return 0;
		} else {
			return Math.Abs(mejorAEste * esteAPeor - mejorAPeor) / (float) (mejorAPeor * mejorAPeor - mejorAPeor);
		}
	}

	private float calcularConsistenciaOrdinal() {
		float suma = 0;
		foreach (Criterio i in criterios) {
			suma += funciónF((comparaciones.getMejorAEste(i).valor - mejorAEste) * (esteAPeor - comparaciones.getEsteAPeor(i).valor), i);
		}
		return suma / criterios.count();
	}

	/*
	 * Implementa la función F() usada para el cálculo de la consistencia ordinal en Liang et al, 2020.
	 */
	private float funciónF(int x, Criterio i) {
		if (x < 0) {
			return 1;
		} else if (x == 0 &&
		(comparaciones.getMejorAEste(i).valor - mejorAEste != 0 || esteAPeor - comparaciones.getEsteAPeor(i).valor != 0)) {
			return 0.5f;
		} else {
			return 0;
		}
	}

	/*
	 * Combina el ID del criterio, los dos valores de consistencia y las comparaciones con el mejor y peor criterio en una
	 * string con formato de columnas.
	 */
	public string toStringColumnas() {
		string ret = "";
		ret = Utils.concatenarColumna(ret, criterio.id, Cst.POS_ID_CRITERIO, Cst.POS_MEJOR_A_ESTE - 1);
		ret = Utils.concatenarColumna(ret, mejorAEste.ToString(), Cst.POS_MEJOR_A_ESTE, Cst.POS_ESTE_A_PEOR - 1);
		ret = Utils.concatenarColumna(ret, esteAPeor.ToString(), Cst.POS_ESTE_A_PEOR, Cst.POS_CONSISTENCIA_ENTRADA - 1);
		ret = Utils.concatenarColumna(ret, consistenciaDeEntrada.ToString(), Cst.POS_CONSISTENCIA_ENTRADA, Cst.POS_CONSISTENCIA_ORDINAL - 1);
		ret = Utils.concatenarColumna(ret, consistenciaOrdinal.ToString(), Cst.POS_CONSISTENCIA_ORDINAL, int.MaxValue);
		return ret;
	}
}